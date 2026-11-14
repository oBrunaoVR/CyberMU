using Mirror;

namespace CyberMU.Shared.Network
{
    // Mensagens de rede do login. Existem no servidor e no cliente.

    // Resultado do login, enviado pelo servidor ao cliente.
    public enum LoginResultCode : byte
    {
        Accepted = 0,
        WrongPassword = 1,
        AlreadyConnected = 2,
        InvalidAccountName = 3,
        InvalidPassword = 4,
        ServerError = 5,
    }

    // Cliente -> servidor: conta e senha digitadas.
    // ATENÇÃO: o Mirror não criptografa o tráfego (pendência registrada no
    // DECISIONS.md). A senha só é protegida (hash) dentro do banco.
    public struct LoginRequestMessage : NetworkMessage
    {
        public string accountName;
        public string password;
    }

    // Servidor -> cliente: aceito ou recusado, com texto para mostrar na tela.
    public struct LoginResponseMessage : NetworkMessage
    {
        public LoginResultCode code;
        public string message;
    }
}
