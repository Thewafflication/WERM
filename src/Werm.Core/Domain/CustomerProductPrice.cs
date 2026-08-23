using System;

namespace Werm.Core.Domain
{
    public sealed class CustomerProductPrice
    {
        public CustomerProductPrice(
            long customerId,
            string productPlu,
            string priceType,
            long amountMinorUnits,
            string currencyCode,
            string priceBasis,
            bool isActive)
        {
            if (customerId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(customerId));
            }
            if (amountMinorUnits < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amountMinorUnits));
            }

            string normalizedCurrency = DomainText.Required(currencyCode, nameof(currencyCode))
                .ToUpperInvariant();
            if (normalizedCurrency.Length != 3)
            {
                throw new ArgumentException(
                    "Currency codes must contain exactly three characters.",
                    nameof(currencyCode));
            }

            CustomerId = customerId;
            ProductPlu = DomainText.Required(productPlu, nameof(productPlu));
            PriceType = DomainText.Required(priceType, nameof(priceType));
            AmountMinorUnits = amountMinorUnits;
            CurrencyCode = normalizedCurrency;
            PriceBasis = DomainText.Optional(priceBasis);
            IsActive = isActive;
        }

        public long CustomerId { get; private set; }
        public string ProductPlu { get; private set; }
        public string PriceType { get; private set; }
        public long AmountMinorUnits { get; private set; }
        public string CurrencyCode { get; private set; }
        public string PriceBasis { get; private set; }
        public bool IsActive { get; private set; }
    }
}
