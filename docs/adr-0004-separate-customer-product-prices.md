# ADR-0004: Separate Product Facts from Customer Prices

**Content type:** Architecture decision record

**Status:** Accepted

**Date:** 2026-08-22

## Context

WERM must store product facts and the prices that customers want marked on
labels. One product may be sold to multiple customers, and each customer may
require a different price for the same product. Products may also have more
than one kind of marked price.

## Decision Drivers

- PLU is the unique product key selected by the stakeholder.
- Product facts must not be duplicated for every customer.
- Customer-specific prices must support a many-to-many product relationship.
- Monetary values must not be exposed to binary floating-point rounding.
- The schema must remain extensible for customer-specific label behavior.

## Considered Options

1. Separate `Product`, `Customer`, and `CustomerProductPrice` tables
2. Add a separate price column to `Product` for every customer
3. Store customer-price collections as unstructured product data

## Decision

WERM will store intrinsic product facts in `Product`, customer identity in
`Customer`, and marked prices in `CustomerProductPrice`.

`Product.PLU` will be a text primary key so leading zeroes are preserved. A
customer-product price will be identified by customer, PLU, and price type.
The price amount will be stored as integer minor currency units rather than a
SQLite `REAL` value.

Products and customers will be deactivated instead of physically deleted
during normal application operation.

## Rationale

The relational model represents the many-to-many relationship without
duplicating product facts or adding customer-specific columns. A price type
allows one customer to request several marked prices for the same PLU. Integer
minor units provide exact storage for ordinary currency amounts.

## Consequences

### Positive

- Product facts have one authoritative record per PLU.
- Customer-specific prices can evolve independently of product facts.
- New customers do not require schema changes.
- Leading zeroes in a PLU are retained.
- Prices avoid binary floating-point rounding behavior.

### Negative

- Label queries must join product, customer, and price records.
- Price basis, currency, and display formatting require explicit rules.
- Changing a PLU requires coordinated foreign-key updates and audit history.

### Follow-up

- Define permitted price types and price bases.
- Decide whether scheduled prices require effective date ranges.
- Define customer-specific template settings for version 0.1.0.
- Defer customer-specific barcode settings until a later milestone.
- Define the customer and price maintenance workflows.

## References

- [Database design](database-design.md)
- [REQ-0007: Unique product PLU](requirements/req-0007-unique-product-plu.md)
- [REQ-0011: Customer-specific prices](requirements/req-0011-customer-specific-prices.md)
