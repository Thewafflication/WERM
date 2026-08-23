using System;
using System.Data;
using System.IO;
using Werm.Core.Database;
using Werm.Data.Connection;

namespace Werm.Data
{
    public sealed class WermDatabaseInstaller
    {
        private readonly IWermDbConnectionFactory _connectionFactory;

        public WermDatabaseInstaller(IWermDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory ??
                throw new ArgumentNullException(nameof(connectionFactory));
        }

        public bool InstallOrValidate(string databasePath, string migrationPath)
        {
            if (string.IsNullOrWhiteSpace(databasePath))
            {
                throw new ArgumentException("A database path is required.", nameof(databasePath));
            }

            string fullPath = Path.GetFullPath(databasePath);
            if (Directory.Exists(fullPath))
            {
                throw new IOException("The database path identifies a directory: " + fullPath);
            }

            bool existed = File.Exists(fullPath);
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
            try
            {
                using (IDbConnection connection = _connectionFactory.OpenConnection())
                {
                    int version = GetSchemaVersion(connection);
                    if (version > InitialSchemaContract.Version)
                    {
                        throw new InvalidOperationException(
                            "The database schema is newer than this version of WERM supports.");
                    }
                    if (version < InitialSchemaContract.Version)
                    {
                        ApplyMigration(connection, SqlMigrationParser.Read(migrationPath));
                    }
                    VerifySchema(connection);
                }
                return !existed;
            }
            catch
            {
                if (!existed && File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }
                throw;
            }
        }

        public void VerifyExisting(string databasePath)
        {
            string fullPath = Path.GetFullPath(databasePath);
            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException(
                    "The database file does not exist. Use Create or validate database first.",
                    fullPath);
            }
            using (IDbConnection connection = _connectionFactory.OpenConnection())
            {
                VerifySchema(connection);
            }
        }

        private static int GetSchemaVersion(IDbConnection connection)
        {
            if (!ObjectExists(connection, "table", "WermSchemaVersion"))
            {
                long tableCount = Convert.ToInt64(Scalar(
                    connection,
                    "SELECT COUNT(*) FROM sqlite_master " +
                    "WHERE type = 'table' AND name NOT LIKE 'sqlite_%'"));
                if (tableCount != 0)
                {
                    throw new InvalidOperationException(
                        "The database is non-empty but is not a recognized WERM database.");
                }
                return 0;
            }
            return Convert.ToInt32(Scalar(
                connection,
                "SELECT COALESCE(MAX(Version), 0) FROM WermSchemaVersion"));
        }

        private static void ApplyMigration(IDbConnection connection, SqlMigration migration)
        {
            using (IDbTransaction transaction = connection.BeginTransaction())
            {
                try
                {
                    foreach (string batch in migration.Batches)
                    {
                        using (IDbCommand command = connection.CreateCommand())
                        {
                            command.CommandText = batch;
                            command.CommandTimeout = 30;
                            command.Transaction = transaction;
                            command.ExecuteNonQuery();
                        }
                    }
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        private static void VerifySchema(IDbConnection connection)
        {
            foreach (string table in InitialSchemaContract.ExpectedTableNames)
            {
                if (!ObjectExists(connection, "table", table))
                {
                    throw new InvalidOperationException("Expected table is missing: " + table);
                }
            }
            foreach (string trigger in InitialSchemaContract.ExpectedTriggerNames)
            {
                if (!ObjectExists(connection, "trigger", trigger))
                {
                    throw new InvalidOperationException("Expected trigger is missing: " + trigger);
                }
            }
            if (Convert.ToInt32(Scalar(connection, "PRAGMA foreign_keys")) != 1)
            {
                throw new InvalidOperationException("SQLite foreign-key enforcement is not enabled.");
            }
            if (GetSchemaVersion(connection) != InitialSchemaContract.Version)
            {
                throw new InvalidOperationException("The WERM database schema version is not supported.");
            }
        }

        private static bool ObjectExists(IDbConnection connection, string type, string name)
        {
            string escapedType = type.Replace("'", "''");
            string escapedName = name.Replace("'", "''");
            return Convert.ToInt64(Scalar(
                connection,
                "SELECT COUNT(*) FROM sqlite_master WHERE type = '" + escapedType +
                "' AND name = '" + escapedName + "'")) == 1;
        }

        private static object Scalar(IDbConnection connection, string commandText)
        {
            using (IDbCommand command = connection.CreateCommand())
            {
                command.CommandText = commandText;
                command.CommandTimeout = 30;
                return command.ExecuteScalar();
            }
        }
    }
}
