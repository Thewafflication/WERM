# ADR-0005: Retain Append-Only Product Audit History

**Content type:** Architecture decision record

**Status:** Accepted

**Date:** 2026-08-22

## Context

The stakeholder requires a history of every product change, including the date
and time of the change. The history must retain the relationship between
successive product revisions and must cover customer-price changes associated
with a product.

## Decision Drivers

- Every committed change must remain discoverable.
- The history must identify when, by whom, and how data changed.
- Related field changes from one save operation must remain grouped.
- Restoring an earlier state must not erase intervening history.
- Product and audit updates must not diverge after a partial failure.

## Considered Options

1. Append-only audit events linked to their parent revision
2. Timestamps on only the current product record
3. Replace the current audit snapshot on every change
4. Store periodic unlinked product snapshots

## Decision

WERM will maintain an append-only product revision chain. Each successful save
will append one `ProductAuditEvent` linked to the preceding event for the PLU.
One or more `ProductAuditChange` rows will record the affected entity, field,
old value, and new value.

The product or customer-price modification and its audit records will commit in
the same SQLite transaction. Existing audit records will not be modified or
deleted through the WERM user interface. Restoring prior values will create a
new audit event.

The parent link supports a linear history for normal operation and can support
a revision tree later if a branching workflow is introduced.

## Rationale

An append-only event and field-change model directly answers who changed what,
when it changed, and which revision preceded it. Grouping changes by event
preserves the boundary of one user action. Transactional writes prevent a data
change from being committed without its audit evidence.

## Consequences

### Positive

- Every application-mediated change has a chronological record.
- A multi-field save is represented as one revision.
- Earlier values can be inspected or restored without rewriting history.
- The parent event makes revision lineage explicit.

### Negative

- Audit storage grows for the lifetime of the product database.
- Reconstructing an old complete product state may require replaying changes.
- SQLite file access must be restricted because direct database access can
  bypass application-level append-only rules.

### Follow-up

- Define audit retention, backup, and integrity-check procedures.
- Decide whether periodic snapshots are required for faster reconstruction.
- Define how Windows identity and application authorization are recorded.
- Evaluate database-level triggers that reject audit updates and deletes.

## References

- [Database design](database-design.md)
- [REQ-0013: Complete change audit](requirements/req-0013-complete-change-audit.md)
- [REQ-0014: Audit event content](requirements/req-0014-audit-event-content.md)
- [REQ-0015: Audit history preservation](requirements/req-0015-audit-history-preservation.md)
