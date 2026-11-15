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

    // Resultado do pedido de entrar no mundo, enviado pelo servidor.
    public enum EnterWorldCode : byte
    {
        Accepted = 0,
        InvalidCharacter = 1, // não existe ou não pertence a esta conta
        AlreadyInWorld = 2,
        ServerError = 3,
    }

    // Resumo de um personagem para o cliente (sem dados internos do banco).
    // O id é usado pelo cliente só para dizer "quero entrar com ESTE";
    // o servidor sempre confere se o personagem é mesmo da conta.
    public struct CharacterSummary
    {
        public int id;
        public byte slot;
        public string name;
        public ClassId classId;
        public int level;
    }

    // Cliente -> servidor: "quero a lista dos meus personagens".
    public struct CharacterListRequestMessage : NetworkMessage { }

    // Servidor -> cliente: a lista de personagens da conta desta conexão e as
    // classes desbloqueadas para criação. O SERVIDOR calcula o desbloqueio;
    // o cliente só mostra (classe bloqueada fica escondida, como no original).
    public struct CharacterListResponseMessage : NetworkMessage
    {
        public CharacterSummary[] characters;
        public ClassId[] unlockedClasses;
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

    // Cliente -> servidor: "quero entrar no mundo com este personagem".
    public struct EnterWorldRequestMessage : NetworkMessage
    {
        public int characterId;
    }

    // Servidor -> cliente: só é útil na recusa (com texto para a tela).
    // Quando aceito, o próprio nascimento da cápsula avisa o cliente.
    public struct EnterWorldResponseMessage : NetworkMessage
    {
        public EnterWorldCode code;
        public string message;
    }

    // Servidor -> cliente: atributos do personagem, enviados SOMENTE ao dono
    // (conn.Send entrega só àquela conexão). Os outros jogadores nunca recebem
    // isto: deles, só o nome chega, pela SyncVar do PlayerNameTag.
    // Sem Id interno nem AccountId: o cliente não precisa de dados do banco.
    public struct CharacterStatsMessage : NetworkMessage
    {
        public string name;
        public ClassId classId; // o cliente converte em nome exibido (NAMING.md)
        public int level;
        public int strength;
        public int agility;
        public int vitality;
        public int energy;
        public int command;       // só tem significado para Overlord
        public int levelUpPoints; // pontos disponíveis para distribuir
    }
}
