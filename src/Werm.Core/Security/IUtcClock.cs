using System;

namespace Werm.Core.Security
{
    public interface IUtcClock
    {
        DateTimeOffset UtcNow { get; }
    }
}
