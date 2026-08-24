# WERM Database Installation

**Content type:** Installation instructions

## Purpose

The Waughtal Shell script `tools/install-werm-database.wsh` creates or validates
the WERM version 0.1.0 SQLite schema through ODBC. It orchestrates the
PowerShell ODBC worker and the controlled SQL migration stored in this
repository.

For interactive workstation setup, the WERM **Database configuration** tab can
perform the same version-1 create-or-validate operation from the packaged SQL
migration. The Waughtal Shell procedure remains the controlled scripting entry
point and is used for `TC-0025` evidence.

The architecture-specific WPM package includes a source-built SQLite ODBC
0.99991 driver linked with SQLite 3.43.2. WPM install registers the versioned
driver in the matching 32-bit or 64-bit ODBC registry view; WPM removal removes
only that exact registration. The source revisions, input digests, license, and
built driver digest are retained in the package.

## Waughtal Shell Baseline

WERM 0.1.0 baselines Waughtal Shell 1.4.0 in matching x86 and x64 release
archives. CI verifies the release-asset digest, executes the `.wsh` entry point,
and retains real ODBC results for both architectures.

WSH 1.4.0 does not yet populate its documented logical `$0` source name and
does not implement top-level `exit`. The WERM launcher therefore resolves its
installed worker through `WERM_HOME` and runs its body inside a function so
`return` preserves the worker status. WPM sets machine `WERM_HOME`; a source
checkout invocation must set it to the repository root for this baseline.

## Installed Schema

Schema version 1 contains:

- `WermSchemaVersion`;
- `MaintenanceCredential`;
- `Product`;
- `Customer`;
- `CustomerProductPrice`;
- `ProductAuditEvent`; and
- `ProductAuditChange`.

Barcode tables are deliberately absent from version 0.1.0.

The migration also installs four triggers that reject update or delete
operations against the two audit tables.

## Prerequisites

1. Use a supported Windows workstation.
2. Install the matching WERM WPM package, which installs the controlled SQLite
   ODBC driver. A source-tree test may instead use the controlled build and
   registration scripts under `tools/` from an administrative runner.
3. Install Waughtal Shell 1.4.0 with the same architecture as WERM and the ODBC
   driver.
4. Choose a local database location whose Windows access control permits WERM
   users to perform their authorized operations.
5. Retain `tools/` and `database/migrations/` together in the repository
   layout.

List registered SQLite drivers from PowerShell with:

```powershell
Get-OdbcDriver |
    Where-Object Name -Match 'SQLite' |
    Select-Object Name, Platform, Version
```

The installed driver name is
`WERM <WERM-version> SQLite3 ODBC Driver 0.99991 (<architecture>)`.

## Run Through Waughtal Shell

Use the architecture-specific WSH 1.4.0 executable. From an installed package,
a driver-name invocation is:

```powershell
& "$env:WSH_HOME\wsh.exe" `
    --non-interactive `
    "$env:WERM_HOME\tools\install-werm-database.wsh" `
    driver `
    'C:\ProgramData\Waughtal\WERM\Data\werm.db' `
    'WERM 0.1.0 SQLite3 ODBC Driver 0.99991 (x64)'
```

For a configured User or System DSN, use:

```powershell
& "$env:WSH_HOME\wsh.exe" `
    --non-interactive `
    "$env:WERM_HOME\tools\install-werm-database.wsh" `
    dsn `
    'C:\ProgramData\Waughtal\WERM\Data\werm.db' `
    'WERM'
```

The Waughtal Shell script validates its arguments and required files, locates
`powershell.exe` through structured process resolution, invokes the ODBC
worker, and returns the worker's exit status.

## Direct Worker Diagnostic

The PowerShell worker may be run directly for diagnosis; the supported
operator entry point remains Waughtal Shell. This still uses `System.Data.Odbc`
and does not bypass the selected database boundary:

```powershell
& "$env:WINDIR\System32\WindowsPowerShell\v1.0\powershell.exe" `
    -NoLogo `
    -NoProfile `
    -NonInteractive `
    -ExecutionPolicy Bypass `
    -File tools\Install-WermDatabase.ps1 `
    -DatabasePath 'C:\ProgramData\Waughtal\WERM\Data\werm.db' `
    -DriverName 'WERM 0.1.0 SQLite3 ODBC Driver 0.99991 (x64)'
```

For a 32-bit ODBC driver on 64-bit Windows, use the 32-bit host:

```text
%WINDIR%\SysWOW64\WindowsPowerShell\v1.0\powershell.exe
```

To use a DSN, replace `-DriverName` and its value with `-Dsn 'WERM'`.

## Expected Result

A successful first run reports that migration `0001-initial-schema.sql` was
applied and schema verification passed. It exits with code `0`.

Running the same command again is safe. It reports that schema version 1 is
already installed, verifies the expected tables and foreign-key setting, and
exits with code `0` without recreating the schema.

The installer refuses to modify:

- an existing non-empty SQLite database without `WermSchemaVersion`; or
- a WERM database whose schema version is newer than the installer supports.

## Exit Codes

| Code | Meaning |
| ---: | --- |
| `0` | Database is installed and verified |
| `2` | Invalid Waughtal Shell installer arguments |
| `4` | ODBC connection, migration, or schema verification failed |
| `5` | Required installer file or path is unavailable |
| `7` | Windows PowerShell cannot be resolved by Waughtal Shell |

## Failure and Recovery

Migration 1 executes in a database transaction. On failure, the worker attempts
to roll back the migration. If the database file did not exist before the run,
the worker then removes only the incomplete file it created.

The installer never removes or replaces a database that existed before it was
started. Back up an existing WERM database before using a future installer that
contains an upgrade migration.

If installation fails:

1. retain the complete console output;
2. confirm the process and ODBC driver architectures match;
3. confirm the registered driver or DSN name exactly matches the command;
4. confirm the target directory is writable;
5. confirm the migration file is present; and
6. rerun only after correcting the reported prerequisite or configuration.

## Security Boundary

The maintenance password prevents unauthorized changes through the WERM GUI;
it does not encrypt or protect the SQLite file from direct access. Restrict the
database directory with Windows access control and include the file in the
approved backup procedure.

Do not place credentials in the command line, DSN, migration file, or committed
connection string. SQLite does not require a database username or password for
this design.

## Authoritative Files

- [Waughtal Shell installer](../tools/install-werm-database.wsh)
- [ODBC worker](../tools/Install-WermDatabase.ps1)
- [SQLite ODBC reproducible build](../tools/Build-SqliteOdbc.ps1)
- [SQLite ODBC registration](../tools/Install-SqliteOdbcDriver.ps1)
- [Initial schema migration](../database/migrations/0001-initial-schema.sql)
- [Database design](database-design.md)
- [ODBC architecture decision](adr-0006-access-sqlite-through-odbc.md)

## Supporting References

- [Waughtal Shell language specification](https://github.com/Thewafflication/wsh/blob/master/docs/specification/language.md)
- [Waughtal Shell standard library](https://github.com/Thewafflication/wsh/blob/master/docs/specification/standard-library.md)
- [Third-party notices](third-party-notices.md)
