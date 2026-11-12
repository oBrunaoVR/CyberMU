using System.IO;
using NUnit.Framework;
using SQLite;

namespace CyberMU.Tests.EditMode
{
    // Teste mínimo: confirma que a biblioteca SQLite-net for Unity consegue
    // criar um arquivo de banco, gravar, fechar, reabrir e ler de volta.
    // Não faz parte do jogo; usa um arquivo temporário e apaga no final.
    public class SqliteSmokeTest
    {
        private class TestRow
        {
            [PrimaryKey, AutoIncrement]
            public int Id { get; set; }
            public string Name { get; set; }
            public int Level { get; set; }
        }

        private string dbPath;

        [SetUp]
        public void SetUp()
        {
            dbPath = Path.Combine(Path.GetTempPath(), "cybermu_sqlite_smoke_test.db");
            if (File.Exists(dbPath)) File.Delete(dbPath);
        }

        [TearDown]
        public void TearDown()
        {
            if (File.Exists(dbPath)) File.Delete(dbPath);
        }

        [Test]
        public void WritesAndReadsBackAfterReopening()
        {
            using (var db = new SQLiteConnection(dbPath))
            {
                db.CreateTable<TestRow>();
                db.Insert(new TestRow { Name = "Teste", Level = 1 });
            }

            Assert.IsTrue(File.Exists(dbPath), "O arquivo do banco deveria existir no disco.");

            using (var db = new SQLiteConnection(dbPath))
            {
                var rows = db.Table<TestRow>().ToList();
                Assert.AreEqual(1, rows.Count);
                Assert.AreEqual("Teste", rows[0].Name);
                Assert.AreEqual(1, rows[0].Level);
            }
        }
    }
}
