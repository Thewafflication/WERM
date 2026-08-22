# ADR-0007: Use Waughtal Shell for Database Installation

**Content type:** Architecture decision record

**Status:** Accepted

**Date:** 2026-08-22

## Context

WERM version 0.1.0 needs a controlled entry point for creating and validating
its SQLite database through ODBC. The stakeholder selected Waughtal Shell
(`wsh`) as the installation scripting language.

The current Waughtal Shell implementation can parse the accepted language but
does not yet evaluate commands or launch Windows processes. Its accepted 1.0
standard library also has no database API.

## Decision Drivers

- The stakeholder requires a Waughtal Shell installation script.
- Database creation must use the selected ODBC boundary.
- The schema must remain a controlled, versioned project artifact.
- The installer must be runnable before WERM 0.1.0 is released.

## Considered Options

1. Waughtal Shell orchestration with a narrow ODBC worker
2. Windows Script Host and ADO
3. PowerShell as the permanent operator-facing installer
4. Manual SQL execution through an ODBC administration tool

## Decision

`tools/install-werm-database.wsh` will be the operator-facing database
installer. It will validate its inputs and files, locate Windows PowerShell
through Waughtal Shell's structured process interface, run the ODBC worker,
and preserve the worker's exit status.

`tools/Install-WermDatabase.ps1` is a temporary bootstrap worker that owns the
`System.Data.Odbc` operations until Waughtal Shell provides a suitable database
interface or WERM provides a dedicated installer executable. The worker may be
invoked directly while the Waughtal Shell evaluator and process facilities are
unavailable.

Windows Script Host is not part of the selected installation design.

## Rationale

The design makes Waughtal Shell the stable operator contract without claiming
capabilities that its current executable does not have. A narrow worker keeps
all schema work behind ODBC and provides an immediately testable bootstrap
path. The versioned SQL migration remains independent of both orchestration
languages.

## Consequences

### Positive

- The requested Waughtal Shell entry point is explicit and reviewable.
- Current development can exercise the ODBC migration before WSH execution is
  available.
- The same SQL migration is used by both invocation paths.
- WSH structured arguments avoid an intermediate command-line reparse.

### Negative

- The complete Waughtal Shell path cannot run on the current WSH baseline.
- Version 0.1.0 temporarily carries a PowerShell bootstrap dependency.
- The WSH process, PowerShell worker, and ODBC driver architectures must match.
- Release verification depends on an execution-capable Waughtal Shell build.

### Follow-up

- Baseline a Waughtal Shell version with evaluation, process, path, and
  filesystem support.
- Verify the `.wsh` script against that exact WSH version and architecture.
- Select and verify the SQLite ODBC driver.
- Decide whether a WERM database-installer executable should replace the
  temporary PowerShell worker.
- Remove the direct bootstrap instructions when the WSH path is supported.

## References

- [Database installation](database-installation.md)
- [ADR-0006: Access SQLite through ODBC](adr-0006-access-sqlite-through-odbc.md)
- [REQ-0021: Waughtal Shell database installer](requirements/req-0021-waughtal-shell-database-installer.md)
- [Milestone 0.1.0 plan](milestones/milestone-0.1.0.md)
