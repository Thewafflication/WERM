# WERM Database Assets

**Content type:** Implementation support

This directory contains the controlled SQLite schema migrations used to create
and upgrade a WERM database through ODBC.

## Migrations

- [0001 — Initial schema](migrations/0001-initial-schema.sql) creates the
  version table, product, customer, customer-price, and product-audit tables.

Migration files are immutable after they have been used by a released WERM
version. A later schema change receives the next sequential migration instead
of modifying an applied migration.

The ODBC worker treats a line containing `-- WERM-BATCH` as a command boundary.
This avoids relying on an ODBC driver to execute multiple SQL statements in one
call.

## Installation

See the [database installation instructions](../docs/database-installation.md).
