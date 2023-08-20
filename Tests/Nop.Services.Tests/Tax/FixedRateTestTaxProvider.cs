using System.Web.Routing;
using Nop.Core.Plugins;
using Nop.Services.Tax;
using System.Collections.Generic;
using Nop.Services.Customers;

namespace Nop.Services.Tests.Tax
{
    public class FixedRateTestTaxProvider : BasePlugin, ITaxProvider
    {
        public CalculateTaxResult GetTaxRate(CalculateTaxRequest calculateTaxRequest)
        {
            var result = new CalculateTaxResult
            {
                TaxRate = GetTaxRate(calculateTaxRequest.TaxCategoryId)
            };
            return result;
        }

        /// <summary>
        /// Gets a tax rate
        /// </summary>
        /// <param name="taxCategoryId">The tax category identifier</param>
        /// <returns>Tax rate</returns>
        protected decimal GetTaxRate(int taxCategoryId)
        {
            decimal rate = 10;
            return rate;
        }

        /// <summary>
        /// Gets a route for provider configuration
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = null;
            controllerName = null;
            routeValues = null;
        }


        //NEW
        public CalculateTaxResult GetTaxRate(CalculateTaxRequest calculateTaxRequest, bool IsAddToCartSelected)
        {
            decimal rate = 10;

            var result = new CalculateTaxResult();
            result.TaxRate = rate;

            return result;
        }

        public void PostDistributiveTaxRequest(Services.Orders.OrderProcessingService.PlaceOrderContainter details, Core.Domain.Orders.Order order, Services.Logging.ILogger logger, ITaxCategoryService tcService, IGlCodeService glService)
        {
            throw new System.NotImplementedException();
        }

        public void PostQuotationTaxRequestinBulk(System.Collections.Generic.IList<Core.Domain.Orders.ShoppingCartItem> cart, Services.Logging.ILogger logger, ITaxCategoryService service)
        {
            throw new System.NotImplementedException();
        }

        public void PostDistributiveTaxRefundRequest(Core.Domain.Orders.Order order, int orderItemId, Services.Logging.ILogger logger, bool isPartialRefund = false, decimal AmountToRefund1 = 0, decimal AmountToRefund2 = 0, decimal AmountToRefund3 = 0, int ProductId = 0, List<int> data = null)
        {
            //throw new System.NotImplementedException();
        }
        public void PostDistributiveFullTaxRefundRequestPerItem(Core.Domain.Orders.Order order, int orderItemId, Nop.Services.Logging.ILogger logger, bool isPartialRefund = false, decimal AmountToRefund1 = 0, decimal AmountToRefund2 = 0, decimal AmountToRefund3 = 0, int ProductId = 0, decimal taxAmoun1 = 0, decimal taxAmount2 = 0, decimal taxAmount3 = 0, decimal DeliveryAmount = 0, decimal DeliveryTax = 0, decimal ShippingAmount = 0, decimal ShippingTax = 0)
        {

        }

        //public System.Collections.Generic.List<Core.Domain.Tax.DynamicVertexGLCode> GetDynamicGLCodes(int orderId, int productId)
        //{
        //    throw new System.NotImplementedException();
        //}
    }
}
