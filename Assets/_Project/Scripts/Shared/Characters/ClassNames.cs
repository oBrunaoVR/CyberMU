namespace CyberMU.Shared.Characters
{
    // Tabela de nomes exibidos das classes (docs/foundation/NAMING.md).
    // O jogador só vê o que está aqui; trocar um nome = mexer só aqui,
    // nenhuma lógica depende destes textos.
    public static class ClassNames
    {
        public static string GetDisplayName(ClassId id)
        {
            switch (id)
            {
                case ClassId.Berserker: return "Cybernetic Berserker";
                case ClassId.Technomancer: return "Technomancer";
                case ClassId.NeonRanger: return "Neon Ranger";
                case ClassId.Ronin: return "Cybernetic Ronin";
                case ClassId.Overlord: return "Corporate Overlord";
                default: return "[A DEFINIR]";
            }
        }
    }
}
