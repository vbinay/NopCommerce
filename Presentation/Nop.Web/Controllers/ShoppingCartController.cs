using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Core.Plugins;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Shipping;
using Nop.Services.Tax;
using Nop.Web.Extensions;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Security;
using Nop.Web.Framework.Security.Captcha;
using Nop.Web.Infrastructure.Cache;
using Nop.Web.Models.Common;
using Nop.Web.Models.Media;
using Nop.Web.Models.ShoppingCart;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Data.SqlClient;
using System.Collections;
using Nop.Core.Data;
namespace Nop.Web.Controllers
{
    public partial class ShoppingCartController : BasePublicController
    {
        #region Fields

        private readonly IProductService _productService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IPictureService _pictureService;
        private readonly ILocalizationService _localizationService;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IProductAttributeFormatter _productAttributeFormatter;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly ITaxService _taxService;
        private readonly ICurrencyService _currencyService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly ICheckoutAttributeParser _checkoutAttributeParser;
        private readonly ICheckoutAttributeFormatter _checkoutAttributeFormatter;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IDiscountService _discountService;
        private readonly ICustomerService _customerService;
        private readonly IGiftCardService _giftCardService;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IShippingService _shippingService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly ICheckoutAttributeService _checkoutAttributeService;
        private readonly IPaymentService _paymentService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly IPermissionService _permissionService;
        private readonly IDownloadService _downloadService;
        private readonly ICacheManager _cacheManager;
        private readonly IWebHelper _webHelper;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IAddressAttributeFormatter _addressAttributeFormatter;
        private readonly IPluginFinder _pluginFinder;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly PaymentSettings _paymentSettings;

        private readonly HttpContextBase _httpContext;

        private readonly MediaSettings _mediaSettings;
        private readonly ShoppingCartSettings _shoppingCartSettings;
        private readonly CatalogSettings _catalogSettings;
        private readonly OrderSettings _orderSettings;
        private readonly ShippingSettings _shippingSettings;
        private readonly TaxSettings _taxSettings;
        private readonly CaptchaSettings _captchaSettings;
        private readonly AddressSettings _addressSettings;
        private readonly RewardPointsSettings _rewardPointsSettings;
        private readonly CustomerSettings _customerSettings;

        #endregion

        #region Constructors

        public ShoppingCartController(IProductService productService,
            IStoreContext storeContext,
            IWorkContext workContext,
            IShoppingCartService shoppingCartService,
            IPictureService pictureService,
            ILocalizationService localizationService,
            IProductAttributeService productAttributeService,
            IProductAttributeFormatter productAttributeFormatter,
            IProductAttributeParser productAttributeParser,
            ITaxService taxService, ICurrencyService currencyService,
            IPriceCalculationService priceCalculationService,
            IPriceFormatter priceFormatter,
            ICheckoutAttributeParser checkoutAttributeParser,
            ICheckoutAttributeFormatter checkoutAttributeFormatter,
            IOrderProcessingService orderProcessingService,
            IDiscountService discountService,
            ICustomerService customerService,
            IGiftCardService giftCardService,
            ICountryService countryService,
            IStateProvinceService stateProvinceService,
            IShippingService shippingService,
            IOrderTotalCalculationService orderTotalCalculationService,
            ICheckoutAttributeService checkoutAttributeService,
            IPaymentService paymentService,
            IWorkflowMessageService workflowMessageService,
            IPermissionService permissionService,
            IDownloadService downloadService,
            ICacheManager cacheManager,
            IWebHelper webHelper,
            ICustomerActivityService customerActivityService,
            IGenericAttributeService genericAttributeService,
            IAddressAttributeFormatter addressAttributeFormatter,
            IPluginFinder pluginFinder,
            IDateTimeHelper dateTimeHelper,
            PaymentSettings paymentSettings,
            HttpContextBase httpContext,
            MediaSettings mediaSettings,
            ShoppingCartSettings shoppingCartSettings,
            CatalogSettings catalogSettings,
            OrderSettings orderSettings,
            ShippingSettings shippingSettings,
            TaxSettings taxSettings,
            CaptchaSettings captchaSettings,
            AddressSettings addressSettings,
            RewardPointsSettings rewardPointsSettings,
            CustomerSettings customerSettings)
        {
            this._productService = productService;
            this._workContext = workContext;
            this._storeContext = storeContext;
            this._shoppingCartService = shoppingCartService;
            this._pictureService = pictureService;
            this._localizationService = localizationService;
            this._productAttributeService = productAttributeService;
            this._productAttributeFormatter = productAttributeFormatter;
            this._productAttributeParser = productAttributeParser;
            this._taxService = taxService;
            this._currencyService = currencyService;
            this._priceCalculationService = priceCalculationService;
            this._priceFormatter = priceFormatter;
            this._checkoutAttributeParser = checkoutAttributeParser;
            this._checkoutAttributeFormatter = checkoutAttributeFormatter;
            this._orderProcessingService = orderProcessingService;
            this._discountService = discountService;
            this._customerService = customerService;
            this._giftCardService = giftCardService;
            this._countryService = countryService;
            this._stateProvinceService = stateProvinceService;
            this._shippingService = shippingService;
            this._orderTotalCalculationService = orderTotalCalculationService;
            this._checkoutAttributeService = checkoutAttributeService;
            this._paymentService = paymentService;
            this._workflowMessageService = workflowMessageService;
            this._permissionService = permissionService;
            this._downloadService = downloadService;
            this._cacheManager = cacheManager;
            this._webHelper = webHelper;
            this._customerActivityService = customerActivityService;
            this._genericAttributeService = genericAttributeService;
            this._addressAttributeFormatter = addressAttributeFormatter;
            this._httpContext = httpContext;
            this._pluginFinder = pluginFinder;
            this._dateTimeHelper = dateTimeHelper;
            this._paymentSettings = paymentSettings;
            this._mediaSettings = mediaSettings;
            this._shoppingCartSettings = shoppingCartSettings;
            this._catalogSettings = catalogSettings;
            this._orderSettings = orderSettings;
            this._shippingSettings = shippingSettings;
            this._taxSettings = taxSettings;
            this._captchaSettings = captchaSettings;
            this._addressSettings = addressSettings;
            this._rewardPointsSettings = rewardPointsSettings;
            this._customerSettings = customerSettings;
        }

        #endregion

        #region Utilities

        [NonAction]
        protected virtual PictureModel PrepareCartItemPictureModel(ShoppingCartItem sci,
            int pictureSize, bool showDefaultPicture, string productName)
        {
            var pictureCacheKey = string.Format(ModelCacheEventConsumer.CART_PICTURE_MODEL_KEY, sci.Id, pictureSize, true, _workContext.WorkingLanguage.Id, _webHelper.IsCurrentConnectionSecured(), _storeContext.CurrentStore.Id);
            var model = _cacheManager.Get(pictureCacheKey,
                //as we cache per user (shopping cart item identifier)
                //let's cache just for 3 minutes
                3, () =>
                {
                    //shopping cart item picture
                    var sciPicture = sci.Product.GetProductPicture(sci.AttributesXml, _pictureService, _productAttributeParser);
                    return new PictureModel
                    {
                        ImageUrl = _pictureService.GetPictureUrl(sciPicture, pictureSize, showDefaultPicture),
                        Title = string.Format(_localizationService.GetResource("Media.Product.ImageLinkTitleFormat"), productName),
                        AlternateText = (!string.IsNullOrEmpty(_localizationService.GetResource("Media.Product.ImageAlternateTextFormat"))) ? string.Format(_localizationService.GetResource("Media.Product.ImageAlternateTextFormat"), productName) : productName,
                    };
                });
            return model;
        }

        /// <summary>
        /// Prepare shopping cart model
        /// </summary>
        /// <param name="model">Model instance</param>
        /// <param name="cart">Shopping cart</param>
        /// <param name="isEditable">A value indicating whether cart is editable</param>
        /// <param name="validateCheckoutAttributes">A value indicating whether we should validate checkout attributes when preparing the model</param>
        /// <param name="prepareEstimateShippingIfEnabled">A value indicating whether we should prepare "Estimate shipping" model</param>
        /// <param name="setEstimateShippingDefaultAddress">A value indicating whether we should prefill "Estimate shipping" model with the default customer address</param>
        /// <param name="prepareAndDisplayOrderReviewData">A value indicating whether we should prepare review data (such as billing/shipping address, payment or shipping data entered during checkout)</param>
        /// <returns>Model</returns>
        [NonAction]
        protected virtual void PrepareShoppingCartModel(ShoppingCartModel model,
            IList<ShoppingCartItem> cart, bool isEditable = true,
            string discountcouponcode = null,
            bool validateCheckoutAttributes = false,
            bool prepareEstimateShippingIfEnabled = true, bool setEstimateShippingDefaultAddress = true,
            bool prepareAndDisplayOrderReviewData = false
            )
        {
            if (cart == null)
            {
                throw new ArgumentNullException("cart");
            }

            if (model == null)
            {
                throw new ArgumentNullException("model");
            }

            model.OnePageCheckoutEnabled = _orderSettings.OnePageCheckoutEnabled;

            if (!cart.Any())
            {
                return;
            }

            #region Simple properties

            model.IsEditable = isEditable;
            model.ShowProductImages = _shoppingCartSettings.ShowProductImagesOnShoppingCart;
            model.ShowSku = _catalogSettings.ShowProductSku;
            var checkoutAttributesXml = _workContext.CurrentCustomer.GetAttribute<string>(SystemCustomerAttributeNames.CheckoutAttributes, _genericAttributeService, _storeContext.CurrentStore.Id);
            model.CheckoutAttributeInfo = _checkoutAttributeFormatter.FormatAttributes(checkoutAttributesXml, _workContext.CurrentCustomer);
            bool minOrderSubtotalAmountOk = _orderProcessingService.ValidateMinOrderSubtotalAmount(cart);
            if (!minOrderSubtotalAmountOk)
            {
                decimal minOrderSubtotalAmount = _currencyService.ConvertFromPrimaryStoreCurrency(_orderSettings.MinOrderSubtotalAmount, _workContext.WorkingCurrency);
                model.MinOrderSubtotalWarning = string.Format(_localizationService.GetResource("Checkout.MinOrderSubtotalAmount"), _priceFormatter.FormatPrice(minOrderSubtotalAmount, true, false));
            }
            model.TermsOfServiceOnShoppingCartPage = _orderSettings.TermsOfServiceOnShoppingCartPage;
            model.TermsOfServiceOnOrderConfirmPage = _orderSettings.TermsOfServiceOnOrderConfirmPage;
            model.DisplayTaxShippingInfo = _catalogSettings.DisplayTaxShippingInfoShoppingCart;

            //gift card and gift card boxes
            model.DiscountBox.Display = _shoppingCartSettings.ShowDiscountBox;
            var discountCouponCode = _workContext.CurrentCustomer.GetAttribute<string>(SystemCustomerAttributeNames.DiscountCouponCode);
            var discount = _discountService.GetDiscountByCouponCode(discountCouponCode);
            if (discount != null &&
                discount.RequiresCouponCode &&
                _discountService.ValidateDiscount(discount, _workContext.CurrentCustomer).IsValid)
            {
                model.DiscountBox.CurrentCode = discount.CouponCode;
            }

            model.GiftCardBox.Display = _shoppingCartSettings.ShowGiftCardBox;

            //cart warnings
            var cartWarnings = _shoppingCartService.GetShoppingCartWarnings(cart, checkoutAttributesXml, validateCheckoutAttributes);
            foreach (var warning in cartWarnings)
            {
                model.Warnings.Add(warning);
            }




            #endregion

            #region Checkout attributes

            var checkoutAttributes = _checkoutAttributeService.GetAllCheckoutAttributes(_storeContext.CurrentStore.Id, !cart.RequiresShipping());
            foreach (var attribute in checkoutAttributes)
            {
                var attributeModel = new ShoppingCartModel.CheckoutAttributeModel
                {
                    Id = attribute.Id,
                    Name = attribute.GetLocalized(x => x.Name),
                    TextPrompt = attribute.GetLocalized(x => x.TextPrompt),
                    IsRequired = attribute.IsRequired,
                    AttributeControlType = attribute.AttributeControlType,
                    DefaultValue = attribute.DefaultValue
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
                    var attributeValues = _checkoutAttributeService.GetCheckoutAttributeValues(attribute.Id);
                    foreach (var attributeValue in attributeValues)
                    {
                        var attributeValueModel = new ShoppingCartModel.CheckoutAttributeValueModel
                        {
                            Id = attributeValue.Id,
                            Name = attributeValue.GetLocalized(x => x.Name),
                            ColorSquaresRgb = attributeValue.ColorSquaresRgb,
                            IsPreSelected = attributeValue.IsPreSelected,
                        };
                        attributeModel.Values.Add(attributeValueModel);

                        //display price if allowed
                        if (_permissionService.Authorize(StandardPermissionProvider.DisplayPrices))
                        {
                            decimal priceAdjustmentBase = _taxService.GetCheckoutAttributePrice(attributeValue);
                            decimal priceAdjustment = _currencyService.ConvertFromPrimaryStoreCurrency(priceAdjustmentBase, _workContext.WorkingCurrency);
                            if (priceAdjustmentBase > decimal.Zero)
                            {
                                attributeValueModel.PriceAdjustment = "+" + _priceFormatter.FormatPrice(priceAdjustment);
                            }
                            else if (priceAdjustmentBase < decimal.Zero)
                            {
                                attributeValueModel.PriceAdjustment = "-" + _priceFormatter.FormatPrice(-priceAdjustment);
                            }
                        }
                    }
                }



                //set already selected attributes
                var selectedCheckoutAttributes = _workContext.CurrentCustomer.GetAttribute<string>(SystemCustomerAttributeNames.CheckoutAttributes, _genericAttributeService, _storeContext.CurrentStore.Id);
                switch (attribute.AttributeControlType)
                {
                    case AttributeControlType.DropdownList:
                    case AttributeControlType.RadioList:
                    case AttributeControlType.Checkboxes:
                    case AttributeControlType.ColorSquares:
                    case AttributeControlType.ImageSquares:
                        {
                            if (!String.IsNullOrEmpty(selectedCheckoutAttributes))
                            {
                                //clear default selection
                                foreach (var item in attributeModel.Values)
                                {
                                    item.IsPreSelected = false;
                                }

                                //select new values
                                var selectedValues = _checkoutAttributeParser.ParseCheckoutAttributeValues(selectedCheckoutAttributes);
                                foreach (var attributeValue in selectedValues)
                                {
                                    foreach (var item in attributeModel.Values)
                                    {
                                        if (attributeValue.Id == item.Id)
                                        {
                                            item.IsPreSelected = true;
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    case AttributeControlType.ReadonlyCheckboxes:
                        {
                            //do nothing
                            //values are already pre-set
                        }
                        break;
                    case AttributeControlType.TextBox:
                    case AttributeControlType.MultilineTextbox:
                        {
                            if (!String.IsNullOrEmpty(selectedCheckoutAttributes))
                            {
                                var enteredText = _checkoutAttributeParser.ParseValues(selectedCheckoutAttributes, attribute.Id);
                                if (enteredText.Any())
                                {
                                    attributeModel.DefaultValue = enteredText[0];
                                }
                            }
                        }
                        break;
                    case AttributeControlType.Datepicker:
                        {
                            //keep in mind my that the code below works only in the current culture
                            var selectedDateStr = _checkoutAttributeParser.ParseValues(selectedCheckoutAttributes, attribute.Id);
                            if (selectedDateStr.Any())
                            {
                                DateTime selectedDate;
                                if (DateTime.TryParseExact(selectedDateStr[0], "D", CultureInfo.CurrentCulture,
                                                       DateTimeStyles.None, out selectedDate))
                                {
                                    //successfully parsed
                                    attributeModel.SelectedDay = selectedDate.Day;
                                    attributeModel.SelectedMonth = selectedDate.Month;
                                    attributeModel.SelectedYear = selectedDate.Year;
                                }
                            }

                        }
                        break;
                    case AttributeControlType.FileUpload:
                        {
                            if (!String.IsNullOrEmpty(selectedCheckoutAttributes))
                            {
                                var downloadGuidStr = _checkoutAttributeParser.ParseValues(selectedCheckoutAttributes, attribute.Id).FirstOrDefault();
                                Guid downloadGuid;
                                Guid.TryParse(downloadGuidStr, out downloadGuid);
                                var download = _downloadService.GetDownloadByGuid(downloadGuid);
                                if (download != null)
                                {
                                    attributeModel.DefaultValue = download.DownloadGuid.ToString();
                                }
                            }
                        }
                        break;
                    default:
                        break;
                }

                model.CheckoutAttributes.Add(attributeModel);
            }

            #endregion

            #region Estimate shipping

            if (prepareEstimateShippingIfEnabled)
            {
                model.EstimateShipping.Enabled = cart.Any() && cart.RequiresShipping() && _shippingSettings.EstimateShippingEnabled;
                if (model.EstimateShipping.Enabled)
                {
                    //countries
                    int? defaultEstimateCountryId = (setEstimateShippingDefaultAddress && _workContext.CurrentCustomer.ShippingAddress != null) ? _workContext.CurrentCustomer.ShippingAddress.CountryId : model.EstimateShipping.CountryId;
                    model.EstimateShipping.AvailableCountries.Add(new SelectListItem { Text = _localizationService.GetResource("Address.SelectCountry"), Value = "0" });
                    foreach (var c in _countryService.GetAllCountriesForShipping(_workContext.WorkingLanguage.Id))
                    {
                        model.EstimateShipping.AvailableCountries.Add(new SelectListItem
                        {
                            Text = c.GetLocalized(x => x.Name),
                            Value = c.Id.ToString(),
                            Selected = c.Id == defaultEstimateCountryId
                        });
                    }
                    //states
                    int? defaultEstimateStateId = (setEstimateShippingDefaultAddress && _workContext.CurrentCustomer.ShippingAddress != null) ? _workContext.CurrentCustomer.ShippingAddress.StateProvinceId : model.EstimateShipping.StateProvinceId;
                    var states = defaultEstimateCountryId.HasValue ? _stateProvinceService.GetStateProvincesByCountryId(defaultEstimateCountryId.Value, _workContext.WorkingLanguage.Id).ToList() : new List<StateProvince>();
                    if (states.Any())
                    {
                        foreach (var s in states)
                        {
                            model.EstimateShipping.AvailableStates.Add(new SelectListItem
                            {
                                Text = s.GetLocalized(x => x.Name),
                                Value = s.Id.ToString(),
                                Selected = s.Id == defaultEstimateStateId
                            });
                        }
                    }
                    else
                    {
                        model.EstimateShipping.AvailableStates.Add(new SelectListItem { Text = _localizationService.GetResource("Address.OtherNonUS"), Value = "0" });
                    }

                    if (setEstimateShippingDefaultAddress && _workContext.CurrentCustomer.ShippingAddress != null)
                    {
                        model.EstimateShipping.ZipPostalCode = _workContext.CurrentCustomer.ShippingAddress.ZipPostalCode;
                    }
                }
            }

            #endregion

            #region Cart items
            decimal disocuntdAmountbySybCategory = decimal.Zero;
            Dictionary<int, decimal> disocuntdAmountbySybCategoryVertex = new Dictionary<int, decimal>();
            foreach (var sci in cart)
            {

                var cartItemModel = new ShoppingCartModel.ShoppingCartItemModel
                {
                    Id = sci.Id,
                    Sku = sci.Product.FormatSku(sci.AttributesXml, _productAttributeParser),
                    ProductId = sci.Product.Id,
                    ProductName = sci.Product.GetLocalized(x => x.Name),
                    ProductSeName = sci.Product.GetSeName(),
                    Quantity = sci.Quantity,
                    AttributeInfo = _productAttributeFormatter.FormatAttributes(sci.Product, sci.AttributesXml),
                    CheckoutNotes = sci.Product.CheckoutNotes,  /// NU-19
                    SelectedWarehouseId = sci.SelectedWarehouseId, /// SODMYWAY-
                    IsReservation = sci.IsReservation,
                    ReservedTimeSlot = sci.IsReservation ? Convert.ToString(_httpContext.Session["ReservationSlots"]) : "",
                    ReservationDate = Convert.ToDateTime(_httpContext.Session["ReservationDate"]),// sci.ReservationDate
                };

                #region SODMYWAY-
                if (cartItemModel.SelectedWarehouseId != null)
                {
                    cartItemModel.SelectedWarehouse =
                        _shippingService.GetWarehouseById(cartItemModel.SelectedWarehouseId.GetValueOrDefault(0));
                }

                if (sci.RequestedFullfilmentDateTime.HasValue)
                {
                    cartItemModel.RequestedFulfillmentDateTime = sci.RequestedFullfilmentDateTime.GetValueOrDefault();
                }

                if (!string.IsNullOrEmpty(sci.SelectedShippingMethodName))
                {
                    cartItemModel.FulfillmentModel.SelectedShippingMethodName = sci.SelectedShippingMethodName;
                }

                if (!string.IsNullOrEmpty(sci.SelectedShippingRateComputationMethodSystemName)) //SODMYWAY-3467
                {
                    cartItemModel.FulfillmentModel.SelectedShippingRateComputationMethodSystemName = sci.SelectedShippingRateComputationMethodSystemName;
                }
                #endregion



                //allow editing?
                //1. setting enabled?
                //2. simple product?
                //3. has attribute or gift card?
                //4. visible individually?
                cartItemModel.AllowItemEditing = _shoppingCartSettings.AllowCartItemEditing &&
                    sci.Product.ProductType == ProductType.SimpleProduct &&
                    (!String.IsNullOrEmpty(cartItemModel.AttributeInfo) || sci.Product.IsGiftCard || sci.Product.IsMealPlan) && 	/// NU-16
                    sci.Product.VisibleIndividually;

                //allowed quantities
                var allowedQuantities = sci.Product.ParseAllowedQuantities();
                foreach (var qty in allowedQuantities)
                {
                    cartItemModel.AllowedQuantities.Add(new SelectListItem
                    {
                        Text = qty.ToString(),
                        Value = qty.ToString(),
                        Selected = sci.Quantity == qty
                    });
                }

                //recurring info
                if (sci.Product.IsRecurring)
                {
                    cartItemModel.RecurringInfo = string.Format(_localizationService.GetResource("ShoppingCart.RecurringPeriod"), sci.Product.RecurringCycleLength, sci.Product.RecurringCyclePeriod.GetLocalizedEnum(_localizationService, _workContext));
                }

                //rental info
                if (sci.Product.IsRental)
                {
                    var rentalStartDate = sci.RentalStartDateUtc.HasValue ? sci.Product.FormatRentalDate(sci.RentalStartDateUtc.Value) : "";
                    var rentalEndDate = sci.RentalEndDateUtc.HasValue ? sci.Product.FormatRentalDate(sci.RentalEndDateUtc.Value) : "";
                    cartItemModel.RentalInfo = string.Format(_localizationService.GetResource("ShoppingCart.Rental.FormattedDate"),
                        rentalStartDate, rentalEndDate);
                }

                //unit prices
                if (sci.Product.CallForPrice)
                {
                    cartItemModel.UnitPrice = _localizationService.GetResource("Products.CallForPrice");
                }
                else
                {
                    decimal taxRate;
                    decimal shoppingCartUnitPriceWithDiscountBase = _taxService.GetProductPrice(sci.Product, _priceCalculationService.GetUnitPrice(sci), out taxRate);
                    decimal shoppingCartUnitPriceWithDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartUnitPriceWithDiscountBase, _workContext.WorkingCurrency);
                    cartItemModel.UnitPrice = _priceFormatter.FormatPrice(shoppingCartUnitPriceWithDiscount);
                    var discountbySub = _discountService.GetDiscountByCouponCode(discountCouponCode);
                    if (discountbySub != null)
                    {
                        if (discountbySub.DiscountType == DiscountType.AssignedToOrderSubTotal)
                        {
                            //var discAmountPercentage = discount.DiscountPercentage / cart.Count();
                            decimal result = (decimal)((((float)shoppingCartUnitPriceWithDiscountBase) * ((float)discount.DiscountPercentage)) / 100f);

                            shoppingCartUnitPriceWithDiscountBase = (shoppingCartUnitPriceWithDiscount - result);
                            shoppingCartUnitPriceWithDiscount = (shoppingCartUnitPriceWithDiscount - result);
                            cartItemModel.UnitPrice = _priceFormatter.FormatPrice(shoppingCartUnitPriceWithDiscount);

                        }
                    }

                }
                //subtotal, discount
                if (sci.Product.CallForPrice)
                {
                    cartItemModel.SubTotal = _localizationService.GetResource("Products.CallForPrice");
                }
                else
                {
                    //sub total
                    List<Discount> scDiscounts;
                    decimal shoppingCartItemDiscountBase;
                    decimal taxRate;

                    decimal shoppingCartItemSubTotalWithDiscountBase = _taxService.GetProductPrice(sci.Product, _priceCalculationService.GetSubTotal(sci, true, out shoppingCartItemDiscountBase, out scDiscounts), out taxRate);
                    decimal shoppingCartItemSubTotalWithDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartItemSubTotalWithDiscountBase, _workContext.WorkingCurrency);
                    cartItemModel.SubTotal = _priceFormatter.FormatPrice(shoppingCartItemSubTotalWithDiscount);
                    var discountbySub = _discountService.GetDiscountByCouponCode(discountCouponCode);
                    if (discountbySub != null)
                    {
                        if (discountbySub.DiscountType == DiscountType.AssignedToOrderSubTotal)
                        {
                            //var discAmountPercentage = discount.DiscountPercentage / cart.Count();
                            decimal result = (decimal)((((float)shoppingCartItemSubTotalWithDiscountBase) * ((float)discount.DiscountPercentage)) / 100f);

                            shoppingCartItemSubTotalWithDiscountBase = (shoppingCartItemSubTotalWithDiscountBase - result);
                            shoppingCartItemSubTotalWithDiscount = (shoppingCartItemSubTotalWithDiscount - result);
                            cartItemModel.SubTotal = _priceFormatter.FormatPrice(shoppingCartItemSubTotalWithDiscountBase);
                            disocuntdAmountbySybCategory = disocuntdAmountbySybCategory + result;
                            disocuntdAmountbySybCategoryVertex.Add(sci.Product.Id, shoppingCartItemSubTotalWithDiscountBase);
                        }
                    }
                    //display an applied discount amount
                    if (shoppingCartItemDiscountBase > decimal.Zero)
                    {
                        shoppingCartItemDiscountBase = _taxService.GetProductPrice(sci.Product, shoppingCartItemDiscountBase, out taxRate);
                        if (shoppingCartItemDiscountBase > decimal.Zero)
                        {
                            decimal shoppingCartItemDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartItemDiscountBase, _workContext.WorkingCurrency);
                            cartItemModel.Discount = _priceFormatter.FormatPrice(shoppingCartItemDiscount);
                        }
                    }
                }

                //picture
                if (_shoppingCartSettings.ShowProductImagesOnShoppingCart)
                {
                    cartItemModel.Picture = PrepareCartItemPictureModel(sci,
                        _mediaSettings.CartThumbPictureSize, true, cartItemModel.ProductName);
                }

                //item warnings
                var itemWarnings = _shoppingCartService.GetShoppingCartItemWarnings(
                    _workContext.CurrentCustomer,
                    sci.ShoppingCartType,
                    sci.Product,
                    sci.StoreId,
                    sci.AttributesXml,
                    sci.CustomerEnteredPrice,
                    sci.RentalStartDateUtc,
                    sci.RentalEndDateUtc,
                    sci.Quantity,
                    false);
                foreach (var warning in itemWarnings)
                {
                    cartItemModel.Warnings.Add(warning);
                }

                model.Items.Add(cartItemModel);
            }

            _httpContext.Session["discountedAmountBySubcategory"] = disocuntdAmountbySybCategory;
            _httpContext.Session["discountedAmountBySubcategoryVertex"] = disocuntdAmountbySybCategoryVertex;
            #endregion

            #region Button payment methods

            var paymentMethods = _paymentService
                .LoadActivePaymentMethods(_workContext.CurrentCustomer.Id, _storeContext.CurrentStore.Id)
                .Where(pm => pm.PaymentMethodType == PaymentMethodType.Button)
                .Where(pm => !pm.HidePaymentMethod(cart))
                .ToList();
            foreach (var pm in paymentMethods)
            {
                if (cart.IsRecurring() && pm.RecurringPaymentType == RecurringPaymentType.NotSupported)
                {
                    continue;
                }

                string actionName;
                string controllerName;
                RouteValueDictionary routeValues;
                pm.GetPaymentInfoRoute(out actionName, out controllerName, out routeValues);

                model.ButtonPaymentMethodActionNames.Add(actionName);
                model.ButtonPaymentMethodControllerNames.Add(controllerName);
                model.ButtonPaymentMethodRouteValues.Add(routeValues);
            }

            #endregion

            #region Order review data

            if (prepareAndDisplayOrderReviewData)
            {
                model.OrderReviewData.Display = true;

                //billing info
                var billingAddress = _workContext.CurrentCustomer.BillingAddress;
                if (billingAddress != null)
                {
                    model.OrderReviewData.BillingAddress.PrepareModel(
                        address: billingAddress,
                        excludeProperties: false,
                        addressSettings: _addressSettings,
                        addressAttributeFormatter: _addressAttributeFormatter);
                }

                //shipping info
                if (cart.RequiresShipping())
                {
                    model.OrderReviewData.IsShippable = true;

                    var pickupPoint = _workContext.CurrentCustomer
                        .GetAttribute<PickupPoint>(SystemCustomerAttributeNames.SelectedPickupPoint, _storeContext.CurrentStore.Id);
                    model.OrderReviewData.SelectedPickUpInStore = _shippingSettings.AllowPickUpInStore && pickupPoint != null;
                    if (!model.OrderReviewData.SelectedPickUpInStore)
                    {
                        if (_workContext.CurrentCustomer.ShippingAddress != null)
                        {
                            model.OrderReviewData.ShippingAddress.PrepareModel(
                                address: _workContext.CurrentCustomer.ShippingAddress,
                                excludeProperties: false,
                                addressSettings: _addressSettings,
                                addressAttributeFormatter: _addressAttributeFormatter);
                        }
                    }
                    else
                    {
                        var country = _countryService.GetCountryByTwoLetterIsoCode(pickupPoint.CountryCode);
                        model.OrderReviewData.PickupAddress = new AddressModel
                        {
                            Address1 = pickupPoint.Address,
                            City = pickupPoint.City,
                            CountryName = country != null ? country.Name : string.Empty,
                            ZipPostalCode = pickupPoint.ZipPostalCode
                        };
                    }

                    //selected shipping method
                    var shippingOption = _workContext.CurrentCustomer.GetAttribute<ShippingOption>(SystemCustomerAttributeNames.SelectedShippingOption, _storeContext.CurrentStore.Id);
                    if (shippingOption != null)
                    {
                        model.OrderReviewData.ShippingMethod = shippingOption.Name;
                    }
                }
                //payment info
                var selectedPaymentMethodSystemName = _workContext.CurrentCustomer.GetAttribute<string>(
                    SystemCustomerAttributeNames.SelectedPaymentMethod, _storeContext.CurrentStore.Id);
                var paymentMethod = _paymentService.LoadPaymentMethodBySystemName(selectedPaymentMethodSystemName);
                model.OrderReviewData.PaymentMethod = paymentMethod != null ? paymentMethod.GetLocalizedFriendlyName(_localizationService, _workContext.WorkingLanguage.Id) : "";

                //custom values
                var processPaymentRequest = _httpContext.Session["OrderPaymentInfo"] as ProcessPaymentRequest;
                if (processPaymentRequest != null)
                {
                    model.OrderReviewData.CustomValues = processPaymentRequest.CustomValues;
                }
            }
            #endregion
        }

        [NonAction]
        protected virtual void PrepareWishlistModel(WishlistModel model,
            IList<ShoppingCartItem> cart, bool isEditable = true)
        {
            if (cart == null)
            {
                throw new ArgumentNullException("cart");
            }

            if (model == null)
            {
                throw new ArgumentNullException("model");
            }

            model.EmailWishlistEnabled = _shoppingCartSettings.EmailWishlistEnabled;
            model.IsEditable = isEditable;
            model.DisplayAddToCart = _permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart);
            model.DisplayTaxShippingInfo = _catalogSettings.DisplayTaxShippingInfoWishlist;

            if (!cart.Any())
            {
                return;
            }

            #region Simple properties

            var customer = cart.GetCustomer();
            model.CustomerGuid = customer.CustomerGuid;
            model.CustomerFullname = customer.GetFullName();
            model.ShowProductImages = _shoppingCartSettings.ShowProductImagesOnShoppingCart;
            model.ShowSku = _catalogSettings.ShowProductSku;

            //cart warnings
            var cartWarnings = _shoppingCartService.GetShoppingCartWarnings(cart, "", false);
            foreach (var warning in cartWarnings)
            {
                model.Warnings.Add(warning);
            }

            #endregion

            #region Cart items

            foreach (var sci in cart)
            {
                var cartItemModel = new WishlistModel.ShoppingCartItemModel
                {
                    Id = sci.Id,
                    Sku = sci.Product.FormatSku(sci.AttributesXml, _productAttributeParser),
                    ProductId = sci.Product.Id,
                    ProductName = sci.Product.GetLocalized(x => x.Name),
                    ProductSeName = sci.Product.GetSeName(),
                    Quantity = sci.Quantity,
                    AttributeInfo = _productAttributeFormatter.FormatAttributes(sci.Product, sci.AttributesXml),
                };

                //allow editing?
                //1. setting enabled?
                //2. simple product?
                //3. has attribute or gift card?
                //4. visible individually?
                cartItemModel.AllowItemEditing = _shoppingCartSettings.AllowCartItemEditing &&
                    sci.Product.ProductType == ProductType.SimpleProduct &&
                    (!String.IsNullOrEmpty(cartItemModel.AttributeInfo) || sci.Product.IsGiftCard) &&
                    sci.Product.VisibleIndividually;

                //allowed quantities
                var allowedQuantities = sci.Product.ParseAllowedQuantities();
                foreach (var qty in allowedQuantities)
                {
                    cartItemModel.AllowedQuantities.Add(new SelectListItem
                    {
                        Text = qty.ToString(),
                        Value = qty.ToString(),
                        Selected = sci.Quantity == qty
                    });
                }


                //recurring info
                if (sci.Product.IsRecurring)
                {
                    cartItemModel.RecurringInfo = string.Format(_localizationService.GetResource("ShoppingCart.RecurringPeriod"), sci.Product.RecurringCycleLength, sci.Product.RecurringCyclePeriod.GetLocalizedEnum(_localizationService, _workContext));
                }

                //rental info
                if (sci.Product.IsRental)
                {
                    var rentalStartDate = sci.RentalStartDateUtc.HasValue ? sci.Product.FormatRentalDate(sci.RentalStartDateUtc.Value) : "";
                    var rentalEndDate = sci.RentalEndDateUtc.HasValue ? sci.Product.FormatRentalDate(sci.RentalEndDateUtc.Value) : "";
                    cartItemModel.RentalInfo = string.Format(_localizationService.GetResource("ShoppingCart.Rental.FormattedDate"),
                        rentalStartDate, rentalEndDate);
                }

                //unit prices
                if (sci.Product.CallForPrice)
                {
                    cartItemModel.UnitPrice = _localizationService.GetResource("Products.CallForPrice");
                }
                else
                {
                    decimal taxRate;
                    decimal shoppingCartUnitPriceWithDiscountBase = _taxService.GetProductPrice(sci.Product, _priceCalculationService.GetUnitPrice(sci), out taxRate);
                    decimal shoppingCartUnitPriceWithDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartUnitPriceWithDiscountBase, _workContext.WorkingCurrency);
                    cartItemModel.UnitPrice = _priceFormatter.FormatPrice(shoppingCartUnitPriceWithDiscount);
                }
                //subtotal, discount
                if (sci.Product.CallForPrice)
                {
                    cartItemModel.SubTotal = _localizationService.GetResource("Products.CallForPrice");
                }
                else
                {
                    //sub total
                    List<Discount> scDiscounts;
                    decimal shoppingCartItemDiscountBase;
                    decimal taxRate;
                    decimal shoppingCartItemSubTotalWithDiscountBase = _taxService.GetProductPrice(sci.Product, _priceCalculationService.GetSubTotal(sci, true, out shoppingCartItemDiscountBase, out scDiscounts), out taxRate);
                    decimal shoppingCartItemSubTotalWithDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartItemSubTotalWithDiscountBase, _workContext.WorkingCurrency);
                    cartItemModel.SubTotal = _priceFormatter.FormatPrice(shoppingCartItemSubTotalWithDiscount);

                    //display an applied discount amount
                    if (shoppingCartItemDiscountBase > decimal.Zero)
                    {
                        shoppingCartItemDiscountBase = _taxService.GetProductPrice(sci.Product, shoppingCartItemDiscountBase, out taxRate);
                        if (shoppingCartItemDiscountBase > decimal.Zero)
                        {
                            decimal shoppingCartItemDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartItemDiscountBase, _workContext.WorkingCurrency);
                            cartItemModel.Discount = _priceFormatter.FormatPrice(shoppingCartItemDiscount);
                        }
                    }
                }

                //picture
                if (_shoppingCartSettings.ShowProductImagesOnShoppingCart)
                {
                    cartItemModel.Picture = PrepareCartItemPictureModel(sci,
                        _mediaSettings.CartThumbPictureSize, true, cartItemModel.ProductName);
                }

                //item warnings
                var itemWarnings = _shoppingCartService.GetShoppingCartItemWarnings(
                    _workContext.CurrentCustomer,
                    sci.ShoppingCartType,
                    sci.Product,
                    sci.StoreId,
                    sci.AttributesXml,
                    sci.CustomerEnteredPrice,
                    sci.RentalStartDateUtc,
                    sci.RentalEndDateUtc,
                    sci.Quantity,
                    false);
                foreach (var warning in itemWarnings)
                {
                    cartItemModel.Warnings.Add(warning);
                }

                model.Items.Add(cartItemModel);
            }

            #endregion
        }

        [NonAction]
        protected virtual MiniShoppingCartModel PrepareMiniShoppingCartModel()
        {
            var model = new MiniShoppingCartModel
            {
                ShowProductImages = _shoppingCartSettings.ShowProductImagesInMiniShoppingCart,
                //let's always display it
                DisplayShoppingCartButton = true,
                CurrentCustomerIsGuest = _workContext.CurrentCustomer.IsGuest(),
                AnonymousCheckoutAllowed = _orderSettings.AnonymousCheckoutAllowed,
            };


            //performance optimization (use "HasShoppingCartItems" property)
            if (_workContext.CurrentCustomer.HasShoppingCartItems)
            {
                var cart = _workContext.CurrentCustomer.ShoppingCartItems
                    .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                    .LimitPerStore(_storeContext.CurrentStore.Id)
                    .ToList();
                model.TotalProducts = cart.GetTotalProducts();
                if (cart.Any())
                {
                    //subtotal
                    decimal orderSubTotalDiscountAmountBase;
                    List<Discount> orderSubTotalAppliedDiscounts;
                    decimal subTotalWithoutDiscountBase;
                    decimal subTotalWithDiscountBase;
                    var subTotalIncludingTax = _workContext.TaxDisplayType == TaxDisplayType.IncludingTax && !_taxSettings.ForceTaxExclusionFromOrderSubtotal;
                    _orderTotalCalculationService.GetShoppingCartSubTotal(cart, subTotalIncludingTax,
                        out orderSubTotalDiscountAmountBase, out orderSubTotalAppliedDiscounts,
                        out subTotalWithoutDiscountBase, out subTotalWithDiscountBase);
                    decimal subtotalBase = subTotalWithoutDiscountBase;
                    decimal subtotal = _currencyService.ConvertFromPrimaryStoreCurrency(subtotalBase, _workContext.WorkingCurrency);
                    model.SubTotal = _priceFormatter.FormatPrice(subtotal, false, _workContext.WorkingCurrency, _workContext.WorkingLanguage, subTotalIncludingTax);

                    var requiresShipping = cart.RequiresShipping();
                    //a customer should visit the shopping cart page (hide checkout button) before going to checkout if:
                    //1. "terms of service" are enabled
                    //2. min order sub-total is OK
                    //3. we have at least one checkout attribute
                    var checkoutAttributesExistCacheKey = string.Format(ModelCacheEventConsumer.CHECKOUTATTRIBUTES_EXIST_KEY,
                        _storeContext.CurrentStore.Id, requiresShipping);
                    var checkoutAttributesExist = _cacheManager.Get(checkoutAttributesExistCacheKey,
                        () =>
                        {
                            var checkoutAttributes = _checkoutAttributeService.GetAllCheckoutAttributes(_storeContext.CurrentStore.Id, !requiresShipping);
                            return checkoutAttributes.Any();
                        });

                    bool minOrderSubtotalAmountOk = _orderProcessingService.ValidateMinOrderSubtotalAmount(cart);
                    bool downloadableProductsRequireRegistration =
                        _customerSettings.RequireRegistrationForDownloadableProducts && cart.Any(sci => sci.Product.IsDownload);

                    model.DisplayCheckoutButton = !_orderSettings.TermsOfServiceOnShoppingCartPage &&
                        minOrderSubtotalAmountOk &&
 !checkoutAttributesExist &&
                        !(downloadableProductsRequireRegistration
                            && _workContext.CurrentCustomer.IsGuest());

                    //products. sort descending (recently added products)
                    foreach (var sci in cart
                        .OrderByDescending(x => x.Id)
                        .Take(_shoppingCartSettings.MiniShoppingCartProductNumber)
                        .ToList())
                    {
                        var cartItemModel = new MiniShoppingCartModel.ShoppingCartItemModel
                        {
                            Id = sci.Id,
                            ProductId = sci.Product.Id,
                            ProductName = sci.Product.GetLocalized(x => x.Name),
                            ProductSeName = sci.Product.GetSeName(),
                            Quantity = sci.Quantity,
                            AttributeInfo = _productAttributeFormatter.FormatAttributes(sci.Product, sci.AttributesXml)
                        };

                        //unit prices
                        if (sci.Product.CallForPrice)
                        {
                            cartItemModel.UnitPrice = _localizationService.GetResource("Products.CallForPrice");
                        }
                        else
                        {
                            decimal taxRate;
                            decimal shoppingCartUnitPriceWithDiscountBase = _taxService.GetProductPrice(sci.Product, _priceCalculationService.GetUnitPrice(sci), out taxRate);
                            decimal shoppingCartUnitPriceWithDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartUnitPriceWithDiscountBase, _workContext.WorkingCurrency);
                            cartItemModel.UnitPrice = _priceFormatter.FormatPrice(shoppingCartUnitPriceWithDiscount);
                        }

                        //picture
                        if (_shoppingCartSettings.ShowProductImagesInMiniShoppingCart)
                        {
                            cartItemModel.Picture = PrepareCartItemPictureModel(sci,
                                _mediaSettings.MiniCartThumbPictureSize, true, cartItemModel.ProductName);
                        }

                        model.Items.Add(cartItemModel);
                    }
                }
            }

            return model;
        }

        [NonAction]
        protected virtual OrderTotalsModel PrepareOrderTotalsModel(IList<ShoppingCartItem> cart, bool isEditable)
        {
            var model = new OrderTotalsModel();
            model.IsEditable = isEditable;

            if (cart.Any())
            {
                //subtotal
                decimal orderSubTotalDiscountAmountBase;
                List<Discount> orderSubTotalAppliedDiscounts;
                decimal subTotalWithoutDiscountBase;
                decimal subTotalWithDiscountBase;
                var subTotalIncludingTax = _workContext.TaxDisplayType == TaxDisplayType.IncludingTax && !_taxSettings.ForceTaxExclusionFromOrderSubtotal;
                _orderTotalCalculationService.GetShoppingCartSubTotal(cart, subTotalIncludingTax,
                    out orderSubTotalDiscountAmountBase, out orderSubTotalAppliedDiscounts,
                    out subTotalWithoutDiscountBase, out subTotalWithDiscountBase);
                decimal subtotalBase = subTotalWithoutDiscountBase;
                decimal subtotal = _currencyService.ConvertFromPrimaryStoreCurrency(subtotalBase, _workContext.WorkingCurrency);
                model.SubTotal = _priceFormatter.FormatPrice(subtotal, true, _workContext.WorkingCurrency, _workContext.WorkingLanguage, subTotalIncludingTax);

                if (orderSubTotalDiscountAmountBase > decimal.Zero)
                {
                    decimal orderSubTotalDiscountAmount = _currencyService.ConvertFromPrimaryStoreCurrency(orderSubTotalDiscountAmountBase, _workContext.WorkingCurrency);
                    //model.SubTotalDiscount = _priceFormatter.FormatPrice(-orderSubTotalDiscountAmount, true, _workContext.WorkingCurrency, _workContext.WorkingLanguage, subTotalIncludingTax);
                    model.SubTotalDiscount = _priceFormatter.FormatPrice(-Convert.ToDecimal(_httpContext.Session["discountedAmountBySubcategory"]), true, _workContext.WorkingCurrency, _workContext.WorkingLanguage, subTotalIncludingTax);
                    //model.SubTotalDiscount = Convert.ToString("$" + _httpContext.Session["discountedAmountBySubcategory"]);
                    model.AllowRemovingSubTotalDiscount = model.IsEditable &&
                        orderSubTotalAppliedDiscounts.Any(d => d.RequiresCouponCode && !String.IsNullOrEmpty(d.CouponCode));
                }

                //shipping info
                model.RequiresShipping = cart.RequiresShipping();

                if (model.RequiresShipping && _storeContext.CurrentStore.IsTieredShippingEnabled == false)
                {
                    decimal? shoppingCartShippingBase = _orderTotalCalculationService.GetShoppingCartShippingTotal(cart);
                    if (shoppingCartShippingBase.HasValue)
                    {
                        decimal shoppingCartShipping = _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartShippingBase.Value, _workContext.WorkingCurrency);
                        model.Shipping = _priceFormatter.FormatShippingPrice(shoppingCartShipping, true);

                        //selected shipping method
                        var shippingOption = _workContext.CurrentCustomer.GetAttribute<ShippingOption>(SystemCustomerAttributeNames.SelectedShippingOption, _storeContext.CurrentStore.Id);
                        if (shippingOption != null)
                        {
                            model.SelectedShippingMethod = shippingOption.Name;
                        }
                    }
                    else
                    {
                        model.Shipping = _priceFormatter.FormatShippingPrice(0, true);
                    }
                }

                if (_storeContext.CurrentStore.IsTieredShippingEnabled == true)
                {
                    decimal shoppingCartShippingBase = 0;
                    foreach (ShoppingCartItem sci in cart)
                    {
                        shoppingCartShippingBase += sci.IsTieredShippingEnabled == true && sci.IsFirstCartItem == true ? sci.FlatShipping : 0;
                    }
                    if (shoppingCartShippingBase > 0)
                    {
                        decimal shoppingCartShipping = _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartShippingBase, _workContext.WorkingCurrency);
                        model.Shipping = _priceFormatter.FormatShippingPrice(shoppingCartShipping, true);

                        //selected shipping method
                        var shippingOption = _workContext.CurrentCustomer.GetAttribute<ShippingOption>(SystemCustomerAttributeNames.SelectedShippingOption, _storeContext.CurrentStore.Id);
                        if (shippingOption != null)
                        {
                            model.SelectedShippingMethod = "Tiered Shipping";
                        }
                    }
                    else
                    {
                        model.Shipping = _priceFormatter.FormatShippingPrice(0, true);
                    }
                }

                //payment method fee
                var paymentMethodSystemName = _workContext.CurrentCustomer.GetAttribute<string>(
                    SystemCustomerAttributeNames.SelectedPaymentMethod, _storeContext.CurrentStore.Id);
                decimal paymentMethodAdditionalFee = _paymentService.GetAdditionalHandlingFee(cart, paymentMethodSystemName);
                decimal paymentMethodAdditionalFeeWithTaxBase = _taxService.GetPaymentMethodAdditionalFee(paymentMethodAdditionalFee, _workContext.CurrentCustomer);
                if (paymentMethodAdditionalFeeWithTaxBase > decimal.Zero)
                {
                    decimal paymentMethodAdditionalFeeWithTax = _currencyService.ConvertFromPrimaryStoreCurrency(paymentMethodAdditionalFeeWithTaxBase, _workContext.WorkingCurrency);
                    model.PaymentMethodAdditionalFee = _priceFormatter.FormatPaymentMethodAdditionalFee(paymentMethodAdditionalFeeWithTax, true);
                }

                //tax
                bool displayTax = true;
                bool displayTaxRates = true;
                if (_taxSettings.HideTaxInOrderSummary && _workContext.TaxDisplayType == TaxDisplayType.IncludingTax)
                {
                    displayTax = false;
                    displayTaxRates = false;
                }
                else
                {
                    SortedDictionary<decimal, decimal> taxRates;
                    decimal shoppingCartTaxBase = _orderTotalCalculationService.GetTaxTotal(cart, out taxRates);
                    decimal shoppingCartTax = _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartTaxBase, _workContext.WorkingCurrency);

                    if (shoppingCartTaxBase == 0 && _taxSettings.HideZeroTax)
                    {
                        displayTax = false;
                        displayTaxRates = false;
                    }
                    else
                    {
                        displayTaxRates = _taxSettings.DisplayTaxRates && taxRates.Any();
                        displayTax = !displayTaxRates;

                        model.Tax = _priceFormatter.FormatPrice(shoppingCartTax, true, false);
                        foreach (var tr in taxRates)
                        {
                            model.TaxRates.Add(new OrderTotalsModel.TaxRate
                            {
                                Rate = _priceFormatter.FormatTaxRate(tr.Key),
                                Value = _priceFormatter.FormatPrice(_currencyService.ConvertFromPrimaryStoreCurrency(tr.Value, _workContext.WorkingCurrency), true, false),
                            });
                        }
                    }
                }
                model.DisplayTaxRates = displayTaxRates;
                model.DisplayTax = displayTax;

                model.CurrentTaxProvider = _taxService.LoadActiveTaxProvider(_storeContext.CurrentStore.Id).PluginDescriptor.SystemName;

                //total
                decimal orderTotalDiscountAmountBase;
                List<Discount> orderTotalAppliedDiscounts;
                List<AppliedGiftCard> appliedGiftCards;
                int redeemedRewardPoints;
                decimal redeemedRewardPointsAmount;
                decimal? shoppingCartTotalBase = _orderTotalCalculationService.GetShoppingCartTotal(cart,
                    out orderTotalDiscountAmountBase, out orderTotalAppliedDiscounts,
                    out appliedGiftCards, out redeemedRewardPoints, out redeemedRewardPointsAmount);
                if (shoppingCartTotalBase.HasValue)
                {
                    decimal shoppingCartTotal = _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartTotalBase.Value, _workContext.WorkingCurrency);
                    model.OrderTotal = _priceFormatter.FormatPrice(shoppingCartTotal, true, false);
                }

                //discount
                if (orderTotalDiscountAmountBase > decimal.Zero)
                {
                    decimal orderTotalDiscountAmount = _currencyService.ConvertFromPrimaryStoreCurrency(orderTotalDiscountAmountBase, _workContext.WorkingCurrency);
                    model.OrderTotalDiscount = _priceFormatter.FormatPrice(-orderTotalDiscountAmount, true, false);
                    model.AllowRemovingOrderTotalDiscount = model.IsEditable &&
                        orderTotalAppliedDiscounts.Any(d => d.RequiresCouponCode && !String.IsNullOrEmpty(d.CouponCode));
                }

                //gift cards
                if (appliedGiftCards != null && appliedGiftCards.Any())
                {
                    foreach (var appliedGiftCard in appliedGiftCards)
                    {
                        var gcModel = new OrderTotalsModel.GiftCard
                        {
                            Id = appliedGiftCard.GiftCard.Id,
                            CouponCode = appliedGiftCard.GiftCard.GiftCardCouponCode,
                        };
                        decimal amountCanBeUsed = _currencyService.ConvertFromPrimaryStoreCurrency(appliedGiftCard.AmountCanBeUsed, _workContext.WorkingCurrency);
                        gcModel.Amount = _priceFormatter.FormatPrice(-amountCanBeUsed, true, false);

                        decimal remainingAmountBase = appliedGiftCard.GiftCard.GetGiftCardRemainingAmount() - appliedGiftCard.AmountCanBeUsed;
                        decimal remainingAmount = _currencyService.ConvertFromPrimaryStoreCurrency(remainingAmountBase, _workContext.WorkingCurrency);
                        gcModel.Remaining = _priceFormatter.FormatPrice(remainingAmount, true, false);

                        model.GiftCards.Add(gcModel);
                    }
                }

                //reward points to be spent (redeemed)
                if (redeemedRewardPointsAmount > decimal.Zero)
                {
                    decimal redeemedRewardPointsAmountInCustomerCurrency = _currencyService.ConvertFromPrimaryStoreCurrency(redeemedRewardPointsAmount, _workContext.WorkingCurrency);
                    model.RedeemedRewardPoints = redeemedRewardPoints;
                    model.RedeemedRewardPointsAmount = _priceFormatter.FormatPrice(-redeemedRewardPointsAmountInCustomerCurrency, true, false);
                }

                //reward points to be earned
                if (_rewardPointsSettings.Enabled &&
                    _rewardPointsSettings.DisplayHowMuchWillBeEarned &&
                    shoppingCartTotalBase.HasValue)
                {
                    model.WillEarnRewardPoints = _orderTotalCalculationService
                        .CalculateRewardPoints(_workContext.CurrentCustomer, shoppingCartTotalBase.Value);
                }

            }

            return model;
        }

        [NonAction]
        protected virtual void ParseAndSaveCheckoutAttributes(List<ShoppingCartItem> cart, FormCollection form)
        {
            if (cart == null)
            {
                throw new ArgumentNullException("cart");
            }

            if (form == null)
            {
                throw new ArgumentNullException("form");
            }

            string attributesXml = "";
            var checkoutAttributes = _checkoutAttributeService.GetAllCheckoutAttributes(_storeContext.CurrentStore.Id, !cart.RequiresShipping());
            foreach (var attribute in checkoutAttributes)
            {
                string controlId = string.Format("checkout_attribute_{0}", attribute.Id);
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
                                {
                                    attributesXml = _checkoutAttributeParser.AddCheckoutAttribute(attributesXml,
                                        attribute, selectedAttributeId.ToString());
                                }
                            }
                        }
                        break;
                    case AttributeControlType.Checkboxes:
                        {
                            var cblAttributes = form[controlId];
                            if (!String.IsNullOrEmpty(cblAttributes))
                            {
                                foreach (var item in cblAttributes.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                                {
                                    int selectedAttributeId = int.Parse(item);
                                    if (selectedAttributeId > 0)
                                    {
                                        attributesXml = _checkoutAttributeParser.AddCheckoutAttribute(attributesXml,
                                            attribute, selectedAttributeId.ToString());
                                    }
                                }
                            }
                        }
                        break;
                    case AttributeControlType.ReadonlyCheckboxes:
                        {
                            //load read-only (already server-side selected) values
                            var attributeValues = _checkoutAttributeService.GetCheckoutAttributeValues(attribute.Id);
                            foreach (var selectedAttributeId in attributeValues
                                .Where(v => v.IsPreSelected)
                                .Select(v => v.Id)
                                .ToList())
                            {
                                attributesXml = _checkoutAttributeParser.AddCheckoutAttribute(attributesXml,
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
                                attributesXml = _checkoutAttributeParser.AddCheckoutAttribute(attributesXml,
                                    attribute, enteredText);
                            }
                        }
                        break;
                    case AttributeControlType.Datepicker:
                        {
                            var date = form[controlId + "_day"];
                            var month = form[controlId + "_month"];
                            var year = form[controlId + "_year"];
                            DateTime? selectedDate = null;
                            try
                            {
                                selectedDate = new DateTime(Int32.Parse(year), Int32.Parse(month), Int32.Parse(date));
                            }
                            catch { }
                            if (selectedDate.HasValue)
                            {
                                attributesXml = _checkoutAttributeParser.AddCheckoutAttribute(attributesXml,
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
                                attributesXml = _checkoutAttributeParser.AddCheckoutAttribute(attributesXml,
                                           attribute, download.DownloadGuid.ToString());
                            }
                        }
                        break;
                    default:
                        break;
                }
            }

            //validate conditional attributes (if specified)
            foreach (var attribute in checkoutAttributes)
            {
                var conditionMet = _checkoutAttributeParser.IsConditionMet(attribute, attributesXml);
                if (conditionMet.HasValue && !conditionMet.Value)
                {
                    attributesXml = _checkoutAttributeParser.RemoveCheckoutAttribute(attributesXml, attribute);
                }
            }

            //save checkout attributes
            _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer, SystemCustomerAttributeNames.CheckoutAttributes, attributesXml, _storeContext.CurrentStore.Id);
        }

        /// <summary>
        /// Parse product attributes on the product details page
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="form">Form</param>
        /// <returns>Parsed attributes</returns>
        [NonAction]
        protected virtual string ParseProductAttributes(Product product, FormCollection form)
        {
            string attributesXml = "";

            #region Product attributes

            var productAttributes = _productAttributeService.GetProductAttributeMappingsByProductId(product.Id);
            foreach (var attribute in productAttributes)
            {
                string controlId = string.Format("product_attribute_{0}", attribute.Id);
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
                                {
                                    attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                        attribute, selectedAttributeId.ToString());
                                }
                            }
                        }
                        break;
                    case AttributeControlType.DatePickerCalendarControl:
                        {
                            var reservedDate = form["reservationDate"];
                            var day = "";
                            var month = "";
                            var year = "";
                            if (!String.IsNullOrEmpty(reservedDate))
                            {
                                var reservedDateParts = reservedDate.Split('-');
                                day = reservedDate.Split('-')[2];
                                month = reservedDate.Split('-')[1];
                                year = reservedDate.Split('-')[0];
                            }
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
                    case AttributeControlType.Checkboxes:
                        {
                            var ctrlAttributes = form[controlId];
                            if (!String.IsNullOrEmpty(ctrlAttributes))
                            {
                                foreach (var item in ctrlAttributes.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                                {
                                    int selectedAttributeId = int.Parse(item);
                                    if (selectedAttributeId > 0)
                                    {
                                        attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                            attribute, selectedAttributeId.ToString());
                                    }
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

            #region Gift cards

            if (product.IsGiftCard)
            {
                string recipientName = "";
                string recipientEmail = "";
                string senderName = "";
                string senderEmail = "";
                string giftCardMessage = "";
                foreach (string formKey in form.AllKeys)
                {
                    if (formKey.Equals(string.Format("giftcard_{0}.RecipientName", product.Id), StringComparison.InvariantCultureIgnoreCase))
                    {
                        recipientName = form[formKey];
                        continue;
                    }
                    if (formKey.Equals(string.Format("giftcard_{0}.RecipientEmail", product.Id), StringComparison.InvariantCultureIgnoreCase))
                    {
                        recipientEmail = form[formKey];
                        continue;
                    }
                    if (formKey.Equals(string.Format("giftcard_{0}.SenderName", product.Id), StringComparison.InvariantCultureIgnoreCase))
                    {
                        senderName = form[formKey];
                        continue;
                    }
                    if (formKey.Equals(string.Format("giftcard_{0}.SenderEmail", product.Id), StringComparison.InvariantCultureIgnoreCase))
                    {
                        senderEmail = form[formKey];
                        continue;
                    }
                    if (formKey.Equals(string.Format("giftcard_{0}.Message", product.Id), StringComparison.InvariantCultureIgnoreCase))
                    {
                        giftCardMessage = form[formKey];
                        continue;
                    }
                }

                attributesXml = _productAttributeParser.AddGiftCardAttribute(attributesXml,
                    recipientName, recipientEmail, senderName, senderEmail, giftCardMessage);
            }

            #endregion

            #region NU-16
            var mealPlanRecipientAcctNum = string.Empty;
            var mealPlanRecipientAddress = string.Empty;
            var mealPlanRecipientEmail = string.Empty;
            var mealPlanRecipientName = string.Empty;
            var mealPlanRecipientPhone = string.Empty;

            if (product.IsMealPlan)
            {
                foreach (string formKey in form.AllKeys)
                {
                    if (formKey.Equals(string.Format("mealplan_{0}.MealPlanRecipientAcctNum", product.Id), StringComparison.InvariantCultureIgnoreCase))
                    {
                        mealPlanRecipientAcctNum = form[formKey];
                        continue;
                    }
                    if (formKey.Equals(string.Format("mealplan_{0}.MealPlanRecipientAddress", product.Id), StringComparison.InvariantCultureIgnoreCase))
                    {
                        mealPlanRecipientAddress = form[formKey];
                        continue;
                    }
                    if (formKey.Equals(string.Format("mealplan_{0}.MealPlanRecipientEmail", product.Id), StringComparison.InvariantCultureIgnoreCase))
                    {
                        mealPlanRecipientEmail = form[formKey];
                        continue;
                    }
                    if (formKey.Equals(string.Format("mealplan_{0}.MealPlanRecipientName", product.Id), StringComparison.InvariantCultureIgnoreCase))
                    {
                        mealPlanRecipientName = form[formKey];
                        continue;
                    }
                    if (formKey.Equals(string.Format("mealplan_{0}.MealPlanRecipientPhone", product.Id), StringComparison.InvariantCultureIgnoreCase))
                    {
                        mealPlanRecipientPhone = form[formKey];
                        continue;
                    }
                }

                attributesXml = _productAttributeParser.AddMealPlanAttribute(attributesXml, mealPlanRecipientAcctNum, mealPlanRecipientAddress, mealPlanRecipientEmail, mealPlanRecipientName, mealPlanRecipientPhone);
            }

            #endregion

            #region NU-17
            if (product.IsDonation)
            {
                var donorFirstName = string.Empty;
                var donorLastName = string.Empty;
                var donorCompany = string.Empty;
                var donorAddress = string.Empty;
                var donorAddress2 = string.Empty;
                var donorCity = string.Empty;
                var donorState = string.Empty;
                var donorZip = string.Empty;
                var donorPhone = string.Empty;
                var donorCountry = string.Empty;
                var comments = string.Empty;
                var notificationEmail = string.Empty;
                var onBehalfOfFullName = string.Empty;
                var includeGiftAmount = false;
                foreach (string formKey in form.AllKeys)
                {
                    if (formKey.Equals(string.Format("donation_{0}.DonorFirstName", product.Id), StringComparison.InvariantCultureIgnoreCase))
                    {
                        donorFirstName = form[formKey];
                        continue;
                    }
                    if (formKey.Equals(string.Format("donation_{0}.DonorLastName", product.Id), StringComparison.InvariantCultureIgnoreCase))
                    {
                        donorLastName = form[formKey];
                        continue;
                    }
                    if (formKey.Equals(string.Format("donation_{0}.DonorCompany", product.Id), StringComparison.InvariantCultureIgnoreCase))
                    {
                        donorCompany = form[formKey];
                        continue;
                    }
                    if (formKey.Equals(string.Format("donation_{0}.DonorAddress", product.Id), StringComparison.InvariantCultureIgnoreCase))
                    {
                        donorAddress = form[formKey];
                        continue;
                    }
                    if (formKey.Equals(string.Format("donation_{0}.DonorAddress2", product.Id), StringComparison.InvariantCultureIgnoreCase))
                    {
                        donorAddress2 = form[formKey];
                        continue;
                    }
                    if (formKey.Equals(string.Format("donation_{0}.DonorCity", product.Id), StringComparison.InvariantCultureIgnoreCase))
                    {
                        donorCity = form[formKey];
                        continue;
                    }
                    if (formKey.Equals(string.Format("donation_{0}.DonorState", product.Id), StringComparison.InvariantCultureIgnoreCase))
                    {
                        donorState = form[formKey];
                        continue;
                    }
                    if (formKey.Equals(string.Format("donation_{0}.DonorZip", product.Id), StringComparison.InvariantCultureIgnoreCase))
                    {
                        donorZip = form[formKey];
                        continue;
                    }
                    if (formKey.Equals(string.Format("donation_{0}.DonorPhone", product.Id), StringComparison.InvariantCultureIgnoreCase))
                    {
                        donorPhone = form[formKey];
                        continue;
                    }
                    if (formKey.Equals(string.Format("donation_{0}.DonorCountry", product.Id), StringComparison.InvariantCultureIgnoreCase))
                    {
                        donorCountry = form[formKey];
                        continue;
                    }
                    if (formKey.Equals(string.Format("donation_{0}.DonorCity", product.Id), StringComparison.InvariantCultureIgnoreCase))
                    {
                        donorCity = form[formKey];
                        continue;
                    }
                    if (formKey.Equals(string.Format("donation_{0}.Comments", product.Id), StringComparison.InvariantCultureIgnoreCase))
                    {
                        comments = form[formKey];
                        continue;
                    }
                    if (formKey.Equals(string.Format("donation_{0}.NotificationEmail", product.Id), StringComparison.InvariantCultureIgnoreCase))
                    {
                        notificationEmail = form[formKey];
                        continue;
                    }
                    if (formKey.Equals(string.Format("donation_{0}.OnBehalfOfFullName", product.Id), StringComparison.InvariantCultureIgnoreCase))
                    {
                        onBehalfOfFullName = form[formKey];
                        continue;
                    }

                    if (formKey.Equals(string.Format("donation_{0}.IncludeGiftAmount", product.Id), StringComparison.InvariantCultureIgnoreCase))
                    {

                        bool isChecked = false;
                        Boolean.TryParse(Request.Form.GetValues(formKey)[0], out isChecked);

                        includeGiftAmount = isChecked;
                        continue;
                    }
                }

                attributesXml = _productAttributeParser.AddDonationAttribute(attributesXml, donorFirstName,
             donorLastName,
             donorAddress,
             donorAddress2,
             donorCity,
             donorState,
             donorZip,
             donorPhone,
             donorCountry,
             comments,
             notificationEmail,
             onBehalfOfFullName, includeGiftAmount, donorCompany);
            }
            #endregion
            return attributesXml;
        }

        /// <summary>
        /// Parse product rental dates on the product details page
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="form">Form</param>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        [NonAction]
        protected virtual void ParseRentalDates(Product product, FormCollection form,
            out DateTime? startDate, out DateTime? endDate)
        {
            startDate = null;
            endDate = null;

            string startControlId = string.Format("rental_start_date_{0}", product.Id);
            string endControlId = string.Format("rental_end_date_{0}", product.Id);
            var ctrlStartDate = form[startControlId];
            var ctrlEndDate = form[endControlId];
            try
            {
                //currenly we support only this format (as in the \Views\Product\_RentalInfo.cshtml file)
                const string datePickerFormat = "MM/dd/yyyy";
                startDate = DateTime.ParseExact(ctrlStartDate, datePickerFormat, CultureInfo.InvariantCulture);
                endDate = DateTime.ParseExact(ctrlEndDate, datePickerFormat, CultureInfo.InvariantCulture);
            }
            catch
            {
            }
        }

        #endregion

        #region Shopping cart

        //add product to cart using AJAX
        //currently we use this method on catalog pages (category/manufacturer/etc)
        [HttpPost]
        public ActionResult AddProductToCart_Catalog(int productId, int shoppingCartTypeId,
            int quantity, bool forceredirection = false)
        {
            var cartType = (ShoppingCartType)shoppingCartTypeId;

            var product = _productService.GetProductById(productId);
            if (product == null)
            {
                //no product found
                return Json(new
                {
                    success = false,
                    message = "No product found with the specified ID"
                });
            }

            //we can add only simple products
            if (product.ProductType != ProductType.SimpleProduct)
            {
                return Json(new
                {
                    redirect = Url.RouteUrl("Product", new { SeName = product.GetSeName() }),
                });
            }

            //products with "minimum order quantity" more than a specified qty
            if (product.OrderMinimumQuantity > quantity)
            {
                //we cannot add to the cart such products from category pages
                //it can confuse customers. That's why we redirect customers to the product details page
                return Json(new
                {
                    redirect = Url.RouteUrl("Product", new { SeName = product.GetSeName() }),
                });
            }

            if (product.CustomerEntersPrice)
            {
                //cannot be added to the cart (requires a customer to enter price)
                return Json(new
                {
                    redirect = Url.RouteUrl("Product", new { SeName = product.GetSeName() }),
                });
            }

            if (product.IsRental)
            {
                //rental products require start/end dates to be entered
                return Json(new
                {
                    redirect = Url.RouteUrl("Product", new { SeName = product.GetSeName() }),
                });
            }

            var allowedQuantities = product.ParseAllowedQuantities();
            if (allowedQuantities.Length == 0 || allowedQuantities.Length > 0)
            {
                //cannot be added to the cart (requires a customer to select a quantity from dropdownlist)
                return Json(new
                {
                    redirect = Url.RouteUrl("Product", new { SeName = product.GetSeName() }),
                });
            }

            if (product.ProductAttributeMappings.Any())
            {
                //product has some attributes. let a customer see them
                return Json(new
                {
                    redirect = Url.RouteUrl("Product", new { SeName = product.GetSeName() }),
                });
            }

            //get standard warnings without attribute validations
            //first, try to find existing shopping cart item
            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == cartType)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();
            var shoppingCartItem = _shoppingCartService.FindShoppingCartItemInTheCart(cart, cartType, product);
            //if we already have the same product in the cart, then use the total quantity to validate
            var quantityToValidate = shoppingCartItem != null ? shoppingCartItem.Quantity + quantity : quantity;
            var addToCartWarnings = _shoppingCartService
                .GetShoppingCartItemWarnings(_workContext.CurrentCustomer, cartType,
                product, _storeContext.CurrentStore.Id, string.Empty,
                decimal.Zero, null, null, quantityToValidate, false, true, false, false, false);
            if (addToCartWarnings.Any())
            {
                //cannot be added to the cart
                //let's display standard warnings
                return Json(new
                {
                    success = false,
                    message = addToCartWarnings.ToArray()
                });
            }

            //now let's try adding product to the cart (now including product attribute validation, etc)
            addToCartWarnings = _shoppingCartService.AddToCart(
                customer: _workContext.CurrentCustomer,
                product: product,
                shoppingCartType: cartType,
                storeId: _storeContext.CurrentStore.Id,
                attributesXml: null,
                customerEnteredPrice: 0,
                rentalStartDate: null,
                rentalEndDate: null,
                quantity: quantity,
                automaticallyAddRequiredProductsIfEnabled: true);
            if (addToCartWarnings.Any())
            {
                //cannot be added to the cart
                //but we do not display attribute and gift card warnings here. let's do it on the product details page
                return Json(new
                {
                    redirect = Url.RouteUrl("Product", new { SeName = product.GetSeName() }),
                });
            }

            //added to the cart/wishlist
            switch (cartType)
            {
                case ShoppingCartType.Wishlist:
                    {
                        //activity log
                        _customerActivityService.InsertActivity("PublicStore.AddToWishlist", _localizationService.GetResource("ActivityLog.PublicStore.AddToWishlist"), product.Name);

                        if (_shoppingCartSettings.DisplayWishlistAfterAddingProduct || forceredirection)
                        {
                            //redirect to the wishlist page
                            return Json(new
                            {
                                redirect = Url.RouteUrl("Wishlist"),
                            });
                        }

                        //display notification message and update appropriate blocks
                        var updatetopwishlistsectionhtml = string.Format(_localizationService.GetResource("Wishlist.HeaderQuantity"),
                        _workContext.CurrentCustomer.ShoppingCartItems
                        .Where(sci => sci.ShoppingCartType == ShoppingCartType.Wishlist)
                        .LimitPerStore(_storeContext.CurrentStore.Id)
                        .ToList()
                        .GetTotalProducts());
                        return Json(new
                        {
                            success = true,
                            message = string.Format(_localizationService.GetResource("Products.ProductHasBeenAddedToTheWishlist.Link"), Url.RouteUrl("Wishlist")),
                            updatetopwishlistsectionhtml = updatetopwishlistsectionhtml,
                        });
                    }
                case ShoppingCartType.ShoppingCart:
                default:
                    {
                        //activity log
                        _customerActivityService.InsertActivity("PublicStore.AddToShoppingCart", _localizationService.GetResource("ActivityLog.PublicStore.AddToShoppingCart"), product.Name);

                        if (_shoppingCartSettings.DisplayCartAfterAddingProduct || forceredirection)
                        {
                            //redirect to the shopping cart page
                            return Json(new
                            {
                                redirect = Url.RouteUrl("ShoppingCart"),
                            });
                        }

                        //display notification message and update appropriate blocks
                        var updatetopcartsectionhtml = string.Format(_localizationService.GetResource("ShoppingCart.HeaderQuantity"),
                        _workContext.CurrentCustomer.ShoppingCartItems
                        .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                        .LimitPerStore(_storeContext.CurrentStore.Id)
                        .ToList()
                        .GetTotalProducts());

                        var updateflyoutcartsectionhtml = _shoppingCartSettings.MiniShoppingCartEnabled
                            ? this.RenderPartialViewToString("FlyoutShoppingCart", PrepareMiniShoppingCartModel())
                            : "";

                        return Json(new
                        {
                            success = true,
                            message = string.Format(_localizationService.GetResource("Products.ProductHasBeenAddedToTheCart.Link"), Url.RouteUrl("ShoppingCart")),
                            updatetopcartsectionhtml = updatetopcartsectionhtml,
                            updateflyoutcartsectionhtml = updateflyoutcartsectionhtml
                        });
                    }
            }
        }

        #region SODMYWAY-
        [HttpPost]
        public ActionResult AddProductToCart_List(List<string> productIdAndQuantites)
        {

            //List<string> productIdAndQuantitess = new List<string>();


            foreach (string productIdAndQuantity in productIdAndQuantites)
            {
                int productId = -1;
                int quantity = -1;


                var elements = productIdAndQuantity.Split('-');
                if (elements.Count() < 2)
                {
                    return RedirectToAction("Index", "Home");
                }

                if (!int.TryParse(elements[0], out productId))
                {
                    return RedirectToAction("Index", "Home");
                }

                if (!int.TryParse(elements[1], out quantity))
                {
                    return RedirectToAction("Index", "Home");
                }

                var cartType = (ShoppingCartType.ShoppingCart);//shoppingCartTypeId;

                var product = _productService.GetProductById(productId);
                if (product == null)
                {
                    //no product found
                    return Json(new
                    {
                        success = false,
                        message = "No product found with the specified ID"
                    });
                }

                //we can add only simple products
                if (product.ProductType != ProductType.SimpleProduct)
                {
                    return Json(new
                    {
                        redirect = Url.RouteUrl("Product", new { SeName = product.GetSeName() }),
                    });
                }

                //products with "minimum order quantity" more than a specified qty
                if (product.OrderMinimumQuantity > quantity)
                {
                    //we cannot add to the cart such products from category pages
                    //it can confuse customers. That's why we redirect customers to the product details page
                    return Json(new
                    {
                        redirect = Url.RouteUrl("Product", new { SeName = product.GetSeName() }),
                    });
                }

                if (product.CustomerEntersPrice)
                {
                    //cannot be added to the cart (requires a customer to enter price)
                    return Json(new
                    {
                        redirect = Url.RouteUrl("Product", new { SeName = product.GetSeName() }),
                    });
                }

                if (product.IsRental)
                {
                    //rental products require start/end dates to be entered
                    return Json(new
                    {
                        redirect = Url.RouteUrl("Product", new { SeName = product.GetSeName() }),
                    });
                }

                var allowedQuantities = product.ParseAllowedQuantities();
                // if (allowedQuantities.Length > 0)
                // {
                //cannot be added to the cart (requires a customer to select a quantity from dropdownlist)
                //   return Json(new
                //   {
                //     redirect = Url.RouteUrl("Product", new { SeName = product.GetSeName() }),
                // });
                // }

                if (product.ProductAttributeMappings.Count > 0)
                {
                    //product has some attributes. let a customer see them
                    return Json(new
                    {
                        redirect = Url.RouteUrl("Product", new { SeName = product.GetSeName() }),
                    });
                }

                //get standard warnings without attribute validations
                //first, try to find existing shopping cart item
                var cart = _workContext.CurrentCustomer.ShoppingCartItems
                    .Where(sci => sci.ShoppingCartType == cartType)
                    .LimitPerStore(_storeContext.CurrentStore.Id)
                    .ToList();
                var shoppingCartItem = _shoppingCartService.FindShoppingCartItemInTheCart(cart, cartType, product);
                //if we already have the same product in the cart, then use the total quantity to validate
                var quantityToValidate = shoppingCartItem != null ? shoppingCartItem.Quantity + quantity : quantity;
                var addToCartWarnings = _shoppingCartService
                    .GetShoppingCartItemWarnings(_workContext.CurrentCustomer, cartType,
                    product, _storeContext.CurrentStore.Id, string.Empty,
                    decimal.Zero, null, null, quantityToValidate, false, true, false, false, false);
                if (addToCartWarnings.Count > 0)
                {
                    //cannot be added to the cart
                    //let's display standard warnings
                    return Json(new
                    {
                        success = false,
                        message = addToCartWarnings.ToArray()
                    });
                }

                //now let's try adding product to the cart (now including product attribute validation, etc)
                addToCartWarnings = _shoppingCartService.AddToCart(
                     customer: _workContext.CurrentCustomer,
                     product: product,
                     shoppingCartType: cartType,
                     storeId: _storeContext.CurrentStore.Id,
                     attributesXml: null,
                     customerEnteredPrice: 0,
                     rentalStartDate: null,
                     rentalEndDate: null,
                     quantity: quantity,
                     automaticallyAddRequiredProductsIfEnabled: true
                     );
                if (addToCartWarnings.Count > 0)
                {
                    //cannot be added to the cart
                    //but we do not display attribute and gift card warnings here. let's do it on the product details page
                    return Json(new
                    {
                        redirect = Url.RouteUrl("Product", new { SeName = product.GetSeName() }),
                    });
                }



            }


            var updatetopcartsectionhtml = string.Format(_localizationService.GetResource("ShoppingCart.HeaderQuantity"),
                            _workContext.CurrentCustomer.ShoppingCartItems
                            .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                            .LimitPerStore(_storeContext.CurrentStore.Id)
                            .ToList()
                            .GetTotalProducts());

            var updateflyoutcartsectionhtml = _shoppingCartSettings.MiniShoppingCartEnabled
                ? this.RenderPartialViewToString("FlyoutShoppingCart", PrepareMiniShoppingCartModel())
                : "";

            return Json(new
            {
                success = true,
                message = string.Format(_localizationService.GetResource("Products.ProductsHasBeenAddedToTheCart.Link"), Url.RouteUrl("ShoppingCart")),
                updatetopcartsectionhtml = updatetopcartsectionhtml,
                updateflyoutcartsectionhtml = updateflyoutcartsectionhtml
            });
        }
        #endregion

        //add product to cart using AJAX
        //currently we use this method on the product details pages
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult AddProductToCart_Details(int productId, int shoppingCartTypeId, FormCollection form)
        {
            if (form.AllKeys.Contains("reservationDate"))
            {
                _httpContext.Session["ReservationDate"] = form["reservationDate"];
                _httpContext.Session["ReservationSlots"] = form["TimeslotsConfigured"];
            }
            var product = _productService.GetProductById(productId);
            ShoppingCartSettings.IsAddToCartSeletected = true;

            if (product == null)
            {
                return Json(new
                {
                    redirect = Url.RouteUrl("HomePage"),
                });
            }

            //we can add only simple products
            if (product.ProductType != ProductType.SimpleProduct)
            {
                return Json(new
                {
                    success = false,
                    message = "Only simple products could be added to the cart"
                });
            }

            #region Update existing shopping cart item?
            int updatecartitemid = 0;
            foreach (string formKey in form.AllKeys)
            {
                if (formKey.Equals(string.Format("addtocart_{0}.UpdatedShoppingCartItemId", productId), StringComparison.InvariantCultureIgnoreCase))
                {
                    int.TryParse(form[formKey], out updatecartitemid);
                    break;
                }
            }

            ShoppingCartItem updatecartitem = null;
            if (_shoppingCartSettings.AllowCartItemEditing && updatecartitemid > 0)
            {
                //search with the same cart type as specified
                var cart = _workContext.CurrentCustomer.ShoppingCartItems
                    .Where(x => x.ShoppingCartTypeId == shoppingCartTypeId)
                    .LimitPerStore(_storeContext.CurrentStore.Id)
                    .ToList();
                updatecartitem = cart.FirstOrDefault(x => x.Id == updatecartitemid);
                //not found? let's ignore it. in this case we'll add a new item
                //if (updatecartitem == null)
                //{
                //    return Json(new
                //    {
                //        success = false,
                //        message = "No shopping cart item found to update"
                //    });
                //}
                //is it this product?
                if (updatecartitem != null && product.Id != updatecartitem.ProductId)
                {
                    return Json(new
                    {
                        success = false,
                        message = "This product does not match a passed shopping cart item identifier"
                    });
                }
            }
            #endregion

            #region Customer entered price
            decimal customerEnteredPriceConverted = decimal.Zero;
            if (product.CustomerEntersPrice)
            {
                foreach (string formKey in form.AllKeys)
                {
                    if (formKey.Equals(string.Format("addtocart_{0}.CustomerEnteredPrice", productId), StringComparison.InvariantCultureIgnoreCase))
                    {
                        decimal customerEnteredPrice;
                        if (decimal.TryParse(form[formKey], out customerEnteredPrice))
                        {
                            customerEnteredPriceConverted = _currencyService.ConvertToPrimaryStoreCurrency(customerEnteredPrice, _workContext.WorkingCurrency);
                        }

                        break;
                    }
                }
            }
            #endregion

            #region Quantity

            int quantity = 1;
            foreach (string formKey in form.AllKeys)
            {
                if (formKey.Equals(string.Format("addtocart_{0}.EnteredQuantity", productId), StringComparison.InvariantCultureIgnoreCase))
                {
                    int.TryParse(form[formKey], out quantity);
                    break;
                }
            }

            DateTime selectedReservationDate = new DateTime();
            string selectedTimeSlot = "";
            string quatityItem = "addtocart_" + product.Id + ".EnteredQuantity";

            int enteredQuantity = int.Parse(Request.Form[quatityItem]);

            if (product.IsReservation && product.ReservationCapPerSlot > 0 && enteredQuantity > product.ReservationCapPerSlot)
            {
                return Json(new
                {
                    success = false,
                    message = "Max entered quantity should not be greater than : Reservation Cap Per Slot : " + product.ReservationCapPerSlot.ToString()
                });
            }

            if (product.IsReservation && product.ReservationCapPerSlot == decimal.Zero)
            {
                var reservationProducts = _productService.GetReservationProductsbyProductId(productId);
                int occupancyConfiguredForSlot = reservationProducts.Where(x => x.TimeSlotsConfigured == Request.Form["TimeslotsConfigured"]).Select(e => e.OccupancyUnitsAvailable).FirstOrDefault();

                var reservedProducts = _productService.GetReservedProductbyProductId(productId);
                int unitsAlreadyReservedForSlot = 0;
                DateTime? selectedDt = (DateTime?)_dateTimeHelper.ConvertToUtcTime(DateTime.Parse(Request.Form["reservationDate"]), _dateTimeHelper.CurrentTimeZone);

                foreach (var rp in reservedProducts)
                {
                    DateTime? rpReservationDate = (DateTime?)_dateTimeHelper.ConvertToUtcTime(rp.ReservationDate, _dateTimeHelper.CurrentTimeZone);
                    if (rp.ReservedTimeSlot == Request.Form["TimeslotsConfigured"] && rpReservationDate == selectedDt)
                    {
                        unitsAlreadyReservedForSlot += rp.ReservedUnits;
                    }
                }
                var availableslots = occupancyConfiguredForSlot - unitsAlreadyReservedForSlot;
                if (quantity > availableslots)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Max exntered quantity should not be greater than available slots: " + availableslots.ToString()
                    });
                }
            }

            if (product.IsReservation && form.AllKeys.Any())
            {
                quantity = enteredQuantity;
                selectedReservationDate = DateTime.Parse(Request.Form["reservationDate"]);
                selectedTimeSlot = Request.Form["TimeslotsConfigured"];
            }

            #endregion

            //product and gift card attributes
            string attributes = ParseProductAttributes(product, form);

            //rental attributes
            DateTime? rentalStartDate = null;
            DateTime? rentalEndDate = null;
            if (product.IsRental)
            {
                ParseRentalDates(product, form, out rentalStartDate, out rentalEndDate);
            }
            var cartType = updatecartitem == null ? (ShoppingCartType)shoppingCartTypeId :
                //if the item to update is found, then we ignore the specified "shoppingCartTypeId" parameter
                updatecartitem.ShoppingCartType;

            // NMSU ID Attribute Validation
            var addToCartWarnings = new List<string>();
            List<int> NMSUProductIds = new List<int> { 26046, 26047, 26048, 26049, 26050, 26051, 26053, 26054, 26055, 26034, 26035, 26036, 26037, 26038, 26039, 26040, 26041, 26042, 26043, 26044, 26054 };
            bool passesIdentityCheck = true;

            //Id	ProductId
            //49058   26035
            //49059   26036
            //49060   26044
            //49061   26039
            //49062   26037
            //49063   26042
            //49064   26034
            //49065   26047
            //49066   26046
            //49067   26049
            //49068   26048
            //49069   26050
            //49070   26051
            //49464   26053
            if (NMSUProductIds.Contains(product.Id)) 
            {
                passesIdentityCheck = false;
                string controlId = "";
                // PROD Values
                switch (product.Id)
                {
                    case 26035:
                        controlId = "product_attribute_49058";
                        break;
                    case 26036:
                        controlId = "product_attribute_49059";
                        break;
                    case 26044:
                        controlId = "product_attribute_49060";
                        break;
                    case 26039:
                        controlId = "product_attribute_49061";
                        break;
                    case 26037:
                        controlId = "product_attribute_49062";
                        break;
                    case 26042:
                        controlId = "product_attribute_49063";
                        break;
                    case 26034:
                        controlId = "product_attribute_49064";
                        break;
                    case 26047:
                        controlId = "product_attribute_49065";
                        break;
                    case 26046:
                        controlId = "product_attribute_49066";
                        break;
                    case 26049:
                        controlId = "product_attribute_49067";
                        break;
                    case 26048:
                        controlId = "product_attribute_49068";
                        break;
                    case 26050:
                        controlId = "product_attribute_49069";
                        break;
                    case 26051:
                        controlId = "product_attribute_49070";
                        break;
                    case 26053:
                        controlId = "product_attribute_49464";
                        break;
                    case 26054:
                        controlId = "product_attribute_49576";
                        break;
                    default:
                        break;
                }
                // QA Values
                //Id ProductId
                //switch (product.Id)
                //{
                //    case 26037:
                //        controlId = "product_attribute_48790";
                //        break;
                //    case 26042:
                //        controlId = "product_attribute_48791";
                //        break;
                //    case 26034:
                //        controlId = "product_attribute_48792";
                //        break;
                //    case 26035:
                //        controlId = "product_attribute_48793";
                //        break;
                //    case 26036:
                //        controlId = "product_attribute_48794";
                //        break;
                //    case 26051:
                //        controlId = "product_attribute_48795";
                //        break;
                //    case 26044:
                //        controlId = "product_attribute_48796";
                //        break;
                //    case 26039:
                //        controlId = "product_attribute_48797";
                //        break;
                //    case 26047:
                //        controlId = "product_attribute_48798";
                //        break;

                //    case 26046:
                //        controlId = "product_attribute_48799";
                //        break;
                //    case 26049:
                //        controlId = "product_attribute_48800";
                //        break;
                //    case 26048:
                //        controlId = "product_attribute_48801";
                //        break;
                //    case 26050:
                //        controlId = "product_attribute_48802";
                //        break;
                //    case 26349:
                //        controlId = "product_attribute_48803";
                //        break;
                //    default:
                //        break;
                //}
                string nmsuid = "000000000";
                try
                {
                    nmsuid = form[controlId].ToString();
                    if (String.IsNullOrWhiteSpace(nmsuid))
                    {
                        nmsuid = "000000000";
                    }
                }
                catch (Exception ex)
                {
                    nmsuid = "000000000";
                }
                var studentProduct = new List<int> { 26034, 26035, 26036, 26044, 26039, 26037, 26042 };
                var employeeProduct = new List<int> { 26046, 26049, 26048 };
                var alumniProduct = new List<int> { 26053 };
                var retireeProduct = new List<int> { 26050, 26051 };
                var affilProduct = new List<int> { 26047, 26049, 26048 };
                var commProduct = new List<int> { 26054 };
                string SqlQuery = $"SELECT nmsunumber, status_student, status_employee, campus, status_affil, status_retiree, status_alumni, status_returning from NMSU_BANNER_EXPORT where nmsunumber={nmsuid}";
                //string connString = new DataSettingsManager().LoadSettings().DataConnectionString;                
                string connString = "Data Source=smw-sql-prod-cluster.cbnedywmxgw1.us-east-1.rds.amazonaws.com;Initial Catalog=nmsu_banner;User Id=smwbluedotuser;Password=Tahz00!;Persist Security Info=False;MultipleActiveResultSets=True";
                if (product.Name.Contains("Community"))
                {
                    // Assume if the nmsu id is invalid it was likely passed as null which is actually ok in this scenario for community products ONLY
                    if (nmsuid == "000000000")
                    {
                        passesIdentityCheck = true;
                    }
                    else { 
                        using (SqlConnection conn = new SqlConnection(connString))
                        {

                            SqlCommand cmd = new SqlCommand(SqlQuery, conn);
                            try
                            {
                                conn.Open();
                                var dbReader = cmd.ExecuteReader();
                                if (dbReader.HasRows)
                                {
                                    passesIdentityCheck = true;
                                }
                                else
                                {
                                    passesIdentityCheck = false;
                                }
                            }
                            catch (Exception ex)
                            {
                                passesIdentityCheck = false;
                            }
                        }
                    }
                }
                else
                {
                    using (SqlConnection conn = new SqlConnection(connString))
                    {
                        SqlCommand cmd = new SqlCommand(SqlQuery, conn);
                        try
                        {
                            conn.Open();
                            var dbReader = cmd.ExecuteReader();
                            if (dbReader.HasRows)
                            {
                                while (dbReader.Read())
                                {
                                    string readId = dbReader["nmsunumber"].ToString();
                                    string affiliateStatus = dbReader["status_affil"].ToString();
                                    string retireeStatus = dbReader["status_retiree"].ToString();
                                    string campus = dbReader["campus"].ToString();
                                    string alumniStatus = dbReader["status_alumni"].ToString();
                                    string studentStatus = dbReader["status_student"].ToString();
                                    string employeeStatus = dbReader["status_employee"].ToString();
                                    string returningStatus = dbReader["status_returning"].ToString();
                                    if (affilProduct.Contains(product.Id))
                                    {
                                        if (affiliateStatus == "True")
                                        {
                                            passesIdentityCheck = true;
                                        }
                                    }
                                    if (retireeProduct.Contains(product.Id))
                                    {
                                        if (retireeStatus == "True")
                                        {
                                            passesIdentityCheck = true;
                                        }
                                    }
                                    if (alumniProduct.Contains(product.Id))
                                    {
                                        if (alumniStatus == "True")
                                        {
                                            passesIdentityCheck = true;
                                        }
                                    }
                                    if (employeeProduct.Contains(product.Id))
                                    {
                                        if (employeeStatus == "FR" || employeeStatus == "SR")
                                        {
                                            passesIdentityCheck = true;
                                        }
                                    }
                                    if (commProduct.Contains(productId) && returningStatus == "True")
                                    {
                                        passesIdentityCheck = true;
                                    }
                                    if (!String.IsNullOrEmpty(studentStatus) && studentProduct.Contains(product.Id))
                                    {
                                        passesIdentityCheck = true;
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    }
                }
                if (!passesIdentityCheck)
                {
                    addToCartWarnings.Add("The supplied NMSU ID does not validate for this product. Please check the ID for typoes or choose the correct product for your status.");
                }
            }
            //save item
            
            if (updatecartitem == null)
            {
                if (passesIdentityCheck) 
                { 
                //add to the cart
                    addToCartWarnings.AddRange(_shoppingCartService.AddToCart(_workContext.CurrentCustomer,
                                                product,
                                                cartType,
                                                _storeContext.CurrentStore.Id,
                                                attributes,
                                                customerEnteredPriceConverted,
                                                rentalStartDate,
                                                rentalEndDate,
                                                quantity,
                                                selectedReservationDate,
                                                selectedTimeSlot,
                                                true));
                }
            }
            else
            {
                var cart = _workContext.CurrentCustomer.ShoppingCartItems
                    .Where(x => x.ShoppingCartType == updatecartitem.ShoppingCartType)
                    .LimitPerStore(_storeContext.CurrentStore.Id)
                    .ToList();
                var otherCartItemWithSameParameters = _shoppingCartService.FindShoppingCartItemInTheCart(
                    cart, updatecartitem.ShoppingCartType, product, attributes, customerEnteredPriceConverted,
                    rentalStartDate, rentalEndDate);
                if (otherCartItemWithSameParameters != null &&
                    otherCartItemWithSameParameters.Id == updatecartitem.Id)
                {
                    //ensure it's some other shopping cart item	
                    otherCartItemWithSameParameters = null;
                }
                //update existing item
                addToCartWarnings.AddRange(_shoppingCartService.UpdateShoppingCartItem(_workContext.CurrentCustomer,
                    updatecartitem.Id, attributes, customerEnteredPriceConverted,
                    rentalStartDate, rentalEndDate, quantity, true,
                    null, -1, null));   /// SODMYWAY-
                if (otherCartItemWithSameParameters != null && !addToCartWarnings.Any())
                {
                    //delete the same shopping cart item (the other one)
                    _shoppingCartService.DeleteShoppingCartItem(otherCartItemWithSameParameters);
                }
            }


            //Update Session for ProductAttributes - to be used in Vertex Calls.
            #region Add associated productIds in Session to DB
            if (attributes != null && attributes.Length > 0)
            {
                bool newCartItem = System.Web.HttpContext.Current.Session["InsertedShoppingCartItemId"] != null;
                bool cartItemToUpdate = updatecartitem != null;
                if (newCartItem || cartItemToUpdate)
                {
                    int currentShoppingCartItemId = 0;
                    bool isCurrentShoppingCartItemBundleProduct = false;
                    if (newCartItem)
                    {
                        currentShoppingCartItemId = (int)System.Web.HttpContext.Current.Session["InsertedShoppingCartItemId"];
                        ShoppingCartItem sc = _shoppingCartService.GetShoppingCartItemById(currentShoppingCartItemId);
                        isCurrentShoppingCartItemBundleProduct = sc.IsBundleProduct;
                    }

                    if (cartItemToUpdate)
                    {
                        currentShoppingCartItemId = updatecartitem.ProductId;
                        isCurrentShoppingCartItemBundleProduct = updatecartitem.IsBundleProduct;
                    }

                    ShoppingCartItem sci = _workContext.CurrentCustomer.ShoppingCartItems.Where(x => x.Id == currentShoppingCartItemId).FirstOrDefault();
                    if (currentShoppingCartItemId > 0 && isCurrentShoppingCartItemBundleProduct)
                    {
                        Dictionary<string, string> associatedProductDict = new Dictionary<string, string>();
                        List<ShoppingCartBundleProductItem> scbpList = _shoppingCartService.GetAssociatedProductsPerShoppingCartItem(currentShoppingCartItemId).ToList();

                        if (scbpList.Any())
                        {
                            foreach (ShoppingCartBundleProductItem scbp in scbpList)
                            {
                                string dictKey = scbp.ShoppingCartItemId + "_" + scbp.ParentProductId + "_" + scbp.AssociatedProductId  ;
                                string dictValue = scbp.AssociatedProductName + "_" + scbp.Price + "_" + scbp.AssociatedProductTaxCategoryId + "_" + scbp.Quantity;
                                if (associatedProductDict != null)
                                {
                                    if (associatedProductDict.ContainsKey(dictKey))
                                    {
                                        associatedProductDict.Remove(dictKey);
                                    }
                                }
                                associatedProductDict.Add(dictKey, dictValue);
                            }
                        }
                        System.Web.HttpContext.Current.Session["AssociatedProductPerShoppingCartItem"] = associatedProductDict;
                    }
                }
            }
            
            //Clear session for reuse
            System.Web.HttpContext.Current.Session["InsertedShoppingCartItemId"] = null;
            #endregion Add associated productIds in Session

            #region Return result
            if (addToCartWarnings.Any())
            {
                //cannot be added to the cart/wishlist
                //let's display warnings
                return Json(new
                {
                    success = false,
                    message = addToCartWarnings.ToArray()
                });
            }

            //added to the cart/wishlist
            switch (cartType)
            {
                case ShoppingCartType.Wishlist:
                    {
                        //activity log
                        _customerActivityService.InsertActivity("PublicStore.AddToWishlist", _localizationService.GetResource("ActivityLog.PublicStore.AddToWishlist"), product.Name);

                        if (_shoppingCartSettings.DisplayWishlistAfterAddingProduct)
                        {
                            //redirect to the wishlist page
                            return Json(new
                            {
                                redirect = Url.RouteUrl("Wishlist"),
                            });
                        }

                        //display notification message and update appropriate blocks
                        var updatetopwishlistsectionhtml = string.Format(_localizationService.GetResource("Wishlist.HeaderQuantity"),
                        _workContext.CurrentCustomer.ShoppingCartItems
                        .Where(sci => sci.ShoppingCartType == ShoppingCartType.Wishlist)
                        .LimitPerStore(_storeContext.CurrentStore.Id)
                        .ToList()
                        .GetTotalProducts());

                        return Json(new
                        {
                            success = true,
                            message = string.Format(_localizationService.GetResource("Products.ProductHasBeenAddedToTheWishlist.Link"), Url.RouteUrl("Wishlist")),
                            updatetopwishlistsectionhtml = updatetopwishlistsectionhtml,
                        });
                    }
                case ShoppingCartType.ShoppingCart:
                default:
                    {
                        //activity log
                        _customerActivityService.InsertActivity("PublicStore.AddToShoppingCart", _localizationService.GetResource("ActivityLog.PublicStore.AddToShoppingCart"), product.Name);

                        if (_shoppingCartSettings.DisplayCartAfterAddingProduct)
                        {
                            //redirect to the shopping cart page
                            return Json(new
                            {
                                redirect = Url.RouteUrl("ShoppingCart"),
                            });
                        }

                        //display notification message and update appropriate blocks
                        var updatetopcartsectionhtml = string.Format(_localizationService.GetResource("ShoppingCart.HeaderQuantity"),
                        _workContext.CurrentCustomer.ShoppingCartItems
                        .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                        .LimitPerStore(_storeContext.CurrentStore.Id)
                        .ToList()
                        .GetTotalProducts());

                        var updateflyoutcartsectionhtml = _shoppingCartSettings.MiniShoppingCartEnabled
                            ? this.RenderPartialViewToString("FlyoutShoppingCart", PrepareMiniShoppingCartModel())
                            : "";

                        ShoppingCartSettings.IsAddToCartSeletected = false;
                        return Json(new
                        {
                            success = true,
                            message = string.Format(_localizationService.GetResource("Products.ProductHasBeenAddedToTheCart.Link"), Url.RouteUrl("ShoppingCart")),
                            updatetopcartsectionhtml = updatetopcartsectionhtml,
                            updateflyoutcartsectionhtml = updateflyoutcartsectionhtml
                        });
                    }
            }



            #endregion
        }

        //handle product attribute selection event. this way we return new price, overridden gtin/sku/mpn
        //currently we use this method on the product details pages
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult ProductDetails_AttributeChange(int productId, bool validateAttributeConditions,
            bool loadPicture, FormCollection form)
        {
            var product = _productService.GetProductById(productId);
            if (product == null)
            {
                return new NullJsonResult();
            }

            string attributeXml = ParseProductAttributes(product, form);

            //rental attributes
            DateTime? rentalStartDate = null;
            DateTime? rentalEndDate = null;
            if (product.IsRental)
            {
                ParseRentalDates(product, form, out rentalStartDate, out rentalEndDate);
            }

            //sku, mpn, gtin
            string sku = product.FormatSku(attributeXml, _productAttributeParser);
            string mpn = product.FormatMpn(attributeXml, _productAttributeParser);
            string gtin = product.FormatGtin(attributeXml, _productAttributeParser);

            //price
            string price = "";
            if (_permissionService.Authorize(StandardPermissionProvider.DisplayPrices) && !product.CustomerEntersPrice)
            {
                //we do not calculate price of "customer enters price" option is enabled
                List<Discount> scDiscounts;
                decimal discountAmount;
                decimal finalPrice = _priceCalculationService.GetUnitPrice(product,
                    _workContext.CurrentCustomer,
                    ShoppingCartType.ShoppingCart,
                    1, attributeXml, 0,
                    rentalStartDate, rentalEndDate,
                    true, out discountAmount, out scDiscounts);
                decimal taxRate;
                decimal finalPriceWithDiscountBase = _taxService.GetProductPrice(product, finalPrice, out taxRate);
                decimal finalPriceWithDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(finalPriceWithDiscountBase, _workContext.WorkingCurrency);
                price = _priceFormatter.FormatPrice(finalPriceWithDiscount);
            }

            //stock
            var stockAvailability = product.FormatStockMessage(attributeXml, _localizationService, _productAttributeParser);

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
                        {
                            enabledAttributeMappingIds.Add(attribute.Id);
                        }
                        else
                        {
                            disabledAttributeMappingIds.Add(attribute.Id);
                        }
                    }
                }
            }

            //picture. used when we want to override a default product picture when some attribute is selected
            var pictureFullSizeUrl = "";
            var pictureDefaultSizeUrl = "";
            if (loadPicture)
            {
                //just load (return) the first found picture (in case if we have several distinct attributes with associated pictures)
                //actually we're going to support pictures associated to attribute combinations (not attribute values) soon. it'll more flexible approach
                var attributeValues = _productAttributeParser.ParseProductAttributeValues(attributeXml);
                var attributeValueWithPicture = attributeValues.FirstOrDefault(x => x.PictureId > 0);
                if (attributeValueWithPicture != null)
                {
                    var productAttributePictureCacheKey = string.Format(ModelCacheEventConsumer.PRODUCTATTRIBUTE_PICTURE_MODEL_KEY,
                                    attributeValueWithPicture.PictureId,
                                    _webHelper.IsCurrentConnectionSecured(),
                                    _storeContext.CurrentStore.Id);
                    var pictureModel = _cacheManager.Get(productAttributePictureCacheKey, () =>
                    {
                        var valuePicture = _pictureService.GetPictureById(attributeValueWithPicture.PictureId);
                        if (valuePicture != null)
                        {
                            return new PictureModel
                            {
                                FullSizeImageUrl = _pictureService.GetPictureUrl(valuePicture),
                                ImageUrl = _pictureService.GetPictureUrl(valuePicture, _mediaSettings.ProductDetailsPictureSize)
                            };
                        }
                        return new PictureModel();
                    });
                    pictureFullSizeUrl = pictureModel.FullSizeImageUrl;
                    pictureDefaultSizeUrl = pictureModel.ImageUrl;
                }

            }

            return Json(new
            {
                gtin = gtin,
                mpn = mpn,
                sku = sku,
                price = price,
                stockAvailability = stockAvailability,
                enabledattributemappingids = enabledAttributeMappingIds.ToArray(),
                disabledattributemappingids = disabledAttributeMappingIds.ToArray(),
                pictureFullSizeUrl = pictureFullSizeUrl,
                pictureDefaultSizeUrl = pictureDefaultSizeUrl
            });
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult CheckoutAttributeChange(FormCollection form)
        {
            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();

            ParseAndSaveCheckoutAttributes(cart, form);
            var attributeXml = _workContext.CurrentCustomer.GetAttribute<string>(SystemCustomerAttributeNames.CheckoutAttributes,
                _genericAttributeService, _storeContext.CurrentStore.Id);

            var enabledAttributeIds = new List<int>();
            var disabledAttributeIds = new List<int>();
            var attributes = _checkoutAttributeService.GetAllCheckoutAttributes(_storeContext.CurrentStore.Id, !cart.RequiresShipping());
            foreach (var attribute in attributes)
            {
                var conditionMet = _checkoutAttributeParser.IsConditionMet(attribute, attributeXml);
                if (conditionMet.HasValue)
                {
                    if (conditionMet.Value)
                    {
                        enabledAttributeIds.Add(attribute.Id);
                    }
                    else
                    {
                        disabledAttributeIds.Add(attribute.Id);
                    }
                }
            }

            return Json(new
            {
                enabledattributeids = enabledAttributeIds.ToArray(),
                disabledattributeids = disabledAttributeIds.ToArray()
            });
        }

        [HttpPost]
        public ActionResult UploadFileProductAttribute(int attributeId)
        {
            var attribute = _productAttributeService.GetProductAttributeMappingById(attributeId);
            if (attribute == null || attribute.AttributeControlType != AttributeControlType.FileUpload)
            {
                return Json(new
                {
                    success = false,
                    downloadGuid = Guid.Empty,
                }, MimeTypes.TextPlain);
            }

            //we process it distinct ways based on a browser
            //find more info here http://stackoverflow.com/questions/4884920/mvc3-valums-ajax-file-upload
            Stream stream = null;
            var fileName = "";
            var contentType = "";
            if (String.IsNullOrEmpty(Request["qqfile"]))
            {
                // IE
                HttpPostedFileBase httpPostedFile = Request.Files[0];
                if (httpPostedFile == null)
                {
                    throw new ArgumentException("No file uploaded");
                }

                stream = httpPostedFile.InputStream;
                fileName = Path.GetFileName(httpPostedFile.FileName);
                contentType = httpPostedFile.ContentType;
            }
            else
            {
                //Webkit, Mozilla
                stream = Request.InputStream;
                fileName = Request["qqfile"];
            }

            var fileBinary = new byte[stream.Length];
            stream.Read(fileBinary, 0, fileBinary.Length);

            var fileExtension = Path.GetExtension(fileName);
            if (!String.IsNullOrEmpty(fileExtension))
            {
                fileExtension = fileExtension.ToLowerInvariant();
            }

            if (attribute.ValidationFileMaximumSize.HasValue)
            {
                //compare in bytes
                var maxFileSizeBytes = attribute.ValidationFileMaximumSize.Value * 1024;
                if (fileBinary.Length > maxFileSizeBytes)
                {
                    //when returning JSON the mime-type must be set to text/plain
                    //otherwise some browsers will pop-up a "Save As" dialog.
                    return Json(new
                    {
                        success = false,
                        message = string.Format(_localizationService.GetResource("ShoppingCart.MaximumUploadedFileSize"), attribute.ValidationFileMaximumSize.Value),
                        downloadGuid = Guid.Empty,
                    }, MimeTypes.TextPlain);
                }
            }

            var download = new Download
            {
                DownloadGuid = Guid.NewGuid(),
                UseDownloadUrl = false,
                DownloadUrl = "",
                DownloadBinary = fileBinary,
                ContentType = contentType,
                //we store filename without extension for downloads
                Filename = Path.GetFileNameWithoutExtension(fileName),
                Extension = fileExtension,
                IsNew = true
            };
            _downloadService.InsertDownload(download);

            //when returning JSON the mime-type must be set to text/plain
            //otherwise some browsers will pop-up a "Save As" dialog.
            return Json(new
            {
                success = true,
                message = _localizationService.GetResource("ShoppingCart.FileUploaded"),
                downloadUrl = Url.Action("GetFileUpload", "Download", new { downloadId = download.DownloadGuid }),
                downloadGuid = download.DownloadGuid,
            }, MimeTypes.TextPlain);
        }

        [HttpPost]
        public ActionResult UploadFileCheckoutAttribute(int attributeId)
        {
            var attribute = _checkoutAttributeService.GetCheckoutAttributeById(attributeId);
            if (attribute == null || attribute.AttributeControlType != AttributeControlType.FileUpload)
            {
                return Json(new
                {
                    success = false,
                    downloadGuid = Guid.Empty,
                }, MimeTypes.TextPlain);
            }

            //we process it distinct ways based on a browser
            //find more info here http://stackoverflow.com/questions/4884920/mvc3-valums-ajax-file-upload
            Stream stream = null;
            var fileName = "";
            var contentType = "";
            if (String.IsNullOrEmpty(Request["qqfile"]))
            {
                // IE
                HttpPostedFileBase httpPostedFile = Request.Files[0];
                if (httpPostedFile == null)
                {
                    throw new ArgumentException("No file uploaded");
                }

                stream = httpPostedFile.InputStream;
                fileName = Path.GetFileName(httpPostedFile.FileName);
                contentType = httpPostedFile.ContentType;
            }
            else
            {
                //Webkit, Mozilla
                stream = Request.InputStream;
                fileName = Request["qqfile"];
            }

            var fileBinary = new byte[stream.Length];
            stream.Read(fileBinary, 0, fileBinary.Length);

            var fileExtension = Path.GetExtension(fileName);
            if (!String.IsNullOrEmpty(fileExtension))
            {
                fileExtension = fileExtension.ToLowerInvariant();
            }

            if (attribute.ValidationFileMaximumSize.HasValue)
            {
                //compare in bytes
                var maxFileSizeBytes = attribute.ValidationFileMaximumSize.Value * 1024;
                if (fileBinary.Length > maxFileSizeBytes)
                {
                    //when returning JSON the mime-type must be set to text/plain
                    //otherwise some browsers will pop-up a "Save As" dialog.
                    return Json(new
                    {
                        success = false,
                        message = string.Format(_localizationService.GetResource("ShoppingCart.MaximumUploadedFileSize"), attribute.ValidationFileMaximumSize.Value),
                        downloadGuid = Guid.Empty,
                    }, MimeTypes.TextPlain);
                }
            }

            var download = new Download
            {
                DownloadGuid = Guid.NewGuid(),
                UseDownloadUrl = false,
                DownloadUrl = "",
                DownloadBinary = fileBinary,
                ContentType = contentType,
                //we store filename without extension for downloads
                Filename = Path.GetFileNameWithoutExtension(fileName),
                Extension = fileExtension,
                IsNew = true
            };
            _downloadService.InsertDownload(download);

            //when returning JSON the mime-type must be set to text/plain
            //otherwise some browsers will pop-up a "Save As" dialog.
            return Json(new
            {
                success = true,
                message = _localizationService.GetResource("ShoppingCart.FileUploaded"),
                downloadUrl = Url.Action("GetFileUpload", "Download", new { downloadId = download.DownloadGuid }),
                downloadGuid = download.DownloadGuid,
            }, MimeTypes.TextPlain);
        }


        [NopHttpsRequirement(SslRequirement.Yes)]
        public ActionResult Cart()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart))
            {
                return RedirectToRoute("HomePage");
            }

            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();

            var model = new ShoppingCartModel();
            ShoppingCartSettings.IsAddToCartSeletected = true;



            PrepareShoppingCartModel(model, cart);
            return View(model);
        }

        [ChildActionOnly]
        public ActionResult OrderSummary(bool? prepareAndDisplayOrderReviewData)
        {
            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();
            //_taxService.PostQuotationBulkTaxData(cart);
            var model = new ShoppingCartModel();
            PrepareShoppingCartModel(model, cart,
                isEditable: false,
                prepareEstimateShippingIfEnabled: false,
                prepareAndDisplayOrderReviewData: prepareAndDisplayOrderReviewData.GetValueOrDefault());
            return PartialView(model);
        }

        [ValidateInput(false)]
        [HttpPost, ActionName("Cart")]
        [FormValueRequired("updatecart")]
        public ActionResult UpdateCart(FormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart))
            {
                return RedirectToRoute("HomePage");
            }

            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();

            var allIdsToRemove = form["removefromcart"] != null ? form["removefromcart"].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => int.Parse(x)).ToList() : new List<int>();

            ViewBag.Message = allIdsToRemove;
            //current warnings <cart item identifier, warnings>
            var innerWarnings = new Dictionary<int, IList<string>>();
            foreach (var sci in cart)
            {
                bool remove = allIdsToRemove.Contains(sci.Id);
                if (remove)
                {
                    _shoppingCartService.DeleteShoppingCartItem(sci, ensureOnlyActiveCheckoutAttributes: true);
                }
                else
                {
                    foreach (string formKey in form.AllKeys)
                    {
                        if (formKey.Equals(string.Format("itemquantity{0}", sci.Id), StringComparison.InvariantCultureIgnoreCase))
                        {
                            int newQuantity;
                            if (int.TryParse(form[formKey], out newQuantity))
                            {
                                var currSciWarnings = _shoppingCartService.UpdateShoppingCartItem(_workContext.CurrentCustomer,
                                    sci.Id, sci.AttributesXml, sci.CustomerEnteredPrice,
                                    sci.RentalStartDateUtc, sci.RentalEndDateUtc,
                                    newQuantity, true,
                                    null, null, null);	/// SODMYWAY-
                                innerWarnings.Add(sci.Id, currSciWarnings);
                            }
                            break;
                        }
                    }
                }
            }

            //parse and save checkout attributes
            ParseAndSaveCheckoutAttributes(cart, form);

            //updated cart
            cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();
            var model = new ShoppingCartModel();
            PrepareShoppingCartModel(model, cart);
            //update current warnings
            foreach (var kvp in innerWarnings)
            {
                //kvp = <cart item identifier, warnings>
                var sciId = kvp.Key;
                var warnings = kvp.Value;
                //find model
                var sciModel = model.Items.FirstOrDefault(x => x.Id == sciId);
                if (sciModel != null)
                {
                    foreach (var w in warnings)
                    {
                        if (!sciModel.Warnings.Contains(w))
                        {
                            sciModel.Warnings.Add(w);
                        }
                    }
                }
            }
            return View(model);
        }

        [ValidateInput(false)]
        [HttpPost, ActionName("Cart")]
        [FormValueRequired("continueshopping")]
        public ActionResult ContinueShopping()
        {
            var returnUrl = _workContext.CurrentCustomer.GetAttribute<string>(SystemCustomerAttributeNames.LastContinueShoppingPage, _storeContext.CurrentStore.Id);
            if (!String.IsNullOrEmpty(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToRoute("HomePage");
            }
        }

        [ValidateInput(false)]
        [HttpPost, ActionName("Cart")]
        [FormValueRequired("checkout")]
        public ActionResult StartCheckout(FormCollection form)
        {
            var model = new ShoppingCartModel();
            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();

            //parse and save checkout attributes
            ParseAndSaveCheckoutAttributes(cart, form);

            //validate attributes
            var checkoutAttributes = _workContext.CurrentCustomer.GetAttribute<string>(SystemCustomerAttributeNames.CheckoutAttributes, _genericAttributeService, _storeContext.CurrentStore.Id);
            var checkoutAttributeWarnings = _shoppingCartService.GetShoppingCartWarnings(cart, checkoutAttributes, true);
            if (checkoutAttributeWarnings.Any())
            {
                //something wrong, redisplay the page with warnings

                PrepareShoppingCartModel(model, cart, validateCheckoutAttributes: true);
                return View(model);
            }
            foreach (var cartItems in cart)
            {
                if (cartItems.Product.IsReservation)
                {
                    var reservationProducts = _productService.GetReservationProductsbyProductId(cartItems.Product.Id);
                    int occupancyConfiguredForSlot = reservationProducts.Where(x => x.TimeSlotsConfigured == cartItems.ReservedTimeSlot).Select(e => e.OccupancyUnitsAvailable).FirstOrDefault();

                    var reservedProducts = _productService.GetReservedProductbyProductId(cartItems.Product.Id);
                    int unitsAlreadyReservedForSlot = 0;
                    DateTime? selectedDt = (DateTime?)_dateTimeHelper.ConvertToUtcTime(DateTime.Parse(cartItems.ReservationDate.ToString()), _dateTimeHelper.CurrentTimeZone);

                    foreach (var rp in reservedProducts)
                    {
                        DateTime? rpReservationDate = (DateTime?)_dateTimeHelper.ConvertToUtcTime(rp.ReservationDate, _dateTimeHelper.CurrentTimeZone);
                        if (rp.ReservedTimeSlot == cartItems.ReservedTimeSlot && rpReservationDate == selectedDt)
                        {
                            unitsAlreadyReservedForSlot += rp.ReservedUnits;
                        }
                    }
                    var availableTimeSlot = occupancyConfiguredForSlot - unitsAlreadyReservedForSlot;
                    if (cartItems.Quantity > availableTimeSlot)
                    {
                        model.Warnings.Add("Time Slots No Longer available.Please Select a different time slot");
                        PrepareShoppingCartModel(model, cart, validateCheckoutAttributes: true);
                        return View(model);
                    }
                }
            }

            //everything is OK
            if (_workContext.CurrentCustomer.IsGuest())
            {
                bool downloadableProductsRequireRegistration =
                    _customerSettings.RequireRegistrationForDownloadableProducts && cart.Any(sci => sci.Product.IsDownload);

                if (!_orderSettings.AnonymousCheckoutAllowed
                    || downloadableProductsRequireRegistration)
                {
                    return new HttpUnauthorizedResult();
                }

                return RedirectToRoute("LoginCheckoutAsGuest", new { returnUrl = Url.RouteUrl("ShoppingCart") });
            }

            return RedirectToRoute("Checkout");
        }

        [ValidateInput(false)]
        [HttpPost, ActionName("Cart")]
        [FormValueRequired("applydiscountcouponcode")]
        public ActionResult ApplyDiscountCoupon(string discountcouponcode, FormCollection form)
        {
            //trim
            if (discountcouponcode != null)
            {
                discountcouponcode = discountcouponcode.Trim();
            }

            //cart
            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();

            //parse and save checkout attributes
            ParseAndSaveCheckoutAttributes(cart, form);

            var model = new ShoppingCartModel();
            if (!String.IsNullOrWhiteSpace(discountcouponcode))
            {
                //we find even hidden records here. this way we can display a user-friendly message if it's expired
                var discount = _discountService.GetDiscountByCouponCode(discountcouponcode, true);
                var appliedToProducts = discount.AppliedToProducts;
                //For MaxDiscount Feature Identification
                foreach (var aTp in appliedToProducts)
                {
                    var cartitemData = cart.Where(x => x.ProductId == aTp.Id).FirstOrDefault();
                    if (cartitemData != null)
                    {

                        if (cartitemData.Quantity > cartitemData.Product.MaxDiscountQuantity && cartitemData.Product.MaxDiscountQuantity != decimal.Zero)
                        {
                            model.DiscountBox.Message = "Cart Items exceeds Maximum discount Quantity";
                            model.DiscountBox.IsApplied = false;
                            PrepareShoppingCartModel(model, cart);
                            return View(model);
                        }
                    }
                }

                if (discount != null && discount.RequiresCouponCode)
                {
                    var validationResult = _discountService.ValidateDiscount(discount, _workContext.CurrentCustomer, discountcouponcode);
                    if (validationResult.IsValid)
                    {
                        //valid
                        _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer, SystemCustomerAttributeNames.DiscountCouponCode, discountcouponcode);
                        model.DiscountBox.Message = _localizationService.GetResource("ShoppingCart.DiscountCouponCode.Applied");
                        model.DiscountBox.IsApplied = true;
                    }
                    else
                    {
                        if (!String.IsNullOrEmpty(validationResult.UserError))
                        {
                            //some user error
                            model.DiscountBox.Message = validationResult.UserError;
                            model.DiscountBox.IsApplied = false;
                        }
                        else
                        {
                            //general error text
                            model.DiscountBox.Message = _localizationService.GetResource("ShoppingCart.DiscountCouponCode.WrongDiscount");
                            model.DiscountBox.IsApplied = false;
                        }
                    }
                }
                else
                {
                    //discount cannot be found
                    model.DiscountBox.Message = _localizationService.GetResource("ShoppingCart.DiscountCouponCode.WrongDiscount");
                    model.DiscountBox.IsApplied = false;
                }
            }
            else
            {
                //empty coupon code
                model.DiscountBox.Message = _localizationService.GetResource("ShoppingCart.DiscountCouponCode.WrongDiscount");
                model.DiscountBox.IsApplied = false;
            }

            PrepareShoppingCartModel(model, cart, true, discountcouponcode);
            return View(model);
        }

        [ValidateInput(false)]
        [HttpPost, ActionName("Cart")]
        [FormValueRequired("applygiftcardcouponcode")]
        public ActionResult ApplyGiftCard(string giftcardcouponcode, FormCollection form)
        {
            //trim
            if (giftcardcouponcode != null)
            {
                giftcardcouponcode = giftcardcouponcode.Trim();
            }

            //cart
            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();

            //parse and save checkout attributes
            ParseAndSaveCheckoutAttributes(cart, form);

            var model = new ShoppingCartModel();
            if (!cart.IsRecurring())
            {
                if (!String.IsNullOrWhiteSpace(giftcardcouponcode))
                {
                    var giftCard = _giftCardService.GetAllGiftCards(giftCardCouponCode: giftcardcouponcode).FirstOrDefault();
                    bool isGiftCardValid = giftCard != null && giftCard.IsGiftCardValid();
                    if (isGiftCardValid)
                    {
                        _workContext.CurrentCustomer.ApplyGiftCardCouponCode(giftcardcouponcode);
                        _customerService.UpdateCustomer(_workContext.CurrentCustomer);
                        model.GiftCardBox.Message = _localizationService.GetResource("ShoppingCart.GiftCardCouponCode.Applied");
                        model.GiftCardBox.IsApplied = true;
                    }
                    else
                    {
                        model.GiftCardBox.Message = _localizationService.GetResource("ShoppingCart.GiftCardCouponCode.WrongGiftCard");
                        model.GiftCardBox.IsApplied = false;
                    }
                }
                else
                {
                    model.GiftCardBox.Message = _localizationService.GetResource("ShoppingCart.GiftCardCouponCode.WrongGiftCard");
                    model.GiftCardBox.IsApplied = false;
                }
            }
            else
            {
                model.GiftCardBox.Message = _localizationService.GetResource("ShoppingCart.GiftCardCouponCode.DontWorkWithAutoshipProducts");
                model.GiftCardBox.IsApplied = false;
            }

            PrepareShoppingCartModel(model, cart);
            return View(model);
        }

        [ValidateInput(false)]
        [PublicAntiForgery]
        [HttpPost]
        public ActionResult GetEstimateShipping(int? countryId, int? stateProvinceId, string zipPostalCode, FormCollection form)
        {
            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();

            //parse and save checkout attributes
            ParseAndSaveCheckoutAttributes(cart, form);

            var model = new EstimateShippingResultModel();

            if (cart.RequiresShipping())
            {
                var address = new Address
                {
                    CountryId = countryId,
                    Country = countryId.HasValue ? _countryService.GetCountryById(countryId.Value) : null,
                    StateProvinceId = stateProvinceId,
                    StateProvince = stateProvinceId.HasValue ? _stateProvinceService.GetStateProvinceById(stateProvinceId.Value) : null,
                    ZipPostalCode = zipPostalCode,
                };
                GetShippingOptionResponse getShippingOptionResponse = _shippingService
                    .GetShippingOptions(cart, address, "", _storeContext.CurrentStore.Id);
                if (getShippingOptionResponse.Success)
                {
                    if (getShippingOptionResponse.ShippingOptions.Any())
                    {
                        foreach (var shippingOption in getShippingOptionResponse.ShippingOptions)
                        {
                            var soModel = new EstimateShippingResultModel.ShippingOptionModel
                            {
                                Name = shippingOption.Name,
                                Description = shippingOption.Description,

                            };
                            //calculate discounted and taxed rate
                            List<Discount> appliedDiscounts = null;
                            decimal shippingTotal = _orderTotalCalculationService.AdjustShippingRate(shippingOption.Rate,
                                cart, out appliedDiscounts);

                            decimal rateBase = _taxService.GetShippingPrice(shippingTotal, _workContext.CurrentCustomer);
                            decimal rate = _currencyService.ConvertFromPrimaryStoreCurrency(rateBase, _workContext.WorkingCurrency);
                            soModel.Price = _priceFormatter.FormatShippingPrice(rate, true);
                            model.ShippingOptions.Add(soModel);
                        }
                    }
                }
                else
                {
                    foreach (var error in getShippingOptionResponse.Errors)
                    {
                        model.Warnings.Add(error);
                    }
                }

                if (_shippingSettings.AllowPickUpInStore)
                {
                    var pickupPointsResponse = _shippingService.GetPickupPoints(address, null, _storeContext.CurrentStore.Id);
                    if (pickupPointsResponse.Success)
                    {
                        if (pickupPointsResponse.PickupPoints.Any())
                        {
                            var soModel = new EstimateShippingResultModel.ShippingOptionModel
                            {
                                Name = _localizationService.GetResource("Checkout.PickupPoints"),
                                Description = _localizationService.GetResource("Checkout.PickupPoints.Description"),
                            };
                            var pickupFee = pickupPointsResponse.PickupPoints.Min(x => x.PickupFee);
                            if (pickupFee > 0)
                            {
                                pickupFee = _taxService.GetShippingPrice(pickupFee, _workContext.CurrentCustomer);
                                pickupFee = _currencyService.ConvertFromPrimaryStoreCurrency(pickupFee, _workContext.WorkingCurrency);
                            }
                            soModel.Price = _priceFormatter.FormatShippingPrice(pickupFee, true);
                            model.ShippingOptions.Add(soModel);
                        }
                    }
                    else
                    {
                        foreach (var error in pickupPointsResponse.Errors)
                        {
                            model.Warnings.Add(error);
                        }
                    }
                }
            }

            return PartialView("_EstimateShippingResult", model);
        }

        [ChildActionOnly]
        public ActionResult OrderTotals(bool isEditable)
        {
            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();
            var model = PrepareOrderTotalsModel(cart, isEditable);
            return PartialView(model);
        }

        [ValidateInput(false)]
        [HttpPost, ActionName("Cart")]
        [FormValueRequired("removesubtotaldiscount", "removeordertotaldiscount", "removediscountcouponcode")]
        public ActionResult RemoveDiscountCoupon()
        {
            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();
            var model = new ShoppingCartModel();

            _genericAttributeService.SaveAttribute<string>(_workContext.CurrentCustomer,
                SystemCustomerAttributeNames.DiscountCouponCode, null);

            PrepareShoppingCartModel(model, cart);
            return View(model);
        }

        [ValidateInput(false)]
        [HttpPost, ActionName("Cart")]
        [FormValueRequired(FormValueRequirement.StartsWith, "removegiftcard-")]
        public ActionResult RemoveGiftCardCode(FormCollection form)
        {
            var model = new ShoppingCartModel();

            //get gift card identifier
            int giftCardId = 0;
            foreach (var formValue in form.AllKeys)
            {
                if (formValue.StartsWith("removegiftcard-", StringComparison.InvariantCultureIgnoreCase))
                {
                    giftCardId = Convert.ToInt32(formValue.Substring("removegiftcard-".Length));
                }
            }

            var gc = _giftCardService.GetGiftCardById(giftCardId);
            if (gc != null)
            {
                _workContext.CurrentCustomer.RemoveGiftCardCouponCode(gc.GiftCardCouponCode);
                _customerService.UpdateCustomer(_workContext.CurrentCustomer);
            }

            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();
            PrepareShoppingCartModel(model, cart);
            return View(model);
        }

        [ChildActionOnly]
        public ActionResult FlyoutShoppingCart()
        {
            if (!_shoppingCartSettings.MiniShoppingCartEnabled)
            {
                return Content("");
            }

            if (!_permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart))
            {
                return Content("");
            }

            ShoppingCartSettings.IsAddToCartSeletected = false;

            var model = PrepareMiniShoppingCartModel();
            return PartialView(model);
        }

        #endregion

        #region Wishlist

        [NopHttpsRequirement(SslRequirement.Yes)]
        public ActionResult Wishlist(Guid? customerGuid)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.EnableWishlist))
            {
                return RedirectToRoute("HomePage");
            }

            Customer customer = customerGuid.HasValue ?
                _customerService.GetCustomerByGuid(customerGuid.Value)
                : _workContext.CurrentCustomer;
            if (customer == null)
            {
                return RedirectToRoute("HomePage");
            }

            var cart = customer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.Wishlist)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();
            var model = new WishlistModel();
            PrepareWishlistModel(model, cart, !customerGuid.HasValue);
            return View(model);
        }

        [ValidateInput(false)]
        [HttpPost, ActionName("Wishlist")]
        [FormValueRequired("updatecart")]
        public ActionResult UpdateWishlist(FormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.EnableWishlist))
            {
                return RedirectToRoute("HomePage");
            }

            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.Wishlist)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();

            var allIdsToRemove = form["removefromcart"] != null
                ? form["removefromcart"].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse)
                .ToList()
                : new List<int>();

            //current warnings <cart item identifier, warnings>
            var innerWarnings = new Dictionary<int, IList<string>>();
            foreach (var sci in cart)
            {
                bool remove = allIdsToRemove.Contains(sci.Id);
                if (remove)
                {
                    _shoppingCartService.DeleteShoppingCartItem(sci);
                }
                else
                {
                    foreach (string formKey in form.AllKeys)
                    {
                        if (formKey.Equals(string.Format("itemquantity{0}", sci.Id), StringComparison.InvariantCultureIgnoreCase))
                        {
                            int newQuantity;
                            if (int.TryParse(form[formKey], out newQuantity))
                            {
                                var currSciWarnings = _shoppingCartService.UpdateShoppingCartItem(_workContext.CurrentCustomer,
                                    sci.Id, sci.AttributesXml, sci.CustomerEnteredPrice,
                                    sci.RentalStartDateUtc, sci.RentalEndDateUtc,
                                    newQuantity, true,
                                    null, null, null);	/// SODMYWAY-
                                innerWarnings.Add(sci.Id, currSciWarnings);
                            }
                            break;
                        }
                    }
                }
            }

            //updated wishlist
            cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.Wishlist)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();
            var model = new WishlistModel();
            PrepareWishlistModel(model, cart);
            //update current warnings
            foreach (var kvp in innerWarnings)
            {
                //kvp = <cart item identifier, warnings>
                var sciId = kvp.Key;
                var warnings = kvp.Value;
                //find model
                var sciModel = model.Items.FirstOrDefault(x => x.Id == sciId);
                if (sciModel != null)
                {
                    foreach (var w in warnings)
                    {
                        if (!sciModel.Warnings.Contains(w))
                        {
                            sciModel.Warnings.Add(w);
                        }
                    }
                }
            }
            return View(model);
        }

        [ValidateInput(false)]
        [HttpPost, ActionName("Wishlist")]
        [FormValueRequired("addtocartbutton")]
        public ActionResult AddItemsToCartFromWishlist(Guid? customerGuid, FormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart))
            {
                return RedirectToRoute("HomePage");
            }

            if (!_permissionService.Authorize(StandardPermissionProvider.EnableWishlist))
            {
                return RedirectToRoute("HomePage");
            }

            var pageCustomer = customerGuid.HasValue
                ? _customerService.GetCustomerByGuid(customerGuid.Value)
                : _workContext.CurrentCustomer;
            if (pageCustomer == null)
            {
                return RedirectToRoute("HomePage");
            }

            var pageCart = pageCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.Wishlist)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();

            var allWarnings = new List<string>();
            var numberOfAddedItems = 0;
            var allIdsToAdd = form["addtocart"] != null
                ? form["addtocart"].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse)
                .ToList()
                : new List<int>();
            foreach (var sci in pageCart)
            {
                if (allIdsToAdd.Contains(sci.Id))
                {
                    var warnings = _shoppingCartService.AddToCart(_workContext.CurrentCustomer,
                                                                    sci.Product,
                                                                    ShoppingCartType.ShoppingCart,
                                                                    _storeContext.CurrentStore.Id,
                                                                    sci.AttributesXml,
                                                                    sci.CustomerEnteredPrice,
                                                                    sci.RentalStartDateUtc,
                                                                    sci.RentalEndDateUtc,
                                                                    sci.Quantity,
                                                                    sci.ReservationDate,
                                                                    sci.ReservedTimeSlot,
                                                                    true);
                    if (!warnings.Any())
                    {
                        numberOfAddedItems++;
                    }

                    if (_shoppingCartSettings.MoveItemsFromWishlistToCart && //settings enabled
                        !customerGuid.HasValue && //own wishlist
                        !warnings.Any()) //no warnings ( already in the cart)	
                    {
                        //let's remove the item from wishlist
                        _shoppingCartService.DeleteShoppingCartItem(sci);
                    }
                    allWarnings.AddRange(warnings);
                }
            }

            if (numberOfAddedItems > 0)
            {
                //redirect to the shopping cart page

                if (allWarnings.Any())
                {
                    ErrorNotification(_localizationService.GetResource("Wishlist.AddToCart.Error"), true);
                }

                return RedirectToRoute("ShoppingCart");
            }
            else
            {
                //no items added. redisplay the wishlist page

                if (allWarnings.Any())
                {
                    ErrorNotification(_localizationService.GetResource("Wishlist.AddToCart.Error"), false);
                }

                var cart = pageCustomer.ShoppingCartItems
                    .Where(sci => sci.ShoppingCartType == ShoppingCartType.Wishlist)
                    .LimitPerStore(_storeContext.CurrentStore.Id)
                    .ToList();
                var model = new WishlistModel();
                PrepareWishlistModel(model, cart, !customerGuid.HasValue);
                return View(model);
            }
        }

        [NopHttpsRequirement(SslRequirement.Yes)]
        public ActionResult EmailWishlist()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.EnableWishlist) || !_shoppingCartSettings.EmailWishlistEnabled)
            {
                return RedirectToRoute("HomePage");
            }

            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.Wishlist)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();

            if (!cart.Any())
            {
                return RedirectToRoute("HomePage");
            }

            var model = new WishlistEmailAFriendModel
            {
                YourEmailAddress = _workContext.CurrentCustomer.Email,
                DisplayCaptcha = _captchaSettings.Enabled && _captchaSettings.ShowOnEmailWishlistToFriendPage
            };
            return View(model);
        }

        [HttpPost, ActionName("EmailWishlist")]
        [PublicAntiForgery]
        [FormValueRequired("send-email")]
        [CaptchaValidator]
        public ActionResult EmailWishlistSend(WishlistEmailAFriendModel model, bool captchaValid)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.EnableWishlist) || !_shoppingCartSettings.EmailWishlistEnabled)
            {
                return RedirectToRoute("HomePage");
            }

            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.Wishlist)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();
            if (!cart.Any())
            {
                return RedirectToRoute("HomePage");
            }

            //validate CAPTCHA
            if (_captchaSettings.Enabled && _captchaSettings.ShowOnEmailWishlistToFriendPage && !captchaValid)
            {
                ModelState.AddModelError("", _captchaSettings.GetWrongCaptchaMessage(_localizationService));
            }

            //check whether the current customer is guest and ia allowed to email wishlist
            if (_workContext.CurrentCustomer.IsGuest() && !_shoppingCartSettings.AllowAnonymousUsersToEmailWishlist)
            {
                ModelState.AddModelError("", _localizationService.GetResource("Wishlist.EmailAFriend.OnlyRegisteredUsers"));
            }

            if (ModelState.IsValid)
            {
                //email
                _workflowMessageService.SendWishlistEmailAFriendMessage(_workContext.CurrentCustomer,
                        _workContext.WorkingLanguage.Id, model.YourEmailAddress,
                        model.FriendEmail, Core.Html.HtmlHelper.FormatText(model.PersonalMessage, false, true, false, false, false, false));

                model.SuccessfullySent = true;
                model.Result = _localizationService.GetResource("Wishlist.EmailAFriend.SuccessfullySent");

                return View(model);
            }

            //If we got this far, something failed, redisplay form
            model.DisplayCaptcha = _captchaSettings.Enabled && _captchaSettings.ShowOnEmailWishlistToFriendPage;
            return View(model);
        }

        #endregion
    }
}
