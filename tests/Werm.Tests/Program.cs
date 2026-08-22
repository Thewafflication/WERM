using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Versioning;
using System.Xml;
using Werm.Core;

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
                    () => VerifyProcessArchitecture(expectedArchitecture))
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
