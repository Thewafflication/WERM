# TC-0018: Credential Persistence

**Status:** Controlled

**Requirements:** `REQ-0012`, `REQ-0019`

**Level:** Data-component test

## Objective

Verify the ODBC repository mapping for password-verifier metadata.

## Expected Result

The repository writes algorithm, iteration count, Base64 salt, Base64 hash, and
timestamps through positional parameters, then reconstructs identical verifier
bytes when reading the controlled row.
