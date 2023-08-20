using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BitShift.Plugin.Payments.FirstData;
using BitShift.Plugin.Payments.FirstData.Domain;
using BitShift.Plugin.Payments.FirstData.Models;
using BitShift.Plugin.Payments.FirstData.Validators;
using BitShift.Plugin.Payments.FirstData.Services;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using System.Security.Cryptography;
using System.Text;
using Nop.Core.Plugins;
using Nop.Services.Common;
using Nop.Core.Domain.Common;
using Nop.Services.Orders;
using Nop.Services.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Data;

namespace BitShift.Plugin.Payments.FirstData.Controllers
{
    public class FirstDataController : BasePaymentController
    {
        private readonly HttpContextBase _httpContext;
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private readonly FirstDataSettings _firstDataSettings;
        private FirstDataStoreSetting _firstDataStoreSetting;
        private readonly IStoreLicenseKeyService _storeLicenseKeyService;
        private readonly ISavedCardService _savedCardService;
        private readonly IEncryptionService _encryptionService;
        private readonly IFirstDataStoreSettingService _firstDataStoreSettingService;
        private readonly IStoreService _storeService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly OrderSettings _orderSettings;
        private readonly PaymentSettings _paymentSettings;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly ILogger _logger;
        private readonly IRepository<Customer> _customerRepository;

        public FirstDataController(
            IRepository<Customer> customerRepository,
            HttpContextBase httpContext,
            ISettingService settingService, 
            ILocalizationService localizationService,
            FirstDataSettings firstDataSettings, IEncryptionService encryptionService,
            IStoreLicenseKeyService storeLicenseKeyService, ISavedCardService savedCardService,
            IFirstDataStoreSettingService firstDataStoreSettingService, IStoreContext storeContext,
            IStoreService storeService, ILogger logger,
            IWorkContext workContext, OrderSettings orderSettings,
            PaymentSettings paymentSettings, IPluginFinder pluginFinder,
            IGenericAttributeService genericAttributeService, IOrderTotalCalculationService orderTotalCalculationService,
            IPriceFormatter priceFormatter)
        {
            this._customerRepository = customerRepository;
            this._httpContext = httpContext;
            _settingService = settingService;
            _localizationService = localizationService;
            _firstDataSettings = firstDataSettings;
            _encryptionService = encryptionService;
            _storeLicenseKeyService = storeLicenseKeyService;
            _savedCardService = savedCardService;
            _workContext = workContext;
            _orderSettings = orderSettings;
            _storeContext = storeContext;
            _storeService = storeService;
            _firstDataStoreSettingService = firstDataStoreSettingService;
            _genericAttributeService = genericAttributeService;
            _logger = logger;
            _paymentSettings = paymentSettings;
            _orderTotalCalculationService = orderTotalCalculationService;
            _priceFormatter = priceFormatter;

            _firstDataStoreSettingService = firstDataStoreSettingService;

            var pluginDescriptor = pluginFinder.GetPluginDescriptorBySystemName("BitShift.Payments.FirstData", LoadPluginsMode.All);
            if (pluginDescriptor != null && pluginDescriptor.Installed)
            {
                _firstDataStoreSetting = _firstDataStoreSettingService.GetByStore(_storeContext.CurrentStore.Id);
                if (_firstDataStoreSetting == null)
                    throw new Exception("First Data plugin not configured");
            }
        }

        #region Utilities

        private SavedCardModel PrepareSavedCardModel(SavedCard card)
        {
            SavedCardModel savedCardModel = new SavedCardModel();

            savedCardModel.Id = card.Id;
            savedCardModel.CardholderName = card.CardholderName;
            savedCardModel.Last4Digits = card.Token.Substring(card.Token.Length - 4);
            savedCardModel.ExpireMonth = card.ExpireMonth;
            savedCardModel.ExpireYear = card.ExpireYear;
            savedCardModel.CardType = card.CardType;
            savedCardModel.IsExpired = (DateTime.Now > new DateTime(card.ExpireYear, card.ExpireMonth, 1).AddMonths(1));
            savedCardModel.CardDescription = _localizationService.GetResource("BitShift.Plugin.FirstData.Payment.CardDescription");
            savedCardModel.ExpirationDescription = _localizationService.GetResource("BitShift.Plugin.FirstData.Payment.ExpirationDescription");
            savedCardModel.ExpiredLabel = _localizationService.GetResource("BitShift.Plugin.FirstData.Payment.ExpiredLabel");
            savedCardModel.UseCardLabel = _localizationService.GetResource("BitShift.Plugin.FirstData.Payment.UseCardLabel");

            return savedCardModel;
        }

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

            model.SandboxURL = _firstDataSettings.SandboxURL;
            model.ProductionURL = _firstDataSettings.ProductionURL;

            model.LicenseKeys = _storeLicenseKeyService.GetAll().Select(k => new StoreLicenseKeyModel
            {
                Id = k.Id,
                LicenseKey = k.LicenseKey,
                Host = _storeLicenseKeyService.GetLicenseHost(k.LicenseKey),
                Type = _storeLicenseKeyService.GetLicenseType(k.LicenseKey)
            }).ToList();

            model.Stores = _storeService.GetAllStores().Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name }).ToList();
            model.Stores.Insert(0, new SelectListItem { Value = "0", Text = "Default Store Settings" });

            return View("~/Plugins/BitShift.Payments.FirstData/Views/Configure.cshtml", model);
        }

        [HttpPost]
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure(ConfigurationModel model)
        {
            if (!ModelState.IsValid)
                return Configure();

            model.SavedSuccessfully = true;
            if (ModelState.IsValid)
            {
                try
                {
                    _firstDataSettings.SandboxURL = model.SandboxURL;
                    _firstDataSettings.ProductionURL = model.ProductionURL;
                    _settingService.SaveSetting(_firstDataSettings);

                    model.SaveMessage = "Settings saved";
                }
                catch (Exception ex)
                {
                    model.SavedSuccessfully = false;
                    model.SaveMessage = ex.Message;
                }
            }
            else
            {
                model.SavedSuccessfully = false;
                model.SaveMessage = ModelState.Values.First().Errors.First().ErrorMessage;
            }

            model.LicenseKeys = _storeLicenseKeyService.GetAll().Select(k => new StoreLicenseKeyModel
            {
                Id = k.Id,
                LicenseKey = k.LicenseKey,
                Host = _storeLicenseKeyService.GetLicenseHost(k.LicenseKey),
                Type = _storeLicenseKeyService.GetLicenseType(k.LicenseKey)
            }).ToList();

            model.Stores = _storeService.GetAllStores().Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name }).ToList();
            model.Stores.Insert(0, new SelectListItem { Value = "0", Text = "Default Store Settings" });

            return View("~/Plugins/BitShift.Payments.FirstData/Views/Configure.cshtml", model);
        }

        [AdminAuthorize]
        public ActionResult GetStoreSettings(int storeId)
        {
            var model = new FirstDataStoreSettingModel();

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

            var setting = _firstDataStoreSettingService.GetByStore(storeId, false);
            if (setting == null)
            {
                model.UseDefaultSettings = true;
            }
            else
            {
                model.UseDefaultSettings = false;
                model.UseSandbox = setting.UseSandbox;
                model.TransactModeId = setting.TransactionMode;
                if (!string.IsNullOrEmpty(setting.HMAC))
                    model.HMAC = _encryptionService.DecryptText(setting.HMAC);
                if (!string.IsNullOrEmpty(setting.GatewayID))
                    model.GatewayID = _encryptionService.DecryptText(setting.GatewayID);
                if (!string.IsNullOrEmpty(setting.Password))
                    model.Password = _encryptionService.DecryptText(setting.Password);
                if (!string.IsNullOrEmpty(setting.KeyID))
                    model.KeyID = _encryptionService.DecryptText(setting.KeyID);
                if (!string.IsNullOrEmpty(setting.PaymentPageID))
                    model.PaymentPageID = _encryptionService.DecryptText(setting.PaymentPageID);
                if (!string.IsNullOrEmpty(setting.TransactionKey))
                    model.TransactionKey = _encryptionService.DecryptText(setting.TransactionKey);
                if (!string.IsNullOrEmpty(setting.ResponseKey))
                    model.ResponseKey = _encryptionService.DecryptText(setting.ResponseKey);
                model.AdditionalFee = setting.AdditionalFee;
                model.AdditionalFeePercentage = setting.AdditionalFeePercentage;
                model.EnableRecurringPayments = setting.EnableRecurringPayments;
                model.EnableCardSaving = setting.EnableCardSaving;
                model.EnablePurchaseOrderNumber = setting.EnablePurchaseOrderNumber;
            }

            return PartialView("~/Plugins/BitShift.Payments.FirstData/Views/_StoreSettings.cshtml", model);
        }

        [AdminAuthorize]
        [HttpPost]
        public string SaveStoreSettings(FirstDataStoreSettingModel model)
        {
            try
            {
                bool alreadyExists = true;
                var setting = _firstDataStoreSettingService.GetByStore(model.StoreID, false);
                if (setting == null)
                {
                    setting = new FirstDataStoreSetting
                    {
                        StoreId = model.StoreID
                    };
                    alreadyExists = false;
                }

                setting.UseSandbox = model.UseSandbox;
                setting.TransactionMode = model.TransactModeId;
                setting.HMAC = _encryptionService.EncryptText(model.HMAC);
                setting.GatewayID = _encryptionService.EncryptText(model.GatewayID);
                setting.Password = _encryptionService.EncryptText(model.Password);
                setting.KeyID = _encryptionService.EncryptText(model.KeyID);
                setting.PaymentPageID = _encryptionService.EncryptText(model.PaymentPageID);
                setting.TransactionKey = _encryptionService.EncryptText(model.TransactionKey);
                setting.ResponseKey = _encryptionService.EncryptText(model.ResponseKey);
                setting.AdditionalFee = model.AdditionalFee;
                setting.AdditionalFeePercentage = model.AdditionalFeePercentage;
                setting.EnableRecurringPayments = model.EnableRecurringPayments;
                setting.EnableCardSaving = model.EnableCardSaving;
                setting.EnablePurchaseOrderNumber = model.EnablePurchaseOrderNumber;

                if (alreadyExists)
                {
                    _firstDataStoreSettingService.Update(setting);
                }
                else
                {
                    _firstDataStoreSettingService.Insert(setting);
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
            var setting = _firstDataStoreSettingService.GetByStore(storeId, false);
            if (setting == null)
            {
                return;
            }

            _firstDataStoreSettingService.Delete(setting);
        }

        [HttpPost]
        [AdminAuthorize]
        public string AddKey(string licenseKey)
        {
            var storeLicenseKey = new StoreLicenseKey
            {
                LicenseKey = licenseKey
            };

            _storeLicenseKeyService.Insert(storeLicenseKey);

            var host = _storeLicenseKeyService.GetLicenseHost(licenseKey);
            var type = _storeLicenseKeyService.GetLicenseType(licenseKey);
            var html = $"<div class=\"form-group\" data-id=\"LicenseRow_{storeLicenseKey.Id}\"><div class=\"col-md-1\">" +
                    $"<img src = \"/Plugins/Bitshift.Payments.FirstData/Content/Images/ico-delete.gif\" id=\"DeleteKey_{storeLicenseKey.Id}\" data-id=\"{ storeLicenseKey.Id }\" />" +
                   "</div>" +
                   $"<div class=\"col-md-2\">{type}</div>" +
                   $"<div class=\"col-md-2\">{host}</div>" +
                   $"<div class=\"col-md-4\">{storeLicenseKey.LicenseKey}</div>" +
                   "</div>";

            return html;
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

            _firstDataStoreSetting = _firstDataStoreSettingService.GetByStore(_storeContext.CurrentStore.Id);

            if (_firstDataStoreSetting.TransactionMode == (int)TransactMode.HostedPaymentPage)
            {
                model.UseHostedPage = true;
            }
            else
            {

                //CC types
                model.CreditCardTypes.Add(new SelectListItem()
                {
                    Text = "Visa",
                    Value = "Visa",
                });
                model.CreditCardTypes.Add(new SelectListItem()
                {
                    Text = "Master card",
                    Value = "MasterCard",
                });
                model.CreditCardTypes.Add(new SelectListItem()
                {
                    Text = "Discover",
                    Value = "Discover",
                });
                model.CreditCardTypes.Add(new SelectListItem()
                {
                    Text = "Amex",
                    Value = "Amex",
                });

                //years
                for (int i = 0; i < 15; i++)
                {
                    string year = Convert.ToString(DateTime.Now.Year + i);
                    model.ExpireYears.Add(new SelectListItem()
                    {
                        Text = year,
                        Value = year,
                    });
                }

                //months
                for (int i = 1; i <= 12; i++)
                {
                    string text = (i < 10) ? "0" + i.ToString() : i.ToString();
                    model.ExpireMonths.Add(new SelectListItem()
                    {
                        Text = text,
                        Value = i.ToString(),
                    });
                }

                if (_firstDataStoreSetting == null)
                    throw new Exception("First Data plugin not configured");

                model.EnableCardSaving = _firstDataStoreSetting.EnableCardSaving;
                model.EnablePurchaseOrderNumber = _firstDataStoreSetting.EnablePurchaseOrderNumber;
                model.SavedCardsLabel = _localizationService.GetResource("BitShift.Plugin.FirstData.Payment.SavedCardsLabel");
                model.NewCardLabel = _localizationService.GetResource("BitShift.Plugin.FirstData.Payment.NewCardLabel");
                model.IsOnePageCheckout = _orderSettings.OnePageCheckoutEnabled;
                model.CustomerId = _workContext.CurrentCustomer.Id;

                if (_firstDataStoreSetting.EnableCardSaving)
                {
                    var savedCards = _savedCardService.GetByCustomer(_workContext.CurrentCustomer.Id);
                    foreach (var savedCard in savedCards)
                    {
                        model.SavedCards.Add(PrepareSavedCardModel(savedCard));
                    }
                }
                //set postback values
                var form = Request.Form;
                model.CardholderName = form["CardholderName"];
                model.CardNumber = form["CardNumber"];
                model.CardCode = form["CardCode"];
                model.SaveCard = (form["SaveCard"] != null ? Convert.ToBoolean(form["SaveCard"]) : false);
                var selectedCcType = model.CreditCardTypes.Where(x => x.Value.Equals(form["CreditCardType"], StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                if (selectedCcType != null)
                    selectedCcType.Selected = true;
                var selectedMonth = model.ExpireMonths.Where(x => x.Value.Equals(form["ExpireMonth"], StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                if (selectedMonth != null)
                    selectedMonth.Selected = true;
                var selectedYear = model.ExpireYears.Where(x => x.Value.Equals(form["ExpireYear"], StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                if (selectedYear != null)
                    selectedYear.Selected = true;
            }

            return View("~/Plugins/BitShift.Payments.FirstData/Views/PaymentInfo.cshtml", model);
        }

        [NonAction]
        public override IList<string> ValidatePaymentForm(FormCollection form)
        {
            var warnings = new List<string>();

            if (_firstDataStoreSetting.TransactionMode != (int)TransactMode.HostedPaymentPage)
            {
                int savedCardId = 0;
                if (form["savedCardId"] != null && int.TryParse(form["savedCardId"], out savedCardId))
                {
                    return warnings;
                }

                //validate
                var validator = new PaymentInfoValidator(_localizationService);
                var model = new PaymentInfoModel()
                {
                    CardholderName = form["CardholderName"],
                    CardNumber = form["CardNumber"],
                    CardCode = form["CardCode"],
                    ExpireMonth = form["ExpireMonth"],
                    ExpireYear = form["ExpireYear"],
                };
                var validationResult = validator.Validate(model);
                if (!validationResult.IsValid)
                {
                    foreach (var error in validationResult.Errors)
                    {
                        warnings.Add(error.ErrorMessage);
                    }
                }
            }

            return warnings;
        }

        [NonAction]
        public override ProcessPaymentRequest GetPaymentInfo(FormCollection form)
        {
            _firstDataStoreSetting = _firstDataStoreSettingService.GetByStore(_storeContext.CurrentStore.Id);
            var paymentInfo = new ProcessPaymentRequest();

            if (_firstDataStoreSetting.TransactionMode != (int)TransactMode.HostedPaymentPage)
            {
                paymentInfo.CreditCardType = form["CreditCardType"];
                _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer, Constants.TransactionCardType, form["CreditCardType"]);

                paymentInfo.CreditCardName = form["CardholderName"];
                paymentInfo.CreditCardNumber = form["CardNumber"];
                paymentInfo.CreditCardExpireMonth = int.Parse(form["ExpireMonth"]);
                paymentInfo.CreditCardExpireYear = int.Parse(form["ExpireYear"]);
                paymentInfo.CreditCardCvv2 = form["CardCode"];
                if (_firstDataStoreSetting.EnablePurchaseOrderNumber)
                {
                    paymentInfo.CustomValues.Add(Constants.PO, form["PurchaseOrderNumber"]);
                }

                if (form["SaveCard"] != null)
                {
                    paymentInfo.CustomValues.Add(Constants.SaveCard, form["SaveCard"].Split(',')[0].ToString());
                }

                int savedCardId = 0;
                if (form["savedCardId"] != null && int.TryParse(form["savedCardId"], out savedCardId))
                {
                    paymentInfo.CustomValues.Add(Constants.SavedCardId, savedCardId.ToString());
                }
            }

            return paymentInfo;
        }
        [HttpGet]
        public string Version()
        {
            return "Beta-Integration Version 1.01";
        }
        public ActionResult GetHostedPaymentForm()
        {
            _firstDataStoreSetting = _firstDataStoreSettingService.GetByStore(_storeContext.CurrentStore.Id);

            //clear the authorization tag from the user
            var authAttribute = _genericAttributeService.GetAttributesForEntity(_workContext.CurrentCustomer.Id, "Customer").Where(ga => ga.Key == Constants.AuthorizationAttribute).FirstOrDefault();
            if (authAttribute != null)
            {
                _genericAttributeService.DeleteAttribute(authAttribute);
            }

            //clear the card type from the user
            var cardTypeAttribute = _genericAttributeService.GetAttributesForEntity(_workContext.CurrentCustomer.Id, "Customer").Where(ga => ga.Key == Constants.TransactionCardType).FirstOrDefault();
            if (cardTypeAttribute != null)
            {
                _genericAttributeService.DeleteAttribute(cardTypeAttribute);
            }

            var shoppingCartItems = _workContext.CurrentCustomer.ShoppingCartItems.Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(_storeContext.CurrentStore.Id).ToList();

            var model = new PaymentInfoModel
            {
                PaymentPageID = _encryptionService.DecryptText(_firstDataStoreSetting.PaymentPageID),
                ReferenceNumber = _workContext.CurrentCustomer.Id.ToString(),
                CurrencyCode = _workContext.WorkingCurrency.CurrencyCode,
                OrderAmount = _orderTotalCalculationService.GetShoppingCartTotal(shoppingCartItems).GetValueOrDefault().ToString("0.00"),
                ResponseURL = Url.RouteUrl("Plugin.Payments.FirstData.PaymentResponse", null, Request.Url.Scheme),
                PaymentURL = _firstDataStoreSetting.UseSandbox ? "https://demo.globalgatewaye4.firstdata.com/payment" :
                                                                 "https://checkout.globalgatewaye4.firstdata.com/payment"
            };

            var billingAddress = _workContext.CurrentCustomer.BillingAddress;
            if (billingAddress != null)
            {
                model.Address1 = billingAddress.Address1;
                model.City = billingAddress.City;
                model.State = billingAddress.StateProvince?.Abbreviation;
                model.Country = billingAddress.Country?.ThreeLetterIsoCode;
                model.Zip = billingAddress.ZipPostalCode;
            }

            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan diff = DateTime.UtcNow - origin;
            var ticks = Math.Floor(diff.TotalSeconds);
            model.Ticks = Convert.ToInt32(ticks);

            // Convert string to array of bytes.
            byte[] data = Encoding.UTF8.GetBytes($"{model.PaymentPageID}^{model.ReferenceNumber}^{ticks}^{model.OrderAmount}^{model.CurrencyCode}");

            // key
            var transactionKey = _encryptionService.DecryptText(_firstDataStoreSetting.TransactionKey);
            byte[] key = Encoding.UTF8.GetBytes(transactionKey);

            // Create HMAC-MD5 Algorithm;
            HMACMD5 hmac = new HMACMD5(key);

            // Compute hash.
            byte[] hashBytes = hmac.ComputeHash(data);

            // Convert to HEX string.
            model.HashCode = BitConverter.ToString(hashBytes).ToLower().Replace("-", "");

            model.CustomerId = _workContext.CurrentCustomer.Id;
            return View("~/Plugins/BitShift.Payments.FirstData/Views/HostedPayment.cshtml", model);
        }

        public ActionResult PaymentResponse(FormCollection form)
        {
            var model = new PaymentResponseModel();
            int userinfo = Convert.ToInt32(form["currentCustomerId"]);
            if(userinfo > 0)
                {
                Customer customer = _customerRepository.GetById(userinfo);
               _workContext.CurrentCustomer = customer;
            }

            //check for valid info
            var shoppingCartItems = _workContext.CurrentCustomer.ShoppingCartItems.Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart).LimitPerStore(_storeContext.CurrentStore.Id).ToList();
            var orderTotal = _orderTotalCalculationService.GetShoppingCartTotal(shoppingCartItems).GetValueOrDefault().ToString("0.00");
            var hashedSignature = MD5Hash($"{_encryptionService.DecryptText(_firstDataStoreSetting.ResponseKey)}{_encryptionService.DecryptText(_firstDataStoreSetting.PaymentPageID)}{form["x_trans_id"]}{orderTotal}");

            if (form["x_MD5_hash"] != hashedSignature)
            {
                _logger.Warning($"Hosted Payment Page Warning: MD5 Hash match failed. Transaction approval: {form["Transaction_Approved"]} Order Amount (pulled from Cart): {orderTotal}. Our hash: {hashedSignature} Their hash: {form["x_MD5_hash"]}.");
            }

            if (!string.IsNullOrEmpty(form["Authorization_Num"]) && !string.IsNullOrEmpty(form["Transaction_Tag"]))
            {
                string value = $"{form["Authorization_Num"]}|{form["Transaction_Tag"]}";
                _logger.Information(string.Format("Inserting Key Entry in GenericAttribute Table For Authorizantion Number : {0}", value));
                model.IsSuccess = true;
                _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer, Constants.AuthorizationAttribute, $"{form["Authorization_Num"]}|{form["Transaction_Tag"]}", _storeContext.CurrentStore.Id);
                _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer, Constants.TransactionCardType, form["TransactionCardType"], _storeContext.CurrentStore.Id);
            }


            if (form["Transaction_Approved"] != "YES")
            {
                model.IsSuccess = false;
                model.ErrorMessage = form["Bank_Message"];
            }

            return PartialView("~/Plugins/BitShift.Payments.FirstData/Views/_PaymentResponse.cshtml", model);
        }
        #endregion

        #region Saved Cards
        [ChildActionOnly]
        public ActionResult SavedCardsLink(string widgetZone, object additionalData = null)
        {
            _firstDataStoreSetting = _firstDataStoreSettingService.GetByStore(_storeContext.CurrentStore.Id);
            if (_firstDataStoreSetting != null && _firstDataStoreSetting.EnableCardSaving && _paymentSettings.ActivePaymentMethodSystemNames.Contains(Constants.SystemName))
            {
                bool model = false;
                return View("~/Plugins/BitShift.Payments.FirstData/Views/SavedCardsLink.cshtml", model);
            }
            else
            {
                return Content("");
            }
        }

        public ActionResult SavedCards()
        {
            return View("~/Plugins/BitShift.Payments.FirstData/Views/SavedCards.cshtml");
        }

        public ActionResult SavedCardsTable()
        {
            IList<SavedCardModel> model = new List<SavedCardModel>();
            var savedCards = _savedCardService.GetByCustomer(_workContext.CurrentCustomer.Id);
            foreach (var savedCard in savedCards)
            {
                model.Add(PrepareSavedCardModel(savedCard));
            }

            return PartialView("~/Plugins/BitShift.Payments.FirstData/Views/SavedCardsTable.cshtml", model);
        }

        [HttpPost]
        public void DeleteSavedCard(int id)
        {
            var card = _savedCardService.GetById(id);
            if (card != null && card.Customer_Id == _workContext.CurrentCustomer.Id)
            {
                _savedCardService.Delete(card);
            }
        }
        #endregion
    }
}