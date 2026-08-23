# TC-0009: Maintenance Password Verifier

**Status:** Controlled

**Requirement:** `REQ-0012`

**Level:** Security unit test

## Objective

Verify the approved password policy, salted PBKDF2-HMAC-SHA512 verifier, and
constant-time verification result boundary.

## Procedure

1. Create two credentials from the same conforming passphrase.
2. Inspect their algorithm, iteration count, salt length, and hash length.
3. Verify the correct and an incorrect password against one credential.
4. Compare the two salts.
5. Attempt to create credentials with 14 and 257 characters.

## Expected Result

The profile is SHA-512 with 220,000 iterations, a 32-byte salt, and a 64-byte
hash. The correct password passes, the incorrect password fails, salts differ,
and both out-of-policy lengths are rejected.
