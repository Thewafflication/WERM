# ADR-0007: Use Waughtal Shell for Database Installation

**Content type:** Architecture decision record

**Status:** Accepted

**Date:** 2026-08-22

## Context

WERM version 0.1.0 needs a controlled entry point for creating and validating
its SQLite database through ODBC. The stakeholder selected Waughtal Shell
(`wsh`) as the installation scripting language.

Waughtal Shell 1.4.0 evaluates the accepted language, launches Windows
processes, and supplies the required path, filesystem, and structured process
library. It has no ODBC database API, so a narrow worker remains required.

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

`tools/Install-WermDatabase.ps1` is a narrow worker that owns the
`System.Data.Odbc` operations until Waughtal Shell provides a suitable database
interface or WERM provides a dedicated installer executable. Direct invocation
is a documented diagnostic path, not the supported operator contract.

For WSH 1.4.0 compatibility, the launcher uses installed `WERM_HOME` rather
than its currently unpopulated `$0` value and evaluates inside a function so
`return` propagates the worker exit status. CI executes that exact path.

Windows Script Host is not part of the selected installation design.

## Rationale

The design makes Waughtal Shell the stable operator contract. A narrow worker
keeps all schema work behind ODBC, while the versioned SQL migration remains
independent of both orchestration languages.

## Consequences

### Positive

- The requested Waughtal Shell entry point is explicit and reviewable.
- The complete WSH-to-ODBC path is exercised for x86 and x64.
- The same SQL migration is used by both invocation paths.
- WSH structured arguments avoid an intermediate command-line reparse.

### Negative

- Version 0.1.0 temporarily carries a PowerShell bootstrap dependency.
- The WSH process, PowerShell worker, and ODBC driver architectures must match.
- Release verification depends on the pinned Waughtal Shell 1.4.0 assets.

### Follow-up

- Re-evaluate the `$0` and top-level `exit` compatibility workarounds after a
  later WSH release implements those accepted contracts.
- Decide whether a WERM database-installer executable should replace the
  temporary PowerShell worker.

## References

- [Database installation](database-installation.md)
- [ADR-0006: Access SQLite through ODBC](adr-0006-access-sqlite-through-odbc.md)
- [REQ-0021: Waughtal Shell database installer](requirements/req-0021-waughtal-shell-database-installer.md)
- [Milestone 0.1.0 plan](milestones/milestone-0.1.0.md)
