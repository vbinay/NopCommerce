using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Nop.Admin.Extensions;
using Nop.Admin.Models.Directory;
using Nop.Admin.Models.Shipping;
using Nop.Core;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Shipping;
using Nop.Core.Plugins;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Services.Shipping;
using Nop.Services.Stores;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using Nop.Web.Framework.Mvc;
using Nop.Admin.Models.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Services.Vendors;
using Nop.Core.Domain.Vendors;
using Nop.Core.Domain.Stores;
using Nop.Core.Data;

//START: Codechages done by(na-sdxcorp\ADas)
using System.Configuration;
using System.Net;
using System.Text;
using System.Xml;
using System.IO;
using Nop.Data;
using Nop.Services.Payments;
//END: Codechages done by (na-sdxcorp\ADas)

namespace Nop.Admin.Controllers
{
    public partial class ShippingController : BaseAdminController
    {
        #region Fields

        private readonly IShippingService _shippingService;
        private readonly ShippingSettings _shippingSettings;
        private readonly ISettingService _settingService;
        private readonly IAddressService _addressService;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly ILanguageService _languageService;
        private readonly IPluginFinder _pluginFinder;
        private readonly IWebHelper _webHelper;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;
        private readonly IStoreService _storeService;
        private readonly IVendorService _vendorService;
        private readonly IVendorMappingService _vendorMappingService;
        private readonly IRepository<VendorMapping> _vendorMappingRepository;
        private readonly IRepository<StoreMapping> _storeMappingRepository;

        #endregion

        #region Constructors

        public ShippingController(IShippingService shippingService,
            ShippingSettings shippingSettings,
            ISettingService settingService,
            IAddressService addressService,
            ICountryService countryService,
            IStateProvinceService stateProvinceService,
            ILocalizationService localizationService,
            IPermissionService permissionService,
             ILocalizedEntityService localizedEntityService,
            ILanguageService languageService,
            IPluginFinder pluginFinder,
            IWebHelper webHelper,
            IStoreMappingService storeMappingService,
            IStoreContext storeContext,
            IWorkContext workContext,
            IStoreService storeService,
            IVendorService vendorService,
            IVendorMappingService vendorMappingService,
            IRepository<VendorMapping> vendorMappingRepository,
            IRepository<StoreMapping> storeMappingRepository
            )
        {
            this._shippingService = shippingService;
            this._shippingSettings = shippingSettings;
            this._settingService = settingService;
            this._addressService = addressService;
            this._countryService = countryService;
            this._stateProvinceService = stateProvinceService;
            this._localizationService = localizationService;
            this._permissionService = permissionService;
            this._localizedEntityService = localizedEntityService;
            this._languageService = languageService;
            this._pluginFinder = pluginFinder;
            this._webHelper = webHelper;
            this._storeMappingService = storeMappingService;
            this._storeContext = storeContext;
            this._workContext = workContext;
            this._storeService = storeService;
            this._vendorService = vendorService;
            this._vendorMappingService = vendorMappingService;
            this._vendorMappingRepository = vendorMappingRepository;
            this._storeMappingRepository = storeMappingRepository;
        }

        #endregion

        #region Utilities

        [NonAction]
        protected virtual void UpdateLocales(ShippingMethod shippingMethod, ShippingMethodModel model)
        {
            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedValue(shippingMethod,
                                                               x => x.Name,
                                                               localized.Name,
                                                               localized.LanguageId);

                _localizedEntityService.SaveLocalizedValue(shippingMethod,
                                                           x => x.Description,
                                                           localized.Description,
                                                           localized.LanguageId);
            }
        }

        [NonAction]
        protected virtual void UpdateLocales(DeliveryDate deliveryDate, DeliveryDateModel model)
        {
            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedValue(deliveryDate,
                                                               x => x.Name,
                                                               localized.Name,
                                                               localized.LanguageId);
            }
        }

        #endregion

        #region Shipping rate computation methods

        public ActionResult Providers()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageStores))
                return AccessDeniedView();

            return View();
        }

        [HttpPost]
        public ActionResult Providers(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageStores))
                return AccessDeniedView();

            var shippingProvidersModel = new List<ShippingRateComputationMethodModel>();
            var shippingProviders = _shippingService.LoadAllShippingRateComputationMethods();
            foreach (var shippingProvider in shippingProviders)
            {
                var tmp1 = shippingProvider.ToModel();
                tmp1.IsActive = shippingProvider.IsShippingRateComputationMethodActive(_shippingSettings);
                tmp1.LogoUrl = shippingProvider.PluginDescriptor.GetLogoUrl(_webHelper);
                shippingProvidersModel.Add(tmp1);
            }
            shippingProvidersModel = shippingProvidersModel.ToList();
            var gridModel = new DataSourceResult
            {
                Data = shippingProvidersModel.
                    OrderBy(s => s.FriendlyName), /// NU-24
                Total = shippingProvidersModel.Count()
            };

            return Json(gridModel);
        }

        [HttpPost]
        public ActionResult ProviderUpdate([Bind(Exclude = "ConfigurationRouteValues")] ShippingRateComputationMethodModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageStores))
                return AccessDeniedView();

            var srcm = _shippingService.LoadShippingRateComputationMethodBySystemName(model.SystemName);
            if (srcm.IsShippingRateComputationMethodActive(_shippingSettings))
            {
                if (!model.IsActive)
                {
                    //mark as disabled
                    _shippingSettings.ActiveShippingRateComputationMethodSystemNames.Remove(srcm.PluginDescriptor.SystemName);
                    _settingService.SaveSetting(_shippingSettings);
                }
            }
            else
            {
                if (model.IsActive)
                {
                    //mark as active
                    _shippingSettings.ActiveShippingRateComputationMethodSystemNames.Add(srcm.PluginDescriptor.SystemName);
                    _settingService.SaveSetting(_shippingSettings);
                }
            }
            var pluginDescriptor = srcm.PluginDescriptor;
            //display order
            pluginDescriptor.DisplayOrder = model.DisplayOrder;
            PluginFileParser.SavePluginDescriptionFile(pluginDescriptor);
            //reset plugin cache
            _pluginFinder.ReloadPlugins();

            return new NullJsonResult();
        }

        public ActionResult ConfigureProvider(string systemName)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageStores))
                return AccessDeniedView();

            var srcm = _shippingService.LoadShippingRateComputationMethodBySystemName(systemName);
            if (srcm == null)
                //No shipping rate computation method found with the specified id
                return RedirectToAction("Providers");

            var model = srcm.ToModel();
            string actionName, controllerName;
            RouteValueDictionary routeValues;
            srcm.GetConfigurationRoute(out actionName, out controllerName, out routeValues);
            model.ConfigurationActionName = actionName;
            model.ConfigurationControllerName = controllerName;
            model.ConfigurationRouteValues = routeValues;
            return View(model);
        }

        #endregion

        #region Pickup point providers

        public ActionResult PickupPointProviders()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageStores))
                return AccessDeniedView();

            return View();
        }

        [HttpPost]
        public ActionResult PickupPointProviders(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageStores))
                return AccessDeniedView();

            var pickupPointProviderModel = new List<PickupPointProviderModel>();
            var allProviders = _shippingService.LoadAllPickupPointProviders();
            foreach (var provider in allProviders)
            {
                var model = provider.ToModel();
                model.IsActive = provider.IsPickupPointProviderActive(_shippingSettings);
                model.LogoUrl = provider.PluginDescriptor.GetLogoUrl(_webHelper);
                pickupPointProviderModel.Add(model);
            }

            var gridModel = new DataSourceResult
            {
                Data = pickupPointProviderModel,
                Total = pickupPointProviderModel.Count
            };

            return Json(gridModel);
        }

        [HttpPost]
        public ActionResult PickupPointProviderUpdate([Bind(Exclude = "ConfigurationRouteValues")] PickupPointProviderModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageStores))
                return AccessDeniedView();

            var pickupPointProvider = _shippingService.LoadPickupPointProviderBySystemName(model.SystemName);
            if (pickupPointProvider.IsPickupPointProviderActive(_shippingSettings))
            {
                if (!model.IsActive)
                {
                    //mark as disabled
                    _shippingSettings.ActivePickupPointProviderSystemNames.Remove(pickupPointProvider.PluginDescriptor.SystemName);
                    _settingService.SaveSetting(_shippingSettings);
                }
            }
            else
            {
                if (model.IsActive)
                {
                    //mark as active
                    _shippingSettings.ActivePickupPointProviderSystemNames.Add(pickupPointProvider.PluginDescriptor.SystemName);
                    _settingService.SaveSetting(_shippingSettings);
                }
            }
            var pluginDescriptor = pickupPointProvider.PluginDescriptor;
            pluginDescriptor.DisplayOrder = model.DisplayOrder;
            PluginFileParser.SavePluginDescriptionFile(pluginDescriptor);
            //reset plugin cache
            _pluginFinder.ReloadPlugins();

            return new NullJsonResult();
        }

        public ActionResult ConfigurePickupPointProvider(string systemName)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageStores))
                return AccessDeniedView();

            var pickupPointProvider = _shippingService.LoadPickupPointProviderBySystemName(systemName);
            if (pickupPointProvider == null)
                return RedirectToAction("PickupPointProviders");

            var model = pickupPointProvider.ToModel();
            string actionName;
            string controllerName;
            RouteValueDictionary routeValues;
            pickupPointProvider.GetConfigurationRoute(out actionName, out controllerName, out routeValues);
            model.ConfigurationActionName = actionName;
            model.ConfigurationControllerName = controllerName;
            model.ConfigurationRouteValues = routeValues;
            return View(model);
        }

        #endregion

        #region Shipping methods

        public ActionResult Methods()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageStores))
                return AccessDeniedView();

            return View();
        }

        [HttpPost]
        public ActionResult Methods(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageStores))
                return AccessDeniedView();

            var shippingMethodsModel = _shippingService.GetAllShippingMethods()
                .Select(x => x.ToModel())
                .ToList();
            var gridModel = new DataSourceResult
            {
                Data = shippingMethodsModel.
                    OrderBy(s => s.Name), /// NU-24
                Total = shippingMethodsModel.Count
            };

            return Json(gridModel);
        }


        public ActionResult CreateMethod()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageStores))
                return AccessDeniedView();

            var model = new ShippingMethodModel();
            //locales
            AddLocales(_languageService, model.Locales);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public ActionResult CreateMethod(ShippingMethodModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageStores))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var sm = model.ToEntity();
                _shippingService.InsertShippingMethod(sm);
                //locales
                UpdateLocales(sm, model);

                SuccessNotification(_localizationService.GetResource("Admin.Configuration.Shipping.Methods.Added"));
                return continueEditing ? RedirectToAction("EditMethod", new { id = sm.Id }) : RedirectToAction("Methods");
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }

        public ActionResult EditMethod(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageStores))
                return AccessDeniedView();

            var sm = _shippingService.GetShippingMethodById(id);
            if (sm == null)
                //No shipping method found with the specified id
                return RedirectToAction("Methods");

            var model = sm.ToModel();
            //locales
            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = sm.GetLocalized(x => x.Name, languageId, false, false);
                locale.Description = sm.GetLocalized(x => x.Description, languageId, false, false);
            });

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public ActionResult EditMethod(ShippingMethodModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageStores))
                return AccessDeniedView();

            var sm = _shippingService.GetShippingMethodById(model.Id);
            if (sm == null)
                //No shipping method found with the specified id
                return RedirectToAction("Methods");

            if (ModelState.IsValid)
            {
                sm = model.ToEntity(sm);
                _shippingService.UpdateShippingMethod(sm);
                //locales
                UpdateLocales(sm, model);
                SuccessNotification(_localizationService.GetResource("Admin.Configuration.Shipping.Methods.Updated"));
                return continueEditing ? RedirectToAction("EditMethod", sm.Id) : RedirectToAction("Methods");
            }


            //If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost]
        public ActionResult DeleteMethod(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageStores))
                return AccessDeniedView();

            var sm = _shippingService.GetShippingMethodById(id);
            if (sm == null)
                //No shipping method found with the specified id
                return RedirectToAction("Methods");

            _shippingService.DeleteShippingMethod(sm);

            SuccessNotification(_localizationService.GetResource("Admin.Configuration.Shipping.Methods.Deleted"));
            return RedirectToAction("Methods");
        }

        #endregion

        #region Delivery dates

        public ActionResult DeliveryDates()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageStores))
                return AccessDeniedView();

            return View();
        }

        [HttpPost]
        public ActionResult DeliveryDates(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageStores))
                return AccessDeniedView();

            var deliveryDatesModel = _shippingService.GetAllDeliveryDates()
                .Select(x => x.ToModel())
                .ToList();
            var gridModel = new DataSourceResult
            {
                Data = deliveryDatesModel,
                Total = deliveryDatesModel.Count
            };

            return Json(gridModel);
        }


        public ActionResult CreateDeliveryDate()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageStores))
                return AccessDeniedView();

            var model = new DeliveryDateModel();
            //locales
            AddLocales(_languageService, model.Locales);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public ActionResult CreateDeliveryDate(DeliveryDateModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageStores))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var deliveryDate = model.ToEntity();
                _shippingService.InsertDeliveryDate(deliveryDate);
                //locales
                UpdateLocales(deliveryDate, model);

                SuccessNotification(_localizationService.GetResource("Admin.Configuration.Shipping.DeliveryDates.Added"));
                return continueEditing ? RedirectToAction("EditDeliveryDate", new { id = deliveryDate.Id }) : RedirectToAction("DeliveryDates");
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }

        public ActionResult EditDeliveryDate(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageStores))
                return AccessDeniedView();

            var deliveryDate = _shippingService.GetDeliveryDateById(id);
            if (deliveryDate == null)
                //No delivery date found with the specified id
                return RedirectToAction("DeliveryDates");

            var model = deliveryDate.ToModel();
            //locales
            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = deliveryDate.GetLocalized(x => x.Name, languageId, false, false);
            });

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public ActionResult EditDeliveryDate(DeliveryDateModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageStores))
                return AccessDeniedView();

            var deliveryDate = _shippingService.GetDeliveryDateById(model.Id);
            if (deliveryDate == null)
                //No delivery date found with the specified id
                return RedirectToAction("DeliveryDates");

            if (ModelState.IsValid)
            {
                deliveryDate = model.ToEntity(deliveryDate);
                _shippingService.UpdateDeliveryDate(deliveryDate);
                //locales
                UpdateLocales(deliveryDate, model);
                SuccessNotification(_localizationService.GetResource("Admin.Configuration.Shipping.DeliveryDates.Updated"));
                return continueEditing ? RedirectToAction("EditDeliveryDate", deliveryDate.Id) : RedirectToAction("DeliveryDates");
            }


            //If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost]
        public ActionResult DeleteDeliveryDate(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageStores))
                return AccessDeniedView();

            var deliveryDate = _shippingService.GetDeliveryDateById(id);
            if (deliveryDate == null)
                //No delivery date found with the specified id
                return RedirectToAction("DeliveryDates");

            _shippingService.DeleteDeliveryDate(deliveryDate);

            SuccessNotification(_localizationService.GetResource("Admin.Configuration.Shipping.DeliveryDates.Deleted"));
            return RedirectToAction("DeliveryDates");
        }

        #endregion

        #region Warehouses

        public ActionResult Warehouses()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageStores))
                return AccessDeniedView();

            #region NU-77
            var model = new WarehouseListModel();

            if (_workContext.CurrentCustomer.IsSystemGlobalAdmin())
                model.AvailableStores.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            foreach (var s in _storeService.GetAllStores().OrderBy(s => s.Name))
                model.AvailableStores.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString() });
            model.SearchStoreId = _storeContext.CurrentStore.Id;

            if (_workContext.CurrentCustomer.IsSystemGlobalAdmin())
                model.AvailableVendors.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            foreach (var s in _vendorService.GetAllVendors().OrderBy(s => s.Name))
                model.AvailableVendors.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString() });
            model.SearchVendorId = _workContext.CurrentCustomer.VendorId;

            model.SearchByStore = true;

            model.IsLoggedAs = _workContext.CurrentCustomer.IsLoggedAs();
            if (model.IsLoggedAs == "STORE.ADMIN")
            {
                model.SearchVendorId = 0;
            }
            else if (model.IsLoggedAs == "VENDOR")
            {
                model.SearchStoreId = 0;
            }
            model.IsReadOnly = _workContext.CurrentCustomer.ReadOnly;
            return View(model);
            #endregion
        }

        [HttpPost]
        public ActionResult Warehouses(DataSourceRequest command,
            WarehouseListModel model)	/// NU-77
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageStores))
                return AccessDeniedView();

            var warehousesModel = _shippingService.GetAllWarehouses(
                storeId: model.SearchStoreId,	/// NU-77
                vendorId: model.SearchVendorId)	/// NU-77
                .Select(x =>
                {
                    var warehouseModel = new WarehouseModel
                    {
                        Id = x.Id,
                        Name = x.Name
                        //ignore address for list view (performance optimization)
                    };
                    return warehouseModel;
                })
                .ToList();

            var gridModel = new DataSourceResult
            {
                Data = warehousesModel.
                    OrderBy(s => s.Name), /// NU-24
                Total = warehousesModel.Count
            };

            return Json(gridModel);
        }


        //---START: Codechages done by (na-sdxcorp\ADas)--------------
        private List<UPSAddress> GetAddressValidatedWithUPS(string city, string stateCode, string zipPostalCode, string countryCode, string addressLine1)
        {
            List<UPSAddress> allMatchingUPSAddress = new List<UPSAddress>();

            //string requestXML = "<?xml version=\"1.0\" ?> <AccessRequest xml:lang='en-US'> <AccessLicenseNumber>YOURACCESSLICENSENUMBER</AccessLicenseNumber> <UserId>YOURUSERID</UserId> <Password>YOURPASSWORD</Password> </AccessRequest> <?xml version=\"1.0\" ?> <AddressValidationRequest xml:lang='en-US'> <Request> <TransactionReference> <CustomerContext>Your Customer Context</CustomerContext> </TransactionReference> <RequestAction>AV</RequestAction> </Request> <Address> <City>YOURCITY</City> <StateProvinceCode>YOURSTATE</StateProvinceCode> <PostalCode>YOURPOSTALCODE</PostalCode> <CountryCode>YOURCOUNTRYCODE</CountryCode> </Address> </AddressValidationRequest>";
            string requestXML = "<?xml version=\"1.0\" ?> <AccessRequest xml:lang='en-US'> <AccessLicenseNumber>YOURACCESSLICENSENUMBER</AccessLicenseNumber> <UserId>YOURUSERID</UserId> <Password>YOURPASSWORD</Password> </AccessRequest> <?xml version=\"1.0\" ?> <AddressValidationRequest xml:lang='en-US'> <Request> <TransactionReference> <CustomerContext>Your Customer Context</CustomerContext> </TransactionReference> <RequestAction>XAV</RequestAction> <RequestOption>3</RequestOption> </Request> <AddressKeyFormat> <AddressLine>YOURADDRESSLINE</AddressLine> <PoliticalDivision2>YOURCITY</PoliticalDivision2> <PoliticalDivision1>YOURSTATE</PoliticalDivision1> <PostcodePrimaryLow>YOURPOSTALCODE</PostcodePrimaryLow> <CountryCode>YOURCOUNTRYCODE</CountryCode> </AddressKeyFormat> </AddressValidationRequest>";
            requestXML = requestXML.Replace("YOURACCESSLICENSENUMBER", ConfigurationManager.AppSettings["ups_AccessLicenseNumber"].ToString());
            requestXML = requestXML.Replace("YOURUSERID", ConfigurationManager.AppSettings["ups_UserId"].ToString());
            requestXML = requestXML.Replace("YOURPASSWORD", ConfigurationManager.AppSettings["ups_Password"].ToString());
            requestXML = requestXML.Replace("YOURCITY", city);
            requestXML = requestXML.Replace("YOURSTATE", stateCode);
            requestXML = requestXML.Replace("YOURCOUNTRYCODE", countryCode);
            requestXML = requestXML.Replace("YOURPOSTALCODE", zipPostalCode);
            requestXML = requestXML.Replace("YOURADDRESSLINE", addressLine1);

            try
            {
                var outputStr = string.Empty;
                byte[] requestBytes = null;
                Uri uri = new Uri(ConfigurationManager.AppSettings["Addressvalidate"]);
                WebRequest request = WebRequest.Create(uri);
                requestBytes = Encoding.GetEncoding("iso-8859-1").GetBytes(requestXML);

                request.ContentLength = requestBytes.Length;
                request.Method = "POST";
                request.ContentType = "multipart/form-data";

                using (Stream requestStream = request.GetRequestStream())
                {
                    requestStream.Write(requestBytes, 0, requestBytes.Length);
                    requestStream.Close();
                }
                WebResponse theResponse = request.GetResponse();
                using (StreamReader reader = new StreamReader(theResponse.GetResponseStream()))
                {
                    outputStr = reader.ReadToEnd();

                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(outputStr);

                    string statusCode = doc.SelectSingleNode("AddressValidationResponse/Response/ResponseStatusCode").InnerText;
                    XmlNode validAddressNode = doc.SelectSingleNode("AddressValidationResponse/ValidAddressIndicator");
                    XmlNode ambiguousAddressNode = doc.SelectSingleNode("AddressValidationResponse/AmbiguousAddressIndicator");
                    XmlNode noCandidatesAddressNode = doc.SelectSingleNode("AddressValidationResponse/NoCandidatesIndicator");
                    XmlNodeList idNodes = doc.SelectNodes("AddressValidationResponse/AddressKeyFormat");

                    if (statusCode == "0")
                    {
                        allMatchingUPSAddress = null; //null indicates wrong address and no match found.
                        Session["UPSFailure"] = true;
                    }
                    else if (statusCode == "1" && noCandidatesAddressNode != null)
                    {
                        //UPS could not find any match
                        allMatchingUPSAddress = null;
                        Session["UPSFailure"] = false;

                    }
                    else if (statusCode == "1" && (ambiguousAddressNode != null || validAddressNode != null)) //list of matches found.
                    {
                        allMatchingUPSAddress = new List<UPSAddress>();
                        foreach (XmlNode node in idNodes)
                        {
                            UPSAddress matchingUPSAddress = new UPSAddress();
                            foreach (XmlNode childNode in node.ChildNodes)
                            {
                                if (childNode.Name.Equals("AddressLine"))
                                    matchingUPSAddress.AddressLine = childNode.InnerText;
                                else if (childNode.Name.Equals("PoliticalDivision2"))
                                    matchingUPSAddress.City = childNode.InnerText;
                                else if (childNode.Name.Equals("PoliticalDivision1"))
                                    matchingUPSAddress.StateProvinceCode = childNode.InnerText;
                                else if (childNode.Name.Equals("PostcodePrimaryLow"))
                                    matchingUPSAddress.PostalCode = childNode.InnerText;
                            }

                            allMatchingUPSAddress.Add(matchingUPSAddress);
                        }

                        if (allMatchingUPSAddress.Count == 1 && validAddressNode != null)
                        {
                            if (city.ToUpper().Equals(allMatchingUPSAddress[0].City.ToUpper())
                                && stateCode.ToUpper().Equals(allMatchingUPSAddress[0].StateProvinceCode.ToUpper())
                                && zipPostalCode.Equals(allMatchingUPSAddress[0].PostalCode)
                                && addressLine1.ToUpper().Equals(allMatchingUPSAddress[0].AddressLine.ToUpper()))
                            {
                                allMatchingUPSAddress = null;
                                allMatchingUPSAddress = new List<UPSAddress>(); //empty list indicates valid address
                            }
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                allMatchingUPSAddress = null;
                Session["UPSFailure"] = true;
            }

            return allMatchingUPSAddress;
        }

        public ActionResult SelectAddress(string address1, string city, string postalCode, string stateProvinceCode)
        {
            WarehouseModel model = Session["LastPostedModel"] as WarehouseModel;
            bool continueEditing = (bool)Session["continueEditing"];

            Session.Remove("LastPostedModel");
            Session.Remove("continueEditing");

            model.Address.AvailableCountries.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Address.SelectCountry"), Value = "0" });
            foreach (var c in _countryService.GetAllCountries(showHidden: true))
                model.Address.AvailableCountries.Add(new SelectListItem { Text = c.Name, Value = c.Id.ToString(), Selected = (c.Id == model.Address.CountryId) });
            //states
            var tmp_states = model.Address.CountryId.HasValue ? _stateProvinceService.GetStateProvincesByCountryId(model.Address.CountryId.Value, showHidden: true).ToList() : new List<StateProvince>();
            if (tmp_states.Count > 0)
            {
                foreach (var s in tmp_states)
                    model.Address.AvailableStates.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString(), Selected = (s.Id == model.Address.StateProvinceId) });
            }
            else
                model.Address.AvailableStates.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Address.OtherNonUS"), Value = "0" });

            ModelState.Remove("Address.City");
            ModelState.Remove("Address.ZipPostalCode");
            ModelState.Remove("Address.StateProvinceId");
            ModelState.Remove("Address.StateProvinceName");
            ModelState.Remove("Address.Address1");
            model.Address.City = city;
            model.Address.ZipPostalCode = postalCode;
            model.Address.StateProvinceId = _stateProvinceService.GetStateProvinceByAbbreviation(stateProvinceCode).Id;
            model.Address.StateProvinceName = _stateProvinceService.GetStateProvinceByAbbreviation(stateProvinceCode).Name;
            model.Address.Address1 = address1;



            var address = model.Address.ToEntity();
            address.CreatedOnUtc = DateTime.UtcNow;

            _addressService.InsertAddress(address);
            var warehouse = new Warehouse
            {
                Name = model.Name,
                AdminComment = model.AdminComment,
                AddressId = address.Id,
                IsDelivery = model.IsDelivery,  /// NU-78
                IsPickup = model.IsPickup,  /// NU-13
                AllowDeliveryTime = model.AllowDeliveryTime,    /// NU-78
                AllowPickupTime = model.AllowPickupTime,    ///  NU-13
                DeliveryOpenTimeMon = model.DeliveryOpenTimeMon,    /// NU-78
                DeliveryCloseTimeMon = model.DeliveryCloseTimeMon,  /// NU-78
                PickupOpenTimeMon = model.PickupOpenTimeMon,    /// NU-13
                PickupCloseTimeMon = model.PickupCloseTimeMon,  /// NU-13


                DeliveryOpenTimeTues = model.DeliveryOpenTimeTues,  /// NU-78
                DeliveryCloseTimeTues = model.DeliveryCloseTimeTues,    /// NU-78
                PickupOpenTimeTues = model.PickupOpenTimeTues,  /// NU-13
                PickupCloseTimeTues = model.PickupCloseTimeTues,    /// NU-13

                DeliveryOpenTimeWeds = model.DeliveryOpenTimeWeds,  /// NU-78
                DeliveryCloseTimeWeds = model.DeliveryCloseTimeWeds,    /// NU-78
                PickupOpenTimeWeds = model.PickupOpenTimeWeds,  /// NU-13
                PickupCloseTimeWeds = model.PickupCloseTimeWeds,    /// NU-13

                DeliveryOpenTimeThurs = model.DeliveryOpenTimeThurs,    /// NU-78
                DeliveryCloseTimeThurs = model.DeliveryCloseTimeThurs,  /// NU-78
                PickupOpenTimeThurs = model.PickupOpenTimeThurs,    /// NU-13
                PickupCloseTimeThurs = model.PickupCloseTimeThurs,  /// NU-13

                DeliveryOpenTimeFri = model.DeliveryOpenTimeFri,    /// NU-78
                DeliveryCloseTimeFri = model.DeliveryCloseTimeFri,  /// NU-78
                PickupOpenTimeFri = model.PickupOpenTimeFri,    /// NU-13
                PickupCloseTimeFri = model.PickupCloseTimeFri,  /// NU-13

                DeliveryOpenTimeSat = model.DeliveryOpenTimeSat,    /// NU-78
                DeliveryCloseTimeSat = model.DeliveryCloseTimeSat,  /// NU-78
                PickupOpenTimeSat = model.PickupOpenTimeSat,    /// NU-13
                PickupCloseTimeSat = model.PickupCloseTimeSat,  /// NU-13

                DeliveryOpenTimeSun = model.DeliveryOpenTimeSun,    /// NU-78
                DeliveryCloseTimeSun = model.DeliveryCloseTimeSun,  /// NU-78
                PickupOpenTimeSun = model.PickupOpenTimeSun,    /// NU-13
                PickupCloseTimeSun = model.PickupCloseTimeSun,  /// NU-13

                PickupFee = model.PickupFee, //NU-90
                DeliveryFee = model.DeliveryFee, //NU-90

                IsMaster = false    /// NU-10
            };

            if (!warehouse.IsDelivery)
            {
                warehouse.DeliveryFee = 0;
            }
            if (!warehouse.IsPickup)
            {
                warehouse.PickupFee = 0;
            }

            _shippingService.InsertWarehouse(warehouse);

            #region NU-77
            if (model.VendorId > 0)
            {
                _vendorMappingService.InsertVendorMapping(warehouse, model.VendorId);
            }
            else
            {
                _storeMappingService.InsertStoreMapping(warehouse, model.StoreId);
            }
            #endregion

            SuccessNotification(_localizationService.GetResource("Admin.Configuration.Shipping.Warehouses.Added"));


            return continueEditing ? RedirectToAction("EditWarehouse", new { id = warehouse.Id }) : RedirectToAction("Warehouses");


        }
        //---END: Codechages done by (na-sdxcorp\ADas)--------------

        public ActionResult CreateWarehouse()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageStores))
                return AccessDeniedView();

            //---START: Codechages done by (na-sdxcorp\ADas)--------------
            ViewBag.UseUPSAPI = "NOT_INITIATED";
            //---END: Codechages done by (na-sdxcorp\ADas)--------------

            var model = new WarehouseModel();
           // model.Address.AvailableCountries.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Address.SelectCountry"), Value = "0" });
            foreach (var c in _countryService.GetAllCountries(showHidden: true))
                model.Address.AvailableCountries.Add(new SelectListItem { Text = c.Name, Value = c.Id.ToString() });
            model.Address.AvailableStates.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Address.OtherNonUS"), Value = "0" });
            model.Address.CountryEnabled = true;            
            model.Address.StateProvinceEnabled = true;
            model.Address.CityEnabled = true;
            model.Address.CityRequired = true;
            model.Address.StreetAddressEnabled = true;
            model.Address.StreetAddressRequired = true;
            model.Address.ZipPostalCodeEnabled = true;
            model.Address.ZipPostalCodeRequired = true;
            model.Address.PhoneEnabled = true;
            model.Address.EmailEnabled = true;
            model.Address.EmailRequired = false;
            

            #region NU-77
            if (_workContext.CurrentCustomer.IsSystemGlobalAdmin())
                model.AvailableStores.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            foreach (var s in _storeService.GetAllStores().OrderBy(s => s.Name))
                model.AvailableStores.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString() });

            

            model.StoreId = _storeContext.CurrentStore.Id;

            if (_workContext.CurrentCustomer.IsSystemGlobalAdmin())
                model.AvailableVendors.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            foreach (var s in _vendorService.GetAllVendors().OrderBy(s => s.Name))
                model.AvailableVendors.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString() });
            model.VendorId = _workContext.CurrentCustomer.VendorId;

            model.IsLoggedAs = _workContext.CurrentCustomer.IsLoggedAs();
            #endregion

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public ActionResult CreateWarehouse(WarehouseModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageStores))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var address = model.Address.ToEntity();
                address.CreatedOnUtc = DateTime.UtcNow;
                //address.Email = model.Email;
                //  var addressFound = _addressService.IsUPSAddressValid(address);

                //---START: Codechages done by (na-sdxcorp\ADas)--------------
                //UPS Address Validation to be done only for UA and CANADA
                bool isEnabled = false;

                var pluginDescriptor = _pluginFinder.GetPluginDescriptorBySystemName("Shipping.UPS");
                var pluginInstance = pluginDescriptor.Instance();
                if (pluginInstance is IShippingRateComputationMethod)
                {
                    //isEnabled = ((IShippingRateComputationMethod)pluginInstance).IsShippingRateComputationMethodActive(_shippingSettings);
                    isEnabled = false;
                }

                if (isEnabled)
                {

                    if (address.CountryId != null && (address.CountryId == 1 || address.CountryId == 2))
                    {
                        //Get countrycode
                        var countryCode = _countryService.GetCountryById((int)address.CountryId).TwoLetterIsoCode;
                        //Get statecode
                        var stateCode = _stateProvinceService.GetStateProvinceById((int)address.StateProvinceId).Abbreviation;

                        //Get Matching Address after UPS Validation
                        List<UPSAddress> allMatchingUPSAddress = GetAddressValidatedWithUPS(address.City, stateCode, address.ZipPostalCode, countryCode, address.Address1);
                        bool isUPSException = false;

                        if (Session["UPSFailure"] != null)
                        {
                            isUPSException = (bool)Session["UPSFailure"];
                            Session.Remove("UPSFailure");
                        }

                        if (allMatchingUPSAddress == null && isUPSException == true)
                        {
                            //UPS having any technical fault.

                        }
                        else if (allMatchingUPSAddress == null && isUPSException == false)
                        {
                            //UPS found no matching result.
                            ViewBag.NoMatchFound = true;

                            model.Address.AvailableCountries.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Address.SelectCountry"), Value = "0" });
                            foreach (var c in _countryService.GetAllCountries(showHidden: true))
                                model.Address.AvailableCountries.Add(new SelectListItem { Text = c.Name, Value = c.Id.ToString(), Selected = (c.Id == model.Address.CountryId) });
                            //states
                            var tmp_states = model.Address.CountryId.HasValue ? _stateProvinceService.GetStateProvincesByCountryId(model.Address.CountryId.Value, showHidden: true).ToList() : new List<StateProvince>();
                            if (tmp_states.Count > 0)
                            {
                                foreach (var s in tmp_states)
                                    model.Address.AvailableStates.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString(), Selected = (s.Id == model.Address.StateProvinceId) });
                            }
                            else
                                model.Address.AvailableStates.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Address.OtherNonUS"), Value = "0" });


                            return View(model);
                        }
                        //If Matching Address Found
                        else if (allMatchingUPSAddress != null && allMatchingUPSAddress.Count > 0)
                        {
                            ViewBag.UserInputReq = true;
                            model.Address.AvailableCountries.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Address.SelectCountry"), Value = "0" });
                            foreach (var c in _countryService.GetAllCountries(showHidden: true))
                                model.Address.AvailableCountries.Add(new SelectListItem { Text = c.Name, Value = c.Id.ToString(), Selected = (c.Id == model.Address.CountryId) });
                            //states
                            var tmp_states = model.Address.CountryId.HasValue ? _stateProvinceService.GetStateProvincesByCountryId(model.Address.CountryId.Value, showHidden: true).ToList() : new List<StateProvince>();
                            if (tmp_states.Count > 0)
                            {
                                foreach (var s in tmp_states)
                                    model.Address.AvailableStates.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString(), Selected = (s.Id == model.Address.StateProvinceId) });
                            }
                            else
                                model.Address.AvailableStates.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Address.OtherNonUS"), Value = "0" });


                            //If only single match found
                            if (allMatchingUPSAddress.Count == 1)
                            {
                                ViewBag.SingleMatchFound = true;
                                //Modified model with validated address
                                ModelState.Remove("Address.City");
                                ModelState.Remove("Address.ZipPostalCode");
                                ModelState.Remove("Address.StateProvinceId");
                                ModelState.Remove("Address.StateProvinceName");
                                ModelState.Remove("Address.Address1");
                                model.Address.City = allMatchingUPSAddress[0].City;
                                model.Address.ZipPostalCode = allMatchingUPSAddress[0].PostalCode;
                                model.Address.StateProvinceId = _stateProvinceService.GetStateProvinceByAbbreviation(allMatchingUPSAddress[0].StateProvinceCode).Id;
                                model.Address.StateProvinceName = _stateProvinceService.GetStateProvinceByAbbreviation(allMatchingUPSAddress[0].StateProvinceCode).Name;
                                model.Address.Address1 = allMatchingUPSAddress[0].AddressLine;
                            }
                            else if (allMatchingUPSAddress.Count > 1)
                            {
                                //If multiple matches found
                                ViewBag.MultipleMatchFound = true;
                                ViewBag.AllMatchingUPSAddress = allMatchingUPSAddress;
                                Session["LastPostedModel"] = model;
                                Session["continueEditing"] = continueEditing;
                            }




                            return View(model);
                        }
                    }
                    //---END: Codechages done by (na-sdxcorp\ADas)--------------
                }

                _addressService.InsertAddress(address);
                var warehouse = new Warehouse
                {
                    Name = model.Name,
                    AdminComment = model.AdminComment,
                    AddressId = address.Id,
                    IsDelivery = model.IsDelivery,	/// NU-78
                    IsPickup = model.IsPickup,	/// NU-13
                    AllowDeliveryTime = model.AllowDeliveryTime,	/// NU-78
                    AllowPickupTime = model.AllowPickupTime,	///  NU-13
                    DeliveryOpenTimeMon = model.DeliveryOpenTimeMon,	/// NU-78
                    DeliveryCloseTimeMon = model.DeliveryCloseTimeMon,	/// NU-78
                    PickupOpenTimeMon = model.PickupOpenTimeMon,	/// NU-13
                    PickupCloseTimeMon = model.PickupCloseTimeMon,	/// NU-13


                    DeliveryOpenTimeTues = model.DeliveryOpenTimeTues,	/// NU-78
                    DeliveryCloseTimeTues = model.DeliveryCloseTimeTues,	/// NU-78
                    PickupOpenTimeTues = model.PickupOpenTimeTues,	/// NU-13
                    PickupCloseTimeTues = model.PickupCloseTimeTues,	/// NU-13

                    DeliveryOpenTimeWeds = model.DeliveryOpenTimeWeds,	/// NU-78
                    DeliveryCloseTimeWeds = model.DeliveryCloseTimeWeds,	/// NU-78
                    PickupOpenTimeWeds = model.PickupOpenTimeWeds,	/// NU-13
                    PickupCloseTimeWeds = model.PickupCloseTimeWeds,	/// NU-13

                    DeliveryOpenTimeThurs = model.DeliveryOpenTimeThurs,	/// NU-78
                    DeliveryCloseTimeThurs = model.DeliveryCloseTimeThurs,	/// NU-78
                    PickupOpenTimeThurs = model.PickupOpenTimeThurs,	/// NU-13
                    PickupCloseTimeThurs = model.PickupCloseTimeThurs,	/// NU-13

                    DeliveryOpenTimeFri = model.DeliveryOpenTimeFri,	/// NU-78
                    DeliveryCloseTimeFri = model.DeliveryCloseTimeFri,	/// NU-78
                    PickupOpenTimeFri = model.PickupOpenTimeFri,	/// NU-13
                    PickupCloseTimeFri = model.PickupCloseTimeFri,	/// NU-13

                    DeliveryOpenTimeSat = model.DeliveryOpenTimeSat,	/// NU-78
                    DeliveryCloseTimeSat = model.DeliveryCloseTimeSat,	/// NU-78
                    PickupOpenTimeSat = model.PickupOpenTimeSat,	/// NU-13
                    PickupCloseTimeSat = model.PickupCloseTimeSat,	/// NU-13

                    DeliveryOpenTimeSun = model.DeliveryOpenTimeSun,	/// NU-78
                    DeliveryCloseTimeSun = model.DeliveryCloseTimeSun,	/// NU-78
                    PickupOpenTimeSun = model.PickupOpenTimeSun,	/// NU-13
                    PickupCloseTimeSun = model.PickupCloseTimeSun,  /// NU-13

                    PickupFee = model.PickupFee, //NU-90
                    DeliveryFee = model.DeliveryFee, //NU-90

                    IsMaster = false	/// NU-10
                };


                if (!warehouse.IsDelivery)
                {
                    warehouse.DeliveryFee = 0;
                }
                if (!warehouse.IsPickup)
                {
                    warehouse.PickupFee = 0;
                }

                _shippingService.InsertWarehouse(warehouse);

                #region NU-77
                if (model.VendorId > 0)
                {
                    _vendorMappingService.InsertVendorMapping(warehouse, model.VendorId);
                }
                else
                {
                    _storeMappingService.InsertStoreMapping(warehouse, model.StoreId);
                }
                #endregion

                SuccessNotification(_localizationService.GetResource("Admin.Configuration.Shipping.Warehouses.Added"));


                return continueEditing ? RedirectToAction("EditWarehouse", new { id = warehouse.Id }) : RedirectToAction("Warehouses");
            }

            //If we got this far, something failed, redisplay form
            //countries
            model.Address.AvailableCountries.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Address.SelectCountry"), Value = "0" });
            foreach (var c in _countryService.GetAllCountries(showHidden: true))
                model.Address.AvailableCountries.Add(new SelectListItem { Text = c.Name, Value = c.Id.ToString(), Selected = (c.Id == model.Address.CountryId) });
            //states
            var states = model.Address.CountryId.HasValue ? _stateProvinceService.GetStateProvincesByCountryId(model.Address.CountryId.Value, showHidden: true).ToList() : new List<StateProvince>();
            if (states.Any())
            {
                foreach (var s in states)
                    model.Address.AvailableStates.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString(), Selected = (s.Id == model.Address.StateProvinceId) });
            }
            else
                model.Address.AvailableStates.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Address.OtherNonUS"), Value = "0" });

            return View(model);
        }

        public ActionResult EditWarehouse(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageStores))
                return AccessDeniedView();

            var warehouse = _shippingService.GetWarehouseById(id);
            if (warehouse == null)
                //No warehouse found with the specified id
                return RedirectToAction("Warehouses");

            var address = _addressService.GetAddressById(warehouse.AddressId);
            var model = new WarehouseModel
            {
                Id = warehouse.Id,
                Name = warehouse.Name,
                AdminComment = warehouse.AdminComment,
                IsDelivery = warehouse.IsDelivery,	/// NU-78
                IsPickup = warehouse.IsPickup,	/// NU-13
                AllowDeliveryTime = warehouse.AllowDeliveryTime,	/// NU-78
                AllowPickupTime = warehouse.AllowPickupTime,	/// NU-13

                DeliveryOpenTimeMon = warehouse.DeliveryOpenTimeMon,	/// NU-78
                DeliveryCloseTimeMon = warehouse.DeliveryCloseTimeMon,	/// NU-78
                PickupOpenTimeMon = warehouse.PickupOpenTimeMon,	/// NU-13
                PickupCloseTimeMon = warehouse.PickupCloseTimeMon,	/// NU-13


                DeliveryOpenTimeTues = warehouse.DeliveryOpenTimeTues,	/// NU-78
                DeliveryCloseTimeTues = warehouse.DeliveryCloseTimeTues,	/// NU-78
                PickupOpenTimeTues = warehouse.PickupOpenTimeTues,	/// NU-13
                PickupCloseTimeTues = warehouse.PickupCloseTimeTues,	/// NU-13

                DeliveryOpenTimeWeds = warehouse.DeliveryOpenTimeWeds,	/// NU-78
                DeliveryCloseTimeWeds = warehouse.DeliveryCloseTimeWeds,	/// NU-78
                PickupOpenTimeWeds = warehouse.PickupOpenTimeWeds,	/// NU-13
                PickupCloseTimeWeds = warehouse.PickupCloseTimeWeds,	/// NU-13

                DeliveryOpenTimeThurs = warehouse.DeliveryOpenTimeThurs,	/// NU-78
                DeliveryCloseTimeThurs = warehouse.DeliveryCloseTimeThurs,	/// NU-78
                PickupOpenTimeThurs = warehouse.PickupOpenTimeThurs,	/// NU-13
                PickupCloseTimeThurs = warehouse.PickupCloseTimeThurs,	/// NU-13

                DeliveryOpenTimeFri = warehouse.DeliveryOpenTimeFri,	/// NU-78
                DeliveryCloseTimeFri = warehouse.DeliveryCloseTimeFri,	/// NU-78
                PickupOpenTimeFri = warehouse.PickupOpenTimeFri,	/// NU-13
                PickupCloseTimeFri = warehouse.PickupCloseTimeFri,	/// NU-13

                DeliveryOpenTimeSat = warehouse.DeliveryOpenTimeSat,	/// NU-78
                DeliveryCloseTimeSat = warehouse.DeliveryCloseTimeSat,	/// NU-78
                PickupOpenTimeSat = warehouse.PickupOpenTimeSat,	/// NU-13
                PickupCloseTimeSat = warehouse.PickupCloseTimeSat,	/// NU-13

                DeliveryOpenTimeSun = warehouse.DeliveryOpenTimeSun,	/// NU-78
                DeliveryCloseTimeSun = warehouse.DeliveryCloseTimeSun,	/// NU-78
                PickupOpenTimeSun = warehouse.PickupOpenTimeSun,	/// NU-13
                PickupCloseTimeSun = warehouse.PickupCloseTimeSun,	/// NU-13
                DeliveryFee = warehouse.DeliveryFee, //NU-90
                PickupFee = warehouse.PickupFee,  //SWU-515
                IsAdmin = true,
                IsReadOnly = _workContext.CurrentCustomer.ReadOnly
                                                                   

            };


            if (!model.IsDelivery) //NU-90
            {
                model.DeliveryFee = 0;
            }
            if (!model.IsPickup)//NU-90
            {
                model.PickupFee = 0;
            }

            if (address != null)
            {
                model.Address = address.ToModel();
            }
            //countries
            model.Address.AvailableCountries.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Address.SelectCountry"), Value = "0" });
            foreach (var c in _countryService.GetAllCountries(showHidden: true))
                model.Address.AvailableCountries.Add(new SelectListItem { Text = c.Name, Value = c.Id.ToString(), Selected = (address != null && c.Id == address.CountryId) });
            //states
            var states = address != null && address.Country != null ? _stateProvinceService.GetStateProvincesByCountryId(address.Country.Id, showHidden: true).ToList() : new List<StateProvince>();
            if (states.Any())
            {
                foreach (var s in states)
                    model.Address.AvailableStates.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString(), Selected = (s.Id == address.StateProvinceId) });
            }
            else
                model.Address.AvailableStates.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Address.OtherNonUS"), Value = "0" });
            model.Address.CountryEnabled = true;
            model.Address.StateProvinceEnabled = true;
            model.Address.CityEnabled = true;
            model.Address.StreetAddressEnabled = true;
            model.Address.ZipPostalCodeEnabled = true;
            model.Address.ZipPostalCodeRequired = false;
            model.Address.PhoneEnabled = true;
            model.Address.EmailEnabled = true;
            model.Address.EmailRequired = false;

            var warehouseMapping = GetWarehouseMapping(warehouse);  // NU-79

            #region NU-80
            model.AvailableStores.Add(new SelectListItem { Text = "", Value = "-1" });
            foreach (var s in _storeService.GetAllStores().OrderBy(s => s.Name))
                model.AvailableStores.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString() });

            model.StoreId = (warehouseMapping.storeMapping == null ? -1 : warehouseMapping.storeMapping.StoreId);
            #endregion

            #region NU-48
            model.AvailableVendors.Add(new SelectListItem { Text = "", Value = "-1" });
            foreach (var s in _vendorService.GetAllVendors().OrderBy(s => s.Name))
                model.AvailableVendors.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString() });
            model.VendorId = (warehouseMapping.vendorMapping == null ? -1 : warehouseMapping.vendorMapping.VendorId);
            #endregion

            model.IsLoggedInAsVendor = _workContext.CurrentCustomer.IsVendor();	/// NU-1

            model.KitchenPrinterURL = warehouse.KitchenPrinterURL;	// NU-84

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public ActionResult EditWarehouse(WarehouseModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageStores))
                return AccessDeniedView();

            var warehouse = _shippingService.GetWarehouseById(model.Id);
            if (warehouse == null)
                //No warehouse found with the specified id
                return RedirectToAction("Warehouses");

            if (ModelState.IsValid)
            {
                var address = _addressService.GetAddressById(warehouse.AddressId) ??
                    new Core.Domain.Common.Address
                    {
                        CreatedOnUtc = DateTime.UtcNow,
                    };
                address = model.Address.ToEntity(address);


                //        var isValidAddress = _addressService.IsUPSAddressValid(address);  TODO:  Reneable


                if (address.Id > 0)
                    _addressService.UpdateAddress(address);
                else
                    _addressService.InsertAddress(address);


                warehouse.Name = model.Name;
                warehouse.AdminComment = model.AdminComment;
                warehouse.AddressId = address.Id;
                warehouse.IsDelivery = model.IsDelivery;	/// NU-78
                warehouse.IsPickup = model.IsPickup;	/// NU-13
                warehouse.AllowDeliveryTime = model.AllowDeliveryTime;	/// NU-78
                warehouse.AllowPickupTime = model.AllowPickupTime;  /// NU-13
                                                                    /// 
                warehouse.DeliveryOpenTimeMon = model.DeliveryOpenTimeMon;	/// NU-78
                warehouse.DeliveryCloseTimeMon = model.DeliveryCloseTimeMon;	/// NU-78
                warehouse.PickupOpenTimeMon = model.PickupOpenTimeMon;	/// NU-13
                warehouse.PickupCloseTimeMon = model.PickupCloseTimeMon;	/// NU-13


                warehouse.DeliveryOpenTimeTues = model.DeliveryOpenTimeTues;	/// NU-78
                warehouse.DeliveryCloseTimeTues = model.DeliveryCloseTimeTues;	/// NU-78
                warehouse.PickupOpenTimeTues = model.PickupOpenTimeTues;	/// NU-13
                warehouse.PickupCloseTimeTues = model.PickupCloseTimeTues;	/// NU-13

                warehouse.DeliveryOpenTimeWeds = model.DeliveryOpenTimeWeds;	/// NU-78
                warehouse.DeliveryCloseTimeWeds = model.DeliveryCloseTimeWeds;	/// NU-78
                warehouse.PickupOpenTimeWeds = model.PickupOpenTimeWeds;	/// NU-13
                warehouse.PickupCloseTimeWeds = model.PickupCloseTimeWeds;	/// NU-13

                warehouse.DeliveryOpenTimeThurs = model.DeliveryOpenTimeThurs;	/// NU-78
                warehouse.DeliveryCloseTimeThurs = model.DeliveryCloseTimeThurs;	/// NU-78
                warehouse.PickupOpenTimeThurs = model.PickupOpenTimeThurs;	/// NU-13
                warehouse.PickupCloseTimeThurs = model.PickupCloseTimeThurs;	/// NU-13

                warehouse.DeliveryOpenTimeFri = model.DeliveryOpenTimeFri;	/// NU-78
                warehouse.DeliveryCloseTimeFri = model.DeliveryCloseTimeFri;	/// NU-78
                warehouse.PickupOpenTimeFri = model.PickupOpenTimeFri;	/// NU-13
                warehouse.PickupCloseTimeFri = model.PickupCloseTimeFri;	/// NU-13

                warehouse.DeliveryOpenTimeSat = model.DeliveryOpenTimeSat;	/// NU-78
                warehouse.DeliveryCloseTimeSat = model.DeliveryCloseTimeSat;	/// NU-78
                warehouse.PickupOpenTimeSat = model.PickupOpenTimeSat;	/// NU-13
                warehouse.PickupCloseTimeSat = model.PickupCloseTimeSat;	/// NU-13

                warehouse.DeliveryOpenTimeSun = model.DeliveryOpenTimeSun;	/// NU-78
                warehouse.DeliveryCloseTimeSun = model.DeliveryCloseTimeSun;	/// NU-78
                warehouse.PickupOpenTimeSun = model.PickupOpenTimeSun;	/// NU-13
                warehouse.PickupCloseTimeSun = model.PickupCloseTimeSun;	/// NU-13

                warehouse.KitchenPrinterURL = model.KitchenPrinterURL;	// NU-84


                warehouse.PickupFee = model.PickupFee; //NU-90
                warehouse.DeliveryFee = model.DeliveryFee; //NU-90

                _shippingService.UpdateWarehouse(warehouse);
                SuccessNotification(_localizationService.GetResource("Admin.Configuration.Shipping.Warehouses.Updated"));

                return continueEditing ? RedirectToAction("EditWarehouse", warehouse.Id) : RedirectToAction("Warehouses");
            }


            //If we got this far, something failed, redisplay form

            //countries
            model.Address.AvailableCountries.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Address.SelectCountry"), Value = "0" });
            foreach (var c in _countryService.GetAllCountries(showHidden: true))
                model.Address.AvailableCountries.Add(new SelectListItem { Text = c.Name, Value = c.Id.ToString(), Selected = (c.Id == model.Address.CountryId) });
            //states
            var states = model.Address.CountryId.HasValue ? _stateProvinceService.GetStateProvincesByCountryId(model.Address.CountryId.Value, showHidden: true).ToList() : new List<StateProvince>();
            if (states.Any())
            {
                foreach (var s in states)
                    model.Address.AvailableStates.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString(), Selected = (s.Id == model.Address.StateProvinceId) });
            }
            else
                model.Address.AvailableStates.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Address.OtherNonUS"), Value = "0" });

            return View(model);
        }

        [HttpPost]
        public ActionResult DeleteWarehouse(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageStores))
                return AccessDeniedView();

            var warehouse = _shippingService.GetWarehouseById(id);
            if (warehouse == null)
                //No warehouse found with the specified id
                return RedirectToAction("Warehouses");

            #region NU-81
            var warehouseMapping = GetWarehouseMapping(warehouse);

            if (warehouseMapping.storeMapping != null)
                _storeMappingService.DeleteStoreMapping(warehouseMapping.storeMapping);
            else
                _vendorMappingService.DeleteVendorMapping(warehouseMapping.vendorMapping);

            _shippingService.DeleteWarehouse(warehouse);
            #endregion

            SuccessNotification(_localizationService.GetResource("Admin.Configuration.Shipping.warehouses.Deleted"));
            return RedirectToAction("Warehouses");
        }

        #endregion

        #region Restrictions

        public ActionResult Restrictions()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageStores))
                return AccessDeniedView();

            var model = new ShippingMethodRestrictionModel();

            var countries = _countryService.GetAllCountries(showHidden: true);
            var shippingMethods = _shippingService.GetAllShippingMethods();
            foreach (var country in countries)
            {
                model.AvailableCountries.Add(new CountryModel
                {
                    Id = country.Id,
                    Name = country.Name
                });
            }
            foreach (var sm in shippingMethods)
            {
                model.AvailableShippingMethods.Add(new ShippingMethodModel
                {
                    Id = sm.Id,
                    Name = sm.Name
                });
            }
            foreach (var country in countries)
                foreach (var shippingMethod in shippingMethods)
                {
                    bool restricted = shippingMethod.CountryRestrictionExists(country.Id);
                    if (!model.Restricted.ContainsKey(country.Id))
                        model.Restricted[country.Id] = new Dictionary<int, bool>();
                    model.Restricted[country.Id][shippingMethod.Id] = restricted;
                }

            return View(model);
        }

        [HttpPost, ActionName("Restrictions")]
        public ActionResult RestrictionSave(FormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageStores))
                return AccessDeniedView();

            var countries = _countryService.GetAllCountries(showHidden: true);
            var shippingMethods = _shippingService.GetAllShippingMethods();


            foreach (var shippingMethod in shippingMethods)
            {
                string formKey = "restrict_" + shippingMethod.Id;
                var countryIdsToRestrict = form[formKey] != null
                    ? form[formKey].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(int.Parse)
                    .ToList()
                    : new List<int>();

                foreach (var country in countries)
                {

                    bool restrict = countryIdsToRestrict.Contains(country.Id);
                    if (restrict)
                    {
                        if (shippingMethod.RestrictedCountries.FirstOrDefault(c => c.Id == country.Id) == null)
                        {
                            shippingMethod.RestrictedCountries.Add(country);
                            _shippingService.UpdateShippingMethod(shippingMethod);
                        }
                    }
                    else
                    {
                        if (shippingMethod.RestrictedCountries.FirstOrDefault(c => c.Id == country.Id) != null)
                        {
                            shippingMethod.RestrictedCountries.Remove(country);
                            _shippingService.UpdateShippingMethod(shippingMethod);
                        }
                    }
                }
            }

            SuccessNotification(_localizationService.GetResource("Admin.Configuration.Shipping.Restrictions.Updated"));
            return RedirectToAction("Restrictions");
        }

        #endregion

        #region NU-81
        public WarehouseMapping GetWarehouseMapping(Warehouse warehouse)
        {
            WarehouseMapping ret = new WarehouseMapping();

            ret.storeMapping = null;
            var storeMappings = _storeMappingService.GetStoreMappings(warehouse);
            if (storeMappings.Count() > 0)
                ret.storeMapping = storeMappings.First();

            ret.vendorMapping = null;
            var vendorMappings = _vendorMappingService.GetVendorMappings(warehouse);
            if (vendorMappings.Count() > 0)
                ret.vendorMapping = vendorMappings.First();

            return ret;
        }
        #endregion
    }

    #region NU-81
    public class WarehouseMapping
    {
        public StoreMapping storeMapping { get; set; }

        public VendorMapping vendorMapping { get; set; }
    }
    #endregion
}
