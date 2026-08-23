# REQ-0021: Waughtal Shell Database Installer

**Content type:** Project requirement

**Status:** Accepted

**Source:** Stakeholder decision and ADR-0007

## Scope

This requirement applies to the operator-facing database installation entry
point for WERM version 0.1.0. Internal helpers may use another implementation
language when the approved design retains ODBC as the database boundary.

## Requirement

WERM shall provide a Waughtal Shell script that installs or validates its
version 0.1.0 database schema.

## Rationale

The stakeholder selected Waughtal Shell for database installation
orchestration.

## Verification

**Method:** Test and inspection

**References:** `TC-0025`, `TC-0028`

Inspect the installer against the accepted Waughtal Shell language and execute
it with the supported WSH, PowerShell, and SQLite ODBC configuration.
Verification passes when new and current databases produce the outcomes
required by `REQ-0020`, and the script preserves the worker's exit status.

## Relationships

- **Derived from:** Stakeholder decision
- **Depends on:** [REQ-0019](req-0019-sqlite-odbc-access.md)
- **Depends on:** [REQ-0020](req-0020-repeatable-database-installation.md)
- **Depends on:** [ADR-0007](../adr-0007-use-waughtal-shell-database-installer.md)
- **Conflicts with:** A Windows Script Host installer as the product entry point

## Tailoring

None.

## Implementation Record

Implemented by `tools/install-werm-database.wsh` with a temporary PowerShell
ODBC worker. Execution verification is blocked until an execution-capable
Waughtal Shell baseline is selected.
