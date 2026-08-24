# WSP Adoption Record

**Content type:** Project process record

**Project:** Waughtal Enterprise Resource Management (WERM)

**WSP baseline:** `1.1.0`

**Submodule path:** `wsp/`

**Pinned commit:** `8c2adb4afb9f95a5632ec783e37a79c29b1f90f5`

**Status:** Accepted for the project process; this is not release approval

**Approval:** Product-owner direction to use WSP for WERM; dispositions
accepted by the process owner on 2026-08-24

## Adoption Rule

Common WSP requirements and requirements from every selected profile apply to
WERM. An `Implemented` disposition identifies an existing project control; a
`Release gate` remains mandatory but lacks final-release evidence; `Tailored`
or `Not applicable` dispositions are governed by the decisions below.
Accepting this record establishes the process baseline and does not claim that
open release gates pass.

## Profile Selection

| Profile | Selected | Project scope or rationale |
| --- | --- | --- |
| Common requirements management, process, testing, documentation, and tools | Yes | All WERM lifecycle and release work |
| Personal process | No | No reliable historical effort/size measures exist; common proportional planning applies. Reassess after the 0.1.0 retrospective if personal measurement would improve forecasting. |
| Security/DFS | Yes | Maintenance credentials, authorization, business data, local database integrity, Word automation, dependencies, and release trust require threat/control review. |
| C source style | No | WERM-owned application source is C#. Upstream SQLite/ODBC C source is an exact, pinned dependency built without becoming project-maintained C source. |
| PowerShell style | Yes | Build, package, driver-registration, test, evidence, and documentation automation; the profile is guidance and adds no `WSP-*` requirement identifiers. |
| CMake style | No | WERM uses MSBuild and does not maintain a CMake project. |
| Windows version resources | Yes | Applies to every project-owned distributed Windows PE on x86 and x64. |
| Windows code signing and Defender | Yes | Applies to the public/customer-facing Windows PE and WPM release artifacts. |
| Common tools | Yes | Applies to project tools and GitHub Actions. |

## Common Requirement Dispositions

### Requirements Management

| Requirement | Disposition | Project artifact, status, or tailoring |
| --- | --- | --- |
| `WSP-REQM-0001` | Implemented | Stable `REQ-NNNN` files and index in `docs/requirements/`; automated duplicate/index validation. |
| `WSP-REQM-0002` | Implemented | Accepted atomic obligations reviewed against WSP requirements-writing style. |
| `WSP-REQM-0003` | Implemented | Requirement files record rationale, source, dependencies, and references. |
| `WSP-REQM-0004` | Implemented | Every accepted requirement identifies objective verification and controlled test allocations. |
| `WSP-REQM-0005` | Implemented | Bidirectional requirement/test/result validation in `tools/Test-WermTraceability.ps1`; design allocation in milestone and test records. |
| `WSP-REQM-0006` | Implemented | This record pins the WSP release/gitlink and disposes every applicable requirement and profile. |
| `WSP-REQM-0007` | Implemented | Tailoring decisions below record rationale, authority, impact, controls, owner, and review condition. |
| `WSP-REQM-0008` | Implemented | `docs/project-process.md` and the milestone change-control section require impact analysis. |
| `WSP-REQM-0009` | Implemented | Git history preserves changes; identifiers are sequential and not reused. |
| `WSP-REQM-0010` | Release gate | Readiness record identifies the intended baseline; exact final revision/artifacts/evidence remain open until release approval. |

### Project Process

| Requirement | Disposition | Project artifact, status, or tailoring |
| --- | --- | --- |
| `WSP-PROC-0001` | Implemented | Accepted `docs/project-process.md` covers plan through support/improvement and references this adoption. |
| `WSP-PROC-0002` | Implemented | Project process and milestone assign product, engineering, verification, security, release, and process roles. |
| `WSP-PROC-0003` | Implemented | `docs/milestones/milestone-0.1.0.md` records scope, assumptions, risks, criteria, evidence, and unavailable estimates without invention. |
| `WSP-PROC-0004` | Implemented | Requirement/ADR/interface/security/test/platform/release changes require impact analysis and approval. |
| `WSP-PROC-0005` | Implemented | Review inputs, findings, resolution, and approval use WSP review records; open findings cannot be silently closed. |
| `WSP-PROC-0006` | Implemented | Public defects use GitHub Issues; sensitive security findings use private GitHub Security Advisories; release records retain blocking items. |
| `WSP-PROC-0007` | Release gate | `docs/releases/0.1.0-readiness.md` evaluates all required fields and rejects release while any required gate is not Pass. |
| `WSP-PROC-0008` | Release gate | Version/source/artifact/approval/date/evidence baseline will be finalized only after readiness approval. |
| `WSP-PROC-0009` | Implemented | `SECURITY.md` defines vulnerability support/intake; project process defines defect response and feedback. |
| `WSP-PROC-0010` | Planned | Required after 0.1.0 release or a material incident/process failure; milestone closeout allocates the record to the process owner. |

### Testing

| Requirement | Disposition | Project artifact, status, or tailoring |
| --- | --- | --- |
| `WSP-TEST-0001` | Implemented | All 22 accepted product requirements map to controlled verification; traceability validation fails missing links/results. |
| `WSP-TEST-0002` | Implemented | Thirty version-controlled `TC-NNNN` specifications are indexed in `docs/tests/README.md`. |
| `WSP-TEST-0003` | Implemented | Case files plus the controlled automated-execution contract provide purpose, priority, references, environment, inputs, procedure, criteria, and cleanup. |
| `WSP-TEST-0004` | Implemented | Specifications are authoritative; generated reports reference rather than duplicate their procedures. |
| `WSP-TEST-0005` | Implemented | Runners isolate `out/` results, establish architecture/preconditions, use controlled data, and define cleanup. |
| `WSP-TEST-0006` | Implemented | Feasible tests are automated; GUI and physical Word/printer cases explain required operator actions/evidence. |
| `WSP-TEST-0007` | Implemented | XML/JSON execution records include case/revision relationships, source, architecture, configuration, dependencies, timestamps, command/status, verdict, and diagnostics as applicable. |
| `WSP-TEST-0008` | Implemented | Validators accept only controlled statuses and require rationale for non-Pass results; only Pass satisfies a gate. |
| `WSP-TEST-0009` | Implemented | GitHub retains failed logs/artifacts; later reruns use new immutable Actions runs and do not overwrite the original. |
| `WSP-TEST-0010` | Implemented | `tools/New-WermTestReport.ps1` generates report data from retained XML/JSON evidence. |
| `WSP-TEST-0011` | Implemented | `tools/Test-WermTraceability.ps1` checks requirement/test/result identities, back-references, source/architecture, and counts. |
| `WSP-TEST-0012` | Implemented | GitHub Actions runs x86/x64 Debug tests, real ODBC/WSH integration, Release builds, WPM clean install/removal, traceability, and reporting on `master`. |
| `WSP-TEST-0013` | Release gate | Matrix is x86/x64 with ARM64 unclaimed; automated gates run in CI while `TC-0026` and `TC-0027` physical gates remain open. |
| `WSP-TEST-0014` | Implemented | CI artifact retention is 30 days; readiness/release records identify longer-lived approved evidence. |
| `WSP-TEST-0015` | Implemented | Specifications use requirements-based scenarios, equivalence partitions, boundaries, decisions, and fault injection as appropriate. |
| `WSP-TEST-0016` | Implemented | `if: always()` architecture artifacts retain tested Debug binaries, PDBs, XML, dependency/integration evidence, and package output for 30 days. |
| `WSP-TEST-0017` | Not applicable | ARM64 is explicitly not claimed or produced for 0.1.0; see tailoring TD-001. |
| `WSP-TEST-0018` | Tailored | GDB cannot inspect .NET Framework/PDB binaries; managed exception stacks, original output, exact binaries/PDBs, and failed-run artifacts are retained; see TD-002. |

### Release Documentation

| Requirement | Disposition | Project artifact, status, or tailoring |
| --- | --- | --- |
| `WSP-DOC-0001` | Release gate | One release-document PDF will contain every source in the controlled manifest. |
| `WSP-DOC-0002` | Release gate | Version-controlled ordered manifest and duplicate/missing input failures required before publication. |
| `WSP-DOC-0003` | Release gate | PDF title/build identity must record project, URL, version, revision, date, Pandoc, and engine. |
| `WSP-DOC-0004` | Release gate | Numbered TOC, bookmarks, and working internal/external links require automated/rendered verification. |
| `WSP-DOC-0005` | Release gate | Shared WSP presentation and rendered-page inspection required. |
| `WSP-DOC-0006` | Implemented | Controlled Markdown/manifest remain authoritative; generated PDF/TeX are outputs only. |
| `WSP-DOC-0007` | Implemented | Project documentation outputs are isolated under project-owned `tmp/` and `output/`, never `wsp/`. |
| `WSP-DOC-0008` | Release gate | Build/negative tests must fail for missing/invalid inputs and empty/missing PDF. |
| `WSP-DOC-0009` | Release gate | Final metadata, pages, text, TOC, outlines, annotations, links, and rendered visual review required. |
| `WSP-DOC-0010` | Release gate | Embedded and displayed descriptive metadata must agree. |
| `WSP-DOC-0011` | Release gate | Exact PDF digest must be placed in the simultaneously published `SHA256SUMS`. |
| `WSP-DOC-0012` | Release gate | Public workflow must issue and verify a GitHub artifact attestation for the final PDF. |
| `WSP-DOC-0013` | Not applicable | PAdES is not required for 0.1.0 and no PDF-signature claim will be made; see TD-003. |

### Common Tools

| Requirement | Disposition | Project artifact, status, or tailoring |
| --- | --- | --- |
| `WSP-TOOL-0001` | Implemented | WSP, actions, WSH, WPM, SQLite ODBC/SQLite source revisions and archives are pinned; downloaded inputs are hash-verified. |
| `WSP-TOOL-0002` | Implemented | Tools resolve the repository root from their controlled script path and accept explicit architecture/configuration/version/input arguments. |
| `WSP-TOOL-0003` | Implemented | Tools isolate prior output, validate controlled inputs, and produce deterministic verdicts for the same baseline/environment. |
| `WSP-TOOL-0004` | Implemented | Native failures and invalid/missing output produce nonzero status/terminating errors; successful expected-negative tests normalize their captured status. |
| `WSP-TOOL-0005` | Implemented | Diagnostics identify the operation, architecture/path/input, expected condition, and observed failure. |
| `WSP-TOOL-0006` | Implemented | Generated binaries, dependencies, packages, results, and evidence remain beneath project `out/`, `tmp/`, or `output/`. |
| `WSP-TOOL-0007` | Implemented | No credential/signing key is accepted by ordinary build/test logs; settings/evidence tests reject persisted credentials; protected signing is separate. |
| `WSP-TOOL-0008` | Implemented | Build/test/traceability/package/version scripts validate outputs, hashes, architecture, identity, status, and cleanup; CI executes the combined toolchain. |
| `WSP-TOOL-0009` | Implemented | `windows-2025` hosted runners and pinned current checkout v6, upload-artifact v7, and download-artifact v8 commits follow WSP 1.1.0 runtime guidance. |

## Selected Profile Dispositions

### Security and Design for Security

| Requirement | Disposition | Project artifact, status, or tailoring |
| --- | --- | --- |
| `WSP-SEC-0001` | Implemented | `docs/design-for-security.md` defines system/security scope, objectives, assets, and explicit non-goals. |
| `WSP-SEC-0002` | Implemented | Controlled DFS records baseline, trust model, threats, derived controls, verification, response, and residual risk. |
| `WSP-SEC-0003` | Implemented | DFS identifies operator, file, ODBC, settings, Word, installer, build, administrator, and third-party trust boundaries. |
| `WSP-SEC-0004` | Implemented | Fourteen threats cover credentials, authorization, direct DB edits, SQL/input, dependencies, Word, packaging, evidence, and recovery. |
| `WSP-SEC-0005` | Implemented | Nine `DFS-SR-NNN` derived requirements allocate implemented and open security controls. |
| `WSP-SEC-0006` | Implemented | Parameterized SQL, schema/path/XML validation, bounded passphrases, controlled migrations, and resource cleanup constrain untrusted inputs/resources. |
| `WSP-SEC-0007` | Implemented | Runtime is least privilege; GUI writes require short maintenance sessions; administration is isolated to install/ODBC registration. |
| `WSP-SEC-0008` | Implemented | ADR-0010 controls PBKDF2 parameters, randomness, fixed-time comparison, password handling, and verifier persistence. |
| `WSP-SEC-0009` | Implemented | Exact dependency inputs/hashes/provenance, pinned actions, controlled runner, build identity, and CI validation protect build integrity. |
| `WSP-SEC-0010` | Implemented | Audit lineage, credential-safe logs/settings, synthetic CI data, and documented database/backup ACL boundary. |
| `WSP-SEC-0011` | Implemented | Transaction rollback, new-file cleanup, preservation of pre-existing databases, Word cleanup, and recovery guidance. |
| `WSP-SEC-0012` | Release gate | Automated security cases run x86/x64; security review, GUI, template/Word, signing, Defender, and physical gates require final evidence. |
| `WSP-SEC-0013` | Release gate | Formal 0.1.0 security review must dispose dependency, direct-file, template, physical, and signing findings before release. |
| `WSP-SEC-0014` | Implemented | `SECURITY.md` defines private intake, ownership, assessment, remediation, verification, disclosure, supported update, and compromise response. |

### Windows Version Resources

| Requirement | Disposition | Project artifact, status, or tailoring |
| --- | --- | --- |
| `WSP-WINRES-0001` | Implemented | Every project-owned x86/x64 executable/DLL receives one native `VERSIONINFO`. |
| `WSP-WINRES-0002` | Implemented | `tools/New-WermVersionResources.ps1` generates `.rc/.res` outputs from the build version; generated files remain under `out/`. |
| `WSP-WINRES-0003` | Implemented | Semantic `0.1.0` maps to numeric `0,1,0,0`; tagged-release build component is zero. |
| `WSP-WINRES-0004` | Implemented | Product version is 0.1.0 and file version 0.1.0.0 consistently across WERM binaries. |
| `WSP-WINRES-0005` | Implemented | All eight required strings and the canonical repository comment are generated and queried. |
| `WSP-WINRES-0006` | Implemented | Original/internal names match outputs; descriptions distinguish application/core/data/printing/test functions. |
| `WSP-WINRES-0007` | Implemented | EXEs use `VFT_APP`, DLLs use `VFT_DLL`; all use `VOS_NT_WINDOWS32`/`VFT2_UNKNOWN`. |
| `WSP-WINRES-0008` | Implemented | Debug resources set `VS_FF_DEBUG`; final Release flags are zero; mask is `VS_FFI_FILEFLAGSMASK`. |
| `WSP-WINRES-0009` | Implemented | US-English Unicode table `040904B0` agrees with translation `0x0409,1200`. |
| `WSP-WINRES-0010` | Implemented | Automated checks compare controlled identity and PE machine type on x86/x64. |
| `WSP-WINRES-0011` | Implemented | Waughtal/product/copyright/repository strings are public controlled values without machine/user/path disclosure. |
| `WSP-WINRES-0012` | Implemented | CI queries every distributed WERM EXE/DLL in Debug/Release on x86/x64 and retains JSON evidence. |

### Windows Code Signing and Defender

| Requirement | Disposition | Project artifact, status, or tailoring |
| --- | --- | --- |
| `WSP-SIGN-0001` | Implemented plan | `docs/windows-release-trust.md` defines artifacts, identity requirements, roles, sequence, verification, and evidence; production identity remains open. |
| `WSP-SIGN-0002` | Implemented | Trust plan and DFS distinguish Authenticode, WPM signatures, digests, Defender, and SmartScreen. |
| `WSP-SIGN-0003` | Release gate | Protected managed/hardware/non-exportable publisher identity and audited access must be approved; no repository key is permitted. |
| `WSP-SIGN-0004` | Release gate | Every final project-owned distributed PE must carry verified SHA-256 Authenticode. |
| `WSP-SIGN-0005` | Release gate | Every PE signature requires a verified trusted RFC 3161 SHA-256 timestamp. |
| `WSP-SIGN-0006` | Not applicable | WERM 0.1.0 claims no legacy platform requiring SHA-1 or dual signing; SHA-1-only signing is prohibited. |
| `WSP-SIGN-0007` | Implemented plan | Trust plan fixes finalization/sign/verify/scan/package/WPM-sign order and requires byte identity. |
| `WSP-SIGN-0008` | Release gate | Every x86/x64 signature and warning must receive an independent Windows-policy verdict. |
| `WSP-SIGN-0009` | Release gate | Final record must bind name, architecture, version, revision, digest, signer, and timestamp. |
| `WSP-SIGN-0010` | Release gate | Exact signed PEs and distributed packages require supported/current Defender scan evidence. |
| `WSP-SIGN-0011` | Conditional control | Any malware/PUA detection blocks release pending a resolved finding or explicit approved decision. |
| `WSP-SIGN-0012` | Conditional control | Trust reports must be classified as Defender malware, PUA, signature/certificate, or SmartScreen before response. |
| `WSP-SIGN-0013` | Conditional control | Suspected false positives require exact-digest reproduction and signature/provenance/build/dependency/behavior/compromise review. |
| `WSP-SIGN-0014` | Conditional control | A disputed Microsoft detection requires submission ID, dates, digest, determination, and release decision. |
| `WSP-SIGN-0015` | Implemented policy | Trust plan prohibits evasion, disabling protection, and routine broad exclusions; any internal exception is narrow/time-limited/approved. |
| `WSP-SIGN-0016` | Release gate | Publisher must equal Waughtal version identity; final instructions must provide publisher, digests, verification, secure location, and warning channel. |
| `WSP-SIGN-0017` | Implemented plan | Trust plan defines renewal, expiration, revocation, compromise stop/investigation/replacement/impact review. |
| `WSP-SIGN-0018` | Release gate | Final signing/timestamp/verification/digest/Defender/exception evidence must accompany the release record and retention baseline. |

## Tailoring Decisions

### TD-001 — ARM64 Test Execution Not Applicable to 0.1.0

- **Requirement:** `WSP-TEST-0017`
- **Decision and rationale:** Not applicable because 0.1.0 neither builds,
  packages, supports, nor claims ARM64.
- **Authority/date:** Product/process owner, 2026-08-24.
- **Impact:** No ARM64 compatibility or test claim is permitted.
- **Compensating control:** x86 and x64 are explicit in packages, PE checks,
  evidence, and release claims.
- **Owner/review condition:** Engineering owner; reopen before any ARM64 build,
  package, support statement, or requirement is approved.

### TD-002 — Managed .NET Debug Diagnostics Replace GDB

- **Requirement:** `WSP-TEST-0018`
- **Decision and rationale:** GDB cannot reliably inspect WERM's .NET Framework
  4.8 PE/PDB managed binaries. Controlled-test exceptions retain managed stack
  traces in XML and console output; GitHub retains the original failed result,
  exact Debug binaries, PDBs, and logs.
- **Authority/date:** Process/verification owner, 2026-08-24.
- **Impact:** A catastrophic runtime termination outside managed exception
  handling may require post-run Windows dump analysis rather than an immediate
  GDB backtrace.
- **Compensating control:** `if: always()` failure artifacts, PDBs, immutable CI
  logs/runs, local reproducibility, and release blocking until an unexplained
  Debug failure is diagnosed.
- **Owner/target:** Verification owner; automate Windows managed crash-dump and
  stack extraction before accepting a release test whose failure cannot be
  explained from retained managed diagnostics.

### TD-003 — PAdES Not Selected for 0.1.0

- **Requirement:** `WSP-DOC-0013`
- **Decision and rationale:** No contract, regulator, or release policy requires
  a digitally signed documentation PDF for 0.1.0; WERM makes no PAdES claim.
- **Authority/date:** Product/process owner, 2026-08-24.
- **Impact:** The PDF will not carry a document-publisher signature.
- **Compensating control:** SHA-256 `SHA256SUMS`, GitHub build-provenance
  attestation, exact source/revision metadata, and secure release publication.
- **Owner/review condition:** Release owner; reassess if a customer, contract,
  regulator, or document-authenticity policy requires PDF signing.

## WSP 1.1.0 Upgrade Impact

The project uses the WSP 1.1.0 milestone plan/design/review/closeout/work-log
records and its clarified deferral rule. `WSP-TEST-0016` is implemented with
30-day Debug artifacts, `WSP-TEST-0017` is explicitly not applicable,
`WSP-TEST-0018` is tailored for managed diagnostics, and `WSP-TOOL-0009` uses
the current supported action majors pinned by commit. Barcode work remains an
approved product-scope deferral and is not a 0.1.0 release gate.

## Baseline History

| Date | WSP baseline | Project change | Summary |
| --- | --- | --- | --- |
| 2026-08-22 | `2198ccab08f969a789448767fe7017b774369adc` | Working baseline | Initial proposed adoption |
| 2026-08-22 | `1.1.0` (`8c2adb4afb9f95a5632ec783e37a79c29b1f90f5`) | Working baseline | Upgrade impact assessed |
| 2026-08-24 | `1.1.0` (`8c2adb4afb9f95a5632ec783e37a79c29b1f90f5`) | Accepted process baseline | Profiles selected; all applicable requirements disposed; release gates remain explicit |
