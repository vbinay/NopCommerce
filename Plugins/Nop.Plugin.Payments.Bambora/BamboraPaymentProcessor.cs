using Bambora.NA.SDK;
using Bambora.NA.SDK.Requests;
using Nop.Core;
using Nop.Core.Domain;
using Nop.Core.Domain.Directory;
using Nop.Core.Plugins;
using Nop.Plugin.Payments.Bambora.Controllers;
using Nop.Plugin.Payments.Bambora.Data;
using Nop.Plugin.Payments.Bambora.Domain;
using Nop.Plugin.Payments.Bambora.Services;
using Nop.Services.Cms;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Routing;
using System.Xml;
using System.Xml.Serialization;

namespace Nop.Plugin.Payments.Bambora
{
    public class BamboraPaymentProcessor : BasePlugin, IPaymentMethod
    {
        #region Fields
        private static Gateway _bambora;
        private readonly BamboraSettings _bamboraSettings;
        private BamboraStoreSetting _bamboraStoreSetting;
        private readonly ISettingService _settingService;
        private readonly ICurrencyService _currencyService;
        private readonly ICustomerService _customerService;
        private readonly CurrencySettings _currencySettings;
        private readonly IWebHelper _webHelper;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly StoreInformationSettings _storeInformationSettings;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IEncryptionService _encryptionService;
        private readonly ILocalizationService _localizationService;
        private readonly ILogger _logger;
        private readonly IStoreLicenseKeyService _storeLicenseKeyService;
        //private readonly ISavedCardService _savedCardService;
        private readonly IOrderService _orderService;
        private readonly BamboraObjectContext _objectContext;
        private readonly IBamboraStoreSettingService _bamboraStoreSettingService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly HttpContextBase _httpContext;

        #endregion

        #region Ctor

        public BamboraPaymentProcessor(BamboraSettings bamboraSettings,
            ISettingService settingService, ICurrencyService currencyService,
            ICustomerService customerService, CurrencySettings currencySettings,
            IWebHelper webHelper, IOrderTotalCalculationService orderTotalCalculationService,
            StoreInformationSettings storeInformationSettings, IWorkContext workContext,
            IEncryptionService encryptionService, ILocalizationService localizationService,
            ILogger logger, IStoreLicenseKeyService storeLicenseKeyService,
            IOrderService orderService,
            BamboraObjectContext objectContext, IStoreContext storeContext,
            IBamboraStoreSettingService bamboraStoreSettingService, IPluginFinder pluginFinder,
            IGenericAttributeService genericAttributeService, HttpContextBase httpContext)
        {
            _bamboraSettings = bamboraSettings;
            _settingService = settingService;
            _currencyService = currencyService;
            _customerService = customerService;
            _currencySettings = currencySettings;
            _webHelper = webHelper;
            _orderTotalCalculationService = orderTotalCalculationService;
            _storeInformationSettings = storeInformationSettings;
            _workContext = workContext;
            _encryptionService = encryptionService;
            _localizationService = localizationService;
            _logger = logger;
            _storeLicenseKeyService = storeLicenseKeyService;
            _orderService = orderService;
            _objectContext = objectContext;
            _storeContext = storeContext;
            _bamboraStoreSettingService = bamboraStoreSettingService;
            _genericAttributeService = genericAttributeService;
            _httpContext = httpContext;

            var pluginDescriptor = pluginFinder.GetPluginDescriptorBySystemName("Payments.Bambora", LoadPluginsMode.All);
            if (pluginDescriptor != null && pluginDescriptor.Installed)
            {
                _bamboraStoreSetting = _bamboraStoreSettingService.GetByStore(_storeContext.CurrentStore.Id);
                if (_bamboraStoreSetting == null)
                    throw new Exception("Bambora plugin not configured");
            }
            if (_bamboraStoreSetting != null && _storeContext != null)
            {
                _bamboraStoreSetting = _bamboraStoreSettingService.GetByStore(_storeContext.CurrentStore.Id);
                _bambora = new Gateway()
                {
                    //MerchantId = 300200578,
                    MerchantId = Convert.ToInt32(_bamboraStoreSetting.MerchantId), //sodexo merchant
                    PaymentsApiKey = _bamboraStoreSetting.PaymentApiKey, // "6Cf20740690F41eeb3f539ad64b51c67",// sodexo payment api key
                    ReportingApiKey = _bamboraStoreSetting.ReportingApiKey,// "4e6Ff318bee64EA391609de89aD4CF5d",
                    ProfilesApiKey = _bamboraStoreSetting.ProfilesApiKey,// "D97D3BE1EE964A6193D17A571D9FBC80",
                    ApiVersion = "1"
                };
            }
        }

        #endregion

        #region Utilities
        /// <summary>
        /// Get Beanstream URL
        /// </summary>
        /// <returns>URL</returns>
        protected string GetBeanstreamUrl()
        {
            return "https://www.beanstream.com/scripts/payment/payment.asp";
        }
        /// <summary>
        /// Claculates MD5 hash
        /// </summary>
        /// <param name="input">Input string for the encoding</param>
        /// <returns>MD5 hash</returns>
        protected string CalculateMD5hash(string input)
        {
            var md5Hasher = new MD5CryptoServiceProvider();
            var hash = md5Hasher.ComputeHash(Encoding.Default.GetBytes(input));

            var output = new StringBuilder();
            foreach (var character in hash)
            {
                output.Append(character.ToString("x2"));
            }

            return output.ToString();
        }
        private XmlNode SendFDRequest(string transaction)
        {
            //SHA1 hash on XML string
            UTF8Encoding encoder = new UTF8Encoding();
            byte[] xml_byte = encoder.GetBytes(transaction);
            SHA1CryptoServiceProvider sha1_crypto = new SHA1CryptoServiceProvider();
            string hash = BitConverter.ToString(sha1_crypto.ComputeHash(xml_byte)).Replace("-", "");
            string hashed_content = hash.ToLower();

            //assign values to hashing and header variables
            string method = "POST\n";
            string type = "application/xml\n";//REST XML
            string time = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
            string url = "/transaction/v12";
            string keyID = _encryptionService.DecryptText(_bamboraStoreSetting.KeyID);//key ID
            string key = _encryptionService.DecryptText(_bamboraStoreSetting.HMAC);//Hmac key
            string hash_data = method + type + hashed_content + "\n" + time + "\n" + url;
            //hmac sha1 hash with key + hash_data
            HMAC hmac_sha1 = new HMACSHA1(Encoding.UTF8.GetBytes(key)); //key
            byte[] hmac_data = hmac_sha1.ComputeHash(Encoding.UTF8.GetBytes(hash_data)); //data
            //base64 encode on hmac_data
            string base64_hash = Convert.ToBase64String(hmac_data);

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            //begin HttpWebRequest // use  for production
            string destination = (_bamboraStoreSetting.UseSandbox ? _bamboraSettings.SandboxURL : _bamboraSettings.ProductionURL);
            HttpWebRequest web_request = (HttpWebRequest)WebRequest.Create(destination);
            web_request.Method = "POST";
            web_request.Accept = "application/xml";
            web_request.Headers.Add("x-gge4-date", time);
            web_request.Headers.Add("x-gge4-content-sha1", hashed_content);
            web_request.ContentLength = Encoding.UTF8.GetByteCount(transaction);
            web_request.ContentType = "application/xml";
            web_request.Headers["Authorization"] = "GGE4_API " + keyID + ":" + base64_hash;

            // send request as stream
            StreamWriter xml = null;
            xml = new StreamWriter(web_request.GetRequestStream());
            xml.Write(transaction);
            xml.Flush();
            xml.Close();

            //get response and read into string
            string response_string;
            try
            {
                HttpWebResponse web_response = (HttpWebResponse)web_request.GetResponse();
                using (StreamReader response_stream = new StreamReader(web_response.GetResponseStream()))
                {
                    response_string = response_stream.ReadToEnd();
                    response_stream.Close();
                }
                //load xml
                XmlDocument xmldoc = new XmlDocument();
                xmldoc.LoadXml(response_string);
                XmlNodeList nodelist = xmldoc.SelectNodes("TransactionResult");

                return nodelist[0];
            }
            catch (System.Net.WebException ex)
            {
                using (StreamReader response_stream = new StreamReader(ex.Response.GetResponseStream()))
                {
                    response_string = response_stream.ReadToEnd();
                    response_stream.Close();
                    throw new Exception(response_string);
                }
            }
        }

        public string SerializeCustomValues(Dictionary<string, object> customValues)
        {
            if (customValues.Count == 0)
                return null;

            //XmlSerializer won't serialize objects that implement IDictionary by default.
            //http://msdn.microsoft.com/en-us/magazine/cc164135.aspx 

            //also see http://ropox.ru/tag/ixmlserializable/ (Russian language)

            var ds = new PaymentExtensions.DictionarySerializer(customValues);
            var xs = new XmlSerializer(typeof(PaymentExtensions.DictionarySerializer));

            using (var textWriter = new StringWriter())
            {
                using (var xmlWriter = XmlWriter.Create(textWriter))
                {
                    xs.Serialize(xmlWriter, ds);
                }
                var result = textWriter.ToString();
                return result;
            }
        }

        #endregion

        #region Methods
        /// <summary>
        /// Process a payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
        {

            return new ProcessPaymentResult() {NewPaymentStatus=Core.Domain.Payments.PaymentStatus.Paid };
        }
        /// <summary>
        /// Post process payment (used by payment gateways that require redirecting to a third-party URL)
        /// </summary>
        /// <param name="postProcessPaymentRequest">Payment info required for an order processing</param>
        public void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest)
        {

            PaymentResponse response = _bambora.Payments.PreAuthCompletion(postProcessPaymentRequest.Order.AuthorizationTransactionId, postProcessPaymentRequest.Order.OrderTotal);
            postProcessPaymentRequest.Order.AuthorizationTransactionId = response.TransactionId;
            postProcessPaymentRequest.Order.CaptureTransactionId = response.TransactionId;
            _orderService.UpdateOrder(postProcessPaymentRequest.Order);

            //_httpContext.Session["transactionId"] = response.TransactionId;
        }
        /// <summary>
        /// Returns a value indicating whether payment method should be hidden during checkout
        /// </summary>
        /// <param name="cart">Shoping cart</param>
        /// <returns>true - hide; false - display.</returns>
        public bool HidePaymentMethod(IList<Nop.Core.Domain.Orders.ShoppingCartItem> cart)
        {
            //you can put any logic here
            //for example, hide this payment method if all products in the cart are downloadable
            //or hide this payment method if current customer is from certain country
            return false;
        }

        /// <summary>
        /// Gets additional handling fee
        /// </summary>
        /// <param name="cart">Shoping cart</param>
        /// <returns>Additional handling fee</returns>
        public decimal GetAdditionalHandlingFee(IList<Nop.Core.Domain.Orders.ShoppingCartItem> cart)
        {
            var result = this.CalculateAdditionalFee(_orderTotalCalculationService, cart,
                _bamboraStoreSetting.AdditionalFee, _bamboraStoreSetting.AdditionalFeePercentage);
            return result;
        }

        /// <summary>
        /// Refunds a payment
        /// </summary>
        /// <param name="refundPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public RefundPaymentResult Refund(RefundPaymentRequest refundPaymentRequest)
        {
            //_bambora = new Gateway()
            //{
            //    //MerchantId = 300200578,
            //    MerchantId = 300205523, //sodexo merchant
            //    PaymentsApiKey = "6Cf20740690F41eeb3f539ad64b51c67",// sodexo payment api key
            //    ReportingApiKey = "4e6Ff318bee64EA391609de89aD4CF5d",
            //    ProfilesApiKey = "D97D3BE1EE964A6193D17A571D9FBC80",
            //    ApiVersion = "1"
            //};
            var result1 = _bambora.Payments.Return(refundPaymentRequest.Order.AuthorizationTransactionId, new ReturnRequest { Amount = refundPaymentRequest.AmountToRefund, OrderNumber = refundPaymentRequest.Order.Id.ToString() });
            if (result1.Message == "Approved" && result1.TransType == "R")
            {
                var result = new RefundPaymentResult();
                result.NewPaymentStatus = Core.Domain.Payments.PaymentStatus.Refunded;
                return result;
            }
            return null;
        }

        //public PaymentResponse Refund(RefundPaymentRequest refundPaymentRequest,string transactionId="0")
        //{
        //    var result = new PaymentResponse();
        //    result = _bambora.Payments.Return(refundPaymentRequest.Order.AuthorizationTransactionId, new ReturnRequest {Amount=refundPaymentRequest.AmountToRefund,OrderNumber=refundPaymentRequest.Order.Id.ToString() });
        //    return result;
        //}
        /// <summary>
        /// Voids a payment
        /// </summary>
        /// <param name="voidPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public VoidPaymentResult Void(VoidPaymentRequest voidPaymentRequest)
        {
            var result = new VoidPaymentResult();
            result.AddError("Void method not supported");
            return result;
        }
        /// <summary>
        /// Process recurring payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public ProcessPaymentResult ProcessRecurringPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult();
            result.AddError("Recurring payment not supported");
            return result;
        }
        /// <summary>
        /// Cancels a recurring payment
        /// </summary>
        /// <param name="cancelPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public CancelRecurringPaymentResult CancelRecurringPayment(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            var result = new CancelRecurringPaymentResult();
            result.AddError("Recurring payment not supported");
            return result;
        }
        public bool CanRePostProcessPayment(Nop.Core.Domain.Orders.Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            //it's not a redirection payment method. So we always return false
            return false;
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
            controllerName = "BamboraPayment";
            routeValues = new RouteValueDictionary() { { "Namespaces", "Nop.Plugin.Payments.Bambora.Controllers" }, { "area", null } };
        }


        /// <summary>
        /// Gets a route for payment info
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetPaymentInfoRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "PaymentInfo";
            controllerName = "BamboraPayment";
            routeValues = new RouteValueDictionary() { { "Namespaces", "Nop.Plugin.Payments.Bambora.Controllers" }, { "area", null } };
        }

        /// <summary>
        /// Captures payment
        /// </summary>
        /// <param name="capturePaymentRequest">Capture payment request</param>
        /// <returns>Capture payment result</returns>
        public CapturePaymentResult Capture(CapturePaymentRequest capturePaymentRequest)
        {
            var result = new CapturePaymentResult();
            result.AddError("Capture method not supported");
            return result;
        }

        /// <summary>
        /// Gets a value indicating whether capture is supported
        /// </summary>
        public bool SupportCapture
        {
            get
            {
                return true;
            }
        }
        /// <summary>
        /// Gets a value indicating whether partial refund is supported
        /// </summary>
        public bool SupportPartiallyRefund
        {
            get
            {
                return true;
            }
        }
        /// <summary>
        /// Gets a value indicating whether refund is supported
        /// </summary>
        public bool SupportRefund
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets a value indicating whether void is supported
        /// </summary>
        public bool SupportVoid
        {
            get
            {
                return true;
            }
        }
        /// <summary>
        /// Gets a recurring payment type of payment method
        /// </summary>
        public RecurringPaymentType RecurringPaymentType
        {
            get
            {
                if (_bamboraStoreSetting.EnableRecurringPayments)
                {
                    return RecurringPaymentType.Manual;
                }
                else
                {
                    return RecurringPaymentType.NotSupported;
                }
            }
        }
        /// <summary>
        /// Gets a payment method type
        /// </summary>
        public PaymentMethodType PaymentMethodType
        {
            get
            {
                return PaymentMethodType.Standard;
            }
        }

        /// <summary>
        /// Gets a value indicating whether we should display a payment information page for this plugin
        /// </summary>
        public bool SkipPaymentInfo
        {
            get
            {
                return false;
            }
        }
        /// <summary>
        /// Get type of the controller
        /// </summary>
        /// <returns>Controller type</returns>
        public Type GetControllerType()
        {
            return typeof(BamboraPaymentController);
        }

        /// <summary>
        /// Install the plugin
        /// </summary>
        public override void Install()
        {
            //database objects
            // _objectContext.Install();

            //settings
            var settings = new BamboraSettings
            {
                SandboxURL =_bamboraSettings.SandboxURL,
                ProductionURL = _bamboraSettings.ProductionURL
            };
            //settings
            _settingService.SaveSetting(new BamboraSettings());
            var defaultStoreSetting = new BamboraStoreSetting
            {
                UseSandbox = true,
                TransactionMode = (int)TransactMode.AuthorizeAndCapture,
                StoreId = _storeContext.CurrentStore.Id
            };

            var isbamboraSettingsExixts = _bamboraStoreSettingService.GetByStore(_storeContext.CurrentStore.Id);
            if (isbamboraSettingsExixts.StoreId == _storeContext.CurrentStore.Id)
            {
                _bamboraStoreSettingService.Update(defaultStoreSetting);

            }
            else
            {
                _bamboraStoreSettingService.Insert(defaultStoreSetting);
            }
            //locales
            this.AddOrUpdatePluginLocaleResource("Bambora.Notes", "If you're using this gateway, ensure that your primary store currency is supported by Bambora payment Gateway.  Recurring Payments and Card Saving require the TransArmor service on your merchant account.  Read more <a href=\"http://bambora.com/transarmor-tokenization\">here</a>.");
            this.AddOrUpdatePluginLocaleResource("Bambora.Plugin.Fields.UseSandbox", "Use Sandbox");
            this.AddOrUpdatePluginLocaleResource("Bambora.Plugin.Fields.UseSandbox.Hint", "Check to enable Sandbox (testing environment).");
            this.AddOrUpdatePluginLocaleResource("Bambora.Plugin.Fields.TransactModeValues", "Transaction mode");
            this.AddOrUpdatePluginLocaleResource("Bambora.Plugin.Fields.TransactModeValues.Hint", "Choose transaction mode");
            this.AddOrUpdatePluginLocaleResource("Bambora.Plugin.Fields.HMAC", "HMAC");
            this.AddOrUpdatePluginLocaleResource("Bambora.Plugin.Fields.HMAC.Hint", "The HMAC for your terminal");
            this.AddOrUpdatePluginLocaleResource("Bambora.Plugin.Fields.GatewayID", "Gateway ID");
            this.AddOrUpdatePluginLocaleResource("Bambora.Plugin.Fields.GatewayID.Hint", "Specify gateway identifier.");
            this.AddOrUpdatePluginLocaleResource("Bambora.Plugin.Fields.Password", "Password");
            this.AddOrUpdatePluginLocaleResource("Bambora.Plugin.Fields.Password.Hint", "Specify terminal password.");
            this.AddOrUpdatePluginLocaleResource("Bambora.Plugin.Fields.KeyID", "Key ID");
            this.AddOrUpdatePluginLocaleResource("Bambora.Plugin.Fields.KeyID.Hint", "Specify key identifier.");
            this.AddOrUpdatePluginLocaleResource("Bambora.Plugin.Fields.AdditionalFee", "Additional fee");
            this.AddOrUpdatePluginLocaleResource("Bambora.Plugin.Fields.AdditionalFee.Hint", "Enter additional fee to charge your customers.");
            this.AddOrUpdatePluginLocaleResource("Bambora.Plugin.Fields.AdditionalFeePercentage", "Additinal fee. Use percentage");
            this.AddOrUpdatePluginLocaleResource("Bambora.Plugin.Fields.AdditionalFeePercentage.Hint", "Determines whether to apply a percentage additional fee to the order total. If not enabled, a fixed value is used.");
            this.AddOrUpdatePluginLocaleResource("Bambora.ExpiryDateError", "Card expired");
            this.AddOrUpdatePluginLocaleResource("Bambora.Plugin.Fields.LicenseKey", "License Keys");
            this.AddOrUpdatePluginLocaleResource("Bambora.Plugin.Fields.LicenseKey.Hint", "The license key should have been emailed to you after your purchase.  If not, contact support@bitshiftweb.com");
            this.AddOrUpdatePluginLocaleResource("Bambora.UnlicensedError", "This plugin is not licensed for this URL.  Purchase a license at http://www.bitshiftweb.com");
            this.AddOrUpdatePluginLocaleResource("Bambora.TechnicalError", "There has been an unexpected error while processing your payment.");
            this.AddOrUpdatePluginLocaleResource("Bambora.Payment.SaveCard", "Save Card");
            this.AddOrUpdatePluginLocaleResource("Bambora.Payment.SaveCard.Hint", "Save card for future use.  Your card number is tokenized on Bambora's servers and not stored on our site.");
            this.AddOrUpdatePluginLocaleResource("Bambora.Plugin.Fields.EnableRecurringPayments", "Enable Recurring Payments");
            this.AddOrUpdatePluginLocaleResource("Bambora.Plugin.Fields.EnableRecurringPayments.Hint", "Allows manual recurring payments by using the TransArmor Token");
            this.AddOrUpdatePluginLocaleResource("Bambora.Plugin.Fields.EnableCardSaving", "Enable Card Saving");
            this.AddOrUpdatePluginLocaleResource("Bambora.Plugin.Fields.EnableCardSaving.Hint", "Allows customers to choose to save a card when they use it.  The TransArmor Token is saved instead of the CC number");
            this.AddOrUpdatePluginLocaleResource("Bambora.Payment.UseCardLabel", "Use");
            this.AddOrUpdatePluginLocaleResource("Bambora.Payment.CardDescription", "{0} ending in {1}");
            this.AddOrUpdatePluginLocaleResource("Bambora.Payment.ExpirationDescription", "Expires {0}/{1}");
            this.AddOrUpdatePluginLocaleResource("Bambora.Payment.ExpiredLabel", "Expired");
            this.AddOrUpdatePluginLocaleResource("Bambora.Payment.SavedCardsLabel", "Saved Cards");
            this.AddOrUpdatePluginLocaleResource("Bambora.Payment.NewCardLabel", "Enter new card");
            this.AddOrUpdatePluginLocaleResource("Bambora.Payment.PurchaseOrder", "Purchase Order");
            this.AddOrUpdatePluginLocaleResource("Bambora.Plugin.Fields.EnablePurchaseOrderNumber", "Enable Purchase Order Number");
            this.AddOrUpdatePluginLocaleResource("Bambora.Plugin.Fields.EnablePurchaseOrderNumber.Hint", "Will optionally capture a purchase order number and append it to the Authorization Transaction ID in the Order Details");
            this.AddOrUpdatePluginLocaleResource("Bambora.Plugin.Fields.SandboxURL", "Sandbox URL");
            this.AddOrUpdatePluginLocaleResource("Bambora.Plugin.Fields.SandboxURL.Hint", "Where to send sandbox transactions");
            this.AddOrUpdatePluginLocaleResource("Bambora.Plugin.Fields.ProductionURL", "Production URL");
            this.AddOrUpdatePluginLocaleResource("Bambora.Plugin.Fields.ProductionURL.Hint", "Where to send real transactions");
            this.AddOrUpdatePluginLocaleResource("Bambora.Plugin.Fields.StoreID", "Store");
            this.AddOrUpdatePluginLocaleResource("Bambora.Plugin.Fields.StoreID.Hint", "The store that these settings apply to.  The Default Store settings will be used for any store that doesn't have settings.");
            this.AddOrUpdatePluginLocaleResource("Bambora.Configure", "Global Settings");
            this.AddOrUpdatePluginLocaleResource("Bambora.Stores", "Store Specific Settings");
            this.AddOrUpdatePluginLocaleResource("Bambora.Storenotes", "Set your store specific settings here.  A store without it's own entry will use the Default Store Settings");
            this.AddOrUpdatePluginLocaleResource("Bambora.Stores.Revert", "Revert to Default Settings");
            this.AddOrUpdatePluginLocaleResource("Bambora.StoreSettingsSaved", "Store settings saved successfully");
            this.AddOrUpdatePluginLocaleResource("Bambora.SavedCards", "Saved Cards");
            this.AddOrUpdatePluginLocaleResource("Bambora.SavedCards.NoCards", "You don't have any cards saved.  Complete an order and check the \"Save Card\" box during checkout.");
            this.AddOrUpdatePluginLocaleResource("Bambora.SavedCards.Fields.Type", "Type");
            this.AddOrUpdatePluginLocaleResource("Bambora.SavedCards.Fields.Name", "Cardholder Name");
            this.AddOrUpdatePluginLocaleResource("Bambora.SavedCards.Fields.Last4", "Number");
            this.AddOrUpdatePluginLocaleResource("Bambora.SavedCards.Fields.Expires", "Expires");
            this.AddOrUpdatePluginLocaleResource("Bambora.savedcards.description", "Review your saved cards here.  You can save a new card during checkout.");
            this.AddOrUpdatePluginLocaleResource("Bambora.Plugin.Fields.PaymentPageID", "Payment Page ID");
            this.AddOrUpdatePluginLocaleResource("Bambora.Plugin.Fields.PaymentPageID.Hint", "The payment page to use during checkout");
            this.AddOrUpdatePluginLocaleResource("Bambora.Plugin.Fields.TransactionKey", "Transaction Key");
            this.AddOrUpdatePluginLocaleResource("Bambora.Plugin.Fields.TransactionKey.Hint", "The payment page's transaction key");
            this.AddOrUpdatePluginLocaleResource("Bambora.Plugin.Fields.ResponseKey", "Response Key");
            this.AddOrUpdatePluginLocaleResource("Bambora.Plugin.Fields.ResponseKey.Hint", "The response key Bambora will send back to the plugin");

            this.AddOrUpdatePluginLocaleResource("Bambora.Plugin.Fields.MerchantId", "Merchant Id");
            this.AddOrUpdatePluginLocaleResource("Bambora.Plugin.Fields.HashKey", "Hash Key");
            this.AddOrUpdatePluginLocaleResource("Bambora.Plugin.Fields.EncryptedSecureKey", "Encrypted Key");

            base.Install();
        }

        /// <summary>
        /// Uninstall the plugin
        /// </summary>
        public override void Uninstall()
        {
            //settings
            _settingService.DeleteSetting<BamboraSettings>();

            //locales
            this.DeletePluginLocaleResource("Bambora.Notes");
            this.DeletePluginLocaleResource("Bambora.Plugin.Fields.UseSandbox");
            this.DeletePluginLocaleResource("Bambora.Plugin.Fields.UseSandbox.Hint");
            this.DeletePluginLocaleResource("Bambora.Plugin.Fields.TransactModeValues");
            this.DeletePluginLocaleResource("Bambora.Plugin.Fields.TransactModeValues.Hint");
            this.DeletePluginLocaleResource("Bambora.Plugin.Fields.HMAC");
            this.DeletePluginLocaleResource("Bambora.Plugin.Fields.HMAC.Hint");
            this.DeletePluginLocaleResource("Bambora.Plugin.Fields.GatewayID");
            this.DeletePluginLocaleResource("Bambora.Plugin.Fields.GatewayID.Hint");
            this.DeletePluginLocaleResource("Bambora.Plugin.Fields.Password");
            this.DeletePluginLocaleResource("Bambora.Plugin.Fields.Password.Hint");
            this.DeletePluginLocaleResource("Bambora.Plugin.Fields.KeyID");
            this.DeletePluginLocaleResource("Bambora.Plugin.Fields.KeyID.Hint");
            this.DeletePluginLocaleResource("Bambora.Plugin.Fields.AdditionalFee");
            this.DeletePluginLocaleResource("Bambora.Plugin.Fields.AdditionalFee.Hint");
            this.DeletePluginLocaleResource("Bambora.Plugin.Fields.AdditionalFeePercentage");
            this.DeletePluginLocaleResource("Bambora.Plugin.Fields.AdditionalFeePercentage.Hint");
            this.DeletePluginLocaleResource("Bambora.ExpiryDateError");
            this.DeletePluginLocaleResource("Bambora.Fields.LicenseKey");
            this.DeletePluginLocaleResource("Bambora.Fields.LicenseKey.Hint");
            this.DeletePluginLocaleResource("Bambora.TechnicalError");
            this.DeletePluginLocaleResource("Bambora.UnlicensedError");
            this.DeletePluginLocaleResource("Bambora.Payment.SaveCard");
            this.DeletePluginLocaleResource("Bambora.Plugin.Fields.EnableRecurringPayments");
            this.DeletePluginLocaleResource("Bambora.Plugin.Fields.EnableRecurringPayments.Hint");
            this.DeletePluginLocaleResource("Bambora.Plugin.Fields.EnableCardSaving");
            this.DeletePluginLocaleResource("Bambora.Plugin.Fields.EnableCardSaving.Hint");
            this.DeletePluginLocaleResource("Bambora.Payment.UseCardLabel");
            this.DeletePluginLocaleResource("Bambora.Payment.CardDescription");
            this.DeletePluginLocaleResource("Bambora.Payment.ExpirationDescription");
            this.DeletePluginLocaleResource("Bambora.Payment.ExpiredLabel");
            this.DeletePluginLocaleResource("Bambora.Payment.SavedCardsLabel");
            this.DeletePluginLocaleResource("Bambora.Payment.NewCardLabel");
            this.DeletePluginLocaleResource("Bambora.Plugin.Fields.EnablePurchaseOrderNumber");
            this.DeletePluginLocaleResource("Bambora.Plugin.Fields.EnablePurchaseOrderNumber.Hint");
            this.DeletePluginLocaleResource("Bambora.Plugin.Fields.SandboxURL");
            this.DeletePluginLocaleResource("Bambora.Plugin.Fields.SandboxURL.Hint");
            this.DeletePluginLocaleResource("Bambora.Plugin.Fields.ProductionURL");
            this.DeletePluginLocaleResource("Bambora.Plugin.Fields.ProductionURL.Hint");
            this.DeletePluginLocaleResource("Bambora.Plugin.Fields.StoreID");
            this.DeletePluginLocaleResource("Bambora.Plugin.Fields.StoreID.Hint");
            this.DeletePluginLocaleResource("Bambora.Configure");
            this.DeletePluginLocaleResource("Bambora.Stores");
            this.DeletePluginLocaleResource("Bambora.Storenotes");
            this.DeletePluginLocaleResource("Bambora.Stores.Revert");
            this.DeletePluginLocaleResource("Bambora.StoreSettingsSaved");
            this.DeletePluginLocaleResource("Bambora.SavedCards");
            this.DeletePluginLocaleResource("Bambora.SavedCards.NoCards");
            this.DeletePluginLocaleResource("Bambora.SavedCards.Fields.Type");
            this.DeletePluginLocaleResource("Bambora.SavedCards.Fields.Name");
            this.DeletePluginLocaleResource("Bambora.SavedCards.Fields.Last4");
            this.DeletePluginLocaleResource("Bambora.SavedCards.Fields.Expires");
            this.DeletePluginLocaleResource("Bambora.savedcards.description");
            this.DeletePluginLocaleResource("Bambora.Plugin.Fields.PaymentPageID");
            this.DeletePluginLocaleResource("Bambora.Plugin.Fields.PaymentPageID.Hint");
            this.DeletePluginLocaleResource("Bambora.Plugin.Fields.TransactionKey");
            this.DeletePluginLocaleResource("Bambora.Plugin.Fields.TransactionKey.Hint");
            this.DeletePluginLocaleResource("Bambora.Plugin.Fields.ResponseKey");
            this.DeletePluginLocaleResource("Bambora.Plugin.Fields.ResponseKey.Hint");
            this.DeletePluginLocaleResource("Bambora.Payment.PurchaseOrder");
            this.DeletePluginLocaleResource("Bambora.Payment.SaveCard.Hint");
            this.DeletePluginLocaleResource("Bambora.Plugin.Fields.LicenseKey");
            this.DeletePluginLocaleResource("Bambora.Plugin.Fields.LicenseKey.Hint");
            this.DeletePluginLocaleResource("Bambora.Plugin.Fields.MerchantId");
            this.DeletePluginLocaleResource("Bambora.Plugin.Fields.HashKey");
            this.DeletePluginLocaleResource("Bambora.Plugin.Fields.EncryptedSecureKey");

            // _objectContext.Uninstall();

            base.Uninstall();
        }
        #endregion
    }

}
