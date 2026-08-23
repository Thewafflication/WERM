using System;

namespace Werm.Core.Domain
{
    internal static class DomainText
    {
        public static string Required(string value, string parameterName)
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }

            string normalized = value.Trim();
            if (normalized.Length == 0)
            {
                throw new ArgumentException("A value is required.", parameterName);
            }

            return normalized;
        }

        public static string Optional(string value)
        {
            return value == null || value.Trim().Length == 0 ? null : value;
        }
    }
}
