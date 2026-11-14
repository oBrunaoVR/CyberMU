using CyberMU.Shared.Network;
using Mirror;
using UnityEngine;

namespace CyberMU.Client.UI
{
    // TELA DE LOGIN TEMPORÁRIA (só para testes; será trocada por uma UI de verdade).
    // Desenhada com OnGUI, do mesmo jeito simples do NetworkManagerHUD.
    // - Campos Conta e Senha (senha mascarada) e botão "Entrar" (conecta como cliente).
    // - O que estiver digitado também vale para o botão "Host" do HUD.
    // - Mostra a mensagem de recusa vinda do servidor.
    // Não decide nada: quem valida é o servidor.
    [AddComponentMenu("CyberMU/Login Screen (temporária)")]
    public class LoginScreen : MonoBehaviour
    {
        [SerializeField] private CyberMUAuthenticator authenticator;

        private string accountName = string.Empty;
        private string password = string.Empty;
        private string statusMessage;
        private bool wasConnecting;

        private void Awake()
        {
            if (authenticator == null)
                authenticator = FindAnyObjectByType<CyberMUAuthenticator>();
        }

        private void Update()
        {
            if (authenticator == null) return;

            // Detecta o fim de uma tentativa de conexão para mostrar o motivo.
            bool connecting = NetworkClient.active;
            if (wasConnecting && !connecting)
            {
                statusMessage = !string.IsNullOrEmpty(authenticator.LastRefusalMessage)
                    ? authenticator.LastRefusalMessage
                    : "Desconectado.";
            }
            wasConnecting = connecting;
        }

        private void OnGUI()
        {
            if (authenticator == null) return;

            // Some quando o cliente está ativo (conectando ou já no jogo) ou
            // quando este programa roda como Server Only (servidor não loga).
            if (NetworkClient.active || NetworkServer.active) return;

            const float width = 260f;
            const float height = 150f;
            var area = new Rect((Screen.width - width) / 2f, (Screen.height - height) / 2f, width, height);

            GUILayout.BeginArea(area, GUI.skin.box);
            GUILayout.Label("Login (temporário)");

            GUILayout.BeginHorizontal();
            GUILayout.Label("Conta", GUILayout.Width(50));
            accountName = GUILayout.TextField(accountName, authenticator.AccountNameMaxLength);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Senha", GUILayout.Width(50));
            password = GUILayout.PasswordField(password, '*', authenticator.PasswordMaxLength);
            GUILayout.EndHorizontal();

            // Mantém o authenticator com o que está digitado (o "Host" do HUD usa isso).
            authenticator.accountName = accountName;
            authenticator.password = password;

            if (GUILayout.Button("Entrar"))
            {
                statusMessage = null;
                NetworkManager.singleton.StartClient();
            }

            if (!string.IsNullOrEmpty(statusMessage))
                GUILayout.Label(statusMessage);

            GUILayout.EndArea();
        }
    }
}
