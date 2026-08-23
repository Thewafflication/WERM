using System;

namespace Werm.Core.Security
{
    public static class PasswordPolicy
    {
        public const int MinimumLength = 15;
        public const int MaximumLength = 256;

        public static void Validate(string password)
        {
            if (password == null)
            {
                throw new ArgumentNullException(nameof(password));
            }
            if (password.Length < MinimumLength)
            {
                throw new ArgumentException(
                    "The maintenance password must contain at least 15 characters.",
                    nameof(password));
            }
            if (password.Length > MaximumLength)
            {
                throw new ArgumentException(
                    "The maintenance password cannot exceed 256 characters.",
                    nameof(password));
            }
        }
    }
}
