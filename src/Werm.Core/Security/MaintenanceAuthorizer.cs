using System;
using System.Collections.Generic;

namespace Werm.Core.Security
{
    public sealed class MaintenanceAuthorizer
    {
        public static readonly TimeSpan DefaultSessionDuration = TimeSpan.FromMinutes(10);
        public static readonly TimeSpan DefaultLockoutDuration = TimeSpan.FromSeconds(30);
        public const int DefaultMaximumFailedAttempts = 5;

        private readonly object _sync = new object();
        private readonly IMaintenanceCredentialStore _credentialStore;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IUtcClock _clock;
        private readonly TimeSpan _sessionDuration;
        private readonly TimeSpan _lockoutDuration;
        private readonly int _maximumFailedAttempts;
        private readonly Dictionary<Guid, DateTimeOffset> _activeSessions =
            new Dictionary<Guid, DateTimeOffset>();
        private int _failedAttempts;
        private DateTimeOffset? _lockedUntilUtc;

        public MaintenanceAuthorizer(
            IMaintenanceCredentialStore credentialStore,
            IPasswordHasher passwordHasher,
            IUtcClock clock)
            : this(
                credentialStore,
                passwordHasher,
                clock,
                DefaultSessionDuration,
                DefaultLockoutDuration,
                DefaultMaximumFailedAttempts)
        {
        }

        public MaintenanceAuthorizer(
            IMaintenanceCredentialStore credentialStore,
            IPasswordHasher passwordHasher,
            IUtcClock clock,
            TimeSpan sessionDuration,
            TimeSpan lockoutDuration,
            int maximumFailedAttempts)
        {
            _credentialStore = credentialStore ?? throw new ArgumentNullException(nameof(credentialStore));
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            if (sessionDuration <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(sessionDuration));
            }
            if (lockoutDuration <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(lockoutDuration));
            }
            if (maximumFailedAttempts <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maximumFailedAttempts));
            }

            _sessionDuration = sessionDuration;
            _lockoutDuration = lockoutDuration;
            _maximumFailedAttempts = maximumFailedAttempts;
        }

        public bool IsCredentialConfigured
        {
            get { return _credentialStore.Get() != null; }
        }

        public void InitializePassword(string password)
        {
            if (_credentialStore.Get() != null)
            {
                throw new InvalidOperationException("The maintenance password is already configured.");
            }

            _credentialStore.Create(_passwordHasher.Create(password));
        }

        public bool TryAuthenticate(
            string password,
            string operatorName,
            out MaintenanceSession session)
        {
            session = null;
            string normalizedOperator = RequireOperatorName(operatorName);
            DateTimeOffset now = _clock.UtcNow;

            lock (_sync)
            {
                if (_lockedUntilUtc.HasValue && now < _lockedUntilUtc.Value)
                {
                    return false;
                }
                if (_lockedUntilUtc.HasValue)
                {
                    _lockedUntilUtc = null;
                    _failedAttempts = 0;
                }
            }

            PasswordCredential credential = _credentialStore.Get();
            bool verified = credential != null && _passwordHasher.Verify(password, credential);

            lock (_sync)
            {
                if (!verified)
                {
                    _failedAttempts++;
                    if (_failedAttempts >= _maximumFailedAttempts)
                    {
                        _lockedUntilUtc = now.Add(_lockoutDuration);
                        _failedAttempts = 0;
                    }
                    return false;
                }

                _failedAttempts = 0;
                _lockedUntilUtc = null;
                DateTimeOffset expiresAtUtc = now.Add(_sessionDuration);
                session = new MaintenanceSession(Guid.NewGuid(), normalizedOperator, expiresAtUtc);
                _activeSessions.Add(session.Identifier, expiresAtUtc);
                return true;
            }
        }

        public string DemandAuthorized(MaintenanceSession session)
        {
            if (session == null)
            {
                throw new MaintenanceAuthorizationException(
                    "A valid maintenance session is required.");
            }

            lock (_sync)
            {
                DateTimeOffset expiresAtUtc;
                if (!_activeSessions.TryGetValue(session.Identifier, out expiresAtUtc) ||
                    _clock.UtcNow >= expiresAtUtc)
                {
                    _activeSessions.Remove(session.Identifier);
                    throw new MaintenanceAuthorizationException(
                        "The maintenance session is absent, expired, or revoked.");
                }

                return session.OperatorName;
            }
        }

        public void EndSession(MaintenanceSession session)
        {
            if (session == null)
            {
                return;
            }

            lock (_sync)
            {
                _activeSessions.Remove(session.Identifier);
            }
        }

        public void ChangePassword(MaintenanceSession session, string newPassword)
        {
            DemandAuthorized(session);
            PasswordCredential credential = _passwordHasher.Create(newPassword);
            _credentialStore.Replace(credential);
            lock (_sync)
            {
                _activeSessions.Clear();
            }
        }

        private static string RequireOperatorName(string operatorName)
        {
            if (operatorName == null)
            {
                throw new ArgumentNullException(nameof(operatorName));
            }

            string normalized = operatorName.Trim();
            if (normalized.Length == 0)
            {
                throw new ArgumentException("An operator name is required.", nameof(operatorName));
            }

            return normalized;
        }
    }
}
