# TC-0004: Product Required Values

**Status:** Controlled

**Requirements:** `REQ-0007`, `REQ-0008`

**Level:** Unit test

## Objective

Verify that the domain boundary rejects blank PLUs and descriptions and
normalizes surrounding whitespace before a product can reach persistence.

## Procedure

1. Attempt to construct a product with a whitespace-only PLU.
2. Attempt to construct a product with a whitespace-only description.
3. Construct a product whose PLU and description have surrounding whitespace.

## Expected Result

The first two constructions raise argument errors. The valid product retains
PLU `0042` as text, preserving its leading zero, and description `Ground Beef`.
