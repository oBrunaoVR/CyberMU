namespace CyberMU.Shared.Characters
{
    // ID neutro e estável de cada classe (regra de nomes do CLAUDE.md).
    // O número é gravado no banco (coluna ClassId dos personagens):
    // NUNCA mudar os valores depois de publicados, para não quebrar saves.
    // A ponte nome original -> ID -> nome exibido está em docs/foundation/NAMING.md.
    public enum ClassId : byte
    {
        None = 0, // "nenhuma classe": valor inválido em pedidos de criação
        Berserker = 1,
        Technomancer = 2,
        NeonRanger = 3,
        Ronin = 4,
        Overlord = 5,
    }
}
