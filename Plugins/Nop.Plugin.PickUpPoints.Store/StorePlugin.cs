using Nop.Core;
using Nop.Core.Plugins;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.PickUpPoint;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Routing;
using System.Xml.Linq;

namespace Nop.Plugin.PickUpPoints.Store
{
    /// <summary>
    /// PLugin
    /// </summary>
    public class StorePlugin : BasePlugin, IPickUpPointPlugin
    {
        private readonly IPictureService _pictureService;
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;
        private readonly StoreSettings _storeSettings;
        private readonly ILocalizationService _localizationService;

        public StorePlugin(IPictureService pictureService,
            ISettingService settingService, IWebHelper webHelper,
            StoreSettings storeSettings, ILocalizationService localizationService)
        {
            this._pictureService = pictureService;
            this._settingService = settingService;
            this._webHelper = webHelper;
            this._storeSettings = storeSettings;
            this._localizationService = localizationService;
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
            controllerName = "PickUpPointsstore";
            routeValues = new RouteValueDictionary { { "Namespaces", "Nop.Plugin.PickUpPoints.Store.Controllers" }, { "area", null } };
        }

        public decimal GetShippingFee(decimal? shippingWeight)
        {
            return _storeSettings.Fee;
        }

        public string GetPointDescription(string pointName)
        {
            return _storeSettings.Town + " " + _storeSettings.PostCode
                + ", " + _storeSettings.Street + " " + _storeSettings.BuildingNumber
                + ", " + _storeSettings.Description;
        }

        public IList<PickUpPointItemModel> GetPickUpPointsList(string postcode)
        {
            var point = new PickUpPointItemModel
            {
                Name = string.Empty,
                Description = _storeSettings.Description,
                BuildingNumber = _storeSettings.BuildingNumber,
                IsPaymentAvailable = false,
                Latitude = _storeSettings.Latitude,
                Longitude = _storeSettings.Longitude,
                PostCode = _storeSettings.PostCode,
                Street = _storeSettings.Street,
                Town = _storeSettings.Town
            };

            return new List<PickUpPointItemModel> { point };
        }

        public string GetDescription()
        {
            return _storeSettings.Description;
        }

        /// <summary>
        /// Install plugin
        /// </summary>
        public override void Install()
        {
            //pictures
            var sampleImagesPath = "";// _webHelper.MapPath("~/Plugins/PickUpPoints.Store/Content/store/sample-images/");

            //settings
            var settings = new StoreSettings
            {
            };
            _settingService.SaveSetting(settings);

            this.AddOrUpdatePluginLocaleResource("Plugins.PickUpPoints.Store.Street", "Street");
            this.AddOrUpdatePluginLocaleResource("Plugins.PickUpPoints.Store.Street.Hint", "Your shop street");
            this.AddOrUpdatePluginLocaleResource("Plugins.PickUpPoints.Store.BuildingNumber", "Building number");
            this.AddOrUpdatePluginLocaleResource("Plugins.PickUpPoints.Store.BuildingNumber.Hint", "Building number example 12/6");
            this.AddOrUpdatePluginLocaleResource("Plugins.PickUpPoints.Store.PostCode", "Postcode");
            this.AddOrUpdatePluginLocaleResource("Plugins.PickUpPoints.Store.PostCode.Hint", "Enter your shop postcode");
            this.AddOrUpdatePluginLocaleResource("Plugins.PickUpPoints.Store.Town", "Town");
            this.AddOrUpdatePluginLocaleResource("Plugins.PickUpPoints.Store.Town.Hint", "Enter your shop Town.");
            this.AddOrUpdatePluginLocaleResource("Plugins.PickUpPoints.Store.Latitude", "Latitude");
            this.AddOrUpdatePluginLocaleResource("Plugins.PickUpPoints.Store.Latitude.Hint", "Enter your shop decimal latitude.");
            this.AddOrUpdatePluginLocaleResource("Plugins.PickUpPoints.Store.Longitude", "Longitude");
            this.AddOrUpdatePluginLocaleResource("Plugins.PickUpPoints.Store.Longitude.Hint", "Enter your shop decimal longitude.");
            this.AddOrUpdatePluginLocaleResource("Plugins.PickUpPoints.Store.Description", "Description");
            this.AddOrUpdatePluginLocaleResource("Plugins.PickUpPoints.Store.Description.Hint", "Enter additional description for your shop.");
            this.AddOrUpdatePluginLocaleResource("Plugins.PickUpPoints.Store.Fee", "Fee");
            this.AddOrUpdatePluginLocaleResource("Plugins.PickUpPoints.Store.Fee.Hint", "Enter fee amount");

            base.Install();
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        public override void Uninstall()
        {
            //settings
            _settingService.DeleteSetting<StoreSettings>();

            //locales
            this.DeletePluginLocaleResource("Plugins.PickUpPoints.Store.Street");
            this.DeletePluginLocaleResource("Plugins.PickUpPoints.Store.Street.Hint");
            this.DeletePluginLocaleResource("Plugins.PickUpPoints.Store.BuildingNumber");
            this.DeletePluginLocaleResource("Plugins.PickUpPoints.Store.BuildingNumber.Hint");
            this.DeletePluginLocaleResource("Plugins.PickUpPoints.Store.PostCode");
            this.DeletePluginLocaleResource("Plugins.PickUpPoints.Store.PostCode.Hint");
            this.DeletePluginLocaleResource("Plugins.PickUpPoints.Store.Town");
            this.DeletePluginLocaleResource("Plugins.PickUpPoints.Store.Town.Hint");
            this.DeletePluginLocaleResource("Plugins.PickUpPoints.Store.Latitude");
            this.DeletePluginLocaleResource("Plugins.PickUpPoints.Store.Latitude.Hint");
            this.DeletePluginLocaleResource("Plugins.PickUpPoints.Store.Longitude");
            this.DeletePluginLocaleResource("Plugins.PickUpPoints.Store.Longitude.Hint");
            this.DeletePluginLocaleResource("Plugins.PickUpPoints.Store.Description");
            this.DeletePluginLocaleResource("Plugins.PickUpPoints.Store.Description.Hint");
            this.DeletePluginLocaleResource("Plugins.PickUpPoints.Store.Fee");
            this.DeletePluginLocaleResource("Plugins.PickUpPoints.Store.Fee.Hint");

            base.Uninstall();
        }
    }
}