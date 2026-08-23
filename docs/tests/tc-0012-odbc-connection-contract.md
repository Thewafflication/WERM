# TC-0012: ODBC Connection Contract

**Status:** Controlled

**Requirement:** `REQ-0019`

**Level:** Component test

## Objective

Verify DSN-less driver and configured-DSN connection strings without opening a
vendor-specific ODBC driver.

## Expected Result

The driver form preserves the registered driver and absolute database path;
the DSN form preserves `WERM`; neither form contains `Password` or `Pwd`.
