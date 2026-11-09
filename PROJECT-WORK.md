# PROJECT-WORK.md

**Project:** Waughtal Enterprise Resource Management (WERM)

**Version:** 0.1.0

**Branch/Worktree:** master

**Status:** Adopted from WSP-PROC-0012

## Work Record

This record tracks the current state of project work, including requirements, prerequisites, branch/worktree, retained checks, acceptance criteria, and publication targets.

### Requirements

- WERM version 0.1.0 is a Windows label-printing application
- Maintains product, customer, and customer-specific price data in SQLite through ODBC
- Populates a Microsoft Word label template and prints through Word to a label printer
- Milestone 0.1.0 deliberately excludes barcode and PDF behavior
- Uses Waughtal Shell 1.4.0 for database installation orchestration
- Employs per-user database configuration with SQLite/ODBC connection management
- Supports x86 and x64 architecture builds
- Accesses SQLite through ODBC with atomic audit transactions
- Retains append-only product audit history
- Uses tagged Word content controls and late-bound automation for label population
- Hashes the maintenance password and gates write sessions

### Prerequisites

- .NET Framework 4.8 build environment
- x86 and x64 build support
- SQLite ODBC runtime (pinned version, digest-verified source)
- Waughtal Shell 1.4.0 for database installation orchestration
- Microsoft Word for label generation and printing
- Authenticode signing verification for distributed artifacts
- Windows release version resource enforcement
- Integration harness success status verification

### Branch/Worktree

- Branch: master
- Worktree: C:\Users\jmwau\source\repos\werm
- Head: aed7cb097a34c04b236a179a0cb8b4b017fc5a87
- Git submodule: wsp pinned at fa84ddedf37a4262ba0d21ffb95890a2d5a89d5c

### Retained Checks

- Real executable version assertions (no fabricated version output)
- Authenticode signing verification for distributed artifacts
- ODBC repository atomic audit transactions
- Windows release version resource enforcement
- Integration harness success status verification
- WSH path executable existence and version validation

### Acceptance

- PROJECT-WORK.md and AGENT-TASKS.md exist and contain template content derived from WSP-PROC-0012
- 3 untracked files preserved ($null, run_test.ps1, tools/run-tests.txt)
- Test-WermSqliteOdbc.ps1 real $WshPath --version line restored per audit requirement
- Build and test pipelines functional: `\\.\\tools\\Build-Werm.ps1` and `\\.\\tools\\Run-WermTests.ps1`
- Local git commit includes the two new/updated markdown files
- Remote branch SHA verified against upstream configuration

### Publication Target

- WPM package build after Release build
- Version: 0.1.0-dev
- x86 or x64 architecture selection
- Packaging builds pinned SQLite ODBC runtime from digest-verified source
- Includes versioned install/remove registration scripts

### Last Failure / Next Experiment

- Missing root work indexes (PROJECT-WORK.md, AGENT-TASKS.md) previously blocked adoption
- WSP-PROC-0012 adoption pending creation of project-owned records
- Next: populate PROJECT-WORK.md and AGENT-TASKS.md from WSP-PROC-0012 templates