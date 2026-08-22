# REQ-0005: Direct Word Printing

**Content type:** Project requirement

**Status:** Accepted

**Source:** Stakeholder decision and ADR-0003

## Scope

This requirement applies to normal label printing in WERM version 0.1.0.

## Requirement

WERM shall submit the populated Word document directly to the selected Windows
label printer without creating a PDF intermediate.

## Rationale

The stakeholder selected a Word-only workflow for version 0.1.0.

## Verification

**Method:** Test

**References:** To be assigned

Print a populated representative template to the test label printer and
inspect the generated working files. Verification passes when the printer
receives the job and no PDF intermediate is created.

## Relationships

- **Derived from:** Stakeholder decision
- **Depends on:** [REQ-0004](req-0004-word-field-population.md)
- **Depends on:** [ADR-0003](../adr-0003-use-word-for-label-printing.md)
- **Conflicts with:** A mandatory PDF-based print workflow

## Tailoring

None.

## Implementation Record

Not yet implemented.
