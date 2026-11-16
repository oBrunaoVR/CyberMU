namespace CyberMU.Shared.Characters
{
    // Tabela de nomes exibidos dos termos da ficha (Vida, Mana, EXP),
    // conforme docs/foundation/NAMING.md. Trocar um nome = mexer só aqui.
    // Rótulos PROVISÓRIOS: idioma da interface ainda não definido
    // (DECISIONS.md, 2026-11-14).
    public static class CharacterTermNames
    {
        public static string GetDisplayName(CharacterTermId id)
        {
            switch (id)
            {
                case CharacterTermId.Health: return "Vida";
                case CharacterTermId.Mana: return "Mana";
                case CharacterTermId.Experience: return "EXP";
                default: return "[A DEFINIR]";
            }
        }
    }
}
