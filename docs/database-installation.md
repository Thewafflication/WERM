# WERM Database Installation

**Content type:** Installation instructions

## Purpose

The Waughtal Shell script `tools/install-werm-database.wsh` creates or validates
the WERM version 0.1.0 SQLite schema through ODBC. It orchestrates the
PowerShell ODBC worker and the controlled SQL migration stored in this
repository.

The installer does not download or silently select an ODBC driver. The project
must approve and baseline the driver, architecture, version, source, license,
and installer before release use.

## Waughtal Shell Availability

The current Waughtal Shell implementation has completed its lexer and parser,
but not language evaluation, Windows process execution, or its standard
library. It can parse the installer source but cannot execute it yet.

The `.wsh` script is written against the accepted Waughtal Shell 1.0 language
and standard-library contracts. Until an execution-capable Waughtal Shell
release is available, invoke `Install-WermDatabase.ps1` directly as described
below. The worker uses `System.Data.Odbc`; it does not bypass the selected ODBC
boundary.

## Installed Schema

Schema version 1 contains:

- `WermSchemaVersion`;
- `Product`;
- `Customer`;
- `CustomerProductPrice`;
- `ProductAuditEvent`; and
- `ProductAuditChange`.

Barcode tables are deliberately absent from version 0.1.0.

## Prerequisites

1. Use a supported Windows workstation.
2. Install the approved SQLite ODBC driver.
3. Match the PowerShell or Waughtal Shell process architecture to the ODBC
   driver architecture.
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

No SQLite ODBC driver was registered on the development workstation when these
instructions were created. Driver installation remains an M1.2 prerequisite.

## Run Through Waughtal Shell

After an execution-capable Waughtal Shell release is available, use its
architecture-specific executable. From PowerShell, a driver-name invocation is:

```powershell
& "$env:WSH_HOME\wsh.exe" `
    --non-interactive `
    --safe-path `
    tools\install-werm-database.wsh `
    driver `
    'C:\ProgramData\Waughtal\WERM\Data\werm.db' `
    'REGISTERED SQLITE ODBC DRIVER NAME'
```

For a configured User or System DSN, use:

```powershell
& "$env:WSH_HOME\wsh.exe" `
    --non-interactive `
    --safe-path `
    tools\install-werm-database.wsh `
    dsn `
    'C:\ProgramData\Waughtal\WERM\Data\werm.db' `
    'WERM'
```

The Waughtal Shell script validates its arguments and required files, locates
`powershell.exe` through structured process resolution, invokes the ODBC
worker, and returns the worker's exit status.

## Current Bootstrap Invocation

Until Waughtal Shell can execute the orchestration script, run its ODBC worker
directly:

```powershell
& "$env:WINDIR\System32\WindowsPowerShell\v1.0\powershell.exe" `
    -NoLogo `
    -NoProfile `
    -NonInteractive `
    -ExecutionPolicy Bypass `
    -File tools\Install-WermDatabase.ps1 `
    -DatabasePath 'C:\ProgramData\Waughtal\WERM\Data\werm.db' `
    -DriverName 'REGISTERED SQLITE ODBC DRIVER NAME'
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
- [Initial schema migration](../database/migrations/0001-initial-schema.sql)
- [Database design](database-design.md)
- [ODBC architecture decision](adr-0006-access-sqlite-through-odbc.md)

## Supporting References

- [Waughtal Shell language specification](https://github.com/Thewafflication/wsh/blob/master/docs/specification/language.md)
- [Waughtal Shell standard library](https://github.com/Thewafflication/wsh/blob/master/docs/specification/standard-library.md)
- [Microsoft OLE DB Provider for ODBC](https://learn.microsoft.com/en-us/office/client-developer/access/desktop-database-reference/microsoft-ole-db-provider-for-odbc)
- [ADO Connection Open method](https://learn.microsoft.com/en-us/office/client-developer/access/desktop-database-reference/open-method-ado-connection)
