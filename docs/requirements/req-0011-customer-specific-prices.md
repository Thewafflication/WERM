# REQ-0011: Customer-Specific Prices

**Content type:** Project requirement

**Status:** Accepted

**Source:** Stakeholder decision and ADR-0004

## Scope

This requirement applies to prices that a customer wants marked for a product.

## Requirement

WERM shall associate each marked price with one customer, one product PLU, and
one price type.

## Rationale

Different customers may require different marked prices for the same product,
and one customer may require more than one price type.

## Verification

**Method:** Test

**References:** `TC-0006`, `TC-0007`, `TC-0015`

Store different prices for the same PLU and different customers, then retrieve
each association. Verification passes when every customer receives only its
configured price for the requested price type.

## Relationships

- **Derived from:** Stakeholder decision
- **Depends on:** [REQ-0007](req-0007-unique-product-plu.md)
- **Depends on:** [ADR-0004](../adr-0004-separate-customer-product-prices.md)
- **Conflicts with:** One global price per product

## Tailoring

None.

## Implementation Record

The schema defines `CustomerProductPrice` with a composite primary key of
customer, PLU, and price type. The core model validates those identity values,
non-negative minor units, and the initial three-character currency contract.
The ODBC data store persists price changes and appends their product audit
lineage in one transaction.
