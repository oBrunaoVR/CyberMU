// COMANDO DE DESENVOLVEDOR (lado do servidor).
// Dupla condição de compilação:
//   (UNITY_SERVER || UNITY_EDITOR)      -> como todo código de servidor,
//                                          nunca entra no build do jogador;
//   (UNITY_EDITOR || DEVELOPMENT_BUILD) -> e, além disso, só existe no
//                                          editor ou em build de
//                                          desenvolvimento.
// Num servidor compilado normal este arquivo fica vazio: o atendimento
// nunca é registrado e o Mirror descarta o pedido de qualquer cliente
// alterado que tente mandá-lo.
#if (UNITY_SERVER || UNITY_EDITOR) && (UNITY_EDITOR || DEVELOPMENT_BUILD)
using CyberMU.Server.Characters;
using CyberMU.Shared.Characters;
using Mirror;
using SQLite;
using UnityEngine;

namespace CyberMU.Server.World
{
    // Recebe o pedido de EXP de teste (tecla F9 no cliente) e dá uma
    // quantidade FIXA decidida aqui: o cliente não manda número nenhum.
    // Registrado com requireAuthentication = true: o próprio Mirror descarta
    // pedidos de conexões que não passaram pelo login.
    public static class DevExperienceCommand
    {
        // EXP por aperto de F9. Mudar o valor de teste = mexer só aqui.
        public const long ExperiencePerPress = 500;

        private static SQLiteConnection db;

        public static void Register(SQLiteConnection databaseConnection)
        {
            db = databaseConnection;
            NetworkServer.RegisterHandler<DevGrantExperienceRequestMessage>(OnRequest, true);
        }

        public static void Unregister()
        {
            db = null;
            NetworkServer.UnregisterHandler<DevGrantExperienceRequestMessage>();
        }

        private static void OnRequest(NetworkConnectionToClient conn, DevGrantExperienceRequestMessage msg)
        {
            // Terceira camada de defesa, em tempo de execução: além de este
            // código nem ser compilado numa build normal, confere que o
            // servidor está mesmo no editor ou numa build de desenvolvimento.
            if (!Application.isEditor && !Debug.isDebugBuild)
            {
                Debug.LogWarning($"[Dev] Pedido de EXP de teste recusado (conexão {conn.connectionId}): o servidor não é de desenvolvimento.");
                return;
            }

            if (db == null) return;

            // Só personagens que estão no mundo ganham EXP.
            if (!ActiveCharacterRegistry.TryGet(conn, out CharacterRecord character)) return;

            ExperienceGrantResult result;
            try
            {
                result = ExperienceService.GrantExperience(db, character, ExperiencePerPress);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[Dev] ERRO ao dar EXP de teste a '{character.Name}': {e.GetType().Name}: {e.Message}");
                return;
            }

            // Mesmo padrão de sempre: mudou atributo no servidor -> reenvia
            // a ficha SÓ ao dono. Os sistemas futuros (monstros) repetem
            // esta dupla: ExperienceService + CharacterStatsNotifier.
            if (result.Changed)
                CharacterStatsNotifier.SendToOwner(conn, character);

            Debug.Log($"[Dev] +{ExperiencePerPress} EXP para '{character.Name}': nível {character.Level}, EXP total {character.Experience}, subiu {result.LevelsGained} nível(is), ganhou {result.PointsGained} ponto(s).");
        }
    }
}
#endif
