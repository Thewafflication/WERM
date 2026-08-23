using System;
using System.Security.Cryptography;

namespace Werm.Core.Security
{
    public sealed class Pbkdf2PasswordHasher : IPasswordHasher
    {
        public const string AlgorithmName = "PBKDF2-HMAC-SHA512";
        public const int DefaultIterationCount = 220000;
        public const int SaltLength = 32;
        public const int HashLength = 64;

        public PasswordCredential Create(string password)
        {
            PasswordPolicy.Validate(password);
            var salt = new byte[SaltLength];
            using (RandomNumberGenerator generator = RandomNumberGenerator.Create())
            {
                generator.GetBytes(salt);
            }

            byte[] hash = Derive(password, salt, DefaultIterationCount);
            return new PasswordCredential(AlgorithmName, DefaultIterationCount, salt, hash);
        }

        public bool Verify(string password, PasswordCredential credential)
        {
            if (password == null || credential == null ||
                !string.Equals(credential.Algorithm, AlgorithmName, StringComparison.Ordinal) ||
                credential.IterationCount <= 0 || password.Length > PasswordPolicy.MaximumLength)
            {
                return false;
            }

            byte[] expected = credential.GetHash();
            byte[] actual = Derive(password, credential.GetSalt(), credential.IterationCount);
            return FixedTimeEquals(expected, actual);
        }

        private static byte[] Derive(string password, byte[] salt, int iterationCount)
        {
            using (var deriveBytes = new Rfc2898DeriveBytes(
                password,
                salt,
                iterationCount,
                HashAlgorithmName.SHA512))
            {
                return deriveBytes.GetBytes(HashLength);
            }
        }

        private static bool FixedTimeEquals(byte[] left, byte[] right)
        {
            int difference = left.Length ^ right.Length;
            int length = Math.Min(left.Length, right.Length);
            for (int index = 0; index < length; index++)
            {
                difference |= left[index] ^ right[index];
            }

            return difference == 0;
        }
    }
}
