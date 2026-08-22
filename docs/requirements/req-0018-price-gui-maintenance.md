# REQ-0018: Price GUI Maintenance

**Content type:** Project requirement

**Status:** Accepted

**Source:** Stakeholder need

## Scope

This requirement applies to creation, modification, and deactivation of
customer-product prices through the WERM GUI. Physical price deletion is
excluded.

## Requirement

WERM shall provide GUI maintenance for customer-product price records after
successful maintenance-password verification.

## Rationale

An authorized user needs to control the prices printed for each customer and
product without directly editing the SQLite file.

## Verification

**Method:** Test

**References:** To be assigned

Create, modify, and deactivate representative customer-product prices through
the GUI. Verification passes when each committed operation produces the
expected database state and product audit event, and unauthorized operation
remains unavailable.

## Relationships

- **Derived from:** Stakeholder need
- **Depends on:** [REQ-0011](req-0011-customer-specific-prices.md)
- **Depends on:** [REQ-0012](req-0012-password-gated-maintenance.md)
- **Depends on:** [REQ-0013](req-0013-complete-change-audit.md)
- **Depends on:** [Database design](../database-design.md)
- **Conflicts with:** Anonymous price maintenance

## Tailoring

None.

## Implementation Record

Not yet implemented.
