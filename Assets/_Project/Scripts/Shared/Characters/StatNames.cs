namespace CyberMU.Shared.Characters
{
    // Tabela de nomes exibidos dos atributos (docs/foundation/NAMING.md).
    // O jogador só vê o que está aqui; trocar um nome = mexer só aqui,
    // nenhuma lógica depende destes textos.
    // Rótulos PROVISÓRIOS: idioma da interface ainda não definido
    // (DECISIONS.md, 2026-11-14).
    public static class StatNames
    {
        public static string GetDisplayName(StatId id)
        {
            switch (id)
            {
                case StatId.Strength: return "Força";
                case StatId.Agility: return "Agilidade";
                case StatId.Vitality: return "Vitalidade";
                case StatId.Energy: return "Energia";
                case StatId.Command: return "Comando";
                default: return "[A DEFINIR]";
            }
        }
    }
}
