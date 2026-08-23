using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;
using System.Xml;
using Werm.Core;
using Werm.Core.Configuration;
using Werm.Core.Database;
using Werm.Core.Domain;
using Werm.Core.Persistence;
using Werm.Core.Printing;
using Werm.Core.Security;
using Werm.Data;
using Werm.Data.Connection;

namespace Werm.Tests
{
    internal static class Program
    {
        private sealed class ControlledTest
        {
            public ControlledTest(string id, string requirement, string title, Action body)
            {
                Id = id;
                Requirement = requirement;
                Title = title;
                Body = body;
            }

            public string Id { get; private set; }
            public string Requirement { get; private set; }
            public string Title { get; private set; }
            public Action Body { get; private set; }
        }

        private sealed class TestResult
        {
            public ControlledTest Test { get; set; }
            public double DurationSeconds { get; set; }
            public Exception Failure { get; set; }
        }

        private static int Main(string[] args)
        {
            string resultsPath = GetOption(args, "--results") ??
                Path.Combine(Environment.CurrentDirectory, "werm-test-results.xml");
            string sourceRevision = GetOption(args, "--source-revision") ??
                Environment.GetEnvironmentVariable("GITHUB_SHA") ?? "local-working-tree";
            string expectedArchitecture = GetOption(args, "--expected-architecture");
            if (string.IsNullOrWhiteSpace(expectedArchitecture))
            {
                throw new ArgumentException("--expected-architecture is required.");
            }
            string repositoryRoot = GetOption(args, "--repository-root");
            if (string.IsNullOrWhiteSpace(repositoryRoot))
            {
                throw new ArgumentException("--repository-root is required.");
            }
            repositoryRoot = Path.GetFullPath(repositoryRoot);

            var tests = new[]
            {
                new ControlledTest(
                    "TC-0001",
                    "REQ-0001",
                    ".NET Framework 4.8 target identity",
                    VerifyTargetFramework),
                new ControlledTest(
                    "TC-0002",
                    "REQ-0001",
                    "Application identity is available to the desktop shell",
                    VerifyApplicationIdentity),
                new ControlledTest(
                    "TC-0003",
                    "ADR-0008",
                    "Explicit process architecture",
                    () => VerifyProcessArchitecture(expectedArchitecture)),
                new ControlledTest(
                    "TC-0004",
                    "REQ-0007,REQ-0008",
                    "Product identity and description validation",
                    VerifyProductRequiredValues),
                new ControlledTest(
                    "TC-0005",
                    "REQ-0009,REQ-0010",
                    "Product label facts are preserved",
                    VerifyProductLabelFacts),
                new ControlledTest(
                    "TC-0006",
                    "REQ-0011",
                    "Customer-specific price identity and amount validation",
                    VerifyCustomerProductPrice),
                new ControlledTest(
                    "TC-0007",
                    "REQ-0002,REQ-0007,REQ-0008,REQ-0009,REQ-0010,REQ-0011,REQ-0015",
                    "Initial SQLite migration contract",
                    () => VerifyInitialMigration(repositoryRoot)),
                new ControlledTest(
                    "TC-0008",
                    "REQ-0019,REQ-0020",
                    "Database installer safe connection failure",
                    () => VerifyInstallerSafeFailure(repositoryRoot, expectedArchitecture)),
                new ControlledTest(
                    "TC-0009",
                    "REQ-0012",
                    "Maintenance password verifier",
                    VerifyMaintenancePassword),
                new ControlledTest(
                    "TC-0010",
                    "REQ-0012",
                    "Password-gated maintenance session",
                    VerifyMaintenanceSession),
                new ControlledTest(
                    "TC-0011",
                    "REQ-0012",
                    "Failed authentication throttling",
                    VerifyAuthenticationThrottling),
                new ControlledTest(
                    "TC-0012",
                    "REQ-0002,REQ-0019",
                    "ODBC connection-string contract",
                    () => VerifyOdbcConnectionOptions(repositoryRoot)),
                new ControlledTest(
                    "TC-0013",
                    "REQ-0013,REQ-0014,REQ-0019",
                    "Atomic product creation and audit",
                    VerifyAtomicProductAudit),
                new ControlledTest(
                    "TC-0014",
                    "REQ-0013,REQ-0019",
                    "Audit failure rolls back product change",
                    VerifyAuditFailureRollback),
                new ControlledTest(
                    "TC-0015",
                    "REQ-0011,REQ-0013,REQ-0014,REQ-0019",
                    "Customer-price audit lineage",
                    VerifyPriceAuditLineage),
                new ControlledTest(
                    "TC-0016",
                    "REQ-0012,REQ-0016,REQ-0017,REQ-0018",
                    "Application write authorization boundary",
                    VerifyMaintenanceServiceAuthorization),
                new ControlledTest(
                    "TC-0017",
                    "REQ-0015",
                    "Append-only audit schema protection",
                    () => VerifyAppendOnlyAuditSchema(repositoryRoot)),
                new ControlledTest(
                    "TC-0018",
                    "REQ-0012,REQ-0019",
                    "ODBC credential persistence mapping",
                    VerifyCredentialPersistence),
                new ControlledTest(
                    "TC-0019",
                    "REQ-0017,REQ-0019",
                    "ODBC customer insertion",
                    VerifyCustomerInsertion),
                new ControlledTest(
                    "TC-0020",
                    "REQ-0003,REQ-0004",
                    "Product customer and price label-field mapping",
                    VerifyLabelFieldMapping),
                new ControlledTest(
                    "TC-0021",
                    "REQ-0004,REQ-0005",
                    "Tagged Word template population and direct print arguments",
                    VerifyWordTemplatePopulation),
                new ControlledTest(
                    "TC-0022",
                    "REQ-0004",
                    "Missing required Word content-control tag rejection",
                    VerifyMissingWordFieldRejected),
                new ControlledTest(
                    "TC-0023",
                    "REQ-0003,REQ-0005",
                    "Selected label record retrieval workflow",
                    VerifyLabelWorkflow),
                new ControlledTest(
                    "TC-0024",
                    "REQ-0005",
                    "Word document cleanup after print failure",
                    VerifyPrintFailureCleanup),
                new ControlledTest(
                    "TC-0029",
                    "REQ-0022,ADR-0013",
                    "Per-user database settings persistence",
                    () => VerifySettingsPersistence(repositoryRoot)),
                new ControlledTest(
                    "TC-0030",
                    "REQ-0020,REQ-0022,ADR-0013",
                    "Application database creation service",
                    () => VerifyApplicationDatabaseCreation(repositoryRoot))
            };

            var results = new List<TestResult>();
            foreach (ControlledTest test in tests)
            {
                var stopwatch = Stopwatch.StartNew();
                Exception failure = null;
                try
                {
                    test.Body();
                    Console.WriteLine("PASS {0}: {1}", test.Id, test.Title);
                }
                catch (Exception exception)
                {
                    failure = exception;
                    Console.Error.WriteLine("FAIL {0}: {1}", test.Id, exception.Message);
                }
                finally
                {
                    stopwatch.Stop();
                }

                results.Add(new TestResult
                {
                    Test = test,
                    DurationSeconds = stopwatch.Elapsed.TotalSeconds,
                    Failure = failure
                });
            }

            WriteResults(resultsPath, sourceRevision, results);
            return results.Exists(result => result.Failure != null) ? 1 : 0;
        }

        private static string GetOption(string[] args, string name)
        {
            for (int index = 0; index < args.Length; index++)
            {
                if (string.Equals(args[index], name, StringComparison.OrdinalIgnoreCase))
                {
                    if (index + 1 >= args.Length)
                    {
                        throw new ArgumentException("Missing value for " + name + ".");
                    }

                    return args[index + 1];
                }
            }

            return null;
        }

        private static void VerifyTargetFramework()
        {
            var attribute = (TargetFrameworkAttribute)Attribute.GetCustomAttribute(
                typeof(Werm.Core.ApplicationIdentity).Assembly,
                typeof(TargetFrameworkAttribute));
            if (attribute == null ||
                !string.Equals(attribute.FrameworkName, ".NETFramework,Version=v4.8", StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "Werm.Core does not identify .NET Framework 4.8 as its target.");
            }
        }

        private static void VerifyApplicationIdentity()
        {
            if (!string.Equals(Werm.Core.ApplicationIdentity.ShortName, "WERM", StringComparison.Ordinal) ||
                !string.Equals(Werm.Core.ApplicationIdentity.MilestoneVersion, "0.1.0", StringComparison.Ordinal))
            {
                throw new InvalidOperationException("The WERM 0.1.0 identity is inconsistent.");
            }
        }

        private static void VerifyProcessArchitecture(string expectedArchitecture)
        {
            string actualArchitecture = Environment.Is64BitProcess ? "x64" : "x86";
            if (!string.Equals(actualArchitecture, expectedArchitecture, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "Expected a " + expectedArchitecture + " process but executed as " + actualArchitecture + ".");
            }
        }

        private static void VerifyProductRequiredValues()
        {
            ExpectException<ArgumentException>(() =>
                new Product(" ", "Ground Beef", null, true, true));
            ExpectException<ArgumentException>(() =>
                new Product("0042", " ", null, true, true));

            var product = new Product(" 0042 ", " Ground Beef ", null, true, true);
            Equal("0042", product.Plu, "PLU normalization");
            Equal("Ground Beef", product.Description, "description normalization");
        }

        private static void VerifyProductLabelFacts()
        {
            const string ingredients = "BEEF, WATER, SALT.\r\nCONTAINS: NONE.";
            var safeProduct = new Product("0042", "Ground Beef", ingredients, true, true);
            var ordinaryProduct = new Product("0043", "Roast", null, false, true);

            Equal(ingredients, safeProduct.IngredientsStatement, "ingredients statement");
            Equal(true, safeProduct.SafeHandlingRequired, "safe-handling required state");
            Equal(false, ordinaryProduct.SafeHandlingRequired, "safe-handling not-required state");
            Equal(null, ordinaryProduct.IngredientsStatement, "optional ingredients statement");
        }

        private static void VerifyCustomerProductPrice()
        {
            var customer = new Customer(17, " STORE-17 ", " Downtown Store ", true);
            var price = new CustomerProductPrice(
                customer.CustomerId,
                " 0042 ",
                " Retail ",
                1299,
                " usd ",
                "per package",
                true);

            Equal(17L, price.CustomerId, "customer identity");
            Equal("0042", price.ProductPlu, "product identity");
            Equal("Retail", price.PriceType, "price type");
            Equal(1299L, price.AmountMinorUnits, "minor-unit amount");
            Equal("USD", price.CurrencyCode, "currency normalization");
            ExpectException<ArgumentOutOfRangeException>(() =>
                new CustomerProductPrice(17, "0042", "Retail", -1, "USD", null, true));
            ExpectException<ArgumentException>(() =>
                new CustomerProductPrice(17, "0042", "Retail", 1, "US", null, true));
        }

        private static void VerifyInitialMigration(string repositoryRoot)
        {
            string migrationPath = Path.Combine(
                repositoryRoot,
                "database",
                "migrations",
                InitialSchemaContract.MigrationName);
            SqlMigration migration = SqlMigrationParser.Read(migrationPath);
            if (migration.Batches.Count != 15)
            {
                throw new InvalidOperationException(
                    "Expected 15 controlled migration batches but found " + migration.Batches.Count + ".");
            }

            foreach (string tableName in InitialSchemaContract.ExpectedTableNames)
            {
                string pattern = @"(?im)^\s*CREATE\s+TABLE\s+" + Regex.Escape(tableName) + @"\s*\(";
                if (!Regex.IsMatch(migration.Sql, pattern))
                {
                    throw new InvalidOperationException("Missing required table definition: " + tableName);
                }
            }

            foreach (string triggerName in InitialSchemaContract.ExpectedTriggerNames)
            {
                string pattern = @"(?im)^\s*CREATE\s+TRIGGER\s+" +
                    Regex.Escape(triggerName) + @"\s*$";
                if (!Regex.IsMatch(migration.Sql, pattern))
                {
                    throw new InvalidOperationException(
                        "Missing required append-only trigger: " + triggerName);
                }
            }

            if (!Regex.IsMatch(migration.Sql, @"(?im)^\s*PRAGMA\s+user_version\s*=\s*1\s*;"))
            {
                throw new InvalidOperationException("Migration does not set SQLite user_version to 1.");
            }
            if (Regex.IsMatch(migration.Sql, @"(?i)\bbarcode\b"))
            {
                throw new InvalidOperationException("The 0.1.0 migration contains deferred barcode scope.");
            }
        }

        private static void VerifyInstallerSafeFailure(
            string repositoryRoot,
            string expectedArchitecture)
        {
            string worker = Path.Combine(repositoryRoot, "tools", "Install-WermDatabase.ps1");
            if (!File.Exists(worker))
            {
                throw new FileNotFoundException("The ODBC installer worker was not found.", worker);
            }

            string testRoot = Path.Combine(
                repositoryRoot,
                "out",
                "test-work",
                "database-installer",
                expectedArchitecture,
                Guid.NewGuid().ToString("N"));
            string databasePath = Path.Combine(testRoot, "must-not-remain.db");
            Directory.CreateDirectory(testRoot);

            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = "-NoLogo -NoProfile -NonInteractive -ExecutionPolicy Bypass -File " +
                        QuoteArgument(worker) + " -DatabasePath " + QuoteArgument(databasePath) +
                        " -DriverName WERM-CONTROLLED-MISSING-DRIVER",
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false
                };

                using (Process process = Process.Start(startInfo))
                {
                    var standardOutputTask = process.StandardOutput.ReadToEndAsync();
                    var standardErrorTask = process.StandardError.ReadToEndAsync();
                    if (!process.WaitForExit(30000))
                    {
                        process.Kill();
                        throw new TimeoutException("The installer did not fail within 30 seconds.");
                    }
                    string standardOutput = standardOutputTask.GetAwaiter().GetResult();
                    string standardError = standardErrorTask.GetAwaiter().GetResult();
                    if (process.ExitCode != 4)
                    {
                        throw new InvalidOperationException(
                            "Expected installer exit 4 but received " + process.ExitCode + ". Output: " +
                            standardOutput + " Error: " + standardError);
                    }
                    if (!standardOutput.Contains("Process architecture: " + expectedArchitecture.ToUpperInvariant()))
                    {
                        throw new InvalidOperationException(
                            "The installer did not run with the selected process architecture. Output: " +
                            standardOutput);
                    }
                }

                if (File.Exists(databasePath))
                {
                    throw new InvalidOperationException(
                        "The failing installer left a new database file behind.");
                }
            }
            finally
            {
                if (Directory.Exists(testRoot))
                {
                    Directory.Delete(testRoot, true);
                }
            }
        }

        private static string QuoteArgument(string value)
        {
            return "\"" + value.Replace("\"", "\\\"") + "\"";
        }

        private static void VerifyMaintenancePassword()
        {
            const string password = "correct horse battery staple";
            var hasher = new Pbkdf2PasswordHasher();
            PasswordCredential first = hasher.Create(password);
            PasswordCredential second = hasher.Create(password);

            Equal(Pbkdf2PasswordHasher.AlgorithmName, first.Algorithm, "password algorithm");
            Equal(Pbkdf2PasswordHasher.DefaultIterationCount, first.IterationCount, "iteration count");
            Equal(Pbkdf2PasswordHasher.SaltLength, first.GetSalt().Length, "salt length");
            Equal(Pbkdf2PasswordHasher.HashLength, first.GetHash().Length, "hash length");
            Equal(true, hasher.Verify(password, first), "correct-password verification");
            Equal(false, hasher.Verify("incorrect password value", first), "wrong-password verification");
            Equal(false, ByteArraysEqual(first.GetSalt(), second.GetSalt()), "unique salts");
            ExpectException<ArgumentException>(() => hasher.Create(new string('x', 14)));
            ExpectException<ArgumentException>(() => hasher.Create(new string('x', 257)));
        }

        private static void VerifyMaintenanceSession()
        {
            var store = new InMemoryCredentialStore();
            var hasher = new DeterministicPasswordHasher("a sufficiently long password");
            var clock = new AdjustableClock(new DateTimeOffset(2026, 8, 22, 20, 0, 0, TimeSpan.Zero));
            var authorizer = new MaintenanceAuthorizer(store, hasher, clock);

            authorizer.InitializePassword("a sufficiently long password");
            ExpectException<InvalidOperationException>(() =>
                authorizer.InitializePassword("another sufficiently long password"));
            ExpectException<MaintenanceAuthorizationException>(() => authorizer.DemandAuthorized(null));

            MaintenanceSession session;
            Equal(false, authorizer.TryAuthenticate("wrong", "operator", out session), "wrong password");
            Equal(true, authorizer.TryAuthenticate(
                "a sufficiently long password", " operator ", out session), "correct password");
            Equal("operator", authorizer.DemandAuthorized(session), "authorized operator");

            clock.Advance(TimeSpan.FromMinutes(11));
            ExpectException<MaintenanceAuthorizationException>(() =>
                authorizer.DemandAuthorized(session));
        }

        private static void VerifyAuthenticationThrottling()
        {
            var store = new InMemoryCredentialStore();
            var hasher = new DeterministicPasswordHasher("a sufficiently long password");
            var clock = new AdjustableClock(new DateTimeOffset(2026, 8, 22, 20, 0, 0, TimeSpan.Zero));
            var authorizer = new MaintenanceAuthorizer(
                store,
                hasher,
                clock,
                TimeSpan.FromMinutes(10),
                TimeSpan.FromSeconds(30),
                5);
            authorizer.InitializePassword("a sufficiently long password");

            MaintenanceSession session;
            for (int attempt = 0; attempt < 5; attempt++)
            {
                Equal(false, authorizer.TryAuthenticate("wrong", "operator", out session),
                    "failed authentication " + (attempt + 1));
            }
            Equal(false, authorizer.TryAuthenticate(
                "a sufficiently long password", "operator", out session), "locked authentication");
            clock.Advance(TimeSpan.FromSeconds(31));
            Equal(true, authorizer.TryAuthenticate(
                "a sufficiently long password", "operator", out session), "post-lockout authentication");
        }

        private static void VerifyOdbcConnectionOptions(string repositoryRoot)
        {
            string databasePath = Path.Combine(repositoryRoot, "out", "test-data", "werm.db");
            OdbcConnectionOptions driverOptions = OdbcConnectionOptions.ForDriver(
                databasePath, "SQLite Controlled Driver");
            var driverBuilder = new OdbcConnectionStringBuilder(
                driverOptions.BuildConnectionString());
            Equal("{SQLite Controlled Driver}", driverBuilder.Driver, "ODBC driver name");
            Equal(Path.GetFullPath(databasePath),
                Convert.ToString(driverBuilder["Database"]), "ODBC database path");

            OdbcConnectionOptions dsnOptions = OdbcConnectionOptions.ForDsn(databasePath, "WERM");
            var dsnBuilder = new OdbcConnectionStringBuilder(dsnOptions.BuildConnectionString());
            Equal("WERM", dsnBuilder.Dsn, "ODBC DSN");
            Equal(false, dsnBuilder.ContainsKey("Password"), "embedded password absence");
            Equal(false, dsnBuilder.ContainsKey("Pwd"), "embedded Pwd absence");
        }

        private static void VerifyAtomicProductAudit()
        {
            var connection = new RecordingDbConnection();
            connection.ReaderHandler = command => new DataTable();
            connection.ScalarHandler = command => 101L;
            OdbcWermDataStore store = CreateDataStore(connection);
            var product = new Product("0042", "Ground Beef", "BEEF, SALT", true, true);

            Equal(true, store.SaveProduct(product, "operator", "initial entry"),
                "product save result");
            Equal(1, connection.LastTransaction.CommitCount, "transaction commits");
            Equal(0, connection.LastTransaction.RollbackCount, "transaction rollbacks");
            Equal(1, CountCommands(connection, "INSERT INTO Product "), "product inserts");
            Equal(1, CountCommands(connection, "INSERT INTO ProductAuditEvent"),
                "audit event inserts");
            Equal(5, CountCommands(connection, "INSERT INTO ProductAuditChange"),
                "audit field inserts");

            foreach (RecordingDbCommand command in connection.Commands)
            {
                if (command.Parameters.Count > 0)
                {
                    Equal(command.Parameters.Count, CountCharacter(command.CommandText, '?'),
                        "positional parameter count");
                }
                if (command.CommandText.StartsWith("INSERT", StringComparison.Ordinal))
                {
                    Equal(connection.LastTransaction, command.Transaction,
                        "write transaction identity");
                }
            }
        }

        private static void VerifyAuditFailureRollback()
        {
            var connection = new RecordingDbConnection();
            connection.ReaderHandler = command => new DataTable();
            connection.ScalarHandler = command => 102L;
            connection.NonQueryHandler = command =>
            {
                if (command.CommandText.StartsWith(
                    "INSERT INTO ProductAuditChange", StringComparison.Ordinal))
                {
                    throw new DataException("controlled audit failure");
                }
                return 1;
            };
            OdbcWermDataStore store = CreateDataStore(connection);

            ExpectException<DataException>(() => store.SaveProduct(
                new Product("0043", "Roast", null, false, true),
                "operator",
                "controlled failure"));
            Equal(0, connection.LastTransaction.CommitCount, "failed transaction commits");
            Equal(1, connection.LastTransaction.RollbackCount, "failed transaction rollbacks");
        }

        private static void VerifyPriceAuditLineage()
        {
            var connection = new RecordingDbConnection();
            connection.ReaderHandler = command =>
            {
                if (command.CommandText.Contains("FROM CustomerProductPrice"))
                {
                    return TableWithRow(
                        new[]
                        {
                            "CustomerId", "ProductPLU", "PriceType", "AmountMinorUnits",
                            "CurrencyCode", "PriceBasis", "IsActive"
                        },
                        new[]
                        {
                            typeof(long), typeof(string), typeof(string), typeof(long),
                            typeof(string), typeof(string), typeof(long)
                        },
                        17L, "0042", "Retail", 1099L, "USD", "per package", 1L);
                }
                if (command.CommandText.Contains("FROM ProductAuditEvent"))
                {
                    return TableWithRow(
                        new[] { "AuditEventId", "RevisionNumber" },
                        new[] { typeof(long), typeof(int) },
                        10L, 3);
                }
                return new DataTable();
            };
            connection.ScalarHandler = command => 11L;
            OdbcWermDataStore store = CreateDataStore(connection);
            var price = new CustomerProductPrice(
                17, "0042", "Retail", 1299, "USD", "per package", false);

            Equal(true, store.SaveCustomerProductPrice(
                price, "operator", "customer price change"), "price save result");
            Equal(1, CountCommands(connection, "UPDATE CustomerProductPrice"), "price updates");
            Equal(2, CountCommands(connection, "INSERT INTO ProductAuditChange"),
                "price field changes");
            RecordingDbCommand eventCommand = FindSingleCommand(
                connection, "INSERT INTO ProductAuditEvent");
            Equal(10L, Convert.ToInt64(ParameterValue(eventCommand, 1)), "parent audit event");
            Equal(4L, Convert.ToInt64(ParameterValue(eventCommand, 2)), "audit revision");
            Equal("Deactivate", Convert.ToString(ParameterValue(eventCommand, 3)),
                "audit change type");
            Equal(1, connection.LastTransaction.CommitCount, "price transaction commits");
            Equal(0, connection.LastTransaction.RollbackCount, "price transaction rollbacks");
        }

        private static void VerifyMaintenanceServiceAuthorization()
        {
            var credentialStore = new InMemoryCredentialStore();
            var hasher = new DeterministicPasswordHasher("a sufficiently long password");
            var clock = new AdjustableClock(
                new DateTimeOffset(2026, 8, 22, 20, 0, 0, TimeSpan.Zero));
            var authorizer = new MaintenanceAuthorizer(credentialStore, hasher, clock);
            authorizer.InitializePassword("a sufficiently long password");
            var dataStore = new RecordingWermDataStore();
            var service = new MaintenanceService(dataStore, authorizer);
            var product = new Product("0042", "Ground Beef", null, false, true);

            ExpectException<MaintenanceAuthorizationException>(() =>
                service.SaveProduct(null, product, "unauthorized"));
            Equal(0, dataStore.ProductSaveCount, "unauthorized store writes");

            MaintenanceSession session;
            Equal(true, authorizer.TryAuthenticate(
                "a sufficiently long password", "operator", out session),
                "maintenance authentication");
            Equal(true, service.SaveProduct(session, product, "authorized"),
                "authorized product write");
            Equal(1, dataStore.ProductSaveCount, "authorized store writes");
            Equal("operator", dataStore.LastChangedBy, "audited operator");
        }

        private static void VerifyAppendOnlyAuditSchema(string repositoryRoot)
        {
            SqlMigration migration = SqlMigrationParser.Read(Path.Combine(
                repositoryRoot,
                "database",
                "migrations",
                InitialSchemaContract.MigrationName));
            foreach (string tableName in new[] { "ProductAuditEvent", "ProductAuditChange" })
            {
                foreach (string operation in new[] { "UPDATE", "DELETE" })
                {
                    string pattern = @"(?is)CREATE\s+TRIGGER\s+TR_" + tableName +
                        @"_Reject" + operation.Substring(0, 1) + operation.Substring(1).ToLowerInvariant() +
                        @"\s+BEFORE\s+" + operation + @"\s+ON\s+" + tableName +
                        @".*?RAISE\s*\(\s*ABORT";
                    if (!Regex.IsMatch(migration.Sql, pattern))
                    {
                        throw new InvalidOperationException(
                            "Missing append-only " + operation + " protection for " + tableName + ".");
                    }
                }
            }
        }

        private static void VerifyCredentialPersistence()
        {
            var writeConnection = new RecordingDbConnection();
            var clock = new AdjustableClock(
                new DateTimeOffset(2026, 8, 22, 20, 0, 0, TimeSpan.Zero));
            var writeStore = new OdbcMaintenanceCredentialStore(
                new RecordingDbConnectionFactory(writeConnection), clock);
            var credential = new PasswordCredential(
                "PBKDF2-HMAC-SHA512", 220000, new byte[] { 1, 2, 3 }, new byte[] { 4, 5, 6 });
            writeStore.Create(credential);
            RecordingDbCommand insert = FindSingleCommand(
                writeConnection, "INSERT INTO MaintenanceCredential");
            Equal("PBKDF2-HMAC-SHA512", Convert.ToString(ParameterValue(insert, 1)),
                "stored credential algorithm");
            Equal("AQID", Convert.ToString(ParameterValue(insert, 3)), "stored Base64 salt");
            Equal("BAUG", Convert.ToString(ParameterValue(insert, 4)), "stored Base64 hash");

            var readConnection = new RecordingDbConnection();
            readConnection.ReaderHandler = command => TableWithRow(
                new[] { "Algorithm", "IterationCount", "SaltBase64", "HashBase64" },
                new[] { typeof(string), typeof(int), typeof(string), typeof(string) },
                "PBKDF2-HMAC-SHA512", 220000, "AQID", "BAUG");
            var readStore = new OdbcMaintenanceCredentialStore(
                new RecordingDbConnectionFactory(readConnection), clock);
            PasswordCredential loaded = readStore.Get();
            Equal(credential.Algorithm, loaded.Algorithm, "loaded credential algorithm");
            Equal(credential.IterationCount, loaded.IterationCount, "loaded credential iterations");
            Equal(true, ByteArraysEqual(credential.GetSalt(), loaded.GetSalt()),
                "loaded credential salt");
            Equal(true, ByteArraysEqual(credential.GetHash(), loaded.GetHash()),
                "loaded credential hash");
        }

        private static void VerifyCustomerInsertion()
        {
            var connection = new RecordingDbConnection();
            connection.ScalarHandler = command => 25L;
            OdbcWermDataStore store = CreateDataStore(connection);
            long customerId = store.SaveCustomer(
                new Customer(0, "STORE-25", "Market Street", true));

            Equal(25L, customerId, "new customer identity");
            Equal(1, CountCommands(connection, "INSERT INTO Customer "), "customer inserts");
            Equal(0, CountCommands(connection, "DELETE FROM Customer"), "customer deletes");
            Equal(1, connection.LastTransaction.CommitCount, "customer transaction commits");
        }

        private static void VerifyLabelFieldMapping()
        {
            Product product;
            Customer customer;
            CustomerProductPrice price;
            CreateLabelRecord(out product, out customer, out price);

            IDictionary<string, string> fields = LabelFieldMapper.Map(
                product, customer, price);
            Equal(9, fields.Count, "mapped field count");
            Equal("0042", fields[LabelFieldNames.ProductPlu], "mapped PLU");
            Equal("Ground Beef", fields[LabelFieldNames.ProductDescription],
                "mapped description");
            Equal("Beef.", fields[LabelFieldNames.IngredientsStatement],
                "mapped ingredients");
            Equal("YES", fields[LabelFieldNames.SafeHandlingRequired],
                "mapped safe handling");
            Equal("STORE-25", fields[LabelFieldNames.CustomerCode],
                "mapped customer code");
            Equal("Market Street", fields[LabelFieldNames.CustomerName],
                "mapped customer name");
            Equal("$12.99", fields[LabelFieldNames.PriceAmount], "mapped price");
            Equal("MARKED", fields[LabelFieldNames.PriceType], "mapped price type");
            Equal("per lb", fields[LabelFieldNames.PriceBasis], "mapped price basis");
        }

        private static void VerifyWordTemplatePopulation()
        {
            var document = new FakeLabelDocument(LabelFieldNames.Required);
            var factory = new FakeLabelDocumentFactory(document);
            var service = new WordLabelPrintService(factory);
            service.Print(CreateLabelPrintJob());

            Equal("label.dotx", factory.LastTemplatePath, "template path");
            Equal(9, document.Values.Count, "populated field count");
            Equal("Ground Beef", document.Values[LabelFieldNames.ProductDescription],
                "populated description");
            Equal(1, document.PrintCount, "direct print count");
            Equal("Test Label Printer", document.PrinterName, "selected printer");
            Equal(2, document.Copies, "print copies");
            Equal(true, document.Disposed, "document cleanup after success");
        }

        private static void VerifyMissingWordFieldRejected()
        {
            var available = new List<string>(LabelFieldNames.Required);
            available.Remove(LabelFieldNames.PriceBasis);
            var document = new FakeLabelDocument(available);
            var service = new WordLabelPrintService(
                new FakeLabelDocumentFactory(document));

            ExpectException<LabelTemplateException>(() => service.Print(CreateLabelPrintJob()));
            Equal(0, document.Values.Count, "fields written before contract validation");
            Equal(0, document.PrintCount, "print count for invalid template");
            Equal(true, document.Disposed, "invalid template document cleanup");
        }

        private static void VerifyLabelWorkflow()
        {
            Product product;
            Customer customer;
            CustomerProductPrice price;
            CreateLabelRecord(out product, out customer, out price);
            var dataStore = new RecordingWermDataStore
            {
                ProductToReturn = product,
                CustomerToReturn = customer,
                PriceToReturn = price
            };
            var printService = new RecordingLabelPrintService();
            var workflow = new LabelWorkflowService(dataStore, printService);

            workflow.PrintLabel(
                "0042", 25, "MARKED", "label.dotx", "Test Label Printer", 3);

            Equal("0042", dataStore.LastProductLookup, "product lookup");
            Equal(25L, dataStore.LastCustomerLookup, "customer lookup");
            Equal("MARKED", dataStore.LastPriceTypeLookup, "price type lookup");
            Equal("label.dotx", printService.LastJob.TemplatePath, "workflow template");
            Equal(3, printService.LastJob.Copies, "workflow copies");
            Equal("$12.99", printService.LastJob.FieldValues[LabelFieldNames.PriceAmount],
                "workflow mapped price");
        }

        private static void VerifyPrintFailureCleanup()
        {
            var document = new FakeLabelDocument(LabelFieldNames.Required)
            {
                ThrowOnPrint = true
            };
            var service = new WordLabelPrintService(
                new FakeLabelDocumentFactory(document));

            ExpectException<InvalidOperationException>(() => service.Print(CreateLabelPrintJob()));
            Equal(1, document.PrintCount, "failed print attempts");
            Equal(true, document.Disposed, "document cleanup after failure");
        }

        private static void VerifySettingsPersistence(string repositoryRoot)
        {
            string path = Path.Combine(
                repositoryRoot,
                "out",
                "test-data",
                "controlled-settings.xml");
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            try
            {
                var store = new WermSettingsStore(path);
                var expected = new WermSettings
                {
                    DatabasePath = Path.Combine(repositoryRoot, "out", "test-data", "werm.db"),
                    OdbcDriverName = "Controlled SQLite ODBC Driver",
                    OdbcDsn = string.Empty,
                    WordTemplatePath = Path.Combine(repositoryRoot, "label.dotx")
                };
                store.Save(expected);
                WermSettings actual = store.Load(new WermSettings());

                Equal(expected.DatabasePath, actual.DatabasePath, "database path setting");
                Equal(expected.OdbcDriverName, actual.OdbcDriverName, "ODBC driver setting");
                Equal(expected.OdbcDsn, actual.OdbcDsn, "ODBC DSN setting");
                Equal(expected.WordTemplatePath, actual.WordTemplatePath, "Word template setting");

                string xml = File.ReadAllText(path);
                if (xml.IndexOf(
                    "<WermSettings version=\"1\">", StringComparison.Ordinal) < 0)
                {
                    throw new InvalidOperationException(
                        "The settings schema version was not persisted.");
                }
                if (Regex.IsMatch(xml, "password|credential|secret|hash", RegexOptions.IgnoreCase))
                {
                    throw new InvalidOperationException(
                        "The per-user settings file contains a credential-like field.");
                }
            }
            finally
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
        }

        private static void VerifyApplicationDatabaseCreation(string repositoryRoot)
        {
            bool schemaInstalled = false;
            var connection = new RecordingDbConnection();
            connection.ScalarHandler = command =>
            {
                if (command.CommandText == "PRAGMA foreign_keys")
                {
                    return 1;
                }
                if (command.CommandText.IndexOf(
                    "COALESCE(MAX(Version)", StringComparison.Ordinal) >= 0)
                {
                    return schemaInstalled ? 1 : 0;
                }
                if (command.CommandText.IndexOf(
                    "name NOT LIKE 'sqlite_%'", StringComparison.Ordinal) >= 0)
                {
                    return 0L;
                }
                return schemaInstalled ? 1L : 0L;
            };
            connection.NonQueryHandler = command =>
            {
                if (command.CommandText.IndexOf(
                    "INSERT INTO WermSchemaVersion", StringComparison.Ordinal) >= 0)
                {
                    schemaInstalled = true;
                }
                return 1;
            };

            string databasePath = Path.Combine(
                repositoryRoot,
                "out",
                "test-data",
                "recording-database.db");
            if (File.Exists(databasePath))
            {
                File.Delete(databasePath);
            }
            string migrationPath = Path.Combine(
                repositoryRoot,
                "database",
                "migrations",
                InitialSchemaContract.MigrationName);
            var installer = new WermDatabaseInstaller(
                new RecordingDbConnectionFactory(connection));
            bool created = installer.InstallOrValidate(databasePath, migrationPath);

            Equal(true, created, "missing database is reported as created");
            Equal(true, schemaInstalled, "schema version batch executed");
            Equal(1, connection.LastTransaction.CommitCount, "migration commit count");
            Equal(0, connection.LastTransaction.RollbackCount, "migration rollback count");
            int transactionalCommands = connection.Commands.FindAll(
                command => command.Transaction != null).Count;
            Equal(
                SqlMigrationParser.Read(migrationPath).Batches.Count,
                transactionalCommands,
                "transactional migration batch count");
        }

        private static LabelPrintJob CreateLabelPrintJob()
        {
            Product product;
            Customer customer;
            CustomerProductPrice price;
            CreateLabelRecord(out product, out customer, out price);
            return new LabelPrintJob(
                "label.dotx",
                "Test Label Printer",
                2,
                LabelFieldMapper.Map(product, customer, price));
        }

        private static void CreateLabelRecord(
            out Product product,
            out Customer customer,
            out CustomerProductPrice price)
        {
            product = new Product("0042", "Ground Beef", "Beef.", true, true);
            customer = new Customer(25, "STORE-25", "Market Street", true);
            price = new CustomerProductPrice(
                25, "0042", "MARKED", 1299, "USD", "per lb", true);
        }

        private static OdbcWermDataStore CreateDataStore(RecordingDbConnection connection)
        {
            return new OdbcWermDataStore(
                new RecordingDbConnectionFactory(connection),
                new AdjustableClock(new DateTimeOffset(
                    2026, 8, 22, 20, 0, 0, TimeSpan.Zero)));
        }

        private static int CountCommands(RecordingDbConnection connection, string prefix)
        {
            int count = 0;
            foreach (RecordingDbCommand command in connection.Commands)
            {
                if (command.CommandText.StartsWith(prefix, StringComparison.Ordinal))
                {
                    count++;
                }
            }
            return count;
        }

        private static RecordingDbCommand FindSingleCommand(
            RecordingDbConnection connection,
            string prefix)
        {
            RecordingDbCommand match = null;
            foreach (RecordingDbCommand command in connection.Commands)
            {
                if (!command.CommandText.StartsWith(prefix, StringComparison.Ordinal))
                {
                    continue;
                }
                if (match != null)
                {
                    throw new InvalidOperationException("More than one matching command: " + prefix);
                }
                match = command;
            }
            if (match == null)
            {
                throw new InvalidOperationException("No matching command: " + prefix);
            }
            return match;
        }

        private static object ParameterValue(RecordingDbCommand command, int index)
        {
            return ((IDataParameter)command.Parameters[index]).Value;
        }

        private static int CountCharacter(string value, char character)
        {
            int count = 0;
            foreach (char candidate in value)
            {
                if (candidate == character)
                {
                    count++;
                }
            }
            return count;
        }

        private static DataTable TableWithRow(
            string[] columnNames,
            Type[] columnTypes,
            params object[] values)
        {
            var table = new DataTable();
            for (int index = 0; index < columnNames.Length; index++)
            {
                table.Columns.Add(columnNames[index], columnTypes[index]);
            }
            table.Rows.Add(values);
            return table;
        }

        private static bool ByteArraysEqual(byte[] left, byte[] right)
        {
            if (left.Length != right.Length)
            {
                return false;
            }
            for (int index = 0; index < left.Length; index++)
            {
                if (left[index] != right[index])
                {
                    return false;
                }
            }
            return true;
        }

        private sealed class InMemoryCredentialStore : IMaintenanceCredentialStore
        {
            private PasswordCredential _credential;

            public PasswordCredential Get()
            {
                return _credential;
            }

            public void Create(PasswordCredential credential)
            {
                if (_credential != null)
                {
                    throw new InvalidOperationException("Credential already exists.");
                }
                _credential = credential;
            }

            public void Replace(PasswordCredential credential)
            {
                if (_credential == null)
                {
                    throw new InvalidOperationException("Credential does not exist.");
                }
                _credential = credential;
            }
        }

        private sealed class DeterministicPasswordHasher : IPasswordHasher
        {
            private readonly string _expectedPassword;

            public DeterministicPasswordHasher(string expectedPassword)
            {
                _expectedPassword = expectedPassword;
            }

            public PasswordCredential Create(string password)
            {
                PasswordPolicy.Validate(password);
                return new PasswordCredential("TEST", 1, new byte[] { 1 }, new byte[] { 2 });
            }

            public bool Verify(string password, PasswordCredential credential)
            {
                return credential != null && string.Equals(
                    password, _expectedPassword, StringComparison.Ordinal);
            }
        }

        private sealed class AdjustableClock : IUtcClock
        {
            public AdjustableClock(DateTimeOffset utcNow)
            {
                UtcNow = utcNow;
            }

            public DateTimeOffset UtcNow { get; private set; }

            public void Advance(TimeSpan duration)
            {
                UtcNow = UtcNow.Add(duration);
            }
        }

        private sealed class RecordingWermDataStore : IWermDataStore
        {
            public int ProductSaveCount { get; private set; }
            public string LastChangedBy { get; private set; }
            public Product ProductToReturn { get; set; }
            public Customer CustomerToReturn { get; set; }
            public CustomerProductPrice PriceToReturn { get; set; }
            public string LastProductLookup { get; private set; }
            public long LastCustomerLookup { get; private set; }
            public string LastPriceTypeLookup { get; private set; }

            public Product GetProduct(string plu)
            {
                LastProductLookup = plu;
                return ProductToReturn;
            }

            public Customer GetCustomer(long customerId)
            {
                LastCustomerLookup = customerId;
                return CustomerToReturn;
            }

            public CustomerProductPrice GetCustomerProductPrice(
                long customerId,
                string productPlu,
                string priceType)
            {
                LastCustomerLookup = customerId;
                LastProductLookup = productPlu;
                LastPriceTypeLookup = priceType;
                return PriceToReturn;
            }

            public IReadOnlyList<ProductAuditEvent> GetProductAuditHistory(string plu)
            {
                return new List<ProductAuditEvent>().AsReadOnly();
            }

            public bool SaveProduct(Product product, string changedBy, string changeReason)
            {
                ProductSaveCount++;
                LastChangedBy = changedBy;
                return true;
            }

            public long SaveCustomer(Customer customer)
            {
                return customer.CustomerId;
            }

            public bool SaveCustomerProductPrice(
                CustomerProductPrice price,
                string changedBy,
                string changeReason)
            {
                return true;
            }
        }

        private sealed class FakeLabelDocumentFactory : ILabelDocumentFactory
        {
            private readonly ILabelDocument _document;

            public FakeLabelDocumentFactory(ILabelDocument document)
            {
                _document = document;
            }

            public string LastTemplatePath { get; private set; }

            public ILabelDocument CreateFromTemplate(string templatePath)
            {
                LastTemplatePath = templatePath;
                return _document;
            }
        }

        private sealed class FakeLabelDocument : ILabelDocument
        {
            private readonly IReadOnlyCollection<string> _availableFieldNames;

            public FakeLabelDocument(IEnumerable<string> availableFieldNames)
            {
                _availableFieldNames = new List<string>(availableFieldNames).AsReadOnly();
                Values = new Dictionary<string, string>(StringComparer.Ordinal);
            }

            public IReadOnlyCollection<string> AvailableFieldNames
            {
                get { return _availableFieldNames; }
            }

            public Dictionary<string, string> Values { get; private set; }
            public int PrintCount { get; private set; }
            public string PrinterName { get; private set; }
            public int Copies { get; private set; }
            public bool ThrowOnPrint { get; set; }
            public bool Disposed { get; private set; }

            public void SetField(string fieldName, string value)
            {
                Values[fieldName] = value;
            }

            public void Print(string printerName, int copies)
            {
                PrintCount++;
                PrinterName = printerName;
                Copies = copies;
                if (ThrowOnPrint)
                {
                    throw new InvalidOperationException("Simulated print failure.");
                }
            }

            public void Dispose()
            {
                Disposed = true;
            }
        }

        private sealed class RecordingLabelPrintService : ILabelPrintService
        {
            public LabelPrintJob LastJob { get; private set; }

            public void Print(LabelPrintJob job)
            {
                LastJob = job;
            }
        }

        private static void ExpectException<TException>(Action action)
            where TException : Exception
        {
            try
            {
                action();
            }
            catch (TException)
            {
                return;
            }

            throw new InvalidOperationException(
                "Expected exception " + typeof(TException).Name + " was not thrown.");
        }

        private static void Equal<T>(T expected, T actual, string fieldName)
        {
            if (!EqualityComparer<T>.Default.Equals(expected, actual))
            {
                throw new InvalidOperationException(
                    fieldName + " mismatch. Expected '" + expected + "' but received '" + actual + "'.");
            }
        }

        private static void WriteResults(
            string resultsPath,
            string sourceRevision,
            IList<TestResult> results)
        {
            string directory = Path.GetDirectoryName(Path.GetFullPath(resultsPath));
            Directory.CreateDirectory(directory);
            int failures = 0;
            foreach (TestResult result in results)
            {
                if (result.Failure != null)
                {
                    failures++;
                }
            }

            var settings = new XmlWriterSettings { Indent = true };
            using (XmlWriter writer = XmlWriter.Create(resultsPath, settings))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("testsuite");
                writer.WriteAttributeString("name", "WERM controlled tests");
                writer.WriteAttributeString("tests", results.Count.ToString());
                writer.WriteAttributeString("failures", failures.ToString());
                writer.WriteAttributeString("timestamp", DateTimeOffset.UtcNow.ToString("O"));
                writer.WriteAttributeString("source-revision", sourceRevision);
                writer.WriteAttributeString("machine", Environment.MachineName);
                writer.WriteAttributeString("process-architecture", Environment.Is64BitProcess ? "x64" : "x86");
                writer.WriteAttributeString("framework", Environment.Version.ToString());

                foreach (TestResult result in results)
                {
                    writer.WriteStartElement("testcase");
                    writer.WriteAttributeString("id", result.Test.Id);
                    writer.WriteAttributeString("requirement", result.Test.Requirement);
                    writer.WriteAttributeString("name", result.Test.Title);
                    writer.WriteAttributeString("time", result.DurationSeconds.ToString("0.000000"));
                    if (result.Failure != null)
                    {
                        writer.WriteStartElement("failure");
                        writer.WriteAttributeString("type", result.Failure.GetType().FullName);
                        writer.WriteString(result.Failure.ToString());
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
        }
    }
}
