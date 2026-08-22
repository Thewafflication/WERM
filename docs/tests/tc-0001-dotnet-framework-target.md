# TC-0001: .NET Framework 4.8 Target Identity

**Status:** Controlled

**Requirement:** `REQ-0001`

**Level:** Build verification

## Objective

Verify that the compiled WERM core assembly declares .NET Framework 4.8 as its
target framework.

## Preconditions

- the selected x86 or x64 Debug solution build completed successfully; and
- `Werm.Tests.exe` and `Werm.Core.dll` are from the same source revision.

## Procedure

1. Run `tools/Run-WermTests.ps1` for the selected architecture with
   `-Configuration Debug -NoBuild`.
2. The runner loads the target-framework attribute from `Werm.Core.dll`.
3. Retain the generated `werm-test-results.xml` evidence.

## Expected Result

The attribute value is exactly `.NETFramework,Version=v4.8`; the test records
`PASS`, and the controlled runner returns zero when all cases pass.

## Postconditions

The source tree is unchanged. Test evidence remains under `out/test-results/`.
