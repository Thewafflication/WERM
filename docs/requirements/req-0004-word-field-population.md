# REQ-0004: Word Field Population

**Content type:** Project requirement

**Status:** Proposed

**Source:** Stakeholder need

## Scope

This requirement applies after WERM retrieves mapped label values and before
it submits a label for printing.

## Requirement

WERM shall insert each retrieved label value into its mapped named field in the
selected Microsoft Word template.

## Rationale

The populated Word document is the rendered label that will be printed.

## Verification

**Method:** Test

**References:** To be assigned

Populate a representative template from a record containing known values.
Verification passes when every mapped field contains its expected value before
the document is submitted for printing.

## Relationships

- **Derived from:** Stakeholder need
- **Depends on:** [REQ-0003](req-0003-label-record-retrieval.md)
- **Depends on:** [ADR-0003](../adr-0003-use-word-for-label-printing.md)
- **Conflicts with:** None known

## Tailoring

None.

## Implementation Record

Not yet implemented. The named-field and mapping mechanisms remain open.
