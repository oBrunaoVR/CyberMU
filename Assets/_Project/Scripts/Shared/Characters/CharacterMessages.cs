using Mirror;

namespace CyberMU.Shared.Characters
{
    // Mensagens de rede de personagens. Existem no servidor e no cliente.
    // No servidor, são registradas com "exigir autenticação": o próprio
    // Mirror descarta pedidos de conexões que não passaram pelo login.

    // Resultado da criação, enviado pelo servidor ao cliente.
    public enum CharacterCreateCode : byte
    {
        Accepted = 0,
        LimitReached = 1,
        NameTaken = 2,
        NameInvalid = 3,
        ClassLocked = 4,
        ServerError = 5,
    }

    // Resumo de um personagem para o cliente (sem dados internos do banco).
    public struct CharacterSummary
    {
        public byte slot;
        public string name;
        public ClassId classId;
        public int level;
    }

    // Cliente -> servidor: "quero a lista dos meus personagens".
    public struct CharacterListRequestMessage : NetworkMessage { }

    // Servidor -> cliente: a lista de personagens da conta desta conexão.
    public struct CharacterListResponseMessage : NetworkMessage
    {
        public CharacterSummary[] characters;
    }

    // Cliente -> servidor: pedido de criação. O cliente só manda nome e
    // classe; slot, atributos e nível são decididos SOMENTE pelo servidor.
    public struct CharacterCreateRequestMessage : NetworkMessage
    {
        public string name;
        public ClassId classId;
    }

    // Servidor -> cliente: resultado da criação, com texto para a tela.
    public struct CharacterCreateResponseMessage : NetworkMessage
    {
        public CharacterCreateCode code;
        public string message;
        public CharacterSummary character; // só preenchido quando aceito
    }
}
