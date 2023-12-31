﻿using System.Web.Routing;
using Nop.Core.Plugins;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Tax;
using System.Collections.Generic;
using Nop.Core.Domain.Tax;

namespace Nop.Plugin.Tax.FixedRate
{
    /// <summary>
    /// Fixed rate tax provider
    /// </summary>
    public class FixedRateTaxProvider : BasePlugin, ITaxProvider
    {
        private readonly ISettingService _settingService;

        public FixedRateTaxProvider(ISettingService settingService)
        {
            this._settingService = settingService;
        }
        
        /// <summary>
        /// Gets tax rate
        /// </summary>
        /// <param name="calculateTaxRequest">Tax calculation request</param>
        /// <returns>Tax</returns>
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
            var rate = this._settingService.GetSettingByKey<decimal>(string.Format("Tax.TaxProvider.FixedRate.TaxCategoryId{0}", taxCategoryId));
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
            actionName = "Configure";
            controllerName = "TaxFixedRate";
            routeValues = new RouteValueDictionary { { "Namespaces", "Nop.Plugin.Tax.FixedRate.Controllers" }, { "area", null } };
        }

        public override void Install()
        {
            //locales
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.FixedRate.Fields.TaxCategoryName", "Tax category");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.FixedRate.Fields.Rate", "Rate");
            
            base.Install();
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        public override void Uninstall()
        {
            //locales
            this.DeletePluginLocaleResource("Plugins.Tax.FixedRate.Fields.TaxCategoryName");
            this.DeletePluginLocaleResource("Plugins.Tax.FixedRate.Fields.Rate");
            
            base.Uninstall();
        }

        public CalculateTaxResult GetTaxRate(CalculateTaxRequest calculateTaxRequest, bool IsAddToCartSelected)
        {
            return GetTaxRate(calculateTaxRequest);
        }


        public void PostDistributiveTaxRequest(Services.Orders.OrderProcessingService.PlaceOrderContainter details, Core.Domain.Orders.Order order, Services.Logging.ILogger logger, ITaxCategoryService tcService, IGlCodeService glCodeService)
        {
            //do nothing
        }

        public void PostQuotationTaxRequestinBulk(System.Collections.Generic.IList<Core.Domain.Orders.ShoppingCartItem> cart, Services.Logging.ILogger logger, ITaxCategoryService service)
        {
            //do nothing
        }


        public System.Collections.Generic.List<Core.Domain.Tax.DynamicVertexGLCode> GetDynamicGLCodes(int orderId)
        {
            return new List<DynamicVertexGLCode>();
        }


        public void PostDistributiveTaxRefundRequest(Core.Domain.Orders.Order order, Services.Logging.ILogger logger)
        {
           //notta
        }
    }
}
