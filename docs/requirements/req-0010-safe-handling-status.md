# REQ-0010: Safe-Handling Status

**Content type:** Project requirement

**Status:** Accepted

**Source:** Stakeholder need

## Scope

This requirement applies to every product stored in the WERM product database.

## Requirement

WERM shall record whether each product requires safe-handling information.

## Rationale

The label workflow must be able to determine whether safe-handling information
applies to the product.

## Verification

**Method:** Test

**References:** `TC-0005`, `TC-0007`

Save and reload products representing both supported states. Verification
passes when the retrieved state matches the saved state in each case.

## Relationships

- **Derived from:** Stakeholder need
- **Depends on:** [REQ-0007](req-0007-unique-product-plu.md)
- **Conflicts with:** None known

## Tailoring

None.

## Implementation Record

The schema defines `Product.SafeHandlingRequired` as a required Boolean value
represented by zero or one. The core `Product` model exposes the value as a
Boolean.
