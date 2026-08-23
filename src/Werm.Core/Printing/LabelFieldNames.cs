using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Werm.Core.Printing
{
    public static class LabelFieldNames
    {
        public const string ProductPlu = "WERM.Product.PLU";
        public const string ProductDescription = "WERM.Product.Description";
        public const string IngredientsStatement = "WERM.Product.IngredientsStatement";
        public const string SafeHandlingRequired = "WERM.Product.SafeHandlingRequired";
        public const string CustomerCode = "WERM.Customer.Code";
        public const string CustomerName = "WERM.Customer.Name";
        public const string PriceAmount = "WERM.Price.Amount";
        public const string PriceType = "WERM.Price.Type";
        public const string PriceBasis = "WERM.Price.Basis";

        public static readonly ReadOnlyCollection<string> Required =
            new ReadOnlyCollection<string>(new List<string>
            {
                ProductPlu,
                ProductDescription,
                IngredientsStatement,
                SafeHandlingRequired,
                CustomerCode,
                CustomerName,
                PriceAmount,
                PriceType,
                PriceBasis
            });
    }
}
