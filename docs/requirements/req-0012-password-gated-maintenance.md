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

**References:** To be assigned

Attempt each protected operation before authentication, with an incorrect
password, and with the correct password. Verification passes when only the
successfully authenticated session can enable the operations.

## Relationships

- **Derived from:** Stakeholder need
- **Depends on:** Credential design to be recorded
- **Conflicts with:** Anonymous GUI maintenance

## Tailoring

None.

## Implementation Record

Not yet implemented. Credential storage, hashing, password administration, and
edit-session timeout remain open design items.
