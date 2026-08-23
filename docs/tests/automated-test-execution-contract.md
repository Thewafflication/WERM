# Automated Controlled-Test Execution Contract

**Applies to:** `TC-0001` through `TC-0024`

This controlled contract supplies the common test-case content incorporated by
reference into each automated WERM specification. A case-specific document's
objective, preconditions, procedure, expected result, or postcondition takes
precedence when it is more specific.

## Design and implementation references

- `tests/Werm.Tests/Program.cs` contains the controlled case registration,
  deterministic fixtures, procedure implementation, and assertions.
- `tools/Run-WermTests.ps1` validates the selected binary and architecture,
  isolates the result location, passes baseline metadata, and returns the
  runner's status.
- `.github/workflows/build.yml` builds and runs the Debug x86/x64 matrix and
  retains binaries, symbols, dependencies, and XML evidence for 30 days.

## Preconditions, environment, and assumptions

- The repository and WSP submodule are at the test baseline identified by the
  execution record.
- A clean Debug solution build for the selected x86 or x64 architecture has
  completed with .NET Framework 4.8 build tools.
- The runner, application assemblies, migration, and scripts come from the same
  source revision.
- Tests use deterministic in-process doubles unless the case explicitly names
  an external process or filesystem fixture. Microsoft Word, a live ODBC
  driver, and a physical printer are not assumed for `TC-0001`–`TC-0024`.

## Inputs and initial state

The runner receives `--expected-architecture`, `--repository-root`,
`--source-revision`, and a new XML result path. Each case creates its stated
objects or disposable work location from controlled values in
`tests/Werm.Tests/Program.cs`; it does not consume production database data.

## Procedure and objective pass criteria

1. Run `tools/Run-WermTests.ps1` for the selected architecture and Debug
   configuration after its matching build.
2. The runner executes every registered case exactly once and compares actual
   results with the case-specific expected result.
3. The case passes only when every assertion completes without an exception.
4. The execution passes only when all required cases pass, the runner exits
   zero, and XML contains the exact source revision, architecture, case IDs,
   verdicts, timestamps, and diagnostics.

The test-design technique is a requirements-based scenario using equivalence
partitioning, boundary values, decision-table combinations, or fault injection
where named by the case objective and controlled fixture.

## Postconditions and cleanup

Production data and configuration remain unchanged. Disposable work is removed
by the case that created it. Machine-readable evidence remains only at the
declared `out/test-results/<architecture>/Debug/` path and in the corresponding
GitHub Actions artifact.
