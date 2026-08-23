using System.Collections.Generic;
using Werm.Core.Domain;

namespace Werm.Core.Persistence
{
    public interface IWermDataStore
    {
        Product GetProduct(string plu);
        Customer GetCustomer(long customerId);
        CustomerProductPrice GetCustomerProductPrice(
            long customerId,
            string productPlu,
            string priceType);
        IReadOnlyList<ProductAuditEvent> GetProductAuditHistory(string plu);
        bool SaveProduct(Product product, string changedBy, string changeReason);
        long SaveCustomer(Customer customer);
        bool SaveCustomerProductPrice(
            CustomerProductPrice price,
            string changedBy,
            string changeReason);
    }
}
