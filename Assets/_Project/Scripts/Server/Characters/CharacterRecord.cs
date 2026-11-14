// Só existe no servidor dedicado e no editor; não entra no build do jogador.
#if UNITY_SERVER || UNITY_EDITOR
using SQLite;

namespace CyberMU.Server.Characters
{
    // Uma linha da tabela Characters (criada na versão 3 do banco).
    // Datas em texto ISO 8601 (UTC), como na tabela Accounts.
    // SEM vida, mana, mapa ou posição: PENDENTES DE VERIFICAÇÃO (entram em
    // versões futuras do banco quando houver valores confirmados).
    [Table("Characters")]
    public class CharacterRecord
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public int Slot { get; set; }
        public string Name { get; set; }
        public int ClassId { get; set; }
        public int Level { get; set; }
        public long Experience { get; set; }
        public int Strength { get; set; }
        public int Agility { get; set; }
        public int Vitality { get; set; }
        public int Energy { get; set; }
        public int Command { get; set; }
        public int LevelUpPoints { get; set; }
        public string CreatedAt { get; set; }
    }
}
#endif
