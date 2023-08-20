using Nop.Services.Configuration;
using Nop.Web.Framework.Controllers;
using panoRazzi.RestService.Models;
using System.Web.Mvc;

namespace panoRazzi.RestService.Controllers
{
    [AdminAuthorize]
    public class AdminController : BasePluginController
    {
        private readonly ISettingService _settingService;
        private readonly RestServiceSettings _settings;

        public AdminController(
            ISettingService settingService,
            RestServiceSettings settings)
        {
            _settingService = settingService;
            _settings = settings;
        }

        public ActionResult Configure()
        {
            var model = new ConfigureModel()
            {
                ApiToken = _settings.ApiToken
            };

            return View("~/Plugins/panoRazzi.RestService/Views/Configure.cshtml", model);
        }

        [HttpPost]
        public ActionResult Configure(ConfigureModel model)
        {
            _settings.ApiToken = model.ApiToken;
            
            _settingService.SaveSetting(_settings);
            SuccessNotification("Settings saved..");

            return View("~/Plugins/panoRazzi.RestService/Views/Configure.cshtml", model);
        }
    }
}