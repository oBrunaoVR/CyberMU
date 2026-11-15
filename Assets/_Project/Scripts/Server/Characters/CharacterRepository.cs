// Só existe no servidor dedicado e no editor; não entra no build do jogador.
#if UNITY_SERVER || UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using SQLite;

namespace CyberMU.Server.Characters
{
    // Leitura e gravação da tabela Characters. Responsabilidade ÚNICA: SQL.
    // Não decide nada (as regras de criação ficam no CharacterService).
    // Recebe a conexão por parâmetro: o servidor passa o banco real e os
    // testes passam um banco temporário.
    public static class CharacterRepository
    {
        public static List<CharacterRecord> ListByAccount(SQLiteConnection db, int accountId)
        {
            return db.Query<CharacterRecord>(
                "SELECT * FROM Characters WHERE AccountId = ? ORDER BY Slot", accountId);
        }

        // Busca um personagem pelo Id. Devolve null se não existir.
        public static CharacterRecord FindById(SQLiteConnection db, int id)
        {
            List<CharacterRecord> rows = db.Query<CharacterRecord>(
                "SELECT * FROM Characters WHERE Id = ? LIMIT 1", id);
            return rows.Count > 0 ? rows[0] : null;
        }

        // SEM diferenciar maiúsculas (a coluna é COLLATE NOCASE) e em
        // QUALQUER conta: o nome é único no servidor inteiro.
        public static bool NameExists(SQLiteConnection db, string name)
        {
            return db.ExecuteScalar<long>("SELECT COUNT(*) FROM Characters WHERE Name = ?", name) > 0;
        }

        // Grava o personagem e devolve o Id gerado pelo banco.
        public static int Insert(SQLiteConnection db, CharacterRecord c)
        {
            db.Execute(@"INSERT INTO Characters
                (AccountId, Slot, Name, ClassId, Level, Experience,
                 Strength, Agility, Vitality, Energy, Command, LevelUpPoints, CreatedAt)
                VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)",
                c.AccountId, c.Slot, c.Name, c.ClassId, c.Level, c.Experience,
                c.Strength, c.Agility, c.Vitality, c.Energy, c.Command, c.LevelUpPoints, c.CreatedAt);
            return (int)db.ExecuteScalar<long>("SELECT last_insert_rowid()");
        }

        // Mesmo formato de data da tabela Accounts (ISO 8601, UTC).
        public static string NowText() =>
            DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture);
    }
}
#endif
