using System;

namespace Werm.Core.Security
{
    public sealed class MaintenanceAuthorizationException : InvalidOperationException
    {
        public MaintenanceAuthorizationException(string message)
            : base(message)
        {
        }
    }
}
