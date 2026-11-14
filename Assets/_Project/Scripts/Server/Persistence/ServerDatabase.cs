// Só existe no servidor dedicado e no editor; não entra no build do jogador.
#if UNITY_SERVER || UNITY_EDITOR
using System;
using System.IO;
using SQLite;
using UnityEngine;

namespace CyberMU.Server.Persistence
{
    // Serviço do banco SQLite real do servidor.
    // Responsabilidade ÚNICA: descobrir o caminho do banco, criar a pasta
    // ServerData se faltar, abrir a conexão e fechá-la.
    // Também garante a tabela SchemaInfo (versão da estrutura do banco) e
    // aplica as atualizações de estrutura (com cópia de segurança antes).
    // NÃO contém regras de jogo: quem lê/grava contas é o AccountRepository.
    public static class ServerDatabase
    {
        public const string FolderName = "ServerData";
        public const string FileName = "cybermu.db";

        // Versão atual da estrutura do banco. Sobe de número quando a estrutura
        // mudar no futuro (novas tabelas/colunas), para sabermos o que atualizar.
        // Histórico: v1 = só SchemaInfo; v2 = tabela Accounts (contas).
        public const int CurrentSchemaVersion = 2;

        // Tabela com uma única linha (Id = 1) guardando a versão do banco.
        [Table("SchemaInfo")]
        private class SchemaInfo
        {
            [PrimaryKey]
            public int Id { get; set; }
            public int Version { get; set; }
        }

        private static SQLiteConnection connection;

        public static bool IsOpen => connection != null;

        // Conexão aberta, para os repositórios do servidor (ex.: AccountRepository).
        // É null quando o banco não está aberto.
        internal static SQLiteConnection Connection => connection;
        public static string DatabasePath { get; private set; }

        // Pasta ServerData:
        // - no editor: <raiz do projeto>/ServerData (pasta acima de Assets)
        // - no build do servidor: ServerData ao lado do .exe
        //   (Application.dataPath no build Windows é "<pasta do exe>/<Nome>_Data")
        private static string GetFolderPath()
        {
            // A mesma conta serve para os dois casos: no editor, dataPath é
            // "<raiz>/Assets"; no build, "<pasta do exe>/<Nome>_Data".
            // Em ambos, a pasta "pai" é onde a ServerData deve ficar.
            string baseFolder = Directory.GetParent(Application.dataPath).FullName;
            return Path.Combine(baseFolder, FolderName);
        }

        // Abre (ou cria) o banco. Nunca lança exceção para fora: se falhar,
        // escreve um erro claro no Console e devolve false; o servidor segue
        // rodando sem banco.
        public static bool Open()
        {
            if (IsOpen)
            {
                Debug.LogWarning($"[ServerDatabase] Banco já estava aberto: {DatabasePath}");
                return true;
            }

            string path = null;
            try
            {
                string folder = GetFolderPath();
                Directory.CreateDirectory(folder); // não faz nada se já existir
                path = Path.Combine(folder, FileName);
                bool existed = File.Exists(path);

                connection = new SQLiteConnection(path);
                DatabasePath = path;

                int version = EnsureSchema(isNewDatabase: !existed);

                Debug.Log($"[ServerDatabase] Banco {(existed ? "reaberto (já existia)" : "criado")}: {path} | versão do banco: {version}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[ServerDatabase] ERRO ao abrir o banco em '{path ?? GetFolderPathSafe()}'. " +
                               $"O servidor continua rodando SEM banco (nada será salvo). " +
                               $"Verifique permissão de escrita na pasta e se o arquivo não está aberto em outro programa. " +
                               $"Detalhe: {e.GetType().Name}: {e.Message}");
                CloseQuietly();
                return false;
            }
        }

        // Fecha o banco, se estiver aberto.
        public static void Close()
        {
            if (!IsOpen) return;
            string path = DatabasePath;
            try
            {
                connection.Close();
                Debug.Log($"[ServerDatabase] Banco fechado: {path}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[ServerDatabase] ERRO ao fechar o banco '{path}'. Detalhe: {e.GetType().Name}: {e.Message}");
            }
            finally
            {
                connection = null;
                DatabasePath = null;
            }
        }

        // Garante a tabela SchemaInfo e deixa a estrutura do banco na versão
        // atual (CurrentSchemaVersion), aplicando as atualizações em ordem.
        //
        // REGRA: antes de atualizar um banco que JÁ EXISTIA, fazemos uma cópia
        // de segurança automática (cybermu.v<versão antiga>.bak). Se a cópia
        // falhar, a atualização NÃO acontece (o Open falha e avisa no Console).
        // Banco recém-criado não precisa de cópia (não há dados a perder).
        private static int EnsureSchema(bool isNewDatabase)
        {
            connection.CreateTable<SchemaInfo>(); // não apaga nada se já existir

            SchemaInfo row = connection.Find<SchemaInfo>(1);
            if (row == null)
            {
                // Banco novo: começa na versão 1 e sobe pelas mesmas etapas
                // de um banco antigo, para existir um único caminho de criação.
                row = new SchemaInfo { Id = 1, Version = 1 };
                connection.Insert(row);
            }

            if (row.Version > CurrentSchemaVersion)
            {
                throw new InvalidOperationException(
                    $"O banco está na versão {row.Version}, mais nova que a deste programa ({CurrentSchemaVersion}). " +
                    "Use uma versão mais nova do servidor.");
            }

            if (row.Version < CurrentSchemaVersion && !isNewDatabase)
            {
                string backup = BackupBeforeMigration(row.Version);
                Debug.Log($"[ServerDatabase] Cópia de segurança antes de atualizar a estrutura: {backup}");
            }

            while (row.Version < CurrentSchemaVersion)
            {
                int from = row.Version;
                // Cada etapa roda numa transação: ou aplica tudo, ou nada.
                connection.RunInTransaction(() =>
                {
                    ApplyMigrationStep(from);
                    row.Version = from + 1;
                    connection.Update(row);
                });
                Debug.Log($"[ServerDatabase] Estrutura do banco atualizada: versão {from} -> {from + 1}");
            }
            return row.Version;
        }

        // Uma etapa de atualização da estrutura: da versão "from" para from+1.
        // Etapas antigas NUNCA devem ser alteradas depois de publicadas;
        // mudanças novas viram uma etapa nova (e CurrentSchemaVersion sobe).
        private static void ApplyMigrationStep(int from)
        {
            switch (from)
            {
                case 1:
                    // v1 -> v2: tabela de contas.
                    // Name usa COLLATE NOCASE + índice único: o PRÓPRIO banco
                    // impede "Teste1" e "teste1" como contas diferentes.
                    connection.Execute(@"CREATE TABLE Accounts (
                        Id             INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name           TEXT    NOT NULL COLLATE NOCASE,
                        PasswordHash   TEXT    NOT NULL,
                        Salt           TEXT    NOT NULL,
                        HashIterations INTEGER NOT NULL,
                        CreatedAt      TEXT    NOT NULL,
                        LastLoginAt    TEXT    NULL
                    )");
                    connection.Execute("CREATE UNIQUE INDEX UX_Accounts_Name ON Accounts (Name COLLATE NOCASE)");
                    break;

                default:
                    throw new InvalidOperationException($"Não existe etapa de atualização a partir da versão {from}.");
            }
        }

        // Copia o banco inteiro para ServerData/cybermu.v<versão>.bak usando
        // "VACUUM INTO" do SQLite, que gera uma cópia consistente mesmo com o
        // banco aberto. Se o nome já existir, acrescenta data/hora.
        private static string BackupBeforeMigration(int version)
        {
            string folder = Path.GetDirectoryName(DatabasePath);
            string baseName = Path.GetFileNameWithoutExtension(FileName); // "cybermu"
            string backupPath = Path.Combine(folder, $"{baseName}.v{version}.bak");
            if (File.Exists(backupPath))
                backupPath = Path.Combine(folder, $"{baseName}.v{version}.{DateTime.Now:yyyyMMdd-HHmmss}.bak");

            connection.Execute($"VACUUM INTO '{backupPath.Replace("'", "''")}'");

            if (!File.Exists(backupPath))
                throw new IOException($"A cópia de segurança não foi criada em '{backupPath}'.");
            return backupPath;
        }

        private static void CloseQuietly()
        {
            try { connection?.Close(); } catch { /* já estamos tratando outro erro */ }
            connection = null;
            DatabasePath = null;
        }

        private static string GetFolderPathSafe()
        {
            try { return GetFolderPath(); } catch { return FolderName; }
        }
    }
}
#endif
