using CyberMU.Shared.Characters;
using NUnit.Framework;

namespace CyberMU.Tests.EditMode
{
    // Testes da fórmula de EXP (MU_099D_REFERENCE.md, seção 7).
    // Valores conferidos à mão com a fórmula.
    public class ExperienceRulesTests
    {
        // (N + 9) × N × N × 10, para N até 255.
        [Test]
        public void FormulaAte255()
        {
            // Nível 1: (1+9) × 1 × 1 × 10 = 100.
            Assert.AreEqual(100, ExperienceRules.TotalExperienceToLevelUp(1));

            // Nível 2: (2+9) × 2 × 2 × 10 = 440.
            Assert.AreEqual(440, ExperienceRules.TotalExperienceToLevelUp(2));

            // Nível 179: (179+9) × 179 × 179 × 10 = 60.237.080.
            // (O exemplo da RageZone, 6.023.708, omite o ×10 — ver a nota na
            // seção 7 da referência.)
            Assert.AreEqual(60_237_080, ExperienceRules.TotalExperienceToLevelUp(179));
        }

        // Acima de 255 entra a parcela extra ×1000. Conferido à mão:
        // Nível 300: [(300−255)+9] × 45 × 45 × 1000 = 54 × 2025 × 1000
        //            = 109.350.000
        //          + (300+9) × 300 × 300 × 10 = 278.100.000
        //          = 387.450.000.
        [Test]
        public void FormulaAcimaDe255()
        {
            Assert.AreEqual(387_450_000, ExperienceRules.TotalExperienceToLevelUp(300));
        }

        // Nível 399 (o último antes do máximo): 3.822.148.080. Não cabe num
        // int (que para em ~2,1 bilhões) — por isso a fórmula usa long.
        [Test]
        public void FormulaPrecisaDeLong()
        {
            Assert.AreEqual(3_822_148_080L, ExperienceRules.TotalExperienceToLevelUp(399));
            Assert.Greater(ExperienceRules.TotalExperienceToLevelUp(399), (long)int.MaxValue);
        }
    }
}
