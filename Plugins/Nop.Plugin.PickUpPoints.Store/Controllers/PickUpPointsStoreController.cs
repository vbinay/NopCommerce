using Nop.Core;
using Nop.Core.Caching;
using Nop.Plugin.PickUpPoints.Store.Models;

//using Nop.Plugin.PickUpPoints.Store.Infrastructure.Cache;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Stores;
using Nop.Web.Framework.Controllers;
using System.Web.Mvc;
using System.Xml.Linq;

namespace Nop.Plugin.PickUpPoints.Store.Controllers
{
    public class PickUpPointsStoreController : BasePluginController
    {
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IStoreService _storeService;
        private readonly IPictureService _pictureService;
        private readonly ISettingService _settingService;
        private readonly ICacheManager _cacheManager;
        private readonly ILocalizationService _localizationService;

        public PickUpPointsStoreController(IWorkContext workContext,
            IStoreContext storeContext,
            IStoreService storeService,
            IPictureService pictureService,
            ISettingService settingService,
            ICacheManager cacheManager,
            ILocalizationService localizationService)
        {
            this._workContext = workContext;
            this._storeContext = storeContext;
            this._storeService = storeService;
            this._pictureService = pictureService;
            this._settingService = settingService;
            this._cacheManager = cacheManager;
            this._localizationService = localizationService;
        }

        //protected string GetPictureUrl(int pictureId)
        //{
        //    string cacheKey = string.Format(ModelCacheEventConsumer.PICTURE_URL_MODEL_KEY, pictureId);
        //    return _cacheManager.Get(cacheKey, () =>
        //    {
        //        var url = _pictureService.GetPictureUrl(pictureId, showDefaultPicture: false);
        //        //little hack here. nulls aren't cacheable so set it to ""
        //        if (url == null)
        //            url = "";

        //        return url;
        //    });
        //}

        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure()
        {
            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var storeSettings = _settingService.LoadSetting<StoreSettings>(storeScope);
            var model = new ConfigurationModel();
            model.Street = storeSettings.Street;
            model.BuildingNumber = storeSettings.BuildingNumber;
            model.PostCode = storeSettings.PostCode;
            model.Town = storeSettings.Town;
            model.Longitude = storeSettings.Longitude;
            model.Latitude = storeSettings.Latitude;
            model.Description = storeSettings.Description;
            model.Fee = storeSettings.Fee;

            model.ActiveStoreScopeConfiguration = storeScope;
            if (storeScope > 0)
            {
                model.Street_OverrideForStore = _settingService.SettingExists(storeSettings, x => x.Street, storeScope);
                model.BuildingNumber_OverrideForStore = _settingService.SettingExists(storeSettings, x => x.BuildingNumber, storeScope);
                model.PostCode_OverrideForStore = _settingService.SettingExists(storeSettings, x => x.PostCode, storeScope);
                model.Town_OverrideForStore = _settingService.SettingExists(storeSettings, x => x.Town, storeScope);
                model.Longitude_OverrideForStore = _settingService.SettingExists(storeSettings, x => x.Longitude, storeScope);
                model.Latitude_OverrideForStore = _settingService.SettingExists(storeSettings, x => x.Latitude, storeScope);
                model.Description_OverrideForStore = _settingService.SettingExists(storeSettings, x => x.Description, storeScope);
                model.Fee_OverrideForStore = _settingService.SettingExists(storeSettings, x => x.Fee, storeScope);
            }

            return View("~/Plugins/PickUpPoints.Store/Views/PickUpPointsStore/Configure.cshtml", model);
        }

        [HttpPost]
        [AdminAuthorize]
        [ChildActionOnly]
        [FormValueRequired("save")]
        public ActionResult Configure(ConfigurationModel model)
        {
            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var storeSettings = _settingService.LoadSetting<StoreSettings>(storeScope);
            storeSettings.Street = model.Street;
            storeSettings.BuildingNumber = model.BuildingNumber;
            storeSettings.PostCode = model.PostCode;
            storeSettings.Town = model.Town;
            storeSettings.Longitude = model.Longitude;
            storeSettings.Latitude = model.Latitude;
            storeSettings.Description = model.Description;
            storeSettings.Fee = model.Fee;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared
             * and loaded from database after each update */
            if (model.Street_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(storeSettings, x => x.Street, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(storeSettings, x => x.Street, storeScope);

            if (model.BuildingNumber_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(storeSettings, x => x.BuildingNumber, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(storeSettings, x => x.BuildingNumber, storeScope);

            if (model.PostCode_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(storeSettings, x => x.PostCode, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(storeSettings, x => x.PostCode, storeScope);

            if (model.Town_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(storeSettings, x => x.Town, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(storeSettings, x => x.Town, storeScope);

            if (model.Longitude_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(storeSettings, x => x.Longitude, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(storeSettings, x => x.Longitude, storeScope);

            if (model.Latitude_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(storeSettings, x => x.Latitude, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(storeSettings, x => x.Latitude, storeScope);

            if (model.Description_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(storeSettings, x => x.Description, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(storeSettings, x => x.Description, storeScope);

            if (model.Fee_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(storeSettings, x => x.Fee, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(storeSettings, x => x.Fee, storeScope);

            //now clear settings cache
            _settingService.ClearCache();

            SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));
            return Configure();
        }

        [ChildActionOnly]
        public ActionResult PublicInfo(string widgetZone, object additionalData = null)
        {
            var storeSettings = _settingService.LoadSetting<StoreSettings>(_storeContext.CurrentStore.Id);

            var model = new PublicInfoModel();

            return View("~/Plugins/PickUpPoints.Store/Views/PickUpPointsStore/PublicInfo.cshtml", model);
        }
    }
}