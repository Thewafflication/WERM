# Controlled Test Specifications

`TC-0001` through `TC-0024` and `TC-0029` through `TC-0030` incorporate the
[automated test execution contract](automated-test-execution-contract.md).
Environment-dependent release tests `TC-0025` through `TC-0028` define their
complete manual procedures and retain operator observations separately.

| Test | Requirement | Purpose |
| --- | --- | --- |
| [TC-0001](tc-0001-dotnet-framework-target.md) | `REQ-0001` | Verify the compiled .NET Framework 4.8 identity |
| [TC-0002](tc-0002-application-identity.md) | `REQ-0001` | Verify the shared WERM 0.1.0 application identity |
| [TC-0003](tc-0003-explicit-architecture.md) | `ADR-0008` | Verify that x86 and x64 builds execute with explicit bitness |
| [TC-0004](tc-0004-product-required-values.md) | `REQ-0007`, `REQ-0008` | Verify required product identity and description rules |
| [TC-0005](tc-0005-product-label-facts.md) | `REQ-0009`, `REQ-0010` | Verify ingredients and safe-handling states |
| [TC-0006](tc-0006-customer-product-price.md) | `REQ-0011` | Verify customer/product/price identity and amount rules |
| [TC-0007](tc-0007-initial-migration-contract.md) | `REQ-0002`, `REQ-0007`–`REQ-0011`, `REQ-0015` | Verify the version-1 migration structure and scope |
| [TC-0008](tc-0008-installer-safe-failure.md) | `REQ-0019`, `REQ-0020` | Verify cleanup after a new-database connection failure |
| [TC-0009](tc-0009-maintenance-password-verifier.md) | `REQ-0012` | Verify salted password hashing and policy limits |
| [TC-0010](tc-0010-maintenance-session.md) | `REQ-0012` | Verify password-gated and expiring write authorization |
| [TC-0011](tc-0011-authentication-throttling.md) | `REQ-0012` | Verify temporary lockout after repeated failures |
| [TC-0012](tc-0012-odbc-connection-contract.md) | `REQ-0002`, `REQ-0019` | Verify driver/DSN strings and absence of embedded credentials |
| [TC-0013](tc-0013-atomic-product-audit.md) | `REQ-0013`, `REQ-0014`, `REQ-0019` | Verify one transaction contains product state and audit inserts |
| [TC-0014](tc-0014-audit-failure-rollback.md) | `REQ-0013`, `REQ-0019` | Verify an audit failure rolls back the product write |
| [TC-0015](tc-0015-price-audit-lineage.md) | `REQ-0011`, `REQ-0013`, `REQ-0014`, `REQ-0019` | Verify customer-price differences and parent revision lineage |
| [TC-0016](tc-0016-write-authorization-boundary.md) | `REQ-0012`, `REQ-0016`–`REQ-0018` | Verify application writes require a valid maintenance session |
| [TC-0017](tc-0017-append-only-audit-schema.md) | `REQ-0015` | Verify database triggers protect existing audit rows |
| [TC-0018](tc-0018-credential-persistence.md) | `REQ-0012`, `REQ-0019` | Verify Base64 verifier mapping through parameterized ADO.NET commands |
| [TC-0019](tc-0019-customer-insertion.md) | `REQ-0017`, `REQ-0019` | Verify parameterized customer insertion without deletion |
| [TC-0020](tc-0020-label-field-mapping.md) | `REQ-0003`, `REQ-0004` | Verify the nine product/customer/price field mappings and formats |
| [TC-0021](tc-0021-word-template-population.md) | `REQ-0004`, `REQ-0005` | Verify tagged-field population and direct-print arguments |
| [TC-0022](tc-0022-missing-word-field.md) | `REQ-0004` | Verify a missing required Word tag fails before population or printing |
| [TC-0023](tc-0023-label-record-workflow.md) | `REQ-0003`, `REQ-0005` | Verify selected-record retrieval and print-job creation |
| [TC-0024](tc-0024-print-failure-cleanup.md) | `REQ-0005` | Verify working-document cleanup after a print failure |
| [TC-0025](tc-0025-sqlite-odbc-wsh-integration.md) | `REQ-0002`, `REQ-0019`–`REQ-0021` | Verify real SQLite ODBC and Waughtal Shell installation states on x86/x64 |
| [TC-0026](tc-0026-authorized-gui-maintenance.md) | `REQ-0012`, `REQ-0015`–`REQ-0018` | Verify the authenticated WPF maintenance and audit-history workflow |
| [TC-0027](tc-0027-word-label-printer-integration.md) | `REQ-0003`–`REQ-0006` | Verify actual Word automation and physical label output |
| [TC-0028](tc-0028-wpm-clean-install.md) | `REQ-0001`, `REQ-0006`, `REQ-0020`, `REQ-0021` | Verify clean WPM install, launch, recovery, and removal |
| [TC-0029](tc-0029-database-settings-persistence.md) | `REQ-0022` | Verify versioned, non-secret per-user database settings |
| [TC-0030](tc-0030-application-database-creation.md) | `REQ-0020`, `REQ-0022` | Verify application-side transactional schema creation |

The executable runner writes architecture, runtime, source revision, timestamp,
verdict, and diagnostics to XML. GitHub Actions retains the Debug executable,
PDB, dependency assemblies, and XML for 30 days for each supported
architecture.
