# REQ-0009: Ingredients Statement

**Content type:** Project requirement

**Status:** Accepted

**Source:** Stakeholder need

## Scope

This requirement applies to products for which an ingredients statement is
available.

## Requirement

WERM shall store the ingredients statement associated with a product.

## Rationale

The ingredients statement may be required on the product label.

## Verification

**Method:** Test

**References:** To be assigned

Save and reload a product containing a representative ingredients statement.
Verification passes when the retrieved text exactly matches the saved text.

## Relationships

- **Derived from:** Stakeholder need
- **Depends on:** [REQ-0007](req-0007-unique-product-plu.md)
- **Conflicts with:** None known

## Tailoring

None.

## Implementation Record

The proposed schema defines `Product.IngredientsStatement` as optional text.
