# REQ-0020: Repeatable Database Installation

**Content type:** Project requirement

**Status:** Accepted

**Source:** Stakeholder need

## Scope

This requirement applies to initial creation and repeated validation of the
WERM version 0.1.0 product database. Upgrade behavior beyond schema version 1
is outside the current scope.

## Requirement

The WERM database installer shall create or recognize the version 0.1.0 schema
through the baselined SQLite ODBC configuration without replacing an existing
database.

## Rationale

A repeatable installer prevents manual schema drift and protects existing
database files during setup.

## Verification

**Method:** Test

**References:** `TC-0008`, `TC-0025`, `TC-0028`

Run the installer against a missing database, the resulting current database,
an unrecognized non-empty database, a database newer than the installer, and a
controlled failing migration. Verification passes when creation and repeated
validation succeed, unsafe inputs are rejected, and existing files are not
replaced.

## Relationships

- **Derived from:** Stakeholder need
- **Depends on:** [REQ-0019](req-0019-sqlite-odbc-access.md)
- **Depends on:** [Database installation](../database-installation.md)
- **Conflicts with:** Destructive database replacement during installation

## Tailoring

None.

## Implementation Record

Implemented initially by `tools/install-werm-database.wsh`, its temporary
PowerShell ODBC worker, and `database/migrations/0001-initial-schema.sql`.
Release verification remains pending an execution-capable Waughtal Shell
baseline and selection of the SQLite ODBC driver.
