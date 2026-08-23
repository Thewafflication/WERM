# Word Label Template Contract

**Applies to:** WERM 0.1.0

WERM populates a new Word document from the selected `.dotx`, `.dotm`, `.docx`,
or `.docm` file. Each mapped location must be a Word content control whose
`Tag` value exactly matches one of the identifiers below. Tags are
case-sensitive. The content-control title and displayed placeholder text are
not used for matching.

| Required tag | Source | Output rule |
| --- | --- | --- |
| `WERM.Product.PLU` | `Product.PLU` | Stored PLU text, including leading zeroes |
| `WERM.Product.Description` | `Product.Description` | Stored description |
| `WERM.Product.IngredientsStatement` | `Product.IngredientsStatement` | Stored text; blank when absent |
| `WERM.Product.SafeHandlingRequired` | `Product.SafeHandlingRequired` | `YES` or `NO` |
| `WERM.Customer.Code` | `Customer.CustomerCode` | Stored customer code |
| `WERM.Customer.Name` | `Customer.CustomerName` | Stored customer name |
| `WERM.Price.Amount` | Customer price amount and currency | USD as invariant U.S. currency, for example `$12.99`; other currencies as `12.99 CAD` |
| `WERM.Price.Type` | `CustomerProductPrice.PriceType` | Stored price-type text |
| `WERM.Price.Basis` | `CustomerProductPrice.PriceBasis` | Stored text; blank when absent |

All nine tags are required even when an optional database value is blank. A
template missing any required tag is rejected before WERM writes a field or
prints. A tag may occur more than once; WERM writes every matching content
control.

## Authoring checklist

1. Set the document page size, margins, orientation, and table/cell dimensions
   for the exact label stock and printer driver.
2. Insert a plain-text or rich-text content control at each mapped location.
3. In the content-control properties, set `Tag` to the exact controlled value.
4. Leave content editing unlocked. WERM temporarily unlocks contents while
   populating the working document.
5. Save the reviewed source template in a location readable by WERM operators.
6. Run the controlled physical-print test after any layout, Word, driver,
   printer, or stock change.

WERM creates no PDF, adds no barcode, does not save the populated working
document, and does not modify the source template during normal printing.
