# TC-0008: Installer Safe Connection Failure

**Status:** Controlled

**Requirements:** `REQ-0019`, `REQ-0020`

**Level:** System test

**Priority:** Required release gate

**Implementation:** `tests/Werm.Tests/Program.cs`

**Execution contract:** [Automated controlled-test execution contract](automated-test-execution-contract.md)

**Technique:** Controlled fault injection

## Objective

Verify that the temporary PowerShell ODBC worker reports a controlled failure
and leaves no new database file when the selected ODBC driver cannot be opened.

## Preconditions

- the Debug controlled-test executable is running as the selected x86 or x64
  target; and
- `WERM-CONTROLLED-MISSING-DRIVER` is not a registered ODBC driver.

## Procedure

1. Create a unique test directory under `out/test-work/`.
2. Invoke `tools/Install-WermDatabase.ps1` through the architecture-matched
   Windows PowerShell host with a new database path and the missing driver.
3. Capture its output and exit status.
4. Inspect the target path and remove the test directory.

## Expected Result

The worker reports the selected process architecture, returns exit code `4`,
and the new database file does not exist after failure.
