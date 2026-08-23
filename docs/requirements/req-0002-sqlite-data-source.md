# REQ-0002: SQLite Data Source

**Content type:** Project requirement

**Status:** Accepted

**Source:** Stakeholder decision and ADR-0002

## Scope

This requirement applies to the label data used by WERM version 0.1.0.

## Requirement

WERM shall use a SQLite database as the source of label data.

## Rationale

The stakeholder selected SQLite for the initial label-printing application.

## Verification

**Method:** Inspection

**References:** `TC-0007`, `TC-0012`, `TC-0025`

Inspect the implemented data-access configuration and dependency baseline.
Verification passes when the application reads label data through a SQLite
database connection.

## Relationships

- **Derived from:** Stakeholder decision
- **Depends on:** [ADR-0002](../adr-0002-use-sqlite-data-source.md)
- **Conflicts with:** None known

## Tailoring

None.

## Implementation Record

The versioned SQLite schema, ODBC connection factory, and ODBC data store are
implemented. Verification through the approved SQLite ODBC driver remains
pending.
