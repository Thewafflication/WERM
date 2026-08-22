# REQ-0007: Unique Product PLU

**Content type:** Project requirement

**Status:** Accepted

**Source:** Stakeholder decision and ADR-0004

## Scope

This requirement applies to every product stored in the WERM product database.

## Requirement

WERM shall identify each product with a unique PLU.

## Rationale

PLU is the stakeholder-selected business key used to find the product and its
related label data.

## Verification

**Method:** Test and inspection

**References:** To be assigned

Inspect the schema constraint, attempt to insert a product without a PLU, and
attempt to insert two products with the same PLU. Verification passes when the
missing and duplicate PLUs are rejected.

## Relationships

- **Derived from:** Stakeholder decision
- **Depends on:** [ADR-0004](../adr-0004-separate-customer-product-prices.md)
- **Conflicts with:** None known

## Tailoring

None.

## Implementation Record

The proposed schema defines `Product.PLU` as a required text primary key.
