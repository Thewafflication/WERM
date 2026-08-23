# TC-0012: ODBC Connection Contract

**Status:** Controlled

**Requirements:** `REQ-0002`, `REQ-0019`

**Level:** Component test

**Priority:** Required release gate

**Implementation:** `tests/Werm.Tests/Program.cs`

**Execution contract:** [Automated controlled-test execution contract](automated-test-execution-contract.md)

**Technique:** Decision-table connection configuration

## Objective

Verify DSN-less driver and configured-DSN connection strings without opening a
vendor-specific ODBC driver.

## Expected Result

The driver form preserves the registered driver and absolute database path;
the DSN form preserves `WERM`; neither form contains `Password` or `Pwd`.
