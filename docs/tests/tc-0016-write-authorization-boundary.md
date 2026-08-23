# TC-0016: Write Authorization Boundary

**Status:** Controlled

**Requirements:** `REQ-0012`, `REQ-0016`, `REQ-0017`, `REQ-0018`

**Level:** Application-service test

## Objective

Attempt a product write without a maintenance session and again after correct
password authentication.

## Expected Result

The unauthenticated attempt raises an authorization error before the data store
is called. The authenticated attempt calls the store once with the authorized
operator identity.
