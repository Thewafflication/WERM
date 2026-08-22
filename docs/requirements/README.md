# WERM Product Requirements

**Content type:** Project requirements index

This directory contains the controlled product requirements for WERM. Each
requirement has a stable identifier and records its source, rationale, planned
verification method, and architectural relationships.

## Version 0.1.0 Requirements

| Identifier | Title | Status |
| --- | --- | --- |
| [REQ-0001](req-0001-dotnet-framework-target.md) | .NET Framework target | Accepted |
| [REQ-0002](req-0002-sqlite-data-source.md) | SQLite data source | Accepted |
| [REQ-0003](req-0003-label-record-retrieval.md) | Label record retrieval | Proposed |
| [REQ-0004](req-0004-word-field-population.md) | Word field population | Proposed |
| [REQ-0005](req-0005-direct-word-printing.md) | Direct Word printing | Accepted |
| [REQ-0006](req-0006-word-prerequisite.md) | Microsoft Word prerequisite | Accepted |
| [REQ-0007](req-0007-unique-product-plu.md) | Unique product PLU | Accepted |
| [REQ-0008](req-0008-product-description.md) | Product description | Accepted |
| [REQ-0009](req-0009-ingredients-statement.md) | Ingredients statement | Accepted |
| [REQ-0010](req-0010-safe-handling-status.md) | Safe-handling status | Accepted |
| [REQ-0011](req-0011-customer-specific-prices.md) | Customer-specific prices | Accepted |
| [REQ-0012](req-0012-password-gated-maintenance.md) | Password-gated maintenance | Accepted |
| [REQ-0013](req-0013-complete-change-audit.md) | Complete change audit | Accepted |
| [REQ-0014](req-0014-audit-event-content.md) | Audit event content | Accepted |
| [REQ-0015](req-0015-audit-history-preservation.md) | Audit history preservation | Accepted |
| [REQ-0016](req-0016-product-gui-maintenance.md) | Product GUI maintenance | Accepted |
| [REQ-0017](req-0017-customer-gui-maintenance.md) | Customer GUI maintenance | Accepted |
| [REQ-0018](req-0018-price-gui-maintenance.md) | Price GUI maintenance | Accepted |
| [REQ-0019](req-0019-sqlite-odbc-access.md) | SQLite ODBC access | Accepted |
| [REQ-0020](req-0020-repeatable-database-installation.md) | Repeatable database installation | Accepted |
| [REQ-0021](req-0021-waughtal-shell-database-installer.md) | Waughtal Shell database installer | Accepted |

The proposed requirements capture the current product intent but need detailed
database, mapping, and error-behavior decisions before acceptance.

## Deferred Beyond Version 0.1.0

Barcode storage, generation, field population, and printing are not included
in the first milestone. Barcode requirements will receive new stable
identifiers when their milestone is planned; they are not implied by the
current product requirements.
