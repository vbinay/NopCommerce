using Nop.Core;
using Nop.Core.Caching;

//using Nop.Plugin.PickUpPoints.Paczkomaty.Infrastructure.Cache;
using Nop.Plugin.PickUpPoints.Paczkomaty.Models;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Stores;
using Nop.Web.Framework.Controllers;
using System.Web.Mvc;
using System.Xml.Linq;

namespace Nop.Plugin.PickUpPoints.Paczkomaty.Controllers
{
    public class PickUpPointsPaczkomatyController : BasePluginController
    {
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IStoreService _storeService;
        private readonly IPictureService _pictureService;
        private readonly ISettingService _settingService;
        private readonly ICacheManager _cacheManager;
        private readonly ILocalizationService _localizationService;

        public PickUpPointsPaczkomatyController(IWorkContext workContext,
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
            var paczkomatySettings = _settingService.LoadSetting<PaczkomatySettings>(storeScope);
            var model = new ConfigurationModel();
            model.ShowNearestThreePoints = paczkomatySettings.ShowNearestThreePoints;
            model.Fee = paczkomatySettings.Fee;
            model.MaxWeight = paczkomatySettings.MaxWeight;

            model.ActiveStoreScopeConfiguration = storeScope;
            if (storeScope > 0)
            {
                model.ShowNearestThreePoints_OverrideForStore = _settingService.SettingExists(paczkomatySettings, x => x.ShowNearestThreePoints, storeScope);
                model.Fee_OverrideForStore = _settingService.SettingExists(paczkomatySettings, x => x.Fee, storeScope);
                model.MaxWeight_OverrideForStore = _settingService.SettingExists(paczkomatySettings, x => x.MaxWeight, storeScope);
            }

            return View("~/Plugins/PickUpPoints.Paczkomaty/Views/PickUpPointsPaczkomaty/Configure.cshtml", model);
        }

        [HttpPost]
        [AdminAuthorize]
        [ChildActionOnly]
        [FormValueRequired("save")]
        public ActionResult Configure(ConfigurationModel model)
        {
            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var paczkomatySettings = _settingService.LoadSetting<PaczkomatySettings>(storeScope);
            paczkomatySettings.ShowNearestThreePoints = model.ShowNearestThreePoints;
            paczkomatySettings.Fee = model.Fee;
            paczkomatySettings.MaxWeight = model.MaxWeight;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared
             * and loaded from database after each update */
            if (model.ShowNearestThreePoints_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(paczkomatySettings, x => x.ShowNearestThreePoints, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(paczkomatySettings, x => x.ShowNearestThreePoints, storeScope);

            if (model.Fee_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(paczkomatySettings, x => x.Fee, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(paczkomatySettings, x => x.Fee, storeScope);

            if (model.MaxWeight_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(paczkomatySettings, x => x.MaxWeight, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(paczkomatySettings, x => x.MaxWeight, storeScope);

            //now clear settings cache
            _settingService.ClearCache();

            SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));
            return Configure();
        }

        [HttpPost, ActionName("Configure")]
        [ChildActionOnly]
        [FormValueRequired("refresh")]
        public ActionResult Refresh(ConfigurationModel model)
        {
            var document = XDocument.Load("http://api.paczkomaty.pl/?do=listmachines_xml");

            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var paczkomatySettings = _settingService.LoadSetting<PaczkomatySettings>(storeScope);
            paczkomatySettings.CachedPointsList = document.ToString();
            _settingService.SaveSetting(paczkomatySettings, x => x.CachedPointsList, storeScope);

            SuccessNotification(_localizationService.GetResource("Lista paczkomatów została odświeżona"));
            return Configure();
        }

        [ChildActionOnly]
        public ActionResult PublicInfo(string widgetZone, object additionalData = null)
        {
            var paczkomatySettings = _settingService.LoadSetting<PaczkomatySettings>(_storeContext.CurrentStore.Id);

            var model = new PublicInfoModel();

            return View("~/Plugins/PickUpPoints.Paczkomaty/Views/PickUpPointsPaczkomaty/PublicInfo.cshtml", model);
        }
    }
}