// Só existe no servidor dedicado e no editor; não entra no build do jogador.
#if UNITY_SERVER || UNITY_EDITOR
using System;
using System.Security.Cryptography;
using System.Text;

namespace CyberMU.Server.Accounts
{
    // Transforma a senha num "hash" (impressão digital) com PBKDF2-SHA256 e um
    // salt aleatório por conta. A senha em si nunca é guardada: no login,
    // calculamos o hash de novo com o mesmo salt e comparamos.
    // Responsabilidade ÚNICA: gerar salt, calcular hash e comparar hashes.
    public static class PasswordHasher
    {
        // Quantas vezes o PBKDF2 repete o cálculo (deixa ataques de força bruta
        // lentos). Fica gravado em cada conta (coluna HashIterations), então
        // pode subir no futuro sem quebrar as contas antigas.
        // DECISÃO TÉCNICA: 100.000 repetições (não é regra do MU).
        public const int DefaultIterations = 100_000;

        private const int SaltSizeBytes = 16; // 128 bits
        private const int HashSizeBytes = 32; // 256 bits (tamanho do SHA-256)

        public static byte[] CreateSalt()
        {
            byte[] salt = new byte[SaltSizeBytes];
            using (var rng = RandomNumberGenerator.Create())
                rng.GetBytes(salt);
            return salt;
        }

        public static byte[] ComputeHash(string password, byte[] salt, int iterations)
        {
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password ?? string.Empty);
            using (var pbkdf2 = new Rfc2898DeriveBytes(passwordBytes, salt, iterations, HashAlgorithmName.SHA256))
                return pbkdf2.GetBytes(HashSizeBytes);
        }

        // Confere a senha digitada contra o hash guardado.
        public static bool Verify(string password, byte[] salt, int iterations, byte[] expectedHash)
        {
            byte[] actual = ComputeHash(password, salt, iterations);
            return FixedTimeEquals(actual, expectedHash);
        }

        // Compara sem "sair cedo" no primeiro byte diferente, para o tempo da
        // comparação não revelar nada sobre o hash.
        private static bool FixedTimeEquals(byte[] a, byte[] b)
        {
            if (a == null || b == null || a.Length != b.Length) return false;
            int diff = 0;
            for (int i = 0; i < a.Length; i++)
                diff |= a[i] ^ b[i];
            return diff == 0;
        }
    }
}
#endif
