using System;

namespace Werm.Core.Security
{
    public sealed class SystemUtcClock : IUtcClock
    {
        public DateTimeOffset UtcNow
        {
            get { return DateTimeOffset.UtcNow; }
        }
    }
}
