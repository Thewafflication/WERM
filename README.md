# Waughtal Enterprise Resource Management

WERM 0.1.0 is a Windows label-printing application. It maintains product,
customer, and customer-specific price data in SQLite through ODBC, populates a
Microsoft Word label template, and prints through Word to a label printer.

The first milestone deliberately excludes barcode and PDF behavior.

Engineering candidates currently build and package successfully, but 0.1.0 is
not approved for release. The [readiness record](docs/releases/0.1.0-readiness.md)
identifies the blocked ODBC/WSH, GUI, physical Word/printer, clean-install, and
process-review gates.

## Build and test

The solution targets .NET Framework 4.8 and supports x86 and x64 builds:

```powershell
.\tools\Build-Werm.ps1 -Architecture x64 -Configuration Debug
.\tools\Run-WermTests.ps1 -Architecture x64 -Configuration Debug -NoBuild
```

Build a WPM package after a Release build:

```powershell
.\tools\Build-Werm.ps1 -Architecture x64 -Configuration Release
.\tools\Build-WpmPackage.ps1 -Architecture x64 -Configuration Release `
    -Version 0.1.0-dev
```

Project decisions, requirements, installation instructions, and the milestone
plan are indexed in [`docs/README.md`](docs/README.md).

For an operational workstation, follow
[`docs/workstation-configuration.md`](docs/workstation-configuration.md) and
author templates against
[`docs/word-template-contract.md`](docs/word-template-contract.md). The WPF
screen lets an operator select PLU, customer ID, price type, template, printer,
and copies. A Database configuration tab selects the SQLite/ODBC connection,
tests or creates the schema, and saves per-user settings. Business-data changes
are available only through an authenticated maintenance session.
