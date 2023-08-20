using System.Web.Mvc;
using System.Collections.Generic;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;

namespace BitShift.Plugin.Payments.FirstData.Models
{
    public class StoreLicenseKeyModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("BitShift.Plugin.FirstData.LicenseKey.Fields.LicenseKey")]
        public string LicenseKey { get; set; }
        public string Type { get; set; }

        public string Host { get; set; }
    }
}