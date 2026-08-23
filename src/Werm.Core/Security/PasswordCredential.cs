using System;

namespace Werm.Core.Security
{
    public sealed class PasswordCredential
    {
        private readonly byte[] _salt;
        private readonly byte[] _hash;

        public PasswordCredential(string algorithm, int iterationCount, byte[] salt, byte[] hash)
        {
            if (string.IsNullOrWhiteSpace(algorithm))
            {
                throw new ArgumentException("An algorithm name is required.", nameof(algorithm));
            }
            if (iterationCount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(iterationCount));
            }
            if (salt == null || salt.Length == 0)
            {
                throw new ArgumentException("A salt is required.", nameof(salt));
            }
            if (hash == null || hash.Length == 0)
            {
                throw new ArgumentException("A password hash is required.", nameof(hash));
            }

            Algorithm = algorithm.Trim();
            IterationCount = iterationCount;
            _salt = (byte[])salt.Clone();
            _hash = (byte[])hash.Clone();
        }

        public string Algorithm { get; private set; }
        public int IterationCount { get; private set; }

        public byte[] GetSalt()
        {
            return (byte[])_salt.Clone();
        }

        public byte[] GetHash()
        {
            return (byte[])_hash.Clone();
        }
    }
}
