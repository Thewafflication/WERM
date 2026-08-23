using System;
using System.Collections.Generic;
using System.Globalization;
using Werm.Core.Domain;

namespace Werm.Core.Printing
{
    public static class LabelFieldMapper
    {
        public static IDictionary<string, string> Map(
            Product product,
            Customer customer,
            CustomerProductPrice price)
        {
            if (product == null)
            {
                throw new ArgumentNullException(nameof(product));
            }
            if (customer == null)
            {
                throw new ArgumentNullException(nameof(customer));
            }
            if (price == null)
            {
                throw new ArgumentNullException(nameof(price));
            }
            if (customer.CustomerId != price.CustomerId ||
                !string.Equals(product.Plu, price.ProductPlu, StringComparison.Ordinal))
            {
                throw new ArgumentException(
                    "The selected product, customer, and price do not describe one label record.");
            }

            return new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [LabelFieldNames.ProductPlu] = product.Plu,
                [LabelFieldNames.ProductDescription] = product.Description,
                [LabelFieldNames.IngredientsStatement] = product.IngredientsStatement ?? string.Empty,
                [LabelFieldNames.SafeHandlingRequired] = product.SafeHandlingRequired ? "YES" : "NO",
                [LabelFieldNames.CustomerCode] = customer.CustomerCode,
                [LabelFieldNames.CustomerName] = customer.CustomerName,
                [LabelFieldNames.PriceAmount] = FormatPrice(price),
                [LabelFieldNames.PriceType] = price.PriceType,
                [LabelFieldNames.PriceBasis] = price.PriceBasis ?? string.Empty
            };
        }

        private static string FormatPrice(CustomerProductPrice price)
        {
            decimal amount = price.AmountMinorUnits / 100m;
            if (string.Equals(price.CurrencyCode, "USD", StringComparison.Ordinal))
            {
                return amount.ToString("C2", CultureInfo.GetCultureInfo("en-US"));
            }
            return amount.ToString("0.00", CultureInfo.InvariantCulture) + " " + price.CurrencyCode;
        }
    }
}
