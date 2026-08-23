# TC-0003: Explicit Process Architecture

**Status:** Controlled

**Decision:** `ADR-0008`

**Level:** Build verification

**Priority:** Required release gate

**Implementation:** `tests/Werm.Tests/Program.cs`

**Execution contract:** [Automated controlled-test execution contract](automated-test-execution-contract.md)

**Technique:** Decision-based configuration coverage

## Objective

Verify that the selected x86 or x64 solution target produces a process with
that explicit architecture rather than an AnyCPU executable with
environment-dependent bitness.

## Procedure

Run `tools/Run-WermTests.ps1` with `-Architecture x86` and again with
`-Architecture x64`. The wrapper supplies the selected architecture to the
controlled executable.

## Expected Result

The process architecture equals the requested architecture in both executions,
each execution records `PASS`, and the XML evidence identifies the actual
process architecture.
