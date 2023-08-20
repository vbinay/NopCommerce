using System.Web.Mvc;
using System.Collections.Generic;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;

namespace BitShift.Plugin.Payments.FirstData.Models
{
    public class ConfigurationModel : BaseNopModel
    {
        [NopResourceDisplayName("BitShift.Plugin.FirstData.Fields.SandboxURL")]
        public string SandboxURL { get; set; }

        [NopResourceDisplayName("BitShift.Plugin.FirstData.Fields.ProductionURL")]
        public string ProductionURL { get; set; }

        [NopResourceDisplayName("BitShift.Plugin.FirstData.Fields.LicenseKey")]
        public IList<StoreLicenseKeyModel> LicenseKeys { get; set; }

        public IList<SelectListItem> Stores { get; set; }

        public bool SavedSuccessfully { get; set; }

        public string SaveMessage { get; set; }
    }
}