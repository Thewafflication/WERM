# TC-0006: Customer Product Price

**Status:** Controlled

**Requirement:** `REQ-0011`

**Level:** Unit test

**Priority:** Required release gate

**Implementation:** `tests/Werm.Tests/Program.cs`

**Execution contract:** [Automated controlled-test execution contract](automated-test-execution-contract.md)

**Technique:** Boundary-value and equivalence-partition testing

## Objective

Verify the customer, product PLU, and price-type association and the initial
minor-unit and three-character currency rules.

## Procedure

1. Construct a customer and a customer-product price with `1299` minor units.
2. Inspect its customer ID, product PLU, price type, currency, and basis.
3. Attempt prices with a negative amount and a two-character currency.

## Expected Result

The valid association is preserved, `usd` normalizes to `USD`, and both invalid
prices raise argument errors before persistence.
