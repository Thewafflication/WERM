# Waughtal Enterprise Resource Management

WERM 0.1.0 is a Windows label-printing application. It maintains product,
customer, and customer-specific price data in SQLite through ODBC, populates a
Microsoft Word label template, and prints through Word to a label printer.

The first milestone deliberately excludes barcode and PDF behavior.

Engineering candidates currently build and package successfully, but 0.1.0 is
not approved for release. The [readiness record](docs/releases/0.1.0-readiness.md)
identifies the blocked interactive GUI, physical Word/label-printer, and
process-review gates. Real x86/x64 ODBC/WSH and clean WPM install/remove paths
are controlled in CI.

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

Packaging builds the pinned x86 or x64 SQLite ODBC runtime from digest-verified
source, records its provenance, and includes versioned install/remove
registration scripts. Waughtal Shell 1.4.0 is the controlled database-installer
orchestration baseline.

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
