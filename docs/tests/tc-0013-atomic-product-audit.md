# TC-0013: Atomic Product Audit

**Status:** Controlled

**Requirements:** `REQ-0013`, `REQ-0014`

**Level:** Data-component test

## Objective

Verify that a new product, one audit event, and five field changes use the same
transaction and positional parameter contract.

## Expected Result

Exactly one product and event insert and five ordered change inserts occur, all
write commands reference the same transaction, every `?` has one ordered
parameter, and the transaction commits once without rollback.
