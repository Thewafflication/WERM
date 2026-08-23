# TC-0027: Word and Physical Label-Printer Integration

**Status:** Controlled

**Requirements:** `REQ-0003`, `REQ-0004`, `REQ-0005`, `REQ-0006`

**Level:** Manual system and physical-output test

**Priority:** Required release gate

**Design references:** ADR-0003, ADR-0012, Word template contract, and
workstation configuration

**Technique:** End-to-end use-case testing, checklist-based physical
inspection, and controlled error guessing

## Purpose

Verify behavior that a document double cannot establish: actual Word COM
automation, template rendering, printer-driver layout, label stock, spooling,
and process cleanup. These observations require the controlled physical setup.

## Preconditions, environment, and assumptions

- `TC-0025` and `TC-0026` have passed for the selected WERM architecture.
- Exact Windows, .NET Framework, Word, ODBC, printer-driver, printer, and stock
  identities are recorded and have matching application bitness where needed.
- A reviewed representative template contains all nine required tags.
- Printer safety, stock loading, and operator authorization are established.

## Inputs and initial state

Use the controlled product/customer/price from `TC-0026`, including leading
zeroes, ingredients, safe-handling `YES`, `$12.99`, price type, and basis. Record
the source template digest and confirm no unrelated `WINWORD.EXE` process is
running.

## Procedure

1. In the Print tab, select the controlled PLU, customer ID, price type,
   template, physical label printer, and two copies.
2. Submit the label and retain the WERM status, Windows print-queue evidence,
   and both physical labels.
3. Inspect every mapped value, page size, orientation, margins, clipping,
   alignment, legibility, and copy count against the approved sample.
4. Confirm the template digest is unchanged, no populated or PDF intermediate
   is retained, and Word exits after synchronous printing.
5. Attempt a controlled template with one missing tag and confirm rejection
   before spooling; verify the source remains unchanged and Word exits.
6. Exercise one safe approved print-error condition and retain the error and
   cleanup observation.

## Expected results and pass criteria

Both physical labels contain the exact nine mapped values and match the
approved layout. The selected printer receives exactly two copies. WERM creates
no PDF, preserves both templates, rejects the invalid template before printing,
reports errors usefully, restores Word's prior printer, and leaves no orphaned
Word process after success or failure.

## Postconditions and cleanup

Restore the printer to its pre-test state, remove disposable invalid templates,
retain the approved template digest, physical samples, screenshots, queue
record, and dependency inventory, and preserve the disposable database.
