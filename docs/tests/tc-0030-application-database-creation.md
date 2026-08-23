# TC-0030: Application Database Creation

**Status:** Controlled

**Requirements:** `REQ-0020`, `REQ-0022`

**Design references:** `ADR-0013`

**Level:** Component test

**Priority:** Required release gate

**Implementation:** `tests/Werm.Tests/Program.cs`

**Execution contract:** [Automated controlled-test execution contract](automated-test-execution-contract.md)

**Technique:** Requirements-based transaction test

## Objective

Verify that the application database-creation service recognizes an empty
database, executes every controlled migration batch in one transaction, and
validates the resulting schema contract.

## Procedure

1. Supply a recording ODBC abstraction whose initial database has no user
   tables.
2. Run `WermDatabaseInstaller` with the controlled version-1 migration.
3. Record migration commands, transaction disposition, and schema checks.

## Expected Result

Every migration batch is associated with one transaction, the transaction is
committed once without rollback, schema version 1 is observed, and the service
reports that it created a missing database.

Real driver file creation and GUI interaction remain part of `TC-0025` and
`TC-0026`.
