﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Routing;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using common = Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Tax;
using Nop.Core.Plugins;
using Nop.Plugin.Tax.Vertex.Domain;
using Nop.Plugin.Tax.Vertex.Helpers;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Tax;
using Nop.Plugin.Tax.Vertex.Services;
using Nop.Core.Infrastructure;
using Nop.Services.Events;
using Nop.Core.Events;
using Nop.Services.Customers;
using Nop.Core.Domain.Discounts;
using Nop.Services.Discounts;
using System.Web;

namespace Nop.Plugin.Tax.Vertex
{
    /// <summary>
    /// Represents Vertex tax provider
    /// </summary>
    public class VertexTaxProvider : BasePlugin, ITaxProvider,
        IConsumer<EntityUpdated<ShoppingCartItem>>,
        IConsumer<EntityInserted<ShoppingCartItem>>,
        IConsumer<EntityDeleted<ShoppingCartItem>>
    {
        #region Constants

        /// <summary>
        /// Key for caching tax rate for certain address
        /// </summary>
        /// <remarks>
        /// {0} - Address
        /// {1} - City
        /// {2} - StateProvinceId
        /// {3} - CountryId
        /// {4} - ZipPostalCode
        /// </remarks>
        private const string TAXRATE_KEY = "Nop.taxrate.id-{0}-{1}-{2}-{3}-{4}-{5}";

        #endregion

        #region Fields

        private readonly VertexTaxSettings _vertexTaxSettings;
        private readonly IAddressService _addressService;
        private readonly ICacheManager _cacheManager;
        private readonly ICheckoutAttributeParser _checkoutAttributeParser;
        private readonly ICountryService _countryService;
        private readonly IGeoLookupService _geoLookupService;
        private readonly ILogger _logger;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly ITaxCategoryService _taxCategoryService;
        private readonly TaxSettings _taxSettings;
        private readonly IGlCodeService _glcodeService;
        private readonly ITaxService _taxDataService;
        private readonly IDiscountService _discountService;
        private readonly HttpContextBase _httpContext;


        #endregion

        #region Ctor

        public VertexTaxProvider(VertexTaxSettings VertexTaxSettings,
            IAddressService addressService,
            ICacheManager cacheManager,
            ICheckoutAttributeParser checkoutAttributeParser,
            ICountryService countryService,
            IGeoLookupService geoLookupService,
            ILogger logger,
            IProductAttributeParser productAttributeParser,
            ISettingService settingService,
            IStoreContext storeContext,
            ITaxCategoryService taxCategoryService,
            TaxSettings taxSettings,
            IGlCodeService glcodeService,
            ITaxService taxDataService,
            IDiscountService discountService,
            HttpContextBase httpContext)
        {
            this._vertexTaxSettings = VertexTaxSettings;
            this._addressService = addressService;
            this._cacheManager = cacheManager;
            this._checkoutAttributeParser = checkoutAttributeParser;
            this._countryService = countryService;
            this._geoLookupService = geoLookupService;
            this._logger = logger;
            this._productAttributeParser = productAttributeParser;
            this._settingService = settingService;
            this._storeContext = storeContext;
            this._taxCategoryService = taxCategoryService;
            this._taxSettings = taxSettings;
            this._glcodeService = glcodeService;
            this._taxDataService = taxDataService;
            this._discountService = discountService;
            this._httpContext = httpContext;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Get address dictionary
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>Dictionary of used addresses</returns>
        protected IDictionary<string, common.Address> GetAddressDictionary(Order order)
        {
            //get destination address
            var destinationAddress = GetDestinationAddress(order);

            //add destination, origin and billing addresses to dictionary
            return new Dictionary<string, common.Address>
            {
                { "origin", GetOriginAddress(order.StoreId) ?? destinationAddress },
                { "destination", destinationAddress },
                { "billing", order.BillingAddress },
            };

        }

        /// <summary>
        /// Get a tax destination address
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>Address</returns>
        protected common.Address GetDestinationAddress(Order order)
        {
            common.Address destinationAddress = null;

            //tax is based on billing address
            if (_taxSettings.TaxBasedOn == TaxBasedOn.BillingAddress)
                destinationAddress = order.BillingAddress;

            //tax is based on shipping address
            if (_taxSettings.TaxBasedOn == TaxBasedOn.ShippingAddress)
                destinationAddress = order.ShippingAddress;

            //or use default address for tax calculation
            if (destinationAddress == null)
                destinationAddress = _addressService.GetAddressById(_taxSettings.DefaultTaxAddressId);

            return destinationAddress;
        }

        /// <summary>
        /// Get a tax origin address
        /// </summary>
        /// <param name="storeId">Store identifier</param>
        /// <returns>Address</returns>
        protected common.Address GetOriginAddress(int storeId)
        {
            //load settings for shipping origin address identifier
            var originAddressId = _settingService.GetSettingByKey<int>("ShippingSettings.ShippingOriginAddressId",
                storeId: storeId, loadSharedValueIfNotFound: true);

            //try get address that will be used for tax origin 
            return _addressService.GetAddressById(originAddressId);
        }

        /// <summary>
        /// Get item lines for the tax request
        /// </summary>
        /// <param name="order">Order</param>
        /// <param name="addresses">Address dictionary</param>
        /// <returns>List of item lines</returns>
        protected IList<Line> GetItemLines(Order order, IDictionary<string, common.Address> addresses)
        {
            //get destination and origin address codes
            var destinationCode = addresses["destination"].Return(address => address.Id.ToString(), null);
            var originCode = addresses["origin"].Return(address => address.Id.ToString(), null);

            //get purchased products details
            var items = CreateLinesForOrderItems(order, destinationCode, originCode).ToList();

            //set payment method additional fee as the separate item line
            if (order.PaymentMethodAdditionalFeeExclTax > decimal.Zero)
                items.Add(CreateLineForPaymentMethod(order, destinationCode, originCode));

            //set shipping rate as the separate item line
            if (order.OrderShippingExclTax > decimal.Zero)
                items.Add(CreateLineForShipping(order, destinationCode, originCode));

            //set checkout attributes as the separate item lines
            if (!string.IsNullOrEmpty(order.CheckoutAttributesXml))
                items.AddRange(CreateLinesForCheckoutAttributes(order, destinationCode, originCode));

            return items;
        }

        /// <summary>
        /// Create item lines for the tax request from order items
        /// </summary>
        /// <param name="order">Order</param>
        /// <param name="destinationCode">Destination address code</param>
        /// <param name="originCode">Origin address code</param>
        /// <returns>Collection of item lines</returns>
        protected IEnumerable<Line> CreateLinesForOrderItems(Order order, string destinationCode, string originCode)
        {
            return order.OrderItems.Select(orderItem =>
            {
                //create line
                var item = new Line
                {
                    Amount = orderItem.PriceExclTax,
                    DestinationCode = destinationCode,
                    Discounted = order.OrderSubTotalDiscountExclTax > decimal.Zero,
                    LineNo = orderItem.Id.ToString(),
                    OriginCode = originCode,
                    Qty = orderItem.Quantity
                };

                //whether to override destination address code
                if (UseEuVatRules(order, orderItem.Product))
                {
                    //use the billing address as the destination one in accordance with the EU VAT rules
                    item.DestinationCode = order.BillingAddress.Return(address => address.Id.ToString(), null);
                }

                //set SKU as item code
                item.ItemCode = orderItem.Product.Return(product => product.FormatSku(orderItem.AttributesXml, _productAttributeParser), null);

                //item description
                item.Description = orderItem.Product.Return(product =>
                    !string.IsNullOrEmpty(product.ShortDescription) ? product.ShortDescription : product.Name, null);

                //set tax category as tax code
                var productTaxCategory = _taxCategoryService.GetTaxCategoryById(orderItem.Product.Return(product => product.TaxCategoryId, 0));
                item.TaxCode = productTaxCategory.Return(taxCategory => taxCategory.Name, null);

                ////try get product exemption
                //item.ExemptionNo = orderItem.Product != null && orderItem.Product.IsTaxExempt
                //    ? string.Format("Product-{0}", orderItem.Product.Id) : null;

                //as written in AvaTax documentation: You can find ExemptionNo in the GetTaxRequest at the document and line level.
                //but in fact, when you try to use it on line level you get the error: "Malformed JSON near 'ExemptionNo'"
                //so set CustomerUsageType to "L" - exempt by nopCommerce reason, it will enable the tax exempt
                if (orderItem.Product.Return(product => product.IsTaxExempt, false))
                    item.CustomerUsageType = CustomerUsageType.L;

                return item;
            });
        }

        /// <summary>
        /// Create item line for the tax request from payment method additional fee
        /// </summary>
        /// <param name="order">Order</param>
        /// <param name="destinationCode">Destination address code</param>
        /// <param name="originCode">Origin address code</param>
        /// <returns>Item line</returns>
        protected Line CreateLineForPaymentMethod(Order order, string destinationCode, string originCode)
        {
            //create line
            var paymentItem = new Line
            {
                Amount = order.PaymentMethodAdditionalFeeExclTax,
                Description = "Payment method additional fee",
                DestinationCode = destinationCode,
                ItemCode = order.PaymentMethodSystemName,
                LineNo = "payment",
                OriginCode = originCode,
                Qty = 1
            };

            //whether payment is taxable
            if (_taxSettings.PaymentMethodAdditionalFeeIsTaxable)
            {
                //try get tax code
                var paymentTaxCategory = _taxCategoryService.GetTaxCategoryById(_taxSettings.PaymentMethodAdditionalFeeTaxClassId);
                paymentItem.TaxCode = paymentTaxCategory.Return(taxCategory => taxCategory.Name, null);
            }
            else
            {
                //if payment was already taxed, set as exempt
                //paymentItem.ExemptionNo = "Payment-fee";

                //as written in AvaTax documentation: You can find ExemptionNo in the GetTaxRequest at the document and line level.
                //but in fact, when you try to use it on line level you get the error: "Malformed JSON near 'ExemptionNo'"
                //so set CustomerUsageType to "L" - exempt by nopCommerce reason, it will enable the tax exempt
                paymentItem.CustomerUsageType = CustomerUsageType.L;
            }

            return paymentItem;
        }

        /// <summary>
        /// Create item line for the tax request from shipping rate
        /// </summary>
        /// <param name="order">Order</param>
        /// <param name="destinationCode">Destination address code</param>
        /// <param name="originCode">Origin address code</param>
        /// <returns>Item line</returns>
        protected Line CreateLineForShipping(Order order, string destinationCode, string originCode)
        {
            //create line
            var shippingItem = new Line
            {
                Amount = order.OrderShippingExclTax,
                Description = "Shipping rate",
                DestinationCode = destinationCode,
                ItemCode = order.ShippingMethod,
                LineNo = "shipping",
                OriginCode = originCode,
                Qty = 1
            };

            //whether shipping is taxable
            if (_taxSettings.ShippingIsTaxable)
            {
                //try get tax code
                var shippingTaxCategory = _taxCategoryService.GetTaxCategoryById(_taxSettings.ShippingTaxClassId);
                shippingItem.TaxCode = shippingTaxCategory.Return(taxCategory => taxCategory.Name, null);
            }
            else
            {
                //if shipping was already taxed, set as exempt
                //shippingItem.ExemptionNo = "Shipping-rate";

                //as written in AvaTax documentation: You can find ExemptionNo in the GetTaxRequest at the document and line level.
                //but in fact, when you try to use it on line level you get the error: "Malformed JSON near 'ExemptionNo'"
                //so set CustomerUsageType to "L" - exempt by nopCommerce reason, it will enable the tax exempt
                shippingItem.CustomerUsageType = CustomerUsageType.L;
            }

            return shippingItem;
        }

        /// <summary>
        /// Create item lines for the tax request from checkout attributes
        /// </summary>
        /// <param name="order">Order</param>
        /// <param name="destinationCode">Destination address code</param>
        /// <param name="originCode">Origin address code</param>
        /// <returns>Collection of item lines</returns>
        protected IEnumerable<Line> CreateLinesForCheckoutAttributes(Order order, string destinationCode, string originCode)
        {
            //get checkout attributes values
            var attributeValues = _checkoutAttributeParser.ParseCheckoutAttributeValues(order.CheckoutAttributesXml);

            return attributeValues.Where(attributeValue => attributeValue.CheckoutAttribute != null).Select(attributeValue =>
            {
                //create line
                var checkoutAttributeItem = new Line
                {
                    Amount = attributeValue.PriceAdjustment,
                    Description = string.Format("{0} ({1})", attributeValue.CheckoutAttribute.Name, attributeValue.Name),
                    DestinationCode = destinationCode,
                    Discounted = order.OrderSubTotalDiscountExclTax > decimal.Zero,
                    ItemCode = string.Format("{0}-{1}", attributeValue.CheckoutAttribute.Name, attributeValue.Name),
                    LineNo = string.Format("Checkout-{0}", attributeValue.CheckoutAttribute.Name),
                    OriginCode = originCode,
                    Qty = 1
                };

                //whether checkout attribute is tax exempt
                if (attributeValue.CheckoutAttribute.IsTaxExempt)
                {
                    //set item as exempt
                    //checkoutAttributeItem.ExemptionNo = "Checkout-attribute";

                    //as written in AvaTax documentation: You can find ExemptionNo in the GetTaxRequest at the document and line level.
                    //but in fact, when you try to use it on line level you get the error: "Malformed JSON near 'ExemptionNo'"
                    //so set CustomerUsageType to "L" - exempt by nopCommerce reason, it will enable the tax exempt
                    checkoutAttributeItem.CustomerUsageType = CustomerUsageType.L;
                }
                else
                {
                    //try get tax code
                    var attributeTaxCategory = _taxCategoryService.GetTaxCategoryById(attributeValue.CheckoutAttribute.TaxCategoryId);
                    checkoutAttributeItem.TaxCode = attributeTaxCategory.Return(taxCategory => taxCategory.Name, null);
                }

                return checkoutAttributeItem;
            });
        }

        /// <summary>
        /// Get a value whether need to use European Union VAT rules for tax calculation
        /// </summary>
        /// <param name="order">Order</param>
        /// <param name="purchasedProduct">Purchased product</param>
        /// <returns>True if need to use EU VAT rules; otherwise false</returns>
        protected bool UseEuVatRules(Order order, Product purchasedProduct)
        {
            //whether EU VAT rules enabled and purchased product belongs to the telecommunications, broadcasting and electronic services
            return _taxSettings.EuVatEnabled
                && purchasedProduct.Return(product => product.IsTelecommunicationsOrBroadcastingOrElectronicServices, false)
                && IsEuConsumer(order.Customer, order.BillingAddress);
        }

        /// <summary>
        /// Get a value indicating whether a customer is consumer located in Europe Union
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <returns>True if is EU consumer; otherwise false</returns>
        protected virtual bool IsEuConsumer(Customer customer, common.Address billingAddress)
        {
            //get country from billing address
            var country = billingAddress.Country;

            //get country specified during registration?
            if (country == null)
                country = _countryService.GetCountryById(customer.GetAttribute<int>(SystemCustomerAttributeNames.CountryId));

            //get country by IP address
            if (country == null)
                country = _countryService.GetCountryByTwoLetterIsoCode(_geoLookupService.LookupCountryIsoCode(customer.LastIpAddress));

            //we cannot detect country
            if (country == null)
                return false;

            //outside EU
            if (!country.SubjectToVat)
                return false;

            //company (business) or consumer?
            if ((VatNumberStatus)customer.GetAttribute<int>(SystemCustomerAttributeNames.VatNumberStatusId) == VatNumberStatus.Valid)
                return false;

            return true;
        }

        /// <summary>
        /// Get a whole request tax exeption
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <returns>Exemption reason</returns>
        protected string GetRequestExemption(Customer customer)
        {
            if (customer == null)
                return null;

            //customer tax exemption
            if (customer.IsTaxExempt)
                return string.Format("Customer-{0}", customer.Id);

            //customer role tax exemption
            var exemptRole = customer.CustomerRoles.FirstOrDefault(role => role.Active && role.TaxExempt);

            return exemptRole.Return(role => string.Format("Role-{0}", role.Id), null);
        }

        /// <summary>
        /// Create the address for request from address entity
        /// </summary>
        /// <param name="address">Address entity</param>
        /// <returns>Address</returns>
        protected Address CreateAddress(common.Address address)
        {
            return new Address
            {
                AddressCode = address.Id.ToString(),
                Line1 = address.Address1,
                Line2 = address.Address2,
                City = address.City,
                Region = address.StateProvince.Return(state => state.Abbreviation, null),
                Country = address.Country.Return(country => country.TwoLetterIsoCode, null),
                PostalCode = address.ZipPostalCode
            };
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets tax rate
        /// </summary>
        /// <param name="calculateTaxRequest">Tax calculation request</param>
        /// <returns>Tax</returns>
        public CalculateTaxResult GetTaxRate(CalculateTaxRequest calculateTaxRequest, bool IsAddToCartSelected)
        {
            if (calculateTaxRequest.ShipFromAddress == null)
                return new CalculateTaxResult { Errors = new[] { "Address is not set" } };

            Dictionary<int, decimal> ChangeinPriceForDiscount = new Dictionary<int, decimal>();
            //construct a cache key
            //var cacheKey = string.Format(TAXRATE_KEY,
            //    calculateTaxRequest.Product.Id,
            //    calculateTaxRequest.Address.Address1,
            //    calculateTaxRequest.Address.City,
            //    calculateTaxRequest.Address.StateProvince.Return(state => state.Id, 0),
            //    calculateTaxRequest.Address.Country.Return(country => country.Id, 0),
            //    calculateTaxRequest.Address.ZipPostalCode);

            //create a simplified tax request (only for get and the further use of tax rate)
            var taxRequest = new TaxRequest
            {
                Client = VertexTaxHelper.VERTEX_CLIENT,
                CompanyCode = _vertexTaxSettings.CompanyCode,
                CustomerCode = calculateTaxRequest.Customer.Return(customer => customer.Id.ToString(), null),
                DetailLevel = DetailLevel.Tax,
                DocCode = Guid.NewGuid().ToString(),
                DocType = DocType.SalesOrder
            };

            //set destination and origqin addresses
            var originAddress = calculateTaxRequest.ShipFromAddress;

            taxRequest.Addresses = new[]
            {
                CreateAddress(originAddress),
                CreateAddress(calculateTaxRequest.ShipToAddress)
            };

            //create a simplified item line
            var line = new Line
            {
                LineNo = "1",
                DestinationCode = calculateTaxRequest.ShipToAddress.Return(address => address.Id.ToString(), null),
                OriginCode = originAddress.Return(address => address.Id.ToString(), null),
            };
            taxRequest.Lines = new[] { line };

            if (_httpContext.Session["discountedAmountBySubcategoryVertex"] != null)
            {
                ChangeinPriceForDiscount = (Dictionary<int, decimal>)(_httpContext.Session["discountedAmountBySubcategoryVertex"]);
            }
            
            List<string> results = new List<string>();
            //get response
            results = VertexTaxHelper.PostTaxRequest(taxRequest, calculateTaxRequest, _logger, _taxCategoryService, _glcodeService, _storeContext.CurrentStore, ChangeinPriceForDiscount);
            if (results == null)
                return new CalculateTaxResult { Errors = new[] { "Bad request" } };

            //tax rate successfully received, so cache it
            var taxRate = decimal.Zero;
            var taxService = EngineContext.Current.Resolve<IVertexTaxRateService>();

            if (results != null)
            {
                foreach (string result in results)
                {
                    taxRate += VertexTaxHelper.GetTaxfromQuotationTaxXML(result, taxService, out Dictionary<string, decimal> outTax);
                }
                //foreach (var p in taxService.GetAllProductTax(System.Web.HttpContext.Current.Session.SessionID, calculateTaxRequest.Product.Id))
                //{
                //    taxRate += p.Tax;
                //}
            }
            return new CalculateTaxResult { TaxRate = taxRate };
        }


        /// <summary>
        /// Void or delete an existing transaction record from the AvaTax system. 
        /// </summary>
        /// <param name="order">Order</param>
        /// <param name="deleted">Whether order was deleted</param>
        public void VoidTaxRequest(Order order, bool deleted)
        {
            if (order == null)
                return;

            var cancelRequest = new CancelTaxRequest
            {
                CancelCode = deleted ? CancelReason.DocDeleted : CancelReason.DocVoided,
                CompanyCode = _vertexTaxSettings.CompanyCode,
                DocCode = order.Id.ToString(),
                DocType = DocType.SalesInvoice
            };

            VertexTaxHelper.CancelTaxRequest(cancelRequest, _vertexTaxSettings, _logger);
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
            controllerName = "TaxVertex";
            routeValues = new RouteValueDictionary { { "Namespaces", "Nop.Plugin.Tax.Vertex.Controllers" }, { "area", null } };
        }

        /// <summary>
        /// Install the plugin
        /// </summary>
        public override void Install()
        {
            //settings
            _settingService.SaveSetting(new VertexTaxSettings
            {
                CompanyCode = "APITrialCompany",
                IsSandboxEnvironment = true,
                CommitTransactions = true
            });

            //locales
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Vertex.Credentials.Declined", "Credentials declined");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Vertex.Credentials.Verified", "Credentials verified");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Vertex.Fields.AccountId", "Account ID");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Vertex.Fields.AccountId.Hint", "Cpecify Vertex account ID.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Vertex.Fields.CommitTransactions", "Commit transactions");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Vertex.Fields.CommitTransactions.Hint", "Check for recording transactions in the history on your Vertex account.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Vertex.Fields.CompanyCode", "Company code");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Vertex.Fields.CompanyCode.Hint", "Enter your company code in Vertex account.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Vertex.Fields.LicenseKey", "License key");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Vertex.Fields.LicenseKey.Hint", "Cpecify Vertex account license key.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Vertex.Fields.IsSandboxEnvironment", "Sandbox environment");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Vertex.Fields.IsSandboxEnvironment.Hint", "Check for using sandbox (testing environment).");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Vertex.Fields.ValidateAddresses", "Validate addresses");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Vertex.Fields.ValidateAddresses.Hint", "Check for validating addresses before tax requesting (only for US or Canadian address).");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Vertex.TestConnection", "Test connection");
            this.AddOrUpdatePluginLocaleResource("Plugins.Tax.Vertex.TestTax", "Test tax calculation");

            base.Install();
        }

        /// <summary>
        /// Uninstall the plugin
        /// </summary>
        public override void Uninstall()
        {
            //settings
            _settingService.DeleteSetting<VertexTaxSettings>();

            //locales
            this.DeletePluginLocaleResource("Plugins.Tax.Vertex.Credentials.Declined");
            this.DeletePluginLocaleResource("Plugins.Tax.Vertex.Credentials.Verified");
            this.DeletePluginLocaleResource("Plugins.Tax.Vertex.Fields.AccountId");
            this.DeletePluginLocaleResource("Plugins.Tax.Vertex.Fields.AccountId.Hint");
            this.DeletePluginLocaleResource("Plugins.Tax.Vertex.Fields.CommitTransactions");
            this.DeletePluginLocaleResource("Plugins.Tax.Vertex.Fields.CommitTransactions.Hint");
            this.DeletePluginLocaleResource("Plugins.Tax.Vertex.Fields.CompanyCode");
            this.DeletePluginLocaleResource("Plugins.Tax.Vertex.Fields.CompanyCode.Hint");
            this.DeletePluginLocaleResource("Plugins.Tax.Vertex.Fields.LicenseKey");
            this.DeletePluginLocaleResource("Plugins.Tax.Vertex.Fields.LicenseKey.Hint");
            this.DeletePluginLocaleResource("Plugins.Tax.Vertex.Fields.IsSandboxEnvironment");
            this.DeletePluginLocaleResource("Plugins.Tax.Vertex.Fields.IsSandboxEnvironment.Hint");
            this.DeletePluginLocaleResource("Plugins.Tax.Vertex.Fields.ValidateAddresses");
            this.DeletePluginLocaleResource("Plugins.Tax.Vertex.Fields.ValidateAddresses.Hint");
            this.DeletePluginLocaleResource("Plugins.Tax.Vertex.TestConnection");
            this.DeletePluginLocaleResource("Plugins.Tax.Vertex.TestTax");

            base.Uninstall();
        }

        #endregion


        /// <summary>
        /// From interface
        /// </summary>
        /// <param name="details"></param>
        /// <param name="order"></param>
        /// <param name="logger"></param>
        /// <param name="tcService"></param>
        public void PostDistributiveTaxRequest(OrderProcessingService.PlaceOrderContainter details, Order order, ILogger logger, ITaxCategoryService tcService, IGlCodeService glCodeService)
        {
            var taxService = EngineContext.Current.Resolve<IVertexTaxRateService>();
            VertexTaxHelper.PostDistributeTaxRequest(details, order, _storeContext.CurrentStore, _logger, _taxCategoryService, glCodeService, taxService, _taxDataService);
        }

        ///// <summary>
        ///// From Interface
        ///// </summary>
        ///// <param name="cart"></param>
        ///// <param name="logger"></param>
        ///// <param name="service"></param>
        //public void PostQuotationTaxRequestinBulk(IList<ShoppingCartItem> cart, ILogger logger, ITaxCategoryService service)
        //{
        //    VertexTaxHelper.PostQuotationTaxRequestinBulk(cart, _logger, _taxCategoryService, _storeContext.CurrentStore);
        //}

        //public List<DynamicVertexGLCode> GetDynamicGLCodes(int orderId, int productId)
        //{
        //    return _glcodeService.GetVertexGlBreakdown(orderId,productId);
        //    //return VertexTaxHelper.GetVertexGlBreakdown(orderId, productId, );
        //}


        public void PostDistributiveTaxRefundRequest(Order order, int orderItemId, ILogger logger, bool isPartialRefund = false, decimal AmountToRefund1 = 0, decimal AmountToRefund2 = 0, decimal AmountToRefund3 = 0, int ProductId = 0, List<int> data = null)
        {
            VertexTaxHelper.PostDistributeTaxRefundRequest(order, orderItemId, _storeContext.CurrentStore, _glcodeService, _logger, isPartialRefund, AmountToRefund1, AmountToRefund2, AmountToRefund3, ProductId, data);
        }

        public void PostDistributiveFullTaxRefundRequestPerItem(Order order, int orderItemId, ILogger logger, bool isPartialRefund = false, decimal AmountToRefund1 = 0, decimal AmountToRefund2 = 0, decimal AmountToRefund3 = 0, int ProductId = 0, decimal taxAmoun1 = 0, decimal taxAmount2 = 0, decimal taxAmount3 = 0, decimal DeliveryAmount = 0, decimal DeliveryTax = 0, decimal ShippingAmount = 0, decimal ShippingTax = 0)
        {
            VertexTaxHelper.PostDistributeFullTaxRefundRequestPerItem(order, orderItemId, _storeContext.CurrentStore, _glcodeService, _logger, isPartialRefund, AmountToRefund1, AmountToRefund2, AmountToRefund3, ProductId, taxAmoun1, taxAmount2, taxAmount3, DeliveryAmount, DeliveryTax, ShippingAmount, ShippingTax);
        }

        public void HandleEvent(EntityUpdated<ShoppingCartItem> eventMessage)
        {
            //using eventMessage.Entity
        }

        public void HandleEvent(EntityInserted<ShoppingCartItem> eventMessage)
        {
            //using eventMessage.Entity
        }

        public void HandleEvent(EntityDeleted<ShoppingCartItem> eventMessage)
        {
            var taxService = EngineContext.Current.Resolve<IVertexTaxRateService>();
            taxService.DeleteTaxRatesForProduct(eventMessage.Entity.ProductId, System.Web.HttpContext.Current.Session.SessionID);
        }
    }
}
