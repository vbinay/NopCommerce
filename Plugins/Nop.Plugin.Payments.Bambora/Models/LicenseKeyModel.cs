using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Payments.Bambora.Models
{
    public class StoreLicenseKeyModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Bambora.Plugin.LicenseKey.Fields.LicenseKey")]
        public string LicenseKey { get; set; }
        public string Type { get; set; }

        public string Host { get; set; }
    }
}
