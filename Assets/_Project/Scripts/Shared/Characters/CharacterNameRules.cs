namespace CyberMU.Shared.Characters
{
    // DECISÃO TÉCNICA PROVISÓRIA: regras de nome do MU original PENDENTES DE
    // VERIFICAÇÃO. Valores aprovados para agora: 4 a 10 caracteres, só letras
    // e números do alfabeto básico (A-Z, a-z, 0-9), sem acentos.
    // Configurável: quem chama o serviço pode passar outros limites.
    public struct CharacterNameRules
    {
        public int MinLength;
        public int MaxLength;

        public static CharacterNameRules Default =>
            new CharacterNameRules { MinLength = 4, MaxLength = 10 };

        public bool IsValid(string name)
        {
            if (name == null) return false;
            if (name.Length < MinLength || name.Length > MaxLength) return false;
            foreach (char c in name)
            {
                bool ok = (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9');
                if (!ok) return false;
            }
            return true;
        }
    }
}
