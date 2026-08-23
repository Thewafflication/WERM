# TC-0007: Initial Migration Contract

**Status:** Controlled

**Requirements:** `REQ-0002`, `REQ-0007`, `REQ-0008`, `REQ-0009`, `REQ-0010`,
`REQ-0011`

**Level:** Component test

## Objective

Verify that the packaged version-1 SQLite migration has the controlled batch,
table, version, and milestone-scope structure expected by application code.

## Procedure

1. Parse `database/migrations/0001-initial-schema.sql` with the same
   `-- WERM-BATCH` contract used by the installer.
2. Verify the fifteen non-empty batches, seven required table definitions, and
   four append-only trigger definitions.
3. Verify `PRAGMA user_version = 1`.
4. Inspect the SQL for deferred barcode scope.

## Expected Result

All expected structures are present, the version is one, and no barcode table
or field appears in the migration.

This contract test does not replace execution through the approved SQLite ODBC
driver; that integration test remains a release prerequisite.
