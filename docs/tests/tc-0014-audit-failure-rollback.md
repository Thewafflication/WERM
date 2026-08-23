# TC-0014: Audit Failure Rollback

**Status:** Controlled

**Requirements:** `REQ-0013`, `REQ-0019`

**Level:** Data-component failure test

**Priority:** Required release gate

**Implementation:** `tests/Werm.Tests/Program.cs`

**Execution contract:** [Automated controlled-test execution contract](automated-test-execution-contract.md)

**Technique:** Controlled fault injection

## Objective

Inject a failure into the first audit-change insert after the product and event
commands have succeeded.

## Expected Result

The data store propagates the failure, commits zero times, and rolls the shared
transaction back exactly once.
