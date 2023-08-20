using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Nop.Core;
using Nop.Plugin.Reports.ReportingServices.Models;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Tax;
using Nop.Web.Framework.Controllers;

namespace Nop.Plugin.Reports.ReportingServices.Controllers
{
    [AdminAuthorize]
    public class ReportingServicesController : BasePluginController
    {
        private const string PATH_VIEW = "~/Plugins/Reports.ReportingServices/Views/ReportingServices/Configure.cshtml";

        private readonly ReportingServicesSettings _ReportingServicesSettings;
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;

        public ReportingServicesController(ReportingServicesSettings ReportingServicesSettings,
            ISettingService settingService,
            ILocalizationService localizationService)
        {
            this._ReportingServicesSettings = ReportingServicesSettings;
            this._settingService = settingService;
            this._localizationService = localizationService;
        }

        [ChildActionOnly]
        public ActionResult Configure()
        {
            var model = new ReportingServicesModel
            {
                SandboxEnvironment = _ReportingServicesSettings.SandboxEnvironment
            };

            return View(PATH_VIEW, model);
        }

        [HttpPost, ActionName("Configure")]
        [FormValueRequired("save")]
        public ActionResult Configure(ReportingServicesModel model)
        {
            if (!ModelState.IsValid)
                return Configure();

            _ReportingServicesSettings.SandboxEnvironment = model.SandboxEnvironment;
            _settingService.SaveSetting(_ReportingServicesSettings);
            SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return View(PATH_VIEW, model);
        }
    }
}