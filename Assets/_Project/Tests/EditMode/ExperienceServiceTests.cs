using System.IO;
using CyberMU.Server.Characters;
using CyberMU.Shared.Characters;
using NUnit.Framework;
using SQLite;

namespace CyberMU.Tests.EditMode
{
    // Testes do ganho de EXP no servidor (MU_099D_REFERENCE.md, seção 7).
    // SEMPRE usam um banco TEMPORÁRIO criado pelo CharacterSchema; o banco
    // real nunca é tocado.
    public class ExperienceServiceTests
    {
        private const int Account1 = 1;

        private string dbPath;
        private SQLiteConnection db;

        [SetUp]
        public void SetUp()
        {
            dbPath = Path.Combine(Path.GetTempPath(), "cybermu_experience_service_tests.db");
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

        // Personagem criado pelo caminho normal (Berserker: 5 pontos/nível).
        private CharacterRecord CreateBerserker(string name)
        {
            CharacterCreateResult result = CharacterService.Create(
                db, Account1, name, ClassId.Berserker, CharacterNameRules.Default);
            Assert.IsTrue(result.Accepted, result.Message);
            return result.Character;
        }

        // Personagem gravado direto no banco de teste (para classes que a
        // criação ainda bloqueia, ou níveis altos).
        private CharacterRecord InsertDirect(string name, ClassId classId, int level, long experience)
        {
            var record = new CharacterRecord
            {
                AccountId = Account1,
                Slot = 1,
                Name = name,
                ClassId = (int)classId,
                Level = level,
                Experience = experience,
                Strength = 1,
                Agility = 1,
                Vitality = 1,
                Energy = 1,
                Command = 0,
                LevelUpPoints = 0,
                CreatedAt = CharacterRepository.NowText(),
            };
            record.Id = CharacterRepository.Insert(db, record);
            return record;
        }

        // (a) 100 de EXP sobem o nível 1 para o 2 e dão 5 pontos (Berserker),
        // e o resultado fica GRAVADO no banco.
        [Test]
        public void SobeUmNivelComCincoPontos()
        {
            CharacterRecord c = CreateBerserker("Teste1");

            ExperienceGrantResult result = ExperienceService.GrantExperience(db, c, 100);

            Assert.IsTrue(result.Changed);
            Assert.AreEqual(1, result.LevelsGained);
            Assert.AreEqual(5, result.PointsGained);
            Assert.AreEqual(2, c.Level);
            Assert.AreEqual(100, c.Experience);
            Assert.AreEqual(5, c.LevelUpPoints);

            // Relê do banco: o progresso foi salvo, não só mudado na memória.
            CharacterRecord saved = CharacterRepository.FindById(db, c.Id);
            Assert.AreEqual(2, saved.Level);
            Assert.AreEqual(100, saved.Experience);
            Assert.AreEqual(5, saved.LevelUpPoints);
        }

        // (b) EXP suficiente para vários níveis de uma vez: 1080 de EXP total
        // alcançam as fórmulas dos níveis 1 (100), 2 (440) e 3 (1080),
        // então o personagem vai direto ao nível 4 com 15 pontos.
        [Test]
        public void SobeVariosNiveisDeUmaVez()
        {
            CharacterRecord c = CreateBerserker("Teste2");

            ExperienceGrantResult result = ExperienceService.GrantExperience(db, c, 1080);

            Assert.AreEqual(3, result.LevelsGained);
            Assert.AreEqual(15, result.PointsGained);
            Assert.AreEqual(4, c.Level);
            Assert.AreEqual(1080, c.Experience);
            Assert.AreEqual(15, c.LevelUpPoints);
        }

        // (c) Classe de 7 pontos (Ronin, via ClassCatalog): subir 1 nível dá 7.
        [Test]
        public void RoninGanhaSetePontosPorNivel()
        {
            CharacterRecord c = InsertDirect("Teste3", ClassId.Ronin, level: 1, experience: 0);

            ExperienceGrantResult result = ExperienceService.GrantExperience(db, c, 100);

            Assert.AreEqual(1, result.LevelsGained);
            Assert.AreEqual(7, result.PointsGained);
            Assert.AreEqual(2, c.Level);
        }

        // (d) O nível máximo (400) nunca é ultrapassado, mesmo com uma EXP
        // absurda, e a EXP fica travada no total necessário para alcançá-lo.
        [Test]
        public void NivelMaximoNaoEhUltrapassado()
        {
            CharacterRecord c = CreateBerserker("Teste4");

            ExperienceService.GrantExperience(db, c, 100_000_000_000_000);

            Assert.AreEqual(ExperienceRules.MaxLevel, c.Level);
            // 399 subidas de nível × 5 pontos.
            Assert.AreEqual(399 * 5, c.LevelUpPoints);
            // A EXP trava na fórmula do nível 399 (o total para chegar ao 400).
            Assert.AreEqual(ExperienceRules.TotalExperienceToLevelUp(399), c.Experience);
        }

        // (e) No nível máximo, não ganha mais EXP nenhuma.
        [Test]
        public void NoNivelMaximoNaoGanhaMaisNada()
        {
            CharacterRecord c = InsertDirect("Teste5", ClassId.Berserker,
                level: ExperienceRules.MaxLevel,
                experience: ExperienceRules.TotalExperienceToLevelUp(ExperienceRules.MaxLevel - 1));
            long expBefore = c.Experience;

            ExperienceGrantResult result = ExperienceService.GrantExperience(db, c, 500);

            Assert.IsFalse(result.Changed);
            Assert.AreEqual(ExperienceRules.MaxLevel, c.Level);
            Assert.AreEqual(expBefore, c.Experience);
            Assert.AreEqual(0, c.LevelUpPoints);
        }
    }
}
