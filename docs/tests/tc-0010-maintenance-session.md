# TC-0010: Maintenance Session

**Status:** Controlled

**Requirement:** `REQ-0012`

**Level:** Application-service test

## Objective

Verify that only a correct password creates a maintenance session and that the
session expires after the approved fixed lifetime.

## Procedure

1. Initialize the credential and reject a second initialization.
2. Demand authorization without a session.
3. Attempt authentication with an incorrect password.
4. Authenticate with the correct password and inspect the normalized operator.
5. Advance the controlled clock beyond ten minutes and demand authorization.

## Expected Result

Missing and expired sessions are rejected, the incorrect password creates no
session, and the valid session authorizes the named operator until expiry.
