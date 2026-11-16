namespace CyberMU.Shared.Characters
{
    // ID neutro e estável de termos da ficha do personagem que aparecem na
    // tela e não são atributos (regra de nomes do CLAUDE.md). Só serve para
    // buscar o nome exibido em CharacterTermNames; não vai pela rede.
    public enum CharacterTermId
    {
        Health = 0,     // Vida (HP)
        Mana = 1,
        Experience = 2, // EXP
    }
}
