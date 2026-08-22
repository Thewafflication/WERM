# Controlled Test Specifications

| Test | Requirement | Purpose |
| --- | --- | --- |
| [TC-0001](tc-0001-dotnet-framework-target.md) | `REQ-0001` | Verify the compiled .NET Framework 4.8 identity |
| [TC-0002](tc-0002-application-identity.md) | `REQ-0001` | Verify the shared WERM 0.1.0 application identity |
| [TC-0003](tc-0003-explicit-architecture.md) | `ADR-0008` | Verify that x86 and x64 builds execute with explicit bitness |

The executable runner writes architecture, runtime, source revision, timestamp,
verdict, and diagnostics to XML. GitHub Actions retains the Debug executable,
PDB, dependency assemblies, and XML for 30 days for each supported
architecture.
