using Nop.Web.Framework.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Services.Payments;
using System.Web.Mvc;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Catalog;
using Nop.Services.Orders;
using Nop.Services.Common;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Orders;
using Nop.Core;
using Nop.Services.Stores;
using Nop.Plugin.Payments.Bambora.Domain;
using Nop.Plugin.Payments.Bambora.Services;
using Nop.Services.Security;
using Nop.Plugin.Payments.Bambora.Models;
using System.Security.Cryptography;
using Nop.Core.Plugins;
using Nop.Web.Controllers;
using Nop.Web.Models.Checkout;
using Nop.Services.Directory;
using Nop.Services.Tax;
using Nop.Services.Customers;
using Nop.Services.Shipping;
using System.Web;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Common;
using Nop.Services.Media;
using System.Web.Routing;

namespace Nop.Plugin.Payments.Bambora.Controllers
{

    public class BamboraPaymentController : BasePaymentController
    {
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private readonly BamboraSettings _bamboraSettings;
        private BamboraStoreSetting _bamboraStoreSetting;
        private readonly IStoreLicenseKeyService _storeLicenseKeyService;
        private readonly IEncryptionService _encryptionService;
        private readonly IBamboraStoreSettingService _bamboraStoreSettingService;
        private readonly IStoreService _storeService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly OrderSettings _orderSettings;
        private readonly PaymentSettings _paymentSettings;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly ILogger _logger;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly ICurrencyService _currencyService;

        private readonly IStoreMappingService _storeMappingService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly ITaxService _taxService;
        private readonly ICustomerService _customerService;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IShippingService _shippingService;
        private readonly IPaymentService _paymentService;
        private readonly IPluginFinder _pluginFinder;
        private readonly IRewardPointService _rewardPointService;
        private readonly IWebHelper _webHelper;
        private readonly HttpContextBase _httpContext;
        private readonly IAddressAttributeParser _addressAttributeParser;
        private readonly IAddressAttributeService _addressAttributeService;
        private readonly IAddressAttributeFormatter _addressAttributeFormatter;
        private readonly IPictureService _pictureService;
        private readonly IFulfillmentService _fulfillmentService;
        private readonly IAddressService _addressService;
        private readonly IShipmentService _shipmentService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IOrderService _orderService;

        private readonly RewardPointsSettings _rewardPointsSettings;
        private readonly ShippingSettings _shippingSettings;
        private readonly AddressSettings _addressSettings;
        private readonly CustomerSettings _customerSettings;

        public BamboraPaymentController(ISettingService settingService, ILocalizationService localizationService,
            BamboraSettings bamboraSettings, IEncryptionService encryptionService,
            IStoreLicenseKeyService storeLicenseKeyService,
            IBamboraStoreSettingService bamboraStoreSettingService, IStoreContext storeContext,
            IStoreService storeService, ILogger logger,
            IWorkContext workContext, OrderSettings orderSettings,
            PaymentSettings paymentSettings, IPluginFinder pluginFinder,
            IGenericAttributeService genericAttributeService, IOrderTotalCalculationService orderTotalCalculationService,
            IPriceFormatter priceFormatter, IOrderProcessingService orderProcessingService, ICurrencyService currencyService, IPaymentService paymentService, HttpContextBase httpContext)
        {
            _settingService = settingService;
            _localizationService = localizationService;
            _bamboraSettings = bamboraSettings;
            _encryptionService = encryptionService;
            _storeLicenseKeyService = storeLicenseKeyService;
            _workContext = workContext;
            _orderSettings = orderSettings;
            _storeContext = storeContext;
            _storeService = storeService;
            _bamboraStoreSettingService = bamboraStoreSettingService;
            _genericAttributeService = genericAttributeService;
            _logger = logger;
            _paymentSettings = paymentSettings;
            _orderTotalCalculationService = orderTotalCalculationService;
            _priceFormatter = priceFormatter;

            _orderProcessingService = orderProcessingService;
            _currencyService = currencyService;
            _paymentService = paymentService;
            _httpContext = httpContext;
            var pluginDescriptor = pluginFinder.GetPluginDescriptorBySystemName("Payments.Bambora", LoadPluginsMode.All);
            if (pluginDescriptor != null && pluginDescriptor.Installed)
            {
                _bamboraStoreSetting = _bamboraStoreSettingService.GetByStore(_storeContext.CurrentStore.Id);
                if (_bamboraStoreSetting == null)
                    throw new Exception("Bambora Plugin is not configured");
            }
        }
        #region Utlities
        private static string MD5Hash(string input)
        {
            StringBuilder hash = new StringBuilder();
            MD5CryptoServiceProvider md5provider = new MD5CryptoServiceProvider();
            byte[] bytes = md5provider.ComputeHash(new UTF8Encoding().GetBytes(input));

            for (int i = 0; i < bytes.Length; i++)
            {
                hash.Append(bytes[i].ToString("x2"));
            }
            return hash.ToString();
        }
        #endregion

        #region Configure Plugin
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure()
        {
            var model = new ConfigurationModel();

            model.SandboxURL = _bamboraSettings.SandboxURL;
            model.ProductionURL = _bamboraSettings.ProductionURL;
            model.ApprovedRedirectUrl = _bamboraSettings.ApprovedRedirectUrl;
            model.RejectedRedirectUrl = _bamboraSettings.RejectedRedirectUrl;
            //model.MerchantId = _bamboraSettings.MerchantId;
            //model.HashKey = _bamboraSettings.HashKey;
            model.EncryptedSecureKey = _bamboraSettings.EncryptedSecureKey;

            //model.LicenseKeys = _storeLicenseKeyService.GetAll().Select(k => new StoreLicenseKeyModel
            //{
            //    Id = k.Id,
            //    LicenseKey = k.LicenseKey,
            //    Host = _storeLicenseKeyService.GetLicenseHost(k.LicenseKey),
            //    Type = _storeLicenseKeyService.GetLicenseType(k.LicenseKey)
            //}).ToList();

            model.Stores = _storeService.GetAllStores().Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name }).ToList();
            model.Stores.Insert(0, new SelectListItem { Value = "0", Text = "Default Store Settings" });

            return View("~/Plugins/Payments.Bambora/Views/BamboraPayment/Configure.cshtml", model);
        }
        [HttpPost]
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure(ConfigurationModel model)
        {
            //if (!ModelState.IsValid)
            //    return Configure();

            model.SavedSuccessfully = true;
            //if (ModelState.IsValid)
            //{
            try
            {
                _bamboraSettings.SandboxURL = model.SandboxURL;
                _bamboraSettings.ProductionURL = model.ProductionURL;
                _bamboraSettings.ApprovedRedirectUrl = model.ApprovedRedirectUrl;
                _bamboraSettings.RejectedRedirectUrl = model.RejectedRedirectUrl;
                //  _bamboraSettings.MerchantId = model.MerchantId;
                //_bamboraSettings.HashKey = model.HashKey;
                //_bamboraSettings.EncryptedSecureKey = model.EncryptedSecureKey;
                _settingService.SaveSetting(_bamboraSettings);

                model.SaveMessage = "Settings saved";
            }
            catch (Exception ex)
            {
                model.SavedSuccessfully = false;
                model.SaveMessage = ex.Message;
            }
            //}
            //else
            //{
            //    model.SavedSuccessfully = false;
            //    model.SaveMessage = ModelState.Values.First().Errors.First().ErrorMessage;
            //}

            //model.LicenseKeys = _storeLicenseKeyService.GetAll().Select(k => new StoreLicenseKeyModel
            //{
            //    Id = k.Id,
            //    LicenseKey = k.LicenseKey,
            //    Host = _storeLicenseKeyService.GetLicenseHost(k.LicenseKey),
            //    Type = _storeLicenseKeyService.GetLicenseType(k.LicenseKey)
            //}).ToList();

            model.Stores = _storeService.GetAllStores().Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name }).ToList();
            model.Stores.Insert(0, new SelectListItem { Value = "0", Text = "Default Store Settings" });

            return View("~/Plugins/Payments.Bambora/Views/BamboraPayment/Configure.cshtml", model);
        }

        [AdminAuthorize]
        public ActionResult GetStoreSettings(int storeId)
        {
            var model = new BamboraStoreSettingModel();

            model.StoreID = storeId;
            model.TransactModeValues = new List<SelectListItem>();
            model.TransactModeValues.Add(new SelectListItem { Text = TransactMode.Authorize.GetLocalizedEnum(_localizationService, _workContext.WorkingLanguage.Id), Value = ((int)TransactMode.Authorize).ToString() });
            model.TransactModeValues.Add(new SelectListItem { Text = TransactMode.AuthorizeAndCapture.GetLocalizedEnum(_localizationService, _workContext.WorkingLanguage.Id), Value = ((int)TransactMode.AuthorizeAndCapture).ToString() });
            model.TransactModeValues.Add(new SelectListItem { Text = TransactMode.AuthorizeAndCaptureAfterOrder.GetLocalizedEnum(_localizationService, _workContext.WorkingLanguage.Id), Value = ((int)TransactMode.AuthorizeAndCaptureAfterOrder).ToString() });
            model.TransactModeValues.Add(new SelectListItem { Text = TransactMode.HostedPaymentPage.GetLocalizedEnum(_localizationService, _workContext.WorkingLanguage.Id), Value = ((int)TransactMode.HostedPaymentPage).ToString() });

            var store = _storeService.GetStoreById(storeId);
            if (store == null)
            {
                model.StoreName = "Default Settings";
            }
            else
            {
                model.StoreName = store.Name;
            }

            var setting = _bamboraStoreSettingService.GetByStore(storeId, false);
            if (setting == null)
            {
                model.UseDefaultSettings = true;
            }
            else
            {
                model.UseDefaultSettings = false;
                model.UseSandbox = setting.UseSandbox;
                // model.TransactModeId = setting.TransactionMode;
                if (!string.IsNullOrEmpty(setting.MerchantId))
                    model.MerchantId = setting.MerchantId;
                if (!string.IsNullOrEmpty(setting.PaymentApiKey))
                    model.PaymentApiKey = setting.PaymentApiKey;
                if (!string.IsNullOrEmpty(setting.ReportingApiKey))
                    model.ReportingApiKey = setting.ReportingApiKey;
                if (!string.IsNullOrEmpty(setting.ProfilesApiKey))
                    model.ProfilesApiKey = setting.ProfilesApiKey;
                if (!string.IsNullOrEmpty(setting.HashKey))
                    model.HashKey = setting.HashKey;
            }

            return PartialView("~/Plugins/Payments.Bambora/Views/BamboraPayment/_StoreSettings.cshtml", model);
        }

        [AdminAuthorize]
        [HttpPost]
        public string SaveStoreSettings(BamboraStoreSettingModel model)
        {
            try
            {
                bool alreadyExists = true;
                var setting = _bamboraStoreSettingService.GetByStore(model.StoreID, false);
                if (setting == null)
                {
                    setting = new BamboraStoreSetting
                    {
                        StoreId = model.StoreID
                    };
                    alreadyExists = false;
                }

                setting.UseSandbox = model.UseSandbox;
                setting.MerchantId = model.MerchantId;
                setting.PaymentApiKey = model.PaymentApiKey;
                setting.ReportingApiKey = model.ReportingApiKey;
                setting.ProfilesApiKey = model.ProfilesApiKey;
                setting.HashKey = model.HashKey;

                if (alreadyExists)
                {
                    _bamboraStoreSettingService.Update(setting);
                }
                else
                {
                    _bamboraStoreSettingService.Insert(setting);
                }

                return "success";
            }
            catch (Exception ex)
            {
                _logger.Error("Error updating store specific settings", ex, _workContext.CurrentCustomer);
                return ex.Message;
            }
        }
        [HttpPost]
        public void RevertStoreSettings(int storeId)
        {
            var setting = _bamboraStoreSettingService.GetByStore(storeId, false);
            if (setting == null)
            {
                return;
            }

            _bamboraStoreSettingService.Delete(setting);
        }
        [HttpPost]
        [AdminAuthorize]
        public string AddKey(string licenseKey)
        {
            //var storeLicenseKey = new StoreLicenseKey
            //{
            //    LicenseKey = licenseKey
            //};

            //_storeLicenseKeyService.Insert(storeLicenseKey);

            //var host = _storeLicenseKeyService.GetLicenseHost(licenseKey);
            //var type = _storeLicenseKeyService.GetLicenseType(licenseKey);
            //var html = $"<div class=\"form-group\" data-id=\"LicenseRow_{storeLicenseKey.Id}\"><div class=\"col-md-1\">" +
            //        $"<img src = \"/Plugins/Bitshift.Payments.FirstData/Content/Images/ico-delete.gif\" id=\"DeleteKey_{storeLicenseKey.Id}\" data-id=\"{ storeLicenseKey.Id }\" />" +
            //       "</div>" +
            //       $"<div class=\"col-md-2\">{type}</div>" +
            //       $"<div class=\"col-md-2\">{host}</div>" +
            //       $"<div class=\"col-md-4\">{storeLicenseKey.LicenseKey}</div>" +
            //       "</div>";

            return "";
        }

        [HttpPost]
        [AdminAuthorize]
        public int DeleteKey(int id)
        {
            var key = _storeLicenseKeyService.GetById(id);
            _storeLicenseKeyService.Delete(key);

            return id;
        }
        #endregion

        #region Checkout
        [ChildActionOnly]
        public ActionResult PaymentInfo()
        {
            var model = new PaymentInfoModel();

            if (_bamboraStoreSetting != null && _storeContext != null)
                _bamboraStoreSetting = _bamboraStoreSettingService.GetByStore(_storeContext.CurrentStore.Id);
            else
                throw new Exception("Invalid Store configuration detected");
            string MerchanntId = string.IsNullOrEmpty(_bamboraStoreSetting.MerchantId) ? string.Empty : _bamboraStoreSetting.MerchantId;
            string HashKey = string.IsNullOrEmpty(_bamboraStoreSetting.HashKey) ? string.Empty : _bamboraStoreSetting.HashKey;
            if (string.IsNullOrEmpty(HashKey))
                throw new Exception("Unable to load Payment Form");

            string baseurl = _bamboraStoreSetting.UseSandbox ? _bamboraSettings.SandboxURL : _bamboraSettings.ProductionURL;
            var shoppingCartItems = _workContext.CurrentCustomer.ShoppingCartItems.Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart).LimitPerStore(_storeContext.CurrentStore.Id).ToList();
            var orderTotal = _orderTotalCalculationService.GetShoppingCartTotal(shoppingCartItems).GetValueOrDefault().ToString("0.00");
            string baseStoreUrl = _storeContext.CurrentStore.Url;  
            string acceptedUrl = baseStoreUrl + _bamboraSettings.ApprovedRedirectUrl;
            string rejectedUrl = baseStoreUrl + _bamboraSettings.RejectedRedirectUrl;

            string Md5haskeyFormatter = "merchant_id=" + MerchanntId + "&trnAmount=" + orderTotal + HashKey;
            string encryptedhashValue = MD5Hash(Md5haskeyFormatter);



            model.Url = baseurl + "?" + "merchant_id=" + MerchanntId + "&" + "trnAmount=" + orderTotal + "&" + "hashValue=" + encryptedhashValue + "&" + "approvedPage=" + Server.UrlEncode(acceptedUrl) + "&" + "declinedPage=" + Server.UrlEncode(rejectedUrl);



            ///WE SHALLL USE THIS IF WE USE ANY OTHER MODE OF PAYMENT RATHER THAN HOSTED PAYEMENT OPTION
            if (_bamboraStoreSetting.TransactionMode == (int)TransactMode.HostedPaymentPage)
            {
                model.UseHostedPage = true;
                model.LoadConfirmOrder = false;
            }
            else
            {
                model.UseHostedPage = true;
                model.LoadConfirmOrder = false;
            }
            return View("~/Plugins/Payments.Bambora/Views/BamboraPayment/PaymentInfo.cshtml", model);
        }
        public ActionResult GetHostedPaymentForm()
        {
            return View("~/Plugins/Payments.Bambora/Views/BamboraPayment/HostedPayment.cshtml");
        }
        #endregion
        [NonAction]
        public override ProcessPaymentRequest GetPaymentInfo(FormCollection form)
        {
            var paymentInfo = new ProcessPaymentRequest();
            return paymentInfo;
        }
        [NonAction]
        public override IList<string> ValidatePaymentForm(FormCollection form)
        {
            var warnings = new List<string>();

            //if (_bamboraStoreSetting.TransactionMode != (int)TransactMode.HostedPaymentPage)
            //{
            //    int savedCardId = 0;
            //    if (form["savedCardId"] != null && int.TryParse(form["savedCardId"], out savedCardId))
            //    {
            //        return warnings;
            //    }

            //    //validate
            //    //var validator = new PaymentInfoValidator(_localizationService);
            //    //var model = new PaymentInfoModel()
            //    //{
            //    //    CardholderName = form["CardholderName"],
            //    //    CardNumber = form["CardNumber"],
            //    //    CardCode = form["CardCode"],
            //    //    ExpireMonth = form["ExpireMonth"],
            //    //    ExpireYear = form["ExpireYear"],
            //    //};
            //    //var validationResult = validator.Validate(model);
            //    //if (!validationResult.IsValid)
            //    //{
            //    //    foreach (var error in validationResult.Errors)
            //    //    {
            //    //        warnings.Add(error.ErrorMessage);
            //    //    }
            //    //}
            //}

            return warnings;
        }

        public ActionResult HandleCallback()
        {
            int transApproved = 0;
            var model = new PaymentInfoModel();
            string transMessage = string.Empty;
            foreach (var data in Request.QueryString.AllKeys)
            {
                if (data == BamboraResponseText.TransResposneText)
                {
                    //Check exists txnid!
                    if (string.IsNullOrEmpty(Request.QueryString[BamboraResponseText.TransResposneText]))
                    {
                        throw new Exception("No GET(ResponseText) was supplied to the system!");
                    }
                    else
                    {
                        transApproved = Convert.ToInt32(Request.QueryString[BamboraResponseText.TransResposneText]);
                    }
                }

                if (data == BamboraResponseText.MessageText)
                {
                    //Check exists txnid!
                    if (string.IsNullOrEmpty(Request.QueryString[BamboraResponseText.MessageText]))
                    {
                        throw new Exception("No GET(MessageText) was supplied to the system!");
                    }
                    else
                    {
                        transMessage = Request.QueryString[BamboraResponseText.MessageText];
                    }
                }
            }
            ///Open Confirm Payment Page by calling Controller of Checkout Page
            if (transApproved == TransactionType.ApprovedTrans && transMessage == TransactionType.ApprovedTransMessage)
            {
                model.LoadConfirmOrder = true;
                _httpContext.Session["transactionId"] = Request.QueryString["trnId"];
                _httpContext.Session["responseMessage"] = Request.QueryString[BamboraResponseText.MessageText] + "(" + "Transaction Code:" + transApproved + ")";
                return View("~/Plugins/Payments.Bambora/Views/BamboraPayment/PaymentResponse.cshtml", model);

            }
            else
            {
                model.LoadPayemntError = true;
                model.ErrorMessage = transMessage;
                return View("~/Plugins/Payments.Bambora/Views/BamboraPayment/PaymentResponse.cshtml", model);
            }
        }
    }
}
