using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Core.Plugins;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Shipping;
using Nop.Services.Stores;
using Nop.Services.Tax;
using Nop.Web.Extensions;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Security;
using Nop.Web.Models.Checkout;
using Nop.Web.Models.Common;
using Nop.Web.Models.Media;
using Nop.Web.Models.ShoppingCart;
using System;
using System.Collections.Generic;
//---START: Codechages done by (na-sdxcorp\ADas)--------------
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Xml;


//---END: Codechages done by (na-sdxcorp\ADas)--------------

namespace Nop.Web.Controllers
{
    [NopHttpsRequirement(SslRequirement.Yes)]
    public partial class CheckoutController : BasePublicController
    {
        #region Fields
        //private static readonly CultureInfo _defaultCulture;
        private readonly IProductService _productService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly ILocalizationService _localizationService;
        private readonly ITaxService _taxService;
        private readonly ICurrencyService _currencyService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly ICustomerService _customerService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IShippingService _shippingService;
        private readonly IPaymentService _paymentService;
        private readonly IPictureService _pictureService;
        private readonly IPluginFinder _pluginFinder;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly IRewardPointService _rewardPointService;
        private readonly ILogger _logger;
        private readonly IOrderService _orderService;
        private readonly IWebHelper _webHelper;
        private readonly HttpContextBase _httpContext;
        private readonly IAddressAttributeParser _addressAttributeParser;
        private readonly IAddressAttributeService _addressAttributeService;
        private readonly IAddressAttributeFormatter _addressAttributeFormatter;
        private readonly IAddressService _addressService;
        private readonly IFulfillmentService _fulfillmentService;
        private readonly IStoreService _storeService;
        //---START: Codechages done by (na-sdxcorp\ADas)--------------
        private readonly IShipmentService _shipmentService;
        private readonly ICustomerActivityService _customerActivityService;
        //---End: Codechages done by (na-sdxcorp\ADas)--------------


        private readonly OrderSettings _orderSettings;
        private readonly RewardPointsSettings _rewardPointsSettings;
        private readonly PaymentSettings _paymentSettings;
        private readonly ShippingSettings _shippingSettings;
        private readonly AddressSettings _addressSettings;
        private readonly CustomerSettings _customerSettings;

        #endregion

        #region Constructors

        public CheckoutController(IProductService productService,
            IWorkContext workContext,
            IStoreService storeService,
            IStoreContext storeContext,
            IStoreMappingService storeMappingService,
            IShoppingCartService shoppingCartService,
            ILocalizationService localizationService,
            ITaxService taxService,
            ICurrencyService currencyService,
            IPriceFormatter priceFormatter,
            IOrderProcessingService orderProcessingService,
            ICustomerService customerService,
            IGenericAttributeService genericAttributeService,
            ICountryService countryService,
            IStateProvinceService stateProvinceService,
            IShippingService shippingService,
            IPaymentService paymentService,
            IPluginFinder pluginFinder,
            IOrderTotalCalculationService orderTotalCalculationService,
            IRewardPointService rewardPointService,
            ILogger logger,
            IOrderService orderService,
            IWebHelper webHelper,
            HttpContextBase httpContext,
            IAddressAttributeParser addressAttributeParser,
            IAddressAttributeService addressAttributeService,
            IAddressAttributeFormatter addressAttributeFormatter,
            OrderSettings orderSettings,
            RewardPointsSettings rewardPointsSettings,
            PaymentSettings paymentSettings,
            ShippingSettings shippingSettings,
            AddressSettings addressSettings,
            CustomerSettings customerSettings,
            IPictureService pictureService,
            IFulfillmentService fulfillmentService,
            IAddressService addressService/*---START: Codechages done by (na-sdxcorp\ADas)--------------*/,
            IShipmentService shipmentService,
            ICustomerActivityService customerActivityService
            /*---END: Codechages done by (na-sdxcorp\ADas)--------------*/)
        {
            this._productService = productService;
            this._workContext = workContext;
            this._storeContext = storeContext;
            this._storeMappingService = storeMappingService;
            this._shoppingCartService = shoppingCartService;
            this._localizationService = localizationService;
            this._taxService = taxService;
            this._currencyService = currencyService;
            this._priceFormatter = priceFormatter;
            this._orderProcessingService = orderProcessingService;
            this._customerService = customerService;
            this._genericAttributeService = genericAttributeService;
            this._countryService = countryService;
            this._stateProvinceService = stateProvinceService;
            this._shippingService = shippingService;
            this._paymentService = paymentService;
            this._pluginFinder = pluginFinder;
            this._orderTotalCalculationService = orderTotalCalculationService;
            this._rewardPointService = rewardPointService;
            this._logger = logger;
            this._orderService = orderService;
            this._webHelper = webHelper;
            this._httpContext = httpContext;
            this._addressAttributeParser = addressAttributeParser;
            this._addressAttributeService = addressAttributeService;
            this._addressAttributeFormatter = addressAttributeFormatter;
            this._storeService = storeService;
            this._orderSettings = orderSettings;
            this._rewardPointsSettings = rewardPointsSettings;
            this._paymentSettings = paymentSettings;
            this._shippingSettings = shippingSettings;
            this._addressSettings = addressSettings;
            this._customerSettings = customerSettings;
            this._pictureService = pictureService;
            this._fulfillmentService = fulfillmentService;
            this._addressService = addressService;
            //---START: Codechages done by (na-sdxcorp\ADas)--------------
            this._shipmentService = shipmentService;
            this._customerActivityService = customerActivityService;

            

            //---END: Codechages done by (na-sdxcorp\ADas)--------------
        }

        #endregion

        #region Utilities
        [NonAction]
        public void UpdateShippingFeeAndFirstCartItem()
        {
            decimal shippingAmount = 0;
            decimal orderTotal = 0;
            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                       .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                       .LimitPerStore(_storeContext.CurrentStore.Id).ToList();

            ShoppingCartItem firstCartItem = new ShoppingCartItem();
            var currentCart = cart.Where(o => o.IsTieredShippingEnabled).ToList();
            if (currentCart.Any())
            {
                firstCartItem = currentCart.FirstOrDefault();
                foreach (ShoppingCartItem sci in currentCart)
                {
                    decimal prodPrice = sci.Product.Price;
                    if (sci.Quantity > 0)
                    {
                        prodPrice = sci.Product.Price * sci.Quantity;
                    }
                    orderTotal += prodPrice;
                }
                var allShipping = _storeService.GetTieredShipping(_storeContext.CurrentStore.Id);
                decimal maxMinPrice = allShipping.Max(x => x.MinPrice);

                if (orderTotal >= maxMinPrice)
                {
                    shippingAmount = allShipping.Where(o => maxMinPrice == o.MinPrice).FirstOrDefault().ShippingAmount;
                }
                else
                {
                    shippingAmount = allShipping.Where(o => orderTotal >= o.MinPrice && orderTotal <= o.MaxPrice).FirstOrDefault().ShippingAmount;
                }
            }

            foreach (ShoppingCartItem sci in cart)
            {
                if (sci == firstCartItem && sci.IsTieredShippingEnabled)
                {
                    sci.FlatShipping = shippingAmount;
                    sci.IsFirstCartItem = true;
                    _shoppingCartService.UpdateShoppingCartItemForTieredShipping(sci);
                }
                else
                {
                    sci.FlatShipping = 0;
                    sci.IsFirstCartItem = false;
                    _shoppingCartService.UpdateShoppingCartItemForTieredShipping(sci);
                }
            }
        }

        [NonAction]
        public bool DecideTieredShippingVisibility(ShoppingCartModel.ShoppingCartItemModel cartItemModel)
        {
            bool IsTieredShippingVisible = false;
            if (cartItemModel != null)
            {
                if (cartItemModel.IsOnlyPickUpProduct)
                {
                    IsTieredShippingVisible = false;
                }
                if (cartItemModel.IsMultipleShippingFeatureEnabled && cartItemModel.IsOnlyShippingEnabledProduct)
                {
                    IsTieredShippingVisible = true;
                }
            }
            return IsTieredShippingVisible;
        }
        [NonAction]
        public decimal GetShippingAmount()
        {
            decimal shippingAmount = 0;
            decimal orderTotal = 0;

            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                   .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                   .LimitPerStore(_storeContext.CurrentStore.Id)
                   .ToList();
            if (cart.Any())
            {
                var firstCartItem = cart.FirstOrDefault();
                foreach (ShoppingCartItem sci in cart)
                {
                    decimal prodPrice = sci.Product.Price;
                    if (sci.Quantity > 0)
                    {
                        prodPrice = sci.Product.Price * sci.Quantity;
                    }
                    orderTotal += prodPrice;
                }
                var allShipping = _storeService.GetTieredShipping(_storeContext.CurrentStore.Id);
                decimal maxMinPrice = allShipping.Max(x => x.MinPrice);

                if (orderTotal >= maxMinPrice)
                {
                    shippingAmount = allShipping.Where(o => maxMinPrice == o.MinPrice).FirstOrDefault().ShippingAmount;
                }
                else
                {
                    shippingAmount = allShipping.Where(o => orderTotal >= o.MinPrice && orderTotal <= o.MaxPrice).FirstOrDefault().ShippingAmount;
                }
            }

            return shippingAmount;
        }

        [NonAction]
        protected virtual bool IsPaymentWorkflowRequired(IList<ShoppingCartItem> cart, bool ignoreRewardPoints = false)
        {
            bool result = true;

            //check whether order total equals zero
            decimal? shoppingCartTotalBase = _orderTotalCalculationService.GetShoppingCartTotal(cart, ignoreRewardPoints);
            if (shoppingCartTotalBase.HasValue && shoppingCartTotalBase.Value == decimal.Zero)
            {
                result = false;
            }

            return result;
        }

        [NonAction]
        protected virtual CheckoutBillingAddressModel PrepareBillingAddressModel(IList<ShoppingCartItem> cart,
            int? selectedCountryId = null,
            bool prePopulateNewAddressWithCustomerFields = false,
            string overrideAttributesXml = "")
        {
            var model = new CheckoutBillingAddressModel();
            model.ShipToSameAddressAllowed = _shippingSettings.ShipToSameAddress && cart.RequiresShipping();
            model.ShipToSameAddress = true;

            //existing addresses
            var addresses = _workContext.CurrentCustomer.Addresses
                .Where(a => a.Country == null ||
                    (//published
                    a.Country.Published &&
                    //allow billing
                    a.Country.AllowsBilling
                   //enabled for the current store
                   ))
                .ToList();
            foreach (var address in addresses)
            {
                var addressModel = new AddressModel();
                addressModel.PrepareModel(
                    address: address,
                    excludeProperties: false,
                    addressSettings: _addressSettings,
                    addressAttributeFormatter: _addressAttributeFormatter);
                model.ExistingAddresses.Add(addressModel);
            }

            //new address
            model.NewAddress.CountryId = selectedCountryId;
            model.NewAddress.PrepareModel(address:
                null,
                excludeProperties: false,
                addressSettings: _addressSettings,
                localizationService: _localizationService,
                stateProvinceService: _stateProvinceService,
                addressAttributeService: _addressAttributeService,
                addressAttributeParser: _addressAttributeParser,
                loadCountries: () => _countryService.GetAllCountriesForBilling(_workContext.WorkingLanguage.Id),
                prePopulateWithCustomerFields: prePopulateNewAddressWithCustomerFields,
                customer: _workContext.CurrentCustomer,
                overrideAttributesXml: overrideAttributesXml);
            return model;
        }

        [NonAction]
        protected virtual CheckoutShippingAddressModel PrepareShippingAddressModel(int? selectedCountryId = null,
            bool prePopulateNewAddressWithCustomerFields = false, string overrideAttributesXml = "")
        {
            var model = new CheckoutShippingAddressModel();
            //allow pickup in store?
            model.AllowPickUpInStore = _shippingSettings.AllowPickUpInStore;
            #region SODMYWAY-
            /*
            if (model.AllowPickUpInStore)
            {
                model.DisplayPickupPointsOnMap = _shippingSettings.DisplayPickupPointsOnMap;
                model.GoogleMapsApiKey = _shippingSettings.GoogleMapsApiKey;
                var pickupPointProviders = _shippingService.LoadActivePickupPointProviders(_workContext.CurrentCustomer, _storeContext.CurrentStore.Id);
                if (pickupPointProviders.Any())
                {
                    var pickupPointsResponse = _shippingService.GetPickupPoints(_workContext.CurrentCustomer.BillingAddress, _workContext.CurrentCustomer, storeId: _storeContext.CurrentStore.Id);
                    if (pickupPointsResponse.Success)
                        model.PickupPoints = pickupPointsResponse.PickupPoints.Select(x =>
                        {
                            var country = _countryService.GetCountryByTwoLetterIsoCode(x.CountryCode);
                            var state = _stateProvinceService.GetStateProvinceByAbbreviation(x.StateAbbreviation);
                            var pickupPointModel = new CheckoutPickupPointModel
                            {
                                Id = x.Id,
                                Name = x.Name,
                                Description = x.Description,
                                ProviderSystemName = x.ProviderSystemName,
                                Address = x.Address,
                                City = x.City,
                                StateName = state != null ? state.Name : string.Empty,
                                CountryName = country != null ? country.Name : string.Empty,
                                ZipPostalCode = x.ZipPostalCode,
                                Latitude = x.Latitude,
                                Longitude = x.Longitude,
                                OpeningHours = x.OpeningHours
                            };
                            if (x.PickupFee > 0)
                            {
                                var amount = _taxService.GetShippingPrice(x.PickupFee, _workContext.CurrentCustomer);
                                amount = _currencyService.ConvertFromPrimaryStoreCurrency(amount, _workContext.WorkingCurrency);
                                pickupPointModel.PickupFee = _priceFormatter.FormatShippingPrice(amount, true);
                            }

                            return pickupPointModel;
                        }).ToList();
                    else
                        foreach (var error in pickupPointsResponse.Errors)
                            model.Warnings.Add(error);
                }

                //only available pickup points
                if (!_shippingService.LoadActiveShippingRateComputationMethods(_workContext.CurrentCustomer, _storeContext.CurrentStore.Id).Any())
                {
                    if (!pickupPointProviders.Any())
                    {
                        model.Warnings.Add(_localizationService.GetResource("Checkout.ShippingIsNotAllowed"));
                        model.Warnings.Add(_localizationService.GetResource("Checkout.PickupPoints.NotAvailable"));
                    }
                    model.PickUpInStoreOnly = true;
                    model.PickUpInStore = true;
                    return model;
                }
            }
            */
            #endregion

            //existing addresses
            var addresses = _workContext.CurrentCustomer.Addresses
                .Where(a => a.Country == null ||
                    (//published
                    a.Country.Published &&
                    //allow shipping
                    a.Country.AllowsShipping
                 //enabled for the current store
                 ))
                .ToList();
            foreach (var address in addresses)
            {
                var addressModel = new AddressModel();
                addressModel.PrepareModel(
                    address: address,
                    excludeProperties: false,
                    addressSettings: _addressSettings,
                    addressAttributeFormatter: _addressAttributeFormatter);
                model.ExistingAddresses.Add(addressModel);
            }

            //new address
            model.NewAddress.CountryId = selectedCountryId;
            model.NewAddress.PrepareModel(
                address: null,
                excludeProperties: false,
                addressSettings: _addressSettings,
                localizationService: _localizationService,
                stateProvinceService: _stateProvinceService,
                addressAttributeService: _addressAttributeService,
                addressAttributeParser: _addressAttributeParser,
                loadCountries: () => _countryService.GetAllCountriesForShipping(_workContext.WorkingLanguage.Id),
                prePopulateWithCustomerFields: prePopulateNewAddressWithCustomerFields,
                customer: _workContext.CurrentCustomer,
                overrideAttributesXml: overrideAttributesXml);
            return model;
        }

        #region SODMYWAY-

        [NonAction]
        protected virtual CheckoutShippingMethodListModel PrepareShippingMethodModel(IList<ShoppingCartItem> cart, Address shippingAddress)
        {
            var cartItemModels = new List<ShoppingCartModel.ShoppingCartItemModel>();

            var getShippingOptionResponse = _shippingService.GetShippingOptions(cart, shippingAddress, "", storeId: _storeContext.CurrentStore.Id);
            bool isMultipleShippingFeatureEnabled = (_storeContext.CurrentStore.IsTieredShippingEnabled == true ? true : false ||
                                                   _storeContext.CurrentStore.IsContractShippingEnabled == true ? true : false ||
                                                   _storeContext.CurrentStore.IsInterOfficeDeliveryEnabled == true ? true : false) ? true : false;

            if (getShippingOptionResponse.Success)
            {
                //---START: Codechages done by (na-sdxcorp\ADas)--------------
                var nonUPS_ShippingOptions = new List<ShippingOption>();
                nonUPS_ShippingOptions = getShippingOptionResponse.ShippingOptions.Where(p => p.ShoppingCartProductId == 0 && !p.ShippingRateComputationMethodSystemName.Equals("Shipping.UPS")).ToList();

                foreach (ShippingOption op in nonUPS_ShippingOptions)
                {
                    op.RequireFulfilmentCalendar = true;
                }

                var upsAll_ShippingOptions = new Dictionary<int, List<ShippingOption>>();
                upsAll_ShippingOptions.Add(0, nonUPS_ShippingOptions);
                foreach (ShoppingCartItem item in cart)
                {
                    var ups_ShippingOptions = new List<ShippingOption>();
                    ups_ShippingOptions = getShippingOptionResponse.ShippingOptions.Where(p => (p.ShoppingCartProductId == item.ProductId && p.ShippingRateComputationMethodSystemName.Equals("Shipping.UPS")) || (p.ShoppingCartProductId == 0 && !p.ShippingRateComputationMethodSystemName.Equals("Shipping.UPS"))).ToList();


                    foreach (ShippingOption op in ups_ShippingOptions)
                    {
                        op.RequireFulfilmentCalendar = true;
                        if (op.Name == "UPS Ground") //New change added by Ryan
                        {
                            op.Rate = op.Rate * item.Quantity;
                            op.RequireFulfilmentCalendar = false;
                        }
                    }

                    upsAll_ShippingOptions.Add(item.Id, ups_ShippingOptions);
                }
                //---END: Codechages done by (na-sdxcorp\ADas)--------------
                foreach (ShoppingCartItem item in cart)
                {
                    var model = new CheckoutShippingMethodModel();
                    //performance optimization. cache returned shipping options.
                    //we'll use them later (after a customer has selected an option).
                    _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer,
                        SystemCustomerAttributeNames.OfferedShippingOptions,
                        /*---START: Codechages done by (na-sdxcorp\ADas)------*/
                        //Commented below line
                        //getShippingOptionResponse.ShippingOptions,
                        upsAll_ShippingOptions.ContainsKey(item.Id) ? upsAll_ShippingOptions[item.Id] : upsAll_ShippingOptions[0],
                        /*---END: Codechages done by (na-sdxcorp\ADas)------*/
                        _storeContext.CurrentStore.Id);
                    if (item == cart.FirstOrDefault())
                    {
                    }

                    //Cart Item Model - Product Info To show
                    var cartItemModel = new ShoppingCartModel.ShoppingCartItemModel
                    {
                        Id = item.Id,
                        Sku = item.Product.Sku,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        CheckoutNotes = item.Product.CheckoutNotes,
                        IsTieredShippingEnabled = _storeContext.CurrentStore.IsTieredShippingEnabled == true ? true : false,
                        IsContractShippingEnabled = _storeContext.CurrentStore.IsContractShippingEnabled == true ? true : false,
                        IsInterOfficeDeliveryEnabled = _storeContext.CurrentStore.IsInterOfficeDeliveryEnabled == true ? true : false,
                        IsMultipleShippingFeatureEnabled = isMultipleShippingFeatureEnabled,
                        FlatShipping = 0,
                        IsOnlyPickUpProduct = (item.Product.IsPickupEnabled && !item.Product.IsShipEnabled) ? true : false,
                        IsOnlyShippingEnabledProduct = (item.Product.IsShipEnabled && !item.Product.IsPickupEnabled) ? true : false
                    };

                    cartItemModel.ProductName = item.Product.GetLocalized(x => x.Name);
                    var picture = _pictureService.GetPicturesByProductId(item.ProductId, 1).FirstOrDefault();
                    cartItemModel.IsTieredShippingVisible = DecideTieredShippingVisibility(cartItemModel);


                    cartItemModel.Picture = new PictureModel
                    {
                        ImageUrl = _pictureService.GetPictureUrl(picture, 50, true),
                        Title =
                            string.Format(
                                _localizationService.GetResource("Media.Product.ImageLinkTitleFormat"), cartItemModel.ProductName),
                        AlternateText = cartItemModel.ProductName
                    };

                    // Fulfillment dates calendar & lead time
                    var minDate = DateTime.Now;
                    minDate = minDate.AddDays(item.Product.ProductionDaysLead.GetValueOrDefault(0))
                                .AddHours(item.Product.ProductionHoursLead.GetValueOrDefault(0))
                                .AddMinutes(item.Product.ProductionMinutesLead.GetValueOrDefault(0));
                    var maxDate = DateTime.Now.Date.AddMonths(3);

                    var sb = new StringBuilder();
                    var excludedDates = _fulfillmentService.GetForDateRange(_storeContext.CurrentStore.Id, DateTime.Now, maxDate);
                    foreach (var exclCalDay in excludedDates)
                    {
                        if (exclCalDay.Date.Date == minDate.Date)
                        {
                            minDate = minDate.AddDays(1);
                        }
                        if (sb.Length > 0)
                        {
                            sb.Append(",");
                        }

                        sb.AppendFormat("'{0}-{1}-{2}'", exclCalDay.Date.Year,
                                        exclCalDay.Date.Month.ToString().PadLeft(2, '0'),
                                        exclCalDay.Date.Day.ToString().PadLeft(2, '0'));
                    }
                    cartItemModel.FulfillmentModel.JsExcludedDatesStr = sb.ToString();
                    cartItemModel.FulfillmentModel.JsMinDateStr = string.Format("{0},{1},{2},{3},{4}", minDate.Year, minDate.Month - 1, minDate.Day, minDate.Hour, minDate.Minute); //minDate.ToString("tt", CultureInfo.InvariantCulture)
                    cartItemModel.FulfillmentModel.JsMaxDateStr = string.Format("{0},{1},{2}", maxDate.Year, maxDate.Month - 1, maxDate.Day);
                    cartItemModel.FulfillmentModel.JsLeadTimeDays = item.Product.ProductionDaysLead;
                    cartItemModel.FulfillmentModel.JsLeadTimeHours = item.Product.ProductionHoursLead;
                    cartItemModel.FulfillmentModel.JsLeadTimeMinutes = item.Product.ProductionMinutesLead;

                    int vendorId = -1;
                    int storeId = -1;
                    if (item.Product.VendorId > 0)
                    {
                        vendorId = item.Product.VendorId;
                    }
                    else
                    {
                        storeId = item.StoreId;
                    }

                    if (!item.Product.IsMealPlan && !item.Product.IsDonation) /// NU-17 & NU-16
                    {
                        foreach (var shippingOption in upsAll_ShippingOptions.ContainsKey(item.Id) ? upsAll_ShippingOptions[item.Id] : upsAll_ShippingOptions[0])
                        {

                            decimal deliveryRate = 0;

                            if (shippingOption.Name == "Delivery")
                            {
                                if (item.Product.WarehouseId == 0)
                                {
                                    deliveryRate = 0;
                                }
                                else
                                {
                                    deliveryRate = _shippingService.GetWarehouseById(item.Product.WarehouseId).DeliveryFee;
                                }
                            }
                            else if (shippingOption.Name == "In-Store Pickup")
                            {
                                if (item.Product.PreferredPickupWarehouseId == 0)
                                {
                                    deliveryRate = 0;
                                }
                                else
                                {
                                    deliveryRate = 0;
                                }

                            }
                            //else if (shippingOption.Name == "FlatRateShipping")
                            //{
                            //    deliveryRate = shippingOption.Rate;
                            //}
                            else //must be UPS
                            {
                                if (!(item.IsContractShippingEnabled || item.IsTieredShippingEnabled || item.IsInterOfficeDeliveryEnabled))
                                {
                                    deliveryRate = shippingOption.Rate;
                                }
                                else
                                {
                                    deliveryRate = 0;
                                }
                            }
                            var soModel = new CheckoutShippingMethodModel.ShippingMethodModel
                            {
                                Name = shippingOption.Name,
                                Description = shippingOption.Description,
                                ShippingRateComputationMethodSystemName = shippingOption.ShippingRateComputationMethodSystemName,
                                ShippingOption = shippingOption,
                                RequireFulfillmentCalendar = shippingOption.RequireFulfilmentCalendar
                            };

                            //adjust rate
                            List<Discount> appliedDiscounts;
                            var shippingTotal = _orderTotalCalculationService.AdjustShippingRate(shippingOption.Rate, cart, out appliedDiscounts);

                            decimal rateBase = _taxService.GetShippingPrice(shippingTotal, _workContext.CurrentCustomer);
                            decimal rate = _currencyService.ConvertFromPrimaryStoreCurrency(rateBase, _workContext.WorkingCurrency);
                            soModel.Fee = _priceFormatter.FormatShippingPrice(rate, true);

                            if (item.Product.IsShipEnabled && shippingOption.Name.Contains("UPS"))
                            {
                                model.ShippingMethods.Add(soModel);
                            }
                            else if (item.Product.IsPickupEnabled && shippingOption.Name == "In-Store Pickup")
                            {
                                model.ShippingMethods.Add(soModel);
                            }
                            else if (item.Product.IsLocalDelivery && shippingOption.Name == "Delivery")
                            {
                                model.ShippingMethods.Add(soModel);
                            }
                            //else if (item.Product.IsLocalDelivery && shippingOption.Name == "FlatRateShipping")
                            //{
                            //    model.ShippingMethods.Add(soModel);
                            //}
                        }

                        //find a selected (previously) shipping method
                        var selectedShippingOption = _workContext.CurrentCustomer.GetAttribute<ShippingOption>(
                            SystemCustomerAttributeNames.SelectedShippingOption, _storeContext.CurrentStore.Id);
                        if (selectedShippingOption != null)
                        {
                            var shippingOptionToSelect = model.ShippingMethods.ToList()
                                .Find(so =>
                                    !String.IsNullOrEmpty(so.Name) &&
                                    so.Name.Equals(selectedShippingOption.Name, StringComparison.InvariantCultureIgnoreCase) &&
                                    !String.IsNullOrEmpty(so.ShippingRateComputationMethodSystemName) &&
                                so.ShippingRateComputationMethodSystemName.Equals(selectedShippingOption.ShippingRateComputationMethodSystemName, StringComparison.InvariantCultureIgnoreCase));
                            if (shippingOptionToSelect != null)
                            {
                                shippingOptionToSelect.Selected = true;
                            }
                        }
                        //if no option has been selected, let's do it for the first one
                        if (model.ShippingMethods.FirstOrDefault(so => so.Selected) == null)
                        {
                            var shippingOptionToSelect = model.ShippingMethods.FirstOrDefault();
                            if (shippingOptionToSelect != null)
                            {
                                shippingOptionToSelect.Selected = true;
                            }
                        }

                        //notify about shipping from multiple locations
                        if (_shippingSettings.NotifyCustomerAboutShippingFromMultipleLocations)
                        {
                            model.NotifyCustomerAboutShippingFromMultipleLocations = getShippingOptionResponse.ShippingFromMultipleLocations;
                        }

                        var cartItemFulfillmentModel = new ShoppingCartModel.ShoppingCartItemFulfillmentModel
                        {
                            SelectedFullfilmentWarehouseId = item.Product.WarehouseId,
                        };

                        var selectedWarehouseId = _productService.GetPickupWarehouseByProductId(item.Product.Id).Select(c => c.WarehouseId).ToList();
                        cartItemModel.FulfillmentModel.CheckoutShippingMethodModel = model;
                        IList<Warehouse> warehouses = _shippingService.GetAllWarehouses(vendorId: vendorId, storeId: storeId);

                        var warehouseModels = warehouses.Select(x => new ShoppingCartModel.WarehouseModel()
                        {
                            Name = x.Name,
                            AdminComment = x.AdminComment,
                            Id = x.Id,
                            AllowDeliveryTime = x.AllowDeliveryTime,
                            AllowPickupTime = x.AllowPickupTime,


                            //NU-90
                            DeliveryOpenTimeMon = x.DeliveryOpenTimeMon,	/// NU-78
                            DeliveryCloseTimeMon = x.DeliveryCloseTimeMon,	/// NU-78
                            PickupOpenTimeMon = x.PickupOpenTimeMon,	/// NU-13
                            PickupCloseTimeMon = x.PickupCloseTimeMon,	/// NU-13


                            DeliveryOpenTimeTues = x.DeliveryOpenTimeTues,	/// NU-78
                            DeliveryCloseTimeTues = x.DeliveryCloseTimeTues,	/// NU-78
                            PickupOpenTimeTues = x.PickupOpenTimeTues,	/// NU-13
                            PickupCloseTimeTues = x.PickupCloseTimeTues,	/// NU-13

                            DeliveryOpenTimeWeds = x.DeliveryOpenTimeWeds,	/// NU-78
                            DeliveryCloseTimeWeds = x.DeliveryCloseTimeWeds,	/// NU-78
                            PickupOpenTimeWeds = x.PickupOpenTimeWeds,	/// NU-13
                            PickupCloseTimeWeds = x.PickupCloseTimeWeds,	/// NU-13

                            DeliveryOpenTimeThurs = x.DeliveryOpenTimeThurs,	/// NU-78
                            DeliveryCloseTimeThurs = x.DeliveryCloseTimeThurs,	/// NU-78
                            PickupOpenTimeThurs = x.PickupOpenTimeThurs,	/// NU-13
                            PickupCloseTimeThurs = x.PickupCloseTimeThurs,	/// NU-13

                            DeliveryOpenTimeFri = x.DeliveryOpenTimeFri,	/// NU-78
                            DeliveryCloseTimeFri = x.DeliveryCloseTimeFri,	/// NU-78
                            PickupOpenTimeFri = x.PickupOpenTimeFri,	/// NU-13
                            PickupCloseTimeFri = x.PickupCloseTimeFri,	/// NU-13

                            DeliveryOpenTimeSat = x.DeliveryOpenTimeSat,	/// NU-78
                            DeliveryCloseTimeSat = x.DeliveryCloseTimeSat,	/// NU-78
                            PickupOpenTimeSat = x.PickupOpenTimeSat,	/// NU-13
                            PickupCloseTimeSat = x.PickupCloseTimeSat,	/// NU-13

                            DeliveryOpenTimeSun = x.DeliveryOpenTimeSun,	/// NU-78
                            DeliveryCloseTimeSun = x.DeliveryCloseTimeSun,	/// NU-78
                            PickupOpenTimeSun = x.PickupOpenTimeSun,	/// NU-13
                            PickupCloseTimeSun = x.PickupCloseTimeSun,	/// NU-13
                            DeliveryFee = x.DeliveryFee, //NU-90
                            PickupFee = x.PickupFee, //NU-90
                            //END NU-90

                            IsDelivery = x.IsDelivery,
                            IsPickup = x.IsPickup,
                            DeliveryOpenTime = x.DeliveryOpenTimeMon,
                            DeliveryCloseTime = x.DeliveryCloseTimeMon,
                            PickupCloseTime = x.PickupCloseTimeMon,
                            PickupOpenTime = x.PickupOpenTimeMon,
                            AddressId = x.AddressId
                        });

                        foreach (ShoppingCartModel.WarehouseModel whModel in warehouseModels)
                        {

                            Address address = _addressService.GetAddressById(whModel.AddressId);
                            whModel.Address.Address1 = address.Address1;
                            whModel.Address.Address2 = address.Address2;
                            whModel.Address.City = address.Address2;
                            whModel.Address.ZipPostalCode = address.ZipPostalCode;
                            if (address.StateProvince != null)
                            {
                                whModel.Address.StateProvinceName = address.StateProvince == null ? string.Empty : address.StateProvince.Abbreviation;
                            }
                            whModel.Address.PhoneNumber = address.PhoneNumber;

                            //If we have preferred warehouses.
                            if (selectedWarehouseId.Count == 0 && item.Product.PreferredPickupWarehouseId > 1)
                            {
                                if (whModel.Id == item.Product.PreferredPickupWarehouseId)
                                {
                                    cartItemModel.FulfillmentModel.ShippingMethodAllWarehouses.Add(whModel);
                                }
                            }

                            else if (selectedWarehouseId.Count > 0)
                            {
                                foreach (var wareHouseId in selectedWarehouseId)
                                {
                                    if (wareHouseId == whModel.Id)
                                    {
                                        cartItemModel.FulfillmentModel.ShippingMethodAllWarehouses.Add(whModel);
                                    }
                                }

                            }
                            else
                            {
                                cartItemModel.FulfillmentModel.ShippingMethodAllWarehouses.Add(whModel);
                            }
                        }
                    }
                    cartItemModels.Add(cartItemModel);
                }
            }
            else
            {
                var model = new CheckoutShippingMethodModel();	/// SODMYWAY-
                foreach (var error in getShippingOptionResponse.Errors)
                {
                    model.Warnings.Add(error);
                }
            }

            var mlist = new CheckoutShippingMethodListModel();
            mlist.ShoppingCartItemModels = cartItemModels;
            mlist.IsTieredShippingEnabled = _storeContext.CurrentStore.IsTieredShippingEnabled == true ? true : false;
            mlist.IsContractShippingEnabled = _storeContext.CurrentStore.IsContractShippingEnabled == true ? true : false;
            mlist.IsInterOfficeDeliveryEnabled = _storeContext.CurrentStore.IsInterOfficeDeliveryEnabled == true ? true : false;
            mlist.IsMultipleShippingFeatureEnabled = isMultipleShippingFeatureEnabled;

            return mlist;
        }
        #endregion


        [NonAction]
        protected virtual CheckoutPaymentMethodModel PreparePaymentMethodModel(IList<ShoppingCartItem> cart, int filterByCountryId)
        {
            var model = new CheckoutPaymentMethodModel();

            //reward points
            if (_rewardPointsSettings.Enabled && !cart.IsRecurring())
            {
                int rewardPointsBalance = _rewardPointService.GetRewardPointsBalance(_workContext.CurrentCustomer.Id, _storeContext.CurrentStore.Id);
                decimal rewardPointsAmountBase = _orderTotalCalculationService.ConvertRewardPointsToAmount(rewardPointsBalance);
                decimal rewardPointsAmount = _currencyService.ConvertFromPrimaryStoreCurrency(rewardPointsAmountBase, _workContext.WorkingCurrency);
                if (rewardPointsAmount > decimal.Zero &&
                    _orderTotalCalculationService.CheckMinimumRewardPointsToUseRequirement(rewardPointsBalance))
                {
                    model.DisplayRewardPoints = true;
                    model.RewardPointsAmount = _priceFormatter.FormatPrice(rewardPointsAmount, true, false);
                    model.RewardPointsBalance = rewardPointsBalance;
                }
            }

            //filter by country
            var paymentMethods = _paymentService
                .LoadActivePaymentMethods(_workContext.CurrentCustomer.Id, _storeContext.CurrentStore.Id, filterByCountryId)
                .Where(pm => pm.PaymentMethodType == PaymentMethodType.Standard || pm.PaymentMethodType == PaymentMethodType.Redirection)
                .Where(pm => !pm.HidePaymentMethod(cart))
                .ToList();
            foreach (var pm in paymentMethods)
            {
                if (cart.IsRecurring() && pm.RecurringPaymentType == RecurringPaymentType.NotSupported)
                {
                    continue;
                }

                var pmModel = new CheckoutPaymentMethodModel.PaymentMethodModel
                {
                    Name = pm.GetLocalizedFriendlyName(_localizationService, _workContext.WorkingLanguage.Id),
                    PaymentMethodSystemName = pm.PluginDescriptor.SystemName,
                    LogoUrl = pm.PluginDescriptor.GetLogoUrl(_webHelper)
                };
                //payment method additional fee
                decimal paymentMethodAdditionalFee = _paymentService.GetAdditionalHandlingFee(cart, pm.PluginDescriptor.SystemName);
                decimal rateBase = _taxService.GetPaymentMethodAdditionalFee(paymentMethodAdditionalFee, _workContext.CurrentCustomer);
                decimal rate = _currencyService.ConvertFromPrimaryStoreCurrency(rateBase, _workContext.WorkingCurrency);
                if (rate > decimal.Zero)
                {
                    pmModel.Fee = _priceFormatter.FormatPaymentMethodAdditionalFee(rate, true);
                }

                model.PaymentMethods.Add(pmModel);
            }

            //find a selected (previously) payment method
            var selectedPaymentMethodSystemName = _workContext.CurrentCustomer.GetAttribute<string>(
                SystemCustomerAttributeNames.SelectedPaymentMethod,
                _genericAttributeService, _storeContext.CurrentStore.Id);
            if (!String.IsNullOrEmpty(selectedPaymentMethodSystemName))
            {
                var paymentMethodToSelect = model.PaymentMethods.ToList()
                    .Find(pm => pm.PaymentMethodSystemName.Equals(selectedPaymentMethodSystemName, StringComparison.InvariantCultureIgnoreCase));
                if (paymentMethodToSelect != null)
                {
                    paymentMethodToSelect.Selected = true;
                }
            }
            //if no option has been selected, let's do it for the first one
            if (model.PaymentMethods.FirstOrDefault(so => so.Selected) == null)
            {
                var paymentMethodToSelect = model.PaymentMethods.FirstOrDefault();
                if (paymentMethodToSelect != null)
                {
                    paymentMethodToSelect.Selected = true;
                }
            }

            return model;
        }

        [NonAction]
        protected virtual CheckoutPaymentInfoModel PreparePaymentInfoModel(IPaymentMethod paymentMethod)
        {
            var model = new CheckoutPaymentInfoModel();
            string actionName;
            string controllerName;
            RouteValueDictionary routeValues;
            paymentMethod.GetPaymentInfoRoute(out actionName, out controllerName, out routeValues);
            model.PaymentInfoActionName = actionName;
            model.PaymentInfoControllerName = controllerName;
            model.PaymentInfoRouteValues = routeValues;
            model.DisplayOrderTotals = _orderSettings.OnePageCheckoutDisplayOrderTotalsOnPaymentInfoTab;

            return model;
        }

        [NonAction]
        protected virtual CheckoutConfirmModel PrepareConfirmOrderModel(IList<ShoppingCartItem> cart)
        {
            var model = new CheckoutConfirmModel();
            //terms of service
            model.TermsOfServiceOnOrderConfirmPage = _orderSettings.TermsOfServiceOnOrderConfirmPage;
            //min order amount validation
            bool minOrderTotalAmountOk = _orderProcessingService.ValidateMinOrderTotalAmount(cart);
            if (!minOrderTotalAmountOk)
            {
                decimal minOrderTotalAmount = _currencyService.ConvertFromPrimaryStoreCurrency(_orderSettings.MinOrderTotalAmount, _workContext.WorkingCurrency);
                model.MinOrderTotalWarning = string.Format(_localizationService.GetResource("Checkout.MinOrderTotalAmount"), _priceFormatter.FormatPrice(minOrderTotalAmount, true, false));
            }
            return model;
        }

        [NonAction]
        protected virtual bool IsMinimumOrderPlacementIntervalValid(Customer customer)
        {
            //prevent 2 orders being placed within an X seconds time frame
            if (_orderSettings.MinimumOrderPlacementInterval == 0)
            {
                return true;
            }

            var lastOrder = _orderService.SearchOrders(storeId: _storeContext.CurrentStore.Id,
                customerId: _workContext.CurrentCustomer.Id, pageSize: 1)
                .FirstOrDefault();
            if (lastOrder == null)
            {
                return true;
            }

            var interval = DateTime.UtcNow - lastOrder.CreatedOnUtc;
            return interval.TotalSeconds > _orderSettings.MinimumOrderPlacementInterval;
        }

        #endregion

        #region Methods (common)

        public ActionResult Index()
        {
            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();
            if (!cart.Any())
            {
                return RedirectToRoute("ShoppingCart");
            }

            bool downloadableProductsRequireRegistration =
                _customerSettings.RequireRegistrationForDownloadableProducts && cart.Any(sci => sci.Product.IsDownload);

            if (_workContext.CurrentCustomer.IsGuest()
                && (!_orderSettings.AnonymousCheckoutAllowed
                    || downloadableProductsRequireRegistration)
                )
            {
                return new HttpUnauthorizedResult();
            }

            //reset checkout data
            _customerService.ResetCheckoutData(_workContext.CurrentCustomer, _storeContext.CurrentStore.Id);

            //validation (cart)
            var checkoutAttributesXml = _workContext.CurrentCustomer.GetAttribute<string>(SystemCustomerAttributeNames.CheckoutAttributes, _genericAttributeService, _storeContext.CurrentStore.Id);
            var scWarnings = _shoppingCartService.GetShoppingCartWarnings(cart, checkoutAttributesXml, true);
            if (scWarnings.Any())
            {
                return RedirectToRoute("ShoppingCart");
            }
            //validation (each shopping cart item)
            foreach (ShoppingCartItem sci in cart)
            {
                var sciWarnings = _shoppingCartService.GetShoppingCartItemWarnings(_workContext.CurrentCustomer,
                    sci.ShoppingCartType,
                    sci.Product,
                    sci.StoreId,
                    sci.AttributesXml,
                    sci.CustomerEnteredPrice,
                    sci.RentalStartDateUtc,
                    sci.RentalEndDateUtc,
                    sci.Quantity,
                    false);
                if (sciWarnings.Any())
                {
                    return RedirectToRoute("ShoppingCart");
                }
            }

            if (_orderSettings.OnePageCheckoutEnabled)
            {
                return RedirectToRoute("CheckoutOnePage");
            }

            return RedirectToRoute("CheckoutBillingAddress");
        }

        public ActionResult Completed(int? orderId)
        {
            //validation
            if (_workContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed)
            {
                return new HttpUnauthorizedResult();
            }

            Order order = null;
            if (orderId.HasValue)
            {
                //load order by identifier (if provided)
                order = _orderService.GetOrderById(orderId.Value);
            }
            if (order == null)
            {
                order = _orderService.SearchOrders(storeId: _storeContext.CurrentStore.Id,
                customerId: _workContext.CurrentCustomer.Id, pageSize: 1)
                    .FirstOrDefault();
            }
            if (order == null || order.Deleted || _workContext.CurrentCustomer.Id != order.CustomerId)
            {
                return RedirectToRoute("HomePage");
            }

            //disable "order completed" page?
            if (_orderSettings.DisableOrderCompletedPage)
            {
                return RedirectToRoute("OrderDetails", new { orderId = order.Id });
            }

            var orderites = _orderService.GetOrderItemsByOrderID(Convert.ToInt32(order.Id));
            //model
            var model = new CheckoutCompletedModel
            {
                OrderId = order.Id,
                OnePageCheckoutEnabled = _orderSettings.OnePageCheckoutEnabled,
                Tax = Convert.ToString(order.OrderTax),
                Authid = Convert.ToString(order.AuthorizationTransactionId),
                shipping = Convert.ToString(order.OrderShippingInclTax)
            };

            foreach (var dataitems in orderites)
            {
                model.ProductItems = new List<CheckoutCompletedModel.itemDetails>
                {
                    new CheckoutCompletedModel.itemDetails
                    {
                        ProducName =dataitems.Product.Name,
                        price =Convert.ToString(dataitems.Product.Price),
                        Quantity =Convert.ToString(dataitems.Quantity),
                        ProductId =Convert.ToString(dataitems.Product.Id)
                    }
                };

            }
            return View(model);
        }
        #endregion

        #region Methods (multistep checkout)
        public ActionResult BillingAddress(FormCollection form)
        {
            //validation
            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();
            if (!cart.Any())
            {
                return RedirectToRoute("ShoppingCart");
            }

            if (_orderSettings.OnePageCheckoutEnabled)
            {
                return RedirectToRoute("CheckoutOnePage");
            }

            if (_workContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed)
            {
                return new HttpUnauthorizedResult();
            }

            //model
            var model = PrepareBillingAddressModel(cart, prePopulateNewAddressWithCustomerFields: true);

            //check whether "billing address" step is enabled
            if (_orderSettings.DisableBillingAddressCheckoutStep)
            {
                if (model.ExistingAddresses.Any())
                {
                    //choose the first one
                    return SelectBillingAddress(model.ExistingAddresses.First().Id);
                }

                TryValidateModel(model);
                TryValidateModel(model.NewAddress);
                return NewBillingAddress(model, form);
            }
            return View(model);
        }

        public ActionResult SelectBillingAddress(int addressId, bool shipToSameAddress = false)
        {
            var address = _workContext.CurrentCustomer.Addresses.FirstOrDefault(a => a.Id == addressId);
            if (address == null)
            {
                return RedirectToRoute("CheckoutBillingAddress");
            }

            _workContext.CurrentCustomer.BillingAddress = address;
            _customerService.UpdateCustomer(_workContext.CurrentCustomer);

            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();

            //ship to the same address?
            if (_shippingSettings.ShipToSameAddress && shipToSameAddress && cart.RequiresShipping())
            {
                _workContext.CurrentCustomer.ShippingAddress = _workContext.CurrentCustomer.BillingAddress;
                _customerService.UpdateCustomer(_workContext.CurrentCustomer);
                //reset selected shipping method (in case if "pick up in store" was selected)
                _genericAttributeService.SaveAttribute<ShippingOption>(_workContext.CurrentCustomer, SystemCustomerAttributeNames.SelectedShippingOption, null, _storeContext.CurrentStore.Id);
                _genericAttributeService.SaveAttribute<PickupPoint>(_workContext.CurrentCustomer, SystemCustomerAttributeNames.SelectedPickupPoint, null, _storeContext.CurrentStore.Id);
                //limitation - "Ship to the same address" doesn't properly work in "pick up in store only" case (when no shipping plugins are available) 
                return RedirectToRoute("CheckoutShippingMethod");
            }

            return RedirectToRoute("CheckoutShippingAddress");
        }
        [HttpPost, ActionName("BillingAddress")]
        [FormValueRequired("nextstep")]
        [ValidateInput(false)]
        public ActionResult NewBillingAddress(CheckoutBillingAddressModel model, FormCollection form)
        {
            //validation
            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();
            if (!cart.Any())
            {
                return RedirectToRoute("ShoppingCart");
            }

            if (_orderSettings.OnePageCheckoutEnabled)
            {
                return RedirectToRoute("CheckoutOnePage");
            }

            if (_workContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed)
            {
                return new HttpUnauthorizedResult();
            }

            //custom address attributes
            var customAttributes = form.ParseCustomAddressAttributes(_addressAttributeParser, _addressAttributeService);
            var customAttributeWarnings = _addressAttributeParser.GetAttributeWarnings(customAttributes);
            foreach (var error in customAttributeWarnings)
            {
                ModelState.AddModelError("", error);
            }

            if (ModelState.IsValid)
            {
                //try to find an address with the same values (don't duplicate records)
                var address = _workContext.CurrentCustomer.Addresses.ToList().FindAddress(
                    model.NewAddress.FirstName, model.NewAddress.LastName, model.NewAddress.PhoneNumber,
                    model.NewAddress.Email, model.NewAddress.FaxNumber, model.NewAddress.Company,
                    model.NewAddress.Address1, model.NewAddress.Address2, model.NewAddress.City,
                    model.NewAddress.StateProvinceId, model.NewAddress.ZipPostalCode,
                    model.NewAddress.CountryId, customAttributes);
                if (address == null)
                {
                    //address is not found. let's create a new one
                    address = model.NewAddress.ToEntity();
                    address.CustomAttributes = customAttributes;
                    address.CreatedOnUtc = DateTime.UtcNow;
                    //some validation
                    if (address.CountryId == 0)
                    {
                        address.CountryId = null;
                    }

                    if (address.StateProvinceId == 0)
                    {
                        address.StateProvinceId = null;
                    }

                    _workContext.CurrentCustomer.Addresses.Add(address);
                }
                _workContext.CurrentCustomer.BillingAddress = address;
                _customerService.UpdateCustomer(_workContext.CurrentCustomer);

                //ship to the same address?
                if (_shippingSettings.ShipToSameAddress && model.ShipToSameAddress && cart.RequiresShipping())
                {
                    _workContext.CurrentCustomer.ShippingAddress = _workContext.CurrentCustomer.BillingAddress;
                    _customerService.UpdateCustomer(_workContext.CurrentCustomer);
                    //reset selected shipping method (in case if "pick up in store" was selected)
                    _genericAttributeService.SaveAttribute<ShippingOption>(_workContext.CurrentCustomer, SystemCustomerAttributeNames.SelectedShippingOption, null, _storeContext.CurrentStore.Id);
                    _genericAttributeService.SaveAttribute<PickupPoint>(_workContext.CurrentCustomer, SystemCustomerAttributeNames.SelectedPickupPoint, null, _storeContext.CurrentStore.Id);
                    //limitation - "Ship to the same address" doesn't properly work in "pick up in store only" case (when no shipping plugins are available) 
                    return RedirectToRoute("CheckoutShippingMethod");
                }

                return RedirectToRoute("CheckoutShippingAddress");
            }


            //If we got this far, something failed, redisplay form
            model = PrepareBillingAddressModel(cart,
                selectedCountryId: model.NewAddress.CountryId,
                overrideAttributesXml: customAttributes);
            return View(model);
        }

        public ActionResult ShippingAddress()
        {
            //validation
            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();
            if (!cart.Any())
            {
                return RedirectToRoute("ShoppingCart");
            }

            if (_orderSettings.OnePageCheckoutEnabled)
            {
                return RedirectToRoute("CheckoutOnePage");
            }

            if (_workContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed)
            {
                return new HttpUnauthorizedResult();
            }

            if (!cart.RequiresShipping())
            {
                _workContext.CurrentCustomer.ShippingAddress = null;
                _customerService.UpdateCustomer(_workContext.CurrentCustomer);
                return RedirectToRoute("CheckoutShippingMethod");
            }

            //model
            var model = PrepareShippingAddressModel(prePopulateNewAddressWithCustomerFields: true);
            return View(model);
        }
        public ActionResult SelectShippingAddress(int addressId)
        {
            var address = _workContext.CurrentCustomer.Addresses.FirstOrDefault(a => a.Id == addressId);
            if (address == null)
            {
                return RedirectToRoute("CheckoutShippingAddress");
            }

            _workContext.CurrentCustomer.ShippingAddress = address;
            _customerService.UpdateCustomer(_workContext.CurrentCustomer);

            if (_shippingSettings.AllowPickUpInStore)
            {
                //set value indicating that "pick up in store" option has not been chosen
                _genericAttributeService.SaveAttribute<PickupPoint>(_workContext.CurrentCustomer, SystemCustomerAttributeNames.SelectedPickupPoint, null, _storeContext.CurrentStore.Id);
            }

            return RedirectToRoute("CheckoutShippingMethod");
        }
        [HttpPost, ActionName("ShippingAddress")]
        [FormValueRequired("nextstep")]
        [ValidateInput(false)]
        public ActionResult NewShippingAddress(CheckoutShippingAddressModel model, FormCollection form)
        {
            //validation
            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();
            if (!cart.Any())
            {
                return RedirectToRoute("ShoppingCart");
            }

            if (_orderSettings.OnePageCheckoutEnabled)
            {
                return RedirectToRoute("CheckoutOnePage");
            }

            if (_workContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed)
            {
                return new HttpUnauthorizedResult();
            }

            if (!cart.RequiresShipping())
            {
                _workContext.CurrentCustomer.ShippingAddress = null;
                _customerService.UpdateCustomer(_workContext.CurrentCustomer);
                return RedirectToRoute("CheckoutShippingMethod");
            }

            //pickup point
            if (_shippingSettings.AllowPickUpInStore)
            {
                if (model.PickUpInStore)
                {
                    //no shipping address selected
                    _workContext.CurrentCustomer.ShippingAddress = null;
                    _customerService.UpdateCustomer(_workContext.CurrentCustomer);

                    var pickupPoint = form["pickup-points-id"].Split(new[] { "___" }, StringSplitOptions.None);
                    var pickupPoints = _shippingService
                        .GetPickupPoints(_workContext.CurrentCustomer.BillingAddress, pickupPoint[1], _storeContext.CurrentStore.Id).PickupPoints.ToList();
                    var selectedPoint = pickupPoints.FirstOrDefault(x => x.Id.Equals(pickupPoint[0]));
                    if (selectedPoint == null)
                    {
                        return RedirectToRoute("CheckoutShippingAddress");
                    }

                    var pickUpInStoreShippingOption = new ShippingOption
                    {
                        Name = string.Format(_localizationService.GetResource("Checkout.PickupPoints.Name"), selectedPoint.Name),
                        Rate = selectedPoint.PickupFee,
                        Description = selectedPoint.Description,
                        ShippingRateComputationMethodSystemName = selectedPoint.ProviderSystemName
                    };

                    _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer, SystemCustomerAttributeNames.SelectedShippingOption, pickUpInStoreShippingOption, _storeContext.CurrentStore.Id);
                    _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer, SystemCustomerAttributeNames.SelectedPickupPoint, selectedPoint, _storeContext.CurrentStore.Id);

                    return RedirectToRoute("CheckoutPaymentMethod");
                }

                //set value indicating that "pick up in store" option has not been chosen
                _genericAttributeService.SaveAttribute<PickupPoint>(_workContext.CurrentCustomer, SystemCustomerAttributeNames.SelectedPickupPoint, null, _storeContext.CurrentStore.Id);
            }

            //custom address attributes
            var customAttributes = form.ParseCustomAddressAttributes(_addressAttributeParser, _addressAttributeService);
            var customAttributeWarnings = _addressAttributeParser.GetAttributeWarnings(customAttributes);
            foreach (var error in customAttributeWarnings)
            {
                ModelState.AddModelError("", error);
            }

            if (ModelState.IsValid)
            {
                //try to find an address with the same values (don't duplicate records)
                var address = _workContext.CurrentCustomer.Addresses.ToList().FindAddress(
                    model.NewAddress.FirstName, model.NewAddress.LastName, model.NewAddress.PhoneNumber,
                    model.NewAddress.Email, model.NewAddress.FaxNumber, model.NewAddress.Company,
                    model.NewAddress.Address1, model.NewAddress.Address2, model.NewAddress.City,
                    model.NewAddress.StateProvinceId, model.NewAddress.ZipPostalCode,
                    model.NewAddress.CountryId, customAttributes);
                if (address == null)
                {
                    address = model.NewAddress.ToEntity();
                    address.CustomAttributes = customAttributes;
                    address.CreatedOnUtc = DateTime.UtcNow;
                    //some validation
                    if (address.CountryId == 0)
                    {
                        address.CountryId = null;
                    }

                    if (address.StateProvinceId == 0)
                    {
                        address.StateProvinceId = null;
                    }

                    _workContext.CurrentCustomer.Addresses.Add(address);
                }
                _workContext.CurrentCustomer.ShippingAddress = address;
                _customerService.UpdateCustomer(_workContext.CurrentCustomer);

                return RedirectToRoute("CheckoutShippingMethod");
            }


            //If we got this far, something failed, redisplay form
            model = PrepareShippingAddressModel(
                selectedCountryId: model.NewAddress.CountryId,
                overrideAttributesXml: customAttributes);
            return View(model);
        }


        public ActionResult ShippingMethod()
        {
            //validation
            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();
            if (!cart.Any())
            {
                return RedirectToRoute("ShoppingCart");
            }

            if (_orderSettings.OnePageCheckoutEnabled)
            {
                return RedirectToRoute("CheckoutOnePage");
            }

            if (_workContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed)
            {
                return new HttpUnauthorizedResult();
            }

            if (!cart.RequiresShipping())
            {
                _genericAttributeService.SaveAttribute<ShippingOption>(_workContext.CurrentCustomer, SystemCustomerAttributeNames.SelectedShippingOption, null, _storeContext.CurrentStore.Id);
                return RedirectToRoute("CheckoutPaymentMethod");
            }

            //model
            var model = PrepareShippingMethodModel(cart, _workContext.CurrentCustomer.ShippingAddress);

            #region SODMYWAY-
            /* if (_shippingSettings.BypassShippingMethodSelectionIfOnlyOne &&
                model.ShippingMethods.Count == 1)
            {
                //if we have only one shipping method, then a customer doesn't have to choose a shipping method
                _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer, 
                    SystemCustomerAttributeNames.SelectedShippingOption,
                    model.ShippingMethods.First().ShippingOption,
                    _storeContext.CurrentStore.Id);
            
                    return RedirectToRoute("CheckoutPaymentMethod");
                } */
            #endregion

            return View(model);
        }
        [HttpPost, ActionName("ShippingMethod")]
        [FormValueRequired("nextstep")]
        [ValidateInput(false)]
        public ActionResult SelectShippingMethod(string shippingoption)
        {
            //validation
            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();
            if (!cart.Any())
            {
                return RedirectToRoute("ShoppingCart");
            }

            if (_orderSettings.OnePageCheckoutEnabled)
            {
                return RedirectToRoute("CheckoutOnePage");
            }

            if (_workContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed)
            {
                return new HttpUnauthorizedResult();
            }

            if (!cart.RequiresShipping())
            {
                _genericAttributeService.SaveAttribute<ShippingOption>(_workContext.CurrentCustomer,
                    SystemCustomerAttributeNames.SelectedShippingOption, null, _storeContext.CurrentStore.Id);
                return RedirectToRoute("CheckoutPaymentMethod");
            }

            //parse selected method 
            if (String.IsNullOrEmpty(shippingoption))
            {
                return ShippingMethod();
            }

            var splittedOption = shippingoption.Split(new[] { "___" }, StringSplitOptions.RemoveEmptyEntries);
            if (splittedOption.Length != 2)
            {
                return ShippingMethod();
            }

            string selectedName = splittedOption[0];
            string shippingRateComputationMethodSystemName = splittedOption[1];

            //find it
            //performance optimization. try cache first
            var shippingOptions = _workContext.CurrentCustomer.GetAttribute<List<ShippingOption>>(SystemCustomerAttributeNames.OfferedShippingOptions, _storeContext.CurrentStore.Id);
            if (shippingOptions == null || !shippingOptions.Any())
            {
                //not found? let's load them using shipping service
                shippingOptions = _shippingService
                    .GetShippingOptions(cart, _workContext.CurrentCustomer.ShippingAddress, shippingRateComputationMethodSystemName, _storeContext.CurrentStore.Id)
                    .ShippingOptions
                    .ToList();
            }
            else
            {
                //loaded cached results. let's filter result by a chosen shipping rate computation method
                shippingOptions = shippingOptions.Where(so => so.ShippingRateComputationMethodSystemName.Equals(shippingRateComputationMethodSystemName, StringComparison.InvariantCultureIgnoreCase))
                    .ToList();
            }

            var shippingOption = shippingOptions
                .Find(so => !String.IsNullOrEmpty(so.Name) && so.Name.Equals(selectedName, StringComparison.InvariantCultureIgnoreCase));
            if (shippingOption == null)
            {
                return ShippingMethod();
            }

            //save
            _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer, SystemCustomerAttributeNames.SelectedShippingOption, shippingOption, _storeContext.CurrentStore.Id);

            return RedirectToRoute("CheckoutPaymentMethod");
        }


        public ActionResult PaymentMethod()
        {
            //validation
            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();
            if (!cart.Any())
            {
                return RedirectToRoute("ShoppingCart");
            }

            if (_orderSettings.OnePageCheckoutEnabled)
            {
                return RedirectToRoute("CheckoutOnePage");
            }

            if (_workContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed)
            {
                return new HttpUnauthorizedResult();
            }

            //Check whether payment workflow is required
            //we ignore reward points during cart total calculation
            bool isPaymentWorkflowRequired = IsPaymentWorkflowRequired(cart, false);	// SODMYWAY-
            if (!isPaymentWorkflowRequired)
            {
                _genericAttributeService.SaveAttribute<string>(_workContext.CurrentCustomer,
                    SystemCustomerAttributeNames.SelectedPaymentMethod, null, _storeContext.CurrentStore.Id);
                return RedirectToRoute("CheckoutPaymentInfo");
            }

            //filter by country
            int filterByCountryId = 0;
            if (_addressSettings.CountryEnabled &&
                _workContext.CurrentCustomer.BillingAddress != null &&
                _workContext.CurrentCustomer.BillingAddress.Country != null)
            {
                filterByCountryId = _workContext.CurrentCustomer.BillingAddress.Country.Id;
            }

            //model
            var paymentMethodModel = PreparePaymentMethodModel(cart, filterByCountryId);

            if (_paymentSettings.BypassPaymentMethodSelectionIfOnlyOne &&
                paymentMethodModel.PaymentMethods.Count == 1 && !paymentMethodModel.DisplayRewardPoints)
            {
                //if we have only one payment method and reward points are disabled or the current customer doesn't have any reward points
                //so customer doesn't have to choose a payment method

                _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer,
                    SystemCustomerAttributeNames.SelectedPaymentMethod,
                    paymentMethodModel.PaymentMethods[0].PaymentMethodSystemName,
                    _storeContext.CurrentStore.Id);
                return RedirectToRoute("CheckoutPaymentInfo");
            }

            return View(paymentMethodModel);
        }
        [HttpPost, ActionName("PaymentMethod")]
        [FormValueRequired("nextstep")]
        [ValidateInput(false)]
        public ActionResult SelectPaymentMethod(string paymentmethod, CheckoutPaymentMethodModel model)
        {
            //validation
            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();
            if (!cart.Any())
            {
                return RedirectToRoute("ShoppingCart");
            }

            if (_orderSettings.OnePageCheckoutEnabled)
            {
                return RedirectToRoute("CheckoutOnePage");
            }

            if (_workContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed)
            {
                return new HttpUnauthorizedResult();
            }

            //reward points
            if (_rewardPointsSettings.Enabled)
            {
                _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer,
                    SystemCustomerAttributeNames.UseRewardPointsDuringCheckout, model.UseRewardPoints,
                    _storeContext.CurrentStore.Id);
            }

            //Check whether payment workflow is required
            bool isPaymentWorkflowRequired = IsPaymentWorkflowRequired(cart);
            if (!isPaymentWorkflowRequired)
            {
                _genericAttributeService.SaveAttribute<string>(_workContext.CurrentCustomer,
                    SystemCustomerAttributeNames.SelectedPaymentMethod, null, _storeContext.CurrentStore.Id);
                return RedirectToRoute("CheckoutPaymentInfo");
            }
            //payment method 
            if (String.IsNullOrEmpty(paymentmethod))
            {
                return PaymentMethod();
            }

            var paymentMethodInst = _paymentService.LoadPaymentMethodBySystemName(paymentmethod);
            if (paymentMethodInst == null ||
                !paymentMethodInst.IsPaymentMethodActive(_paymentSettings) ||
                !_pluginFinder.AuthenticateStore(paymentMethodInst.PluginDescriptor, _storeContext.CurrentStore.Id))
            {
                return PaymentMethod();
            }

            //save
            _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer,
                SystemCustomerAttributeNames.SelectedPaymentMethod, paymentmethod, _storeContext.CurrentStore.Id);

            return RedirectToRoute("CheckoutPaymentInfo");
        }


        public ActionResult PaymentInfo()
        {
            //validation
            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();
            if (!cart.Any())
            {
                return RedirectToRoute("ShoppingCart");
            }

            if (_orderSettings.OnePageCheckoutEnabled)
            {
                return RedirectToRoute("CheckoutOnePage");
            }

            if (_workContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed)
            {
                return new HttpUnauthorizedResult();
            }

            //Check whether payment workflow is required
            bool isPaymentWorkflowRequired = IsPaymentWorkflowRequired(cart);
            if (!isPaymentWorkflowRequired)
            {
                return RedirectToRoute("CheckoutConfirm");
            }

            //load payment method
            var paymentMethodSystemName = _workContext.CurrentCustomer.GetAttribute<string>(
                SystemCustomerAttributeNames.SelectedPaymentMethod,
                _genericAttributeService, _storeContext.CurrentStore.Id);
            var paymentMethod = _paymentService.LoadPaymentMethodBySystemName(paymentMethodSystemName);
            if (paymentMethod == null)
            {
                return RedirectToRoute("CheckoutPaymentMethod");
            }

            //Check whether payment info should be skipped
            if (paymentMethod.SkipPaymentInfo)
            {
                //skip payment info page
                var paymentInfo = new ProcessPaymentRequest();
                //session save
                _httpContext.Session["OrderPaymentInfo"] = paymentInfo;

                return RedirectToRoute("CheckoutConfirm");
            }

            //model
            var model = PreparePaymentInfoModel(paymentMethod);
            return View(model);
        }
        [HttpPost, ActionName("PaymentInfo")]
        [FormValueRequired("nextstep")]
        [ValidateInput(false)]
        public ActionResult EnterPaymentInfo(FormCollection form)
        {
            //validation
            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();
            if (!cart.Any())
            {
                return RedirectToRoute("ShoppingCart");
            }

            if (_orderSettings.OnePageCheckoutEnabled)
            {
                return RedirectToRoute("CheckoutOnePage");
            }

            if (_workContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed)
            {
                return new HttpUnauthorizedResult();
            }

            //Check whether payment workflow is required
            bool isPaymentWorkflowRequired = IsPaymentWorkflowRequired(cart);
            if (!isPaymentWorkflowRequired)
            {
                return RedirectToRoute("CheckoutConfirm");
            }

            //load payment method
            var paymentMethodSystemName = _workContext.CurrentCustomer.GetAttribute<string>(
                SystemCustomerAttributeNames.SelectedPaymentMethod,
                _genericAttributeService, _storeContext.CurrentStore.Id);
            var paymentMethod = _paymentService.LoadPaymentMethodBySystemName(paymentMethodSystemName);
            if (paymentMethod == null)
            {
                return RedirectToRoute("CheckoutPaymentMethod");
            }

            var paymentControllerType = paymentMethod.GetControllerType();
            var paymentController = DependencyResolver.Current.GetService(paymentControllerType) as BasePaymentController;
            if (paymentController == null)
            {
                throw new Exception("Payment controller cannot be loaded");
            }

            var warnings = paymentController.ValidatePaymentForm(form);
            foreach (var warning in warnings)
            {
                ModelState.AddModelError("", warning);
            }

            if (ModelState.IsValid)
            {
                //get payment info
                var paymentInfo = paymentController.GetPaymentInfo(form);
                //session save
                _httpContext.Session["OrderPaymentInfo"] = paymentInfo;
                return RedirectToRoute("CheckoutConfirm");
            }

            //If we got this far, something failed, redisplay form
            //model
            var model = PreparePaymentInfoModel(paymentMethod);
            return View(model);
        }


        public ActionResult Confirm()
        {
            //validation
            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();
            if (!cart.Any())
            {
                return RedirectToRoute("ShoppingCart");
            }

            if (_orderSettings.OnePageCheckoutEnabled)
            {
                return RedirectToRoute("CheckoutOnePage");
            }

            if (_workContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed)
            {
                return new HttpUnauthorizedResult();
            }

            //model
            var model = PrepareConfirmOrderModel(cart);
            return View(model);
        }
        [HttpPost, ActionName("Confirm")]
        [ValidateInput(false)]
        public ActionResult ConfirmOrder()
        {
            //validation
            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();
            if (!cart.Any())
            {
                return RedirectToRoute("ShoppingCart");
            }

            if (_orderSettings.OnePageCheckoutEnabled)
            {
                return RedirectToRoute("CheckoutOnePage");
            }

            if (_workContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed)
            {
                return new HttpUnauthorizedResult();
            }


            //model
            var model = PrepareConfirmOrderModel(cart);
            try
            {
                var processPaymentRequest = _httpContext.Session["OrderPaymentInfo"] as ProcessPaymentRequest;
                if (processPaymentRequest == null)
                {
                    //Check whether payment workflow is required
                    if (IsPaymentWorkflowRequired(cart))
                    {
                        return RedirectToRoute("CheckoutPaymentInfo");
                    }

                    processPaymentRequest = new ProcessPaymentRequest();
                }

                //prevent 2 orders being placed within an X seconds time frame
                if (!IsMinimumOrderPlacementIntervalValid(_workContext.CurrentCustomer))
                {
                    throw new Exception(_localizationService.GetResource("Checkout.MinOrderPlacementInterval"));
                }

                //place order
                processPaymentRequest.StoreId = _storeContext.CurrentStore.Id;
                processPaymentRequest.CustomerId = _workContext.CurrentCustomer.Id;
                processPaymentRequest.PaymentMethodSystemName = _workContext.CurrentCustomer.GetAttribute<string>(
                    SystemCustomerAttributeNames.SelectedPaymentMethod,
                    _genericAttributeService, _storeContext.CurrentStore.Id);
                var placeOrderResult = _orderProcessingService.PlaceOrder(processPaymentRequest);
                if (placeOrderResult.Success)
                {
                    _httpContext.Session["OrderPaymentInfo"] = null;
                    var postProcessPaymentRequest = new PostProcessPaymentRequest
                    {
                        Order = placeOrderResult.PlacedOrder
                    };
                    _paymentService.PostProcessPayment(postProcessPaymentRequest);

                    if (_webHelper.IsRequestBeingRedirected || _webHelper.IsPostBeingDone)
                    {
                        //redirection or POST has been done in PostProcessPayment
                        return Content("Redirected");
                    }

                    return RedirectToRoute("CheckoutCompleted", new { orderId = placeOrderResult.PlacedOrder.Id });
                }

                foreach (var error in placeOrderResult.Errors)
                {
                    model.Warnings.Add(error);
                }
            }
            catch (Exception exc)
            {
                _logger.Warning(exc.Message, exc);
                model.Warnings.Add(exc.Message);
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }


        [ChildActionOnly]
        public ActionResult CheckoutProgress(CheckoutProgressStep step)
        {
            var model = new CheckoutProgressModel { CheckoutProgressStep = step };
            return PartialView(model);
        }

        #endregion

        #region Methods (one page checkout)

        [NonAction]
        protected JsonResult OpcLoadStepAfterShippingAddress(List<ShoppingCartItem> cart)
        {
            var shippingMethodModel = PrepareShippingMethodModel(cart, _workContext.CurrentCustomer.ShippingAddress);
            #region SODMYWAY-
            /* if (_shippingSettings.BypassShippingMethodSelectionIfOnlyOne &&
                shippingMethodModel.ShippingMethods.Count == 1)
            {
                //if we have only one shipping method, then a customer doesn't have to choose a shipping method
                _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer,
                    SystemCustomerAttributeNames.SelectedShippingOption,
                    shippingMethodModel.ShippingMethods.First().ShippingOption,
                    _storeContext.CurrentStore.Id);

                //load next step
                return OpcLoadStepAfterShippingMethod(cart);
            } */
            #endregion

            return Json(new
            {
                update_section = new UpdateSectionJsonModel
                {
                    name = "shipping-method",
                    html = this.RenderPartialViewToString("OpcShippingMethods", shippingMethodModel)
                },
                goto_section = "shipping_method"
            });
        }

        [NonAction]
        protected JsonResult OpcLoadStepAfterShippingMethod(List<ShoppingCartItem> cart)
        {
            //Check whether payment workflow is required
            //we ignore reward points during cart total calculation
            bool isPaymentWorkflowRequired = IsPaymentWorkflowRequired(cart, false);

            bool isTroveEnabled = false;
            isTroveEnabled = Convert.ToBoolean(Session["TroveEnabledStore"]);

            UIResponseVieModel resModel = new UIResponseVieModel();
            TroveDetailCustomerModel customer = new TroveDetailCustomerModel();
            if (!String.IsNullOrEmpty(ConfigurationManager.AppSettings["TroveInstitutionId"]))
            {
                customer.InstitutionId = ConfigurationManager.AppSettings["TroveInstitutionId"];
            }

            customer.FirstName = _workContext.CurrentCustomer.BillingAddress.FirstName;
            customer.LastName = _workContext.CurrentCustomer.BillingAddress.LastName;
            customer.Email = _workContext.CurrentCustomer.BillingAddress.Email;
            customer.Phone = _workContext.CurrentCustomer.BillingAddress.PhoneNumber;
            resModel.Customer = customer;

            if (isTroveEnabled)
            {
                return Json(new
                {
                    update_section = new UpdateSectionJsonModel
                    {
                        name = "payment-method",
                        html =  RenderPartialViewToString("OpcTroveUserDetails", resModel)
                    },
                    goto_section = "payment_method"
                });
            }

            if (isPaymentWorkflowRequired)
            {
                //filter by country
                int filterByCountryId = 0;
                if (_addressSettings.CountryEnabled &&
                    _workContext.CurrentCustomer.BillingAddress != null &&
                    _workContext.CurrentCustomer.BillingAddress.Country != null)
                {
                    filterByCountryId = _workContext.CurrentCustomer.BillingAddress.Country.Id;
                }

                //payment is required
                var paymentMethodModel = PreparePaymentMethodModel(cart, filterByCountryId);

                if (_paymentSettings.BypassPaymentMethodSelectionIfOnlyOne &&
                    paymentMethodModel.PaymentMethods.Count == 1 && !paymentMethodModel.DisplayRewardPoints)
                {
                    //if we have only one payment method and reward points are disabled or the current customer doesn't have any reward points
                    //so customer doesn't have to choose a payment method

                    var selectedPaymentMethodSystemName = paymentMethodModel.PaymentMethods[0].PaymentMethodSystemName;
                    _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer,
                        SystemCustomerAttributeNames.SelectedPaymentMethod,
                        selectedPaymentMethodSystemName, _storeContext.CurrentStore.Id);

                    var paymentMethodInst = _paymentService.LoadPaymentMethodBySystemName(selectedPaymentMethodSystemName);
                    if (paymentMethodInst == null ||
                        !paymentMethodInst.IsPaymentMethodActive(_paymentSettings) ||
                        !_pluginFinder.AuthenticateStore(paymentMethodInst.PluginDescriptor, _storeContext.CurrentStore.Id))
                    {
                        throw new Exception("Selected payment method can't be parsed");
                    }

                    //bool isTroveEnabled = false;
                    //isTroveEnabled = Convert.ToBoolean(Session["TroveEnabledStore"]);

                    //UIResponseVieModel resModel = new UIResponseVieModel();
                    //if (isTroveEnabled)
                    //{
                    //    return Json(new
                    //    {
                    //        update_section = new UpdateSectionJsonModel
                    //        {
                    //            name = "payment-method",
                    //            html = this.RenderPartialViewToString("OpcTroveUserDetails", resModel)
                    //        },
                    //        goto_section = "payment_method"
                    //    });
                    //}
                    //else
                    //{
                        return OpcLoadStepAfterPaymentMethod(paymentMethodInst, cart);
                    //}
                }

                //customer have to choose a payment method
                return Json(new
                {
                    update_section = new UpdateSectionJsonModel
                    {
                        name = "payment-method",
                        html = this.RenderPartialViewToString("OpcPaymentMethods", paymentMethodModel)
                    },
                    goto_section = "payment_method"
                });
            }

            //payment is not required
            _genericAttributeService.SaveAttribute<string>(_workContext.CurrentCustomer,
                SystemCustomerAttributeNames.SelectedPaymentMethod, null, _storeContext.CurrentStore.Id);

            var confirmOrderModel = PrepareConfirmOrderModel(cart);
            return Json(new
            {
                update_section = new UpdateSectionJsonModel
                {
                    name = "confirm-order",
                    html = this.RenderPartialViewToString("OpcConfirmOrder", confirmOrderModel)
                },
                goto_section = "confirm_order"
            });
        }

        [NonAction]
        protected JsonResult OpcLoadStepAfterPaymentMethod(IPaymentMethod paymentMethod, List<ShoppingCartItem> cart)
        {
            if (paymentMethod.SkipPaymentInfo)
            {
                //skip payment info page
                var paymentInfo = new ProcessPaymentRequest();
                //session save
                _httpContext.Session["OrderPaymentInfo"] = paymentInfo;

                var confirmOrderModel = PrepareConfirmOrderModel(cart);
                return Json(new
                {
                    update_section = new UpdateSectionJsonModel
                    {
                        name = "confirm-order",
                        html = this.RenderPartialViewToString("OpcConfirmOrder", confirmOrderModel)
                    },
                    goto_section = "confirm_order"
                });
            }


            //return payment info page
            var paymenInfoModel = PreparePaymentInfoModel(paymentMethod);

            //bool isDisablePayment = false;
            //isDisablePayment = Convert.ToBoolean(Session["TroveEnabledStore"]);

            return Json(new
            {
                update_section = new UpdateSectionJsonModel
                {
                    name = "payment-info",
                    html = this.RenderPartialViewToString( "OpcPaymentInfo", paymenInfoModel)
                },
                goto_section = "payment_info"
            });

        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Calendar_DayChange(string date, int warehouseId, bool pickup, bool delivery)
        {
            Warehouse warehouse = _shippingService.GetWarehouseById(warehouseId);

            DateTime dt = DateTime.ParseExact(date, "yyyy-MM-dd hh:mm tt", CultureInfo.InvariantCulture);


            string dayOfWeek = "";
            string openTime = "";
            string closedTime = "";

            if (pickup && warehouse.AllowPickupTime)
            {
                switch (dt.DayOfWeek)
                {
                    case DayOfWeek.Monday:
                        openTime = warehouse.PickupOpenTimeMon;
                        closedTime = warehouse.PickupCloseTimeMon;
                        break;
                    case DayOfWeek.Tuesday:
                        openTime = warehouse.PickupOpenTimeTues;
                        closedTime = warehouse.PickupCloseTimeTues;
                        break;
                    case DayOfWeek.Wednesday:
                        openTime = warehouse.PickupOpenTimeWeds;
                        closedTime = warehouse.PickupCloseTimeWeds;
                        break;
                    case DayOfWeek.Thursday:
                        openTime = warehouse.PickupOpenTimeThurs;
                        closedTime = warehouse.PickupCloseTimeThurs;
                        break;
                    case DayOfWeek.Friday:
                        openTime = warehouse.PickupOpenTimeFri;
                        closedTime = warehouse.PickupCloseTimeFri;
                        break;
                    case DayOfWeek.Saturday:
                        openTime = warehouse.PickupOpenTimeSat;
                        closedTime = warehouse.PickupCloseTimeSat;
                        break;
                    case DayOfWeek.Sunday:
                        openTime = warehouse.PickupOpenTimeSun;
                        closedTime = warehouse.PickupCloseTimeSun;
                        break;

                }

            }
            else if (delivery && warehouse.AllowDeliveryTime) //Not pickup, must be delivery
            {
                switch (dt.DayOfWeek)
                {
                    case DayOfWeek.Monday:
                        openTime = warehouse.DeliveryOpenTimeMon;
                        closedTime = warehouse.DeliveryCloseTimeMon;
                        break;
                    case DayOfWeek.Tuesday:
                        openTime = warehouse.DeliveryOpenTimeTues;
                        closedTime = warehouse.DeliveryCloseTimeTues;
                        break;
                    case DayOfWeek.Wednesday:
                        openTime = warehouse.DeliveryOpenTimeWeds;
                        closedTime = warehouse.DeliveryCloseTimeWeds;
                        break;
                    case DayOfWeek.Thursday:
                        openTime = warehouse.DeliveryOpenTimeThurs;
                        closedTime = warehouse.DeliveryCloseTimeThurs;
                        break;
                    case DayOfWeek.Friday:
                        openTime = warehouse.DeliveryOpenTimeFri;
                        closedTime = warehouse.DeliveryCloseTimeFri;
                        break;
                    case DayOfWeek.Saturday:
                        openTime = warehouse.DeliveryOpenTimeSat;
                        closedTime = warehouse.DeliveryCloseTimeSat;
                        break;
                    case DayOfWeek.Sunday:
                        openTime = warehouse.DeliveryOpenTimeSun;
                        closedTime = warehouse.DeliveryCloseTimeSun;
                        break;

                }



            }



            if (openTime != "")
            {
                if (dt.Date == DateTime.Today)
                {
                    //need to get nearest time.
                    DateTime nearestTime = RoundUp(DateTime.Now, TimeSpan.FromMinutes(30));
                    openTime = nearestTime.ToString("hh:mm tt");
                }

            }



            var formattedTimes = openTime != "" ? FormatTimes(openTime, closedTime) : "";

            return Json(new
            {
                date = date,
                warehouseName = warehouse.Name,
                dayOfWeek = dayOfWeek,
                openTime = openTime,
                closedTime = closedTime,
                availableTimes = formattedTimes
                //gtin = gtin,
                //mpn = mpn,
                //sku = sku,
                //price = price,
                //stockAvailability = stockAvailability,
                //enabledattributemappingids = enabledAttributeMappingIds.ToArray(),
                //disabledattributemappingids = disabledAttributeMappingIds.ToArray(),
                //pictureFullSizeUrl = pictureFullSizeUrl,
                //pictureDefaultSizeUrl = pictureDefaultSizeUrl
            });
        }


        private string FormatTimes(string openTime, string closeTime)
        {
            // add a nullcheck on the string and have it function the same way start > end
            if (String.IsNullOrWhiteSpace(closeTime) || String.IsNullOrWhiteSpace(openTime))
            {
                return "";
            }

            var startTime = DateTime.Parse(openTime);
            startTime = RoundUp(startTime, TimeSpan.FromMinutes(15));

            var endTime = DateTime.Parse(closeTime);
            endTime = RoundUp(endTime, TimeSpan.FromMinutes(15));

            if (startTime > endTime)
            {
                return "";
            }

            double l = Math.Round(endTime.Subtract(startTime).TotalMinutes / 15.0);
            int maxHours = Convert.ToInt16(Math.Round(l, MidpointRounding.AwayFromZero));

            var clockQuery = from offset in Enumerable.Range(0, maxHours + 1) select TimeSpan.FromMinutes(15 * offset);

            var sb = new StringBuilder();
            foreach (var time in clockQuery)
            {
                if (sb.Length > 0)
                {
                    sb.Append(",");
                }

                sb.AppendFormat("{0}", (startTime + time).ToString("hh:mm tt"));
            }
            return sb.ToString();
        }

        DateTime RoundUp(DateTime dt, TimeSpan d)
        {
            return new DateTime(((dt.Ticks + d.Ticks - 1) / d.Ticks) * d.Ticks);
        }

        public ActionResult OnePageCheckout()
        {
            Session["TroveEnabledStore"] = null;
            //validation
            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();
            if (!cart.Any())
            {
                return RedirectToRoute("ShoppingCart");
            }

            if (!_orderSettings.OnePageCheckoutEnabled)
            {
                return RedirectToRoute("Checkout");
            }

            if (_workContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed)
            {
                return new HttpUnauthorizedResult();
            }

            string troveStoreIds = "";
            bool disablePaymentMethodStep = false;

            if (!String.IsNullOrEmpty(ConfigurationManager.AppSettings["TroveAPIEnabledStoreId"]))
            {
                troveStoreIds = ConfigurationManager.AppSettings["TroveAPIEnabledStoreId"];
            }

            if (troveStoreIds != "")
            {
                string[] splitStore = troveStoreIds.Split(',');
                foreach (var storeIdString in splitStore)
                {
                    if (storeIdString == _storeContext.CurrentStore.Id.ToString())
                    {
                        disablePaymentMethodStep = true;
                        Session["TroveEnabledStore"] = true;
                    }
                }
            }

            var model = new OnePageCheckoutModel
            {
                ShippingRequired = cart.RequiresShipping(),
                DisableBillingAddressCheckoutStep = _orderSettings.DisableBillingAddressCheckoutStep,
                DisablePaymentMethodStep = disablePaymentMethodStep,
            };
            return View(model);
        }

        [ChildActionOnly]
        public ActionResult OpcBillingForm()
        {
            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();


            var billingAddressModel = PrepareBillingAddressModel(cart, prePopulateNewAddressWithCustomerFields: true);
            return PartialView("OpcBillingAddress", billingAddressModel);
        }

        //---START: Codechages done by (na-sdxcorp\ADas)--------------
        private List<UPSAddress> GetAddressValidatedWithUPS(string city, string stateCode, string zipPostalCode, string countryCode, string addressLine1)
        {

            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;


            List<UPSAddress> allMatchingUPSAddress = new List<UPSAddress>();

            //string requestXML = "<?xml version=\"1.0\" ?> <AccessRequest xml:lang='en-US'> <AccessLicenseNumber>YOURACCESSLICENSENUMBER</AccessLicenseNumber> <UserId>YOURUSERID</UserId> <Password>YOURPASSWORD</Password> </AccessRequest> <?xml version=\"1.0\" ?> <AddressValidationRequest xml:lang='en-US'> <Request> <TransactionReference> <CustomerContext>Your Customer Context</CustomerContext> </TransactionReference> <RequestAction>AV</RequestAction> </Request> <Address> <City>YOURCITY</City> <StateProvinceCode>YOURSTATE</StateProvinceCode> <PostalCode>YOURPOSTALCODE</PostalCode> <CountryCode>YOURCOUNTRYCODE</CountryCode> </Address> </AddressValidationRequest>";
            string requestXML = "<?xml version=\"1.0\" ?> <AccessRequest xml:lang='en-US'> <AccessLicenseNumber>YOURACCESSLICENSENUMBER</AccessLicenseNumber> <UserId>YOURUSERID</UserId> <Password>YOURPASSWORD</Password> </AccessRequest> <?xml version=\"1.0\" ?> <AddressValidationRequest xml:lang='en-US'> <Request> <TransactionReference> <CustomerContext>Your Customer Context</CustomerContext> </TransactionReference> <RequestAction>XAV</RequestAction> <RequestOption>3</RequestOption> </Request> <AddressKeyFormat> <AddressLine>YOURADDRESSLINE</AddressLine> <PoliticalDivision2>YOURCITY</PoliticalDivision2> <PoliticalDivision1>YOURSTATE</PoliticalDivision1> <PostcodePrimaryLow>YOURPOSTALCODE</PostcodePrimaryLow> <CountryCode>YOURCOUNTRYCODE</CountryCode> </AddressKeyFormat> </AddressValidationRequest>";
            requestXML = requestXML.Replace("YOURACCESSLICENSENUMBER", ConfigurationManager.AppSettings["ups_AccessLicenseNumber"].ToString());
            requestXML = requestXML.Replace("YOURUSERID", ConfigurationManager.AppSettings["ups_UserId"].ToString());
            requestXML = requestXML.Replace("YOURPASSWORD", ConfigurationManager.AppSettings["ups_Password"].ToString());
            requestXML = requestXML.Replace("YOURCITY", city);
            requestXML = requestXML.Replace("YOURSTATE", stateCode);
            requestXML = requestXML.Replace("YOURCOUNTRYCODE", countryCode);
            requestXML = requestXML.Replace("YOURPOSTALCODE", zipPostalCode);
            requestXML = requestXML.Replace("YOURADDRESSLINE", addressLine1);

            try
            {
                var outputStr = string.Empty;
                byte[] requestBytes = null;
                Uri uri = new Uri(ConfigurationManager.AppSettings["Addressvalidate"]);
                WebRequest request = WebRequest.Create(uri);
                requestBytes = Encoding.GetEncoding("iso-8859-1").GetBytes(requestXML);

                request.ContentLength = requestBytes.Length;
                request.Method = "POST";
                request.ContentType = "multipart/form-data";

                using (Stream requestStream = request.GetRequestStream())
                {
                    requestStream.Write(requestBytes, 0, requestBytes.Length);
                    requestStream.Close();
                }
                WebResponse theResponse = request.GetResponse();
                using (StreamReader reader = new StreamReader(theResponse.GetResponseStream()))
                {
                    outputStr = reader.ReadToEnd();

                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(outputStr);

                    string statusCode = doc.SelectSingleNode("AddressValidationResponse/Response/ResponseStatusCode").InnerText;
                    XmlNode validAddressNode = doc.SelectSingleNode("AddressValidationResponse/ValidAddressIndicator");
                    XmlNode ambiguousAddressNode = doc.SelectSingleNode("AddressValidationResponse/AmbiguousAddressIndicator");
                    XmlNode noCandidatesAddressNode = doc.SelectSingleNode("AddressValidationResponse/NoCandidatesIndicator");
                    XmlNodeList idNodes = doc.SelectNodes("AddressValidationResponse/AddressKeyFormat");

                    if (statusCode == "0")
                    {
                        allMatchingUPSAddress = null; //null indicates wrong address and no match found.
                        Session["UPSFailure"] = true;
                    }
                    else if (statusCode == "1" && noCandidatesAddressNode != null)
                    {
                        //UPS could not find any match
                        allMatchingUPSAddress = null;
                        Session["UPSFailure"] = false;

                    }
                    else if (statusCode == "1" && (ambiguousAddressNode != null || validAddressNode != null)) //list of matches found.
                    {
                        allMatchingUPSAddress = new List<UPSAddress>();
                        foreach (XmlNode node in idNodes)
                        {
                            UPSAddress matchingUPSAddress = new UPSAddress();
                            foreach (XmlNode childNode in node.ChildNodes)
                            {
                                if (childNode.Name.Equals("AddressLine"))
                                {
                                    matchingUPSAddress.AddressLine = childNode.InnerText;
                                }
                                else if (childNode.Name.Equals("PoliticalDivision2"))
                                {
                                    matchingUPSAddress.City = childNode.InnerText;
                                }
                                else if (childNode.Name.Equals("PoliticalDivision1"))
                                {
                                    matchingUPSAddress.StateProvinceCode = childNode.InnerText;
                                }
                                else if (childNode.Name.Equals("PostcodePrimaryLow"))
                                {
                                    matchingUPSAddress.PostalCode = childNode.InnerText;
                                }
                            }

                            allMatchingUPSAddress.Add(matchingUPSAddress);
                        }

                        if (allMatchingUPSAddress.Count == 1 && validAddressNode != null)
                        {
                            if (city.ToUpper().Equals(allMatchingUPSAddress[0].City.ToUpper())
                                && stateCode.ToUpper().Equals(allMatchingUPSAddress[0].StateProvinceCode.ToUpper())
                                && zipPostalCode.Equals(allMatchingUPSAddress[0].PostalCode)
                                && addressLine1.ToUpper().Equals(allMatchingUPSAddress[0].AddressLine.ToUpper()))
                            {
                                allMatchingUPSAddress = null;
                                allMatchingUPSAddress = new List<UPSAddress>(); //empty list indicates valid address
                            }
                        }
                    }

                }
            }
            catch (Exception)
            {
                allMatchingUPSAddress = null;
                Session["UPSFailure"] = true;
            }

            return allMatchingUPSAddress;
        }



        //---END: Codechages done by (na-sdxcorp\ADas)--------------

        [ValidateInput(false)]
        public ActionResult OpcSaveBilling(FormCollection form)
        {
            try
            {
                bool isEnabled = false;
                bool isUPSException = false;
                var pluginDescriptor = _pluginFinder.GetPluginDescriptorBySystemName("Shipping.UPS");
                var pluginInstance = pluginDescriptor.Instance();
                if (pluginInstance is IShippingRateComputationMethod)
                {
                    //isEnabled = ((IShippingRateComputationMethod)pluginInstance).IsShippingRateComputationMethodActive(_shippingSettings);

                    //COMMENTED THIS SO THAT THE UPS VALIDATION IS NOT PASSING THROUGH THIS. IN ORDER TO ENABLE UPS VALIDATION UNCOMMENT THE CODE
                }
                //validation
                var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();
                if (!cart.Any())
                {
                    throw new Exception("Your cart is empty");
                }

                if (!_orderSettings.OnePageCheckoutEnabled)
                {
                    throw new Exception("One page checkout is disabled");
                }

                if (_workContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed)
                {
                    throw new Exception("Anonymous checkout is not allowed");
                }

                int billingAddressId;
                int.TryParse(form["billing_address_id"], out billingAddressId);

                if (billingAddressId > 0)
                {
                    if (!isEnabled)
                    {
                        //existing address
                        var address = _workContext.CurrentCustomer.Addresses.FirstOrDefault(a => a.Id == billingAddressId);
                        if (address == null)
                        {
                            throw new Exception("Address can't be loaded");
                        }

                        _workContext.CurrentCustomer.BillingAddress = address;
                        _customerService.UpdateCustomer(_workContext.CurrentCustomer);
                    }
                    else
                    {
                        var address = _workContext.CurrentCustomer.Addresses.FirstOrDefault(a => a.Id == billingAddressId);
                        if (address == null)
                        {
                            throw new Exception("Address can't be loaded");
                        }

                        List<UPSAddress> allMatchingUPSAddress = GetAddressValidatedWithUPS(address.City, address.StateProvince == null ? string.Empty : address.StateProvince.Abbreviation, address.ZipPostalCode, address.Country.TwoLetterIsoCode, address.Address1);
                        if (Session["UPSFailure"] != null)
                        {
                            isUPSException = (bool)Session["UPSFailure"];
                            Session.Remove("UPSFailure");
                        }

                        if (allMatchingUPSAddress == null && (isUPSException == false || isUPSException == true))
                        {
                            //UPS found no matching result.
                            var billingAddressModel = PrepareBillingAddressModel(cart,
                                        selectedCountryId: address.CountryId);

                            billingAddressModel.NewAddressPreselected = true;
                            ViewBag.NoMatchFoundForExisting = true;
                            return Json(new
                            {
                                update_section = new UpdateSectionJsonModel
                                {
                                    name = "billing",
                                    html = this.RenderPartialViewToString("OpcBillingAddress", billingAddressModel)
                                },
                                //wrong_billing_address = true,
                            });
                        }
                        else
                        {
                            _workContext.CurrentCustomer.BillingAddress = address;
                            _customerService.UpdateCustomer(_workContext.CurrentCustomer);
                        }
                    }
                }
                else
                {
                    //new address
                    var model = new CheckoutBillingAddressModel();
                    TryUpdateModel(model.NewAddress, "BillingNewAddress");

                    //custom address attributes
                    var customAttributes = form.ParseCustomAddressAttributes(_addressAttributeParser, _addressAttributeService);
                    var customAttributeWarnings = _addressAttributeParser.GetAttributeWarnings(customAttributes);
                    foreach (var error in customAttributeWarnings)
                    {
                        ModelState.AddModelError("", error);
                    }

                    //validate model
                    TryValidateModel(model.NewAddress);
                    if (!ModelState.IsValid)
                    {
                        //model is not valid. redisplay the form with errors
                        var billingAddressModel = PrepareBillingAddressModel(cart,
                            selectedCountryId: model.NewAddress.CountryId,
                            overrideAttributesXml: customAttributes);
                        billingAddressModel.NewAddressPreselected = true;
                        return Json(new
                        {
                            update_section = new UpdateSectionJsonModel
                            {
                                name = "billing",
                                html = this.RenderPartialViewToString("OpcBillingAddress", billingAddressModel)
                            },
                            wrong_billing_address = true,
                        });
                    }

                    #region Codechages done by (na-sdxcorp\ADas)

                    //---START: Codechages done by (na-sdxcorp\ADas)--------------
                    //---To do UPS API Validation here for USA-------------------------
                    if (model.NewAddress.CountryId != null && model.NewAddress.CountryId == 1)
                    {
                        if (isEnabled)
                        {
                            //Get countrycode
                            var countryCode = _countryService.GetCountryById((int)model.NewAddress.CountryId).TwoLetterIsoCode;
                            //Get statecode
                            var stateCode = _stateProvinceService.GetStateProvinceById((int)model.NewAddress.StateProvinceId).Abbreviation;

                            //Get Matching Address after UPS Validation
                            List<UPSAddress> allMatchingUPSAddress = GetAddressValidatedWithUPS(model.NewAddress.City, stateCode, model.NewAddress.ZipPostalCode, countryCode, model.NewAddress.Address1);

                            if (Session["UPSFailure"] != null)
                            {
                                isUPSException = (bool)Session["UPSFailure"];
                                Session.Remove("UPSFailure");
                            }


                            if (allMatchingUPSAddress == null && (isUPSException == false || isUPSException == true))
                            {
                                //UPS found no matching result.

                                var billingAddressModel = PrepareBillingAddressModel(cart,
                                            selectedCountryId: model.NewAddress.CountryId,
                                            overrideAttributesXml: customAttributes);

                                billingAddressModel.NewAddressPreselected = true;
                                ViewBag.NoMatchFound = true;
                                return Json(new
                                {
                                    update_section = new UpdateSectionJsonModel
                                    {
                                        name = "billing",
                                        html = this.RenderPartialViewToString("OpcBillingAddress", billingAddressModel)
                                    },
                                    //wrong_billing_address = true,
                                });

                            }
                            //If Matching Address Found
                            else if (allMatchingUPSAddress != null && allMatchingUPSAddress.Count > 0)
                            {
                                //prepare model with rest of items
                                //--ToDo:-------
                                var billingAddressModel = PrepareBillingAddressModel(cart,
                                           selectedCountryId: model.NewAddress.CountryId,
                                           overrideAttributesXml: customAttributes);

                                billingAddressModel.NewAddressPreselected = true;

                                if (allMatchingUPSAddress.Count == 1)
                                {
                                    ViewBag.SingleMatchFound = true;
                                    //Modified model with validated address
                                    ModelState.Remove("ShippingNewAddress.City");
                                    ModelState.Remove("ShippingNewAddress.ZipPostalCode");
                                    ModelState.Remove("ShippingNewAddress.StateProvinceId");
                                    ModelState.Remove("ShippingNewAddress.StateProvinceName");
                                    ModelState.Remove("ShippingNewAddress.Address1");
                                    model.NewAddress.City = allMatchingUPSAddress[0].City;
                                    model.NewAddress.ZipPostalCode = allMatchingUPSAddress[0].PostalCode;
                                    model.NewAddress.StateProvinceId = _stateProvinceService.GetStateProvinceByAbbreviation(allMatchingUPSAddress[0].StateProvinceCode).Id;
                                    model.NewAddress.StateProvinceName = _stateProvinceService.GetStateProvinceByAbbreviation(allMatchingUPSAddress[0].StateProvinceCode).Name;
                                    model.NewAddress.Address1 = allMatchingUPSAddress[0].AddressLine;

                                    //----Modify model with valid address                                
                                    billingAddressModel.NewAddress.City = model.NewAddress.City;
                                    billingAddressModel.NewAddress.ZipPostalCode = model.NewAddress.ZipPostalCode;
                                    billingAddressModel.NewAddress.StateProvinceId = model.NewAddress.StateProvinceId;
                                    billingAddressModel.NewAddress.StateProvinceName = model.NewAddress.StateProvinceName;
                                    billingAddressModel.NewAddress.Address1 = model.NewAddress.Address1;
                                    //----Return to view  
                                    //return Json(new
                                    //{
                                    //    update_section = new UpdateSectionJsonModel
                                    //    {
                                    //        name = "billing",
                                    //        html = this.RenderPartialViewToString("OpcBillingAddress", billingAddressModel)
                                    //    }
                                    //    //wrong_billing_address = true,
                                    //});
                                }
                                else if (allMatchingUPSAddress.Count > 1)
                                {
                                    //If multiple matches found

                                    List<UPSAddress> allMatchingUPSAddressNew = new List<UPSAddress>();
                                    foreach (UPSAddress ups in allMatchingUPSAddress)
                                    {
                                        ups.StateID = _stateProvinceService.GetStateProvinceByAbbreviation(ups.StateProvinceCode).Id.ToString();
                                        allMatchingUPSAddressNew.Add(ups);
                                    }
                                    ViewBag.MultipleMatchFound = true;
                                    ViewBag.AllMatchingUPSAddress = allMatchingUPSAddressNew;

                                    //----Return to view  
                                    return Json(new
                                    {
                                        update_section = new UpdateSectionJsonModel
                                        {
                                            name = "billing",
                                            html = this.RenderPartialViewToString("OpcBillingAddress", billingAddressModel)
                                        }
                                        //wrong_billing_address = true,
                                    });

                                }

                            }
                        }
                    }
                    //---END: Codechages done by (na-sdxcorp\ADas)--------------
                    #endregion


                    //try to find an address with the same values (don't duplicate records)
                    var address = _workContext.CurrentCustomer.Addresses.ToList().FindAddress(
                        model.NewAddress.FirstName, model.NewAddress.LastName, model.NewAddress.PhoneNumber,
                        model.NewAddress.Email, model.NewAddress.FaxNumber, model.NewAddress.Company,
                        model.NewAddress.Address1, model.NewAddress.Address2, model.NewAddress.City,
                        model.NewAddress.StateProvinceId, model.NewAddress.ZipPostalCode,
                        model.NewAddress.CountryId, customAttributes);
                    if (address == null)
                    {
                        //address is not found. let's create a new one
                        address = model.NewAddress.ToEntity();
                        address.CustomAttributes = customAttributes;
                        address.CreatedOnUtc = DateTime.UtcNow;
                        //some validation
                        if (address.CountryId == 0)
                        {
                            address.CountryId = null;
                        }

                        if (address.StateProvinceId == 0)
                        {
                            address.StateProvinceId = null;
                        }

                        if (address.CountryId.HasValue && address.CountryId.Value > 0)
                        {
                            address.Country = _countryService.GetCountryById(address.CountryId.Value);
                        }
                        _workContext.CurrentCustomer.Addresses.Add(address);
                    }
                    _workContext.CurrentCustomer.BillingAddress = address;
                    _customerService.UpdateCustomer(_workContext.CurrentCustomer);
                }

                if (cart.RequiresShipping())
                {
                    //shipping is required

                    var model = new CheckoutBillingAddressModel();
                    TryUpdateModel(model);
                    if (_shippingSettings.ShipToSameAddress && model.ShipToSameAddress)
                    {
                        //ship to the same address
                        _workContext.CurrentCustomer.ShippingAddress = _workContext.CurrentCustomer.BillingAddress;
                        _customerService.UpdateCustomer(_workContext.CurrentCustomer);
                        //reset selected shipping method (in case if "pick up in store" was selected)
                        _genericAttributeService.SaveAttribute<ShippingOption>(_workContext.CurrentCustomer, SystemCustomerAttributeNames.SelectedShippingOption, null, _storeContext.CurrentStore.Id);
                        _genericAttributeService.SaveAttribute<PickupPoint>(_workContext.CurrentCustomer, SystemCustomerAttributeNames.SelectedPickupPoint, null, _storeContext.CurrentStore.Id);
                        //limitation - "Ship to the same address" doesn't properly work in "pick up in store only" case (when no shipping plugins are available) 
                        return OpcLoadStepAfterShippingAddress(cart);
                    }
                    else
                    {
                        //do not ship to the same address
                        var shippingAddressModel = PrepareShippingAddressModel(prePopulateNewAddressWithCustomerFields: true);
                        return Json(new
                        {
                            update_section = new UpdateSectionJsonModel
                            {
                                name = "shipping",
                                html = this.RenderPartialViewToString("OpcShippingAddress", shippingAddressModel)
                            },
                            goto_section = "shipping"
                        });
                    }
                }

                //shipping is not required
                _workContext.CurrentCustomer.ShippingAddress = null;
                _customerService.UpdateCustomer(_workContext.CurrentCustomer);

                _genericAttributeService.SaveAttribute<ShippingOption>(_workContext.CurrentCustomer, SystemCustomerAttributeNames.SelectedShippingOption, null, _storeContext.CurrentStore.Id);

                //load next step
                return OpcLoadStepAfterShippingMethod(cart);
            }
            catch (Exception exc)
            {
                _logger.Warning(exc.Message, exc, _workContext.CurrentCustomer);
                return Json(new { error = 1, message = exc.Message });
            }
        }

        [ValidateInput(false)]
        public ActionResult OpcSaveShipping(FormCollection form)
        {
            try
            {
                bool isEnabled = false;
                bool isUPSException = false;
                var pluginDescriptor = _pluginFinder.GetPluginDescriptorBySystemName("Shipping.UPS");
                var pluginInstance = pluginDescriptor.Instance();
                if (pluginInstance is IShippingRateComputationMethod)
                {
                    //isEnabled = ((IShippingRateComputationMethod)pluginInstance).IsShippingRateComputationMethodActive(_shippingSettings);
                }
                //validation
                var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();
                if (!cart.Any())
                {
                    throw new Exception("Your cart is empty");
                }

                if (!_orderSettings.OnePageCheckoutEnabled)
                {
                    throw new Exception("One page checkout is disabled");
                }

                if (_workContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed)
                {
                    throw new Exception("Anonymous checkout is not allowed");
                }

                if (!cart.RequiresShipping())
                {
                    throw new Exception("Shipping is not required");
                }

                //pickup point
                /*
                if (_shippingSettings.AllowPickUpInStore)
                {
                    var model = new CheckoutShippingAddressModel();
                    TryUpdateModel(model);

                    if (model.PickUpInStore)
                    {
                        //no shipping address selected
                        _workContext.CurrentCustomer.ShippingAddress = null;
                        _customerService.UpdateCustomer(_workContext.CurrentCustomer);

                        var pickupPoint = form["pickup-points-id"].Split(new[] { "___" }, StringSplitOptions.None);
                        var pickupPoints = _shippingService.GetPickupPoints(_workContext.CurrentCustomer.BillingAddress,
                            _workContext.CurrentCustomer, pickupPoint[1], _storeContext.CurrentStore.Id).PickupPoints.ToList();
                        var selectedPoint = pickupPoints.FirstOrDefault(x => x.Id.Equals(pickupPoint[0]));
                        if (selectedPoint == null)
                            throw new Exception("Pickup point is not allowed");

                        var pickUpInStoreShippingOption = new ShippingOption
                        {
                            Name = string.Format(_localizationService.GetResource("Checkout.PickupPoints.Name"), selectedPoint.Name),
                            Rate = selectedPoint.PickupFee,
                            Description = selectedPoint.Description,
                            ShippingRateComputationMethodSystemName = selectedPoint.ProviderSystemName
                        };
                        _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer, SystemCustomerAttributeNames.SelectedShippingOption, pickUpInStoreShippingOption, _storeContext.CurrentStore.Id);
                        _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer, SystemCustomerAttributeNames.SelectedPickupPoint, selectedPoint, _storeContext.CurrentStore.Id);

                        //load next step
                        return OpcLoadStepAfterShippingMethod(cart);
                    }

                    //set value indicating that "pick up in store" option has not been chosen
                    _genericAttributeService.SaveAttribute<PickupPoint>(_workContext.CurrentCustomer, SystemCustomerAttributeNames.SelectedPickupPoint, null, _storeContext.CurrentStore.Id);
                }
                */
                int shippingAddressId;
                int.TryParse(form["shipping_address_id"], out shippingAddressId);

                #region SODMYWAY-
                _genericAttributeService.SaveAttribute<bool>(_workContext.CurrentCustomer, "SendGiftNotification", form["SendGiftNotification"] == "false" ? false : true, _storeContext.CurrentStore.Id);
                #endregion


                if (shippingAddressId > 0)
                {
                    if (!isEnabled)
                    {
                        //existing address
                        var address = _workContext.CurrentCustomer.Addresses.FirstOrDefault(a => a.Id == shippingAddressId);
                        if (address == null)
                        {
                            throw new Exception("Address can't be loaded");
                        }

                        _workContext.CurrentCustomer.ShippingAddress = address;
                        _customerService.UpdateCustomer(_workContext.CurrentCustomer);
                    }
                    else
                    {
                        var address = _workContext.CurrentCustomer.Addresses.FirstOrDefault(a => a.Id == shippingAddressId);
                        if (address == null)
                        {
                            throw new Exception("Address can't be loaded");
                        }

                        List<UPSAddress> allMatchingUPSAddress = GetAddressValidatedWithUPS(address.City, address.StateProvince == null ? string.Empty : address.StateProvince.Abbreviation, address.ZipPostalCode, address.Country.TwoLetterIsoCode, address.Address1);
                        if (Session["UPSFailure"] != null)
                        {
                            isUPSException = (bool)Session["UPSFailure"];
                            Session.Remove("UPSFailure");
                        }

                        if (allMatchingUPSAddress == null && (isUPSException == false || isUPSException == true))
                        {
                            //UPS found no matching result.
                            var shippingAddressModel = PrepareShippingAddressModel(
                                    selectedCountryId: address.CountryId);

                            shippingAddressModel.NewAddressPreselected = true;
                            ViewBag.NoMatchFoundForExisting = true;
                            return Json(new
                            {
                                update_section = new UpdateSectionJsonModel
                                {
                                    name = "shipping",
                                    html = this.RenderPartialViewToString("OpcShippingAddress", shippingAddressModel)
                                }
                            });
                        }
                        else
                        {
                            _workContext.CurrentCustomer.ShippingAddress = address;
                            _customerService.UpdateCustomer(_workContext.CurrentCustomer);
                        }
                    }

                }
                else
                {
                    //new address
                    var model = new CheckoutShippingAddressModel();
                    TryUpdateModel(model.NewAddress, "ShippingNewAddress");

                    //custom address attributes
                    var customAttributes = form.ParseCustomAddressAttributes(_addressAttributeParser, _addressAttributeService);
                    var customAttributeWarnings = _addressAttributeParser.GetAttributeWarnings(customAttributes);
                    foreach (var error in customAttributeWarnings)
                    {
                        ModelState.AddModelError("", error);
                    }

                    //validate model
                    TryValidateModel(model.NewAddress);
                    if (!ModelState.IsValid)
                    {
                        //model is not valid. redisplay the form with errors
                        var shippingAddressModel = PrepareShippingAddressModel(
                            selectedCountryId: model.NewAddress.CountryId,
                            overrideAttributesXml: customAttributes);
                        shippingAddressModel.NewAddressPreselected = true;
                        return Json(new
                        {
                            update_section = new UpdateSectionJsonModel
                            {
                                name = "shipping",
                                html = this.RenderPartialViewToString("OpcShippingAddress", shippingAddressModel)
                            }
                        });
                    }

                    #region Codechages done by (na-sdxcorp\ADas)

                    //---START: Codechages done by (na-sdxcorp\ADas)--------------
                    //---To do UPS API Validation here for USA-------------------------
                    if (model.NewAddress.CountryId != null && model.NewAddress.CountryId == 1)
                    {
                        if (isEnabled)
                        {
                            //Get countrycode
                            var countryCode = _countryService.GetCountryById((int)model.NewAddress.CountryId).TwoLetterIsoCode;
                            //Get statecode
                            var stateCode = _stateProvinceService.GetStateProvinceById((int)model.NewAddress.StateProvinceId).Abbreviation;

                            //Get Matching Address after UPS Validation
                            List<UPSAddress> allMatchingUPSAddress = GetAddressValidatedWithUPS(model.NewAddress.City, stateCode, model.NewAddress.ZipPostalCode, countryCode, model.NewAddress.Address1);



                            if (Session["UPSFailure"] != null)
                            {
                                isUPSException = (bool)Session["UPSFailure"];
                                Session.Remove("UPSFailure");
                            }

                            //if (allMatchingUPSAddress == null && isUPSException == true)
                            //{
                            //    // throw new Exception("No matching address found as per UPS Logistics");
                            //    //UPS having any technical fault.

                            //}
                            if (allMatchingUPSAddress == null && (isUPSException == false || isUPSException == true))
                            {
                                //UPS found no matching result.
                                var shippingAddressModel = PrepareShippingAddressModel(
                                    selectedCountryId: model.NewAddress.CountryId,
                                    overrideAttributesXml: customAttributes);

                                shippingAddressModel.NewAddressPreselected = true;

                                ViewBag.NoMatchFound = true;

                                return Json(new
                                {
                                    update_section = new UpdateSectionJsonModel
                                    {
                                        name = "shipping",
                                        html = this.RenderPartialViewToString("OpcShippingAddress", shippingAddressModel)
                                    },
                                    //wrong_billing_address = true,
                                });

                            }
                            //If Matching Address Found
                            else if (allMatchingUPSAddress != null && allMatchingUPSAddress.Count > 0)
                            {
                                //prepare model with rest of items
                                //--ToDo:-------
                                var shippingAddressModel = PrepareShippingAddressModel(
                                    selectedCountryId: model.NewAddress.CountryId,
                                    overrideAttributesXml: customAttributes);

                                shippingAddressModel.NewAddressPreselected = true;

                                if (allMatchingUPSAddress.Count == 1)
                                {
                                    ViewBag.SingleMatchFound = true;
                                    //Modified model with validated address
                                    ModelState.Remove("ShippingNewAddress.City");
                                    ModelState.Remove("ShippingNewAddress.ZipPostalCode");
                                    ModelState.Remove("ShippingNewAddress.StateProvinceId");
                                    ModelState.Remove("ShippingNewAddress.StateProvinceName");
                                    ModelState.Remove("ShippingNewAddress.Address1");
                                    model.NewAddress.City = allMatchingUPSAddress[0].City;
                                    model.NewAddress.ZipPostalCode = allMatchingUPSAddress[0].PostalCode;
                                    model.NewAddress.StateProvinceId = _stateProvinceService.GetStateProvinceByAbbreviation(allMatchingUPSAddress[0].StateProvinceCode).Id;
                                    model.NewAddress.StateProvinceName = _stateProvinceService.GetStateProvinceByAbbreviation(allMatchingUPSAddress[0].StateProvinceCode).Name;
                                    model.NewAddress.Address1 = allMatchingUPSAddress[0].AddressLine;

                                    //----Modify model with valid address                                
                                    shippingAddressModel.NewAddress.City = model.NewAddress.City;
                                    shippingAddressModel.NewAddress.ZipPostalCode = model.NewAddress.ZipPostalCode;
                                    shippingAddressModel.NewAddress.StateProvinceId = model.NewAddress.StateProvinceId;
                                    shippingAddressModel.NewAddress.StateProvinceName = model.NewAddress.StateProvinceName;
                                    shippingAddressModel.NewAddress.Address1 = model.NewAddress.Address1;
                                    //----Return to view  
                                    return Json(new
                                    {
                                        update_section = new UpdateSectionJsonModel
                                        {
                                            name = "shipping",
                                            html = this.RenderPartialViewToString("OpcShippingAddress", shippingAddressModel)
                                        }
                                        //wrong_billing_address = true,
                                    });
                                }
                                else if (allMatchingUPSAddress.Count > 1)
                                {
                                    //If multiple matches found

                                    List<UPSAddress> allMatchingUPSAddressNew = new List<UPSAddress>();
                                    foreach (UPSAddress ups in allMatchingUPSAddress)
                                    {
                                        ups.StateID = _stateProvinceService.GetStateProvinceByAbbreviation(ups.StateProvinceCode).Id.ToString();
                                        allMatchingUPSAddressNew.Add(ups);
                                    }
                                    ViewBag.MultipleMatchFound = true;
                                    ViewBag.AllMatchingUPSAddress = allMatchingUPSAddressNew;

                                    //----Return to view  
                                    return Json(new
                                    {
                                        update_section = new UpdateSectionJsonModel
                                        {
                                            name = "shipping",
                                            html = this.RenderPartialViewToString("OpcShippingAddress", shippingAddressModel)
                                        }
                                        //wrong_billing_address = true,
                                    });

                                }

                            }
                        }
                    }
                    //---END: Codechages done by (na-sdxcorp\ADas)--------------
                    #endregion

                    //try to find an address with the same values (don't duplicate records)
                    var address = _workContext.CurrentCustomer.Addresses.ToList().FindAddress(
                        model.NewAddress.FirstName, model.NewAddress.LastName, model.NewAddress.PhoneNumber,
                        model.NewAddress.Email, model.NewAddress.FaxNumber, model.NewAddress.Company,
                        model.NewAddress.Address1, model.NewAddress.Address2, model.NewAddress.City,
                        model.NewAddress.StateProvinceId, model.NewAddress.ZipPostalCode,
                        model.NewAddress.CountryId, customAttributes);
                    if (address == null)
                    {
                        address = model.NewAddress.ToEntity();
                        address.CustomAttributes = customAttributes;
                        address.CreatedOnUtc = DateTime.UtcNow;
                        //little hack here (TODO: find a better solution)
                        //EF does not load navigation properties for newly created entities (such as this "Address").
                        //we have to load them manually 
                        //otherwise, "Country" property of "Address" entity will be null in shipping rate computation methods
                        if (address.CountryId.HasValue)
                        {
                            address.Country = _countryService.GetCountryById(address.CountryId.Value);
                        }

                        if (address.StateProvinceId.HasValue)
                        {
                            address.StateProvince = _stateProvinceService.GetStateProvinceById(address.StateProvinceId.Value);
                        }

                        //other null validations
                        if (address.CountryId == 0)
                        {
                            address.CountryId = null;
                        }

                        if (address.StateProvinceId == 0)
                        {
                            address.StateProvinceId = null;
                        }

                        _workContext.CurrentCustomer.Addresses.Add(address);
                    }
                    _workContext.CurrentCustomer.ShippingAddress = address;
                    _customerService.UpdateCustomer(_workContext.CurrentCustomer);
                }

                return OpcLoadStepAfterShippingAddress(cart);
            }
            catch (Exception exc)
            {
                _logger.Warning(exc.Message, exc, _workContext.CurrentCustomer);
                return Json(new { error = 1, message = exc.Message });
            }
        }


        [ValidateInput(false)]
        public ActionResult OpcSaveShippingMethod(FormCollection form)
        {
            try
            {
                //validation
                var cart = _workContext.CurrentCustomer.ShoppingCartItems
                    .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                    .LimitPerStore(_storeContext.CurrentStore.Id)
                    .ToList();
                if (!cart.Any())
                {
                    throw new Exception("Your cart is empty");
                }

                if (!_orderSettings.OnePageCheckoutEnabled)
                {
                    throw new Exception("One page checkout is disabled");
                }

                if (_workContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed)
                {
                    throw new Exception("Anonymous checkout is not allowed");
                }

                if (!cart.RequiresShipping())
                {
                    throw new Exception("Shipping is not required");
                }

                int i = 0;
                foreach (ShoppingCartItem sci in cart)	/// SODMYWAY-
                {
                    if (findKey("shippingOption", form, sci.Id) != null)	/// SODMYWAY-
                    {
                        string key = findKey("shippingOption", form, sci.Id);	/// SODMYWAY-
                        //parse selected method 
                        string shippingoption = form[key];	/// SODMYWAY-
                        if (String.IsNullOrEmpty(shippingoption))
                        {
                            throw new Exception("Selected shipping method can't be parsed");
                        }

                        var splittedOption = shippingoption.Split(new[] { "___" }, StringSplitOptions.RemoveEmptyEntries);

                        if (splittedOption.Length == 1)
                        {
                            if (shippingoption == "FedexShipping" || shippingoption == "MailStopShipping")
                            {
                                #region for tiered shipping 
                                bool isMultipleShippingFeatureEnabled = (_storeContext.CurrentStore.IsTieredShippingEnabled == true ? true : false ||
                                                               _storeContext.CurrentStore.IsContractShippingEnabled == true ? true : false ||
                                                               _storeContext.CurrentStore.IsInterOfficeDeliveryEnabled == true ? true : false) ? true : false;

                                if (isMultipleShippingFeatureEnabled)
                                {
                                    string rcNo = Request.Form["ShoppingCartItemModels[" + i + "].RCNumber"];
                                    string mailStopAddress = Request.Form["ShoppingCartItemModels[" + i + "].MailStopAddress"];
                                    if (shippingoption == "FedexShipping")
                                    {

                                        if (String.IsNullOrEmpty(rcNo))
                                        {
                                            sci.IsFirstCartItem = false;
                                            sci.FlatShipping = 0;
                                            sci.IsTieredShippingEnabled = true;
                                            sci.IsContractShippingEnabled = false;
                                            sci.RCNumber = "";
                                        }
                                        else
                                        {
                                            sci.IsFirstCartItem = false;
                                            sci.IsContractShippingEnabled = true;
                                            sci.RCNumber = rcNo;
                                            sci.IsTieredShippingEnabled = false;
                                            sci.FlatShipping = 0;
                                        }

                                        sci.IsInterOfficeDeliveryEnabled = false;
                                        sci.MailStopAddress = "";
                                    }
                                    if (shippingoption == "MailStopShipping")
                                    {
                                        sci.IsTieredShippingEnabled = false;
                                        sci.FlatShipping = 0;
                                        sci.IsContractShippingEnabled = false;
                                        sci.RCNumber = "";
                                        sci.IsInterOfficeDeliveryEnabled = true;
                                        sci.MailStopAddress = mailStopAddress;
                                        sci.IsFirstCartItem = false;
                                    }
                                    _shoppingCartService.UpdateShoppingCartItemForTieredShipping(sci);
                                }

                                #endregion
                            }
                        }

                        else
                        {
                            if (splittedOption.Length != 2)
                            {
                                throw new Exception("Selected shipping method can't be parsed");
                            }

                            #region plugins shipping method
                            string selectedName = splittedOption[0];

                            var cartItemId = Int32.Parse(key.Substring(key.LastIndexOf("__") + 2)); /// SODMYWAY-
                            string shippingRateComputationMethodSystemName = splittedOption[1].Split(new string[] { "__" }, StringSplitOptions.RemoveEmptyEntries)[0];  /// SODMYWAY-
                            var shippingOptions = _workContext.CurrentCustomer.GetAttribute<List<ShippingOption>>(SystemCustomerAttributeNames.OfferedShippingOptions, _storeContext.CurrentStore.Id);
                            shippingOptions = null;
                            if (shippingOptions == null || !shippingOptions.Any())
                            {
                                //not found? let's load them using shipping service
                                shippingOptions = _shippingService.GetShippingOptions(cart, _workContext.CurrentCustomer.ShippingAddress, shippingRateComputationMethodSystemName, _storeContext.CurrentStore.Id)
                                        .ShippingOptions
                                        .ToList();
                            }
                            else
                            {
                                //loaded cached results. let's filter result by a chosen shipping rate computation method
                                shippingOptions = shippingOptions.Where(so => so.ShippingRateComputationMethodSystemName.Equals(shippingRateComputationMethodSystemName, StringComparison.InvariantCultureIgnoreCase))
                                    .ToList();
                            }

                            if (!selectedName.Contains("UPS"))
                            {
                                var shippingOption = shippingOptions
                                    .Find(so => !String.IsNullOrEmpty(so.Name) && so.Name.Equals(selectedName, StringComparison.InvariantCultureIgnoreCase));
                                //shippingOption = shippingOptions.Where(x => x.ShoppingCartProductId == sci.ProductId).FirstOrDefault();
                                if (shippingOption == null)
                                {
                                    throw new Exception("Selected shipping method can't be loaded");
                                }

                                #region SODMYWAY-
                                sci.SelectedShippingMethodName = shippingOption.Name;
                                sci.SelectedShippingRateComputationMethodSystemName = shippingOption.ShippingRateComputationMethodSystemName;//SODMYWAY-3467

                                _shoppingCartService.UpdateShoppingCartItem(_workContext.CurrentCustomer,
                                    sci.Id, sci.AttributesXml,
                                    sci.CustomerEnteredPrice,
                                    null, null,
                                    sci.Quantity, false,
                                    null, null, shippingOption.Name);
                                string lastItemShippingOption = SystemCustomerAttributeNames.SelectedShippingOption + "" + cartItemId;

                                _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer, lastItemShippingOption, shippingOption, _storeContext.CurrentStore.Id);
                            }
                            #endregion
                            //save
                            else
                            {

                                var upsDropDownValue = form["UpsOptionSelect__" + sci.Id];
                                var upsDropDownSelectedShippingMethod = upsDropDownValue.Split(new[] { "___" }, StringSplitOptions.RemoveEmptyEntries);
                                string upsDropDownSelectedShippingMethodName = upsDropDownSelectedShippingMethod[0];



                                var shippingOption = shippingOptions
                                    .Find(so => !String.IsNullOrEmpty(so.Name) && so.Name.Equals(upsDropDownSelectedShippingMethodName, StringComparison.InvariantCultureIgnoreCase));

                                shippingOption = shippingOptions.Where(x => x.ShoppingCartProductId == sci.ProductId).FirstOrDefault();

                                _shoppingCartService.UpdateShoppingCartItem(_workContext.CurrentCustomer,
                                  sci.Id, sci.AttributesXml,
                                  sci.CustomerEnteredPrice,
                                  null, null,
                                  sci.Quantity, false,
                                  null, null, upsDropDownSelectedShippingMethodName);
                                string lastItemShippingOption = SystemCustomerAttributeNames.SelectedShippingOption + "" + cartItemId;
                                _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer, lastItemShippingOption, shippingOption, _storeContext.CurrentStore.Id);
                            }

                            #region SODMYWAY-2957
                            if (findKey("requestedFulfillmentDate", form, sci.Id) != null)
                            {
                                key = findKey("requestedFulfillmentDate", form, sci.Id);
                                string timeKey = findKey("timePicker", form, sci.Id);
                                string timeValue = form[timeKey];
                                string requestedFulfillmentDate = form[key] + ' ' + timeValue;
                                if (requestedFulfillmentDate != "")
                                {
                                    DateTime reqFulfillmentDate;
                                    var formatProvider = new DateTimeFormatInfo { ShortDatePattern = "yyyy-M-d" };

                                    if (!DateTime.TryParse(requestedFulfillmentDate, formatProvider, DateTimeStyles.None, out reqFulfillmentDate))
                                    {
                                        return ShippingMethod();
                                    }

                                    if (reqFulfillmentDate < DateTime.Now.Date)
                                    {
                                        return ShippingMethod();
                                    }

                                    cartItemId = Int32.Parse(key.Substring(key.IndexOf("__") + 2));

                                    sci.RequestedFullfilmentDateTime = reqFulfillmentDate;
                                    //update the requested fulfillment date.
                                    _shoppingCartService.UpdateShoppingCartItem(_workContext.CurrentCustomer,
                                        sci.Id, sci.AttributesXml,
                                        sci.CustomerEnteredPrice,
                                        null, null,
                                        sci.Quantity, false,
                                        reqFulfillmentDate, null, null);    /// SODMYWAY-

                                }
                            }

                            if (findKey("UpsOptionSelect", form, sci.Id) != null)
                            {

                                string key1 = findKey("UpsOptionSelect", form, sci.Id);


                                string selectedStoreIdups = form[key1];
                                if (String.IsNullOrEmpty(selectedStoreIdups))
                                {
                                    throw new Exception("Selected shipping method can't be parsed");
                                }

                                splittedOption = selectedStoreIdups.Split(new[] { "___" }, StringSplitOptions.RemoveEmptyEntries);
                                if (splittedOption.Length != 2)
                                {
                                    throw new Exception("Selected shipping method can't be parsed");
                                }

                                selectedName = splittedOption[0];
                                var cartItemupsId = Int32.Parse(key1.Substring(key1.IndexOf("__") + 2));
                                //update the requested fulfillment date.
                                if (sci.SelectedShippingMethodName.Contains("UPS"))
                                {
                                    _shoppingCartService.UpdateShoppingCartItem(_workContext.CurrentCustomer,
                                         sci.Id, sci.AttributesXml,
                                         sci.CustomerEnteredPrice,
                                         null, null,
                                         sci.Quantity, false,
                                         null, null, selectedName);
                                }
                            }


                            if (findKey("StoreSelect", form, sci.Id) != null)
                            {

                                key = findKey("StoreSelect", form, sci.Id);
                                string selectedStoreId = form[key];
                                cartItemId = Int32.Parse(key.Substring(key.IndexOf("__") + 2));
                                if (sci.SelectedShippingMethodName == "In-Store Pickup")
                                {
                                    int storeId = Int32.Parse(selectedStoreId);
                                    sci.SelectedWarehouseId = storeId;
                                    sci.SelectedWarehouse = _shippingService.GetWarehouseById(storeId);

                                    //item.PreferredSourceSiteStoreLocationId
                                    ////update the requested fulfillment date.
                                    _shoppingCartService.UpdateShoppingCartItem(_workContext.CurrentCustomer,
                                        sci.Id, sci.AttributesXml,
                                        sci.CustomerEnteredPrice,
                                        null, null,
                                        sci.Quantity, false,
                                        null, storeId, null);   /// SODMYWAY- 

                                }
                                else //delivery
                                {


                                    IList<Warehouse> warehouses = _shippingService.GetAllWarehouses(_storeContext.CurrentStore.Id);

                                    var defaultStore = warehouses[0];

                                    foreach (Warehouse location in warehouses)
                                    {
                                        if (location.IsDelivery)
                                        {
                                            defaultStore = location;
                                            break;
                                        }
                                    }

                                    sci.SelectedWarehouseId = defaultStore.Id;
                                    sci.SelectedWarehouse = defaultStore;
                                    _shoppingCartService.UpdateShoppingCartItem(_workContext.CurrentCustomer,
                                        sci.Id, sci.AttributesXml,
                                        sci.CustomerEnteredPrice,
                                        null, null,
                                        sci.Quantity, false,
                                        null, defaultStore.Id, null);   /// SODMYWAY-
                                }
                            }
                            else //must be a delivery store.
                            {
                                if (sci.SelectedWarehouseId == null)
                                {
                                    IList<Warehouse> warehouses = _shippingService.GetAllWarehouses(_storeContext.CurrentStore.Id);


                                    var defaultStore = warehouses[0];

                                    foreach (Warehouse location in warehouses)
                                    {
                                        if (location.IsDelivery)
                                        {
                                            defaultStore = location;
                                            break;
                                        }
                                    }


                                    sci.SelectedWarehouseId = defaultStore.Id;
                                    sci.SelectedWarehouse = defaultStore;

                                    _shoppingCartService.UpdateShoppingCartItem(_workContext.CurrentCustomer,
                                        sci.Id, sci.AttributesXml,
                                        sci.CustomerEnteredPrice,
                                        null, null,
                                        sci.Quantity, false,
                                        null, defaultStore.Id, null);   /// SODMYWAY-
                                }
                            }
                            #endregion
                            #endregion
                        }
                        i++;
                    }
                }
                UpdateShippingFeeAndFirstCartItem();
                //load next step
                return OpcLoadStepAfterShippingMethod(cart);
            }
            catch (Exception exc)
            {
                _logger.Warning(exc.Message, exc, _workContext.CurrentCustomer);
                return Json(new { error = 1, message = exc.Message });
            }
        }

        [ValidateInput(false)]
        public ActionResult OpcSavePaymentMethod(FormCollection form)
        {
            try
            {
                bool isTroveEnabledStore = Convert.ToBoolean(Session["TroveEnabledStore"]);
                //validation
                var cart = _workContext.CurrentCustomer.ShoppingCartItems
                    .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                    .LimitPerStore(_storeContext.CurrentStore.Id)
                    .ToList();
                if (!cart.Any())
                {
                    throw new Exception("Your cart is empty");
                }

                if (!_orderSettings.OnePageCheckoutEnabled)
                {
                    throw new Exception("One page checkout is disabled");
                }

                if (_workContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed)
                {
                    throw new Exception("Anonymous checkout is not allowed");
                }

                string paymentmethod = form["paymentmethod"];
                string hdndata = form["hdnSelectedPaymentType"];
                string hdnPaymentOption = form["hdnSelectedPaymentOption"];
                if (hdndata== "FirstDataPayment")
                {
                    paymentmethod = "BitShift.Payments.FirstData";
                    _httpContext.Session["SelectedPaymentType"] = "FirstDataPayment";
                }
                else
                    _httpContext.Session["SelectedPaymentType"] = "TrovePayment";

                //string selectedPaymentOption = form["paymentmethod"];
               
                if (!String.IsNullOrEmpty(hdnPaymentOption))
                {
                    string hdnCardData = form["hdnSelectedCardName"];
                    string hdnAccountdata = form["hdnSelectedAccountName"];
                    if (hdnPaymentOption == "rdAvlblCards")
                    {
                        if (!String.IsNullOrEmpty(hdnCardData))
                            _httpContext.Session["CardDetails"] = hdnCardData;
                        _httpContext.Session["AccountDetails"] = null;

                    }
                    else if (hdnPaymentOption == "rdAvlblAccounts")
                    {
                        if (!String.IsNullOrEmpty(hdnAccountdata))
                            _httpContext.Session["AccountDetails"] = hdnAccountdata;
                        _httpContext.Session["CardDetails"] = null;
                    }
                }


                //payment method 
                if (String.IsNullOrEmpty(paymentmethod))
                {
                    if (!isTroveEnabledStore)
                    {
                        throw new Exception("Selected payment method can't be parsed");
                    }
                }

                var model = new CheckoutPaymentMethodModel();
                TryUpdateModel(model);

                //reward points
                if (_rewardPointsSettings.Enabled)
                {
                    _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer,
                        SystemCustomerAttributeNames.UseRewardPointsDuringCheckout, model.UseRewardPoints,
                        _storeContext.CurrentStore.Id);
                }

                //Check whether payment workflow is required
                bool isPaymentWorkflowRequired = IsPaymentWorkflowRequired(cart);

                //Disable Payment method step for Trove payments.

                if (isTroveEnabledStore && String.IsNullOrEmpty(paymentmethod))
                {
                    isPaymentWorkflowRequired = false;
                }

                if (!isPaymentWorkflowRequired)
                {
                    //payment is not required
                    _genericAttributeService.SaveAttribute<string>(_workContext.CurrentCustomer,
                        SystemCustomerAttributeNames.SelectedPaymentMethod, null, _storeContext.CurrentStore.Id);

                    var confirmOrderModel = PrepareConfirmOrderModel(cart);
                    return Json(new
                    {
                        update_section = new UpdateSectionJsonModel
                        {
                            name = "confirm-order",
                            html = this.RenderPartialViewToString("OpcConfirmOrder", confirmOrderModel)
                        },
                        goto_section = "confirm_order"
                    });
                }

                var paymentMethodInst = _paymentService.LoadPaymentMethodBySystemName(paymentmethod);
                if (paymentMethodInst == null ||
                    !paymentMethodInst.IsPaymentMethodActive(_paymentSettings) ||
                    !_pluginFinder.AuthenticateStore(paymentMethodInst.PluginDescriptor, _storeContext.CurrentStore.Id))
                {
                    throw new Exception("Selected payment method can't be parsed");
                }

                //save
                _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer,
                    SystemCustomerAttributeNames.SelectedPaymentMethod, paymentmethod, _storeContext.CurrentStore.Id);

                return OpcLoadStepAfterPaymentMethod(paymentMethodInst, cart);
            }
            catch (Exception exc)
            {
                _logger.Warning(exc.Message, exc, _workContext.CurrentCustomer);
                return Json(new { error = 1, message = exc.Message });
            }
        }

        [HttpPost]
        public ActionResult PayTrove(string realId)
        {
            string troveApiCustBaseUrl = String.Empty;
            string troveApiCardsBaseUrl = String.Empty;

            string troveApiVersion = "1.0";
            string consumerId = String.Empty;
            string hash = "7d84adb360717834a2682454f71b63f3abff33ea";

            bool isTroveEnabled = Convert.ToBoolean(Session["TroveEnabledStore"]);
            if (!String.IsNullOrEmpty(ConfigurationManager.AppSettings["TroveBaseUrl"]))
            {
                troveApiCustBaseUrl = ConfigurationManager.AppSettings["TroveBaseUrl"]+ "jcard/api/customers/" + realId + "?cards=true"; ;
                troveApiCardsBaseUrl = ConfigurationManager.AppSettings["TroveBaseUrl"] + "jcard/api/wallets/" + realId + "/tender?cards=true";
            }
            if (!String.IsNullOrEmpty(ConfigurationManager.AppSettings["TroveConsumerId"]))
            {
                consumerId = ConfigurationManager.AppSettings["TroveConsumerId"];
            }

            TroveDetailCustomerModel cust = new TroveDetailCustomerModel();
            List<Card> cards = new List<Card>();
            List<TroveWalletAccountModel> accounts = new List<TroveWalletAccountModel>();

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(troveApiCustBaseUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072; 
                client.DefaultRequestHeaders.Add("version", troveApiVersion);
                client.DefaultRequestHeaders.Add("consumer-id", consumerId);
                client.DefaultRequestHeaders.Add("nonce", Guid.NewGuid().ToString());
                client.DefaultRequestHeaders.Add("hash", hash);
                using (var custResponse = client.GetAsync(troveApiCustBaseUrl).Result)
                {
                    var res = TroveUtilities.ProcessResponseMessage<TroveDetailCustomerResponseModel>(custResponse);
                    cust = res.Content.Customer;

                    //assign RealId to current customer and update the current customer contexxt
                    _workContext.CurrentCustomer.BillingAddress.RealId = cust.RealId;
                    _addressService.UpdateAddress(_workContext.CurrentCustomer.BillingAddress);

                    _httpContext.Session["RealId"] = cust.RealId;
                }
            }

            if (cust != null)
            {
                using (var client2 = new HttpClient())
                {
                    client2.BaseAddress = new Uri(troveApiCardsBaseUrl);
                    client2.DefaultRequestHeaders.Accept.Clear();
                    ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
                    client2.DefaultRequestHeaders.Add("version", troveApiVersion);
                    client2.DefaultRequestHeaders.Add("consumer-id", consumerId);
                    client2.DefaultRequestHeaders.Add("nonce", Guid.NewGuid().ToString());
                    client2.DefaultRequestHeaders.Add("hash", hash);
                    using (var cardResponse = client2.GetAsync(troveApiCardsBaseUrl).Result)
                    {
                        var tenderResponse = TroveUtilities.ProcessResponseMessage<GetWalletTendersResponse>(cardResponse);
                        accounts = tenderResponse.Content.Accounts;
                        cards = tenderResponse.Content.Cards;
                        _httpContext.Session["CustomerBYOCs"] = cards;
                        _httpContext.Session["CampusTenders"] = accounts;
                    }
                }
            }

            UIResponseVieModel uiRes = new UIResponseVieModel();
            uiRes.AvailableCards = new List<SelectListItem>();
            uiRes.AvailableAccounts = new List<SelectListItem>();

            if (cards!= null && cards.Count > 0)
            {
                foreach (Card cc in cards)
                {
                    SelectListItem sli = new SelectListItem();
                    sli.Text = cc.Name;
                    sli.Value = cc.CardId.ToString();
                    uiRes.AvailableCards.Add(sli);
                }
            }

            if (accounts != null && accounts.Count > 0)
            {
                foreach (TroveWalletAccountModel cc in accounts)
                {
                    if (cc.Type == "icb/dcb")
                    {
                        SelectListItem sli2 = new SelectListItem();
                        sli2.Text = cc.Name;
                        sli2.Value = cc.Tender;
                        uiRes.AvailableAccounts.Add(sli2);
                    }
                }
            }
            uiRes.Customer = cust;

            return new JsonResult
            {
                Data = RenderPartialViewToString("OpcTrovePaymentInfo", uiRes),
            };
        }


        [ValidateInput(false)]
        public ActionResult OpcSavePaymentInfo(FormCollection form)
        {
            try
            {
                //validation
                var cart = _workContext.CurrentCustomer.ShoppingCartItems
                    .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                    .LimitPerStore(_storeContext.CurrentStore.Id)
                    .ToList();
                if (!cart.Any())
                {
                    throw new Exception("Your cart is empty");
                }

                if (!_orderSettings.OnePageCheckoutEnabled)
                {
                    throw new Exception("One page checkout is disabled");
                }

                if (_workContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed)
                {
                    throw new Exception("Anonymous checkout is not allowed");
                }

                var paymentMethodSystemName = _workContext.CurrentCustomer.GetAttribute<string>(
                                                        SystemCustomerAttributeNames.SelectedPaymentMethod,
                                                        _genericAttributeService, _storeContext.CurrentStore.Id);
                var paymentMethod = _paymentService.LoadPaymentMethodBySystemName(paymentMethodSystemName);
                if (paymentMethod == null)
                {
                    throw new Exception("Payment method is not selected");
                }

                var paymentControllerType = paymentMethod.GetControllerType();
                var paymentController = DependencyResolver.Current.GetService(paymentControllerType) as BasePaymentController;
                if (paymentController == null)
                {
                    throw new Exception("Payment controller cannot be loaded");
                }

                var warnings = paymentController.ValidatePaymentForm(form);
                foreach (var warning in warnings)
                {
                    ModelState.AddModelError("", warning);
                }

                if (ModelState.IsValid)
                {
                    //get payment info
                    var paymentInfo = paymentController.GetPaymentInfo(form);
                    //session save
                    _httpContext.Session["OrderPaymentInfo"] = paymentInfo;

                    var confirmOrderModel = PrepareConfirmOrderModel(cart);
                    return Json(new
                    {
                        update_section = new UpdateSectionJsonModel
                        {
                            name = "confirm-order",
                            html = this.RenderPartialViewToString("OpcConfirmOrder", confirmOrderModel)
                        },
                        goto_section = "confirm_order"
                    });
                }

                //If we got this far, something failed, redisplay form
                var paymenInfoModel = PreparePaymentInfoModel(paymentMethod);
                return Json(new
                {
                    update_section = new UpdateSectionJsonModel
                    {
                        name = "payment-info",
                        html = this.RenderPartialViewToString("OpcPaymentInfo", paymenInfoModel)
                    }
                });
            }
            catch (Exception exc)
            {
                _logger.Warning(exc.Message, exc, _workContext.CurrentCustomer);
                return Json(new { error = 1, message = exc.Message });
            }
        }

        

        [ValidateInput(false)]
        public ActionResult OpcConfirmOrder()
        {
            try
            {
                bool isTroveEnabled = false;
                isTroveEnabled = Convert.ToBoolean(Session["TroveEnabledStore"]);

                string tender = string.Empty;
                string cardId = string.Empty;

                string paymntMethod = "";
                if (!String.IsNullOrEmpty(_httpContext.Session["SelectedPaymentType"].ToString()))
                    paymntMethod = _httpContext.Session["SelectedPaymentType"].ToString();

                if (isTroveEnabled && paymntMethod == "TrovePayment")
                {
                    string realId = string.Empty;
                    if (!String.IsNullOrEmpty(_httpContext.Session["RealId"].ToString()))
                        realId = _httpContext.Session["RealId"].ToString();   //"f9802154-3be8-402f-acb9-cddf25dd35bf";

                    int biteOrderId = 2310;
                    float amount = (float)_workContext.CurrentCustomer.ShoppingCartItems.LastOrDefault().Product.Price;
                    long systemTraceAuditNumber = TroveUtilities.CreateSystemTraceAuditNumber();
                    string terminalId = String.Empty;
                    string merchantId = String.Empty;

                    if (!String.IsNullOrEmpty(ConfigurationManager.AppSettings["TroveTID"]))
                    {
                        terminalId = ConfigurationManager.AppSettings["TroveTID"];
                    }
                    if (!String.IsNullOrEmpty(ConfigurationManager.AppSettings["TroveMID"]))
                    {
                        merchantId = ConfigurationManager.AppSettings["TroveMID"];
                    }

                    DateTime date = DateTime.Now;
                    const bool loyalty = false;
                    const bool mealPlan = false;
                    const bool redeem = false;

                    TroveWalletsTokenResponseModel result = TroveUtilities.GetWalletsToken(realId);
                    var walletsToken = result as TroveWalletsTokenResponseModel;
                    string token = walletsToken.Token;

                    if (_httpContext.Session["AccountDetails"] != null && !String.IsNullOrEmpty(_httpContext.Session["AccountDetails"].ToString()))
                    {
                        tender = _httpContext.Session["AccountDetails"].ToString();
                        if (tender.Length <= 1)
                        {
                            tender = "0" + tender; // done for QA
                        }
                        PostPaymentToWalletRequest body = new PostPaymentToWalletRequest
                        {
                            mti = "sale",
                            amount = amount,
                            stan = biteOrderId,
                            date = TroveUtilities.CreateDateString(date),
                            tender= tender,
                            tid = terminalId,
                            mid = merchantId,
                            mealPlan = TroveUtilities.CreateFlag(mealPlan),
                            token = token,
                            loyalty = TroveUtilities.CreateFlag(loyalty),
                            redeem = TroveUtilities.CreateFlag(redeem),
                            pcode = TroveUtilities.CreatePcode(PCode.Campus),
                            subscription = null,
                        };

                       
                        PostPaymentToWalletResponse ccPaymentResponse = TroveUtilities.PostPaymentToWallet(body);
                        if (ccPaymentResponse.Success)
                        {
                            ProcessPaymentResult processedPaymentDetails = new ProcessPaymentResult();
                            processedPaymentDetails.TroveResultCode = ccPaymentResponse.ResultCode;
                            processedPaymentDetails.TroveResultMsg = ccPaymentResponse.ResultMsg;
                            processedPaymentDetails.TroveSuccess = ccPaymentResponse.Success;
                            processedPaymentDetails.TroveTenderId = ccPaymentResponse.TenderId;
                            processedPaymentDetails.TroveAmount = ccPaymentResponse.amount;
                            processedPaymentDetails.TroveApprovalCode = ccPaymentResponse.approvalCode;
                            processedPaymentDetails.TroveBalance = ccPaymentResponse.balance;
                            processedPaymentDetails.TroveId = ccPaymentResponse.id;
                            processedPaymentDetails.TroveRrn = ccPaymentResponse.rrn;
                            _httpContext.Session["OrderTrovePaymentInfo"] = processedPaymentDetails;
                        }
                        else
                        {
                            throw new Exception(ccPaymentResponse.ResultMsg);
                        }
                    }

                    if (_httpContext.Session["CardDetails"] != null && !String.IsNullOrEmpty(_httpContext.Session["CardDetails"].ToString()))
                    {
                        cardId = _httpContext.Session["CardDetails"].ToString();
                        PostPaymentToWalletRequest body = new PostPaymentToWalletRequest
                        {
                            mti = "sale",
                            amount = amount,
                            stan = biteOrderId,
                            date = TroveUtilities.CreateDateString(date),
                            tid = terminalId,
                            mid = merchantId,
                            mealPlan = TroveUtilities.CreateFlag(mealPlan),
                            token = token,
                            loyalty = TroveUtilities.CreateFlag(loyalty),
                            redeem = TroveUtilities.CreateFlag(redeem),
                            pcode = TroveUtilities.CreatePcode(PCode.Credit),
                            subscription = null,
                            cardId = cardId,
                        };

                        PostPaymentToWalletResponse ccPaymentResponse = TroveUtilities.PostPaymentToWallet(body);
                        if (ccPaymentResponse.Success)
                        {
                            ProcessPaymentResult processedPaymentDetails = new ProcessPaymentResult();
                            processedPaymentDetails.TroveResultCode = ccPaymentResponse.ResultCode;
                            processedPaymentDetails.TroveResultMsg = ccPaymentResponse.ResultMsg;
                            processedPaymentDetails.TroveSuccess = ccPaymentResponse.Success;
                            processedPaymentDetails.TroveTenderId = ccPaymentResponse.TenderId;
                            processedPaymentDetails.TroveAmount = ccPaymentResponse.amount;
                            processedPaymentDetails.TroveApprovalCode = ccPaymentResponse.approvalCode;
                            processedPaymentDetails.TroveBalance = ccPaymentResponse.balance;
                            processedPaymentDetails.TroveId = ccPaymentResponse.id;
                            processedPaymentDetails.TroveRrn = ccPaymentResponse.rrn;
                            processedPaymentDetails.AllowStoringCreditCardNumber = true;
                            _httpContext.Session["OrderTrovePaymentInfo"] = processedPaymentDetails;
                        }
                        else
                        {
                            throw new Exception(ccPaymentResponse.ResultMsg);
                        }
                    }
                      
                }

                List<ShoppingCartItem> cartitems = _workContext.CurrentCustomer.ShoppingCartItems.Where(sc => sc.IsReservation).ToList();
                if (cartitems.Any())
                {
                    foreach (ShoppingCartItem sc in cartitems)
                    {
                        if (sc.IsReservation && String.IsNullOrEmpty(sc.ReservationDate.ToString()))
                        {
                            throw new Exception("Reservation date cannot be empty.");
                        }

                        if (sc.IsReservation && String.IsNullOrEmpty(sc.ReservedTimeSlot))
                        {
                            throw new Exception("Reserved time slot cannot be empty.");
                        }
                    }
                }

                //validation
                var cart = _workContext.CurrentCustomer.ShoppingCartItems
                    .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                    .LimitPerStore(_storeContext.CurrentStore.Id)
                    .ToList();
                if (!cart.Any())
                {
                    throw new Exception("Your cart is empty");
                }

                if (!_orderSettings.OnePageCheckoutEnabled)
                {
                    throw new Exception("One page checkout is disabled");
                }

                if (_workContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed)
                {
                    throw new Exception("Anonymous checkout is not allowed");
                }

                //prevent 2 orders being placed within an X seconds time frame
                if (!IsMinimumOrderPlacementIntervalValid(_workContext.CurrentCustomer))
                {
                    throw new Exception(_localizationService.GetResource("Checkout.MinOrderPlacementInterval"));
                }

                //place order
                var processPaymentRequest = new ProcessPaymentRequest();
                if (_httpContext.Session["OrderPaymentInfo"] != null)
                {
                    processPaymentRequest = _httpContext.Session["OrderPaymentInfo"] as ProcessPaymentRequest ;
                }
               
                if (!isTroveEnabled)
                {
                    if (processPaymentRequest == null)
                    {
                        //Check whether payment workflow is required
                        if (IsPaymentWorkflowRequired(cart))
                        {
                            throw new Exception("Payment information is not entered");
                        }
                        else
                        {
                            processPaymentRequest = new ProcessPaymentRequest();
                        }
                    }
                }


                processPaymentRequest.StoreId = _storeContext.CurrentStore.Id;
                processPaymentRequest.CustomerId = _workContext.CurrentCustomer.Id;
                processPaymentRequest.PaymentMethodSystemName = _workContext.CurrentCustomer.GetAttribute<string>(
                    SystemCustomerAttributeNames.SelectedPaymentMethod,
                    _genericAttributeService, _storeContext.CurrentStore.Id);

                var placeOrderResult = _orderProcessingService.PlaceOrder(processPaymentRequest);

                if (placeOrderResult.Success)
                {
                    _httpContext.Session["OrderPaymentInfo"] = null;
                    _httpContext.Session["OrderTrovePaymentInfo"] = null;

                    var postProcessPaymentRequest = new PostProcessPaymentRequest
                    {
                        Order = placeOrderResult.PlacedOrder
                    };


                    var paymentMethod = _paymentService.LoadPaymentMethodBySystemName(placeOrderResult.PlacedOrder.PaymentMethodSystemName);
                    if (paymentMethod == null)
                    {
                        //payment method could be null if order total is 0
                        //success
                        return Json(new { success = 1 });
                    }

                    if (paymentMethod.PaymentMethodType == PaymentMethodType.Redirection)
                    {
                        //Redirection will not work because it's AJAX request.
                        //That's why we don't process it here (we redirect a user to another page where he'll be redirected)

                        //redirect
                        return Json(new
                        {
                            redirect = string.Format("{0}checkout/OpcCompleteRedirectionPayment", _webHelper.GetStoreLocation())
                        });
                    }

                    _paymentService.PostProcessPayment(postProcessPaymentRequest);

                    //---START: Codechages done by (na-sdxcorp\ADas)--------------
                    //Automatically create shipments for order items that have UPS shipping selected

                    //Get the order
                    var order = _orderService.GetOrderById(placeOrderResult.PlacedOrder.Id);

                    var orderItems = order.OrderItems;


                    foreach (var orderItem in orderItems)
                    {
                        bool shipmentWithUPSAPI_Failed = false;
                        Shipment shipment = null;
                        decimal? totalWeight = null;

                        //is shippable
                        if (!orderItem.Product.IsShipEnabled)
                        {
                            continue;
                        }

                        //is UPS shipping selected
                        if (orderItem.SelectedShippingRateComputationMethodSystemName != null)
                        {
                            if (!orderItem.SelectedShippingRateComputationMethodSystemName.Equals("Shipping.UPS"))
                            {
                                continue;
                            }
                        }

                        //ensure that this product can be shipped (have at least one item to ship)
                        var maxQtyToAdd = orderItem.GetTotalNumberOfItemsCanBeAddedToShipment();
                        if (maxQtyToAdd <= 0)
                        {
                            continue;
                        }

                        //ok. we have at least one item. let's create a shipment (if it does not exist)

                        var orderItemTotalWeight = orderItem.ItemWeight.HasValue ? orderItem.ItemWeight * orderItem.Quantity : null;
                        if (orderItemTotalWeight.HasValue)
                        {
                            if (!totalWeight.HasValue)
                            {
                                totalWeight = 0;
                            }

                            totalWeight += orderItemTotalWeight.Value;
                        }

                        // var shipmentWithUPSAPIResult = _shippingService.CreateShipmentWithUPSAPI(orderItem); //WE SHALL NOT CREATE UPS TACKING NUMBER AND GENERATE LABEL FROM HERE ANYMORE SO COMMENTED
                        var shipmentWithUPSAPIResult = string.Empty;

                        shipmentWithUPSAPI_Failed = shipmentWithUPSAPIResult == string.Empty ? false : true;

                        if (!shipmentWithUPSAPI_Failed)
                        {
                            //var trackingNumber = shipmentWithUPSAPIResult[0]; //"Track-" + trackNo;  //To be taken from UPS Shipping API
                            //var shippingLabelImageBase64 = shipmentWithUPSAPIResult[1];
                            for (int i = 1; i <= orderItem.Quantity; i++)
                            {
                                var adminComment = "";

                                shipment = new Shipment
                                {
                                    OrderId = order.Id,
                                    TrackingNumber = string.Empty,
                                    TotalWeight = null,
                                    ShippedDateUtc = null,
                                    DeliveryDateUtc = null,
                                    AdminComment = adminComment,
                                    CreatedOnUtc = DateTime.UtcNow,
                                    //OrderItemId = orderItem.Id,
                                    //ShippingLabelImage = Convert.FromBase64String(string.Empty)
                                };


                                //create a shipment item
                                var shipmentItem = new ShipmentItem
                                {
                                    OrderItemId = orderItem.Id,
                                    Quantity = 1,//always set this to 1 so that the products are always seperated.
                                    WarehouseId = orderItem.Product.WarehouseId
                                };
                                shipment.ShipmentItems.Add(shipmentItem);

                                //if we have at least one item in the shipment, then save it
                                if (shipment != null && shipment.ShipmentItems.Any())
                                {
                                    //shipment.TotalWeight = totalWeight; orderItem.ItemWeight
                                    shipment.TotalWeight = orderItem.ItemWeight; // Setting this to individual weight of the item so that the weight does not get multiplied.
                                    _shipmentService.InsertShipment(shipment);

                                    //add a note
                                    order.OrderNotes.Add(new OrderNote
                                    {
                                        Note = "A shipment has been Automatically Created",
                                        DisplayToCustomer = false,
                                        CreatedOnUtc = DateTime.UtcNow
                                    });
                                    _orderService.UpdateOrder(order);
                                    LogEditOrder(order.Id);


                                }

                            }
                        }
                        else
                        {
                            //If shipmentWithUPSAPI Failed add a note
                            order.OrderNotes.Add(new OrderNote
                            {
                                Note = "Automatic Shipment with UPS Failed.",
                                DisplayToCustomer = false,
                                CreatedOnUtc = DateTime.UtcNow
                            });
                            _orderService.UpdateOrder(order);
                            LogEditOrder(order.Id);
                        }
                    }

                    //---END: Codechages done by (na-sdxcorp\ADas)--------------

                    //success
                    return Json(new { success = 1 });
                }

                //error
                var confirmOrderModel = new CheckoutConfirmModel();
                foreach (var error in placeOrderResult.Errors)
                {
                    confirmOrderModel.Warnings.Add(error);
                }

                return Json(new
                {
                    update_section = new UpdateSectionJsonModel
                    {
                        name = "confirm-order",
                        html = this.RenderPartialViewToString("OpcConfirmOrder", confirmOrderModel)
                    },
                    goto_section = "confirm_order"
                });
            }
            catch (Exception exc)
            {
                _logger.Warning(exc.Message, exc, _workContext.CurrentCustomer);
                return Json(new { error = 1, message = exc.Message });
            }
        }

        //---START: Codechages done by (na-sdxcorp\ADas)--------------
        [NonAction]
        protected void LogEditOrder(int orderId)
        {
            _customerActivityService.InsertActivity("EditOrder", _localizationService.GetResource("ActivityLog.EditOrder"), orderId);
        }
        //---END: Codechages done by (na-sdxcorp\ADas)--------------

        public ActionResult OpcCompleteRedirectionPayment()
        {
            try
            {
                //validation
                if (!_orderSettings.OnePageCheckoutEnabled)
                {
                    return RedirectToRoute("HomePage");
                }

                if (_workContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed)
                {
                    return new HttpUnauthorizedResult();
                }

                //get the order
                var order = _orderService.SearchOrders(storeId: _storeContext.CurrentStore.Id,
                customerId: _workContext.CurrentCustomer.Id, pageSize: 1)
                    .FirstOrDefault();
                if (order == null)
                {
                    return RedirectToRoute("HomePage");
                }

                var paymentMethod = _paymentService.LoadPaymentMethodBySystemName(order.PaymentMethodSystemName);
                if (paymentMethod == null)
                {
                    return RedirectToRoute("HomePage");
                }

                if (paymentMethod.PaymentMethodType != PaymentMethodType.Redirection)
                {
                    return RedirectToRoute("HomePage");
                }

                //ensure that order has been just placed
                if ((DateTime.UtcNow - order.CreatedOnUtc).TotalMinutes > 3)
                {
                    return RedirectToRoute("HomePage");
                }


                //Redirection will not work on one page checkout page because it's AJAX request.
                //That's why we process it here
                var postProcessPaymentRequest = new PostProcessPaymentRequest
                {
                    Order = order
                };

                _paymentService.PostProcessPayment(postProcessPaymentRequest);

                if (_webHelper.IsRequestBeingRedirected || _webHelper.IsPostBeingDone)
                {
                    //redirection or POST has been done in PostProcessPayment
                    return Content("Redirected");
                }

                //if no redirection has been done (to a third-party payment page)
                //theoretically it's not possible
                return RedirectToRoute("CheckoutCompleted", new { orderId = order.Id });
            }
            catch (Exception exc)
            {
                _logger.Warning(exc.Message, exc, _workContext.CurrentCustomer);
                return Content(exc.Message);
            }
        }

        #endregion

        #region SODMYWAY-2941
        /// <summary>
        /// </summary>
        /// <param name="formItemToLookFor"></param>
        /// <param name="form"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public string findKey(string formItemToLookFor, FormCollection form, int id)
        {
            String[] formKeys = form.AllKeys;

            foreach (String key in formKeys)
            {
                if (key.Contains(formItemToLookFor + "__" + id.ToString()))
                {
                    return key;
                }
            }
            return null;
        }
        #endregion
    }
}
