namespace CyberMU.Shared.Characters
{
    // A fórmula de experiência num lugar SÓ (MU_099D_REFERENCE.md, seção 7).
    // Este arquivo existe no servidor e no cliente, mas quem calcula e decide
    // é sempre o SERVIDOR; o cliente recebe os valores prontos pela
    // CharacterStatsMessage e só mostra.
    public static class ExperienceRules
    {
        // PENDENTE DE VERIFICAÇÃO para a 0.99D (CONSENSO em guias clássicos).
        // No nível máximo o personagem não ganha mais EXP: DECISÃO TÉCNICA
        // (DECISIONS.md) — PENDENTE DE VERIFICAÇÃO, não vem da referência.
        public const int MaxLevel = 400;

        // EXP TOTAL acumulada que o personagem precisa ter para sair do
        // nível N (ir para N+1):
        //   N até 255:      (N + 9) × N × N × 10
        //   N acima de 255: [(N−255)+9] × (N−255) × (N−255) × 1000
        //                   + (N + 9) × N × N × 10
        // Em long (64 bits): perto do nível 400 os valores passam de
        // 9 bilhões e não cabem num int comum (32 bits).
        public static long TotalExperienceToLevelUp(int level)
        {
            long n = level;
            long total = (n + 9) * n * n * 10;
            if (level > 255)
            {
                long m = level - 255;
                total += (m + 9) * m * m * 1000;
            }
            return total;
        }
    }
}
