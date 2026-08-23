# TC-0022: Missing Word Field Rejection

**Status:** Controlled

**Requirements:** `REQ-0004`

**Level:** Application-component test

## Objective

Attempt printing with a document that omits one required content-control tag.

## Expected Result

WERM raises a template-contract error before writing any control or printing,
then disposes the invalid working document.
