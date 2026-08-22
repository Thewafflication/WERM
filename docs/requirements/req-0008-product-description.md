# REQ-0008: Product Description

**Content type:** Project requirement

**Status:** Accepted

**Source:** Stakeholder need

## Scope

This requirement applies to every product stored in the WERM product database.

## Requirement

WERM shall store a description for each product.

## Rationale

The description identifies the product to users and may be printed on a label.

## Verification

**Method:** Test

**References:** To be assigned

Attempt to save a product without a description. Verification passes when the
application rejects the product and the database retains no incomplete row.

## Relationships

- **Derived from:** Stakeholder need
- **Depends on:** [REQ-0007](req-0007-unique-product-plu.md)
- **Conflicts with:** None known

## Tailoring

None.

## Implementation Record

The proposed schema defines `Product.Description` as required text.
