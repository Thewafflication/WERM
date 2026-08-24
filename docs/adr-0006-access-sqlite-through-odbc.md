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

WERM baselines the SQLite ODBC Driver 0.99991 source at revision
`539531394dcedf415de574daa95367a93f5eb41d`, linked with SQLite 3.43.2. The
reproducible MSVC build verifies both source-archive digests and records the
built driver digest. Each x86 or x64 WPM package registers its matching driver
under a versioned WERM name. WERM uses a DSN-less driver-name connection by
default and continues to allow an explicitly configured DSN.

## Rationale

The decision satisfies the stakeholder's requested database boundary while
using the ODBC support available to .NET Framework applications. Owning the
source build and versioned registration makes deployment and bitness
reproducible without silently trusting an unversioned workstation driver.

## Consequences

### Positive

- Application data access uses a standard database interface.
- The selected SQLite driver can be tested and controlled as a deployment
  prerequisite.
- Business and application services remain isolated from driver-specific APIs.

### Negative

- Every workstation requires the package-installed SQLite ODBC registration.
- The application and driver architectures must be compatible.
- ODBC SQL parameters are positional, so command parameters must be added in
  the same order as their `?` placeholders.
- SQLite connection initialization, transactions, schema metadata, error
  mapping, and concurrency behavior require driver-specific verification.
- Driver installation and configuration become release and support concerns.

### Verification

- `TC-0025` builds and registers both driver architectures on disposable
  runners and exercises missing, current, unrecognized, newer, and failed
  migration states through Waughtal Shell and `System.Data.Odbc`.
- `TC-0028` verifies package installation, driver registration, launch, and
  removal on an administrative ephemeral Windows runner.
- Driver license and provenance are recorded in
  [third-party notices](third-party-notices.md).

## References

- [ADR-0002: Use SQLite as the label data source](adr-0002-use-sqlite-data-source.md)
- [REQ-0002: SQLite data source](requirements/req-0002-sqlite-data-source.md)
- [REQ-0019: SQLite ODBC access](requirements/req-0019-sqlite-odbc-access.md)
- [Database design](database-design.md)
- [Milestone 0.1.0 plan](milestones/milestone-0.1.0.md)
