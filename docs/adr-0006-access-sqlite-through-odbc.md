# ADR-0006: Access SQLite through ODBC

**Content type:** Architecture decision record

**Status:** Accepted

**Date:** 2026-08-22

## Context

WERM version 0.1.0 requires a .NET Framework 4.8 data-access mechanism for its
SQLite product database. The stakeholder selected Open Database Connectivity
(ODBC) rather than a SQLite-specific ADO.NET provider.

## Decision Drivers

- The stakeholder requires SQLite access through ODBC.
- Data access must work from a .NET Framework 4.8 desktop application.
- Deployment must identify and reproduce the required database driver.
- The selected process architecture and ODBC driver architecture must match.

## Considered Options

1. `System.Data.Odbc` with an installed SQLite ODBC driver
2. A SQLite-specific ADO.NET provider
3. Direct use of the native SQLite API

## Decision

WERM will access SQLite through the .NET Framework `System.Data.Odbc` API and
an installed SQLite ODBC driver. Application data-access code will depend on
ODBC abstractions rather than a SQLite-specific ADO.NET provider.

The exact driver, driver version, application architecture, installation
method, and data-source-name (DSN) model remain open until the M1.2
compatibility spike. Both a configured DSN and a DSN-less connection string
remain candidates.

## Rationale

The decision satisfies the stakeholder's requested database boundary while
using the ODBC support available to .NET Framework applications. Deferring the
exact driver and DSN model permits verification of deployment, bitness,
transactions, parameter handling, and SQLite behavior before they become part
of the release baseline.

## Consequences

### Positive

- Application data access uses a standard database interface.
- The selected SQLite driver can be tested and controlled as a deployment
  prerequisite.
- Business and application services remain isolated from driver-specific APIs.

### Negative

- Every workstation requires a compatible SQLite ODBC driver.
- The application and driver architectures must be compatible.
- ODBC SQL parameters are positional, so command parameters must be added in
  the same order as their `?` placeholders.
- SQLite connection initialization, transactions, schema metadata, error
  mapping, and concurrency behavior require driver-specific verification.
- Driver installation and configuration become release and support concerns.

### Follow-up

- Select and baseline the SQLite ODBC driver and its license.
- Decide the supported application architecture and matching driver bitness.
- Select a configured DSN or DSN-less connection model.
- Define secure storage and validation of the database connection settings.
- Verify foreign keys, transactions, migrations, parameter binding, Unicode,
  timestamps, large ingredients statements, locking, and error behavior.
- Document driver installation, repair, upgrade, and removal.

## References

- [ADR-0002: Use SQLite as the label data source](adr-0002-use-sqlite-data-source.md)
- [REQ-0002: SQLite data source](requirements/req-0002-sqlite-data-source.md)
- [REQ-0019: SQLite ODBC access](requirements/req-0019-sqlite-odbc-access.md)
- [Database design](database-design.md)
- [Milestone 0.1.0 plan](milestones/milestone-0.1.0.md)
