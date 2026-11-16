// Só existe no servidor dedicado e no editor; não entra no build do jogador.
#if UNITY_SERVER || UNITY_EDITOR
using CyberMU.Shared.Characters;
using SQLite;

namespace CyberMU.Server.Characters
{
    // O que aconteceu num ganho de EXP (para o chamador logar e avisar).
    public struct ExperienceGrantResult
    {
        public bool Changed;      // false = nada mudou (ex.: já está no nível máximo)
        public int LevelsGained;
        public int PointsGained;
    }

    // Ganho de experiência no SERVIDOR (MU_099D_REFERENCE.md, seção 7).
    // Soma a EXP, sobe quantos níveis couberem, soma os pontos da classe
    // (ClassCatalog) e grava no banco. Quem chama decide avisar o dono
    // (CharacterStatsNotifier.SendToOwner) — os testes chamam sem rede.
    public static class ExperienceService
    {
        public static ExperienceGrantResult GrantExperience(
            SQLiteConnection db, CharacterRecord character, long amount)
        {
            var result = new ExperienceGrantResult();
            if (character == null || amount <= 0) return result;

            // DECISÃO TÉCNICA (DECISIONS.md, 2026-11-15) — PENDENTE DE
            // VERIFICAÇÃO: no nível máximo, não ganha mais EXP. A seção 7 da
            // referência só traz o nível máximo (400, PENDENTE).
            if (character.Level >= ExperienceRules.MaxLevel) return result;

            // Classe desconhecida não dá pontos (não acontece com dados do
            // nosso servidor; é só defesa contra banco editado à mão).
            int pointsPerLevel = ClassCatalog.TryGet((ClassId)character.ClassId,
                out ClassDefinition def) ? def.PointsPerLevel : 0;

            character.Experience += amount;

            // EXP acumulada: sobe enquanto o total alcançar a fórmula do
            // nível atual. Uma EXP grande pode subir vários níveis de uma vez.
            while (character.Level < ExperienceRules.MaxLevel &&
                   character.Experience >= ExperienceRules.TotalExperienceToLevelUp(character.Level))
            {
                character.Level++;
                result.LevelsGained++;
                result.PointsGained += pointsPerLevel;
            }
            character.LevelUpPoints += result.PointsGained;

            // DECISÃO TÉCNICA: ao chegar no nível máximo, a EXP fica travada
            // no total necessário para alcançá-lo (a sobra é descartada,
            // porque "no nível máximo não ganha mais EXP").
            if (character.Level >= ExperienceRules.MaxLevel)
            {
                long cap = ExperienceRules.TotalExperienceToLevelUp(ExperienceRules.MaxLevel - 1);
                if (character.Experience > cap) character.Experience = cap;
            }

            CharacterRepository.SaveProgress(db, character);
            result.Changed = true;
            return result;
        }
    }
}
#endif
