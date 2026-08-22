# WERM Database Design

**Content type:** Project design

**Status:** Proposed

## Purpose and Scope

This document defines the initial SQLite schema for WERM version 0.1.0. It
covers product master data, customers, customer-specific prices, and product
change history. Barcode storage and behavior are expressly outside the version
0.1.0 milestone. Word-template tables will be added only if the selected
template and field-mapping design requires them.

The SQL is a design baseline. WERM will execute it through `System.Data.Odbc`
and a SQLite ODBC driver. Migration packaging, the exact driver, driver
bitness, and the DSN model remain open.

## Relationships

```text
Customer 1 ---- * CustomerProductPrice * ---- 1 Product
                                                |
                                                +---- * ProductAuditEvent
                                                          |
                                                          +---- * ProductAuditChange
```

`ProductAuditEvent.ParentAuditEventId` points to the preceding event for the
same PLU. Normal updates form a linked revision chain. The relationship permits
a tree if a later workflow intentionally creates revisions from older events.

## Product

`Product` contains facts intrinsic to a product. PLU is stored as text to
preserve leading zeroes.

```sql
CREATE TABLE Product
(
    PLU                  TEXT NOT NULL PRIMARY KEY,
    Description          TEXT NOT NULL,
    IngredientsStatement TEXT,
    SafeHandlingRequired INTEGER NOT NULL DEFAULT 0
        CHECK (SafeHandlingRequired IN (0, 1)),
    IsActive              INTEGER NOT NULL DEFAULT 1
        CHECK (IsActive IN (0, 1)),
    CreatedUtc            TEXT NOT NULL,
    ModifiedUtc           TEXT NOT NULL
);
```

## Customer

`CustomerCode` is the stable business identifier presented to users and
imports. `CustomerId` is an internal relationship key.

```sql
CREATE TABLE Customer
(
    CustomerId   INTEGER PRIMARY KEY AUTOINCREMENT,
    CustomerCode TEXT NOT NULL UNIQUE,
    CustomerName TEXT NOT NULL,
    IsActive     INTEGER NOT NULL DEFAULT 1
        CHECK (IsActive IN (0, 1)),
    CreatedUtc   TEXT NOT NULL,
    ModifiedUtc  TEXT NOT NULL
);
```

## Customer Product Price

`CustomerProductPrice` records what a customer wants marked for a product.
`PriceType` distinguishes multiple marked prices for the same customer and
PLU. `AmountMinorUnits` stores cents when `CurrencyCode` is `USD`; it does not
use SQLite floating-point storage.

```sql
CREATE TABLE CustomerProductPrice
(
    CustomerId       INTEGER NOT NULL,
    ProductPLU       TEXT NOT NULL,
    PriceType        TEXT NOT NULL,
    AmountMinorUnits INTEGER NOT NULL
        CHECK (AmountMinorUnits >= 0),
    CurrencyCode     TEXT NOT NULL DEFAULT 'USD',
    PriceBasis       TEXT,
    IsActive         INTEGER NOT NULL DEFAULT 1
        CHECK (IsActive IN (0, 1)),
    CreatedUtc       TEXT NOT NULL,
    ModifiedUtc      TEXT NOT NULL,

    PRIMARY KEY (CustomerId, ProductPLU, PriceType),

    FOREIGN KEY (CustomerId)
        REFERENCES Customer(CustomerId),

    FOREIGN KEY (ProductPLU)
        REFERENCES Product(PLU)
);
```

`PriceType`, `CurrencyCode`, and `PriceBasis` require controlled value rules
before this schema is accepted. Effective dates will be added only if the
application must schedule future price changes.

## Product Audit Event

One audit event represents one committed user action. `RevisionNumber` is
sequential within a PLU. `ChangedAtUtc` uses an ISO 8601 UTC timestamp with
date, time, and fractional seconds.

```sql
CREATE TABLE ProductAuditEvent
(
    AuditEventId       INTEGER PRIMARY KEY AUTOINCREMENT,
    ProductPLU         TEXT NOT NULL,
    ParentAuditEventId INTEGER,
    RevisionNumber     INTEGER NOT NULL,
    ChangeType         TEXT NOT NULL
        CHECK (ChangeType IN
            ('Create', 'Update', 'Deactivate', 'Restore')),
    ChangedAtUtc       TEXT NOT NULL,
    ChangedBy          TEXT NOT NULL,
    ChangeReason       TEXT,

    FOREIGN KEY (ProductPLU)
        REFERENCES Product(PLU),

    FOREIGN KEY (ParentAuditEventId)
        REFERENCES ProductAuditEvent(AuditEventId),

    UNIQUE (ProductPLU, RevisionNumber)
);

CREATE INDEX IX_ProductAuditEvent_ProductTime
    ON ProductAuditEvent(ProductPLU, ChangedAtUtc, AuditEventId);
```

The application must verify that the parent event belongs to the same PLU and
is the current event when it appends a normal revision. SQLite does not express
that complete rule through the parent foreign key alone.

## Product Audit Change

An event contains one or more field changes. `EntityType` and `EntityKey`
allow the product history to include changes to the product, its customer
prices, and later its barcode records.

```sql
CREATE TABLE ProductAuditChange
(
    AuditEventId  INTEGER NOT NULL,
    ChangeSequence INTEGER NOT NULL,
    EntityType   TEXT NOT NULL,
    EntityKey    TEXT NOT NULL,
    FieldName    TEXT NOT NULL,
    OldValue     TEXT,
    NewValue     TEXT,

    PRIMARY KEY (AuditEventId, ChangeSequence),

    FOREIGN KEY (AuditEventId)
        REFERENCES ProductAuditEvent(AuditEventId)
);
```

`EntityKey` is a stable serialized identity, not a display label. Its exact
encoding will be specified before the schema is accepted.

## Transactions and Preservation

The application will execute each data change and its audit inserts in one
SQLite transaction. A failed audit insert will therefore prevent the data
change from committing.

Normal GUI workflows will deactivate products, customers, and prices rather
than deleting them. The GUI will not expose operations that update or delete
existing audit records. Direct access to the SQLite file remains outside this
application boundary and must be controlled through operating-system file
permissions, backups, and administrative procedure.

## ODBC Access Contract

Application code will open SQLite connections through `System.Data.Odbc`. SQL
commands will use positional `?` parameters, and code will add parameter values
in placeholder order. The data-access layer will own connection creation,
connection initialization, transaction boundaries, parameter conversion, and
translation of driver errors into application errors.

The M1.2 compatibility spike must establish:

- exact driver identity, version, source, license, and installer;
- supported application architecture and matching driver architecture;
- configured DSN or DSN-less connection-string format;
- Unicode and large-text behavior;
- Boolean, integer-minor-unit, null, and UTC timestamp conversions;
- foreign-key enforcement and per-connection initialization;
- transaction, locking, busy, and rollback behavior;
- migration and schema-metadata behavior; and
- diagnostic information available for connection and SQL failures.

No password or secret will be embedded in a committed connection string.

The versioned migration and Waughtal Shell setup procedure are defined by the
[database installation instructions](database-installation.md).

## Password-Gated Maintenance

The GUI will allow reading, searching, and printing without entering the
maintenance password. A user must successfully enter the maintenance password
before modifying product, customer, or price data.

The credential-storage and password-hashing mechanism remains an open security
design decision. WERM will not store a plaintext password.

## Open Design Items

- permitted price types, bases, currencies, and display rules;
- effective dating and scheduled prices;
- customer-specific label-template configuration;
- credential storage, password changes, and edit-session timeout;
- audit snapshot, integrity-check, retention, and backup policy; and
- migration and schema-version management.
- ODBC driver, architecture, installer, and DSN model.

Barcode symbologies, stored values, and generated barcode rules are deferred
until a milestone after version 0.1.0.

## References

- [ADR-0002: Use SQLite as the label data source](adr-0002-use-sqlite-data-source.md)
- [ADR-0004: Separate product facts from customer prices](adr-0004-separate-customer-product-prices.md)
- [ADR-0005: Retain append-only product audit history](adr-0005-append-only-product-audit-history.md)
- [ADR-0006: Access SQLite through ODBC](adr-0006-access-sqlite-through-odbc.md)
- [Database installation](database-installation.md)
- [Product requirements](requirements/README.md)
