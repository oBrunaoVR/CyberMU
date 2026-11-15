using System.IO;
using CyberMU.Server.Characters;
using CyberMU.Shared.Characters;
using NUnit.Framework;
using SQLite;

namespace CyberMU.Tests.EditMode
{
    // Testes da distribuição de pontos no servidor (MU_099D_REFERENCE.md,
    // seções 6 e 7). SEMPRE usam um banco TEMPORÁRIO criado pelo
    // CharacterSchema; o banco real nunca é tocado.
    // Todo caso RELÊ do banco no final: prova que o resultado (ou a recusa)
    // foi gravado, não só mudado na memória.
    public class StatPointServiceTests
    {
        private const int Account1 = 1;

        private string dbPath;
        private SQLiteConnection db;

        [SetUp]
        public void SetUp()
        {
            dbPath = Path.Combine(Path.GetTempPath(), "cybermu_stat_point_service_tests.db");
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

        // Personagem gravado direto no banco de teste, com os pontos e a
        // Força que o caso precisar (a criação normal começa com 0 pontos).
        private CharacterRecord InsertDirect(
            string name, ClassId classId, int levelUpPoints, int strength = 28)
        {
            var record = new CharacterRecord
            {
                AccountId = Account1,
                Slot = 1,
                Name = name,
                ClassId = (int)classId,
                Level = 1,
                Experience = 0,
                Strength = strength,
                Agility = 20,
                Vitality = 25,
                Energy = 10,
                Command = 0,
                LevelUpPoints = levelUpPoints,
                CreatedAt = CharacterRepository.NowText(),
            };
            record.Id = CharacterRepository.Insert(db, record);
            return record;
        }

        // (a) Com 5 pontos, 1 clique em Força: Força +1, sobram 4, e o
        // resultado fica GRAVADO no banco.
        [Test]
        public void AdicionaUmPontoEmForca()
        {
            CharacterRecord c = InsertDirect("Teste1", ClassId.Berserker, levelUpPoints: 5);

            StatIncreaseCode code = StatPointService.IncreaseStat(db, c, StatId.Strength);

            Assert.AreEqual(StatIncreaseCode.Accepted, code);
            Assert.AreEqual(29, c.Strength);
            Assert.AreEqual(4, c.LevelUpPoints);

            CharacterRecord saved = CharacterRepository.FindById(db, c.Id);
            Assert.AreEqual(29, saved.Strength);
            Assert.AreEqual(4, saved.LevelUpPoints);
        }

        // (b) Com 0 pontos, o pedido é recusado e NADA muda (nem no banco).
        [Test]
        public void SemPontosEhRecusado()
        {
            CharacterRecord c = InsertDirect("Teste2", ClassId.Berserker, levelUpPoints: 0);

            StatIncreaseCode code = StatPointService.IncreaseStat(db, c, StatId.Strength);

            Assert.AreEqual(StatIncreaseCode.NoPointsAvailable, code);
            Assert.AreEqual(28, c.Strength);
            Assert.AreEqual(0, c.LevelUpPoints);

            CharacterRecord saved = CharacterRepository.FindById(db, c.Id);
            Assert.AreEqual(28, saved.Strength);
            Assert.AreEqual(0, saved.LevelUpPoints);
        }

        // (c) Comando em personagem que NÃO é Overlord: recusado, nada muda,
        // e o ponto continua disponível.
        [Test]
        public void ComandoForaDoOverlordEhRecusado()
        {
            CharacterRecord c = InsertDirect("Teste3", ClassId.Berserker, levelUpPoints: 5);

            StatIncreaseCode code = StatPointService.IncreaseStat(db, c, StatId.Command);

            Assert.AreEqual(StatIncreaseCode.InvalidStat, code);
            Assert.AreEqual(0, c.Command);
            Assert.AreEqual(5, c.LevelUpPoints);

            CharacterRecord saved = CharacterRepository.FindById(db, c.Id);
            Assert.AreEqual(0, saved.Command);
            Assert.AreEqual(5, saved.LevelUpPoints);
        }

        // (c2) No Overlord, Comando é válido e funciona como os outros.
        [Test]
        public void ComandoNoOverlordEhAceito()
        {
            CharacterRecord c = InsertDirect("Teste4", ClassId.Overlord, levelUpPoints: 3);

            StatIncreaseCode code = StatPointService.IncreaseStat(db, c, StatId.Command);

            Assert.AreEqual(StatIncreaseCode.Accepted, code);
            Assert.AreEqual(1, c.Command);
            Assert.AreEqual(2, c.LevelUpPoints);

            CharacterRecord saved = CharacterRepository.FindById(db, c.Id);
            Assert.AreEqual(1, saved.Command);
            Assert.AreEqual(2, saved.LevelUpPoints);
        }

        // (d) Atributo no teto de 32767 (seção 7): recusado, nada muda.
        [Test]
        public void NoTetoEhRecusado()
        {
            CharacterRecord c = InsertDirect("Teste5", ClassId.Berserker,
                levelUpPoints: 5, strength: StatPointService.MaxStatValue);

            StatIncreaseCode code = StatPointService.IncreaseStat(db, c, StatId.Strength);

            Assert.AreEqual(StatIncreaseCode.StatAtMax, code);
            Assert.AreEqual(StatPointService.MaxStatValue, c.Strength);
            Assert.AreEqual(5, c.LevelUpPoints);

            CharacterRecord saved = CharacterRepository.FindById(db, c.Id);
            Assert.AreEqual(StatPointService.MaxStatValue, saved.Strength);
            Assert.AreEqual(5, saved.LevelUpPoints);
        }

        // (f) EXP e ponto na SEQUÊNCIA, sobre o MESMO objeto em memória
        // (como no servidor, via ActiveCharacterRegistry): depois dos dois,
        // nível, EXP, atributo e pontos restantes estão corretos no banco —
        // um serviço não sobrescreveu o que o outro gravou.
        [Test]
        public void ExpEPontoNaSequenciaGravamTudoCerto()
        {
            // Criação pelo caminho normal: Berserker nível 1, 0 pontos.
            CharacterCreateResult created = CharacterService.Create(
                db, Account1, "Teste6", ClassId.Berserker, CharacterNameRules.Default);
            Assert.IsTrue(created.Accepted, created.Message);
            CharacterRecord c = created.Character;

            // 100 de EXP: nível 2, 5 pontos (como nos testes de EXP).
            ExperienceGrantResult exp = ExperienceService.GrantExperience(db, c, 100);
            Assert.IsTrue(exp.Changed);

            // Logo em seguida, 1 ponto em Força, no MESMO objeto.
            StatIncreaseCode code = StatPointService.IncreaseStat(db, c, StatId.Strength);
            Assert.AreEqual(StatIncreaseCode.Accepted, code);

            // Relê do banco: TUDO correto ao mesmo tempo.
            CharacterRecord saved = CharacterRepository.FindById(db, c.Id);
            Assert.AreEqual(2, saved.Level);
            Assert.AreEqual(100, saved.Experience);
            Assert.AreEqual(29, saved.Strength); // 28 inicial + 1
            Assert.AreEqual(4, saved.LevelUpPoints); // 5 ganhos - 1 gasto
        }
    }
}
