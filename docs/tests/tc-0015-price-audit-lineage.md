# TC-0015: Customer-Price Audit Lineage

**Status:** Controlled

**Requirements:** `REQ-0011`, `REQ-0013`, `REQ-0014`, `REQ-0019`

**Level:** Data-component test

**Priority:** Required release gate

**Implementation:** `tests/Werm.Tests/Program.cs`

**Execution contract:** [Automated controlled-test execution contract](automated-test-execution-contract.md)

**Technique:** State-transition and transaction scenario testing

## Objective

Change a customer price from 1099 active minor units to 1299 inactive minor
units when the product's latest audit event is revision three, event ID ten.

## Expected Result

The price update records two field changes, appends a `Deactivate` event with
parent ID ten and revision four, and commits without rollback.
