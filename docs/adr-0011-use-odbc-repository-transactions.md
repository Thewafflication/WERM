# ADR-0011: Use ODBC Repositories with Atomic Audit Transactions

**Status:** Accepted

**Date:** 2026-08-22

## Context

WERM must read and maintain SQLite data through ODBC. Every product and
customer-product price modification must append complete audit lineage without
allowing current data and history to diverge after a partial failure.

## Decision

`Werm.Data` will own ODBC connection creation, SQLite per-connection setup,
SQL, parameter conversion, persistence mapping, and transactions. Production
connections use `System.Data.Odbc.OdbcConnection`; repositories depend on the
standard ADO.NET interfaces so their command and transaction behavior can be
tested without selecting a vendor-specific SQLite provider.

Every connection enables `PRAGMA foreign_keys = ON` and a 5000-millisecond busy
timeout. SQL uses positional `?` placeholders, and parameters are added in
placeholder order. Driver and DSN connection strings include only the database
location and selected ODBC identity; WERM does not embed a username or
password.

A product or customer-price save performs these operations in one transaction:

1. load current state;
2. compute field-level differences;
3. insert or update the current row;
4. read the latest audit event for the PLU;
5. append the next event and its parent link;
6. append the ordered field changes; and
7. commit.

Any failure rolls back the transaction. A save with no field differences does
not append an event. Existing audit events and changes have no repository
update or delete methods, and SQLite triggers reject direct update/delete SQL
against both audit tables.

Customer-price audit entity keys use
`<customer-id>:<base64-utf8-plu>:<base64-utf8-price-type>`, making the composite
identity unambiguous without depending on display text.

## Consequences

- Product and price writes have a single atomic evidence boundary.
- Concurrent attempts to claim the same next revision are resolved by the
  unique `(ProductPLU, RevisionNumber)` constraint; a loser rolls back and may
  be retried by the application.
- Customer changes are password-gated but are not product-audit events because
  `REQ-0013` scopes the append-only history to product and customer-price data.
- CI uses a deterministic ADO.NET test double to verify command ordering,
  positional parameters, commit, and rollback behavior.
- Successful migration and round-trip testing through the approved SQLite ODBC
  driver remains required before release.

## References

- [ADR-0005: Retain append-only product audit history](adr-0005-append-only-product-audit-history.md)
- [ADR-0006: Access SQLite through ODBC](adr-0006-access-sqlite-through-odbc.md)
- [Database design](database-design.md)
- [Controlled tests](tests/README.md)
