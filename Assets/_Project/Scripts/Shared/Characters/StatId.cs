namespace CyberMU.Shared.Characters
{
    // ID neutro e estável de cada atributo (regra de nomes do CLAUDE.md).
    // Vai pela rede no pedido de distribuição de pontos: o cliente diz SÓ
    // qual atributo quer aumentar; quantidade e validação são do servidor.
    // NUNCA mudar os valores depois de publicados.
    public enum StatId : byte
    {
        Strength = 0,
        Agility = 1,
        Vitality = 2,
        Energy = 3,
        Command = 4, // só existe para a classe com Comando (Overlord)
    }
}
