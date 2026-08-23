# TC-0011: Authentication Throttling

**Status:** Controlled

**Requirement:** `REQ-0012`

**Level:** Security unit test

## Objective

Verify the temporary online-guessing control at the application boundary.

## Procedure

1. Submit five consecutive incorrect passwords.
2. Submit the correct password during the lockout interval.
3. Advance the controlled clock by 31 seconds and submit the correct password.

## Expected Result

All incorrect attempts fail, the correct password remains blocked during the
30-second lockout, and authentication succeeds after the interval.
