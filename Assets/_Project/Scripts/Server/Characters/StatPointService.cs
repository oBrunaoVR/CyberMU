// Só existe no servidor dedicado e no editor; não entra no build do jogador.
#if UNITY_SERVER || UNITY_EDITOR
using CyberMU.Shared.Characters;
using SQLite;

namespace CyberMU.Server.Characters
{
    // Por que a resposta é um código e não só verdadeiro/falso: quem chama
    // (StatPointHandler) loga o MOTIVO da recusa, para investigar depois.
    public enum StatIncreaseCode : byte
    {
        Accepted = 0,
        NoPointsAvailable = 1, // sem pontos disponíveis
        InvalidStat = 2,       // atributo desconhecido ou não é da classe
        StatAtMax = 3,         // já está no teto de 32767
    }

    // Distribuição de pontos de atributo no SERVIDOR (MU_099D_REFERENCE.md,
    // seções 6 e 7). Trabalha sobre o MESMO CharacterRecord em memória que o
    // ExperienceService: o do ActiveCharacterRegistry, entregue pelo
    // atendimento. Nenhum dos dois relê ou guarda cópia própria, então um
    // nunca sobrescreve o que o outro gravou.
    // Se válido: soma no atributo, desconta dos disponíveis e grava no banco.
    // Se inválido: recusa SEM alterar nada (nem memória, nem banco).
    // Não existe tirar ponto de atributo (não está na referência).
    public static class StatPointService
    {
        // PENDENTE DE VERIFICAÇÃO: quantidade por clique no MU 0.99D.
        // O projeto usa 1; mudar o valor = mexer só aqui.
        public const int PointsPerClick = 1;

        // Seção 7: teto de cada atributo.
        public const int MaxStatValue = 32767;

        public static StatIncreaseCode IncreaseStat(
            SQLiteConnection db, CharacterRecord character, StatId stat)
        {
            if (character == null) return StatIncreaseCode.InvalidStat;

            // 1) O atributo existe e é válido para a classe?
            int current;
            switch (stat)
            {
                case StatId.Strength: current = character.Strength; break;
                case StatId.Agility: current = character.Agility; break;
                case StatId.Vitality: current = character.Vitality; break;
                case StatId.Energy: current = character.Energy; break;
                case StatId.Command:
                    // Comando só para a classe que o tem (ClassCatalog diz
                    // qual; hoje, só Overlord). Classe desconhecida recusa.
                    if (!ClassCatalog.TryGet((ClassId)character.ClassId, out ClassDefinition def)
                        || !def.HasCommand)
                        return StatIncreaseCode.InvalidStat;
                    current = character.Command;
                    break;
                default:
                    return StatIncreaseCode.InvalidStat;
            }

            // 2) Há pontos disponíveis?
            if (character.LevelUpPoints < PointsPerClick)
                return StatIncreaseCode.NoPointsAvailable;

            // 3) Não pode passar do teto.
            if (current + PointsPerClick > MaxStatValue)
                return StatIncreaseCode.StatAtMax;

            // Tudo válido: aplica e grava.
            int updated = current + PointsPerClick;
            switch (stat)
            {
                case StatId.Strength: character.Strength = updated; break;
                case StatId.Agility: character.Agility = updated; break;
                case StatId.Vitality: character.Vitality = updated; break;
                case StatId.Energy: character.Energy = updated; break;
                case StatId.Command: character.Command = updated; break;
            }
            character.LevelUpPoints -= PointsPerClick;

            CharacterRepository.SaveStats(db, character);
            return StatIncreaseCode.Accepted;
        }
    }
}
#endif
