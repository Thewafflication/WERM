# ADR-0008: Use WPF with x86 and x64 Packages

**Status:** Accepted

**Date:** 2026-08-22

## Context

WERM 0.1.0 requires a Windows desktop GUI and must use Microsoft Word and a
SQLite ODBC driver installed on the workstation. Those COM and ODBC components
must be usable from the application's process architecture.

## Decision

WERM 0.1.0 will use Windows Presentation Foundation on .NET Framework 4.8. The
solution will build and test explicit x86 and x64 targets, and WPM will produce
one package for each architecture.

ARM64 is not a 0.1.0 release target. Adding it requires evidence that the chosen
Word installation, SQLite ODBC driver, .NET Framework runtime, and printing
workflow all support the claimed configuration.

## Consequences

- Operators select the WERM package whose architecture matches Word and the
  configured SQLite ODBC driver.
- `AnyCPU` is not a release configuration, preventing an implicit bitness
  change on different workstations.
- The UI can use data binding and testable application-service boundaries.
- The supported Windows, Word, printer, and ODBC versions still require
  confirmation before release approval.

## References

- [ADR-0001](adr-0001-target-dotnet-framework-4-8.md)
- [ADR-0003](adr-0003-use-word-for-label-printing.md)
- [ADR-0006](adr-0006-access-sqlite-through-odbc.md)
- [Milestone 0.1.0](milestones/milestone-0.1.0.md)
