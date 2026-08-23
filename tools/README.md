# WERM Tools

**Content type:** Implementation support

- [install-werm-database.wsh](install-werm-database.wsh) is the Waughtal Shell
  entry point for creating or validating the WERM SQLite database.
- [Install-WermDatabase.ps1](Install-WermDatabase.ps1) is the narrow ODBC
  worker launched by the Waughtal Shell script. It may be invoked directly
  until Waughtal Shell implements evaluation and Windows process launch.
- [Build-Werm.ps1](Build-Werm.ps1) builds the explicit x86 or x64 Debug/Release
  solution target.
- [Run-WermTests.ps1](Run-WermTests.ps1) executes the controlled runner and
  writes source- and architecture-identified XML.
- [Build-WpmPackage.ps1](Build-WpmPackage.ps1) builds and structurally validates
  an architecture-specific WPM package.
- [Test-WermTraceability.ps1](Test-WermTraceability.ps1) rejects missing,
  duplicate, or inconsistent requirement, test, and result relationships.
- [New-WermTestReport.ps1](New-WermTestReport.ps1) combines both architecture
  XML files, package digests, and manual statuses into Markdown and JSON.

See the [database installation instructions](../docs/database-installation.md)
for prerequisites, usage, expected results, and recovery guidance.
