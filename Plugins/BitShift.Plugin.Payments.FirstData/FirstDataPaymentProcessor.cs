using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Web.Routing;
using System.Security.Cryptography;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Web;
using Nop.Core;
using Nop.Core.Domain;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Plugins;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using BitShift.Plugin.Payments.FirstData.Domain;
using BitShift.Plugin.Payments.FirstData.Controllers;
using BitShift.Plugin.Payments.FirstData.Data;
using BitShift.Plugin.Payments.FirstData.Services;
using Nop.Services.Common;

namespace BitShift.Plugin.Payments.FirstData
{
    /// <summary>
    /// FirstData payment processor
    /// </summary>
    public class FirstDataPaymentProcessor : BasePlugin, IPaymentMethod, IWidgetPlugin
    {
        #region Fields

        private readonly FirstDataSettings _firstDataSettings;
        private FirstDataStoreSetting _firstDataStoreSetting;
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
        private readonly ISavedCardService _savedCardService;
        private readonly IOrderService _orderService;
        private readonly FirstDataObjectContext _objectContext;
        private readonly IFirstDataStoreSettingService _firstDataStoreSettingService;
        private readonly IGenericAttributeService _genericAttributeService;

        #endregion

        #region Ctor

        public FirstDataPaymentProcessor(FirstDataSettings firstDataSettings,
            ISettingService settingService, ICurrencyService currencyService,
            ICustomerService customerService, CurrencySettings currencySettings,
            IWebHelper webHelper, IOrderTotalCalculationService orderTotalCalculationService,
            StoreInformationSettings storeInformationSettings, IWorkContext workContext,
            IEncryptionService encryptionService, ILocalizationService localizationService,
            ILogger logger, IStoreLicenseKeyService storeLicenseKeyService,
            ISavedCardService savedCardService, IOrderService orderService,
            FirstDataObjectContext objectContext, IStoreContext storeContext,
            IFirstDataStoreSettingService firstDataStoreSettingService, IPluginFinder pluginFinder,
            IGenericAttributeService genericAttributeService)
        {
            _firstDataSettings = firstDataSettings;
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
            _savedCardService = savedCardService;
            _orderService = orderService;
            _objectContext = objectContext;
            _storeContext = storeContext;
            _firstDataStoreSettingService = firstDataStoreSettingService;
            _genericAttributeService = genericAttributeService;

            var pluginDescriptor = pluginFinder.GetPluginDescriptorBySystemName("BitShift.Payments.FirstData", LoadPluginsMode.All);
            if (pluginDescriptor != null && pluginDescriptor.Installed)
            {
                _firstDataStoreSetting = _firstDataStoreSettingService.GetByStore(_storeContext.CurrentStore.Id);
                if (_firstDataStoreSetting == null)
                    throw new Exception("First Data plugin not configured");
            }
        }

        #endregion

        #region Utilities

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
            string keyID = _encryptionService.DecryptText(_firstDataStoreSetting.KeyID);//key ID
            string key = _encryptionService.DecryptText(_firstDataStoreSetting.HMAC);//Hmac key
            string hash_data = method + type + hashed_content + "\n" + time + "\n" + url;
            //hmac sha1 hash with key + hash_data
            HMAC hmac_sha1 = new HMACSHA1(Encoding.UTF8.GetBytes(key)); //key
            byte[] hmac_data = hmac_sha1.ComputeHash(Encoding.UTF8.GetBytes(hash_data)); //data
            //base64 encode on hmac_data
            string base64_hash = Convert.ToBase64String(hmac_data);

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            //begin HttpWebRequest // use  for production
            string destination = (_firstDataStoreSetting.UseSandbox ? _firstDataSettings.SandboxURL : _firstDataSettings.ProductionURL);
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
            var result = new ProcessPaymentResult();

            var customer = _customerService.GetCustomerById(processPaymentRequest.CustomerId);

            if (_firstDataStoreSetting.TransactionMode == (int)TransactMode.HostedPaymentPage)
            {
                string authAttribute = "";
                GenericAttribute auth = _genericAttributeService.GetAttributeByEntityId_Keygroup_KeyName_StoreId(processPaymentRequest.CustomerId, "Customer", Constants.AuthorizationAttribute, _storeContext.CurrentStore.Id);
                authAttribute = auth.Value;

                if (!string.IsNullOrEmpty(authAttribute))
                {
                    var authSplit = authAttribute.Split(new[] { '|' });
                    result.AuthorizationTransactionId = authSplit[0];
                    processPaymentRequest.CustomValues.Add(Constants.TransactionTag, authSplit[1]);
                    result.AuthorizationTransactionResult = string.Format("Approved on Payment Page");
                    result.AllowStoringCreditCardNumber = false;
                    result.NewPaymentStatus = PaymentStatus.Authorized;
                }
                else
                {
                    result.AddError(_localizationService.GetResource("BitShift.Plugin.FirstData.HostedPaymentPage.AuthorizationEmpty"));
                }
            }
            else
            {
                #region Else
                var cardNumber = processPaymentRequest.CreditCardNumber;
                bool saveCard = processPaymentRequest.CustomValues.ContainsKey(Constants.SaveCard) && Convert.ToBoolean(processPaymentRequest.CustomValues[Constants.SaveCard]);
                bool useSavedCard = processPaymentRequest.CustomValues.ContainsKey(Constants.SavedCardId);

                StringBuilder string_builder = new StringBuilder();
                try
                {
                    using (StringWriter string_writer = new StringWriter(string_builder))
                    {
                        using (XmlTextWriter xml_writer = new XmlTextWriter(string_writer))
                        {     //build XML string
                            xml_writer.Formatting = Formatting.Indented;
                            xml_writer.WriteStartElement("Transaction");
                            xml_writer.WriteElementString("ExactID", _encryptionService.DecryptText(_firstDataStoreSetting.GatewayID));//Gateway ID
                            xml_writer.WriteElementString("Password", _encryptionService.DecryptText(_firstDataStoreSetting.Password));//Password
                            xml_writer.WriteElementString("Transaction_Type", (_firstDataStoreSetting.TransactionMode == (int)TransactMode.AuthorizeAndCapture ? TransactionType.Purchase : TransactionType.PreAuth));  //check settings
                            xml_writer.WriteElementString("DollarAmount", processPaymentRequest.OrderTotal.ToString());

                            if (useSavedCard)
                            {
                                int savedCardId = Convert.ToInt32(processPaymentRequest.CustomValues[Constants.SavedCardId]);
                                SavedCard card = _savedCardService.GetById(savedCardId);
                                if (card == null)
                                {
                                    throw new NullReferenceException("Saved Card #" + savedCardId.ToString() + " is null");
                                }

                                xml_writer.WriteElementString("CardHoldersName", card.CardholderName);
                                xml_writer.WriteElementString("TransarmorToken", card.Token);
                                xml_writer.WriteElementString("CardType", card.CardType);
                                xml_writer.WriteElementString("Expiry_Date", card.ExpireMonth.ToString("00") + card.ExpireYear.ToString().Substring(2, 2));
                            }
                            else
                            {
                                xml_writer.WriteElementString("Expiry_Date", processPaymentRequest.CreditCardExpireMonth.ToString("00") + processPaymentRequest.CreditCardExpireYear.ToString().Substring(2, 2));
                                xml_writer.WriteElementString("CardHoldersName", processPaymentRequest.CreditCardName);
                                xml_writer.WriteElementString("Card_Number", cardNumber);
                                xml_writer.WriteElementString("VerificationStr1", customer.BillingAddress.Address1 ?? "" + "|"
                                                                                + customer.BillingAddress.ZipPostalCode ?? "" + "|"
                                                                                + (customer.BillingAddress.StateProvince != null ? customer.BillingAddress.StateProvince.Name : "") + "|"
                                                                                + customer.BillingAddress.Country.ThreeLetterIsoCode);

                                xml_writer.WriteElementString("VerificationStr2", processPaymentRequest.CreditCardCvv2);
                                xml_writer.WriteElementString("CVD_Presence_Ind", "1");
                            }

                            xml_writer.WriteElementString("Client_Email", customer.Email);
                            xml_writer.WriteElementString("Currency", _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId).CurrencyCode);
                            xml_writer.WriteElementString("Customer_Ref", customer.CustomerGuid.ToString());
                            xml_writer.WriteElementString("Reference_No", processPaymentRequest.OrderGuid.ToString());
                            xml_writer.WriteElementString("Client_IP", _webHelper.GetCurrentIpAddress());
                            xml_writer.WriteElementString("ZipCode", customer.BillingAddress.ZipPostalCode ?? "");
                            xml_writer.WriteEndElement();
                            String xml_string = string_builder.ToString();

                            if (!_firstDataStoreSetting.UseSandbox && !_storeLicenseKeyService.IsLicensed(HttpContext.Current.Request.Url.Host) && processPaymentRequest.OrderTotal > 1m)
                            {
                                result.AddError(_localizationService.GetResource("BitShift.Plugin.FirstData.UnlicensedError"));
                            }
                            else
                            {
                                try
                                {
                                    var response = SendFDRequest(xml_string);
                                    if (response.SelectSingleNode("Transaction_Approved").InnerText == "true")
                                    {
                                        result.AuthorizationTransactionId = response.SelectSingleNode("Authorization_Num").InnerText;
                                        processPaymentRequest.CustomValues.Add(Constants.TransactionTag, response.SelectSingleNode("Transaction_Tag").InnerText);
                                        result.AuthorizationTransactionResult = string.Format("Approved ({0}: {1})", response.SelectSingleNode("EXact_Resp_Code").InnerText, response.SelectSingleNode("EXact_Message").InnerText);
                                        if (_firstDataStoreSetting.TransactionMode == (int)TransactMode.AuthorizeAndCapture)
                                        {
                                            result.CaptureTransactionId = response.SelectSingleNode("Authorization_Num").InnerText;
                                        }

                                        result.AvsResult = response.SelectSingleNode("AVS").InnerText;


                                        ////    if (true)
                                        ////{
                                        //result.AuthorizationTransactionId = "4564136822";
                                        //processPaymentRequest.CustomValues.Add(Constants.TransactionTag, "ET113809");
                                        //result.AuthorizationTransactionResult = string.Format("Approved ({0}: {1})", "00", "Transaction Normal");
                                        //if (_firstDataStoreSetting.TransactionMode == (int)TransactMode.AuthorizeAndCapture)
                                        //{
                                        //    result.CaptureTransactionId = "4564136822";
                                        //}
                                        //result.AvsResult = "1";
                                        ////

                                        result.AvsResult = response.SelectSingleNode("AVS").InnerText;
                                        result.AllowStoringCreditCardNumber = false;

                                        result.NewPaymentStatus = (_firstDataStoreSetting.TransactionMode == (int)TransactMode.AuthorizeAndCapture ? PaymentStatus.Paid : PaymentStatus.Authorized);

                                        if (saveCard && !useSavedCard)
                                        {
                                            var token = response.SelectSingleNode("TransarmorToken").InnerText;

                                            var existingCard = _savedCardService.GetByToken(customer.Id, token);
                                            if (existingCard == null) //don't save the same card twice
                                            {
                                                SavedCard card = new SavedCard();
                                                card.BillingAddress_Id = customer.BillingAddress.Id;
                                                card.CardholderName = processPaymentRequest.CreditCardName;
                                                card.CardType = response.SelectSingleNode("CardType").InnerText;
                                                card.Customer_Id = customer.Id;
                                                card.ExpireMonth = processPaymentRequest.CreditCardExpireMonth;
                                                card.ExpireYear = processPaymentRequest.CreditCardExpireYear;
                                                card.Token = token;
                                                _savedCardService.Insert(card);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var errorMessage = _localizationService.GetLocaleStringResourceByName($"BitShift.Plugin.FirstData.CustomError.{response.SelectSingleNode("EXact_Resp_Code").InnerText}");
                                        if (errorMessage != null)
                                        {
                                            result.AddError(errorMessage.ResourceValue);
                                        }
                                        else
                                        {
                                            result.AddError($"{response.SelectSingleNode("EXact_Resp_Code").InnerText}: {response.SelectSingleNode("EXact_Message").InnerText}");
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    result.AddError(_localizationService.GetResource("BitShift.Plugin.FirstData.TechnicalError"));
                                    _logger.Error("Error processing payment", ex, _workContext.CurrentCustomer);
                                }
                            }
                        }
                    }

                    processPaymentRequest.CustomValues.Remove(Constants.SaveCard);
                    if (processPaymentRequest.CustomValues.ContainsKey(Constants.PO) && processPaymentRequest.CustomValues[Constants.PO].ToString() == "")
                        processPaymentRequest.CustomValues.Remove(Constants.PO);
                }
                catch (Exception ex)
                {
                    result.AddError(_localizationService.GetResource("BitShift.Plugin.FirstData.TechnicalError"));
                    _logger.Error("Error processing payment.", ex, _workContext.CurrentCustomer);
                }
                #endregion Else
            }

            return result;
        }

        /// <summary>
        /// Post process payment (used by payment gateways that require redirecting to a third-party URL)
        /// </summary>
        /// <param name="postProcessPaymentRequest">Payment info required for an order processing</param>
        public void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            //nothing
        }

        /// <summary>
        /// Gets additional handling fee
        /// </summary>
        /// <param name="cart">Shoping cart</param>
        /// <returns>Additional handling fee</returns>
        public decimal GetAdditionalHandlingFee(IList<ShoppingCartItem> cart)
        {
            var result = this.CalculateAdditionalFee(_orderTotalCalculationService, cart,
                _firstDataStoreSetting.AdditionalFee, _firstDataStoreSetting.AdditionalFeePercentage);
            return result;
        }

        /// <summary>
        /// Captures payment
        /// </summary>
        /// <param name="capturePaymentRequest">Capture payment request</param>
        /// <returns>Capture payment result</returns>
        public CapturePaymentResult Capture(CapturePaymentRequest capturePaymentRequest)
        {
            var result = new CapturePaymentResult();
            var customValues = capturePaymentRequest.Order.DeserializeCustomValues();

            StringBuilder string_builder = new StringBuilder();
            using (StringWriter string_writer = new StringWriter(string_builder))
            {
                using (XmlTextWriter xml_writer = new XmlTextWriter(string_writer))
                {     //build XML string
                    xml_writer.Formatting = Formatting.Indented;
                    xml_writer.WriteStartElement("Transaction");
                    xml_writer.WriteElementString("ExactID", _encryptionService.DecryptText(_firstDataStoreSetting.GatewayID));//Gateway ID
                    xml_writer.WriteElementString("Password", _encryptionService.DecryptText(_firstDataStoreSetting.Password));//Password
                    xml_writer.WriteElementString("Transaction_Type", TransactionType.TaggedPreAuthCompletion);
                    xml_writer.WriteElementString("DollarAmount", capturePaymentRequest.Order.OrderTotal.ToString());
                    xml_writer.WriteElementString("Authorization_Num", capturePaymentRequest.Order.AuthorizationTransactionId);
                    xml_writer.WriteElementString("Reference_No", capturePaymentRequest.Order.Id.ToString());
                    xml_writer.WriteElementString("Transaction_Tag", customValues[Constants.TransactionTag].ToString());
                    xml_writer.WriteEndElement();
                    String xml_string = string_builder.ToString();

                    try
                    {
                        var response = SendFDRequest(xml_string);
                        if (response.SelectSingleNode("Transaction_Approved").InnerText == "true")
                        {
                            result.CaptureTransactionId = response.SelectSingleNode("Authorization_Num").InnerText;
                            customValues[Constants.TransactionTag] = response.SelectSingleNode("Transaction_Tag").InnerText;
                            result.CaptureTransactionResult = string.Format("Approved ({0}: {1})", response.SelectSingleNode("EXact_Resp_Code").InnerText, response.SelectSingleNode("EXact_Message").InnerText);
                            result.NewPaymentStatus = PaymentStatus.Paid;

                            capturePaymentRequest.Order.CustomValuesXml = SerializeCustomValues(customValues);
                            _orderService.UpdateOrder(capturePaymentRequest.Order);
                        }
                        else
                        {
                            var errorMessage = _localizationService.GetLocaleStringResourceByName($"BitShift.Plugin.FirstData.CustomError.{response.SelectSingleNode("EXact_Resp_Code").InnerText}");
                            if (errorMessage != null)
                            {
                                result.AddError(errorMessage.ResourceValue);
                            }
                            else
                            {
                                result.AddError($"{response.SelectSingleNode("EXact_Resp_Code").InnerText}: {response.SelectSingleNode("EXact_Message").InnerText}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        result.AddError(ex.Message);
                        _logger.Error("Error capturing payment", ex, capturePaymentRequest.Order.Customer);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Refunds a payment
        /// </summary>
        /// <param name="refundPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public RefundPaymentResult Refund(RefundPaymentRequest refundPaymentRequest)
        {
            var result = new RefundPaymentResult();
            var customValues = refundPaymentRequest.Order.DeserializeCustomValues();

            if (refundPaymentRequest.Order.CaptureTransactionId != null && refundPaymentRequest.Order.CaptureTransactionId.Contains("|"))
            {
                if (customValues.ContainsKey(Constants.TransactionTag))
                    customValues.Remove(Constants.TransactionTag);

                customValues.Add(Constants.TransactionTag, refundPaymentRequest.Order.CaptureTransactionId.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries)[1]);
                refundPaymentRequest.Order.CaptureTransactionId = refundPaymentRequest.Order.CaptureTransactionId.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries)[0];
                refundPaymentRequest.Order.CustomValuesXml = SerializeCustomValues(customValues);
            }

            StringBuilder string_builder = new StringBuilder();
            try
            {
                using (StringWriter string_writer = new StringWriter(string_builder))
                {
                    using (XmlTextWriter xml_writer = new XmlTextWriter(string_writer))
                    {     //build XML string
                        xml_writer.Formatting = Formatting.Indented;
                        xml_writer.WriteStartElement("Transaction");
                        xml_writer.WriteElementString("ExactID", _encryptionService.DecryptText(_firstDataStoreSetting.GatewayID));//Gateway ID
                        xml_writer.WriteElementString("Password", _encryptionService.DecryptText(_firstDataStoreSetting.Password));//Password
                        xml_writer.WriteElementString("Transaction_Type", TransactionType.TaggedRefund);
                        xml_writer.WriteElementString("DollarAmount", refundPaymentRequest.AmountToRefund.ToString());
                        xml_writer.WriteElementString("Authorization_Num", refundPaymentRequest.Order.CaptureTransactionId);
                        xml_writer.WriteElementString("Transaction_Tag", customValues[Constants.TransactionTag].ToString());
                        xml_writer.WriteEndElement();
                        String xml_string = string_builder.ToString();

                        try
                        {
                            var response = SendFDRequest(xml_string);
                            string_builder.Append("TransactionSent-Response:");
                            string_builder.Append(response.ToString());
                            if (response.SelectSingleNode("Transaction_Approved").InnerText == "true")
                            {
                                result.NewPaymentStatus = PaymentStatus.Refunded;
                                _orderService.UpdateOrder(refundPaymentRequest.Order);
                            }
                            else
                            {
                                var errorMessage = _localizationService.GetLocaleStringResourceByName($"BitShift.Plugin.FirstData.CustomError.{response.SelectSingleNode("EXact_Resp_Code").InnerText}");
                                if (errorMessage != null)
                                {
                                    result.AddError(errorMessage.ResourceValue);
                                }
                                else
                                {
                                    result.AddError($"{response.SelectSingleNode("EXact_Resp_Code").InnerText}: {response.SelectSingleNode("EXact_Message").InnerText}");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            result.AddError(ex.Message);
                            _logger.Error("Error refunding payment - Sending to FD or reading response - " + string_builder.ToString(), ex, refundPaymentRequest.Order.Customer);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result.AddError("Error refunding payment.  See Log for more details");
                _logger.Error("Error refunding payment - Building request - " + string_builder.ToString(), ex, refundPaymentRequest.Order.Customer);
            }
            return result;
        }

        /// <summary>
        /// Voids a payment
        /// </summary>
        /// <param name="voidPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public VoidPaymentResult Void(VoidPaymentRequest voidPaymentRequest)
        {
            var result = new VoidPaymentResult();
            var customValues = voidPaymentRequest.Order.DeserializeCustomValues();

            StringBuilder string_builder = new StringBuilder();
            using (StringWriter string_writer = new StringWriter(string_builder))
            {
                using (XmlTextWriter xml_writer = new XmlTextWriter(string_writer))
                {     //build XML string
                    xml_writer.Formatting = Formatting.Indented;
                    xml_writer.WriteStartElement("Transaction");
                    xml_writer.WriteElementString("ExactID", _encryptionService.DecryptText(_firstDataStoreSetting.GatewayID));//Gateway ID
                    xml_writer.WriteElementString("Password", _encryptionService.DecryptText(_firstDataStoreSetting.Password));//Password
                    xml_writer.WriteElementString("Transaction_Type", TransactionType.TaggedVoid);
                    xml_writer.WriteElementString("DollarAmount", voidPaymentRequest.Order.OrderTotal.ToString());
                    xml_writer.WriteElementString("Authorization_Num", voidPaymentRequest.Order.AuthorizationTransactionId);
                    xml_writer.WriteElementString("Transaction_Tag", customValues[Constants.TransactionTag].ToString());
                    xml_writer.WriteEndElement();
                    String xml_string = string_builder.ToString();

                    try
                    {
                        var response = SendFDRequest(xml_string);
                        if (response.SelectSingleNode("Transaction_Approved").InnerText == "true")
                        {
                            result.NewPaymentStatus = PaymentStatus.Voided;
                        }
                        else
                        {
                            var errorMessage = _localizationService.GetLocaleStringResourceByName($"BitShift.Plugin.FirstData.CustomError.{response.SelectSingleNode("EXact_Resp_Code").InnerText}");
                            if (errorMessage != null)
                            {
                                result.AddError(errorMessage.ResourceValue);
                            }
                            else
                            {
                                result.AddError($"{response.SelectSingleNode("EXact_Resp_Code").InnerText}: {response.SelectSingleNode("EXact_Message").InnerText}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        result.AddError(ex.Message);
                        _logger.Error("Error voiding payment", ex, voidPaymentRequest.Order.Customer);
                    }
                }
            }

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

            var customer = _customerService.GetCustomerById(processPaymentRequest.CustomerId);
            var cardNumber = processPaymentRequest.CreditCardNumber.Split('|')[0];
            bool useSavedCard = (processPaymentRequest.CreditCardNumber.StartsWith("T"));

            StringBuilder string_builder = new StringBuilder();
            try
            {
                using (StringWriter string_writer = new StringWriter(string_builder))
                {
                    using (XmlTextWriter xml_writer = new XmlTextWriter(string_writer))
                    {     //build XML string
                        xml_writer.Formatting = Formatting.Indented;
                        xml_writer.WriteStartElement("Transaction");
                        xml_writer.WriteElementString("ExactID", _encryptionService.DecryptText(_firstDataStoreSetting.GatewayID));//Gateway ID
                        xml_writer.WriteElementString("Password", _encryptionService.DecryptText(_firstDataStoreSetting.Password));//Password
                        xml_writer.WriteElementString("Transaction_Type", (_firstDataStoreSetting.TransactionMode == (int)TransactMode.Authorize ? TransactionType.PreAuth : TransactionType.Purchase));  //check settings
                        xml_writer.WriteElementString("DollarAmount", processPaymentRequest.OrderTotal.ToString());

                        if (processPaymentRequest.InitialOrderId == 0)
                        {
                            if (useSavedCard)
                            {
                                int savedCardId = Convert.ToInt32(processPaymentRequest.CreditCardNumber.Substring(1));
                                SavedCard card = _savedCardService.GetById(savedCardId);
                                if (card == null)
                                {
                                    throw new NullReferenceException("Saved Card #" + savedCardId.ToString() + " is null");
                                }
                                result.SubscriptionTransactionId = "T" + card.Id.ToString();
                                xml_writer.WriteElementString("CardHoldersName", card.CardholderName);
                                xml_writer.WriteElementString("TransarmorToken", card.Token);
                                xml_writer.WriteElementString("CardType", card.CardType);
                                xml_writer.WriteElementString("Expiry_Date", card.ExpireMonth.ToString("00") + card.ExpireYear.ToString().Substring(2, 2));
                            }
                            else
                            {
                                xml_writer.WriteElementString("Expiry_Date", processPaymentRequest.CreditCardExpireMonth.ToString("00") + processPaymentRequest.CreditCardExpireYear.ToString().Substring(2, 2));
                                xml_writer.WriteElementString("CardHoldersName", processPaymentRequest.CreditCardName);
                                xml_writer.WriteElementString("Card_Number", cardNumber);
                                xml_writer.WriteElementString("VerificationStr1", customer.BillingAddress.Address1 ?? "" + "|"
                                                                                + customer.BillingAddress.ZipPostalCode ?? "" + "|"
                                                                                + (customer.BillingAddress.StateProvince != null ? customer.BillingAddress.StateProvince.Name : "") + "|"
                                                                                + customer.BillingAddress.Country.ThreeLetterIsoCode);
                                xml_writer.WriteElementString("VerificationStr2", processPaymentRequest.CreditCardCvv2);
                                xml_writer.WriteElementString("CVD_Presence_Ind", "1");
                            }
                        }
                        else
                        {
                            var initialOrder = _orderService.GetOrderById(processPaymentRequest.InitialOrderId);
                            int savedCardId = Convert.ToInt32(initialOrder.SubscriptionTransactionId.Substring(1));
                            SavedCard card = _savedCardService.GetById(savedCardId);
                            if (card == null)
                            {
                                throw new NullReferenceException("Saved Card #" + savedCardId.ToString() + " is null");
                            }

                            xml_writer.WriteElementString("CardHoldersName", card.CardholderName);
                            xml_writer.WriteElementString("TransarmorToken", card.Token);
                            xml_writer.WriteElementString("CardType", card.CardType);
                            xml_writer.WriteElementString("Expiry_Date", card.ExpireMonth.ToString("00") + card.ExpireYear.ToString().Substring(2, 2));
                        }

                        xml_writer.WriteElementString("Client_Email", customer.Email);
                        xml_writer.WriteElementString("Currency", _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId).CurrencyCode);
                        xml_writer.WriteElementString("Customer_Ref", customer.CustomerGuid.ToString());
                        xml_writer.WriteElementString("Reference_No", processPaymentRequest.OrderGuid.ToString());
                        xml_writer.WriteElementString("Client_IP", _webHelper.GetCurrentIpAddress());
                        xml_writer.WriteElementString("ZipCode", customer.BillingAddress.ZipPostalCode ?? "");
                        xml_writer.WriteEndElement();
                        String xml_string = string_builder.ToString();

                        try
                        {
                            var response = SendFDRequest(xml_string);
                            if (response.SelectSingleNode("Transaction_Approved").InnerText == "true")
                            {
                                result.AuthorizationTransactionId = response.SelectSingleNode("Authorization_Num").InnerText;
                                processPaymentRequest.CustomValues.Add(Constants.TransactionTag, response.SelectSingleNode("Transaction_Tag").InnerText);

                                result.AuthorizationTransactionResult = string.Format("Approved ({0}: {1})", response.SelectSingleNode("EXact_Resp_Code").InnerText, response.SelectSingleNode("EXact_Message").InnerText);
                                if (_firstDataStoreSetting.TransactionMode == (int)TransactMode.AuthorizeAndCapture)
                                {
                                    result.CaptureTransactionId = response.SelectSingleNode("Authorization_Num").InnerText;
                                }

                                result.AvsResult = response.SelectSingleNode("AVS").InnerText;
                                result.AllowStoringCreditCardNumber = false;

                                result.NewPaymentStatus = (_firstDataStoreSetting.TransactionMode == (int)TransactMode.Authorize ? PaymentStatus.Authorized : PaymentStatus.Paid);

                                if (!useSavedCard)
                                {
                                    var token = response.SelectSingleNode("TransarmorToken").InnerText;

                                    var existingCard = _savedCardService.GetByToken(customer.Id, token);
                                    if (existingCard == null) //don't save the same card twice
                                    {
                                        SavedCard card = new SavedCard();
                                        card.BillingAddress_Id = customer.BillingAddress.Id;
                                        card.CardholderName = processPaymentRequest.CreditCardName;
                                        card.CardType = response.SelectSingleNode("CardType").InnerText;
                                        card.Customer_Id = customer.Id;
                                        card.ExpireMonth = processPaymentRequest.CreditCardExpireMonth;
                                        card.ExpireYear = processPaymentRequest.CreditCardExpireYear;
                                        card.Token = token;
                                        _savedCardService.Insert(card);

                                        result.SubscriptionTransactionId = "T" + card.Id.ToString();
                                    }
                                }
                            }
                            else
                            {
                                result.AddError(string.Format("Error {0}: {1}", response.SelectSingleNode("Bank_Resp_Code").InnerText, response.SelectSingleNode("Bank_Message").InnerText));
                            }
                        }
                        catch (Exception ex)
                        {
                            result.AddError(_localizationService.GetResource("BitShift.Plugin.FirstData.TechnicalError"));
                            _logger.Error("Error processing payment", ex, _workContext.CurrentCustomer);
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                result.AddError(_localizationService.GetResource("BitShift.Plugin.FirstData.TechnicalError"));
                _logger.Error("Error processing payment", ex, _workContext.CurrentCustomer);
            }

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
            //nothing special needed from First Data
            return result;
        }

        /// <summary>
        /// Gets a value indicating whether customers can complete a payment after order is placed but not completed (for redirection payment methods)
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>Result</returns>
        public bool CanRePostProcessPayment(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            //it's not a redirection payment method. So we always return false
            return false;
        }

        /// <summary>
        /// Returns a value indicating whether payment method should be hidden during checkout
        /// </summary>
        /// <param name="cart">Shoping cart</param>
        /// <returns>true - hide; false - display.</returns>
        public bool HidePaymentMethod(IList<ShoppingCartItem> cart)
        {
            //you can put any logic here
            //for example, hide this payment method if all products in the cart are downloadable
            //or hide this payment method if current customer is from certain country
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
            controllerName = "FirstData";
            routeValues = new RouteValueDictionary() { { "Namespaces", "BitShift.Plugin.Payments.FirstData.Controllers" }, { "area", null } };
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
            controllerName = "FirstData";
            routeValues = new RouteValueDictionary() { { "Namespaces", "BitShift.Plugin.Payments.FirstData.Controllers" }, { "area", null } };
        }

        public Type GetControllerType()
        {
            return typeof(FirstDataController);
        }

        public override void Install()
        {
            //database objects
            _objectContext.Install();

            //settings
            var settings = new FirstDataSettings
            {
                SandboxURL = "https://api.demo.globalgatewaye4.firstdata.com/transaction/v12",
                ProductionURL = "https://api.globalgatewaye4.firstdata.com/transaction/v12",
            };
            _settingService.SaveSetting(settings);

            var defaultStoreSetting = new FirstDataStoreSetting
            {
                UseSandbox = true,
                TransactionMode = (int)TransactMode.AuthorizeAndCapture,
                StoreId = 0
            };
            _firstDataStoreSettingService.Insert(defaultStoreSetting);

            //locales
            this.AddOrUpdatePluginLocaleResource("BitShift.Plugin.FirstData.Notes", "If you're using this gateway, ensure that your primary store currency is supported by FirstData Global Gateway e4.  Recurring Payments and Card Saving require the TransArmor service on your merchant account.  Read more <a href=\"http://bitshiftweb.com/transarmor-tokenization\">here</a>.");
            this.AddOrUpdatePluginLocaleResource("BitShift.Plugin.FirstData.Fields.UseSandbox", "Use Sandbox");
            this.AddOrUpdatePluginLocaleResource("BitShift.Plugin.FirstData.Fields.UseSandbox.Hint", "Check to enable Sandbox (testing environment).");
            this.AddOrUpdatePluginLocaleResource("BitShift.Plugin.FirstData.Fields.TransactModeValues", "Transaction mode");
            this.AddOrUpdatePluginLocaleResource("BitShift.Plugin.FirstData.Fields.TransactModeValues.Hint", "Choose transaction mode");
            this.AddOrUpdatePluginLocaleResource("BitShift.Plugin.FirstData.Fields.HMAC", "HMAC");
            this.AddOrUpdatePluginLocaleResource("BitShift.Plugin.FirstData.Fields.HMAC.Hint", "The HMAC for your terminal");
            this.AddOrUpdatePluginLocaleResource("BitShift.Plugin.FirstData.Fields.GatewayID", "Gateway ID");
            this.AddOrUpdatePluginLocaleResource("BitShift.Plugin.FirstData.Fields.GatewayID.Hint", "Specify gateway identifier.");
            this.AddOrUpdatePluginLocaleResource("BitShift.Plugin.FirstData.Fields.Password", "Password");
            this.AddOrUpdatePluginLocaleResource("BitShift.Plugin.FirstData.Fields.Password.Hint", "Specify terminal password.");
            this.AddOrUpdatePluginLocaleResource("BitShift.Plugin.FirstData.Fields.KeyID", "Key ID");
            this.AddOrUpdatePluginLocaleResource("BitShift.Plugin.FirstData.Fields.KeyID.Hint", "Specify key identifier.");
            this.AddOrUpdatePluginLocaleResource("BitShift.Plugin.FirstData.Fields.AdditionalFee", "Additional fee");
            this.AddOrUpdatePluginLocaleResource("BitShift.Plugin.FirstData.Fields.AdditionalFee.Hint", "Enter additional fee to charge your customers.");
            this.AddOrUpdatePluginLocaleResource("BitShift.Plugin.FirstData.Fields.AdditionalFeePercentage", "Additinal fee. Use percentage");
            this.AddOrUpdatePluginLocaleResource("BitShift.Plugin.FirstData.Fields.AdditionalFeePercentage.Hint", "Determines whether to apply a percentage additional fee to the order total. If not enabled, a fixed value is used.");
            this.AddOrUpdatePluginLocaleResource("BitShift.Plugin.FirstData.ExpiryDateError", "Card expired");
            this.AddOrUpdatePluginLocaleResource("BitShift.Plugin.FirstData.Fields.LicenseKey", "License Keys");
            this.AddOrUpdatePluginLocaleResource("BitShift.Plugin.FirstData.Fields.LicenseKey.Hint", "The license key should have been emailed to you after your purchase.  If not, contact support@bitshiftweb.com");
            this.AddOrUpdatePluginLocaleResource("BitShift.Plugin.FirstData.UnlicensedError", "This plugin is not licensed for this URL.  Purchase a license at http://www.bitshiftweb.com");
            this.AddOrUpdatePluginLocaleResource("BitShift.Plugin.FirstData.TechnicalError", "There has been an unexpected error while processing your payment.");
            this.AddOrUpdatePluginLocaleResource("BitShift.Plugin.FirstData.Payment.SaveCard", "Save Card");
            this.AddOrUpdatePluginLocaleResource("BitShift.Plugin.FirstData.Payment.SaveCard.Hint", "Save card for future use.  Your card number is tokenized on First Data's servers and not stored on our site.");
            this.AddOrUpdatePluginLocaleResource("BitShift.Plugin.FirstData.Fields.EnableRecurringPayments", "Enable Recurring Payments");
            this.AddOrUpdatePluginLocaleResource("BitShift.Plugin.FirstData.Fields.EnableRecurringPayments.Hint", "Allows manual recurring payments by using the TransArmor Token");
            this.AddOrUpdatePluginLocaleResource("BitShift.Plugin.FirstData.Fields.EnableCardSaving", "Enable Card Saving");
            this.AddOrUpdatePluginLocaleResource("BitShift.Plugin.FirstData.Fields.EnableCardSaving.Hint", "Allows customers to choose to save a card when they use it.  The TransArmor Token is saved instead of the CC number");
            this.AddOrUpdatePluginLocaleResource("BitShift.Plugin.FirstData.Payment.UseCardLabel", "Use");
            this.AddOrUpdatePluginLocaleResource("BitShift.Plugin.FirstData.Payment.CardDescription", "{0} ending in {1}");
            this.AddOrUpdatePluginLocaleResource("BitShift.Plugin.FirstData.Payment.ExpirationDescription", "Expires {0}/{1}");
            this.AddOrUpdatePluginLocaleResource("BitShift.Plugin.FirstData.Payment.ExpiredLabel", "Expired");
            this.AddOrUpdatePluginLocaleResource("BitShift.Plugin.FirstData.Payment.SavedCardsLabel", "Saved Cards");
            this.AddOrUpdatePluginLocaleResource("BitShift.Plugin.FirstData.Payment.NewCardLabel", "Enter new card");
            this.AddOrUpdatePluginLocaleResource("BitShift.Plugin.FirstData.Payment.PurchaseOrder", "Purchase Order");
            this.AddOrUpdatePluginLocaleResource("BitShift.Plugin.FirstData.Fields.EnablePurchaseOrderNumber", "Enable Purchase Order Number");
            this.AddOrUpdatePluginLocaleResource("BitShift.Plugin.FirstData.Fields.EnablePurchaseOrderNumber.Hint", "Will optionally capture a purchase order number and append it to the Authorization Transaction ID in the Order Details");
            this.AddOrUpdatePluginLocaleResource("BitShift.Plugin.FirstData.Fields.SandboxURL", "Sandbox URL");
            this.AddOrUpdatePluginLocaleResource("BitShift.Plugin.FirstData.Fields.SandboxURL.Hint", "Where to send sandbox transactions");
            this.AddOrUpdatePluginLocaleResource("BitShift.Plugin.FirstData.Fields.ProductionURL", "Production URL");
            this.AddOrUpdatePluginLocaleResource("BitShift.Plugin.FirstData.Fields.ProductionURL.Hint", "Where to send real transactions");
            this.AddOrUpdatePluginLocaleResource("BitShift.Plugin.FirstData.Fields.StoreID", "Store");
            this.AddOrUpdatePluginLocaleResource("BitShift.Plugin.FirstData.Fields.StoreID.Hint", "The store that these settings apply to.  The Default Store settings will be used for any store that doesn't have settings.");
            this.AddOrUpdatePluginLocaleResource("BitShift.Plugin.FirstData.Configure", "Global Settings");
            this.AddOrUpdatePluginLocaleResource("BitShift.Plugin.FirstData.Stores", "Store Specific Settings");
            this.AddOrUpdatePluginLocaleResource("Bitshift.Plugin.FirstData.Storenotes", "Set your store specific settings here.  A store without it's own entry will use the Default Store Settings");
            this.AddOrUpdatePluginLocaleResource("BitShift.Plugin.FirstData.Stores.Revert", "Revert to Default Settings");
            this.AddOrUpdatePluginLocaleResource("BitShift.Plugin.FirstData.StoreSettingsSaved", "Store settings saved successfully");
            this.AddOrUpdatePluginLocaleResource("BitShift.Plugin.FirstData.SavedCards", "Saved Cards");
            this.AddOrUpdatePluginLocaleResource("BitShift.Plugin.FirstData.SavedCards.NoCards", "You don't have any cards saved.  Complete an order and check the \"Save Card\" box during checkout.");
            this.AddOrUpdatePluginLocaleResource("BitShift.Plugin.FirstData.SavedCards.Fields.Type", "Type");
            this.AddOrUpdatePluginLocaleResource("BitShift.Plugin.FirstData.SavedCards.Fields.Name", "Cardholder Name");
            this.AddOrUpdatePluginLocaleResource("BitShift.Plugin.FirstData.SavedCards.Fields.Last4", "Number");
            this.AddOrUpdatePluginLocaleResource("BitShift.Plugin.FirstData.SavedCards.Fields.Expires", "Expires");
            this.AddOrUpdatePluginLocaleResource("bitshift.plugin.firstdata.savedcards.description", "Review your saved cards here.  You can save a new card during checkout.");
            this.AddOrUpdatePluginLocaleResource("BitShift.Plugin.FirstData.Fields.PaymentPageID", "Payment Page ID");
            this.AddOrUpdatePluginLocaleResource("BitShift.Plugin.FirstData.Fields.PaymentPageID.Hint", "The payment page to use during checkout");
            this.AddOrUpdatePluginLocaleResource("BitShift.Plugin.FirstData.Fields.TransactionKey", "Transaction Key");
            this.AddOrUpdatePluginLocaleResource("BitShift.Plugin.FirstData.Fields.TransactionKey.Hint", "The payment page's transaction key");
            this.AddOrUpdatePluginLocaleResource("BitShift.Plugin.FirstData.Fields.ResponseKey", "Response Key");
            this.AddOrUpdatePluginLocaleResource("BitShift.Plugin.FirstData.Fields.ResponseKey.Hint", "The response key First Data will send back to the plugin");

            base.Install();
        }

        public override void Uninstall()
        {
            //settings
            _settingService.DeleteSetting<FirstDataSettings>();

            //locales
            this.DeletePluginLocaleResource("BitShift.Plugin.FirstData.Notes");
            this.DeletePluginLocaleResource("BitShift.Plugin.FirstData.Fields.UseSandbox");
            this.DeletePluginLocaleResource("BitShift.Plugin.FirstData.Fields.UseSandbox.Hint");
            this.DeletePluginLocaleResource("BitShift.Plugin.FirstData.Fields.TransactModeValues");
            this.DeletePluginLocaleResource("BitShift.Plugin.FirstData.Fields.TransactModeValues.Hint");
            this.DeletePluginLocaleResource("BitShift.Plugin.FirstData.Fields.HMAC");
            this.DeletePluginLocaleResource("BitShift.Plugin.FirstData.Fields.HMAC.Hint");
            this.DeletePluginLocaleResource("BitShift.Plugin.FirstData.Fields.GatewayID");
            this.DeletePluginLocaleResource("BitShift.Plugin.FirstData.Fields.GatewayID.Hint");
            this.DeletePluginLocaleResource("BitShift.Plugin.FirstData.Fields.Password");
            this.DeletePluginLocaleResource("BitShift.Plugin.FirstData.Fields.Password.Hint");
            this.DeletePluginLocaleResource("BitShift.Plugin.FirstData.Fields.KeyID");
            this.DeletePluginLocaleResource("BitShift.Plugin.FirstData.Fields.KeyID.Hint");
            this.DeletePluginLocaleResource("BitShift.Plugin.FirstData.Fields.AdditionalFee");
            this.DeletePluginLocaleResource("BitShift.Plugin.FirstData.Fields.AdditionalFee.Hint");
            this.DeletePluginLocaleResource("BitShift.Plugin.FirstData.Fields.AdditionalFeePercentage");
            this.DeletePluginLocaleResource("BitShift.Plugin.FirstData.Fields.AdditionalFeePercentage.Hint");
            this.DeletePluginLocaleResource("BitShift.Plugin.FirstData.ExpiryDateError");
            this.DeletePluginLocaleResource("BitShift.Plugin.FirstData.Fields.LicenseKey");
            this.DeletePluginLocaleResource("BitShift.Plugin.FirstData.Fields.LicenseKey.Hint");
            this.DeletePluginLocaleResource("BitShift.Plugin.FirstData.TechnicalError");
            this.DeletePluginLocaleResource("BitShift.Plugin.FirstData.UnlicensedError");
            this.DeletePluginLocaleResource("BitShift.Plugin.FirstData.Payment.SaveCard");
            this.DeletePluginLocaleResource("BitShift.Plugin.FirstData.Fields.EnableRecurringPayments");
            this.DeletePluginLocaleResource("BitShift.Plugin.FirstData.Fields.EnableRecurringPayments.Hint");
            this.DeletePluginLocaleResource("BitShift.Plugin.FirstData.Fields.EnableCardSaving");
            this.DeletePluginLocaleResource("BitShift.Plugin.FirstData.Fields.EnableCardSaving.Hint");
            this.DeletePluginLocaleResource("BitShift.Plugin.FirstData.Payment.UseCardLabel");
            this.DeletePluginLocaleResource("BitShift.Plugin.FirstData.Payment.CardDescription");
            this.DeletePluginLocaleResource("BitShift.Plugin.FirstData.Payment.ExpirationDescription");
            this.DeletePluginLocaleResource("BitShift.Plugin.FirstData.Payment.ExpiredLabel");
            this.DeletePluginLocaleResource("BitShift.Plugin.FirstData.Payment.SavedCardsLabel");
            this.DeletePluginLocaleResource("BitShift.Plugin.FirstData.Payment.NewCardLabel");
            this.DeletePluginLocaleResource("BitShift.Plugin.FirstData.Fields.EnablePurchaseOrderNumber");
            this.DeletePluginLocaleResource("BitShift.Plugin.FirstData.Fields.EnablePurchaseOrderNumber.Hint");
            this.DeletePluginLocaleResource("BitShift.Plugin.FirstData.Fields.SandboxURL");
            this.DeletePluginLocaleResource("BitShift.Plugin.FirstData.Fields.SandboxURL.Hint");
            this.DeletePluginLocaleResource("BitShift.Plugin.FirstData.Fields.ProductionURL");
            this.DeletePluginLocaleResource("BitShift.Plugin.FirstData.Fields.ProductionURL.Hint");
            this.DeletePluginLocaleResource("BitShift.Plugin.FirstData.Fields.StoreID");
            this.DeletePluginLocaleResource("BitShift.Plugin.FirstData.Fields.StoreID.Hint");
            this.DeletePluginLocaleResource("BitShift.Plugin.FirstData.Configure");
            this.DeletePluginLocaleResource("BitShift.Plugin.FirstData.Stores");
            this.DeletePluginLocaleResource("Bitshift.Plugin.FirstData.Storenotes");
            this.DeletePluginLocaleResource("BitShift.Plugin.FirstData.Stores.Revert");
            this.DeletePluginLocaleResource("BitShift.Plugin.FirstData.StoreSettingsSaved");
            this.DeletePluginLocaleResource("BitShift.Plugin.FirstData.SavedCards");
            this.DeletePluginLocaleResource("BitShift.Plugin.FirstData.SavedCards.NoCards");
            this.DeletePluginLocaleResource("BitShift.Plugin.FirstData.SavedCards.Fields.Type");
            this.DeletePluginLocaleResource("BitShift.Plugin.FirstData.SavedCards.Fields.Name");
            this.DeletePluginLocaleResource("BitShift.Plugin.FirstData.SavedCards.Fields.Last4");
            this.DeletePluginLocaleResource("BitShift.Plugin.FirstData.SavedCards.Fields.Expires");
            this.DeletePluginLocaleResource("bitshift.plugin.firstdata.savedcards.description");
            this.DeletePluginLocaleResource("BitShift.Plugin.FirstData.Fields.PaymentPageID");
            this.DeletePluginLocaleResource("BitShift.Plugin.FirstData.Fields.PaymentPageID.Hint");
            this.DeletePluginLocaleResource("BitShift.Plugin.FirstData.Fields.TransactionKey");
            this.DeletePluginLocaleResource("BitShift.Plugin.FirstData.Fields.TransactionKey.Hint");
            this.DeletePluginLocaleResource("BitShift.Plugin.FirstData.Fields.ResponseKey");
            this.DeletePluginLocaleResource("BitShift.Plugin.FirstData.Fields.ResponseKey.Hint");

            _objectContext.Uninstall();

            base.Uninstall();
        }

        #endregion

        #region Manage Saved Cards

        /// <summary>
        /// Gets widget zones where this widget should be rendered
        /// </summary>
        /// <returns>Widget zones</returns>
        public IList<string> GetWidgetZones()
        {
            return new List<string> { "account_navigation_after" };
        }

        /// <summary>
        /// Gets a route for displaying widget
        /// </summary>
        /// <param name="widgetZone">Widget zone where it's displayed</param>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetDisplayWidgetRoute(string widgetZone, out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "SavedCardsLink";
            controllerName = "FirstData";
            routeValues = new RouteValueDictionary
            {
                {"Namespaces", "BitShift.Plugin.Payments.FirstData.Controllers"},
                {"area", null},
                {"widgetZone", widgetZone}
            };
        }

        #endregion

        #region Properties

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
                if (_firstDataStoreSetting.EnableRecurringPayments)
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

        #endregion
    }
}
