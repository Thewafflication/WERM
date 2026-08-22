# REQ-0016: Product GUI Maintenance

**Content type:** Project requirement

**Status:** Accepted

**Source:** Stakeholder need

## Scope

This requirement applies to product creation, modification, and deactivation
through the WERM GUI. Physical product deletion is excluded.

## Requirement

WERM shall provide GUI maintenance for product records after successful
maintenance-password verification.

## Rationale

An authorized user needs to maintain the product database without directly
editing the SQLite file.

## Verification

**Method:** Test

**References:** To be assigned

Create, modify, and deactivate representative products through the GUI.
Verification passes when each committed operation produces the expected
database state and an audit event, and when no physical deletion is offered.

## Relationships

- **Derived from:** Stakeholder need
- **Depends on:** [REQ-0012](req-0012-password-gated-maintenance.md)
- **Depends on:** [REQ-0013](req-0013-complete-change-audit.md)
- **Depends on:** [Database design](../database-design.md)
- **Conflicts with:** Anonymous product maintenance

## Tailoring

None.

## Implementation Record

Not yet implemented.
