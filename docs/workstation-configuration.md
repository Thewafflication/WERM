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
3. Create the SQLite database using the Waughtal Shell installer described in
   [Database installation](database-installation.md).
4. Set the connection and optional default-template values below.
5. Start WERM, initialize the maintenance password once, then unlock the
   Maintenance tab with an operator name and that password.

## Configuration values

Environment variables take precedence over keys in `Werm.exe.config`.

| Environment variable | Configuration key | Meaning |
| --- | --- | --- |
| `WERM_DATABASE_PATH` | `DatabasePath` | Full path to the initialized SQLite file; defaults to `%ProgramData%\WERM\werm.db` |
| `WERM_ODBC_DRIVER` | `OdbcDriverName` | Registered SQLite ODBC driver name for a driver connection |
| `WERM_ODBC_DSN` | `OdbcDsn` | Configured SQLite ODBC DSN; when set, it takes precedence over the driver name |
| `WERM_WORD_TEMPLATE` | `WordTemplatePath` | Optional default label-template path |

At least one of `WERM_ODBC_DRIVER` or `WERM_ODBC_DSN` must be set. The shipped
configuration intentionally does not guess a vendor-specific driver name.
WERM shows a configuration error in its status area when neither is present.

Printing itself does not require the maintenance password. Product, customer,
and customer-price changes do. Maintenance authorization expires after ten
minutes and can be ended immediately with the **Lock** button.

See the [Word template contract](word-template-contract.md) for the exact
content-control tags and output rules.
