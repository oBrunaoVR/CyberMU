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
    // Também garante a tabela SchemaInfo (versão da estrutura do banco).
    // NÃO contém dados de jogo (contas, personagens, itens etc.).
    public static class ServerDatabase
    {
        public const string FolderName = "ServerData";
        public const string FileName = "cybermu.db";

        // Versão atual da estrutura do banco. Sobe de número quando a estrutura
        // mudar no futuro (novas tabelas/colunas), para sabermos o que atualizar.
        public const int CurrentSchemaVersion = 1;

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

                int version = EnsureSchemaInfo();

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

        // Cria a tabela SchemaInfo se não existir e grava a versão 1 só no
        // banco novo. Em banco já existente, apenas lê a versão gravada.
        private static int EnsureSchemaInfo()
        {
            connection.CreateTable<SchemaInfo>(); // não apaga nada se já existir

            SchemaInfo row = connection.Find<SchemaInfo>(1);
            if (row == null)
            {
                row = new SchemaInfo { Id = 1, Version = CurrentSchemaVersion };
                connection.Insert(row);
            }
            else if (row.Version != CurrentSchemaVersion)
            {
                Debug.LogWarning($"[ServerDatabase] Versão do banco ({row.Version}) diferente da esperada " +
                                 $"({CurrentSchemaVersion}). Ainda não há atualização automática de estrutura.");
            }
            return row.Version;
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
