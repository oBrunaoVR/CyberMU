namespace CyberMU.Shared.Characters
{
    // Fórmulas de Vida e Mana MÁXIMAS num lugar só (MU_099D_REFERENCE.md,
    // seção 8). Os parâmetros por classe são DADOS do ClassCatalog; aqui
    // fica só a conta:
    //   Máximo = Base + (Nível − 1) × PorNível + Atributo × PorPonto
    // (Vida usa Vitalidade; Mana usa Energia).
    // Este arquivo existe no servidor e no cliente, mas quem calcula e envia
    // é sempre o SERVIDOR; o cliente recebe pronto pela CharacterStatsMessage.
    // Nada disto é salvo no banco: recalcula-se de classe + nível + atributos.
    public static class VitalsRules
    {
        public static int MaxHealth(in ClassDefinition def, int level, int vitality)
        {
            return Floor(def.HealthBase
                + (level - 1) * (double)def.HealthPerLevel
                + vitality * (double)def.HealthPerVitality);
        }

        public static int MaxMana(in ClassDefinition def, int level, int energy)
        {
            return Floor(def.ManaBase
                + (level - 1) * (double)def.ManaPerLevel
                + energy * (double)def.ManaPerEnergy);
        }

        // Versão segura para quem envia ao jogador: devolve false (e 0/0)
        // quando a classe não existe no catálogo ou tem Vida/Mana PENDENTES
        // (hoje só o Overlord). Assim nunca se mostra "0" como valor real.
        public static bool TryGetMax(ClassId classId, int level, int vitality, int energy,
            out int maxHealth, out int maxMana)
        {
            maxHealth = 0;
            maxMana = 0;
            if (!ClassCatalog.TryGet(classId, out ClassDefinition def) || !def.VitalsConfirmed)
                return false;
            maxHealth = MaxHealth(def, level, vitality);
            maxMana = MaxMana(def, level, energy);
            return true;
        }

        // Calcula com fração e usa a parte inteira (arredonda para baixo:
        // 20,5 vira 20). DECISÃO TÉCNICA — PENDENTE DE VERIFICAÇÃO como o
        // original arredondava.
        private static int Floor(double value)
        {
            return (int)System.Math.Floor(value);
        }
    }
}
