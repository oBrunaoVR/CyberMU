using System;
using CyberMU.Shared.Characters;
using NUnit.Framework;

namespace CyberMU.Tests.EditMode
{
    // Regra de nomes (CLAUDE.md / NAMING.md): todo ID que aparece na tela
    // precisa ter nome na tabela de nomes. Se alguém criar um ID novo e
    // esquecer o nome, este teste falha em vez de aparecer "[A DEFINIR]".
    public class NameTablesTests
    {
        [Test]
        public void TodoAtributoTemNomeExibido()
        {
            foreach (StatId id in Enum.GetValues(typeof(StatId)))
                Assert.AreNotEqual("[A DEFINIR]", StatNames.GetDisplayName(id), $"StatId.{id} sem nome");
        }

        [Test]
        public void TodoTermoDaFichaTemNomeExibido()
        {
            foreach (CharacterTermId id in Enum.GetValues(typeof(CharacterTermId)))
                Assert.AreNotEqual("[A DEFINIR]", CharacterTermNames.GetDisplayName(id), $"CharacterTermId.{id} sem nome");
        }

        // Mesmos textos da coluna "Nome exibido no jogo" do NAMING.md.
        [Test]
        public void NomesBatemComNaming()
        {
            Assert.AreEqual("Força", StatNames.GetDisplayName(StatId.Strength));
            Assert.AreEqual("Agilidade", StatNames.GetDisplayName(StatId.Agility));
            Assert.AreEqual("Vitalidade", StatNames.GetDisplayName(StatId.Vitality));
            Assert.AreEqual("Energia", StatNames.GetDisplayName(StatId.Energy));
            Assert.AreEqual("Comando", StatNames.GetDisplayName(StatId.Command));
            Assert.AreEqual("Vida", CharacterTermNames.GetDisplayName(CharacterTermId.Health));
            Assert.AreEqual("Mana", CharacterTermNames.GetDisplayName(CharacterTermId.Mana));
            Assert.AreEqual("EXP", CharacterTermNames.GetDisplayName(CharacterTermId.Experience));
        }
    }
}
