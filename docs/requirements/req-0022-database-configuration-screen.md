# REQ-0022: Database Configuration Screen

**Content type:** Project requirement

**Status:** Accepted

**Source:** Stakeholder need

## Scope

This requirement applies to the WERM 0.1.0 Windows desktop application and its
SQLite ODBC connection settings.

## Requirement

WERM shall provide a database-configuration screen that lets the operator
select a SQLite file, select or enter a matching-bitness registered ODBC driver
or DSN, test an existing database, save per-user settings, and safely create
and initialize the WERM schema when the selected database file does not exist.

## Rationale

An operator should not need to edit `Werm.exe.config` or define environment
variables before first use. Database creation from the same screen removes an
otherwise unnecessary setup boundary while retaining the controlled schema.

## Verification

**Method:** Test and demonstration

**References:** `TC-0026`, `TC-0029`, `TC-0030`

Verify that non-secret settings round-trip through the per-user file, schema
creation uses the controlled migration transaction, and the physical WPF
screen can enumerate the selected architecture's registrations, test, create,
save, reload, and reject an unsafe existing database.

## Relationships

- **Derived from:** Stakeholder need
- **Depends on:** [REQ-0019](req-0019-sqlite-odbc-access.md)
- **Depends on:** [REQ-0020](req-0020-repeatable-database-installation.md)
- **Depends on:** [ADR-0013](../adr-0013-use-per-user-database-configuration.md)
- **Conflicts with:** Storing database or maintenance credentials in the user settings file

## Tailoring

Environment variables remain supported for managed deployment and take
precedence over per-user settings. The screen identifies active overrides.

## Implementation Record

Implemented by the Database configuration tab, `WermSettingsStore`, the ODBC
registry catalog, application service reload, and `WermDatabaseInstaller`.
The Waughtal Shell installer remains supported for scripted deployment.
