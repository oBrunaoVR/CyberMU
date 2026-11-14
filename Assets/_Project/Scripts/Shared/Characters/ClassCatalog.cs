using System.Collections.Generic;

namespace CyberMU.Shared.Characters
{
    // A "ficha" de uma classe: só dados de criação, nenhuma lógica.
    public struct ClassDefinition
    {
        public ClassId Id;

        // Atributos iniciais (0 quando PENDENTES DE VERIFICAÇÃO).
        public int Strength;
        public int Agility;
        public int Vitality;
        public int Energy;

        public int PointsPerLevel;

        // true = pode ser criada numa conta nova, sem requisito.
        public bool CreatableAtStart;

        // Para classes bloqueadas no início: menor nível que ALGUM personagem
        // da conta precisa ter para desbloquear a criação.
        public int UnlockAccountLevel;

        // Só o Overlord tem o atributo Comando.
        public bool HasCommand;

        // false = atributos iniciais PENDENTES DE VERIFICAÇÃO; a criação fica
        // bloqueada até haver valores confirmados (regra do CLAUDE.md:
        // não inventar valores).
        public bool InitialStatsConfirmed;
    }

    // Dados de todas as classes num lugar só, conforme MU_099D_REFERENCE.md
    // (seção 6 e "Atributos iniciais"). Mudar um valor = mexer só aqui.
    public static class ClassCatalog
    {
        private static readonly ClassDefinition[] all =
        {
            // CONSENSO CLÁSSICO (três fontes concordam).
            new ClassDefinition
            {
                Id = ClassId.Berserker,
                Strength = 28, Agility = 20, Vitality = 25, Energy = 10,
                PointsPerLevel = 5,
                CreatableAtStart = true,
                InitialStatsConfirmed = true,
            },
            // CONSENSO CLÁSSICO (três fontes concordam).
            new ClassDefinition
            {
                Id = ClassId.Technomancer,
                Strength = 18, Agility = 18, Vitality = 15, Energy = 30,
                PointsPerLevel = 5,
                CreatableAtStart = true,
                InitialStatsConfirmed = true,
            },
            // CONSENSO CLÁSSICO com uma fonte principal; reconfirmar.
            new ClassDefinition
            {
                Id = ClassId.NeonRanger,
                Strength = 22, Agility = 25, Vitality = 20, Energy = 15,
                PointsPerLevel = 5,
                CreatableAtStart = true,
                InitialStatsConfirmed = true,
            },
            // FONTE DA ÉPOCA: desbloqueia quando um personagem da conta passa
            // do nível 220 (ou seja, nível 221 ou mais).
            // Atributos iniciais PENDENTES DE VERIFICAÇÃO -> criação bloqueada.
            new ClassDefinition
            {
                Id = ClassId.Ronin,
                PointsPerLevel = 7,
                CreatableAtStart = false,
                UnlockAccountLevel = 221,
                InitialStatsConfirmed = false,
            },
            // FONTE DA ÉPOCA: desbloqueia com personagem da conta de nível 250+.
            // Atributos iniciais (inclusive Comando) PENDENTES DE VERIFICAÇÃO
            // -> criação bloqueada.
            new ClassDefinition
            {
                Id = ClassId.Overlord,
                PointsPerLevel = 7,
                CreatableAtStart = false,
                UnlockAccountLevel = 250,
                HasCommand = true,
                InitialStatsConfirmed = false,
            },
        };

        public static IReadOnlyList<ClassDefinition> All => all;

        public static bool TryGet(ClassId id, out ClassDefinition definition)
        {
            foreach (ClassDefinition def in all)
            {
                if (def.Id == id)
                {
                    definition = def;
                    return true;
                }
            }
            definition = default;
            return false;
        }
    }
}
