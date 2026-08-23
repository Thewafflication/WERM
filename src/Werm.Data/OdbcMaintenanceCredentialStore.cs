using System;
using System.Data;
using Werm.Core.Security;
using Werm.Data.Connection;

namespace Werm.Data
{
    public sealed class OdbcMaintenanceCredentialStore : IMaintenanceCredentialStore
    {
        private readonly IWermDbConnectionFactory _connectionFactory;
        private readonly IUtcClock _clock;

        public OdbcMaintenanceCredentialStore(
            IWermDbConnectionFactory connectionFactory,
            IUtcClock clock)
        {
            _connectionFactory = connectionFactory ??
                throw new ArgumentNullException(nameof(connectionFactory));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        }

        public PasswordCredential Get()
        {
            using (IDbConnection connection = _connectionFactory.OpenConnection())
            using (IDbCommand command = SqlCommandBuilder.Create(
                connection,
                null,
                "SELECT Algorithm, IterationCount, SaltBase64, HashBase64 " +
                "FROM MaintenanceCredential WHERE CredentialId = ?",
                SqlCommandBuilder.Integer(1)))
            using (IDataReader reader = command.ExecuteReader())
            {
                if (!reader.Read())
                {
                    return null;
                }

                return new PasswordCredential(
                    Convert.ToString(reader.GetValue(0)),
                    Convert.ToInt32(reader.GetValue(1)),
                    Convert.FromBase64String(Convert.ToString(reader.GetValue(2))),
                    Convert.FromBase64String(Convert.ToString(reader.GetValue(3))));
            }
        }

        public void Create(PasswordCredential credential)
        {
            if (credential == null)
            {
                throw new ArgumentNullException(nameof(credential));
            }

            string now = SqlCommandBuilder.FormatUtc(_clock.UtcNow);
            using (IDbConnection connection = _connectionFactory.OpenConnection())
            using (IDbCommand command = SqlCommandBuilder.Create(
                connection,
                null,
                "INSERT INTO MaintenanceCredential " +
                "(CredentialId, Algorithm, IterationCount, SaltBase64, HashBase64, CreatedUtc, ModifiedUtc) " +
                "VALUES (?, ?, ?, ?, ?, ?, ?)",
                SqlCommandBuilder.Integer(1),
                SqlCommandBuilder.Text(credential.Algorithm),
                SqlCommandBuilder.Integer(credential.IterationCount),
                SqlCommandBuilder.Text(Convert.ToBase64String(credential.GetSalt())),
                SqlCommandBuilder.Text(Convert.ToBase64String(credential.GetHash())),
                SqlCommandBuilder.Text(now),
                SqlCommandBuilder.Text(now)))
            {
                if (command.ExecuteNonQuery() != 1)
                {
                    throw new DataException("The maintenance credential was not created.");
                }
            }
        }

        public void Replace(PasswordCredential credential)
        {
            if (credential == null)
            {
                throw new ArgumentNullException(nameof(credential));
            }

            using (IDbConnection connection = _connectionFactory.OpenConnection())
            using (IDbCommand command = SqlCommandBuilder.Create(
                connection,
                null,
                "UPDATE MaintenanceCredential SET Algorithm = ?, IterationCount = ?, " +
                "SaltBase64 = ?, HashBase64 = ?, ModifiedUtc = ? WHERE CredentialId = ?",
                SqlCommandBuilder.Text(credential.Algorithm),
                SqlCommandBuilder.Integer(credential.IterationCount),
                SqlCommandBuilder.Text(Convert.ToBase64String(credential.GetSalt())),
                SqlCommandBuilder.Text(Convert.ToBase64String(credential.GetHash())),
                SqlCommandBuilder.Text(SqlCommandBuilder.FormatUtc(_clock.UtcNow)),
                SqlCommandBuilder.Integer(1)))
            {
                if (command.ExecuteNonQuery() != 1)
                {
                    throw new DataException("The maintenance credential does not exist.");
                }
            }
        }
    }
}
