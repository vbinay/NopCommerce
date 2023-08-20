using System.Web.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Plugin.Widgets.AdminSecurity.Infrastructure.Cache;
using Nop.Plugin.Widgets.AdminSecurity.Models;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Stores;
using Nop.Web.Framework.Controllers;
using System.Collections.Generic;
using Nop.Core.Domain.Stores;
using System.Linq;
using Nop.Core.Domain.Customers;
using Nop.Services.Customers;

namespace Nop.Plugin.Widgets.AdminSecurity.Controllers
{
    public class WidgetsAdminSecurityController : BasePluginController
    {
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IStoreService _storeService;
        private readonly ISettingService _settingService;
        private readonly ICacheManager _cacheManager;
        private readonly ILocalizationService _localizationService;
        private readonly ICustomerService _customerService;

        public WidgetsAdminSecurityController(IWorkContext workContext,
            IStoreContext storeContext,
            IStoreService storeService, 
            ISettingService settingService,
            ICacheManager cacheManager,
            ILocalizationService localizationService,
            ICustomerService customerService)
        {
            this._workContext = workContext;
            this._storeContext = storeContext;
            this._storeService = storeService;
            this._settingService = settingService;
            this._cacheManager = cacheManager;
            this._localizationService = localizationService;
            this._customerService = customerService;
        }

        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure()
        {
            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);

            var model = new ConfigurationModel();

            return View("~/Plugins/Widgets.AdminSecurity/Views/WidgetsAdminSecurity/Configure.cshtml", model);
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

            if(!_workContext.CurrentCustomer.IsSystemGlobalAdmin())
            {
                model.hasAccess = _workContext.CurrentCustomer.IsStoreAdmin();

                if (model.hasAccess)
                {
                    model.hasAccess = (_customerService.GetCustomerByIdAndStoreId(_workContext.CurrentCustomer.Id, _storeContext.CurrentStore.Id) != null);
                }
            }
            else
            {
                model.hasAccess = true;
            }

            return View("~/Plugins/Widgets.AdminSecurity/Views/WidgetsAdminSecurity/PublicInfo.cshtml", model);
        }
    }
}