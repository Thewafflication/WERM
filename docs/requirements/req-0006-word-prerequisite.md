# REQ-0006: Microsoft Word Prerequisite

**Content type:** Project requirement

**Status:** Accepted

**Source:** Stakeholder decision and ADR-0003

## Scope

This requirement applies to every workstation used to print labels with WERM
version 0.1.0.

## Requirement

The WERM deployment documentation shall identify an installed Microsoft Word
desktop application as a label-printing prerequisite.

## Rationale

The selected label workflow depends on Microsoft Word for template rendering
and direct printing.

## Verification

**Method:** Inspection

**References:** [Workstation configuration](../workstation-configuration.md)

Inspect the deployment documentation. Verification passes when the Microsoft
Word prerequisite and the supported configuration are stated.

## Relationships

- **Derived from:** Stakeholder decision
- **Depends on:** [ADR-0003](../adr-0003-use-word-for-label-printing.md)
- **Conflicts with:** A deployment requirement that prohibits Microsoft Word

## Tailoring

None.

## Implementation Record

The packaged workstation instructions identify Microsoft Word desktop, .NET
Framework 4.8, a matching-bitness SQLite ODBC driver, and the Windows label
printer driver as prerequisites.
