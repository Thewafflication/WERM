# REQ-0015: Audit History Preservation

**Content type:** Project requirement

**Status:** Accepted

**Source:** Stakeholder decision and ADR-0005

## Scope

This requirement applies to existing product audit events and their field
changes.

## Requirement

WERM shall not provide a GUI operation that modifies or deletes an existing
audit record.

## Rationale

Editing an earlier audit event would make the recorded history unreliable.
Restoring product values is represented by a new event instead.

## Verification

**Method:** Test and inspection

**References:** To be assigned

Inspect the GUI and application data-access operations, then restore an earlier
product state. Verification passes when no audit update or delete operation is
available and the restoration appends a new event.

## Relationships

- **Derived from:** Stakeholder decision
- **Depends on:** [ADR-0005](../adr-0005-append-only-product-audit-history.md)
- **Conflicts with:** Destructive audit-history editing

## Tailoring

None.

## Implementation Record

Not yet implemented. Database-level protection against direct audit updates or
deletes remains an open design item.
