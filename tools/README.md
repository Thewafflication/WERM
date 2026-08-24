# WERM Tools

**Content type:** Implementation support

- [install-werm-database.wsh](install-werm-database.wsh) is the Waughtal Shell
  entry point for creating or validating the WERM SQLite database.
- [Install-WermDatabase.ps1](Install-WermDatabase.ps1) is the narrow ODBC
  worker launched by the Waughtal Shell 1.4.0 script.
- [Build-SqliteOdbc.ps1](Build-SqliteOdbc.ps1) builds the pinned SQLite ODBC
  0.99991 and SQLite 3.43.2 sources for x86 or x64 with verified inputs.
- [Install-SqliteOdbcDriver.ps1](Install-SqliteOdbcDriver.ps1) installs,
  verifies, or removes the versioned architecture-specific driver registration.
- [Test-WermSqliteOdbc.ps1](Test-WermSqliteOdbc.ps1) executes controlled real
  ODBC and Waughtal Shell integration states on a disposable runner.
- [Test-WermWpmInstall.ps1](Test-WermWpmInstall.ps1) proves WPM install,
  startup, external-state preservation, and removal on a disposable runner.
- [Build-Werm.ps1](Build-Werm.ps1) builds the explicit x86 or x64 Debug/Release
  solution target.
- [Run-WermTests.ps1](Run-WermTests.ps1) executes the controlled runner and
  writes source- and architecture-identified XML.
- [Build-WpmPackage.ps1](Build-WpmPackage.ps1) builds and structurally validates
  an architecture-specific WPM package.
- [Test-WermTraceability.ps1](Test-WermTraceability.ps1) rejects missing,
  duplicate, or inconsistent requirement, test, and result relationships.
- [New-WermTestReport.ps1](New-WermTestReport.ps1) combines both architecture
  XML and integration results, package digests, and manual statuses into
  Markdown and JSON.

See the [database installation instructions](../docs/database-installation.md)
for prerequisites, usage, expected results, and recovery guidance.
