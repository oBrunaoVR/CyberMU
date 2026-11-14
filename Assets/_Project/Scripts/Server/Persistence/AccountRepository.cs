// Só existe no servidor dedicado e no editor; não entra no build do jogador.
#if UNITY_SERVER || UNITY_EDITOR
using System;
using System.Globalization;
using SQLite;

namespace CyberMU.Server.Persistence
{
    // Uma linha da tabela Accounts (criada na versão 2 do banco).
    // Hash e salt ficam em texto Base64; datas em texto ISO 8601 (UTC).
    [Table("Accounts")]
    public class AccountRecord
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string PasswordHash { get; set; }
        public string Salt { get; set; }
        public int HashIterations { get; set; }
        public string CreatedAt { get; set; }
        public string LastLoginAt { get; set; }
    }

    // Leitura e gravação da tabela Accounts. Responsabilidade ÚNICA: SQL de
    // contas. Não decide nada (regras de login ficam no AccountService).
    public static class AccountRepository
    {
        private static SQLiteConnection Db
        {
            get
            {
                SQLiteConnection db = ServerDatabase.Connection;
                if (db == null) throw new InvalidOperationException("Banco de dados não está aberto.");
                return db;
            }
        }

        // Busca pelo nome SEM diferenciar maiúsculas (a coluna é COLLATE NOCASE).
        public static AccountRecord FindByName(string name)
        {
            var rows = Db.Query<AccountRecord>("SELECT * FROM Accounts WHERE Name = ? LIMIT 1", name);
            return rows.Count > 0 ? rows[0] : null;
        }

        // Cria a conta e devolve o Id gerado. Se o nome já existir (mesmo com
        // maiúsculas diferentes), o índice único do banco lança SQLiteException.
        public static int Create(string name, string passwordHashBase64, string saltBase64, int iterations)
        {
            string now = NowText();
            Db.Execute(
                "INSERT INTO Accounts (Name, PasswordHash, Salt, HashIterations, CreatedAt, LastLoginAt) VALUES (?, ?, ?, ?, ?, ?)",
                name, passwordHashBase64, saltBase64, iterations, now, now);
            return (int)Db.ExecuteScalar<long>("SELECT last_insert_rowid()");
        }

        public static void UpdateLastLogin(int accountId)
        {
            Db.Execute("UPDATE Accounts SET LastLoginAt = ? WHERE Id = ?", NowText(), accountId);
        }

        private static string NowText() =>
            DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture);
    }
}
#endif
