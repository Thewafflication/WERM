using System;
using System.Data.Odbc;
using System.IO;

namespace Werm.Data.Connection
{
    public sealed class OdbcConnectionOptions
    {
        private OdbcConnectionOptions(string databasePath, string driverName, string dsn)
        {
            if (string.IsNullOrWhiteSpace(databasePath))
            {
                throw new ArgumentException("A database path is required.", nameof(databasePath));
            }

            DatabasePath = Path.GetFullPath(databasePath);
            DriverName = driverName;
            Dsn = dsn;
        }

        public string DatabasePath { get; private set; }
        public string DriverName { get; private set; }
        public string Dsn { get; private set; }

        public static OdbcConnectionOptions ForDriver(string databasePath, string driverName)
        {
            if (string.IsNullOrWhiteSpace(driverName))
            {
                throw new ArgumentException("A registered ODBC driver name is required.", nameof(driverName));
            }

            return new OdbcConnectionOptions(databasePath, driverName.Trim(), null);
        }

        public static OdbcConnectionOptions ForDsn(string databasePath, string dsn)
        {
            if (string.IsNullOrWhiteSpace(dsn))
            {
                throw new ArgumentException("An ODBC data-source name is required.", nameof(dsn));
            }

            return new OdbcConnectionOptions(databasePath, null, dsn.Trim());
        }

        public string BuildConnectionString()
        {
            var builder = new OdbcConnectionStringBuilder();
            if (DriverName != null)
            {
                builder.Driver = DriverName;
            }
            else
            {
                builder.Dsn = Dsn;
            }
            builder["Database"] = DatabasePath;
            return builder.ConnectionString;
        }
    }
}
