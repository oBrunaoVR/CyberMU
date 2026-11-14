// Só existe no servidor dedicado e no editor; não entra no build do jogador.
#if UNITY_SERVER || UNITY_EDITOR
using System.Collections.Generic;
using CyberMU.Shared.Characters;
using SQLite;

namespace CyberMU.Server.Characters
{
    public struct CharacterCreateResult
    {
        public CharacterCreateCode Code;
        public string Message;             // texto mostrado ao jogador
        public CharacterRecord Character;  // só preenchido quando aceito
        public bool Accepted => Code == CharacterCreateCode.Accepted;
    }

    // Regras de personagens no servidor: listar e criar.
    // Recebe a conexão do banco por parâmetro: o servidor passa o banco real,
    // os testes passam um banco temporário (nunca o real).
    public static class CharacterService
    {
        // FONTE DA ÉPOCA (MU_099D_REFERENCE.md, seção 6): 5 por conta.
        public const int MaxCharactersPerAccount = 5;

        public static List<CharacterRecord> ListByAccount(SQLiteConnection db, int accountId)
        {
            return CharacterRepository.ListByAccount(db, accountId);
        }

        public static CharacterCreateResult Create(
            SQLiteConnection db, int accountId, string name, ClassId classId, CharacterNameRules nameRules)
        {
            name = name ?? string.Empty;

            if (!nameRules.IsValid(name))
                return Refuse(CharacterCreateCode.NameInvalid, "Nome inválido");

            // Classe desconhecida (ex.: cliente alterado) conta como bloqueada.
            if (!ClassCatalog.TryGet(classId, out ClassDefinition def))
                return Refuse(CharacterCreateCode.ClassLocked, "Classe bloqueada");

            List<CharacterRecord> existing = CharacterRepository.ListByAccount(db, accountId);

            if (existing.Count >= MaxCharactersPerAccount)
                return Refuse(CharacterCreateCode.LimitReached, "Limite de 5 personagens");

            if (!IsClassUnlocked(def, existing))
                return Refuse(CharacterCreateCode.ClassLocked, "Classe bloqueada");

            if (CharacterRepository.NameExists(db, name))
                return Refuse(CharacterCreateCode.NameTaken, "Nome já em uso");

            var record = new CharacterRecord
            {
                AccountId = accountId,
                Slot = FirstFreeSlot(existing),
                Name = name,
                ClassId = (int)classId,
                Level = 1,
                Experience = 0,
                Strength = def.Strength,
                Agility = def.Agility,
                Vitality = def.Vitality,
                Energy = def.Energy,
                // Comando inicial do Overlord: PENDENTE DE VERIFICAÇÃO (a
                // criação dele está bloqueada); as outras classes não têm.
                Command = 0,
                LevelUpPoints = 0,
                CreatedAt = CharacterRepository.NowText(),
            };

            try
            {
                record.Id = CharacterRepository.Insert(db, record);
            }
            catch (SQLiteException)
            {
                // O índice único do banco barrou o nome (última linha de defesa).
                return Refuse(CharacterCreateCode.NameTaken, "Nome já em uso");
            }

            return new CharacterCreateResult
            {
                Code = CharacterCreateCode.Accepted,
                Message = "Personagem criado",
                Character = record,
            };
        }

        private static bool IsClassUnlocked(ClassDefinition def, List<CharacterRecord> existing)
        {
            // Atributos iniciais PENDENTES -> criação bloqueada, mesmo que a
            // conta cumpra o requisito de nível (não inventar valores).
            if (!def.InitialStatsConfirmed) return false;
            if (def.CreatableAtStart) return true;

            foreach (CharacterRecord c in existing)
            {
                if (c.Level >= def.UnlockAccountLevel) return true;
            }
            return false;
        }

        private static int FirstFreeSlot(List<CharacterRecord> existing)
        {
            for (int slot = 0; slot < MaxCharactersPerAccount; slot++)
            {
                bool used = false;
                foreach (CharacterRecord c in existing)
                {
                    if (c.Slot == slot) { used = true; break; }
                }
                if (!used) return slot;
            }
            return -1; // não acontece: o limite de 5 é checado antes
        }

        private static CharacterCreateResult Refuse(CharacterCreateCode code, string message)
        {
            return new CharacterCreateResult { Code = code, Message = message };
        }
    }
}
#endif
