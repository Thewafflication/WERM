# TC-0028: WPM Clean Install, Launch, and Removal

**Status:** Controlled

**Requirements:** `REQ-0001`, `REQ-0006`, `REQ-0020`, `REQ-0021`

**Level:** Automated installation and recovery test

**Priority:** Required release gate

**Design references:** ADR-0008, ADR-0009, package build script, workstation
configuration, and database installation instructions

**Technique:** Installation use-case, state-transition, and rollback testing

**Execution contract:** `.github/workflows/build.yml` invokes
`tools/Test-WermWpmInstall.ps1` on a fresh ephemeral runner for each package and
retains `out/integration-results/<architecture>/tc-0028.json`.

## Purpose

Verify the WPM package on a clean administrative Windows runner. The GitHub
host is ephemeral and administrator-enabled, so CI can safely prove the
machine-wide Program Files, registry, launch, and removal effects.

## Preconditions, environment, and assumptions

- A disposable clean Windows runner has pinned WPM 1.0.17 and no prior WERM
  installation or `WERM_HOME` value.
- The candidate x86 and x64 package filenames, source revision, and SHA-256
  digests match the readiness record.
- Administrator approval and a VM snapshot make changes recoverable.

## Inputs and initial state

Use each architecture-specific RC package. Record Windows, WPM, .NET Framework,
Word, and available ODBC identities. Start from a snapshot before each
architecture or restore an equivalent clean state.

## Procedure

1. Verify the package digest and inspect WPM metadata/source revision.
2. Install the candidate using WPM and record command, output, timestamps, exit
   status, installed files, and `WERM_HOME`.
3. Confirm `Werm.exe`, configuration, Core/Data/Printing DLLs, migration, WSH
   installer, PowerShell worker, README, and operating instructions are present.
4. Launch WERM hidden for three seconds and confirm it remains alive without an
   unhandled startup exception. Database behavior is proven independently by
   `TC-0025`; physical and interactive behavior remains under `TC-0026` and
   `TC-0027`.
5. Remove WERM using WPM and confirm the versioned directory and matching
   `WERM_HOME` value are removed without deleting the external database or
   template.
6. Repeat for x86 and x64 from a controlled clean state.

## Expected results and pass criteria

Both packages install to their declared architecture/version paths, expose the
correct source identity and complete payload, launch without an unhandled
exception, and remove cleanly. Removal preserves external data and templates.
No step requires a PDF or barcode component.

## Postconditions and cleanup

Retain WPM output, package digest, installed-file inventory, launch duration,
and removal evidence. The runner is destroyed after the job; preserve the
candidate package in the controlled artifact location.
