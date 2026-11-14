using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
#if UNITY_SERVER || UNITY_EDITOR
using CyberMU.Server.Accounts;
#endif

namespace CyberMU.Shared.Network
{
    // "Porteiro" do Mirror: toda conexão (cliente comum ou o próprio host)
    // precisa passar por aqui antes de entrar no jogo.
    // - Cliente: envia conta + senha e recebe "aceito" ou "recusado".
    // - Servidor: pede ao AccountService para decidir. Se aceito, guarda o Id
    //   da conta na conexão (conn.authenticationData). Se recusado, responde
    //   o motivo e desconecta 1 segundo depois (para a resposta chegar).
    // Baseado no BasicAuthenticator de exemplo do Mirror 96.
    [AddComponentMenu("CyberMU/CyberMU Authenticator")]
    public class CyberMUAuthenticator : NetworkAuthenticator
    {
        // DECISÃO TÉCNICA PROVISÓRIA: limites do MU original PENDENTES DE VERIFICAÇÃO.
        // Valores aprovados para agora: 4 a 10 caracteres, só letras e números.
        [Header("Limites (servidor) - PROVISÓRIOS, PENDENTES DE VERIFICAÇÃO")]
        [SerializeField, Min(1)] private int accountNameMinLength = 4;
        [SerializeField, Min(1)] private int accountNameMaxLength = 10;
        [SerializeField, Min(1)] private int passwordMinLength = 4;
        [SerializeField, Min(1)] private int passwordMaxLength = 10;

        // O cliente se desconecta sozinho ao receber a recusa; este prazo é só
        // a garantia do servidor. Com 1 s (valor do exemplo do Mirror), um
        // cliente travado por ~1 s recebia o "desconectar" junto com a resposta
        // e perdia a mensagem (visto no teste com o Player 2 a ~2 quadros/s).
        [Tooltip("Segundos que o servidor espera, após avisar a recusa, antes de forçar a desconexão.")]
        [SerializeField, Min(0f)] private float rejectDisconnectDelay = 3f;

        // Preenchidos pela tela de login antes de conectar.
        // NonSerialized: a senha NUNCA é salva na cena.
        [System.NonSerialized] public string accountName = string.Empty;
        [System.NonSerialized] public string password = string.Empty;

        // Última mensagem de recusa recebida pelo cliente (a tela de login mostra).
        public string LastRefusalMessage { get; private set; }

        public int AccountNameMaxLength => accountNameMaxLength;
        public int PasswordMaxLength => passwordMaxLength;

        // Conexões já recusadas, esperando o desconectar.
        private readonly HashSet<NetworkConnectionToClient> connectionsPendingDisconnect = new HashSet<NetworkConnectionToClient>();

        #region Servidor

        public override void OnStartServer()
        {
            connectionsPendingDisconnect.Clear();
            NetworkServer.RegisterHandler<LoginRequestMessage>(OnLoginRequest, false);
        }

        public override void OnStopServer()
        {
            NetworkServer.UnregisterHandler<LoginRequestMessage>();
            StopAllCoroutines();
            connectionsPendingDisconnect.Clear();
        }

        public override void OnServerAuthenticate(NetworkConnectionToClient conn)
        {
            // Nada a fazer: esperamos a LoginRequestMessage do cliente.
        }

        private void OnLoginRequest(NetworkConnectionToClient conn, LoginRequestMessage msg)
        {
            // Ignora pedidos repetidos de quem já entrou ou já foi recusado.
            if (conn.isAuthenticated || connectionsPendingDisconnect.Contains(conn)) return;

#if UNITY_SERVER || UNITY_EDITOR
            var rules = new AccountRules
            {
                AccountNameMinLength = accountNameMinLength,
                AccountNameMaxLength = accountNameMaxLength,
                PasswordMinLength = passwordMinLength,
                PasswordMaxLength = passwordMaxLength,
            };
            LoginResult result = AccountService.TryLogin(conn, msg.accountName, msg.password, rules);

            if (result.Accepted)
            {
                try
                {
                    // Guarda de qual conta é esta conexão (usado pelos próximos
                    // sistemas, como personagens, sem consultar o banco de novo).
                    conn.authenticationData = result.AccountId;
                    conn.Send(new LoginResponseMessage { code = LoginResultCode.Accepted, message = result.Message });
                    ServerAccept(conn);
                }
                catch
                {
                    // Algo falhou depois de a conta entrar na lista: tira de lá.
                    AccountService.MarkOffline(conn);
                    throw;
                }
            }
            else
            {
                RejectConnection(conn, result.Code, result.Message);
            }
#else
            // Build do jogador não tem código de banco: não pode ser servidor.
            RejectConnection(conn, LoginResultCode.ServerError, "Este programa não pode atuar como servidor.");
#endif
        }

        private void RejectConnection(NetworkConnectionToClient conn, LoginResultCode code, string message)
        {
            connectionsPendingDisconnect.Add(conn);
            conn.Send(new LoginResponseMessage { code = code, message = message });
            conn.isAuthenticated = false;
            StartCoroutine(DelayedReject(conn, rejectDisconnectDelay));
        }

        private IEnumerator DelayedReject(NetworkConnectionToClient conn, float waitTime)
        {
            yield return new WaitForSeconds(waitTime);
            ServerReject(conn); // desconecta
            yield return null;
            connectionsPendingDisconnect.Remove(conn);
        }

        #endregion

        #region Cliente

        public override void OnStartClient()
        {
            NetworkClient.RegisterHandler<LoginResponseMessage>(OnLoginResponse, false);
        }

        public override void OnStopClient()
        {
            NetworkClient.UnregisterHandler<LoginResponseMessage>();
        }

        public override void OnClientAuthenticate()
        {
            LastRefusalMessage = null;
            NetworkClient.Send(new LoginRequestMessage { accountName = accountName, password = password });
        }

        private void OnLoginResponse(LoginResponseMessage msg)
        {
            if (msg.code == LoginResultCode.Accepted)
            {
                ClientAccept();
            }
            else
            {
                // Recusa é um resultado normal (não é erro do programa).
                LastRefusalMessage = msg.message;
                Debug.Log($"[Login] Recusado pelo servidor: {msg.message}");
                ClientReject();
            }
        }

        #endregion
    }
}
