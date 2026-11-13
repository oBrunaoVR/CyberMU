// Só existe no servidor dedicado e no editor; não entra no build do jogador.
#if UNITY_SERVER || UNITY_EDITOR
using System;
using System.IO;
using SQLite;
using UnityEngine;

namespace CyberMU.Server.Diagnostics
{
    // Verificação de diagnóstico: confirma que o SQLite funciona dentro de um
    // build (por exemplo, Windows Dedicated Server).
    // Só roda se o executável for aberto com o argumento "-sqliteCheck".
    // Sem esse argumento, não faz nada. Grava e lê um registro num arquivo
    // temporário, escreve o resultado no log, apaga o arquivo e fecha o programa.
    public static class SqliteBuildCheck
    {
        private const string Argument = "-sqliteCheck";

        private class CheckRow
        {
            [PrimaryKey, AutoIncrement]
            public int Id { get; set; }
            public string Name { get; set; }
            public int Level { get; set; }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RunIfRequested()
        {
            if (Application.isEditor) return;
            if (Array.IndexOf(Environment.GetCommandLineArgs(), Argument) < 0) return;

            string dbPath = Path.Combine(Application.temporaryCachePath, "cybermu_sqlite_build_check.db");
            bool ok = false;
            try
            {
                if (File.Exists(dbPath)) File.Delete(dbPath);

                using (var db = new SQLiteConnection(dbPath))
                {
                    db.CreateTable<CheckRow>();
                    db.Insert(new CheckRow { Name = "Teste", Level = 1 });
                }

                using (var db = new SQLiteConnection(dbPath))
                {
                    var rows = db.Table<CheckRow>().ToList();
                    ok = rows.Count == 1 && rows[0].Name == "Teste" && rows[0].Level == 1;
                    Debug.Log($"[SqliteCheck] Versão do SQLite: {db.ExecuteScalar<string>("select sqlite_version()")}");
                }

                Debug.Log(ok
                    ? "[SqliteCheck] OK: gravou, fechou, reabriu e leu de volta."
                    : "[SqliteCheck] FALHOU: os dados lidos não conferem.");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SqliteCheck] FALHOU com erro: {e}");
            }
            finally
            {
                try { if (File.Exists(dbPath)) File.Delete(dbPath); } catch { }
            }

            Application.Quit(ok ? 0 : 1);
        }
    }
}
#endif
