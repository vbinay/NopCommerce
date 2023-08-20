using Nop.Admin.Extensions;
using Nop.Admin.Helpers;
using Nop.Admin.Models.Orders;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Core.Domain.Vendors;
using Nop.Services;
using Nop.Services.Affiliates;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.ExportImport;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.SAP;
using Nop.Services.Security;
using Nop.Services.Shipping;
using Nop.Services.Shipping.Tracking;
using Nop.Services.Stores;
using Nop.Services.Tax;
using Nop.Services.Vendors;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using Nop.Web.Framework.Mvc;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Nop.Services.Customers;

namespace Nop.Admin.Controllers
{
    public partial class OrderController : BaseAdminController
    {
        #region Fields

        private readonly IDonationService _donationService;
        private readonly IOrderService _orderService;
        private readonly IOrderReportService _orderReportService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IReturnRequestService _returnRequestService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly ITaxService _taxService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IDiscountService _discountService;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
        private readonly ICurrencyService _currencyService;
        private readonly IEncryptionService _encryptionService;
        private readonly IPaymentService _paymentService;
        private readonly IMeasureService _measureService;
        private readonly IPdfService _pdfService;
        private readonly IAddressService _addressService;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IProductService _productService;
        private readonly IExportManager _exportManager;
        private readonly IPermissionService _permissionService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly ICategoryService _categoryService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IProductAttributeFormatter _productAttributeFormatter;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IGiftCardService _giftCardService;
        private readonly IDownloadService _downloadService;
        private readonly IShipmentService _shipmentService;
        private readonly IShippingService _shippingService;
        private readonly IStoreService _storeService;
        private readonly IVendorService _vendorService;
        private readonly IAddressAttributeParser _addressAttributeParser;
        private readonly IAddressAttributeService _addressAttributeService;
        private readonly IAddressAttributeFormatter _addressAttributeFormatter;
        private readonly IAffiliateService _affiliateService;
        private readonly IPictureService _pictureService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ICacheManager _cacheManager;

        private readonly OrderSettings _orderSettings;
        private readonly CurrencySettings _currencySettings;
        private readonly TaxSettings _taxSettings;
        private readonly MeasureSettings _measureSettings;
        private readonly AddressSettings _addressSettings;
        private readonly ShippingSettings _shippingSettings;

        private readonly IReportingService _reportingService;//NU-90           

        private readonly IStoreContext _storeContext;

        private readonly ISAPService _SAPService;
        private readonly HttpContextBase _httpContext;
        private readonly IGlCodeService _glCodeService;
        private readonly ITaxCategoryService _taxCategoryService;
        private readonly IRepository<OrderItem> _orderItemRepository;
        private readonly ICustomerService _customerService;

        #endregion

        #region Ctor

        public OrderController(IDonationService donationService,
            IOrderService orderService,
            IOrderReportService orderReportService,
            IOrderProcessingService orderProcessingService,
            IReturnRequestService returnRequestService,
            IPriceCalculationService priceCalculationService,
            ITaxService taxService,
            IDateTimeHelper dateTimeHelper,
            IPriceFormatter priceFormatter,
            IDiscountService discountService,
            ILocalizationService localizationService,
            IWorkContext workContext,
            ICurrencyService currencyService,
            IEncryptionService encryptionService,
            IPaymentService paymentService,
            IMeasureService measureService,
            IPdfService pdfService,
            IAddressService addressService,
            ICountryService countryService,
            IStateProvinceService stateProvinceService,
            IProductService productService,
            IExportManager exportManager,
            IPermissionService permissionService,
            IWorkflowMessageService workflowMessageService,
            ICategoryService categoryService,
            IManufacturerService manufacturerService,
            IProductAttributeService productAttributeService,
            IProductAttributeParser productAttributeParser,
            IProductAttributeFormatter productAttributeFormatter,
            IShoppingCartService shoppingCartService,
            IGiftCardService giftCardService,
            IDownloadService downloadService,
            IShipmentService shipmentService,
            IShippingService shippingService,
            IStoreService storeService,
            IVendorService vendorService,
            IAddressAttributeParser addressAttributeParser,
            IAddressAttributeService addressAttributeService,
            IAddressAttributeFormatter addressAttributeFormatter,
            IAffiliateService affiliateService,
            IPictureService pictureService,
            ICustomerActivityService customerActivityService,
            ICacheManager cacheManager,
            OrderSettings orderSettings,
            CurrencySettings currencySettings,
            TaxSettings taxSettings,
            MeasureSettings measureSettings,
            AddressSettings addressSettings,
            ShippingSettings shippingSettings,
            IStoreContext storeContext,
            ISAPService SAPService,
            IReportingService reportingService,
             HttpContextBase httpContext,
             IGlCodeService glCodeService,
             ITaxCategoryService taxCategoryService,
              IRepository<OrderItem> orderItemRepository,
              ICustomerService customerService)	 //NU-90
        {
            this._donationService = donationService;
            this._orderService = orderService;
            this._orderReportService = orderReportService;
            this._orderProcessingService = orderProcessingService;
            this._returnRequestService = returnRequestService;
            this._priceCalculationService = priceCalculationService;
            this._taxService = taxService;
            this._dateTimeHelper = dateTimeHelper;
            this._priceFormatter = priceFormatter;
            this._discountService = discountService;
            this._localizationService = localizationService;
            this._workContext = workContext;
            this._currencyService = currencyService;
            this._encryptionService = encryptionService;
            this._paymentService = paymentService;
            this._measureService = measureService;
            this._pdfService = pdfService;
            this._addressService = addressService;
            this._countryService = countryService;
            this._stateProvinceService = stateProvinceService;
            this._productService = productService;
            this._exportManager = exportManager;
            this._permissionService = permissionService;
            this._workflowMessageService = workflowMessageService;
            this._categoryService = categoryService;
            this._manufacturerService = manufacturerService;
            this._productAttributeService = productAttributeService;
            this._productAttributeParser = productAttributeParser;
            this._productAttributeFormatter = productAttributeFormatter;
            this._shoppingCartService = shoppingCartService;
            this._giftCardService = giftCardService;
            this._downloadService = downloadService;
            this._shipmentService = shipmentService;
            this._shippingService = shippingService;
            this._storeService = storeService;
            this._vendorService = vendorService;
            this._addressAttributeParser = addressAttributeParser;
            this._addressAttributeService = addressAttributeService;
            this._addressAttributeFormatter = addressAttributeFormatter;
            this._affiliateService = affiliateService;
            this._pictureService = pictureService;
            this._customerActivityService = customerActivityService;
            this._cacheManager = cacheManager;
            this._orderSettings = orderSettings;
            this._currencySettings = currencySettings;
            this._taxSettings = taxSettings;
            this._measureSettings = measureSettings;
            this._addressSettings = addressSettings;
            this._shippingSettings = shippingSettings;
            this._storeContext = storeContext;
            this._SAPService = SAPService;
            this._reportingService = reportingService;
            this._httpContext = httpContext;
            this._glCodeService = glCodeService;
            this._taxCategoryService = taxCategoryService;
            this._orderItemRepository = orderItemRepository;
            this._customerService = customerService;
        }

        #endregion

        #region Utilities

        [NonAction]
        protected virtual bool HasAccessToOrder(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            if (_workContext.CurrentVendor == null)
                //not a vendor; has access
                return true;

            var vendorId = _workContext.CurrentVendor.Id;
            var hasVendorProducts = order.OrderItems.Any(orderItem => orderItem.Product.VendorId == vendorId);
            return hasVendorProducts;
        }

        [NonAction]
        protected virtual bool HasAccessToOrderItem(OrderItem orderItem)
        {
            if (orderItem == null)
                throw new ArgumentNullException("orderItem");

            if (_workContext.CurrentVendor == null)
                //not a vendor; has access
                return true;

            var vendorId = _workContext.CurrentVendor.Id;
            return orderItem.Product.VendorId == vendorId;
        }

        [NonAction]
        protected virtual bool HasAccessToProduct(Product product)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            if (_workContext.CurrentVendor == null)
                //not a vendor; has access
                return true;

            var vendorId = _workContext.CurrentVendor.Id;
            return product.VendorId == vendorId;
        }

        [NonAction]
        protected virtual bool HasAccessToShipment(Shipment shipment)
        {
            if (shipment == null)
                throw new ArgumentNullException("shipment");

            if (_workContext.CurrentVendor == null)
                //not a vendor; has access
                return true;

            var hasVendorProducts = false;
            var vendorId = _workContext.CurrentVendor.Id;
            foreach (var shipmentItem in shipment.ShipmentItems)
            {
                var orderItem = _orderService.GetOrderItemById(shipmentItem.OrderItemId);
                if (orderItem != null)
                {
                    if (orderItem.Product.VendorId == vendorId)
                    {
                        hasVendorProducts = true;
                        break;
                    }
                }
            }
            return hasVendorProducts;
        }

        /// <summary>
        /// Parse product attributes on the add product to order details page
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="form">Form</param>
        /// <returns>Parsed attributes</returns>
        [NonAction]
        protected virtual string ParseProductAttributes(Product product, FormCollection form)
        {
            var attributesXml = string.Empty;

            #region Product attributes

            var productAttributes = _productAttributeService.GetProductAttributeMappingsByProductId(product.Id);
            foreach (var attribute in productAttributes)
            {
                var controlId = string.Format("product_attribute_{0}", attribute.Id);
                switch (attribute.AttributeControlType)
                {
                    case AttributeControlType.DropdownList:
                    case AttributeControlType.RadioList:
                    case AttributeControlType.ColorSquares:
                    case AttributeControlType.ImageSquares:
                        {
                            var ctrlAttributes = form[controlId];
                            if (!String.IsNullOrEmpty(ctrlAttributes))
                            {
                                int selectedAttributeId = int.Parse(ctrlAttributes);
                                if (selectedAttributeId > 0)
                                    attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                        attribute, selectedAttributeId.ToString());
                            }
                        }
                        break;
                    case AttributeControlType.Checkboxes:
                        {
                            var ctrlAttributes = form[controlId];
                            if (!String.IsNullOrEmpty(ctrlAttributes))
                            {
                                foreach (var item in ctrlAttributes.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                                {
                                    int selectedAttributeId = int.Parse(item);
                                    if (selectedAttributeId > 0)
                                        attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                            attribute, selectedAttributeId.ToString());
                                }
                            }
                        }
                        break;
                    case AttributeControlType.ReadonlyCheckboxes:
                        {
                            //load read-only (already server-side selected) values
                            var attributeValues = _productAttributeService.GetProductAttributeValues(attribute.Id);
                            foreach (var selectedAttributeId in attributeValues
                                .Where(v => v.IsPreSelected)
                                .Select(v => v.Id)
                                .ToList())
                            {
                                attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                    attribute, selectedAttributeId.ToString());
                            }
                        }
                        break;
                    case AttributeControlType.TextBox:
                    case AttributeControlType.MultilineTextbox:
                        {
                            var ctrlAttributes = form[controlId];
                            if (!String.IsNullOrEmpty(ctrlAttributes))
                            {
                                string enteredText = ctrlAttributes.Trim();
                                attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                    attribute, enteredText);
                            }
                        }
                        break;
                    case AttributeControlType.Datepicker:
                        {
                            var day = form[controlId + "_day"];
                            var month = form[controlId + "_month"];
                            var year = form[controlId + "_year"];
                            DateTime? selectedDate = null;
                            try
                            {
                                selectedDate = new DateTime(Int32.Parse(year), Int32.Parse(month), Int32.Parse(day));
                            }
                            catch { }
                            if (selectedDate.HasValue)
                            {
                                attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                    attribute, selectedDate.Value.ToString("D"));
                            }
                        }
                        break;
                    case AttributeControlType.FileUpload:
                        {
                            Guid downloadGuid;
                            Guid.TryParse(form[controlId], out downloadGuid);
                            var download = _downloadService.GetDownloadByGuid(downloadGuid);
                            if (download != null)
                            {
                                attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                        attribute, download.DownloadGuid.ToString());
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
            //validate conditional attributes (if specified)
            foreach (var attribute in productAttributes)
            {
                var conditionMet = _productAttributeParser.IsConditionMet(attribute, attributesXml);
                if (conditionMet.HasValue && !conditionMet.Value)
                {
                    attributesXml = _productAttributeParser.RemoveProductAttribute(attributesXml, attribute);
                }
            }

            #endregion

            return attributesXml;
        }

        /// <summary>
        /// Parse rental dates on the add product to order details page
        /// </summary>
        /// <param name="form">Form</param>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        [NonAction]
        protected virtual void ParseRentalDates(FormCollection form,
            out DateTime? startDate, out DateTime? endDate)
        {
            startDate = null;
            endDate = null;

            var ctrlStartDate = form["rental_start_date"];
            var ctrlEndDate = form["rental_end_date"];
            try
            {
                const string datePickerFormat = "MM/dd/yyyy";
                startDate = DateTime.ParseExact(ctrlStartDate, datePickerFormat, CultureInfo.InvariantCulture);
                endDate = DateTime.ParseExact(ctrlEndDate, datePickerFormat, CultureInfo.InvariantCulture);
            }
            catch
            {
            }
        }

        [NonAction]
        public virtual void PrepareOrderDetailsModel(OrderModel model, Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            if (model == null)
                throw new ArgumentNullException("model");

            model.Id = order.Id;
            model.OrderStatus = order.OrderStatus.GetLocalizedEnum(_localizationService, _workContext);
            model.OrderStatusId = order.OrderStatusId;
            model.OrderGuid = order.OrderGuid;
            var store = _storeService.GetStoreById(order.StoreId);
            model.StoreName = store != null ? store.Name : "Unknown";
            model.CustomerId = order.CustomerId;
            var customer = order.Customer;
            model.CustomerInfo = customer.IsRegistered() ? customer.Email : _localizationService.GetResource("Admin.Customers.Guest");
            model.CustomerIp = order.CustomerIp;
            model.VatNumber = order.VatNumber;
            model.CreatedOn = _dateTimeHelper.ConvertToUserTime(order.CreatedOnUtc, DateTimeKind.Utc);
            model.AllowCustomersToSelectTaxDisplayType = _taxSettings.AllowCustomersToSelectTaxDisplayType;
            model.TaxDisplayType = _taxSettings.TaxDisplayType;

            var affiliate = _affiliateService.GetAffiliateById(order.AffiliateId);
            if (affiliate != null)
            {
                model.AffiliateId = affiliate.Id;
                model.AffiliateName = affiliate.GetFullName();
            }

            model.IsLoggedAs = _workContext.CurrentCustomer.IsLoggedAs();	// NU-1
            //a vendor should have access only to his products
            model.IsLoggedInAsVendor = _workContext.CurrentVendor != null;
            //custom values
            model.CustomValues = order.DeserializeCustomValues();

            //Tiered Shipping
            model.RCNumber = order.RCNumber;
            model.MailStopAddress = order.MailStopAddress;

            #region Order totals

            var primaryStoreCurrency = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId);
            if (primaryStoreCurrency == null)
                throw new Exception("Cannot load primary store currency");

            //subtotal
            model.OrderSubtotalInclTax = _priceFormatter.FormatPrice(order.OrderSubtotalInclTax, true, primaryStoreCurrency, _workContext.WorkingLanguage, true);
            model.OrderSubtotalExclTax = _priceFormatter.FormatPrice(order.OrderSubtotalExclTax, true, primaryStoreCurrency, _workContext.WorkingLanguage, false);
            model.OrderSubtotalInclTaxValue = order.OrderSubtotalInclTax;
            model.OrderSubtotalExclTaxValue = order.OrderSubtotalExclTax;
            //discount (applied to order subtotal)
            string orderSubtotalDiscountInclTaxStr = _priceFormatter.FormatPrice(order.OrderSubTotalDiscountInclTax, true, primaryStoreCurrency, _workContext.WorkingLanguage, true);
            string orderSubtotalDiscountExclTaxStr = _priceFormatter.FormatPrice(order.OrderSubTotalDiscountExclTax, true, primaryStoreCurrency, _workContext.WorkingLanguage, false);
            if (order.OrderSubTotalDiscountInclTax > decimal.Zero)
                model.OrderSubTotalDiscountInclTax = orderSubtotalDiscountInclTaxStr;
            if (order.OrderSubTotalDiscountExclTax > decimal.Zero)
                model.OrderSubTotalDiscountExclTax = orderSubtotalDiscountExclTaxStr;
            model.OrderSubTotalDiscountInclTaxValue = order.OrderSubTotalDiscountInclTax;
            model.OrderSubTotalDiscountExclTaxValue = order.OrderSubTotalDiscountExclTax;

            //shipping
            model.OrderShippingInclTax = _priceFormatter.FormatShippingPrice(order.OrderShippingInclTax, true, primaryStoreCurrency, _workContext.WorkingLanguage, true);
            model.OrderShippingExclTax = _priceFormatter.FormatShippingPrice(order.OrderShippingExclTax, true, primaryStoreCurrency, _workContext.WorkingLanguage, false);
            model.OrderShippingInclTaxValue = order.OrderShippingInclTax;
            model.OrderShippingExclTaxValue = order.OrderShippingExclTax;

            //payment method additional fee
            if (order.PaymentMethodAdditionalFeeInclTax > decimal.Zero)
            {
                model.PaymentMethodAdditionalFeeInclTax = _priceFormatter.FormatPaymentMethodAdditionalFee(order.PaymentMethodAdditionalFeeInclTax, true, primaryStoreCurrency, _workContext.WorkingLanguage, true);
                model.PaymentMethodAdditionalFeeExclTax = _priceFormatter.FormatPaymentMethodAdditionalFee(order.PaymentMethodAdditionalFeeExclTax, true, primaryStoreCurrency, _workContext.WorkingLanguage, false);
            }
            model.PaymentMethodAdditionalFeeInclTaxValue = order.PaymentMethodAdditionalFeeInclTax;
            model.PaymentMethodAdditionalFeeExclTaxValue = order.PaymentMethodAdditionalFeeExclTax;


            //tax
            model.Tax = _priceFormatter.FormatPrice(order.OrderTax, true, false);
            SortedDictionary<decimal, decimal> taxRates = order.TaxRatesDictionary;
            bool displayTaxRates = _taxSettings.DisplayTaxRates && taxRates.Any();
            bool displayTax = !displayTaxRates;
            foreach (var tr in order.TaxRatesDictionary)
            {
                model.TaxRates.Add(new OrderModel.TaxRate
                {
                    Rate = _priceFormatter.FormatTaxRate(tr.Key),
                    Value = _priceFormatter.FormatPrice(tr.Value, true, false),
                });
            }
            model.DisplayTaxRates = displayTaxRates;
            model.DisplayTax = displayTax;
            model.TaxValue = order.OrderTax;
            model.TaxRatesValue = order.TaxRates;

            model.CurrentTaxProvider = _taxService.LoadActiveTaxProvider(_storeContext.CurrentStore.Id).PluginDescriptor.SystemName;


            //discount
            if (order.OrderDiscount > 0)
                model.OrderTotalDiscount = _priceFormatter.FormatPrice(-order.OrderDiscount, true, false);
            model.OrderTotalDiscountValue = order.OrderDiscount;

            //gift cards
            foreach (var gcuh in order.GiftCardUsageHistory)
            {
                model.GiftCards.Add(new OrderModel.GiftCard
                {
                    CouponCode = gcuh.GiftCard.GiftCardCouponCode,
                    Amount = _priceFormatter.FormatPrice(-gcuh.UsedValue, true, false),
                });
            }

            //reward points
            if (order.RedeemedRewardPointsEntry != null)
            {
                model.RedeemedRewardPoints = -order.RedeemedRewardPointsEntry.Points;
                model.RedeemedRewardPointsAmount = _priceFormatter.FormatPrice(-order.RedeemedRewardPointsEntry.UsedAmount, true, false);
            }

            //total
            model.OrderTotal = _priceFormatter.FormatPrice(order.OrderTotal, true, false);
            model.OrderTotalOnRefund = _priceFormatter.FormatPrice(order.OrderTotalOnRefund, true, false);
            model.OrderTotalValue = order.OrderTotal;

            //refunded amount
            if (order.RefundedAmount > decimal.Zero)
                model.RefundedAmount = _priceFormatter.FormatPrice(order.RefundedAmount, true, false);

            //used discounts
            var duh = _discountService.GetAllDiscountUsageHistory(orderId: order.Id);
            foreach (var d in duh)
            {
                model.UsedDiscounts.Add(new OrderModel.UsedDiscountModel
                {
                    DiscountId = d.DiscountId,
                    DiscountName = d.Discount.Name
                });
            }

            //profit (hide for vendors)
            if (_workContext.CurrentVendor == null)
            {
                if (order.OrderTotal == order.RefundedAmount)
                {
                    model.Profit = _priceFormatter.FormatPrice(0m, true, false);
                }
                else
                {
                    var profit = _orderReportService.ProfitReport(orderId: order.Id);
                    model.Profit = _priceFormatter.FormatPrice(profit, true, false);
                }
            }

            #endregion

            #region Payment info

            if (order.AllowStoringCreditCardNumber)
            {
                //card type
                model.CardType = _encryptionService.DecryptText(order.CardType);
                //cardholder name
                model.CardName = _encryptionService.DecryptText(order.CardName);
                //card number
                model.CardNumber = _encryptionService.DecryptText(order.CardNumber);
                //cvv
                model.CardCvv2 = _encryptionService.DecryptText(order.CardCvv2);
                //expiry date
                string cardExpirationMonthDecrypted = _encryptionService.DecryptText(order.CardExpirationMonth);
                if (!String.IsNullOrEmpty(cardExpirationMonthDecrypted) && cardExpirationMonthDecrypted != "0")
                    model.CardExpirationMonth = cardExpirationMonthDecrypted;
                string cardExpirationYearDecrypted = _encryptionService.DecryptText(order.CardExpirationYear);
                if (!String.IsNullOrEmpty(cardExpirationYearDecrypted) && cardExpirationYearDecrypted != "0")
                    model.CardExpirationYear = cardExpirationYearDecrypted;

                model.AllowStoringCreditCardNumber = true;
            }
            else
            {
                string maskedCreditCardNumberDecrypted = _encryptionService.DecryptText(order.MaskedCreditCardNumber);
                if (!String.IsNullOrEmpty(maskedCreditCardNumberDecrypted))
                    model.CardNumber = maskedCreditCardNumberDecrypted;
            }


            //payment transaction info
            model.AuthorizationTransactionId = order.AuthorizationTransactionId;
            model.CaptureTransactionId = order.CaptureTransactionId;
            model.SubscriptionTransactionId = order.SubscriptionTransactionId;

            //payment method info
            var pm = _paymentService.LoadPaymentMethodBySystemName(order.PaymentMethodSystemName);
            model.PaymentMethod = pm != null ? pm.PluginDescriptor.FriendlyName : order.PaymentMethodSystemName;
            model.PaymentStatus = order.PaymentStatus.GetLocalizedEnum(_localizationService, _workContext);

            //payment method buttons
            model.CanCancelOrder = _orderProcessingService.CanCancelOrder(order);
            model.CanCapture = _orderProcessingService.CanCapture(order);
            model.CanMarkOrderAsPaid = _orderProcessingService.CanMarkOrderAsPaid(order);
            model.CanRefund = _orderProcessingService.CanRefund(order);
            model.CanRefundOffline = _orderProcessingService.CanRefundOffline(order);
            model.CanPartiallyRefund = _orderProcessingService.CanPartiallyRefund(order, decimal.Zero);
            model.CanCustomRefund = _orderProcessingService.CanCustomRefund(order, decimal.Zero);
            model.CanPartiallyRefundOffline = _orderProcessingService.CanPartiallyRefundOffline(order, decimal.Zero);
            model.CanVoid = _orderProcessingService.CanVoid(order);
            model.CanVoidOffline = _orderProcessingService.CanVoidOffline(order);
            model.UseVertexForRefund = order.ProcessRefundWithVertex;
            model.PrimaryStoreCurrencyCode = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId).CurrencyCode;
            model.MaxAmountToRefund = order.OrderTotal - order.RefundedAmount;

            //recurring payment record
            var recurringPayment = _orderService.SearchRecurringPayments(initialOrderId: order.Id, showHidden: true).FirstOrDefault();
            if (recurringPayment != null)
            {
                model.RecurringPaymentId = recurringPayment.Id;
            }
            #endregion

            #region Billing & shipping info

            model.BillingAddress = order.BillingAddress.ToModel();
            model.BillingAddress.FormattedCustomAddressAttributes = _addressAttributeFormatter.FormatAttributes(order.BillingAddress.CustomAttributes);
            model.BillingAddress.FirstNameEnabled = true;
            model.BillingAddress.FirstNameRequired = true;
            model.BillingAddress.LastNameEnabled = true;
            model.BillingAddress.LastNameRequired = true;
            model.BillingAddress.EmailEnabled = true;
            model.BillingAddress.EmailRequired = true;
            model.BillingAddress.CompanyEnabled = _addressSettings.CompanyEnabled;
            model.BillingAddress.CompanyRequired = _addressSettings.CompanyRequired;
            model.BillingAddress.CountryEnabled = _addressSettings.CountryEnabled;
            model.BillingAddress.StateProvinceEnabled = _addressSettings.StateProvinceEnabled;
            model.BillingAddress.CityEnabled = _addressSettings.CityEnabled;
            model.BillingAddress.CityRequired = _addressSettings.CityRequired;
            model.BillingAddress.StreetAddressEnabled = _addressSettings.StreetAddressEnabled;
            model.BillingAddress.StreetAddressRequired = _addressSettings.StreetAddressRequired;
            model.BillingAddress.StreetAddress2Enabled = _addressSettings.StreetAddress2Enabled;
            model.BillingAddress.StreetAddress2Required = _addressSettings.StreetAddress2Required;
            model.BillingAddress.ZipPostalCodeEnabled = _addressSettings.ZipPostalCodeEnabled;
            model.BillingAddress.ZipPostalCodeRequired = _addressSettings.ZipPostalCodeRequired;
            model.BillingAddress.PhoneEnabled = _addressSettings.PhoneEnabled;
            model.BillingAddress.PhoneRequired = _addressSettings.PhoneRequired;
            model.BillingAddress.FaxEnabled = _addressSettings.FaxEnabled;
            model.BillingAddress.FaxRequired = _addressSettings.FaxRequired;

            model.ShippingStatus = order.ShippingStatus.GetLocalizedEnum(_localizationService, _workContext); ;
            if (order.ShippingStatus != ShippingStatus.ShippingNotRequired)
            {
                model.IsShippable = true;

                model.PickUpInStore = order.PickUpInStore;
                if (order.ShippingAddress != null)
                {
                    if (!order.PickUpInStore)
                    {
                        model.ShippingAddress = order.ShippingAddress.ToModel();
                        model.ShippingAddress.FormattedCustomAddressAttributes = _addressAttributeFormatter.FormatAttributes(order.ShippingAddress.CustomAttributes);
                        model.ShippingAddress.FirstNameEnabled = true;
                        model.ShippingAddress.FirstNameRequired = true;
                        model.ShippingAddress.LastNameEnabled = true;
                        model.ShippingAddress.LastNameRequired = true;
                        model.ShippingAddress.EmailEnabled = true;
                        model.ShippingAddress.EmailRequired = true;
                        model.ShippingAddress.CompanyEnabled = _addressSettings.CompanyEnabled;
                        model.ShippingAddress.CompanyRequired = _addressSettings.CompanyRequired;
                        model.ShippingAddress.CountryEnabled = _addressSettings.CountryEnabled;
                        model.ShippingAddress.StateProvinceEnabled = _addressSettings.StateProvinceEnabled;
                        model.ShippingAddress.CityEnabled = _addressSettings.CityEnabled;
                        model.ShippingAddress.CityRequired = _addressSettings.CityRequired;
                        model.ShippingAddress.StreetAddressEnabled = _addressSettings.StreetAddressEnabled;
                        model.ShippingAddress.StreetAddressRequired = _addressSettings.StreetAddressRequired;
                        model.ShippingAddress.StreetAddress2Enabled = _addressSettings.StreetAddress2Enabled;
                        model.ShippingAddress.StreetAddress2Required = _addressSettings.StreetAddress2Required;
                        model.ShippingAddress.ZipPostalCodeEnabled = _addressSettings.ZipPostalCodeEnabled;
                        model.ShippingAddress.ZipPostalCodeRequired = _addressSettings.ZipPostalCodeRequired;
                        model.ShippingAddress.PhoneEnabled = _addressSettings.PhoneEnabled;
                        model.ShippingAddress.PhoneRequired = _addressSettings.PhoneRequired;
                        model.ShippingAddress.FaxEnabled = _addressSettings.FaxEnabled;
                        model.ShippingAddress.FaxRequired = _addressSettings.FaxRequired;

                        if (order.ShippingAddress != null)
                        {
                            string address1 = !String.IsNullOrEmpty(order.ShippingAddress.Address1) ? order.ShippingAddress.Address1 : "";
                            string zip = !String.IsNullOrEmpty(order.ShippingAddress.ZipPostalCode) ? order.ShippingAddress.ZipPostalCode : "";
                            string city = !String.IsNullOrEmpty(order.ShippingAddress.City) ? order.ShippingAddress.City : "";
                            var countryData = order.ShippingAddress.Country;
                            string country = (countryData != null && !String.IsNullOrEmpty(countryData.Name)) ? countryData.Name : "";

                            model.ShippingAddressGoogleMapsUrl = string.Format("http://maps.google.com/maps?f=q&hl=en&ie=UTF8&oe=UTF8&geocode=&q={0}", Server.UrlEncode(address1 + " " + zip + " " + city + " " + country));
                        }
                    }
                }
                else
                {
                    if (order.PickupAddress != null)
                    {
                        model.PickupAddress = order.PickupAddress.ToModel();
                        model.PickupAddressGoogleMapsUrl = string.Format("http://maps.google.com/maps?f=q&hl=en&ie=UTF8&oe=UTF8&geocode=&q={0}",
                            Server.UrlEncode(string.Format("{0} {1} {2} {3}", order.PickupAddress.Address1, order.PickupAddress.ZipPostalCode, order.PickupAddress.City,
                                order.PickupAddress.Country != null ? order.PickupAddress.Country.Name : string.Empty)));
                    }
                }
                model.ShippingMethod = order.ShippingMethod;

                model.CanAddNewShipments = order.HasItemsToAddToShipment();
            }

            #endregion

            #region Products

            model.CheckoutAttributeInfo = order.CheckoutAttributeDescription;
            bool hasDownloadableItems = false;
            List<OrderItem> allOrderItems = order.OrderItems.ToList();

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                allOrderItems = allOrderItems
                    .Where(orderItem => orderItem.Product.VendorId == _workContext.CurrentVendor.Id)
                    .ToList();
            }

            //foreach (var orderItem in allOrderItems)
            for (int i = 0; i <= allOrderItems.Count - 1; i++)
            {
               
                OrderItem orderItem = allOrderItems[i];
                decimal upriceInctx = 0.0M;
                decimal upriceExcTax = 0.0M;
                decimal priceInctx = 0.0M;
                decimal priceExcTax = 0.0M;
                Product parentBundleProduct = new Product();

                if (orderItem.IsBundleProduct)
                {
                    List<ShoppingCartBundleProductItem> scbpiList = _shoppingCartService.GetAssociatedProductsByOrderItemId(orderItem.Id).ToList();
                    int SHoppingCartItemId = scbpiList.First().ShoppingCartItemId;
                    int parentProductId = scbpiList.First().ParentProductId;

                    parentBundleProduct = _productService.GetProductById(parentProductId);
                    //orderItem.Product = pr;

                    if (i > 0 && allOrderItems[0].Order.Id == allOrderItems[i].Order.Id)
                    {
                        List<ShoppingCartBundleProductItem> scbp = _shoppingCartService.GetAssociatedProductsByOrderItemId(allOrderItems[0].Id).ToList();
                        if (parentProductId == scbp.First().ParentProductId)
                            continue;
                    }

                    List<ShoppingCartBundleProductItem> scbpiList2 = _shoppingCartService.GetAssociatedProductsPerShoppingCartItem(SHoppingCartItemId).ToList();
                    int count = scbpiList2.Count();
                   

                    foreach (ShoppingCartBundleProductItem sc in scbpiList2)
                    {
                        upriceInctx += orderItem.UnitPriceInclTax;
                        upriceExcTax += orderItem.UnitPriceExclTax;
                        priceInctx += orderItem.PriceInclTax;
                        priceExcTax += orderItem.PriceExclTax;
                    }
                }

                if (orderItem.Product.IsDownload)
                    hasDownloadableItems = true;

                var orderItemModel = new OrderModel.OrderItemModel
                {
                    Id = orderItem.Id,
                    ProductId = orderItem.IsBundleProduct ? parentBundleProduct.Id : orderItem.ProductId,
                    ProductName = orderItem.IsBundleProduct ? parentBundleProduct.Name : orderItem.Product.Name,
                    Sku = orderItem.Product.FormatSku(orderItem.AttributesXml, _productAttributeParser),
                    Quantity = orderItem.Quantity,
                    IsDownload = orderItem.Product.IsDownload,
                    DownloadCount = orderItem.DownloadCount,
                    DownloadActivationType = orderItem.Product.DownloadActivationType,
                    IsDownloadActivated = orderItem.IsDownloadActivated,
                    RequestedFulfillmentDateTime = orderItem.RequestedFulfillmentDateTime, // NU-31
                    FulfillmentDateTime = orderItem.FulfillmentDateTime, // NU-31
                    IsReservation = orderItem.IsReservation
                };

                if (orderItemModel.IsReservation)
                {
                    ReservedProduct reservedproduct = new ReservedProduct();
                    reservedproduct = _productService.GetReservedProductbyOrderItemId(orderItemModel.Id);
                    if (reservedproduct != null)
                    {
                        orderItemModel.ReservedProduct = reservedproduct;
                    }
                }


                //picture
                var orderItemPicture = orderItem.Product.GetProductPicture(orderItem.AttributesXml, _pictureService, _productAttributeParser);
                orderItemModel.PictureThumbnailUrl = _pictureService.GetPictureUrl(orderItemPicture, 75, true);

                //license file
                if (orderItem.LicenseDownloadId.HasValue)
                {
                    var licenseDownload = _downloadService.GetDownloadById(orderItem.LicenseDownloadId.Value);
                    if (licenseDownload != null)
                    {
                        orderItemModel.LicenseDownloadGuid = licenseDownload.DownloadGuid;
                    }
                }
                //vendor
                var vendor = _vendorService.GetVendorById(orderItem.Product.VendorId);
                orderItemModel.VendorName = vendor != null ? vendor.Name : "";

                //unit price
                orderItemModel.UnitPriceInclTaxValue = orderItem.IsBundleProduct ?  upriceInctx : orderItem.UnitPriceInclTax;
                orderItemModel.UnitPriceExclTaxValue = orderItem.IsBundleProduct ?  upriceExcTax : orderItem.UnitPriceExclTax;
                orderItemModel.UnitPriceInclTax = _priceFormatter.FormatPrice(orderItem.IsBundleProduct ? upriceInctx : orderItem.UnitPriceInclTax, true, primaryStoreCurrency, _workContext.WorkingLanguage, true, true);
                orderItemModel.UnitPriceExclTax = _priceFormatter.FormatPrice(orderItem.IsBundleProduct ? upriceExcTax : orderItem.UnitPriceExclTax, true, primaryStoreCurrency, _workContext.WorkingLanguage, false, true);
                
                //discounts
                orderItemModel.DiscountInclTaxValue = orderItem.DiscountAmountInclTax;
                orderItemModel.DiscountExclTaxValue = orderItem.DiscountAmountExclTax;
                orderItemModel.DiscountInclTax = _priceFormatter.FormatPrice(orderItem.DiscountAmountInclTax, true, primaryStoreCurrency, _workContext.WorkingLanguage, true, true);
                orderItemModel.DiscountExclTax = _priceFormatter.FormatPrice(orderItem.DiscountAmountExclTax, true, primaryStoreCurrency, _workContext.WorkingLanguage, false, true);
                
                //subtotal
                orderItemModel.SubTotalInclTaxValue = orderItem.IsBundleProduct ? priceInctx : orderItem.PriceInclTax;
                orderItemModel.SubTotalExclTaxValue = orderItem.IsBundleProduct ? priceExcTax : orderItem.PriceExclTax;
                orderItemModel.SubTotalInclTax = _priceFormatter.FormatPrice(orderItem.IsBundleProduct ? priceInctx : orderItem.PriceInclTax, true, primaryStoreCurrency, _workContext.WorkingLanguage, true, true);
                orderItemModel.SubTotalExclTax = _priceFormatter.FormatPrice(orderItem.IsBundleProduct ? priceExcTax : orderItem.PriceExclTax, true, primaryStoreCurrency, _workContext.WorkingLanguage, false, true);

                orderItemModel.AttributeInfo = orderItem.AttributeDescription;
                if (orderItem.Product.IsRecurring)
                    orderItemModel.RecurringInfo = string.Format(_localizationService.GetResource("Admin.Orders.Products.RecurringPeriod"), orderItem.Product.RecurringCycleLength, orderItem.Product.RecurringCyclePeriod.GetLocalizedEnum(_localizationService, _workContext));
                
                //rental info
                if (orderItem.Product.IsRental)
                {
                    var rentalStartDate = orderItem.RentalStartDateUtc.HasValue ? orderItem.Product.FormatRentalDate(orderItem.RentalStartDateUtc.Value) : "";
                    var rentalEndDate = orderItem.RentalEndDateUtc.HasValue ? orderItem.Product.FormatRentalDate(orderItem.RentalEndDateUtc.Value) : "";
                    orderItemModel.RentalInfo = string.Format(_localizationService.GetResource("Order.Rental.FormattedDate"),
                        rentalStartDate, rentalEndDate);
                }

                //return requests
                orderItemModel.ReturnRequests = _returnRequestService
                    .SearchReturnRequests(orderItemId: orderItem.Id)
                    .Select(item => new OrderModel.OrderItemModel.ReturnRequestBriefModel
                    {
                        CustomNumber = item.CustomNumber,
                        Id = item.Id
                    }).ToList();

                //gift cards
                orderItemModel.PurchasedGiftCardIds = _giftCardService.GetGiftCardsByPurchasedWithOrderItemId(orderItem.Id)
                    .Select(gc => gc.Id).ToList();


                model.Items.Add(orderItemModel);
            }
            model.HasDownloadableProducts = hasDownloadableItems;

            for (int i = 0; i < order.OrderItems.ToList().Count; i++)
            {
                var orderItem = order.OrderItems.ToList()[i];
                _orderItemRepository.Detach(orderItem);
            }

            #endregion
        }

        #region NU-31
        [HttpPost, ActionName("Edit")]
        [FormValueRequired(FormValueRequirement.StartsWith, "btnFullfillOpv")]
        [ValidateInput(false)]
        public ActionResult FulfillProduct(int id, FormCollection form)
        {

            //get order product variant identifier
            int orderItemId = 0;
            foreach (var formValue in form.AllKeys)
                if (formValue.StartsWith("btnFullfillOpv", StringComparison.InvariantCultureIgnoreCase))
                    orderItemId = Convert.ToInt32(formValue.Substring("btnFullfillOpv".Length));


            var orderItem = _orderService.GetOrderItemById(orderItemId);
            if (orderItem == null)
                throw new ArgumentException("No order item found with the specified id");


            ViewData["selectedTab"] = "products";

            try
            {
                _orderProcessingService.Fulfill(orderItem, true);
                var model = new OrderModel();
                PrepareOrderDetailsModel(model, orderItem.Order);
                return View(model);
            }
            catch (Exception exc)
            {
                //error
                var model = new OrderModel();
                PrepareOrderDetailsModel(model, orderItem.Order);
                ErrorNotification(exc, false);
                return View(model);
            }
        }
        #endregion

        [NonAction]
        protected virtual OrderModel.AddOrderProductModel.ProductDetailsModel PrepareAddProductToOrderModel(int orderId, int productId)
        {
            var product = _productService.GetProductById(productId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            var order = _orderService.GetOrderById(orderId);
            if (order == null)
                throw new ArgumentException("No order found with the specified id");

            var presetQty = 1;
            var presetPrice = _priceCalculationService.GetFinalPrice(product, order.Customer, decimal.Zero, true, presetQty);
            decimal taxRate;
            decimal presetPriceInclTax = _taxService.GetProductPrice(product, presetPrice, true, order.Customer, false, out taxRate);
            decimal presetPriceExclTax = _taxService.GetProductPrice(product, presetPrice, false, order.Customer, false, out taxRate);

            var model = new OrderModel.AddOrderProductModel.ProductDetailsModel
            {
                ProductId = productId,
                OrderId = orderId,
                Name = product.Name,
                ProductType = product.ProductType,
                UnitPriceExclTax = presetPriceExclTax,
                UnitPriceInclTax = presetPriceInclTax,
                Quantity = presetQty,
                SubTotalExclTax = presetPriceExclTax,
                SubTotalInclTax = presetPriceInclTax,
                AutoUpdateOrderTotals = _orderSettings.AutoUpdateOrderTotalsOnEditingOrder
            };

            //attributes
            var attributes = _productAttributeService.GetProductAttributeMappingsByProductId(product.Id);
            foreach (var attribute in attributes)
            {
                var attributeModel = new OrderModel.AddOrderProductModel.ProductAttributeModel
                {
                    Id = attribute.Id,
                    ProductAttributeId = attribute.ProductAttributeId,
                    Name = attribute.ProductAttribute.Name,
                    TextPrompt = attribute.TextPrompt,
                    IsRequired = attribute.IsRequired,
                    AttributeControlType = attribute.AttributeControlType,
                    HasCondition = !String.IsNullOrEmpty(attribute.ConditionAttributeXml)
                };
                if (!String.IsNullOrEmpty(attribute.ValidationFileAllowedExtensions))
                {
                    attributeModel.AllowedFileExtensions = attribute.ValidationFileAllowedExtensions
                        .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .ToList();
                }

                if (attribute.ShouldHaveValues())
                {
                    //values
                    var attributeValues = _productAttributeService.GetProductAttributeValues(attribute.Id);
                    foreach (var attributeValue in attributeValues)
                    {
                        var attributeValueModel = new OrderModel.AddOrderProductModel.ProductAttributeValueModel
                        {
                            Id = attributeValue.Id,
                            Name = attributeValue.Name,
                            IsPreSelected = attributeValue.IsPreSelected
                        };
                        attributeModel.Values.Add(attributeValueModel);
                    }
                }

                model.ProductAttributes.Add(attributeModel);
            }
            model.HasCondition = model.ProductAttributes.Any(a => a.HasCondition);
            //gift card
            model.GiftCard.IsGiftCard = product.IsGiftCard;
            if (model.GiftCard.IsGiftCard)
            {
                model.GiftCard.GiftCardType = product.GiftCardType;
            }
            //rental
            model.IsRental = product.IsRental;
            return model;
        }

        [NonAction]
        protected virtual ShipmentModel PrepareShipmentModel(Shipment shipment, bool prepareProducts, bool prepareShipmentEvent = false)
        {
            //measures
            var baseWeight = _measureService.GetMeasureWeightById(_measureSettings.BaseWeightId);
            var baseWeightIn = baseWeight != null ? baseWeight.Name : "";
            var baseDimension = _measureService.GetMeasureDimensionById(_measureSettings.BaseDimensionId);
            var baseDimensionIn = baseDimension != null ? baseDimension.Name : "";
            int vendorid = 0;
            foreach (var items in shipment.ShipmentItems)
            {
                var orderItem = _orderService.GetOrderItemById(items.OrderItemId);
                if (orderItem != null)
                {
                    if (_workContext.CurrentVendor == null)
                    {
                        vendorid = orderItem.Product.VendorId;
                    }
                }
            }

            var model = new ShipmentModel
            {
                Id = shipment.Id,
                OrderId = shipment.OrderId,
                TrackingNumber = shipment.TrackingNumber,
                TotalWeight = shipment.TotalWeight.HasValue ? string.Format("{0:F2} [{1}]", shipment.TotalWeight, baseWeightIn) : "",
                ShippedDate = shipment.ShippedDateUtc.HasValue ? _dateTimeHelper.ConvertToUserTime(shipment.ShippedDateUtc.Value, DateTimeKind.Utc).ToString() : _localizationService.GetResource("Admin.Orders.Shipments.ShippedDate.NotYet"),
                ShippedDateUtc = shipment.ShippedDateUtc,
                CanShip = !shipment.ShippedDateUtc.HasValue,
                DeliveryDate = shipment.DeliveryDateUtc.HasValue ? _dateTimeHelper.ConvertToUserTime(shipment.DeliveryDateUtc.Value, DateTimeKind.Utc).ToString() : _localizationService.GetResource("Admin.Orders.Shipments.DeliveryDate.NotYet"),
                DeliveryDateUtc = shipment.DeliveryDateUtc,
                CanDeliver = shipment.ShippedDateUtc.HasValue && !shipment.DeliveryDateUtc.HasValue,
                AdminComment = shipment.AdminComment,
                //------START: Codechages done by (na-sdxcorp\ADas)
                ShippingLabelImage = shipment.ShippingLabelImage,
                VendorId = vendorid
                //------END: Codechages done by (na-sdxcorp\ADas)
            };

            if (prepareProducts)
            {
                foreach (var shipmentItem in shipment.ShipmentItems)
                {
                    var orderItem = _orderService.GetOrderItemById(shipmentItem.OrderItemId);
                    if (orderItem == null)
                        continue;

                    //quantities
                    var qtyInThisShipment = shipmentItem.Quantity;
                    var maxQtyToAdd = orderItem.GetTotalNumberOfItemsCanBeAddedToShipment();
                    var qtyOrdered = orderItem.Quantity;
                    var qtyInAllShipments = orderItem.GetTotalNumberOfItemsInAllShipment();

                    var warehouse = _shippingService.GetWarehouseById(shipmentItem.WarehouseId);
                    var shipmentItemModel = new ShipmentModel.ShipmentItemModel
                    {
                        Id = shipmentItem.Id,
                        OrderItemId = orderItem.Id,
                        ProductId = orderItem.ProductId,
                        ProductName = orderItem.Product.Name,
                        Sku = orderItem.Product.FormatSku(orderItem.AttributesXml, _productAttributeParser),
                        AttributeInfo = orderItem.AttributeDescription,
                        ShippedFromWarehouse = warehouse != null ? warehouse.Name : null,
                        ShipSeparately = orderItem.Product.ShipSeparately,
                        ItemWeight = orderItem.ItemWeight.HasValue ? string.Format("{0:F2} [{1}]", orderItem.ItemWeight, baseWeightIn) : "",
                        ItemDimensions = string.Format("{0:F2} x {1:F2} x {2:F2} [{3}]", orderItem.Product.Length, orderItem.Product.Width, orderItem.Product.Height, baseDimensionIn),
                        QuantityOrdered = qtyOrdered,
                        QuantityInThisShipment = qtyInThisShipment,
                        QuantityInAllShipments = qtyInAllShipments,
                        QuantityToAdd = maxQtyToAdd,
                    };
                    //rental info
                    if (orderItem.Product.IsRental)
                    {
                        var rentalStartDate = orderItem.RentalStartDateUtc.HasValue ? orderItem.Product.FormatRentalDate(orderItem.RentalStartDateUtc.Value) : "";
                        var rentalEndDate = orderItem.RentalEndDateUtc.HasValue ? orderItem.Product.FormatRentalDate(orderItem.RentalEndDateUtc.Value) : "";
                        shipmentItemModel.RentalInfo = string.Format(_localizationService.GetResource("Order.Rental.FormattedDate"),
                            rentalStartDate, rentalEndDate);
                    }

                    model.Items.Add(shipmentItemModel);
                }
            }

            if (prepareShipmentEvent && !String.IsNullOrEmpty(shipment.TrackingNumber))
            {
                var shipmentTracker = shipment.GetShipmentTracker(_shippingService, _shippingSettings);
                if (shipmentTracker != null)
                {
                    model.TrackingNumberUrl = shipmentTracker.GetUrl(shipment.TrackingNumber);
                    if (_shippingSettings.DisplayShipmentEventsToStoreOwner)
                    {
                        var shipmentEvents = shipmentTracker.GetShipmentEvents(shipment.TrackingNumber);
                        if (shipmentEvents != null)
                            foreach (var shipmentEvent in shipmentEvents)
                            {
                                var shipmentStatusEventModel = new ShipmentModel.ShipmentStatusEventModel();
                                var shipmentEventCountry = _countryService.GetCountryByTwoLetterIsoCode(shipmentEvent.CountryCode);
                                shipmentStatusEventModel.Country = shipmentEventCountry != null
                                    ? shipmentEventCountry.GetLocalized(x => x.Name)
                                    : shipmentEvent.CountryCode;
                                shipmentStatusEventModel.Date = shipmentEvent.Date;
                                shipmentStatusEventModel.EventName = shipmentEvent.EventName;
                                shipmentStatusEventModel.Location = shipmentEvent.Location;
                                model.ShipmentStatusEvents.Add(shipmentStatusEventModel);
                            }
                    }
                }
            }

            return model;
        }

        #endregion

        #region Order list

        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        public ActionResult List(
            [ModelBinder(typeof(CommaSeparatedModelBinder))] List<string> orderStatusIds = null,
            [ModelBinder(typeof(CommaSeparatedModelBinder))] List<string> paymentStatusIds = null,
            [ModelBinder(typeof(CommaSeparatedModelBinder))] List<string> shippingStatusIds = null)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            //order statuses
            var model = new OrderListModel();
            model.isReadOnly = _workContext.CurrentCustomer.IsReadOnly();
            model.AvailableOrderStatuses = OrderStatus.Pending.ToSelectList(false).ToList();
            model.AvailableOrderStatuses.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0", Selected = true });
            if (orderStatusIds != null && orderStatusIds.Any())
            {
                foreach (var item in model.AvailableOrderStatuses.Where(os => orderStatusIds.Contains(os.Value)))
                    item.Selected = true;
                model.AvailableOrderStatuses.First().Selected = false;
            }
            //payment statuses
            model.AvailablePaymentStatuses = PaymentStatus.Pending.ToSelectList(false).ToList();
            model.AvailablePaymentStatuses.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0", Selected = true });
            if (paymentStatusIds != null && paymentStatusIds.Any())
            {
                foreach (var item in model.AvailablePaymentStatuses.Where(ps => paymentStatusIds.Contains(ps.Value)))
                    item.Selected = true;
                model.AvailablePaymentStatuses.First().Selected = false;
            }

            //shipping statuses
            model.AvailableShippingStatuses = ShippingStatus.NotYetShipped.ToSelectList(false).ToList();
            model.AvailableShippingStatuses.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0", Selected = true });
            if (shippingStatusIds != null && shippingStatusIds.Any())
            {
                foreach (var item in model.AvailableShippingStatuses.Where(ss => shippingStatusIds.Contains(ss.Value)))
                    item.Selected = true;
                model.AvailableShippingStatuses.First().Selected = false;
            }

            //stores
            model.AvailableStores.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            foreach (var s in _storeService.GetAllStores())
                model.AvailableStores.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString() });

            //vendors
            model.AvailableVendors.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            foreach (var v in _vendorService.GetAllVendors(showHidden: true)
                .OrderBy(v => v.Name)) /// NU-34
                model.AvailableVendors.Add(new SelectListItem { Text = v.Name, Value = v.Id.ToString() });

            //warehouses
            model.AvailableWarehouses.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            foreach (var w in _shippingService.GetAllWarehouses(_storeContext.CurrentStore.Id)
                .OrderBy(w => w.Name)) /// NU-35
                model.AvailableWarehouses.Add(new SelectListItem { Text = w.Name, Value = w.Id.ToString() });


            //fullfilment
            model.AvailableFulfillment.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            model.AvailableFulfillment.Add(new SelectListItem { Text = "Orders With Unfulfilled Items", Value = "1" });




            //payment methods
            model.AvailablePaymentMethods.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "" });
            foreach (var pm in _paymentService.LoadAllPaymentMethods()
                .OrderBy(p => p.PluginDescriptor.FriendlyName)) /// NU-34
                model.AvailablePaymentMethods.Add(new SelectListItem { Text = pm.PluginDescriptor.FriendlyName, Value = pm.PluginDescriptor.SystemName });

            //billing countries
            foreach (var c in _countryService.GetAllCountriesForBilling(showHidden: true))
            {
                model.AvailableCountries.Add(new SelectListItem { Text = c.Name, Value = c.Id.ToString() });
            }
            model.AvailableCountries.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });

            //a vendor should have access only to orders with his products
            model.IsLoggedInAsVendor = _workContext.CurrentVendor != null;
            model.IsLoggedAs = _workContext.CurrentCustomer.IsLoggedAs();	// NU-1

            return View(model);
        }

        [HttpPost]
        public ActionResult OrderList(DataSourceRequest command, OrderListModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                model.VendorId = _workContext.CurrentVendor.Id;
            }

            DateTime? startDateValue = (model.StartDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.StartDate.Value, _dateTimeHelper.CurrentTimeZone);

            DateTime? endDateValue = (model.EndDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.EndDate.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);

            var orderStatusIds = !model.OrderStatusIds.Contains(0) ? model.OrderStatusIds : null;
            var paymentStatusIds = !model.PaymentStatusIds.Contains(0) ? model.PaymentStatusIds : null;
            var shippingStatusIds = !model.ShippingStatusIds.Contains(0) ? model.ShippingStatusIds : null;
            var fulfillmentTypeId = model.FulfillmentTypeId;

            bool onlyUnfulfilled = false;

            if (fulfillmentTypeId == 1)
            {
                onlyUnfulfilled = true;
            }


            var filterByProductId = 0;
            var product = _productService.GetProductById(model.ProductId);
            if (product != null && HasAccessToProduct(product))
                filterByProductId = model.ProductId;

            int storeID = model.VendorId != 0 ? 0 : _storeContext.CurrentStore.Id;
            //load orders
            var orders = _orderService.SearchOrders(storeId: storeID, /// NU-36
                vendorId: model.VendorId,
                productId: filterByProductId,
                warehouseId: model.WarehouseId,
                paymentMethodSystemName: model.PaymentMethodSystemName,
                createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                osIds: orderStatusIds,
                psIds: paymentStatusIds,
                ssIds: shippingStatusIds,
                billingEmail: model.BillingEmail,
                billingLastName: model.BillingLastName,
                billingCountryId: model.BillingCountryId,
                onlyUnfulfilled: onlyUnfulfilled,
                orderNotes: model.OrderNotes,
                pageIndex: command.Page - 1,
                pageSize: command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = orders.Select(x =>
                {
                    var store = _storeService.GetStoreById(x.StoreId);
                    return new OrderModel
                    {
                        Id = x.Id,
                        StoreName = store != null ? store.Name : "Unknown",
                        OrderTotal = _priceFormatter.FormatPrice(x.OrderTotal, true, false),
                        OrderStatus = x.OrderStatus.GetLocalizedEnum(_localizationService, _workContext),
                        OrderStatusId = x.OrderStatusId,
                        PaymentStatus = x.PaymentStatus.GetLocalizedEnum(_localizationService, _workContext),
                        PaymentStatusId = x.PaymentStatusId,
                        ShippingStatus = x.ShippingStatus.GetLocalizedEnum(_localizationService, _workContext),
                        ShippingStatusId = x.ShippingStatusId,
                        CustomerEmail = x.BillingAddress.Email,
                        CustomerFullName = string.Format("{0} {1}", x.BillingAddress.FirstName, x.BillingAddress.LastName),
                        CreatedOn = _dateTimeHelper.ConvertToUserTime(x.CreatedOnUtc, DateTimeKind.Utc)


                    };
                }).OrderByDescending(o => o.CreatedOn), /// NU-37
                Total = orders.TotalCount
            };

            //summary report
            //currently we do not support productId and warehouseId parameters for this report
            var reportSummary = _orderReportService.GetOrderAverageReportLine(
                storeId: _storeContext.CurrentStore.Id,	/// NU-42
                vendorId: model.VendorId,
                orderId: 0,
                paymentMethodSystemName: model.PaymentMethodSystemName,
                osIds: orderStatusIds,
                psIds: paymentStatusIds,
                ssIds: shippingStatusIds,
                startTimeUtc: startDateValue,
                endTimeUtc: endDateValue,
                billingEmail: model.BillingEmail,
                billingLastName: model.BillingLastName,
                billingCountryId: model.BillingCountryId,
                orderNotes: model.OrderNotes);
            var profit = _orderReportService.ProfitReport(
                storeId: _storeContext.CurrentStore.Id,	/// NU-42
                vendorId: model.VendorId,
                paymentMethodSystemName: model.PaymentMethodSystemName,
                osIds: orderStatusIds,
                psIds: paymentStatusIds,
                ssIds: shippingStatusIds,
                startTimeUtc: startDateValue,
                endTimeUtc: endDateValue,
                billingEmail: model.BillingEmail,
                billingLastName: model.BillingLastName,
                billingCountryId: model.BillingCountryId,
                orderNotes: model.OrderNotes);
            var primaryStoreCurrency = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId);
            if (primaryStoreCurrency == null)
                throw new Exception("Cannot load primary store currency");

            gridModel.ExtraData = new OrderAggreratorModel
            {
                aggregatorprofit = _priceFormatter.FormatPrice(profit, true, false),
                aggregatorshipping = _priceFormatter.FormatShippingPrice(reportSummary.SumShippingExclTax, true, primaryStoreCurrency, _workContext.WorkingLanguage, false),
                aggregatortax = _priceFormatter.FormatPrice(reportSummary.SumTax, true, false),
                aggregatortotal = _priceFormatter.FormatPrice(reportSummary.SumOrders, true, false)
            };
            return new JsonResult
            {
                Data = gridModel
            };
        }

        [HttpPost, ActionName("List")]
        [FormValueRequired("go-to-order-by-number")]
        public ActionResult GoToOrderId(OrderListModel model)
        {
            var order = _orderService.GetOrderById(model.GoDirectlyToNumber);
            if (order == null)
                return List();

            return RedirectToAction("Edit", "Order", new { id = order.Id });
        }

        public ActionResult ProductSearchAutoComplete(string term)
        {
            const int searchTermMinimumLength = 3;
            if (String.IsNullOrWhiteSpace(term) || term.Length < searchTermMinimumLength)
                return Content("");

            //a vendor should have access only to his products
            var vendorId = 0;
            if (_workContext.CurrentVendor != null)
            {
                vendorId = _workContext.CurrentVendor.Id;
            }

            //products
            const int productNumber = 15;
            var products = _productService.SearchProducts(
                storeId: _storeContext.CurrentStore.Id,	// NU-36
                vendorId: vendorId,
                keywords: term,
                pageSize: productNumber,
                showHidden: true);

            var result = (from p in products
                          select new
                          {
                              label = p.Name,
                              productid = p.Id
                          })
                          .ToList();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Export / Import

        [HttpPost, ActionName("List")]
        [FormValueRequired("exportxml-all")]
        public ActionResult ExportXmlAll(OrderListModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            //a vendor cannot export orders
            if (_workContext.CurrentVendor != null)
                return AccessDeniedView();

            DateTime? startDateValue = (model.StartDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.StartDate.Value, _dateTimeHelper.CurrentTimeZone);

            DateTime? endDateValue = (model.EndDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.EndDate.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);

            var orderStatusIds = !model.OrderStatusIds.Contains(0) ? model.OrderStatusIds : null;
            var paymentStatusIds = !model.PaymentStatusIds.Contains(0) ? model.PaymentStatusIds : null;
            var shippingStatusIds = !model.ShippingStatusIds.Contains(0) ? model.ShippingStatusIds : null;

            var filterByProductId = 0;
            var product = _productService.GetProductById(model.ProductId);
            if (product != null && HasAccessToProduct(product))
                filterByProductId = model.ProductId;

            //load orders
            var orders = _orderService.SearchOrders(storeId: _storeContext.CurrentStore.Id,	/// NU-42
                vendorId: model.VendorId,
                productId: filterByProductId,
                warehouseId: model.WarehouseId,
                paymentMethodSystemName: model.PaymentMethodSystemName,
                createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                osIds: orderStatusIds,
                psIds: paymentStatusIds,
                ssIds: shippingStatusIds,
                billingEmail: model.BillingEmail,
                billingLastName: model.BillingLastName,
                billingCountryId: model.BillingCountryId,
                orderNotes: model.OrderNotes);

            try
            {
                var xml = _exportManager.ExportOrdersToXml(orders);
                return new XmlDownloadResult(xml, "orders.xml");
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("List");
            }
        }

        [HttpPost]
        public ActionResult ExportXmlSelected(string selectedIds)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            //a vendor cannot export orders
            if (_workContext.CurrentVendor != null)
                return AccessDeniedView();

            var orders = new List<Order>();
            if (selectedIds != null)
            {
                var ids = selectedIds
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => Convert.ToInt32(x))
                    .ToArray();
                orders.AddRange(_orderService.GetOrdersByIds(ids));
            }

            var xml = _exportManager.ExportOrdersToXml(orders);
            return new XmlDownloadResult(xml, "orders.xml");
        }

        //---START: Codechages done by (na-sdxcorp\ADas)--------------
        [HttpPost, ActionName("List")]
        [FormValueRequired("exportshipmentexcel-all")]
        public ActionResult ExportShipmentExcelAll(OrderListModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            //a vendor cannot export orders
            // if (_workContext.CurrentVendor != null)
            //     return AccessDeniedView();

            DateTime? startDateValue = (model.StartDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.StartDate.Value, _dateTimeHelper.CurrentTimeZone);

            DateTime? endDateValue = (model.EndDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.EndDate.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);

            var orderStatusIds = !model.OrderStatusIds.Contains(0) ? model.OrderStatusIds : null;
            var paymentStatusIds = !model.PaymentStatusIds.Contains(0) ? model.PaymentStatusIds : null;
            var shippingStatusIds = !model.ShippingStatusIds.Contains(0) ? model.ShippingStatusIds : null;

            var filterByProductId = 0;
            var product = _productService.GetProductById(model.ProductId);
            if (product != null && HasAccessToProduct(product))
                filterByProductId = model.ProductId;

            //load orders
            var orders = _orderService.SearchOrders(storeId: _storeContext.CurrentStore.Id,	/// NU-42
                vendorId: model.VendorId,
                productId: filterByProductId,
                warehouseId: model.WarehouseId,
                paymentMethodSystemName: model.PaymentMethodSystemName,
                createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                osIds: orderStatusIds,
                psIds: paymentStatusIds,
                ssIds: shippingStatusIds,
                billingEmail: model.BillingEmail,
                billingLastName: model.BillingLastName,
                billingCountryId: model.BillingCountryId,
                orderNotes: model.OrderNotes);

            //Populate all ShipmentReports from orders
            var shipmentReports = new List<ShipmentReport>();
            shipmentReports = _shippingService.GetShipmentReports(orders);

            try
            {
                byte[] bytes = _exportManager.ExportShipmentReportToXlsx(shipmentReports);
                return File(bytes, MimeTypes.TextXlsx, "shipmentReports.xlsx");
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("List");
            }
        }

        [HttpPost]
        public ActionResult ExportShipmentExcelSelected(string selectedIds)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            //a vendor cannot export orders
            if (_workContext.CurrentVendor != null)
                return AccessDeniedView();

            var orders = new List<Order>();
            if (selectedIds != null)
            {
                var ids = selectedIds
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => Convert.ToInt32(x))
                    .ToArray();
                orders.AddRange(_orderService.GetOrdersByIds(ids));
            }

            //Populate all ShipmentReports from orders
            var shipmentReports = new List<ShipmentReport>();
            shipmentReports = _shippingService.GetShipmentReports(orders);

            try
            {
                byte[] bytes = _exportManager.ExportShipmentReportToXlsx(shipmentReports);
                return File(bytes, MimeTypes.TextXlsx, "shipmentReports.xlsx");
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("List");
            }
        }

        //---END: Codechages done by (na-sdxcorp\ADas)--------------

        [HttpPost, ActionName("List")]
        [FormValueRequired("exportexcel-all")]
        public ActionResult ExportExcelAll(OrderListModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            //a vendor cannot export orders
            if (_workContext.CurrentVendor != null)
                return AccessDeniedView();

            DateTime? startDateValue = (model.StartDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.StartDate.Value, _dateTimeHelper.CurrentTimeZone);

            DateTime? endDateValue = (model.EndDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.EndDate.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);

            var orderStatusIds = !model.OrderStatusIds.Contains(0) ? model.OrderStatusIds : null;
            var paymentStatusIds = !model.PaymentStatusIds.Contains(0) ? model.PaymentStatusIds : null;
            var shippingStatusIds = !model.ShippingStatusIds.Contains(0) ? model.ShippingStatusIds : null;

            var filterByProductId = 0;
            var product = _productService.GetProductById(model.ProductId);
            if (product != null && HasAccessToProduct(product))
                filterByProductId = model.ProductId;

            //load orders
            var orders = _orderService.SearchOrders(storeId: _storeContext.CurrentStore.Id,	/// NU-42
                vendorId: model.VendorId,
                productId: filterByProductId,
                warehouseId: model.WarehouseId,
                paymentMethodSystemName: model.PaymentMethodSystemName,
                createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                osIds: orderStatusIds,
                psIds: paymentStatusIds,
                ssIds: shippingStatusIds,
                billingEmail: model.BillingEmail,
                billingLastName: model.BillingLastName,
                billingCountryId: model.BillingCountryId,
                orderNotes: model.OrderNotes);



            try
            {
                byte[] bytes = _exportManager.ExportOrdersToXlsx(orders);
                return File(bytes, MimeTypes.TextXlsx, "orders.xlsx");
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("List");
            }
        }

        [HttpPost]
        public ActionResult ExportExcelSelected(string selectedIds)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            //a vendor cannot export orders
            if (_workContext.CurrentVendor != null)
                return AccessDeniedView();

            var orders = new List<Order>();
            if (selectedIds != null)
            {
                var ids = selectedIds
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => Convert.ToInt32(x))
                    .ToArray();
                orders.AddRange(_orderService.GetOrdersByIds(ids));
            }

            try
            {
                byte[] bytes = _exportManager.ExportOrdersToXlsx(orders);
                return File(bytes, MimeTypes.TextXlsx, "orders.xlsx");
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("List");
            }
        }

        [HttpPost, ActionName("List")]
        [FormValueRequired("exportexcel-donor")]
        public ActionResult ExportDonorReportExcel(OrderListModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            //a vendor cannot export orders
            if (_workContext.CurrentVendor != null)
                return AccessDeniedView();

            DateTime? startDateValue = (model.StartDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.StartDate.Value, _dateTimeHelper.CurrentTimeZone);

            DateTime? endDateValue = (model.EndDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.EndDate.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);

            var DonationModel = new List<DonationModel>();

            var donationModelList = _donationService.GetDonationsForExport(_storeContext.CurrentStore.Id, null, true, startDateValue, endDateValue);

            try
            {
                byte[] bytes = _exportManager.ExportDonationsToXlsx(donationModelList, true);
                return File(bytes, MimeTypes.TextXlsx, "DonorReport.xlsx");
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("List");
            }
        }


        #endregion

        #region Order details

        #region Payments and other order workflow

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("cancelorder")]
        public ActionResult CancelOrder(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var order = _orderService.GetOrderById(id);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null)
                return RedirectToAction("Edit", "Order", new { id = id });

            try
            {
                _orderProcessingService.CancelOrder(order, true);

                #region NU-63
                //foreach (OrderItem orderItem in order.OrderItems)
                //{
                //    _SAPService.CreateEntry(orderItem, 4);
                //}
                #endregion

                LogEditOrder(order.Id);
                var model = new OrderModel();
                PrepareOrderDetailsModel(model, order);
                return View(model);
            }
            catch (Exception exc)
            {
                //error
                var model = new OrderModel();
                PrepareOrderDetailsModel(model, order);
                ErrorNotification(exc, false);
                return View(model);
            }
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("captureorder")]
        public ActionResult CaptureOrder(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var order = _orderService.GetOrderById(id);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null)
                return RedirectToAction("Edit", "Order", new { id = id });

            try
            {
                var errors = _orderProcessingService.Capture(order);
                LogEditOrder(order.Id);
                var model = new OrderModel();
                PrepareOrderDetailsModel(model, order);
                foreach (var error in errors)
                    ErrorNotification(error, false);
                return View(model);
            }
            catch (Exception exc)
            {
                //error
                var model = new OrderModel();
                PrepareOrderDetailsModel(model, order);
                ErrorNotification(exc, false);
                return View(model);
            }

        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("markorderaspaid")]
        public ActionResult MarkOrderAsPaid(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var order = _orderService.GetOrderById(id);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null)
                return RedirectToAction("Edit", "Order", new { id = id });

            try
            {
                _orderProcessingService.MarkOrderAsPaid(order);
                LogEditOrder(order.Id);
                var model = new OrderModel();
                PrepareOrderDetailsModel(model, order);
                return View(model);
            }
            catch (Exception exc)
            {
                //error
                var model = new OrderModel();
                PrepareOrderDetailsModel(model, order);
                ErrorNotification(exc, false);
                return View(model);
            }
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("refundorder")]
        public ActionResult RefundOrder(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var order = _orderService.GetOrderById(id);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null)
                return RedirectToAction("Edit", "Order", new { id = id });

            try
            {
                var errors = _orderProcessingService.Refund(order);
                _orderProcessingService.ApplyJEBusinessRulesFullReFund(id);

                var orderItemList1 = order.OrderItems.Where(x => x.OrderId == order.Id).ToList();
                if (order.PaymentStatus != PaymentStatus.Pending)
                {
                    _orderProcessingService.CancelOrder(order, true);
                }
                var isCancelled = order.OrderStatus == OrderStatus.Cancelled;

                //var itemToFulfill = orderItemList1.Where(x => x.Id == orderItemId).FirstOrDefault();

                foreach (var items in orderItemList1)
                {
                    _orderProcessingService.InsertGlCodeCalcs(order, items, true, true);
                    _orderProcessingService.InsertIntoSAPSalesJournalOrderItem(order, items, 3);
                    items.DateOfRefund = DateTime.Now;
                    _orderItemRepository.Update(items);
                }

                LogEditOrder(order.Id);
                var model = new OrderModel();
                PrepareOrderDetailsModel(model, order);
                foreach (var error in errors)
                    ErrorNotification(error, false);
                return View(model);
            }
            catch (Exception exc)
            {
                //error
                var model = new OrderModel();
                PrepareOrderDetailsModel(model, order);
                ErrorNotification(exc, false);
                return View(model);
            }
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("refundorderoffline")]
        public ActionResult RefundOrderOffline(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var order = _orderService.GetOrderById(id);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null)
                return RedirectToAction("Edit", "Order", new { id = id });

            try
            {
                _orderProcessingService.RefundOffline(order);
                LogEditOrder(order.Id);
                var model = new OrderModel();
                PrepareOrderDetailsModel(model, order);
                return View(model);
            }
            catch (Exception exc)
            {
                //error
                var model = new OrderModel();
                PrepareOrderDetailsModel(model, order);
                ErrorNotification(exc, false);
                return View(model);
            }
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("voidorder")]
        public ActionResult VoidOrder(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var order = _orderService.GetOrderById(id);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null)
                return RedirectToAction("Edit", "Order", new { id = id });

            try
            {
                var errors = _orderProcessingService.Void(order);
                LogEditOrder(order.Id);
                var model = new OrderModel();
                PrepareOrderDetailsModel(model, order);
                foreach (var error in errors)
                    ErrorNotification(error, false);
                return View(model);
            }
            catch (Exception exc)
            {
                //error
                var model = new OrderModel();
                PrepareOrderDetailsModel(model, order);
                ErrorNotification(exc, false);
                return View(model);
            }
        }


        [HttpPost, ActionName("Edit")]
        [FormValueRequired("voidorderoffline")]
        public ActionResult VoidOrderOffline(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var order = _orderService.GetOrderById(id);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null)
                return RedirectToAction("Edit", "Order", new { id = id });

            try
            {
                _orderProcessingService.VoidOffline(order);
                LogEditOrder(order.Id);
                var model = new OrderModel();
                PrepareOrderDetailsModel(model, order);
                return View(model);
            }
            catch (Exception exc)
            {
                //error
                var model = new OrderModel();
                PrepareOrderDetailsModel(model, order);
                ErrorNotification(exc, false);
                return View(model);
            }
        }
        public ActionResult CustomRefundOrderPopup(int id, bool online)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var order = _orderService.GetOrderById(id);
            if (order == null)
                //    //No order found with the specified id
                return RedirectToAction("List");

            ////a vendor does not have access to this functionality
            //if (_workContext.CurrentVendor != null)
            //    return RedirectToAction("Edit", "Order", new { id = id });
            var glCodesPaid = _glCodeService.GetGlCodes(GLCodeStatusType.Paid);
            glCodesPaid.Distinct().Select(x => x.GlCodeName);

            var model = new Nop.Admin.Models.Orders.OrderModel.CustomRefundData();
            model.OrderId = order.Id;

            foreach (var glcode in glCodesPaid)
            {
                model.AvailablePaidGLCodes1.Add(new SelectListItem
                {
                    Text = glcode.GlCodeName + "-" + glcode.Description,
                    Value = glcode.GlCodeName
                    //Selected = model.SelectedPaidGlCodeIds.Contains(glcode.Id)
                });

                model.AvailablePaidGLCodes2.Add(new SelectListItem
                {
                    Text = glcode.GlCodeName + "-" + glcode.Description,
                    Value = glcode.GlCodeName
                    //Selected = model.SelectedPaidGlCodeIds.Contains(glcode.Id)
                });
                model.AvailablePaidGLCodes3.Add(new SelectListItem
                {
                    Text = glcode.GlCodeName + "-" + glcode.Description,
                    Value = glcode.GlCodeName
                    //Selected = model.SelectedPaidGlCodeIds.Contains(glcode.Id)
                });
            }
            return View(model);
        }
        public ActionResult PartiallyRefundOrderPopup(int id, bool online)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var order = _orderService.GetOrderById(id);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null)
                return RedirectToAction("Edit", "Order", new { id = id });

            var model = new OrderModel();
            PrepareOrderDetailsModel(model, order);

            return View(model);
        }

        [HttpPost]
        [FormValueRequired("partialrefundorder")]
        public ActionResult PartiallyRefundOrderPopup(string btnId, string formId, int id, bool online, OrderModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var order = _orderService.GetOrderById(id);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null)
                return RedirectToAction("Edit", "Order", new { id = id });

            try
            {
                decimal amountToRefund = model.AmountToRefund;
                if (amountToRefund <= decimal.Zero)
                    throw new NopException("Enter amount to refund");

                decimal maxAmountToRefund = order.OrderTotal - order.RefundedAmount;
                if (amountToRefund > maxAmountToRefund)
                    amountToRefund = maxAmountToRefund;

                var errors = new List<string>();
                if (online)
                    errors = _orderProcessingService.PartiallyRefund(order, amountToRefund).ToList();
                else
                    _orderProcessingService.PartiallyRefundOffline(order, amountToRefund);

                LogEditOrder(order.Id);

                if (!errors.Any())
                {
                    //success
                    ViewBag.RefreshPage = true;
                    ViewBag.btnId = btnId;
                    ViewBag.formId = formId;

                    PrepareOrderDetailsModel(model, order);
                    return View(model);
                }

                //error
                PrepareOrderDetailsModel(model, order);
                foreach (var error in errors)
                    ErrorNotification(error, false);
                return View(model);
            }
            catch (Exception exc)
            {
                //error
                PrepareOrderDetailsModel(model, order);
                ErrorNotification(exc, false);
                return View(model);
            }
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("btnSaveOrderStatus")]
        public ActionResult ChangeOrderStatus(int id, OrderModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var order = _orderService.GetOrderById(id);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null)
                return RedirectToAction("Edit", "Order", new { id = id });

            try
            {
                order.OrderStatusId = model.OrderStatusId;
                _orderService.UpdateOrder(order);

                //add a note
                order.OrderNotes.Add(new OrderNote
                {
                    Note = string.Format("Order status has been edited. New status: {0}", order.OrderStatus.GetLocalizedEnum(_localizationService, _workContext)),
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow
                });
                _orderService.UpdateOrder(order);
                LogEditOrder(order.Id);

                if (order.OrderStatusId == 30)
                {
                    SaveIntoSapJournal(order, 2);
                }
                else
                {
                    if (order.OrderStatusId == 40)
                    {
                        SaveIntoSapJournal(order, 3);
                    }
                }

                model = new OrderModel();
                PrepareOrderDetailsModel(model, order);
                return View(model);
            }
            catch (Exception exc)
            {
                //error
                model = new OrderModel();
                PrepareOrderDetailsModel(model, order);
                ErrorNotification(exc, false);
                return View(model);
            }
        }

        public void UpdateSAPJournal(int OrderId)
        {
            string SqlQuery = "Update SAP_SalesJournal set IsProcessed = 1,ProcessedDate = @date where OrderNumber = " + OrderId;

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
            catch (Exception ex)
            {

            }
        }

        #endregion

        #region Edit, delete

        public ActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var order = _orderService.GetOrderById(id);
            if (order == null || order.Deleted)
                //No order found with the specified id
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null && !HasAccessToOrder(order))
                return RedirectToAction("List");

            var model = new OrderModel();
            PrepareOrderDetailsModel(model, order);

            string data = System.Configuration.ConfigurationManager.AppSettings["VertexEnabledForStoreId"]; // Applying check to enable Partial Refunds for only Pilot accounts
            if (data.Contains(_storeContext.CurrentStore.Id.ToString()))
                model.IsPilot = true;

            var warnings = TempData["nop.admin.order.warnings"] as List<string>;
            if (warnings != null)
                model.Warnings = warnings;

            model.IsReadOnly = _workContext.CurrentCustomer.IsReadOnly();

            return View(model);
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var order = _orderService.GetOrderById(id);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null)
                return RedirectToAction("Edit", "Order", new { id = id });

            _orderProcessingService.DeleteOrder(order);

            //activity log
            _customerActivityService.InsertActivity("DeleteOrder", _localizationService.GetResource("ActivityLog.DeleteOrder"), order.Id);

            return RedirectToAction("List");
        }

        public ActionResult PdfInvoice(int orderId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null)
                return RedirectToAction("Edit", "Order", new { id = orderId });

            var order = _orderService.GetOrderById(orderId);
            var orders = new List<Order>();
            orders.Add(order);
            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                _pdfService.PrintOrdersToPdf(stream, orders, _orderSettings.GeneratePdfInvoiceInCustomerLanguage ? 0 : _workContext.WorkingLanguage.Id);
                bytes = stream.ToArray();
            }
            return File(bytes, MimeTypes.ApplicationPdf, string.Format("order_{0}.pdf", order.Id));
        }


        [HttpPost, ActionName("List")]
        [FormValueRequired("payment-report-all")]
        public ActionResult PaymentReportAll(OrderListModel model)
        {

            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                model.VendorId = _workContext.CurrentVendor.Id;
            }

            DateTime? startDateValue = (model.StartDate == null) ? new DateTime?(DateTime.UtcNow.Date)
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.StartDate.Value, _dateTimeHelper.CurrentTimeZone);

            DateTime? endDateValue = (model.EndDate == null) ? new DateTime?(DateTime.UtcNow.Date.AddDays(1.0))
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.EndDate.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);


            TimeZoneInfo storeTimeZone = _dateTimeHelper.GetStoreTimeZone(_storeContext.CurrentStore);
            try
            {


                //var orders = _orderService.SearchOrders(0, 0, 0, 0, 0, 0, 0, null, startDateValue, endDateValue, null, null, null, null, null, null, false, false, 0, int.MaxValue);
                var orders = _orderService.SearchOrders(_storeContext.CurrentStore.Id, 0, 0, 0, 0, 0, 0, null, startDateValue, endDateValue, null, null, null, null, null, null, false, false, 0, int.MaxValue);


                string fileName = string.Format("payments_{0}_{1}.xlsx", DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"), CommonHelper.GenerateRandomDigitCode(4));
                string filePath = string.Format("{0}content\\files\\ExportImport\\{1}", Request.PhysicalApplicationPath, fileName);

                _exportManager.ExportPaymentReport(filePath, orders, DateTime.Now, DateTime.Now, "Daily Payment Report", false, _storeContext.CurrentStore.Name, false);

                var bytes = System.IO.File.ReadAllBytes(filePath);
                return File(bytes, "text/xls", fileName);
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("List");
            }
        }

        [HttpPost, ActionName("List")]
        [FormValueRequired("reconciliation-report")]
        public ActionResult ReconciliationReport(OrderListModel model)
        {
            //if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
            //    return AccessDeniedView();

            ////DateTime? startDateValue = (model.StartDate == null) ? new DateTime?(DateTime.UtcNow.Date)
            ////                         : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.StartDate.Value, _dateTimeHelper.CurrentTimeZone);

            //DateTime? endDateValue = (model.EndDate == null) ? new DateTime?(DateTime.UtcNow.Date.AddDays(1.0))
            //                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.EndDate.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);


            //TimeZoneInfo storeTimeZone = _dateTimeHelper.GetStoreTimeZone(_storeContext.CurrentStore);

            //try
            //{
            //    // IPagedList<Order> FutureDeliveryReportorders = _orderService.SearchOrders(0, 0, 0, 0, 0, 0, 0, null, dateTime1, dateTime2, null, null, null, null, null, null, false, true, 0, int.MaxValue); //Retreiving Future Reports Orders
            //    //IPagedList<Order> PaymentReportorders = _orderService.SearchOrders(0, 0, 0, 0, 0, 0, 0, null, startDateValue, endDateValue, null, null, null, null, null, null, false, true, 0, int.MaxValue); //Retreiving Payment Reports Orders
            //    IPagedList<Order> PaymentReportorders = _orderService.SearchOrders(0, 0, 0, 0, 0, 0, 0, null, null, endDateValue, null, null, null, null, null, null, false, true, 0, int.MaxValue);
            //    string fileName = string.Format("Reconciliation_Report_{0}_{1}.xlsx", DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"), CommonHelper.GenerateRandomDigitCode(4));
            //    string filePath = string.Format("{0}content\\files\\ExportImport\\{1}", Request.PhysicalApplicationPath, fileName);

            //    _exportManager.ExportPaymentReportToXlsxVERTEX(filePath, PaymentReportorders, DateTime.Now, DateTime.Now, "Consolidated Payment and Future Delivery Report", false, _storeContext.CurrentStore.Name);
            //    var bytes = System.IO.File.ReadAllBytes(filePath);
            //    return File(bytes, "text/xls", fileName);
            //}
            //catch (Exception exc)
            //{
            //    ErrorNotification(exc);
            //    return RedirectToAction("List");
            //}

            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            //Need to enable it if startdate needs to enabled in future
            DateTime? startDateValue = (model.StartDate == null) ? new DateTime?(DateTime.UtcNow.Date)
                                     : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.StartDate.Value, _dateTimeHelper.CurrentTimeZone);


            DateTime? endDateValue = (model.EndDate == null) ? new DateTime?(DateTime.UtcNow.Date.AddDays(1.0))
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.EndDate.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);


            TimeZoneInfo storeTimeZone = _dateTimeHelper.GetStoreTimeZone(_storeContext.CurrentStore);

            try
            {
                // IPagedList<Order> FutureDeliveryReportorders = _orderService.SearchOrders(0, 0, 0, 0, 0, 0, 0, null, dateTime1, dateTime2, null, null, null, null, null, null, false, true, 0, int.MaxValue); //Retreiving Future Reports Orders
                IPagedList<Order> PaymentReportorders = _orderService.SearchOrders(0, 0, 0, 0, 0, 0, 0, null, startDateValue, endDateValue, null, null, null, null, null, null, false, false, 0, int.MaxValue);

                string fileName = string.Format("Reconciliation_Report_{0}_{1}.xlsx", DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"), CommonHelper.GenerateRandomDigitCode(4));
                string filePath = string.Format("{0}content\\files\\ExportImport\\{1}", Request.PhysicalApplicationPath, fileName);
                _exportManager.ExportPaymentReportToXlsxVERTEX(filePath, PaymentReportorders, DateTime.Now, DateTime.Now, "Reconciliation_Report", true, _storeContext.CurrentStore.Name, false, false, startDateValue, endDateValue);
                var bytes = System.IO.File.ReadAllBytes(filePath);
                return File(bytes, "text/xls", fileName);
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("List");
            }
        }



        [HttpPost, ActionName("List")]
        [FormValueRequired("delivery-report-all")]
        public ActionResult DeliveryReportAll(OrderListModel model)
        {

            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                model.VendorId = _workContext.CurrentVendor.Id;
            }

            DateTime? startDateValue = (model.StartDate == null) ? new DateTime?(DateTime.UtcNow.Date)
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.StartDate.Value, _dateTimeHelper.CurrentTimeZone);

            DateTime? endDateValue = (model.EndDate == null) ? new DateTime?(DateTime.UtcNow.Date.AddDays(1.0))
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.EndDate.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);




            TimeZoneInfo storeTimeZone = _dateTimeHelper.GetStoreTimeZone(_storeContext.CurrentStore);
            try
            {


                //var orders = _orderService.SearchOrders(0, 0, 0, 0, 0, 0, 0, null, startDateValue, endDateValue, null, null, null, null, null, null, true, false, 0, int.MaxValue);
                var orders = _orderService.SearchOrders(_storeContext.CurrentStore.Id, 0, 0, 0, 0, 0, 0, null, startDateValue, endDateValue, null, null, null, null, null, null, true, false, 0, int.MaxValue);


                var unfulfilled = new List<Order>();
                string fileName = string.Format("delivery_{0}_{1}.xlsx", DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"), CommonHelper.GenerateRandomDigitCode(4));
                string filePath = string.Format("{0}content\\files\\ExportImport\\{1}", Request.PhysicalApplicationPath, fileName);

                //_exportManager.ExportDeliveryReportToXlsxVERTEX(filePath, orders, DateTime.Now, DateTime.Now, "Daily Delivery Report", false, _storeContext.CurrentStore.Name);
                _exportManager.ExportPaymentReportToXlsxVERTEX(filePath, orders, DateTime.Now, DateTime.Now, "Daily Delivery Report", false, _storeContext.CurrentStore.Name, false);

                var bytes = System.IO.File.ReadAllBytes(filePath);
                return File(bytes, "text/xls", fileName);
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("List");
            }



            //if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
            //    return AccessDeniedView();

            ////a vendor should have access only to his products
            //if (_workContext.CurrentVendor != null)
            //{
            //    model.VendorId = _workContext.CurrentVendor.Id;
            //}

            //DateTime? startDateValue = (model.StartDate == null) ? null
            //                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.StartDate.Value, _dateTimeHelper.CurrentTimeZone);

            //DateTime? endDateValue = (model.EndDate == null) ? null
            //                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.EndDate.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);

            //var orderStatusIds = !model.OrderStatusIds.Contains(0) ? model.OrderStatusIds : null;
            //var paymentStatusIds = !model.PaymentStatusIds.Contains(0) ? model.PaymentStatusIds : null;
            //var shippingStatusIds = !model.ShippingStatusIds.Contains(0) ? model.ShippingStatusIds : null;


            //bool getFutureDeliveries = false;

            //TimeZoneInfo storeTimeZone = _dateTimeHelper.GetStoreTimeZone(_storeContext.CurrentStore);
            //try
            //{

            //    //var  orderStatusId = null;
            //    // var paymentStatusId = null;
            //    // var shippingStatusId =  null;
            //    //var  billingEmail =  null;
            //    // var orderGuid =  null;

            //    IList<OrderExcelExport> orders = this._reportingService.DeliveriesForExcel(this._storeContext.CurrentStore.Id, startDateValue, endDateValue, 0, int.MaxValue, getFutureDeliveries);
            //    IList<GlCode> glCodesForReport = this._reportingService.GetGlCodesForReport(true);
            //    string format = "deliveries_{0}_{1}.xlsx";
            //    var dateTime = DateTime.Now;
            //    string str1 = dateTime.ToString("yyyy-MM-dd-HH-mm-ss");
            //    string randomDigitCode = CommonHelper.GenerateRandomDigitCode(4);
            //    string 8 = string.Format(format, (object)str1, (object)randomDigitCode);
            //    string str2 = string.Format("{0}content\\files\\ExportImport\\{1}", (object)this.Request.PhysicalApplicationPath, (object)fileDownloadName);
            //    this._exportManager.ExportDeliveryReportToXlsx(str2, orders, glCodesForReport, startDateValue, endDateValue, this._storeContext.CurrentStore.Name);

            //    var bytes = System.IO.File.ReadAllBytes(str2);

            //    return this.File(bytes, "text/xls", fileDownloadName);
            //}
            //catch (Exception exc)
            //{
            //    ErrorNotification(exc);
            //    return RedirectToAction("List");
            //}
        }

        [HttpPost, ActionName("List")]
        [FormValueRequired("future-report-all")]
        public ActionResult FutureReportAll(OrderListModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();


            DateTime dateTime1 = new DateTime(1900, 1, 1);
            DateTime dateTime2 = DateTime.UtcNow;
            dateTime2 = dateTime2.AddDays(800.0);
            DateTime dateTime3 = dateTime2.AddTicks(-1L);
            try
            {


                //var orders = _orderService.SearchOrders(0, 0, 0, 0, 0, 0, 0, null, dateTime1, dateTime2, null, null, null, null, null, null, false, true, 0, int.MaxValue);
                var orders = _orderService.SearchOrders(_storeContext.CurrentStore.Id, 0, 0, 0, 0, 0, 0, null, dateTime1, dateTime2, null, null, null, null, null, null, false, true, 0, int.MaxValue);
                string fileDownloadName = string.Format("futuredeliveries_{0}_{1}.xlsx", (object)DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"), (object)CommonHelper.GenerateRandomDigitCode(4));
                string str = string.Format("{0}content\\files\\ExportImport\\{1}", (object)this.Request.PhysicalApplicationPath, (object)fileDownloadName);
                _exportManager.ExportPaymentReportToXlsxVERTEX(str, orders, DateTime.Now, DateTime.Now, "Future Delivery Report", true, _storeContext.CurrentStore.Name, false, false);
                var bytes = System.IO.File.ReadAllBytes(str);
                return this.File(bytes, "text/xls", fileDownloadName);
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("List");
            }
        }

        [HttpPost, ActionName("List")]
        [FormValueRequired("future-report-per-store")]
        public ActionResult FutureDeliveryReportPerStore(OrderListModel model)
        {

            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            //Need to enable it if startdate needs to enabled in future
            //DateTime? startDateValue = (model.StartDate == null) ? new DateTime?(DateTime.UtcNow.Date)
            //                         : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.StartDate.Value, _dateTimeHelper.CurrentTimeZone);


            DateTime? endDateValue = (model.EndDate == null) ? new DateTime?(DateTime.UtcNow.Date.AddDays(1.0))
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.EndDate.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);


            TimeZoneInfo storeTimeZone = _dateTimeHelper.GetStoreTimeZone(_storeContext.CurrentStore);

            try
            {
                // IPagedList<Order> FutureDeliveryReportorders = _orderService.SearchOrders(0, 0, 0, 0, 0, 0, 0, null, dateTime1, dateTime2, null, null, null, null, null, null, false, true, 0, int.MaxValue); //Retreiving Future Reports Orders
                IPagedList<Order> PaymentReportorders = _orderService.SearchOrders(_storeContext.CurrentStore.Id, 0, 0, 0, 0, 0, 0, null, null, endDateValue, null, null, null, null, null, null, false, true, 0, int.MaxValue);

                string fileName = string.Format("Future_Delivery_Report_Per_Store{0}_{1}.xlsx", DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"), CommonHelper.GenerateRandomDigitCode(4));
                string filePath = string.Format("{0}content\\files\\ExportImport\\{1}", Request.PhysicalApplicationPath, fileName);
                _exportManager.ExportPaymentReportToXlsxVERTEX(filePath, PaymentReportorders, DateTime.Now, DateTime.Now, "Future_Delivery_Report_Per_Store", true, _storeContext.CurrentStore.Name, false, true);
                var bytes = System.IO.File.ReadAllBytes(filePath);
                return File(bytes, "text/xls", fileName);
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("List");
            }
        }

        [HttpPost, ActionName("List")]
        [FormValueRequired("journal-payment-report-old")]
        public ActionResult JournalPaymentReport(OrderListModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            if (_workContext.CurrentVendor != null)
            {
                model.VendorId = _workContext.CurrentVendor.Id;
            }
            DateTime? startDateValue = model.StartDate.Value;
            DateTime? endDateValue = (model.EndDate == null) ? null
                            : (DateTime?)model.EndDate.Value.AddDays(1);
            try
            {
                var journalreports = _reportingService.JournalReportForExcel(startDateValue, endDateValue).ToList();
                if (!string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["ExcludedStores"]))
                {
                    var storesToExclude = System.Configuration.ConfigurationManager.AppSettings["ExcludedStores"].Split(',');
                    foreach (var items in storesToExclude)
                    {
                        if (journalreports != null)
                            journalreports.RemoveAll(x => x.storeid == Convert.ToInt32(items));
                    }
                }

                string fileName = string.Format("JE_Reports_Payments_{0}_{1}.xlsx", DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"), CommonHelper.GenerateRandomDigitCode(4));
                string filePath = string.Format("{0}content\\files\\ExportImport\\{1}", Request.PhysicalApplicationPath, fileName);
                _exportManager.ExportJournalPaymentReportPerStoretoXlsx(filePath, journalreports, DateTime.Now, DateTime.Now, "Journal Payment Report All Store", false, _storeContext.CurrentStore.Name);
                var bytes = System.IO.File.ReadAllBytes(filePath);
                return File(bytes, "text/xls", fileName);
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("List");
            }
        }

        [HttpPost, ActionName("List")]
        [FormValueRequired("journal-payment-report")]
        public ActionResult JournalPaymentReportAllStore(OrderListModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            if (_workContext.CurrentVendor != null)
            {
                model.VendorId = _workContext.CurrentVendor.Id;
            }
            DateTime? startDateValue = model.StartDate;
            DateTime? endDateValue = (model.EndDate == null) ? null
                            : (DateTime?)model.EndDate.Value.AddDays(1);
            try
            {
                var journalreports = _reportingService.JournalPaymentReportForExcelAllStore(startDateValue, endDateValue).ToList();
                if (!string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["ExcludedStores"]))
                {
                    var storesToExclude = System.Configuration.ConfigurationManager.AppSettings["ExcludedStores"].Split(',');
                    foreach (var items in storesToExclude)
                    {
                        if (journalreports != null)
                            journalreports.RemoveAll(x => x.storeid == Convert.ToInt32(items));
                    }
                }

                string fileName = string.Format("JE_Reports_Payments_{0}_{1}.xlsx", DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"), CommonHelper.GenerateRandomDigitCode(4));
                string filePath = string.Format("{0}content\\files\\ExportImport\\{1}", Request.PhysicalApplicationPath, fileName);
                _exportManager.ExportJournalPaymentReportPerAndAllStore(filePath, journalreports, DateTime.Now, DateTime.Now, "Journal Payment Report All Store", false, _storeContext.CurrentStore.Name);
                var bytes = System.IO.File.ReadAllBytes(filePath);
                return File(bytes, "text/xls", fileName);
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("List");
            }
        }



        [HttpPost, ActionName("List")]
        [FormValueRequired("payment-report-per-store-old")]
        public ActionResult JournalPaymentReportPerStoreOld(OrderListModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            if (_workContext.CurrentVendor != null)
            {
                model.VendorId = _workContext.CurrentVendor.Id;
            }
            DateTime? startDateValue = model.StartDate.Value;
            DateTime? endDateValue = (model.EndDate == null) ? null
                            : (DateTime?)model.EndDate.Value.AddDays(1);
            try
            {
                var storeId = _storeContext.CurrentStore.Id;
                var journalreports = _reportingService.JournalReportForExcel(startDateValue, endDateValue, storeId).ToList();
                string fileName = string.Format("Reports_Payments_Per_Store{0}_{1}.xlsx", DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"), CommonHelper.GenerateRandomDigitCode(4));
                string filePath = string.Format("{0}content\\files\\ExportImport\\{1}", Request.PhysicalApplicationPath, fileName);
                _exportManager.ExportJournalPaymentReportPerStoretoXlsx(filePath, journalreports, DateTime.Now, DateTime.Now, "Journal Payments Report Per Store", false, _storeContext.CurrentStore.Name, storeId);
                var bytes = System.IO.File.ReadAllBytes(filePath);
                return File(bytes, "text/xls", fileName);
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("List");
            }
        }

        [HttpPost, ActionName("List")]
        [FormValueRequired("payment-report-per-store")]
        public ActionResult JournalPaymentReportPerStore(OrderListModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            if (_workContext.CurrentVendor != null)
            {
                model.VendorId = _workContext.CurrentVendor.Id;
            }
            DateTime? startDateValue = model.StartDate.Value;
            DateTime? endDateValue = (model.EndDate == null) ? null
                            : (DateTime?)model.EndDate.Value.AddDays(1);
            try
            {
                var storeId = _storeContext.CurrentStore.Id;
                //var storeId = 339;
                var journalreports = _reportingService.JournalPaymentReportForExcelPerStore(startDateValue, endDateValue, storeId).ToList();
                string fileName = string.Format("Reports_Payments_Per_Store{0}_{1}.xlsx", DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"), CommonHelper.GenerateRandomDigitCode(4));
                string filePath = string.Format("{0}content\\files\\ExportImport\\{1}", Request.PhysicalApplicationPath, fileName);
                _exportManager.ExportJournalPaymentReportPerAndAllStore(filePath, journalreports, DateTime.Now, DateTime.Now, "Journal Payments Report Per Store", false, _storeContext.CurrentStore.Name, storeId);
                var bytes = System.IO.File.ReadAllBytes(filePath);
                return File(bytes, "text/xls", fileName);
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("List");
            }
        }



        [HttpPost, ActionName("List")]
        [FormValueRequired("journal-Delivery-report-old")]
        public ActionResult JournalDeliveryReportOld(OrderListModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();
            if (_workContext.CurrentVendor != null)
            {
                model.VendorId = _workContext.CurrentVendor.Id;
            }
            DateTime? startDateValue = model.StartDate.Value;
            DateTime? endDateValue = (model.EndDate == null) ? null
                            : (DateTime?)model.EndDate.Value.AddDays(1);

            try
            {
                var journalreports = _reportingService.JournalDeliveryReportForExcel(startDateValue, endDateValue).ToList();
                if (!string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["ExcludedStores"]))
                {
                    var storesToExclude = System.Configuration.ConfigurationManager.AppSettings["ExcludedStores"].Split(',');
                    foreach (var items in storesToExclude)
                    {
                        if (journalreports != null)
                            journalreports.RemoveAll(x => x.storeid == Convert.ToInt32(items));
                    }
                }


                string fileName = string.Format("JE_Reports_Delivery_{0}_{1}.xlsx", DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"), CommonHelper.GenerateRandomDigitCode(4));
                string filePath = string.Format("{0}content\\files\\ExportImport\\{1}", Request.PhysicalApplicationPath, fileName);
                _exportManager.ExportJournalDEliveryReporttoXlsx(filePath, journalreports, DateTime.Now, DateTime.Now, "Journal Delivery  Report", false, _storeContext.CurrentStore.Name);
                var bytes = System.IO.File.ReadAllBytes(filePath);
                return File(bytes, "text/xls", fileName);
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("List");
            }
        }



        [HttpPost, ActionName("List")]
        [FormValueRequired("journal-Delivery-report")]
        public ActionResult JournalDeliveryReport(OrderListModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();
            if (_workContext.CurrentVendor != null)
            {
                model.VendorId = _workContext.CurrentVendor.Id;
            }
            DateTime? startDateValue = model.StartDate.Value;
            DateTime? endDateValue = (model.EndDate == null) ? null
                            : (DateTime?)model.EndDate.Value.AddDays(1);
            try
            {
                var journalreports = _reportingService.JournalDeliveryForExcelAllStore(startDateValue, endDateValue).ToList();
                if (!string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["ExcludedStores"]))
                {
                    var storesToExclude = System.Configuration.ConfigurationManager.AppSettings["ExcludedStores"].Split(',');
                    foreach (var items in storesToExclude)
                    {
                        if (journalreports != null)
                            journalreports.RemoveAll(x => x.storeid == Convert.ToInt32(items));
                    }
                }


                string fileName = string.Format("JE_Reports_Delivery_{0}_{1}.xlsx", DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"), CommonHelper.GenerateRandomDigitCode(4));
                string filePath = string.Format("{0}content\\files\\ExportImport\\{1}", Request.PhysicalApplicationPath, fileName);
                _exportManager.ExportJournalDEliveryReporttoXlsx(filePath, journalreports, DateTime.Now, DateTime.Now, "Journal Delivery  Report", false, _storeContext.CurrentStore.Name);
                var bytes = System.IO.File.ReadAllBytes(filePath);
                return File(bytes, "text/xls", fileName);
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("List");
            }
        }



        [HttpPost, ActionName("List")]
        [FormValueRequired("delivery-report-per-store-old")]
        public ActionResult JournalDeliveryReportPerStoreold(OrderListModel model)
        {

            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();
            if (_workContext.CurrentVendor != null)
            {
                model.VendorId = _workContext.CurrentVendor.Id;
            }
            DateTime? startDateValue = model.StartDate.Value;
            DateTime? endDateValue = (model.EndDate == null) ? null
                            : (DateTime?)model.EndDate.Value.AddDays(1);

            try
            {
                var storeId = _storeContext.CurrentStore.Id;
                var journalreports = _reportingService.JournalDeliveryReportForExcel(startDateValue, endDateValue, storeId);
                string fileName = string.Format("Reports_Delivery_Per_Store{0}_{1}.xlsx", DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"), CommonHelper.GenerateRandomDigitCode(4));
                string filePath = string.Format("{0}content\\files\\ExportImport\\{1}", Request.PhysicalApplicationPath, fileName);
                _exportManager.ExportJournalDEliveryReportPerStoretoXlsx(filePath, journalreports, DateTime.Now, DateTime.Now, "Delivery Report Per Store", false, _storeContext.CurrentStore.Name);
                var bytes = System.IO.File.ReadAllBytes(filePath);
                return File(bytes, "text/xls", fileName);
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("List");
            }
        }


        [HttpPost, ActionName("List")]
        [FormValueRequired("delivery-report-per-store")]
        public ActionResult JournalDeliveryReportPerStore(OrderListModel model)
        {

            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();
            if (_workContext.CurrentVendor != null)
            {
                model.VendorId = _workContext.CurrentVendor.Id;
            }
            DateTime? startDateValue = model.StartDate.Value;
            DateTime? endDateValue = (model.EndDate == null) ? null
                            : (DateTime?)model.EndDate.Value.AddDays(1);

            try
            {
                var storeId = _storeContext.CurrentStore.Id;
                var journalreports = _reportingService.JournalDeliveryForExcelPerStore(startDateValue, endDateValue, storeId);
                string fileName = string.Format("Reports_Delivery_Per_Store{0}_{1}.xlsx", DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"), CommonHelper.GenerateRandomDigitCode(4));
                string filePath = string.Format("{0}content\\files\\ExportImport\\{1}", Request.PhysicalApplicationPath, fileName);
                _exportManager.ExportJournalDEliveryReportPerStoretoXlsx(filePath, journalreports, DateTime.Now, DateTime.Now, "Delivery Report Per Store", false, _storeContext.CurrentStore.Name);
                var bytes = System.IO.File.ReadAllBytes(filePath);
                return File(bytes, "text/xls", fileName);
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("List");
            }
        }


        [HttpPost, ActionName("List")]
        [FormValueRequired("refund-report-all")]
        public ActionResult RefundReportAll(OrderListModel model)
        {

            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                model.VendorId = _workContext.CurrentVendor.Id;
            }

            DateTime? startDateValue = (model.StartDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.StartDate.Value, _dateTimeHelper.CurrentTimeZone);

            DateTime? endDateValue = (model.EndDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.EndDate.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);




            TimeZoneInfo storeTimeZone = _dateTimeHelper.GetStoreTimeZone(_storeContext.CurrentStore);
            try
            {

                //var  orderStatusId = null;
                // var paymentStatusId = null;
                // var shippingStatusId =  null;
                //var  billingEmail =  null;
                // var orderGuid =  null;

                var orders = _reportingService.RefundsForExcel(this._storeContext.CurrentStore.Id, startDateValue, endDateValue, 0, int.MaxValue, storeTimeZone).ToList();
                var selectedOrders = new List<OrderExcelExport>();
                if (startDateValue.HasValue)
                {
                    selectedOrders = orders.Where(o => startDateValue.Value <= o.DateOfRefund).ToList();
                }

                if (endDateValue.HasValue)
                {
                    selectedOrders = selectedOrders.Where(o => endDateValue.Value >= o.DateOfRefund).ToList();
                }


                var glcodes = _reportingService.GetGlCodesForReport(false);

                string fileName = string.Format("refunds_{0}_{1}.xlsx", DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"), CommonHelper.GenerateRandomDigitCode(4));
                string filePath = string.Format("{0}content\\files\\ExportImport\\{1}", Request.PhysicalApplicationPath, fileName);

                _exportManager.ExportRefundReportToXlsx(filePath, selectedOrders, glcodes, startDateValue, endDateValue, "Refund Report", false, _storeContext.CurrentStore.Name);

                var bytes = System.IO.File.ReadAllBytes(filePath);
                return File(bytes, "text/xls", fileName);
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("List");
            }
        }

        [HttpPost, ActionName("List")]
        [FormValueRequired("refund-report-all-units")]
        public ActionResult RefundReportAllUnits(OrderListModel model)
        {

            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                model.VendorId = _workContext.CurrentVendor.Id;
            }

            DateTime? startDateValue = (model.StartDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.StartDate.Value, _dateTimeHelper.CurrentTimeZone);

            DateTime? endDateValue = (model.EndDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.EndDate.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);


            TimeZoneInfo storeTimeZone = _dateTimeHelper.GetStoreTimeZone(_storeContext.CurrentStore);
            try
            {

                var orders = _reportingService.RefundsAllUnitsForExcel(startDateValue, endDateValue, 0, int.MaxValue, storeTimeZone);

                var selectedOrders = new List<OrderExcelExport>();
                if (startDateValue.HasValue)
                {
                    selectedOrders = orders.Where(o => startDateValue.Value <= o.DateOfRefund).ToList();
                }

                if (endDateValue.HasValue)
                {
                    selectedOrders = selectedOrders.Where(o => endDateValue.Value >= o.DateOfRefund).ToList();
                }

                var glcodes = _reportingService.GetGlCodesForReport(false);

                DateTime? startTime = startDateValue.HasValue ? startDateValue : new DateTime?(DateTime.UtcNow.Date);
                DateTime? endTime = endDateValue.HasValue ? endDateValue : new DateTime?(DateTime.UtcNow.Date.AddDays(1.0));

                string startDate = String.Format("{0:yyyy-MM-dd}", startTime);
                string endDate = String.Format("{0:yyyy-MM-dd}", endTime);

                var connStr = new DataSettingsManager().LoadSettings().DataConnectionString;
                List<CovidRefunds> covidRefundList = new List<CovidRefunds>();
                using (var conn = new SqlConnection(connStr))
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;

                    var sqlSelect = string.Format("Select * from covidRefunds where CreatedDateUtc >= '{0}' and CreatedDateUtc <= '{1}'", startDate, endDate);

                    cmd.CommandText = sqlSelect;
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            CovidRefunds record = new CovidRefunds();
                            record.CovidRefundId = (int)reader["CovidRefundId"];
                            record.ProductId = (int)reader["ProductId"];
                            record.OrderId = (int)reader["OrderId"];
                            record.OrderItemId = (int)reader["OrderItemId"];
                            record.GLAmount1 = (decimal)reader["GLAmount1"];
                            record.GLAmount2 = (decimal)reader["GLAmount2"];
                            record.GLAmount3 = (decimal)reader["GLAmount3"];
                            record.TaxAmount1 = (decimal)reader["TaxAmount1"];
                            record.TaxAmount2 = (decimal)reader["TaxAmount2"];
                            record.TaxAmount3 = (decimal)reader["TaxAmount3"];

                            covidRefundList.Add(record);
                        }
                    }
                }




                //var covidOrders = _reportingService.CovidRefundsAllUnitsForExcel(startDateValue, endDateValue, 0, int.MaxValue, storeTimeZone);

                string fileName = string.Format("refunds_{0}_{1}.xlsx", DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"), CommonHelper.GenerateRandomDigitCode(4));
                string filePath = string.Format("{0}content\\files\\ExportImport\\{1}", Request.PhysicalApplicationPath, fileName);

                _exportManager.ExportAllUnitsRefundReportToXlsx(filePath, selectedOrders, covidRefundList, glcodes, startDateValue, endDateValue, "Refund Report", false, _storeContext.CurrentStore.Name);

                var bytes = System.IO.File.ReadAllBytes(filePath);
                return File(bytes, "text/xls", fileName);
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("List");
            }
        }

        [HttpPost, ActionName("List")]
        [FormValueRequired("pdf-invoice-all")]
        public ActionResult PdfInvoiceAll(OrderListModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                model.VendorId = _workContext.CurrentVendor.Id;
            }

            DateTime? startDateValue = (model.StartDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.StartDate.Value, _dateTimeHelper.CurrentTimeZone);

            DateTime? endDateValue = (model.EndDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.EndDate.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);

            var orderStatusIds = !model.OrderStatusIds.Contains(0) ? model.OrderStatusIds : null;
            var paymentStatusIds = !model.PaymentStatusIds.Contains(0) ? model.PaymentStatusIds : null;
            var shippingStatusIds = !model.ShippingStatusIds.Contains(0) ? model.ShippingStatusIds : null;

            var filterByProductId = 0;
            var product = _productService.GetProductById(model.ProductId);
            if (product != null && HasAccessToProduct(product))
                filterByProductId = model.ProductId;

            //load orders
            var orders = _orderService.SearchOrders(storeId: _storeContext.CurrentStore.Id,	/// NU-42
                vendorId: model.VendorId,
                productId: filterByProductId,
                warehouseId: model.WarehouseId,
                paymentMethodSystemName: model.PaymentMethodSystemName,
                createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                osIds: orderStatusIds,
                psIds: paymentStatusIds,
                ssIds: shippingStatusIds,
                billingEmail: model.BillingEmail,
                billingLastName: model.BillingLastName,
                billingCountryId: model.BillingCountryId,
                orderNotes: model.OrderNotes);

            byte[] bytes;



            using (var stream = new MemoryStream())
            {
                _pdfService.PrintOrdersToPdf(stream, orders, _orderSettings.GeneratePdfInvoiceInCustomerLanguage ? 0 : _workContext.WorkingLanguage.Id);
                bytes = stream.ToArray();
            }
            return File(bytes, MimeTypes.ApplicationPdf, "orders.pdf");
        }

        [HttpPost]
        public ActionResult PdfInvoiceSelected(string selectedIds)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var orders = new List<Order>();
            if (selectedIds != null)
            {
                var ids = selectedIds
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => Convert.ToInt32(x))
                    .ToArray();
                orders.AddRange(_orderService.GetOrdersByIds(ids));
            }

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                orders = orders.Where(HasAccessToOrder).ToList();
            }

            //ensure that we at least one order selected
            if (!orders.Any())
            {
                ErrorNotification(_localizationService.GetResource("Admin.Orders.PdfInvoice.NoOrders"));
                return RedirectToAction("List");
            }

            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                _pdfService.PrintOrdersToPdf(stream, orders, _orderSettings.GeneratePdfInvoiceInCustomerLanguage ? 0 : _workContext.WorkingLanguage.Id);
                bytes = stream.ToArray();
            }
            return File(bytes, MimeTypes.ApplicationPdf, "orders_" + selectedIds + ".pdf");
        }

        //currently we use this method on the add product to order details pages
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult ProductDetails_AttributeChange(int productId, bool validateAttributeConditions,
            FormCollection form)
        {
            var product = _productService.GetProductById(productId);
            if (product == null)
                return new NullJsonResult();

            var attributeXml = ParseProductAttributes(product, form);

            //conditional attributes
            var enabledAttributeMappingIds = new List<int>();
            var disabledAttributeMappingIds = new List<int>();
            if (validateAttributeConditions)
            {
                var attributes = _productAttributeService.GetProductAttributeMappingsByProductId(product.Id);
                foreach (var attribute in attributes)
                {
                    var conditionMet = _productAttributeParser.IsConditionMet(attribute, attributeXml);
                    if (conditionMet.HasValue)
                    {
                        if (conditionMet.Value)
                            enabledAttributeMappingIds.Add(attribute.Id);
                        else
                            disabledAttributeMappingIds.Add(attribute.Id);
                    }
                }
            }

            return Json(new
            {
                enabledattributemappingids = enabledAttributeMappingIds.ToArray(),
                disabledattributemappingids = disabledAttributeMappingIds.ToArray()
            });
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("btnSaveCC")]
        public ActionResult EditCreditCardInfo(int id, OrderModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var order = _orderService.GetOrderById(id);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null)
                return RedirectToAction("Edit", "Order", new { id = id });

            if (order.AllowStoringCreditCardNumber)
            {
                string cardType = model.CardType;
                string cardName = model.CardName;
                string cardNumber = model.CardNumber;
                string cardCvv2 = model.CardCvv2;
                string cardExpirationMonth = model.CardExpirationMonth;
                string cardExpirationYear = model.CardExpirationYear;

                order.CardType = _encryptionService.EncryptText(cardType);
                order.CardName = _encryptionService.EncryptText(cardName);
                order.CardNumber = _encryptionService.EncryptText(cardNumber);
                order.MaskedCreditCardNumber = _encryptionService.EncryptText(_paymentService.GetMaskedCreditCardNumber(cardNumber));
                order.CardCvv2 = _encryptionService.EncryptText(cardCvv2);
                order.CardExpirationMonth = _encryptionService.EncryptText(cardExpirationMonth);
                order.CardExpirationYear = _encryptionService.EncryptText(cardExpirationYear);
                _orderService.UpdateOrder(order);
            }

            //add a note
            order.OrderNotes.Add(new OrderNote
            {
                Note = "Credit card info has been edited",
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow
            });
            _orderService.UpdateOrder(order);
            LogEditOrder(order.Id);

            PrepareOrderDetailsModel(model, order);
            return View(model);
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("btnSaveOrderTotals")]
        public ActionResult EditOrderTotals(int id, OrderModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var order = _orderService.GetOrderById(id);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null)
                return RedirectToAction("Edit", "Order", new { id = id });

            order.OrderSubtotalInclTax = model.OrderSubtotalInclTaxValue;
            order.OrderSubtotalExclTax = model.OrderSubtotalExclTaxValue;
            order.OrderSubTotalDiscountInclTax = model.OrderSubTotalDiscountInclTaxValue;
            order.OrderSubTotalDiscountExclTax = model.OrderSubTotalDiscountExclTaxValue;
            order.OrderShippingInclTax = model.OrderShippingInclTaxValue;
            order.OrderShippingExclTax = model.OrderShippingExclTaxValue;
            order.PaymentMethodAdditionalFeeInclTax = model.PaymentMethodAdditionalFeeInclTaxValue;
            order.PaymentMethodAdditionalFeeExclTax = model.PaymentMethodAdditionalFeeExclTaxValue;
            order.TaxRates = model.TaxRatesValue;
            order.OrderTax = model.TaxValue;
            order.OrderDiscount = model.OrderTotalDiscountValue;
            order.OrderTotal = model.OrderTotalValue;
            _orderService.UpdateOrder(order);

            //add a note
            order.OrderNotes.Add(new OrderNote
            {
                Note = "Order totals have been edited",
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow
            });
            _orderService.UpdateOrder(order);
            LogEditOrder(order.Id);

            PrepareOrderDetailsModel(model, order);
            return View(model);
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("save-shipping-method")]
        public ActionResult EditShippingMethod(int id, OrderModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var order = _orderService.GetOrderById(id);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null)
                return RedirectToAction("Edit", "Order", new { id = id });

            order.ShippingMethod = model.ShippingMethod;
            _orderService.UpdateOrder(order);

            //add a note
            order.OrderNotes.Add(new OrderNote
            {
                Note = "Shipping method has been edited",
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow
            });
            _orderService.UpdateOrder(order);
            LogEditOrder(order.Id);

            PrepareOrderDetailsModel(model, order);

            //selected tab
            SaveSelectedTabName(persistForTheNextRequest: false);

            return View(model);
        }

        #region NU-31
        [HttpPost, ActionName("Edit")]
        [FormValueRequired(FormValueRequirement.StartsWith, "btnNotifyFulfillOrderItem")]
        [ValidateInput(false)]
        public ActionResult EditOrderItemFulfillNotify(int id, FormCollection form)
        {
            return (EditOrderItem(id, form, true, true));
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired(FormValueRequirement.StartsWith, "btnOnlyFulfillOrderItem")]
        [ValidateInput(false)]
        public ActionResult EditOrderItemFulfillOnly(int id, FormCollection form)
        {
            return (EditOrderItem(id, form, true, false));
        }
        #endregion

        [HttpPost, ActionName("Edit")]
        [FormValueRequired(FormValueRequirement.StartsWith, "btnSaveOrderItem")]
        [ValidateInput(false)]
        public ActionResult EditOrderItem(int id, FormCollection form, bool isFulfill = false, bool isNotify = false)	// NU-31
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var order = _orderService.GetOrderById(id);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            // if (_workContext.CurrentVendor != null)
            //   return RedirectToAction("Edit", "Order", new { id = id });

            //get order item identifier
            int orderItemId = 0;
            foreach (var formValue in form.AllKeys)
            {
                if (formValue.StartsWith("btnSaveOrderItem", StringComparison.InvariantCultureIgnoreCase))
                    orderItemId = Convert.ToInt32(formValue.Substring("btnSaveOrderItem".Length));
                #region NU-31
                if (formValue.StartsWith("btnNotifyFulfillOrderItem", StringComparison.InvariantCultureIgnoreCase))
                    orderItemId = Convert.ToInt32(formValue.Substring("btnNotifyFulfillOrderItem".Length));
                if (formValue.StartsWith("btnOnlyFulfillOrderItem", StringComparison.InvariantCultureIgnoreCase))
                    orderItemId = Convert.ToInt32(formValue.Substring("btnOnlyFulfillOrderItem".Length));
                #endregion
            }

            var orderItem = order.OrderItems.FirstOrDefault(x => x.Id == orderItemId);
            if (orderItem == null)
                throw new ArgumentException("No order item found with the specified id");

            #region NU-38
            decimal unitPriceInclTax = 0m, unitPriceExclTax = 0m, discountInclTax = 0m, discountExclTax = 0m, priceInclTax = 0m, priceExclTax = 0m;
            int quantity = 0;
            #endregion
            DateTime fulfillmentDate = new DateTime();	// NU-31

            if (!isFulfill) // NU-31
            {
                if (!decimal.TryParse(form["pvUnitPriceInclTax" + orderItemId], out unitPriceInclTax))
                    unitPriceInclTax = orderItem.UnitPriceInclTax;
                if (!decimal.TryParse(form["pvUnitPriceExclTax" + orderItemId], out unitPriceExclTax))
                    unitPriceExclTax = orderItem.UnitPriceExclTax;
                if (!int.TryParse(form["pvQuantity" + orderItemId], out quantity))
                    quantity = orderItem.Quantity;
                if (!decimal.TryParse(form["pvDiscountInclTax" + orderItemId], out discountInclTax))
                    discountInclTax = orderItem.DiscountAmountInclTax;
                if (!decimal.TryParse(form["pvDiscountExclTax" + orderItemId], out discountExclTax))
                    discountExclTax = orderItem.DiscountAmountExclTax;
                if (!decimal.TryParse(form["pvPriceInclTax" + orderItemId], out priceInclTax))
                    priceInclTax = orderItem.PriceInclTax;
                if (!decimal.TryParse(form["pvPriceExclTax" + orderItemId], out priceExclTax))
                    priceExclTax = orderItem.PriceExclTax;
            }
            else
            {
                #region NU-31
                if (!DateTime.TryParse(form["pvFulfillmentDateTime" + orderItemId], out fulfillmentDate)) //grab date from datepicker
                {
                    fulfillmentDate = orderItem.RequestedFulfillmentDateTime.GetValueOrDefault();
                }
                #endregion
            }

            var orderItemList1 = order.OrderItems.Where(x => x.OrderId == order.Id).ToList();
            var isCancelled = order.OrderStatus == OrderStatus.Cancelled;
            var itemToFulfill = orderItemList1.Where(x => x.Id == orderItemId).FirstOrDefault();
            if (isFulfill)	// NU-31
            {
               
                if (orderItem.Product.VendorId == 0)
                {
                    if (orderItem.IsBundleProduct)
                    {
                        List<ShoppingCartBundleProductItem> lstAssociatedProds = _shoppingCartService.GetAssociatedProductsByOrderId(orderItem.OrderId).ToList();
                        foreach (var scbpi in lstAssociatedProds)
                        {
                            int orderItemid = scbpi.OrderItemId;
                            OrderItem ordrItm = _orderItemRepository.GetById(orderItemid);
                            ordrItm.FulfillmentDateTime = DateTime.Now;
                            _orderProcessingService.InsertIntoSAPSalesJournalOrderItem(order, ordrItm, 2);
                            _orderProcessingService.InsertGlCodeCalcs(order, ordrItm, isFulfill, false);
                        }
                    }
                    else
                    {
                        orderItem.FulfillmentDateTime = DateTime.Now;
                        _orderProcessingService.InsertIntoSAPSalesJournalOrderItem(order, orderItem, 2);
                        _orderProcessingService.InsertGlCodeCalcs(order, itemToFulfill, isFulfill, false);
                    }
                }
                
                //Setting Shipping status if the product is partially fulfilled or completely fulfilled
                var AllunfulfilledItesmCount = _orderService.GetOrderItemsByOrderID(id).Where(x => x.FulfillmentDateTime == null).ToList().Count;
                if (AllunfulfilledItesmCount == 0)
                    order.ShippingStatus = ShippingStatus.Delivered;
                else
                    order.ShippingStatus = ShippingStatus.PartiallyShipped;

                _orderService.UpdateOrder(order);
            }
            else if (quantity > 0)	// NU-31
            {
                int qtyDifference = orderItem.Quantity - quantity;

                if (!_orderSettings.AutoUpdateOrderTotalsOnEditingOrder)
                {
                    orderItem.UnitPriceInclTax = unitPriceInclTax;
                    orderItem.UnitPriceExclTax = unitPriceExclTax;
                    orderItem.Quantity = quantity;
                    orderItem.DiscountAmountInclTax = discountInclTax;
                    orderItem.DiscountAmountExclTax = discountExclTax;
                    orderItem.PriceInclTax = priceInclTax;
                    orderItem.PriceExclTax = priceExclTax;
                }
                _orderService.UpdateOrder(order);

                //adjust inventory
                _productService.AdjustInventory(orderItem.Product, qtyDifference, orderItem.AttributesXml);
            }
            else
            {
                //adjust inventory
                _productService.AdjustInventory(orderItem.Product, orderItem.Quantity, orderItem.AttributesXml);

                //delete item
                _orderService.DeleteOrderItem(orderItem);
            }

            #region NU-31
            UpdateOrderParameters updateOrderParameters = null;
            if (!isFulfill)
            {
                //update order totals
                updateOrderParameters = new UpdateOrderParameters
                {
                    UpdatedOrder = order,
                    UpdatedOrderItem = orderItem,
                    PriceInclTax = unitPriceInclTax,
                    PriceExclTax = unitPriceExclTax,
                    DiscountAmountInclTax = discountInclTax,
                    DiscountAmountExclTax = discountExclTax,
                    SubTotalInclTax = priceInclTax,
                    SubTotalExclTax = priceExclTax,
                    Quantity = quantity,
                    RequestedFulfillmentDate = fulfillmentDate.ToShortDateString()/// NU-31

                };
            }
            else
            {
                if (orderItem.IsBundleProduct)
                {
                    List<ShoppingCartBundleProductItem> lstAssociatedProds = _shoppingCartService.GetAssociatedProductsByOrderId(orderItem.OrderId).ToList();
                    foreach (var scbpi in lstAssociatedProds)
                    {
                        int orderItemid = scbpi.OrderItemId;
                        OrderItem ordrItm = _orderItemRepository.GetById(orderItemid);
                        updateOrderParameters = new UpdateOrderParameters
                        {
                            UpdatedOrder = order,
                            UpdatedOrderItem = ordrItm,
                            RequestedFulfillmentDate = fulfillmentDate.ToShortDateString(),
                            FulfillmentDate = fulfillmentDate.ToShortDateString()
                        };
                        _orderProcessingService.UpdateOrderTotals(updateOrderParameters);
                    }
                }
                else
                {
                    updateOrderParameters = new UpdateOrderParameters
                    {
                        UpdatedOrder = order,
                        UpdatedOrderItem = orderItem,
                        RequestedFulfillmentDate = fulfillmentDate.ToShortDateString(),
                        FulfillmentDate = fulfillmentDate.ToShortDateString()
                    };
                    _orderProcessingService.UpdateOrderTotals(updateOrderParameters);
                }
            }
            #endregion
           

            //add a note
            order.OrderNotes.Add(new OrderNote
            {
                Note = "Order item has been edited",
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow
            });
            _orderService.UpdateOrder(order);
            LogEditOrder(order.Id);

            var model = new OrderModel();
            if (isFulfill)
            {
                var orderItemList = order.OrderItems.Where(x => x.OrderId == order.Id).ToList();

                foreach (var item in orderItemList)
                {
                    if (item.FulfillmentDateTime == null)
                    {
                        model.OrderStatus = order.OrderStatus.GetLocalizedEnum(_localizationService, _workContext);
                        model.OrderStatusId = order.OrderStatusId;
                        break;
                    }
                    else
                    {
                        model.OrderStatusId = 30;
                    }
                }

                ChangeOrderStatus(order.Id, model);
                if (isNotify)
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
                _orderProcessingService.CheckOrderStatus(order);
            }

            PrepareOrderDetailsModel(model, order);
            model.Warnings = updateOrderParameters.Warnings;

            //selected tab
            SaveSelectedTabName(persistForTheNextRequest: false);
            string data = System.Configuration.ConfigurationManager.AppSettings["VertexEnabledForStoreId"]; //Applying check to enable Partial Refunds for only Pilot accounts
            if (isFulfill)
            {
                if (orderItem.ProductId > 0)
                {
                    if (orderItem.IsBundleProduct)
                    {
                        List<ShoppingCartBundleProductItem> lstAssociatedProds = _shoppingCartService.GetAssociatedProductsByOrderId(orderItem.OrderId).ToList();
                        foreach (var scbpi in lstAssociatedProds)
                        {
                            int orderItemid = scbpi.OrderItemId;
                            OrderItem ordrItm = _orderItemRepository.GetById(orderItemid);
                            var Product = _productService.GetProductById(ordrItm.ProductId);
                            if (Product.VendorId == 0)
                                _orderProcessingService.ApplyfulfilledProductsJEGlCodeBusinessRules(ordrItm, order);
                        }
                    }
                    else
                    {
                        var Product = _productService.GetProductById(orderItem.ProductId);
                        if (Product.VendorId == 0)
                            _orderProcessingService.ApplyfulfilledProductsJEGlCodeBusinessRules(itemToFulfill, order);
                    }
                }
            }

            if (data.Contains(_storeContext.CurrentStore.Id.ToString()))
                model.IsPilot = true;
            return View(model);
        }


        [HttpPost, ActionName("Edit")]
        [FormValueRequired(FormValueRequirement.StartsWith, "btnRefundOrderItem")]	// NU-63
        [ValidateInput(false)]
        public ActionResult DeleteOrderItem(int id, FormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var order = _orderService.GetOrderById(id);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null)
                return RedirectToAction("Edit", "Order", new { id = id });

            //get order item identifier
            int orderItemId = 0;
            foreach (var formValue in form.AllKeys)
                if (formValue.StartsWith("btnRefundOrderItem", StringComparison.InvariantCultureIgnoreCase))
                    orderItemId = Convert.ToInt32(formValue.Substring("btnRefundOrderItem".Length));	// NU-63

            var orderItem = order.OrderItems.FirstOrDefault(x => x.Id == orderItemId);
            if (orderItem == null)
                throw new ArgumentException("No order item found with the specified id");

            if (_giftCardService.GetGiftCardsByPurchasedWithOrderItemId(orderItem.Id).Any())
            {
                //we cannot delete an order item with associated gift cards
                //a store owner should delete them first

                var model = new OrderModel();
                PrepareOrderDetailsModel(model, order);

                ErrorNotification("This order item has an associated gift card record. Please delete it first.", false);

                //selected tab
                SaveSelectedTabName(persistForTheNextRequest: false);

                return View(model);

            }
            else
            {
                //adjust inventory
                _productService.AdjustInventory(orderItem.Product, orderItem.Quantity, orderItem.AttributesXml);

                // _SAPService.CreateEntry(orderItem, 3);	// NU-63

                //delete item
                _orderService.DeleteOrderItem(orderItem);

                //update order totals
                var updateOrderParameters = new UpdateOrderParameters
                {
                    UpdatedOrder = order,
                    UpdatedOrderItem = orderItem
                };
                _orderProcessingService.UpdateOrderTotals(updateOrderParameters);



                //add a note
                order.OrderNotes.Add(new OrderNote
                {
                    Note = "Order item has been deleted",
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow
                });
                _orderService.UpdateOrder(order);
                LogEditOrder(order.Id);

                var model = new OrderModel();
                PrepareOrderDetailsModel(model, order);
                model.Warnings = updateOrderParameters.Warnings;

                //selected tab
                SaveSelectedTabName(persistForTheNextRequest: false);

                return View(model);
            }
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired(FormValueRequirement.StartsWith, "btnResetDownloadCount")]
        [ValidateInput(false)]
        public ActionResult ResetDownloadCount(int id, FormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var order = _orderService.GetOrderById(id);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            //get order item identifier
            int orderItemId = 0;
            foreach (var formValue in form.AllKeys)
                if (formValue.StartsWith("btnResetDownloadCount", StringComparison.InvariantCultureIgnoreCase))
                    orderItemId = Convert.ToInt32(formValue.Substring("btnResetDownloadCount".Length));

            var orderItem = order.OrderItems.FirstOrDefault(x => x.Id == orderItemId);
            if (orderItem == null)
                throw new ArgumentException("No order item found with the specified id");

            //ensure a vendor has access only to his products 
            if (_workContext.CurrentVendor != null && !HasAccessToOrderItem(orderItem))
                return RedirectToAction("List");

            orderItem.DownloadCount = 0;
            _orderService.UpdateOrder(order);
            LogEditOrder(order.Id);

            var model = new OrderModel();
            PrepareOrderDetailsModel(model, order);

            //selected tab
            SaveSelectedTabName(persistForTheNextRequest: false);

            return View(model);
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired(FormValueRequirement.StartsWith, "btnPvActivateDownload")]
        [ValidateInput(false)]
        public ActionResult ActivateDownloadItem(int id, FormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var order = _orderService.GetOrderById(id);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            //get order item identifier
            int orderItemId = 0;
            foreach (var formValue in form.AllKeys)
                if (formValue.StartsWith("btnPvActivateDownload", StringComparison.InvariantCultureIgnoreCase))
                    orderItemId = Convert.ToInt32(formValue.Substring("btnPvActivateDownload".Length));

            var orderItem = order.OrderItems.FirstOrDefault(x => x.Id == orderItemId);
            if (orderItem == null)
                throw new ArgumentException("No order item found with the specified id");

            //ensure a vendor has access only to his products 
            if (_workContext.CurrentVendor != null && !HasAccessToOrderItem(orderItem))
                return RedirectToAction("List");

            orderItem.IsDownloadActivated = !orderItem.IsDownloadActivated;
            _orderService.UpdateOrder(order);
            LogEditOrder(order.Id);

            var model = new OrderModel();
            PrepareOrderDetailsModel(model, order);

            //selected tab
            SaveSelectedTabName(persistForTheNextRequest: false);

            return View(model);
        }

        public ActionResult UploadLicenseFilePopup(int id, int orderItemId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var order = _orderService.GetOrderById(id);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            var orderItem = order.OrderItems.FirstOrDefault(x => x.Id == orderItemId);
            if (orderItem == null)
                throw new ArgumentException("No order item found with the specified id");

            if (!orderItem.Product.IsDownload)
                throw new ArgumentException("Product is not downloadable");

            //ensure a vendor has access only to his products 
            if (_workContext.CurrentVendor != null && !HasAccessToOrderItem(orderItem))
                return RedirectToAction("List");

            var model = new OrderModel.UploadLicenseModel
            {
                LicenseDownloadId = orderItem.LicenseDownloadId.HasValue ? orderItem.LicenseDownloadId.Value : 0,
                OrderId = order.Id,
                OrderItemId = orderItem.Id
            };

            return View(model);
        }

        [HttpPost]
        [FormValueRequired("uploadlicense")]
        public ActionResult UploadLicenseFilePopup(string btnId, string formId, OrderModel.UploadLicenseModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var order = _orderService.GetOrderById(model.OrderId);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            var orderItem = order.OrderItems.FirstOrDefault(x => x.Id == model.OrderItemId);
            if (orderItem == null)
                throw new ArgumentException("No order item found with the specified id");

            //ensure a vendor has access only to his products 
            if (_workContext.CurrentVendor != null && !HasAccessToOrderItem(orderItem))
                return RedirectToAction("List");

            //attach license
            if (model.LicenseDownloadId > 0)
                orderItem.LicenseDownloadId = model.LicenseDownloadId;
            else
                orderItem.LicenseDownloadId = null;
            _orderService.UpdateOrder(order);
            LogEditOrder(order.Id);

            //success
            ViewBag.RefreshPage = true;
            ViewBag.btnId = btnId;
            ViewBag.formId = formId;

            return View(model);
        }

        [HttpPost, ActionName("UploadLicenseFilePopup")]
        [FormValueRequired("deletelicense")]
        public ActionResult DeleteLicenseFilePopup(string btnId, string formId, OrderModel.UploadLicenseModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var order = _orderService.GetOrderById(model.OrderId);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            var orderItem = order.OrderItems.FirstOrDefault(x => x.Id == model.OrderItemId);
            if (orderItem == null)
                throw new ArgumentException("No order item found with the specified id");

            //ensure a vendor has access only to his products 
            if (_workContext.CurrentVendor != null && !HasAccessToOrderItem(orderItem))
                return RedirectToAction("List");

            //attach license
            orderItem.LicenseDownloadId = null;
            _orderService.UpdateOrder(order);
            LogEditOrder(order.Id);

            //success
            ViewBag.RefreshPage = true;
            ViewBag.btnId = btnId;
            ViewBag.formId = formId;

            return View(model);
        }

        public ActionResult AddProductToOrder(int orderId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null)
                return RedirectToAction("Edit", "Order", new { id = orderId });

            var model = new OrderModel.AddOrderProductModel();
            model.OrderId = orderId;
            //categories
            model.AvailableCategories.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            var categories = SelectListHelper.GetCategoryList(_categoryService, _cacheManager, true);
            foreach (var c in categories)
                model.AvailableCategories.Add(c);

            //manufacturers
            model.AvailableManufacturers.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            foreach (var m in _manufacturerService.GetAllManufacturers(showHidden: true))
                model.AvailableManufacturers.Add(new SelectListItem { Text = m.Name, Value = m.Id.ToString() });

            //product types
            model.AvailableProductTypes = ProductType.SimpleProduct.ToSelectList(false).ToList();
            model.AvailableProductTypes.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });

            return View(model);
        }

        [HttpPost]
        public ActionResult AddProductToOrder(DataSourceRequest command, OrderModel.AddOrderProductModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null)
                return Content("");

            var gridModel = new DataSourceResult();
            var products = _productService.SearchProducts(categoryIds: new List<int> { model.SearchCategoryId },
                manufacturerId: model.SearchManufacturerId,
                productType: model.SearchProductTypeId > 0 ? (ProductType?)model.SearchProductTypeId : null,
                keywords: model.SearchProductName,
                pageIndex: command.Page - 1,
                pageSize: command.PageSize,
                showHidden: true);
            gridModel.Data = products.Select(x =>
            {
                var productModel = new OrderModel.AddOrderProductModel.ProductModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    Sku = x.Sku,
                };

                return productModel;
            }).OrderBy(o => o.Name); /// NU-24
            gridModel.Total = products.TotalCount;

            return Json(gridModel);
        }

        public ActionResult AddProductToOrderDetails(int orderId, int productId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null)
                return RedirectToAction("Edit", "Order", new { id = orderId });

            var model = PrepareAddProductToOrderModel(orderId, productId);
            return View(model);
        }

        [HttpPost]
        public ActionResult AddProductToOrderDetails(int orderId, int productId, FormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null)
                return RedirectToAction("Edit", "Order", new { id = orderId });

            var order = _orderService.GetOrderById(orderId);
            var product = _productService.GetProductById(productId);
            //save order item

            //basic properties
            decimal unitPriceInclTax;
            decimal.TryParse(form["UnitPriceInclTax"], out unitPriceInclTax);
            decimal unitPriceExclTax;
            decimal.TryParse(form["UnitPriceExclTax"], out unitPriceExclTax);
            int quantity;
            int.TryParse(form["Quantity"], out quantity);
            decimal priceInclTax;
            decimal.TryParse(form["SubTotalInclTax"], out priceInclTax);
            decimal priceExclTax;
            decimal.TryParse(form["SubTotalExclTax"], out priceExclTax);

            //warnings
            var warnings = new List<string>();

            //attributes
            var attributesXml = ParseProductAttributes(product, form);

            #region Gift cards

            string recipientName = "";
            string recipientEmail = "";
            string senderName = "";
            string senderEmail = "";
            string giftCardMessage = "";
            if (product.IsGiftCard)
            {
                foreach (string formKey in form.AllKeys)
                {
                    if (formKey.Equals("giftcard.RecipientName", StringComparison.InvariantCultureIgnoreCase))
                    {
                        recipientName = form[formKey];
                        continue;
                    }
                    if (formKey.Equals("giftcard.RecipientEmail", StringComparison.InvariantCultureIgnoreCase))
                    {
                        recipientEmail = form[formKey];
                        continue;
                    }
                    if (formKey.Equals("giftcard.SenderName", StringComparison.InvariantCultureIgnoreCase))
                    {
                        senderName = form[formKey];
                        continue;
                    }
                    if (formKey.Equals("giftcard.SenderEmail", StringComparison.InvariantCultureIgnoreCase))
                    {
                        senderEmail = form[formKey];
                        continue;
                    }
                    if (formKey.Equals("giftcard.Message", StringComparison.InvariantCultureIgnoreCase))
                    {
                        giftCardMessage = form[formKey];
                        continue;
                    }
                }

                attributesXml = _productAttributeParser.AddGiftCardAttribute(attributesXml,
                    recipientName, recipientEmail, senderName, senderEmail, giftCardMessage);
            }

            #endregion

            #region Rental product

            DateTime? rentalStartDate = null;
            DateTime? rentalEndDate = null;
            if (product.IsRental)
            {
                ParseRentalDates(form, out rentalStartDate, out rentalEndDate);
            }

            #endregion

            //warnings
            warnings.AddRange(_shoppingCartService.GetShoppingCartItemAttributeWarnings(order.Customer, ShoppingCartType.ShoppingCart, product, quantity, attributesXml));
            warnings.AddRange(_shoppingCartService.GetShoppingCartItemGiftCardWarnings(ShoppingCartType.ShoppingCart, product, attributesXml));
            warnings.AddRange(_shoppingCartService.GetRentalProductWarnings(product, rentalStartDate, rentalEndDate));
            if (!warnings.Any())
            {
                //no errors

                //attributes
                var attributeDescription = _productAttributeFormatter.FormatAttributes(product, attributesXml, order.Customer);

                //save item
                var orderItem = new OrderItem
                {
                    OrderItemGuid = Guid.NewGuid(),
                    Order = order,
                    ProductId = product.Id,
                    UnitPriceInclTax = unitPriceInclTax,
                    UnitPriceExclTax = unitPriceExclTax,
                    PriceInclTax = priceInclTax,
                    PriceExclTax = priceExclTax,
                    OriginalProductCost = _priceCalculationService.GetProductCost(product, attributesXml),
                    AttributeDescription = attributeDescription,
                    AttributesXml = attributesXml,
                    Quantity = quantity,
                    DiscountAmountInclTax = decimal.Zero,
                    DiscountAmountExclTax = decimal.Zero,
                    DownloadCount = 0,
                    IsDownloadActivated = false,
                    LicenseDownloadId = 0,
                    RentalStartDateUtc = rentalStartDate,
                    RentalEndDateUtc = rentalEndDate
                };
                order.OrderItems.Add(orderItem);
                _orderService.UpdateOrder(order);

                //adjust inventory
                _productService.AdjustInventory(orderItem.Product, -orderItem.Quantity, orderItem.AttributesXml);

                //update order totals
                var updateOrderParameters = new UpdateOrderParameters
                {
                    UpdatedOrder = order,
                    UpdatedOrderItem = orderItem,
                    PriceInclTax = unitPriceInclTax,
                    PriceExclTax = unitPriceExclTax,
                    SubTotalInclTax = priceInclTax,
                    SubTotalExclTax = priceExclTax,
                    Quantity = quantity
                };
                _orderProcessingService.UpdateOrderTotals(updateOrderParameters);

                //add a note
                order.OrderNotes.Add(new OrderNote
                {
                    Note = "A new order item has been added",
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow
                });
                _orderService.UpdateOrder(order);
                LogEditOrder(order.Id);

                //gift cards
                if (product.IsGiftCard)
                {
                    for (int i = 0; i < orderItem.Quantity; i++)
                    {
                        var gc = new GiftCard
                        {
                            GiftCardType = product.GiftCardType,
                            PurchasedWithOrderItem = orderItem,
                            Amount = unitPriceExclTax,
                            IsGiftCardActivated = false,
                            GiftCardCouponCode = _giftCardService.GenerateGiftCardCode(),
                            RecipientName = recipientName,
                            RecipientEmail = recipientEmail,
                            SenderName = senderName,
                            SenderEmail = senderEmail,
                            Message = giftCardMessage,
                            IsRecipientNotified = false,
                            CreatedOnUtc = DateTime.UtcNow
                        };
                        _giftCardService.InsertGiftCard(gc);
                    }
                }

                //redirect to order details page
                TempData["nop.admin.order.warnings"] = updateOrderParameters.Warnings;
                return RedirectToAction("Edit", "Order", new { id = order.Id });
            }

            //errors
            var model = PrepareAddProductToOrderModel(order.Id, product.Id);
            model.Warnings.AddRange(warnings);
            return View(model);
        }

        #endregion

        #endregion

        #region Addresses

        public ActionResult AddressEdit(int addressId, int orderId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var order = _orderService.GetOrderById(orderId);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null)
                return RedirectToAction("Edit", "Order", new { id = orderId });

            var address = _addressService.GetAddressById(addressId);
            if (address == null)
                throw new ArgumentException("No address found with the specified id", "addressId");

            var model = new OrderAddressModel();
            model.OrderId = orderId;
            model.Address = address.ToModel();
            model.Address.FirstNameEnabled = true;
            model.Address.FirstNameRequired = true;
            model.Address.LastNameEnabled = true;
            model.Address.LastNameRequired = true;
            model.Address.EmailEnabled = true;
            model.Address.EmailRequired = true;
            model.Address.CompanyEnabled = _addressSettings.CompanyEnabled;
            model.Address.CompanyRequired = _addressSettings.CompanyRequired;
            model.Address.CountryEnabled = _addressSettings.CountryEnabled;
            model.Address.StateProvinceEnabled = _addressSettings.StateProvinceEnabled;
            model.Address.CityEnabled = _addressSettings.CityEnabled;
            model.Address.CityRequired = _addressSettings.CityRequired;
            model.Address.StreetAddressEnabled = _addressSettings.StreetAddressEnabled;
            model.Address.StreetAddressRequired = _addressSettings.StreetAddressRequired;
            model.Address.StreetAddress2Enabled = _addressSettings.StreetAddress2Enabled;
            model.Address.StreetAddress2Required = _addressSettings.StreetAddress2Required;
            model.Address.ZipPostalCodeEnabled = _addressSettings.ZipPostalCodeEnabled;
            model.Address.ZipPostalCodeRequired = _addressSettings.ZipPostalCodeRequired;
            model.Address.PhoneEnabled = _addressSettings.PhoneEnabled;
            model.Address.PhoneRequired = _addressSettings.PhoneRequired;
            model.Address.FaxEnabled = _addressSettings.FaxEnabled;
            model.Address.FaxRequired = _addressSettings.FaxRequired;

            //countries
            model.Address.AvailableCountries.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Address.SelectCountry"), Value = "0" });
            foreach (var c in _countryService.GetAllCountries(showHidden: true))
                model.Address.AvailableCountries.Add(new SelectListItem { Text = c.Name, Value = c.Id.ToString(), Selected = (c.Id == address.CountryId) });
            //states
            var states = address.Country != null ? _stateProvinceService.GetStateProvincesByCountryId(address.Country.Id, showHidden: true).ToList() : new List<StateProvince>();
            if (states.Any())
            {
                foreach (var s in states)
                    model.Address.AvailableStates.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString(), Selected = (s.Id == address.StateProvinceId) });
            }
            else
                model.Address.AvailableStates.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Address.OtherNonUS"), Value = "0" });
            //customer attribute services
            model.Address.PrepareCustomAddressAttributes(address, _addressAttributeService, _addressAttributeParser);

            return View(model);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult AddressEdit(OrderAddressModel model, FormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var order = _orderService.GetOrderById(model.OrderId);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null)
                return RedirectToAction("Edit", "Order", new { id = order.Id });

            var address = _addressService.GetAddressById(model.Address.Id);
            if (address == null)
                throw new ArgumentException("No address found with the specified id");

            //custom address attributes
            var customAttributes = form.ParseCustomAddressAttributes(_addressAttributeParser, _addressAttributeService);
            var customAttributeWarnings = _addressAttributeParser.GetAttributeWarnings(customAttributes);
            foreach (var error in customAttributeWarnings)
            {
                ModelState.AddModelError("", error);
            }

            if (ModelState.IsValid)
            {
                address = model.Address.ToEntity(address);
                address.CustomAttributes = customAttributes;
                _addressService.UpdateAddress(address);

                //add a note
                order.OrderNotes.Add(new OrderNote
                {
                    Note = "Address has been edited",
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow
                });
                _orderService.UpdateOrder(order);
                LogEditOrder(order.Id);
                SuccessNotification("Address has been updated successfully");
                return RedirectToAction("AddressEdit", new { addressId = model.Address.Id, orderId = model.OrderId });
            }

            //If we got this far, something failed, redisplay form
            model.OrderId = order.Id;
            model.Address = address.ToModel();
            model.Address.FirstNameEnabled = true;
            model.Address.FirstNameRequired = true;
            model.Address.LastNameEnabled = true;
            model.Address.LastNameRequired = true;
            model.Address.EmailEnabled = true;
            model.Address.EmailRequired = true;
            model.Address.CompanyEnabled = _addressSettings.CompanyEnabled;
            model.Address.CompanyRequired = _addressSettings.CompanyRequired;
            model.Address.CountryEnabled = _addressSettings.CountryEnabled;
            model.Address.StateProvinceEnabled = _addressSettings.StateProvinceEnabled;
            model.Address.CityEnabled = _addressSettings.CityEnabled;
            model.Address.CityRequired = _addressSettings.CityRequired;
            model.Address.StreetAddressEnabled = _addressSettings.StreetAddressEnabled;
            model.Address.StreetAddressRequired = _addressSettings.StreetAddressRequired;
            model.Address.StreetAddress2Enabled = _addressSettings.StreetAddress2Enabled;
            model.Address.StreetAddress2Required = _addressSettings.StreetAddress2Required;
            model.Address.ZipPostalCodeEnabled = _addressSettings.ZipPostalCodeEnabled;
            model.Address.ZipPostalCodeRequired = _addressSettings.ZipPostalCodeRequired;
            model.Address.PhoneEnabled = _addressSettings.PhoneEnabled;
            model.Address.PhoneRequired = _addressSettings.PhoneRequired;
            model.Address.FaxEnabled = _addressSettings.FaxEnabled;
            model.Address.FaxRequired = _addressSettings.FaxRequired;
            //countries
            model.Address.AvailableCountries.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Address.SelectCountry"), Value = "0" });
            foreach (var c in _countryService.GetAllCountries(showHidden: true))
                model.Address.AvailableCountries.Add(new SelectListItem { Text = c.Name, Value = c.Id.ToString(), Selected = (c.Id == address.CountryId) });
            //states
            var states = address.Country != null ? _stateProvinceService.GetStateProvincesByCountryId(address.Country.Id, showHidden: true).ToList() : new List<StateProvince>();
            if (states.Any())
            {
                foreach (var s in states)
                    model.Address.AvailableStates.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString(), Selected = (s.Id == address.StateProvinceId) });
            }
            else
                model.Address.AvailableStates.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Address.OtherNonUS"), Value = "0" });
            //customer attribute services
            model.Address.PrepareCustomAddressAttributes(address, _addressAttributeService, _addressAttributeParser);

            return View(model);
        }

        public ActionResult UpdateMailStopAddress(string msAddress, int orderId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var order = _orderService.GetOrderById(orderId);
            if (order == null)
                return RedirectToAction("List");

            if (_workContext.CurrentVendor != null)
                return RedirectToAction("Edit", "Order", new { id = order.Id });

            if (!String.IsNullOrEmpty(msAddress))
                order.MailStopAddress = msAddress;

            _orderService.UpdateOrder(order);
            LogEditOrder(order.Id);

            return RedirectToAction("Edit", "Order", new { id = orderId });
        }

        public ActionResult UpdateRCNumber(string rcNumber, int orderId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var order = _orderService.GetOrderById(orderId);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null)
                return RedirectToAction("Edit", "Order", new { id = order.Id });

            if (!String.IsNullOrEmpty(rcNumber))
                order.RCNumber = rcNumber;

            _orderService.UpdateOrder(order);
            LogEditOrder(order.Id);

            return RedirectToAction("Edit", "Order", new { id = orderId });
        }

        #endregion

        #region Shipments

        public ActionResult ShipmentList()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var model = new ShipmentListModel();
            //countries
            model.AvailableCountries.Add(new SelectListItem { Text = "*", Value = "0" });
            foreach (var c in _countryService.GetAllCountries(showHidden: true))
                model.AvailableCountries.Add(new SelectListItem { Text = c.Name, Value = c.Id.ToString() });
            //states
            model.AvailableStates.Add(new SelectListItem { Text = "*", Value = "0" });

            //warehouses
            model.AvailableWarehouses.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            #region NU-35
            if (_workContext.CurrentCustomer.IsVendor())
            {
                foreach (var w in _shippingService.GetAllWarehouses(
                    vendorId: _workContext.CurrentVendor.Id))
                    model.AvailableWarehouses.Add(new SelectListItem { Text = w.Name, Value = w.Id.ToString() });
            }
            else
            {
                foreach (var w in _shippingService.GetAllWarehouses(
                    _storeContext.CurrentStore.Id))	/// SODMYWAY-2946
                    model.AvailableWarehouses.Add(new SelectListItem { Text = w.Name, Value = w.Id.ToString() });
            }
            #endregion

            return View(model);
        }

        [HttpPost]
        public ActionResult ShipmentListSelect(DataSourceRequest command, ShipmentListModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            DateTime? startDateValue = (model.StartDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.StartDate.Value, _dateTimeHelper.CurrentTimeZone);

            DateTime? endDateValue = (model.EndDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.EndDate.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);

            //a vendor should have access only to his products
            int vendorId = 0;
            if (_workContext.CurrentVendor != null)
                vendorId = _workContext.CurrentVendor.Id;

            //load shipments
            var shipments = _shipmentService.GetAllShipments(vendorId: vendorId,
                warehouseId: model.WarehouseId,
                shippingCountryId: model.CountryId,
                shippingStateId: model.StateProvinceId,
                shippingCity: model.City,
                trackingNumber: model.TrackingNumber,
                loadNotShipped: model.LoadNotShipped,
                createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                pageIndex: command.Page - 1,
                pageSize: command.PageSize,
                storeId: _storeContext.CurrentStore.Id);	/// NU-36
            var gridModel = new DataSourceResult
            {
                Data = shipments.Select(shipment => PrepareShipmentModel(shipment, false))
                    .OrderByDescending(o => o.Id), /// NU-37
                Total = shipments.TotalCount
            };
            return new JsonResult
            {
                Data = gridModel
            };
        }

        [HttpPost]
        public ActionResult ShipmentsByOrder(int orderId, DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var order = _orderService.GetOrderById(orderId);
            if (order == null)
                throw new ArgumentException("No order found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && !HasAccessToOrder(order))
                return Content("");

            //shipments
            var shipmentModels = new List<ShipmentModel>();
            var shipments = order.Shipments
                //a vendor should have access only to his products
                .Where(s => _workContext.CurrentVendor == null || HasAccessToShipment(s))
                .OrderBy(s => s.CreatedOnUtc)
                .ToList();
            foreach (var shipment in shipments)
                shipmentModels.Add(PrepareShipmentModel(shipment, false));

            var gridModel = new DataSourceResult
            {
                Data = shipmentModels
                    .OrderByDescending(o => o.Id), /// NU-37
                Total = shipmentModels.Count
            };


            return Json(gridModel);
        }

        [HttpPost]
        public ActionResult ShipmentsItemsByShipmentId(int shipmentId, DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var shipment = _shipmentService.GetShipmentById(shipmentId);
            if (shipment == null)
                throw new ArgumentException("No shipment found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && !HasAccessToShipment(shipment))
                return Content("");

            var order = _orderService.GetOrderById(shipment.OrderId);
            if (order == null)
                throw new ArgumentException("No order found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && !HasAccessToOrder(order))
                return Content("");

            //shipments
            var shipmentModel = PrepareShipmentModel(shipment, true);
            var gridModel = new DataSourceResult
            {
                Data = shipmentModel.Items
                    .OrderBy(o => o.ProductName), /// NU-24
                Total = shipmentModel.Items.Count
            };

            return Json(gridModel);
        }


        [HttpPost]
        public ActionResult GetTaxDetailsPerOrderItem(int orderItemId, DataSourceRequest command)
        {
            List<PartiallyRefundGridModel> gridDataList = new List<PartiallyRefundGridModel>();
            List<PartiallyRefundGridModel_SB> gridDataList_SB = new List<PartiallyRefundGridModel_SB>();
            Dictionary<string, decimal> productGlAmountList = new Dictionary<string, decimal>();
            List<DynamicVertexGLCode> vertexOrderTaxGls = new List<DynamicVertexGLCode>();
            var listProducts = new List<int>();

            var orderItem = _orderService.GetOrderItemById(orderItemId);
            bool isOrderItemBundleProduct = false;
            isOrderItemBundleProduct = orderItem.IsBundleProduct;

            try
            {
                PartiallyRefundGridModel gridData = new PartiallyRefundGridModel();
                gridData.OrderItemId = orderItem.Id;

                if (!orderItem.IsPartialRefund)
                {
                    bool productExistInList = false;
                    Dictionary<int, decimal> taxDictionary = new Dictionary<int, decimal>();
                    Dictionary<int, string> taxCategoryIdNameDictionary = new Dictionary<int, string>();

                    if (gridDataList.Count() > 0)
                    {
                        var isProductExist = gridDataList.Where(x => x.ProductId == orderItem.Product.Id).ToList();
                        if (isProductExist.Count() > 0)
                        {
                            productExistInList = false; // Always set this to false so that the Records dont Merge up
                        }
                    }

                    var taxCategoryList = _taxCategoryService.GetAllTaxCategories();
                    if (listProducts.Contains(orderItem.ProductId))
                    {
                        vertexOrderTaxGls = _glCodeService.GetVertexGlBreakdown(orderItem.OrderId, orderItem.ProductId, TaxRequestType.ConfirmDistributiveTax, orderItem.Id);

                    }
                    else
                    {
                        vertexOrderTaxGls = _glCodeService.GetVertexGlBreakdown(orderItem.OrderId, orderItem.ProductId, TaxRequestType.ConfirmDistributiveTax);
                    }

                    var vertexDeliveryShippingGls = vertexOrderTaxGls.Where(x => x.taxCode == "DELV" || x.taxCode == "SHIP").ToList();
                    vertexOrderTaxGls = vertexOrderTaxGls.Where(x => x.taxCode != "DELV" && x.taxCode != "SHIP").ToList();


                    var data = vertexOrderTaxGls.Where(x => x.GlCode != "44571098").ToList();

                    List<DynamicVertexGLCode> vertexCodeWithAmountList = new List<DynamicVertexGLCode>();

                    foreach (var response in vertexOrderTaxGls)
                    {
                        //if (response.GlCode == "44571098")
                        //{
                        var d = data.Where(x => x.taxCode == response.taxCode).ToList();
                        if (vertexOrderTaxGls.Count() > 0)
                        {
                            decimal total = +response.Total;
                            vertexCodeWithAmountList.Add(new DynamicVertexGLCode(response.productId, response.GlCode, response.Description, response.Total, response.taxCode));
                        }
                        else
                        {
                            vertexCodeWithAmountList.Add(new DynamicVertexGLCode(response.productId, response.GlCode, response.Description, response.Total, response.taxCode));
                        }
                        //}
                    }

                    if (orderItem.Product.TaxCategoryId == -1)
                    {

                        List<ProductGlCode> paidCodes = _glCodeService.GetProductGlCodes(orderItem.Product, GLCodeStatusType.Paid).ToList();
                        var splitGls = _glCodeService.GetProductsGlByType(1, paidCodes, true, false, true); //only gls with taxcategory
                        var bonusProduct = splitGls.Where(x => x.GlCode.Description.Contains("Bonus")).FirstOrDefault();

                        int count = 0;
                        foreach (var paidCode in paidCodes)
                        {

                            if (paidCode.GlCode.GlCodeName != "62610001" && paidCode.GlCode.GlCodeName != "70161010")
                            {
                                string glName = paidCode.GlCode.GlCodeName + '-' + paidCode.GlCode.Description;

                                count++;
                                if (productExistInList)
                                {
                                    var existingProduct = gridDataList.Where(x => x.ProductId == orderItem.Product.Id).ToList();

                                    foreach (var product in existingProduct)
                                    {
                                        if (count == 1)
                                        {
                                            if (bonusProduct != null)
                                            {
                                                var SplitGlDepositCashForBonus = System.Configuration.ConfigurationManager.AppSettings["DepositCash"];
                                                if (!string.IsNullOrEmpty(SplitGlDepositCashForBonus))
                                                {
                                                    var cashvalueGl = paidCode.GlCode.GlCodeName;
                                                    if (SplitGlDepositCashForBonus.Contains(cashvalueGl))
                                                    {
                                                        if (paidCode.GlCode.GlCodeName == cashvalueGl) //DEPOSITS CASH VALUE BOARD
                                                        {
                                                            product.GLAmount1 = Convert.ToDecimal(paidCodes.Where(x => x.GlCode.GlCodeName == cashvalueGl).FirstOrDefault().Amount + paidCodes.Where(x => x.GlCode.GlCodeName == "64701000").FirstOrDefault().Amount) * orderItem.Quantity;
                                                        }
                                                    }
                                                }
                                            }

                                            else
                                            {
                                                product.GLAmount1 = product.GLAmount1 + paidCode.Amount * orderItem.Quantity;
                                            }
                                            gridData.GLCodeName1 = glName;
                                            gridData.TaxAmount1 = gridData.TaxAmount1 + vertexCodeWithAmountList[0].Total;
                                            gridData.TaxName1 = vertexOrderTaxGls[0].GlCode + "-" + vertexCodeWithAmountList[0].Description;

                                        }

                                        if (count == 2)
                                        {
                                            gridData.GLCodeName2 = glName;
                                            product.GLAmount2 = product.GLAmount2 + paidCode.Amount * orderItem.Quantity;
                                            gridData.TaxAmount2 = gridData.TaxAmount2 + vertexCodeWithAmountList[1].Total;
                                            gridData.TaxName2 = vertexOrderTaxGls[1].GlCode + "-" + vertexCodeWithAmountList[1].Description;

                                        }

                                        if (count == 3)
                                        {
                                            gridData.GLCodeName3 = glName;
                                            product.GLAmount3 = product.GLAmount3 + paidCode.Amount * orderItem.Quantity;

                                            gridData.TaxAmount3 = gridData.TaxAmount3 + vertexCodeWithAmountList[2].Total;
                                            gridData.TaxName3 = vertexOrderTaxGls[2].GlCode + "-" + vertexCodeWithAmountList[2].Description;
                                        }
                                    }
                                }

                                else
                                {
                                    if (count == 1)
                                    {
                                        if (bonusProduct != null)
                                        {
                                            var SplitGlDepositCashForBonus = System.Configuration.ConfigurationManager.AppSettings["DepositCash"];
                                            if (!string.IsNullOrEmpty(SplitGlDepositCashForBonus))
                                            {
                                                var cashvalueGl = paidCode.GlCode.GlCodeName;
                                                if (SplitGlDepositCashForBonus.Contains(cashvalueGl))
                                                {
                                                    if (paidCode.GlCode.GlCodeName == cashvalueGl) //DEPOSITS CASH VALUE BOARD
                                                    {
                                                        gridData.GLAmount1 = Convert.ToDecimal(paidCodes.Where(x => x.GlCode.GlCodeName == cashvalueGl).FirstOrDefault().Amount + paidCodes.Where(x => x.GlCode.GlCodeName == "64701000").FirstOrDefault().Amount) * orderItem.Quantity;
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            gridData.GLAmount1 = paidCode.Amount * orderItem.Quantity;
                                        }
                                        gridData.GLCodeName1 = glName;
                                        gridData.GLAmount1 = paidCode.Amount * orderItem.Quantity;
                                        gridData.TaxAmount1 = gridData.TaxAmount1 + vertexCodeWithAmountList[0].Total;
                                        gridData.TaxName1 = vertexOrderTaxGls[0].GlCode + "-" + vertexCodeWithAmountList[0].Description;

                                    }

                                    if (count == 2)
                                    {
                                        gridData.GLCodeName2 = glName;
                                        gridData.GLAmount2 = paidCode.Amount * orderItem.Quantity;
                                        if (vertexCodeWithAmountList.Count == 2)
                                        {
                                            gridData.TaxAmount1 = vertexCodeWithAmountList[0].Total;
                                            gridData.TaxName1 = vertexOrderTaxGls[0].GlCode + "-" + vertexCodeWithAmountList[0].Description;

                                            gridData.TaxAmount2 = vertexCodeWithAmountList[1].Total;
                                            gridData.TaxName2 = vertexOrderTaxGls[1].GlCode + "-" + vertexCodeWithAmountList[1].Description;
                                        }
                                        else
                                        {
                                            gridData.TaxAmount2 = gridData.TaxAmount2 + decimal.Zero;
                                            gridData.TaxName2 = "None";
                                        }

                                    }

                                    if (count == 3)
                                    {
                                        gridData.GLCodeName3 = glName;
                                        gridData.GLAmount3 = paidCode.Amount * orderItem.Quantity;
                                        if (vertexCodeWithAmountList.Count == 3)
                                        {
                                            gridData.TaxAmount1 = vertexCodeWithAmountList[0].Total;
                                            gridData.TaxName1 = vertexOrderTaxGls[0].GlCode + "-" + vertexCodeWithAmountList[0].Description;

                                            gridData.TaxAmount2 = vertexCodeWithAmountList[1].Total;
                                            gridData.TaxName2 = vertexOrderTaxGls[1].GlCode + "-" + vertexCodeWithAmountList[1].Description;

                                            gridData.TaxAmount3 = vertexCodeWithAmountList[2].Total;
                                            gridData.TaxName3 = vertexOrderTaxGls[2].GlCode + "-" + vertexCodeWithAmountList[2].Description;
                                        }
                                        else
                                        {
                                            gridData.TaxAmount3 = gridData.TaxAmount3 + decimal.Zero;
                                            gridData.TaxName3 = "None";
                                        }
                                    }
                                }

                            }
                        }


                    }
                    else
                    {
                        List<ProductGlCode> paidCodes = _glCodeService.GetProductGlCodes(orderItem.Product, GLCodeStatusType.Paid).ToList();
                        bool vertexProcessed = false;
                        foreach (var paidCode in paidCodes)
                        {
                            if (paidCode.GlCode.GlCodeName != "62610001" && paidCode.GlCode.GlCodeName != "70161010")
                            {

                                if (productExistInList)
                                {
                                    var existingProduct = gridDataList.Where(x => x.ProductId == orderItem.Product.Id).ToList();
                                    foreach (var product in existingProduct)
                                    {
                                        product.GLAmount1 = product.GLAmount1 + orderItem.PriceExclTax;

                                        if (vertexOrderTaxGls.Count() > 0)
                                        {
                                            foreach (var taxAmount in vertexOrderTaxGls)
                                            {
                                                product.TaxAmount1 = product.TaxAmount1 + taxAmount.Total;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    gridData.GLCodeName1 = paidCode.GlCode.GlCodeName + '-' + paidCode.GlCode.Description;
                                    gridData.GLAmount1 = orderItem.PriceExclTax;

                                    if (!vertexProcessed)
                                    {
                                        if (vertexOrderTaxGls.Count() > 0)
                                        {
                                            vertexProcessed = true;


                                            // foreach (var taxAmount in vertexOrderTaxGls)
                                            //{
                                            if (vertexOrderTaxGls.Count == 1)
                                            {
                                                gridData.TaxAmount1 = gridData.TaxAmount1 + vertexOrderTaxGls[0].Total;
                                                gridData.TaxName1 = vertexOrderTaxGls[0].GlCode + "-" + vertexOrderTaxGls[0].Description;
                                            }
                                            if (vertexOrderTaxGls.Count == 2)
                                            {
                                                gridData.TaxAmount1 = gridData.TaxAmount1 + vertexOrderTaxGls[0].Total;
                                                gridData.TaxName1 = vertexOrderTaxGls[0].GlCode + "-" + vertexOrderTaxGls[0].Description;
                                                gridData.TaxAmount2 = gridData.TaxAmount2 + vertexOrderTaxGls[1].Total;
                                                gridData.TaxName2 = vertexOrderTaxGls[1].GlCode + "-" + vertexOrderTaxGls[1].Description;
                                            }
                                            if (vertexOrderTaxGls.Count == 3)
                                            {
                                                gridData.TaxAmount1 = gridData.TaxAmount1 + vertexOrderTaxGls[0].Total;
                                                gridData.TaxName1 = vertexOrderTaxGls[0].GlCode + "-" + vertexOrderTaxGls[0].Description;
                                                gridData.TaxAmount2 = gridData.TaxAmount2 + vertexOrderTaxGls[1].Total;
                                                gridData.TaxName2 = vertexOrderTaxGls[1].GlCode + "-" + vertexOrderTaxGls[1].Description;
                                                gridData.TaxAmount3 = gridData.TaxAmount3 + vertexOrderTaxGls[2].Total;
                                                gridData.TaxName3 = vertexOrderTaxGls[2].GlCode + "-" + vertexOrderTaxGls[2].Description;
                                            }
                                            //}
                                        }

                                    }
                                }
                            }
                        }


                    }


                    if (!productExistInList)
                    {
                        gridData.ProductId = orderItem.Product.Id;
                        gridData.ProductName = orderItem.Product.Name;
                        gridData.Sku = orderItem.Product.Sku;
                        foreach (var gridataItemsForDelvandShip in vertexDeliveryShippingGls)
                        {
                            if (gridataItemsForDelvandShip.taxCode == "DELV")
                            {
                                gridData.DeliveryTaxName = gridataItemsForDelvandShip.GlCode + "-" + gridataItemsForDelvandShip.Description;
                                gridData.DeliveryGLCodeName = "70161010-Delivery Fee Onsite";
                                gridData.DeliveryPickupAmount = Convert.ToDecimal(orderItem.DeliveryPickupFee);
                                gridData.DeliveryTax = gridataItemsForDelvandShip.Total;

                                gridData.ShippingTaxName = "None";
                                gridData.ShippingGlName = "None";

                            }
                            else if (gridataItemsForDelvandShip.taxCode == "SHIP")
                            {
                                gridData.ShippingTaxName = gridataItemsForDelvandShip.GlCode + "-" + gridataItemsForDelvandShip.Description;
                                gridData.ShippingGlName = "62610001-Shipping Fee";
                                gridData.ShippingAmount = Convert.ToDecimal(orderItem.ShippingFee);
                                gridData.ShippingTax = gridataItemsForDelvandShip.Total;

                                gridData.DeliveryTaxName = "None";
                                gridData.DeliveryGLCodeName = "None";
                            }
                            else
                            {
                                gridData.ShippingTaxName = "None";
                                gridData.ShippingGlName = "None";

                                gridData.DeliveryTaxName = "None";
                                gridData.DeliveryGLCodeName = "None";
                            }
                        }

                        gridDataList.Add(gridData);
                    }
                }
                else
                {
                    if (orderItem.Product.TaxCategoryId != -1)
                    {
                        gridData.ProductId = orderItem.Product.Id;
                        gridData.ProductName = orderItem.Product.Name;
                        gridData.Sku = orderItem.Product.Sku;
                        gridData.TaxAmount1 = Convert.ToDecimal(orderItem.TaxAmount1);
                        gridData.GLAmount1 = Convert.ToDecimal(orderItem.PartialRefundOrderAmount);
                        gridData.TaxAmount2 = Convert.ToDecimal(orderItem.TaxAmount2);
                        gridData.GLAmount2 = Convert.ToDecimal(orderItem.GlcodeAmount2);
                        gridData.TaxAmount3 = Convert.ToDecimal(orderItem.TaxAmount3);
                        gridData.GLAmount3 = Convert.ToDecimal(orderItem.GlcodeAmount3);
                        gridData.TotalRefund = Convert.ToDecimal(orderItem.TotalRefundedAmount);
                        gridData.RefundedTaxAmount1 = Convert.ToDecimal(orderItem.RefundedTaxAmount1);
                        gridData.RefundedTaxAmount2 = Convert.ToDecimal(orderItem.RefundedTaxAmount2);
                        gridData.RefundedTaxAmount3 = Convert.ToDecimal(orderItem.RefundedTaxAmount3);
                        gridData.GLCodeName1 = orderItem.GLCodeName1;
                        gridData.GLCodeName2 = orderItem.GLCodeName2;
                        gridData.GLCodeName3 = orderItem.GLCodeName3;
                        gridData.TaxName1 = orderItem.TaxName1;
                        gridData.TaxName2 = orderItem.TaxName2;
                        gridData.TaxName3 = orderItem.TaxName3;

                        vertexOrderTaxGls = _glCodeService.GetVertexGlBreakdown(orderItem.OrderId, orderItem.ProductId, TaxRequestType.ConfirmDistributiveTax);
                        var vertexDeliveryShippingGls = vertexOrderTaxGls.Where(x => x.taxCode == "DELV" || x.taxCode == "SHIP").ToList();
                        foreach (var gridataItemsForDelvandShip in vertexDeliveryShippingGls)
                        {
                            if (gridataItemsForDelvandShip.taxCode == "DELV")
                            {
                                gridData.DeliveryTaxName = gridataItemsForDelvandShip.GlCode + "-" + gridataItemsForDelvandShip.Description;
                                gridData.DeliveryGLCodeName = "70161010-Delivery Fee Onsite";
                                gridData.DeliveryPickupAmount = orderItem.IsFullRefund ? 0 : Convert.ToDecimal(orderItem.DeliveryPickupFee);
                                gridData.DeliveryTax = orderItem.IsFullRefund ? 0 : gridataItemsForDelvandShip.Total;

                                gridData.ShippingTaxName = "None";
                                gridData.ShippingGlName = "None";

                            }
                            else if (gridataItemsForDelvandShip.taxCode == "SHIP")
                            {
                                gridData.ShippingTaxName = gridataItemsForDelvandShip.GlCode + "-" + gridataItemsForDelvandShip.Description;
                                gridData.ShippingGlName = "62610001-Shipping Fee";
                                gridData.ShippingAmount = orderItem.IsFullRefund ? 0 : Convert.ToDecimal(orderItem.ShippingFee);
                                gridData.ShippingTax = orderItem.IsFullRefund ? 0 : gridataItemsForDelvandShip.Total;

                                gridData.DeliveryTaxName = "None";
                                gridData.DeliveryGLCodeName = "None";
                            }

                            else
                            {
                                gridData.ShippingTaxName = "None";
                                gridData.ShippingGlName = "None";

                                gridData.DeliveryTaxName = "None";
                                gridData.DeliveryGLCodeName = "None";
                            }
                        }
                        gridDataList.Add(gridData);


                    }
                    else if (orderItem.Product.TaxCategoryId == -1)
                    {
                        //List<ProductGlCode> paidCodes = _glCodeService.GetProductGlCodes(orderItem.Product, GLCodeStatusType.Paid).ToList();

                        gridData.ProductId = orderItem.Product.Id;
                        gridData.ProductName = orderItem.Product.Name;
                        gridData.Sku = orderItem.Product.Sku;
                        gridData.TaxAmount1 = Convert.ToDecimal(orderItem.TaxAmount1);
                        gridData.TaxAmount2 = Convert.ToDecimal(orderItem.TaxAmount2);
                        gridData.TaxAmount3 = Convert.ToDecimal(orderItem.TaxAmount3);
                        gridData.TotalRefund = Convert.ToDecimal(orderItem.TotalRefundedAmount);
                        gridData.GLAmount1 = Convert.ToDecimal(orderItem.GlcodeAmount1);
                        gridData.GLAmount2 = Convert.ToDecimal(orderItem.GlcodeAmount2);
                        gridData.GLAmount3 = Convert.ToDecimal(orderItem.GlcodeAmount3);
                        gridData.RefundedTaxAmount1 = Convert.ToDecimal(orderItem.RefundedTaxAmount1);
                        gridData.RefundedTaxAmount2 = Convert.ToDecimal(orderItem.RefundedTaxAmount2);
                        gridData.RefundedTaxAmount3 = Convert.ToDecimal(orderItem.RefundedTaxAmount3);
                        gridData.GLCodeName1 = orderItem.GLCodeName1;
                        gridData.GLCodeName2 = orderItem.GLCodeName2;
                        gridData.GLCodeName3 = orderItem.GLCodeName3;
                        gridData.TaxName1 = Convert.ToString(orderItem.TaxName1);
                        gridData.TaxName2 = Convert.ToString(orderItem.TaxName2);
                        gridData.TaxName3 = Convert.ToString(orderItem.TaxName3);
                        gridDataList.Add(gridData);
                    }
                }
            }
            catch (Exception exc)
            {
                throw exc;
            }
            ////CALLS THE DATA FROM VERTEX AS WELL AS FROM XMLS
            foreach (var griddata in gridDataList)
            {
                griddata.OrderId = 0;

                if (string.IsNullOrEmpty(griddata.DeliveryTaxName))
                {
                    griddata.DeliveryTaxName = "None";
                    griddata.DeliveryTax = 0;
                }
                if (string.IsNullOrEmpty(griddata.ShippingTaxName))
                {
                    griddata.ShippingTaxName = "None";
                    griddata.ShippingTax = 0;
                }

                var overriddenGlitems = GetOveriddenGls(griddata.OrderId, griddata.ProductId, griddata.OrderItemId);
                if (overriddenGlitems.Any())
                {

                    griddata.Glcodeid1 = overriddenGlitems[0];
                    griddata.Glcodeid2 = overriddenGlitems[1];
                    griddata.Glcodeid3 = overriddenGlitems[2];
                }
                else
                {
                    //Setting to Default Gl to prevent showing undeifned
                    griddata.Glcodeid1 = 00000000;
                    griddata.Glcodeid2 = 00000000;
                    griddata.Glcodeid3 = 00000000;
                }

                PartiallyRefundGridModel_SB gridDataList_SB_Item = new PartiallyRefundGridModel_SB();

                gridDataList_SB_Item.ProductId_SB = griddata.ProductId;
                gridDataList_SB_Item.Sku_SB = griddata.Sku;
                gridDataList_SB_Item.ProductName_SB = griddata.ProductName;
                gridDataList_SB_Item.OrderItemId_SB = griddata.OrderItemId;

                gridDataList_SB_Item.GLCodeName1_SB = griddata.GLCodeName1;
                gridDataList_SB_Item.GLAmount1_SB = griddata.GLAmount1;
                gridDataList_SB_Item.GLCodeName2_SB = griddata.GLCodeName2;
                gridDataList_SB_Item.GLAmount2_SB = griddata.GLAmount2;
                gridDataList_SB_Item.GLCodeName3_SB = griddata.GLCodeName3;
                gridDataList_SB_Item.GLAmount3_SB = griddata.GLAmount3;

                gridDataList_SB_Item.TaxAmount1_SB = griddata.TaxAmount1;

                gridDataList_SB_Item.TaxName1_SB = griddata.TaxName1;

                gridDataList_SB_Item.TaxAmount2_SB = griddata.TaxAmount2;

                gridDataList_SB_Item.TaxName2_SB = griddata.TaxName2;

                gridDataList_SB_Item.TaxAmount3_SB = griddata.TaxAmount3;
                gridDataList_SB_Item.TaxName3_SB = griddata.TaxName3;

                gridDataList_SB_Item.TotalRefund_SB = griddata.TotalRefund;

                gridDataList_SB_Item.RefundedTaxAmount1_SB = griddata.RefundedTaxAmount1;
                gridDataList_SB_Item.RefundedTaxAmount2_SB = griddata.RefundedTaxAmount2;
                gridDataList_SB_Item.RefundedTaxAmount3_SB = griddata.RefundedTaxAmount3;

                gridDataList_SB_Item.DeliveryTaxName_SB = griddata.DeliveryTaxName;
                gridDataList_SB_Item.DeliveryGLCodeName_SB = griddata.DeliveryGLCodeName;

                gridDataList_SB_Item.DeliveryTax_SB = griddata.DeliveryTax;

                gridDataList_SB_Item.DeliveryPickupAmount_SB = griddata.DeliveryPickupAmount;
                gridDataList_SB_Item.ShippingAmount_SB = griddata.ShippingAmount;

                gridDataList_SB_Item.ShippingTax_SB = griddata.ShippingTax;
                gridDataList_SB_Item.ShippingTaxName_SB = griddata.ShippingTaxName;
                gridDataList_SB_Item.ShippingGlName_SB = griddata.ShippingGlName;
                gridDataList_SB_Item.Error_SB = griddata.Error;

                gridDataList_SB_Item.Success_SB = griddata.Success;

                gridDataList_SB_Item.Glcodeid1_SB = griddata.Glcodeid1;
                gridDataList_SB_Item.Glcodeid2_SB = griddata.Glcodeid2;
                gridDataList_SB_Item.Glcodeid3_SB = griddata.Glcodeid3;
                gridDataList_SB_Item.ConsoleRefund_SB = griddata.ConsoleRefund;

                gridDataList_SB.Add(gridDataList_SB_Item);

            }

            var gridModel = new DataSourceResult
            {
                Data = gridDataList_SB,
                Total = gridDataList.Count
            };

            return Json(gridModel);
        }

        public ActionResult ReShipment(int orderId, int shipmentId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();
            var model = new Shipment();
            var order = _orderService.GetOrderById(orderId);
            if (order == null)
                return RedirectToAction("List");
            decimal? totalWeight = null;
            var shipments = _shipmentService.GetShipmentById(shipmentId);
            if (shipments == null)
                return RedirectToAction("List");
            foreach (var shipmentItem in shipments.ShipmentItems)
            {
                var orderItem = _orderService.GetOrderItemById(shipmentItem.OrderItemId);
                if (orderItem == null)
                    continue;
                bool shipmentWithUPSAPI_Failed = false;
                if (!orderItem.Product.IsShipEnabled)
                    continue;
                if (orderItem.SelectedShippingRateComputationMethodSystemName != null)
                {
                    if (!orderItem.SelectedShippingRateComputationMethodSystemName.Equals("Shipping.UPS"))
                        continue;
                }
                var orderItemTotalWeight = orderItem.ItemWeight.HasValue ? orderItem.ItemWeight * orderItem.Quantity : null;
                if (orderItemTotalWeight.HasValue)
                {
                    if (!totalWeight.HasValue)
                        totalWeight = 0;
                    totalWeight += orderItemTotalWeight.Value;
                }
                var shipmentWithUPSAPIResult = string.Empty;
                shipmentWithUPSAPI_Failed = shipmentWithUPSAPIResult == string.Empty ? false : true;
                var adminComment = "Re-Shipment Created With UPS";
                shipments = new Shipment
                {
                    OrderId = orderId,
                    TrackingNumber = string.Empty,
                    TotalWeight = null,
                    ShippedDateUtc = null,
                    DeliveryDateUtc = null,
                    AdminComment = adminComment,
                    CreatedOnUtc = DateTime.UtcNow,
                };
                var shipmentItems = new ShipmentItem
                {
                    OrderItemId = orderItem.Id,
                    Quantity = orderItem.Quantity,
                    WarehouseId = orderItem.Product.WarehouseId
                };
                shipments.ShipmentItems.Add(shipmentItems);
            }
            if (shipments != null && shipments.ShipmentItems.Any())
            {
                shipments.TotalWeight = totalWeight;
                _shipmentService.InsertShipment(shipments);
                order.OrderNotes.Add(new OrderNote
                {
                    Note = "A shipment has been added for Re-Shipping with UPS",
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow
                });
                _orderService.UpdateOrder(order);
                LogEditOrder(order.Id);
            }
            SuccessNotification("A shipment has been created successfully");
            return RedirectToAction("Edit", new { id = orderId }); ;
        }
        public ActionResult GetProductsByOrderId(int orderId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();
            var order = _orderService.GetOrderById(orderId);
            var products = order.OrderItems;
            if (_workContext.CurrentVendor != null)
            {
                products = products
                    .Where(orderItem => orderItem.Product.VendorId == _workContext.CurrentVendor.Id)
                    .ToList();
            }
            var orderItemModel = new List<OrderModel.OrderItemModel>();
            foreach (var orderItem in products)
            {
                orderItemModel.Add(new OrderModel.OrderItemModel
                {
                    Id = orderItem.Id,
                    ProductId = orderItem.ProductId,
                    ProductName = orderItem.Product.Name,
                    Sku = orderItem.Product.FormatSku(orderItem.AttributesXml, _productAttributeParser),
                    Quantity = orderItem.Quantity,
                    IsDownload = orderItem.Product.IsDownload,
                    DownloadCount = orderItem.DownloadCount,
                    DownloadActivationType = orderItem.Product.DownloadActivationType,
                    IsDownloadActivated = orderItem.IsDownloadActivated,
                    RequestedFulfillmentDateTime = orderItem.RequestedFulfillmentDateTime, // NU-31
                    FulfillmentDateTime = orderItem.FulfillmentDateTime // NU-31
                });
            }
            var gridModel = new DataSourceResult
            {
                Data = orderItemModel,
                Total = orderItemModel.Count()
            };
            return Json(gridModel);
        }
        public ActionResult AddShipment(int orderId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var order = _orderService.GetOrderById(orderId);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && !HasAccessToOrder(order))
                return RedirectToAction("List");

            var model = new ShipmentModel
            {
                OrderId = order.Id,
            };

            //measures
            var baseWeight = _measureService.GetMeasureWeightById(_measureSettings.BaseWeightId);
            var baseWeightIn = baseWeight != null ? baseWeight.Name : "";
            var baseDimension = _measureService.GetMeasureDimensionById(_measureSettings.BaseDimensionId);
            var baseDimensionIn = baseDimension != null ? baseDimension.Name : "";

            var orderItems = order.OrderItems;
            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                orderItems = orderItems.Where(HasAccessToOrderItem).ToList();
            }

            foreach (var orderItem in orderItems)
            {
                //we can ship only shippable products
                if (!orderItem.Product.IsShipEnabled)
                    continue;

                //quantities
                var qtyInThisShipment = 0;
                var maxQtyToAdd = orderItem.GetTotalNumberOfItemsCanBeAddedToShipment();
                var qtyOrdered = orderItem.Quantity;
                var qtyInAllShipments = orderItem.GetTotalNumberOfItemsInAllShipment();

                //ensure that this product can be added to a shipment
                if (maxQtyToAdd <= 0)
                    continue;

                var shipmentItemModel = new ShipmentModel.ShipmentItemModel
                {
                    OrderItemId = orderItem.Id,
                    ProductId = orderItem.ProductId,
                    ProductName = orderItem.Product.Name,
                    Sku = orderItem.Product.FormatSku(orderItem.AttributesXml, _productAttributeParser),
                    AttributeInfo = orderItem.AttributeDescription,
                    ShipSeparately = orderItem.Product.ShipSeparately,
                    ItemWeight = orderItem.ItemWeight.HasValue ? string.Format("{0:F2} [{1}]", orderItem.ItemWeight, baseWeightIn) : "",
                    ItemDimensions = string.Format("{0:F2} x {1:F2} x {2:F2} [{3}]", orderItem.Product.Length, orderItem.Product.Width, orderItem.Product.Height, baseDimensionIn),
                    QuantityOrdered = qtyOrdered,
                    QuantityInThisShipment = qtyInThisShipment,
                    QuantityInAllShipments = qtyInAllShipments,
                    QuantityToAdd = maxQtyToAdd,
                };
                //rental info
                if (orderItem.Product.IsRental)
                {
                    var rentalStartDate = orderItem.RentalStartDateUtc.HasValue ? orderItem.Product.FormatRentalDate(orderItem.RentalStartDateUtc.Value) : "";
                    var rentalEndDate = orderItem.RentalEndDateUtc.HasValue ? orderItem.Product.FormatRentalDate(orderItem.RentalEndDateUtc.Value) : "";
                    shipmentItemModel.RentalInfo = string.Format(_localizationService.GetResource("Order.Rental.FormattedDate"),
                        rentalStartDate, rentalEndDate);
                }

                if (orderItem.Product.ManageInventoryMethod == ManageInventoryMethod.ManageStock &&
                    orderItem.Product.UseMultipleWarehouses)
                {
                    //multiple warehouses supported
                    shipmentItemModel.AllowToChooseWarehouse = true;
                    foreach (var pwi in orderItem.Product.ProductWarehouseInventory
                        .OrderBy(w => w.WarehouseId).ToList())
                    {
                        var warehouse = pwi.Warehouse;
                        if (warehouse != null)
                        {
                            shipmentItemModel.AvailableWarehouses.Add(new ShipmentModel.ShipmentItemModel.WarehouseInfo
                            {
                                WarehouseId = warehouse.Id,
                                WarehouseName = warehouse.Name,
                                StockQuantity = pwi.StockQuantity,
                                ReservedQuantity = pwi.ReservedQuantity,
                                PlannedQuantity = _shipmentService.GetQuantityInShipments(orderItem.Product, warehouse.Id, true, true)
                            });
                        }
                    }
                }
                else
                {
                    //multiple warehouses are not supported
                    var warehouse = _shippingService.GetWarehouseById(orderItem.Product.WarehouseId);
                    if (warehouse != null)
                    {
                        shipmentItemModel.AvailableWarehouses.Add(new ShipmentModel.ShipmentItemModel.WarehouseInfo
                        {
                            WarehouseId = warehouse.Id,
                            WarehouseName = warehouse.Name,
                            StockQuantity = orderItem.Product.StockQuantity
                        });
                    }
                }

                model.Items.Add(shipmentItemModel);
            }

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        public ActionResult AddShipment(int orderId, FormCollection form, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var order = _orderService.GetOrderById(orderId);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && !HasAccessToOrder(order))
                return RedirectToAction("List");

            var orderItems = order.OrderItems;
            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                orderItems = orderItems.Where(HasAccessToOrderItem).ToList();
            }

            Shipment shipment = null;
            decimal? totalWeight = null;
            foreach (var orderItem in orderItems)
            {
                //is shippable
                if (!orderItem.Product.IsShipEnabled)
                    continue;

                //ensure that this product can be shipped (have at least one item to ship)
                var maxQtyToAdd = orderItem.GetTotalNumberOfItemsCanBeAddedToShipment();
                if (maxQtyToAdd <= 0)
                    continue;

                int qtyToAdd = 0; //parse quantity
                foreach (string formKey in form.AllKeys)
                    if (formKey.Equals(string.Format("qtyToAdd{0}", orderItem.Id), StringComparison.InvariantCultureIgnoreCase))
                    {
                        int.TryParse(form[formKey], out qtyToAdd);
                        break;
                    }

                int warehouseId = 0;
                if (orderItem.Product.ManageInventoryMethod == ManageInventoryMethod.ManageStock &&
                    orderItem.Product.UseMultipleWarehouses)
                {
                    //multiple warehouses supported
                    //warehouse is chosen by a store owner
                    foreach (string formKey in form.AllKeys)
                        if (formKey.Equals(string.Format("warehouse_{0}", orderItem.Id), StringComparison.InvariantCultureIgnoreCase))
                        {
                            int.TryParse(form[formKey], out warehouseId);
                            break;
                        }
                }
                else
                {
                    //multiple warehouses are not supported
                    warehouseId = orderItem.Product.WarehouseId;
                }

                foreach (string formKey in form.AllKeys)
                    if (formKey.Equals(string.Format("qtyToAdd{0}", orderItem.Id), StringComparison.InvariantCultureIgnoreCase))
                    {
                        int.TryParse(form[formKey], out qtyToAdd);
                        break;
                    }

                //validate quantity
                if (qtyToAdd <= 0)
                    continue;
                if (qtyToAdd > maxQtyToAdd)
                    qtyToAdd = maxQtyToAdd;

                //ok. we have at least one item. let's create a shipment (if it does not exist)

                var orderItemTotalWeight = orderItem.ItemWeight.HasValue ? orderItem.ItemWeight * qtyToAdd : null;
                if (orderItemTotalWeight.HasValue)
                {
                    if (!totalWeight.HasValue)
                        totalWeight = 0;
                    totalWeight += orderItemTotalWeight.Value;
                }
                if (shipment == null)
                {
                    var trackingNumber = form["TrackingNumber"];
                    var adminComment = form["AdminComment"];
                    shipment = new Shipment
                    {
                        OrderId = order.Id,
                        TrackingNumber = trackingNumber,
                        TotalWeight = null,
                        ShippedDateUtc = null,
                        DeliveryDateUtc = null,
                        AdminComment = adminComment,
                        CreatedOnUtc = DateTime.UtcNow,
                    };
                }
                //create a shipment item
                var shipmentItem = new ShipmentItem
                {
                    OrderItemId = orderItem.Id,
                    Quantity = qtyToAdd,
                    WarehouseId = warehouseId
                };
                shipment.ShipmentItems.Add(shipmentItem);
            }

            //if we have at least one item in the shipment, then save it
            if (shipment != null && shipment.ShipmentItems.Any())
            {
                shipment.TotalWeight = totalWeight;
                _shipmentService.InsertShipment(shipment);

                //add a note
                order.OrderNotes.Add(new OrderNote
                {
                    Note = "A shipment has been added",
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow
                });
                _orderService.UpdateOrder(order);
                LogEditOrder(order.Id);

                SuccessNotification(_localizationService.GetResource("Admin.Orders.Shipments.Added"));
                return continueEditing
                           ? RedirectToAction("ShipmentDetails", new { id = shipment.Id })
                           : RedirectToAction("Edit", new { id = orderId });
            }

            ErrorNotification(_localizationService.GetResource("Admin.Orders.Shipments.NoProductsSelected"));
            return RedirectToAction("AddShipment", new { orderId = orderId });
        }

        public ActionResult ShipmentDetails(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var shipment = _shipmentService.GetShipmentById(id);
            if (shipment == null)
                //No shipment found with the specified id
                return RedirectToAction("List");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && !HasAccessToShipment(shipment))
                return RedirectToAction("List");

            var model = PrepareShipmentModel(shipment, true, true);
            return View(model);
        }

        [HttpPost]
        public ActionResult DeleteShipment(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var shipment = _shipmentService.GetShipmentById(id);
            if (shipment == null)
                //No shipment found with the specified id
                return RedirectToAction("List");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && !HasAccessToShipment(shipment))
                return RedirectToAction("List");

            foreach (var shipmentItem in shipment.ShipmentItems)
            {
                var orderItem = _orderService.GetOrderItemById(shipmentItem.OrderItemId);
                if (orderItem == null)
                    continue;

                _productService.ReverseBookedInventory(orderItem.Product, shipmentItem);
            }

            var orderId = shipment.OrderId;
            _shipmentService.DeleteShipment(shipment);

            var order = _orderService.GetOrderById(orderId);
            //add a note
            order.OrderNotes.Add(new OrderNote
            {
                Note = "A shipment has been deleted",
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow
            });
            _orderService.UpdateOrder(order);
            LogEditOrder(order.Id);

            SuccessNotification(_localizationService.GetResource("Admin.Orders.Shipments.Deleted"));
            return RedirectToAction("Edit", new { id = orderId });
        }

        [HttpPost, ActionName("ShipmentDetails")]
        [FormValueRequired("settrackingnumber")]
        public ActionResult SetTrackingNumber(ShipmentModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var shipment = _shipmentService.GetShipmentById(model.Id);
            if (shipment == null)
                //No shipment found with the specified id
                return RedirectToAction("List");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && !HasAccessToShipment(shipment))
                return RedirectToAction("List");

            shipment.TrackingNumber = model.TrackingNumber;
            _shipmentService.UpdateShipment(shipment);

            return RedirectToAction("ShipmentDetails", new { id = shipment.Id });
        }

        [HttpPost, ActionName("ShipmentDetails")]
        [FormValueRequired("setadmincomment")]
        public ActionResult SetShipmentAdminComment(ShipmentModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var shipment = _shipmentService.GetShipmentById(model.Id);
            if (shipment == null)
                //No shipment found with the specified id
                return RedirectToAction("List");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && !HasAccessToShipment(shipment))
                return RedirectToAction("List");

            shipment.AdminComment = model.AdminComment;
            _shipmentService.UpdateShipment(shipment);

            return RedirectToAction("ShipmentDetails", new { id = shipment.Id });
        }

        [HttpPost, ActionName("ShipmentDetails")]
        [FormValueRequired("setasshipped")]
        public ActionResult SetAsShipped(int id)
        {



            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var shipment = _shipmentService.GetShipmentById(id);
            if (shipment == null)
                //No shipment found with the specified id
                return RedirectToAction("List");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && !HasAccessToShipment(shipment))
                return RedirectToAction("List");

            try
            {
                _orderProcessingService.Ship(shipment, true);


                var order = _orderService.GetOrderById(shipment.OrderId);
                var orderItemList1 = order.OrderItems.Where(x => x.Id == shipment.ShipmentItems.FirstOrDefault().OrderItemId).ToList();
                var isCancelled = order.OrderStatus == OrderStatus.Cancelled;

                foreach (var items in orderItemList1)
                {
                    bool isVendor = _productService.GetProductById(items.ProductId).VendorId == 0 ? false : true;
                    if (isVendor)
                    {
                        items.FulfillmentDateTime = DateTime.Now;
                        var itemToFulfill = orderItemList1.Where(x => x.Id == items.Id).FirstOrDefault();
                        _orderProcessingService.InsertGlCodeCalcs(order, itemToFulfill, true, false);
                        _orderProcessingService.InsertIntoSAPSalesJournalOrderItem(order, items, 2);
                        _orderProcessingService.ApplyfulfilledProductsJEGlCodeBusinessRules(itemToFulfill, order);
                    }
                }

                LogEditOrder(shipment.OrderId);
                return RedirectToAction("ShipmentDetails", new { id = shipment.Id });
            }
            catch (Exception exc)
            {
                //error
                ErrorNotification(exc, true);
                return RedirectToAction("ShipmentDetails", new { id = shipment.Id });
            }
        }

        [HttpPost, ActionName("ShipmentDetails")]
        [FormValueRequired("saveshippeddate")]
        public ActionResult EditShippedDate(ShipmentModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var shipment = _shipmentService.GetShipmentById(model.Id);
            if (shipment == null)
                //No shipment found with the specified id
                return RedirectToAction("List");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && !HasAccessToShipment(shipment))
                return RedirectToAction("List");

            try
            {
                if (!model.ShippedDateUtc.HasValue)
                {
                    throw new Exception("Enter shipped date");
                }
                shipment.ShippedDateUtc = model.ShippedDateUtc;
                _shipmentService.UpdateShipment(shipment);
                return RedirectToAction("ShipmentDetails", new { id = shipment.Id });
            }
            catch (Exception exc)
            {
                //error
                ErrorNotification(exc, true);
                return RedirectToAction("ShipmentDetails", new { id = shipment.Id });
            }
        }

        [HttpPost, ActionName("ShipmentDetails")]
        [FormValueRequired("setasdelivered")]
        public ActionResult SetAsDelivered(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var shipment = _shipmentService.GetShipmentById(id);
            if (shipment == null)
                //No shipment found with the specified id
                return RedirectToAction("List");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && !HasAccessToShipment(shipment))
                return RedirectToAction("List");

            try
            {
                _orderProcessingService.Deliver(shipment, true);
                var order = _orderService.GetOrderById(shipment.OrderId);
                var orderItemList1 = order.OrderItems.Where(x => x.Id == shipment.ShipmentItems.FirstOrDefault().OrderItemId).ToList();
                var isCancelled = order.OrderStatus == OrderStatus.Cancelled;

                foreach (var items in orderItemList1)
                {
                    bool isVendor = _productService.GetProductById(items.ProductId).VendorId == 0 ? false : true;
                    if (isVendor)
                    {
                        items.FulfillmentDateTime = DateTime.Now;
                        var itemToFulfill = orderItemList1.Where(x => x.Id == items.Id).FirstOrDefault();
                        _orderProcessingService.InsertGlCodeCalcs(order, itemToFulfill, true, false);
                        _orderProcessingService.InsertIntoSAPSalesJournalOrderItem(order, items, 2);
                        _orderProcessingService.ApplyfulfilledProductsJEGlCodeBusinessRules(itemToFulfill, order);
                    }
                }

                LogEditOrder(shipment.OrderId);
                return RedirectToAction("ShipmentDetails", new { id = shipment.Id });
            }
            catch (Exception exc)
            {
                //error
                ErrorNotification(exc, true);
                return RedirectToAction("ShipmentDetails", new { id = shipment.Id });
            }
        }


        [HttpPost, ActionName("ShipmentDetails")]
        [FormValueRequired("savedeliverydate")]
        public ActionResult EditDeliveryDate(ShipmentModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var shipment = _shipmentService.GetShipmentById(model.Id);
            if (shipment == null)
                //No shipment found with the specified id
                return RedirectToAction("List");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && !HasAccessToShipment(shipment))
                return RedirectToAction("List");

            try
            {
                if (!model.DeliveryDateUtc.HasValue)
                {
                    throw new Exception("Enter delivery date");
                }
                shipment.DeliveryDateUtc = model.DeliveryDateUtc;
                _shipmentService.UpdateShipment(shipment);
                return RedirectToAction("ShipmentDetails", new { id = shipment.Id });
            }
            catch (Exception exc)
            {
                //error
                ErrorNotification(exc, true);
                return RedirectToAction("ShipmentDetails", new { id = shipment.Id });
            }
        }

        //---START: Codechages done by (na-sdxcorp\ADas)--------------
        public ActionResult PdfShippingLabel(int shipmentId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var shipment = _shipmentService.GetShipmentById(shipmentId);
            if (shipment == null)
                //no shipment found with the specified id
                return RedirectToAction("List");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && !HasAccessToShipment(shipment))
                return RedirectToAction("List");


            byte[] pdfBase64 = shipment.ShippingLabelImage;

            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                _pdfService.PrintShippingLabelToPdf(stream, pdfBase64);
                bytes = stream.ToArray();
            }


            return File(bytes, MimeTypes.ApplicationPdf, string.Format("shippinglabel_{0}.pdf", shipment.Id));

        }

        public ActionResult ViewPdfShippingLabel(int shipmentId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var shipment = _shipmentService.GetShipmentById(shipmentId);
            if (shipment == null)
                //no shipment found with the specified id
                return RedirectToAction("List");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && !HasAccessToShipment(shipment))
                return RedirectToAction("List");


            byte[] pdfBase64 = shipment.ShippingLabelImage;

            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                _pdfService.PrintShippingLabelToPdf(stream, pdfBase64);
                bytes = stream.ToArray();
            }


            return File(bytes, MimeTypes.ApplicationPdf);

        }


        public ActionResult GenerateShipment(int shipmentId)
        {

            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var shipment = _shipmentService.GetShipmentById(shipmentId);
            if (shipment == null)
                //No shipment found with the specified id
                return RedirectToAction("List");
            foreach (var shipmetitems in shipment.ShipmentItems)
            {
                var orderItem = _orderService.GetOrderItemById(shipmetitems.OrderItemId);
                if (orderItem == null)
                    continue;

                //var order = _orderService.GetOrderById(shipment.OrderId);
                //var orderItems = orderItem.Where(x => x.SelectedShippingMethodName.Contains("UPS")).FirstOrDefault();

                bool shipmentWithUPSAPI_Failed = false;
                decimal? totalWeight = null;
                //ok. we have at least one item. let's create a shipment (if it does not exist)

                var orderItemTotalWeight = orderItem.ItemWeight.HasValue ? orderItem.ItemWeight * 1 : null; //setting this to 1 as each products shall have 1 quatity each for processing seperately
                if (orderItemTotalWeight.HasValue)
                {
                    if (!totalWeight.HasValue)
                        totalWeight = 0;
                    totalWeight += orderItemTotalWeight.Value;
                }

                var shipmentWithUPSAPIResult = _shippingService.CreateShipmentWithUPSAPI(orderItem);

                shipmentWithUPSAPI_Failed = shipmentWithUPSAPIResult.Count == 0 ? true : false;

                if (!shipmentWithUPSAPI_Failed)
                {
                    shipment.TrackingNumber = shipmentWithUPSAPIResult[0]; //"Track-" + trackNo;  //To be taken from UPS Shipping API
                    shipment.ShippingLabelImage = Convert.FromBase64String(shipmentWithUPSAPIResult[1]);
                    _shipmentService.UpdateShipment(shipment);
                }
                else
                {
                    ErrorNotification(Convert.ToString(_httpContext.Session["ErrorDescription"]));
                    return RedirectToAction("ShipmentDetails", new { id = shipment.Id });
                }
            }
            return RedirectToAction("ShipmentDetails", new { id = shipment.Id });
        }
        //---END: Codechages done by (na-sdxcorp\ADas)--------------

        public ActionResult PdfPackagingSlip(int shipmentId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var shipment = _shipmentService.GetShipmentById(shipmentId);
            if (shipment == null)
                //no shipment found with the specified id
                return RedirectToAction("List");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && !HasAccessToShipment(shipment))
                return RedirectToAction("List");

            var shipments = new List<Shipment>();
            shipments.Add(shipment);

            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                _pdfService.PrintPackagingSlipsToPdf(stream, shipments, _orderSettings.GeneratePdfInvoiceInCustomerLanguage ? 0 : _workContext.WorkingLanguage.Id);
                bytes = stream.ToArray();
            }
            return File(bytes, MimeTypes.ApplicationPdf, string.Format("packagingslip_{0}.pdf", shipment.Id));
        }

        [HttpPost, ActionName("ShipmentList")]
        [FormValueRequired("exportpackagingslips-all")]
        public ActionResult PdfPackagingSlipAll(ShipmentListModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            DateTime? startDateValue = (model.StartDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.StartDate.Value, _dateTimeHelper.CurrentTimeZone);

            DateTime? endDateValue = (model.EndDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.EndDate.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);

            //a vendor should have access only to his products
            int vendorId = 0;
            if (_workContext.CurrentVendor != null)
                vendorId = _workContext.CurrentVendor.Id;

            //load shipments
            var shipments = _shipmentService.GetAllShipments(vendorId: vendorId,
                warehouseId: model.WarehouseId,
                shippingCountryId: model.CountryId,
                shippingStateId: model.StateProvinceId,
                shippingCity: model.City,
                trackingNumber: model.TrackingNumber,
                loadNotShipped: model.LoadNotShipped,
                createdFromUtc: startDateValue,
                createdToUtc: endDateValue);

            //ensure that we at least one shipment selected
            if (!shipments.Any())
            {
                ErrorNotification(_localizationService.GetResource("Admin.Orders.Shipments.NoShipmentsSelected"));
                return RedirectToAction("ShipmentList");
            }

            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                _pdfService.PrintPackagingSlipsToPdf(stream, shipments, _orderSettings.GeneratePdfInvoiceInCustomerLanguage ? 0 : _workContext.WorkingLanguage.Id);
                bytes = stream.ToArray();
            }
            return File(bytes, MimeTypes.ApplicationPdf, "packagingslips.pdf");
        }

        [HttpPost]
        public ActionResult PdfPackagingSlipSelected(string selectedIds)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var shipments = new List<Shipment>();
            if (selectedIds != null)
            {
                var ids = selectedIds
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => Convert.ToInt32(x))
                    .ToArray();
                shipments.AddRange(_shipmentService.GetShipmentsByIds(ids));
            }
            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                shipments = shipments.Where(HasAccessToShipment).ToList();
            }

            //ensure that we at least one shipment selected
            if (!shipments.Any())
            {
                ErrorNotification(_localizationService.GetResource("Admin.Orders.Shipments.NoShipmentsSelected"));
                return RedirectToAction("ShipmentList");
            }

            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                _pdfService.PrintPackagingSlipsToPdf(stream, shipments, _orderSettings.GeneratePdfInvoiceInCustomerLanguage ? 0 : _workContext.WorkingLanguage.Id);
                bytes = stream.ToArray();
            }
            return File(bytes, MimeTypes.ApplicationPdf, "packagingslips.pdf");
        }

        [HttpPost]
        public ActionResult SetAsShippedSelected(ICollection<int> selectedIds)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var shipments = new List<Shipment>();
            if (selectedIds != null)
            {
                shipments.AddRange(_shipmentService.GetShipmentsByIds(selectedIds.ToArray()));
            }
            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                shipments = shipments.Where(HasAccessToShipment).ToList();
            }

            foreach (var shipment in shipments)
            {
                try
                {
                    _orderProcessingService.Ship(shipment, true);
                }
                catch
                {
                    //ignore any exception
                }
            }

            return Json(new { Result = true });
        }

        [HttpPost]
        public ActionResult SetAsDeliveredSelected(ICollection<int> selectedIds)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var shipments = new List<Shipment>();
            if (selectedIds != null)
            {
                shipments.AddRange(_shipmentService.GetShipmentsByIds(selectedIds.ToArray()));
            }
            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                shipments = shipments.Where(HasAccessToShipment).ToList();
            }

            foreach (var shipment in shipments)
            {
                try
                {
                    _orderProcessingService.Deliver(shipment, true);
                }
                catch
                {
                    //ignore any exception
                }
            }

            return Json(new { Result = true });
        }

        #endregion

        #region Order notes

        [HttpPost]
        public ActionResult OrderNotesSelect(int orderId, DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var order = _orderService.GetOrderById(orderId);
            if (order == null)
                throw new ArgumentException("No order found with the specified id");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null)
                return Content("");

            //order notes
            var orderNoteModels = new List<OrderModel.OrderNote>();
            foreach (var orderNote in order.OrderNotes
                .OrderByDescending(on => on.CreatedOnUtc)) /// NU-37
            {
                var download = _downloadService.GetDownloadById(orderNote.DownloadId);
                orderNoteModels.Add(new OrderModel.OrderNote
                {
                    Id = orderNote.Id,
                    OrderId = orderNote.OrderId,
                    DownloadId = orderNote.DownloadId,
                    DownloadGuid = download != null ? download.DownloadGuid : Guid.Empty,
                    DisplayToCustomer = orderNote.DisplayToCustomer,
                    Note = orderNote.FormatOrderNoteText(),
                    CreatedOn = _dateTimeHelper.ConvertToUserTime(orderNote.CreatedOnUtc, DateTimeKind.Utc)
                });
            }

            var gridModel = new DataSourceResult
            {
                Data = orderNoteModels
                    .OrderByDescending(o => o.CreatedOn), /// NU-37
                Total = orderNoteModels.Count
            };

            return Json(gridModel);
        }

        [ValidateInput(false)]
        public ActionResult OrderNoteAdd(int orderId, int downloadId, bool displayToCustomer, string message)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var order = _orderService.GetOrderById(orderId);
            if (order == null)
                return Json(new { Result = false }, JsonRequestBehavior.AllowGet);

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null)
                return Json(new { Result = false }, JsonRequestBehavior.AllowGet);

            var orderNote = new OrderNote
            {
                DisplayToCustomer = displayToCustomer,
                Note = message,
                DownloadId = downloadId,
                CreatedOnUtc = DateTime.UtcNow,
            };
            order.OrderNotes.Add(orderNote);
            _orderService.UpdateOrder(order);

            //new order notification
            if (displayToCustomer)
            {
                //email
                _workflowMessageService.SendNewOrderNoteAddedCustomerNotification(
                    orderNote, _workContext.WorkingLanguage.Id);

            }

            return Json(new { Result = true }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult OrderNoteDelete(int id, int orderId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var order = _orderService.GetOrderById(orderId);
            if (order == null)
                throw new ArgumentException("No order found with the specified id");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null)
                return RedirectToAction("Edit", "Order", new { id = orderId });

            var orderNote = order.OrderNotes.FirstOrDefault(on => on.Id == id);
            if (orderNote == null)
                throw new ArgumentException("No order note found with the specified id");
            _orderService.DeleteOrderNote(orderNote);

            return new NullJsonResult();
        }

        #endregion

        #region Reports

        [NonAction]
        protected DataSourceResult GetBestsellersBriefReportModel(int pageIndex,
            int pageSize, int orderBy)
        {
            //a vendor should have access only to his products
            int vendorId = 0;
            if (_workContext.CurrentVendor != null)
                vendorId = _workContext.CurrentVendor.Id;

            var items = _orderReportService.BestSellersReport(
                storeId: _storeContext.CurrentStore.Id,	/// NU-36
                vendorId: vendorId,
                orderBy: orderBy,
                pageIndex: pageIndex,
                pageSize: pageSize,
                showHidden: true);
            var gridModel = new DataSourceResult
            {
                Data = items.Select(x =>
                {
                    var m = new BestsellersReportLineModel
                    {
                        ProductId = x.ProductId,
                        TotalAmount = _priceFormatter.FormatPrice(x.TotalAmount, true, false),
                        TotalQuantity = x.TotalQuantity,
                    };
                    var product = _productService.GetProductById(x.ProductId);
                    if (product != null)
                        m.ProductName = product.Name;
                    return m;
                }),
                Total = items.TotalCount
            };
            return gridModel;
        }
        public ActionResult BestsellersBriefReportByQuantity()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return Content("");

            return PartialView();
        }
        [HttpPost]
        public ActionResult BestsellersBriefReportByQuantityList(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return Content("");

            var gridModel = GetBestsellersBriefReportModel(command.Page - 1,
                command.PageSize, 1);

            return Json(gridModel);
        }
        public ActionResult BestsellersBriefReportByAmount()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return Content("");

            return PartialView();
        }
        [HttpPost]
        public ActionResult BestsellersBriefReportByAmountList(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return Content("");

            var gridModel = GetBestsellersBriefReportModel(command.Page - 1,
                command.PageSize, 2);

            return Json(gridModel);
        }



        public ActionResult BestsellersReport()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var model = new BestsellersReportModel();
            //vendor
            model.IsLoggedInAsVendor = _workContext.CurrentVendor != null;

            //stores
            model.AvailableStores.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            foreach (var s in _storeService.GetAllStores())
                model.AvailableStores.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString() });

            //order statuses
            model.AvailableOrderStatuses = OrderStatus.Pending.ToSelectList(false).ToList();
            model.AvailableOrderStatuses.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });

            //payment statuses
            model.AvailablePaymentStatuses = PaymentStatus.Pending.ToSelectList(false).ToList();
            model.AvailablePaymentStatuses.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });

            //categories
            model.AvailableCategories.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            var categories = SelectListHelper.GetCategoryList(_categoryService, _cacheManager, true);
            foreach (var c in categories)
                model.AvailableCategories.Add(c);

            //manufacturers
            model.AvailableManufacturers.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            foreach (var m in _manufacturerService.GetAllManufacturers(showHidden: true))
                model.AvailableManufacturers.Add(new SelectListItem { Text = m.Name, Value = m.Id.ToString() });

            //billing countries
            foreach (var c in _countryService.GetAllCountriesForBilling(showHidden: true))
                model.AvailableCountries.Add(new SelectListItem { Text = c.Name, Value = c.Id.ToString() });
            model.AvailableCountries.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });

            //vendors
            model.AvailableVendors.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            var vendors = _vendorService.GetAllVendors(showHidden: true);
            foreach (var v in vendors)
                model.AvailableVendors.Add(new SelectListItem { Text = v.Name, Value = v.Id.ToString() });

            return View(model);
        }
        [HttpPost]
        public ActionResult BestsellersReportList(DataSourceRequest command, BestsellersReportModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return Content("");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                model.VendorId = _workContext.CurrentVendor.Id;
            }

            DateTime? startDateValue = (model.StartDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.StartDate.Value, _dateTimeHelper.CurrentTimeZone);

            DateTime? endDateValue = (model.EndDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.EndDate.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);

            OrderStatus? orderStatus = model.OrderStatusId > 0 ? (OrderStatus?)(model.OrderStatusId) : null;
            PaymentStatus? paymentStatus = model.PaymentStatusId > 0 ? (PaymentStatus?)(model.PaymentStatusId) : null;

            var items = _orderReportService.BestSellersReport(
                createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                os: orderStatus,
                ps: paymentStatus,
                billingCountryId: model.BillingCountryId,
                orderBy: 2,
                vendorId: model.VendorId,
                categoryId: model.CategoryId,
                manufacturerId: model.ManufacturerId,
                storeId: _storeContext.CurrentStore.Id, // NU-42
                pageIndex: command.Page - 1,
                pageSize: command.PageSize,
                showHidden: true)
                .OrderByDescending(r => r.TotalQuantity).ToList();   // NU-47
            var gridModel = new DataSourceResult
            {
                Data = items.Select(x =>
                {
                    var m = new BestsellersReportLineModel
                    {
                        ProductId = (_workContext.CurrentCustomer.IsVendor() ? _productService.GetProductMaster(_productService.GetProductById(x.ProductId)).Id : x.ProductId),	/// NU-10
                        TotalAmount = _priceFormatter.FormatPrice(x.TotalAmount, true, false),
                        TotalQuantity = x.TotalQuantity,
                    };
                    var product = _productService.GetProductById(m.ProductId);	/// NU-10
                    if (product != null)
                        m.ProductName = product.Name;
                    return m;
                }),
                Total = items.Count()   // NU-47
            };

            return Json(gridModel);
        }



        public ActionResult NeverSoldReport()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var model = new NeverSoldReportModel();

            return View(model);
        }
        [HttpPost]
        public ActionResult NeverSoldReportList(DataSourceRequest command, NeverSoldReportModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return Content("");

            //a vendor should have access only to his products
            int vendorId = 0;
            if (_workContext.CurrentVendor != null)
                vendorId = _workContext.CurrentVendor.Id;

            DateTime? startDateValue = (model.StartDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.StartDate.Value, _dateTimeHelper.CurrentTimeZone);

            DateTime? endDateValue = (model.EndDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.EndDate.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);

            var items = _orderReportService.ProductsNeverSold(vendorId,
                startDateValue, endDateValue,
                command.Page - 1, command.PageSize, true,
                _storeContext.CurrentStore.Id)	// NU-36
                .OrderBy(r => r.Name).ToList(); // NU-24
            var gridModel = new DataSourceResult
            {
                Data = items.Select(x =>
                    new NeverSoldReportLineModel
                    {
                        ProductId = x.Id,
                        ProductName = x.Name,
                    }),
                Total = items.Count()   // NU-24
            };

            return Json(gridModel);
        }


        [ChildActionOnly]
        public ActionResult OrderAverageReport()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return Content("");

            return PartialView();
        }
        [HttpPost]
        public ActionResult OrderAverageReportList(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return Content("");

            //a vendor doesn't have access to this report
            if (_workContext.CurrentVendor != null)
                return Content("");


            var report = new List<OrderAverageReportLineSummary>();
            report.Add(_orderReportService.OrderAverageReport(0, OrderStatus.Pending));
            report.Add(_orderReportService.OrderAverageReport(0, OrderStatus.Processing));
            report.Add(_orderReportService.OrderAverageReport(0, OrderStatus.Complete));
            report.Add(_orderReportService.OrderAverageReport(0, OrderStatus.Cancelled));
            var model = report.Select(x => new OrderAverageReportLineSummaryModel
            {
                OrderStatus = x.OrderStatus.GetLocalizedEnum(_localizationService, _workContext),
                SumTodayOrders = _priceFormatter.FormatPrice(x.SumTodayOrders, true, false),
                SumThisWeekOrders = _priceFormatter.FormatPrice(x.SumThisWeekOrders, true, false),
                SumThisMonthOrders = _priceFormatter.FormatPrice(x.SumThisMonthOrders, true, false),
                SumThisYearOrders = _priceFormatter.FormatPrice(x.SumThisYearOrders, true, false),
                SumAllTimeOrders = _priceFormatter.FormatPrice(x.SumAllTimeOrders, true, false),
            }).ToList();

            var gridModel = new DataSourceResult
            {
                Data = model,
                Total = model.Count
            };

            return Json(gridModel);
        }

        [ChildActionOnly]
        public ActionResult OrderIncompleteReport()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return Content("");

            return PartialView();
        }
        [HttpPost]
        public ActionResult OrderIncompleteReportList(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return Content("");

            //a vendor doesn't have access to this report
            if (_workContext.CurrentVendor != null)
                return Content("");

            var model = new List<OrderIncompleteReportLineModel>();
            //not paid
            var orderStatuses = Enum.GetValues(typeof(OrderStatus)).Cast<int>().Where(os => os != (int)OrderStatus.Cancelled).ToList();
            var paymentStatuses = new List<int>() { (int)PaymentStatus.Pending };
            var psPending = _orderReportService.GetOrderAverageReportLine(psIds: paymentStatuses, osIds: orderStatuses);
            model.Add(new OrderIncompleteReportLineModel
            {
                Item = _localizationService.GetResource("Admin.SalesReport.Incomplete.TotalUnpaidOrders"),
                Count = psPending.CountOrders,
                Total = _priceFormatter.FormatPrice(psPending.SumOrders, true, false),
                ViewLink = Url.Action("List", "Order", new
                {
                    orderStatusIds = string.Join(",", orderStatuses),
                    paymentStatusIds = string.Join(",", paymentStatuses)
                })
            });
            //not shipped
            var shippingStatuses = new List<int>() { (int)ShippingStatus.NotYetShipped };
            var ssPending = _orderReportService.GetOrderAverageReportLine(osIds: orderStatuses, ssIds: shippingStatuses);
            model.Add(new OrderIncompleteReportLineModel
            {
                Item = _localizationService.GetResource("Admin.SalesReport.Incomplete.TotalNotShippedOrders"),
                Count = ssPending.CountOrders,
                Total = _priceFormatter.FormatPrice(ssPending.SumOrders, true, false),
                ViewLink = Url.Action("List", "Order", new
                {
                    orderStatusIds = string.Join(",", orderStatuses),
                    shippingStatusIds = string.Join(",", shippingStatuses)
                })
            });
            //pending
            orderStatuses = new List<int>() { (int)OrderStatus.Pending };
            var osPending = _orderReportService.GetOrderAverageReportLine(osIds: orderStatuses);
            model.Add(new OrderIncompleteReportLineModel
            {
                Item = _localizationService.GetResource("Admin.SalesReport.Incomplete.TotalIncompleteOrders"),
                Count = osPending.CountOrders,
                Total = _priceFormatter.FormatPrice(osPending.SumOrders, true, false),
                ViewLink = Url.Action("List", "Order", new { orderStatusIds = string.Join(",", orderStatuses) })
            });

            var gridModel = new DataSourceResult
            {
                Data = model,
                Total = model.Count
            };

            return Json(gridModel);
        }



        public ActionResult CountryReport()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.OrderCountryReport))
                return AccessDeniedView();

            var model = new CountryReportModel();

            //order statuses
            model.AvailableOrderStatuses = OrderStatus.Pending.ToSelectList(false).ToList();
            model.AvailableOrderStatuses.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });

            //payment statuses
            model.AvailablePaymentStatuses = PaymentStatus.Pending.ToSelectList(false).ToList();
            model.AvailablePaymentStatuses.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });

            return View(model);
        }
        [HttpPost]
        public ActionResult CountryReportList(DataSourceRequest command, CountryReportModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.OrderCountryReport))
                return Content("");

            DateTime? startDateValue = (model.StartDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.StartDate.Value, _dateTimeHelper.CurrentTimeZone);

            DateTime? endDateValue = (model.EndDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.EndDate.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);

            OrderStatus? orderStatus = model.OrderStatusId > 0 ? (OrderStatus?)(model.OrderStatusId) : null;
            PaymentStatus? paymentStatus = model.PaymentStatusId > 0 ? (PaymentStatus?)(model.PaymentStatusId) : null;

            var items = _orderReportService.GetCountryReport(
                os: orderStatus,
                ps: paymentStatus,
                startTimeUtc: startDateValue,
                endTimeUtc: endDateValue,
                storeId: _storeContext.CurrentStore.Id)	/// NU-42
                .OrderByDescending(r => r.TotalOrders).ToList();    /// NU-47
            var gridModel = new DataSourceResult
            {
                Data = items.Select(x =>
                {
                    var country = _countryService.GetCountryById(x.CountryId.HasValue ? x.CountryId.Value : 0);
                    var m = new CountryReportLineModel
                    {
                        CountryName = country != null ? country.Name : "Unknown",
                        SumOrders = _priceFormatter.FormatPrice(x.SumOrders, true, false),
                        TotalOrders = x.TotalOrders,
                    };
                    return m;
                }),
                Total = items.Count
            };

            return Json(gridModel);
        }

        [ChildActionOnly]
        public ActionResult OrderStatistics()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return Content("");

            //a vendor doesn't have access to this report
            if (_workContext.CurrentVendor != null)
                return Content("");

            return PartialView();
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult LoadOrderStatistics(string period)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return Content("");

            //a vendor doesn't have access to this report
            if (_workContext.CurrentVendor != null)
                return Content("");

            var result = new List<object>();

            var nowDt = _dateTimeHelper.ConvertToUserTime(DateTime.Now);
            var timeZone = _dateTimeHelper.CurrentTimeZone;

            switch (period)
            {
                case "year":
                    //year statistics
                    var yearAgoRoundedDt = nowDt.AddYears(-1).AddMonths(1);
                    var searchYearDateUser = new DateTime(yearAgoRoundedDt.Year, yearAgoRoundedDt.Month, 1);
                    if (!timeZone.IsInvalidTime(searchYearDateUser))
                    {
                        DateTime searchYearDateUtc = _dateTimeHelper.ConvertToUtcTime(searchYearDateUser, timeZone);

                        for (int i = 0; i <= 12; i++)
                        {
                            result.Add(new
                            {
                                date = searchYearDateUser.Date.ToString("Y"),
                                value = _orderService.SearchOrders(storeId: _storeContext.CurrentStore.Id,
                                    createdFromUtc: searchYearDateUtc,
                                    createdToUtc: searchYearDateUtc.AddMonths(1),
                                    pageIndex: 0,
                                    pageSize: 1).TotalCount.ToString()
                            });

                            searchYearDateUtc = searchYearDateUtc.AddMonths(1);
                            searchYearDateUser = searchYearDateUser.AddMonths(1);
                        }
                    }
                    break;

                case "month":
                    //month statistics
                    var searchMonthDateUser = new DateTime(nowDt.Year, nowDt.AddDays(-30).Month, nowDt.AddDays(-30).Day);
                    if (!timeZone.IsInvalidTime(searchMonthDateUser))
                    {
                        DateTime searchMonthDateUtc = _dateTimeHelper.ConvertToUtcTime(searchMonthDateUser, timeZone);

                        for (int i = 0; i <= 30; i++)
                        {
                            result.Add(new
                            {
                                date = searchMonthDateUser.Date.ToString("M"),
                                value = _orderService.SearchOrders(storeId: _storeContext.CurrentStore.Id,
                                    createdFromUtc: searchMonthDateUtc,
                                    createdToUtc: searchMonthDateUtc.AddDays(1),
                                    pageIndex: 0,
                                    pageSize: 1).TotalCount.ToString()
                            });

                            searchMonthDateUtc = searchMonthDateUtc.AddDays(1);
                            searchMonthDateUser = searchMonthDateUser.AddDays(1);
                        }
                    }
                    break;

                case "week":
                default:
                    //week statistics
                    var searchWeekDateUser = new DateTime(nowDt.Year, nowDt.AddDays(-7).Month, nowDt.AddDays(-7).Day);
                    if (!timeZone.IsInvalidTime(searchWeekDateUser))
                    {
                        DateTime searchWeekDateUtc = _dateTimeHelper.ConvertToUtcTime(searchWeekDateUser, timeZone);

                        for (int i = 0; i <= 7; i++)
                        {
                            result.Add(new
                            {
                                date = searchWeekDateUser.Date.ToString("d dddd"),
                                value = _orderService.SearchOrders(storeId: _storeContext.CurrentStore.Id,
                                    createdFromUtc: searchWeekDateUtc,
                                    createdToUtc: searchWeekDateUtc.AddDays(1),
                                    pageIndex: 0,
                                    pageSize: 1).TotalCount.ToString()
                            });

                            searchWeekDateUtc = searchWeekDateUtc.AddDays(1);
                            searchWeekDateUser = searchWeekDateUser.AddDays(1);
                        }
                    }
                    break;
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [ChildActionOnly]
        public ActionResult LatestOrders()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return Content("");

            return PartialView();
        }

        #endregion

        #region Activity log

        [NonAction]
        protected void LogEditOrder(int orderId)
        {
            _customerActivityService.InsertActivity("EditOrder", _localizationService.GetResource("ActivityLog.EditOrder"), orderId);
        }

        #region NU-64
        public IList<StoreCommissionReportLineModel> GetVendorItems(IPagedList<Order> orders, int vendorId)
        {
            IList<StoreCommissionReportLineModel> ret = new List<StoreCommissionReportLineModel>();

            foreach (var order in orders)
            {
                foreach (var item in order.OrderItems)
                {
                    if (
                        (_workContext.CurrentCustomer.IsVendor() && item.Product.VendorId == vendorId) ||
                        (_workContext.CurrentCustomer.IsSystemGlobalAdmin() && item.Product.VendorId > 0)
                        )
                    {
                        var line = new StoreCommissionReportLineModel();

                        var store = _storeService.GetStoreById(order.StoreId);
                        var rate = item.StoreCommission != null ? item.StoreCommission : 0; ;
                        var commission = item.PriceExclTax * ((decimal)rate / 100);
                        var earned = item.PriceExclTax - commission;

                        line.OrderId = order.Id;
                        line.StoreName = store != null ? store.Name : "Unknown";
                        line.UnitNumber = store != null ? store.LegalEntity : "Unknown";
                        line.ProductName = item.Product.Name;
                        line.Quantity = item.Quantity;
                        line.Rate = rate;
                        line.Commission = _priceFormatter.FormatPrice(commission);
                        line.Earned = _priceFormatter.FormatPrice(earned);
                        line.Total = _priceFormatter.FormatPrice(item.PriceExclTax, true, false);
                        line.Date = _dateTimeHelper.ConvertToUserTime(order.CreatedOnUtc, DateTimeKind.Utc).ToString();
                        line.VendorName = _vendorService.GetVendorById(item.Product.VendorId).Name;

                        ret.Add(line);
                    }
                }
            }

            return ret;
        }

        public ActionResult StoreCommissionsReport()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var model = new StoreCommissionReportModel();

            //stores
            if (_workContext.CurrentCustomer.IsSystemGlobalAdmin())
                model.AvailableStores.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            foreach (var s in _storeService.GetAllStores(_workContext.CurrentCustomer).OrderBy(s => s.Name))    // NU-75
                model.AvailableStores.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString() });

            //vendors
            if (_workContext.CurrentCustomer.IsSystemGlobalAdmin())
            {
                model.AvailableVendors.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            }
            int vendorId = _workContext.CurrentCustomer.IsVendor() ? _workContext.CurrentCustomer.VendorId : 0;
            foreach (var v in _vendorService.GetAllVendors(showHidden: true, isVendor: _workContext.CurrentCustomer.IsVendor(), vendorId: vendorId).OrderBy(v => v.Name))   // NU-51
                model.AvailableVendors.Add(new SelectListItem { Text = v.Name, Value = v.Id.ToString() });

            //a vendor should have access only to orders with his products
            model.IsLoggedInAsVendor = _workContext.CurrentVendor != null;

            return View(model);
        }

        [HttpPost]
        public ActionResult StoreCommissionsReportList(DataSourceRequest command, StoreCommissionReportModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageVendors))
                return AccessDeniedView();

            DateTime? startDateValue = (model.StartDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.StartDate.Value, _dateTimeHelper.CurrentTimeZone);

            DateTime? endDateValue = (model.EndDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.EndDate.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);

            var filterByProductId = 0;
            var product = _productService.GetProductById(model.ProductId);
            if (product != null && HasAccessToProduct(product))
                filterByProductId = model.ProductId;

            var orders = _orderService.SearchOrders(storeId: _storeContext.CurrentStore.Id,	/// NU-42
                vendorId: model.VendorId,
                productId: filterByProductId,
                createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                pageIndex: command.Page - 1,
                pageSize: command.PageSize);

            var commissions = GetVendorItems(orders, model.VendorId);

            var gridModel = new DataSourceResult
            {
                Data = commissions.Select(x =>
                {
                    return new StoreCommissionReportLineModel
                    {
                        OrderId = x.OrderId,
                        StoreName = x.StoreName,
                        ProductName = x.ProductName,
                        Quantity = x.Quantity,
                        Rate = x.Rate,
                        Commission = x.Commission,
                        Earned = x.Earned,
                        Total = x.Total,
                        Date = x.Date,
                        VendorName = x.VendorName,
                    };
                }).OrderByDescending(o => o.OrderId),
                Total = commissions.Count()
            };

            var reportSummary = _orderReportService.GetOrderAverageReportLine(
                storeId: _storeContext.CurrentStore.Id,	/// NU-42
                vendorId: model.VendorId,
                orderId: 0,
                startTimeUtc: startDateValue,
                endTimeUtc: endDateValue,
                onlyVendorOrders: true);
            var primaryStoreCurrency = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId);
            if (primaryStoreCurrency == null)
                throw new Exception("Cannot load primary store currency");

            gridModel.ExtraData = new StoreCommissionAggregatorModel
            {
                aggregatorcommission = _priceFormatter.FormatPrice(reportSummary.SumCommission, true, false),
                aggregatorearned = _priceFormatter.FormatPrice(reportSummary.SumEarned, true, false),
                aggregatortotal = _priceFormatter.FormatPrice(reportSummary.SumNoTax, true, false),
            };
            return new JsonResult
            {
                Data = gridModel
            };
        }

        [HttpPost, ActionName("StoreCommissionsReport")]
        [FormValueRequired("exportstorecommissions-xml")]
        public ActionResult ExportStoreCommissionsXml(StoreCommissionReportModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            DateTime? startDateValue = (model.StartDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.StartDate.Value, _dateTimeHelper.CurrentTimeZone);

            DateTime? endDateValue = (model.EndDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.EndDate.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);

            var filterByProductId = 0;
            var product = _productService.GetProductById(model.ProductId);
            if (product != null && HasAccessToProduct(product))
                filterByProductId = model.ProductId;

            var orders = _orderService.SearchOrders(storeId: _storeContext.CurrentStore.Id,	/// NU-42
                vendorId: model.VendorId,
                productId: filterByProductId,
                createdFromUtc: startDateValue,
                createdToUtc: endDateValue);

            var commissions = GetVendorItems(orders, model.VendorId);

            try
            {
                var xml = _exportManager.ExportStoreCommissionsToXml(commissions);
                return new XmlDownloadResult(xml, "storecommissions.xml");
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("StoreCommissionsReport");
            }
        }

        [HttpPost, ActionName("StoreCommissionsReport")]
        [FormValueRequired("exportstorecommissions-excel")]
        public ActionResult ExportStoreCommissionsExcel(StoreCommissionReportModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            DateTime? startDateValue = (model.StartDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.StartDate.Value, _dateTimeHelper.CurrentTimeZone);

            DateTime? endDateValue = (model.EndDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.EndDate.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);

            var filterByProductId = 0;
            var product = _productService.GetProductById(model.ProductId);
            if (product != null && HasAccessToProduct(product))
                filterByProductId = model.ProductId;

            var orders = _orderService.SearchOrders(storeId: _storeContext.CurrentStore.Id,	/// NU-42
                vendorId: model.VendorId,
                productId: filterByProductId,
                createdFromUtc: startDateValue,
                createdToUtc: endDateValue);

            var commissions = GetVendorItems(orders, model.VendorId);

            try
            {
                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    _exportManager.ExportStoreCommissionsToXlsx(stream, commissions);
                    bytes = stream.ToArray();
                }
                return File(bytes, "text/xls", "storecommissions.xlsx");
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("StoreCommissionsReport");
            }
        }

        [HttpPost, ActionName("StoreCommissionsReport")]
        [FormValueRequired("exportstorecommissions-pdf")]
        public ActionResult ExportStoreCommissionsPdf(StoreCommissionReportModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            DateTime? startDateValue = (model.StartDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.StartDate.Value, _dateTimeHelper.CurrentTimeZone);

            DateTime? endDateValue = (model.EndDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.EndDate.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);

            var filterByProductId = 0;
            var product = _productService.GetProductById(model.ProductId);
            if (product != null && HasAccessToProduct(product))
                filterByProductId = model.ProductId;

            var orders = _orderService.SearchOrders(storeId: _storeContext.CurrentStore.Id,	/// NU-42
                vendorId: model.VendorId,
                productId: filterByProductId,
                createdFromUtc: startDateValue,
                createdToUtc: endDateValue);

            var commissions = GetVendorItems(orders, model.VendorId);

            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                _pdfService.PrintStoreCommissionsToPdf(stream, commissions);
                bytes = stream.ToArray();
            }
            return File(bytes, "application/pdf", "storecommissions.pdf");
        }

        #endregion

        #region SODMYWAY-71
        public ActionResult DeliveryReport()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var model = new DeliveryReportModel();

            return View(model);
        }

        [HttpPost]
        public ActionResult DeliveryReportList(DataSourceRequest command, DeliveryReportModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return Content("");

            DateTime? startDateValue = (model.StartDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.StartDate.Value, _dateTimeHelper.CurrentTimeZone);

            DateTime? endDateValue = (model.EndDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.EndDate.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);

            TimeZoneInfo storeTimeZone = _dateTimeHelper.GetStoreTimeZone(_storeContext.CurrentStore);  // NU-67
            var orders = _orderReportService.SearchDeliveriesByUnit(
                storeId: _storeContext.CurrentStore.Id, // NU-42
                startDate: startDateValue,
                endDate: endDateValue,
                unitNumber: model.UnitNumber,
                timeZone: storeTimeZone,
                pageIndex: command.Page - 1,
                pageSize: command.PageSize);

            var gridModel = new DataSourceResult
            {
                Data = orders,
                Total = orders.Count
            };

            return Json(gridModel);
        }


        [HttpPost]
        public ActionResult PaymentReportList(DataSourceRequest command, DeliveryReportModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return Content("");

            DateTime? startDateValue = (model.StartDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.StartDate.Value, _dateTimeHelper.CurrentTimeZone);

            DateTime? endDateValue = (model.EndDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.EndDate.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);

            TimeZoneInfo storeTimeZone = _dateTimeHelper.GetStoreTimeZone(_storeContext.CurrentStore);  // NU-67
            var orders = _orderReportService.SearchDeliveriesByUnit(
                storeId: _storeContext.CurrentStore.Id, // NU-42
                startDate: startDateValue,
                endDate: endDateValue,
                unitNumber: model.UnitNumber,
                timeZone: storeTimeZone,
                pageIndex: command.Page - 1,
                pageSize: command.PageSize);

            var gridModel = new DataSourceResult
            {
                Data = orders,
                Total = orders.Count
            };

            return Json(gridModel);
        }

        //[HttpPost, ActionName("PaymentReport")]
        //[FormValueRequired("exportxml-all-paymentreport")]
        //public ActionResult PaymentReportExcel(DeliveryReportModel model)
        //{
        //    if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
        //        return AccessDeniedView();

        //    //a vendor cannot export orders
        //    if (_workContext.CurrentVendor != null)
        //        return AccessDeniedView();

        //    DateTime? startDateValue = (model.StartDate == null) ? null
        //                    : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.StartDate.Value, _dateTimeHelper.CurrentTimeZone);

        //    DateTime? endDateValue = (model.EndDate == null) ? null
        //                    : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.EndDate.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);

        //    TimeZoneInfo storeTimeZone = _dateTimeHelper.GetStoreTimeZone(_storeContext.CurrentStore);  // NU-67
        //    var orders = _reportingService.PaymentsForExcel(
        //        storeId: _storeContext.CurrentStore.Id,
        //        startDate: startDateValue,  
        //        endDate: endDateValue,
        //        unitNumber: model.UnitNumber,
        //        timeZone: storeTimeZone);

        //    try
        //    {

        //        string fileName = string.Format("payments_{0}_{1}.xlsx", DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"), CommonHelper.GenerateRandomDigitCode(4));
        //        string filePath = string.Format("{0}content\\files\\ExportImport\\{1}", Request.PhysicalApplicationPath, fileName);

        //        byte[] bytes = _exportManager.ExportOrdersToXlsx(orders);
        //        return File(bytes, MimeTypes.TextXlsx, filePath);
        //    }
        //    catch (Exception exc)
        //    {
        //        ErrorNotification(exc);
        //        return RedirectToAction("PaymentReport");
        //    }
        //}

        [HttpPost, ActionName("DeliveryReport")]
        [FormValueRequired("exportxml-all-deliveryreport")]
        public ActionResult DeliveryReportExcel(DeliveryReportModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            //a vendor cannot export orders
            if (_workContext.CurrentVendor != null)
                return AccessDeniedView();

            DateTime? startDateValue = (model.StartDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.StartDate.Value, _dateTimeHelper.CurrentTimeZone);

            DateTime? endDateValue = (model.EndDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.EndDate.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);

            TimeZoneInfo storeTimeZone = _dateTimeHelper.GetStoreTimeZone(_storeContext.CurrentStore);  // NU-67
            var orders = _orderReportService.SearchDeliveriesByUnit(
                storeId: _storeContext.CurrentStore.Id,
                startDate: startDateValue,
                endDate: endDateValue,
                unitNumber: model.UnitNumber,
                timeZone: storeTimeZone);

            try
            {
                byte[] bytes = _exportManager.ExportOrdersToXlsx(orders);
                return File(bytes, MimeTypes.TextXlsx, "deliveryreport.xlsx");
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("DeliveryReport");
            }
        }

        #endregion

        #endregion


        #region old reports
        [HttpPost, ActionName("List")]
        [FormValueRequired("payment-report-all-old")]
        public ActionResult PaymentReportAllOld(OrderListModel model)
        {

            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                model.VendorId = _workContext.CurrentVendor.Id;
            }

            DateTime? startDateValue = (model.StartDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.StartDate.Value, _dateTimeHelper.CurrentTimeZone);

            DateTime? endDateValue = (model.EndDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.EndDate.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);


            TimeZoneInfo storeTimeZone = _dateTimeHelper.GetStoreTimeZone(_storeContext.CurrentStore);
            try
            {

                //var  orderStatusId = null;
                // var paymentStatusId = null;
                // var shippingStatusId =  null;
                //var  billingEmail =  null;
                // var orderGuid =  null;

                var orders = _reportingService.PaymentsForExcel(this._storeContext.CurrentStore.Id, startDateValue, endDateValue, 0, int.MaxValue, storeTimeZone);

                var glcodes = _reportingService.GetGlCodesForReport(false);

                string fileName = string.Format("payments_{0}_{1}.xlsx", DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"), CommonHelper.GenerateRandomDigitCode(4));
                string filePath = string.Format("{0}content\\files\\ExportImport\\{1}", Request.PhysicalApplicationPath, fileName);

                _exportManager.ExportPaymentReportToXlsx(filePath, orders, glcodes, DateTime.Now, DateTime.Now, "Daily Payment Report", false, _storeContext.CurrentStore.Name);

                var bytes = System.IO.File.ReadAllBytes(filePath);
                return File(bytes, "text/xls", fileName);
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("List");
            }
        }




        [HttpPost, ActionName("List")]
        [FormValueRequired("delivery-report-all-old")]
        public ActionResult DeliveryReportAllOld(OrderListModel model)
        {




            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                model.VendorId = _workContext.CurrentVendor.Id;
            }

            DateTime? startDateValue = (model.StartDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.StartDate.Value, _dateTimeHelper.CurrentTimeZone);

            DateTime? endDateValue = (model.EndDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.EndDate.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);

            var orderStatusIds = !model.OrderStatusIds.Contains(0) ? model.OrderStatusIds : null;
            var paymentStatusIds = !model.PaymentStatusIds.Contains(0) ? model.PaymentStatusIds : null;
            var shippingStatusIds = !model.ShippingStatusIds.Contains(0) ? model.ShippingStatusIds : null;


            bool getFutureDeliveries = false;

            TimeZoneInfo storeTimeZone = _dateTimeHelper.GetStoreTimeZone(_storeContext.CurrentStore);
            try
            {

                //var  orderStatusId = null;
                // var paymentStatusId = null;
                // var shippingStatusId =  null;
                //var  billingEmail =  null;
                // var orderGuid =  null;

                IList<OrderExcelExport> orders = this._reportingService.DeliveriesForExcel(this._storeContext.CurrentStore.Id, startDateValue, endDateValue, 0, int.MaxValue, getFutureDeliveries);
                IList<GlCode> glCodesForReport = this._reportingService.GetGlCodesForReport(true);
                string format = "deliveries_{0}_{1}.xlsx";
                var dateTime = DateTime.Now;
                string str1 = dateTime.ToString("yyyy-MM-dd-HH-mm-ss");
                string randomDigitCode = CommonHelper.GenerateRandomDigitCode(4);
                string fileDownloadName = string.Format(format, (object)str1, (object)randomDigitCode);
                string str2 = string.Format("{0}content\\files\\ExportImport\\{1}", (object)this.Request.PhysicalApplicationPath, (object)fileDownloadName);
                this._exportManager.ExportDeliveryReportToXlsx(str2, orders, glCodesForReport, startDateValue, endDateValue, this._storeContext.CurrentStore.Name);

                var bytes = System.IO.File.ReadAllBytes(str2);

                return this.File(bytes, "text/xls", fileDownloadName);
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("List");
            }
        }

        [HttpPost, ActionName("List")]
        [FormValueRequired("future-report-all-old")]
        public ActionResult FutureReportAllOld(OrderListModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();


            DateTime dateTime1 = new DateTime(1900, 1, 1);
            DateTime dateTime2 = DateTime.UtcNow;
            dateTime2 = dateTime2.AddDays(800.0);
            DateTime dateTime3 = dateTime2.AddTicks(-1L);
            try
            {
                IList<OrderExcelExport> orders = this._reportingService.FutureDeliveriesForExcel(this._storeContext.CurrentStore.Id, new DateTime?(dateTime1), new DateTime?(dateTime3), 0, int.MaxValue, true);
                IList<GlCode> glCodesForReport = this._reportingService.GetGlCodesForReport(true);
                string fileDownloadName = string.Format("futuredeliveries_{0}_{1}.xlsx", (object)DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"), (object)CommonHelper.GenerateRandomDigitCode(4));
                string str = string.Format("{0}content\\files\\ExportImport\\{1}", (object)this.Request.PhysicalApplicationPath, (object)fileDownloadName);
                this._exportManager.ExportPaymentReportToXlsx(str, orders, glCodesForReport, new DateTime?(dateTime1), new DateTime?(dateTime3), "Future Delivery Report", true, this._storeContext.CurrentStore.Name);
                var bytes = System.IO.File.ReadAllBytes(str);
                return this.File(bytes, "text/xls", fileDownloadName);
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("List");
            }
        }
        #endregion

        [HttpPost]
        public ActionResult GetPartiallyRefundGridDetails(int orderId)
        {
            List<PartiallyRefundGridModel> gridDataList = new List<PartiallyRefundGridModel>();
            Dictionary<string, decimal> productGlAmountList = new Dictionary<string, decimal>();
            List<DynamicVertexGLCode> vertexOrderTaxGls = new List<DynamicVertexGLCode>();
            var listProducts = new List<int>();

            var orderDetails = _orderService.GetOrderById(orderId);

            var sameProductcount = orderDetails.OrderItems.GroupBy(x => x.ProductId)
                 .Where(g => g.Count() > 1)
                 .Select(y => y.Key)
                 .ToList();
            if (sameProductcount.Any())
            {
                foreach (var itemData in sameProductcount)
                {
                    listProducts.Add(itemData);
                }
            }

            foreach (var orderItem in orderDetails.OrderItems)  ////CALLS THE DATA FROM VERTEX AS WELL AS FROM XMLS
            {
                try
                {
                    PartiallyRefundGridModel gridData = new PartiallyRefundGridModel();
                    gridData.OrderItemId = orderItem.Id;

                    if (!orderItem.IsPartialRefund)
                    {
                        bool productExistInList = false;
                        Dictionary<int, decimal> taxDictionary = new Dictionary<int, decimal>();
                        Dictionary<int, string> taxCategoryIdNameDictionary = new Dictionary<int, string>();

                        if (gridDataList.Count() > 0)
                        {
                            var isProductExist = gridDataList.Where(x => x.ProductId == orderItem.Product.Id).ToList();
                            if (isProductExist.Count() > 0)
                            {
                                productExistInList = false; // Always set this to false so that the Records dont Merge up
                            }

                        }

                        var taxCategoryList = _taxCategoryService.GetAllTaxCategories();
                        if (listProducts.Contains(orderItem.ProductId))
                        {
                            vertexOrderTaxGls = _glCodeService.GetVertexGlBreakdown(orderItem.OrderId, orderItem.ProductId, TaxRequestType.ConfirmDistributiveTax, orderItem.Id);

                        }
                        else
                        {
                            vertexOrderTaxGls = _glCodeService.GetVertexGlBreakdown(orderItem.OrderId, orderItem.ProductId, TaxRequestType.ConfirmDistributiveTax);
                        }

                        var vertexDeliveryShippingGls = vertexOrderTaxGls.Where(x => x.taxCode == "DELV" || x.taxCode == "SHIP").ToList();
                        vertexOrderTaxGls = vertexOrderTaxGls.Where(x => x.taxCode != "DELV" && x.taxCode != "SHIP").ToList();


                        var data = vertexOrderTaxGls.Where(x => x.GlCode != "44571098").ToList();

                        List<DynamicVertexGLCode> vertexCodeWithAmountList = new List<DynamicVertexGLCode>();

                        foreach (var response in vertexOrderTaxGls)
                        {
                            //if (response.GlCode == "44571098")
                            //{
                            var d = data.Where(x => x.taxCode == response.taxCode).ToList();
                            if (vertexOrderTaxGls.Count() > 0)
                            {
                                decimal total = +response.Total;
                                vertexCodeWithAmountList.Add(new DynamicVertexGLCode(response.productId, response.GlCode, response.Description, response.Total, response.taxCode));
                            }
                            else
                            {
                                vertexCodeWithAmountList.Add(new DynamicVertexGLCode(response.productId, response.GlCode, response.Description, response.Total, response.taxCode));
                            }
                            //}
                        }

                        if (orderItem.Product.TaxCategoryId == -1)
                        {

                            List<ProductGlCode> paidCodes = _glCodeService.GetProductGlCodes(orderItem.Product, GLCodeStatusType.Paid).ToList();
                            var splitGls = _glCodeService.GetProductsGlByType(1, paidCodes, true, false, true); //only gls with taxcategory
                            var bonusProduct = splitGls.Where(x => x.GlCode.Description.Contains("Bonus")).FirstOrDefault();

                            int count = 0;
                            foreach (var paidCode in paidCodes)
                            {

                                if (paidCode.GlCode.GlCodeName != "62610001" && paidCode.GlCode.GlCodeName != "70161010")
                                {
                                    string glName = paidCode.GlCode.GlCodeName + '-' + paidCode.GlCode.Description;

                                    count++;
                                    if (productExistInList)
                                    {
                                        var existingProduct = gridDataList.Where(x => x.ProductId == orderItem.Product.Id).ToList();

                                        foreach (var product in existingProduct)
                                        {
                                            if (count == 1)
                                            {
                                                if (bonusProduct != null)
                                                {
                                                    var SplitGlDepositCashForBonus = System.Configuration.ConfigurationManager.AppSettings["DepositCash"];
                                                    if (!string.IsNullOrEmpty(SplitGlDepositCashForBonus))
                                                    {
                                                        var cashvalueGl = paidCode.GlCode.GlCodeName;
                                                        if (SplitGlDepositCashForBonus.Contains(cashvalueGl))
                                                        {
                                                            if (paidCode.GlCode.GlCodeName == cashvalueGl) //DEPOSITS CASH VALUE BOARD
                                                            {
                                                                product.GLAmount1 = Convert.ToDecimal(paidCodes.Where(x => x.GlCode.GlCodeName == cashvalueGl).FirstOrDefault().Amount + paidCodes.Where(x => x.GlCode.GlCodeName == "64701000").FirstOrDefault().Amount) * orderItem.Quantity;
                                                            }
                                                        }
                                                    }
                                                }

                                                else
                                                {
                                                    product.GLAmount1 = product.GLAmount1 + paidCode.Amount * orderItem.Quantity;
                                                }
                                                gridData.GLCodeName1 = glName;
                                                gridData.TaxAmount1 = gridData.TaxAmount1 + vertexCodeWithAmountList[0].Total;
                                                gridData.TaxName1 = vertexOrderTaxGls[0].GlCode + "-" + vertexCodeWithAmountList[0].Description;

                                            }

                                            if (count == 2)
                                            {
                                                gridData.GLCodeName2 = glName;
                                                product.GLAmount2 = product.GLAmount2 + paidCode.Amount * orderItem.Quantity;
                                                gridData.TaxAmount2 = gridData.TaxAmount2 + vertexCodeWithAmountList[1].Total;
                                                gridData.TaxName2 = vertexOrderTaxGls[1].GlCode + "-" + vertexCodeWithAmountList[1].Description;

                                            }

                                            if (count == 3)
                                            {
                                                gridData.GLCodeName3 = glName;
                                                product.GLAmount3 = product.GLAmount3 + paidCode.Amount * orderItem.Quantity;

                                                gridData.TaxAmount3 = gridData.TaxAmount3 + vertexCodeWithAmountList[2].Total;
                                                gridData.TaxName3 = vertexOrderTaxGls[2].GlCode + "-" + vertexCodeWithAmountList[2].Description;
                                            }
                                        }
                                    }

                                    else
                                    {
                                        if (count == 1)
                                        {
                                            if (bonusProduct != null)
                                            {
                                                var SplitGlDepositCashForBonus = System.Configuration.ConfigurationManager.AppSettings["DepositCash"];
                                                if (!string.IsNullOrEmpty(SplitGlDepositCashForBonus))
                                                {
                                                    var cashvalueGl = paidCode.GlCode.GlCodeName;
                                                    if (SplitGlDepositCashForBonus.Contains(cashvalueGl))
                                                    {
                                                        if (paidCode.GlCode.GlCodeName == cashvalueGl) //DEPOSITS CASH VALUE BOARD
                                                        {
                                                            gridData.GLAmount1 = Convert.ToDecimal(paidCodes.Where(x => x.GlCode.GlCodeName == cashvalueGl).FirstOrDefault().Amount + paidCodes.Where(x => x.GlCode.GlCodeName == "64701000").FirstOrDefault().Amount) * orderItem.Quantity;
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                gridData.GLAmount1 = paidCode.Amount * orderItem.Quantity;
                                            }
                                            gridData.GLCodeName1 = glName;
                                            gridData.GLAmount1 = paidCode.Amount * orderItem.Quantity;
                                            gridData.TaxAmount1 = gridData.TaxAmount1 + vertexCodeWithAmountList[0].Total;
                                            gridData.TaxName1 = vertexOrderTaxGls[0].GlCode + "-" + vertexCodeWithAmountList[0].Description;

                                        }

                                        if (count == 2)
                                        {
                                            gridData.GLCodeName2 = glName;
                                            gridData.GLAmount2 = paidCode.Amount * orderItem.Quantity;
                                            if (vertexCodeWithAmountList.Count == 2)
                                            {
                                                gridData.TaxAmount1 = vertexCodeWithAmountList[0].Total;
                                                gridData.TaxName1 = vertexOrderTaxGls[0].GlCode + "-" + vertexCodeWithAmountList[0].Description;

                                                gridData.TaxAmount2 = vertexCodeWithAmountList[1].Total;
                                                gridData.TaxName2 = vertexOrderTaxGls[1].GlCode + "-" + vertexCodeWithAmountList[1].Description;
                                            }
                                            else
                                            {
                                                gridData.TaxAmount2 = gridData.TaxAmount2 + decimal.Zero;
                                                gridData.TaxName2 = "None";
                                            }

                                        }

                                        if (count == 3)
                                        {
                                            gridData.GLCodeName3 = glName;
                                            gridData.GLAmount3 = paidCode.Amount * orderItem.Quantity;
                                            if (vertexCodeWithAmountList.Count == 3)
                                            {
                                                gridData.TaxAmount1 = vertexCodeWithAmountList[0].Total;
                                                gridData.TaxName1 = vertexOrderTaxGls[0].GlCode + "-" + vertexCodeWithAmountList[0].Description;

                                                gridData.TaxAmount2 = vertexCodeWithAmountList[1].Total;
                                                gridData.TaxName2 = vertexOrderTaxGls[1].GlCode + "-" + vertexCodeWithAmountList[1].Description;

                                                gridData.TaxAmount3 = vertexCodeWithAmountList[2].Total;
                                                gridData.TaxName3 = vertexOrderTaxGls[2].GlCode + "-" + vertexCodeWithAmountList[2].Description;
                                            }
                                            else
                                            {
                                                gridData.TaxAmount3 = gridData.TaxAmount3 + decimal.Zero;
                                                gridData.TaxName3 = "None";
                                            }
                                        }
                                    }

                                }
                            }


                        }
                        else
                        {
                            List<ProductGlCode> paidCodes = _glCodeService.GetProductGlCodes(orderItem.Product, GLCodeStatusType.Paid).ToList();
                            bool vertexProcessed = false;
                            foreach (var paidCode in paidCodes)
                            {
                                if (paidCode.GlCode.GlCodeName != "62610001" && paidCode.GlCode.GlCodeName != "70161010")
                                {

                                    if (productExistInList)
                                    {
                                        var existingProduct = gridDataList.Where(x => x.ProductId == orderItem.Product.Id).ToList();
                                        foreach (var product in existingProduct)
                                        {
                                            product.GLAmount1 = product.GLAmount1 + orderItem.PriceExclTax;

                                            if (vertexOrderTaxGls.Count() > 0)
                                            {
                                                foreach (var taxAmount in vertexOrderTaxGls)
                                                {
                                                    product.TaxAmount1 = product.TaxAmount1 + taxAmount.Total;
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        gridData.GLCodeName1 = paidCode.GlCode.GlCodeName + '-' + paidCode.GlCode.Description;
                                        gridData.GLAmount1 = orderItem.PriceExclTax;

                                        if (!vertexProcessed)
                                        {
                                            if (vertexOrderTaxGls.Count() > 0)
                                            {
                                                vertexProcessed = true;


                                                // foreach (var taxAmount in vertexOrderTaxGls)
                                                //{
                                                if (vertexOrderTaxGls.Count == 1)
                                                {
                                                    gridData.TaxAmount1 = gridData.TaxAmount1 + vertexOrderTaxGls[0].Total;
                                                    gridData.TaxName1 = vertexOrderTaxGls[0].GlCode + "-" + vertexOrderTaxGls[0].Description;
                                                }
                                                if (vertexOrderTaxGls.Count == 2)
                                                {
                                                    gridData.TaxAmount1 = gridData.TaxAmount1 + vertexOrderTaxGls[0].Total;
                                                    gridData.TaxName1 = vertexOrderTaxGls[0].GlCode + "-" + vertexOrderTaxGls[0].Description;
                                                    gridData.TaxAmount2 = gridData.TaxAmount2 + vertexOrderTaxGls[1].Total;
                                                    gridData.TaxName2 = vertexOrderTaxGls[1].GlCode + "-" + vertexOrderTaxGls[1].Description;
                                                }
                                                if (vertexOrderTaxGls.Count == 3)
                                                {
                                                    gridData.TaxAmount1 = gridData.TaxAmount1 + vertexOrderTaxGls[0].Total;
                                                    gridData.TaxName1 = vertexOrderTaxGls[0].GlCode + "-" + vertexOrderTaxGls[0].Description;
                                                    gridData.TaxAmount2 = gridData.TaxAmount2 + vertexOrderTaxGls[1].Total;
                                                    gridData.TaxName2 = vertexOrderTaxGls[1].GlCode + "-" + vertexOrderTaxGls[1].Description;
                                                    gridData.TaxAmount3 = gridData.TaxAmount3 + vertexOrderTaxGls[2].Total;
                                                    gridData.TaxName3 = vertexOrderTaxGls[2].GlCode + "-" + vertexOrderTaxGls[2].Description;
                                                }
                                                //}
                                            }

                                        }
                                    }
                                }
                            }


                        }


                        if (!productExistInList)
                        {
                            gridData.ProductId = orderItem.Product.Id;
                            gridData.ProductName = orderItem.Product.Name;
                            gridData.Sku = orderItem.Product.Sku;
                            foreach (var gridataItemsForDelvandShip in vertexDeliveryShippingGls)
                            {
                                if (gridataItemsForDelvandShip.taxCode == "DELV")
                                {
                                    gridData.DeliveryTaxName = gridataItemsForDelvandShip.GlCode + "-" + gridataItemsForDelvandShip.Description;
                                    gridData.DeliveryGLCodeName = "70161010-Delivery Fee Onsite";
                                    gridData.DeliveryPickupAmount = Convert.ToDecimal(orderItem.DeliveryPickupFee);
                                    gridData.DeliveryTax = gridataItemsForDelvandShip.Total;

                                    gridData.ShippingTaxName = "None";
                                    gridData.ShippingGlName = "None";

                                }
                                else if (gridataItemsForDelvandShip.taxCode == "SHIP")
                                {
                                    gridData.ShippingTaxName = gridataItemsForDelvandShip.GlCode + "-" + gridataItemsForDelvandShip.Description;
                                    gridData.ShippingGlName = "62610001-Shipping Fee";
                                    gridData.ShippingAmount = Convert.ToDecimal(orderItem.ShippingFee);
                                    gridData.ShippingTax = gridataItemsForDelvandShip.Total;

                                    gridData.DeliveryTaxName = "None";
                                    gridData.DeliveryGLCodeName = "None";
                                }
                                else
                                {
                                    gridData.ShippingTaxName = "None";
                                    gridData.ShippingGlName = "None";

                                    gridData.DeliveryTaxName = "None";
                                    gridData.DeliveryGLCodeName = "None";
                                }
                            }

                            gridDataList.Add(gridData);
                        }
                    }
                    else
                    {
                        if (orderItem.Product.TaxCategoryId != -1)
                        {
                            gridData.ProductId = orderItem.Product.Id;
                            gridData.ProductName = orderItem.Product.Name;
                            gridData.Sku = orderItem.Product.Sku;
                            gridData.TaxAmount1 = Convert.ToDecimal(orderItem.TaxAmount1);
                            gridData.GLAmount1 = Convert.ToDecimal(orderItem.PartialRefundOrderAmount);
                            gridData.TaxAmount2 = Convert.ToDecimal(orderItem.TaxAmount2);
                            gridData.GLAmount2 = Convert.ToDecimal(orderItem.GlcodeAmount2);
                            gridData.TaxAmount3 = Convert.ToDecimal(orderItem.TaxAmount3);
                            gridData.GLAmount3 = Convert.ToDecimal(orderItem.GlcodeAmount3);
                            gridData.TotalRefund = Convert.ToDecimal(orderItem.TotalRefundedAmount);
                            gridData.RefundedTaxAmount1 = Convert.ToDecimal(orderItem.RefundedTaxAmount1);
                            gridData.RefundedTaxAmount2 = Convert.ToDecimal(orderItem.RefundedTaxAmount2);
                            gridData.RefundedTaxAmount3 = Convert.ToDecimal(orderItem.RefundedTaxAmount3);
                            gridData.GLCodeName1 = orderItem.GLCodeName1;
                            gridData.GLCodeName2 = orderItem.GLCodeName2;
                            gridData.GLCodeName3 = orderItem.GLCodeName3;
                            gridData.TaxName1 = orderItem.TaxName1;
                            gridData.TaxName2 = orderItem.TaxName2;
                            gridData.TaxName3 = orderItem.TaxName3;

                            vertexOrderTaxGls = _glCodeService.GetVertexGlBreakdown(orderItem.OrderId, orderItem.ProductId, TaxRequestType.ConfirmDistributiveTax);
                            var vertexDeliveryShippingGls = vertexOrderTaxGls.Where(x => x.taxCode == "DELV" || x.taxCode == "SHIP").ToList();
                            foreach (var gridataItemsForDelvandShip in vertexDeliveryShippingGls)
                            {
                                if (gridataItemsForDelvandShip.taxCode == "DELV")
                                {
                                    gridData.DeliveryTaxName = gridataItemsForDelvandShip.GlCode + "-" + gridataItemsForDelvandShip.Description;
                                    gridData.DeliveryGLCodeName = "70161010-Delivery Fee Onsite";
                                    gridData.DeliveryPickupAmount = orderItem.IsFullRefund ? 0 : Convert.ToDecimal(orderItem.DeliveryPickupFee);
                                    gridData.DeliveryTax = orderItem.IsFullRefund ? 0 : gridataItemsForDelvandShip.Total;

                                    gridData.ShippingTaxName = "None";
                                    gridData.ShippingGlName = "None";

                                }
                                else if (gridataItemsForDelvandShip.taxCode == "SHIP")
                                {
                                    gridData.ShippingTaxName = gridataItemsForDelvandShip.GlCode + "-" + gridataItemsForDelvandShip.Description;
                                    gridData.ShippingGlName = "62610001-Shipping Fee";
                                    gridData.ShippingAmount = orderItem.IsFullRefund ? 0 : Convert.ToDecimal(orderItem.ShippingFee);
                                    gridData.ShippingTax = orderItem.IsFullRefund ? 0 : gridataItemsForDelvandShip.Total;

                                    gridData.DeliveryTaxName = "None";
                                    gridData.DeliveryGLCodeName = "None";
                                }

                                else
                                {
                                    gridData.ShippingTaxName = "None";
                                    gridData.ShippingGlName = "None";

                                    gridData.DeliveryTaxName = "None";
                                    gridData.DeliveryGLCodeName = "None";
                                }
                            }
                            gridDataList.Add(gridData);


                        }
                        else if (orderItem.Product.TaxCategoryId == -1)
                        {
                            //List<ProductGlCode> paidCodes = _glCodeService.GetProductGlCodes(orderItem.Product, GLCodeStatusType.Paid).ToList();

                            gridData.ProductId = orderItem.Product.Id;
                            gridData.ProductName = orderItem.Product.Name;
                            gridData.Sku = orderItem.Product.Sku;
                            gridData.TaxAmount1 = Convert.ToDecimal(orderItem.TaxAmount1);
                            gridData.TaxAmount2 = Convert.ToDecimal(orderItem.TaxAmount2);
                            gridData.TaxAmount3 = Convert.ToDecimal(orderItem.TaxAmount3);
                            gridData.TotalRefund = Convert.ToDecimal(orderItem.TotalRefundedAmount);
                            gridData.GLAmount1 = Convert.ToDecimal(orderItem.GlcodeAmount1);
                            gridData.GLAmount2 = Convert.ToDecimal(orderItem.GlcodeAmount2);
                            gridData.GLAmount3 = Convert.ToDecimal(orderItem.GlcodeAmount3);
                            gridData.RefundedTaxAmount1 = Convert.ToDecimal(orderItem.RefundedTaxAmount1);
                            gridData.RefundedTaxAmount2 = Convert.ToDecimal(orderItem.RefundedTaxAmount2);
                            gridData.RefundedTaxAmount3 = Convert.ToDecimal(orderItem.RefundedTaxAmount3);
                            gridData.GLCodeName1 = orderItem.GLCodeName1;
                            gridData.GLCodeName2 = orderItem.GLCodeName2;
                            gridData.GLCodeName3 = orderItem.GLCodeName3;
                            gridData.TaxName1 = Convert.ToString(orderItem.TaxName1);
                            gridData.TaxName2 = Convert.ToString(orderItem.TaxName2);
                            gridData.TaxName3 = Convert.ToString(orderItem.TaxName3);
                            gridDataList.Add(gridData);
                        }
                    }
                }
                catch (Exception exc)
                {
                    throw exc;
                }
            } ////CALLS THE DATA FROM VERTEX AS WELL AS FROM XMLS
            foreach (var griddata in gridDataList)
            {
                griddata.OrderId = orderId;

                if (string.IsNullOrEmpty(griddata.DeliveryTaxName))
                {
                    griddata.DeliveryTaxName = "None";
                    griddata.DeliveryTax = 0;
                }
                if (string.IsNullOrEmpty(griddata.ShippingTaxName))
                {
                    griddata.ShippingTaxName = "None";
                    griddata.ShippingTax = 0;
                }

                var overriddenGlitems = GetOveriddenGls(griddata.OrderId, griddata.ProductId, griddata.OrderItemId);
                if (overriddenGlitems.Any())
                {

                    griddata.Glcodeid1 = overriddenGlitems[0];
                    griddata.Glcodeid2 = overriddenGlitems[1];
                    griddata.Glcodeid3 = overriddenGlitems[2];
                }
                else
                {
                    //Setting to Default Gl to prevent showing undeifned
                    griddata.Glcodeid1 = 00000000;
                    griddata.Glcodeid2 = 00000000;
                    griddata.Glcodeid3 = 00000000;
                }
            }

            var gridModel = new DataSourceResult
            {
                Data = gridDataList,
                Total = gridDataList.Count
            };

            return Json(gridModel);

        }
        [HttpPost]
        public ActionResult UpdateCustomRefundData(PartiallyRefundGridModel refundData)
        {
            if (String.IsNullOrEmpty(Request.Headers["consoleKey"]))
            {
                if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                    return AccessDeniedView();
            }
            else
            {
                if (Request.Headers["consoleKey"] != "c0ns0l3p4ss")
                {
                    return AccessDeniedView();
                }
            }

            var order = _orderService.GetOrderById(refundData.OrderId);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null)
                return RedirectToAction("Edit", "Order", new { id = refundData.OrderId });

            var productDetails = _productService.GetProductById(refundData.ProductId);
            var actualProductPrice = productDetails.Price;
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


            try
            {
                if (productDetails.TaxCategoryId != -1)
                {

                    if (refundData.GLAmount1 <= decimal.Zero)
                        throw new NopException("Enter amount to refund");

                    //if (refundData.GLAmount1 > order.OrderTotal || refundData.GLAmount1 > actualProductPrice)
                    //    refundData.GLAmount1 = actualProductPrice;

                    var items = order.OrderItems.Where(x => x.ProductId == refundData.ProductId && x.Id == refundData.OrderItemId).FirstOrDefault();
                    if (items.IsPartialRefund)
                        throw new Exception("The item is already Refunded");

                    decimal amount = refundData.GLAmount1 + (refundData.RefundedTaxAmount1);


                    var errors = _orderProcessingService.PartiallyRefund(order, Convert.ToDecimal(amount), items.ProductId).ToList();

                    items.PartialRefundOrderAmount = Convert.ToDecimal(items.PartialRefundOrderAmount - refundData.GLAmount1);


                    items.TaxAmount1 = refundData.TaxAmount1 - refundData.RefundedTaxAmount1;
                    items.RefundedTaxAmount1 = Convert.ToDecimal(items.RefundedTaxAmount1) + refundData.RefundedTaxAmount1;

                    items.RefundedGlAmount1 = Convert.ToDecimal(items.RefundedGlAmount1) + refundData.GLAmount1;

                    items.GlcodeAmount1 = refundData.GLAmount1;
                    items.GlcodeAmount2 = refundData.GLAmount2;
                    items.GlcodeAmount3 = refundData.GLAmount3;


                    items.TotalRefundedAmount = refundData.GLAmount1 + Convert.ToDecimal(items.TotalRefundedAmount) + (refundData.RefundedTaxAmount1);
                    items.GLCodeName1 = refundData.GLCodeName1;
                    items.GLCodeName2 = refundData.GLCodeName2;
                    items.GLCodeName3 = refundData.GLCodeName3;
                    items.TaxName1 = refundData.TaxName1;
                    items.TaxName2 = refundData.TaxName2;
                    items.TaxName3 = refundData.TaxName3;
                    items.IsPartialRefund = true;
                    _orderItemRepository.Update(items);

                    order.OrderTax = order.OrderTax - (Convert.ToDecimal(items.RefundedTaxAmount1) + Convert.ToDecimal(items.RefundedTaxAmount2) + Convert.ToDecimal(items.RefundedTaxAmount3));
                    order.OrderTotalOnRefund = order.OrderTotalOnRefund - (Convert.ToDecimal(items.RefundedGlAmount1) + Convert.ToDecimal(items.RefundedGlAmount2) + Convert.ToDecimal(items.RefundedGlAmount3) + Convert.ToDecimal(items.RefundedTaxAmount1) + Convert.ToDecimal(items.RefundedTaxAmount2) + Convert.ToDecimal(items.RefundedTaxAmount3));

                    _orderService.UpdateOrder(order);


                }
                else
                {
                    decimal amount = 0;
                    var items = order.OrderItems.Where(x => x.ProductId == refundData.ProductId && x.Id == refundData.OrderItemId).FirstOrDefault();
                    if (items.FulfillmentDateTime == null)
                        throw new Exception("Unfulfilled items may not be partially refunded");
                    if (items.IsPartialRefund)
                        throw new Exception("The item is already  Refunded");
                    List<ProductGlCode> paidCodes = _glCodeService.GetProductGlCodes(items.Product, GLCodeStatusType.Paid).ToList();
                    List<int> data = new List<int>();
                    if (!items.IsPartialRefund)
                    {
                        if (paidCodes.Count == 1)
                        {
                            if (paidCodes[0].Amount * items.Quantity != refundData.GLAmount1)
                                data.Add(1);
                        }
                        if (paidCodes.Count == 2)
                        {
                            if (paidCodes[0].Amount * items.Quantity != refundData.GLAmount1)
                                data.Add(1);
                            if (paidCodes[1].Amount * items.Quantity != refundData.GLAmount2)
                                data.Add(2);
                        }
                        if (paidCodes.Count == 3)
                        {
                            if (paidCodes[0].Amount * items.Quantity != refundData.GLAmount1)
                                data.Add(1);
                            if (paidCodes[1].Amount * items.Quantity != refundData.GLAmount2)
                                data.Add(2);
                            if (paidCodes[2].Amount * items.Quantity != refundData.GLAmount3)
                                data.Add(3);
                        }
                    }
                    else
                    {
                        if (items.GlcodeAmount1 != refundData.GLAmount1)
                            data.Add(1);
                        if (items.GlcodeAmount2 != refundData.GLAmount2)
                            data.Add(2);
                        if (items.GlcodeAmount3 != refundData.GLAmount3)
                            data.Add(3);
                    }

                    //var vertexOrderTaxGls = _glCodeService.GetVertexGlBreakdown(refundData.OrderId, refundData.ProductId, TaxRequestType.ResponseDistributiveTaxRefund);
                    //var refundedTaxtoDisplay = vertexOrderTaxGls.Where(x => x.Total <= 0).ToList();

                    if (!items.IsPartialRefund)
                    {

                        if (paidCodes.Count == 1)
                        {
                            if (paidCodes[0].Amount * items.Quantity != refundData.GLAmount1)
                                data.Add(1);
                        }
                        if (paidCodes.Count == 2)
                        {
                            if (paidCodes[0].Amount * items.Quantity != refundData.GLAmount1)
                                data.Add(1);
                            if (paidCodes[1].Amount * items.Quantity != refundData.GLAmount2)
                                data.Add(2);
                        }
                        if (paidCodes.Count == 3)
                        {
                            if (paidCodes[0].Amount * items.Quantity != refundData.GLAmount1)
                                data.Add(1);
                            if (paidCodes[1].Amount * items.Quantity != refundData.GLAmount2)
                                data.Add(2);
                            if (paidCodes[2].Amount * items.Quantity != refundData.GLAmount3)
                                data.Add(3);

                        }

                        //if (paidCodes[0].Amount != refundData.GLAmount1)
                        //    data.Add(1);
                        //if (paidCodes[1].Amount != refundData.GLAmount2)
                        //    data.Add(2);
                        //if (paidCodes[2].Amount != refundData.GLAmount3)
                        //    data.Add(3);
                    }
                    else
                    {

                        if (items.GlcodeAmount1 != refundData.GLAmount1)
                            data.Add(1);
                        if (items.GlcodeAmount2 != refundData.GLAmount2)
                            data.Add(2);
                        if (items.GlcodeAmount3 != refundData.GLAmount3)
                            data.Add(3);
                    }

                    if (data.Contains(1))
                    {
                        items.RefundedGlAmount1 = Convert.ToDecimal(items.RefundedGlAmount1) + refundData.GLAmount1; // Amount refunded for GlcodeAmount1 from UI
                        amount = amount + (refundData.GLAmount1 + (refundData.RefundedTaxAmount1)); //Summation of tax with the Refunded Glcode1Amount1 from UI
                                                                                                    //refundData.GLAmount1 = amount; //the Amount we are going to Refund Setting this Up for a common Refund Goal
                        items.RefundedTaxAmount1 = Convert.ToDecimal(items.RefundedTaxAmount1) + refundData.RefundedTaxAmount1; // The Tax amount refunded for GLAmount1. Shows the current Refunded Tax amount
                        items.TaxAmount1 = refundData.TaxAmount1 - (refundData.RefundedTaxAmount1);// Summation of Previous refunded tax amount with the Current refunded tax Amount
                        data.Remove(1);
                    }
                    if (data.Contains(2))
                    {
                        items.RefundedGlAmount2 = Convert.ToDecimal(items.RefundedGlAmount2) + refundData.GLAmount2;
                        amount = amount + (refundData.GLAmount2 + (refundData.TaxAmount2));
                        //refundData.GLAmount1 = amount;

                        items.RefundedTaxAmount2 = Convert.ToDecimal(items.RefundedTaxAmount2) + refundData.RefundedTaxAmount2;
                        items.TaxAmount2 = refundData.TaxAmount2 - (refundData.RefundedTaxAmount2);
                        data.Remove(2);
                    }
                    if (data.Contains(3))
                    {
                        items.RefundedGlAmount3 = Convert.ToDecimal(items.RefundedGlAmount3) + refundData.GLAmount3;
                        amount = amount + (refundData.GLAmount3 + (refundData.TaxAmount3));
                        //refundData.GLAmount1 = amount;

                        items.RefundedTaxAmount3 = Convert.ToDecimal(items.RefundedTaxAmount3) + refundData.RefundedTaxAmount3;
                        items.TaxAmount3 = refundData.TaxAmount3 - (refundData.RefundedTaxAmount3);
                        data.Remove(3);
                    }
                    if (items.TaxAmount1 == null)
                        items.TaxAmount1 = refundData.TaxAmount1;
                    if (items.TaxAmount2 == null)
                        items.TaxAmount2 = refundData.TaxAmount2;
                    if (items.TaxAmount3 == null)
                        items.TaxAmount3 = refundData.TaxAmount3;

                    var errors = _orderProcessingService.PartiallyRefund(order, Convert.ToDecimal(amount), items.ProductId).ToList();


                    //items.TaxAmount1 = refundData.TaxAmount1 + (vertexOrderTaxGls[0].Total);
                    //items.TaxAmount2 = refundData.TaxAmount2;
                    //items.TaxAmount3 = refundData.TaxAmount3;
                    items.TotalRefundedAmount = Convert.ToDecimal(items.TotalRefundedAmount) + amount;
                    if (!items.IsPartialRefund)
                    {
                        if (paidCodes.Count == 1)
                        {
                            items.GlcodeAmount1 = paidCodes[0].Amount * items.Quantity - Convert.ToDecimal(items.RefundedGlAmount1);
                            items.GlcodeAmount2 = 0;
                            items.GlcodeAmount3 = 0;
                        }
                        if (paidCodes.Count == 2)
                        {
                            items.GlcodeAmount1 = paidCodes[0].Amount * items.Quantity - Convert.ToDecimal(items.RefundedGlAmount1);
                            items.GlcodeAmount2 = paidCodes[1].Amount * items.Quantity - Convert.ToDecimal(items.RefundedGlAmount2);
                            items.GlcodeAmount3 = 0;
                        }
                        if (paidCodes.Count == 3)
                        {
                            items.GlcodeAmount1 = paidCodes[0].Amount * items.Quantity - Convert.ToDecimal(items.RefundedGlAmount1);
                            items.GlcodeAmount2 = paidCodes[1].Amount * items.Quantity - Convert.ToDecimal(items.RefundedGlAmount2);
                            items.GlcodeAmount3 = paidCodes[2].Amount * items.Quantity - Convert.ToDecimal(items.RefundedGlAmount3);

                        }
                    }
                    else
                    {
                        if (paidCodes.Count == 1)
                        {
                            items.GlcodeAmount1 = (items.GlcodeAmount1 != refundData.GLAmount1) ? Convert.ToDecimal(items.GlcodeAmount1) - refundData.GLAmount1 : items.GlcodeAmount1;
                            items.GlcodeAmount2 = 0;
                            items.GlcodeAmount3 = 0;
                        }
                        if (paidCodes.Count == 2)
                        {
                            items.GlcodeAmount1 = (items.GlcodeAmount1 != refundData.GLAmount1) ? Convert.ToDecimal(items.GlcodeAmount1) - refundData.GLAmount1 : items.GlcodeAmount1;
                            items.GlcodeAmount2 = (items.GlcodeAmount2 != refundData.GLAmount2) ? Convert.ToDecimal(items.GlcodeAmount2) - refundData.GLAmount2 : items.GlcodeAmount2;
                            items.GlcodeAmount3 = 0;
                        }
                        if (paidCodes.Count == 3)
                        {
                            items.GlcodeAmount1 = (items.GlcodeAmount1 != refundData.GLAmount1) ? Convert.ToDecimal(items.GlcodeAmount1) - refundData.GLAmount1 : items.GlcodeAmount1;
                            items.GlcodeAmount2 = (items.GlcodeAmount2 != refundData.GLAmount2) ? Convert.ToDecimal(items.GlcodeAmount2) - refundData.GLAmount2 : items.GlcodeAmount2;
                            items.GlcodeAmount3 = (items.GlcodeAmount3 != refundData.GLAmount3) ? Convert.ToDecimal(items.GlcodeAmount3) - refundData.GLAmount3 : items.GlcodeAmount3;

                        }
                    }

                    items.GLCodeName1 = refundData.GLCodeName1;
                    items.GLCodeName2 = refundData.GLCodeName2;
                    items.GLCodeName3 = refundData.GLCodeName3;

                    items.TaxName1 = refundData.TaxName1;
                    items.TaxName2 = refundData.TaxName2;
                    items.TaxName3 = refundData.TaxName3;

                    items.IsPartialRefund = true;
                    // items.DateOfRefund = DateTime.Now;
                    _orderItemRepository.Update(items);

                    var totalRefundedAmount = Convert.ToDecimal(items.RefundedGlAmount1) + Convert.ToDecimal(items.RefundedGlAmount2) + Convert.ToDecimal(items.RefundedGlAmount3);
                    var totalRefundedtax = Convert.ToDecimal(items.RefundedTaxAmount1) + Convert.ToDecimal(items.RefundedTaxAmount2) + Convert.ToDecimal(items.RefundedTaxAmount3);
                    order.OrderTax = order.OrderTax - (Convert.ToDecimal(items.RefundedTaxAmount1) + Convert.ToDecimal(items.RefundedTaxAmount2) + Convert.ToDecimal(items.RefundedTaxAmount3));
                    order.OrderTotalOnRefund = order.OrderTotalOnRefund - totalRefundedAmount + totalRefundedtax;

                    _orderService.UpdateOrder(order);
                }
                string connString = new DataSettingsManager().LoadSettings().DataConnectionString;
                string query = "InsertCovidRefundRecord";
                using (var db = new SqlConnection(connString))
                {
                    db.Open();
                    var sqlCommand = db.CreateCommand();
                    sqlCommand.CommandType = CommandType.StoredProcedure;
                    sqlCommand.CommandText = query;
                    sqlCommand.Parameters.AddWithValue("@ProductId", refundData.ProductId);
                    sqlCommand.Parameters.AddWithValue("@OrderId", refundData.OrderId);
                    sqlCommand.Parameters.AddWithValue("@OrderItemId", refundData.OrderItemId);
                    sqlCommand.Parameters.AddWithValue("@GlCodeName1", refundData.GLCodeName1);
                    sqlCommand.Parameters.AddWithValue("@GLAmount1", refundData.GLAmount1);
                    sqlCommand.Parameters.AddWithValue("@GLCodeName2", refundData.GLCodeName2);
                    sqlCommand.Parameters.AddWithValue("@GLAmount2", refundData.GLAmount2);
                    sqlCommand.Parameters.AddWithValue("@GLCodeName3", refundData.GLCodeName3);
                    sqlCommand.Parameters.AddWithValue("@GLAmount3", refundData.GLAmount3);
                    sqlCommand.Parameters.AddWithValue("@TaxAmount1", refundData.TaxAmount1);
                    sqlCommand.Parameters.AddWithValue("@TaxName1", refundData.TaxName1);
                    sqlCommand.Parameters.AddWithValue("@TaxAmount2", refundData.TaxAmount2);
                    sqlCommand.Parameters.AddWithValue("@TaxName2", refundData.TaxName2);
                    sqlCommand.Parameters.AddWithValue("@TaxAmount3", refundData.TaxAmount3);
                    sqlCommand.Parameters.AddWithValue("@TaxName3", refundData.TaxName3);
                    sqlCommand.Parameters.AddWithValue("@TotalRefund", refundData.TotalRefund);
                    sqlCommand.Parameters.AddWithValue("@RefundedTaxAmount1", refundData.RefundedTaxAmount1);
                    sqlCommand.Parameters.AddWithValue("@RefundedTaxAmount2", refundData.RefundedTaxAmount2);
                    sqlCommand.Parameters.AddWithValue("@RefundedTaxAmount3", refundData.RefundedTaxAmount3);
                    sqlCommand.Parameters.AddWithValue("@DeliveryTaxName", refundData.DeliveryTaxName);
                    sqlCommand.Parameters.AddWithValue("@DeliveryGLCodeName", refundData.DeliveryGLCodeName);
                    sqlCommand.Parameters.AddWithValue("@DeliveryTax", refundData.DeliveryTax);
                    sqlCommand.Parameters.AddWithValue("@DeliveryPickupAmount", refundData.DeliveryPickupAmount);
                    sqlCommand.Parameters.AddWithValue("@ShippingAmount", refundData.ShippingAmount);
                    sqlCommand.Parameters.AddWithValue("@ShippingTax", refundData.ShippingTax);
                    sqlCommand.Parameters.AddWithValue("@ShippingTaxName", refundData.ShippingTaxName);
                    sqlCommand.Parameters.AddWithValue("@Error", refundData.Error);
                    sqlCommand.Parameters.AddWithValue("@Success", true);
                    sqlCommand.Parameters.AddWithValue("@CreatedDateUtc", DateTime.UtcNow);
                    sqlCommand.Parameters.AddWithValue("@OverridenGlcode1", refundData.Glcodeid1);
                    sqlCommand.Parameters.AddWithValue("@OverridenGlcode2", refundData.Glcodeid2);
                    sqlCommand.Parameters.AddWithValue("@OverridenGlcode3", refundData.Glcodeid3);
                    var affectedrows = sqlCommand.ExecuteNonQuery();
                    if (affectedrows == 1)
                    {
                        refundData.Success = "Partially Refunded successfully";
                        return Json(refundData);
                    }
                    db.Close();
                }

            }
            catch (Exception ex)
            {
                refundData.Error = ex.Message.ToString();
                return Json(refundData);
            }

            refundData.Success = "Partially Refunded successfully";
            return null;
        }

        [HttpPost]
        public ActionResult UpdatePartiallyRefundData(PartiallyRefundGridModel refundData)
        {
            if (String.IsNullOrEmpty(Request.Headers["consoleKey"]))
            {
                if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                    return AccessDeniedView();
            }
            else
            {
                if (Request.Headers["consoleKey"] != "c0ns0l3p4ss")
                {
                    return AccessDeniedView();
                }
            }

            var order = _orderService.GetOrderById(refundData.OrderId);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null)
                return RedirectToAction("Edit", "Order", new { id = refundData.OrderId });

            var productDetails = _productService.GetProductById(refundData.ProductId);
            var actualProductPrice = productDetails.Price;
            var listofProd = new List<int>();
            List<DynamicVertexGLCode> vertexOrderTaxGls = new List<DynamicVertexGLCode>();

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


            try
            {
                if (productDetails.TaxCategoryId != -1)
                {

                    if (refundData.GLAmount1 <= decimal.Zero)
                        throw new NopException("Enter amount to refund");

                    //if (refundData.GLAmount1 > order.OrderTotal || refundData.GLAmount1 > actualProductPrice)
                    //    refundData.GLAmount1 = actualProductPrice;

                    var items = order.OrderItems.Where(x => x.ProductId == refundData.ProductId && x.Id == refundData.OrderItemId).FirstOrDefault();
                    if (items.IsPartialRefund)
                        throw new Exception("The item is already Refunded");
                    if (items.FulfillmentDateTime == null)
                        throw new Exception("Unfulfilled items may not be partially refunded");
                    if (refundData.GLAmount1 > items.PartialRefundOrderAmount)
                        throw new Exception("amount cannot be greater than the Products actual cost");

                    _taxService.PostDistributiveTaxRefundData(order, refundData.OrderItemId, true, refundData.GLAmount1, refundData.GLAmount2, refundData.GLAmount3, refundData.ProductId);
                    if (listofProd.Contains(refundData.ProductId))
                    {
                        vertexOrderTaxGls = _glCodeService.GetVertexGlBreakdown(refundData.OrderId, refundData.ProductId, TaxRequestType.ResponseDistributiveTaxRefund, refundData.OrderItemId);

                    }
                    else
                    {
                        vertexOrderTaxGls = _glCodeService.GetVertexGlBreakdown(refundData.OrderId, refundData.ProductId, TaxRequestType.ResponseDistributiveTaxRefund);

                    }
                    decimal calculatedTax = 0;
                    foreach (var vertexdata in vertexOrderTaxGls)
                    {
                        calculatedTax = calculatedTax + vertexdata.Total;
                    }
                    decimal amount = refundData.GLAmount1 - (calculatedTax);


                    var errors = _orderProcessingService.PartiallyRefund(order, Convert.ToDecimal(amount), items.ProductId).ToList();

                    items.PartialRefundOrderAmount = Convert.ToDecimal(items.PartialRefundOrderAmount - refundData.GLAmount1);

                    if (vertexOrderTaxGls.Any())
                    {
                        if (vertexOrderTaxGls.Count == 1)
                        {
                            items.TaxAmount1 = refundData.TaxAmount1 + vertexOrderTaxGls[0].Total;
                            items.RefundedTaxAmount1 = Convert.ToDecimal(items.RefundedTaxAmount1) + vertexOrderTaxGls[0].Total;
                        }
                        if (vertexOrderTaxGls.Count == 2)
                        {
                            items.TaxAmount1 = refundData.TaxAmount1 + vertexOrderTaxGls[0].Total;
                            items.TaxAmount2 = refundData.TaxAmount2 + vertexOrderTaxGls[1].Total;

                            items.RefundedTaxAmount1 = Convert.ToDecimal(items.RefundedTaxAmount1) + vertexOrderTaxGls[0].Total;
                            items.RefundedTaxAmount2 = Convert.ToDecimal(items.RefundedTaxAmount2) + vertexOrderTaxGls[1].Total;
                        }
                        if (vertexOrderTaxGls.Count == 3)
                        {
                            items.TaxAmount1 = refundData.TaxAmount1 + vertexOrderTaxGls[0].Total;
                            items.TaxAmount2 = refundData.TaxAmount2 + vertexOrderTaxGls[1].Total;
                            items.TaxAmount3 = refundData.TaxAmount3 + vertexOrderTaxGls[2].Total;


                            items.RefundedTaxAmount1 = Convert.ToDecimal(items.RefundedTaxAmount1) + vertexOrderTaxGls[0].Total;
                            items.RefundedTaxAmount2 = Convert.ToDecimal(items.RefundedTaxAmount2) + vertexOrderTaxGls[1].Total;
                            items.RefundedTaxAmount3 = Convert.ToDecimal(items.RefundedTaxAmount3) + vertexOrderTaxGls[2].Total;
                        }
                    }

                    items.RefundedGlAmount1 = Convert.ToDecimal(items.RefundedGlAmount1) + refundData.GLAmount1;
                    items.GlcodeAmount1 = refundData.GLAmount1;
                    items.GlcodeAmount2 = refundData.GLAmount2;
                    items.GlcodeAmount3 = refundData.GLAmount3;

                    //items.RefundedTaxAmount1 = Convert.ToDecimal(items.RefundedTaxAmount1) + calculatedTax;

                    items.TotalRefundedAmount = refundData.GLAmount1 + Convert.ToDecimal(items.TotalRefundedAmount) - (calculatedTax);
                    items.GLCodeName1 = refundData.GLCodeName1;
                    items.GLCodeName2 = refundData.GLCodeName2;
                    items.GLCodeName3 = refundData.GLCodeName3;
                    items.TaxName1 = refundData.TaxName1;
                    items.TaxName2 = refundData.TaxName2;
                    items.TaxName3 = refundData.TaxName3;
                    items.IsPartialRefund = true;
                    items.DateOfRefund = DateTime.Now;
                    _orderItemRepository.Update(items);

                    order.OrderTax = order.OrderTax + Convert.ToDecimal(items.RefundedTaxAmount1) + Convert.ToDecimal(items.RefundedTaxAmount2) + Convert.ToDecimal(items.RefundedTaxAmount3);
                    order.OrderTotalOnRefund = order.OrderTotalOnRefund - Convert.ToDecimal(items.RefundedGlAmount1) + Convert.ToDecimal(items.RefundedTaxAmount1) + Convert.ToDecimal(items.RefundedTaxAmount2) + Convert.ToDecimal(items.RefundedTaxAmount3);

                    _orderService.UpdateOrder(order);

                    _orderProcessingService.InsertGlCodeCalcs(order, items, true, true);
                    _orderProcessingService.InsertIntoSAPSalesJournalOrderItem(order, items, 4);
                }
                else
                {
                    decimal amount = 0;
                    var items = order.OrderItems.Where(x => x.ProductId == refundData.ProductId && x.Id == refundData.OrderItemId).FirstOrDefault();
                    if (items.FulfillmentDateTime == null)
                        throw new Exception("Unfulfilled items may not be partially refunded");
                    if (items.IsPartialRefund)
                        throw new Exception("The item is already  Refunded");
                    List<ProductGlCode> paidCodes = _glCodeService.GetProductGlCodes(items.Product, GLCodeStatusType.Paid).ToList();
                    List<int> data = new List<int>();
                    if (!items.IsPartialRefund)
                    {
                        if (paidCodes.Count == 1)
                        {
                            if (paidCodes[0].Amount * items.Quantity != refundData.GLAmount1)
                                data.Add(1);
                        }
                        if (paidCodes.Count == 2)
                        {
                            if (paidCodes[0].Amount * items.Quantity != refundData.GLAmount1)
                                data.Add(1);
                            if (paidCodes[1].Amount * items.Quantity != refundData.GLAmount2)
                                data.Add(2);
                        }
                        if (paidCodes.Count == 3)
                        {
                            if (paidCodes[0].Amount * items.Quantity != refundData.GLAmount1)
                                data.Add(1);
                            if (paidCodes[1].Amount * items.Quantity != refundData.GLAmount2)
                                data.Add(2);
                            if (paidCodes[2].Amount * items.Quantity != refundData.GLAmount3)
                                data.Add(3);
                        }
                    }
                    else
                    {
                        if (items.GlcodeAmount1 != refundData.GLAmount1)
                            data.Add(1);
                        if (items.GlcodeAmount2 != refundData.GLAmount2)
                            data.Add(2);
                        if (items.GlcodeAmount3 != refundData.GLAmount3)
                            data.Add(3);
                    }

                    _taxService.PostDistributiveTaxRefundData(order, refundData.OrderItemId, true, refundData.GLAmount1, refundData.GLAmount2, refundData.GLAmount3, refundData.ProductId, data);
                    if (listofProd.Contains(refundData.ProductId))
                    {
                        vertexOrderTaxGls = _glCodeService.GetVertexGlBreakdown(refundData.OrderId, refundData.ProductId, TaxRequestType.ResponseDistributiveTaxRefund, refundData.OrderItemId);

                    }
                    else
                    {
                        vertexOrderTaxGls = _glCodeService.GetVertexGlBreakdown(refundData.OrderId, refundData.ProductId, TaxRequestType.ResponseDistributiveTaxRefund);

                    }
                    //vertexOrderTaxGls = _glCodeService.GetVertexGlBreakdown(refundData.OrderId, refundData.ProductId, TaxRequestType.ResponseDistributiveTaxRefund);
                    var refundedTaxtoDisplay = vertexOrderTaxGls.Where(x => x.Total <= 0).ToList();

                    if (!items.IsPartialRefund)
                    {

                        if (paidCodes.Count == 1)
                        {
                            if (paidCodes[0].Amount * items.Quantity != refundData.GLAmount1)
                                data.Add(1);
                            if ((refundData.GLAmount1) > (paidCodes[0].Amount * items.Quantity))
                                throw new Exception("amount cannot be greater than displayed GlAmount");
                        }
                        if (paidCodes.Count == 2)
                        {
                            if (paidCodes[0].Amount * items.Quantity != refundData.GLAmount1)
                                data.Add(1);
                            if (paidCodes[1].Amount * items.Quantity != refundData.GLAmount2)
                                data.Add(2);
                            if ((refundData.GLAmount1 + refundData.GLAmount2) > (paidCodes[0].Amount * items.Quantity + paidCodes[1].Amount * items.Quantity))
                                throw new Exception("amount cannot be greater than displayed GlAmount");
                        }
                        if (paidCodes.Count == 3)
                        {
                            if (paidCodes[0].Amount * items.Quantity != refundData.GLAmount1)
                                data.Add(1);
                            if (paidCodes[1].Amount * items.Quantity != refundData.GLAmount2)
                                data.Add(2);
                            if (paidCodes[2].Amount * items.Quantity != refundData.GLAmount3)
                                data.Add(3);

                            if ((refundData.GLAmount1 + refundData.GLAmount2 + refundData.GLAmount3) > (paidCodes[0].Amount * items.Quantity + paidCodes[1].Amount * items.Quantity + paidCodes[2].Amount * items.Quantity))
                                throw new Exception("amount cannot be greater than displayed GlAmount");
                        }

                        //if (paidCodes[0].Amount != refundData.GLAmount1)
                        //    data.Add(1);
                        //if (paidCodes[1].Amount != refundData.GLAmount2)
                        //    data.Add(2);
                        //if (paidCodes[2].Amount != refundData.GLAmount3)
                        //    data.Add(3);
                    }
                    else
                    {
                        if ((refundData.GLAmount1 + refundData.GLAmount2 + refundData.GLAmount3) > (items.GlcodeAmount1 + items.GlcodeAmount2 + items.GlcodeAmount3))
                            throw new Exception("amount cannot be greater than displayed GlAmount");

                        if (items.GlcodeAmount1 != refundData.GLAmount1)
                            data.Add(1);
                        if (items.GlcodeAmount2 != refundData.GLAmount2)
                            data.Add(2);
                        if (items.GlcodeAmount3 != refundData.GLAmount3)
                            data.Add(3);
                    }

                    foreach (var refundedtax in refundedTaxtoDisplay)
                    {
                        if (data.Contains(1))
                        {
                            items.RefundedGlAmount1 = Convert.ToDecimal(items.RefundedGlAmount1) + refundData.GLAmount1; // Amount refunded for GlcodeAmount1 from UI
                            amount = amount + (refundData.GLAmount1 - (refundedTaxtoDisplay[0].Total)); //Summation of tax with the Refunded Glcode1Amount1 from UI
                            //refundData.GLAmount1 = amount; //the Amount we are going to Refund Setting this Up for a common Refund Goal
                            items.RefundedTaxAmount1 = Convert.ToDecimal(items.RefundedTaxAmount1) + refundedtax.Total; // The Tax amount refunded for GLAmount1. Shows the current Refunded Tax amount
                            items.TaxAmount1 = refundData.TaxAmount1 + (refundedtax.Total);// Summation of Previous refunded tax amount with the Current refunded tax Amount
                            data.Remove(1);
                            continue;
                        }
                        if (data.Contains(2))
                        {
                            items.RefundedGlAmount2 = Convert.ToDecimal(items.RefundedGlAmount2) + refundData.GLAmount2;
                            amount = amount + (refundData.GLAmount2 - (refundedtax.Total));
                            //refundData.GLAmount1 = amount;

                            items.RefundedTaxAmount2 = Convert.ToDecimal(items.RefundedTaxAmount2) + refundedtax.Total;
                            items.TaxAmount2 = refundData.TaxAmount2 + (refundedtax.Total);
                            data.Remove(2);
                            continue;
                        }
                        if (data.Contains(3))
                        {
                            items.RefundedGlAmount3 = Convert.ToDecimal(items.RefundedGlAmount3) + refundData.GLAmount3;
                            amount = amount + (refundData.GLAmount3 - (refundedtax.Total));
                            //refundData.GLAmount1 = amount;

                            items.RefundedTaxAmount3 = Convert.ToDecimal(items.RefundedTaxAmount3) + refundedtax.Total;
                            items.TaxAmount3 = refundData.TaxAmount3 + (refundedtax.Total);
                            data.Remove(3);
                            continue;
                        }
                    }
                    if (items.TaxAmount1 == null)
                        items.TaxAmount1 = refundData.TaxAmount1;
                    if (items.TaxAmount2 == null)
                        items.TaxAmount2 = refundData.TaxAmount2;
                    if (items.TaxAmount3 == null)
                        items.TaxAmount3 = refundData.TaxAmount3;

                    var errors = _orderProcessingService.PartiallyRefund(order, Convert.ToDecimal(amount), items.ProductId).ToList();


                    //items.TaxAmount1 = refundData.TaxAmount1 + (vertexOrderTaxGls[0].Total);
                    //items.TaxAmount2 = refundData.TaxAmount2;
                    //items.TaxAmount3 = refundData.TaxAmount3;
                    items.TotalRefundedAmount = Convert.ToDecimal(items.TotalRefundedAmount) + amount;
                    if (!items.IsPartialRefund)
                    {
                        if (paidCodes.Count == 1)
                        {
                            items.GlcodeAmount1 = paidCodes[0].Amount * items.Quantity - Convert.ToDecimal(items.RefundedGlAmount1);
                            items.GlcodeAmount2 = 0;
                            items.GlcodeAmount3 = 0;
                        }
                        if (paidCodes.Count == 2)
                        {
                            items.GlcodeAmount1 = paidCodes[0].Amount * items.Quantity - Convert.ToDecimal(items.RefundedGlAmount1);
                            items.GlcodeAmount2 = paidCodes[1].Amount * items.Quantity - Convert.ToDecimal(items.RefundedGlAmount2);
                            items.GlcodeAmount3 = 0;
                        }
                        if (paidCodes.Count == 3)
                        {
                            items.GlcodeAmount1 = paidCodes[0].Amount * items.Quantity - Convert.ToDecimal(items.RefundedGlAmount1);
                            items.GlcodeAmount2 = paidCodes[1].Amount * items.Quantity - Convert.ToDecimal(items.RefundedGlAmount2);
                            items.GlcodeAmount3 = paidCodes[2].Amount * items.Quantity - Convert.ToDecimal(items.RefundedGlAmount3);

                        }
                    }
                    else
                    {
                        if (paidCodes.Count == 1)
                        {
                            items.GlcodeAmount1 = (items.GlcodeAmount1 != refundData.GLAmount1) ? Convert.ToDecimal(items.GlcodeAmount1) - refundData.GLAmount1 : items.GlcodeAmount1;
                            items.GlcodeAmount2 = 0;
                            items.GlcodeAmount3 = 0;
                        }
                        if (paidCodes.Count == 2)
                        {
                            items.GlcodeAmount1 = (items.GlcodeAmount1 != refundData.GLAmount1) ? Convert.ToDecimal(items.GlcodeAmount1) - refundData.GLAmount1 : items.GlcodeAmount1;
                            items.GlcodeAmount2 = (items.GlcodeAmount2 != refundData.GLAmount2) ? Convert.ToDecimal(items.GlcodeAmount2) - refundData.GLAmount2 : items.GlcodeAmount2;
                            items.GlcodeAmount3 = 0;
                        }
                        if (paidCodes.Count == 3)
                        {
                            items.GlcodeAmount1 = (items.GlcodeAmount1 != refundData.GLAmount1) ? Convert.ToDecimal(items.GlcodeAmount1) - refundData.GLAmount1 : items.GlcodeAmount1;
                            items.GlcodeAmount2 = (items.GlcodeAmount2 != refundData.GLAmount2) ? Convert.ToDecimal(items.GlcodeAmount2) - refundData.GLAmount2 : items.GlcodeAmount2;
                            items.GlcodeAmount3 = (items.GlcodeAmount3 != refundData.GLAmount3) ? Convert.ToDecimal(items.GlcodeAmount3) - refundData.GLAmount3 : items.GlcodeAmount3;

                        }
                    }

                    items.GLCodeName1 = refundData.GLCodeName1;
                    items.GLCodeName2 = refundData.GLCodeName2;
                    items.GLCodeName3 = refundData.GLCodeName3;

                    items.TaxName1 = refundData.TaxName1;
                    items.TaxName2 = refundData.TaxName2;
                    items.TaxName3 = refundData.TaxName3;
                    //if (vertexOrderTaxGls.Count == 1)
                    //{
                    //    items.TaxName1 = vertexOrderTaxGls[0].GlCode + "-" + vertexOrderTaxGls[0].Description;
                    //}
                    //if (vertexOrderTaxGls.Count == 2)
                    //{
                    //    items.TaxName1 = vertexOrderTaxGls[0].GlCode + "-" + vertexOrderTaxGls[0].Description;
                    //    items.TaxName2 = vertexOrderTaxGls[1].GlCode + "-" + vertexOrderTaxGls[1].Description;
                    //}
                    //if (vertexOrderTaxGls.Count == 3)
                    //{
                    //    items.TaxName1 = vertexOrderTaxGls[0].GlCode + "-" + vertexOrderTaxGls[0].Description;
                    //    items.TaxName2 = vertexOrderTaxGls[1].GlCode + "-" + vertexOrderTaxGls[1].Description;
                    //    items.TaxName3 = vertexOrderTaxGls[2].GlCode + "-" + vertexOrderTaxGls[2].Description;
                    //}

                    //items.TaxName1 =vertexOrderTaxGls refundData.TaxName1;
                    //items.TaxName2 = refundData.TaxName2;
                    //items.TaxName3 = refundData.TaxName3;

                    items.IsPartialRefund = true;
                    items.DateOfRefund = DateTime.Now;
                    _orderItemRepository.Update(items);

                    var totalRefundedAmount = Convert.ToDecimal(items.RefundedGlAmount1) + Convert.ToDecimal(items.RefundedGlAmount2) + Convert.ToDecimal(items.RefundedGlAmount3);
                    var totalRefundedtax = Convert.ToDecimal(items.RefundedTaxAmount1) + Convert.ToDecimal(items.RefundedTaxAmount2) + Convert.ToDecimal(items.RefundedTaxAmount3);
                    order.OrderTax = order.OrderTax + Convert.ToDecimal(items.RefundedTaxAmount1) + Convert.ToDecimal(items.RefundedTaxAmount2) + Convert.ToDecimal(items.RefundedTaxAmount3);
                    order.OrderTotalOnRefund = order.OrderTotalOnRefund - totalRefundedAmount + totalRefundedtax;

                    _orderService.UpdateOrder(order);

                    _orderProcessingService.InsertIntoSAPSalesJournalOrderItem(order, items, 4);
                }
            }
            catch (Exception ex)
            {
                refundData.Error = ex.Message.ToString();
                return Json(refundData);
            }

            refundData.Success = "Partially Refunded successfully";
            return Json(refundData);
        }


        [NonAction]
        public virtual List<int> GetOveriddenGls(int orderid, int productid, int orderitemd)
        {
            var connStr = new DataSettingsManager().LoadSettings().DataConnectionString;
            List<int> OverriddenGlcode = new List<int>();
            using (var conn = new SqlConnection(connStr))
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandType = CommandType.Text;
                //Section to fetch records that are f
                var sqlSelect = string.Format("Select OverriddenGlcode1,OverriddenGlcode2,OverriddenGlcode3 from covidRefunds where orderid={0} and productid={1} and orderitemid={2}", orderid, productid, orderitemd);
                cmd.CommandText = sqlSelect;
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        OverriddenGlcode.Add(Convert.ToInt32(reader["OverriddenGlcode1"]));
                        OverriddenGlcode.Add(Convert.ToInt32(reader["OverriddenGlcode2"]));
                        OverriddenGlcode.Add(Convert.ToInt32(reader["OverriddenGlcode3"]));

                    }
                }
            }
            return OverriddenGlcode;
        }

        [HttpPost]
        public ActionResult UpdateFullRefundData(PartiallyRefundGridModel refundData)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var order = _orderService.GetOrderById(refundData.OrderId);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null)
                return RedirectToAction("Edit", "Order", new { id = refundData.OrderId });

            var productDetails = _productService.GetProductById(refundData.ProductId);
            var actualProductPrice = productDetails.Price;
            var listofProd = new List<int>();
            List<DynamicVertexGLCode> vertexOrderTaxGls = new List<DynamicVertexGLCode>();

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
            try
            {
                if (productDetails.TaxCategoryId != -1)
                {
                    if (refundData.GLAmount1 <= decimal.Zero)
                        throw new NopException("Product has already been fully refunded.");

                    var items = order.OrderItems.Where(x => x.ProductId == refundData.ProductId && x.Id == refundData.OrderItemId).FirstOrDefault();

                    if (items.IsPartialRefund)
                        throw new Exception("The item is already  Refunded");

                    if (refundData.GLAmount1 > order.OrderTotal || refundData.GLAmount1 > items.PartialRefundOrderAmount)
                        refundData.GLAmount1 = actualProductPrice;

                    var totalTaxToRefund = refundData.TaxAmount1 + refundData.TaxAmount2 + refundData.TaxAmount3;
                    _taxService.PostDistributiveFullTaxRefundDataPerOrderItem(order, refundData.OrderItemId, true, refundData.GLAmount1, refundData.GLAmount2, refundData.GLAmount3, refundData.ProductId,
                        refundData.TaxAmount1, refundData.TaxAmount2, refundData.TaxAmount3, refundData.DeliveryPickupAmount, refundData.DeliveryTax, refundData.ShippingAmount, refundData.ShippingTax);
                    if (listofProd.Contains(refundData.ProductId))
                    {
                        vertexOrderTaxGls = _glCodeService.GetVertexGlBreakdown(refundData.OrderId, refundData.ProductId, TaxRequestType.ResponseDistributiveTaxRefund, refundData.OrderItemId);
                    }
                    else
                    {
                        vertexOrderTaxGls = _glCodeService.GetVertexGlBreakdown(refundData.OrderId, refundData.ProductId, TaxRequestType.ResponseDistributiveTaxRefund);
                    }
                    var VertexOrderTaxGlsForDelvandShip = vertexOrderTaxGls.Where(x => x.taxCode == "DELV" || x.taxCode == "SHIP").ToList();
                    decimal calculatedTax = 0;

                    foreach (var taxCalulated in vertexOrderTaxGls)
                    {
                        calculatedTax = calculatedTax + taxCalulated.Total;
                    }

                    vertexOrderTaxGls = vertexOrderTaxGls.Where(x => x.taxCode != "DELV" && x.taxCode != "SHIP").ToList();

                    decimal amount = Convert.ToDecimal(refundData.GLAmount1) + Convert.ToDecimal(refundData.GLAmount2) + Convert.ToDecimal(refundData.GLAmount3) + Convert.ToDecimal(refundData.DeliveryPickupAmount) + Convert.ToDecimal(refundData.ShippingAmount) +
                        Convert.ToDecimal(refundData.TaxAmount1) + Convert.ToDecimal(refundData.TaxAmount2) + Convert.ToDecimal(refundData.TaxAmount3) + Convert.ToDecimal(refundData.DeliveryTax) + Convert.ToDecimal(refundData.ShippingTax);

                    var errors = _orderProcessingService.PartiallyRefund(order, Convert.ToDecimal(amount), items.ProductId).ToList();

                    items.PartialRefundOrderAmount = Convert.ToDecimal(items.PartialRefundOrderAmount) - Convert.ToDecimal(refundData.GLAmount1);

                    if (vertexOrderTaxGls.Any())
                    {
                        if (vertexOrderTaxGls.Count == 1)
                        {
                            items.TaxAmount1 = refundData.TaxAmount1 + vertexOrderTaxGls[0].Total;
                            items.RefundedTaxAmount1 = Convert.ToDecimal(items.RefundedTaxAmount1) + vertexOrderTaxGls[0].Total;
                        }
                        if (vertexOrderTaxGls.Count == 2)
                        {
                            items.TaxAmount1 = refundData.TaxAmount1 + vertexOrderTaxGls[0].Total;
                            items.TaxAmount2 = refundData.TaxAmount2 + vertexOrderTaxGls[1].Total;

                            items.RefundedTaxAmount1 = Convert.ToDecimal(items.RefundedTaxAmount1) + vertexOrderTaxGls[0].Total;
                            items.RefundedTaxAmount2 = Convert.ToDecimal(items.RefundedTaxAmount2) + vertexOrderTaxGls[1].Total;
                        }
                        if (vertexOrderTaxGls.Count == 3)
                        {
                            items.TaxAmount1 = refundData.TaxAmount1 + vertexOrderTaxGls[0].Total;
                            items.TaxAmount2 = refundData.TaxAmount2 + vertexOrderTaxGls[1].Total;
                            items.TaxAmount3 = refundData.TaxAmount3 + vertexOrderTaxGls[2].Total;


                            items.RefundedTaxAmount1 = Convert.ToDecimal(items.RefundedTaxAmount1) + vertexOrderTaxGls[0].Total;
                            items.RefundedTaxAmount2 = Convert.ToDecimal(items.RefundedTaxAmount2) + vertexOrderTaxGls[1].Total;
                            items.RefundedTaxAmount3 = Convert.ToDecimal(items.RefundedTaxAmount3) + vertexOrderTaxGls[2].Total;
                        }
                    }



                    //items.TaxAmount1 = refundData.TaxAmount1 + (calculatedTax);
                    //items.TaxAmount2 = refundData.TaxAmount2;
                    items.RefundedGlAmount1 = Convert.ToDecimal(items.RefundedGlAmount1) + refundData.GLAmount1;
                    //items.RefundedTaxAmount1 = calculatedTax;
                    // items.TaxAmount3 = refundData.TaxAmount3;
                    items.GlcodeAmount1 = items.PartialRefundOrderAmount;
                    items.GlcodeAmount2 = refundData.GLAmount2;
                    items.GlcodeAmount3 = refundData.GLAmount3;
                    items.TotalRefundedAmount = Convert.ToDecimal(items.TotalRefundedAmount) + amount;
                    items.GLCodeName1 = refundData.GLCodeName1;
                    items.GLCodeName2 = refundData.GLCodeName2;
                    items.GLCodeName3 = refundData.GLCodeName3;
                    items.TaxName1 = refundData.TaxName1;
                    items.TaxName2 = refundData.TaxName2;
                    items.TaxName3 = refundData.TaxName3;
                    items.IsPartialRefund = true;
                    items.IsFullRefund = true; //To identify if an item has been fully refunded
                    items.DeliveryPickupAmount = refundData.DeliveryPickupAmount;
                    items.ShippingAmount = refundData.ShippingAmount;
                    items.DateOfRefund = DateTime.Now;

                    foreach (var shipandDelvtax in VertexOrderTaxGlsForDelvandShip)
                    {
                        if (shipandDelvtax.taxCode == "DELV")
                            items.DelliveryTax = shipandDelvtax.Total;
                        if (shipandDelvtax.taxCode == "SHIP")
                            items.ShippingTax = shipandDelvtax.Total;
                    }

                    _orderItemRepository.Update(items);

                    var GlAmount = Convert.ToDecimal(items.RefundedGlAmount1) + Convert.ToDecimal(items.DeliveryPickupAmount) + Convert.ToDecimal(items.ShippingAmount);
                    var taxAmount = Convert.ToDecimal(items.RefundedTaxAmount1) + Convert.ToDecimal(items.RefundedTaxAmount2) + Convert.ToDecimal(items.RefundedTaxAmount3) + Convert.ToDecimal(items.DelliveryTax) + Convert.ToDecimal(items.ShippingTax);
                    order.OrderTax = order.OrderTax + Convert.ToDecimal(items.RefundedTaxAmount1) + Convert.ToDecimal(items.RefundedTaxAmount2) + Convert.ToDecimal(items.RefundedTaxAmount3) + Convert.ToDecimal(items.DelliveryTax) + Convert.ToDecimal(items.ShippingTax);
                    order.OrderTotalOnRefund = order.OrderTotalOnRefund - (GlAmount - taxAmount);


                    _orderService.UpdateOrder(order);

                    _orderProcessingService.InsertIntoSAPSalesJournalOrderItem(order, items, 4);

                }
                else
                {

                    var items = order.OrderItems.Where(x => x.ProductId == refundData.ProductId && x.Id == refundData.OrderItemId).FirstOrDefault();

                    if (items.IsPartialRefund)
                        throw new Exception("The item is already Refunded");

                    List<ProductGlCode> paidCodes = _glCodeService.GetProductGlCodes(items.Product, GLCodeStatusType.Paid).ToList();
                    _taxService.PostDistributiveFullTaxRefundDataPerOrderItem(order, refundData.OrderItemId, true, refundData.GLAmount1, refundData.GLAmount2, refundData.GLAmount3, refundData.ProductId, refundData.TaxAmount1, refundData.TaxAmount2, refundData.TaxAmount3);

                    // vertexOrderTaxGls = _glCodeService.GetVertexGlBreakdown(refundData.OrderId, refundData.ProductId, TaxRequestType.ResponseDistributiveTaxRefund);
                    if (listofProd.Contains(refundData.ProductId))
                    {
                        vertexOrderTaxGls = _glCodeService.GetVertexGlBreakdown(refundData.OrderId, refundData.ProductId, TaxRequestType.ResponseDistributiveTaxRefund, refundData.OrderItemId);
                    }
                    else
                    {
                        vertexOrderTaxGls = _glCodeService.GetVertexGlBreakdown(refundData.OrderId, refundData.ProductId, TaxRequestType.ResponseDistributiveTaxRefund);
                    }
                    var refundedTaxtoDisplay = vertexOrderTaxGls.Where(x => x.Total <= 0).ToList();

                    decimal amount = Convert.ToDecimal(refundData.GLAmount1) + Convert.ToDecimal(refundData.GLAmount2) + Convert.ToDecimal(refundData.GLAmount3) +
                     Convert.ToDecimal(refundData.TaxAmount1) + Convert.ToDecimal(refundData.TaxAmount2) + Convert.ToDecimal(refundData.TaxAmount3);

                    var errors = _orderProcessingService.PartiallyRefund(order, Convert.ToDecimal(amount), items.ProductId).ToList();


                    if (vertexOrderTaxGls.Any())
                    {
                        if (vertexOrderTaxGls.Count == 1)
                        {
                            items.TaxAmount1 = refundData.TaxAmount1 + vertexOrderTaxGls[0].Total;
                            items.RefundedTaxAmount1 = Convert.ToDecimal(items.RefundedTaxAmount1) + vertexOrderTaxGls[0].Total;
                        }
                        if (vertexOrderTaxGls.Count == 2)
                        {
                            items.TaxAmount1 = refundData.TaxAmount1 + vertexOrderTaxGls[0].Total;
                            items.TaxAmount2 = refundData.TaxAmount2 + vertexOrderTaxGls[1].Total;

                            items.RefundedTaxAmount1 = Convert.ToDecimal(items.RefundedTaxAmount1) + vertexOrderTaxGls[0].Total;
                            items.RefundedTaxAmount2 = Convert.ToDecimal(items.RefundedTaxAmount2) + vertexOrderTaxGls[1].Total;
                        }
                        if (vertexOrderTaxGls.Count == 3)
                        {
                            items.TaxAmount1 = refundData.TaxAmount1 + vertexOrderTaxGls[0].Total;
                            items.TaxAmount2 = refundData.TaxAmount2 + vertexOrderTaxGls[1].Total;
                            items.TaxAmount3 = refundData.TaxAmount3 + vertexOrderTaxGls[2].Total;


                            items.RefundedTaxAmount1 = Convert.ToDecimal(items.RefundedTaxAmount1) + vertexOrderTaxGls[0].Total;
                            items.RefundedTaxAmount2 = Convert.ToDecimal(items.RefundedTaxAmount2) + vertexOrderTaxGls[1].Total;
                            items.RefundedTaxAmount3 = Convert.ToDecimal(items.RefundedTaxAmount3) + vertexOrderTaxGls[2].Total;
                        }
                    }

                    //items.TaxAmount1 = refundData.TaxAmount1 + (refundedTaxtoDisplay[0].Total);
                    //items.TaxAmount2 = refundData.TaxAmount2 + (refundedTaxtoDisplay[1].Total);
                    //items.TaxAmount3 = refundData.TaxAmount3 + (refundedTaxtoDisplay[2].Total);
                    items.RefundedGlAmount1 = Convert.ToDecimal(items.RefundedGlAmount1) + refundData.GLAmount1;
                    //items.RefundedTaxAmount1 = refundedTaxtoDisplay[0].Total;
                    items.RefundedGlAmount2 = Convert.ToDecimal(items.RefundedGlAmount2) + refundData.GLAmount2;
                    //items.RefundedTaxAmount2 = refundedTaxtoDisplay[1].Total;
                    items.RefundedGlAmount3 = Convert.ToDecimal(items.RefundedGlAmount3) + refundData.GLAmount3;
                    //items.RefundedTaxAmount3 = refundedTaxtoDisplay[2].Total;

                    if (paidCodes.Any())
                    {
                        if (paidCodes.Count == 1)
                        {
                            items.GlcodeAmount1 = items.GlcodeAmount1 == null ? (paidCodes[0].Amount * items.Quantity - refundData.GLAmount1) : (items.GlcodeAmount1 - refundData.GLAmount1);
                            items.GlcodeAmount2 = 0;
                            items.GlcodeAmount3 = 0;
                        }
                        if (paidCodes.Count == 2)
                        {
                            items.GlcodeAmount1 = items.GlcodeAmount1 == null ? (paidCodes[0].Amount * items.Quantity - refundData.GLAmount1) : (items.GlcodeAmount1 - refundData.GLAmount1);
                            items.GlcodeAmount2 = items.GlcodeAmount2 == null ? (paidCodes[1].Amount * items.Quantity - refundData.GLAmount2) : (items.GlcodeAmount2 - refundData.GLAmount2);
                            items.GlcodeAmount3 = 0;
                        }
                        if (paidCodes.Count == 3)
                        {
                            items.GlcodeAmount1 = items.GlcodeAmount1 == null ? (paidCodes[0].Amount * items.Quantity - refundData.GLAmount1) : (items.GlcodeAmount1 - refundData.GLAmount1);
                            items.GlcodeAmount2 = items.GlcodeAmount2 == null ? (paidCodes[1].Amount * items.Quantity - refundData.GLAmount2) : (items.GlcodeAmount2 - refundData.GLAmount2);
                            items.GlcodeAmount3 = items.GlcodeAmount3 == null ? (paidCodes[2].Amount * items.Quantity - refundData.GLAmount3) : (items.GlcodeAmount3 - refundData.GLAmount3);
                        }
                    }

                    items.TotalRefundedAmount = Convert.ToDecimal(items.TotalRefundedAmount) + amount;
                    items.GLCodeName1 = refundData.GLCodeName1;
                    items.GLCodeName2 = refundData.GLCodeName2;
                    items.GLCodeName3 = refundData.GLCodeName3;
                    items.TaxName1 = refundData.TaxName1;
                    items.TaxName2 = refundData.TaxName2;
                    items.TaxName3 = refundData.TaxName3;
                    items.IsPartialRefund = true;
                    items.IsFullRefund = true; //To identify if an item has been fully refunded
                    items.DateOfRefund = DateTime.Now;
                    _orderItemRepository.Update(items);

                    var totalRefundedAmount = Convert.ToDecimal(items.RefundedGlAmount1) + Convert.ToDecimal(items.RefundedGlAmount2) + Convert.ToDecimal(items.RefundedGlAmount3);
                    var totalRefundedtax = Convert.ToDecimal(items.RefundedTaxAmount1) + Convert.ToDecimal(items.RefundedTaxAmount2) + Convert.ToDecimal(items.RefundedTaxAmount3);
                    order.OrderTax = order.OrderTax + Convert.ToDecimal(items.RefundedTaxAmount1) + Convert.ToDecimal(items.RefundedTaxAmount2) + Convert.ToDecimal(items.RefundedTaxAmount3);
                    order.OrderTotalOnRefund = order.OrderTotalOnRefund - totalRefundedAmount + totalRefundedtax;

                    _orderService.UpdateOrder(order);

                    _orderProcessingService.InsertIntoSAPSalesJournalOrderItem(order, items, 4);
                }

            }
            catch (Exception ex)
            {
                refundData.Error = ex.Message.ToString();
                return Json(refundData);
            }
            return Json(refundData);
        }


        [HttpPost]

        public ActionResult UpdateCustomFullRefundData(PartiallyRefundGridModel refundData)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();
            List<DynamicVertexGLCode> vertexOrderTaxGls = new List<DynamicVertexGLCode>();

            var order = _orderService.GetOrderById(refundData.OrderId);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null)
                return RedirectToAction("Edit", "Order", new { id = refundData.OrderId });

            var productDetails = _productService.GetProductById(refundData.ProductId);
            var actualProductPrice = productDetails.Price;
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


            try
            {
                if (productDetails.TaxCategoryId != -1)
                {

                    if (refundData.GLAmount1 <= decimal.Zero)
                        throw new NopException("Enter amount to refund");

                    //if (refundData.GLAmount1 > order.OrderTotal || refundData.GLAmount1 > actualProductPrice)
                    //    refundData.GLAmount1 = actualProductPrice;

                    var items = order.OrderItems.Where(x => x.ProductId == refundData.ProductId && x.Id == refundData.OrderItemId).FirstOrDefault();
                    if (items.IsPartialRefund)
                        throw new Exception("The item is already Refunded");
                    if (refundData.GLAmount1 > items.PartialRefundOrderAmount)
                        throw new Exception("amount cannot be greater than the Products actual cost");

                    decimal amount = refundData.GLAmount1 + (refundData.TaxAmount1) + refundData.DeliveryPickupAmount + refundData.DeliveryTax + refundData.ShippingAmount + refundData.ShippingTax;


                    var errors = _orderProcessingService.PartiallyRefund(order, Convert.ToDecimal(amount), items.ProductId).ToList();
                    items.PartialRefundOrderAmount = Convert.ToDecimal(items.PartialRefundOrderAmount - refundData.GLAmount1);


                    items.TaxAmount1 = Convert.ToDecimal(items.TaxAmount1);
                    items.RefundedTaxAmount1 = Convert.ToDecimal(items.RefundedTaxAmount1) + refundData.TaxAmount1;
                    items.RefundedGlAmount1 = Convert.ToDecimal(items.RefundedGlAmount1) + refundData.GLAmount1;

                    items.GlcodeAmount1 = items.PartialRefundOrderAmount;
                    items.GlcodeAmount2 = refundData.GLAmount2;
                    items.GlcodeAmount3 = refundData.GLAmount3;


                    items.TotalRefundedAmount = amount;
                    items.GLCodeName1 = refundData.GLCodeName1;
                    items.GLCodeName2 = refundData.GLCodeName2;
                    items.GLCodeName3 = refundData.GLCodeName3;
                    items.TaxName1 = refundData.TaxName1;
                    items.TaxName2 = refundData.TaxName2;
                    items.TaxName3 = refundData.TaxName3;
                    items.IsPartialRefund = true;
                    items.DeliveryPickupAmount = refundData.DeliveryPickupAmount;
                    items.ShippingAmount = refundData.ShippingAmount;
                    items.DelliveryTax = Convert.ToDecimal(refundData.DeliveryTax);
                    items.ShippingTax = Convert.ToDecimal(refundData.ShippingTax);


                    _orderItemRepository.Update(items);

                    order.OrderTax = order.OrderTax - (Convert.ToDecimal(items.RefundedTaxAmount1) + Convert.ToDecimal(items.RefundedTaxAmount2) + Convert.ToDecimal(items.RefundedTaxAmount3));
                    order.OrderTotalOnRefund = order.OrderTotalOnRefund - (Convert.ToDecimal(items.RefundedGlAmount1) + Convert.ToDecimal(items.RefundedGlAmount2) + Convert.ToDecimal(items.RefundedGlAmount3) + Convert.ToDecimal(items.RefundedTaxAmount1) + Convert.ToDecimal(items.RefundedTaxAmount2) + Convert.ToDecimal(items.RefundedTaxAmount3));

                    var orderitemsCount = order.OrderItems.Count();
                    var itemsRefundedCount = order.OrderItems.Where(x => x.IsPartialRefund).ToList().Count();
                    if (orderitemsCount == itemsRefundedCount)
                        order.PaymentStatus = PaymentStatus.Refunded;


                    _orderService.UpdateOrder(order);


                }
                else
                {
                    var items = order.OrderItems.Where(x => x.ProductId == refundData.ProductId && x.Id == refundData.OrderItemId).FirstOrDefault();

                    if (items.IsPartialRefund)
                        throw new Exception("The item is already Refunded");

                    List<ProductGlCode> paidCodes = _glCodeService.GetProductGlCodes(items.Product, GLCodeStatusType.Paid).ToList();

                    decimal amount = Convert.ToDecimal(refundData.GLAmount1) + Convert.ToDecimal(refundData.GLAmount2) + Convert.ToDecimal(refundData.GLAmount3) +
                     Convert.ToDecimal(refundData.TaxAmount1) + Convert.ToDecimal(refundData.TaxAmount2) + Convert.ToDecimal(refundData.TaxAmount3);

                    var errors = _orderProcessingService.PartiallyRefund(order, Convert.ToDecimal(amount), items.ProductId).ToList();



                    items.RefundedGlAmount1 = Convert.ToDecimal(items.RefundedGlAmount1) + refundData.GLAmount1;
                    items.RefundedGlAmount2 = Convert.ToDecimal(items.RefundedGlAmount2) + refundData.GLAmount2;
                    items.RefundedGlAmount3 = Convert.ToDecimal(items.RefundedGlAmount3) + refundData.GLAmount3;

                    if (paidCodes.Any())
                    {
                        if (paidCodes.Count == 1)
                        {
                            items.TaxAmount1 = decimal.Zero;
                            items.RefundedTaxAmount1 = -refundData.TaxAmount1;

                            items.GlcodeAmount1 = items.GlcodeAmount1 == null ? (paidCodes[0].Amount * items.Quantity - refundData.GLAmount1) : (items.GlcodeAmount1 - refundData.GLAmount1);
                            items.GlcodeAmount2 = 0;
                            items.GlcodeAmount3 = 0;
                        }
                        if (paidCodes.Count == 2)
                        {
                            items.TaxAmount1 = decimal.Zero;
                            items.TaxAmount2 = decimal.Zero;

                            items.RefundedTaxAmount1 = -refundData.TaxAmount1;
                            items.RefundedTaxAmount2 = -refundData.TaxAmount2;

                            items.GlcodeAmount1 = items.GlcodeAmount1 == null ? (paidCodes[0].Amount * items.Quantity - refundData.GLAmount1) : (items.GlcodeAmount1 - refundData.GLAmount1);
                            items.GlcodeAmount2 = items.GlcodeAmount2 == null ? (paidCodes[1].Amount * items.Quantity - refundData.GLAmount2) : (items.GlcodeAmount2 - refundData.GLAmount2);
                            items.GlcodeAmount3 = 0;
                        }
                        if (paidCodes.Count == 3)
                        {
                            items.TaxAmount1 = decimal.Zero;
                            items.TaxAmount2 = decimal.Zero;
                            items.TaxAmount3 = decimal.Zero;


                            items.RefundedTaxAmount1 = -refundData.TaxAmount1;
                            items.RefundedTaxAmount2 = -refundData.TaxAmount2;
                            items.RefundedTaxAmount3 = -refundData.TaxAmount3;

                            items.GlcodeAmount1 = items.GlcodeAmount1 == null ? (paidCodes[0].Amount * items.Quantity - refundData.GLAmount1) : (items.GlcodeAmount1 - refundData.GLAmount1);
                            items.GlcodeAmount2 = items.GlcodeAmount2 == null ? (paidCodes[1].Amount * items.Quantity - refundData.GLAmount2) : (items.GlcodeAmount2 - refundData.GLAmount2);
                            items.GlcodeAmount3 = items.GlcodeAmount3 == null ? (paidCodes[2].Amount * items.Quantity - refundData.GLAmount3) : (items.GlcodeAmount3 - refundData.GLAmount3);
                        }
                    }

                    items.TotalRefundedAmount = amount;
                    items.GLCodeName1 = refundData.GLCodeName1;
                    items.GLCodeName2 = refundData.GLCodeName2;
                    items.GLCodeName3 = refundData.GLCodeName3;
                    items.TaxName1 = refundData.TaxName1;
                    items.TaxName2 = refundData.TaxName2;
                    items.TaxName3 = refundData.TaxName3;
                    items.IsPartialRefund = true;
                    items.IsFullRefund = true; //To identify if an item has been fully refunded
                                               // items.DateOfRefund = DateTime.Now;
                    _orderItemRepository.Update(items);

                    var totalRefundedAmount = Convert.ToDecimal(items.RefundedGlAmount1) + Convert.ToDecimal(items.RefundedGlAmount2) + Convert.ToDecimal(items.RefundedGlAmount3);
                    var totalRefundedtax = Convert.ToDecimal(items.RefundedTaxAmount1) + Convert.ToDecimal(items.RefundedTaxAmount2) + Convert.ToDecimal(items.RefundedTaxAmount3);
                    order.OrderTax = order.OrderTax + Convert.ToDecimal(items.RefundedTaxAmount1) + Convert.ToDecimal(items.RefundedTaxAmount2) + Convert.ToDecimal(items.RefundedTaxAmount3);
                    order.OrderTotalOnRefund = order.OrderTotalOnRefund - totalRefundedAmount + totalRefundedtax;

                    var orderitemsCount = order.OrderItems.Count();
                    var itemsRefundedCount = order.OrderItems.Where(x => x.IsPartialRefund).ToList().Count();
                    if (orderitemsCount == itemsRefundedCount)
                        order.PaymentStatus = PaymentStatus.Refunded;

                    _orderService.UpdateOrder(order);
                }
                string connString = new DataSettingsManager().LoadSettings().DataConnectionString;
                string query = "InsertCovidRefundRecord";
                using (var db = new SqlConnection(connString))
                {
                    db.Open();
                    var sqlCommand = db.CreateCommand();
                    sqlCommand.CommandType = CommandType.StoredProcedure;
                    sqlCommand.CommandText = query;
                    sqlCommand.Parameters.AddWithValue("@ProductId", refundData.ProductId);
                    sqlCommand.Parameters.AddWithValue("@OrderId", refundData.OrderId);
                    sqlCommand.Parameters.AddWithValue("@OrderItemId", refundData.OrderItemId);
                    sqlCommand.Parameters.AddWithValue("@GlCodeName1", refundData.GLCodeName1);
                    sqlCommand.Parameters.AddWithValue("@GLAmount1", refundData.GLAmount1);
                    sqlCommand.Parameters.AddWithValue("@GLCodeName2", refundData.GLCodeName2);
                    sqlCommand.Parameters.AddWithValue("@GLAmount2", refundData.GLAmount2);
                    sqlCommand.Parameters.AddWithValue("@GLCodeName3", refundData.GLCodeName3);
                    sqlCommand.Parameters.AddWithValue("@GLAmount3", refundData.GLAmount3);
                    sqlCommand.Parameters.AddWithValue("@TaxAmount1", refundData.TaxAmount1);
                    sqlCommand.Parameters.AddWithValue("@TaxName1", refundData.TaxName1);
                    sqlCommand.Parameters.AddWithValue("@TaxAmount2", refundData.TaxAmount2);
                    sqlCommand.Parameters.AddWithValue("@TaxName2", refundData.TaxName2);
                    sqlCommand.Parameters.AddWithValue("@TaxAmount3", refundData.TaxAmount3);
                    sqlCommand.Parameters.AddWithValue("@TaxName3", refundData.TaxName3);
                    sqlCommand.Parameters.AddWithValue("@TotalRefund", refundData.TotalRefund);
                    sqlCommand.Parameters.AddWithValue("@RefundedTaxAmount1", refundData.RefundedTaxAmount1);
                    sqlCommand.Parameters.AddWithValue("@RefundedTaxAmount2", refundData.RefundedTaxAmount2);
                    sqlCommand.Parameters.AddWithValue("@RefundedTaxAmount3", refundData.RefundedTaxAmount3);
                    sqlCommand.Parameters.AddWithValue("@DeliveryTaxName", refundData.DeliveryTaxName);
                    sqlCommand.Parameters.AddWithValue("@DeliveryGLCodeName", refundData.DeliveryGLCodeName);
                    sqlCommand.Parameters.AddWithValue("@DeliveryTax", refundData.DeliveryTax);
                    sqlCommand.Parameters.AddWithValue("@DeliveryPickupAmount", refundData.DeliveryPickupAmount);
                    sqlCommand.Parameters.AddWithValue("@ShippingAmount", refundData.ShippingAmount);
                    sqlCommand.Parameters.AddWithValue("@ShippingTax", refundData.ShippingTax);
                    sqlCommand.Parameters.AddWithValue("@ShippingTaxName", refundData.ShippingTaxName);
                    sqlCommand.Parameters.AddWithValue("@Error", refundData.Error);
                    sqlCommand.Parameters.AddWithValue("@Success", true);
                    sqlCommand.Parameters.AddWithValue("@CreatedDateUtc", DateTime.UtcNow);
                    sqlCommand.Parameters.AddWithValue("@OverridenGlcode1", refundData.Glcodeid1);
                    sqlCommand.Parameters.AddWithValue("@OverridenGlcode2", refundData.Glcodeid2);
                    sqlCommand.Parameters.AddWithValue("@OverridenGlcode3", refundData.Glcodeid3);
                    var affectedrows = sqlCommand.ExecuteNonQuery();
                    if (affectedrows == 1)
                    {
                        refundData.Success = "Partially Refunded successfully";
                        return Json(refundData);
                    }
                    db.Close();
                }

            }
            catch (Exception ex)
            {
                refundData.Error = ex.Message.ToString();
                return Json(refundData);
            }

            refundData.Success = "Fully Refunded successfully";
            return null;
        }
    }
}
