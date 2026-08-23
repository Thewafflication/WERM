# REQ-0003: Label Record Retrieval

**Content type:** Project requirement

**Status:** Accepted

**Source:** Stakeholder need

## Scope

This requirement applies when a user requests a label for a record available
in the configured SQLite database.

## Requirement

WERM shall retrieve the database values mapped to the label requested by the
user.

## Rationale

The application needs the selected database values before it can populate and
print a label.

## Verification

**Method:** Test

**References:** `TC-0020`, `TC-0023`, `TC-0027`

For a database containing a known record, request its label and compare every
retrieved mapped value with the expected database value.

## Relationships

- **Derived from:** Stakeholder need
- **Depends on:** [REQ-0002](req-0002-sqlite-data-source.md)
- **Conflicts with:** None known

## Tailoring

None.

## Implementation Record

`LabelWorkflowService` retrieves the active product, customer, and exact
customer-product price selected by PLU, customer ID, and price type. It rejects
missing or inactive records before creating the print job.
