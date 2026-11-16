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

        // Quais classes contam para o desbloqueio acima. null = qualquer
        // classe da conta conta (ex.: Overlord, "personagem de nível 250").
        public ClassId[] UnlockQualifyingClasses;

        // true = um personagem desta classe conta para o desbloqueio.
        public bool CountsForUnlock(ClassId classId)
        {
            if (UnlockQualifyingClasses == null) return true;
            foreach (ClassId id in UnlockQualifyingClasses)
            {
                if (id == classId) return true;
            }
            return false;
        }

        // Só o Overlord tem o atributo Comando.
        public bool HasCommand;

        // false = atributos iniciais PENDENTES DE VERIFICAÇÃO; a criação fica
        // bloqueada até haver valores confirmados (regra do CLAUDE.md:
        // não inventar valores).
        public bool InitialStatsConfirmed;

        // Vida e Mana máximas (MU_099D_REFERENCE.md, seção 8):
        //   Máximo = Base + (Nível − 1) × PorNível + Atributo × PorPonto
        // (Vida usa Vitalidade; Mana usa Energia). São DADOS da classe;
        // a conta fica num lugar só (VitalsRules). Frações existem (ex.:
        // 0,5 de Mana por nível no Berserker).
        public float HealthBase;
        public float HealthPerLevel;
        public float HealthPerVitality;
        public float ManaBase;
        public float ManaPerLevel;
        public float ManaPerEnergy;

        // false = valores de Vida/Mana PENDENTES (hoje só o Overlord).
        public bool VitalsConfirmed;
    }

    // Dados de todas as classes num lugar só, conforme MU_099D_REFERENCE.md
    // (seção 6 e "Atributos iniciais"). Mudar um valor = mexer só aqui.
    public static class ClassCatalog
    {
        private static readonly ClassDefinition[] all =
        {
            // CONSENSO CLÁSSICO (três fontes concordam).
            // Vida/Mana (seção 8): CONSENSO CLÁSSICO — nível 1 dá 110/20.
            new ClassDefinition
            {
                Id = ClassId.Berserker,
                Strength = 28, Agility = 20, Vitality = 25, Energy = 10,
                PointsPerLevel = 5,
                CreatableAtStart = true,
                InitialStatsConfirmed = true,
                HealthBase = 35f, HealthPerLevel = 2f, HealthPerVitality = 3f,
                ManaBase = 10f, ManaPerLevel = 0.5f, ManaPerEnergy = 1f,
                VitalsConfirmed = true,
            },
            // CONSENSO CLÁSSICO (três fontes concordam).
            // Vida/Mana (seção 8): nível 1 dá 60/60. HP por Vitalidade = 1
            // por DECISÃO DO DONO DO PROJETO (2026-11-15): fontes divergem e
            // guias atuais dizem 2. Valor configurável: mudar = mexer só aqui.
            new ClassDefinition
            {
                Id = ClassId.Technomancer,
                Strength = 18, Agility = 18, Vitality = 15, Energy = 30,
                PointsPerLevel = 5,
                CreatableAtStart = true,
                InitialStatsConfirmed = true,
                HealthBase = 45f, HealthPerLevel = 1f, HealthPerVitality = 1f,
                ManaBase = 0f, ManaPerLevel = 2f, ManaPerEnergy = 2f,
                VitalsConfirmed = true,
            },
            // CONSENSO CLÁSSICO com uma fonte principal; reconfirmar.
            // Vida/Mana (seção 8): nível 1 dá 80/30. HP por Vitalidade = 1
            // por DECISÃO DO DONO DO PROJETO (2026-11-15): fontes divergem e
            // guias atuais dizem 2. Valor configurável: mudar = mexer só aqui.
            new ClassDefinition
            {
                Id = ClassId.NeonRanger,
                Strength = 22, Agility = 25, Vitality = 20, Energy = 15,
                PointsPerLevel = 5,
                CreatableAtStart = true,
                InitialStatsConfirmed = true,
                HealthBase = 60f, HealthPerLevel = 1f, HealthPerVitality = 1f,
                ManaBase = 7.5f, ManaPerLevel = 1.5f, ManaPerEnergy = 1.5f,
                VitalsConfirmed = true,
            },
            // FONTE DA ÉPOCA (seção 6): desbloqueia quando um DK, DW ou Elf
            // (Berserker, Technomancer, NeonRanger) da MESMA conta passa do
            // nível 220 (ou seja, nível 221 ou mais). Outras classes não contam.
            // Atributos iniciais PENDENTES DE VERIFICAÇÃO -> criação bloqueada.
            // Vida/Mana (seção 8, CONSENSO CLÁSSICO): as fórmulas por nível e
            // por ponto e a Base de Mana 8 vêm direto da fórmula citada
            // (Mana = 8 + (Nv−1) + Ene×2). O que depende da Energia inicial
            // PENDENTE (26 ou 16) é só a Mana na criação; a classe segue
            // bloqueada até decidir.
            new ClassDefinition
            {
                Id = ClassId.Ronin,
                PointsPerLevel = 7,
                CreatableAtStart = false,
                UnlockAccountLevel = 221,
                UnlockQualifyingClasses = new[]
                {
                    ClassId.Berserker, ClassId.Technomancer, ClassId.NeonRanger,
                },
                InitialStatsConfirmed = false,
                HealthBase = 58f, HealthPerLevel = 1f, HealthPerVitality = 2f,
                ManaBase = 8f, ManaPerLevel = 1f, ManaPerEnergy = 2f,
                VitalsConfirmed = true,
            },
            // FONTE DA ÉPOCA: desbloqueia com personagem da conta de nível 250+
            // (a fonte não limita a classe: UnlockQualifyingClasses = null).
            // Atributos iniciais (inclusive Comando) PENDENTES DE VERIFICAÇÃO
            // -> criação bloqueada.
            // Vida/Mana (seção 8): PENDENTES (sem valores; classe bloqueada).
            new ClassDefinition
            {
                Id = ClassId.Overlord,
                PointsPerLevel = 7,
                CreatableAtStart = false,
                UnlockAccountLevel = 250,
                HasCommand = true,
                InitialStatsConfirmed = false,
                VitalsConfirmed = false,
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
