# ADR-0001: Target .NET Framework 4.8

**Content type:** Architecture decision record

**Status:** Accepted

**Date:** 2026-08-22

## Context

WERM version 0.1.0 will be a Windows desktop application. The application
needs to integrate with an installed Microsoft Word desktop application and a
Windows label-printer driver. The target runtime must be selected before the
solution and deployment model are established.

## Decision Drivers

- The stakeholder specified .NET Framework 4.8.
- The application requires Windows desktop and Microsoft Office integration.
- The release must run in the intended existing Windows environment.

## Considered Options

1. .NET Framework 4.8
2. A current cross-platform .NET runtime
3. A non-.NET Windows desktop implementation

## Decision

WERM version 0.1.0 will target .NET Framework 4.8. This decision does not
select WPF or Windows Forms; the user-interface framework remains open.

## Rationale

.NET Framework 4.8 satisfies the stated deployment constraint and supports
the required Windows and Microsoft Office automation interfaces. Selecting a
newer or non-.NET runtime would depart from the stakeholder's required
platform without providing a necessary version 0.1.0 capability.

## Consequences

### Positive

- The solution can use established Windows desktop and Office automation APIs.
- The implementation matches the specified runtime environment.

### Negative

- The application is Windows-specific.
- Deployment requires .NET Framework 4.8 on each workstation.
- Libraries and tooling selected by the project must support this target.

### Follow-up

- Select and record the desktop user-interface framework.
- Define the supported Windows versions and deployment prerequisites.
- Verify the target framework from the project and built assembly metadata.

## References

- [REQ-0001: .NET Framework target](requirements/req-0001-dotnet-framework-target.md)
- [ADR-0003: Use Microsoft Word for label generation and printing](adr-0003-use-word-for-label-printing.md)
