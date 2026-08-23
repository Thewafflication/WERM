using System;

namespace Werm.Core.Security
{
    public sealed class MaintenanceSession
    {
        internal MaintenanceSession(Guid identifier, string operatorName, DateTimeOffset expiresAtUtc)
        {
            Identifier = identifier;
            OperatorName = operatorName;
            ExpiresAtUtc = expiresAtUtc;
        }

        internal Guid Identifier { get; private set; }
        public string OperatorName { get; private set; }
        public DateTimeOffset ExpiresAtUtc { get; private set; }
    }
}
