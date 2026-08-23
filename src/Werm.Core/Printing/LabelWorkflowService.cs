using System;
using Werm.Core.Domain;
using Werm.Core.Persistence;

namespace Werm.Core.Printing
{
    public sealed class LabelWorkflowService
    {
        private readonly IWermDataStore _dataStore;
        private readonly ILabelPrintService _printService;

        public LabelWorkflowService(IWermDataStore dataStore, ILabelPrintService printService)
        {
            _dataStore = dataStore ?? throw new ArgumentNullException(nameof(dataStore));
            _printService = printService ?? throw new ArgumentNullException(nameof(printService));
        }

        public void PrintLabel(
            string productPlu,
            long customerId,
            string priceType,
            string templatePath,
            string printerName,
            int copies)
        {
            Product product = _dataStore.GetProduct(productPlu);
            if (product == null || !product.IsActive)
            {
                throw new InvalidOperationException("The selected active product was not found.");
            }
            Customer customer = _dataStore.GetCustomer(customerId);
            if (customer == null || !customer.IsActive)
            {
                throw new InvalidOperationException("The selected active customer was not found.");
            }
            CustomerProductPrice price = _dataStore.GetCustomerProductPrice(
                customerId, product.Plu, priceType);
            if (price == null || !price.IsActive)
            {
                throw new InvalidOperationException("The selected active customer price was not found.");
            }

            _printService.Print(new LabelPrintJob(
                templatePath,
                printerName,
                copies,
                LabelFieldMapper.Map(product, customer, price)));
        }
    }
}
