using Nop.Core;
using Nop.Core.Data;
using Nop.Plugin.Tax.Vertex.Models;
using Nop.Plugin.Tax.Vertex.Services;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Web.Framework.Controllers;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using System.Xml;

namespace Nop.Plugin.Tax.Vertex.Controllers
{
    [AdminAuthorize]
    public class TaxVertexController : BasePluginController
    {
        #region Fields

        private readonly VertexTaxSettings _VertexTaxSettings;
        private readonly IAddressService _addressService;
        private readonly ICountryService _countryService;
        private readonly ILocalizationService _localizationService;
        private readonly ILogger _logger;
        private readonly ISettingService _settingService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;

        private readonly IVertexTaxRateService _taxRateService;

        #endregion

        #region Ctor

        public TaxVertexController(VertexTaxSettings VertexTaxSettings,
            IAddressService addressService,
            ICountryService countryService,
            ILocalizationService localizationService,
            ILogger logger,
            ISettingService settingService,
            IStateProvinceService stateProvinceService,
            IVertexTaxRateService taxRateService,
            IStoreContext storeContext,
            IWorkContext workContext)
        {
            this._VertexTaxSettings = VertexTaxSettings;
            this._addressService = addressService;
            this._countryService = countryService;
            this._localizationService = localizationService;
            this._logger = logger;
            this._settingService = settingService;
            this._stateProvinceService = stateProvinceService;
            this._storeContext = storeContext;
            this._workContext = workContext;
            this._taxRateService = taxRateService;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Check that plugin is configured
        /// </summary>
        /// <returns>True if is configured; otherwise false</returns>
        protected bool IsConfigured()
        {
            return !string.IsNullOrEmpty(_VertexTaxSettings.AccountId) && !string.IsNullOrEmpty(_VertexTaxSettings.LicenseKey);
        }

        /// <summary>
        /// Prepare address model
        /// </summary>
        /// <param name="model">Address model</param>
        protected void PrepareAddress(TaxVertexAddressModel model)
        {
            //populate list of countries
            model.AvailableCountries = _countryService.GetAllCountries(showHidden: true)
                .Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() }).ToList();
            model.AvailableCountries.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Address.SelectCountry"), Value = "0" });

            //populate list of states and provinces
            if (model.CountryId != 0)
                model.AvailableStates = _stateProvinceService.GetStateProvincesByCountryId(model.CountryId, showHidden: true)
                    .Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() }).ToList();
            if (!model.AvailableStates.Any())
                model.AvailableStates.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Address.OtherNonUS"), Value = "0" });
        }

        public static void StoreXMLResponseInDB(int CustomerId, string ProductId, int RequestType, int OrderId, int orderitemId, string SessionId, string httpresponse)
        {
            string connString = new DataSettingsManager().LoadSettings().DataConnectionString;

            int OrderID = OrderId == 0 ? 0 : OrderId;
            int taxRequestType = RequestType;
            string HttpResponse = httpresponse;

            XmlDocument responseDoc = new XmlDocument();
            responseDoc.LoadXml(httpresponse);
            XmlNodeList nodeitem = responseDoc.GetElementsByTagName("QuotationResponse");
            if (nodeitem != null)
            {
                foreach (XmlNode xmlnode in nodeitem)
                {
                    for (int i = 0; i < xmlnode.ChildNodes.Count; i++)
                    {
                        XmlNode childnode = xmlnode.ChildNodes[i];
                        if (childnode.Name == "LineItem")
                        {
                            ProductId = childnode.Attributes["lineItemId"].Value;
                        }
                    }
                }
            }

            string sql = "Insert into dbo.TaxResponseStorage(CustomerID,OrderID,SessionId, ProductID,TypeOfCall,XMlResponse,AddedDate,OrderItemId) values(@CustomerID,@OrderID, @SessionId, @ProductID,@TypeOfCall,@XMlResponse,@AddedDate,@OrderItemId)";
            using (SqlConnection conn = new SqlConnection(connString))
            {
                SqlCommand cmd = new SqlCommand(sql, conn);
                try
                {
                    cmd.Parameters.AddWithValue("@CustomerID", CustomerId);
                    cmd.Parameters.AddWithValue("@OrderID", OrderID);
                    cmd.Parameters.AddWithValue("@SessionID", SessionId);
                    cmd.Parameters.AddWithValue("@ProductID", ProductId);
                    cmd.Parameters.AddWithValue("@TypeOfCall", taxRequestType);
                    cmd.Parameters.AddWithValue("@XMlResponse", HttpResponse);
                    cmd.Parameters.AddWithValue("@AddedDate", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture));
                    cmd.Parameters.AddWithValue("@OrderItemId", orderitemId);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        public static void UpdateStoreXMLResponseInDB(int CustomerId, string ProductId, int RequestType, int OrderId, int orderitemId, string SessionId, string httpresponse)
        {
            string connString = new DataSettingsManager().LoadSettings().DataConnectionString;

            int OrderID = OrderId == 0 ? 0 : OrderId;
            int taxRequestType = RequestType;
            string HttpResponse = httpresponse;


            string sql = "update dbo.TaxResponseStorage(CustomerID,OrderID,SessionId, ProductID,TypeOfCall,XMlResponse,AddedDate,OrderItemId) values(@CustomerID,@OrderID, @SessionId, @ProductID,@TypeOfCall,@XMlResponse,@AddedDate,@OrderItemId)";
            using (SqlConnection conn = new SqlConnection(connString))
            {
                SqlCommand cmd = new SqlCommand(sql, conn);
                try
                {
                    cmd.Parameters.AddWithValue("@CustomerID", CustomerId);
                    cmd.Parameters.AddWithValue("@OrderID", OrderID);
                    cmd.Parameters.AddWithValue("@SessionID", SessionId);
                    cmd.Parameters.AddWithValue("@ProductID", ProductId);
                    cmd.Parameters.AddWithValue("@TypeOfCall", taxRequestType);
                    cmd.Parameters.AddWithValue("@XMlResponse", HttpResponse);
                    cmd.Parameters.AddWithValue("@AddedDate", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture));
                    cmd.Parameters.AddWithValue("@OrderItemId", orderitemId);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        public static bool CheckXMLResponseInDB(int CustomerId, string ProductId, int RequestType,  string SessionId)
        {
            string connString = new DataSettingsManager().LoadSettings().DataConnectionString;
            bool flag = false;
            int taxRequestType = RequestType;


            using (var conn = new SqlConnection(connString))
            using (var cmd = conn.CreateCommand())
            {
                string sqlSelect = string.Empty;
                sqlSelect = string.Format("Select *  from dbo.TaxResponseStorage where Customerid={0} and productId={1} and TypeofCall={2} and SessionId='{3}'", CustomerId, ProductId, taxRequestType, SessionId);

                cmd.CommandText = sqlSelect;
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        flag = true;
                        break;
                    }
                }
            }
            return flag;
        }

        public static bool GetXmlResponsesStorageInDB(int CustomerId, string ProductId, int RequestType, int OrderId)
        {
            string connString = new DataSettingsManager().LoadSettings().DataConnectionString;
            bool dataExists = false;
            int OrderID = OrderId == 0 ? 0 : OrderId;
            int taxRequestType = RequestType;
            //string HttpResponse = httpresponse;


            string sql = string.Format("select * from  dbo.TaxResponseStorage where Customerid={0} and OrderID={1} and ProductID='{2}' and TypeofCall={3}", CustomerId, OrderId, ProductId, RequestType);
            using (SqlConnection conn = new SqlConnection(connString))
            {
                SqlCommand cmd = new SqlCommand(sql, conn);
                conn.Open();
                cmd.CommandType = CommandType.Text;
                try
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            dataExists = true;
                        }
                    }

                    if (dataExists)
                        return true;
                    else
                        return false;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }


        #endregion

        #region Methods

        [ChildActionOnly]
        public ActionResult Configure()
        {
            //prepare model
            var model = new TaxVertexModel
            {
                IsConfigured = IsConfigured(),
                AccountId = _VertexTaxSettings.AccountId,
                LicenseKey = _VertexTaxSettings.LicenseKey,
                CompanyCode = _VertexTaxSettings.CompanyCode,
                IsSandboxEnvironment = _VertexTaxSettings.IsSandboxEnvironment,
                CommitTransactions = _VertexTaxSettings.CommitTransactions,
                ValidateAddresses = _VertexTaxSettings.ValidateAddresses
            };
            PrepareAddress(model.TestAddress);

            return View("~/Plugins/Tax.Vertex/Views/TaxVertex/Configure.cshtml", model);
        }

        [HttpPost, ActionName("Configure")]
        [FormValueRequired("save")]
        public ActionResult Configure(TaxVertexModel model)
        {
            if (!ModelState.IsValid)
                return Configure();

            //save settings
            _VertexTaxSettings.AccountId = model.AccountId;
            _VertexTaxSettings.LicenseKey = model.LicenseKey;
            _VertexTaxSettings.CompanyCode = model.CompanyCode;
            _VertexTaxSettings.IsSandboxEnvironment = model.IsSandboxEnvironment;
            _VertexTaxSettings.CommitTransactions = model.CommitTransactions;
            _VertexTaxSettings.ValidateAddresses = model.ValidateAddresses;
            _settingService.SaveSetting(_VertexTaxSettings);

            SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            //prepare model
            model.IsConfigured = IsConfigured();
            PrepareAddress(model.TestAddress);

            return View("~/Plugins/Tax.Vertex/Views/TaxVertex/Configure.cshtml", model);
        }


        //[HttpPost, ActionName("Configure")]
        //[FormValueRequired("testTax")]
        //public ActionResult TestRequest(TaxVertexModel model)
        //{
        //    if (!ModelState.IsValid)
        //        return Configure();

        //    //create test tax request
        //    var taxRequest = new TaxRequest
        //    {
        //        Commit = true,
        //        Client = VertexTaxHelper.VERTEX_CLIENT,
        //        CompanyCode = _VertexTaxSettings.CompanyCode,
        //        CustomerCode = _workContext.CurrentCustomer.Return(customer => customer.Id.ToString(), null),
        //        CurrencyCode = _workContext.WorkingCurrency.Return(currency => currency.CurrencyCode, null),
        //        DetailLevel = DetailLevel.Tax,
        //        DocCode = string.Format("Test-{0}", Guid.NewGuid()),
        //        DocType = _VertexTaxSettings.CommitTransactions ? DocType.SalesInvoice : DocType.SalesOrder,
        //        DocDate = DateTime.UtcNow.ToString("yyyy-MM-dd")
        //    };

        //    //set destination and origin addresses
        //    var addresses = new List<Address>();

        //    var destinationCountry = _countryService.GetCountryById(model.TestAddress.CountryId);
        //    var destinationStateOrProvince = _stateProvinceService.GetStateProvinceById(model.TestAddress.RegionId);
        //    addresses.Add(new Address
        //    {
        //        AddressCode = "0",
        //        Line1 = model.TestAddress.Address,
        //        City = model.TestAddress.City,
        //        Region = destinationStateOrProvince.Return(state => state.Abbreviation, null),
        //        Country = destinationCountry.Return(country => country.TwoLetterIsoCode, null),
        //        PostalCode = model.TestAddress.ZipPostalCode
        //    });

        //    //load settings for shipping origin address identifier
        //    var originAddressId = _settingService.GetSettingByKey<int>("ShippingSettings.ShippingOriginAddressId",
        //        storeId: _storeContext.CurrentStore.Id, loadSharedValueIfNotFound: true);

        //    //try get address that will be used for tax origin 
        //    var originAddress = _addressService.GetAddressById(originAddressId);
        //    if (originAddress != null)
        //    {
        //        addresses.Add(new Address
        //        {
        //            AddressCode = originAddress.Id.ToString(),
        //            Line1 = originAddress.Address1,
        //            Line2 = originAddress.Address2,
        //            City = originAddress.City,
        //            Region = originAddress.StateProvince.Return(state => state.Abbreviation, null),
        //            Country = originAddress.Country.Return(country => country.TwoLetterIsoCode, null),
        //            PostalCode = originAddress.ZipPostalCode
        //        });
        //    }

        //    taxRequest.Addresses = addresses.ToArray();

        //    //create test item line
        //    var item = new Line
        //    {
        //        Amount = 100,
        //        Description = "Item line for the test tax request",
        //        DestinationCode = "0",
        //        ItemCode = "Test item line",
        //        LineNo = "test",
        //        OriginCode = originAddress.Return(address => address.Id, 0).ToString(),
        //        Qty = 1,
        //        TaxCode = "test"
        //    };
        //    taxRequest.Lines = new[] { item };

        //    //post tax request
        //    var taxResult = VertexTaxHelper.PostTaxRequest(taxRequest, _VertexTaxSettings, _logger);

        //    //display results
        //    var resultstring = new StringBuilder();
        //    if (taxResult.Return(result => result.ResultCode, SeverityLevel.Error) == SeverityLevel.Success)
        //    {
        //        //display validated address (only for US or Canadian address)
        //        if (_VertexTaxSettings.ValidateAddresses && taxResult.TaxAddresses != null && taxResult.TaxAddresses.Any() &&
        //            (destinationCountry.TwoLetterIsoCode.Equals("us", StringComparison.InvariantCultureIgnoreCase) ||
        //            destinationCountry.TwoLetterIsoCode.Equals("ca", StringComparison.InvariantCultureIgnoreCase)))
        //        {
        //            resultstring.Append("Validated address <br />");
        //            resultstring.AppendFormat("Postal / Zip code: {0}<br />", HttpUtility.HtmlEncode(taxResult.TaxAddresses[0].PostalCode));
        //            resultstring.AppendFormat("Country: {0}<br />", HttpUtility.HtmlEncode(taxResult.TaxAddresses[0].Country));
        //            resultstring.AppendFormat("Region: {0}<br />", HttpUtility.HtmlEncode(taxResult.TaxAddresses[0].Region));
        //            resultstring.AppendFormat("City: {0}<br />", HttpUtility.HtmlEncode(taxResult.TaxAddresses[0].City));
        //            resultstring.AppendFormat("Address: {0}<br /><br />", HttpUtility.HtmlEncode(taxResult.TaxAddresses[0].Address));
        //        }

        //        //display tax rates by jurisdictions
        //        if (taxResult.TaxLines != null && taxResult.TaxLines.Any())
        //        {
        //            resultstring.AppendFormat("Total tax rate: {0:0.00}%<br />", taxResult.TaxLines[0].Rate * 100);
        //            if (taxResult.TaxLines[0].TaxDetails != null)
        //                foreach (var taxDetail in taxResult.TaxLines[0].TaxDetails)
        //                    resultstring.AppendFormat("Jurisdiction: {0}, Tax rate: {1:0.00}%<br />", HttpUtility.HtmlEncode(taxDetail.JurisName), taxDetail.Rate * 100);
        //        }
        //    }
        //    else
        //    {
        //        resultstring.Append("<font color=\"red\">");
        //        foreach (var message in taxResult.Messages)
        //            resultstring.AppendFormat("{0}<br />", HttpUtility.HtmlEncode(message.Summary));
        //        resultstring.Append("</font>");
        //    }

        //    //prepare model
        //    model.TestTaxResult = resultstring.ToString();
        //    model.IsConfigured = IsConfigured();
        //    PrepareAddress(model.TestAddress);

        //    return View("~/Plugins/Tax.Vertex/Views/TaxVertex/Configure.cshtml", model);
        //}

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetStatesByCountryId(int countryId)
        {
            //get states and provinces for specified country
            var states = _stateProvinceService.GetStateProvincesByCountryId(countryId, showHidden: true)
                .Select(x => new { id = x.Id, name = x.Name }).ToList();
            if (!states.Any())
                states.Insert(0, new { id = 0, name = _localizationService.GetResource("Admin.Address.OtherNonUS") });

            return Json(states, JsonRequestBehavior.AllowGet);
        }

        #endregion
    }
}