# TC-0023: Label Record Workflow

**Status:** Controlled

**Requirements:** `REQ-0003`, `REQ-0005`

**Level:** Application-service test

**Priority:** Required release gate

**Implementation:** `tests/Werm.Tests/Program.cs`

**Execution contract:** [Automated controlled-test execution contract](automated-test-execution-contract.md)

**Technique:** End-to-end service scenario

## Objective

Request a label by PLU, customer ID, and price type from a store containing a
known active record.

## Expected Result

The workflow performs the exact three lookups and submits one print job with
the selected template, printer, copy count, and mapped values.
