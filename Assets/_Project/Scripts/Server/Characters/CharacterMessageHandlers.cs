// Só existe no servidor dedicado e no editor; não entra no build do jogador.
#if UNITY_SERVER || UNITY_EDITOR
using CyberMU.Shared.Characters;
using Mirror;
using SQLite;
using UnityEngine;

namespace CyberMU.Server.Characters
{
    // Recebe as mensagens de personagens no servidor e responde.
    // Registrado com requireAuthentication = true: o PRÓPRIO Mirror descarta
    // mensagens de conexões que não passaram pelo login.
    // Não conhece o ServerDatabase: o NetworkManager ENTREGA a conexão do
    // banco no Register (dependência só no sentido código solto -> caixa).
    public static class CharacterMessageHandlers
    {
        private static SQLiteConnection db;

        // Chamado pelo NetworkManager ao iniciar o servidor. Se o banco não
        // abriu, databaseConnection vem null e os pedidos respondem erro.
        public static void Register(SQLiteConnection databaseConnection)
        {
            db = databaseConnection;
            NetworkServer.RegisterHandler<CharacterListRequestMessage>(OnListRequest, true);
            NetworkServer.RegisterHandler<CharacterCreateRequestMessage>(OnCreateRequest, true);
        }

        // Chamado pelo NetworkManager ao parar o servidor.
        public static void Unregister()
        {
            db = null;
            NetworkServer.UnregisterHandler<CharacterListRequestMessage>();
            NetworkServer.UnregisterHandler<CharacterCreateRequestMessage>();
        }

        // Id da conta guardado pelo login (conn.authenticationData).
        private static bool TryGetAccountId(NetworkConnectionToClient conn, out int accountId)
        {
            if (conn.authenticationData is int id && id > 0)
            {
                accountId = id;
                return true;
            }
            accountId = 0;
            return false;
        }

        private static void OnListRequest(NetworkConnectionToClient conn, CharacterListRequestMessage msg)
        {
            if (db == null || !TryGetAccountId(conn, out int accountId))
            {
                conn.Send(new CharacterListResponseMessage { characters = new CharacterSummary[0] });
                return;
            }

            var records = CharacterService.ListByAccount(db, accountId);
            var summaries = new CharacterSummary[records.Count];
            for (int i = 0; i < records.Count; i++)
                summaries[i] = ToSummary(records[i]);

            conn.Send(new CharacterListResponseMessage { characters = summaries });
        }

        private static void OnCreateRequest(NetworkConnectionToClient conn, CharacterCreateRequestMessage msg)
        {
            if (db == null || !TryGetAccountId(conn, out int accountId))
            {
                conn.Send(new CharacterCreateResponseMessage
                {
                    code = CharacterCreateCode.ServerError,
                    message = "Erro no servidor. Tente mais tarde.",
                });
                return;
            }

            CharacterCreateResult result;
            try
            {
                // Limites de nome provisórios (CharacterNameRules.Default):
                // DECISÃO TÉCNICA PROVISÓRIA, PENDENTE DE VERIFICAÇÃO.
                result = CharacterService.Create(db, accountId, msg.name, msg.classId, CharacterNameRules.Default);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[Characters] ERRO ao criar personagem (conta {accountId}): {e.GetType().Name}: {e.Message}");
                conn.Send(new CharacterCreateResponseMessage
                {
                    code = CharacterCreateCode.ServerError,
                    message = "Erro no servidor. Tente mais tarde.",
                });
                return;
            }

            if (result.Accepted)
                Debug.Log($"[Characters] Personagem criado: '{result.Character.Name}' (classe {(ClassId)result.Character.ClassId}, slot {result.Character.Slot}) | conta {accountId}");
            else
                Debug.Log($"[Characters] Criação recusada | conta {accountId} | motivo: {result.Message}");

            conn.Send(new CharacterCreateResponseMessage
            {
                code = result.Code,
                message = result.Message,
                character = result.Accepted ? ToSummary(result.Character) : default,
            });
        }

        private static CharacterSummary ToSummary(CharacterRecord r)
        {
            return new CharacterSummary
            {
                slot = (byte)r.Slot,
                name = r.Name,
                classId = (ClassId)r.ClassId,
                level = r.Level,
            };
        }
    }
}
#endif
