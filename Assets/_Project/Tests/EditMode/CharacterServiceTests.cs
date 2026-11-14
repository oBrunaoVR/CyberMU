using System.Collections.Generic;
using System.IO;
using CyberMU.Server.Characters;
using CyberMU.Shared.Characters;
using NUnit.Framework;
using SQLite;

namespace CyberMU.Tests.EditMode
{
    // Testes do serviço de personagens (regras da MU_099D_REFERENCE.md, seção 6).
    // SEMPRE usam um banco TEMPORÁRIO criado pelo CharacterSchema (a mesma
    // estrutura da migração v2 -> v3); o banco real nunca é tocado.
    public class CharacterServiceTests
    {
        private const int Account1 = 1;
        private const int Account2 = 2;

        private string dbPath;
        private SQLiteConnection db;

        [SetUp]
        public void SetUp()
        {
            dbPath = Path.Combine(Path.GetTempPath(), "cybermu_character_service_tests.db");
            if (File.Exists(dbPath)) File.Delete(dbPath);
            db = new SQLiteConnection(dbPath);
            CharacterSchema.Apply(db);
        }

        [TearDown]
        public void TearDown()
        {
            db.Close();
            if (File.Exists(dbPath)) File.Delete(dbPath);
        }

        private CharacterCreateResult Create(int accountId, string name, ClassId classId)
        {
            return CharacterService.Create(db, accountId, name, classId, CharacterNameRules.Default);
        }

        // (a) As três classes iniciais são criadas com os atributos corretos.
        [Test]
        public void CriaClassesIniciaisComAtributosCorretos()
        {
            CharacterCreateResult berserker = Create(Account1, "Lamina1", ClassId.Berserker);
            Assert.IsTrue(berserker.Accepted, berserker.Message);
            AssertCharacter(berserker.Character, Account1, slot: 0, "Lamina1", ClassId.Berserker,
                strength: 28, agility: 20, vitality: 25, energy: 10);

            CharacterCreateResult technomancer = Create(Account1, "Codigo1", ClassId.Technomancer);
            Assert.IsTrue(technomancer.Accepted, technomancer.Message);
            AssertCharacter(technomancer.Character, Account1, slot: 1, "Codigo1", ClassId.Technomancer,
                strength: 18, agility: 18, vitality: 15, energy: 30);

            CharacterCreateResult ranger = Create(Account1, "Flecha1", ClassId.NeonRanger);
            Assert.IsTrue(ranger.Accepted, ranger.Message);
            AssertCharacter(ranger.Character, Account1, slot: 2, "Flecha1", ClassId.NeonRanger,
                strength: 22, agility: 25, vitality: 20, energy: 15);
        }

        // (b) O sexto personagem é recusado com "Limite de 5 personagens".
        [Test]
        public void SextoPersonagemEhRecusado()
        {
            for (int i = 1; i <= 5; i++)
            {
                CharacterCreateResult ok = Create(Account1, $"Heroi{i}", ClassId.Berserker);
                Assert.IsTrue(ok.Accepted, $"O personagem {i} deveria ser aceito: {ok.Message}");
            }

            CharacterCreateResult sixth = Create(Account1, "Heroi6", ClassId.Berserker);
            Assert.IsFalse(sixth.Accepted);
            Assert.AreEqual(CharacterCreateCode.LimitReached, sixth.Code);
            Assert.AreEqual("Limite de 5 personagens", sixth.Message);
            Assert.AreEqual(5, CharacterService.ListByAccount(db, Account1).Count);
        }

        // (c) Nome repetido é recusado, mesmo com maiúsculas diferentes e
        // mesmo quando o pedido vem de OUTRA conta (nome único no servidor).
        [Test]
        public void NomeRepetidoEhRecusado()
        {
            Assert.IsTrue(Create(Account1, "Fantasma", ClassId.Berserker).Accepted);

            CharacterCreateResult sameAccount = Create(Account1, "fantasma", ClassId.Technomancer);
            Assert.AreEqual(CharacterCreateCode.NameTaken, sameAccount.Code);
            Assert.AreEqual("Nome já em uso", sameAccount.Message);

            CharacterCreateResult otherAccount = Create(Account2, "FANTASMA", ClassId.NeonRanger);
            Assert.AreEqual(CharacterCreateCode.NameTaken, otherAccount.Code);
            Assert.AreEqual("Nome já em uso", otherAccount.Message);
        }

        // (d) Nomes fora das regras provisórias (4 a 10, letras e números)
        // são recusados com "Nome inválido".
        [Test]
        public void NomeInvalidoEhRecusado()
        {
            string[] invalidNames = { "", "abc", "NomeMuitoGrande1", "Hero!", "He ro", "Herói", null };
            foreach (string name in invalidNames)
            {
                CharacterCreateResult result = Create(Account1, name, ClassId.Berserker);
                Assert.AreEqual(CharacterCreateCode.NameInvalid, result.Code, $"Nome testado: '{name}'");
                Assert.AreEqual("Nome inválido", result.Message);
            }
            Assert.AreEqual(0, CharacterService.ListByAccount(db, Account1).Count);
        }

        // (e) Ronin e Overlord são recusados com "Classe bloqueada": os
        // atributos iniciais estão PENDENTES DE VERIFICAÇÃO, então ficam
        // bloqueados MESMO que a conta tenha personagem de nível alto.
        [Test]
        public void RoninEOverlordEstaoBloqueados()
        {
            CharacterCreateResult ronin = Create(Account1, "Espada1", ClassId.Ronin);
            Assert.AreEqual(CharacterCreateCode.ClassLocked, ronin.Code);
            Assert.AreEqual("Classe bloqueada", ronin.Message);

            CharacterCreateResult overlord = Create(Account1, "Chefe1", ClassId.Overlord);
            Assert.AreEqual(CharacterCreateCode.ClassLocked, overlord.Code);
            Assert.AreEqual("Classe bloqueada", overlord.Message);

            // Conta com personagem de nível 300 (gravado direto no banco de
            // teste): continua bloqueado, porque os atributos são PENDENTES.
            CharacterRepository.Insert(db, new CharacterRecord
            {
                AccountId = Account1,
                Slot = 0,
                Name = "Veterano1",
                ClassId = (int)ClassId.Berserker,
                Level = 300,
                Experience = 0,
                Strength = 28,
                Agility = 20,
                Vitality = 25,
                Energy = 10,
                Command = 0,
                LevelUpPoints = 0,
                CreatedAt = CharacterRepository.NowText(),
            });

            CharacterCreateResult roninHighLevel = Create(Account1, "Espada2", ClassId.Ronin);
            Assert.AreEqual(CharacterCreateCode.ClassLocked, roninHighLevel.Code);

            // Classe que não existe no catálogo também conta como bloqueada.
            CharacterCreateResult unknown = Create(Account1, "Fruta1", (ClassId)99);
            Assert.AreEqual(CharacterCreateCode.ClassLocked, unknown.Code);
        }

        // (f) Cada conta só vê os próprios personagens.
        [Test]
        public void ContasNaoVeemPersonagensUmaDaOutra()
        {
            Assert.IsTrue(Create(Account1, "Sombra1", ClassId.Berserker).Accepted);
            Assert.IsTrue(Create(Account1, "Sombra2", ClassId.Technomancer).Accepted);
            Assert.IsTrue(Create(Account2, "Neon1", ClassId.NeonRanger).Accepted);

            List<CharacterRecord> account1 = CharacterService.ListByAccount(db, Account1);
            List<CharacterRecord> account2 = CharacterService.ListByAccount(db, Account2);

            Assert.AreEqual(2, account1.Count);
            Assert.AreEqual(1, account2.Count);
            Assert.AreEqual("Neon1", account2[0].Name);
            foreach (CharacterRecord c in account1)
                Assert.AreNotEqual("Neon1", c.Name);

            // O primeiro personagem de cada conta começa no slot 0.
            Assert.AreEqual(0, account1[0].Slot);
            Assert.AreEqual(0, account2[0].Slot);
        }

        private static void AssertCharacter(CharacterRecord c, int accountId, int slot, string name,
            ClassId classId, int strength, int agility, int vitality, int energy)
        {
            Assert.AreEqual(accountId, c.AccountId);
            Assert.AreEqual(slot, c.Slot);
            Assert.AreEqual(name, c.Name);
            Assert.AreEqual((int)classId, c.ClassId);
            Assert.AreEqual(1, c.Level);
            Assert.AreEqual(0, c.Experience);
            Assert.AreEqual(strength, c.Strength);
            Assert.AreEqual(agility, c.Agility);
            Assert.AreEqual(vitality, c.Vitality);
            Assert.AreEqual(energy, c.Energy);
            Assert.AreEqual(0, c.Command);
            Assert.AreEqual(0, c.LevelUpPoints);
            Assert.IsTrue(c.Id > 0, "O Id deveria vir do banco.");
            Assert.IsFalse(string.IsNullOrEmpty(c.CreatedAt), "CreatedAt deveria estar preenchido.");
        }
    }
}
