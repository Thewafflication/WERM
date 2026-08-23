# Workstation Configuration

WERM 0.1.0 requires Windows with .NET Framework 4.8, Microsoft Word desktop,
a SQLite ODBC driver, and an installed Windows label-printer driver. Install
the x86 or x64 WPM package whose architecture matches both Word and the SQLite
ODBC driver. Mixed-bitness components cannot communicate in this design.

Before starting WERM:

1. Install Microsoft Word desktop and confirm it can open the reviewed label
   template and print a test page to the label printer.
2. Install or configure the approved SQLite ODBC driver or DSN with the same
   bitness as WERM.
3. Start WERM and open **Database configuration**. Choose the SQLite file and
   a registered driver or DSN. Use **Create or validate database**; a missing
   file is created with the controlled schema. Then use **Save settings**.
4. Optionally use the Waughtal Shell installer described in
   [Database installation](database-installation.md) for scripted deployment.
5. Initialize the maintenance password once, then unlock the
   Maintenance tab with an operator name and that password.

## Configuration values

Interactive choices are stored in `%LOCALAPPDATA%\WERM\settings.xml` for the
current Windows user. Environment variables take precedence over the user
file and keys in `Werm.exe.config`; the screen identifies active overrides.

| Environment variable | Configuration key | Meaning |
| --- | --- | --- |
| `WERM_DATABASE_PATH` | `DatabasePath` | Full path to the initialized SQLite file; defaults to `%ProgramData%\WERM\werm.db` |
| `WERM_ODBC_DRIVER` | `OdbcDriverName` | Registered SQLite ODBC driver name for a driver connection |
| `WERM_ODBC_DSN` | `OdbcDsn` | Configured SQLite ODBC DSN; when set, it takes precedence over the driver name |
| `WERM_WORD_TEMPLATE` | `WordTemplatePath` | Optional default label-template path |

At least one driver or DSN must be selected in the screen or supplied by
configuration. The shipped configuration intentionally does not guess a
vendor-specific driver name. WERM shows a configuration error in its status
area when neither is present.

The settings file contains only paths and ODBC registration names. It does not
contain a maintenance password or verifier. Database creation safely applies
the packaged version-1 migration to a missing or empty file, validates a
current WERM database, and refuses to replace an unrecognized or newer file.

Printing itself does not require the maintenance password. Product, customer,
and customer-price changes do. Maintenance authorization expires after ten
minutes and can be ended immediately with the **Lock** button.

See the [Word template contract](word-template-contract.md) for the exact
content-control tags and output rules.
