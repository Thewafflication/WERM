CREATE TABLE WermSchemaVersion
(
    Version      INTEGER NOT NULL PRIMARY KEY,
    Migration    TEXT NOT NULL UNIQUE,
    AppliedAtUtc TEXT NOT NULL
);

-- WERM-BATCH

CREATE TABLE MaintenanceCredential
(
    CredentialId   INTEGER NOT NULL PRIMARY KEY
        CHECK (CredentialId = 1),
    Algorithm      TEXT NOT NULL
        CHECK (length(trim(Algorithm)) > 0),
    IterationCount INTEGER NOT NULL
        CHECK (IterationCount > 0),
    SaltBase64     TEXT NOT NULL
        CHECK (length(trim(SaltBase64)) > 0),
    HashBase64     TEXT NOT NULL
        CHECK (length(trim(HashBase64)) > 0),
    CreatedUtc     TEXT NOT NULL,
    ModifiedUtc    TEXT NOT NULL
);

-- WERM-BATCH

CREATE TABLE Product
(
    PLU                  TEXT NOT NULL PRIMARY KEY
        CHECK (length(trim(PLU)) > 0),
    Description          TEXT NOT NULL
        CHECK (length(trim(Description)) > 0),
    IngredientsStatement TEXT,
    SafeHandlingRequired INTEGER NOT NULL DEFAULT 0
        CHECK (SafeHandlingRequired IN (0, 1)),
    IsActive              INTEGER NOT NULL DEFAULT 1
        CHECK (IsActive IN (0, 1)),
    CreatedUtc            TEXT NOT NULL,
    ModifiedUtc           TEXT NOT NULL
);

-- WERM-BATCH

CREATE TABLE Customer
(
    CustomerId   INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    CustomerCode TEXT NOT NULL UNIQUE
        CHECK (length(trim(CustomerCode)) > 0),
    CustomerName TEXT NOT NULL
        CHECK (length(trim(CustomerName)) > 0),
    IsActive     INTEGER NOT NULL DEFAULT 1
        CHECK (IsActive IN (0, 1)),
    CreatedUtc   TEXT NOT NULL,
    ModifiedUtc  TEXT NOT NULL
);

-- WERM-BATCH

CREATE TABLE CustomerProductPrice
(
    CustomerId       INTEGER NOT NULL,
    ProductPLU       TEXT NOT NULL,
    PriceType        TEXT NOT NULL
        CHECK (length(trim(PriceType)) > 0),
    AmountMinorUnits INTEGER NOT NULL
        CHECK (AmountMinorUnits >= 0),
    CurrencyCode     TEXT NOT NULL DEFAULT 'USD'
        CHECK (length(CurrencyCode) = 3),
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

-- WERM-BATCH

CREATE INDEX IX_CustomerProductPrice_Product
    ON CustomerProductPrice(ProductPLU, CustomerId, PriceType);

-- WERM-BATCH

CREATE TABLE ProductAuditEvent
(
    AuditEventId       INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    ProductPLU         TEXT NOT NULL,
    ParentAuditEventId INTEGER,
    RevisionNumber     INTEGER NOT NULL
        CHECK (RevisionNumber > 0),
    ChangeType         TEXT NOT NULL
        CHECK (ChangeType IN
            ('Create', 'Update', 'Deactivate', 'Restore')),
    ChangedAtUtc       TEXT NOT NULL,
    ChangedBy          TEXT NOT NULL
        CHECK (length(trim(ChangedBy)) > 0),
    ChangeReason       TEXT,

    FOREIGN KEY (ProductPLU)
        REFERENCES Product(PLU),

    FOREIGN KEY (ParentAuditEventId)
        REFERENCES ProductAuditEvent(AuditEventId),

    UNIQUE (ProductPLU, RevisionNumber)
);

-- WERM-BATCH

CREATE INDEX IX_ProductAuditEvent_ProductTime
    ON ProductAuditEvent(ProductPLU, ChangedAtUtc, AuditEventId);

-- WERM-BATCH

CREATE TABLE ProductAuditChange
(
    AuditEventId   INTEGER NOT NULL,
    ChangeSequence INTEGER NOT NULL
        CHECK (ChangeSequence > 0),
    EntityType     TEXT NOT NULL
        CHECK (length(trim(EntityType)) > 0),
    EntityKey      TEXT NOT NULL
        CHECK (length(trim(EntityKey)) > 0),
    FieldName      TEXT NOT NULL
        CHECK (length(trim(FieldName)) > 0),
    OldValue       TEXT,
    NewValue       TEXT,

    PRIMARY KEY (AuditEventId, ChangeSequence),

    FOREIGN KEY (AuditEventId)
        REFERENCES ProductAuditEvent(AuditEventId)
);

-- WERM-BATCH

CREATE TRIGGER TR_ProductAuditEvent_RejectUpdate
BEFORE UPDATE ON ProductAuditEvent
BEGIN
    SELECT RAISE(ABORT, 'ProductAuditEvent is append-only');
END;

-- WERM-BATCH

CREATE TRIGGER TR_ProductAuditEvent_RejectDelete
BEFORE DELETE ON ProductAuditEvent
BEGIN
    SELECT RAISE(ABORT, 'ProductAuditEvent is append-only');
END;

-- WERM-BATCH

CREATE TRIGGER TR_ProductAuditChange_RejectUpdate
BEFORE UPDATE ON ProductAuditChange
BEGIN
    SELECT RAISE(ABORT, 'ProductAuditChange is append-only');
END;

-- WERM-BATCH

CREATE TRIGGER TR_ProductAuditChange_RejectDelete
BEFORE DELETE ON ProductAuditChange
BEGIN
    SELECT RAISE(ABORT, 'ProductAuditChange is append-only');
END;

-- WERM-BATCH

INSERT INTO WermSchemaVersion (Version, Migration, AppliedAtUtc)
VALUES (1, '0001-initial-schema.sql', CURRENT_TIMESTAMP);

-- WERM-BATCH

PRAGMA user_version = 1;
