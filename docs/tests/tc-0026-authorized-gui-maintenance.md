# TC-0026: Authorized GUI Maintenance and History

**Status:** Controlled

**Requirements:** `REQ-0012`, `REQ-0015`, `REQ-0016`, `REQ-0017`, `REQ-0018`,
`REQ-0022`

**Level:** Manual GUI system test

**Priority:** Required release gate

**Design references:** ADR-0010, ADR-0011, ADR-0013, database design, and the
WPF Database configuration and Maintenance tabs

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

1. On the Database configuration tab, enumerate the installed ODBC choices,
   select a new database path, create/validate it, save the settings, and test
   the connection. Restart WERM and confirm the selection reloads.
2. Confirm product, customer, price, and history reads remain available while
   locked, the three Save buttons are disabled, and the Print tab remains
   available.
3. Initialize the password with matching confirmation, attempt an incorrect
   unlock, then unlock with the correct password and operator name.
4. Create, reload, modify, and deactivate the product, customer, and price using
   the GUI. Record status messages and screenshots at each state.
5. Load product audit history and compare revision, parent link, timestamp,
   operator, reason, entity key, field, old value, and new value with direct
   read-only ODBC queries.
6. Lock maintenance and confirm another save cannot reach the database. If
   practical, repeat after the ten-minute session expiry.
7. Confirm the GUI offers deactivation but no physical delete operation.

## Expected results and pass criteria

The selected ODBC configuration persists without a credential and the created
database has the controlled schema. Only the authenticated session enables
writes. The exact current rows and
append-only audit lineage match every committed product and price change;
customer changes are password-gated. Incorrect, locked, and expired attempts do
not change the database. History is readable, existing audit rows cannot be
edited or deleted, and no physical business-row deletion is offered.

## Postconditions and cleanup

Lock maintenance, close WERM, retain the disposable database and captured
evidence, and remove the test credential only by deleting the disposable
database. Production state remains unchanged.
