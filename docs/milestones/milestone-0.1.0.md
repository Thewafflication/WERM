# Milestone 0.1.0: Initial Label Printing

**Content type:** Project milestone work plan

**Milestone or work package:** M1 — WERM 0.1.0

**Status:** In progress

**Planned period:** Start 2026-08-22; target completion to be forecast after
the M1.2 design spikes

**Inherited baseline:** Initial WERM project and WSP 1.1.0

**Owner:** Engineering owner

**Approval:** Product owner authorized implementation on 2026-08-22; release
approval remains pending

## Objective and Scope

Deliver a Windows desktop application that maintains label data in SQLite,
populates a Microsoft Word label template from a selected product and customer
price, and prints the populated document directly to a label printer.

### Included Work

- .NET Framework 4.8 Windows desktop application;
- SQLite schema creation and version management;
- product records keyed by PLU;
- product description, ingredients statement, and safe-handling status;
- customer records and customer-product prices;
- password-gated GUI maintenance for products, customers, and prices;
- append-only product and customer-price change history;
- product, customer, and price selection for a print job;
- population of named fields in a Word label template;
- direct Word printing to a selected Windows label printer;
- useful error reporting and cleanup after database, Word, or printer failure;
  and
- controlled verification evidence and release-readiness review.

### Excluded or Deferred Work

Milestone 0.1.0 excludes:

- barcode storage, generation, validation, field population, and printing;
- PDF generation or printing;
- scheduled or effective-dated pricing;
- bulk customer, product, or price import;
- a general-purpose Word-template editor;
- unattended Windows-service execution; and
- multi-user database-server operation.

Barcode work is an approved deferral recorded below. It is not a version 0.1.0
exit criterion or release gate and will not be included in the release's
completion or verification claims.

## Baseline and Assumptions

The milestone inherits the accepted WERM requirements and ADRs, the proposed
database design, the project process, and WSP 1.1.0 at commit
`8c2adb4afb9f95a5632ec783e37a79c29b1f90f5`.

The current assumptions are:

- each printing workstation has .NET Framework 4.8 and an approved Microsoft
  Word desktop version installed;
- a representative Word label template, label printer, printer driver, and
  label stock are available for the printing spike and release verification;
- WERM owns the initial SQLite schema and migration history;
- PLUs are stable text identifiers during this milestone; and
- version 0.1.0 uses one locally accessible SQLite database file.

The accepted build and package matrix is:

| Target | Debug build and controlled tests | Release WPM package |
| --- | --- | --- |
| Windows x86 | Required | Required |
| Windows x64 | Required | Required |
| Windows ARM64 | Not claimed for 0.1.0 | Not produced |

Word, SQLite ODBC, and printer components must match the selected x86 or x64
application architecture. See ADR-0008.

M1.2 must confirm these assumptions. A failed assumption requires plan and
impact review rather than an implicit scope change.

## Deliverables

| Deliverable | Owner | Completion evidence |
| --- | --- | --- |
| Accepted WSP adoption and milestone baseline | Process owner | Adoption and review records |
| Buildable .NET Framework 4.8 solution | Engineering owner | Clean build and retained build record |
| Versioned SQLite schema and data layer | Engineering owner | Migration and integration-test evidence |
| Authorized maintenance GUI | Engineering owner | GUI and authorization-test evidence |
| Append-only audit implementation | Engineering owner | Transaction and lineage-test evidence |
| Word template contract and integration | Engineering owner | Mapping tests and reviewed contract |
| Direct label-printer workflow | Engineering owner | Controlled physical-print evidence |
| Installation and user documentation | Release owner | Documentation review |
| WERM 0.1.0 release candidate | Release owner | Artifact identity and readiness record |
| Test report and closeout record | Verification owner | Approved report and milestone closeout |

## Requirement and Verification Allocation

| Requirement or objective | Design or implementation allocation | Verification | Required gate |
| --- | --- | --- | --- |
| `REQ-0001`, `REQ-0002`, `REQ-0019` through `REQ-0021` | Solution, ODBC migration, WSH installer, and data access | Build inspection, installation, and ODBC integration tests | Yes |
| `REQ-0003` through `REQ-0006` | Selection, Word integration, and printing | Mapping tests, inspection, and physical print test | Yes |
| `REQ-0007` through `REQ-0011` | SQLite schema, models, and data-access layer | Constraint and round-trip tests | Yes |
| `REQ-0012` | Maintenance authorization boundary | Authorization tests and security review | Yes |
| `REQ-0013` through `REQ-0015` | Audit transaction and query components | Transaction, lineage, and preservation tests | Yes |
| `REQ-0016` through `REQ-0018` | Maintenance GUI and application services | GUI system and database-state tests | Yes |
| `REQ-0022` | Per-user database configuration and safe schema creation | Persistence, transaction, and GUI system tests | Yes |
| Barcode deferral | Outside version 0.1.0 | Scope and release-claim inspection | No |

Controlled `TC-NNNN` identifiers are indexed in `docs/tests/README.md`.
`REQ-0003` and `REQ-0004` are accepted: record selection uses PLU, customer ID,
and price type, and the controlled Word contract defines nine tagged content
controls. Physical Word/printer verification remains a release gate.

## Roles and Review

| Role | Assignment | Responsibility or independence condition |
| --- | --- | --- |
| Owner | Engineering owner | Scope, implementation, and completion forecast |
| Product owner | Product owner | Requirement acceptance, scope, and release approval |
| Reviewer | Assigned reviewer | Requirements, design, source, tests, and documentation review |
| Verifier | Verification owner | Controlled evidence and verdicts |
| Security reviewer | Security owner | Credential, file-access, audit, and dependency review |
| Release owner | Release owner | Artifact identity, readiness, and release baseline |

One person may hold multiple roles. Any required independence or single-person
exception must be recorded in the applicable WSP review record.

## Risks and Controls

| Risk | Impact | Planned control | Owner or trigger |
| --- | --- | --- | --- |
| Word COM process remains after failure | Workstation instability | Exercise cleanup and failure paths in an early spike | M1.2 / engineering owner |
| Printer driver changes layout | Misaligned or clipped labels | Verify the exact template, driver, stock, and printer | M1.2 and M1.5 |
| SQLite file is edited outside WERM | Authorization and audit bypass | Restrict file access and document the security boundary | Security owner |
| Credential design is delayed | Maintenance work is blocked | Complete credential decision before M1.4 | M1.2 exit |
| Field contract remains ambiguous | Wrong or missing label values | Review a representative template contract | M1.2 exit |
| ODBC driver or bitness mismatch | Database connection failure | Test the exact driver and application architecture | M1.2 exit |
| Waughtal Shell cannot execute scripts yet | Required installer entry point cannot run | Baseline an execution-capable WSH release before M1.3 closes | M1.3 exit |
| Supported platform matrix is unknown | Incomplete release claim | Define architectures, Windows, Word, and ODBC versions | M1.2 exit |
| GDB cannot inspect .NET Framework tests | WSP Debug diagnostic mismatch | Approve equivalent .NET diagnostic tailoring | M1.1 / process owner |

## Estimate and Forecast

Reliable calendar, effort, and size estimates are not yet available. They will
be created after M1.2 resolves the UI, Word field, credential, platform, and
printer uncertainties. No unavailable measure will be reconstructed or
invented.

| Phase or activity | Planned size or effort | Current forecast | Basis |
| --- | ---: | ---: | --- |
| Plan and baseline | Unavailable | In progress | Initial project records |
| Resolve blocking design | Unavailable | Pending M1.2 spikes | No project history |
| Implement and review | Unavailable | Pending design | Architecture unresolved |
| Verify and close | Unavailable | Pending release matrix | Test inventory unresolved |

The milestone must be replanned when a blocking assumption fails, included
scope changes, a required gate becomes infeasible, or the approved forecast is
exceeded by the threshold selected after M1.2.

## Execution and Evidence

- Unit tests will cover validation, mapping, price conversion, and audit-event
  construction.
- Integration tests will cover SQLite constraints, migrations, transactions,
  revision lineage, and data retrieval.
- GUI system tests will cover maintenance authorization and workflows.
- Word integration tests will use a controlled representative template.
- A controlled manual test will verify physical output on the intended label
  printer because stock alignment and printer-driver behavior are physical
  observations.
- Inspection will verify the target framework, prerequisites, dependencies,
  documentation, and absence of barcode and PDF behavior.

Tests used as release evidence will receive controlled `TC-NNNN`
specifications. Execution evidence will identify the source revision, build,
environment, dependency versions, timestamps, result, and diagnostics. Failed
results will be retained when a later execution passes.

M1.1 defines GitHub Actions as the CI dispatcher, x86 and x64 as the supported
matrix, XML as the initial controlled-test evidence format, `out/` and GitHub
Actions artifacts as the artifact locations, and 30 days as CI retention. A
successful Debug job retains the executable, PDB files, dependency assemblies,
controlled-test executable, and XML results. The .NET diagnostic tailoring
still requires process approval.

## Rollback and Recovery

- Database migrations will be versioned, transactional where SQLite permits,
  and tested against a disposable copy before release use.
- Installation documentation will require a database backup before an upgrade
  that changes the schema.
- A failed data modification will roll back its data and audit writes together.
- Word working documents will be temporary, and failure cleanup will preserve
  the original template.
- Incomplete implementation increments will remain isolated through version
  control and will not be represented as verified release artifacts.
- Failed test evidence will be preserved even when the software or environment
  is repaired and the test is rerun.

## Work Packages

### M1.1 — Baseline Requirements and Process

Review the WSP 1.1.0 adoption impact, complete profile and requirement
dispositions, accept milestone requirements, allocate `TC-NNNN` identifiers,
define the release matrix, and establish traceability and evidence rules.

### M1.2 — Resolve Blocking Design Decisions

Confirm the selected WPF workflow, SQLite ODBC driver deployment baseline,
tagged Word content-control contract, credential handling, supported platform
and architecture, and printer-selection behavior. Complete representative
ODBC, Word, and physical-printer spikes. The software decisions are complete;
the vendor-specific ODBC and physical Word/printer baselines remain open.

### M1.3 — Establish the Solution and Database

Create the .NET Framework 4.8 solution, repeatable build, ODBC database
migration mechanism, initial schema, data-access layer, and automated database
tests. Validate the Waughtal Shell database installer through the selected ODBC
driver. The work package requires an execution-capable Waughtal Shell baseline;
the PowerShell worker is temporary bootstrap infrastructure.

### M1.4 — Implement Authorized Data Maintenance

Implement password verification and the product, customer, price,
deactivation, and history workflows.

### M1.5 — Implement Word Label Printing

Implement record selection, field mapping, working-document handling,
invisible Word automation, printer selection, direct printing, and COM cleanup.

### M1.6 — Integrate, Verify, and Prepare Release

Complete test specifications, traceability checks, CI gates, installation
documentation, dependency baseline, test report, release-readiness review, and
milestone closeout.

## Exit Criteria

| Criterion | Required evidence | Gate | Status |
| --- | --- | --- | --- |
| WSP adoption and milestone baselines are accepted | Adoption and review record | Required | Fail — adoption remains Proposed |
| Every included requirement is accepted and traced | Requirements and traceability report | Required | Pass — 22 requirements, 30 specifications, 56 result records |
| A clean environment builds and installs the release candidate | Build and installation evidence | Required | Fail — build passes; `TC-0028` is Blocked |
| Authorized maintenance and audit behavior pass | Test report | Required | Fail — automated boundary passes; `TC-0026` is Blocked |
| Representative Word fields contain expected data | Mapping and system-test evidence | Required | Fail — mapping passes; physical Word system test is Blocked |
| A representative physical label is correct | Print-test record and inspected sample | Required | Fail — `TC-0027` is Blocked |
| Failure recovery and Word cleanup pass | Failure-path tests | Required | Fail — component test passes; physical cleanup remains Blocked |
| Required release-matrix entries pass | Test report and evidence inventory | Required | Fail — 52 automated Pass, four manual Blocked |
| Documentation and release-readiness review pass | Review and readiness records | Required | Fail — readiness decision is Reject pending required reviews |
| Barcode behavior is absent from release claims | Scope inspection | Informative | Pass |

Every required gate must pass before milestone closeout.

## Deferred Objectives

| Objective | Impact | Owner | Target milestone or release | Compensating control | Approval |
| --- | --- | --- | --- | --- | --- |
| Barcode storage, generation, validation, field population, and printing | No barcode label capability in 0.1.0 | Product owner | First approved milestone containing barcode requirements | None | Stakeholder scope decision, 2026-08-22 |
| Scheduled and effective-dated prices | Prices cannot be scheduled in advance | Product owner | Milestone selected after 0.1.0 requirements review | Manual price update | Plan approval pending |
| Bulk data import | Initial data requires GUI maintenance or controlled setup | Product owner | Milestone selected after 0.1.0 use evaluation | GUI maintenance | Plan approval pending |

Deferred objectives remain outside version 0.1.0 completion and verification
claims. Release approval must close, revise, or explicitly carry each one
forward.

## Change Control

The milestone requires impact analysis and product-owner review when a change:

- adds or removes included behavior;
- changes an accepted requirement, ADR, database schema, template contract,
  supported platform, security boundary, or required release gate;
- introduces barcode or PDF behavior into version 0.1.0;
- changes the Word or SQLite dependency strategy;
- changes the authorization or audit model; or
- invalidates a baseline assumption or accepted estimate.

A durable technical change receives a new or superseding ADR. A security-
relevant change receives DFS impact review if the Security/DFS profile is
selected. Failed or unknown required gates cannot be waived through ordinary
replanning; they require correction or a revised release claim.

## References

- [Project process](../project-process.md)
- [WSP adoption record](../wsp-adoption.md)
- [Product requirements](../requirements/README.md)
- [Database design](../database-design.md)
- [ADR-0006: Access SQLite through ODBC](../adr-0006-access-sqlite-through-odbc.md)
- [ADR-0007: Use Waughtal Shell for database installation](../adr-0007-use-waughtal-shell-database-installer.md)
- [ADR-0008: Use WPF with x86 and x64 packages](../adr-0008-use-wpf-and-x86-x64-packages.md)
- [ADR-0009: Use WPM and GitHub Actions for delivery](../adr-0009-use-wpm-and-github-actions.md)
- [ADR-0012: Use tagged Word content controls](../adr-0012-use-tagged-word-content-controls.md)
- [ADR-0013: Use per-user database configuration](../adr-0013-use-per-user-database-configuration.md)
- [Database installation](../database-installation.md)
- [Word template contract](../word-template-contract.md)
- [Workstation configuration](../workstation-configuration.md)
- [0.1.0 release readiness](../releases/0.1.0-readiness.md)
- [WSP milestone work-plan template](../../wsp/processes/milestone-plan-template.md)
- [WSP test strategy](../../wsp/testing/test-strategy.md)
- [WSP release-readiness template](../../wsp/processes/release-readiness-template.md)
