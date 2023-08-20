using Newtonsoft.Json.Linq;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Tax;
using Nop.Core.Domain.Vendors;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Stores;
using Nop.Services.Vendors;
using Nop.Web.Framework.Controllers;
using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Nop.Services.Seo;
using Nop.Services.Tax;
using System.IO;
using System.Web.Script.Serialization;
using System.Text;


namespace panoRazzi.RestService.Controllers
{
    public class ApiController : BaseController
    {
        #region Fields

        private ICustomerService _customerService;
        private IOrderService _orderService;
        private IWorkContext _workContext;
        private IVendorService _vendorService;
        private IProductAttributeParser _productAttributeParser;
        private RestServiceSettings _settings;
        private CustomerSettings _customerSettings;
        private IStoreMappingService _storeMappingService;
        private IStoreService _storeService;
        private IShoppingCartService _shoppingCartService;
        private IPriceFormatter _priceFormatter;
        private ICurrencyService _currencyService;
        private IOrderTotalCalculationService _orderTotalCalculationService;
        private TaxSettings _taxSettings;
        private IPictureService _pictureService;
        private IStoreContext _storeContext;
        private ITaxService _taxService;
        private IPriceCalculationService _priceCalculationService;
        #endregion

        #region Ctor

        public ApiController(
            ICustomerService customerService,
            IOrderService orderService,
            IWorkContext workContext,
            IProductAttributeParser productAttributeParser,
            IVendorService vendorService,
            RestServiceSettings settings,
            CustomerSettings customerSettings,
            IStoreMappingService storeMappingService,
            IStoreService storeService,
            IShoppingCartService shoppingCartService,
            IPriceFormatter priceFormatter,
            ICurrencyService currencyService,
            IOrderTotalCalculationService orderTotalCalculationService,
            TaxSettings taxSettings,
            IPictureService pictureService,
            IStoreContext storeContext,
            ITaxService taxService,
            IPriceCalculationService priceCalculationService)
        {
            _customerService = customerService;
            _orderService = orderService;
            _workContext = workContext;
            _productAttributeParser = productAttributeParser;
            _vendorService = vendorService;
            _settings = settings;
            _customerSettings = customerSettings;
            _storeMappingService = storeMappingService;
            _storeService = storeService;
            _shoppingCartService = shoppingCartService;
            _priceFormatter = priceFormatter;
            _currencyService = currencyService;
            _orderTotalCalculationService = orderTotalCalculationService;
            _taxSettings = taxSettings;
            _productAttributeParser = productAttributeParser;
            _pictureService = pictureService;
            _storeContext = storeContext;
            _taxService = taxService;
            _priceCalculationService = priceCalculationService;
        }

        #endregion

        #region Customers

        public ActionResult GetCustomerById(int id, string apiToken)
        {
            if (!IsApiTokenValid(apiToken))
                return InvalidApiToken(apiToken);

            var customer = _customerService.GetCustomerById(id);
            if (customer == null)
                return ErrorOccured("Customer not found.");

            return Successful(GetCustomerJson(customer));
        }

        public ActionResult GetCustomerByUsername(string username, string apiToken)
        {
            if (!IsApiTokenValid(apiToken))
                return InvalidApiToken(apiToken);

            var customer = _customerService.GetCustomerByUsername(username);
            if (customer == null)
                return ErrorOccured("Customer not found.");

            return Successful(GetCustomerJson(customer));
        }

        public ActionResult GetCustomerByEmail(string email, string apiToken)
        {
            if (!IsApiTokenValid(apiToken))
                return InvalidApiToken(apiToken);

            if (String.IsNullOrWhiteSpace(email))
                return ErrorOccured("Email is empty.");

            var customer = _customerService.GetCustomerByEmail(email);
            if (customer == null)
                return ErrorOccured("Customer not found.");

            return Successful(GetCustomerJson(customer));
        }

        public ActionResult GetCustomerByGuid(string guid, string apiToken)
        {
            if (!IsApiTokenValid(apiToken))
                return InvalidApiToken(apiToken);

            if (String.IsNullOrWhiteSpace(guid))
                return ErrorOccured("Guid is empty.");

            Guid id = new Guid(guid);

            var customer = _customerService.GetCustomerByGuid(id);
            if (customer == null)
                return ErrorOccured("Customer not found.");

            return Successful(GetCustomerJson(customer));
        }

        public ActionResult GetCustomerBySystemName(string systemName, string apiToken)
        {
            if (!IsApiTokenValid(apiToken))
                return InvalidApiToken(apiToken);

            if (String.IsNullOrWhiteSpace(systemName))
                return ErrorOccured("System Name is empty.");

            var customer = _customerService.GetCustomerBySystemName(systemName);
            if (customer == null)
                return ErrorOccured("Customer not found.");

            return Successful(GetCustomerJson(customer));
        }

        public ActionResult GetAllCustomers(string apiToken, DateTime? createdFromUtc = null,
            DateTime? createdToUtc = null, int affiliateId = 0, int vendorId = 0,
            string customerRoleIds = null, string email = null, string username = null,
            string firstName = null, string lastName = null,
            int dayOfBirth = 0, int monthOfBirth = 0,
            string company = null, string phone = null, string zipPostalCode = null,
            string loadOnlyWithShoppingCart = null, int shoppingCartTypeId = 0,
            int pageIndex = 0, int pageSize = 2147483647)
        {
            if (!IsApiTokenValid(apiToken))
                return InvalidApiToken(apiToken);

            int[] customerRole = null;
            ShoppingCartType? sct = null;
            bool isLoadOnlyWithShoppingCart = false;

            if (!String.IsNullOrWhiteSpace(customerRoleIds))
            {
                customerRole = Array.ConvertAll(customerRoleIds.Split(','), int.Parse);
            }

            if (shoppingCartTypeId > 0)
            {
                sct = (ShoppingCartType)shoppingCartTypeId;
            }

            if (!String.IsNullOrEmpty(loadOnlyWithShoppingCart))
            {
                if (loadOnlyWithShoppingCart.ToLower().Equals("true"))
                {
                    isLoadOnlyWithShoppingCart = true;
                }
            }

            /*
            var customers = _customerService.GetAllCustomers(createdFromUtc, createdToUtc, affiliateId, vendorId,
                customerRole, email, username, firstName, lastName, dayOfBirth, monthOfBirth, company, phone,
                zipPostalCode, isLoadOnlyWithShoppingCart, sct, pageIndex, pageSize);
            */
            var customers = _customerService.GetAllCustomers();

            if (customers.Count == 0)
                return ErrorOccured("Customers are not found.");

            return Successful(GetCustomersJson(customers));
        }

        public ActionResult GetOnlineCustomers(string apiToken, string customerRoleIds = null,
             int pageIndex = 0, int pageSize = 2147483647)
        {
            if (!IsApiTokenValid(apiToken))
                return InvalidApiToken(apiToken);

            int[] customerRole = null;

            if (!String.IsNullOrWhiteSpace(customerRoleIds))
            {
                customerRole = Array.ConvertAll(customerRoleIds.Split(','), int.Parse);
            }

            var customers = _customerService.GetOnlineCustomers(DateTime.UtcNow.AddMinutes(-_customerSettings.OnlineCustomerMinutes), customerRole, pageIndex, pageSize);

            if (customers.Count == 0)
                return ErrorOccured("Customers are not found.");

            return Successful(GetCustomersJson(customers));
        }

        public ActionResult GetCustomersByIds(string apiToken, string customerRoleIds = null)
        {
            if (!IsApiTokenValid(apiToken))
                return InvalidApiToken(apiToken);

            int[] customerRole = null;

            if (!String.IsNullOrWhiteSpace(customerRoleIds))
            {
                customerRole = Array.ConvertAll(customerRoleIds.Split(','), int.Parse);
            }

            var customers = _customerService.GetCustomersByIds(customerRole);

            if (customers.Count == 0)
                return ErrorOccured("Customers are not found.");

            return Successful(GetCustomersJson(customers));
        }

        #endregion

        #region Orders

        public ActionResult GetOrderById(int id, string apiToken)
        {
            if (!IsApiTokenValid(apiToken))
                return InvalidApiToken(apiToken);

            var order = _orderService.GetOrderById(id);
            if (order == null)
                return ErrorOccured("Order not found.");

            return Successful(GetOrderJson(order));
        }


        public ActionResult GetOrderByGuid(Guid orderGuid, string apiToken)
        {
            if (!IsApiTokenValid(apiToken))
                return InvalidApiToken(apiToken);

            var order = _orderService.GetOrderByGuid(orderGuid);
            if (order == null)
                return ErrorOccured("Order not found");

            return Successful(GetOrderJson(order));
        }


        public ActionResult GetOrderByAuthorizationTransactionIdAndPaymentMethod(
            string authorizationTransactionId, string paymentMethodSystemName,
            string apiToken)
        {
            if (!IsApiTokenValid(apiToken))
                return InvalidApiToken(apiToken);

            var order = _orderService.GetOrderByAuthorizationTransactionIdAndPaymentMethod(
                authorizationTransactionId, paymentMethodSystemName);
            if (order == null)
                return ErrorOccured("Order not found");

            return Successful(GetOrderJson(order));
        }

        public ActionResult GetOrderItemByGuid(Guid orderItemGuid, string apiToken)
        {
            if (!IsApiTokenValid(apiToken))
                return InvalidApiToken(apiToken);

            var orderItem = _orderService.GetOrderItemByGuid(orderItemGuid);
            if (orderItem == null)
                return ErrorOccured("Order item not founf");

            return Successful(GetOrderItemJson(orderItem));
        }

        public ActionResult GetOrderItemById(int orderItemId, string apiToken)
        {
            if (!IsApiTokenValid(apiToken))
                return InvalidApiToken(apiToken);

            var orderItem = _orderService.GetOrderItemById(orderItemId);

            if (orderItem == null)
                return ErrorOccured("Order item not found");

            return Successful(GetOrderItemJson(orderItem));
        }

        public ActionResult GetOrderNoteById(int id, string apiToken)
        {
            if (!IsApiTokenValid(apiToken))
                return InvalidApiToken(apiToken);

            var orderNote = _orderService.GetOrderNoteById(id);
            if (orderNote == null)
                return ErrorOccured("Ordernote not found");

            return Successful(GetOrderNoteJson(orderNote));
        }

        // TODO: add paging
        public ActionResult GetOrdersByVendorId(int vendorId, string apiToken)
        {
            if (!IsApiTokenValid(apiToken))
                return InvalidApiToken(apiToken);

            var vendor = _vendorService.GetVendorById(vendorId);
            if (vendor == null)
                return ErrorOccured("Vendor not found.");

            var vendorOrders = _orderService.SearchOrders(vendorId: vendorId);

            return Successful(GetOrdersJson(vendorOrders, vendorId));
        }

        #endregion

        #region StoreMappings


        public ActionResult GetAllStoresAvailable(int vendorId, string apiToken)
        {
            if (!IsApiTokenValid(apiToken))
                return InvalidApiToken(apiToken);

            var vendor = _vendorService.GetVendorById(vendorId);
            if (vendor == null)
                return ErrorOccured("Vendor not found.");

            return Successful(GetAllStoresAvailableJson(vendor));
        }

        #endregion

        #region Misc

        public ActionResult InvalidApiToken(string apiToken)
        {
            var errorMessage = string.Empty;
            if (string.IsNullOrWhiteSpace(apiToken))
                errorMessage = "No API token supplied.";
            else
                errorMessage = string.Format("Invalid API token: {0}", apiToken);

            return ErrorOccured(errorMessage);
        }

        public ActionResult ErrorOccured(string errorMessage)
        {
            JsonResult result = Json(new
            {
                success = false,
                errorMessage = errorMessage
            });
            result.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return result;
        }

        public ActionResult Successful(object data)
        {
            JsonResult result = Json(new
            {
                success = true,
                data = data
            });

            result.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            result.MaxJsonLength = Int32.MaxValue;

            return result;
        }

        #endregion

        #region HeatMapping

        public object GetMemberCountByStoreID(string apiToken, string accessToken, string secretToken)
        {

            if (!IsApiTokenValid(apiToken))
                return InvalidApiToken(apiToken);

            Dictionary<string, string> locationDetails = new Dictionary<string, string>();
            string maxOccupancyCount = "";
            string realTimeCount = "";
            string locationName = "";

            // For Maximum Count               
            var maxCountUrl = string.Format("{0}", "https://apollo-psq.bluefoxengage.com/third_party_fetch_api/v100/get_location_info");
            var request = (HttpWebRequest)WebRequest.Create(maxCountUrl);

            request.Headers.Add("cache-control", "no-cache");
            request.Headers.Add("x-api-access-token", accessToken);
            request.Headers.Add("x-api-secret-token", secretToken);
            request.ContentType = "application/plain";
            request.Method = "POST";
            string postData = "{}";

            ASCIIEncoding encoding = new ASCIIEncoding();
            byte[] byte1 = encoding.GetBytes(postData);

            using (var stream = request.GetRequestStream())
            {
                stream.Write(byte1, 0, byte1.Length);
            }

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                using (Stream dataStream = response.GetResponseStream())
                {
                    using (StreamReader reader = new StreamReader(dataStream))
                    {
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            var json = new JavaScriptSerializer();

                            var maxCountdata = json.Deserialize<IDictionary<string, object>>(reader.ReadToEnd());

                            foreach (var item in maxCountdata.Values)
                            {
                                if (item.ToString() != "OK")
                                {
                                    Dictionary<string, object> locationInfoItem = (Dictionary<string, object>)item;
                                    maxOccupancyCount = Convert.ToString(locationInfoItem["max_occupancy_count"]);
                                    locationName = Convert.ToString(locationInfoItem["nickname"]);
                                }
                            }
                        }
                    }
                }
            }

            locationDetails.Add("MaximumCount", maxOccupancyCount);
            locationDetails.Add("LocationName", locationName);

            // For realtime Count
            var realtimeCountUrl = string.Format("{0}", "https://apollo-psq.bluefoxengage.com/third_party_fetch_api/v100/get_location_realtime_occupancy_count");
            var realTimeCountrequest = (HttpWebRequest)WebRequest.Create(realtimeCountUrl);

            realTimeCountrequest.Headers.Add("cache-control", "no-cache");
            realTimeCountrequest.Headers.Add("x-api-access-token", accessToken);
            realTimeCountrequest.Headers.Add("x-api-secret-token", secretToken);
            realTimeCountrequest.ContentType = "application/plain";
            realTimeCountrequest.Method = "POST";
            string realTimeData = "{}";
            ASCIIEncoding realencoding = new ASCIIEncoding();
            byte[] byte2 = realencoding.GetBytes(realTimeData);

            using (var stream1 = realTimeCountrequest.GetRequestStream())
            {
                stream1.Write(byte2, 0, byte2.Length);
            }

            using (HttpWebResponse response1 = (HttpWebResponse)realTimeCountrequest.GetResponse())
            {
                using (Stream dataStream1 = response1.GetResponseStream())
                {
                    using (StreamReader reader1 = new StreamReader(dataStream1))
                    {
                        if (response1.StatusCode == HttpStatusCode.OK)
                        {
                            var json1 = new JavaScriptSerializer();

                            var realTimeCountdata = json1.Deserialize<IDictionary<string, object>>(reader1.ReadToEnd());
                            realTimeCount = Convert.ToString(realTimeCountdata["occupancy_count"]);
                        }
                    }
                }
            }

            locationDetails.Add("RealTimeCount", realTimeCount);

            decimal occupiedPercent = Decimal.Divide(Convert.ToInt32(realTimeCount), Convert.ToInt32(maxOccupancyCount));

            locationDetails.Add("OccupiedPercent", Convert.ToString(occupiedPercent));

            return locationDetails;

        }

        #endregion

        #region Shopping Cart
        public object GetShoppingCartItems(string apiToken)
        {
            if (!IsApiTokenValid(apiToken))
                return InvalidApiToken(apiToken);

            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();

            // ---

            decimal orderSubTotalDiscountAmountBase;
            List<Discount> orderSubTotalAppliedDiscounts;
            decimal subTotalWithoutDiscountBase;
            decimal subTotalWithDiscountBase;

            var subTotalIncludingTax = _workContext.TaxDisplayType == TaxDisplayType.IncludingTax && !_taxSettings.ForceTaxExclusionFromOrderSubtotal;
            _orderTotalCalculationService.GetShoppingCartSubTotal(cart.ToList(), subTotalIncludingTax,
                out orderSubTotalDiscountAmountBase, out orderSubTotalAppliedDiscounts,
                out subTotalWithoutDiscountBase, out subTotalWithDiscountBase);

            decimal subtotalBase = subTotalWithoutDiscountBase;
            decimal scSubtotal = _currencyService.ConvertFromPrimaryStoreCurrency(subtotalBase, _workContext.WorkingCurrency);

            // ---

            int QuantityTotal = 0;

            // ---

            List<Dictionary<string, object>> cartItems = new List<Dictionary<string, object>>();
            foreach (var ci in cart)
            {
                decimal taxRate;

                // ---

                decimal shoppingCartUnitPriceWithDiscountBase = _taxService.GetProductPrice(ci.Product, _priceCalculationService.GetUnitPrice(ci), out taxRate);
                decimal shoppingCartUnitPriceWithDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartUnitPriceWithDiscountBase, _workContext.WorkingCurrency);

                // ---

                List<Discount> scDiscounts;
                decimal shoppingCartItemDiscountBase;
                decimal shoppingCartItemSubTotalWithDiscountBase = _taxService.GetProductPrice(ci.Product, _priceCalculationService.GetSubTotal(ci, true, out shoppingCartItemDiscountBase, out scDiscounts), out taxRate);
                decimal shoppingCartItemSubTotalWithDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartItemSubTotalWithDiscountBase, _workContext.WorkingCurrency);

                // ---

                Dictionary<string, object> row = new Dictionary<string, object>();

                row.Add("Id", ci.Id);
                row.Add("StoreId", ci.StoreId);
                row.Add("ShoppingCartTypeId", ci.ShoppingCartTypeId);
                row.Add("CustomerId", ci.CustomerId);
                row.Add("Sku", ci.Product.FormatSku(ci.AttributesXml, _productAttributeParser));
                row.Add("Picture", _pictureService.GetPictureUrl(ci.Product.GetProductPicture(ci.AttributesXml, _pictureService, _productAttributeParser)));
                row.Add("ProductId", ci.ProductId);
                row.Add("ProductName", ci.Product.Name);
                row.Add("Url", Url.RouteUrl("Product", new { SeName = ci.Product.GetSeName() }, this.Request.Url.Scheme));
                row.Add("ProductSeName", ci.Product.GetSeName());
                row.Add("UnitPrice", shoppingCartUnitPriceWithDiscount);
                row.Add("SubTotal", shoppingCartItemSubTotalWithDiscount);
                row.Add("Quantity", ci.Quantity);

                // ---

                QuantityTotal += ci.Quantity;

                cartItems.Add(row);
            }

            var ret = new
            {
                Subtotal = scSubtotal,
                Quantity = QuantityTotal,
                items = cartItems
            };

            /*
            var customers = _customerService.GetAllCustomers(
                loadOnlyWithShoppingCart: true,
                sct: ShoppingCartType.ShoppingCart);

            var ret = customers.First().ShoppingCartItems
                .Select(c =>
                    new
                    {
                        Id = c.Id,
                        StoreId = c.StoreId,
                        ShoppingCartTypeId = c.ShoppingCartTypeId,
                        CustomerId = c.CustomerId,
                        ProductId = c.ProductId,
                        Product = c.Product.Name,
                        Quantity = c.Quantity,
                        ItemSubtotal = (c.Product.Price * c.Quantity),
                        URL = c.Product.Url
                    });
            */

            return Successful(ret);
        }

        public object GetShoppingCartItemsTahzoo(string apiToken)
        {
            if (!IsApiTokenValid(apiToken))
                return InvalidApiToken(apiToken);

            var customer = _customerService.GetAllCustomers(
                loadOnlyWithShoppingCart: true,
                sct: ShoppingCartType.ShoppingCart).FirstOrDefault();

            var cart = customer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();

            // ---

            decimal orderSubTotalDiscountAmountBase;
            List<Discount> orderSubTotalAppliedDiscounts;
            decimal subTotalWithoutDiscountBase;
            decimal subTotalWithDiscountBase;

            var subTotalIncludingTax = _workContext.TaxDisplayType == TaxDisplayType.IncludingTax && !_taxSettings.ForceTaxExclusionFromOrderSubtotal;
            _orderTotalCalculationService.GetShoppingCartSubTotal(cart.ToList(), subTotalIncludingTax,
                out orderSubTotalDiscountAmountBase, out orderSubTotalAppliedDiscounts,
                out subTotalWithoutDiscountBase, out subTotalWithDiscountBase);

            decimal subtotalBase = subTotalWithoutDiscountBase;
            decimal scSubtotal = _currencyService.ConvertFromPrimaryStoreCurrency(subtotalBase, _workContext.WorkingCurrency);

            // ---

            int QuantityTotal = 0;

            // ---

            List<Dictionary<string, object>> cartItems = new List<Dictionary<string, object>>();
            foreach (var ci in cart)
            {
                decimal taxRate;

                // ---

                decimal shoppingCartUnitPriceWithDiscountBase = _taxService.GetProductPrice(ci.Product, _priceCalculationService.GetUnitPrice(ci), out taxRate);
                decimal shoppingCartUnitPriceWithDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartUnitPriceWithDiscountBase, _workContext.WorkingCurrency);

                // ---

                List<Discount> scDiscounts;
                decimal shoppingCartItemDiscountBase;
                decimal shoppingCartItemSubTotalWithDiscountBase = _taxService.GetProductPrice(ci.Product, _priceCalculationService.GetSubTotal(ci, true, out shoppingCartItemDiscountBase, out scDiscounts), out taxRate);
                decimal shoppingCartItemSubTotalWithDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartItemSubTotalWithDiscountBase, _workContext.WorkingCurrency);

                // ---

                Dictionary<string, object> row = new Dictionary<string, object>();

                row.Add("Id", ci.Id);
                row.Add("StoreId", ci.StoreId);
                row.Add("ShoppingCartTypeId", ci.ShoppingCartTypeId);
                row.Add("CustomerId", ci.CustomerId);
                row.Add("Sku", ci.Product.FormatSku(ci.AttributesXml, _productAttributeParser));
                row.Add("Picture", _pictureService.GetPictureUrl(ci.Product.GetProductPicture(ci.AttributesXml, _pictureService, _productAttributeParser)));
                row.Add("ProductId", ci.ProductId);
                row.Add("ProductName", ci.Product.Name);
                row.Add("Url", Url.RouteUrl("Product", new { SeName = ci.Product.GetSeName() }));
                row.Add("ProductSeName", ci.Product.GetSeName());
                row.Add("UnitPrice", shoppingCartUnitPriceWithDiscount);
                row.Add("SubTotal", shoppingCartItemSubTotalWithDiscount);
                row.Add("Quantity", ci.Quantity);

                // ---

                QuantityTotal += ci.Quantity;

                cartItems.Add(row);
            }

            var ret = new
            {
                Subtotal = scSubtotal,
                Quantity = QuantityTotal,
                items = cartItems
            };

            /*
            var customers = _customerService.GetAllCustomers(
                loadOnlyWithShoppingCart: true,
                sct: ShoppingCartType.ShoppingCart);

            var ret = customers.First().ShoppingCartItems
                .Select(c =>
                    new
                    {
                        Id = c.Id,
                        StoreId = c.StoreId,
                        ShoppingCartTypeId = c.ShoppingCartTypeId,
                        CustomerId = c.CustomerId,
                        ProductId = c.ProductId,
                        Product = c.Product.Name,
                        Quantity = c.Quantity,
                        ItemSubtotal = (c.Product.Price * c.Quantity),
                        URL = c.Product.Url
                    });
            */

            return Successful(ret);
        }


        private string GetSCSubTotal(IList<ShoppingCartItem> cart)
        {
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

            return _priceFormatter.FormatPrice(subtotal, true, _workContext.WorkingCurrency, _workContext.WorkingLanguage, subTotalIncludingTax);
        }
        #endregion

        #region Helper methods

        private bool IsApiTokenValid(string apiToken)
        {
            if (string.IsNullOrWhiteSpace(apiToken) ||
                string.IsNullOrWhiteSpace(_settings.ApiToken))
                return false;

            return _settings.ApiToken.Trim().Equals(apiToken.Trim(),
                StringComparison.InvariantCultureIgnoreCase);
        }

        private object GetAddressJson(Address address)
        {
            if (address == null)
                return null;

            var addressJson = new
            {
                FirstName = address.FirstName,
                LastName = address.LastName,
                Email = address.Email,
                Company = address.Company,
                CountryId = address.CountryId,
                CountryName = address.Country == null ? null : address.Country.Name,
                StateProvinceId = address.StateProvinceId,
                StateProvinceName = address.StateProvince == null ? null : address.StateProvince.Name,
                City = address.City,
                Address1 = address.Address1,
                Address2 = address.Address2,
                ZipPostalCode = address.ZipPostalCode,
                PhoneNumber = address.PhoneNumber,
                FaxNumber = address.FaxNumber,
                CreatedOnUtc = address.CreatedOnUtc
            };

            return addressJson;
        }

        private object GetCustomerJson(Customer customer)
        {
            // TODO: refactor into own method for reuse
            var customerRoles = customer.CustomerRoles
                .Select(c =>
                    new
                    {
                        Id = c.Id,
                        Name = c.Name,
                        SystemName = c.SystemName
                    });

            // TODO: refactor into own method for reuse
            var externalAuthenticationRecords = customer.ExternalAuthenticationRecords
                .Select(e =>
                    new
                    {
                        Id = e.Id,
                        CustomerId = e.CustomerId,
                        Email = e.Email,
                        ExternalIdentifier = e.ExternalIdentifier,
                        ExternalDisplayIdentifier = e.ExternalDisplayIdentifier,
                        OAuthToken = e.OAuthToken,
                        OAuthAccessToken = e.OAuthAccessToken,
                        ProviderSystemName = e.ProviderSystemName
                    });

            // TODO: refactor into own method for reuse
            var shoppingCartItem = customer.ShoppingCartItems
                .Select(c =>
                    new
                    {
                        Id = c.Id,
                        StoreId = c.StoreId,
                        ShoppingCartTypeId = c.ShoppingCartTypeId,
                        CustomerId = c.CustomerId,
                        ProductId = c.ProductId,
                        AttributesXml = c.AttributesXml,
                        CustomerEnteredPrice = c.CustomerEnteredPrice,
                        Quantity = c.Quantity,
                        CreatedOnUtc = c.CreatedOnUtc,
                        UpdatedOnUtc = c.UpdatedOnUtc,
                        IsFreeShipping = c.IsFreeShipping,
                        IsShipEnabled = c.IsShipEnabled,
                        AdditionalShippingCharge = c.AdditionalShippingCharge,
                        IsTaxExempt = c.IsTaxExempt
                    });

            var customerJson = new
            {
                Id = customer.Id,
                CustomerGuid = customer.CustomerGuid,
                UserName = customer.Username,
                Email = customer.Email,
                CustomerRoles = customerRoles,
                AdminComment = customer.AdminComment,
                IsTaxExempt = customer.IsTaxExempt,
                AffiliateId = customer.AffiliateId,
                VendorId = customer.VendorId,
                HasShoppingCartItems = customer.HasShoppingCartItems,
                Active = customer.Active,
                Deleted = customer.Deleted,
                IsSystemAccount = customer.IsSystemAccount,
                SystemName = customer.SystemName,
                LastIpAddress = customer.LastIpAddress,
                CreatedOnUtc = customer.CreatedOnUtc,
                LastLoginDateUtc = customer.LastLoginDateUtc,
                LastActivityDateUtc = customer.LastActivityDateUtc,
                ExternalAuthenticationRecords = externalAuthenticationRecords,
                ShoppingCartItems = shoppingCartItem
            };

            return customerJson;
        }

        // TODO: add in paging info
        private object GetCustomersJson(IList<Customer> customers)
        {
            var customerJsonList = new List<object>();

            foreach (var customer in customers)
            {
                customerJsonList.Add(GetCustomerJson(customer));
            }

            return customerJsonList;
        }

        private object GetOrderJson(Order order, int vendorId = 0)
        {
            IEnumerable<OrderItem> orderItems = order.OrderItems;
            if (vendorId > 0)
                orderItems = orderItems.Where(item => item.Product.VendorId == vendorId);

            var orderJson = new
            {
                Id = order.Id,
                Customer = GetCustomerJson(order.Customer),
                BillingAddress = GetAddressJson(order.BillingAddress),
                ShippingAddress = GetAddressJson(order.ShippingAddress),
                OrderStatusId = order.OrderStatusId,
                OrderStatus = order.OrderStatus.ToString(),
                ShippingStatusId = order.ShippingStatusId,
                ShippingStatus = order.ShippingStatus.ToString(),
                PaymentStatusId = order.PaymentStatusId,
                PaymentStatus = order.PaymentStatus.ToString(),
                OrderItems = orderItems.Select(item => GetOrderItemJson(item)),
                // need to add in other properties to complete this
            };

            return orderJson;
        }

        // TODO: add in paging info
        private object GetOrdersJson(IPagedList<Order> orders, int vendorId = 0)
        {
            if (orders == null)
                return null;

            var ordersJson = orders.Select(o => GetOrderJson(o, vendorId));

            return ordersJson;
        }

        private object GetOrderItemJson(OrderItem orderItem)
        {
            var orderItemJson = new
            {
                AttributeDescription = orderItem.AttributeDescription,
                AttributesXml = orderItem.AttributesXml,
                DiscountAmountExclTax = orderItem.DiscountAmountExclTax,
                DiscountAmountInclTax = orderItem.DiscountAmountInclTax,
                DownloadCount = orderItem.DownloadCount,
                IsDownloadActivated = orderItem.IsDownloadActivated,
                ItemWeight = orderItem.ItemWeight,
                LicenseDownloadId = orderItem.LicenseDownloadId,
                //Order 
                OrderId = orderItem.OrderId,
                OrderItemGuid = orderItem.OrderItemGuid,
                OriginalProductCost = orderItem.OriginalProductCost,
                PriceExclTax = orderItem.PriceExclTax,
                PriceInclTax = orderItem.PriceInclTax,
                // Product 
                ProductId = orderItem.ProductId,
                ProductName = orderItem.Product.Name,
                Quantity = orderItem.Quantity,
                UnitPriceExclTax = orderItem.UnitPriceExclTax,
                UnitPriceInclTax = orderItem.UnitPriceInclTax,
            };

            return orderItemJson;
        }

        private object GetOrderNoteJson(OrderNote orderNote)
        {
            var orderNoteJson = new
            {
                CreatedOnUtc = orderNote.CreatedOnUtc,
                DisplayToCustomer = orderNote.DisplayToCustomer,
                DownloadId = orderNote.DownloadId,
                Note = orderNote.Note,
                OrderId = orderNote.OrderId,
            };

            return orderNoteJson;
        }

        private object GetAllStoresAvailableJson(Vendor vendor)
        {
            //var vendorStores = _storeMappingService.GetStoreMappings(vendor)
            //    .Select(x => x.StoreId)
            //    .ToArray();

            //    var stores = _storeService.GetAllStores();


            //    var storeList = new List<object>();

            ////    foreach(var store in stores)
            ////    {
            //        storeList.Add(new
            //    {
            //        Id = store.Id,
            //        Name = store.Name,


            //    });
            //    }



            //    var ret = new JArray();

            //  if(!vendorStores.Contains(store.Id))
            //        {
            //if(!vendorStores.Contains(store.Id))
            //{
            //            var storeJson = new
            //            {
            //                Id = store.Id,
            //                Name = store.Name
            //            };

            //}
            //        }
            //    }

            //    return ret;
            return new object();
        }

        #endregion
    }
}
