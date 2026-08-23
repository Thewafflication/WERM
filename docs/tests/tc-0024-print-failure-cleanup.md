# TC-0024: Print Failure Cleanup

**Status:** Controlled

**Requirements:** `REQ-0005`

**Level:** Application-component test

**Priority:** Required release gate

**Implementation:** `tests/Werm.Tests/Program.cs`

**Execution contract:** [Automated controlled-test execution contract](automated-test-execution-contract.md)

**Technique:** Controlled fault injection

## Objective

Simulate a document print failure after successful field population.

## Expected Result

The failure propagates to the caller and the working document is disposed.
The production document adapter maps disposal to close-without-save, Word
quit, and COM-reference release.
