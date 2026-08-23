# REQ-0001: .NET Framework Target

**Content type:** Project requirement

**Status:** Accepted

**Source:** Stakeholder decision and ADR-0001

## Scope

This requirement applies to WERM version 0.1.0 application assemblies.

## Requirement

WERM version 0.1.0 shall target .NET Framework 4.8.

## Rationale

The stakeholder selected .NET Framework 4.8 as the application runtime.

## Verification

**Method:** Inspection

**References:** `TC-0001`, `TC-0002`

Inspect the project configuration and built assembly metadata. Verification
passes when each shipped application assembly targets .NET Framework 4.8.

## Relationships

- **Derived from:** Stakeholder decision
- **Depends on:** [ADR-0001](../adr-0001-target-dotnet-framework-4-8.md)
- **Conflicts with:** None known

## Tailoring

None.

## Implementation Record

All solution assemblies target .NET Framework 4.8. The controlled runner
inspects the compiled target-framework identity on both supported
architectures.
