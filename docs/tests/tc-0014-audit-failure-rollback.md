# TC-0014: Audit Failure Rollback

**Status:** Controlled

**Requirement:** `REQ-0013`

**Level:** Data-component failure test

## Objective

Inject a failure into the first audit-change insert after the product and event
commands have succeeded.

## Expected Result

The data store propagates the failure, commits zero times, and rolls the shared
transaction back exactly once.
