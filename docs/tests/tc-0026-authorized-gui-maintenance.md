# TC-0026: Authorized GUI Maintenance and History

**Status:** Controlled

**Requirements:** `REQ-0012`, `REQ-0015`, `REQ-0016`, `REQ-0017`, `REQ-0018`

**Level:** Manual GUI system test

**Priority:** Required release gate

**Design references:** ADR-0010, ADR-0011, database design, and the WPF
Maintenance tab

**Technique:** State-transition, use-case, and authorization decision-table
testing

## Purpose

Verify the operator-visible maintenance workflows through a real approved ODBC
database. GUI automation is not selected for 0.1.0 because the exact deployment
and UI automation baseline is not yet controlled; operator actions and database
observations are retained instead.

## Preconditions, environment, and assumptions

- `TC-0025` has passed for the tested architecture and disposable database.
- WERM is configured to that database and starts without a configuration error.
- The database contains no maintenance credential or controlled business rows.
- Screen capture and ODBC query output can be retained without sensitive data.

## Inputs and initial state

Use a controlled operator name, a policy-compliant test password, one product,
one customer, and one marked price. Use distinct second values for update and
deactivation steps and explicit change reasons. Do not use production secrets
or customer data.

## Procedure

1. Confirm product, customer, price, and history reads remain available while
   locked, the three Save buttons are disabled, and the Print tab remains
   available.
2. Initialize the password with matching confirmation, attempt an incorrect
   unlock, then unlock with the correct password and operator name.
3. Create, reload, modify, and deactivate the product, customer, and price using
   the GUI. Record status messages and screenshots at each state.
4. Load product audit history and compare revision, parent link, timestamp,
   operator, reason, entity key, field, old value, and new value with direct
   read-only ODBC queries.
5. Lock maintenance and confirm another save cannot reach the database. If
   practical, repeat after the ten-minute session expiry.
6. Confirm the GUI offers deactivation but no physical delete operation.

## Expected results and pass criteria

Only the authenticated session enables writes. The exact current rows and
append-only audit lineage match every committed product and price change;
customer changes are password-gated. Incorrect, locked, and expired attempts do
not change the database. History is readable, existing audit rows cannot be
edited or deleted, and no physical business-row deletion is offered.

## Postconditions and cleanup

Lock maintenance, close WERM, retain the disposable database and captured
evidence, and remove the test credential only by deleting the disposable
database. Production state remains unchanged.
