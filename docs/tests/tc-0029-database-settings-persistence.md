# TC-0029: Database Settings Persistence

**Status:** Controlled

**Requirements:** `REQ-0022`

**Design references:** `ADR-0013`

**Level:** Component test

**Priority:** Required release gate

**Implementation:** `tests/Werm.Tests/Program.cs`

**Execution contract:** [Automated controlled-test execution contract](automated-test-execution-contract.md)

**Technique:** Requirements-based persistence test

## Objective

Verify that WERM saves and reloads the database path, ODBC selection, and Word
template using the versioned per-user settings contract without persisting a
credential-like field.

## Procedure

1. Save controlled settings to a disposable XML file through
   `WermSettingsStore`.
2. Reload the file over an empty settings object.
3. Compare every supported setting and inspect the XML field names.
4. Remove the disposable file.

## Expected Result

Every non-secret setting round-trips unchanged, the schema version is one, and
no password, credential, secret, or hash field is present.
