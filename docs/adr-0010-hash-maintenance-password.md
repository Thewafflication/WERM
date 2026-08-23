# ADR-0010: Hash the Maintenance Password and Gate Write Sessions

**Status:** Accepted

**Date:** 2026-08-22

## Context

WERM must permit ordinary search and printing without authentication while
requiring a password before the GUI enables product, customer, or price
changes. The password must not be stored as plaintext or reversible encrypted
text.

## Decision

WERM will store one maintenance-password verifier in SQLite. The record
contains the algorithm, work factor, random salt, derived hash, and timestamps;
it never contains the password.

The 0.1.0 verifier uses PBKDF2-HMAC-SHA512 with 220,000 iterations, a 32-byte
cryptographically random salt, and a 64-byte derived value. Verification uses
a constant-time byte comparison. A new installation accepts a passphrase from
15 through 256 characters, permits spaces and Unicode, and imposes no
character-class composition rule.

Five consecutive failures impose a 30-second in-process lockout. Successful
verification creates a non-transferable application session with a fixed
10-minute lifetime and records the trimmed operator name for later audit use.
Logout, expiry, or a password change revokes authorization; changing the
password revokes every active session.

The GUI will call the authorization boundary before every write service. The
database file remains an operating-system security boundary: someone who can
directly edit the file can bypass GUI authentication.

## Rationale

PBKDF2 with SHA-512 is directly available in .NET Framework 4.8, avoids a new
native dependency, and has a published work factor. Storing the algorithm and
iteration count permits a future verifier upgrade. Short sessions reduce the
time an unattended workstation remains authorized.

## Consequences

- First-run setup must initialize the only credential before maintenance is
  enabled.
- Password entry and change code must not log, persist, or place the plaintext
  value in exception messages.
- Online throttling is process-local; restarting WERM clears the temporary
  lockout but does not affect the stored verifier.
- File permissions, backup protection, and administrator procedures remain
  necessary controls.
- A future algorithm or work-factor change can retain compatibility by reading
  the stored algorithm and iteration count.

## Guidance Baseline

- [NIST SP 800-63B password verifier guidance](https://pages.nist.gov/800-63-4/sp800-63b.html#passwordver)
- [OWASP Password Storage Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Password_Storage_Cheat_Sheet.html)
- [Microsoft .NET Framework cryptography changes](https://learn.microsoft.com/en-us/dotnet/framework/whats-new/)

## References

- [REQ-0012: Password-gated maintenance](requirements/req-0012-password-gated-maintenance.md)
- [Database design](database-design.md)
- [Milestone 0.1.0](milestones/milestone-0.1.0.md)
