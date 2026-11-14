// Só existe no servidor dedicado e no editor; não entra no build do jogador.
#if UNITY_SERVER || UNITY_EDITOR
using SQLite;

namespace CyberMU.Server.Characters
{
    // O SQL que cria a tabela Characters, num lugar só: a migração do banco
    // real (ServerDatabase, v2 -> v3) e os testes (banco temporário) chamam
    // este mesmo código, então os dois criam EXATAMENTE a mesma estrutura.
    public static class CharacterSchema
    {
        public static void Apply(SQLiteConnection db)
        {
            // Name: COLLATE NOCASE + índice único -> o PRÓPRIO banco impede
            // "Heroi" e "heroi" como personagens diferentes, em qualquer conta
            // (nome único no servidor inteiro).
            // (AccountId, Slot) único -> uma conta nunca tem dois personagens
            // no mesmo slot.
            db.Execute(@"CREATE TABLE Characters (
                Id            INTEGER PRIMARY KEY AUTOINCREMENT,
                AccountId     INTEGER NOT NULL,
                Slot          INTEGER NOT NULL,
                Name          TEXT    NOT NULL COLLATE NOCASE,
                ClassId       INTEGER NOT NULL,
                Level         INTEGER NOT NULL,
                Experience    INTEGER NOT NULL,
                Strength      INTEGER NOT NULL,
                Agility       INTEGER NOT NULL,
                Vitality      INTEGER NOT NULL,
                Energy        INTEGER NOT NULL,
                Command       INTEGER NOT NULL,
                LevelUpPoints INTEGER NOT NULL,
                CreatedAt     TEXT    NOT NULL
            )");
            db.Execute("CREATE UNIQUE INDEX UX_Characters_Name ON Characters (Name COLLATE NOCASE)");
            db.Execute("CREATE UNIQUE INDEX UX_Characters_Account_Slot ON Characters (AccountId, Slot)");
        }
    }
}
#endif
