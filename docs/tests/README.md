# Controlled Test Specifications

| Test | Requirement | Purpose |
| --- | --- | --- |
| [TC-0001](tc-0001-dotnet-framework-target.md) | `REQ-0001` | Verify the compiled .NET Framework 4.8 identity |
| [TC-0002](tc-0002-application-identity.md) | `REQ-0001` | Verify the shared WERM 0.1.0 application identity |
| [TC-0003](tc-0003-explicit-architecture.md) | `ADR-0008` | Verify that x86 and x64 builds execute with explicit bitness |
| [TC-0004](tc-0004-product-required-values.md) | `REQ-0007`, `REQ-0008` | Verify required product identity and description rules |
| [TC-0005](tc-0005-product-label-facts.md) | `REQ-0009`, `REQ-0010` | Verify ingredients and safe-handling states |
| [TC-0006](tc-0006-customer-product-price.md) | `REQ-0011` | Verify customer/product/price identity and amount rules |
| [TC-0007](tc-0007-initial-migration-contract.md) | `REQ-0002`, `REQ-0007`–`REQ-0011` | Verify the version-1 migration structure and scope |
| [TC-0008](tc-0008-installer-safe-failure.md) | `REQ-0020` | Verify cleanup after a new-database connection failure |

The executable runner writes architecture, runtime, source revision, timestamp,
verdict, and diagnostics to XML. GitHub Actions retains the Debug executable,
PDB, dependency assemblies, and XML for 30 days for each supported
architecture.
