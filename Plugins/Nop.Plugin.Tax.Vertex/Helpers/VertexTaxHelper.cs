using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using Newtonsoft.Json;
using Nop.Plugin.Tax.Vertex.Domain;
using Nop.Plugin.Tax.Vertex.Controllers;
using Nop.Services.Logging;
using Nop.Services.Tax;
using Nop.Services.Orders;
using System.Xml;
using System.Configuration;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Tax;
using System.Text.RegularExpressions;
using Nop.Core;
using Nop.Core.Domain.Stores;
using System.Data.SqlClient;
using Nop.Services.Catalog;
using Nop.Core.Infrastructure;
using Nop.Services.Directory;
using Nop.Services.Common;
using Nop.Plugin.Tax.Vertex.Services;
using Nop.Services.Shipping;
using static Nop.Services.Orders.OrderProcessingService;
using ProductTax = Nop.Plugin.Tax.Vertex.Domain.ProductTax;
using Nop.Core.Domain.Catalog;

namespace Nop.Plugin.Tax.Vertex.Helpers
{
    /// <summary>
    /// Represents helper for the Vertex tax provider 
    /// </summary>
    public static class VertexTaxHelper
    {
        #region Properties


        /// <summary>
        /// Storing ProductID and taxrate retrieve from Quotation tax and used for Distributive tax 
        /// </summary>
        // public static List<ProductTax> producttax = new List<ProductTax>();

        /// <summary>
        /// Get an identifier of software client generating the API call
        /// </summary>
        public static string VERTEX_CLIENT
        {
            get { return "nopCommerce-VertexTaxRateProvider-1.21"; }
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Get Vertex tax provider service URL
        /// </summary>
        /// <param name="isSandbox">Whether it is sandbox environment</param>
        /// <returns>URL</returns>
        private static string GetServiceUrl(bool isSandbox)
        {
            return isSandbox
                ? "https://sodexo.ondemand.vertexinc.com/vertex-ws/services/CalculateTax80"
                : "https://avatax.Vertex.net/";
        }


        /// <summary>
        /// Check that plugin is configured
        /// </summary>
        /// <param name="VertexTaxSettings">Vertex tax settings</param>
        /// <returns>True if is configured; otherwise false</returns>
        private static bool IsConfigured(VertexTaxSettings VertexTaxSettings)
        {
            return !string.IsNullOrEmpty(VertexTaxSettings.AccountId) && !string.IsNullOrEmpty(VertexTaxSettings.LicenseKey);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Post tax request and get response from Vertex API
        /// </summary>
        /// <param name="taxRequest">Tax calculation request</param>
        /// <param name="VertexTaxSettings">Vertex tax settings</param>
        /// <param name="logger">Logger</param>
        /// <returns>The response from Vertex API</returns>
        public static TaxResponse PostTaxRequest(TaxRequest taxRequest, VertexTaxSettings VertexTaxSettings, ILogger logger)
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            ////validate addresses
            if (VertexTaxSettings.ValidateAddresses)
            {
                var errors = new List<Message>();
                foreach (var address in taxRequest.Addresses)
                {
                    //(only for US or Canadian address)
                    if (!address.Country.Equals("us", StringComparison.InvariantCultureIgnoreCase) &&
                        !address.Country.Equals("ca", StringComparison.InvariantCultureIgnoreCase))
                        continue;

                    var validatingResult = ValidateAddress(address, VertexTaxSettings, logger);
                    if (validatingResult != null)
                    {
                        //set new addresses details
                        if (validatingResult.ResultCode == SeverityLevel.Success && validatingResult.ValidatedAddress != null)
                        {
                            address.City = validatingResult.ValidatedAddress.City;
                            address.Country = validatingResult.ValidatedAddress.Country;
                            address.Line1 = validatingResult.ValidatedAddress.Line1;
                            address.Line2 = validatingResult.ValidatedAddress.Line2;
                            address.PostalCode = validatingResult.ValidatedAddress.PostalCode;
                            address.Region = validatingResult.ValidatedAddress.Region;
                        }
                        else
                            errors.AddRange(validatingResult.Messages);
                    }
                    else
                        errors.Add(new Message() { Summary = "Vertex error on validating address " });
                }
                if (errors.Any())
                    return new TaxResponse() { Messages = errors.ToArray(), ResultCode = SeverityLevel.Error };
            }

            //create post data
            var postData = Encoding.Default.GetBytes(JsonConvert.SerializeObject(taxRequest));

            //create web request
            var url = string.Format("{0}", GetServiceUrl(VertexTaxSettings.IsSandboxEnvironment));
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/json";
            request.ContentLength = postData.Length;

            try
            {
                //post request
                using (var stream = request.GetRequestStream())
                {
                    stream.Write(postData, 0, postData.Length);
                }

                //get response
                var httpResponse = (HttpWebResponse)request.GetResponse();

                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    return JsonConvert.DeserializeObject<TaxResponse>(streamReader.ReadToEnd());
                }
            }
            catch (WebException ex)
            {
                var httpResponse = (HttpWebResponse)ex.Response;
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    //log error                                  
                    var responseText = streamReader.ReadToEnd();
                    HandleVertexError(JsonConvert.SerializeObject(taxRequest), responseText, ex, logger);

                    return JsonConvert.DeserializeObject<TaxResponse>(responseText);
                }
            }
            catch (Exception ex)
            {
                //log error
                HandleVertexError(ex, logger, JsonConvert.SerializeObject(taxRequest));
                throw;
            }
        }

        public static List<string> PostTaxRequest(TaxRequest taxRequest, CalculateTaxRequest calculateTaxRequest, ILogger logger, ITaxCategoryService service, IGlCodeService glcodeService, Store store, Dictionary<int, decimal> changeinReqforSubtotalDisc = null)
        {

            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            List<XmlDocument> dataToPost = new List<XmlDocument>();


            List<string> responses = new List<string>();

            if (calculateTaxRequest.Product.TaxCategoryId == -1) //split
            {
                //Need to grab GL Codes because the tax categories are stored on each of the GLCodes
                List<ProductGlCode> glCodes = glcodeService.GetProductGlCodes(calculateTaxRequest.Product, GLCodeStatusType.Paid);
                var splitGls = glcodeService.GetProductsGlByType(1, glCodes, true, false, true); //only gls with taxcategory
                var bonusProduct = splitGls.Where(x => x.GlCode.Description.Contains("Bonus")).FirstOrDefault();
                int count = 1;
                foreach (var splitGL in splitGls)
                {

                    XmlDocument doc = BuildQuotationPostDataXML(calculateTaxRequest, taxRequest, service, store, null, splitGL, bonusProduct != null ? true : false, splitGls, count);
                    dataToPost.Add(doc);
                    TaxVertexController.StoreXMLResponseInDB(calculateTaxRequest.Customer.Id, calculateTaxRequest.Product.Id.ToString() + "-" + splitGL.Id.ToString(), Convert.ToInt32(Nop.Core.Domain.Tax.TaxRequestType.RequestQuotationTax), 0, 0, System.Web.HttpContext.Current.Session.SessionID, doc.InnerXml); //store the DT request              
                    count++;
                }

            }
            else
            {
                XmlDocument doc = new XmlDocument();
                int count = 0;
                if (changeinReqforSubtotalDisc != null)
                {
                    if (calculateTaxRequest.Product.IsBundleProduct)
                    {
                        //Find the matching cartitem from session
                        var workContext = EngineContext.Current.Resolve<IWorkContext>();
                        var cartItems = workContext.CurrentCustomer.ShoppingCartItems.Where(c => c.ProductId == calculateTaxRequest.Product.Id);
                        var cartItem = cartItems.First();

                        //Get the data from Session to setup Vertex Call.
                        Dictionary<string, string> associatedProductDict = new Dictionary<string, string>();
                        associatedProductDict = (Dictionary<string, string>)HttpContext.Current.Session["AssociatedProductPerShoppingCartItem"];
                        if (associatedProductDict != null && associatedProductDict.Count > 0 && !calculateTaxRequest.orderPlaced)
                        {
                            foreach (KeyValuePair<string, string> entry in associatedProductDict)
                            {
                                var scService = EngineContext.Current.Resolve<IShoppingCartService>();
                                string[] dictKey = entry.Key.Split('_');
                                string[] dictValue = entry.Value.Split('_');

                                ShoppingCartBundleProductItem scbp = new ShoppingCartBundleProductItem();
                                scbp.ShoppingCartItemId = Convert.ToInt32(dictKey[0]);
                                scbp.ParentProductId = Convert.ToInt32(dictKey[1]);
                                scbp.AssociatedProductId = Convert.ToInt32(dictKey[2]);
                                scbp.AssociatedProductName = dictValue[0];
                                scbp.Price = Convert.ToDecimal(dictValue[1]);
                                scbp.AssociatedProductTaxCategoryId = Convert.ToInt32(dictValue[2]);
                                scbp.Quantity = Convert.ToInt32(dictValue[3]);
                                doc = BuildQuotationPostDataXML(calculateTaxRequest, taxRequest, service, store, changeinReqforSubtotalDisc, null, false, null, count, scbp);
                                dataToPost.Add(doc);
                                count++;
                                TaxVertexController.StoreXMLResponseInDB(calculateTaxRequest.Customer.Id, scbp.AssociatedProductId.ToString(), Convert.ToInt32(TaxRequestType.RequestQuotationTax), 0, 0, HttpContext.Current.Session.SessionID, doc.InnerXml); //store the DT request              
                            }
                        }
                        else
                        {
                            var _shoppingcartService = EngineContext.Current.Resolve<IShoppingCartService>();
                            List<ShoppingCartBundleProductItem> scbpList = _shoppingcartService.GetAssociatedProductsPerShoppingCartItem(cartItem.Id).ToList();
                            ShoppingCartBundleProductItem scbpItem = scbpList.Where(x => x.AssociatedProductId == calculateTaxRequest.Product.Id).FirstOrDefault();
                            doc = BuildQuotationPostDataXML(calculateTaxRequest, taxRequest, service, store, changeinReqforSubtotalDisc, null, false, null, count, scbpItem);
                            dataToPost.Add(doc);
                            count++;
                            if(scbpItem!=null)
                            TaxVertexController.StoreXMLResponseInDB(calculateTaxRequest.Customer.Id, scbpItem.AssociatedProductId.ToString(), Convert.ToInt32(TaxRequestType.RequestQuotationTax), 0, 0, HttpContext.Current.Session.SessionID, doc.InnerXml); //store the DT request              
                        }
                    }
                    else
                    {
                        doc = BuildQuotationPostDataXML(calculateTaxRequest, taxRequest, service, store, changeinReqforSubtotalDisc, null, false, null, count);
                        dataToPost.Add(doc);
                        count++;
                        TaxVertexController.StoreXMLResponseInDB(calculateTaxRequest.Customer.Id, calculateTaxRequest.Product.Id.ToString(), Convert.ToInt32(TaxRequestType.RequestQuotationTax), 0, 0, HttpContext.Current.Session.SessionID, doc.InnerXml); //store the DT request              
                    }

                }
                else
                {
                    doc = BuildQuotationPostDataXML(calculateTaxRequest, taxRequest, service, store);
                    dataToPost.Add(doc);
                    TaxVertexController.StoreXMLResponseInDB(calculateTaxRequest.Customer.Id, calculateTaxRequest.Product.Id.ToString(), Convert.ToInt32(TaxRequestType.RequestQuotationTax), 0, 0, HttpContext.Current.Session.SessionID, doc.InnerXml); //store the DT request              
                }

            }

            foreach (XmlDocument postData in dataToPost)
            {
                HttpWebResponse httpResponse;
                int retry = 1;

                var url = string.Format("{0}", GetServiceUrl(true));
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.Headers.Add(@"SOAP:Action");
                request.Method = "POST";
                request.Timeout = 3000000;
                request.ContentType = "text/xml;charset=\"utf-8\"";
                try
                {
                    ServicePointManager.Expect100Continue = true;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                    //post request
                    using (var stream = request.GetRequestStream())
                    {
                        postData.Save(stream);
                    }

                //get response
                redo:
                    //get response
                    try
                    {

                        httpResponse = (HttpWebResponse)request.GetResponse();
                    }
                    catch (Exception ex)
                    {
                        retry++;
                        if (retry < 4)
                            goto redo;
                        else
                            throw;
                    }

                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        //Store Quotation response
                        string responseXml = streamReader.ReadToEnd();
                        //if (!TaxVertexController.CheckXMLResponseInDB(calculateTaxRequest.Customer.Id, Convert.ToString(calculateTaxRequest.Product.Id), calculateTaxRequest.TaxRequestType, System.Web.HttpContext.Current.Session.SessionID))
                        //{
                        TaxVertexController.StoreXMLResponseInDB(calculateTaxRequest.Customer.Id, Convert.ToString(calculateTaxRequest.Product.Id), calculateTaxRequest.TaxRequestType, 0, 0, System.Web.HttpContext.Current.Session.SessionID, responseXml);
                        //}
                        //else
                        //{

                        //}
                        responses.Add(responseXml);
                    }
                }
                catch (WebException ex)
                {
                    var httpResponseError = (HttpWebResponse)ex.Response;

                    if (httpResponseError != null)
                    {
                        using (var streamReader = new StreamReader(httpResponseError.GetResponseStream()))
                        {
                            //log error
                            string responseText = streamReader.ReadToEnd();
                            HandleVertexError(JsonConvert.SerializeObject(taxRequest), responseText, ex, logger);

                            responses.Add(responseText);
                        }
                    }

                    HandleVertexError(ex, logger, postData.InnerXml.ToString());
                    throw;
                }
                catch (Exception ex)
                {
                    //log error
                    HandleVertexError(ex, logger, postData.InnerXml.ToString());
                    throw;
                }
            }
            return responses;
        }


        /// <summary>
        /// Posts the distributeTax for all products in the cart
        /// </summary>
        /// <param name="details"></param>
        /// <param name="order"></param>
        /// <param name="store"></param>
        /// <param name="logger"></param>
        /// <param name="tcService"></param>
        public static void PostDistributeTaxRequest(OrderProcessingService.PlaceOrderContainter details,
            Order order, Store store, ILogger logger, ITaxCategoryService tcService, IGlCodeService glCodeService, IVertexTaxRateService taxRateService, ITaxService _taxDataService)
        {

            var workContext = EngineContext.Current.Resolve<IWorkContext>();
            var addressService = EngineContext.Current.Resolve<IAddressService>();
            var stateProvinceService = EngineContext.Current.Resolve<IStateProvinceService>();
            var shippingService = EngineContext.Current.Resolve<IShippingService>();
            var orderCalculationService = EngineContext.Current.Resolve<IOrderTotalCalculationService>();
            var taxService = EngineContext.Current.Resolve<IVertexTaxRateService>();
            var scService = EngineContext.Current.Resolve<IShoppingCartService>();

            var uniqueShippingRates = GetUniqueShippingRates(details, workContext, orderCalculationService);

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

            foreach (var orderItem in order.OrderItems)
            {
                if (orderItem.PriceInclTax == decimal.Zero)
                    continue;
                var originalProductName = orderItem.Product.Name;
                orderItem.Product.Name = HttpContext.Current.Server.HtmlEncode(orderItem.Product.Name);

                ShoppingCartItem cartItem = new ShoppingCartItem();
                decimal shippingrate = 0;
                if (orderItem.Product.IsBundleProduct)
                {
                    int shoppingCartId = scService.GetAssociatedProductsByOrderItemId(orderItem.Id).FirstOrDefault().ShoppingCartItemId;
                    cartItem = workContext.CurrentCustomer.ShoppingCartItems.Where(c => c.Id == shoppingCartId).First();
                }
                else
                {
                    var cartItems = workContext.CurrentCustomer.ShoppingCartItems.Where(c => c.ProductId == orderItem.Product.Id);
                    cartItem = cartItems.First();
                }

                foreach (var bucket in uniqueShippingRates)
                {
                    if (bucket.Key == cartItem.Id)
                    {
                        shippingrate = bucket.Value;
                        break;
                    }
                }


                var shippingOption = workContext.CurrentCustomer.GetAttribute<string>("SelectedShippingOption" + cartItem.Id, store.Id);


                if (shippingOption != null && (shippingOption.Contains("UPS")))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(shippingOption);
                    shippingrate = Decimal.Parse(doc.ChildNodes[1].ChildNodes[1].InnerText);
                }

                var shipFromAddress = new Nop.Core.Domain.Common.Address();
                if (orderItem.Product.IsMealPlan)  //should use local unit address.
                {
                    shipFromAddress = new Nop.Core.Domain.Common.Address
                    {
                        Address1 = store.CompanyAddress,
                        Address2 = store.CompanyAddress2,
                        City = store.CompanyCity,
                        StateProvinceId = store.CompanyStateProvinceId,
                        StateProvince = stateProvinceService.GetStateProvinceById(store.CompanyStateProvinceId.GetValueOrDefault()),
                        ZipPostalCode = store.CompanyZipPostalCode,
                        CountryId = store.CompanyCountryId
                    };
                }
                else if (shippingOption != null && (shippingOption.Contains("In-Store Pickup"))) //use the pickup location address
                {
                    //get selected warehouse                    
                    shipFromAddress = addressService.GetAddressById(cartItem.SelectedWarehouse.AddressId);
                }
                else if (shippingOption != null && (shippingOption.Contains("Delivery")))
                {
                    if (orderItem.Product.WarehouseId != 0)
                    {
                        var warehouse = shippingService.GetWarehouseById(orderItem.Product.WarehouseId);
                        shipFromAddress = addressService.GetAddressById(warehouse.AddressId); //Ryan Markle nu-90
                    }
                    else
                    {
                        shipFromAddress = new Nop.Core.Domain.Common.Address
                        {
                            Address1 = store.CompanyAddress,
                            Address2 = store.CompanyAddress2,
                            City = store.CompanyCity,
                            StateProvinceId = store.CompanyStateProvinceId,
                            StateProvince = stateProvinceService.GetStateProvinceById(store.CompanyStateProvinceId.GetValueOrDefault()),
                            ZipPostalCode = store.CompanyZipPostalCode,
                            CountryId = store.CompanyCountryId
                        };
                    }
                }
                else if (shippingOption != null && (shippingOption.Contains("UPS"))) //use the vendors warehouse.
                {

                    if (orderItem.Product.WarehouseId != 0)
                    {
                        var warehouse = shippingService.GetWarehouseById(orderItem.Product.WarehouseId);
                        shipFromAddress = addressService.GetAddressById(warehouse.AddressId); //Ryan Markle nu-90
                    }
                    else
                    {
                        shipFromAddress = new Nop.Core.Domain.Common.Address
                        {
                            Address1 = store.CompanyAddress,
                            Address2 = store.CompanyAddress2,
                            City = store.CompanyCity,
                            StateProvinceId = store.CompanyStateProvinceId,
                            StateProvince = stateProvinceService.GetStateProvinceById(store.CompanyStateProvinceId.GetValueOrDefault()),
                            ZipPostalCode = store.CompanyZipPostalCode,
                            CountryId = store.CompanyCountryId
                        };
                    }
                }
                else //use local unit    address
                {
                    shipFromAddress = new Nop.Core.Domain.Common.Address
                    {
                        Address1 = store.CompanyAddress,
                        Address2 = store.CompanyAddress2,
                        City = store.CompanyCity,
                        StateProvinceId = store.CompanyStateProvinceId,
                        StateProvince = stateProvinceService.GetStateProvinceById(store.CompanyStateProvinceId.GetValueOrDefault()),
                        ZipPostalCode = store.CompanyZipPostalCode,
                        CountryId = store.CompanyCountryId
                    };
                }

                //Ship to addrss
                var shipToAddress = new Nop.Core.Domain.Common.Address();
                if (orderItem.Product.IsMealPlan || orderItem.Product.IsDonation || orderItem.Product.IsDownload || orderItem.Product.IsGiftCard)  //should use local unit address.
                {
                    shipToAddress = new Nop.Core.Domain.Common.Address
                    {
                        Address1 = store.CompanyAddress,
                        Address2 = store.CompanyAddress2,
                        City = store.CompanyCity,
                        StateProvinceId = store.CompanyStateProvinceId,
                        StateProvince = stateProvinceService.GetStateProvinceById(store.CompanyStateProvinceId.GetValueOrDefault()),
                        ZipPostalCode = store.CompanyZipPostalCode,
                        CountryId = store.CompanyCountryId
                    };
                }
                else if (shippingOption != null && (shippingOption.Contains("In-Store Pickup"))) //use the pickup location address
                {
                    //get selected warehouse                    
                    shipToAddress = addressService.GetAddressById(cartItem.SelectedWarehouse.AddressId);
                }
                else if (shippingOption != null && (shippingOption.Contains("Delivery")))
                {
                    shipToAddress = workContext.CurrentCustomer.ShippingAddress == null ? workContext.CurrentCustomer.BillingAddress : workContext.CurrentCustomer.ShippingAddress; //Souradip Das nu-91
                }
                else if (!orderItem.Product.IsPickupEnabled && !orderItem.Product.IsShipEnabled && !orderItem.Product.IsLocalDelivery)
                {
                    if (!orderItem.Product.IsMealPlan && !orderItem.Product.IsDonation && !orderItem.Product.IsDownload)
                    {
                        shipToAddress = shipFromAddress;
                    }
                }
                else //use the customer shipping address for on campus delivery or UPS shiping
                {
                    shipToAddress = workContext.CurrentCustomer.ShippingAddress == null ? workContext.CurrentCustomer.BillingAddress : workContext.CurrentCustomer.ShippingAddress; //Souradip Das nu-91
                }
                if (orderItem.IsShipping && orderItem.SelectedShippingRateComputationMethodSystemName == "Tiered Shipping")
                {
                    shippingrate = orderItem.ShippingFee.Value;
                }

                XmlDocument postData = BuildDistributeTaxPostDataXML(orderItem, store, tcService, glCodeService, shipFromAddress, shipToAddress, shippingrate, taxService, _taxDataService, details, listofProd);

                HttpWebResponse httpResponse;
                int retry = 1;


                var url = string.Format("{0}", GetServiceUrl(true));
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.Headers.Add(@"SOAP:Action");
                request.Method = "POST";
                request.Timeout = 3000000;
                request.ContentType = "text/xml;charset=\"utf-8\"";
                string ProductIDs = "";

                ProductIDs = String.Concat(orderItem.ProductId.ToString() + "|");
                ProductIDs = ProductIDs.Trim();

                if (ProductIDs != "")
                {
                    ProductIDs = ProductIDs.Remove(ProductIDs.Length - 1, 1);
                    try
                    {
                        //post request
                        using (var stream = request.GetRequestStream())
                        {
                            postData.Save(stream);
                        }

                    //get response
                    redo:
                        //get response
                        try
                        {
                            httpResponse = (HttpWebResponse)request.GetResponse();
                        }
                        catch (Exception ex)
                        {
                            retry++;
                            if (retry < 4)
                                goto redo;
                            else
                                throw;
                        }

                        using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                        {
                            string responseXml = streamReader.ReadToEnd();
                            TaxVertexController.StoreXMLResponseInDB(order.Customer.Id, ProductIDs, Convert.ToInt32(Nop.Core.Domain.Tax.TaxRequestType.RequestDistributiveTax), order.Id, orderItem.Id, System.Web.HttpContext.Current.Session.SessionID, postData.InnerXml); //store the DT request
                            TaxVertexController.StoreXMLResponseInDB(order.Customer.Id, ProductIDs, Convert.ToInt32(Nop.Core.Domain.Tax.TaxRequestType.ConfirmDistributiveTax), order.Id, orderItem.Id, System.Web.HttpContext.Current.Session.SessionID, responseXml); //store the DT response
                        }
                    }
                    catch (WebException ex)
                    {
                        var httpResponseError = (HttpWebResponse)ex.Response;
                        using (var streamReader = new StreamReader(httpResponseError.GetResponseStream()))
                        {
                            //log error
                            var responseText = streamReader.ReadToEnd();
                            HandleVertexError(postData.InnerXml.ToString(), responseText, ex, logger);

                            throw;
                            //return JsonConvert.DeserializeObject<TaxResponse>(responseText);
                        }
                    }
                    catch (Exception ex)
                    {
                        HandleVertexError(ex, logger, postData.InnerXml.ToString());
                        throw;
                        //log error

                        //return null;
                    }
                }
                orderItem.Product.Name = originalProductName;
                // }
            }
        }

        public static List<KeyValuePair<int, decimal>> GetUniqueShippingRates(OrderProcessingService.PlaceOrderContainter details, IWorkContext workContext, IOrderTotalCalculationService orderCalculationService)
        {
            var buckets = orderCalculationService.GetShippingBuckets(details.Cart, workContext.CurrentCustomer);
            var newList = new List<KeyValuePair<int, decimal>>();
            foreach (var bucket in buckets)
            {
                if (bucket.Value.Key.Name == "Delivery")
                {
                    KeyValuePair<int, decimal> cartItemShippingRate = new KeyValuePair<int, decimal>(bucket.Value.Value[0].Id, bucket.Value.Value[0].SelectedWarehouse.DeliveryFee);
                    newList.Add(cartItemShippingRate);
                }
                else if (bucket.Value.Key.Name == "In-Store Pickup")
                {
                    KeyValuePair<int, decimal> cartItemShippingRate = new KeyValuePair<int, decimal>(bucket.Value.Value[0].Id, bucket.Value.Value[0].SelectedWarehouse.PickupFee);
                    newList.Add(cartItemShippingRate);
                }
            }
            return newList;
        }

        /// <summary>
        /// Posts the distributeTax for all products in the cart
        /// </summary>
        /// <param name="details"></param>
        /// <param name="order"></param>
        /// <param name="store"></param>
        /// <param name="logger"></param>
        /// <param name="tcService"></param>
        public static void PostDistributeTaxRefundRequest(Order order, int orderitemid, Store store, IGlCodeService glCodeService, ILogger logger, bool isPartialRefund = false, decimal AmountToRefund1 = 0, decimal AmountToRefund2 = 0, decimal AmountToRefund3 = 0, int ProductId = 0, List<int> data = null)
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            List<OrderItem> ProductItems = new List<OrderItem>();
            bool isSplit = false;
            var listofProd = new List<int>();
            if (isPartialRefund)
            {
                ProductItems = order.OrderItems.Where(x => x.ProductId == ProductId && x.Id == orderitemid).ToList();
                isSplit = (ProductItems[0].Product.TaxCategoryId == -1 ? true : false);

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
            }
            if (!isPartialRefund)
            {
                //var sameProductcount = order.OrderItems.Where(x => x.ProductId == ProductId).ToList();
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
            }
            //need to get DT Request XML from DB
            foreach (var items in isPartialRefund ? ProductItems : order.OrderItems)
            {

                if (items.PriceInclTax == decimal.Zero)
                    continue;
                //var  response = glCodeService.GetProductIdByOrderid(order.Id, TaxRequestType.RequestDistributiveTax);
                //string productToRefund = string.Empty;
                string productToRefund = items.ProductId.ToString();


                XmlDocument postData = BuildDistributeTaxRefundPostDataXML(order, items, store, glCodeService, productToRefund, isPartialRefund, listofProd, AmountToRefund1, AmountToRefund2, AmountToRefund3, isSplit, data);

                HttpWebResponse httpResponse;
                int retry = 1;


                var url = string.Format("{0}", GetServiceUrl(true));
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.Headers.Add(@"SOAP:Action");
                request.Method = "POST";
                request.Timeout = 3000000;
                request.ContentType = "text/xml;charset=\"utf-8\"";
                //request.ContentLength = postData.ToString().Length;
                string ProductIDs = "";

                // COMMENTING THIS AS WE DONOT WANT THE PRODUCT ID COLUMN TO CONCATENATE RECORDS AND PUT THERE AS IT SHALL PULLING UP RECORDS THAT ARE UNIQUE FOR EACH ITEM
                //foreach (var orderItem in isPartialRefund ? ProductItems : order.OrderItems)
                //{
                //    //   if (!details.Cart[i].IsTaxExempt)
                //    //  {
                //    ProductIDs = String.Concat(orderItem.Product.Id.ToString() + "|" + ProductIDs);
                //    ProductIDs = ProductIDs.Trim();
                //    //  }
                //}
                ProductIDs = items.Product.Id.ToString();

                if (ProductIDs != "")
                {
                    // ProductIDs = ProductIDs.Remove(ProductIDs.Length - 1, 1);

                    try
                    {
                        //post request
                        using (var stream = request.GetRequestStream())
                        {
                            postData.Save(stream);
                        }

                    //get response
                    redo:
                        //get response
                        try
                        {
                            httpResponse = (HttpWebResponse)request.GetResponse();
                        }
                        catch (Exception ex)
                        {
                            retry++;
                            if (retry < 4)
                                goto redo;
                            else
                                throw new Exception(ex.Message.ToString());
                        }

                        using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                        {
                            string responseXml = streamReader.ReadToEnd();

                            TaxVertexController.StoreXMLResponseInDB(order.Customer.Id, ProductIDs, Convert.ToInt32(Nop.Core.Domain.Tax.TaxRequestType.RequestDistributiveTaxRefund), order.Id, items.Id, System.Web.HttpContext.Current.Session.SessionID, postData.InnerXml); //store the DT request
                            TaxVertexController.StoreXMLResponseInDB(order.Customer.Id, ProductIDs, Convert.ToInt32(Nop.Core.Domain.Tax.TaxRequestType.ResponseDistributiveTaxRefund), order.Id, items.Id, System.Web.HttpContext.Current.Session.SessionID, responseXml); //store the DT response

                        }
                    }
                    catch (WebException ex)
                    {
                        var httpResponseError = (HttpWebResponse)ex.Response;
                        using (var streamReader = new StreamReader(httpResponseError.GetResponseStream()))
                        {
                            //log error
                            var responseText = streamReader.ReadToEnd();
                            HandleVertexError(postData.InnerXml.ToString(), responseText, ex, logger);

                            throw;
                            //return JsonConvert.DeserializeObject<TaxResponse>(responseText);
                        }
                    }
                    catch (Exception ex)
                    {
                        HandleVertexError(ex, logger, postData.InnerXml.ToString());
                        throw;
                        //log error

                        //return null;
                    }
                }
            }
        }

        public static void PostDistributeFullTaxRefundRequestPerItem(Order order, int orderitemId, Store store, IGlCodeService glCodeService, ILogger logger, bool isPartialRefund = false, decimal AmountToRefund1 = 0, decimal AmountToRefund2 = 0, decimal AmountToRefund3 = 0, int ProductId = 0, decimal taxAmoun1 = 0, decimal taxAmount2 = 0, decimal taxAmount3 = 0, decimal DeliveryAmount = 0, decimal DeliveryTax = 0, decimal ShippingAmount = 0, decimal ShippingTax = 0)
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            List<OrderItem> ProductItems = new List<OrderItem>();
            var listofProd = new List<int>();

            bool isSplit = false;
            if (isPartialRefund)
            {
                ProductItems = order.OrderItems.Where(x => x.ProductId == ProductId && x.Id == orderitemId).ToList();
                isSplit = (ProductItems[0].Product.TaxCategoryId == -1 ? true : false);

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
            }


            //int count = 0;
            //need to get DT Request XML from DB
            foreach (var items in isPartialRefund ? ProductItems : order.OrderItems)
            {


                //var response = glCodeService.GetProductIdByOrderid(order.Id, TaxRequestType.RequestDistributiveTax);
                //string productToRefund = string.Empty;
                string productToRefund = items.ProductId.ToString();


                XmlDocument postData = BuildDistributeFullTaxRefundPostDataPerItemXML(order, items, store, glCodeService, productToRefund, isPartialRefund, listofProd, AmountToRefund1, AmountToRefund2, AmountToRefund3, isSplit, taxAmoun1, taxAmount2, taxAmount3, DeliveryAmount, DeliveryTax, ShippingAmount, ShippingTax);

                HttpWebResponse httpResponse;
                int retry = 1;


                var url = string.Format("{0}", GetServiceUrl(true));
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.Headers.Add(@"SOAP:Action");
                request.Method = "POST";
                request.Timeout = 3000000;
                request.ContentType = "text/xml;charset=\"utf-8\"";
                //request.ContentLength = postData.ToString().Length;
                string ProductIDs = "";

                foreach (var orderItem in isPartialRefund ? ProductItems : order.OrderItems)
                {
                    //   if (!details.Cart[i].IsTaxExempt)
                    //  {
                    ProductIDs = String.Concat(orderItem.Product.Id.ToString() + "|" + ProductIDs);
                    ProductIDs = ProductIDs.Trim();
                    //  }
                }

                if (ProductIDs != "")
                {
                    ProductIDs = ProductIDs.Remove(ProductIDs.Length - 1, 1);

                    try
                    {
                        //post request
                        using (var stream = request.GetRequestStream())
                        {
                            postData.Save(stream);
                        }

                    //get response
                    redo:
                        //get response
                        try
                        {
                            httpResponse = (HttpWebResponse)request.GetResponse();
                        }
                        catch (Exception ex)
                        {
                            retry++;
                            if (retry < 4)
                                goto redo;
                            else
                                throw;
                        }

                        using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                        {
                            string responseXml = streamReader.ReadToEnd();

                            TaxVertexController.StoreXMLResponseInDB(order.Customer.Id, ProductIDs, Convert.ToInt32(Nop.Core.Domain.Tax.TaxRequestType.RequestDistributiveTaxRefund), order.Id, items.Id, System.Web.HttpContext.Current.Session.SessionID, postData.InnerXml); //store the DT request
                            TaxVertexController.StoreXMLResponseInDB(order.Customer.Id, ProductIDs, Convert.ToInt32(Nop.Core.Domain.Tax.TaxRequestType.ResponseDistributiveTaxRefund), order.Id, items.Id, System.Web.HttpContext.Current.Session.SessionID, responseXml); //store the DT response

                        }
                    }
                    catch (WebException ex)
                    {
                        var httpResponseError = (HttpWebResponse)ex.Response;
                        using (var streamReader = new StreamReader(httpResponseError.GetResponseStream()))
                        {
                            //log error
                            var responseText = streamReader.ReadToEnd();
                            HandleVertexError(postData.InnerXml.ToString(), responseText, ex, logger);

                            throw;
                            //return JsonConvert.DeserializeObject<TaxResponse>(responseText);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error("Vertex tax request error", ex);
                        throw;
                        //log error

                        //return null;
                    }
                }
            }
        }
        private static void HandleVertexError(string taxRequest, string taxResponse, Exception ex, ILogger logger)
        {
            logger.Error(string.Format("Vertex tax request error. Sodexo Request: --START-- {0} --END--, Vertex Response: -- START -- {1} -- END --", taxRequest, taxResponse), ex);
        }
        private static void HandleVertexError(Exception ex, ILogger logger, string optional = null)
        {
            // In the event this doesn't appear to be a webrequest exception
            logger.Error($"Unhandled exception in Vertex Tax Request with message {ex.Message}, Stack Trace {ex.StackTrace}, extra info: {optional}");
        }
        ///// <summary>
        ///// Post Quotation tax after clicking checkout for all product
        ///// </summary>
        ///// <param name="logger"></param>
        ///// <param name="cart"></param>
        //public static void PostQuotationTaxRequestinBulk(IList<ShoppingCartItem> cart, ILogger logger, ITaxCategoryService service, Store store)
        //{

        //    foreach(var shoppingCartItem  in cart)
        //    {
        //        XmlDocument postData = BuildQuotationPostBulkDataXML(cart, service, store);
        //    }




        //    HttpWebResponse httpResponse;
        //    int retry = 1;

        //    var url = string.Format("{0}", GetServiceUrl(true));
        //    var request = (HttpWebRequest)WebRequest.Create(url);
        //    request.Headers.Add(@"SOAP:Action");
        //    request.Method = "POST";
        //    request.Timeout = 3000000;
        //    request.ContentType = "text/xml;charset=\"utf-8\"";
        //    string ProductIDs = "";

        //    for (int i = 0; i < cart.Count; i++)
        //    {
        //        if (!cart[i].IsTaxExempt)
        //        {
        //            ProductIDs = String.Concat(cart[i].ProductId.ToString() + "|" + ProductIDs);
        //            ProductIDs = ProductIDs.Trim();
        //        }
        //    }

        //    if (ProductIDs != "")
        //    {
        //        ProductIDs = ProductIDs.Remove(ProductIDs.Length - 1, 1);

        //        try
        //        {
        //            //post request
        //            using (var stream = request.GetRequestStream())
        //            {
        //                postData.Save(stream);
        //            }

        //        redo:
        //            //get response
        //            try
        //            {
        //                httpResponse = (HttpWebResponse)request.GetResponse();
        //            }
        //            catch (Exception ex)
        //            {
        //                retry++;
        //                if (retry < 4)
        //                    goto redo;
        //                else
        //                    throw;
        //            }

        //            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
        //            {
        //                string responseXml = streamReader.ReadToEnd();

        //                AddTaxToDictfromXML(responseXml);
        //                TaxVertexController.StoreXMLResponseInDB(cart[0].Customer.Id, ProductIDs, Convert.ToInt32(Nop.Core.Domain.Tax.TaxRequestType.RequestBulkQuotationTax), 0, System.Web.HttpContext.Current.Session.SessionID, postData.InnerXml); //store the quote request
        //                TaxVertexController.StoreXMLResponseInDB(cart[0].Customer.Id, ProductIDs, Convert.ToInt32(Nop.Core.Domain.Tax.TaxRequestType.ResponseBulkQuotationTax), 0, System.Web.HttpContext.Current.Session.SessionID, responseXml); //store the quotate response
        //            }
        //        }
        //        catch (WebException ex)
        //        {
        //            var httpResponseError = (HttpWebResponse)ex.Response;
        //            using (var streamReader = new StreamReader(httpResponseError.GetResponseStream()))
        //            {
        //                //log error
        //                string responseText = streamReader.ReadToEnd();
        //                logger.Error(string.Format("Vertex tax request error: {0}", responseText), ex);
        //                throw;
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            //log error
        //            logger.Error("Vertex tax request error", ex);
        //            throw;
        //        }
        //    }
        //}

        /// <summary>
        /// Post cancel tax request and get response from Vertex API
        /// </summary>
        /// <param name="cancelRequest">Cancel tax request</param>
        /// <param name="VertexTaxSettings">Vertex tax settings</param>
        /// <param name="logger">Logger</param>
        /// <returns>The response from Vertex API</returns>
        public static CancelTaxResponse CancelTaxRequest(CancelTaxRequest cancelRequest, VertexTaxSettings VertexTaxSettings, ILogger logger)
        {
            if (!IsConfigured(VertexTaxSettings))
            {
                logger.Error("Vertex tax provider is not configured");
                return null;
            }

            //create post data
            var postData = Encoding.Default.GetBytes(JsonConvert.SerializeObject(cancelRequest));

            //create web request
            var url = string.Format("{0}1.0/tax/cancel", GetServiceUrl(VertexTaxSettings.IsSandboxEnvironment));
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/json";
            request.ContentLength = postData.Length;

            //add authorization header
            var login = string.Format("{0}:{1}", VertexTaxSettings.AccountId, VertexTaxSettings.LicenseKey);
            var authorization = Convert.ToBase64String(Encoding.ASCII.GetBytes(login));
            request.Headers.Add(HttpRequestHeader.Authorization, string.Format("Basic {0}", authorization));

            try
            {
                //post request
                using (var stream = request.GetRequestStream())
                {
                    stream.Write(postData, 0, postData.Length);
                }

                //get response
                var httpResponse = (HttpWebResponse)request.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    return JsonConvert.DeserializeObject<CancelTaxResponse>(streamReader.ReadToEnd());
                }
            }
            catch (WebException ex)
            {
                var httpResponse = (HttpWebResponse)ex.Response;
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    //log error
                    var responseText = streamReader.ReadToEnd();
                    HandleVertexError(JsonConvert.SerializeObject(cancelRequest), responseText, ex, logger);

                    return JsonConvert.DeserializeObject<CancelTaxResponse>(responseText);
                }
            }
            catch (Exception ex)
            {
                //not a web error so prob network or something else like that
                HandleVertexError(ex, logger, JsonConvert.SerializeObject(cancelRequest));
                throw;
            }
        }

        /// <summary>
        /// Post validate address request and get response from Vertex API
        /// </summary>
        /// <param name="address">Address to validate</param>
        /// <param name="VertexTaxSettings">Vertex tax settings</param>
        /// <param name="logger">Logger</param>
        /// <returns>The response from Vertex API</returns>
        public static ValidateAddressResponse ValidateAddress(Address address, VertexTaxSettings VertexTaxSettings, ILogger logger)
        {
            if (!IsConfigured(VertexTaxSettings))
            {
                logger.Error("Vertex tax provider is not configured");
                return null;
            }

            //construct service url
            var url = string.Format("{0}1.0/address/validate", GetServiceUrl(VertexTaxSettings.IsSandboxEnvironment));

            //add query parameters
            var uriBuilder = new UriBuilder(url);
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            query["Line1"] = address.Line1;
            query["Line2"] = address.Line2;
            query["City"] = address.City;
            query["Region"] = address.Region;
            query["Country"] = address.Country;
            query["PostalCode"] = address.PostalCode;
            uriBuilder.Query = HttpUtility.UrlPathEncode(query.ToString());

            //create web request
            var request = (HttpWebRequest)WebRequest.Create(uriBuilder.Uri);
            request.Method = "GET";
            request.ContentType = "application/json";

            //add authorization header
            var login = string.Format("{0}:{1}", VertexTaxSettings.AccountId, VertexTaxSettings.LicenseKey);
            var authorization = Convert.ToBase64String(Encoding.ASCII.GetBytes(login));
            request.Headers.Add(HttpRequestHeader.Authorization, string.Format("Basic {0}", authorization));

            try
            {
                //get response
                var httpResponse = (HttpWebResponse)request.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    return JsonConvert.DeserializeObject<ValidateAddressResponse>(streamReader.ReadToEnd());
                }
            }
            catch (WebException ex)
            {
                var httpResponse = (HttpWebResponse)ex.Response;
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    //log error
                    var responseText = streamReader.ReadToEnd();
                    HandleVertexError(uriBuilder.Query, responseText.ToString(), ex, logger);

                    return JsonConvert.DeserializeObject<ValidateAddressResponse>(responseText);
                }
            }
            catch (Exception ex)
            {
                //log error
                HandleVertexError(ex, logger, uriBuilder.Query);
                throw;
            }
        }

        /// <summary>
        /// Create XMlDocument for single Product for AddToCarT
        /// </summary>
        /// <param name="calculateTaxRequest"></param>
        /// <param name="taxRequest"></param>
        /// <returns></returns>
        public static XmlDocument BuildQuotationPostDataXML(CalculateTaxRequest calculateTaxRequest, TaxRequest taxRequest, ITaxCategoryService _taxCategoryService, Store store, Dictionary<int, decimal> changeinReqforSubtotalDisc = null, ProductGlCode glCode = null, bool isBonus = false, List<ProductGlCode> codes = null, int count = 0, ShoppingCartBundleProductItem scbp = null)
        {
            string UserName = ConfigurationManager.AppSettings["VertexTaxUserName"].ToString();
            string Password = ConfigurationManager.AppSettings["VertexTaxPassword"].ToString();
            //string trustedId = ConfigurationManager.AppSettings["TrustedId"].ToString();

            var workContext = EngineContext.Current.Resolve<IWorkContext>();
            var addressService = EngineContext.Current.Resolve<IAddressService>();
            var stateProvinceService = EngineContext.Current.Resolve<IStateProvinceService>();
            if (changeinReqforSubtotalDisc != null && changeinReqforSubtotalDisc.Any())
            {
                calculateTaxRequest.Price = changeinReqforSubtotalDisc[calculateTaxRequest.Product.Id];
            }
            var cartItems = workContext.CurrentCustomer.ShoppingCartItems.Where(c => c.ProductId == calculateTaxRequest.Product.Id);
            var cartItem = cartItems.First();


            decimal shippingrate = 0;
            var shippingOption = workContext.CurrentCustomer.GetAttribute<string>("SelectedShippingOption" + cartItem.Id, store.Id);
            if (store.IsTieredShippingEnabled == false)
            {
                if (shippingOption != null && (shippingOption.Contains("In-Store Pickup") || shippingOption.Contains("Delivery")))
                {
                    if (shippingOption.Contains("In-Store Pickup"))
                    {
                        shippingrate = cartItem.SelectedWarehouse.PickupFee;
                    }
                    else if (shippingOption.Contains("Delivery"))
                    {
                        shippingrate = cartItem.SelectedWarehouse.DeliveryFee;
                    }

                }
                else if (shippingOption != null && (shippingOption.Contains("UPS")))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(shippingOption);
                    shippingrate = Decimal.Parse(doc.ChildNodes[1].ChildNodes[1].InnerText);
                }
            }
            else
            {
                if (cartItem.IsTieredShippingEnabled && cartItem.FlatShipping > 0)
                {
                    shippingrate = cartItem.FlatShipping;
                }
                else
                {
                    //Update the tax rate to 0 if tiered shipping was enabled in previous run in the same cart
                    updateTaxForTieredShipping(cartItem.Product.Id, HttpContext.Current.Session.SessionID, "SHIP");
                }
            }

            TaxCategory taxCategory = calculateTaxRequest.Product.IsBundleProduct == true && scbp != null ? _taxCategoryService.GetTaxCategoryById(scbp.AssociatedProductTaxCategoryId) : _taxCategoryService.GetTaxCategoryById(calculateTaxRequest.Product.TaxCategoryId);
            XmlDocument xmlRequest = new XmlDocument();
            StringBuilder xmlBuilder = new StringBuilder(@"<?xml version=""1.0"" encoding=""utf-8""?>");
            xmlBuilder.AppendLine("<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:urn=\"urn:vertexinc:o-series:tps:8:0\">");
            xmlBuilder.AppendLine("<soapenv:Header />");
            xmlBuilder.AppendLine("<soapenv:Body>");
            xmlBuilder.AppendLine("<urn:VertexEnvelope>");
            xmlBuilder.AppendLine("<urn:Login>");
            xmlBuilder.AppendLine("<urn:UserName>" + UserName + "</urn:UserName>");
            xmlBuilder.AppendLine("<urn:Password>" + Password + "</urn:Password>");
            //xmlBuilder.AppendLine("<urn:TrustedId>" + trustedId + "</urn:TrustedId>");
            xmlBuilder.AppendLine("</urn:Login>");
            xmlBuilder.AppendLine("<urn:QuotationRequest documentDate=" + "\"" + DateTime.Now.ToString("yyyy-MM-dd") + "\"" + " " + "transactionType=\"SALE\">");

            //Seller
            xmlBuilder.AppendLine("<urn:Seller>");
            xmlBuilder.AppendLine("<urn:Company>1000</urn:Company>"); //taxRequest.CompanyCode
            xmlBuilder.AppendLine("<urn:Division>" + store.LegalEntity + "</urn:Division>");
            xmlBuilder.AppendLine("<urn:PhysicalOrigin>");
            var _stateProvinceService = EngineContext.Current.Resolve<IStateProvinceService>();
            var stateCode = _stateProvinceService.GetStateProvinceById(calculateTaxRequest.ShipFromAddress.StateProvinceId.GetValueOrDefault(0)).Abbreviation;
            xmlBuilder.AppendLine("<urn:StreetAddress1>" + calculateTaxRequest.ShipFromAddress.Address1 + "</urn:StreetAddress1>");

            if (!String.IsNullOrEmpty(calculateTaxRequest.ShipFromAddress.Address2))
            {
                xmlBuilder.AppendLine("<urn:StreetAddress2>" + calculateTaxRequest.ShipFromAddress.Address2 + "</urn:StreetAddress2>");
            }
            xmlBuilder.AppendLine("<urn:City>" + calculateTaxRequest.ShipFromAddress.City + "</urn:City>");
            xmlBuilder.AppendLine("<urn:MainDivision>" + stateCode + "</urn:MainDivision>");
            xmlBuilder.AppendLine("<urn:PostalCode>" + calculateTaxRequest.ShipFromAddress.ZipPostalCode + "</urn:PostalCode>");
            xmlBuilder.AppendLine("<urn:Country>" + "US" + "</urn:Country>");
            xmlBuilder.AppendLine("</urn:PhysicalOrigin>");
            xmlBuilder.AppendLine("</urn:Seller>");
            //Seller

            //customer
            xmlBuilder.AppendLine("<urn:Customer>");
            xmlBuilder.AppendLine("<urn:Destination>");
            xmlBuilder.AppendLine("<urn:StreetAddress1>" + taxRequest.Addresses[1].Line1 + "</urn:StreetAddress1>");

            if (!String.IsNullOrEmpty(taxRequest.Addresses[1].Line2))
            {
                xmlBuilder.AppendLine("<urn:StreetAddress2>" + taxRequest.Addresses[1].Line2 + "</urn:StreetAddress2>");
            }
            xmlBuilder.AppendLine("<urn:City>" + taxRequest.Addresses[1].City + "</urn:City>");
            xmlBuilder.AppendLine("<urn:MainDivision>" + taxRequest.Addresses[1].Region + "</urn:MainDivision>");
            xmlBuilder.AppendLine("<urn:PostalCode>" + taxRequest.Addresses[1].PostalCode + "</urn:PostalCode>");
            xmlBuilder.AppendLine("<urn:Country>" + "US" + "</urn:Country>");
            xmlBuilder.AppendLine("</urn:Destination>");
            xmlBuilder.AppendLine("</urn:Customer>");
            //Customer

            //LineItem
            int lineItem = 1;
            var originalProductName = calculateTaxRequest.Product.IsBundleProduct == true && scbp != null ? scbp.AssociatedProductName : calculateTaxRequest.Product.Name;
            var productName = calculateTaxRequest.Product.IsBundleProduct == true && scbp != null ? scbp.AssociatedProductName.Replace(@"""", "").Trim() : calculateTaxRequest.Product.Name.Replace(@"""", "").Trim();
            productName = calculateTaxRequest.Product.IsBundleProduct == true && scbp != null ? HttpContext.Current.Server.HtmlEncode(productName) : HttpContext.Current.Server.HtmlEncode(productName);

            if (glCode == null)
            {
                int prodId = calculateTaxRequest.Product.IsBundleProduct == true && scbp != null ? scbp.AssociatedProductId : calculateTaxRequest.Product.Id;
                xmlBuilder.AppendLine("<urn:LineItem lineItemNumber=\"" + lineItem + "\" lineItemId=" + "\"" + prodId + "\">");

                int maxLength = Math.Min(productName.Length, 38);
                decimal scbpMultipleItems = 0.0M;
                decimal extendedPrice = 0.0M;

                if (calculateTaxRequest.Product.IsBundleProduct)
                {
                    if(scbp!=null)
                    scbpMultipleItems = calculateTaxRequest.Quantity > 1 ? Convert.ToDecimal(scbp.Price * calculateTaxRequest.Quantity) : scbp.Price;
                    extendedPrice = decimal.Round(scbp != null ? scbpMultipleItems : calculateTaxRequest.Price, 4);
                }
                else
                    extendedPrice = decimal.Round(calculateTaxRequest.Price, 4);

                xmlBuilder.AppendLine("<urn:Product productClass=" + "\"" + taxCategory.Code + "\">" + productName.Substring(0, maxLength) + "</urn:Product>");
                xmlBuilder.AppendLine("<urn:ExtendedPrice>" + extendedPrice.ToString() + "</urn:ExtendedPrice>");
                xmlBuilder.AppendLine("</urn:LineItem>");
                lineItem++;
            }
            else //split
            {
                decimal? Amount = 0;
                if (isBonus)
                {
                    var SplitGlDepositCashForBonus = System.Configuration.ConfigurationManager.AppSettings["DepositCash"];
                    if (!string.IsNullOrEmpty(SplitGlDepositCashForBonus))
                    {
                        var cashvalueGl = glCode.GlCode.GlCodeName;
                        if (SplitGlDepositCashForBonus.Contains(cashvalueGl))
                        {
                            if (isBonus && glCode.GlCode.GlCodeName == cashvalueGl) //DEPOSITS CASH VALUE BOARD
                            {
                                Amount = Convert.ToDecimal(codes.Where(x => x.GlCode.GlCodeName == cashvalueGl).FirstOrDefault().Amount + codes.Where(x => x.GlCode.GlCodeName == "64701000").FirstOrDefault().Amount) * calculateTaxRequest.Quantity;
                            }
                        }
                    }
                    if (isBonus && glCode.GlCode.GlCodeName == "40116050")//MEAL PLAN DEPOSITS-OFF-CAMPUS
                    {
                        Amount = Convert.ToDecimal(codes.Where(x => x.GlCode.GlCodeName == "40116050").FirstOrDefault().Amount) * calculateTaxRequest.Quantity;
                    }
                    if (isBonus && glCode.GlCode.GlCodeName == "64701000")//Declining Balance Bonus
                    {
                        Amount = Convert.ToDecimal(-codes.Where(x => x.GlCode.GlCodeName == "64701000").FirstOrDefault().Amount) * calculateTaxRequest.Quantity;
                    }
                    else if (isBonus && _taxCategoryService.GetTaxCategoryById(glCode.TaxCategoryId).Code == "HCHG")
                    {
                        Amount = Convert.ToDecimal(codes.Where(x => x.TaxCategoryId == glCode.TaxCategoryId).FirstOrDefault().Amount) * calculateTaxRequest.Quantity;
                    }
                }
                if (!isBonus)
                {
                    Amount = glCode.Amount * calculateTaxRequest.Quantity;
                }
                int maxLength = Math.Min(productName.Length, 38);
                int prodId = calculateTaxRequest.Product.IsBundleProduct == true && scbp != null ? scbp.AssociatedProductId : calculateTaxRequest.Product.Id;

                xmlBuilder.AppendLine("<urn:LineItem lineItemNumber=\"" + lineItem + "\" lineItemId=" + "\"" + prodId + "-" + glCode.Id + "\">");
                xmlBuilder.AppendLine("<urn:Product productClass=" + "\"" + _taxCategoryService.GetTaxCategoryById(glCode.TaxCategoryId).Code + "\">" + productName.Substring(0, maxLength) + "</urn:Product>");
                xmlBuilder.AppendLine("<urn:ExtendedPrice>" + Amount + "</urn:ExtendedPrice>");
                xmlBuilder.AppendLine("</urn:LineItem>");
                lineItem++;
            }

            if (shippingrate != 0)
            {
                int prodId = calculateTaxRequest.Product.IsBundleProduct == true && scbp != null ? scbp.AssociatedProductId : calculateTaxRequest.Product.Id;
                if (shippingOption != null && shippingOption.Contains("UPS"))
                {
                    xmlBuilder.AppendLine("<urn:LineItem lineItemNumber=\"" + lineItem + "\" lineItemId=" + "\"" + prodId + "-" + "62" + "\">");

                    int maxLength = Math.Min(productName.Length, 38);

                    xmlBuilder.AppendLine("<urn:Product productClass=" + "\"" + "SHIP" + "\">" + productName.Substring(0, maxLength) + "</urn:Product>");
                    xmlBuilder.AppendLine("<urn:ExtendedPrice>" + shippingrate + "</urn:ExtendedPrice>");
                    xmlBuilder.AppendLine("</urn:LineItem>");
                }
                else if (cartItem.IsTieredShippingEnabled == true && cartItem.IsFirstCartItem == true)
                {
                    xmlBuilder.AppendLine("<urn:LineItem lineItemNumber=\"" + lineItem + "\" lineItemId=" + "\"" + prodId + "-" + "62" + "\">");

                    int maxLength = Math.Min(productName.Length, 38);

                    xmlBuilder.AppendLine("<urn:Product productClass=" + "\"" + "SHIP" + "\">" + productName.Substring(0, maxLength) + "</urn:Product>");
                    xmlBuilder.AppendLine("<urn:ExtendedPrice>" + shippingrate + "</urn:ExtendedPrice>");
                    xmlBuilder.AppendLine("</urn:LineItem>");
                }
                else
                {
                    xmlBuilder.AppendLine("<urn:LineItem lineItemNumber=\"" + lineItem + "\" lineItemId=" + "\"" + prodId + "-" + "61" + "\">");
                    int maxLength = Math.Min(productName.Length, 38);
                    xmlBuilder.AppendLine("<urn:Product productClass=" + "\"" + "DELV" + "\">" + productName.Substring(0, maxLength) + "</urn:Product>");
                    xmlBuilder.AppendLine("<urn:ExtendedPrice>" + shippingrate + "</urn:ExtendedPrice>");
                    xmlBuilder.AppendLine("</urn:LineItem>");
                }
                lineItem++;
            }

            //LineItem
            xmlBuilder.AppendLine("</urn:QuotationRequest>");
            xmlBuilder.AppendLine("</urn:VertexEnvelope>");
            xmlBuilder.AppendLine("</soapenv:Body>");
            xmlBuilder.AppendLine("</soapenv:Envelope>");
            xmlRequest.LoadXml(Regex.Replace(xmlBuilder.ToString(), @"\t|\n|\r", ""));

            //calculateTaxRequest.Product.Name = originalProductName;
            return xmlRequest;
        }


        public static XmlDocument BuildDistributeTaxPostDataXML(OrderItem orderItem, Store store, ITaxCategoryService categoryService, IGlCodeService glCodeService, Nop.Core.Domain.Common.Address shipFromAddress, Nop.Core.Domain.Common.Address shipToAddress, decimal shippingRate, IVertexTaxRateService taxService, ITaxService _taxservices, OrderProcessingService.PlaceOrderContainter details, List<int> itemProducts = null, ShoppingCartBundleProductItem scbpi = null)
        {
            string UserName = ConfigurationManager.AppSettings["VertexTaxUserName"].ToString();
            string Password = ConfigurationManager.AppSettings["VertexTaxPassword"].ToString();
            decimal tax = decimal.Zero;
            decimal taxRate = decimal.Zero;
            XmlDocument xmlRequest = new XmlDocument();

            StringBuilder xmlBuilder = new StringBuilder(@"<?xml version=""1.0"" encoding=""utf-8""?>");
            xmlBuilder.AppendLine("<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:urn=\"urn:vertexinc:o-series:tps:8:0\">");
            xmlBuilder.AppendLine("<soapenv:Header />");
            xmlBuilder.AppendLine("<soapenv:Body>");
            xmlBuilder.AppendLine("<urn:VertexEnvelope>");
            xmlBuilder.AppendLine("<urn:Login>");
            xmlBuilder.AppendLine("<urn:UserName>" + UserName + "</urn:UserName>");
            xmlBuilder.AppendLine("<urn:Password>" + Password + "</urn:Password>");
            xmlBuilder.AppendLine("</urn:Login>");
            xmlBuilder.AppendLine("<urn:DistributeTaxRequest documentNumber=" + "\"" + orderItem.Order.Id.ToString() + "\"" + " " + "documentDate=" + "\"" + DateTime.Now.ToString("yyyy-MM-dd") + "\"" + " " + "transactionType=\"SALE\">");

            //Seller
            xmlBuilder.AppendLine("<urn:Seller>");
            xmlBuilder.AppendLine("<urn:Company>1000</urn:Company>");
            xmlBuilder.AppendLine("<urn:Division>" + store.LegalEntity + "</urn:Division>");
            xmlBuilder.AppendLine("<urn:PhysicalOrigin>");


            var _stateProvinceService = EngineContext.Current.Resolve<IStateProvinceService>();
            var stateCode = _stateProvinceService.GetStateProvinceById(shipFromAddress.StateProvinceId.GetValueOrDefault(0)).Abbreviation;

            xmlBuilder.AppendLine("<urn:StreetAddress1>" + shipFromAddress.Address1 + "</urn:StreetAddress1>");

            if (!String.IsNullOrEmpty(shipFromAddress.Address2))
            {
                xmlBuilder.AppendLine("<urn:StreetAddress2>" + shipFromAddress.Address2 + "</urn:StreetAddress2>");
            }
            xmlBuilder.AppendLine("<urn:City>" + shipFromAddress.City + "</urn:City>");
            xmlBuilder.AppendLine("<urn:MainDivision>" + stateCode + "</urn:MainDivision>");
            xmlBuilder.AppendLine("<urn:PostalCode>" + shipFromAddress.ZipPostalCode + "</urn:PostalCode>");
            xmlBuilder.AppendLine("<urn:Country>" + "US" + "</urn:Country>");
            xmlBuilder.AppendLine("</urn:PhysicalOrigin>");
            xmlBuilder.AppendLine("</urn:Seller>");
            //Seller


            //customer
            xmlBuilder.AppendLine("<urn:Customer>");
            xmlBuilder.AppendLine("<urn:CustomerCode>" + orderItem.Order.CustomerId + "</urn:CustomerCode>");
            xmlBuilder.AppendLine("<urn:Destination>");

            xmlBuilder.AppendLine("<urn:StreetAddress1>" + shipToAddress.Address1 + "</urn:StreetAddress1>");

            if (!String.IsNullOrEmpty(shipToAddress.Address2))
            {
                xmlBuilder.AppendLine("<urn:StreetAddress2>" + shipToAddress.Address2 + "</urn:StreetAddress2>");
            }
            xmlBuilder.AppendLine("<urn:City>" + shipToAddress.City + "</urn:City>");
            xmlBuilder.AppendLine("<urn:MainDivision>" + shipToAddress.StateProvince.Abbreviation + "</urn:MainDivision>");
            xmlBuilder.AppendLine("<urn:PostalCode>" + shipToAddress.ZipPostalCode + "</urn:PostalCode>");
            xmlBuilder.AppendLine("<urn:Country>" + "US" + "</urn:Country>");
            xmlBuilder.AppendLine("</urn:Destination>");

            xmlBuilder.AppendLine("</urn:Customer>");
            //customer

            //lineItems

            var productName = scbpi != null ? scbpi.AssociatedProductName.Replace(@"""", "") : orderItem.Product.Name.Replace(@"""", "");

            if ((scbpi != null && scbpi.AssociatedProductTaxCategoryId == -1) || orderItem.Product.TaxCategoryId == -1) //split, must add 2 line items for each.
            {
                List<ProductGlCode> glCodes = new List<ProductGlCode>();
                if (scbpi != null)
                {
                    glCodes = glCodeService.GetProductGlCodesByProductId(scbpi.AssociatedProductId, GLCodeStatusType.Paid);
                }
                else
                {
                    glCodes = glCodeService.GetProductGlCodes(orderItem.Product, GLCodeStatusType.Paid);
                }
                var splitGls = glCodeService.GetProductsGlByType(1, glCodes, true, false, true); //only GLs with tax categories
                int i = 1;

                var bonusProduct = splitGls.Where(x => x.GlCode.Description.Contains("Bonus")).FirstOrDefault();
                foreach (ProductGlCode glCode in splitGls)
                {
                    decimal? Amount = 0;
                    decimal UnitPrice = 0;
                    int quantity = 1;
                    if (scbpi != null)
                        quantity = scbpi.Quantity;
                    else
                        quantity = orderItem.Quantity;

                    if (bonusProduct != null)
                    {
                        var SplitGlDepositCashForBonus = ConfigurationManager.AppSettings["DepositCash"];
                        if (!string.IsNullOrEmpty(SplitGlDepositCashForBonus))
                        {
                            var cashvalueGl = glCode.GlCode.GlCodeName;
                            if (SplitGlDepositCashForBonus.Contains(cashvalueGl))
                            {
                                if (bonusProduct != null && glCode.GlCode.GlCodeName == cashvalueGl) //DEPOSITS CASH VALUE BOARD
                                {
                                    Amount = Convert.ToDecimal(splitGls.Where(x => x.GlCode.GlCodeName == cashvalueGl).FirstOrDefault().Amount + splitGls.Where(x => x.GlCode.GlCodeName == "64701000").FirstOrDefault().Amount) * quantity;
                                    UnitPrice = Convert.ToDecimal(splitGls.Where(x => x.GlCode.GlCodeName == cashvalueGl).FirstOrDefault().Amount + splitGls.Where(x => x.GlCode.GlCodeName == "64701000").FirstOrDefault().Amount);
                                }
                            }
                        }

                        if (bonusProduct != null && glCode.GlCode.GlCodeName == "40116050")//MEAL PLAN DEPOSITS-OFF-CAMPUS
                        {
                            Amount = decimal.Round(Convert.ToDecimal(splitGls.Where(x => x.GlCode.GlCodeName == "40116050").FirstOrDefault().Amount) * quantity, 4);
                            UnitPrice = Convert.ToDecimal(splitGls.Where(x => x.GlCode.GlCodeName == "40116050").FirstOrDefault().Amount);
                        }
                        if (bonusProduct != null && glCode.GlCode.GlCodeName == "64701000")//Declining Balance Bonus
                        {
                            Amount = decimal.Round(Convert.ToDecimal(-splitGls.Where(x => x.GlCode.GlCodeName == "64701000").FirstOrDefault().Amount) * quantity, 4);
                            UnitPrice = Convert.ToDecimal(-splitGls.Where(x => x.GlCode.GlCodeName == "64701000").FirstOrDefault().Amount);
                        }
                        else if (bonusProduct != null && categoryService.GetTaxCategoryById(glCode.TaxCategoryId).Code == "HCHG")
                        {
                            Amount = decimal.Round(Convert.ToDecimal(splitGls.Where(x => x.TaxCategoryId == glCode.TaxCategoryId).FirstOrDefault().Amount) * quantity, 4);
                            UnitPrice = Convert.ToDecimal(splitGls.Where(x => x.TaxCategoryId == glCode.TaxCategoryId).FirstOrDefault().Amount);
                        }
                    }

                    if (bonusProduct == null)
                    {
                        Amount = decimal.Round(glCode.Amount * quantity, 4);
                        UnitPrice = glCode.Amount;
                    }
                    var tc = categoryService.GetTaxCategoryById(glCode.TaxCategoryId);

                    int maxLength = Math.Min(productName.Length, 38);
                    if(scbpi !=null)
                    xmlBuilder.AppendLine("<urn:LineItem lineItemNumber=" + "\"" + i + "\" lineItemId=" + "\"" + scbpi.AssociatedProductId.ToString() + "-" + scbpi.AssociatedProductTaxCategoryId + "\">");
                    else
                        xmlBuilder.AppendLine("<urn:LineItem lineItemNumber=" + "\"" + i + "\" lineItemId=" + "\"" +  orderItem.Product.Id + "-" + glCode.Id + "\">");

                    xmlBuilder.AppendLine("<urn:Product productClass=" + "\"" + tc.Code + "\">" + productName.Substring(0, maxLength) + "</urn:Product>");
                    xmlBuilder.AppendLine("<urn:Quantity>" + quantity + "</urn:Quantity>");
                    xmlBuilder.AppendLine("<urn:UnitPrice>" + decimal.Round(UnitPrice, 4) + "</urn:UnitPrice>");
                    xmlBuilder.AppendLine("<urn:ExtendedPrice>" + Amount + "</urn:ExtendedPrice>");
                    tax = GetTaxFromStoredList(scbpi != null ? scbpi.AssociatedProductId : orderItem.ProductId, HttpContext.Current.Session.SessionID, taxService, tc.Code, glCode.Id);

                    xmlBuilder.AppendLine("<urn:InputTotalTax>" + tax + "</urn:InputTotalTax>"); // Take from Quotation Tax

                    xmlBuilder.AppendLine("</urn:LineItem>");
                    i++;
                }
            }
            else //not split
            {
                int taxCategoryId = scbpi != null ? scbpi.AssociatedProductTaxCategoryId : orderItem.Product.TaxCategoryId;
                int productId = scbpi != null ? scbpi.AssociatedProductId : orderItem.Product.Id;
                var tc = categoryService.GetTaxCategoryById(taxCategoryId);
                int i = 1;
                int quantity = 0;
                decimal unitPrice = 0;
                decimal extendedPrice = 0;
                if (scbpi != null)
                {
                    quantity = scbpi.Quantity;
                    unitPrice = decimal.Round(scbpi.Price, 4);
                    extendedPrice = decimal.Round(scbpi.Quantity * scbpi.Price, 4);
                }
                else
                {
                    quantity = orderItem.Quantity;
                    unitPrice = decimal.Round(orderItem.UnitPriceExclTax);
                    extendedPrice = decimal.Round(orderItem.Quantity * orderItem.UnitPriceExclTax, 4);
                }


                xmlBuilder.AppendLine("<urn:LineItem lineItemNumber=" + "\"" + i + "\" lineItemId=" + "\"" + productId + "\">");
                int maxLength = Math.Min(productName.Length, 38);
                xmlBuilder.AppendLine("<urn:Product productClass=" + "\"" + tc.Code + "\">" + productName.Substring(0, maxLength) + "</urn:Product>");
                xmlBuilder.AppendLine("<urn:Quantity>" + quantity + "</urn:Quantity>");
                xmlBuilder.AppendLine("<urn:UnitPrice>" + unitPrice + "</urn:UnitPrice>");
                xmlBuilder.AppendLine("<urn:ExtendedPrice>" + extendedPrice + "</urn:ExtendedPrice>");



                xmlBuilder.AppendLine("<urn:Customer>");
                xmlBuilder.AppendLine("<urn:CustomerCode>" + orderItem.Order.CustomerId + "</urn:CustomerCode>");
                xmlBuilder.AppendLine("<urn:Destination>");

                xmlBuilder.AppendLine("<urn:StreetAddress1>" + shipToAddress.Address1 + "</urn:StreetAddress1>");

                if (!String.IsNullOrEmpty(shipToAddress.Address2))
                {
                    xmlBuilder.AppendLine("<urn:StreetAddress2>" + shipToAddress.Address2 + "</urn:StreetAddress2>");
                }
                xmlBuilder.AppendLine("<urn:City>" + shipToAddress.City + "</urn:City>");
                xmlBuilder.AppendLine("<urn:MainDivision>" + shipToAddress.StateProvince.Abbreviation + "</urn:MainDivision>");
                xmlBuilder.AppendLine("<urn:PostalCode>" + shipToAddress.ZipPostalCode + "</urn:PostalCode>");
                xmlBuilder.AppendLine("<urn:Country>" + "US" + "</urn:Country>");
                xmlBuilder.AppendLine("</urn:Destination>");

                xmlBuilder.AppendLine("</urn:Customer>");

                var _productService = EngineContext.Current.Resolve<IProductService>();
                var appliedDiscounts = scbpi != null ? _productService.GetProductById(scbpi.AssociatedProductId).AppliedDiscounts : orderItem.Product.AppliedDiscounts;
                if (itemProducts.Contains(productId) || appliedDiscounts.Any())
                {
                    _taxservices.GetProductPrice(scbpi != null ? _productService.GetProductById(scbpi.AssociatedProductId) : orderItem.Product,
                                                extendedPrice,
                                                true, details.Customer, true, out taxRate);
                    tax = taxRate;
                }
                else
                {
                    tax = GetTaxFromStoredList(productId, HttpContext.Current.Session.SessionID, taxService, tc.Code);
                }
                xmlBuilder.AppendLine("<urn:InputTotalTax>" + tax + "</urn:InputTotalTax>"); //Take from Quotation Tax
                xmlBuilder.AppendLine("</urn:LineItem>");
                i++;

                if (shippingRate != 0)
                {
                    if (orderItem.SelectedShippingMethodName.Contains("UPS"))
                    {
                        decimal shipTax = GetTaxFromStoredList(productId, HttpContext.Current.Session.SessionID, taxService, "SHIP", 62);

                        xmlBuilder.AppendLine("<urn:LineItem lineItemNumber=\"" + i + "\" lineItemId=" + "\"" + productId + "-" + "62" + "\">");
                        xmlBuilder.AppendLine("<urn:Product productClass=" + "\"" + "SHIP" + "\">" + productName.Substring(0, maxLength) + "</urn:Product>");
                        xmlBuilder.AppendLine("<urn:ExtendedPrice>" + shippingRate + "</urn:ExtendedPrice>");
                        xmlBuilder.AppendLine("<urn:UnitPrice>" + shippingRate + "</urn:UnitPrice>");
                        xmlBuilder.AppendLine("<urn:InputTotalTax>" + shipTax + "</urn:InputTotalTax>"); // Take from Quotation Tax
                        xmlBuilder.AppendLine("</urn:LineItem>");
                    }
                    else if (orderItem.IsShipping && orderItem.SelectedShippingRateComputationMethodSystemName == "Tiered Shipping")
                    {
                        decimal shipTax = GetTaxFromStoredList(productId, HttpContext.Current.Session.SessionID, taxService, "SHIP", 62);

                        xmlBuilder.AppendLine("<urn:LineItem lineItemNumber=\"" + i + "\" lineItemId=" + "\"" + productId + "-" + "62" + "\">");
                        xmlBuilder.AppendLine("<urn:Product productClass=" + "\"" + "SHIP" + "\">" + productName.Substring(0, maxLength) + "</urn:Product>");
                        xmlBuilder.AppendLine("<urn:ExtendedPrice>" + shippingRate + "</urn:ExtendedPrice>");
                        xmlBuilder.AppendLine("<urn:UnitPrice>" + shippingRate + "</urn:UnitPrice>");
                        xmlBuilder.AppendLine("<urn:InputTotalTax>" + shipTax + "</urn:InputTotalTax>");
                        xmlBuilder.AppendLine("</urn:LineItem>");
                    }
                    else//should only be on campus delivery
                    {
                        decimal shipTax = GetTaxFromStoredList(productId, HttpContext.Current.Session.SessionID, taxService, "DELV", 61);

                        xmlBuilder.AppendLine("<urn:LineItem lineItemNumber=\"" + i + "\" lineItemId=" + "\"" + productId + "-" + "61" + "\">");
                        xmlBuilder.AppendLine("<urn:Product productClass=" + "\"" + "DELV" + "\">" + productName.Substring(0, maxLength) + "</urn:Product>");
                        xmlBuilder.AppendLine("<urn:ExtendedPrice>" + shippingRate + "</urn:ExtendedPrice>");
                        xmlBuilder.AppendLine("<urn:UnitPrice>" + shippingRate + "</urn:UnitPrice>");
                        xmlBuilder.AppendLine("<urn:InputTotalTax>" + shipTax + "</urn:InputTotalTax>"); // Take from Quotation Tax
                        xmlBuilder.AppendLine("</urn:LineItem>");
                    }
                    i++;
                }
            }
            //lineitems
            xmlBuilder.AppendLine("</urn:DistributeTaxRequest>");
            xmlBuilder.AppendLine("</urn:VertexEnvelope>");
            xmlBuilder.AppendLine("</soapenv:Body>");
            xmlBuilder.AppendLine("</soapenv:Envelope>");
            xmlRequest.LoadXml(xmlBuilder.ToString());

            return xmlRequest;
        }



        public static XmlDocument BuildDistributeTaxRefundPostDataXML(Order order, OrderItem items, Store store, IGlCodeService glCodeService, string productToRefund = null, bool isPartialRefund = false, List<int> commonitems = null, decimal AmountToRefund1 = 0, decimal AmountToRefund2 = 0, decimal AmountToRefund3 = 0, bool isSplit = false, List<int> data = null)
        {
            string response = string.Empty;
            if (!isPartialRefund)
            {
                if (commonitems.Contains(items.ProductId))
                {
                    response = glCodeService.GetVertexXML(order.Id, productToRefund, TaxRequestType.RequestDistributiveTax, items.Id);
                }
                else
                {
                    response = glCodeService.GetVertexXML(order.Id, productToRefund, TaxRequestType.RequestDistributiveTax);
                }
            }
            else
            {
                if (commonitems.Contains(items.ProductId))
                {
                    response = glCodeService.GetVertexXML(order.Id, productToRefund, TaxRequestType.RequestDistributiveTax, items.Id);
                }
                else
                {
                    response = glCodeService.GetVertexXML(order.Id, productToRefund, TaxRequestType.RequestDistributiveTax);
                }
            }
            var gls = glCodeService.GetProductGlCodes(items.Product);
            XmlDocument postData = new XmlDocument();
            if (!string.IsNullOrEmpty(response))
            {
                postData.LoadXml(response);

                //var node = postData.SelectSingleNode("urn");
                //node.ParentNode.RemoveAll();

                XmlNamespaceManager xmlNS = new XmlNamespaceManager(postData.NameTable);
                xmlNS.AddNamespace("soapenv", "http://schemas.xmlsoap.org/soap/envelope/");
                xmlNS.AddNamespace("urn", "urn:vertexinc:o-series:tps:8:0");

                if (data != null)
                {
                    if (!data.Contains(1))
                    {
                        XmlNode t = postData.SelectSingleNode("//urn:LineItem[@lineItemNumber='1']", xmlNS);
                        if (t != null)
                            t.ParentNode.RemoveChild(t);
                    }
                    if (!data.Contains(2))
                    {
                        XmlNode t = postData.SelectSingleNode("//urn:LineItem[@lineItemNumber='2']", xmlNS);
                        if (t != null)
                            t.ParentNode.RemoveChild(t);
                    }
                    if (!data.Contains(3))
                    {
                        XmlNode t = postData.SelectSingleNode("//urn:LineItem[@lineItemNumber='3']", xmlNS);
                        if (t != null)
                            t.ParentNode.RemoveChild(t);
                    }
                }

                XmlNodeList list = postData.DocumentElement.SelectNodes("//urn:LineItem ", xmlNS);


                if (!isPartialRefund)
                {
                    foreach (XmlNode xmlnode in list)
                    {
                        //foreach (XmlNode childnode in xmlnode.ChildNodes)
                        //{
                        xmlnode["urn:ExtendedPrice"].InnerText = "-" + xmlnode["urn:ExtendedPrice"].InnerText;
                        xmlnode["urn:InputTotalTax"].InnerText = "-" + xmlnode["urn:InputTotalTax"].InnerText;
                        //}

                    }
                }
                else
                {
                    if (isSplit)
                    {

                        foreach (XmlNode xmlnode in list)
                        {
                            var xmlnodedata = xmlnode.InnerXml;

                            if (xmlnodedata.Contains("SHIP") || xmlnodedata.Contains("DELV"))
                            {
                                xmlnode.ParentNode.RemoveChild(xmlnode);
                                continue;
                            }

                            if (data.Contains(1))
                            {
                                if (xmlnode.OuterXml.Contains("lineItemNumber=\"1\""))
                                {
                                    var ExtendedPrice = xmlnode["urn:ExtendedPrice"].InnerText;
                                    var AmountRefunded = AmountToRefund1;
                                    var TaxCalculated = xmlnode["urn:InputTotalTax"].InnerText;

                                    var TaxToReturn = (Convert.ToDecimal(TaxCalculated) / Convert.ToDecimal(ExtendedPrice)) * Convert.ToDecimal(AmountToRefund1);
                                    TaxToReturn = System.Math.Round(TaxToReturn, 2);

                                    xmlnode["urn:ExtendedPrice"].InnerText = "-" + AmountToRefund1;

                                    xmlnode["urn:InputTotalTax"].InnerText = "-" + TaxToReturn;
                                    data.Remove(1);
                                }

                            }
                            if (data.Contains(2))
                            {

                                if (xmlnode.OuterXml.Contains("lineItemNumber=\"2\""))
                                {
                                    var ExtendedPrice = xmlnode["urn:ExtendedPrice"].InnerText;
                                    var AmountRefunded = AmountToRefund1;
                                    var TaxCalculated = xmlnode["urn:InputTotalTax"].InnerText;

                                    var TaxToReturn = (Convert.ToDecimal(TaxCalculated) / Convert.ToDecimal(ExtendedPrice)) * Convert.ToDecimal(AmountToRefund2);
                                    TaxToReturn = System.Math.Round(TaxToReturn, 2);

                                    xmlnode["urn:ExtendedPrice"].InnerText = "-" + AmountToRefund2;

                                    xmlnode["urn:InputTotalTax"].InnerText = "-" + TaxToReturn;
                                    data.Remove(2);
                                }

                            }
                            if (data.Contains(3))
                            {

                                if (xmlnode.OuterXml.Contains("lineItemNumber=\"3\""))
                                {
                                    var ExtendedPrice = xmlnode["urn:ExtendedPrice"].InnerText;
                                    var AmountRefunded = AmountToRefund1;
                                    var TaxCalculated = xmlnode["urn:InputTotalTax"].InnerText;

                                    var TaxToReturn = (Convert.ToDecimal(TaxCalculated) / Convert.ToDecimal(ExtendedPrice)) * Convert.ToDecimal(AmountToRefund3);
                                    TaxToReturn = System.Math.Round(TaxToReturn, 2);

                                    xmlnode["urn:ExtendedPrice"].InnerText = "-" + AmountToRefund3;

                                    xmlnode["urn:InputTotalTax"].InnerText = "-" + TaxToReturn;
                                    data.Remove(3);
                                }

                            }
                        }
                    }
                    else
                    {
                        foreach (XmlNode xmlnode in list)
                        {
                            var xmlnodedata = xmlnode.InnerXml;

                            if (xmlnodedata.Contains("SHIP") || xmlnodedata.Contains("DELV"))
                            {
                                xmlnode.ParentNode.RemoveChild(xmlnode);
                                continue;
                            }

                            var ExtendedPrice = xmlnode["urn:ExtendedPrice"].InnerText;
                            var AmountRefunded = AmountToRefund1;
                            var TaxCalculated = xmlnode["urn:InputTotalTax"].InnerText;

                            var TaxToReturn = (Convert.ToDecimal(TaxCalculated) / Convert.ToDecimal(ExtendedPrice)) * Convert.ToDecimal(AmountToRefund1);
                            TaxToReturn = System.Math.Round(TaxToReturn, 2);

                            xmlnode["urn:ExtendedPrice"].InnerText = "-" + AmountToRefund1;

                            xmlnode["urn:InputTotalTax"].InnerText = "-" + TaxToReturn;

                        }
                    }
                }
                return postData;
            }
            else
            { return postData; }
        }


        public static XmlDocument BuildDistributeFullTaxRefundPostDataPerItemXML(Order order, OrderItem item, Store store, IGlCodeService glCodeService, string productToRefund = null, bool isPartialRefund = false, List<int> commonitems = null, decimal AmountToRefund1 = 0, decimal AmountToRefund2 = 0, decimal AmountToRefund3 = 0, bool isSplit = false, decimal taxAmoun1 = 0, decimal taxAmount2 = 0, decimal taxAmount3 = 0, decimal DeliveryAmount = 0, decimal DeliveryTax = 0, decimal ShippingAmount = 0, decimal ShippingTax = 0)
        {
            string response = string.Empty;
            if (commonitems.Contains(item.ProductId))
            {
                response = glCodeService.GetVertexXML(order.Id, productToRefund, TaxRequestType.RequestDistributiveTax, item.Id);
            }
            else
            {
                response = glCodeService.GetVertexXML(order.Id, productToRefund, TaxRequestType.RequestDistributiveTax);
            }
            //string response = glCodeService.GetVertexXML(order.Id, productToRefund, TaxRequestType.RequestDistributiveTax);

            XmlDocument postData = new XmlDocument();
            if (!string.IsNullOrEmpty(response))
            {
                postData.LoadXml(response);

                XmlNamespaceManager xmlNS = new XmlNamespaceManager(postData.NameTable);
                xmlNS.AddNamespace("soapenv", "http://schemas.xmlsoap.org/soap/envelope/");
                xmlNS.AddNamespace("urn", "urn:vertexinc:o-series:tps:8:0");


                XmlNodeList list = postData.DocumentElement.SelectNodes("//urn:LineItem", xmlNS);
                if (isSplit)
                {
                    foreach (XmlNode xmlnode in list)
                    {
                        var xmlnodedata = xmlnode.InnerXml;
                        if (xmlnodedata.Contains("SHIP") || xmlnodedata.Contains("DELV"))
                        {
                            xmlnode.ParentNode.RemoveChild(xmlnode);
                            continue;
                        }
                        if (xmlnode.OuterXml.Contains("lineItemNumber=\"1\""))
                        {
                            xmlnode["urn:ExtendedPrice"].InnerText = "-" + AmountToRefund1;
                            xmlnode["urn:InputTotalTax"].InnerText = "-" + taxAmoun1;
                        }

                        if (xmlnode.OuterXml.Contains("lineItemNumber=\"2\""))
                        {
                            if (AmountToRefund2 < decimal.Zero)
                                xmlnode["urn:ExtendedPrice"].InnerText = Convert.ToString(-AmountToRefund2);
                            else
                                xmlnode["urn:ExtendedPrice"].InnerText = "-" + AmountToRefund2;
                            xmlnode["urn:InputTotalTax"].InnerText = "-" + taxAmount2;
                        }
                        if (xmlnode.OuterXml.Contains("lineItemNumber=\"3\""))
                        {
                            if (AmountToRefund3 < decimal.Zero)
                                xmlnode["urn:ExtendedPrice"].InnerText = Convert.ToString(-AmountToRefund3);
                            else
                                xmlnode["urn:ExtendedPrice"].InnerText = "-" + AmountToRefund3;
                            xmlnode["urn:InputTotalTax"].InnerText = "-" + taxAmount3;
                        }


                    }
                }
                else
                {
                    foreach (XmlNode xmlnode in list)
                    {
                        var xmlnodedata = xmlnode.InnerXml;

                        //COMMENTED SO IN ORDER TO ALLOW TO REFUND DELIVERY AND SHIPPING FEE IN CASE OF FULL REFUND
                        //if (xmlnodedata.Contains("SHIP") || xmlnodedata.Contains("DELV"))
                        //{
                        //    xmlnode.ParentNode.RemoveChild(xmlnode);
                        //    continue;
                        //}
                        if (DeliveryAmount != 0 && xmlnodedata.Contains("DELV"))
                        {
                            xmlnode["urn:ExtendedPrice"].InnerText = "-" + DeliveryAmount;
                            xmlnode["urn:InputTotalTax"].InnerText = "-" + DeliveryTax;

                        }
                        if (ShippingAmount != 0 && xmlnodedata.Contains("SHIP"))
                        {
                            xmlnode["urn:ExtendedPrice"].InnerText = "-" + ShippingAmount;
                            xmlnode["urn:InputTotalTax"].InnerText = "-" + ShippingTax;

                        }
                        if (!xmlnodedata.Contains("DELV") && !xmlnodedata.Contains("SHIP"))
                        {
                            xmlnode["urn:ExtendedPrice"].InnerText = "-" + AmountToRefund1;
                            xmlnode["urn:InputTotalTax"].InnerText = "-" + (taxAmoun1 + taxAmount2 + taxAmount3);

                        }


                    }
                }
                return postData;
            }
            else
            { return postData; }
        }


        public static decimal GetTaxFromStoredList(int productId, string sessionId, IVertexTaxRateService taxService, string taxCode, int glcodeId = 0)
        {
            var productTaxes = taxService.GetAllProductTax(sessionId, productId);


            foreach (var pt in productTaxes)
            {
                if (glcodeId != 0)
                {
                    if (productId == pt.ProductId && pt.SessionId == sessionId && pt.GlCodeId == glcodeId && pt.TaxCode == taxCode)
                    {
                        return pt.Tax;
                    }
                }
                else
                {
                    if (productId == pt.ProductId && pt.SessionId == sessionId && pt.TaxCode == taxCode)
                    {
                        return pt.Tax;
                    }
                }
            }
            return 0;
        }

        public static decimal GetTaxfromQuotationTaxXML(string result, IVertexTaxRateService taxService, out Dictionary<string, decimal> outTax)
        {
            decimal tax = 0;
            int productID = 0;
            decimal totaltax = 0;
            string taxCategoryCode = "";
            outTax = new Dictionary<string, decimal>();

            XmlDocument xmlDoc = new XmlDocument();

            xmlDoc.LoadXml(result);

            XmlNodeList nodeitem = xmlDoc.GetElementsByTagName("QuotationResponse");

            foreach (XmlNode xmlnode in nodeitem)
            {
                foreach (XmlNode childnode in xmlnode.ChildNodes)
                {
                    if (childnode.Name == "LineItem")
                    {
                        int glId = 0;
                        string[] prod = childnode.Attributes["lineItemId"].Value.Split('-');


                        productID = Convert.ToInt32(prod[0]); //need to split GL

                        if (prod.Length > 1)
                        {

                            //  if (prod[1] != "DELV")
                            // {
                            glId = Convert.ToInt32(prod[1]); //grab glid
                            // }

                        }
                        taxCategoryCode = childnode.FirstChild.Attributes["productClass"].Value;

                        tax = Convert.ToDecimal(childnode["TotalTax"].InnerText);
                        totaltax = totaltax + tax;
                        var productTax = taxService.CreateOrUpdateProductTax(productID, taxCategoryCode, System.Web.HttpContext.Current.Session.SessionID, tax, glId);

                        if (!outTax.ContainsKey(productTax.TaxCode))
                        {
                            outTax.Add(productTax.TaxCode, productTax.Tax);
                        }
                    }
                }

            }
            return totaltax;
        }

        public static void updateTaxForTieredShipping(int productId, string sessionId, string taxCode)
        {
            string currentSessionId = HttpContext.Current.Session["VertexCurrentSessionId"] != null ? HttpContext.Current.Session["VertexCurrentSessionId"].ToString() : null;
            var taxService = EngineContext.Current.Resolve<IVertexTaxRateService>();
            ProductTax taxItem = taxService.GetTaxRateBySessionIdProductIdTaxCode(productId, taxCode, currentSessionId ?? sessionId);
            if (taxItem != null && taxItem.Tax > 0)
            {
                taxItem.Tax = 0;
                taxService.UpdateTaxRate(taxItem);
            }
        }
        #endregion


    }
}
