using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Routing;
using Newtonsoft.Json;
using Nop.Core.Caching;
using Nop.Core.Plugins;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Tax;

namespace Nop.Plugin.Reports.ReportingServices
{
    /// <summary>
    /// ReportingServices provider
    /// </summary>
    public class ReportingServicesProvider : BasePlugin
    {
        private const string REPSERVICES_KEY = "Nop.ReportingServices.id-{0}";
        private const string REPSERVICES_CLIENT = "nopCommerce ReportingServices provider 1.0";

        private readonly ReportingServicesSettings _ReportingServicesSettings;
        private readonly ICacheManager _cacheManager;
        private readonly ILogger _logger;
        private readonly ISettingService _settingService;        

        public ReportingServicesProvider(ReportingServicesSettings ReportingServicesSettings,
            ICacheManager cacheManager,
            ILogger logger,
            ISettingService settingService)
        {
            this._ReportingServicesSettings = ReportingServicesSettings;
            this._cacheManager = cacheManager;
            this._logger = logger;
            this._settingService = settingService;            
        }
        
        public ReportingServicesResult GetSales(ReportingServicesRequest repServiceRequest)
        {
            repServiceRequest.start = "01-01-2017";
            repServiceRequest.end = "01-02-2017";

            var streamRequest = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(repServiceRequest));

            var serviceUrl = _ReportingServicesSettings.SandboxEnvironment ? "http://localhost" : "http://reportingservices.sodexomyway.net";
            var request = (HttpWebRequest)WebRequest.Create(string.Format("{0}/Results", serviceUrl));
            request.Method = "POST";
            request.ContentType = "application/json";
            request.ContentLength = streamRequest.Length;
            using (var stream = request.GetRequestStream())
            {
                stream.Write(streamRequest, 0, streamRequest.Length);
            }

            var repServicesResult = new ReportingServicesResult();
            try
            {
                var httpResponse = (HttpWebResponse)request.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var stringResult = streamReader.ReadToEnd();
                    repServicesResult = JsonConvert.DeserializeObject<ReportingServicesResult>(stringResult);
                }
            }
            catch (WebException ex)
            {
                var httpResponse = (HttpWebResponse)ex.Response;
            }

            return repServicesResult;
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
            controllerName = "ReportingServices";
            routeValues = new RouteValueDictionary { { "Namespaces", "Nop.Plugin.Reports.ReportingServices.Controllers" }, { "area", null } };
        }

        /// <summary>
        /// Install plugin
        /// </summary>
        public override void Install()
        {
            //settings
            var settings = new ReportingServicesSettings
            {
                SandboxEnvironment = false
            };
            _settingService.SaveSetting(settings);

            //locales
            this.AddOrUpdatePluginLocaleResource("Plugins.Reports.ReportingServices.SandboxEnvironment", "Sandbox environment");
            this.AddOrUpdatePluginLocaleResource("Plugins.Reports.ReportingServices.SandboxEnvironment.Hint", "Use development account (for testing).");

            base.Install();
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        public override void Uninstall()
        {
            //settings
            _settingService.DeleteSetting<ReportingServicesSettings>();

            //locales
            this.DeletePluginLocaleResource("Plugins.Reports.ReportingServices.SandboxEnvironment");
            this.DeletePluginLocaleResource("Plugins.Reports.ReportingServices.SandboxEnvironment.Hint");

            base.Uninstall();
        }
    }
}
