# REQ-0019: SQLite ODBC Access

**Content type:** Project requirement

**Status:** Accepted

**Source:** Stakeholder decision and ADR-0006

## Scope

This requirement applies to every runtime database connection made by WERM
version 0.1.0. Test utilities that validate a database file without executing
product behavior may use another interface when their evidence remains
configuration-equivalent.

## Requirement

WERM shall access its SQLite product database through ODBC.

## Rationale

The stakeholder selected ODBC as the database-access interface.

## Verification

**Method:** Test and inspection

**References:** `TC-0008`, `TC-0012`, `TC-0013`–`TC-0015`, `TC-0018`, `TC-0019`;
approved-driver integration tests pending

Inspect the application dependency and data-access boundaries, then execute the
database integration tests with the baselined SQLite ODBC driver. Verification
passes when all runtime product connections use `System.Data.Odbc` and the
required database behaviors pass through the supported driver configuration.

## Relationships

- **Derived from:** Stakeholder decision
- **Depends on:** [REQ-0002](req-0002-sqlite-data-source.md)
- **Depends on:** [ADR-0006](../adr-0006-access-sqlite-through-odbc.md)
- **Conflicts with:** Runtime use of a SQLite-specific ADO.NET provider

## Tailoring

None.

## Implementation Record

`Werm.Data` uses `System.Data.Odbc.OdbcConnection` for production connections,
supports registered-driver and DSN connection strings, enables foreign keys
and the busy timeout on every connection, and maps all SQL values through
ordered positional parameters. The exact driver and supported configuration
will be established by the M1.2 compatibility spike.
