# REQ-0014: Audit Event Content

**Content type:** Project requirement

**Status:** Accepted

**Source:** Stakeholder decision and ADR-0005

## Scope

This requirement applies to every product audit event.

## Requirement

Each product audit event shall record the product PLU, preceding event, revision
number, change type, UTC date and time, responsible user, affected fields, old
values, and new values.

## Rationale

These attributes establish the lineage and content of every change and allow a
reviewer to determine who changed what and when.

## Verification

**Method:** Test and inspection

**References:** `TC-0013`, `TC-0015`

Inspect the schema and representative events for every supported change type.
Verification passes when the required attributes are present and match the
performed operation.

## Relationships

- **Derived from:** Stakeholder decision
- **Depends on:** [ADR-0005](../adr-0005-append-only-product-audit-history.md)
- **Conflicts with:** None known

## Tailoring

None.

## Implementation Record

The implemented data store records parent lineage, sequential revision,
change type, UTC timestamp, operator, reason, and ordered old/new field values.
