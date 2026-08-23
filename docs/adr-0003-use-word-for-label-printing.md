# ADR-0003: Use Microsoft Word for Label Generation and Printing

**Content type:** Architecture decision record

**Status:** Accepted

**Date:** 2026-08-22

## Context

WERM version 0.1.0 must insert database values into fields in a Word document
and print the result on a label printer. A PDF intermediate was considered,
but the stakeholder selected a Word-only workflow. Microsoft Word may be a
required workstation prerequisite.

## Decision Drivers

- Label layouts must be authored as Microsoft Word documents.
- Database values must populate named fields in the template.
- The populated label must print through the Windows label-printer driver.
- The initial release should avoid an additional PDF renderer or viewer.

## Considered Options

1. Automate Microsoft Word and print directly from Word
2. Automate Word, export a PDF, and print the PDF
3. Manipulate DOCX files without Word and use another rendering engine
4. Replace Word templates with a dedicated label-description format

## Decision

WERM version 0.1.0 will automate an installed Microsoft Word desktop
application to populate a Word label template and print the populated document
directly to the selected label printer. It will not create or print an
intermediate PDF.

Tagged Word content controls are the selected named-field mechanism. ADR-0012
defines the exact contract, late-bound automation boundary, validation,
printing, and cleanup behavior. The automation preserves the original template
and does not present the Word user interface during normal printing.

## Rationale

Direct Word automation preserves Word as the template authoring and rendering
system. It avoids differences between Word pagination and a separate renderer,
and it avoids adding a PDF printing dependency to version 0.1.0. The approach
also relies on printer behavior already exposed through Word and the installed
Windows printer driver.

## Consequences

### Positive

- Label authors can work with standard Word templates.
- Word controls document layout and printer rendering.
- Version 0.1.0 has no PDF generation or viewing dependency.

### Negative

- A compatible Microsoft Word desktop installation is required on each
  printing workstation.
- Office automation introduces process-lifetime, error-recovery, and COM
  resource-management concerns.
- Printing remains sensitive to template page size, margins, printer driver,
  and physical printer configuration.
- Unattended service execution is outside the selected desktop workflow.

### Follow-up

- Verify page size, margins, Word automation, and cleanup with the approved
  template and intended label printer.
- Record the approved Word, printer-driver, printer, and stock versions in the
  physical-print evidence.

## References

- [REQ-0004: Word field population](requirements/req-0004-word-field-population.md)
- [REQ-0005: Direct Word printing](requirements/req-0005-direct-word-printing.md)
- [REQ-0006: Microsoft Word prerequisite](requirements/req-0006-word-prerequisite.md)
- [ADR-0012: Use tagged Word content controls](adr-0012-use-tagged-word-content-controls.md)
