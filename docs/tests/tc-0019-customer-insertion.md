# TC-0019: Customer Insertion

**Status:** Controlled

**Requirements:** `REQ-0017`, `REQ-0019`

**Level:** Data-component test

## Objective

Insert a new customer through the ODBC data store and obtain its SQLite row ID.

## Expected Result

One parameterized customer insert occurs, `last_insert_rowid()` returns the new
identity, no customer delete command is issued, and the transaction commits.
