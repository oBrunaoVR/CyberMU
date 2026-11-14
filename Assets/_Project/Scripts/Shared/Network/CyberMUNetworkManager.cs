using Mirror;
#if UNITY_SERVER || UNITY_EDITOR
using CyberMU.Server.Accounts;
using CyberMU.Server.Persistence;
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
            ServerDatabase.Open();
#endif
        }

        // Chamado pelo Mirror ao parar o servidor (inclusive ao sair do Play
        // ou fechar o programa).
        public override void OnStopServer()
        {
#if UNITY_SERVER || UNITY_EDITOR
            AccountService.ClearOnline();
            ServerDatabase.Close();
#endif
            base.OnStopServer();
        }

        // Chamado no servidor sempre que uma conexão termina: saída normal,
        // queda (timeout), recusa de login ou o host parando.
        // Tira a conta da lista de "conectadas" e depois faz o padrão do
        // Mirror (destruir a cápsula do jogador).
        public override void OnServerDisconnect(NetworkConnectionToClient conn)
        {
#if UNITY_SERVER || UNITY_EDITOR
            AccountService.MarkOffline(conn);
#endif
            base.OnServerDisconnect(conn);
        }
    }
}
