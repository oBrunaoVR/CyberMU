// Só existe no servidor dedicado e no editor; não entra no build do jogador.
#if UNITY_SERVER || UNITY_EDITOR
using CyberMU.Server.Characters;
using CyberMU.Shared.Characters;
using CyberMU.Shared.Player;
using Mirror;
using SQLite;
using UnityEngine;

namespace CyberMU.Server.World
{
    // Recebe o pedido "entrar no mundo com o personagem X" e decide.
    // Registrado com requireAuthentication = true: o PRÓPRIO Mirror descarta
    // pedidos de conexões que não passaram pelo login.
    // Só o servidor cria a cápsula, e só depois de conferir que o personagem
    // pertence à conta da conexão.
    public static class EnterWorldHandler
    {
        private static SQLiteConnection db;

        // Chamado pelo NetworkManager ao iniciar o servidor. Se o banco não
        // abriu, databaseConnection vem null e os pedidos respondem erro.
        public static void Register(SQLiteConnection databaseConnection)
        {
            db = databaseConnection;
            NetworkServer.RegisterHandler<EnterWorldRequestMessage>(OnEnterWorldRequest, true);
        }

        // Chamado pelo NetworkManager ao parar o servidor.
        public static void Unregister()
        {
            db = null;
            NetworkServer.UnregisterHandler<EnterWorldRequestMessage>();
        }

        private static void OnEnterWorldRequest(NetworkConnectionToClient conn, EnterWorldRequestMessage msg)
        {
            // Já tem cápsula nesta conexão (também cobre pedido repetido).
            if (conn.identity != null || ActiveCharacterRegistry.IsInWorld(conn))
            {
                Refuse(conn, EnterWorldCode.AlreadyInWorld, "Já está no mundo");
                return;
            }

            if (db == null || !(conn.authenticationData is int accountId) || accountId <= 0)
            {
                Refuse(conn, EnterWorldCode.ServerError, "Erro no servidor. Tente mais tarde.");
                return;
            }

            CharacterRecord character;
            try
            {
                // Devolve null se o personagem não existir OU for de outra conta.
                character = CharacterService.FindOwned(db, accountId, msg.characterId);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[World] ERRO ao buscar personagem {msg.characterId} (conta {accountId}): {e.GetType().Name}: {e.Message}");
                Refuse(conn, EnterWorldCode.ServerError, "Erro no servidor. Tente mais tarde.");
                return;
            }

            if (character == null)
            {
                Debug.Log($"[World] Entrada recusada: personagem {msg.characterId} não pertence à conta {accountId} (conexão {conn.connectionId})");
                Refuse(conn, EnterWorldCode.InvalidCharacter, "Personagem inválido");
                return;
            }

            try
            {
                SpawnPlayer(conn, character);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[World] ERRO ao criar jogador de '{character.Name}': {e.GetType().Name}: {e.Message}");
                ActiveCharacterRegistry.Remove(conn);
                Refuse(conn, EnterWorldCode.ServerError, "Erro no servidor. Tente mais tarde.");
                return;
            }

            Debug.Log($"[World] '{character.Name}' entrou no mundo (conta {accountId}, conexão {conn.connectionId}). No mundo agora: {ActiveCharacterRegistry.Count}");
        }

        private static void SpawnPlayer(NetworkConnectionToClient conn, CharacterRecord character)
        {
            // Mesmo prefab e mesmos pontos de nascimento que o Mirror usava
            // na criação automática.
            NetworkManager manager = NetworkManager.singleton;
            Transform start = manager.GetStartPosition();
            GameObject player = start != null
                ? Object.Instantiate(manager.playerPrefab, start.position, start.rotation)
                : Object.Instantiate(manager.playerPrefab);
            player.name = $"{manager.playerPrefab.name} [{character.Name}]";

            // O nome vai num SyncVar: preenchido ANTES do AddPlayerForConnection,
            // chega a todos os clientes junto com o nascimento da cápsula.
            if (player.TryGetComponent(out PlayerNameTag nameTag))
                nameTag.characterName = character.Name;

            ActiveCharacterRegistry.Set(conn, character);
            NetworkServer.AddPlayerForConnection(conn, player);
            conn.Send(new EnterWorldResponseMessage { code = EnterWorldCode.Accepted, message = "Entrou no mundo" });

            // Atributos vão SÓ para o dono (conn.Send); os outros jogadores
            // continuam recebendo apenas o nome, pela SyncVar do PlayerNameTag.
            CharacterStatsNotifier.SendToOwner(conn, character);
        }

        private static void Refuse(NetworkConnectionToClient conn, EnterWorldCode code, string message)
        {
            conn.Send(new EnterWorldResponseMessage { code = code, message = message });
        }
    }
}
#endif
