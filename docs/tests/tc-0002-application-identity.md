# TC-0002: Application Identity

**Status:** Controlled

**Requirement:** `REQ-0001`

**Level:** Unit test

## Objective

Verify that the application shell consumes a single core identity for the WERM
name and milestone version.

## Procedure

Run the controlled test executable through `tools/Run-WermTests.ps1` for each
supported architecture.

## Expected Result

The short name is `WERM`, the milestone identity is `0.1.0`, and the test
records `PASS`.
