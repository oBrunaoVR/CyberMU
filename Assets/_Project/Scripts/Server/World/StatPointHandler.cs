// Só existe no servidor dedicado e no editor; não entra no build do jogador.
#if UNITY_SERVER || UNITY_EDITOR
using CyberMU.Server.Characters;
using CyberMU.Shared.Characters;
using Mirror;
using SQLite;
using UnityEngine;

namespace CyberMU.Server.World
{
    // Recebe o pedido do botão + da janela C e repassa ao StatPointService.
    // Registrado com requireAuthentication = true: o próprio Mirror descarta
    // pedidos de conexões que não passaram pelo login.
    // O personagem vem do ActiveCharacterRegistry: é o MESMO objeto em
    // memória que o ganho de EXP usa, e estar lá já prova que ele pertence
    // a esta conexão (só a entrada no mundo o coloca lá).
    public static class StatPointHandler
    {
        private static SQLiteConnection db;

        public static void Register(SQLiteConnection databaseConnection)
        {
            db = databaseConnection;
            NetworkServer.RegisterHandler<IncreaseStatRequestMessage>(OnRequest, true);
        }

        public static void Unregister()
        {
            db = null;
            NetworkServer.UnregisterHandler<IncreaseStatRequestMessage>();
        }

        private static void OnRequest(NetworkConnectionToClient conn, IncreaseStatRequestMessage msg)
        {
            if (db == null) return;

            // Só personagens que estão no mundo distribuem pontos.
            if (!ActiveCharacterRegistry.TryGet(conn, out CharacterRecord character)) return;

            StatIncreaseCode code;
            try
            {
                code = StatPointService.IncreaseStat(db, character, msg.stat);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[World] ERRO ao distribuir ponto de '{character.Name}': {e.GetType().Name}: {e.Message}");
                return;
            }

            if (code == StatIncreaseCode.Accepted)
            {
                // Mudou atributo no servidor -> reenvia a ficha SÓ ao dono.
                CharacterStatsNotifier.SendToOwner(conn, character);
            }
            else
            {
                // Recusa silenciosa para o cliente (a janela continua com os
                // valores antigos); o motivo fica no log do servidor.
                Debug.Log($"[World] Ponto recusado ({code}) para '{character.Name}': atributo {msg.stat}, pontos disponíveis {character.LevelUpPoints}.");
            }
        }
    }
}
#endif
