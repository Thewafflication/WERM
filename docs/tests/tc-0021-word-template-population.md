# TC-0021: Word Template Population and Print Arguments

**Status:** Controlled

**Requirements:** `REQ-0004`, `REQ-0005`

**Level:** Application-component test

## Objective

Populate a document double exposing every required content-control tag and
submit it to a selected printer.

## Expected Result

All nine controls receive the mapped values, one direct print call receives
the exact printer and copy count, and the document is disposed after success.
