using System;
using System.Collections.Generic;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Tax;
using Nop.Services.Orders;

namespace Nop.Services.Tax
{
    /// <summary>
    /// Tax service
    /// </summary>
    public partial interface ITaxService
    {
        /// <summary>
        /// Load active tax provider
        /// </summary>
        /// <returns>Active tax provider</returns>
        ITaxProvider LoadActiveTaxProvider(int storeId);

        /// <summary>
        /// Load tax provider by system name
        /// </summary>
        /// <param name="systemName">System name</param>
        /// <returns>Found tax provider</returns>
        ITaxProvider LoadTaxProviderBySystemName(string systemName);

        /// <summary>
        /// Load all tax providers
        /// </summary>
        /// <returns>Tax providers</returns>
        IList<ITaxProvider> LoadAllTaxProviders();






        /// <summary>
        /// Gets price
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="price">Price</param>
        /// <param name="taxRate">Tax rate</param>
        /// <returns>Price</returns>
        decimal GetProductPrice(Product product, decimal price,
            out decimal taxRate);

        /// <summary>
        /// Gets price
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="price">Price</param>
        /// <param name="customer">Customer</param>
        /// <param name="taxRate">Tax rate</param>
        /// <returns>Price</returns>
        decimal GetProductPrice(Product product, decimal price,
            Customer customer, out decimal taxRate);

        /// <summary>
        /// Gets price
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="price">Price</param>
        /// <param name="includingTax">A value indicating whether calculated price should include tax</param>
        /// <param name="customer">Customer</param>
        /// <param name="taxRate">Tax rate</param>
        /// <returns>Price</returns>
        decimal GetProductPrice(Product product, decimal price,
            bool includingTax, Customer customer, bool orderPlaced, out decimal taxRate);

        /// <summary>
        /// Gets price
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="taxCategoryId">Tax category identifier</param>
        /// <param name="price">Price</param>
        /// <param name="includingTax">A value indicating whether calculated price should include tax</param>
        /// <param name="customer">Customer</param>
        /// <param name="priceIncludesTax">A value indicating whether price already includes tax</param>
        /// <param name="taxRate">Tax rate</param>
        /// <returns>Price</returns>
        decimal GetProductPrice(Product product, int taxCategoryId, decimal price,
            bool includingTax, Customer customer,
            bool priceIncludesTax, bool orderPlaced, out decimal taxRate);




        /// <summary>
        /// Gets shipping price
        /// </summary>
        /// <param name="price">Price</param>
        /// <param name="customer">Customer</param>
        /// <returns>Price</returns>
        decimal GetShippingPrice(decimal price, Customer customer);

        /// <summary>
        /// Gets shipping price
        /// </summary>
        /// <param name="price">Price</param>
        /// <param name="includingTax">A value indicating whether calculated price should include tax</param>
        /// <param name="customer">Customer</param>
        /// <returns>Price</returns>
        decimal GetShippingPrice(decimal price, bool includingTax, Customer customer);

        /// <summary>
        /// Gets shipping price
        /// </summary>
        /// <param name="price">Price</param>
        /// <param name="includingTax">A value indicating whether calculated price should include tax</param>
        /// <param name="customer">Customer</param>
        /// <param name="taxRate">Tax rate</param>
        /// <returns>Price</returns>
        decimal GetShippingPrice(decimal price, bool includingTax, Customer customer, out decimal taxRate);





        /// <summary>
        /// Gets payment method additional handling fee
        /// </summary>
        /// <param name="price">Price</param>
        /// <param name="customer">Customer</param>
        /// <returns>Price</returns>
        decimal GetPaymentMethodAdditionalFee(decimal price, Customer customer);

        /// <summary>
        /// Gets payment method additional handling fee
        /// </summary>
        /// <param name="price">Price</param>
        /// <param name="includingTax">A value indicating whether calculated price should include tax</param>
        /// <param name="customer">Customer</param>
        /// <returns>Price</returns>
        decimal GetPaymentMethodAdditionalFee(decimal price, bool includingTax, Customer customer);

        /// <summary>
        /// Gets payment method additional handling fee
        /// </summary>
        /// <param name="price">Price</param>
        /// <param name="includingTax">A value indicating whether calculated price should include tax</param>
        /// <param name="customer">Customer</param>
        /// <param name="taxRate">Tax rate</param>
        /// <returns>Price</returns>
        decimal GetPaymentMethodAdditionalFee(decimal price, bool includingTax, Customer customer, out decimal taxRate);


        void PostDistributiveTaxData(OrderProcessingService.PlaceOrderContainter details, Order order);

        void PostDistributiveTaxRefundData(Order order, int orderItemId=0, bool partialRefund = false, decimal AmountToRefund1 = 0, decimal AmountToRefund2 = 0, decimal AmountToRefund3 = 0, int ProductId = 0, List<int> data = null);
        void PostDistributiveFullTaxRefundDataPerOrderItem(Order order, int orderItemId = 0, bool partialRefund = false, decimal AmountToRefund1 = 0, decimal AmountToRefund2 = 0, decimal AmountToRefund3 = 0, int ProductId = 0, decimal taxAmoun1 = 0, decimal taxAmount2 = 0, decimal taxAmount3 = 0, decimal DeliveryAmount = 0, decimal DeliveryTax = 0, decimal ShippingAmount = 0, decimal ShippingTax = 0);

        //void PostQuotationBulkTaxData(IList<ShoppingCartItem> cart);

        //System.Collections.Generic.List<Core.Domain.Tax.DynamicVertexGLCode> GetDynamicGLCodes(int orderId, int productId);

        /// <summary>
        /// Gets checkout attribute value price
        /// </summary>
        /// <param name="cav">Checkout attribute value</param>
        /// <returns>Price</returns>
        decimal GetCheckoutAttributePrice(CheckoutAttributeValue cav);

        /// <summary>
        /// Gets checkout attribute value price
        /// </summary>
        /// <param name="cav">Checkout attribute value</param>
        /// <param name="customer">Customer</param>
        /// <returns>Price</returns>
        decimal GetCheckoutAttributePrice(CheckoutAttributeValue cav, Customer customer);

        /// <summary>
        /// Gets checkout attribute value price
        /// </summary>
        /// <param name="cav">Checkout attribute value</param>
        /// <param name="includingTax">A value indicating whether calculated price should include tax</param>
        /// <param name="customer">Customer</param>
        /// <returns>Price</returns>
        decimal GetCheckoutAttributePrice(CheckoutAttributeValue cav,
            bool includingTax, Customer customer);

        /// <summary>
        /// Gets checkout attribute value price
        /// </summary>
        /// <param name="cav">Checkout attribute value</param>
        /// <param name="includingTax">A value indicating whether calculated price should include tax</param>
        /// <param name="customer">Customer</param>
        /// <param name="taxRate">Tax rate</param>
        /// <returns>Price</returns>
        decimal GetCheckoutAttributePrice(CheckoutAttributeValue cav,
            bool includingTax, Customer customer, out decimal taxRate);






        /// <summary>
        /// Gets VAT Number status
        /// </summary>
        /// <param name="fullVatNumber">Two letter ISO code of a country and VAT number (e.g. GB 111 1111 111)</param>
        /// <returns>VAT Number status</returns>
        VatNumberStatus GetVatNumberStatus(string fullVatNumber);

        /// <summary>
        /// Gets VAT Number status
        /// </summary>
        /// <param name="fullVatNumber">Two letter ISO code of a country and VAT number (e.g. GB 111 1111 111)</param>
        /// <param name="name">Name (if received)</param>
        /// <param name="address">Address (if received)</param>
        /// <returns>VAT Number status</returns>
        VatNumberStatus GetVatNumberStatus(string fullVatNumber,
            out string name, out string address);
        /// <summary>
        /// Gets VAT Number status
        /// </summary>
        /// <param name="twoLetterIsoCode">Two letter ISO code of a country</param>
        /// <param name="vatNumber">VAT number</param>
        /// <returns>VAT Number status</returns>
        VatNumberStatus GetVatNumberStatus(string twoLetterIsoCode, string vatNumber);

        /// <summary>
        /// Gets VAT Number status
        /// </summary>
        /// <param name="twoLetterIsoCode">Two letter ISO code of a country</param>
        /// <param name="vatNumber">VAT number</param>
        /// <param name="name">Name (if received)</param>
        /// <param name="address">Address (if received)</param>
        /// <returns>VAT Number status</returns>
        VatNumberStatus GetVatNumberStatus(string twoLetterIsoCode, string vatNumber,
            out string name, out string address);

        /// <summary>
        /// Performs a basic check of a VAT number for validity
        /// </summary>
        /// <param name="twoLetterIsoCode">Two letter ISO code of a country</param>
        /// <param name="vatNumber">VAT number</param>
        /// <param name="name">Company name</param>
        /// <param name="address">Address</param>
        /// <param name="exception">Exception</param>
        /// <returns>VAT number status</returns>
        VatNumberStatus DoVatCheck(string twoLetterIsoCode, string vatNumber,
            out string name, out string address, out Exception exception);





        /// <summary>
        /// Gets a value indicating whether a product is tax exempt
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="customer">Customer</param>
        /// <returns>A value indicating whether a product is tax exempt</returns>
        bool IsTaxExempt(Product product, Customer customer);

        /// <summary>
        /// Gets a value indicating whether EU VAT exempt (the European Union Value Added Tax)
        /// </summary>
        /// <param name="address">Address</param>
        /// <param name="customer">Customer</param>
        /// <returns>Result</returns>
        bool IsVatExempt(Address address, Customer customer);

        ProductTax GetTaxRateBySessionIdProductIdTaxCode(int productId, string taxCode, string sessionId);

        /// <summary>
        /// NU-90
        /// Get the tax response storage
        /// </summary>
        /// <param name="taxRequestType"></param>
        /// <param name="orderId"></param>
        /// <param name="productId"></param>
        /// <param name="customerId"></param>
        /// <returns></returns>
        List<TaxResponseStorage> GetTaxResponseStorage(TaxRequestType taxRequestType, int? orderId = null, int? productId = null, int? customerId = null);
    }
}
