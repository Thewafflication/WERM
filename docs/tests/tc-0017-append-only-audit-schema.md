# TC-0017: Append-Only Audit Schema

**Status:** Controlled

**Requirement:** `REQ-0015`

**Level:** Schema inspection

**Priority:** Required release gate

**Implementation:** `tests/Werm.Tests/Program.cs`

**Execution contract:** [Automated controlled-test execution contract](automated-test-execution-contract.md)

**Technique:** Requirements-based schema inspection

## Objective

Verify that both audit tables have `BEFORE UPDATE` and `BEFORE DELETE` triggers
whose bodies abort the attempted mutation.

## Expected Result

All four controlled trigger definitions contain `RAISE(ABORT, ...)`. Local
SQLite execution additionally confirms all four mutation attempts fail.
