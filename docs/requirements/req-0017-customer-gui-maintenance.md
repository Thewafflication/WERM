# REQ-0017: Customer GUI Maintenance

**Content type:** Project requirement

**Status:** Accepted

**Source:** Stakeholder need

## Scope

This requirement applies to customer creation, modification, and deactivation
through the WERM GUI. Physical customer deletion is excluded.

## Requirement

WERM shall provide GUI maintenance for customer records after successful
maintenance-password verification.

## Rationale

Customer records are required to associate products with the prices customers
want marked.

## Verification

**Method:** Test

**References:** `TC-0016`, `TC-0019`; GUI workflow test pending

Create, modify, and deactivate representative customers through the GUI.
Verification passes when each committed operation produces the expected
database state and unauthorized operation remains unavailable.

## Relationships

- **Derived from:** Stakeholder need
- **Depends on:** [REQ-0012](req-0012-password-gated-maintenance.md)
- **Depends on:** [Database design](../database-design.md)
- **Conflicts with:** Anonymous customer maintenance

## Tailoring

None.

## Implementation Record

The authorized application service and parameterized ODBC customer save are
implemented. The WPF maintenance workflow remains pending.
