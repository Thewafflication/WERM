# WERM Project Process

**Content type:** Project process

**Status:** Proposed

## Purpose

This document maps the Waughtal Software Process (WSP) lifecycle to WERM. The
process is iterative; an activity may repeat whenever a finding or decision
changes an earlier artifact.

## Roles and Responsibilities

One person may hold multiple roles. Release approval remains a distinct
recorded decision even when roles overlap.

| Role | Responsibilities |
| --- | --- |
| Product owner | Scope, priorities, requirements acceptance, release approval |
| Engineering owner | Architecture, implementation, dependencies, configuration control |
| Verification owner | Test specifications, execution evidence, traceability, defect verification |
| Security owner | Credential design, database protection, security review and response |
| Release owner | Build identity, packaging, readiness record, publication and support baseline |
| Process owner | WSP adoption, retrospective, and approved process improvements |

## Lifecycle

### Adopt and Plan

The project pins WSP at `wsp/`, maintains the adoption record, and creates a
proportional plan for each milestone. A milestone plan identifies scope,
excluded work, dependencies, risks, completion criteria, reviews,
verification, and release effect using the WSP milestone work-plan structure.

### Specify

Stakeholder needs become controlled `REQ-NNNN` requirements in
`docs/requirements/`. Proposed requirements must identify unresolved details
before acceptance. Accepted requirements receive planned verification before
entering a release baseline.

### Design

Durable technical decisions use sequential ADRs in `docs/`. Design documents
describe the resulting components, interfaces, and data structures. A changed
accepted decision receives a superseding ADR instead of being silently
rewritten.

### Implement

Implementation proceeds in reviewable increments traceable to requirements and
design. Dependencies, generated content, configuration, and migrations remain
identifiable. Each increment must build before it enters verification.

### Review

Requirements, ADRs, design, source, migrations, tests, and release records
receive review proportional to their risk. Material findings remain open until
corrected, approved for deferral, or accepted as a recorded risk.
Reviews use the WSP review-record structure when their result supports a
milestone or release decision.

### Verify

Each accepted requirement maps to one or more controlled tests, inspections,
analyses, reviews, or demonstrations. Tests used as release evidence receive
`TC-NNNN` specifications. Execution records retain the tested source revision,
environment, result, and diagnostics.

### Release and Baseline

The release owner prepares a readiness record identifying the requirements
baseline, source revision, artifacts, dependencies, verification report,
unresolved issues, supported environment, and approval. A required gate with a
status other than Pass prevents an unqualified verified-release claim.

### Support and Improve

Reported defects and security findings record the affected release, impact,
owner, resolution, and verification. Each release or material process failure
receives a retrospective. Approved improvements update this process or are
proposed to WSP when generally reusable.

Each milestone receives a controlled closeout record. An optional work log may
retain a concise chronological execution history when it improves handoff or
reproducibility.

## Change and Defect Control

Changes to accepted requirements, ADRs, database schema, Word-template
contract, supported platform, or release gates require impact analysis. The
analysis considers dependent requirements, implementation, tests,
compatibility, security, schedule, and retained evidence.

The project issue mechanism remains to be selected. Until then, material open
items remain in the applicable controlled project document and must have an
owner and completion condition.

## Configuration and Records

Git controls requirements, designs, source, migrations, test specifications,
automation, and release records. Generated local output is not a controlled
record unless a release record explicitly retains it as evidence. The WSP
submodule commit and adoption record must agree.

## Milestone Completion

A milestone is complete only when its included work and completion criteria
are satisfied, required reviews have no unresolved material findings, planned
verification has passing evidence, and any deferral or accepted risk has an
owner, rationale, approval, impact, and review condition.

## References

- [WSP adoption record](wsp-adoption.md)
- [Milestone 0.1.0 plan](milestones/milestone-0.1.0.md)
- [Product requirements](requirements/README.md)
- [WSP software lifecycle](../wsp/processes/software-lifecycle.md)
- [WSP project process requirements](../wsp/processes/project-process.md)
- [WSP milestone work-plan template](../wsp/processes/milestone-plan-template.md)
- [WSP milestone closeout template](../wsp/processes/milestone-closeout-template.md)
- [WSP review-record template](../wsp/processes/review-record-template.md)
