# REQ-0012: Password-Gated Maintenance

**Content type:** Project requirement

**Status:** Accepted

**Source:** Stakeholder need

## Scope

This requirement applies to GUI operations that create or modify product,
customer, or customer-price data. It does not require a password for reading,
searching, or printing.

## Requirement

WERM shall require successful password verification before enabling product,
customer, or customer-price modifications through the GUI.

## Rationale

The stakeholder requires an explicit authorization step before database
maintenance.

## Verification

**Method:** Test

**References:** `TC-0009`, `TC-0010`, `TC-0011`

Attempt each protected operation before authentication, with an incorrect
password, and with the correct password. Verification passes when only the
successfully authenticated session can enable the operations.

## Relationships

- **Derived from:** Stakeholder need
- **Depends on:** [ADR-0010](../adr-0010-hash-maintenance-password.md)
- **Conflicts with:** Anonymous GUI maintenance

## Tailoring

None.

## Implementation Record

The core authorization boundary implements salted PBKDF2-HMAC-SHA512 password
verification, first-run initialization, failed-attempt throttling, fixed
10-minute sessions, logout, and session revocation on password change. The
SQLite schema stores only verifier metadata. GUI integration remains pending.
