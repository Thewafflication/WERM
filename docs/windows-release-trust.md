# WERM 0.1.0 Windows Release Trust Plan

**Content type:** Windows release signing, Defender, and trust plan

**Release:** 0.1.0

**Status:** Approved process; execution evidence pending

**Signing operator or service:** Release owner using an approved protected
signing service or non-exportable hardware-backed identity

## Trust Layers

WERM treats these mechanisms as separate controls:

1. Authenticode establishes the Windows publisher identity and protects every
   distributed PE after signing.
2. An RFC 3161 timestamp preserves signature validity evidence beyond the
   certificate's ordinary validity period.
3. Microsoft Defender scans the exact signed bytes and final packages for
   malware and potentially unwanted software.
4. The WPM envelope signature authenticates the package as a package and does
   not replace Authenticode on its PE payload.
5. SHA-256 digests and the release record identify exact artifacts; they do
   not themselves establish a publisher.
6. SmartScreen reputation is observed separately and is not represented as a
   Defender or signature verdict.

## Signing Identity and Certificate Lifecycle

The production Authenticode key shall be non-exportable and protected by a
managed signing service or suitable hardware. It shall not be stored in this
repository, a WPM payload, CI artifact, ordinary environment variable, or
developer-readable file. The release owner records the certificate subject,
issuer, serial/thumbprint, validity, key-protection mechanism, authorized
operators, and timestamp authority in the final trust record.

Renewal is planned before certificate expiration. Suspected loss, unauthorized
use, or compromise immediately blocks signing and publication, triggers
certificate/provider investigation and revocation where applicable, and
requires impact review of every artifact signed by that identity.

The WPM signing key is a distinct trust decision and must be explicitly
approved for WERM 0.1.0. Possession of a local WPM private-key file alone is
not evidence of approval, ownership, or protected Authenticode identity.

## Required Artifact Sequence

1. Check out the approved `master` source revision and verify the repository
   and WSP submodule are clean and at the recorded commits.
2. Build x86 and x64 Release binaries from that exact revision.
3. Run controlled tests and native VERSIONINFO verification; finalize all PE
   bytes and manifests.
4. Authenticode-sign and RFC 3161 timestamp every distributed `.exe` and
   `.dll` with SHA-256.
5. Verify each signature under the applicable Windows trust policy and record
   its signed-file SHA-256 digest and certificate/timestamp details.
6. Build the architecture-specific WPM packages without modifying the signed
   PE files.
7. Verify extracted package PE digests equal the signed inputs, then sign the
   WPM envelope with the separately approved WPM identity.
8. Install, launch, and remove the exact final packages in clean matching-
   architecture Windows environments.
9. Run Microsoft Defender custom scans over the exact signed files and final
   packages. Record engine, platform, intelligence, scan time, and results.
10. Generate the release trust record, test report, documentation PDF,
    digests, provenance, and readiness decision before tagging or publishing.

Packaging, compression, metadata injection, or binary rewriting after PE
signing invalidates the affected signature evidence and requires repetition
from step 3. A package modification after the WPM envelope is signed requires
repackaging and resigning.

## Required Evidence

The final trust record, based on the WSP Windows release trust template, shall
list every distributed PE and package separately with architecture, version,
SHA-256 digest, Authenticode verdict, publisher/certificate identity, RFC 3161
timestamp details, Defender versions/result, WPM signature result, verification
commands, evidence location, operator, and review approval.

The evidence must prove that:

- x86 files contain x86 PE headers and x64 files contain x64 PE headers;
- every PE reports product version 0.1.0 and file version 0.1.0.0 with the
  controlled Waughtal product identity;
- the extracted PE files are byte-for-byte the files that were signed,
  verified, scanned, installed, and packaged;
- the two WPM packages identify the same approved source revision; and
- the published hashes identify the exact approved packages.

## Verification and Failure Policy

Signature absence, an untrusted chain, wrong publisher, digest mismatch,
missing/invalid timestamp, Defender detection, PUA classification, unexpected
SmartScreen observation, package-signature failure, or incomplete evidence is
a release finding. Signature/detection failures block publication unless the
WSP profile permits a specifically reviewed exception; WERM 0.1.0 currently
has no such approved exception.

False-positive investigation must preserve the exact SHA-256 artifact and
detection, reproduce it with recorded Defender versions, review clean-build
provenance/dependencies/behavior, submit the artifact to Microsoft when
appropriate, retain the submission ID and determination, and obtain a new
release decision. WERM will not evade detection through obfuscation, routine
exclusions, or instructions to disable security controls.

## Publisher and Operator Guidance

Published instructions shall name Waughtal as the expected publisher, explain
how to inspect the Authenticode signature and compare package digests, and tell
operators not to install unsigned candidates or bypass Defender. An
unestablished SmartScreen reputation may be documented as a reputation state;
it must not be described as a clean malware verdict.

## Open Release Gates

| Gate | Status | Completion condition |
| --- | --- | --- |
| Approved protected Authenticode identity | Open | Security/release owner records and approves the production certificate and protection mechanism |
| Approved WERM WPM signing identity | Open | Release owner confirms the authorized WPM public identity and protected signing operation |
| Signed exact-source x86/x64 PE files | Open | All PE signature/timestamp checks pass with retained evidence |
| Signed and independently verified WPM packages | Open | Final envelopes verify and extracted PE digests match signed inputs |
| Exact-artifact Defender scan | Open | No unresolved malware/PUA finding; versions and results retained |
| Publisher/operator guidance and final trust record | Open | Documentation/release review passes |

No WERM 0.1.0 engineering candidate may be renamed, tagged, or advertised as
the approved release while any gate in this table remains open.

## References

- [WSP Windows code-signing and Defender profile](../wsp/security/windows-code-signing-and-defender.md)
- [WSP Windows release trust template](../wsp/templates/windows-release-trust-template.md)
- [WERM release readiness record](releases/0.1.0-readiness.md)
- [WERM security policy](../SECURITY.md)
