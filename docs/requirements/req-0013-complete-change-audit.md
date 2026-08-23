# REQ-0013: Complete Change Audit

**Content type:** Project requirement

**Status:** Accepted

**Source:** Stakeholder decision and ADR-0005

## Scope

This requirement applies to every committed modification of product data and
customer-product price data made through WERM.

## Requirement

WERM shall append an audit event for every committed modification within this
scope.

## Rationale

The stakeholder requires a permanent history of every product-related change.

## Verification

**Method:** Test

**References:** `TC-0013`, `TC-0014`, `TC-0015`

Perform each supported modification type and compare committed data operations
with the resulting audit events. Verification passes when every committed
modification has exactly one associated event and failed operations have none.

## Relationships

- **Derived from:** Stakeholder decision
- **Depends on:** [ADR-0005](../adr-0005-append-only-product-audit-history.md)
- **Depends on:** [REQ-0014](req-0014-audit-event-content.md)
- **Conflicts with:** None known

## Tailoring

None.

## Implementation Record

The ODBC data store writes `ProductAuditEvent` and `ProductAuditChange` in the
same ADO.NET transaction as each product or customer-price modification. An
injected audit failure rolls the shared transaction back.
