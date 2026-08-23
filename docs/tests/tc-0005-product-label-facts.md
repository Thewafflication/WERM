# TC-0005: Product Label Facts

**Status:** Controlled

**Requirements:** `REQ-0009`, `REQ-0010`

**Level:** Unit test

**Priority:** Required release gate

**Implementation:** `tests/Werm.Tests/Program.cs`

**Execution contract:** [Automated controlled-test execution contract](automated-test-execution-contract.md)

**Technique:** Decision-table state coverage

## Objective

Verify that the domain model preserves representative multi-line ingredients
text and both safe-handling states.

## Procedure

Construct one product with a representative ingredients statement and
safe-handling enabled, and a second product without ingredients and with
safe-handling disabled.

## Expected Result

The first statement is unchanged, both Boolean states match their inputs, and
the absent ingredients statement remains null.
