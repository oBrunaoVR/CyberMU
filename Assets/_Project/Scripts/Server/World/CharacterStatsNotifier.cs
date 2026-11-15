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
            });
        }
    }
}
#endif
