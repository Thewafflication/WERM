# WERM Engineering Documentation

**Content type:** Project documentation index

This directory contains the controlled engineering records for Waughtal
Enterprise Resource Management (WERM).

## Product Scope

WERM version 0.1.0 is a Windows label-printing application. It reads label
data from a SQLite database, inserts that data into fields in a Microsoft Word
template, and prints the populated document directly to a label printer.
Version 0.1.0 does not store, generate, populate, or print barcodes.

## Architecture Decisions

- [ADR-0001: Target .NET Framework 4.8](adr-0001-target-dotnet-framework-4-8.md)
- [ADR-0002: Use SQLite as the label data source](adr-0002-use-sqlite-data-source.md)
- [ADR-0003: Use Microsoft Word for label generation and printing](adr-0003-use-word-for-label-printing.md)
- [ADR-0004: Separate product facts from customer prices](adr-0004-separate-customer-product-prices.md)
- [ADR-0005: Retain append-only product audit history](adr-0005-append-only-product-audit-history.md)
- [ADR-0006: Access SQLite through ODBC](adr-0006-access-sqlite-through-odbc.md)
- [ADR-0007: Use Waughtal Shell for database installation](adr-0007-use-waughtal-shell-database-installer.md)
- [ADR-0008: Use WPF with x86 and x64 packages](adr-0008-use-wpf-and-x86-x64-packages.md)
- [ADR-0009: Use WPM and GitHub Actions for delivery](adr-0009-use-wpm-and-github-actions.md)

## Design

- [Database design](database-design.md)
- [Database installation](database-installation.md)

## Requirements

- [Product requirements index](requirements/README.md)

## Process

- [WSP adoption record](wsp-adoption.md)
- [Project process](project-process.md)
- [Milestone 0.1.0 plan](milestones/milestone-0.1.0.md)
- [Controlled test specifications](tests/README.md)

## Open Decisions

The following choices remain open and require separate review before their
implementation becomes authoritative:

- WPF or Windows Forms for the desktop user interface;
- SQLite ODBC driver, version, deployment, bitness, and DSN model;
- Word content controls, bookmarks, or another named-field mechanism;
- field-mapping configuration and template discovery;
- customer and price import workflows;
- price effective dates and scheduled price changes;
- password hashing and credential-administration mechanism;
- printer selection, printer defaults, and print-error behavior; and
- print history and operational logging;
- record-selection workflow.

Barcode symbologies and customer-specific barcode rules are deferred until a
milestone after version 0.1.0.
