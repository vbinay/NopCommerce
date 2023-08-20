using System.Web.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Plugin.Widgets.StoreSelector.Infrastructure.Cache;
using Nop.Plugin.Widgets.StoreSelector.Models;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Stores;
using Nop.Web.Framework.Controllers;
using System.Collections.Generic;
using Nop.Core.Domain.Stores;
using System.Linq;

namespace Nop.Plugin.Widgets.StoreSelector.Controllers
{
    public class WidgetsStoreSelectorController : BasePluginController
    {
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IStoreService _storeService;
        private readonly IPictureService _pictureService;
        private readonly ISettingService _settingService;
        private readonly ICacheManager _cacheManager;
        private readonly ILocalizationService _localizationService;

        public WidgetsStoreSelectorController(IWorkContext workContext,
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

        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure()
        {
            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);

            var model = new ConfigurationModel();

            return View("~/Plugins/Widgets.StoreSelector/Views/WidgetsStoreSelector/Configure.cshtml", model);
        }

        [HttpPost]
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure(ConfigurationModel model)
        {
            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            
            //now clear settings cache
            _settingService.ClearCache();

            SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));
            return Configure();
        }

        [ChildActionOnly]
        public ActionResult PublicInfo(string widgetZone, object additionalData = null)
        {
            var model = new PublicInfoModel();

            model.availableStores = _storeService.GetAllStores(_workContext.CurrentCustomer).OrderBy(s => s.Name).ToList();
            model.currentStore = _storeContext.CurrentStore;
            model.currentURLTemplate = HttpContext.Request.Url.AbsoluteUri.Replace(HttpContext.Request.Url.Host, "{0}");

            return View("~/Plugins/Widgets.StoreSelector/Views/WidgetsStoreSelector/PublicInfo.cshtml", model);
        }
    }
}