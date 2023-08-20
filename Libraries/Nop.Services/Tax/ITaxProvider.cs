using System.Web.Routing;
using Nop.Core.Domain.Orders;
using Nop.Core.Plugins;
using Nop.Services.Orders;
using System.Collections.Generic;
using Nop.Services.Logging;
using Nop.Core.Domain.Tax;
using Nop.Services.Customers;

namespace Nop.Services.Tax
{
    /// <summary>
    /// Provides an interface for creating tax providers
    /// </summary>
    public partial interface ITaxProvider : IPlugin
    {
        /// <summary>
        /// Gets tax rate
        /// </summary>
        /// <param name="calculateTaxRequest">Tax calculation request</param>
        /// <returns>Tax</returns>
        CalculateTaxResult GetTaxRate(CalculateTaxRequest calculateTaxRequest, bool IsAddToCartSelected);

        /// <summary>
        /// Gets a route for provider configuration
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues);

        //void ProcessDistributiveTaxRequest(OrderProcessingService.PlaceOrderContainter details, Order order);


        void PostDistributiveTaxRequest(OrderProcessingService.PlaceOrderContainter details, Order order, ILogger logger, ITaxCategoryService tcService, IGlCodeService glService);

        // void PostQuotationTaxRequestinBulk(IList<ShoppingCartItem> cart, ILogger logger, ITaxCategoryService service);

        void PostDistributiveTaxRefundRequest(Order order, int orderItemId, ILogger logger,bool partialRefund, decimal AmountToRefund1 = 0, decimal AmountToRefund2 = 0, decimal AmountToRefund3 = 0, int ProductId = 0, List<int> data = null);

        void PostDistributiveFullTaxRefundRequestPerItem(Order order, int orderItemId,  ILogger logger, bool isPartialRefund = false, decimal AmountToRefund1 = 0, decimal AmountToRefund2 = 0, decimal AmountToRefund3 = 0, int ProductId = 0, decimal taxAmoun1 = 0, decimal taxAmount2 = 0, decimal taxAmount3 = 0, decimal DeliveryAmount = 0, decimal DeliveryTax = 0, decimal ShippingAmount = 0, decimal ShippingTax = 0);
        //List<DynamicVertexGLCode> GetDynamicGLCodes(int orderId, int productId);
    }
}
