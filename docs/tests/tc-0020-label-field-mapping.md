# TC-0020: Label Field Mapping

**Status:** Controlled

**Requirements:** `REQ-0003`, `REQ-0004`

**Level:** Component test

**Priority:** Required release gate

**Implementation:** `tests/Werm.Tests/Program.cs`

**Execution contract:** [Automated controlled-test execution contract](automated-test-execution-contract.md)

**Technique:** Requirements-based mapping example

## Objective

Map a known active product, customer, and customer-product price to the Word
label contract.

## Expected Result

Exactly nine tagged values are produced. PLU, description, ingredients,
safe-handling text, customer values, price type, price basis, and formatted
amount equal the controlled expected values.
