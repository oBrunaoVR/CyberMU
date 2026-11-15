// Só existe no servidor dedicado e no editor; não entra no build do jogador.
#if UNITY_SERVER || UNITY_EDITOR
using System.Collections.Generic;
using CyberMU.Server.Characters;
using Mirror;
using UnityEngine;

namespace CyberMU.Server.World
{
    // Qual personagem cada conexão está usando AGORA. Vive só na memória do
    // SERVIDOR: nada daqui é enviado ao cliente nem gravado no banco.
    // É a fonte que os próximos sistemas (combate, XP, salvar) vão consultar
    // para saber "quem é" o jogador de uma conexão.
    public static class ActiveCharacterRegistry
    {
        private static readonly Dictionary<NetworkConnectionToClient, CharacterRecord> byConnection =
            new Dictionary<NetworkConnectionToClient, CharacterRecord>();

        public static int Count => byConnection.Count;

        // Registra o personagem que entrou no mundo por esta conexão.
        public static void Set(NetworkConnectionToClient conn, CharacterRecord character)
        {
            byConnection[conn] = character;
        }

        public static bool TryGet(NetworkConnectionToClient conn, out CharacterRecord character)
        {
            return byConnection.TryGetValue(conn, out character);
        }

        public static bool IsInWorld(NetworkConnectionToClient conn)
        {
            return byConnection.ContainsKey(conn);
        }

        // Tira o personagem desta conexão do registro (se estiver).
        // Seguro chamar várias vezes e para conexões que nunca entraram.
        public static void Remove(NetworkConnectionToClient conn)
        {
            if (conn == null) return;
            if (byConnection.TryGetValue(conn, out CharacterRecord character))
            {
                byConnection.Remove(conn);
                Debug.Log($"[World] '{character.Name}' saiu do mundo (conexão {conn.connectionId}). No mundo agora: {Count}");
            }
        }

        // Esvazia o registro (servidor iniciando ou parando).
        public static void Clear()
        {
            byConnection.Clear();
        }
    }
}
#endif
