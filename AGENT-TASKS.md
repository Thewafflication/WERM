# AGENT-TASKS.md

**Project:** Waughtal Enterprise Resource Management (WERM)

**Version:** 0.1.0

**Branch/Worktree:** master

**Status:** Adopted from WSP-PROC-0012

## Agent Task Tracking

This record tracks individual agent tasks, their status, prerequisites, and acceptance criteria for the WERM project.

### Current Task

**Task:** Adopt WSP-PROC-0012 project-work creation

**Status:** In progress

**Description:** Create PROJECT-WORK.md and AGENT-TASKS.md from WSP-PROC-0012 templates without overwriting existing files. Populate project-owned records from actual requirements, code/tests and unfinished work.

**Prerequisites:**
- WSP-PROC-0012 reference: https://github.com/Thewafflication/wsp commit fa84ddedf37a4262ba0d21ffb95890a2d5a89d5c
- WSP templates in wsp/templates/ directory
- Existing repo structure and untracked files preserved

**Acceptance Criteria:**
- PROJECT-WORK.md exists with template-derived content
- AGENT-TASKS.md exists with template-derived content
- 3 untracked files preserved ($null, run_test.ps1, tools/run-tests.txt)
- Test-WermSqliteOdbc.ps1 real $WshPath --version line restored per audit requirement
- Local git commit includes the two new markdown files

### Completed Work

- Verified repo root lacks PROJECT-WORK.md and AGENT-TASKS.md (initially)
- Confirmed wsp/templates/ contains 3 candidate template files
- Restored real $WshPath --version in Test-WermSqliteOdbc.ps1 (per audit requirement)
- Documented 3 untracked files from prior git status

### Pending Work

- Verify git status after populating markdowns
- Make local git commit with the two new markdown files
- Push validated worker commits to configured upstream
- Verify remote branch SHA

### Agent Notes

- Preserve all unrelated owner work and existing acceptance gates
- Do not force push, rewrite history, publish unrelated owner changes, tag, release or install this project
- Avoid repeated broad discovery and do not duplicate an active worker
- Report actual change, validation commands/results, commit, pushed remote branch/SHA and remaining work