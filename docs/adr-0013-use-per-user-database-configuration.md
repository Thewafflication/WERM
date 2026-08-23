# ADR-0013: Use Per-User Database Configuration

**Content type:** Architecture decision record

**Status:** Accepted

## Context

WERM originally required an operator or deployment system to set an ODBC
environment variable or edit application configuration before database use.
The stakeholder requested an in-application database configuration screen and
the ability to create the SQLite file when it is missing.

The installed executable normally resides below Program Files, so runtime
configuration must not depend on writing beside the executable. ODBC driver
registrations are architecture-specific. Creation must preserve the existing
safe migration behavior and must not replace an unrelated database.

## Decision

WERM stores the selected database path, ODBC driver or DSN, and optional Word
template in `%LOCALAPPDATA%\WERM\settings.xml`. The file has a versioned,
non-secret XML contract. It never contains a database password, maintenance
password, password verifier, or connection-string credential.

Application configuration supplies defaults, the per-user file supplies
interactive choices, and documented `WERM_*` environment variables take final
precedence for managed deployments. The screen displays active environment
overrides. Driver and DSN choices are read from the current process
architecture's Windows ODBC registry view and remain editable.

The application uses the same controlled SQL migration as the Waughtal Shell
installer. A missing or empty file may receive schema version 1 in one
transaction. A current WERM file is validated. Unrecognized, partial, or newer
databases are rejected without replacement; a newly-created incomplete file is
removed after failure. Database creation does not create the maintenance
credential—the operator initializes it through the existing password workflow.

## Consequences

- First-run setup can be completed inside WERM after the SQLite ODBC driver is
  installed.
- WPM removal does not delete user settings or the external database.
- A driver registered only for the other process architecture will not appear.
- Managed deployments can continue using environment variables.
- The Waughtal Shell database installer remains available for scripted and
  controlled installation evidence.

## Verification

`TC-0029` verifies the non-secret per-user persistence contract. `TC-0030`
verifies transactional application-side migration. `TC-0026` exercises the
physical screen and database workflow.

## References

- [REQ-0022](requirements/req-0022-database-configuration-screen.md)
- [Database installation](database-installation.md)
- [Workstation configuration](workstation-configuration.md)
