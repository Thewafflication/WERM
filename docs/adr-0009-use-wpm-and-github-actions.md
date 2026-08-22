# ADR-0009: Use WPM and GitHub Actions for Delivery

**Status:** Accepted

**Date:** 2026-08-22

## Context

The milestone requires repeatable packages and controlled tests. The requested
delivery process should resemble WCRT: CI must exercise supported
architectures, retain Debug diagnostic evidence, and construct WPM artifacts
from the tested revision.

## Decision

GitHub Actions will build x86 and x64 Debug configurations, execute controlled
`TC-NNNN` tests, and retain the executable, PDB files, dependencies, XML test
evidence, and source identity for 30 days. The same jobs will build Release and
produce unsigned, architecture-specific WPM packages.

The workflow pins third-party actions by commit and downloads a pinned WPM
archive whose SHA-256 digest is verified before use. Release signing and GitHub
release publication will be added before the 0.1.0 release candidate.

Tests requiring Word, a physical printer, or a locally installed SQLite ODBC
driver are not replaced by runner-only tests. They will have controlled
workstation procedures and retained results.

## Consequences

- A push to `master` starts both architecture jobs without blocking local work.
- Failures in one architecture do not cancel evidence from the other.
- CI WPM packages use a `0.1.0-ci.N` version and are not release claims.
- Release acceptance still requires the controlled physical and integration
  tests allocated by the milestone plan.

## References

- [WSP adoption](wsp-adoption.md)
- [Controlled tests](tests/README.md)
- [Milestone 0.1.0](milestones/milestone-0.1.0.md)
