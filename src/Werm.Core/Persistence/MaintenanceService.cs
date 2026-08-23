using System;
using System.Collections.Generic;
using Werm.Core.Domain;
using Werm.Core.Security;

namespace Werm.Core.Persistence
{
    public sealed class MaintenanceService
    {
        private readonly IWermDataStore _dataStore;
        private readonly MaintenanceAuthorizer _authorizer;

        public MaintenanceService(IWermDataStore dataStore, MaintenanceAuthorizer authorizer)
        {
            _dataStore = dataStore ?? throw new ArgumentNullException(nameof(dataStore));
            _authorizer = authorizer ?? throw new ArgumentNullException(nameof(authorizer));
        }

        public Product GetProduct(string plu)
        {
            return _dataStore.GetProduct(plu);
        }

        public Customer GetCustomer(long customerId)
        {
            return _dataStore.GetCustomer(customerId);
        }

        public CustomerProductPrice GetCustomerProductPrice(
            long customerId,
            string productPlu,
            string priceType)
        {
            return _dataStore.GetCustomerProductPrice(customerId, productPlu, priceType);
        }

        public IReadOnlyList<ProductAuditEvent> GetProductAuditHistory(string plu)
        {
            return _dataStore.GetProductAuditHistory(plu);
        }

        public bool SaveProduct(
            MaintenanceSession session,
            Product product,
            string changeReason)
        {
            string changedBy = _authorizer.DemandAuthorized(session);
            return _dataStore.SaveProduct(product, changedBy, changeReason);
        }

        public long SaveCustomer(MaintenanceSession session, Customer customer)
        {
            _authorizer.DemandAuthorized(session);
            return _dataStore.SaveCustomer(customer);
        }

        public bool SaveCustomerProductPrice(
            MaintenanceSession session,
            CustomerProductPrice price,
            string changeReason)
        {
            string changedBy = _authorizer.DemandAuthorized(session);
            return _dataStore.SaveCustomerProductPrice(price, changedBy, changeReason);
        }
    }
}
