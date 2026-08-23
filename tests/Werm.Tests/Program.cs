using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;
using System.Xml;
using Werm.Core;
using Werm.Core.Database;
using Werm.Core.Domain;
using Werm.Core.Security;

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
                    "REQ-0002,REQ-0007,REQ-0008,REQ-0009,REQ-0010,REQ-0011",
                    "Initial SQLite migration contract",
                    () => VerifyInitialMigration(repositoryRoot)),
                new ControlledTest(
                    "TC-0008",
                    "REQ-0020",
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
                    VerifyAuthenticationThrottling)
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
            if (migration.Batches.Count != 11)
            {
                throw new InvalidOperationException(
                    "Expected 11 controlled migration batches but found " + migration.Batches.Count + ".");
            }

            foreach (string tableName in InitialSchemaContract.ExpectedTableNames)
            {
                string pattern = @"(?im)^\s*CREATE\s+TABLE\s+" + Regex.Escape(tableName) + @"\s*\(";
                if (!Regex.IsMatch(migration.Sql, pattern))
                {
                    throw new InvalidOperationException("Missing required table definition: " + tableName);
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
