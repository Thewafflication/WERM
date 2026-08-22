# ADR-0002: Use SQLite as the Label Data Source

**Content type:** Architecture decision record

**Status:** Accepted

**Date:** 2026-08-22

## Context

WERM version 0.1.0 needs a source for the data inserted into label templates.
The stakeholder selected SQLite as the database technology. The database
schema, query model, and record-selection workflow have not yet been defined.

## Decision Drivers

- The stakeholder specified SQLite.
- Label data must be available to a Windows desktop application.
- Version 0.1.0 should minimize database deployment and administration.

## Considered Options

1. SQLite database
2. A client-server relational database
3. Flat files such as CSV or JSON

## Decision

WERM version 0.1.0 will read label source data from a SQLite database. The
schema, ownership of database creation and migration, and write behavior are
outside this decision and remain open.

## Rationale

SQLite meets the stated requirement and provides a relational, file-based data
source without requiring a separate database service. A client-server system
would add administration that has not been justified for the initial release,
while flat files would not satisfy the selected database constraint.

## Consequences

### Positive

- Deployment does not require a separate database server.
- The application can use parameterized SQL and relational constraints.
- The database can be backed up as a controlled file when it is not in use.

### Negative

- File access, locking, and backup behavior must be defined.
- The application must validate the database path, schema, and retrieved data.
- Concurrent multi-user behavior may require later architectural review.

### Follow-up

- Define the database schema and ownership of migrations.
- Define how the application locates and opens the database.
- Define record selection, field mapping, and database error behavior.
- Select and baseline a .NET Framework 4.8-compatible SQLite ODBC driver.

## References

- [REQ-0002: SQLite data source](requirements/req-0002-sqlite-data-source.md)
- [REQ-0003: Label record retrieval](requirements/req-0003-label-record-retrieval.md)
- [ADR-0006: Access SQLite through ODBC](adr-0006-access-sqlite-through-odbc.md)
