# ADR-0012: Use Tagged Word Content Controls and Late-Bound Automation

**Status:** Accepted

**Date:** 2026-08-22

## Context

ADR-0003 selects direct Microsoft Word printing but leaves the named-field,
automation, validation, and cleanup mechanisms open. The build and controlled
tests must also run on GitHub-hosted runners where Microsoft Word is not
installed.

## Decision

WERM 0.1.0 templates use Word content controls identified by their exact `Tag`
property. The nine required tags and value formats are the controlled contract
in [Word template contract](word-template-contract.md). WERM validates that all
required tags exist before changing any content or submitting a print job.

`Werm.Core` owns record retrieval, field mapping, template validation, and the
print workflow behind document interfaces. `Werm.Printing` uses late-bound COM
automation of `Word.Application`, so compiling and testing do not require an
Office interop assembly or Word installation. Production printing requires
Word to be registered on the workstation.

Word is hidden and alerts are disabled. WERM creates a new document from the
selected template, writes tagged controls, temporarily selects the requested
Word printer, calls synchronous `PrintOut`, restores the prior printer, closes
the working document without saving, quits Word, and releases COM references.
The original template is never saved by the print workflow.

## Consequences

- Template authors have stable, visible identifiers independent of displayed
  placeholder text.
- Missing tags fail before partial population or printing.
- Mapping, validation, direct-print arguments, and cleanup are testable without
  automating Word in CI.
- A physical verification remains necessary for Word version, printer driver,
  page layout, label stock, and actual print output.
- Word and the SQLite ODBC driver must match the installed WERM package
  architecture.
- PDF and barcode behavior remain outside version 0.1.0.

## References

- [ADR-0003: Use Microsoft Word for label generation and printing](adr-0003-use-word-for-label-printing.md)
- [Word template contract](word-template-contract.md)
- [Workstation configuration](workstation-configuration.md)
- [Controlled tests](tests/README.md)
