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

namespace Nop.Plugin.PickUpPoints.Paczkomaty
{
    /// <summary>
    /// PLugin
    /// </summary>
    public class PaczkomatyPlugin : BasePlugin, IPickUpPointPlugin
    {
        private readonly IPictureService _pictureService;
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;
        private readonly PaczkomatySettings _paczkomatySettings;
        private readonly ILocalizationService _localizationService;

        public PaczkomatyPlugin(IPictureService pictureService,
            ISettingService settingService, IWebHelper webHelper,
            PaczkomatySettings paczkomatySettings, ILocalizationService localizationService)
        {
            this._pictureService = pictureService;
            this._settingService = settingService;
            this._webHelper = webHelper;
            this._paczkomatySettings = paczkomatySettings;
            this._localizationService = localizationService;
        }

        /// <summary>
        /// Gets widget zones where this widget should be rendered
        /// </summary>
        /// <returns>Widget zones</returns>
        public IList<string> GetWidgetZones()
        {
            return new List<string> { "home_page_top" };
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
            controllerName = "PickUpPointspaczkomaty";
            routeValues = new RouteValueDictionary { { "Namespaces", "Nop.Plugin.PickUpPoints.paczkomaty.Controllers" }, { "area", null } };
        }

        /// <summary>
        /// Gets a route for displaying widget
        /// </summary>
        /// <param name="widgetZone">Widget zone where it's displayed</param>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetDisplayWidgetRoute(string widgetZone, out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "PublicInfo";
            controllerName = "PickUpPointspaczkomaty";
            routeValues = new RouteValueDictionary
            {
                {"Namespaces", "Nop.Plugin.PickUpPoints.paczkomaty.Controllers"},
                {"area", null},
                {"widgetZone", widgetZone}
            };
        }

        public decimal GetShippingFee(decimal? shippingWeight)
        {
            // todo dive parcel
            if (shippingWeight != null)
            {
                return _paczkomatySettings.Fee;
            }
            else
            {
                return _paczkomatySettings.Fee;
            }
        }

        public string GetPointDescription(string pointName)
        {
            var pickUpPointsList = GetPickUpPointsList(string.Empty);
            var selectedPickUpPoint = pickUpPointsList.SingleOrDefault(m => m.Name == pointName);
            if (selectedPickUpPoint == null)
            {
                return string.Empty;
            }
            else
            {
                return selectedPickUpPoint.Town + " " + selectedPickUpPoint.PostCode
                    + ", " + selectedPickUpPoint.Street + " " + selectedPickUpPoint.BuildingNumber
                    + ", " + selectedPickUpPoint.Description
                    + " " + (selectedPickUpPoint.IsPaymentAvailable ? "(pobranie)" : string.Empty);
            }
        }

        public IList<PickUpPointItemModel> GetPickUpPointsList(string postcode)
        {
            XDocument allMachines = null;
            XDocument nearestMachines = null;

            if (string.IsNullOrEmpty(_paczkomatySettings.CachedPointsList))
            {
                allMachines = XDocument.Load("http://api.paczkomaty.pl/?do=listmachines_xml");
            }
            else
            {
                allMachines = XDocument.Load(_paczkomatySettings.CachedPointsList);
            }

            if (_paczkomatySettings.ShowNearestThreePoints && !string.IsNullOrEmpty(postcode))
            {
                nearestMachines = XDocument.Load("http://api.paczkomaty.pl/?do=findnearestmachines&postcode=" + postcode);
            }

            List<PickUpPointItemModel> allMachinesList = GetAllMachineList(allMachines);
            List<PickUpPointItemModel> nearestMachinesList = GetNearestMachineList(nearestMachines);
            nearestMachinesList.AddRange(allMachinesList);
            return nearestMachinesList;
        }

        public string GetDescription()
        {
            return _localizationService.GetResource("Plugins.PickUpPoints.Paczkomaty.Description");
        }

        private static List<PickUpPointItemModel> GetNearestMachineList(XDocument allMachines)
        {
            if (allMachines != null)
            {
                var query = from paczkomaty in allMachines.Descendants("machine")
                            select new PickUpPointItemModel
                            {
                                Name = paczkomaty.Element("name").Value,
                                Latitude = paczkomaty.Element("latitude").Value,
                                Longitude = paczkomaty.Element("longitude").Value,
                                BuildingNumber = paczkomaty.Element("buildingnumber").Value,
                                Street = paczkomaty.Element("street").Value,
                                Town = paczkomaty.Element("town").Value,
                                Description = paczkomaty.Element("locationdescription").Value,
                                IsPaymentAvailable = paczkomaty.Element("paymenttype").Value != "0" ? true : false,
                                PostCode = paczkomaty.Element("postcode").Value
                            };
                return query.ToList();
            }
            else
            {
                return new List<PickUpPointItemModel>();
            }
        }

        private static List<PickUpPointItemModel> GetAllMachineList(XDocument allMachines)
        {
            var query = from paczkomaty in allMachines.Descendants("machine")
                        select new PickUpPointItemModel
                        {
                            Name = paczkomaty.Element("name").Value,
                            Latitude = paczkomaty.Element("latitude").Value,
                            Longitude = paczkomaty.Element("longitude").Value,
                            BuildingNumber = paczkomaty.Element("buildingnumber").Value,
                            Street = paczkomaty.Element("street").Value,
                            Town = paczkomaty.Element("town").Value,
                            Description = paczkomaty.Element("locationdescription").Value,
                            IsPaymentAvailable = paczkomaty.Element("paymentavailable").Value == "t" ? true : false,
                            PostCode = paczkomaty.Element("postcode").Value,
                            Province = paczkomaty.Element("province").Value
                        };
            return query.ToList();
        }

        /// <summary>
        /// Install plugin
        /// </summary>
        public override void Install()
        {
            //pictures
            var sampleImagesPath = _webHelper.MapPath("~/Plugins/PickUpPoints.paczkomaty/Content/paczkomaty/sample-images/");

            //settings
            var settings = new PaczkomatySettings
            {
            };
            _settingService.SaveSetting(settings);

            this.AddOrUpdatePluginLocaleResource("Plugins.PickUpPoints.Paczkomaty.ShowNearestThreePoints", "Show nearest 3 points");
            this.AddOrUpdatePluginLocaleResource("Plugins.PickUpPoints.Paczkomaty.ShowNearestThreePoints.Hint", "Use this featre to show 3 nearest pickup point to user address");
            this.AddOrUpdatePluginLocaleResource("Plugins.PickUpPoints.Paczkomaty.Fee", "Fee");
            this.AddOrUpdatePluginLocaleResource("Plugins.PickUpPoints.Paczkomaty.Fee.Hint", "Enter fee amount per parcel");
            this.AddOrUpdatePluginLocaleResource("Plugins.PickUpPoints.Paczkomaty.MaxWeight", "Max weight");
            this.AddOrUpdatePluginLocaleResource("Plugins.PickUpPoints.Paczkomaty.MaxWeight.Hint", "Enter max weight per parcel.");
            this.AddOrUpdatePluginLocaleResource("Plugins.PickUpPoints.Paczkomaty.Description", "Us³uga Paczkomaty InPost - odbiór przesy³ki 24 godziny na dobê przez 7 dni w tygodniu. SprawdŸ szczegó³y: www.paczkomaty.pl");
            this.AddOrUpdatePluginLocaleResource("Plugins.PickUpPoints.Paczkomaty.RefreshButton", "Refresh paczkomaty list");

            base.Install();
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        public override void Uninstall()
        {
            //settings
            _settingService.DeleteSetting<PaczkomatySettings>();

            //locales
            this.DeletePluginLocaleResource("Plugins.PickUpPoints.Paczkomaty.ShowNearestThreePoints");
            this.DeletePluginLocaleResource("Plugins.PickUpPoints.Paczkomaty.ShowNearestThreePoints.Hint");
            this.DeletePluginLocaleResource("Plugins.PickUpPoints.Paczkomaty.Fee");
            this.DeletePluginLocaleResource("Plugins.PickUpPoints.Paczkomaty.Fee.Hint");
            this.DeletePluginLocaleResource("Plugins.PickUpPoints.Paczkomaty.MaxWeight");
            this.DeletePluginLocaleResource("Plugins.PickUpPoints.Paczkomaty.MaxWeight.Hint");
            this.DeletePluginLocaleResource("Plugins.PickUpPoints.Paczkomaty.Description");
            this.DeletePluginLocaleResource("Plugins.PickUpPoints.Paczkomaty.RefreshButton");

            base.Uninstall();
        }
    }
}