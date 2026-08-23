# TC-0025: SQLite ODBC and Waughtal Shell Integration

**Status:** Controlled

**Requirements:** `REQ-0002`, `REQ-0019`, `REQ-0020`, `REQ-0021`

**Level:** Manual system and integration test

**Priority:** Required release gate

**Design references:** ADR-0006, ADR-0007, database installation instructions,
and `tools/install-werm-database.wsh`

**Technique:** Use-case testing, error guessing, and state-transition coverage
for missing, current, unrecognized, newer, and failed-migration databases

## Purpose

Verify real x86 and x64 SQLite ODBC connections and the operator-facing
Waughtal Shell installer against disposable database states. This cannot be
reliably automated on the general GitHub runner because WERM does not yet own a
baselined third-party ODBC/WSH deployment.

## Preconditions, environment, and assumptions

- A controlled Windows workstation or VM records its OS version and bitness.
- The approved execution-capable Waughtal Shell and approved SQLite ODBC driver
  are installed with exact versions and provenance recorded.
- Driver/DSN bitness matches each x86 or x64 WERM test configuration.
- The operator has a disposable directory and does not select production data.

## Inputs and initial state

Use the controlled version-1 migration, a new database path, the resulting
current database, an unrecognized non-empty SQLite database, a copy whose
schema version is newer than supported, and a disposable controlled failing
migration. Record the exact WSH command line without credentials.

## Procedure

1. Run `tools/install-werm-database.wsh` through Waughtal Shell for a missing
   database using the approved driver or DSN.
2. Record stdout, stderr, timestamps, exit status, process architecture,
   database digest, and installed dependency versions.
3. Inspect schema version, required tables, foreign-key enforcement, and the
   four append-only audit triggers through the real ODBC connection.
4. Run the WSH command again against the current database and confirm it is
   recognized without replacement or data loss.
5. Repeat the installer against the unrecognized, newer, and failing-migration
   disposable inputs; retain each distinct result.
6. Repeat applicable creation and current-database cases for x86 and x64.

## Expected results and pass criteria

New and current databases succeed; repeated execution preserves identity and
contents. Unrecognized and newer databases are rejected. A controlled failure
returns nonzero, rolls back changes, and removes only a newly created failed
database. All access is through `System.Data.Odbc`, and both architectures pass
with matching driver bitness.

## Postconditions and cleanup

Retain commands, logs, digests, version inventory, and the disposable passing
database as controlled evidence. Remove all other disposable inputs. Production
files and machine DSNs remain unchanged.
