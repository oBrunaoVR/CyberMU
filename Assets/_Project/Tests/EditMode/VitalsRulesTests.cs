using CyberMU.Shared.Characters;
using NUnit.Framework;

namespace CyberMU.Tests.EditMode
{
    // Testes das fórmulas de Vida e Mana máximas (MU_099D_REFERENCE.md,
    // seção 8), com os parâmetros reais do ClassCatalog e os atributos de
    // criação de cada classe. Valores conferidos à mão.
    public class VitalsRulesTests
    {
        private static ClassDefinition Get(ClassId id)
        {
            Assert.IsTrue(ClassCatalog.TryGet(id, out ClassDefinition def));
            Assert.IsTrue(def.VitalsConfirmed, $"Vida/Mana de {id} deveriam estar confirmadas.");
            return def;
        }

        // a) Berserker nível 1 (28/20/25/10): 35 + 0 + 25×3 = 110;
        //    Mana 10 + 0 + 10×1 = 20.
        [Test]
        public void BerserkerNivel1()
        {
            ClassDefinition def = Get(ClassId.Berserker);
            Assert.AreEqual(110, VitalsRules.MaxHealth(def, 1, def.Vitality));
            Assert.AreEqual(20, VitalsRules.MaxMana(def, 1, def.Energy));
        }

        // b) Technomancer nível 1 (18/18/15/30): 45 + 0 + 15×1 = 60;
        //    Mana 0 + 0 + 30×2 = 60.
        [Test]
        public void TechnomancerNivel1()
        {
            ClassDefinition def = Get(ClassId.Technomancer);
            Assert.AreEqual(60, VitalsRules.MaxHealth(def, 1, def.Vitality));
            Assert.AreEqual(60, VitalsRules.MaxMana(def, 1, def.Energy));
        }

        // c) NeonRanger nível 1 (22/25/20/15): 60 + 0 + 20×1 = 80;
        //    Mana 7,5 + 0 + 15×1,5 = 30.
        [Test]
        public void NeonRangerNivel1()
        {
            ClassDefinition def = Get(ClassId.NeonRanger);
            Assert.AreEqual(80, VitalsRules.MaxHealth(def, 1, def.Vitality));
            Assert.AreEqual(30, VitalsRules.MaxMana(def, 1, def.Energy));
        }

        // d) Berserker nível 3 sem distribuir: 35 + 2×2 + 75 = 114;
        //    Mana 10 + 2×0,5 + 10 = 21.
        [Test]
        public void BerserkerNivel3()
        {
            ClassDefinition def = Get(ClassId.Berserker);
            Assert.AreEqual(114, VitalsRules.MaxHealth(def, 3, def.Vitality));
            Assert.AreEqual(21, VitalsRules.MaxMana(def, 3, def.Energy));
        }

        // e) Technomancer nível 3 sem distribuir: 45 + 2×1 + 15 = 62;
        //    Mana 0 + 2×2 + 60 = 64.
        [Test]
        public void TechnomancerNivel3()
        {
            ClassDefinition def = Get(ClassId.Technomancer);
            Assert.AreEqual(62, VitalsRules.MaxHealth(def, 3, def.Vitality));
            Assert.AreEqual(64, VitalsRules.MaxMana(def, 3, def.Energy));
        }

        // f) NeonRanger nível 3 sem distribuir: 60 + 2×1 + 20 = 82;
        //    Mana 7,5 + 2×1,5 + 22,5 = 33.
        [Test]
        public void NeonRangerNivel3()
        {
            ClassDefinition def = Get(ClassId.NeonRanger);
            Assert.AreEqual(82, VitalsRules.MaxHealth(def, 3, def.Vitality));
            Assert.AreEqual(33, VitalsRules.MaxMana(def, 3, def.Energy));
        }

        // g) Berserker nível 2: Mana 10 + 0,5 + 10 = 20,5 -> parte inteira 20
        //    (arredondamento para baixo).
        [Test]
        public void BerserkerNivel2ArredondaParaBaixo()
        {
            ClassDefinition def = Get(ClassId.Berserker);
            Assert.AreEqual(20, VitalsRules.MaxMana(def, 2, def.Energy));
        }

        // h) Berserker com +1 de Vitalidade: Vida sobe exatamente +3.
        [Test]
        public void BerserkerUmPontoDeVitalidadeDaTresDeVida()
        {
            ClassDefinition def = Get(ClassId.Berserker);
            int antes = VitalsRules.MaxHealth(def, 1, def.Vitality);
            int depois = VitalsRules.MaxHealth(def, 1, def.Vitality + 1);
            Assert.AreEqual(3, depois - antes);
        }
    }
}
