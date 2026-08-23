using System;
using System.Data;
using System.Data.Odbc;

namespace Werm.Data.Connection
{
    public sealed class OdbcConnectionFactory : IWermDbConnectionFactory
    {
        private readonly OdbcConnectionOptions _options;

        public OdbcConnectionFactory(OdbcConnectionOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public IDbConnection OpenConnection()
        {
            var connection = new OdbcConnection(_options.BuildConnectionString());
            try
            {
                connection.Open();
                Configure(connection, "PRAGMA foreign_keys = ON");
                Configure(connection, "PRAGMA busy_timeout = 5000");
                return connection;
            }
            catch
            {
                connection.Dispose();
                throw;
            }
        }

        private static void Configure(IDbConnection connection, string commandText)
        {
            using (IDbCommand command = connection.CreateCommand())
            {
                command.CommandText = commandText;
                command.CommandTimeout = 30;
                command.ExecuteNonQuery();
            }
        }
    }
}
