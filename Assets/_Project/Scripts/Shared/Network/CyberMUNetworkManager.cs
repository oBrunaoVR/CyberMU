using Mirror;
using UnityEngine;
#if UNITY_SERVER || UNITY_EDITOR
using CyberMU.Server.Accounts;
using CyberMU.Server.Characters;
using CyberMU.Server.Persistence;
using CyberMU.Server.World;
#endif

namespace CyberMU.Shared.Network
{
    // NetworkManager do CyberMU. É o NetworkManager padrão do Mirror com
    // ganchos nossos no início e na parada do servidor.
    // Este arquivo existe no servidor E no cliente (a cena precisa dele nos
    // dois builds). Só as partes do servidor ficam dentro do #if, para o
    // build do jogador nunca conter código de banco.
    public class CyberMUNetworkManager : NetworkManager
    {
        // Chamado pelo Mirror ao iniciar o servidor (Server Only e Host).
        // Não é chamado em modo Client.
        public override void OnStartServer()
        {
            base.OnStartServer();
#if UNITY_SERVER || UNITY_EDITOR
            AccountService.ClearOnline();
            ActiveCharacterRegistry.Clear();
            ServerDatabase.Open();
            // Entrega a conexão do banco às caixas de personagens e de
            // entrada no mundo (elas não conhecem o ServerDatabase). Se o
            // banco não abriu, vai null e os pedidos respondem erro, sem
            // derrubar o servidor.
            CharacterMessageHandlers.Register(ServerDatabase.Connection);
            EnterWorldHandler.Register(ServerDatabase.Connection);
            StatPointHandler.Register(ServerDatabase.Connection);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            // Comando de desenvolvedor (F9 = EXP de teste): só existe no
            // editor e em build de desenvolvimento. Num servidor normal este
            // registro não é compilado e o pedido é descartado pelo Mirror.
            DevExperienceCommand.Register(ServerDatabase.Connection);
#endif
#endif
        }

        // Chamado pelo Mirror ao parar o servidor (inclusive ao sair do Play
        // ou fechar o programa).
        public override void OnStopServer()
        {
#if UNITY_SERVER || UNITY_EDITOR
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            DevExperienceCommand.Unregister();
#endif
            StatPointHandler.Unregister();
            EnterWorldHandler.Unregister();
            CharacterMessageHandlers.Unregister();
            AccountService.ClearOnline();
            ActiveCharacterRegistry.Clear();
            ServerDatabase.Close();
#endif
            base.OnStopServer();
        }

        // Chamado no servidor sempre que uma conexão termina: saída normal,
        // queda (timeout), recusa de login ou o host parando.
        // Tira a conta da lista de "conectadas" e o personagem do registro
        // "em uso", e depois faz o padrão do Mirror (destruir a cápsula).
        public override void OnServerDisconnect(NetworkConnectionToClient conn)
        {
#if UNITY_SERVER || UNITY_EDITOR
            ActiveCharacterRegistry.Remove(conn);
            AccountService.MarkOffline(conn);
#endif
            base.OnServerDisconnect(conn);
        }

        // Chamado pelo Mirror quando um cliente envia o pedido ANTIGO de
        // criação automática de jogador. Com a seleção de personagem, a única
        // porta de entrada é a EnterWorldRequestMessage (EnterWorldHandler):
        // um cliente alterado que mandar o pedido antigo não ganha cápsula.
        public override void OnServerAddPlayer(NetworkConnectionToClient conn)
        {
            Debug.Log($"[World] Pedido automático de jogador ignorado (conexão {conn.connectionId}). A entrada é pela seleção de personagem.");
        }
    }
}
