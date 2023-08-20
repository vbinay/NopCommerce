using System;
using System.Linq;
using System.Web.Mvc;
using Nop.Admin.Extensions;
using Nop.Admin.Models.Stores;
using Nop.Core.Domain.Stores;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using Nop.Core;
using Nop.Web.Framework.Mvc;
using Nop.Core.Domain.Orders;
using Nop.Services.Directory;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Web.Script.Serialization;
using System.Net.Http;
using System.Web;
using System.Net;
using System.Data;

namespace Nop.Admin.Controllers
{
    public partial class StoreController : BaseAdminController
    {
        #region Fields

        private readonly IStoreService _storeService;
        private readonly ISettingService _settingService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly IPermissionService _permissionService;
        private readonly IFulfillmentService _fulfillmentService;
        private readonly IStoreContext _storeContext;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IWorkContext _workContext;
        // private readonly IEmailTypeService _emailTypeService;	
        #endregion

        #region Constructors

        public StoreController(IStoreService storeService,
            ISettingService settingService,
            ILanguageService languageService,
            ILocalizationService localizationService,
            ILocalizedEntityService localizedEntityService,
            IPermissionService permissionService,
            IFulfillmentService FulfillmentService,
            IWorkContext workContext,
            IStoreContext storeContext, IStateProvinceService stateProvinceService)
        //IEmailTypeService emailTypeService)	
        {
            this._storeService = storeService;
            this._settingService = settingService;
            this._languageService = languageService;
            this._localizationService = localizationService;
            this._localizedEntityService = localizedEntityService;
            this._permissionService = permissionService;
            this._fulfillmentService = FulfillmentService;
            this._storeContext = storeContext;
            this._workContext = workContext;
            this._stateProvinceService = stateProvinceService;
            //this._emailTypeService = emailTypeService;	
        }

        #endregion

        #region Utilities

        [NonAction]
        protected virtual void PrepareLanguagesModel(StoreModel model)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            model.AvailableLanguages.Add(new SelectListItem
            {
                Text = "---",
                Value = "0"
            });
            var languages = _languageService.GetAllLanguages(true);
            foreach (var language in languages)
            {
                model.AvailableLanguages.Add(new SelectListItem
                {
                    Text = language.Name,
                    Value = language.Id.ToString()
                });
            }
        }

        [NonAction]
        protected virtual void UpdateAttributeLocales(Store store, StoreModel model)
        {
            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedValue(store,
                    x => x.Name,
                    localized.Name,
                    localized.LanguageId);
            }
        }

        #endregion

        #region Tiered Shipping
        public JsonResult GetTieredShipping()
        {
            var tieredShippingData = _storeService.GetTieredShipping(this._storeContext.CurrentStore.Id);

            var gridModel = new DataSourceResult
            {
                Data = tieredShippingData.Select(x =>
                {
                    return new TieredShippingModel
                    {
                        Id = x.Id,
                        MinPrice = x.MinPrice,
                        MaxPrice = x.MaxPrice,
                        ShippingAmount = x.ShippingAmount,
                        StoreId = x.StoreId
                    };
                })
            };
            return new JsonResult  { Data = gridModel, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public bool CheckforExclusiveMinMaxPrice(StoreWiseTierShipping tieredShippingModel)
        {
            bool isValid = true ;
            //var allTieredShippingData = _storeService.GetTieredShipping(this._storeContext.CurrentStore.Id);
           
            //if (tieredShippingModel != null)
            //{
            //    foreach (var tsm in allTieredShippingData)
            //    {
            //        if (tieredShippingModel.MinPrice >= tsm.MinPrice && tieredShippingModel.MinPrice <= tsm.MaxPrice)
            //            isValid = false;
                    
            //        if (tieredShippingModel.MaxPrice >= tsm.MinPrice && tieredShippingModel.MaxPrice <= tsm.MaxPrice)
            //            isValid = false;
            //    }
            //}

            //if (tieredShippingModel.MinPrice == allTieredShippingData.Max(x => x.MinPrice) && 
            //    tieredShippingModel.MaxPrice == 0)
            //{
            //    isValid = true;
            //}
            return isValid;
        }

        public JsonResult GetMinPrice()
        {
            decimal maxPrice = _storeService.GetTieredShipping(this._storeContext.CurrentStore.Id).Max(x => x.MaxPrice);
            maxPrice = maxPrice + Convert.ToDecimal(0.01);
            var finalTierSA = _storeService.GetTieredShipping(this._storeContext.CurrentStore.Id).Where(x => x.MinPrice == maxPrice).FirstOrDefault().ShippingAmount;
            if (finalTierSA > 0)
                maxPrice = -1;
            return new JsonResult { Data = maxPrice, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public JsonResult CreateTieredShipping()
        {
            var querystring = Request.QueryString["models"];
            TieredShippingModel record = JsonConvert.DeserializeObject<TieredShippingModel>(querystring);
            record.StoreId = this._storeContext.CurrentStore.Id;

            StoreWiseTierShipping tierShipping = new StoreWiseTierShipping();
            tierShipping.MinPrice = record.MinPrice;
            tierShipping.MaxPrice = record.MaxPrice;
            tierShipping.ShippingAmount = record.ShippingAmount;
            tierShipping.StoreId = record.StoreId;

            if (CheckforExclusiveMinMaxPrice(tierShipping))
            {
                _storeService.CreateTieredShipping(tierShipping);
                var data = _storeService.GetTieredShipping(this._storeContext.CurrentStore.Id);
                _storeContext.CurrentStore.ShippingTiers = data;
            }
            else
                ErrorNotification("Min/Max price should be exclusive for each tier of Shipping charges.");

            var tieredShippingData = _storeService.GetTieredShipping(this._storeContext.CurrentStore.Id);
            var gridModel = new DataSourceResult
            {
                Data = tieredShippingData.Select(x =>
                {
                    return new TieredShippingModel
                    {
                        Id = x.Id,
                        MinPrice = x.MinPrice,
                        MaxPrice = x.MaxPrice,
                        ShippingAmount = x.ShippingAmount,
                        StoreId = x.StoreId
                    };
                })
            };
            return new JsonResult { Data = gridModel, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
               
        public JsonResult UpdateTieredShipping()
        {
            var querystring = Request.QueryString["models"];
            TieredShippingModel record = JsonConvert.DeserializeObject<TieredShippingModel>(querystring);
            record.StoreId = this._storeContext.CurrentStore.Id;

            StoreWiseTierShipping tierShipping = new StoreWiseTierShipping();
            tierShipping.Id = record.Id;
            tierShipping.MinPrice = record.MinPrice;
            tierShipping.MaxPrice = record.MaxPrice;
            tierShipping.ShippingAmount = record.ShippingAmount;
            tierShipping.StoreId = record.StoreId;

            if (CheckforExclusiveMinMaxPrice(tierShipping))
            {
                _storeService.UpdateStoreTieredShippingbyId(tierShipping);
                var data = _storeService.GetTieredShipping(this._storeContext.CurrentStore.Id);
                _storeContext.CurrentStore.ShippingTiers = data;
            }
            else
                ErrorNotification("Min/Max price should be exclusive for each tier of Shipping charges.");

            var tieredShippingData = _storeService.GetTieredShipping(this._storeContext.CurrentStore.Id);

            var gridModel = new DataSourceResult
            {
                Data = tieredShippingData.Select(x =>
                {
                    return new TieredShippingModel
                    {
                        Id = x.Id,
                        MinPrice = x.MinPrice,
                        MaxPrice = x.MaxPrice,
                        ShippingAmount = x.ShippingAmount,
                        StoreId = x.StoreId
                    };
                })
            };
            return new JsonResult { Data = gridModel, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public JsonResult DeleteTieredShipping()
        {
            var querystring = Request.QueryString["models"];
            TieredShippingModel record = JsonConvert.DeserializeObject<TieredShippingModel>(querystring);
            record.StoreId = this._storeContext.CurrentStore.Id;

            StoreWiseTierShipping tierShipping = new StoreWiseTierShipping();
            tierShipping.Id = record.Id;
            tierShipping.MinPrice = record.MinPrice;
            tierShipping.MaxPrice = record.MaxPrice;
            tierShipping.ShippingAmount = record.ShippingAmount;
            tierShipping.StoreId = record.StoreId;

            _storeService.DeleteTieredShippingbyId(tierShipping);
            var tieredShippingData = _storeService.GetTieredShipping(this._storeContext.CurrentStore.Id);
            _storeContext.CurrentStore.ShippingTiers = tieredShippingData;

            var gridModel = new DataSourceResult
            {
                Data = tieredShippingData.Select(x =>
                {
                    return new TieredShippingModel
                    {
                        Id = x.Id,
                        MinPrice = x.MinPrice,
                        MaxPrice = x.MaxPrice,
                        ShippingAmount = x.ShippingAmount,
                        StoreId = x.StoreId
                    };
                })
            };
            return new JsonResult { Data = gridModel, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
        #endregion

        #region Methods

        public ActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageStores))
                return AccessDeniedView();

            return View();
        }

        [HttpPost]
        public ActionResult List(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageStores))
                return AccessDeniedView();

            var storeModels = _storeService.GetAllStores(_workContext.CurrentCustomer)	/// NU-75
                .Select(x => x.ToModel())
                .OrderBy(x => x.Name)	/// NU-24
                .ToList();

            var gridModel = new DataSourceResult
            {
                Data = storeModels,
                Total = storeModels.Count()
            };

            return Json(gridModel);
        }

        public ActionResult Create()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageStores))
                return AccessDeniedView();

            var model = new StoreModel();
            //languages
            PrepareLanguagesModel(model);



            var states = _stateProvinceService.GetStateProvincesByCountryId(1, _workContext.WorkingLanguage.Id).ToList();
            if (states.Any())
            {
                model.AvailableStates.Add(new SelectListItem { Text = _localizationService.GetResource("Address.SelectState"), Value = "0" });

                foreach (var s in states)
                {
                    model.AvailableStates.Add(new SelectListItem { Text = s.GetLocalized(x => x.Name), Value = s.Id.ToString(), Selected = (s.Id == model.CompanyStateProvinceId) });
                }
            }

            //locales
            AddLocales(_languageService, model.Locales);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public ActionResult Create(StoreModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageStores))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var store = model.ToEntity();
                //ensure we have "/" at the end
                if (!store.Url.EndsWith("/"))
                    store.Url += "/";
                _storeService.InsertStore(store);
                //locales
                UpdateAttributeLocales(store, model);

                SuccessNotification(_localizationService.GetResource("Admin.Configuration.Stores.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = store.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form

            //languages
            PrepareLanguagesModel(model);
            return View(model);
        }

        public ActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageStores))
                return AccessDeniedView();

            var store = _storeService.GetStoreById(id);
            if (store == null)
                //No store found with the specified id
                return RedirectToAction("List");

            var model = store.ToModel();

            //TierShipping
            var tieredShipping = _storeService.GetTieredShipping(id);
            foreach (var tier in tieredShipping)
            {
                TieredShippingModel ts = new TieredShippingModel();
                ts.MinPrice = tier.MinPrice;
                ts.MaxPrice = tier.MaxPrice;
                ts.ShippingAmount = tier.ShippingAmount;
                model.TieredShipping.Add(ts);
            }
           
            //languages
            PrepareLanguagesModel(model);
            //locales
            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = store.GetLocalized(x => x.Name, languageId, false, false);
            });


            var states = _stateProvinceService.GetStateProvincesByCountryId(1, _workContext.WorkingLanguage.Id).ToList();
            if (states.Any())
            {
                model.AvailableStates.Add(new SelectListItem { Text = _localizationService.GetResource("Address.SelectState"), Value = "0" });

                foreach (var s in states)
                {
                    model.AvailableStates.Add(new SelectListItem { Text = s.GetLocalized(x => x.Name), Value = s.Id.ToString(), Selected = (s.Id == model.CompanyStateProvinceId) });
                }
            }
            model.IsReadOnly = _workContext.CurrentCustomer.ReadOnly;
            return View(model);
        }

        public ActionResult EditCurrentStore()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageStores))
                return AccessDeniedView();

            return RedirectToAction("Edit", new { id = this._storeContext.CurrentStore.Id });
        }


        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        public ActionResult Edit(StoreModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageStores))
                return AccessDeniedView();

            var store = _storeService.GetStoreById(model.Id);
            if (store == null)
                //No store found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                store = model.ToEntity(store);
                //ensure we have "/" at the end
                if (!store.Url.EndsWith("/"))
                    store.Url += "/";
                _storeService.UpdateStore(store);


                //locales
                UpdateAttributeLocales(store, model);

                SuccessNotification(_localizationService.GetResource("Admin.Configuration.Stores.Updated"));
                return continueEditing ? RedirectToAction("Edit", new { id = store.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form

            //languages
            PrepareLanguagesModel(model);
            return View(model);
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageStores))
                return AccessDeniedView();

            var store = _storeService.GetStoreById(id);
            if (store == null)
                //No store found with the specified id
                return RedirectToAction("List");

            try
            {
                _storeService.DeleteStore(store);
                //when we delete a store we should also ensure that all "per store" settings will also be deleted
                var settingsToDelete = _settingService
                    .GetAllSettings()
                    .Where(s => s.StoreId == id)
                    .ToList();
                _settingService.DeleteSettings(settingsToDelete);
                //when we had two stores and now have only one store, we also should delete all "per store" settings
                var allStores = _storeService.GetAllStores();
                if (allStores.Count == 1)
                {
                    settingsToDelete = _settingService
                        .GetAllSettings()
                        .Where(s => s.StoreId == allStores[0].Id)
                        .ToList();
                    _settingService.DeleteSettings(settingsToDelete);
                }
                SuccessNotification(_localizationService.GetResource("Admin.Configuration.Stores.Deleted"));
                return RedirectToAction("List");
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("Edit", new { id = store.Id });
            }
        }

        #region NU-74
        [HttpPost]
        public ActionResult StoreContactList(DataSourceRequest command, int storeId)
        {
            var emailContacts = _storeService.GetAllStoreContacts(storeId);
            var emailContactsModel = emailContacts
                .Select(x => new StoreModel.StoreContactModel
                {
                    Id = x.Id,
                    EmailType = x.EmailTypeId.ToString(),
                    Email = x.Email,
                    DisplayName = x.DisplayName
                })
                .ToList();

            var gridModel = new DataSourceResult
            {
                Data = emailContactsModel,
                Total = emailContactsModel.Count
            };

            return Json(gridModel);
        }

        [HttpPost]
        public ActionResult StoreContactUpdate(StoreModel.StoreContactModel model)
        {
            var emailContact = _storeService.GetStoreContactById(model.Id);
            if (emailContact == null)
                throw new ArgumentException("No email contact found with the specified id");

            emailContact.Email = model.Email;
            emailContact.DisplayName = model.DisplayName;

            _storeService.UpdateStoreContact(emailContact);

            return new NullJsonResult();
        }
        #endregion

        #region SODMYWAY-
        /// <summary>
        /// Shows fulfillment availablility for Store
        /// </summary>
        /// <returns></returns>
        public ActionResult FulfillmentCalendar()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageStores))	// NU-91
                return AccessDeniedView();

            return View();
        }


        public JsonResult SaveEvent(string date)
        {
            FulfillmentCalDay day = new FulfillmentCalDay();
            day.Date = DateTime.Parse(date.Replace("\"", ""));
            var existingDay = _fulfillmentService.GetCalDay(_storeContext.CurrentStore.Id, day);
            if (existingDay != null)
            {
                _fulfillmentService.DeleteCalDay(_storeContext.CurrentStore.Id, existingDay);
            }
            else
            {
                _fulfillmentService.InsertCalDay(_storeContext.CurrentStore.Id, day);
            }
            return Json(true, JsonRequestBehavior.AllowGet); ;
        }




        public JsonResult GetFulfillmentAvailablility(string date)
        {


            var dateTime = DateTime.Parse(date.Replace("\"", ""));
            var ApptListForDate = _fulfillmentService.GetForMonths(_storeContext.CurrentStore.Id, dateTime.Year, dateTime.Month);
            var eventList = from e in ApptListForDate
                            select new
                            {
                                id = e.Id,
                                //title = "Closed",	// SODMYWAY-2780
                                name = "Closed",
                                //event_date = e.Date.ToString("yyyy-MM-dd"),	// SODMYWAY-2780
                                startdate = e.Date.ToString("yyyy-MM-dd"),
                                enddate = e.Date.ToString("yyyy-MM-dd"),
                                color = "#d3d3d3",//e.StatusColor,
                                //baccolor = "#01DF3A",//e.StatusColor,	// SODMYWAY-2780
                                //className = e.ClassName,	// SODMYWAY-2780
                                //someKey = e.SomeImportantKeyID,	// SODMYWAY-2780
                                //allDay = true	// SODMYWAY-2780
                            };
            var rows = eventList.ToArray();

            return Json(rows, JsonRequestBehavior.AllowGet);
        }


        #endregion


        #endregion

    }
}
