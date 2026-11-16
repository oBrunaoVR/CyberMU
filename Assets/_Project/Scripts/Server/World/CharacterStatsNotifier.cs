// Só existe no servidor dedicado e no editor; não entra no build do jogador.
#if UNITY_SERVER || UNITY_EDITOR
using CyberMU.Server.Characters;
using CyberMU.Shared.Characters;
using Mirror;

namespace CyberMU.Server.World
{
    // Envia os atributos de um personagem SOMENTE ao dono: conn.Send entrega
    // a mensagem só àquela conexão, nunca aos outros clientes.
    // Hoje é chamado na entrada no mundo; deve ser reutilizado sempre que os
    // atributos mudarem no servidor (subida de nível, distribuição de pontos),
    // para o cliente nunca calcular nada por conta própria.
    public static class CharacterStatsNotifier
    {
        public static void SendToOwner(NetworkConnectionToClient conn, CharacterRecord character)
        {
            // Vida e Mana máximas: calculadas de classe + nível + atributos
            // (VitalsRules, seção 8), nunca salvas no banco. Como esta função
            // roda em toda mudança (entrada no mundo, nível, pontos), o
            // recálculo acontece sempre que nível ou atributos mudam.
            // Classe com Vida/Mana PENDENTES: vai vitalsPending = true e o
            // cliente mostra "[A DEFINIR]" em vez de 0.
            bool vitalsKnown = VitalsRules.TryGetMax((ClassId)character.ClassId,
                character.Level, character.Vitality, character.Energy,
                out int maxHealth, out int maxMana);

            conn.Send(new CharacterStatsMessage
            {
                name = character.Name,
                classId = (ClassId)character.ClassId,
                level = character.Level,
                strength = character.Strength,
                agility = character.Agility,
                vitality = character.Vitality,
                energy = character.Energy,
                command = character.Command,
                levelUpPoints = character.LevelUpPoints,
                experience = character.Experience,
                // No nível máximo vai 0 (o cliente mostra "—").
                experienceForNextLevel = character.Level >= ExperienceRules.MaxLevel
                    ? 0
                    : ExperienceRules.TotalExperienceToLevelUp(character.Level),
                // TEMPORÁRIO: quando houver dano/gasto de mana, Vida e Mana
                // ATUAIS devem passar a existir no estado do personagem no
                // servidor (ActiveCharacterRegistry), não aqui.
                vitalsPending = !vitalsKnown,
                currentHealth = maxHealth,
                maxHealth = maxHealth,
                currentMana = maxMana,
                maxMana = maxMana,
            });
        }
    }
}
#endif
