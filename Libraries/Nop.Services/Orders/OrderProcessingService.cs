using Nop.Core;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Logging;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Stores;
using Nop.Core.Domain.Tax;
using Nop.Core.Domain.Vendors;
using Nop.Core.Infrastructure;
using Nop.Core.Plugins;
using Nop.Services.Affiliates;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Payments;
using Nop.Services.SAP;
using Nop.Services.Security;
using Nop.Services.Shipping;
using Nop.Services.Tax;
using Nop.Services.Vendors;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Web;

namespace Nop.Services.Orders
{
    /// <summary>
    /// Order processing service
    /// </summary>
    public partial class OrderProcessingService : IOrderProcessingService
    {
        #region Fields

        private readonly IStoreContext _storeContext;
        private readonly IOrderService _orderService;
        private readonly IWebHelper _webHelper;
        private readonly ILocalizationService _localizationService;
        private readonly ILanguageService _languageService;
        private readonly IProductService _productService;
        private readonly IPaymentService _paymentService;
        private readonly ILogger _logger;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IProductAttributeFormatter _productAttributeFormatter;
        private readonly IGiftCardService _giftCardService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly ICheckoutAttributeFormatter _checkoutAttributeFormatter;
        private readonly IShippingService _shippingService;
        private readonly IShipmentService _shipmentService;
        private readonly ITaxService _taxService;
        private readonly ICustomerService _customerService;
        private readonly IDiscountService _discountService;
        private readonly IEncryptionService _encryptionService;
        private readonly IWorkContext _workContext;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly IVendorService _vendorService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ICurrencyService _currencyService;
        private readonly IAffiliateService _affiliateService;
        private readonly IEventPublisher _eventPublisher;
        private readonly IPdfService _pdfService;
        private readonly IRewardPointService _rewardPointService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ICountryService _countryService;
        private readonly IGlCodeService _glCodeService;

        private readonly IRepository<OrderItem> _orderItemRepository;

        private readonly ShippingSettings _shippingSettings;
        private readonly PaymentSettings _paymentSettings;
        private readonly RewardPointsSettings _rewardPointsSettings;
        private readonly OrderSettings _orderSettings;
        private readonly TaxSettings _taxSettings;
        private readonly LocalizationSettings _localizationSettings;
        private readonly CurrencySettings _currencySettings;

        private readonly IDonationService _donationService;
        private readonly IMealPlanService _mealPlanService;

        private readonly ISAPService _SAPService;

        private readonly IMessageTemplateService _messageTemplateService;

        private readonly IPluginFinder _pluginFinder;
        private readonly HttpContextBase _httpContext;
        private readonly IRepository<Warehouse> _warehouseRepository;
        private readonly IRepository<Address> _addressRepository;
        private readonly IRepository<Store> _storeRepository;
        private readonly IRepository<StateProvince> _stateProvinceRepository;
        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="orderService">Order service</param>
        /// <param name="webHelper">Web helper</param>
        /// <param name="localizationService">Localization service</param>
        /// <param name="languageService">Language service</param>
        /// <param name="productService">Product service</param>
        /// <param name="paymentService">Payment service</param>
        /// <param name="logger">Logger</param>
        /// <param name="orderTotalCalculationService">Order total calculationservice</param>
        /// <param name="priceCalculationService">Price calculation service</param>
        /// <param name="priceFormatter">Price formatter</param>
        /// <param name="productAttributeParser">Product attribute parser</param>
        /// <param name="productAttributeFormatter">Product attribute formatter</param>
        /// <param name="giftCardService">Gift card service</param>
        /// <param name="shoppingCartService">Shopping cart service</param>
        /// <param name="checkoutAttributeFormatter">Checkout attribute service</param>
        /// <param name="shippingService">Shipping service</param>
        /// <param name="shipmentService">Shipment service</param>
        /// <param name="taxService">Tax service</param>
        /// <param name="customerService">Customer service</param>
        /// <param name="discountService">Discount service</param>
        /// <param name="encryptionService">Encryption service</param>
        /// <param name="workContext">Work context</param>
        /// <param name="workflowMessageService">Workflow message service</param>
        /// <param name="vendorService">Vendor service</param>
        /// <param name="customerActivityService">Customer activity service</param>
        /// <param name="currencyService">Currency service</param>
        /// <param name="affiliateService">Affiliate service</param>
        /// <param name="eventPublisher">Event published</param>
        /// <param name="pdfService">PDF service</param>
        /// <param name="rewardPointService">Reward point service</param>
        /// <param name="genericAttributeService">Generic attribute service</param>
        /// <param name="countryService">Country service</param>	
        /// <param name="paymentSettings">Payment settings</param>
        /// <param name="shippingSettings">Shipping settings</param>
        /// <param name="rewardPointsSettings">Reward points settings</param>
        /// <param name="orderSettings">Order settings</param>
        /// <param name="taxSettings">Tax settings</param>
        /// <param name="localizationSettings">Localization settings</param>
        /// <param name="currencySettings">Currency settings</param>
        public OrderProcessingService(IOrderService orderService,
            IWebHelper webHelper,
            ILocalizationService localizationService,
            ILanguageService languageService,
            IProductService productService,
            IPaymentService paymentService,
            ILogger logger,
            IOrderTotalCalculationService orderTotalCalculationService,
            IPriceCalculationService priceCalculationService,
            IPriceFormatter priceFormatter,
            IProductAttributeParser productAttributeParser,
            IProductAttributeFormatter productAttributeFormatter,
            IGiftCardService giftCardService,
            IShoppingCartService shoppingCartService,
            ICheckoutAttributeFormatter checkoutAttributeFormatter,
            IShippingService shippingService,
            IShipmentService shipmentService,
            ITaxService taxService,
            ICustomerService customerService,
            IDiscountService discountService,
            IEncryptionService encryptionService,
            IWorkContext workContext,
            IWorkflowMessageService workflowMessageService,
            IVendorService vendorService,
            ICustomerActivityService customerActivityService,
            ICurrencyService currencyService,
            IAffiliateService affiliateService,
            IEventPublisher eventPublisher,
            IPdfService pdfService,
            IRewardPointService rewardPointService,
            IGenericAttributeService genericAttributeService,
            ICountryService countryService,
            ShippingSettings shippingSettings,
            PaymentSettings paymentSettings,
            RewardPointsSettings rewardPointsSettings,
            OrderSettings orderSettings,
            TaxSettings taxSettings,
            LocalizationSettings localizationSettings,
            CurrencySettings currencySettings,
            IDonationService donationService,
            IMealPlanService mealPlanService,
            ISAPService SAPService,
            IMessageTemplateService messageTemplateService,
            IRepository<OrderItem> orderItemRepository,
            IPluginFinder pluginFinder,
            HttpContextBase httpContext,
            IGlCodeService glCodeService,
            IRepository<Warehouse> warehouseRepository,
            IRepository<Address> addressRepository,
            IRepository<Store> storeRepository,
            IRepository<StateProvince> stateProvinceRepository,
            IStoreContext storeContext)

        {
            this._orderService = orderService;
            this._storeContext = storeContext;
            this._webHelper = webHelper;
            this._localizationService = localizationService;
            this._languageService = languageService;
            this._productService = productService;
            this._paymentService = paymentService;
            this._logger = logger;
            this._orderTotalCalculationService = orderTotalCalculationService;
            this._priceCalculationService = priceCalculationService;
            this._priceFormatter = priceFormatter;
            this._productAttributeParser = productAttributeParser;
            this._productAttributeFormatter = productAttributeFormatter;
            this._giftCardService = giftCardService;
            this._shoppingCartService = shoppingCartService;
            this._checkoutAttributeFormatter = checkoutAttributeFormatter;
            this._workContext = workContext;
            this._workflowMessageService = workflowMessageService;
            this._vendorService = vendorService;
            this._shippingService = shippingService;
            this._shipmentService = shipmentService;
            this._taxService = taxService;
            this._customerService = customerService;
            this._discountService = discountService;
            this._encryptionService = encryptionService;
            this._customerActivityService = customerActivityService;
            this._currencyService = currencyService;
            this._affiliateService = affiliateService;
            this._eventPublisher = eventPublisher;
            this._pdfService = pdfService;
            this._rewardPointService = rewardPointService;
            this._genericAttributeService = genericAttributeService;
            this._countryService = countryService;
            this._paymentSettings = paymentSettings;
            this._shippingSettings = shippingSettings;
            this._rewardPointsSettings = rewardPointsSettings;
            this._orderSettings = orderSettings;
            this._taxSettings = taxSettings;
            this._localizationSettings = localizationSettings;
            this._currencySettings = currencySettings;
            this._donationService = donationService;
            this._mealPlanService = mealPlanService;
            this._SAPService = SAPService;
            this._messageTemplateService = messageTemplateService;
            this._pluginFinder = pluginFinder;
            this._glCodeService = glCodeService;
            this._httpContext = httpContext;
            this._warehouseRepository = warehouseRepository;
            this._addressRepository = addressRepository;
            this._storeRepository = storeRepository;
            this._stateProvinceRepository = stateProvinceRepository;
            this._orderItemRepository = orderItemRepository;
        }

        #endregion

        #region Nested classes

        public class PlaceOrderContainter
        {
            public PlaceOrderContainter()
            {
                this.Cart = new List<ShoppingCartItem>();
                this.AppliedDiscounts = new List<Discount>();
                this.AppliedGiftCards = new List<AppliedGiftCard>();
            }

            public Customer Customer { get; set; }
            public Language CustomerLanguage { get; set; }
            public int AffiliateId { get; set; }
            public TaxDisplayType CustomerTaxDisplayType { get; set; }
            public string CustomerCurrencyCode { get; set; }
            public decimal CustomerCurrencyRate { get; set; }

            public Address BillingAddress { get; set; }
            public Address ShippingAddress { get; set; }

            public ShippingStatus ShippingStatus { get; set; }
            public string ShippingMethodName { get; set; }
            public string ShippingRateComputationMethodSystemName { get; set; }
            public bool PickUpInStore { get; set; }
            public Address PickupAddress { get; set; }

            public bool IsRecurringShoppingCart { get; set; }

            //initial order (used with recurring payments)
            public Order InitialOrder { get; set; }

            public string CheckoutAttributeDescription { get; set; }
            public string CheckoutAttributesXml { get; set; }

            public IList<ShoppingCartItem> Cart { get; set; }
            public List<Discount> AppliedDiscounts { get; set; }
            public List<AppliedGiftCard> AppliedGiftCards { get; set; }

            public decimal OrderSubTotalInclTax { get; set; }
            public decimal OrderSubTotalExclTax { get; set; }
            public decimal OrderSubTotalDiscountInclTax { get; set; }
            public decimal OrderSubTotalDiscountExclTax { get; set; }
            public decimal OrderShippingTotalInclTax { get; set; }
            public decimal OrderShippingTotalExclTax { get; set; }
            public decimal PaymentAdditionalFeeInclTax { get; set; }
            public decimal PaymentAdditionalFeeExclTax { get; set; }
            public decimal OrderTaxTotal { get; set; }
            public string VatNumber { get; set; }
            public string TaxRates { get; set; }
            public decimal OrderDiscountAmount { get; set; }
            public int RedeemedRewardPoints { get; set; }
            public decimal RedeemedRewardPointsAmount { get; set; }
            public decimal OrderTotal { get; set; }
            public decimal OrderTotalOnRefund { get; set; }

        }

        #endregion

        #region Utilities

        /// <summary>
        /// Prepare details to place an order. It also sets some properties to "processPaymentRequest"
        /// </summary>
        /// <param name="processPaymentRequest">Process payment request</param>
        /// <returns>Details</returns>
        protected virtual PlaceOrderContainter PreparePlaceOrderDetails(ProcessPaymentRequest processPaymentRequest)
        {
            var details = new PlaceOrderContainter();

            //customer
            details.Customer = _customerService.GetCustomerById(processPaymentRequest.CustomerId);
            if (details.Customer == null)
            {
                throw new ArgumentException("Customer is not set");
            }

            //affiliate
            var affiliate = _affiliateService.GetAffiliateById(details.Customer.AffiliateId);
            if (affiliate != null && affiliate.Active && !affiliate.Deleted)
            {
                details.AffiliateId = affiliate.Id;
            }

            //check whether customer is guest
            if (details.Customer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed)
            {
                throw new NopException("Anonymous checkout is not allowed");
            }

            //customer currency
            var currencyTmp = _currencyService.GetCurrencyById(
                details.Customer.GetAttribute<int>(SystemCustomerAttributeNames.CurrencyId, processPaymentRequest.StoreId));
            var customerCurrency = (currencyTmp != null && currencyTmp.Published) ? currencyTmp : _workContext.WorkingCurrency;
            var primaryStoreCurrency = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId);
            details.CustomerCurrencyCode = customerCurrency.CurrencyCode;
            details.CustomerCurrencyRate = customerCurrency.Rate / primaryStoreCurrency.Rate;

            //customer language
            details.CustomerLanguage = _languageService.GetLanguageById(
                details.Customer.GetAttribute<int>(SystemCustomerAttributeNames.LanguageId, processPaymentRequest.StoreId));
            if (details.CustomerLanguage == null || !details.CustomerLanguage.Published)
            {
                details.CustomerLanguage = _workContext.WorkingLanguage;
            }

            //billing address
            if (details.Customer.BillingAddress == null)
            {
                throw new NopException("Billing address is not provided");
            }

            if (!CommonHelper.IsValidEmail(details.Customer.BillingAddress.Email))
            {
                throw new NopException("Email is not valid");
            }

            details.BillingAddress = (Address)details.Customer.BillingAddress.Clone();
            if (details.BillingAddress.Country != null && !details.BillingAddress.Country.AllowsBilling)
            {
                throw new NopException(string.Format("Country '{0}' is not allowed for billing", details.BillingAddress.Country.Name));
            }

            //checkout attributes
            details.CheckoutAttributesXml = details.Customer.GetAttribute<string>(SystemCustomerAttributeNames.CheckoutAttributes, processPaymentRequest.StoreId);
            details.CheckoutAttributeDescription = _checkoutAttributeFormatter.FormatAttributes(details.CheckoutAttributesXml, details.Customer);

            //load shopping cart
            details.Cart = details.Customer.ShoppingCartItems.Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(processPaymentRequest.StoreId).ToList();

            if (!details.Cart.Any())
            {
                throw new NopException("Cart is empty");
            }

            //validate the entire shopping cart
            var warnings = _shoppingCartService.GetShoppingCartWarnings(details.Cart, details.CheckoutAttributesXml, true);
            if (warnings.Any())
            {
                throw new NopException(warnings.Aggregate(string.Empty, (current, next) => string.Format("{0}{1};", current, next)));
            }

            //validate individual cart items
            foreach (var sci in details.Cart)
            {
                var sciWarnings = _shoppingCartService.GetShoppingCartItemWarnings(details.Customer,
                    sci.ShoppingCartType, sci.Product, processPaymentRequest.StoreId, sci.AttributesXml,
                    sci.CustomerEnteredPrice, sci.RentalStartDateUtc, sci.RentalEndDateUtc, sci.Quantity, false);
                if (sciWarnings.Any())
                {
                    throw new NopException(sciWarnings.Aggregate(string.Empty, (current, next) => string.Format("{0}{1};", current, next)));
                }
            }

            //min totals validation
            if (!ValidateMinOrderSubtotalAmount(details.Cart))
            {
                var minOrderSubtotalAmount = _currencyService.ConvertFromPrimaryStoreCurrency(_orderSettings.MinOrderSubtotalAmount, _workContext.WorkingCurrency);
                throw new NopException(string.Format(_localizationService.GetResource("Checkout.MinOrderSubtotalAmount"),
                    _priceFormatter.FormatPrice(minOrderSubtotalAmount, true, false)));
            }

            if (!ValidateMinOrderTotalAmount(details.Cart))
            {
                var minOrderTotalAmount = _currencyService.ConvertFromPrimaryStoreCurrency(_orderSettings.MinOrderTotalAmount, _workContext.WorkingCurrency);
                throw new NopException(string.Format(_localizationService.GetResource("Checkout.MinOrderTotalAmount"),
                    _priceFormatter.FormatPrice(minOrderTotalAmount, true, false)));
            }

            //tax display type
            if (_taxSettings.AllowCustomersToSelectTaxDisplayType)
            {
                details.CustomerTaxDisplayType = (TaxDisplayType)details.Customer.GetAttribute<int>(SystemCustomerAttributeNames.TaxDisplayTypeId, processPaymentRequest.StoreId);
            }
            else
            {
                details.CustomerTaxDisplayType = _taxSettings.TaxDisplayType;
            }

            //sub total (incl tax)
            decimal orderSubTotalDiscountAmount;
            List<Discount> orderSubTotalAppliedDiscounts;
            decimal subTotalWithoutDiscountBase;
            decimal subTotalWithDiscountBase;
            _orderTotalCalculationService.GetShoppingCartSubTotal(details.Cart, true, out orderSubTotalDiscountAmount,
                out orderSubTotalAppliedDiscounts, out subTotalWithoutDiscountBase, out subTotalWithDiscountBase);
            details.OrderSubTotalInclTax = subTotalWithoutDiscountBase;
            details.OrderSubTotalDiscountInclTax = orderSubTotalDiscountAmount;

            //discount history
            foreach (var disc in orderSubTotalAppliedDiscounts)
            {
                if (!details.AppliedDiscounts.ContainsDiscount(disc))
                {
                    details.AppliedDiscounts.Add(disc);
                }
            }

            //sub total (excl tax)
            _orderTotalCalculationService.GetShoppingCartSubTotal(details.Cart, false, out orderSubTotalDiscountAmount,
                out orderSubTotalAppliedDiscounts, out subTotalWithoutDiscountBase, out subTotalWithDiscountBase);
            details.OrderSubTotalExclTax = subTotalWithoutDiscountBase;
            details.OrderSubTotalDiscountExclTax = orderSubTotalDiscountAmount;

            //shipping info
            if (details.Cart.RequiresShipping())
            {
                var pickupPoint = details.Customer.GetAttribute<PickupPoint>(SystemCustomerAttributeNames.SelectedPickupPoint, processPaymentRequest.StoreId);
                if (_shippingSettings.AllowPickUpInStore && pickupPoint != null)
                {
                    details.PickUpInStore = true;
                    details.PickupAddress = new Address
                    {
                        Address1 = pickupPoint.Address,
                        City = pickupPoint.City,
                        Country = _countryService.GetCountryByTwoLetterIsoCode(pickupPoint.CountryCode),
                        ZipPostalCode = pickupPoint.ZipPostalCode,
                        CreatedOnUtc = DateTime.UtcNow,
                    };
                }
                else
                {
                    if (details.Customer.ShippingAddress == null)
                    {
                        throw new NopException("Shipping address is not provided");
                    }

                    if (!CommonHelper.IsValidEmail(details.Customer.ShippingAddress.Email))
                    {
                        throw new NopException("Email is not valid");
                    }

                    //clone shipping address
                    details.ShippingAddress = (Address)details.Customer.ShippingAddress.Clone();
                    if (details.ShippingAddress.Country != null && !details.ShippingAddress.Country.AllowsShipping)
                    {
                        throw new NopException(string.Format("Country '{0}' is not allowed for shipping", details.ShippingAddress.Country.Name));
                    }
                }

                var shippingOption = details.Customer.GetAttribute<ShippingOption>(SystemCustomerAttributeNames.SelectedShippingOption, processPaymentRequest.StoreId);
                if (shippingOption != null)
                {
                    details.ShippingMethodName = shippingOption.Name;
                    details.ShippingRateComputationMethodSystemName = shippingOption.ShippingRateComputationMethodSystemName;
                }

                details.ShippingStatus = ShippingStatus.NotYetShipped;
            }
            else
            {
                details.ShippingStatus = ShippingStatus.ShippingNotRequired;
            }

            if (_storeContext.CurrentStore.IsTieredShippingEnabled == true)
            {
                details.ShippingMethodName = "Tiered Shipping";
                details.ShippingRateComputationMethodSystemName = "Tiered Shipping";
            }

            //shipping total
            decimal tax;
            List<Discount> shippingTotalDiscounts;
            var orderShippingTotalInclTax = _orderTotalCalculationService.GetShoppingCartShippingTotal(details.Cart, true, out tax, out shippingTotalDiscounts);
            var orderShippingTotalExclTax = _orderTotalCalculationService.GetShoppingCartShippingTotal(details.Cart, false);

            _httpContext.Session["orderShippingTotalExclTax"] = orderShippingTotalExclTax;
            if (!orderShippingTotalInclTax.HasValue || !orderShippingTotalExclTax.HasValue)
            {
                throw new NopException("Shipping total couldn't be calculated");
            }

            details.OrderShippingTotalInclTax = orderShippingTotalInclTax.Value;
            details.OrderShippingTotalExclTax = orderShippingTotalExclTax.Value;

            foreach (var disc in shippingTotalDiscounts)
            {
                if (!details.AppliedDiscounts.ContainsDiscount(disc))
                {
                    details.AppliedDiscounts.Add(disc);
                }
            }

            //payment total
            var paymentAdditionalFee = _paymentService.GetAdditionalHandlingFee(details.Cart, processPaymentRequest.PaymentMethodSystemName);
            details.PaymentAdditionalFeeInclTax = _taxService.GetPaymentMethodAdditionalFee(paymentAdditionalFee, true, details.Customer);
            details.PaymentAdditionalFeeExclTax = _taxService.GetPaymentMethodAdditionalFee(paymentAdditionalFee, false, details.Customer);

            //tax amount
            SortedDictionary<decimal, decimal> taxRatesDictionary;
            details.OrderTaxTotal = _orderTotalCalculationService.GetTaxTotal(details.Cart, out taxRatesDictionary);

            //VAT number
            var customerVatStatus = (VatNumberStatus)details.Customer.GetAttribute<int>(SystemCustomerAttributeNames.VatNumberStatusId);
            if (_taxSettings.EuVatEnabled && customerVatStatus == VatNumberStatus.Valid)
            {
                details.VatNumber = details.Customer.GetAttribute<string>(SystemCustomerAttributeNames.VatNumber);
            }

            //tax rates
            details.TaxRates = taxRatesDictionary.Aggregate(string.Empty, (current, next) =>
                string.Format("{0}{1}:{2};   ", current, next.Key.ToString(CultureInfo.InvariantCulture), next.Value.ToString(CultureInfo.InvariantCulture)));

            //order total (and applied discounts, gift cards, reward points)
            List<AppliedGiftCard> appliedGiftCards;
            List<Discount> orderAppliedDiscounts;
            decimal orderDiscountAmount;
            int redeemedRewardPoints;
            decimal redeemedRewardPointsAmount;
            var orderTotal = _orderTotalCalculationService.GetShoppingCartTotal(details.Cart, out orderDiscountAmount,
                out orderAppliedDiscounts, out appliedGiftCards, out redeemedRewardPoints, out redeemedRewardPointsAmount);
            if (!orderTotal.HasValue)
            {
                throw new NopException("Order total couldn't be calculated");
            }

            details.OrderDiscountAmount = orderDiscountAmount;
            details.RedeemedRewardPoints = redeemedRewardPoints;
            details.RedeemedRewardPointsAmount = redeemedRewardPointsAmount;
            details.AppliedGiftCards = appliedGiftCards;
            details.OrderTotal = orderTotal.Value;
            details.OrderTotalOnRefund = orderTotal.Value;

            //discount history
            foreach (var disc in orderAppliedDiscounts)
            {
                if (!details.AppliedDiscounts.ContainsDiscount(disc))
                {
                    details.AppliedDiscounts.Add(disc);
                }
            }

            processPaymentRequest.OrderTotal = details.OrderTotal;
            processPaymentRequest.OrderTotalOnRefund = details.OrderTotal;
            //recurring or standard shopping cart?
            details.IsRecurringShoppingCart = details.Cart.IsRecurring();
            if (details.IsRecurringShoppingCart)
            {
                int recurringCycleLength;
                RecurringProductCyclePeriod recurringCyclePeriod;
                int recurringTotalCycles;
                var recurringCyclesError = details.Cart.GetRecurringCycleInfo(_localizationService,
                        out recurringCycleLength, out recurringCyclePeriod, out recurringTotalCycles);
                if (!string.IsNullOrEmpty(recurringCyclesError))
                {
                    throw new NopException(recurringCyclesError);
                }

                processPaymentRequest.RecurringCycleLength = recurringCycleLength;
                processPaymentRequest.RecurringCyclePeriod = recurringCyclePeriod;
                processPaymentRequest.RecurringTotalCycles = recurringTotalCycles;
            }

            return details;
        }

        /// <summary>
        /// Prepare details to place order based on the recurring payment.
        /// </summary>
        /// <param name="processPaymentRequest">Process payment request</param>
        /// <returns>Details</returns>
        protected virtual PlaceOrderContainter PrepareRecurringOrderDetails(ProcessPaymentRequest processPaymentRequest)
        {
            var details = new PlaceOrderContainter();
            details.IsRecurringShoppingCart = true;

            //Load initial order
            details.InitialOrder = _orderService.GetOrderById(processPaymentRequest.InitialOrderId);
            if (details.InitialOrder == null)
            {
                throw new ArgumentException("Initial order is not set for recurring payment");
            }

            processPaymentRequest.PaymentMethodSystemName = details.InitialOrder.PaymentMethodSystemName;

            //customer
            details.Customer = _customerService.GetCustomerById(processPaymentRequest.CustomerId);
            if (details.Customer == null)
            {
                throw new ArgumentException("Customer is not set");
            }

            //affiliate
            var affiliate = _affiliateService.GetAffiliateById(details.Customer.AffiliateId);
            if (affiliate != null && affiliate.Active && !affiliate.Deleted)
            {
                details.AffiliateId = affiliate.Id;
            }

            //check whether customer is guest
            if (details.Customer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed)
            {
                throw new NopException("Anonymous checkout is not allowed");
            }

            //customer currency
            details.CustomerCurrencyCode = details.InitialOrder.CustomerCurrencyCode;
            details.CustomerCurrencyRate = details.InitialOrder.CurrencyRate;

            //customer language
            details.CustomerLanguage = _languageService.GetLanguageById(details.InitialOrder.CustomerLanguageId);
            if (details.CustomerLanguage == null || !details.CustomerLanguage.Published)
            {
                details.CustomerLanguage = _workContext.WorkingLanguage;
            }

            //billing address
            if (details.InitialOrder.BillingAddress == null)
            {
                throw new NopException("Billing address is not available");
            }

            details.BillingAddress = (Address)details.InitialOrder.BillingAddress.Clone();
            if (details.BillingAddress.Country != null && !details.BillingAddress.Country.AllowsBilling)
            {
                throw new NopException(string.Format("Country '{0}' is not allowed for billing", details.BillingAddress.Country.Name));
            }

            //checkout attributes
            details.CheckoutAttributesXml = details.InitialOrder.CheckoutAttributesXml;
            details.CheckoutAttributeDescription = details.InitialOrder.CheckoutAttributeDescription;

            //tax display type
            details.CustomerTaxDisplayType = details.InitialOrder.CustomerTaxDisplayType;

            //sub total
            details.OrderSubTotalInclTax = details.InitialOrder.OrderSubtotalInclTax;
            details.OrderSubTotalExclTax = details.InitialOrder.OrderSubtotalExclTax;

            //shipping info
            if (details.InitialOrder.ShippingStatus != ShippingStatus.ShippingNotRequired)
            {
                details.PickUpInStore = details.InitialOrder.PickUpInStore;
                if (!details.PickUpInStore)
                {
                    if (details.InitialOrder.ShippingAddress == null)
                    {
                        throw new NopException("Shipping address is not available");
                    }

                    //clone shipping address
                    details.ShippingAddress = (Address)details.InitialOrder.ShippingAddress.Clone();
                    if (details.ShippingAddress.Country != null && !details.ShippingAddress.Country.AllowsShipping)
                    {
                        throw new NopException(string.Format("Country '{0}' is not allowed for shipping", details.ShippingAddress.Country.Name));
                    }
                }
                else
                    if (details.InitialOrder.PickupAddress != null)
                {
                    details.PickupAddress = (Address)details.InitialOrder.PickupAddress.Clone();
                }

                details.ShippingMethodName = details.InitialOrder.ShippingMethod;
                details.ShippingRateComputationMethodSystemName = details.InitialOrder.ShippingRateComputationMethodSystemName;
                details.ShippingStatus = ShippingStatus.NotYetShipped;
            }
            else
            {
                details.ShippingStatus = ShippingStatus.ShippingNotRequired;
            }

            //shipping total
            details.OrderShippingTotalInclTax = details.InitialOrder.OrderShippingInclTax;
            details.OrderShippingTotalExclTax = details.InitialOrder.OrderShippingExclTax;

            //payment total
            details.PaymentAdditionalFeeInclTax = details.InitialOrder.PaymentMethodAdditionalFeeInclTax;
            details.PaymentAdditionalFeeExclTax = details.InitialOrder.PaymentMethodAdditionalFeeExclTax;

            //tax total
            details.OrderTaxTotal = details.InitialOrder.OrderTax;

            //VAT number
            details.VatNumber = details.InitialOrder.VatNumber;

            //order total
            details.OrderDiscountAmount = details.InitialOrder.OrderDiscount;
            details.OrderTotal = details.InitialOrder.OrderTotal;
            processPaymentRequest.OrderTotal = details.OrderTotal;

            return details;
        }

        /// <summary>
        /// Save order and add order notes
        /// </summary>
        /// <param name="processPaymentRequest">Process payment request</param>
        /// <param name="processPaymentResult">Process payment result</param>
        /// <param name="details">Details</param>
        /// <returns>Order</returns>
        protected virtual Order SaveOrderDetails(ProcessPaymentRequest processPaymentRequest,
            ProcessPaymentResult processPaymentResult, PlaceOrderContainter details)
        {
            HttpCookie ordertax = new HttpCookie("OrderTax");
            HttpCookie transactionTag = new HttpCookie("TransTag");
            bool isTroveEnabled = Convert.ToBoolean(_httpContext.Session["TroveEnabledStore"]);
            bool IspaidThrghTrove = false;
            string paymntType = _httpContext.Session["SelectedPaymentType"].ToString();
            if (paymntType == "TrovePayment" && isTroveEnabled)
            {
                IspaidThrghTrove = true;
            }



            Card cc = new Card();
            TroveWalletAccountModel tender = new TroveWalletAccountModel();
            string PaymentType  = "";
            if (_httpContext.Session["CardDetails"] != null && !String.IsNullOrEmpty(_httpContext.Session["CardDetails"].ToString()))
            {
                if (_httpContext.Session["CustomerBYOCs"] != null)
                {
                    var cards = (List<Card>)_httpContext.Session["CustomerBYOCs"];
                    int CardNum = Convert.ToInt32(_httpContext.Session["CardDetails"]);
                    cc = cards.Where(x => x.CardId == CardNum).FirstOrDefault();
                    PaymentType = cc.Brand;
                }
            }

            if (_httpContext.Session["AccountDetails"] != null && !String.IsNullOrEmpty(_httpContext.Session["AccountDetails"].ToString()))
            {
                if (_httpContext.Session["CampusTenders"] != null)
                {
                    var tenders = (List<TroveWalletAccountModel>)_httpContext.Session["CampusTenders"];
                    string tenderId = Convert.ToString(_httpContext.Session["AccountDetails"]);
                    tender = tenders.Where(x => x.Tender == tenderId).FirstOrDefault();
                    processPaymentResult.AllowStoringCreditCardNumber = true;
                    PaymentType = "BitePay";
                }
            }

            var order = new Order
            {
                #region Prepare Order
                StoreId = processPaymentRequest.StoreId,
                OrderGuid = processPaymentRequest.OrderGuid,
                CustomerId = details.Customer.Id,
                CustomerLanguageId = details.CustomerLanguage.Id,
                CustomerTaxDisplayType = details.CustomerTaxDisplayType,
                CustomerIp = _webHelper.GetCurrentIpAddress(),
                OrderSubtotalInclTax = details.OrderSubTotalInclTax,
                OrderSubtotalExclTax = details.OrderSubTotalExclTax,
                OrderSubTotalDiscountInclTax = details.OrderSubTotalDiscountInclTax,
                OrderSubTotalDiscountExclTax = details.OrderSubTotalDiscountExclTax,
                OrderShippingInclTax = details.OrderShippingTotalInclTax,
                OrderShippingExclTax = details.OrderShippingTotalExclTax,
                PaymentMethodAdditionalFeeInclTax = details.PaymentAdditionalFeeInclTax,
                PaymentMethodAdditionalFeeExclTax = details.PaymentAdditionalFeeExclTax,
                TaxRates = details.TaxRates,
                OrderTax = details.OrderTaxTotal,
                OrderTotal = details.OrderTotal,
                OrderTotalOnRefund = details.OrderTotal,
                RefundedAmount = decimal.Zero,
                OrderDiscount = details.OrderDiscountAmount,
                CheckoutAttributeDescription = details.CheckoutAttributeDescription,
                CheckoutAttributesXml = details.CheckoutAttributesXml,
                CustomerCurrencyCode = details.CustomerCurrencyCode,
                CurrencyRate = details.CustomerCurrencyRate,
                AffiliateId = details.AffiliateId,
                OrderStatus = OrderStatus.Pending,
                AllowStoringCreditCardNumber = processPaymentResult.AllowStoringCreditCardNumber,
                CardType = processPaymentResult.AllowStoringCreditCardNumber ? (IspaidThrghTrove ? _encryptionService.EncryptText(PaymentType) : _encryptionService.EncryptText(processPaymentRequest.CreditCardType)) : string.Empty,
                CardName = processPaymentResult.AllowStoringCreditCardNumber ? (IspaidThrghTrove ? _encryptionService.EncryptText(cc.Name) : _encryptionService.EncryptText(processPaymentRequest.CreditCardName)) : string.Empty,
                CardNumber = processPaymentResult.AllowStoringCreditCardNumber ? (IspaidThrghTrove ? _encryptionService.EncryptText(cc.Last4) : _encryptionService.EncryptText(processPaymentRequest.CreditCardNumber)) : string.Empty,
                MaskedCreditCardNumber = !IspaidThrghTrove ? (IspaidThrghTrove ? cc.Last4.ToString() : _encryptionService.EncryptText(_paymentService.GetMaskedCreditCardNumber(processPaymentRequest.CreditCardNumber))) : null,
                CardCvv2 = !IspaidThrghTrove ? (processPaymentResult.AllowStoringCreditCardNumber ? _encryptionService.EncryptText(processPaymentRequest.CreditCardCvv2) : string.Empty) : string.Empty,
                CardExpirationMonth = !IspaidThrghTrove ? processPaymentResult.AllowStoringCreditCardNumber ? _encryptionService.EncryptText(processPaymentRequest.CreditCardExpireMonth.ToString()) : string.Empty : string.Empty,
                CardExpirationYear = !IspaidThrghTrove ? processPaymentResult.AllowStoringCreditCardNumber ? _encryptionService.EncryptText(processPaymentRequest.CreditCardExpireYear.ToString()) : string.Empty : string.Empty,
                PaymentMethodSystemName = !IspaidThrghTrove ? processPaymentRequest.PaymentMethodSystemName : "TroveDigitalPayments",
                AuthorizationTransactionId = !IspaidThrghTrove ? processPaymentResult.AuthorizationTransactionId : processPaymentResult.TroveRrn,
                AuthorizationTransactionCode = !IspaidThrghTrove ? processPaymentResult.AuthorizationTransactionCode : processPaymentResult.TroveResultCode,
                AuthorizationTransactionResult = !IspaidThrghTrove ? processPaymentResult.AuthorizationTransactionResult : processPaymentResult.TroveResultMsg,
                CaptureTransactionId = !IspaidThrghTrove ? processPaymentResult.CaptureTransactionId : processPaymentResult.TroveRrn,
                CaptureTransactionResult = !IspaidThrghTrove ? processPaymentResult.CaptureTransactionResult : processPaymentResult.TroveResultCode,
                SubscriptionTransactionId = !IspaidThrghTrove ? processPaymentResult.SubscriptionTransactionId : processPaymentResult.TroveRrn,
                PaymentStatus = processPaymentResult.NewPaymentStatus, //Where Payment Status is being Set
                PaidDateUtc = DateTime.UtcNow,
                BillingAddress = details.BillingAddress,
                ShippingAddress = details.ShippingAddress,
                ShippingStatus = details.ShippingStatus,
                ShippingMethod = details.ShippingMethodName,
                PickUpInStore = details.PickUpInStore,
                PickupAddress = details.PickupAddress,
                ShippingRateComputationMethodSystemName = details.ShippingRateComputationMethodSystemName,
                CustomValuesXml = processPaymentRequest.SerializeCustomValues(),
                VatNumber = details.VatNumber,
                CreatedOnUtc = DateTime.UtcNow,
                #endregion prepare order
            };
            _orderService.InsertOrder(order);

            //reward points history
            if (details.RedeemedRewardPointsAmount > decimal.Zero)
            {
                _rewardPointService.AddRewardPointsHistoryEntry(details.Customer, -details.RedeemedRewardPoints, order.StoreId,
                    string.Format(_localizationService.GetResource("RewardPoints.Message.RedeemedForOrder", order.CustomerLanguageId), order.Id),
                    order, details.RedeemedRewardPointsAmount);

                _customerService.UpdateCustomer(details.Customer);
            }

            //ordertax.Value = Convert.ToString(order.OrderTax);
            //transactionTag.Value = Convert.ToString(order.AuthorizationTransactionId);
            //this._httpContext.Response.Cookies.Add(ordertax);
            //this._httpContext.Response.Cookies.Add(transactionTag);

            return order;
        }

        /// <summary>
        /// Send "order placed" notifications and save order notes
        /// </summary>
        /// <param name="order">Order</param>
        protected virtual void SendNotificationsAndSaveNotes(Order order)
        {
            //notes, messages
            if (_workContext.OriginalCustomerIfImpersonated != null)
            {
                //this order is placed by a store administrator impersonating a customer
                order.OrderNotes.Add(new OrderNote
                {
                    Note = string.Format("Order placed by a store owner ('{0}'. ID = {1}) impersonating the customer.",
                        _workContext.OriginalCustomerIfImpersonated.Email, _workContext.OriginalCustomerIfImpersonated.Id),
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow
                });
            }
            else
            {
                order.OrderNotes.Add(new OrderNote
                {
                    Note = "Order placed",
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow
                });
            }

            _orderService.UpdateOrder(order);

            foreach (var orderItem in order.OrderItems.ToList())
            {
                _orderItemRepository.Detach(orderItem);
            }

            //send email notifications
            var orderPlacedStoreOwnerNotificationQueuedEmailId = _workflowMessageService.SendOrderPlacedorDonationStoreOwnerNotification(order.Id, _localizationSettings.DefaultAdminLanguageId);

            if (!String.IsNullOrEmpty(orderPlacedStoreOwnerNotificationQueuedEmailId))
            {
                order.OrderNotes.Add(new OrderNote
                {
                    Note = string.Format("\"Order placed\" email (to store owner) has been queued. Queued email identifier: {0}.", orderPlacedStoreOwnerNotificationQueuedEmailId),
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow
                });
                _orderService.UpdateOrder(order);
            }

            var orderPlacedAttachmentFilePath = _orderSettings.AttachPdfInvoiceToOrderPlacedEmail ?
                _pdfService.PrintOrderToPdf(order) : null;
            var orderPlacedAttachmentFileName = _orderSettings.AttachPdfInvoiceToOrderPlacedEmail ?
                "order.pdf" : null;
            var orderPlacedCustomerNotificationQueuedEmailId = _workflowMessageService
                .SendOrderPlacedCustomerOrDonationCustomerNotification(order, order.CustomerLanguageId, orderPlacedAttachmentFilePath, orderPlacedAttachmentFileName);

            if (orderPlacedCustomerNotificationQueuedEmailId.Count > 0)
            {
                foreach (int id in orderPlacedCustomerNotificationQueuedEmailId)
                {
                    order.OrderNotes.Add(new OrderNote
                    {
                        Note = string.Format("\"Order placed\" email (to customer) has been queued. Queued email identifier: {0}.", orderPlacedCustomerNotificationQueuedEmailId),
                        DisplayToCustomer = false,
                        CreatedOnUtc = DateTime.UtcNow
                    });

                }
                _orderService.UpdateOrder(order);
            }


            //send for corporate notifications donations if needed.
            int donationOrderPlacedCorporateNotificationQueuedEmailIds =
                _workflowMessageService.SendDonationPlacedForCorporateNotification(order, _localizationSettings.DefaultAdminLanguageId);
            if (donationOrderPlacedCorporateNotificationQueuedEmailIds > 0)
            {

                order.OrderNotes.Add(
                    new OrderNote
                    {
                        Note =
                            string.Format(
                                "\"Corporate Notification for Donation\" email has been queued. Queued email identifier: {0}.",
                                donationOrderPlacedCorporateNotificationQueuedEmailIds),
                        DisplayToCustomer = false,
                        CreatedOnUtc = DateTime.UtcNow
                    });



                _orderService.UpdateOrder(order);
            }


            #region NU-17

            //send on behalf for donations if needed.
            List<int> orderPlacedOnBehalfNotificationQueuedEmailIds =
                _workflowMessageService.SendDonationOrderPlacedOnBehalfOfNotifications(order, _localizationSettings.DefaultAdminLanguageId);
            if (orderPlacedOnBehalfNotificationQueuedEmailIds.Count > 0)
            {
                foreach (int id in orderPlacedOnBehalfNotificationQueuedEmailIds)
                {
                    order.OrderNotes.Add(
                        new OrderNote
                        {
                            Note =
                                string.Format(
                                    "\"Donation on Behalf\" email has been queued. Queued email identifier: {0}.",
                                    id),
                            DisplayToCustomer = false,
                            CreatedOnUtc = DateTime.UtcNow
                        });


                }
                _orderService.UpdateOrder(order);
            }


            bool sendGiftNotification = _workContext.CurrentCustomer.GetAttribute<bool>("SendGiftNotification", order.StoreId);
            //Ryan Markle:  If user wants a notification sent to the order recipient
            if (sendGiftNotification)
            {

                int orderGiftRecipientMessageId = _workflowMessageService.SendCustomerOrderGiftNotification(order, _localizationSettings.DefaultAdminLanguageId);

                if (orderGiftRecipientMessageId > 0)
                {
                    order.OrderNotes.Add(
                        new OrderNote
                        {
                            Note =
                                string.Format(
                                    "\"Recipient Gift Notification\" email has been queued. Queued email identifier: {0}.",
                                    orderGiftRecipientMessageId),
                            DisplayToCustomer = false,
                            CreatedOnUtc = DateTime.UtcNow
                        });


                    _orderService.UpdateOrder(order);
                }

            }

            #endregion




            var vendors = GetVendorsInOrder(order);
            foreach (var vendor in vendors)
            {
                var orderPlacedVendorNotificationQueuedEmailId = _workflowMessageService.SendOrderPlacedVendorNotification(order, vendor, order.CustomerLanguageId);
                if (orderPlacedVendorNotificationQueuedEmailId > 0)
                {
                    order.OrderNotes.Add(new OrderNote
                    {
                        Note = string.Format("\"Order placed\" email (to vendor) has been queued. Queued email identifier: {0}.", orderPlacedVendorNotificationQueuedEmailId),
                        DisplayToCustomer = false,
                        CreatedOnUtc = DateTime.UtcNow
                    });
                    _orderService.UpdateOrder(order);
                }
            }
        }
        /// <summary>
        /// Award (earn) reward points (for placing a new order)
        /// </summary>
        /// <param name="order">Order</param>
        protected virtual void AwardRewardPoints(Order order)
        {
            int points = _orderTotalCalculationService.CalculateRewardPoints(order.Customer, order.OrderTotal);
            if (points == 0)
            {
                return;
            }

            //Ensure that reward points were not added (earned) before. We should not add reward points if they were already earned for this order
            if (order.RewardPointsWereAdded)
            {
                return;
            }

            //add reward points
            _rewardPointService.AddRewardPointsHistoryEntry(order.Customer, points, order.StoreId,
                string.Format(_localizationService.GetResource("RewardPoints.Message.EarnedForOrder"), order.Id));
            order.RewardPointsWereAdded = true;
            _orderService.UpdateOrder(order);
        }

        /// <summary>
        /// Reduce (cancel) reward points (previously awarded for placing an order)
        /// </summary>
        /// <param name="order">Order</param>
        protected virtual void ReduceRewardPoints(Order order)
        {
            int points = _orderTotalCalculationService.CalculateRewardPoints(order.Customer, order.OrderTotal);
            if (points == 0)
            {
                return;
            }

            //ensure that reward points were already earned for this order before
            if (!order.RewardPointsWereAdded)
            {
                return;
            }

            //reduce reward points
            _rewardPointService.AddRewardPointsHistoryEntry(order.Customer, -points, order.StoreId,
                string.Format(_localizationService.GetResource("RewardPoints.Message.ReducedForOrder"), order.Id));
            _orderService.UpdateOrder(order);
        }

        /// <summary>
        /// Return back redeemded reward points to a customer (spent when placing an order)
        /// </summary>
        /// <param name="order">Order</param>
        protected virtual void ReturnBackRedeemedRewardPoints(Order order)
        {
            //were some points redeemed when placing an order?
            if (order.RedeemedRewardPointsEntry == null)
            {
                return;
            }

            //return back
            _rewardPointService.AddRewardPointsHistoryEntry(order.Customer, -order.RedeemedRewardPointsEntry.Points, order.StoreId,
                string.Format(_localizationService.GetResource("RewardPoints.Message.ReturnedForOrder"), order.Id));
            _orderService.UpdateOrder(order);
        }


        /// <summary>
        /// Set IsActivated value for purchase gift cards for particular order
        /// </summary>
        /// <param name="order">Order</param>
        /// <param name="activate">A value indicating whether to activate gift cards; true - activate, false - deactivate</param>
        protected virtual void SetActivatedValueForPurchasedGiftCards(Order order, bool activate)
        {
            var giftCards = _giftCardService.GetAllGiftCards(purchasedWithOrderId: order.Id,
                isGiftCardActivated: !activate);
            foreach (var gc in giftCards)
            {
                if (activate)
                {
                    //activate
                    bool isRecipientNotified = gc.IsRecipientNotified;
                    if (gc.GiftCardType == GiftCardType.Virtual)
                    {
                        //send email for virtual gift card
                        if (!String.IsNullOrEmpty(gc.RecipientEmail) &&
                            !String.IsNullOrEmpty(gc.SenderEmail))
                        {
                            var customerLang = _languageService.GetLanguageById(order.CustomerLanguageId);
                            if (customerLang == null)
                            {
                                customerLang = _languageService.GetAllLanguages().FirstOrDefault();
                            }

                            if (customerLang == null)
                            {
                                throw new Exception("No languages could be loaded");
                            }

                            int queuedEmailId = _workflowMessageService.SendGiftCardNotification(gc, customerLang.Id);
                            if (queuedEmailId > 0)
                            {
                                isRecipientNotified = true;
                            }
                        }
                    }
                    gc.IsGiftCardActivated = true;
                    gc.IsRecipientNotified = isRecipientNotified;
                    _giftCardService.UpdateGiftCard(gc);
                }
                else
                {
                    //deactivate
                    gc.IsGiftCardActivated = false;
                    _giftCardService.UpdateGiftCard(gc);
                }
            }
        }

        /// <summary>
        /// Sets an order status
        /// </summary>
        /// <param name="order">Order</param>
        /// <param name="os">New order status</param>
        /// <param name="notifyCustomer">True to notify customer</param>
        protected virtual void SetOrderStatus(Order order, OrderStatus os, bool notifyCustomer)
        {
            if (order == null)
            {
                throw new ArgumentNullException("order");
            }

            OrderStatus prevOrderStatus = order.OrderStatus;
            if (prevOrderStatus == os)
            {
                return;
            }

            //set and save new order status
            order.OrderStatusId = (int)os;
            _orderService.UpdateOrder(order);

            //order notes, notifications
            order.OrderNotes.Add(new OrderNote
            {
                Note = string.Format("Order status has been changed to {0}", os.ToString()),
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow
            });
            _orderService.UpdateOrder(order);


            if (prevOrderStatus != OrderStatus.Complete &&
                os == OrderStatus.Complete
                && notifyCustomer)
            {
                //notification
                var orderCompletedAttachmentFilePath = _orderSettings.AttachPdfInvoiceToOrderCompletedEmail ?
                    _pdfService.PrintOrderToPdf(order) : null;
                var orderCompletedAttachmentFileName = _orderSettings.AttachPdfInvoiceToOrderCompletedEmail ?
                    "order.pdf" : null;
                int orderCompletedCustomerNotificationQueuedEmailId = _workflowMessageService
                    .SendOrderCompletedCustomerNotification(order, order.CustomerLanguageId, orderCompletedAttachmentFilePath,
                    orderCompletedAttachmentFileName);
                if (orderCompletedCustomerNotificationQueuedEmailId > 0)
                {
                    order.OrderNotes.Add(new OrderNote
                    {
                        Note = string.Format("\"Order completed\" email (to customer) has been queued. Queued email identifier: {0}.", orderCompletedCustomerNotificationQueuedEmailId),
                        DisplayToCustomer = false,
                        CreatedOnUtc = DateTime.UtcNow
                    });
                    _orderService.UpdateOrder(order);
                }
            }

            if (prevOrderStatus != OrderStatus.Cancelled &&
                os == OrderStatus.Cancelled
                && notifyCustomer)
            {
                //notification
                int orderCancelledCustomerNotificationQueuedEmailId = _workflowMessageService.SendOrderCancelledCustomerNotification(order, order.CustomerLanguageId);
                if (orderCancelledCustomerNotificationQueuedEmailId > 0)
                {
                    order.OrderNotes.Add(new OrderNote
                    {
                        Note = string.Format("\"Order cancelled\" email (to customer) has been queued. Queued email identifier: {0}.", orderCancelledCustomerNotificationQueuedEmailId),
                        DisplayToCustomer = false,
                        CreatedOnUtc = DateTime.UtcNow
                    });
                    _orderService.UpdateOrder(order);
                }
            }

            //reward points
            if (_rewardPointsSettings.PointsForPurchases_Awarded == order.OrderStatus)
            {
                AwardRewardPoints(order);
            }
            if (_rewardPointsSettings.PointsForPurchases_Canceled == order.OrderStatus)
            {
                ReduceRewardPoints(order);
            }

            //gift cards activation
            if (_orderSettings.GiftCards_Activated_OrderStatusId > 0 &&
               _orderSettings.GiftCards_Activated_OrderStatusId == (int)order.OrderStatus)
            {
                SetActivatedValueForPurchasedGiftCards(order, true);
            }

            //gift cards deactivation
            if (_orderSettings.GiftCards_Deactivated_OrderStatusId > 0 &&
               _orderSettings.GiftCards_Deactivated_OrderStatusId == (int)order.OrderStatus)
            {
                SetActivatedValueForPurchasedGiftCards(order, false);
            }


        }

        /// <summary>
        /// Process order paid status
        /// </summary>
        /// <param name="order">Order</param>
        protected virtual void ProcessOrderPaid(Order order)
        {
            if (order == null)
            {
                throw new ArgumentNullException("order");
            }

            //raise event
            _eventPublisher.Publish(new OrderPaidEvent(order));

            //order paid email notification
            if (order.OrderTotal != decimal.Zero)
            {
                //we should not send it for free ($0 total) orders?
                //remove this "if" statement if you want to send it in this case

                var orderPaidAttachmentFilePath = _orderSettings.AttachPdfInvoiceToOrderPaidEmail ?
                    _pdfService.PrintOrderToPdf(order) : null;
                var orderPaidAttachmentFileName = _orderSettings.AttachPdfInvoiceToOrderPaidEmail ?
                    "order.pdf" : null;
                _workflowMessageService.SendOrderPaidCustomerNotification(order, order.CustomerLanguageId,
                    orderPaidAttachmentFilePath, orderPaidAttachmentFileName);

                _workflowMessageService.SendOrderPaidStoreOwnerNotification(order, _localizationSettings.DefaultAdminLanguageId);
                var vendors = GetVendorsInOrder(order);
                foreach (var vendor in vendors)
                {
                    _workflowMessageService.SendOrderPaidVendorNotification(order, vendor, _localizationSettings.DefaultAdminLanguageId);
                }
                //TODO add "order paid email sent" order note
            }

            //customer roles with "purchased with product" specified
            ProcessCustomerRolesWithPurchasedProductSpecified(order, true);
        }

        /// <summary>
        /// Process customer roles with "Purchased with Product" property configured
        /// </summary>
        /// <param name="order">Order</param>
        /// <param name="add">A value indicating whether to add configured customer role; true - add, false - remove</param>
        protected virtual void ProcessCustomerRolesWithPurchasedProductSpecified(Order order, bool add)
        {
            if (order == null)
            {
                throw new ArgumentNullException("order");
            }

            //purchased product identifiers
            var purchasedProductIds = new List<int>();
            foreach (var orderItem in order.OrderItems)
            {
                //standard items
                purchasedProductIds.Add(orderItem.ProductId);

                //bundled (associated) products
                var attributeValues = _productAttributeParser.ParseProductAttributeValues(orderItem.AttributesXml);
                foreach (var attributeValue in attributeValues)
                {
                    if (attributeValue.AttributeValueType == AttributeValueType.AssociatedToProduct)
                    {
                        purchasedProductIds.Add(attributeValue.AssociatedProductId);
                    }
                }
            }

            //list of customer roles
            var customerRoles = _customerService
                .GetAllCustomerRoles(true)
                .Where(cr => purchasedProductIds.Contains(cr.PurchasedWithProductId))
                .ToList();

            if (customerRoles.Any())
            {
                var customer = order.Customer;
                foreach (var customerRole in customerRoles)
                {
                    if (customer.CustomerRoles.Count(cr => cr.Id == customerRole.Id) == 0)
                    {
                        //not in the list yet
                        if (add)
                        {
                            //add
                            customer.CustomerRoles.Add(customerRole);
                        }
                    }
                    else
                    {
                        //already in the list
                        if (!add)
                        {
                            //remove
                            customer.CustomerRoles.Remove(customerRole);
                        }
                    }
                }
                _customerService.UpdateCustomer(customer);
            }
        }

        /// <summary>
        /// Get a list of vendors in order (order items)
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>Vendors</returns>
        protected virtual IList<Vendor> GetVendorsInOrder(Order order)
        {
            var vendors = new List<Vendor>();
            foreach (var orderItem in order.OrderItems)
            {
                var vendorId = orderItem.Product.VendorId;
                //find existing
                var vendor = vendors.FirstOrDefault(v => v.Id == vendorId);
                if (vendor == null)
                {
                    //not found. load by Id
                    vendor = _vendorService.GetVendorById(vendorId);
                    if (vendor != null && !vendor.Deleted && vendor.Active)
                    {
                        vendors.Add(vendor);
                    }
                }
            }

            return vendors;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Checks order status
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>Validated order</returns>
        public virtual void CheckOrderStatus(Order order)
        {
            if (order == null)
            {
                throw new ArgumentNullException("order");
            }

            if (order.PaymentStatus == PaymentStatus.Paid && !order.PaidDateUtc.HasValue)
            {
                //ensure that paid date is set
                order.PaidDateUtc = DateTime.UtcNow;
                _orderService.UpdateOrder(order);
            }

            if (order.OrderStatus == OrderStatus.Pending)
            {
                if (order.PaymentStatus == PaymentStatus.Authorized ||
                    order.PaymentStatus == PaymentStatus.Paid)
                {
                    SetOrderStatus(order, OrderStatus.Processing, false);
                }
            }

            if (order.OrderStatus == OrderStatus.Pending)
            {
                if (order.ShippingStatus == ShippingStatus.PartiallyShipped ||
                    order.ShippingStatus == ShippingStatus.Shipped ||
                    order.ShippingStatus == ShippingStatus.Delivered)
                {
                    SetOrderStatus(order, OrderStatus.Processing, false);
                }
            }

            if (order.OrderStatus != OrderStatus.Cancelled &&
                order.OrderStatus != OrderStatus.Complete)
            {
                if (order.PaymentStatus == PaymentStatus.Paid)
                {
                    if (order.ShippingStatus == ShippingStatus.ShippingNotRequired || order.ShippingStatus == ShippingStatus.Delivered)
                    {
                        SetOrderStatus(order, OrderStatus.Complete, true);
                    }
                }
            }
        }

        /// <summary>
        /// Places an order
        /// </summary>
        /// <param name="processPaymentRequest">Process payment request</param>
        /// <returns>Place order result</returns>
        public virtual PlaceOrderResult PlaceOrder(ProcessPaymentRequest processPaymentRequest)
        {
            bool isTroveEnabled = false;
            ProcessPaymentResult processPaymentResult = null;

            isTroveEnabled = Convert.ToBoolean(_httpContext.Session["TroveEnabledStore"]);
            if (!isTroveEnabled)
            {
                if (processPaymentRequest == null)
                {
                    throw new ArgumentNullException("processPaymentRequest");
                }
            }
            else
            {

            }

            var result = new PlaceOrderResult();
            try
            {
                if (processPaymentRequest.OrderGuid == Guid.Empty)
                {
                    processPaymentRequest.OrderGuid = Guid.NewGuid();
                }

                //prepare order details
                var details = PreparePlaceOrderDetails(processPaymentRequest);

                #region Payment workflow

                //process payment

                //skip payment workflow if order total equals zero
                var skipPaymentWorkflow = details.OrderTotal == decimal.Zero;
                if ((!skipPaymentWorkflow && !isTroveEnabled) || processPaymentRequest.PaymentMethodSystemName == "BitShift.Payments.FirstData")
                {
                    var paymentMethod = _paymentService.LoadPaymentMethodBySystemName(processPaymentRequest.PaymentMethodSystemName);
                    if (paymentMethod == null)
                    {
                        throw new NopException("Payment method couldn't be loaded");
                    }

                    //ensure that payment method is active
                    if (!paymentMethod.IsPaymentMethodActive(_paymentSettings))
                    {
                        throw new NopException("Payment method is not active");
                    }

                    if (details.IsRecurringShoppingCart)
                    {
                        //recurring cart
                        switch (_paymentService.GetRecurringPaymentType(processPaymentRequest.PaymentMethodSystemName))
                        {
                            case RecurringPaymentType.NotSupported:
                                throw new NopException("Recurring payments are not supported by selected payment method");
                            case RecurringPaymentType.Manual:
                            case RecurringPaymentType.Automatic:
                                processPaymentResult = _paymentService.ProcessRecurringPayment(processPaymentRequest);
                                break;
                            default:
                                throw new NopException("Not supported recurring payment type");
                        }
                    }
                    else
                    {
                        //standard cart
                        processPaymentResult = _paymentService.ProcessPayment(processPaymentRequest);
                    }
                }
                else if (isTroveEnabled
                                    && (!String.IsNullOrEmpty(_httpContext.Session["SelectedPaymentType"].ToString()))
                                                && _httpContext.Session["SelectedPaymentType"].ToString() == "TrovePayment")
                {
                    //payment is not required
                    processPaymentResult = _httpContext.Session["OrderTrovePaymentInfo"] as ProcessPaymentResult;
                    processPaymentResult.NewPaymentStatus = PaymentStatus.Paid;
                }
                else
                {
                    processPaymentResult = new ProcessPaymentResult { NewPaymentStatus = PaymentStatus.Paid };
                }

                if (processPaymentResult == null)
                {
                    throw new NopException("processPaymentResult is not available");
                }

                #endregion

                if (processPaymentResult.Success || processPaymentResult.TroveSuccess)
                {
                    #region Save order details
                    var order = SaveOrderDetails(processPaymentRequest, processPaymentResult, details);

                    result.PlacedOrder = order;
                    //HttpContext.Current.Session["AssociatedProductPerShoppingCartItem"] = null;

                    var shippingCollections = _httpContext.Session["shippingBucket"] == null ? null : (Dictionary<string, decimal>)_httpContext.Session["shippingBucket"];
                    int count = 0;
                    //move shopping cart items to order items

                    foreach (var sc in details.Cart)
                    {
                        if (sc.Product.IsBundleProduct)
                        {
                            List<ShoppingCartBundleProductItem> scbpList = _shoppingCartService.GetAssociatedProductsPerShoppingCartItem(sc.Id).ToList();
                            foreach (ShoppingCartBundleProductItem scb in scbpList)
                            {
                                Product bundlePro = _productService.GetProductById(scb.AssociatedProductId);
                                //prices
                                decimal taxRate;
                                List<Discount> scDiscounts;
                                decimal discountAmount;
                                ShoppingCartItem bundleSCi = new ShoppingCartItem();
                                bundleSCi = sc;
                                bundleSCi.ProductId = bundlePro.Id;
                                bundleSCi.Product = bundlePro;
                                bundleSCi.AttributesXml = bundleSCi.AttributesXml;
                                bundleSCi.Product.Price = scb.Price;
                                bundleSCi.Product.IsBundleProduct = true;
                                var scUnitPrice = _priceCalculationService.GetUnitPrice(bundleSCi);
                                var scSubTotal = _priceCalculationService.GetSubTotal(bundleSCi, true, out discountAmount, out scDiscounts);
                                var scUnitPriceInclTax = _taxService.GetProductPrice(bundleSCi.Product, scUnitPrice, true, details.Customer, true, out taxRate);
                                var scUnitPriceExclTax = _taxService.GetProductPrice(bundleSCi.Product, scUnitPrice, false, details.Customer, true, out taxRate);
                                var scSubTotalInclTax = _taxService.GetProductPrice(bundleSCi.Product, scSubTotal, true, details.Customer, true, out taxRate);
                                var scSubTotalExclTax = _taxService.GetProductPrice(bundleSCi.Product, scSubTotal, false, details.Customer, true, out taxRate);
                                var discountAmountInclTax = _taxService.GetProductPrice(bundleSCi.Product, discountAmount, true, details.Customer, true, out taxRate);
                                var discountAmountExclTax = _taxService.GetProductPrice(bundleSCi.Product, discountAmount, false, details.Customer, true, out taxRate);
                                foreach (var disc in scDiscounts)
                                {
                                    if (!details.AppliedDiscounts.ContainsDiscount(disc))
                                    {
                                        details.AppliedDiscounts.Add(disc);
                                    }
                                }

                                var storeCommission = scSubTotal * (bundleSCi.Product.StoreCommission / 100);  /// NU-15

                                //attributes
                                var attributeDescription = _productAttributeFormatter.FormatAttributes(bundleSCi.Product, bundleSCi.AttributesXml, details.Customer);

                                var itemWeight = _shippingService.GetShoppingCartItemWeight(bundleSCi);

                                //save order item
                                var orderItem = new OrderItem
                                {
                                    OrderItemGuid = Guid.NewGuid(),
                                    Order = order,
                                    IsReservation = bundleSCi.Product.IsReservation,
                                    ProductId = bundleSCi.Product.Id,
                                    UnitPriceInclTax = scUnitPriceInclTax,
                                    UnitPriceExclTax = scUnitPriceExclTax,
                                    PriceInclTax = scSubTotalInclTax,
                                    PriceExclTax = scSubTotalExclTax,
                                    PartialRefundOrderAmount = scSubTotalExclTax,
                                    OriginalProductCost = _priceCalculationService.GetProductCost(bundleSCi.Product, bundleSCi.AttributesXml),
                                    AttributeDescription = attributeDescription,
                                    AttributesXml = bundleSCi.AttributesXml,
                                    Quantity = bundleSCi.Quantity,
                                    DiscountAmountInclTax = discountAmountInclTax,
                                    DiscountAmountExclTax = discountAmountExclTax,
                                    DownloadCount = 0,
                                    IsDownloadActivated = false,
                                    LicenseDownloadId = 0,
                                    ItemWeight = itemWeight,
                                    RentalStartDateUtc = bundleSCi.RentalStartDateUtc,
                                    RentalEndDateUtc = bundleSCi.RentalEndDateUtc,
                                    RequestedFulfillmentDateTime = bundleSCi.RequestedFullfilmentDateTime,/// NU-13
                                    SelectedFulfillmentWarehouseId = bundleSCi.SelectedWarehouseId,/// NU-13           
                                    SelectedShippingMethodName = bundleSCi.SelectedShippingMethodName, /// NU-13                                
                                    SelectedShippingRateComputationMethodSystemName = bundleSCi.SelectedShippingRateComputationMethodSystemName, //SODMYWAY-
                                    StoreCommission = storeCommission,  // NU-72
                                    DeliveryAmountExclTax = bundleSCi.Product.AdditionalShippingCharge, //NU-90
                                    DeliveryPickupFee = (shippingCollections == null || !shippingCollections.Any()) ? 0 : shippingCollections.Where(x => x.Key.Contains("Delivery-" + count + sc.Product.Id) || x.Key.Contains("In-Store Pickup-" + count + sc.Product.Id)).FirstOrDefault().Value,
                                    ShippingFee = (shippingCollections == null || !shippingCollections.Any()) ? 0 : shippingCollections.Where(x => x.Key.Contains("UPS-" + count + sc.Product.Id)).FirstOrDefault().Value,
                                    IsBundleProduct = bundleSCi.Product.IsBundleProduct ? true : false,
                                };

                                if (shippingCollections != null)
                                {
                                    if (shippingCollections.Keys.Contains("Delivery-" + count + bundleSCi.Product.Id) || shippingCollections.Keys.Contains("In-Store Pickup-" + count + bundleSCi.Product.Id))
                                    {
                                        orderItem.IsDeliveryPickUp = true;
                                    }
                                    if (shippingCollections.Keys.Contains("UPS-" + count + bundleSCi.Product.Id))
                                    {
                                        orderItem.IsShipping = true;
                                    }
                                }

                                #region Vanguard Shipping
                                if (bundleSCi.IsTieredShippingEnabled)
                                {
                                    order.ShippingMethod = "Flat Rate Shipping";
                                    order.ShippingRateComputationMethodSystemName = "Tiered Shipping";
                                    orderItem.IsShipping = true;
                                    orderItem.ShippingFee = bundleSCi.FlatShipping;
                                    orderItem.SelectedShippingMethodName = "Flat Rate Shipping";
                                    orderItem.SelectedShippingRateComputationMethodSystemName = "Tiered Shipping";
                                }
                                if (bundleSCi.IsContractShippingEnabled)
                                {
                                    order.ShippingMethod = "Contract Shipping";
                                    order.ShippingRateComputationMethodSystemName = "Tiered Shipping";
                                    order.RCNumber = bundleSCi.RCNumber;
                                    orderItem.IsShipping = true;
                                    orderItem.ShippingFee = 0;
                                    orderItem.SelectedShippingMethodName = "Contract Shipping";
                                    orderItem.SelectedShippingRateComputationMethodSystemName = "Tiered Shipping";
                                    orderItem.RCNumber = bundleSCi.RCNumber;
                                }
                                if (bundleSCi.IsInterOfficeDeliveryEnabled)
                                {
                                    order.ShippingMethod = "MailStop Shipping";
                                    order.ShippingRateComputationMethodSystemName = "Tiered Shipping";
                                    order.MailStopAddress = bundleSCi.MailStopAddress;
                                    orderItem.IsShipping = true;
                                    orderItem.ShippingFee = 0;
                                    orderItem.SelectedShippingMethodName = "MailStop Shipping";
                                    orderItem.SelectedShippingRateComputationMethodSystemName = "Tiered Shipping";
                                    orderItem.MailStopAddress = bundleSCi.MailStopAddress;
                                }
                                #endregion Vanguard Shipping

                                #region  NU-13
                                if (orderItem.SelectedFulfillmentWarehouseId != null)
                                {
                                    orderItem.SelectedFulfillmentWarehouse = _shippingService.GetWarehouseById(orderItem.SelectedFulfillmentWarehouseId.GetValueOrDefault(-1));
                                }
                                if (!bundleSCi.Product.IsPickupEnabled && !bundleSCi.Product.IsShipEnabled)
                                {
                                    if (bundleSCi.Product.IsMealPlan || bundleSCi.Product.IsDonation || bundleSCi.Product.IsDownload) /// NU-16
                                    {
                                        orderItem.FulfillmentDateTime = DateTime.Now;
                                    }
                                }
                                else
                                {
                                    count++;
                                }
                                #endregion


                                order.OrderItems.Add(orderItem);
                                _orderService.UpdateOrder(order);

                                #region Update ShoppingCartBundleProductItem with orderId and orderItemid
                                scb.OrderId = order.Id;
                                scb.OrderItemId = orderItem.Id;
                                _shoppingCartService.UpdateShoppingCartBundleProductItem(scb);
                                #endregion

                                #region prepare to insert list of reserved products

                                if (bundleSCi.Product.IsReservation)
                                {
                                    ReservedProduct reservedProduct = new ReservedProduct();
                                    reservedProduct.ProductId = bundleSCi.ProductId;
                                    reservedProduct.OrderItemId = orderItem.Id;
                                    reservedProduct.ReservationDate = bundleSCi.ReservationDate;
                                    reservedProduct.ReservedTimeSlot = bundleSCi.ReservedTimeSlot;
                                    reservedProduct.ReservedUnits = bundleSCi.Quantity;
                                    _orderService.InsertReservedProduct(reservedProduct);
                                }
                                #endregion

                                #region Section to Apply the JE BusinessRules

                                var orderItemDetails = _orderService.GetOrderItemByGuid(orderItem.OrderItemGuid);
                                var orderDetails = new OrderDetails
                                {
                                    OrderId = order.Id,
                                    OrderItemId = orderItemDetails.Id,
                                    StoreId = order.StoreId,
                                    IsMealPlan = bundleSCi.Product.IsMealPlan,
                                    IsDonation = bundleSCi.Product.IsDonation,
                                    IsGift = bundleSCi.Product.IsGiftCard,
                                    ProductName = bundleSCi.Product.Name
                                };
                                ApplyJEAddressBusinessRules(orderItem, order, orderDetails);
                                _orderService.InsertJEBusinessLogicOrderDetails(orderDetails);

                                var itemsMapping = new ProductGlCodeMapping();
                                ApplyUnfulfilledProductsJeGLCodeBusinessRules(orderItem, order, itemsMapping);
                                #endregion


                                //"Auto" fullfill.  Not running the fulfill method for some reason.
                                //if (!sc.Product.IsPickupEnabled && !sc.Product.IsShipEnabled)
                                //{
                                if (bundleSCi.Product.IsMealPlan || bundleSCi.Product.IsDonation || bundleSCi.Product.IsDownload) /// NU-16
                                {
                                    var itemDetails = _orderService.GetOrderItemByGuid(orderItem.OrderItemGuid);
                                    InsertIntoSAPSalesJournalOrderItem(order, itemDetails, 2);
                                }
                                //}

                                if (!bundleSCi.Product.IsMealPlan && !bundleSCi.Product.IsDonation && !bundleSCi.Product.IsDownload)
                                {
                                    var itemDetails = _orderService.GetOrderItemByGuid(orderItem.OrderItemGuid);
                                    if (bundleSCi.Product.IsBundleProduct)
                                    {
                                        var associatedProductDict = (Dictionary<string, string>)HttpContext.Current.Session["AssociatedProductPerShoppingCartItem"];
                                        foreach (KeyValuePair<string, string> entry in associatedProductDict)
                                        {
                                            var scService = EngineContext.Current.Resolve<IShoppingCartService>();
                                            string[] dictKey = entry.Key.Split('_');
                                            string[] dictValue = entry.Value.Split('_');
                                            InsertIntoSAPSalesJournalOrderItem(order, itemDetails, 1, Convert.ToInt32(dictKey[2]), bundleSCi.Product.IsBundleProduct);

                                        }
                                    }
                                    else
                                    {
                                        InsertIntoSAPSalesJournalOrderItem(order, itemDetails, 1, bundleSCi.Product.Id, bundleSCi.Product.IsBundleProduct);
                                    }
                                }

                                #region NU-63
                                //_SAPService.CreateEntry(orderItem, 1);
                                //if (sc.Product.IsMealPlan || sc.Product.IsDonation || sc.Product.IsDownload)
                                //{
                                //    _SAPService.CreateEntry(orderItem, 2);
                                //}
                                #endregion

                                //gift cards
                                if (bundleSCi.Product.IsGiftCard)
                                {
                                    string giftCardRecipientName;
                                    string giftCardRecipientEmail;
                                    string giftCardSenderName;
                                    string giftCardSenderEmail;
                                    string giftCardMessage;
                                    _productAttributeParser.GetGiftCardAttribute(bundleSCi.AttributesXml, out giftCardRecipientName,
                                        out giftCardRecipientEmail, out giftCardSenderName, out giftCardSenderEmail, out giftCardMessage);

                                    for (var i = 0; i < bundleSCi.Quantity; i++)
                                    {
                                        _giftCardService.InsertGiftCard(new GiftCard
                                        {
                                            GiftCardType = bundleSCi.Product.GiftCardType,
                                            PurchasedWithOrderItem = orderItem,
                                            Amount = bundleSCi.Product.OverriddenGiftCardAmount.HasValue ? bundleSCi.Product.OverriddenGiftCardAmount.Value : scUnitPriceExclTax,
                                            IsGiftCardActivated = false,
                                            GiftCardCouponCode = _giftCardService.GenerateGiftCardCode(),
                                            RecipientName = giftCardRecipientName,
                                            RecipientEmail = giftCardRecipientEmail,
                                            SenderName = giftCardSenderName,
                                            SenderEmail = giftCardSenderEmail,
                                            Message = giftCardMessage,
                                            IsRecipientNotified = false,
                                            CreatedOnUtc = DateTime.UtcNow
                                        });
                                    }
                                }

                                #region NU-16
                                if (bundleSCi.Product.IsMealPlan)
                                {
                                    string mealPlanRecipientAcctNum = "", mealPlanRecipientAddress = "", mealPlanRecipientEmail = "", mealPlanRecipientName = "", mealPlanRecipientPhone = "";
                                    if (bundleSCi.Product.ShowStandardMealPlanFields)
                                    {
                                        _productAttributeParser.GetMealPlanAttribute(
                                                bundleSCi.AttributesXml,
                                                out mealPlanRecipientAcctNum,
                                                out mealPlanRecipientAddress,
                                                out mealPlanRecipientEmail,
                                                out mealPlanRecipientName,
                                                out mealPlanRecipientPhone
                                            );
                                    }
                                    else
                                    {

                                        mealPlanRecipientName = order.BillingAddress.FirstName + " " + order.BillingAddress.LastName;
                                        mealPlanRecipientEmail = order.BillingAddress.Email;
                                        mealPlanRecipientPhone = order.BillingAddress.PhoneNumber;
                                        mealPlanRecipientAddress = order.BillingAddress.Address1 + " " + order.BillingAddress.Address2 + " " + order.BillingAddress.City + ", " + order.BillingAddress.StateProvince + " " + order.BillingAddress.ZipPostalCode;


                                        if (_workContext.CurrentCustomer.GetAttribute<String>("IsSSOUser", processPaymentRequest.StoreId) == "true")
                                        {
                                            mealPlanRecipientAcctNum = _workContext.CurrentCustomer.GetAttribute<String>("fsueduemplid", processPaymentRequest.StoreId);
                                        }
                                        else //TODO:  NEed to tie the generic attribute to the mealplan somehow
                                        {
                                            string values = _productAttributeFormatter.FormatAttributes(bundleSCi.Product, bundleSCi.AttributesXml);


                                            if (values.Contains("Account ID Number: "))
                                            {
                                                mealPlanRecipientAcctNum = values.Split(new[] { "Account ID Number: " }, StringSplitOptions.None)[1];
                                            }
                                            else
                                            {
                                                mealPlanRecipientAcctNum = "NOT FOUND";
                                            }
                                        }



                                    }
                                    for (int i = 0; i < bundleSCi.Quantity; i++)
                                    {
                                        var mealPlan = new MealPlan
                                        {
                                            PurchasedWithOrderItemId = orderItem.Id,
                                            RecipientName = mealPlanRecipientName,
                                            RecipientAddress = mealPlanRecipientAddress,
                                            RecipientPhone = mealPlanRecipientPhone,
                                            RecipientEmail = mealPlanRecipientEmail,
                                            RecipientAcctNum = mealPlanRecipientAcctNum,
                                            CreatedOnUtc = DateTime.UtcNow,
                                            //BlackBoardCustomerNumber = Convert.ToInt64(mealPlanRecipientAcctNum == "NOT FOUND" || mealPlanRecipientAcctNum == "" ? "" : Convert.ToString(_customerService.GetBlackBoardCustomerNumber(int.Parse(mealPlanRecipientAcctNum))))
                                        };
                                        _mealPlanService.InsertMealPlan(mealPlan);
                                        orderItem.FulfillmentDateTime = DateTime.UtcNow;
                                        //If any product is auto fulfiled, we have to do two inserts into SAP_SalesJournal table (Status 1 and a status 2)
                                    }
                                }
                                #endregion

                                #region NU-17
                                if (bundleSCi.Product.IsDonation)
                                {

                                    string donorFirstName;
                                    string donorLastName;
                                    string donorAddress;
                                    string donorAddress2;
                                    string donorCity;
                                    string donorState;
                                    string donorZip;
                                    string donorPhone;
                                    string donorCountry;
                                    string comments;
                                    string notificationEmail;
                                    string onBehalfOfFullName;
                                    string donorCompany;
                                    bool includeGiftAmount;

                                    _productAttributeParser.GetDonationAttribute(
                                            bundleSCi.AttributesXml,
                                            out donorFirstName, out donorLastName,
                                                             out donorAddress, out donorAddress2, out donorCity,
                                                             out donorState, out donorZip, out donorPhone,
                                                             out donorCountry, out comments, out notificationEmail,
                                                             out onBehalfOfFullName, out includeGiftAmount, out donorCompany
                                        );

                                    for (int i = 0; i < sc.Quantity; i++)
                                    {
                                        var donation = new Donation
                                        {
                                            PurchasedWithOrderItemId = orderItem.Id,
                                            //PurchasedWithOrderItem =  orderItem,
                                            DonorFirstName = donorFirstName,
                                            DonorLastName = donorLastName,
                                            DonorCompany = donorCompany,
                                            DonorAddress = donorAddress,
                                            DonorAddress2 = donorAddress2,
                                            DonorCity = donorCity,
                                            DonorState = donorState,
                                            DonorZip = donorZip,
                                            DonorPhone = donorPhone,
                                            DonorCountry = donorCountry,
                                            Comments = comments,
                                            NotificationEmail = notificationEmail,
                                            OnBehalfOfFullName = onBehalfOfFullName,
                                            IncludeGiftAmount = includeGiftAmount,
                                            CreatedOnUtc = DateTime.UtcNow,
                                            Amount = scUnitPriceExclTax
                                        };
                                        _donationService.InsertDonation(donation);
                                        orderItem.FulfillmentDateTime = DateTime.UtcNow; //Donation Auto fulfilled
                                    }
                                }
                                #endregion

                                //inventory
                                _productService.AdjustInventory(bundleSCi.Product, -bundleSCi.Quantity, bundleSCi.AttributesXml);
                            }
                        }
                        else
                        {
                            //prices
                            decimal taxRate;
                            List<Discount> scDiscounts;
                            decimal discountAmount;
                            var scUnitPrice = _priceCalculationService.GetUnitPrice(sc);
                            var scSubTotal = _priceCalculationService.GetSubTotal(sc, true, out discountAmount, out scDiscounts);
                            var scUnitPriceInclTax = _taxService.GetProductPrice(sc.Product, scUnitPrice, true, details.Customer, true, out taxRate);
                            var scUnitPriceExclTax = _taxService.GetProductPrice(sc.Product, scUnitPrice, false, details.Customer, true, out taxRate);
                            var scSubTotalInclTax = _taxService.GetProductPrice(sc.Product, scSubTotal, true, details.Customer, true, out taxRate);
                            var scSubTotalExclTax = _taxService.GetProductPrice(sc.Product, scSubTotal, false, details.Customer, true, out taxRate);
                            var discountAmountInclTax = _taxService.GetProductPrice(sc.Product, discountAmount, true, details.Customer, true, out taxRate);
                            var discountAmountExclTax = _taxService.GetProductPrice(sc.Product, discountAmount, false, details.Customer, true, out taxRate);
                            foreach (var disc in scDiscounts)
                            {
                                if (!details.AppliedDiscounts.ContainsDiscount(disc))
                                {
                                    details.AppliedDiscounts.Add(disc);
                                }
                            }

                            var storeCommission = scSubTotal * (sc.Product.StoreCommission / 100);  /// NU-15

                            //attributes
                            var attributeDescription = _productAttributeFormatter.FormatAttributes(sc.Product, sc.AttributesXml, details.Customer);

                            var itemWeight = _shippingService.GetShoppingCartItemWeight(sc);

                            //save order item
                            var orderItem = new OrderItem
                            {
                                OrderItemGuid = Guid.NewGuid(),
                                Order = order,
                                IsReservation = sc.Product.IsReservation,
                                ProductId = sc.Product.Id,
                                UnitPriceInclTax = scUnitPriceInclTax,
                                UnitPriceExclTax = scUnitPriceExclTax,
                                PriceInclTax = scSubTotalInclTax,
                                PriceExclTax = scSubTotalExclTax,
                                PartialRefundOrderAmount = scSubTotalExclTax,
                                OriginalProductCost = _priceCalculationService.GetProductCost(sc.Product, sc.AttributesXml),
                                AttributeDescription = attributeDescription,
                                AttributesXml = sc.AttributesXml,
                                Quantity = sc.Quantity,
                                DiscountAmountInclTax = discountAmountInclTax,
                                DiscountAmountExclTax = discountAmountExclTax,
                                DownloadCount = 0,
                                IsDownloadActivated = false,
                                LicenseDownloadId = 0,
                                ItemWeight = itemWeight,
                                RentalStartDateUtc = sc.RentalStartDateUtc,
                                RentalEndDateUtc = sc.RentalEndDateUtc,
                                RequestedFulfillmentDateTime = sc.RequestedFullfilmentDateTime,/// NU-13
                                SelectedFulfillmentWarehouseId = sc.SelectedWarehouseId,/// NU-13           
                                SelectedShippingMethodName = sc.SelectedShippingMethodName, /// NU-13                                
                                SelectedShippingRateComputationMethodSystemName = sc.SelectedShippingRateComputationMethodSystemName, //SODMYWAY-
                                StoreCommission = storeCommission,  // NU-72
                                DeliveryAmountExclTax = sc.Product.AdditionalShippingCharge, //NU-90
                                DeliveryPickupFee = (shippingCollections == null || !shippingCollections.Any()) ? 0 : shippingCollections.Where(x => x.Key.Contains("Delivery-" + count + sc.Product.Id) || x.Key.Contains("In-Store Pickup-" + count + sc.Product.Id)).FirstOrDefault().Value,
                                ShippingFee = (shippingCollections == null || !shippingCollections.Any()) ? 0 : shippingCollections.Where(x => x.Key.Contains("UPS-" + count + sc.Product.Id)).FirstOrDefault().Value,
                                IsBundleProduct = sc.Product.IsBundleProduct ? true : false,
                            };


                            if (shippingCollections != null)
                            {
                                if (shippingCollections.Keys.Contains("Delivery-" + count + sc.Product.Id) || shippingCollections.Keys.Contains("In-Store Pickup-" + count + sc.Product.Id))
                                {
                                    orderItem.IsDeliveryPickUp = true;
                                }
                                if (shippingCollections.Keys.Contains("UPS-" + count + sc.Product.Id))
                                {
                                    orderItem.IsShipping = true;
                                }
                            }

                            #region Vanguard Shipping
                            if (sc.IsTieredShippingEnabled)
                            {
                                order.ShippingMethod = "Flat Rate Shipping";
                                order.ShippingRateComputationMethodSystemName = "Tiered Shipping";
                                orderItem.IsShipping = true;
                                orderItem.ShippingFee = sc.FlatShipping;
                                orderItem.SelectedShippingMethodName = "Flat Rate Shipping";
                                orderItem.SelectedShippingRateComputationMethodSystemName = "Tiered Shipping";
                            }
                            if (sc.IsContractShippingEnabled)
                            {
                                order.ShippingMethod = "Contract Shipping";
                                order.ShippingRateComputationMethodSystemName = "Tiered Shipping";
                                order.RCNumber = sc.RCNumber;
                                orderItem.IsShipping = true;
                                orderItem.ShippingFee = 0;
                                orderItem.SelectedShippingMethodName = "Contract Shipping";
                                orderItem.SelectedShippingRateComputationMethodSystemName = "Tiered Shipping";
                                orderItem.RCNumber = sc.RCNumber;
                            }
                            if (sc.IsInterOfficeDeliveryEnabled)
                            {
                                order.ShippingMethod = "MailStop Shipping";
                                order.ShippingRateComputationMethodSystemName = "Tiered Shipping";
                                order.MailStopAddress = sc.MailStopAddress;
                                orderItem.IsShipping = true;
                                orderItem.ShippingFee = 0;
                                orderItem.SelectedShippingMethodName = "MailStop Shipping";
                                orderItem.SelectedShippingRateComputationMethodSystemName = "Tiered Shipping";
                                orderItem.MailStopAddress = sc.MailStopAddress;
                            }
                            #endregion Vanguard Shipping

                            #region  NU-13
                            if (orderItem.SelectedFulfillmentWarehouseId != null)
                            {
                                orderItem.SelectedFulfillmentWarehouse = _shippingService.GetWarehouseById(orderItem.SelectedFulfillmentWarehouseId.GetValueOrDefault(-1));
                            }
                            if (!sc.Product.IsPickupEnabled && !sc.Product.IsShipEnabled)
                            {
                                if (sc.Product.IsMealPlan || sc.Product.IsDonation || sc.Product.IsDownload) /// NU-16
                                {
                                    orderItem.FulfillmentDateTime = DateTime.Now;
                                }
                            }
                            else
                            {
                                count++;
                            }
                            #endregion


                            order.OrderItems.Add(orderItem);
                            _orderService.UpdateOrder(order);

                            #region prepare to insert list of reserved products

                            if (sc.Product.IsReservation)
                            {
                                ReservedProduct reservedProduct = new ReservedProduct();
                                reservedProduct.ProductId = sc.ProductId;
                                reservedProduct.OrderItemId = orderItem.Id;
                                reservedProduct.ReservationDate = sc.ReservationDate;
                                reservedProduct.ReservedTimeSlot = sc.ReservedTimeSlot;
                                reservedProduct.ReservedUnits = sc.Quantity;
                                _orderService.InsertReservedProduct(reservedProduct);
                            }
                            #endregion

                            #region Section to Apply the JE BusinessRules

                            var orderItemDetails = _orderService.GetOrderItemByGuid(orderItem.OrderItemGuid);
                            var orderDetails = new OrderDetails
                            {
                                OrderId = order.Id,
                                OrderItemId = orderItemDetails.Id,
                                StoreId = order.StoreId,
                                IsMealPlan = sc.Product.IsMealPlan,
                                IsDonation = sc.Product.IsDonation,
                                IsGift = sc.Product.IsGiftCard,
                                ProductName = sc.Product.Name
                            };
                            ApplyJEAddressBusinessRules(orderItem, order, orderDetails);
                            _orderService.InsertJEBusinessLogicOrderDetails(orderDetails);

                            var itemsMapping = new ProductGlCodeMapping();
                            ApplyUnfulfilledProductsJeGLCodeBusinessRules(orderItem, order, itemsMapping);
                            #endregion

                            //"Auto" fullfill.  Not running the fulfill method for some reason.
                            //if (!sc.Product.IsPickupEnabled && !sc.Product.IsShipEnabled)
                            //{
                            if (sc.Product.IsMealPlan || sc.Product.IsDonation || sc.Product.IsDownload) /// NU-16
                            {
                                var itemDetails = _orderService.GetOrderItemByGuid(orderItem.OrderItemGuid);
                                InsertIntoSAPSalesJournalOrderItem(order, itemDetails, 2);
                            }
                            //}

                            if (!sc.Product.IsMealPlan && !sc.Product.IsDonation && !sc.Product.IsDownload)
                            {
                                var itemDetails = _orderService.GetOrderItemByGuid(orderItem.OrderItemGuid);
                                if (sc.Product.IsBundleProduct)
                                {
                                    var associatedProductDict = (Dictionary<string, string>)HttpContext.Current.Session["AssociatedProductPerShoppingCartItem"];
                                    foreach (KeyValuePair<string, string> entry in associatedProductDict)
                                    {
                                        var scService = EngineContext.Current.Resolve<IShoppingCartService>();
                                        string[] dictKey = entry.Key.Split('_');
                                        string[] dictValue = entry.Value.Split('_');
                                        InsertIntoSAPSalesJournalOrderItem(order, itemDetails, 1, Convert.ToInt32(dictKey[2]), sc.Product.IsBundleProduct);

                                    }
                                }
                                else
                                {
                                    InsertIntoSAPSalesJournalOrderItem(order, itemDetails, 1, sc.Product.Id, sc.Product.IsBundleProduct);
                                }
                            }

                            #region NU-63
                            //_SAPService.CreateEntry(orderItem, 1);
                            //if (sc.Product.IsMealPlan || sc.Product.IsDonation || sc.Product.IsDownload)
                            //{
                            //    _SAPService.CreateEntry(orderItem, 2);
                            //}
                            #endregion

                            //gift cards
                            if (sc.Product.IsGiftCard)
                            {
                                string giftCardRecipientName;
                                string giftCardRecipientEmail;
                                string giftCardSenderName;
                                string giftCardSenderEmail;
                                string giftCardMessage;
                                _productAttributeParser.GetGiftCardAttribute(sc.AttributesXml, out giftCardRecipientName,
                                    out giftCardRecipientEmail, out giftCardSenderName, out giftCardSenderEmail, out giftCardMessage);

                                for (var i = 0; i < sc.Quantity; i++)
                                {
                                    _giftCardService.InsertGiftCard(new GiftCard
                                    {
                                        GiftCardType = sc.Product.GiftCardType,
                                        PurchasedWithOrderItem = orderItem,
                                        Amount = sc.Product.OverriddenGiftCardAmount.HasValue ? sc.Product.OverriddenGiftCardAmount.Value : scUnitPriceExclTax,
                                        IsGiftCardActivated = false,
                                        GiftCardCouponCode = _giftCardService.GenerateGiftCardCode(),
                                        RecipientName = giftCardRecipientName,
                                        RecipientEmail = giftCardRecipientEmail,
                                        SenderName = giftCardSenderName,
                                        SenderEmail = giftCardSenderEmail,
                                        Message = giftCardMessage,
                                        IsRecipientNotified = false,
                                        CreatedOnUtc = DateTime.UtcNow
                                    });
                                }
                            }

                            #region NU-16
                            if (sc.Product.IsMealPlan)
                            {
                                string mealPlanRecipientAcctNum = "", mealPlanRecipientAddress = "", mealPlanRecipientEmail = "", mealPlanRecipientName = "", mealPlanRecipientPhone = "";
                                if (sc.Product.ShowStandardMealPlanFields)
                                {
                                    _productAttributeParser.GetMealPlanAttribute(
                                            sc.AttributesXml,
                                            out mealPlanRecipientAcctNum,
                                            out mealPlanRecipientAddress,
                                            out mealPlanRecipientEmail,
                                            out mealPlanRecipientName,
                                            out mealPlanRecipientPhone
                                        );
                                }
                                else
                                {

                                    mealPlanRecipientName = order.BillingAddress.FirstName + " " + order.BillingAddress.LastName;
                                    mealPlanRecipientEmail = order.BillingAddress.Email;
                                    mealPlanRecipientPhone = order.BillingAddress.PhoneNumber;
                                    mealPlanRecipientAddress = order.BillingAddress.Address1 + " " + order.BillingAddress.Address2 + " " + order.BillingAddress.City + ", " + order.BillingAddress.StateProvince + " " + order.BillingAddress.ZipPostalCode;


                                    if (_workContext.CurrentCustomer.GetAttribute<String>("IsSSOUser", processPaymentRequest.StoreId) == "true")
                                    {
                                        mealPlanRecipientAcctNum = _workContext.CurrentCustomer.GetAttribute<String>("fsueduemplid", processPaymentRequest.StoreId);
                                    }
                                    else //TODO:  NEed to tie the generic attribute to the mealplan somehow
                                    {
                                        string values = _productAttributeFormatter.FormatAttributes(sc.Product, sc.AttributesXml);


                                        if (values.Contains("Account ID Number: "))
                                        {
                                            mealPlanRecipientAcctNum = values.Split(new[] { "Account ID Number: " }, StringSplitOptions.None)[1];
                                        }
                                        else
                                        {
                                            mealPlanRecipientAcctNum = "NOT FOUND";
                                        }
                                    }



                                }
                                for (int i = 0; i < sc.Quantity; i++)
                                {
                                    var mealPlan = new MealPlan
                                    {
                                        PurchasedWithOrderItemId = orderItem.Id,
                                        RecipientName = mealPlanRecipientName,
                                        RecipientAddress = mealPlanRecipientAddress,
                                        RecipientPhone = mealPlanRecipientPhone,
                                        RecipientEmail = mealPlanRecipientEmail,
                                        RecipientAcctNum = mealPlanRecipientAcctNum,
                                        CreatedOnUtc = DateTime.UtcNow,
                                        //BlackBoardCustomerNumber = Convert.ToInt64(mealPlanRecipientAcctNum == "NOT FOUND" || mealPlanRecipientAcctNum == "" ? "" : Convert.ToString(_customerService.GetBlackBoardCustomerNumber(int.Parse(mealPlanRecipientAcctNum))))
                                    };
                                    _mealPlanService.InsertMealPlan(mealPlan);
                                    orderItem.FulfillmentDateTime = DateTime.UtcNow;
                                    //If any product is auto fulfiled, we have to do two inserts into SAP_SalesJournal table (Status 1 and a status 2)
                                }
                            }
                            #endregion

                            #region NU-17
                            if (sc.Product.IsDonation)
                            {

                                string donorFirstName;
                                string donorLastName;
                                string donorAddress;
                                string donorAddress2;
                                string donorCity;
                                string donorState;
                                string donorZip;
                                string donorPhone;
                                string donorCountry;
                                string comments;
                                string notificationEmail;
                                string onBehalfOfFullName;
                                string donorCompany;
                                bool includeGiftAmount;

                                _productAttributeParser.GetDonationAttribute(
                                        sc.AttributesXml,
                                        out donorFirstName, out donorLastName,
                                                         out donorAddress, out donorAddress2, out donorCity,
                                                         out donorState, out donorZip, out donorPhone,
                                                         out donorCountry, out comments, out notificationEmail,
                                                         out onBehalfOfFullName, out includeGiftAmount, out donorCompany
                                    );

                                for (int i = 0; i < sc.Quantity; i++)
                                {
                                    var donation = new Donation
                                    {
                                        PurchasedWithOrderItemId = orderItem.Id,
                                        //PurchasedWithOrderItem =  orderItem,
                                        DonorFirstName = donorFirstName,
                                        DonorLastName = donorLastName,
                                        DonorCompany = donorCompany,
                                        DonorAddress = donorAddress,
                                        DonorAddress2 = donorAddress2,
                                        DonorCity = donorCity,
                                        DonorState = donorState,
                                        DonorZip = donorZip,
                                        DonorPhone = donorPhone,
                                        DonorCountry = donorCountry,
                                        Comments = comments,
                                        NotificationEmail = notificationEmail,
                                        OnBehalfOfFullName = onBehalfOfFullName,
                                        IncludeGiftAmount = includeGiftAmount,
                                        CreatedOnUtc = DateTime.UtcNow,
                                        Amount = scUnitPriceExclTax
                                    };
                                    _donationService.InsertDonation(donation);
                                    orderItem.FulfillmentDateTime = DateTime.UtcNow; //Donation Auto fulfilled
                                }
                            }
                            #endregion

                            //inventory
                            _productService.AdjustInventory(sc.Product, -sc.Quantity, sc.AttributesXml);
                        }
                    }

                    _taxService.PostDistributiveTaxData(details, order);  //Calculated

                    var listofProd = new List<int>();
                    var sameProductcount = order.OrderItems.GroupBy(x => x.ProductId)
                          .Where(g => g.Count() > 1)
                          .Select(y => y.Key)
                          .ToList();
                    if (sameProductcount.Any())
                    {
                        foreach (var datainsameprod in sameProductcount)
                        {
                            listofProd.Add(datainsameprod);
                        }
                    }

                    foreach (var orderItems in order.OrderItems)
                    {
                        var itemsMapping = new ProductGlCodeMapping();
                        ApplyJeVertexBusinessRules(orderItems, order, itemsMapping, false, listofProd);
                    }
                    //clear shopping cart
                    details.Cart.ToList().ForEach(sci => _shoppingCartService.DeleteShoppingCartItem(sci, false));

                    //discount usage history
                    foreach (var discount in details.AppliedDiscounts)
                    {
                        _discountService.InsertDiscountUsageHistory(new DiscountUsageHistory
                        {
                            Discount = discount,
                            Order = order,
                            CreatedOnUtc = DateTime.UtcNow
                        });
                    }

                    //gift card usage history
                    if (details.AppliedGiftCards != null)
                    {
                        foreach (var agc in details.AppliedGiftCards)
                        {
                            agc.GiftCard.GiftCardUsageHistory.Add(new GiftCardUsageHistory
                            {
                                GiftCard = agc.GiftCard,
                                UsedWithOrder = order,
                                UsedValue = agc.AmountCanBeUsed,
                                CreatedOnUtc = DateTime.UtcNow
                            });
                            _giftCardService.UpdateGiftCard(agc.GiftCard);
                        }
                    }

                    //recurring orders
                    if (details.IsRecurringShoppingCart)
                    {
                        //create recurring payment (the first payment)
                        var rp = new RecurringPayment
                        {
                            CycleLength = processPaymentRequest.RecurringCycleLength,
                            CyclePeriod = processPaymentRequest.RecurringCyclePeriod,
                            TotalCycles = processPaymentRequest.RecurringTotalCycles,
                            StartDateUtc = DateTime.UtcNow,
                            IsActive = true,
                            CreatedOnUtc = DateTime.UtcNow,
                            InitialOrder = order,
                        };
                        _orderService.InsertRecurringPayment(rp);

                        switch (_paymentService.GetRecurringPaymentType(processPaymentRequest.PaymentMethodSystemName))
                        {
                            case RecurringPaymentType.NotSupported:
                                //not supported
                                break;
                            case RecurringPaymentType.Manual:
                                rp.RecurringPaymentHistory.Add(new RecurringPaymentHistory
                                {
                                    RecurringPayment = rp,
                                    CreatedOnUtc = DateTime.UtcNow,
                                    OrderId = order.Id,
                                });
                                _orderService.UpdateRecurringPayment(rp);
                                break;
                            case RecurringPaymentType.Automatic:
                                //will be created later (process is automated)
                                break;
                            default:
                                break;
                        }
                    }
                    #endregion

                    //notifications
                    SendNotificationsAndSaveNotes(order);

                    //reset checkout data
                    _customerService.ResetCheckoutData(details.Customer, processPaymentRequest.StoreId, clearCouponCodes: true, clearCheckoutAttributes: true);
                    _customerActivityService.InsertActivity("PublicStore.PlaceOrder", _localizationService.GetResource("ActivityLog.PublicStore.PlaceOrder"), order.Id);

                    //check order status
                    CheckOrderStatus(order);

                    //raise event       
                    _eventPublisher.Publish(new OrderPlacedEvent(order));

                    if (order.PaymentStatus == PaymentStatus.Paid)
                    {
                        ProcessOrderPaid(order);
                    }
                }
                else
                {
                    foreach (var paymentError in processPaymentResult.Errors)
                    {
                        result.AddError(string.Format(_localizationService.GetResource("Checkout.PaymentError"), paymentError));
                    }
                }
            }
            catch (Exception exc)
            {
                _logger.Error(exc.Message, exc);
                result.AddError(exc.Message);
            }

            #region Process errors

            if (!result.Success)
            {
                for (int i = 0; i < result.Errors.Count; i++)
                {
                    if (result.Errors[i].ToLower().Contains("payment error: 100") || result.Errors[i].ToLower().Contains("payment error: 44") || result.Errors[i].ToLower().Contains("payment error: 0")) // Condition added for throwing Generic SE-67
                    {
                        result.Errors[i] = "Payment Error:  Please verify your Billing Address, City, State and Postal Code and try again";
                    }

                }

                //log errors
                var logError = result.Errors.Aggregate("Error while placing order. ",
                    (current, next) => string.Format("{0}Error {1}: {2}. ", current, result.Errors.IndexOf(next) + 1, next));
                var customer = _customerService.GetCustomerById(processPaymentRequest.CustomerId);
                _logger.Error(logError, customer: customer);
            }

            #endregion

            return result;
        }


        #region Method to Insert Records into The Product_glcode_calc for JE Reports
        public void InsertGlCodeCalcs(Order order, OrderItem orderItem, bool isFulfilled, bool isRefunded)
        {
            bool splitGl = orderItem.Product.TaxCategoryId == -1 ? true : false;
            int storeId = order.StoreId;

            #region Product_glcode_calc
            Dictionary<int, decimal?> glcodesAndAmount = new Dictionary<int, decimal?>();
            //find GLCode and calculate Amount

            //This is from the Product_GlCode_Mapping Table
            List<ProductGlCode> PaidCodes = _glCodeService.GetProductGlCodes(orderItem.Product, GLCodeStatusType.Paid).ToList();

            //we need to get the Tax GL Codes from the Vertex Response XML 
            //from the GlCodeService
            var vertexOrderTaxGls = _glCodeService.GetVertexGlBreakdown(orderItem.OrderId, orderItem.ProductId, TaxRequestType.ConfirmDistributiveTax); //gets the order gls from vertex response


            //This is from the Product_GlCode_Mapping Table
            List<ProductGlCode> DeliveredglCodes = _glCodeService.GetProductGlCodes(orderItem.Product, GLCodeStatusType.Deferred).ToList();



            var isCancelled = order.OrderStatus == OrderStatus.Cancelled;
            int glstatusType = 0;

            decimal? GlAmount = 0;
            if (orderItem.FulfillmentDateTime != null) // Section to Capture Preffered Gl Code.
            {
                int GlcodeId = 0;
                bool isProcessedShippingGl = false;
                bool isProcessedDeliveryGl = false;
                foreach (var paidCodes in PaidCodes)
                {
                    //if (/*(paidCodes.GlCode.GlCodeName == "70161010" || paidCodes.GlCode.GlCodeName == "48702001")*/ (orderItem.Product.IsPickupEnabled || orderItem.Product.IsLocalDelivery)) //Gathering the Delivery Fee
                    if (!isProcessedDeliveryGl && (orderItem.Product.IsPickupEnabled || orderItem.Product.IsLocalDelivery) && paidCodes.GlCode.GlCodeName == "70161010" && orderItem.IsDeliveryPickUp == true) //Gathering the Delivery Fee
                    {
                        GlAmount = orderItem.DeliveryPickupFee == null ? orderItem.ShippingFee * (isRefunded || isCancelled ? -1 : 1) : orderItem.DeliveryPickupFee * (isRefunded || isCancelled ? -1 : 1);
                        GlcodeId = paidCodes.GlCodeId; // Takes the GlCode ID for the Delivery Fee/Local Delivery.
                        isProcessedDeliveryGl = true;
                    }
                    //else if (/*(paidCodes.GlCode.GlCodeName == "62610001" || paidCodes.GlCode.GlCodeName == "48702001") &&*/ (orderItem.Product.IsShipEnabled)) // Gathering the Shipping Fee
                    else if (!isProcessedShippingGl && (orderItem.Product.IsShipEnabled) && paidCodes.GlCode.GlCodeName == "62610001" && orderItem.IsShipping == true) // Gathering the Shipping Fee
                    {
                        GlAmount = orderItem.DeliveryPickupFee == null ? orderItem.ShippingFee * (isRefunded || isCancelled ? -1 : 1) : orderItem.DeliveryPickupFee * (isRefunded || isCancelled ? -1 : 1);
                        GlcodeId = paidCodes.GlCodeId;
                        isProcessedShippingGl = true;
                    }
                    else
                    {
                        GlcodeId = paidCodes.GlCodeId;
                        GlAmount = (paidCodes.CalculateGLAmount(orderItem, splitGl)) * (isRefunded || isCancelled ? -1 : 1);
                    }
                    if (!glcodesAndAmount.ContainsKey(GlcodeId))
                    {
                        glcodesAndAmount.Add(GlcodeId, GlAmount);
                    }
                }
                if (orderItem.Product.IsMealPlan)
                {
                    glstatusType = Convert.ToInt32(GLCodeStatusType.Paid);
                }
                else
                {
                    glstatusType = Convert.ToInt32(GLCodeStatusType.Deferred);
                }

                foreach (var items in glcodesAndAmount)
                {
                    if (items.Value != 0)
                    {
                        var productcalculatedGlCodes = new ProductGlCodeCalculation()
                        {
                            StoreId = storeId,
                            OrderId = order.Id,
                            ProductId = orderItem.ProductId,
                            GLCodeId = items.Key,
                            GLCodeName = _glCodeService.GetGlCodeById(items.Key).GlCodeName,
                            GlStatusType = (isRefunded || isCancelled) ? 1 : glstatusType,
                            Amount = items.Value,
                            Processed = false

                        };
                        _orderService.InsertProductCalculatedGlCodes(productcalculatedGlCodes);
                    }
                }
            }
            else // Section to Capture Deferred Gl Code.
            {
                glstatusType = Convert.ToInt32(GLCodeStatusType.Paid);

                if (orderItem.Product.VendorId != 0)
                {
                    int DeliveredGlcodeId = 0;
                    bool isProcessed = false;
                    foreach (var deliveredGl in DeliveredglCodes)
                    {
                        DeliveredGlcodeId = deliveredGl.GlCodeId;
                        //if ((orderItem.Product.IsShipEnabled || orderItem.Product.IsPickupEnabled || orderItem.Product.IsLocalDelivery) /*&& deliveredGl.GlCode.GlCodeName == "48702040"*/)
                        if ((orderItem.Product.IsShipEnabled || orderItem.Product.IsPickupEnabled || orderItem.Product.IsLocalDelivery) && (!isProcessed && DeliveredglCodes.Count == 1))  // THIS KIND OF CODING SHOULD NEVER BE CONSIDERED FOR FINAL RELEASE OR FUTURE CONSIDERATION. IT MAY LEAD TO ERROR IF THERE IS A CHANGE IN DATA IN THE DATABASE.
                        {
                            var DeliveryglcodeDetails = _glCodeService.GetGlCodeByGlcodeName("48702040");
                            //GlAmount = (_httpContext.Session["orderShippingTotalExclTax"] == null ? 0 : Convert.ToDecimal(_httpContext.Session["orderShippingTotalExclTax"])) * (isRefunded || isCancelled ? -1 : 1);//PUSH AMOUNT OF UPS
                            GlAmount = (orderItem.DeliveryPickupFee == null || orderItem.DeliveryPickupFee == 0) ? orderItem.ShippingFee * (isRefunded || isCancelled ? -1 : 1) : orderItem.DeliveryPickupFee * (isRefunded || isCancelled ? -1 : 1);//PUSH AMOUNT OF UPS
                            DeliveredGlcodeId = DeliveryglcodeDetails == null ? 0 : DeliveryglcodeDetails.Id;
                            isProcessed = true;
                        }
                        else
                        {
                            GlAmount = (deliveredGl.CalculateGLAmount(orderItem, splitGl)) * (isRefunded || isCancelled ? -1 : 1);
                        }
                        if (GlAmount != 0)
                        {
                            var productcalculatedGlCodes = new ProductGlCodeCalculation()
                            {
                                StoreId = storeId,
                                OrderId = order.Id,
                                ProductId = orderItem.ProductId,
                                GLCodeId = DeliveredGlcodeId,
                                GLCodeName = _glCodeService.GetGlCodeById(DeliveredGlcodeId).GlCodeName,
                                GlStatusType = (isRefunded || isCancelled) ? 1 : glstatusType,
                                Amount = GlAmount,
                                Processed = false
                            };
                            _orderService.InsertProductCalculatedGlCodes(productcalculatedGlCodes);
                        }
                        if (DeliveredglCodes.Count == 1)
                        {
                            GlAmount = (deliveredGl.CalculateGLAmount(orderItem, splitGl)) * (isRefunded || isCancelled ? -1 : 1);
                            if (GlAmount != 0)
                            {
                                var productcalculatedGlCodes1 = new ProductGlCodeCalculation()
                                {
                                    StoreId = storeId,
                                    OrderId = order.Id,
                                    ProductId = orderItem.ProductId,
                                    GLCodeId = DeliveredGlcodeId,
                                    GLCodeName = "48702041",
                                    GlStatusType = (isRefunded || isCancelled) ? 1 : glstatusType,
                                    Amount = GlAmount,
                                    Processed = false
                                };
                                _orderService.InsertProductCalculatedGlCodes(productcalculatedGlCodes1);
                            }
                        }
                    }
                }
                else
                {
                    var productcalculatedGlCodes = new ProductGlCodeCalculation();
                    if (orderItem.Product.IsPickupEnabled || orderItem.Product.IsShipEnabled || orderItem.Product.IsLocalDelivery)
                    {
                        if (!orderItem.IsPartialRefund)
                        {
                            productcalculatedGlCodes = new ProductGlCodeCalculation()
                            {
                                StoreId = storeId,
                                OrderId = order.Id,
                                ProductId = orderItem.ProductId,
                                GLCodeId = DeliveredglCodes[0].GlCodeId,
                                GLCodeName = _glCodeService.GetGlCodeById(DeliveredglCodes[0].GlCodeId).GlCodeName,
                                GlStatusType = (isRefunded || isCancelled) ? 1 : glstatusType,
                                //Amount = (_httpContext.Session["orderShippingTotalExclTax"] == null ? 0 : Convert.ToDecimal(_httpContext.Session["orderShippingTotalExclTax"])) * (isRefunded || isCancelled ? -1 : 1),
                                Amount = (orderItem.DeliveryPickupFee == null || orderItem.DeliveryPickupFee == 0) ? orderItem.ShippingFee * (isRefunded || isCancelled ? -1 : 1) : orderItem.DeliveryPickupFee * (isRefunded || isCancelled ? -1 : 1),
                                Processed = false
                            };
                            if (productcalculatedGlCodes.Amount != 0)
                            {
                                _orderService.InsertProductCalculatedGlCodes(productcalculatedGlCodes);
                            }
                        }

                        productcalculatedGlCodes = new ProductGlCodeCalculation()
                        {
                            StoreId = storeId,
                            OrderId = order.Id,
                            ProductId = orderItem.ProductId,
                            GLCodeId = DeliveredglCodes[0].GlCodeId,
                            GLCodeName = _glCodeService.GetGlCodeById(DeliveredglCodes[0].GlCodeId).GlCodeName,
                            GlStatusType = (isRefunded || isCancelled) ? 1 : glstatusType,
                            Amount = orderItem.IsPartialRefund ? (orderItem.TotalRefundedAmount * (isRefunded || isCancelled ? -1 : 1)) : (DeliveredglCodes[0].CalculateGLAmount(orderItem, splitGl)) * (isRefunded || isCancelled ? -1 : 1),
                            Processed = false
                        };
                        if (productcalculatedGlCodes.Amount != 0)
                        {
                            _orderService.InsertProductCalculatedGlCodes(productcalculatedGlCodes);
                        }
                    }
                    else
                    {
                        productcalculatedGlCodes = new ProductGlCodeCalculation()
                        {
                            StoreId = storeId,
                            OrderId = order.Id,
                            ProductId = orderItem.ProductId,
                            GLCodeId = DeliveredglCodes[0].GlCodeId,
                            GLCodeName = _glCodeService.GetGlCodeById(DeliveredglCodes[0].GlCodeId).GlCodeName,
                            GlStatusType = (isRefunded || isCancelled) ? 1 : glstatusType,
                            Amount = (DeliveredglCodes[0].CalculateGLAmount(orderItem, splitGl)) * (isRefunded || isCancelled ? -1 : 1),
                            Processed = false
                        };
                        _orderService.InsertProductCalculatedGlCodes(productcalculatedGlCodes);
                    }

                }
            }
            //for orders that don't use vertex for tax calc, we need to get the tax from the calc of priceInclTax - PriceExclTax from order Item table
            if (vertexOrderTaxGls.Any())
            {
                int statusType = (isRefunded || isCancelled) ? 1 : glstatusType;

                foreach (var vertexGL in vertexOrderTaxGls)
                {
                    if (statusType == 2) // checking if the tax to be posted is for Delivery Report JE
                    {
                        //if (vertexGL.taxCode != "DELV" || vertexGL.taxCode != "SHIP") // Do not insert the Tax if the tax code has delivery or shipping fee with them in Delivery Reports JE
                        //{
                        //    if (vertexGL.Total != 0) // If the Tax amount is 0 then skip the Insert of 0 dollar tax in to the databse for Deliervy report JE
                        //    {
                        //        var productcalculatedGlCodes = new ProductGlCodeCalculation()
                        //        {
                        //            StoreId = storeId,
                        //            OrderId = order.Id,
                        //            ProductId = orderItem.ProductId,
                        //            GLCodeName = vertexGL.GlCode,
                        //            Amount = (vertexGL.Total) * (isRefunded || isCancelled ? -1 : 1),
                        //            GlStatusType = (isRefunded || isCancelled) ? 1 : glstatusType,
                        //            //GlStatusType = (isFulfilled && isAutoFullfilled) ? Convert.ToInt32(GLCodeStatusType.Paid) : Convert.ToInt32(GLCodeStatusType.Deferred),
                        //            Processed = false

                        //        };
                        //        _orderService.InsertProductCalculatedGlCodes(productcalculatedGlCodes);
                        //    }
                        //}
                    }
                    else
                    {
                        if (vertexGL.Total != 0) // If the Tax amount is 0 then skip the Insert of 0 dollar tax in to the databse for Deliervy report JE
                        {
                            var productcalculatedGlCodes = new ProductGlCodeCalculation()
                            {
                                StoreId = storeId,
                                OrderId = order.Id,
                                ProductId = orderItem.ProductId,
                                GLCodeName = vertexGL.GlCode,
                                Amount = (vertexGL.Total) * (isRefunded || isCancelled ? -1 : 1),
                                GlStatusType = (isRefunded || isCancelled) ? 1 : glstatusType,
                                //GlStatusType = (isFulfilled && isAutoFullfilled) ? Convert.ToInt32(GLCodeStatusType.Paid) : Convert.ToInt32(GLCodeStatusType.Deferred),
                                Processed = false

                            };
                            _orderService.InsertProductCalculatedGlCodes(productcalculatedGlCodes);
                        }

                    }
                }
            }
            else
            {
                //either no tax on vertex order or order not using vertex.
                decimal tax = orderItem.PriceInclTax - orderItem.PriceExclTax;
                //decimal tax = 0;
                if (tax > 0)
                {
                    var productcalculatedGlCodes = new ProductGlCodeCalculation()
                    {
                        StoreId = storeId,
                        OrderId = order.Id,
                        ProductId = orderItem.ProductId,
                        GLCodeId = 10,
                        GLCodeName = "44571098", //tax glcode
                        Amount = (tax) * (isRefunded || isCancelled ? -1 : 1),
                        GlStatusType = (isRefunded || isCancelled) ? 1 : glstatusType,
                        Processed = false

                    };
                    _orderService.InsertProductCalculatedGlCodes(productcalculatedGlCodes);
                }
            }



            #endregion
        }
        #endregion
        /// <summary>
        /// Update order totals
        /// </summary>
        /// <param name="updateOrderParameters">Parameters for the updating order</param>
        public virtual void UpdateOrderTotals(UpdateOrderParameters updateOrderParameters)
        {
            if (!_orderSettings.AutoUpdateOrderTotalsOnEditingOrder)
            {
                return;
            }

            var updatedOrder = updateOrderParameters.UpdatedOrder;
            var updatedOrderItem = updateOrderParameters.UpdatedOrderItem;

            //restore shopping cart from order items
            var restoredCart = updatedOrder.OrderItems.Select(orderItem => new ShoppingCartItem
            {
                Id = orderItem.Id,
                AttributesXml = orderItem.AttributesXml,
                Customer = updatedOrder.Customer,
                Product = orderItem.Product,
                Quantity = orderItem.Id == updatedOrderItem.Id ? updateOrderParameters.Quantity : orderItem.Quantity,
                RentalEndDateUtc = orderItem.RentalEndDateUtc,
                RentalStartDateUtc = orderItem.RentalStartDateUtc,
                ShoppingCartType = ShoppingCartType.ShoppingCart,
                StoreId = updatedOrder.StoreId
            }).ToList();

            //get shopping cart item which has been updated
            var updatedShoppingCartItem = restoredCart.FirstOrDefault(shoppingCartItem => shoppingCartItem.Id == updatedOrderItem.Id);
            var itemDeleted = updatedShoppingCartItem == null;

            //validate shopping cart for warnings
            updateOrderParameters.Warnings.AddRange(_shoppingCartService.GetShoppingCartWarnings(restoredCart, string.Empty, false));
            if (!itemDeleted)
            {
                updateOrderParameters.Warnings.AddRange(_shoppingCartService.GetShoppingCartItemWarnings(updatedOrder.Customer, updatedShoppingCartItem.ShoppingCartType,
                    updatedShoppingCartItem.Product, updatedOrder.StoreId, updatedShoppingCartItem.AttributesXml, updatedShoppingCartItem.CustomerEnteredPrice,
                    updatedShoppingCartItem.RentalStartDateUtc, updatedShoppingCartItem.RentalEndDateUtc, updatedShoppingCartItem.Quantity, false));
            }

            _orderTotalCalculationService.UpdateOrderTotals(updateOrderParameters, restoredCart);

            if (updateOrderParameters.PickupPoint != null)
            {
                updatedOrder.PickUpInStore = true;
                updatedOrder.PickupAddress = new Address
                {
                    Address1 = updateOrderParameters.PickupPoint.Address,
                    City = updateOrderParameters.PickupPoint.City,
                    Country = _countryService.GetCountryByTwoLetterIsoCode(updateOrderParameters.PickupPoint.CountryCode),
                    ZipPostalCode = updateOrderParameters.PickupPoint.ZipPostalCode,
                    CreatedOnUtc = DateTime.UtcNow,
                };
                updatedOrder.ShippingMethod = string.Format(_localizationService.GetResource("Checkout.PickupPoints.Name"), updateOrderParameters.PickupPoint.Name);
                updatedOrder.ShippingRateComputationMethodSystemName = updateOrderParameters.PickupPoint.ProviderSystemName;
            }

            if (!itemDeleted)
            {
                updatedOrderItem.ItemWeight = _shippingService.GetShoppingCartItemWeight(updatedShoppingCartItem);
                updatedOrderItem.OriginalProductCost = _priceCalculationService.GetProductCost(updatedShoppingCartItem.Product, updatedShoppingCartItem.AttributesXml);
                updatedOrderItem.AttributeDescription = _productAttributeFormatter.FormatAttributes(updatedShoppingCartItem.Product,
                    updatedShoppingCartItem.AttributesXml, updatedOrder.Customer);

                //gift cards
                if (updatedShoppingCartItem.Product.IsGiftCard)
                {
                    string giftCardRecipientName;
                    string giftCardRecipientEmail;
                    string giftCardSenderName;
                    string giftCardSenderEmail;
                    string giftCardMessage;
                    _productAttributeParser.GetGiftCardAttribute(updatedShoppingCartItem.AttributesXml, out giftCardRecipientName,
                        out giftCardRecipientEmail, out giftCardSenderName, out giftCardSenderEmail, out giftCardMessage);

                    for (var i = 0; i < updatedShoppingCartItem.Quantity; i++)
                    {
                        _giftCardService.InsertGiftCard(new GiftCard
                        {
                            GiftCardType = updatedShoppingCartItem.Product.GiftCardType,
                            PurchasedWithOrderItem = updatedOrderItem,
                            Amount = updatedShoppingCartItem.Product.OverriddenGiftCardAmount.HasValue ?
                                updatedShoppingCartItem.Product.OverriddenGiftCardAmount.Value : updatedOrderItem.UnitPriceExclTax,
                            IsGiftCardActivated = false,
                            GiftCardCouponCode = _giftCardService.GenerateGiftCardCode(),
                            RecipientName = giftCardRecipientName,
                            RecipientEmail = giftCardRecipientEmail,
                            SenderName = giftCardSenderName,
                            SenderEmail = giftCardSenderEmail,
                            Message = giftCardMessage,
                            IsRecipientNotified = false,
                            CreatedOnUtc = DateTime.UtcNow
                        });
                    }
                }
            }

            _orderService.UpdateOrder(updatedOrder);

            //discount usage history
            var discountUsageHistoryForOrder = _discountService.GetAllDiscountUsageHistory(null, updatedOrder.Customer.Id, updatedOrder.Id);
            foreach (var discount in updateOrderParameters.AppliedDiscounts)
            {
                if (!discountUsageHistoryForOrder.Where(history => history.DiscountId == discount.Id).Any())
                {
                    _discountService.InsertDiscountUsageHistory(new DiscountUsageHistory
                    {
                        Discount = discount,
                        Order = updatedOrder,
                        CreatedOnUtc = DateTime.UtcNow
                    });
                }
            }

            CheckOrderStatus(updatedOrder);
        }


        #region Applying Business Rules For JE Reports
        public virtual void ApplyJEAddressBusinessRules(OrderItem items, Order order, OrderDetails orderDetails)
        {
            try
            {

                #region StoreDetails and State ProvinceDetails And Ship To Address

                var storeDetails = _storeRepository.GetById(order.StoreId); // Fetching the store Details only once for item
                var stateProvinceDetails = _stateProvinceRepository.GetById(storeDetails.CompanyStateProvinceId); // Fetcing the state province details for the store
                var shipToAddress = _addressRepository.GetById(order.ShippingAddressId); // Recipient Address of the Customer


                // Setting the Store Address here for One time which shall be the Unit Details in the Report
                // In the event store details comes back null, populate the values with ""
                orderDetails.StoreZip = storeDetails?.CompanyZipPostalCode ?? "";
                orderDetails.StoreCity = storeDetails?.CompanyCity ?? "";
                // In the event teh store details are partially populated but missing a 
                // state, let's also nullcheck the state object
                orderDetails.StoreState = stateProvinceDetails?.Abbreviation ?? "";
                orderDetails.StoreName = storeDetails?.Name ?? "";
                // These two should always be provided, so I will not be nullchecking these. 
                orderDetails.StoreExtKey = storeDetails.ExtKey;
                orderDetails.StoreLegalEntity = storeDetails.LegalEntity;

                #endregion

                #region Vendor Products Address Configurations
                if (items.Product.VendorId != 0)
                {
                    if (items.Product.IsShipEnabled || items.IsShipping)
                    {
                        var warehouseDetails = _shippingService.GetWarehouseById(items.Product.WarehouseId); // For vendor pulling the Warehouse details configured in the Product Config.

                        if (warehouseDetails == null) // Ship From as Store Address
                        {
                            // In case store details is null -- it has still not been 
                            // nullchecked, so do null conditionals on the object
                            orderDetails.ShipFromZip = storeDetails?.CompanyZipPostalCode ?? "";
                            orderDetails.ShipFromCity = storeDetails?.CompanyCity ?? "";
                            // Same with state object. 
                            orderDetails.ShipFromState = stateProvinceDetails?.Abbreviation ?? ""; // This is the Store state details
                        }
                        else // Ship From as Warehouse Address
                        {
                            var warehouseAddressDetails = _addressRepository.GetById(warehouseDetails.AddressId);
                            if (warehouseAddressDetails != null)
                            {
                                orderDetails.ShipFromZip = warehouseAddressDetails.ZipPostalCode;
                                orderDetails.ShipFromCity = warehouseAddressDetails.City;
                                // Warehouse was nullchecked so the zip/city values should be present (even if they're null themselves)
                                // But we haven't confirmed the state pull, so we'll nullcheck that.
                                orderDetails.ShipFromState = warehouseAddressDetails.StateProvince?.Abbreviation ?? "";
                            }
                        }
                        //Set the Ship to Address Details
                        // Likewise, shipToAddress hasn't been properly nullchecked so use null conditionals to ensure 
                        // we succeed here.
                        orderDetails.ShipToZip = shipToAddress?.ZipPostalCode ?? "";
                        orderDetails.ShipToCity = shipToAddress?.City ?? "";
                        orderDetails.ShipToState = shipToAddress?.StateProvince?.Abbreviation ?? "";
                        orderDetails.VertexTaxAreaId = shipToAddress.StateProvince.Abbreviation + shipToAddress?.ZipPostalCode;
                    }
                }
                #endregion

                #region MealPlans Donation GiftCard
                if (items.Product.IsMealPlan || items.Product.IsDonation || items.Product.IsGiftCard)
                {
                    // orderdetails should have values that can transitively pass from these fields,
                    // so nullchecking is unnecessary.
                    orderDetails.ShipFromZip = orderDetails.StoreZip;
                    orderDetails.ShipFromCity = orderDetails.StoreCity;
                    orderDetails.ShipFromState = orderDetails.StoreState;

                    orderDetails.ShipToZip = orderDetails.StoreZip;
                    orderDetails.ShipToCity = orderDetails.StoreCity;
                    orderDetails.ShipToState = orderDetails.StoreState;

                    orderDetails.VertexTaxAreaId = orderDetails.ShipToState + orderDetails.ShipToZip;
                }
                #endregion

                #region Non Meal Plan Products
                if (!items.Product.IsMealPlan && !items.Product.IsDonation && !items.Product.IsGiftCard && items.Product.VendorId == 0)
                {
                    if (items.IsDeliveryPickUp || items.Product.IsLocalDelivery || items.Product.IsPickupEnabled) // When the customer has selected Delivery option either DElIVER/IN-STORE PICKUP
                    {
                        if (items.Product.IsLocalDelivery && items.SelectedShippingMethodName == "Delivery") // if the Product has Set Delivery as fulfillment option and has the Warehouse configured to it
                        {
                            var warehouseDetails = _shippingService.GetWarehouseById(items.Product.WarehouseId);
                            if (warehouseDetails != null)
                            {
                                var warehouseAddressDetails = _addressRepository.GetById(warehouseDetails.AddressId);
                                // Warehouse address has NOT been nullchecked, we'll use null conditionals to ensure
                                // that we won't experience a failure here.
                                orderDetails.ShipFromZip = warehouseAddressDetails?.ZipPostalCode ?? "";
                                orderDetails.ShipFromCity = warehouseAddressDetails?.City ?? "";
                                orderDetails.ShipFromState = warehouseAddressDetails?.StateProvince?.Abbreviation ?? "";
                            }
                            else // Pick up the ship from address as the store address
                            {
                                orderDetails.ShipFromZip = orderDetails.StoreZip;
                                orderDetails.ShipFromCity = orderDetails.StoreCity;
                                orderDetails.ShipFromState = orderDetails.StoreState;
                            }
                            //Set the Ship to Address Details
                            orderDetails.ShipToZip = shipToAddress?.ZipPostalCode ?? "";
                            orderDetails.ShipToCity = shipToAddress?.City ?? "";
                            orderDetails.ShipToState = shipToAddress?.StateProvince?.Abbreviation ?? "";
                            orderDetails.VertexTaxAreaId = orderDetails.ShipToState + orderDetails.ShipToZip;

                        }

                        if (items.Product.IsLocalDelivery && string.IsNullOrEmpty(items.SelectedShippingMethodName))
                        {
                            var warehouseDetails = _shippingService.GetWarehouseById(items.Product.WarehouseId);
                            if (warehouseDetails != null)
                            {
                                var warehouseAddressDetails = _addressRepository.GetById(warehouseDetails.AddressId);
                                // Warehouse address has NOT been nullchecked, we'll use null conditionals to ensure
                                // that we won't experience a failure here.
                                orderDetails.ShipFromZip = warehouseAddressDetails?.ZipPostalCode ?? "";
                                orderDetails.ShipFromCity = warehouseAddressDetails?.City ?? "";
                                orderDetails.ShipFromState = warehouseAddressDetails.StateProvince?.Abbreviation ?? "";
                            }
                            else // Pick up the ship from address as the store address
                            {
                                orderDetails.ShipFromZip = orderDetails.StoreZip;
                                orderDetails.ShipFromCity = orderDetails.StoreCity;
                                orderDetails.ShipFromState = orderDetails.StoreState;
                            }
                            orderDetails.ShipToZip = orderDetails.StoreZip;
                            orderDetails.ShipToCity = orderDetails.StoreCity;
                            orderDetails.ShipToState = orderDetails.StoreState;
                            orderDetails.VertexTaxAreaId = orderDetails.ShipToState + orderDetails.ShipToZip;
                        }
                        else if (items.Product.IsPickupEnabled && items.SelectedShippingMethodName == "In-Store Pickup")// If the Product has the set In-Store Pickup as fulfillment option
                        {
                            var warehouseDetails = _shippingService.GetWarehouseById(Convert.ToInt32(items.SelectedFulfillmentWarehouseId));
                            var warehouseAddressDetails = _addressRepository.GetById(warehouseDetails.AddressId);
                            if (warehouseAddressDetails != null)
                            {
                                orderDetails.ShipFromZip = warehouseAddressDetails.ZipPostalCode;
                                orderDetails.ShipFromCity = warehouseAddressDetails.City;
                                // warehouseAddressDetails was nullchecked so we only need to validate
                                // that it pulled a proper state object before attempting to assign abbreviation.
                                orderDetails.ShipFromState = warehouseAddressDetails.StateProvince?.Abbreviation ?? "";

                                orderDetails.ShipToZip = warehouseAddressDetails.ZipPostalCode;
                                orderDetails.ShipToCity = warehouseAddressDetails.City;
                                orderDetails.ShipToState = warehouseAddressDetails.StateProvince?.Abbreviation ?? "";

                                orderDetails.VertexTaxAreaId = orderDetails.ShipToState + orderDetails.ShipToZip;
                            }
                        }

                    }
                    if (items.Product.IsShipEnabled || items.IsShipping)
                    {
                        var warehouseDetails = _shippingService.GetWarehouseById(items.Product.WarehouseId);
                        if (warehouseDetails != null) // Pickup from the warehouse if warehouse is configured else pick from the store address
                        {
                            var warehouseAddressDetails = _addressRepository.GetById(warehouseDetails.AddressId);
                            // null conditional on warehouseAddressDetails and the state object both
                            orderDetails.ShipFromZip = warehouseAddressDetails?.ZipPostalCode ?? "";
                            orderDetails.ShipFromCity = warehouseAddressDetails?.City ?? "";
                            orderDetails.ShipFromState = warehouseAddressDetails?.StateProvince?.Abbreviation ?? "";
                        }
                        else // Pick up the ship from address as the store address
                        {
                            orderDetails.ShipFromZip = orderDetails.StoreZip;
                            orderDetails.ShipFromCity = orderDetails.StoreCity;
                            orderDetails.ShipFromState = orderDetails.StoreState;
                        }
                        //Set the Ship to Address Details
                        orderDetails.ShipToZip = shipToAddress.ZipPostalCode;
                        orderDetails.ShipToCity = shipToAddress.City;
                        orderDetails.ShipToState = shipToAddress.StateProvince.Abbreviation;

                        orderDetails.VertexTaxAreaId = orderDetails.ShipToState + orderDetails.ShipToZip;
                    }

                }
                #endregion

                #region Products With No FullfillmentOption and Shipping Option. Event Tickets
                if (!items.Product.IsMealPlan && !items.Product.IsDonation && !items.Product.IsGiftCard //Always Sets this to store address for Event Tickets
                    && !items.Product.IsLocalDelivery && !items.Product.IsShipEnabled && !items.Product.IsPickupEnabled)
                {
                    orderDetails.ShipFromZip = orderDetails.StoreZip;
                    orderDetails.ShipFromCity = orderDetails.StoreCity;
                    orderDetails.ShipFromState = orderDetails.StoreState;

                    orderDetails.ShipToZip = orderDetails.StoreZip;
                    orderDetails.ShipToCity = orderDetails.StoreCity;
                    orderDetails.ShipToState = orderDetails.StoreState;

                    orderDetails.VertexTaxAreaId = orderDetails.ShipToState + orderDetails.ShipToZip;
                }
                #endregion
            }
            catch (Exception ex)
            {
                throw ex.InnerException;
            }
        }




        public virtual void ApplyUnfulfilledProductsJeGLCodeBusinessRules(OrderItem items, Order order, ProductGlCodeMapping itemsMapping)
        {
            try
            {
                #region Commom Region to set te Details for each items
                itemsMapping.OrderId = order.Id;
                itemsMapping.ProductID = items.Id;
                itemsMapping.GlStatusType = 1;// This is to identify the data in Payment report
                itemsMapping.ProductName = items.Product.Name;
                #endregion

                #region For Normal Unfulfilled Products Set the Glcode to 2040
                if (!items.Product.IsMealPlan && !items.Product.IsDonation && !items.Product.IsGiftCard && items.Product.VendorId == 0)
                {
                    if (items.PriceExclTax != 0)
                    {
                        itemsMapping.Amount = items.PriceExclTax;
                        itemsMapping.Glcode = "48702040";
                        _productService.InsertJeGlCodeAndAmountBusinessRules(itemsMapping);
                    }
                }
                #endregion

                #region For Vendor Products
                if (items.Product.VendorId != 0)
                {
                    if (items.PriceExclTax != 0)
                    {
                        itemsMapping.Amount = items.PriceExclTax;
                        itemsMapping.Glcode = "48702041";
                        _productService.InsertJeGlCodeAndAmountBusinessRules(itemsMapping);
                    }
                }
                #endregion

                #region For MealPlans, Donation,GiftCards
                if (items.Product.IsMealPlan || items.Product.IsDonation || items.Product.IsDownload || items.Product.IsGiftCard)
                {
                    List<ProductGlCode> PaidCodes = _glCodeService.GetProductGlCodes(items.Product, GLCodeStatusType.Paid).ToList();
                    var dis = PaidCodes.GroupBy(x => x.GlCodeId).Select(y => y.First());
                    foreach (var codes in items.Product.TaxCategoryId == -1 ? PaidCodes : dis)
                    {
                        itemsMapping.Glcode = codes.GlCode.GlCodeName;
                        itemsMapping.Amount = items.Product.TaxCategoryId == -1 ? (codes.Amount * items.Quantity) : ((items.Product.Price == decimal.Zero ? items.UnitPriceExclTax : items.Product.Price) * items.Quantity);
                        if (itemsMapping.Amount != decimal.Zero)
                        {
                            _productService.InsertJeGlCodeAndAmountBusinessRules(itemsMapping);
                        }
                    }
                }
                #endregion

                #region For Setting the Delivery GL
                if (items.IsDeliveryPickUp)
                {
                    if (items.DeliveryPickupFee != decimal.Zero)
                    {
                        itemsMapping.Amount = items.DeliveryPickupFee;
                        itemsMapping.Glcode = "48702040";
                        itemsMapping.ProductName = items.Product.Name + "-DeliveryPickupFee";
                        _productService.InsertJeGlCodeAndAmountBusinessRules(itemsMapping);
                    }
                }
                #endregion

                #region For Setting the Shipping GL
                if (items.IsShipping)
                {
                    if (items.ShippingFee != decimal.Zero)
                    {
                        itemsMapping.Amount = items.ShippingFee;
                        itemsMapping.Glcode = "48702040";
                        itemsMapping.ProductName = items.Product.Name + "-ShippingPickupFee";
                        _productService.InsertJeGlCodeAndAmountBusinessRules(itemsMapping);
                    }
                }
                #endregion
            }
            catch (Exception)
            {
                throw;
            }
        }


        public virtual void ApplyJeVertexBusinessRules(OrderItem items, Order order, ProductGlCodeMapping itemsMapping, bool refund = false, List<int> itemproducts = null)
        {
            #region Commom Region to set te Details for each items
            itemsMapping.OrderId = order.Id;
            itemsMapping.ProductID = items.Id;
            itemsMapping.GlStatusType = 1;// This is to identify the data in Payment report
            itemsMapping.ProductName = items.Product.Name;
            #endregion

            List<DynamicVertexGLCode> vertexOrderTaxGls = new List<DynamicVertexGLCode>();

            #region to get the VertexGLs and Insert them in the Mapping table
            if (itemproducts.Contains(items.ProductId))
            {
                vertexOrderTaxGls = _glCodeService.GetVertexGlBreakdown(items.OrderId, items.Product.Id, TaxRequestType.ConfirmDistributiveTax, items.Id); //gets the order gls from vertex response
            }
            else
            {
                vertexOrderTaxGls = _glCodeService.GetVertexGlBreakdown(items.OrderId, items.Product.Id, TaxRequestType.ConfirmDistributiveTax);
            }

            if (vertexOrderTaxGls.Any())
            {
                foreach (var vertexGl in vertexOrderTaxGls)
                {
                    if (vertexGl.Total != decimal.Zero)
                    {
                        itemsMapping.Amount = refund ? -vertexGl.Total : vertexGl.Total;
                        itemsMapping.Glcode = vertexGl.GlCode;
                        itemsMapping.ProductName = (vertexGl.taxCode == "DELV" || vertexGl.taxCode == "SHIP") ? items.Product.Name + "-Delivery/Shipping Tax" : items.Product.Name + "-Product tax";
                        _productService.InsertJeGlCodeAndAmountBusinessRules(itemsMapping);
                    }
                }
            }
            else
            {
                string taxGL = "44571098";
                var tax = items.PriceInclTax - items.PriceExclTax;
                itemsMapping.Amount = refund ? -tax : tax;
                itemsMapping.Glcode = taxGL;
                itemsMapping.ProductName = items.Product.Name + "Tax";
                if (tax != decimal.Zero)
                {
                    _productService.InsertJeGlCodeAndAmountBusinessRules(itemsMapping);
                }
            }
            #endregion
        }

        public virtual void ApplyfulfilledProductsJEGlCodeBusinessRules(OrderItem items, Order order)
        {
            #region Commom Region to set the Details for each items
            ProductGlCodeMapping itemsMapping = new ProductGlCodeMapping();
            itemsMapping.OrderId = order.Id;
            itemsMapping.ProductID = items.Id;
            itemsMapping.GlStatusType = 2;// This is to identify the data in Payment report
            Product product = _productService.GetProductById(items.ProductId);
            itemsMapping.ProductName = product.Name;
            #endregion
            List<ProductGlCode> PaidCodes = _glCodeService.GetProductGlCodes(product, GLCodeStatusType.Paid).ToList();
            var mappingDetails = _orderService.GetDetailsFromProductGlcodeMapping(items.OrderId, items.Id, 2);
            if (!mappingDetails.Any())
            {
                if (product.TaxCategoryId != -1)
                {
                    var dis = PaidCodes.GroupBy(x => x.GlCodeId).Select(y => y.First());

                    foreach (var codes in dis)
                    {
                        if (codes.GlCode.GlCodeName != "62610001" && codes.GlCode.GlCodeName != "70161010")
                        {
                            itemsMapping.Glcode = codes.GlCode.GlCodeName;
                            itemsMapping.Amount = items.PriceExclTax;
                            if (items.PriceExclTax != decimal.Zero)
                            {
                                _productService.InsertJeGlCodeAndAmountBusinessRules(itemsMapping);
                            }
                        }
                        if (items.IsDeliveryPickUp && codes.GlCode.GlCodeName == "70161010")
                        {
                            itemsMapping.Glcode = codes.GlCode.GlCodeName;
                            itemsMapping.Amount = items.DeliveryPickupFee;
                            itemsMapping.ProductName = product.Name + "-DeliveryPickup-Fee";
                            if (itemsMapping.Amount != decimal.Zero)
                            {
                                _productService.InsertJeGlCodeAndAmountBusinessRules(itemsMapping);
                            }
                        }
                        if (items.IsShipping && codes.GlCode.GlCodeName == "62610001")
                        {
                            itemsMapping.Glcode = codes.GlCode.GlCodeName;
                            itemsMapping.Amount = items.ShippingFee;
                            itemsMapping.ProductName = product.Name + "-ShippingPickup-Fee";
                            if (itemsMapping.Amount != decimal.Zero)
                            {
                                _productService.InsertJeGlCodeAndAmountBusinessRules(itemsMapping);
                            }
                        }
                    }
                }
                else
                {
                    foreach (var codes in PaidCodes)
                    {
                        itemsMapping.Glcode = codes.GlCode.GlCodeName;
                        itemsMapping.Amount = codes.Amount;
                        if (itemsMapping.Amount != decimal.Zero)
                        {
                            _productService.InsertJeGlCodeAndAmountBusinessRules(itemsMapping);
                        }
                    }
                }

            }
        }

        public virtual void ApplyJEBusinessRulesFullReFund(int orderId)
        {
            ProductGlCodeMapping itemsMapping = new ProductGlCodeMapping();
            List<ProductGlCodeMapping> mappingDetails = new List<ProductGlCodeMapping>();
            var orderItems = _orderService.GetOrderItemsByOrderID(orderId);
            var order = _orderService.GetOrderById(orderId);

            var listofProd = new List<int>();
            var sameProductcount = order.OrderItems.GroupBy(x => x.ProductId)
                  .Where(g => g.Count() > 1)
                  .Select(y => y.Key)
                  .ToList();
            if (sameProductcount.Any())
            {
                foreach (var datainsameprod in sameProductcount)
                {
                    listofProd.Add(datainsameprod);
                }
            }

            foreach (var items in orderItems)
            {

                if (items.FulfillmentDateTime == null)
                {
                    mappingDetails = _orderService.GetDetailsFromProductGlcodeMapping(items.OrderId, items.Id);
                    foreach (var mappingItems in mappingDetails)
                    {
                        mappingItems.Amount = -mappingItems.Amount;
                        _productService.InsertJeGlCodeAndAmountBusinessRules(mappingItems);
                    }
                }
                else
                {
                    if (items.Product.IsMealPlan || items.Product.IsDonation || items.Product.IsDownload || items.Product.IsGiftCard)
                    {
                        mappingDetails = _orderService.GetDetailsFromProductGlcodeMapping(items.OrderId, items.Id);
                        // ApplyJeVertexBusinessRules(items, order, itemsMapping, true, listofProd);
                    }
                    else
                    {
                        mappingDetails = _orderService.GetDetailsFromProductGlcodeMapping(items.OrderId, items.Id, 2);
                        ApplyJeVertexBusinessRules(items, order, itemsMapping, true, listofProd);
                    }

                    foreach (var mappingItems in mappingDetails)
                    {
                        mappingItems.Amount = -mappingItems.Amount;
                        mappingItems.GlStatusType = 1; // setting this to one so that we can have refunded amount against the revenue Gl once the productis 
                        _productService.InsertJeGlCodeAndAmountBusinessRules(mappingItems);
                    }
                }
                // ApplyJeVertexBusinessRules(items, order, itemsMapping, true);
            }


        }
        #endregion
        /// <summary>
        /// Deletes an order
        /// </summary>
        /// <param name="order">The order</param>
        public virtual void DeleteOrder(Order order)
        {
            if (order == null)
            {
                throw new ArgumentNullException("order");
            }

            //check whether the order wasn't cancelled before
            //if it already was cancelled, then there's no need to make the following adjustments
            //(such as reward points, inventory, recurring payments)
            //they already was done when cancelling the order
            if (order.OrderStatus != OrderStatus.Cancelled)
            {
                //return (add) back redeemded reward points
                ReturnBackRedeemedRewardPoints(order);
                //reduce (cancel) back reward points (previously awarded for this order)
                ReduceRewardPoints(order);

                //cancel recurring payments
                var recurringPayments = _orderService.SearchRecurringPayments(initialOrderId: order.Id);
                foreach (var rp in recurringPayments)
                {
                    var errors = CancelRecurringPayment(rp);
                    //use "errors" variable?
                }

                //Adjust inventory for already shipped shipments
                //only products with "use multiple warehouses"
                foreach (var shipment in order.Shipments)
                {
                    foreach (var shipmentItem in shipment.ShipmentItems)
                    {
                        var orderItem = _orderService.GetOrderItemById(shipmentItem.OrderItemId);
                        if (orderItem == null)
                        {
                            continue;
                        }

                        _productService.ReverseBookedInventory(orderItem.Product, shipmentItem);
                    }
                }
                //Adjust inventory
                foreach (var orderItem in order.OrderItems)
                {
                    _productService.AdjustInventory(orderItem.Product, orderItem.Quantity, orderItem.AttributesXml);
                }

            }

            //add a note
            order.OrderNotes.Add(new OrderNote
            {
                Note = "Order has been deleted",
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow
            });
            _orderService.UpdateOrder(order);

            //now delete an order
            _orderService.DeleteOrder(order);
        }

        /// <summary>
        /// Process next recurring payment
        /// </summary>
        /// <param name="recurringPayment">Recurring payment</param>
        /// <param name="paymentResult">Process payment result (info about last payment for automatic recurring payments)</param>
        public virtual void ProcessNextRecurringPayment(RecurringPayment recurringPayment, ProcessPaymentResult paymentResult = null)
        {
            if (recurringPayment == null)
            {
                throw new ArgumentNullException("recurringPayment");
            }

            try
            {
                if (!recurringPayment.IsActive)
                {
                    throw new NopException("Recurring payment is not active");
                }

                var initialOrder = recurringPayment.InitialOrder;
                if (initialOrder == null)
                {
                    throw new NopException("Initial order could not be loaded");
                }

                var customer = initialOrder.Customer;
                if (customer == null)
                {
                    throw new NopException("Customer could not be loaded");
                }

                var nextPaymentDate = recurringPayment.NextPaymentDate;
                if (!nextPaymentDate.HasValue)
                {
                    throw new NopException("Next payment date could not be calculated");
                }

                //payment info
                var processPaymentRequest = new ProcessPaymentRequest
                {
                    StoreId = initialOrder.StoreId,
                    CustomerId = customer.Id,
                    OrderGuid = Guid.NewGuid(),
                    InitialOrderId = initialOrder.Id,
                    RecurringCycleLength = recurringPayment.CycleLength,
                    RecurringCyclePeriod = recurringPayment.CyclePeriod,
                    RecurringTotalCycles = recurringPayment.TotalCycles,
                };

                //prepare order details
                var details = PrepareRecurringOrderDetails(processPaymentRequest);

                ProcessPaymentResult processPaymentResult;
                //skip payment workflow if order total equals zero
                var skipPaymentWorkflow = details.OrderTotal == decimal.Zero;
                if (!skipPaymentWorkflow)
                {
                    var paymentMethod = _paymentService.LoadPaymentMethodBySystemName(processPaymentRequest.PaymentMethodSystemName);
                    if (paymentMethod == null)
                    {
                        throw new NopException("Payment method couldn't be loaded");
                    }

                    if (!paymentMethod.IsPaymentMethodActive(_paymentSettings))
                    {
                        throw new NopException("Payment method is not active");
                    }

                    //Old credit card info
                    if (details.InitialOrder.AllowStoringCreditCardNumber)
                    {
                        processPaymentRequest.CreditCardType = _encryptionService.DecryptText(details.InitialOrder.CardType);
                        processPaymentRequest.CreditCardName = _encryptionService.DecryptText(details.InitialOrder.CardName);
                        processPaymentRequest.CreditCardNumber = _encryptionService.DecryptText(details.InitialOrder.CardNumber);
                        processPaymentRequest.CreditCardCvv2 = _encryptionService.DecryptText(details.InitialOrder.CardCvv2);
                        try
                        {
                            processPaymentRequest.CreditCardExpireMonth = Convert.ToInt32(_encryptionService.DecryptText(details.InitialOrder.CardExpirationMonth));
                            processPaymentRequest.CreditCardExpireYear = Convert.ToInt32(_encryptionService.DecryptText(details.InitialOrder.CardExpirationYear));
                        }
                        catch { }
                    }

                    //payment type
                    switch (_paymentService.GetRecurringPaymentType(processPaymentRequest.PaymentMethodSystemName))
                    {
                        case RecurringPaymentType.NotSupported:
                            throw new NopException("Recurring payments are not supported by selected payment method");
                        case RecurringPaymentType.Manual:
                            processPaymentResult = _paymentService.ProcessRecurringPayment(processPaymentRequest);
                            break;
                        case RecurringPaymentType.Automatic:
                            //payment is processed on payment gateway site, info about last transaction in paymentResult parameter
                            processPaymentResult = paymentResult ?? new ProcessPaymentResult();
                            break;
                        default:
                            throw new NopException("Not supported recurring payment type");
                    }
                }
                else
                {
                    processPaymentResult = paymentResult ?? new ProcessPaymentResult { NewPaymentStatus = PaymentStatus.Paid };
                }

                if (processPaymentResult == null)
                {
                    throw new NopException("processPaymentResult is not available");
                }

                if (processPaymentResult.Success)
                {
                    //save order details
                    var order = SaveOrderDetails(processPaymentRequest, processPaymentResult, details);

                    foreach (var orderItem in details.InitialOrder.OrderItems)
                    {
                        //save item
                        var newOrderItem = new OrderItem
                        {
                            OrderItemGuid = Guid.NewGuid(),
                            Order = order,
                            ProductId = orderItem.ProductId,
                            UnitPriceInclTax = orderItem.UnitPriceInclTax,
                            UnitPriceExclTax = orderItem.UnitPriceExclTax,
                            PriceInclTax = orderItem.PriceInclTax,
                            PriceExclTax = orderItem.PriceExclTax,
                            OriginalProductCost = orderItem.OriginalProductCost,
                            AttributeDescription = orderItem.AttributeDescription,
                            AttributesXml = orderItem.AttributesXml,
                            Quantity = orderItem.Quantity,
                            DiscountAmountInclTax = orderItem.DiscountAmountInclTax,
                            DiscountAmountExclTax = orderItem.DiscountAmountExclTax,
                            DownloadCount = 0,
                            IsDownloadActivated = false,
                            LicenseDownloadId = 0,
                            ItemWeight = orderItem.ItemWeight,
                            RentalStartDateUtc = orderItem.RentalStartDateUtc,
                            RentalEndDateUtc = orderItem.RentalEndDateUtc,
                            RequestedFulfillmentDateTime = orderItem.RequestedFulfillmentDateTime, /// NU-13
                            SelectedFulfillmentWarehouseId = orderItem.SelectedFulfillmentWarehouseId, /// NU-13
                            SelectedShippingMethodName = orderItem.SelectedShippingMethodName, /// NU-13     
                            DeliveryAmountExclTax = orderItem.DeliveryAmountExclTax //NU-90
                        };
                        #region NU-13
                        if (newOrderItem.SelectedFulfillmentWarehouseId != null)
                        {
                            newOrderItem.SelectedFulfillmentWarehouse = _shippingService.GetWarehouseById(orderItem.SelectedFulfillmentWarehouseId.GetValueOrDefault(-1));
                        }
                        #endregion
                        order.OrderItems.Add(newOrderItem);
                        _orderService.UpdateOrder(order);

                        //gift cards
                        if (orderItem.Product.IsGiftCard)
                        {
                            string giftCardRecipientName;
                            string giftCardRecipientEmail;
                            string giftCardSenderName;
                            string giftCardSenderEmail;
                            string giftCardMessage;

                            _productAttributeParser.GetGiftCardAttribute(orderItem.AttributesXml, out giftCardRecipientName,
                                out giftCardRecipientEmail, out giftCardSenderName, out giftCardSenderEmail, out giftCardMessage);

                            for (var i = 0; i < orderItem.Quantity; i++)
                            {
                                _giftCardService.InsertGiftCard(new GiftCard
                                {
                                    GiftCardType = orderItem.Product.GiftCardType,
                                    PurchasedWithOrderItem = newOrderItem,
                                    Amount = orderItem.UnitPriceExclTax,
                                    IsGiftCardActivated = false,
                                    GiftCardCouponCode = _giftCardService.GenerateGiftCardCode(),
                                    RecipientName = giftCardRecipientName,
                                    RecipientEmail = giftCardRecipientEmail,
                                    SenderName = giftCardSenderName,
                                    SenderEmail = giftCardSenderEmail,
                                    Message = giftCardMessage,
                                    IsRecipientNotified = false,
                                    CreatedOnUtc = DateTime.UtcNow
                                });
                            }
                        }

                        #region NU-16
                        if (orderItem.Product.IsMealPlan)
                        {
                            string mealPlanRecipientAcctNum = "", mealPlanRecipientAddress = "", mealPlanRecipientEmail = "", mealPlanRecipientName = "", mealPlanRecipientPhone = "";
                            if (orderItem.Product.ShowStandardMealPlanFields)
                            {

                                _productAttributeParser.GetMealPlanAttribute(
                                       orderItem.AttributesXml,
                                       out mealPlanRecipientAcctNum,
                                       out mealPlanRecipientAddress,
                                       out mealPlanRecipientEmail,
                                       out mealPlanRecipientName,
                                       out mealPlanRecipientPhone
                                   );
                            }
                            else
                            {

                                mealPlanRecipientName = order.BillingAddress.FirstName + " " + order.BillingAddress.LastName;
                                mealPlanRecipientEmail = order.BillingAddress.Email;
                                mealPlanRecipientPhone = order.BillingAddress.PhoneNumber;
                                mealPlanRecipientAddress = order.BillingAddress.Address1 + " " + order.BillingAddress.Address2 + " " + order.BillingAddress.City + ", " + order.BillingAddress.StateProvince + " " + order.BillingAddress.ZipPostalCode;



                                if (_workContext.CurrentCustomer.GetAttribute<String>("IsSSOUser", processPaymentRequest.StoreId) == "true")
                                {
                                    mealPlanRecipientAcctNum = _workContext.CurrentCustomer.GetAttribute<String>("fsueduemplid", processPaymentRequest.StoreId);
                                }
                                else //TODO:  NEed to tie the generic product attribute to the mealplan somehow
                                {

                                    string values = _productAttributeFormatter.FormatAttributes(orderItem.Product, orderItem.AttributesXml);
                                    if (values.Contains("Account ID Number: "))
                                    {
                                        mealPlanRecipientAcctNum = values.Split(new[] { "Account ID Number: " }, StringSplitOptions.None)[1];
                                    }
                                    else
                                    {
                                        mealPlanRecipientAcctNum = "NOT FOUND";
                                    }
                                }


                            }
                            for (int i = 0; i < orderItem.Quantity; i++)
                            {
                                var mealPlan = new MealPlan
                                {
                                    PurchasedWithOrderItemId = orderItem.Id,
                                    RecipientName = mealPlanRecipientName,
                                    RecipientAddress = mealPlanRecipientAddress,
                                    RecipientPhone = mealPlanRecipientPhone,
                                    RecipientEmail = mealPlanRecipientEmail,
                                    RecipientAcctNum = mealPlanRecipientAcctNum,
                                    CreatedOnUtc = DateTime.UtcNow,
                                    //  BlackBoardCustomerNumber = Convert.ToInt64(mealPlanRecipientAcctNum == "NOT FOUND" || mealPlanRecipientAcctNum == "" ? "" : Convert.ToString(_customerService.GetBlackBoardCustomerNumber(int.Parse(mealPlanRecipientAcctNum))))

                                };
                                _mealPlanService.InsertMealPlan(mealPlan);
                                orderItem.FulfillmentDateTime = DateTime.UtcNow;  //Meal Plan auto fufilled
                            }
                        }
                        #endregion

                        #region NU-17
                        if (orderItem.Product.IsDonation)
                        {


                            string donorFirstName;
                            string donorLastName;
                            string donorAddress;
                            string donorAddress2;
                            string donorCity;
                            string donorState;
                            string donorZip;
                            string donorPhone;
                            string donorCountry;
                            string comments;
                            string notificationEmail;
                            string onBehalfOfFullName;
                            string donorCompany;
                            bool includeGiftAmount;


                            _productAttributeParser.GetDonationAttribute(
                                    orderItem.AttributesXml,
                                    out donorFirstName, out donorLastName,
                                                     out donorAddress, out donorAddress2, out donorCity,
                                                     out donorState, out donorZip, out donorPhone,
                                                     out donorCountry, out comments, out notificationEmail,
                                                     out onBehalfOfFullName, out includeGiftAmount, out donorCompany
                                );

                            for (int i = 0; i < orderItem.Quantity; i++)
                            {
                                var donation = new Donation
                                {
                                    PurchasedWithOrderItemId = orderItem.Id,
                                    //PurchasedWithOrderItem =  orderItem,
                                    DonorFirstName = donorFirstName,
                                    DonorLastName = donorLastName,
                                    DonorCompany = donorCompany,
                                    DonorAddress = donorAddress,
                                    DonorAddress2 = donorAddress2,
                                    DonorCity = donorCity,
                                    DonorState = donorState,
                                    DonorZip = donorZip,
                                    DonorPhone = donorPhone,
                                    DonorCountry = donorCountry,
                                    Comments = comments,
                                    NotificationEmail = notificationEmail,
                                    OnBehalfOfFullName = onBehalfOfFullName,
                                    IncludeGiftAmount = includeGiftAmount,
                                    CreatedOnUtc = DateTime.UtcNow,
                                    Amount = newOrderItem.UnitPriceExclTax
                                };
                                _donationService.InsertDonation(donation);
                                orderItem.FulfillmentDateTime = DateTime.UtcNow; //Donation Auto Fulfilled
                            }
                        }
                        #endregion

                        //inventory
                        _productService.AdjustInventory(orderItem.Product, -orderItem.Quantity, orderItem.AttributesXml);
                    }

                    //notifications
                    SendNotificationsAndSaveNotes(order);

                    //check order status
                    CheckOrderStatus(order);

                    //raise event       
                    _eventPublisher.Publish(new OrderPlacedEvent(order));

                    if (order.PaymentStatus == PaymentStatus.Paid)
                    {
                        ProcessOrderPaid(order);
                    }

                    //next recurring payment
                    recurringPayment.RecurringPaymentHistory.Add(new RecurringPaymentHistory
                    {
                        RecurringPayment = recurringPayment,
                        CreatedOnUtc = DateTime.UtcNow,
                        OrderId = order.Id,
                    });
                    _orderService.UpdateRecurringPayment(recurringPayment);
                }
                else
                {
                    //log errors
                    var logError = processPaymentResult.Errors.Aggregate("Error while processing recurring order. ",
                        (current, next) => string.Format("{0}Error {1}: {2}. ", current, processPaymentResult.Errors.IndexOf(next) + 1, next));
                    _logger.Error(logError, customer: customer);
                }
            }
            catch (Exception exc)
            {
                _logger.Error(string.Format("Error while processing recurring order. {0}", exc.Message), exc);
                throw;
            }
        }

        /// <summary>
        /// Cancels a recurring payment
        /// </summary>
        /// <param name="recurringPayment">Recurring payment</param>
        public virtual IList<string> CancelRecurringPayment(RecurringPayment recurringPayment)
        {
            if (recurringPayment == null)
            {
                throw new ArgumentNullException("recurringPayment");
            }

            var initialOrder = recurringPayment.InitialOrder;
            if (initialOrder == null)
            {
                return new List<string> { "Initial order could not be loaded" };
            }

            var request = new CancelRecurringPaymentRequest();
            CancelRecurringPaymentResult result = null;
            try
            {
                request.Order = initialOrder;
                result = _paymentService.CancelRecurringPayment(request);
                if (result.Success)
                {
                    //update recurring payment
                    recurringPayment.IsActive = false;
                    _orderService.UpdateRecurringPayment(recurringPayment);


                    //add a note
                    initialOrder.OrderNotes.Add(new OrderNote
                    {
                        Note = "Recurring payment has been cancelled",
                        DisplayToCustomer = false,
                        CreatedOnUtc = DateTime.UtcNow
                    });
                    _orderService.UpdateOrder(initialOrder);

                    //notify a store owner
                    _workflowMessageService
                        .SendRecurringPaymentCancelledStoreOwnerNotification(recurringPayment,
                        _localizationSettings.DefaultAdminLanguageId);
                }
            }
            catch (Exception exc)
            {
                if (result == null)
                {
                    result = new CancelRecurringPaymentResult();
                }

                result.AddError(string.Format("Error: {0}. Full exception: {1}", exc.Message, exc.ToString()));
            }


            //process errors
            string error = "";
            for (int i = 0; i < result.Errors.Count; i++)
            {
                error += string.Format("Error {0}: {1}", i, result.Errors[i]);
                if (i != result.Errors.Count - 1)
                {
                    error += ". ";
                }
            }
            if (!String.IsNullOrEmpty(error))
            {
                //add a note
                initialOrder.OrderNotes.Add(new OrderNote
                {
                    Note = string.Format("Unable to cancel recurring payment. {0}", error),
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow
                });
                _orderService.UpdateOrder(initialOrder);

                //log it
                string logError = string.Format("Error cancelling recurring payment. Order #{0}. Error: {1}", initialOrder.Id, error);
                _logger.InsertLog(LogLevel.Error, logError, logError);
            }
            return result.Errors;
        }

        /// <summary>
        /// Gets a value indicating whether a customer can cancel recurring payment
        /// </summary>
        /// <param name="customerToValidate">Customer</param>
        /// <param name="recurringPayment">Recurring Payment</param>
        /// <returns>value indicating whether a customer can cancel recurring payment</returns>
        public virtual bool CanCancelRecurringPayment(Customer customerToValidate, RecurringPayment recurringPayment)
        {
            if (recurringPayment == null)
            {
                return false;
            }

            if (customerToValidate == null)
            {
                return false;
            }

            var initialOrder = recurringPayment.InitialOrder;
            if (initialOrder == null)
            {
                return false;
            }

            var customer = recurringPayment.InitialOrder.Customer;
            if (customer == null)
            {
                return false;
            }

            if (initialOrder.OrderStatus == OrderStatus.Cancelled)
            {
                return false;
            }

            if (!customerToValidate.IsAdmin())
            {
                if (customer.Id != customerToValidate.Id)
                {
                    return false;
                }
            }

            if (!recurringPayment.NextPaymentDate.HasValue)
            {
                return false;
            }

            return true;
        }



        /// <summary>
        /// Send a shipment
        /// </summary>
        /// <param name="shipment">Shipment</param>
        /// <param name="notifyCustomer">True to notify customer</param>
        public virtual void Ship(Shipment shipment, bool notifyCustomer,/*---START: Codechages done by (na-sdxcorp\ADas)--------------*/ bool notifyStoreAdmins = false, DateTime? shipDate = null /*---END: Codechages done by (na-sdxcorp\ADas)--------------*/)
        {
            if (shipment == null)
            {
                throw new ArgumentNullException("shipment");
            }

            var order = _orderService.GetOrderById(shipment.OrderId);
            if (order == null)
            {
                throw new Exception("Order cannot be loaded");
            }

            if (shipment.ShippedDateUtc.HasValue)
            {
                throw new Exception("This shipment is already shipped");
            }

            //---START: Codechages done by (na-sdxcorp\ADas)------Commented out--------
            //shipment.ShippedDateUtc = DateTime.UtcNow;
            //---END: Codechages done by (na-sdxcorp\ADas)--------------

            //---START: Codechages done by (na-sdxcorp\ADas)--------------
            shipment.ShippedDateUtc = shipDate != null ? shipDate : DateTime.UtcNow;
            //---END: Codechages done by (na-sdxcorp\ADas)--------------

            _shipmentService.UpdateShipment(shipment);

            //process products with "Multiple warehouse" support enabled
            foreach (var item in shipment.ShipmentItems)
            {
                var orderItem = _orderService.GetOrderItemById(item.OrderItemId);
                _productService.BookReservedInventory(orderItem.Product, item.WarehouseId, -item.Quantity);
            }

            //check whether we have more items to ship
            if (order.HasItemsToAddToShipment() || order.HasItemsToShip())
            {
                order.ShippingStatusId = (int)ShippingStatus.PartiallyShipped;
            }
            else
            {
                order.ShippingStatusId = (int)ShippingStatus.Shipped;
            }

            _orderService.UpdateOrder(order);

            //add a note
            order.OrderNotes.Add(new OrderNote
            {
                Note = string.Format("Shipment# {0} has been sent", shipment.Id),
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow
            });
            _orderService.UpdateOrder(order);

            if (notifyCustomer)
            {
                //notify customer
                int queuedEmailId = _workflowMessageService.SendShipmentSentCustomerNotification(shipment, order.CustomerLanguageId);
                if (queuedEmailId > 0)
                {
                    order.OrderNotes.Add(new OrderNote
                    {
                        Note = string.Format("\"Shipped\" email (to customer) has been queued. Queued email identifier: {0}.", queuedEmailId),
                        DisplayToCustomer = false,
                        CreatedOnUtc = DateTime.UtcNow
                    });
                    _orderService.UpdateOrder(order);
                }
            }

            //---START: Codechages done by (na-sdxcorp\ADas)--------------
            //---send email notification to store admin
            if (notifyStoreAdmins)
            {
                //notify store owner
                int queuedEmailId = _workflowMessageService.SendShipmentSentStoreOwnerNotification(shipment, order.CustomerLanguageId);
                if (queuedEmailId > 0)
                {
                    order.OrderNotes.Add(new OrderNote
                    {
                        Note = string.Format("\"Shipped\" email (to store owner) has been queued. Queued email identifier: {0}.", queuedEmailId),
                        DisplayToCustomer = false,
                        CreatedOnUtc = DateTime.UtcNow
                    });
                    _orderService.UpdateOrder(order);
                }
            }
            //---END: Codechages done by (na-sdxcorp\ADas)--------------

            //event
            _eventPublisher.PublishShipmentSent(shipment);

            //check order status
            CheckOrderStatus(order);
        }

        /// <summary>
        /// Marks a shipment as delivered
        /// </summary>
        /// <param name="shipment">Shipment</param>
        /// <param name="notifyCustomer">True to notify customer</param>
        public virtual void Deliver(Shipment shipment, bool notifyCustomer,/*---START: Codechages done by (na-sdxcorp\ADas)--------------*/ bool notifyStoreAdmins = false, DateTime? deliveryDate = null  /*---END: Codechages done by (na-sdxcorp\ADas)--------------*/)
        {
            if (shipment == null)
            {
                throw new ArgumentNullException("shipment");
            }

            var order = shipment.Order;
            if (order == null)
            {
                throw new Exception("Order cannot be loaded");
            }

            if (!shipment.ShippedDateUtc.HasValue)
            {
                throw new Exception("This shipment is not shipped yet");
            }

            if (shipment.DeliveryDateUtc.HasValue)
            {
                throw new Exception("This shipment is already delivered");
            }

            //---START: Codechages done by (na-sdxcorp\ADas)----Commented out----------
            //shipment.DeliveryDateUtc = DateTime.UtcNow;
            //---END: Codechages done by (na-sdxcorp\ADas)--------------

            //---START: Codechages done by (na-sdxcorp\ADas)--------------
            shipment.DeliveryDateUtc = deliveryDate != null ? deliveryDate : DateTime.UtcNow;

            foreach (var item in shipment.ShipmentItems)
            {

                var orderItem = _orderService.GetOrderItemById(item.OrderItemId);
                this.Fulfill(orderItem, false);
            }
            //---END: Codechages done by (na-sdxcorp\ADas)--------------

            _shipmentService.UpdateShipment(shipment);

            if (!order.HasItemsToAddToShipment() && !order.HasItemsToShip() && !order.HasItemsToDeliver())
            {
                order.ShippingStatusId = (int)ShippingStatus.Delivered;
            }

            _orderService.UpdateOrder(order);

            //add a note
            order.OrderNotes.Add(new OrderNote
            {
                Note = string.Format("Shipment# {0} has been delivered", shipment.Id),
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow
            });
            _orderService.UpdateOrder(order);

            if (notifyCustomer)
            {
                //send email notification
                int queuedEmailId = _workflowMessageService.SendShipmentDeliveredCustomerNotification(shipment, order.CustomerLanguageId);
                if (queuedEmailId > 0)
                {
                    order.OrderNotes.Add(new OrderNote
                    {
                        Note = string.Format("\"Delivered\" email (to customer) has been queued. Queued email identifier: {0}.", queuedEmailId),
                        DisplayToCustomer = false,
                        CreatedOnUtc = DateTime.UtcNow
                    });
                    _orderService.UpdateOrder(order);
                }
            }

            //---START: Codechages done by (na-sdxcorp\ADas)--------------
            //---send email notification to store admin
            if (notifyStoreAdmins)
            {
                //notify store owner
                int queuedEmailId = _workflowMessageService.SendShipmentDeliveredStoreOwnerNotification(shipment, order.CustomerLanguageId);
                if (queuedEmailId > 0)
                {
                    order.OrderNotes.Add(new OrderNote
                    {
                        Note = string.Format("\"Delivered\" email (to customer) has been queued. Queued email identifier: {0}.", queuedEmailId),
                        DisplayToCustomer = false,
                        CreatedOnUtc = DateTime.UtcNow
                    });
                    _orderService.UpdateOrder(order);
                }
            }
            //---END: Codechages done by (na-sdxcorp\ADas)--------------

            //event
            _eventPublisher.PublishShipmentDelivered(shipment);

            //check order status
            CheckOrderStatus(order);
        }
        #region SODMYWAY-
        /// <summary>
        ///  For local fulfillment, sets the date of the fulfillment and sends notification.
        /// </summary>
        /// <param name="orderSiteProduct"></param>
        /// <param name="notifyCustomer"></param>
        public virtual void Fulfill(OrderItem orderItem, bool notifyCustomer)
        {
            if (orderItem == null)
            {
                throw new ArgumentNullException("orderItem");
            }


            Order order = orderItem.Order;

            orderItem.FulfillmentDateTime = DateTime.UtcNow;
            if (order.OrderItems.Count == 1)
            {
                order.ShippingStatusId = (int)ShippingStatus.Delivered;
                InsertIntoSAPSalesJournal(order, 2);
            }
            else
            {
                //Check to see if other order site products need to be delivered.

                bool completelyFulfilled = true;
                foreach (var osp in order.OrderItems)
                {
                    if (osp.FulfillmentDateTime == null)
                    {
                        completelyFulfilled = false;

                    }
                }
                if (!completelyFulfilled)
                {
                    order.ShippingStatusId = (int)ShippingStatus.PartiallyShipped;
                }
                else
                {
                    order.ShippingStatusId = (int)ShippingStatus.Delivered;
                    InsertIntoSAPSalesJournal(order, 2);
                    //SaveIntoSapJournal(order, 2); //need to reverse deferred and go into revenue.
                }

            }

            //add a note
            order.OrderNotes.Add(
                new OrderNote { Note = "Product has been fulfilled", DisplayToCustomer = false, CreatedOnUtc = DateTime.UtcNow });
            _orderService.UpdateOrder(order);

            if (notifyCustomer)
            {
                //send email notification
                int queuedEmailId = _workflowMessageService.SendFulfillementCustomerNotification(orderItem, order.CustomerLanguageId);
                if (queuedEmailId > 0)
                {
                    order.OrderNotes.Add(new OrderNote
                    {
                        Note = string.Format("\"Fulfillement\" email (to customer) has been queued. Queued email identifier: {0}.", queuedEmailId),
                        DisplayToCustomer = false,
                        CreatedOnUtc = DateTime.UtcNow
                    });

                }
            }
            _orderService.UpdateOrder(order);

            //check order status
            CheckOrderStatus(order);
        }

        #endregion


        /// <summary>
        /// Gets a value indicating whether cancel is allowed
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>A value indicating whether cancel is allowed</returns>
        public virtual bool CanCancelOrder(Order order)
        {
            if (order == null)
            {
                throw new ArgumentNullException("order");
            }

            if (order.OrderStatus == OrderStatus.Cancelled)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Cancels order
        /// </summary>
        /// <param name="order">Order</param>
        /// <param name="notifyCustomer">True to notify customer</param>
        public virtual void CancelOrder(Order order, bool notifyCustomer)
        {
            if (order == null)
            {
                throw new ArgumentNullException("order");
            }

            //if (!CanCancelOrder(order))
            //    throw new NopException("Cannot do cancel for order.");

            //Cancel order
            SetOrderStatus(order, OrderStatus.Cancelled, notifyCustomer);

            //add a note
            order.OrderNotes.Add(new OrderNote
            {
                Note = "Order has been cancelled",
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow
            });
            _orderService.UpdateOrder(order);

            //return (add) back redeemded reward points
            ReturnBackRedeemedRewardPoints(order);

            //cancel recurring payments
            var recurringPayments = _orderService.SearchRecurringPayments(initialOrderId: order.Id);
            foreach (var rp in recurringPayments)
            {
                var errors = CancelRecurringPayment(rp);
                //use "errors" variable?
            }

            //Adjust inventory for already shipped shipments
            //only products with "use multiple warehouses"
            foreach (var shipment in order.Shipments)
            {
                foreach (var shipmentItem in shipment.ShipmentItems)
                {
                    var orderItem = _orderService.GetOrderItemById(shipmentItem.OrderItemId);
                    if (orderItem == null)
                    {
                        continue;
                    }

                    _productService.ReverseBookedInventory(orderItem.Product, shipmentItem);
                }
            }
            //Adjust inventory
            foreach (var orderItem in order.OrderItems)
            {
                _productService.AdjustInventory(orderItem.Product, orderItem.Quantity, orderItem.AttributesXml);
            }
            //InsertIntoSAPSalesJournal(order, 3);
            //SaveIntoSapJournal(order, 3);

            _eventPublisher.Publish(new OrderCancelledEvent(order));

        }

        /// <summary>
        /// Gets a value indicating whether order can be marked as authorized
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>A value indicating whether order can be marked as authorized</returns>
        public virtual bool CanMarkOrderAsAuthorized(Order order)
        {
            if (order == null)
            {
                throw new ArgumentNullException("order");
            }

            if (order.OrderStatus == OrderStatus.Cancelled)
            {
                return false;
            }

            if (order.PaymentStatus == PaymentStatus.Pending)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Marks order as authorized
        /// </summary>
        /// <param name="order">Order</param>
        public virtual void MarkAsAuthorized(Order order)
        {
            if (order == null)
            {
                throw new ArgumentNullException("order");
            }

            order.PaymentStatusId = (int)PaymentStatus.Authorized;
            _orderService.UpdateOrder(order);

            //add a note
            order.OrderNotes.Add(new OrderNote
            {
                Note = "Order has been marked as authorized",
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow
            });
            _orderService.UpdateOrder(order);

            //check order status
            CheckOrderStatus(order);
        }



        /// <summary>
        /// Gets a value indicating whether capture from admin panel is allowed
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>A value indicating whether capture from admin panel is allowed</returns>
        public virtual bool CanCapture(Order order)
        {
            if (order == null)
            {
                throw new ArgumentNullException("order");
            }

            if (order.OrderStatus == OrderStatus.Cancelled ||
                order.OrderStatus == OrderStatus.Pending)
            {
                return false;
            }

            if (order.PaymentStatus == PaymentStatus.Authorized &&
                _paymentService.SupportCapture(order.PaymentMethodSystemName))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Capture an order (from admin panel)
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>A list of errors; empty list if no errors</returns>
        public virtual IList<string> Capture(Order order)
        {
            if (order == null)
            {
                throw new ArgumentNullException("order");
            }

            if (!CanCapture(order))
            {
                throw new NopException("Cannot do capture for order.");
            }

            var request = new CapturePaymentRequest();
            CapturePaymentResult result = null;
            try
            {
                //old info from placing order
                request.Order = order;
                result = _paymentService.Capture(request);

                if (result.Success)
                {
                    var paidDate = order.PaidDateUtc;
                    if (result.NewPaymentStatus == PaymentStatus.Paid)
                    {
                        paidDate = DateTime.UtcNow;
                    }

                    order.CaptureTransactionId = result.CaptureTransactionId;
                    order.CaptureTransactionResult = result.CaptureTransactionResult;
                    order.PaymentStatus = result.NewPaymentStatus;
                    order.PaidDateUtc = paidDate;
                    _orderService.UpdateOrder(order);

                    //add a note
                    order.OrderNotes.Add(new OrderNote
                    {
                        Note = "Order has been captured",
                        DisplayToCustomer = false,
                        CreatedOnUtc = DateTime.UtcNow
                    });
                    _orderService.UpdateOrder(order);

                    CheckOrderStatus(order);

                    if (order.PaymentStatus == PaymentStatus.Paid)
                    {
                        ProcessOrderPaid(order);
                    }
                }
            }
            catch (Exception exc)
            {
                if (result == null)
                {
                    result = new CapturePaymentResult();
                }

                result.AddError(string.Format("Error: {0}. Full exception: {1}", exc.Message, exc.ToString()));
            }


            //process errors
            string error = "";
            for (int i = 0; i < result.Errors.Count; i++)
            {
                error += string.Format("Error {0}: {1}", i, result.Errors[i]);
                if (i != result.Errors.Count - 1)
                {
                    error += ". ";
                }
            }
            if (!String.IsNullOrEmpty(error))
            {
                //add a note
                order.OrderNotes.Add(new OrderNote
                {
                    Note = string.Format("Unable to capture order. {0}", error),
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow
                });
                _orderService.UpdateOrder(order);

                //log it
                string logError = string.Format("Error capturing order #{0}. Error: {1}", order.Id, error);
                _logger.InsertLog(LogLevel.Error, logError, logError);
            }
            return result.Errors;
        }

        /// <summary>
        /// Gets a value indicating whether order can be marked as paid
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>A value indicating whether order can be marked as paid</returns>
        public virtual bool CanMarkOrderAsPaid(Order order)
        {
            if (order == null)
            {
                throw new ArgumentNullException("order");
            }

            if (order.OrderStatus == OrderStatus.Cancelled)
            {
                return false;
            }

            if (order.PaymentStatus == PaymentStatus.Paid ||
                order.PaymentStatus == PaymentStatus.Refunded ||
                order.PaymentStatus == PaymentStatus.Voided)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Marks order as paid
        /// </summary>
        /// <param name="order">Order</param>
        public virtual void MarkOrderAsPaid(Order order)
        {
            if (order == null)
            {
                throw new ArgumentNullException("order");
            }

            if (!CanMarkOrderAsPaid(order))
            {
                throw new NopException("You can't mark this order as paid");
            }

            order.PaymentStatusId = (int)PaymentStatus.Paid;
            order.PaidDateUtc = DateTime.UtcNow;
            _orderService.UpdateOrder(order);

            //add a note
            order.OrderNotes.Add(new OrderNote
            {
                Note = "Order has been marked as paid",
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow
            });
            _orderService.UpdateOrder(order);

            CheckOrderStatus(order);

            if (order.PaymentStatus == PaymentStatus.Paid)
            {
                ProcessOrderPaid(order);
            }
        }



        /// <summary>
        /// Gets a value indicating whether refund from admin panel is allowed
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>A value indicating whether refund from admin panel is allowed</returns>
        public virtual bool CanRefund(Order order)
        {
            if (order == null)
            {
                throw new ArgumentNullException("order");
            }

            if (order.OrderTotal == decimal.Zero)
            {
                return false;
            }

            //uncomment the lines below in order to allow this operation for cancelled orders
            //if (order.OrderStatus == OrderStatus.Cancelled)
            //    return false;


            //Trove Enables Stores - Can Refund .
            string troveStoreIds = "";
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
                        return true;
                    }
                }
            }

            if (order.PaymentStatus == PaymentStatus.Paid &&
                _paymentService.SupportRefund(order.PaymentMethodSystemName))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Refunds an order (from admin panel)
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>A list of errors; empty list if no errors</returns>
        public virtual IList<string> Refund(Order order)
        {
            if (order == null)
            {
                throw new ArgumentNullException("order");
            }

            if (!CanRefund(order))
            {
                throw new NopException("Cannot do refund for order.");
            }

            var request = new RefundPaymentRequest();
            RefundPaymentResult result = null;
            request.Order = order;
            request.AmountToRefund = order.OrderTotal;
            request.IsPartialRefund = false;
            try
            {
                result = _paymentService.Refund(request);
            }
            catch (Exception ex)
            {
                if (result == null)
                {
                    result = new RefundPaymentResult();
                }

                result.AddError(string.Format("Error occured while gateway processing refund: {0}. Full exception: {1}. Cap Transaction Tag {2}, Auth Transaction ID {3}", ex.Message, ex.ToString(), order.CaptureTransactionId, order.AuthorizationTransactionId));
            }

            if (result != null)
            {
                //total amount refunded
                decimal totalAmountRefunded = order.RefundedAmount + request.AmountToRefund;

                //update order info
                order.RefundedAmount = totalAmountRefunded;
                if (result.Success)
                {
                    order.PaymentStatus = result.NewPaymentStatus;
                }
                else
                {
                    order.PaymentStatus = PaymentStatus.Pending;
                    order.OrderStatus = OrderStatus.Pending;
                }
                _orderService.UpdateOrder(order);

                //add a note
                order.OrderNotes.Add(new OrderNote
                {
                    Note = string.Format("Order has been full refunded. Amount = {0}", request.AmountToRefund),
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow
                });

                _logger.InsertLog(LogLevel.Information, string.Format("Order has been full refunded. Amount = {0}", request.AmountToRefund), string.Format("Order has been full refunded. Amount = {0}", request.AmountToRefund));

                _orderService.UpdateOrder(order);

                //check order status
                CheckOrderStatus(order);

                //notifications
                var orderRefundedStoreOwnerNotificationQueuedEmailId = _workflowMessageService.SendOrderRefundedStoreOwnerNotification(order, request.AmountToRefund, _localizationSettings.DefaultAdminLanguageId);
                if (orderRefundedStoreOwnerNotificationQueuedEmailId > 0)
                {
                    order.OrderNotes.Add(new OrderNote
                    {
                        Note = string.Format("\"Order refunded\" email (to store owner) has been queued. Queued email identifier: {0}.", orderRefundedStoreOwnerNotificationQueuedEmailId),
                        DisplayToCustomer = false,
                        CreatedOnUtc = DateTime.UtcNow
                    });
                    _orderService.UpdateOrder(order);
                }
                var orderRefundedCustomerNotificationQueuedEmailId = _workflowMessageService.SendOrderRefundedCustomerNotification(order, request.AmountToRefund, order.CustomerLanguageId);
                if (orderRefundedCustomerNotificationQueuedEmailId > 0)
                {
                    order.OrderNotes.Add(new OrderNote
                    {
                        Note = string.Format("\"Order refunded\" email (to customer) has been queued. Queued email identifier: {0}.", orderRefundedCustomerNotificationQueuedEmailId),
                        DisplayToCustomer = false,
                        CreatedOnUtc = DateTime.UtcNow
                    });
                    _orderService.UpdateOrder(order);
                }

                _taxService.PostDistributiveTaxRefundData(order); //gotta refund those taxes to vertex, ya'll

                //raise event       
                _eventPublisher.Publish(new OrderRefundedEvent(order, request.AmountToRefund));
            }

            //process errors
            string error = "";
            for (int i = 0; i < result.Errors.Count; i++)
            {
                error += string.Format("Error {0}: {1}", i, result.Errors[i]);
                if (i != result.Errors.Count - 1)
                {
                    error += ". ";
                }
            }
            if (!String.IsNullOrEmpty(error))
            {
                //add a note
                order.OrderNotes.Add(new OrderNote
                {
                    Note = string.Format("Refund failed to complete fully due to: {0}", error),
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow
                });
                _orderService.UpdateOrder(order);

                //log it
                string logError = string.Format("Error refunding order #{0}. Error: {1}. Please check the gateway and complete the refund as needed.", order.Id, error);
                _logger.InsertLog(LogLevel.Error, logError, logError);
            }
            return result.Errors;
        }

        /// <summary>
        /// Gets a value indicating whether order can be marked as refunded
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>A value indicating whether order can be marked as refunded</returns>
        public virtual bool CanRefundOffline(Order order)
        {
            if (order == null)
            {
                throw new ArgumentNullException("order");
            }

            if (order.OrderTotal == decimal.Zero)
            {
                return false;
            }

            //uncomment the lines below in order to allow this operation for cancelled orders
            //if (order.OrderStatus == OrderStatus.Cancelled)
            //     return false;

            //Trove Enables Stores - Cannot Refund Offline.
            string troveStoreIds = "";
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
                        return false;
                    }
                }
            }

            if (order.PaymentStatus == PaymentStatus.Paid)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Refunds an order (offline)
        /// </summary>
        /// <param name="order">Order</param>
        public virtual void RefundOffline(Order order)
        {
            if (order == null)
            {
                throw new ArgumentNullException("order");
            }

            if (!CanRefundOffline(order))
            {
                throw new NopException("You can't refund this order");
            }

            //amout to refund
            decimal amountToRefund = order.OrderTotal;

            //total amount refunded
            decimal totalAmountRefunded = order.RefundedAmount + amountToRefund;

            //update order info
            order.RefundedAmount = totalAmountRefunded;
            order.PaymentStatus = PaymentStatus.Refunded;
            _orderService.UpdateOrder(order);

            //add a note
            order.OrderNotes.Add(new OrderNote
            {
                Note = string.Format("Order has been marked as refunded via RefundOffline. Amount = {0}", amountToRefund),
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow
            });

            _logger.InsertLog(LogLevel.Information, string.Format("Order has been marked as refunded via RefundOffline. Amount = {0}", amountToRefund), string.Format("Order has been marked as refunded via RefundOffline. Amount = {0}", amountToRefund));

            InsertIntoSAPSalesJournal(order, 3);

            _orderService.UpdateOrder(order);

            //check order status
            CheckOrderStatus(order);

            //notifications
            var orderRefundedStoreOwnerNotificationQueuedEmailId = _workflowMessageService.SendOrderRefundedStoreOwnerNotification(order, amountToRefund, _localizationSettings.DefaultAdminLanguageId);
            if (orderRefundedStoreOwnerNotificationQueuedEmailId > 0)
            {
                order.OrderNotes.Add(new OrderNote
                {
                    Note = string.Format("\"Order refunded\" email (to store owner) has been queued. Queued email identifier: {0}.", orderRefundedStoreOwnerNotificationQueuedEmailId),
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow
                });
                _orderService.UpdateOrder(order);
            }
            var orderRefundedCustomerNotificationQueuedEmailId = _workflowMessageService.SendOrderRefundedCustomerNotification(order, amountToRefund, order.CustomerLanguageId);
            if (orderRefundedCustomerNotificationQueuedEmailId > 0)
            {
                order.OrderNotes.Add(new OrderNote
                {
                    Note = string.Format("\"Order refunded\" email (to customer) has been queued. Queued email identifier: {0}.", orderRefundedCustomerNotificationQueuedEmailId),
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow
                });
                _orderService.UpdateOrder(order);
            }

            _taxService.PostDistributiveTaxRefundData(order); //gotta refund those taxes to vertex, ya'll

            //raise event       
            _eventPublisher.Publish(new OrderRefundedEvent(order, amountToRefund));
        }

        /// <summary>
        /// Gets a value indicating whether partial refund from admin panel is allowed
        /// </summary>
        /// <param name="order">Order</param>
        /// <param name="amountToRefund">Amount to refund</param>
        /// <returns>A value indicating whether refund from admin panel is allowed</returns>
        public virtual bool CanPartiallyRefund(Order order, decimal amountToRefund)
        {
            if (order == null)
            {
                throw new ArgumentNullException("order");
            }

            if (order.OrderTotal == decimal.Zero)
            {
                return false;
            }

            //uncomment the lines below in order to allow this operation for cancelled orders
            //if (order.OrderStatus == OrderStatus.Cancelled)
            //    return false;

            decimal canBeRefunded = order.OrderTotal - order.RefundedAmount;
            if (canBeRefunded <= decimal.Zero)
            {
                return false;
            }

            if (amountToRefund > canBeRefunded)
            {
                return false;
            }

            //Trove Enables Stores - Cannot Refund Offline.
            string troveStoreIds = "";
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
                        return false;
                    }
                }
            }

            if ((order.PaymentStatus == PaymentStatus.Paid ||
                order.PaymentStatus == PaymentStatus.PartiallyRefunded) &&
                _paymentService.SupportPartiallyRefund(order.PaymentMethodSystemName))
            {
                return true;
            }

            return false;
        }

        public virtual bool CanCustomRefund(Order order, decimal amountToRefund)
        {
            bool canCustomRefund = true;
            string troveStoreIds = "";
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
                        canCustomRefund = false;
                    }
                }
            }
            return canCustomRefund;
        }

        public virtual IList<string> PartiallyRefundWithCustomGLs(Order order, decimal amountToRefund1 = 0, decimal amountToRefund2 = 0, decimal amountToRefund3 = 0, bool isSplit = false, int ProductId = 0)
        {
            if (order == null)
            {
                throw new ArgumentNullException("Order cannot be null.");
            }
            // Set up the Payment gateway request
            var request = new RefundPaymentRequest();
            RefundPaymentResult result = null;
            request.Order = order;
            request.AmountToRefund = amountToRefund1;
            request.IsPartialRefund = true;
            try
            {
                result = _paymentService.Refund(request);

            }
            catch (Exception ex)
            {
                if (result == null)
                {
                    result = new RefundPaymentResult();
                }

                string errorReport = string.Format("Error occured while gateway processing refund: {0}. Full exception: {1}. Cap Transaction Tag {2}, Auth Transaction ID {3}", ex.Message, ex.ToString(), order.CaptureTransactionId, order.AuthorizationTransactionId);
                result.AddError(errorReport);
                _logger.Error(errorReport);
            }


            if (result != null)
            {
                //total amount refunded
                decimal totalAmountRefunded = order.RefundedAmount + amountToRefund1;

                //update order info
                order.RefundedAmount = totalAmountRefunded; //Look at the order items to get the total amout refunded.
                if (result.Success)
                {
                    order.PaymentStatus = result.NewPaymentStatus;
                }
                else
                {
                    order.PaymentStatus = PaymentStatus.PartiallyRefunded;
                    order.OrderStatus = OrderStatus.Pending;
                }
                try
                {
                    _orderService.UpdateOrder(order);
                }
                catch (Exception ex)
                {
                    string errorReport = string.Format("While updating order status for refund, exception was handled: {0}", ex.Message);
                    result.AddError(errorReport);
                    _logger.Error(errorReport);
                }


                try
                {
                    //add a note
                    order.OrderNotes.Add(new OrderNote
                    {
                        Note = string.Format("Order has been partially refunded via PartiallyRefundWithCustomGls. Amount = {0}", amountToRefund1),
                        DisplayToCustomer = false,
                        CreatedOnUtc = DateTime.UtcNow
                    });

                    _logger.InsertLog(LogLevel.Information, string.Format("Order has been partially refunded via PartiallyRefundWithCustomGLs.  Amount = {0}", amountToRefund1), string.Format("Order has been partially refunded via PartiallyRefundWithCustomGLs.  Amount = {0}", amountToRefund1));

                    _orderService.UpdateOrder(order);
                }
                catch (Exception ex)
                {
                    string errorReport = string.Format("While updating order notes for refund, exception was handled: {0}", ex.Message);
                    result.AddError(errorReport);
                    _logger.Error(errorReport);
                }


                //check order status
                CheckOrderStatus(order);

                //notifications
                var orderRefundedStoreOwnerNotificationQueuedEmailId = _workflowMessageService.SendOrderRefundedStoreOwnerNotification(order, amountToRefund1, _localizationSettings.DefaultAdminLanguageId);
                if (orderRefundedStoreOwnerNotificationQueuedEmailId > 0)
                {
                    order.OrderNotes.Add(new OrderNote
                    {
                        Note = string.Format("\"Order refunded\" email (to store owner) has been queued. Queued email identifier: {0}.", orderRefundedStoreOwnerNotificationQueuedEmailId),
                        DisplayToCustomer = false,
                        CreatedOnUtc = DateTime.UtcNow
                    });
                    _orderService.UpdateOrder(order);
                }
                var orderRefundedCustomerNotificationQueuedEmailId = _workflowMessageService.SendOrderRefundedCustomerNotification(order, amountToRefund1, order.CustomerLanguageId);
                if (orderRefundedCustomerNotificationQueuedEmailId > 0)
                {
                    order.OrderNotes.Add(new OrderNote
                    {
                        Note = string.Format("\"Order refunded\" email (to customer) has been queued. Queued email identifier: {0}.", orderRefundedCustomerNotificationQueuedEmailId),
                        DisplayToCustomer = false,
                        CreatedOnUtc = DateTime.UtcNow
                    });
                    _orderService.UpdateOrder(order);
                }


                //raise event       
                _eventPublisher.Publish(new OrderRefundedEvent(order, amountToRefund1));
            }

            //Error handling for all errors that got caught
            string error = "";
            for (int i = 0; i < result.Errors.Count; i++)
            {
                error += string.Format("Error {0}: {1}", i, result.Errors[i]);
                if (i != result.Errors.Count - 1)
                {
                    error += ". ";
                }
            }
            if (!String.IsNullOrEmpty(error))
            {
                //add a note
                order.OrderNotes.Add(new OrderNote
                {
                    Note = string.Format("Unable to partially refund order. {0}", error),
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow
                });
                _orderService.UpdateOrder(order);

                //log it
                string logError = string.Format("Error refunding order #{0}. Error: {1}", order.Id, error);
                _logger.InsertLog(LogLevel.Error, logError, logError);
            }

            return new List<string>();
        }
        /// <summary>
        /// Partially refunds an order (from admin panel)
        /// </summary>
        /// <param name="order">Order</param>
        /// <param name="amountToRefund">Amount to refund</param>
        /// <returns>A list of errors; empty list if no errors</returns>
        public virtual IList<string> PartiallyRefund(Order order, decimal amountToRefund1 = 0, decimal amountToRefund2 = 0, decimal amountToRefund3 = 0, bool isSplit = false, int ProductId = 0)
        {

            if (order == null)
            {
                throw new ArgumentNullException("order");
            }

            //if (!CanPartiallyRefund(order, amountToRefund1))
            //    throw new NopException("Cannot do partial refund for order.");

            var request = new RefundPaymentRequest();
            RefundPaymentResult result = null;
            request.Order = order;
            request.AmountToRefund = amountToRefund1;
            request.IsPartialRefund = true;

            try
            {
                result = _paymentService.Refund(request);
            }
            catch (Exception ex)
            {
                if (result == null)
                {
                    result = new RefundPaymentResult();
                }

                string errorReport = string.Format("Error occured while gateway processing refund: {0}. Full exception: {1}. Cap Transaction Tag {2}, Auth Transaction ID {3}", ex.Message, ex.ToString(), order.CaptureTransactionId, order.AuthorizationTransactionId);
                result.AddError(errorReport);
                _logger.Error(errorReport);
            }

            if (result != null)
            {
                //total amount refunded
                decimal totalAmountRefunded = order.RefundedAmount + amountToRefund1;

                //update order info
                order.RefundedAmount = totalAmountRefunded; //Look at the order items to get the total amout refunded.
                if (result.Success)
                {
                    order.PaymentStatus = result.NewPaymentStatus;
                }
                else
                {
                    order.PaymentStatus = PaymentStatus.Pending;
                    order.OrderStatus = OrderStatus.Pending;
                }
                _orderService.UpdateOrder(order);


                //add a note
                order.OrderNotes.Add(new OrderNote
                {
                    Note = string.Format("Order has been partially refunded. Amount = {0}", amountToRefund1),
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow
                });

                _logger.InsertLog(LogLevel.Information, string.Format("Order has been partially refunded.  Amount = {0}", amountToRefund1), string.Format("Order has been partially refunded.  Amount = {0}", amountToRefund1));

                _orderService.UpdateOrder(order);


                //check order status
                CheckOrderStatus(order);

                //notifications
                var orderRefundedStoreOwnerNotificationQueuedEmailId = _workflowMessageService.SendOrderRefundedStoreOwnerNotification(order, amountToRefund1, _localizationSettings.DefaultAdminLanguageId);
                if (orderRefundedStoreOwnerNotificationQueuedEmailId > 0)
                {
                    order.OrderNotes.Add(new OrderNote
                    {
                        Note = string.Format("\"Order refunded\" email (to store owner) has been queued. Queued email identifier: {0}.", orderRefundedStoreOwnerNotificationQueuedEmailId),
                        DisplayToCustomer = false,
                        CreatedOnUtc = DateTime.UtcNow
                    });
                    _orderService.UpdateOrder(order);
                }
                var orderRefundedCustomerNotificationQueuedEmailId = _workflowMessageService.SendOrderRefundedCustomerNotification(order, amountToRefund1, order.CustomerLanguageId);
                if (orderRefundedCustomerNotificationQueuedEmailId > 0)
                {
                    order.OrderNotes.Add(new OrderNote
                    {
                        Note = string.Format("\"Order refunded\" email (to customer) has been queued. Queued email identifier: {0}.", orderRefundedCustomerNotificationQueuedEmailId),
                        DisplayToCustomer = false,
                        CreatedOnUtc = DateTime.UtcNow
                    });
                    _orderService.UpdateOrder(order);
                }


                //raise event       
                _eventPublisher.Publish(new OrderRefundedEvent(order, amountToRefund1));
            }

            //process errors
            string error = "";
            for (int i = 0; i < result.Errors.Count; i++)
            {
                error += string.Format("Error {0}: {1}", i, result.Errors[i]);
                if (i != result.Errors.Count - 1)
                {
                    error += ". ";
                }
            }
            if (!String.IsNullOrEmpty(error))
            {
                //add a note
                order.OrderNotes.Add(new OrderNote
                {
                    Note = string.Format("Unable to partially refund order. {0}", error),
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow
                });
                _orderService.UpdateOrder(order);

                //log it
                string logError = string.Format("Error refunding order #{0}. Error: {1}", order.Id, error);
                _logger.InsertLog(LogLevel.Error, logError, logError);
            }
            return result.Errors;
        }

        /// <summary>
        /// Gets a value indicating whether order can be marked as partially refunded
        /// </summary>
        /// <param name="order">Order</param>
        /// <param name="amountToRefund">Amount to refund</param>
        /// <returns>A value indicating whether order can be marked as partially refunded</returns>
        public virtual bool CanPartiallyRefundOffline(Order order, decimal amountToRefund)
        {
            if (order == null)
            {
                throw new ArgumentNullException("order");
            }

            if (order.OrderTotal == decimal.Zero)
            {
                return false;
            }

            //uncomment the lines below in order to allow this operation for cancelled orders
            //if (order.OrderStatus == OrderStatus.Cancelled)
            //    return false;

            decimal canBeRefunded = order.OrderTotal - order.RefundedAmount;
            if (canBeRefunded <= decimal.Zero)
            {
                return false;
            }

            if (amountToRefund > canBeRefunded)
            {
                return false;
            }

            //Trove API Enables Stores Cannot do partial refund offline
            string troveStoreIds = "";
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
                        return false;
                    }
                }
            }

            if (order.PaymentStatus == PaymentStatus.Paid ||
                order.PaymentStatus == PaymentStatus.PartiallyRefunded)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Partially refunds an order (offline)
        /// </summary>
        /// <param name="order">Order</param>
        /// <param name="amountToRefund">Amount to refund</param>
        public virtual void PartiallyRefundOffline(Order order, decimal amountToRefund)
        {
            if (order == null)
            {
                throw new ArgumentNullException("order");
            }

            if (!CanPartiallyRefundOffline(order, amountToRefund))
            {
                throw new NopException("You can't partially refund (offline) this order");
            }

            //total amount refunded
            decimal totalAmountRefunded = order.RefundedAmount + amountToRefund;

            //update order info
            order.RefundedAmount = totalAmountRefunded;
            //if (order.OrderTotal == totalAmountRefunded), then set order.PaymentStatus = PaymentStatus.Refunded;
            order.PaymentStatus = PaymentStatus.PartiallyRefunded;
            _orderService.UpdateOrder(order);

            //add a note
            order.OrderNotes.Add(new OrderNote
            {
                Note = string.Format("Order has been marked as partially refunded via PartiallyRefundOffline. Amount = {0}", amountToRefund),
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow
            });

            _logger.InsertLog(LogLevel.Information, string.Format("Order has been marked as partially refunded via PartiallyRefundOffline. Amount = {0}", amountToRefund), string.Format("Order has been marked as partially refunded via PartiallyRefundOffline. Amount = {0}", amountToRefund));

            _orderService.UpdateOrder(order);

            //check order status
            CheckOrderStatus(order);

            //notifications
            var orderRefundedStoreOwnerNotificationQueuedEmailId = _workflowMessageService.SendOrderRefundedStoreOwnerNotification(order, amountToRefund, _localizationSettings.DefaultAdminLanguageId);
            if (orderRefundedStoreOwnerNotificationQueuedEmailId > 0)
            {
                order.OrderNotes.Add(new OrderNote
                {
                    Note = string.Format("\"Order refunded\" email (to store owner) has been queued. Queued email identifier: {0}.", orderRefundedStoreOwnerNotificationQueuedEmailId),
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow
                });
                _orderService.UpdateOrder(order);
            }
            var orderRefundedCustomerNotificationQueuedEmailId = _workflowMessageService.SendOrderRefundedCustomerNotification(order, amountToRefund, order.CustomerLanguageId);
            if (orderRefundedCustomerNotificationQueuedEmailId > 0)
            {
                order.OrderNotes.Add(new OrderNote
                {
                    Note = string.Format("\"Order refunded\" email (to customer) has been queued. Queued email identifier: {0}.", orderRefundedCustomerNotificationQueuedEmailId),
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow
                });
                _orderService.UpdateOrder(order);
            }
            //raise event       
            _eventPublisher.Publish(new OrderRefundedEvent(order, amountToRefund));
        }



        /// <summary>
        /// Gets a value indicating whether void from admin panel is allowed
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>A value indicating whether void from admin panel is allowed</returns>
        public virtual bool CanVoid(Order order)
        {
            if (order == null)
            {
                throw new ArgumentNullException("order");
            }

            if (order.OrderTotal == decimal.Zero)
            {
                return false;
            }

            //uncomment the lines below in order to allow this operation for cancelled orders
            //if (order.OrderStatus == OrderStatus.Cancelled)
            //    return false;

            if (order.PaymentStatus == PaymentStatus.Authorized &&
                _paymentService.SupportVoid(order.PaymentMethodSystemName))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Voids order (from admin panel)
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>Voided order</returns>
        public virtual IList<string> Void(Order order)
        {
            if (order == null)
            {
                throw new ArgumentNullException("order");
            }

            if (!CanVoid(order))
            {
                throw new NopException("Cannot do void for order.");
            }

            var request = new VoidPaymentRequest();
            VoidPaymentResult result = null;
            try
            {
                request.Order = order;
                result = _paymentService.Void(request);

                if (result.Success)
                {
                    //update order info
                    order.PaymentStatus = result.NewPaymentStatus;
                    _orderService.UpdateOrder(order);

                    //add a note
                    order.OrderNotes.Add(new OrderNote
                    {
                        Note = "Order has been voided",
                        DisplayToCustomer = false,
                        CreatedOnUtc = DateTime.UtcNow
                    });
                    _orderService.UpdateOrder(order);

                    //check order status
                    CheckOrderStatus(order);
                }
            }
            catch (Exception exc)
            {
                if (result == null)
                {
                    result = new VoidPaymentResult();
                }

                result.AddError(string.Format("Error: {0}. Full exception: {1}", exc.Message, exc.ToString()));
            }

            //process errors
            string error = "";
            for (int i = 0; i < result.Errors.Count; i++)
            {
                error += string.Format("Error {0}: {1}", i, result.Errors[i]);
                if (i != result.Errors.Count - 1)
                {
                    error += ". ";
                }
            }
            if (!String.IsNullOrEmpty(error))
            {
                //add a note
                order.OrderNotes.Add(new OrderNote
                {
                    Note = string.Format("Unable to voiding order. {0}", error),
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow
                });
                _orderService.UpdateOrder(order);

                //log it
                string logError = string.Format("Error voiding order #{0}. Error: {1}", order.Id, error);
                _logger.InsertLog(LogLevel.Error, logError, logError);
            }
            return result.Errors;
        }

        /// <summary>
        /// Gets a value indicating whether order can be marked as voided
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>A value indicating whether order can be marked as voided</returns>
        public virtual bool CanVoidOffline(Order order)
        {
            if (order == null)
            {
                throw new ArgumentNullException("order");
            }

            if (order.OrderTotal == decimal.Zero)
            {
                return false;
            }

            //uncomment the lines below in order to allow this operation for cancelled orders
            //if (order.OrderStatus == OrderStatus.Cancelled)
            //    return false;

            if (order.PaymentStatus == PaymentStatus.Authorized)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Voids order (offline)
        /// </summary>
        /// <param name="order">Order</param>
        public virtual void VoidOffline(Order order)
        {
            if (order == null)
            {
                throw new ArgumentNullException("order");
            }

            if (!CanVoidOffline(order))
            {
                throw new NopException("You can't void this order");
            }

            order.PaymentStatusId = (int)PaymentStatus.Voided;
            _orderService.UpdateOrder(order);

            //add a note
            order.OrderNotes.Add(new OrderNote
            {
                Note = "Order has been marked as voided",
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow
            });
            _orderService.UpdateOrder(order);

            //check orer status
            CheckOrderStatus(order);
        }



        /// <summary>
        /// Place order items in current user shopping cart.
        /// </summary>
        /// <param name="order">The order</param>
        public virtual void ReOrder(Order order)
        {
            if (order == null)
            {
                throw new ArgumentNullException("order");
            }

            //move shopping cart items (if possible)
            foreach (var orderItem in order.OrderItems)
            {
                if (orderItem.IsBundleProduct)
                {
                    int productId = _shoppingCartService.GetAssociatedProductsByOrderItemId(orderItem.Id).FirstOrDefault().ParentProductId;
                    Product pr = _productService.GetProductById(productId);
                    orderItem.Product = pr;
                    if (_workContext.CurrentCustomer.ShoppingCartItems.Where(x => x.Product == pr).Any())
                    {
                        continue;
                    }
                }

                var reservedProduct = _productService.GetReservedProductbyOrderItemId(orderItem.Id);
                _shoppingCartService.AddToCart(
                    order.Customer,
                    orderItem.Product,
                    ShoppingCartType.ShoppingCart,
                    order.StoreId,
                    orderItem.AttributesXml,
                    orderItem.UnitPriceExclTax,
                    orderItem.RentalStartDateUtc,
                    orderItem.RentalEndDateUtc,
                    orderItem.Quantity,
                    orderItem.IsReservation && reservedProduct != null ? reservedProduct.ReservationDate : DateTime.UtcNow,
                    orderItem.IsReservation && reservedProduct != null ? reservedProduct.ReservedTimeSlot : String.Empty,
                    false);

            }
            //set checkout attributes
            //comment the code below if you want to disable this functionality
            _genericAttributeService.SaveAttribute(order.Customer, SystemCustomerAttributeNames.CheckoutAttributes, order.CheckoutAttributesXml, order.StoreId);
        }

        /// <summary>
        /// Check whether return request is allowed
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>Result</returns>
        public virtual bool IsReturnRequestAllowed(Order order)
        {
            if (!_orderSettings.ReturnRequestsEnabled)
            {
                return false;
            }

            if (order == null || order.Deleted)
            {
                return false;
            }

            //status should be compelte
            if (order.OrderStatus != OrderStatus.Complete)
            {
                return false;
            }

            //validate allowed number of days
            if (_orderSettings.NumberOfDaysReturnRequestAvailable > 0)
            {
                var daysPassed = (DateTime.UtcNow - order.CreatedOnUtc).TotalDays;
                if (daysPassed >= _orderSettings.NumberOfDaysReturnRequestAvailable)
                {
                    return false;
                }
            }

            //ensure that we have at least one returnable product
            return order.OrderItems.Any(oi => !oi.Product.NotReturnable);
        }



        /// <summary>
        /// Valdiate minimum order sub-total amount
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <returns>true - OK; false - minimum order sub-total amount is not reached</returns>
        public virtual bool ValidateMinOrderSubtotalAmount(IList<ShoppingCartItem> cart)
        {
            if (cart == null)
            {
                throw new ArgumentNullException("cart");
            }

            //min order amount sub-total validation
            if (cart.Any() && _orderSettings.MinOrderSubtotalAmount > decimal.Zero)
            {
                //subtotal
                decimal orderSubTotalDiscountAmountBase;
                List<Discount> orderSubTotalAppliedDiscounts;
                decimal subTotalWithoutDiscountBase;
                decimal subTotalWithDiscountBase;
                _orderTotalCalculationService.GetShoppingCartSubTotal(cart, _orderSettings.MinOrderSubtotalAmountIncludingTax,
                    out orderSubTotalDiscountAmountBase, out orderSubTotalAppliedDiscounts,
                    out subTotalWithoutDiscountBase, out subTotalWithDiscountBase);

                if (subTotalWithoutDiscountBase < _orderSettings.MinOrderSubtotalAmount)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Valdiate minimum order total amount
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <returns>true - OK; false - minimum order total amount is not reached</returns>
        public virtual bool ValidateMinOrderTotalAmount(IList<ShoppingCartItem> cart)
        {
            if (cart == null)
            {
                throw new ArgumentNullException("cart");
            }

            if (cart.Any() && _orderSettings.MinOrderTotalAmount > decimal.Zero)
            {
                decimal? shoppingCartTotalBase = _orderTotalCalculationService.GetShoppingCartTotal(cart);
                if (shoppingCartTotalBase.HasValue && shoppingCartTotalBase.Value < _orderSettings.MinOrderTotalAmount)
                {
                    return false;
                }
            }

            return true;
        }

        #endregion

        #region Operation for Sap_SalesJournal



        public void InsertIntoSAPSalesJournal(Order order, int status)
        {

            foreach (var item in order.OrderItems)
            {
                InsertIntoSAPSalesJournalOrderItem(order, item, status);
            }


        }


        public void InsertIntoSAPSalesJournalOrderItem(Order order, OrderItem item, int status)
        {

            string connString = new DataSettingsManager().LoadSettings().DataConnectionString;
            var sqlSelect = string.Format(@"select * from SAPOrderProcess where orderid={0} and orderitemid={1} and statusid={2}", order.Id, item.Id, status);
            string sql = "Insert into dbo.SAPOrderProcess(OrderId,OrderItemId,StatusId) values(@Order,@OrderItemId,@StatusId)";
            using (SqlConnection conn = new SqlConnection(connString))
            {
                SqlCommand cmd = new SqlCommand(sql, conn);
                SqlCommand cmd1 = new SqlCommand(sqlSelect, conn);
                try
                {
                    conn.Open();
                    cmd1.CommandText = sqlSelect;
                    using (var reader = cmd1.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            cmd.Parameters.AddWithValue("@Order", order.Id);
                            cmd.Parameters.AddWithValue("@OrderItemId", item.Id);
                            cmd.Parameters.AddWithValue("@StatusId", status);
                            cmd.ExecuteNonQuery();
                        }
                    }

                }
                catch (Exception ex)
                {
                    throw ex;
                }

            }

        }
        /// <summary>
        /// Method to insert record into sapOrderProcess table to create rcords for Attribute Products
        /// </summary>
        /// <param name="order"></param>
        /// <param name="item"></param>
        /// <param name="status"></param>
        /// <param name="productid"></param>
        /// <param name="isBundleProduct"></param>
        public void InsertIntoSAPSalesJournalOrderItem(Order order, OrderItem item, int status, int productid, bool isBundleProduct)
        {

            string connString = new DataSettingsManager().LoadSettings().DataConnectionString;
            var sqlSelect = string.Format(@"select * from SAPOrderProcess where orderid={0} and orderitemid={1} and statusid={2}", order.Id, item.Id, status);
            string sql = "Insert into dbo.SAPOrderProcess(OrderId,OrderItemId,StatusId) values(@Order,@OrderItemId,@StatusId)";
            using (SqlConnection conn = new SqlConnection(connString))
            {
                SqlCommand cmd = new SqlCommand(sql, conn);
                SqlCommand cmd1 = new SqlCommand(sqlSelect, conn);
                try
                {
                    conn.Open();
                    cmd1.CommandText = sqlSelect;
                    using (var reader = cmd1.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            cmd.Parameters.AddWithValue("@Order", order.Id);
                            cmd.Parameters.AddWithValue("@OrderItemId", item.Id);
                            cmd.Parameters.AddWithValue("@StatusId", status);
                            cmd.ExecuteNonQuery();
                        }
                    }

                }
                catch (Exception ex)
                {
                    throw ex;
                }

            }

        }


        /// <summary>
        /// Added by Muzahid for saving into SAP_SalesJournal 
        /// </summary>
        /// <param name="order"></param>
        /// <param name="Status"></param>
        public void SaveIntoSapJournal(Order order, int Status)
        {
            try
            {
                string SqlQuery = string.Format("Select * from  SAP_SalesJournal where OrderNumber = " + order.Id + " and Status != " + Status);
                DataTable dt = new DataTable();

                string connString = new DataSettingsManager().LoadSettings().DataConnectionString;

                using (SqlConnection conn = new SqlConnection(connString))
                {
                    try
                    {
                        conn.Open();
                        SqlCommand cmd = new SqlCommand(SqlQuery, conn);
                        SqlDataAdapter sqlAdp = new SqlDataAdapter(cmd);
                        sqlAdp.Fill(dt);

                        if (dt.Rows.Count < 1)
                        {
                            return;
                        }

                        foreach (DataRow dtRow in dt.Rows)
                        {
                            dtRow.SetField("Status", Status);
                            if (Status == 3)
                            {
                                dtRow.SetField("IsProcessed", 0);
                            }
                        }

                        using (SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(conn))
                        {
                            //Set the database table name
                            sqlBulkCopy.DestinationTableName = "dbo.SAP_SalesJournal";

                            sqlBulkCopy.WriteToServer(dt);
                            conn.Close();
                        }


                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }

            }
            catch (Exception)
            {

            }
        }

        public void UpdateSAPJournal(int OrderId)
        {
            string SqlQuery = "Update SAP_SalesJournal set IsProcessed = 0,ProcessedDate = @date where OrderNumber = " + OrderId;

            string connString = new DataSettingsManager().LoadSettings().DataConnectionString;

            using (SqlConnection conn = new SqlConnection(connString))
            {
                SqlCommand cmd = new SqlCommand(SqlQuery, conn);
                cmd.Parameters.Add("@date", SqlDbType.DateTime2).Value = DateTime.Now;
                try
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        #endregion
    }
}
