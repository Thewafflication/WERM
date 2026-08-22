# WSP Adoption Record

**Content type:** Project process record

**Project:** Waughtal Enterprise Resource Management (WERM)

**WSP baseline:** `1.1.0`

**Submodule path:** `wsp/`

**Pinned commit:** `8c2adb4afb9f95a5632ec783e37a79c29b1f90f5`

**Status:** Proposed

**Approval:** Pending initial project documentation review

## Common Baseline

| Requirement set or practice | Applicability | Project artifact or scope |
| --- | --- | --- |
| Common requirements management | Yes | `docs/requirements/` |
| WSP software lifecycle | Yes | `docs/project-process.md` |
| Project process | Yes | `docs/project-process.md` and milestone plans |
| Documentation requirements | Yes | Controlled project documentation |
| Documentation style and identifiers | Yes | Project-authored artifacts |
| Testing requirements | Yes | Milestone verification plans and controlled tests |

## Selected Profiles

Profile selection remains proposed until the initial project process and
release plan are reviewed.

| Profile | Selected | Project scope or rationale |
| --- | --- | --- |
| Personal process | Pending | Decide with the initial project process |
| Security/DFS | Pending | Decide during milestone work package M1.1 |
| C source style | No | The application will be implemented in .NET |
| PowerShell style | Yes | Temporary ODBC worker and later automation |
| CMake style | No | The .NET project will not use CMake |
| Windows version resources | Yes | Applies to the shipped Windows executable |
| Windows code signing and Defender | Pending | Decide during milestone work package M1.1 |
| Common tools | Yes | GitHub Actions with pinned actions and verified WPM bootstrap |

## Requirement Dispositions

The complete requirement disposition table will be added before this adoption
record is accepted. No WSP requirement is omitted by the current proposed
record. Common requirements are presumed applicable unless an approved
tailoring decision records otherwise.

## Tailoring Decisions

No tailoring decisions have been approved.

## WSP 1.1.0 Upgrade Impact

WSP 1.1.0 adds the reusable milestone work-plan, design, review, closeout, and
work-log records now referenced by the WERM project process. It also clarifies
that approved deferred work remains outside current release gates and
verification claims. The version 0.1.0 plan applies that rule to barcode work.

The release adds these normative obligations requiring disposition during
milestone work package M1.1:

| Requirement | Preliminary impact | Required action |
| --- | --- | --- |
| `WSP-TEST-0016` | Applies to successful Debug CI jobs | Retain executables, PDBs, dependencies, runner, and XML evidence for 30 days |
| `WSP-TEST-0017` | ARM64 is not claimed for 0.1.0 | Reassess only before adding an ARM64 release target |
| `WSP-TEST-0018` | GDB is not the normal .NET Framework debug path | Approve .NET-specific tailoring and equivalent diagnostics |
| `WSP-TOOL-0009` | Applies to GitHub Actions and common tools | Pin maintained actions and use the Windows 2025 runner |

No accepted WERM product requirement or ADR conflicts with WSP 1.1.0. The
complete disposition table and the identified .NET diagnostic tailoring must
be approved before the adoption record becomes Accepted.

## Baseline History

| Date | WSP baseline | Project change | Summary |
| --- | --- | --- | --- |
| 2026-08-22 | `2198ccab08f969a789448767fe7017b774369adc` | Working tree | Initial proposed adoption |
| 2026-08-22 | `1.1.0` (`8c2adb4afb9f95a5632ec783e37a79c29b1f90f5`) | Working tree | Upgrade assessed; adoption remains proposed |
