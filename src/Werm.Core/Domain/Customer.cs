using System;

namespace Werm.Core.Domain
{
    public sealed class Customer
    {
        public Customer(long customerId, string customerCode, string customerName, bool isActive)
        {
            if (customerId < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(customerId));
            }

            CustomerId = customerId;
            CustomerCode = DomainText.Required(customerCode, nameof(customerCode));
            CustomerName = DomainText.Required(customerName, nameof(customerName));
            IsActive = isActive;
        }

        public long CustomerId { get; private set; }
        public string CustomerCode { get; private set; }
        public string CustomerName { get; private set; }
        public bool IsActive { get; private set; }
    }
}
