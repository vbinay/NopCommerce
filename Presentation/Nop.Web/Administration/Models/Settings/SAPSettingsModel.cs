using System.Collections.Generic;
using System.Web.Mvc;
using Nop.Admin.Models.Common;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;

namespace Nop.Admin.Models.Settings
{
    public partial class SAPSettingsModel : BaseNopModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        //[NopResourceDisplayName("Plugins.SAP.Fields.AdditionalFeePercentage")]
        public SelectList Protocols { get; set; }
        public string Protocol { get; set; }

        public string Hostname { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public int PortNumber { get; set; }

        public string SshHostKeyFingerprint { get; set; }

        public string localPath { get; set; }

        public string remotePath { get; set; }

        public bool remove { get; set; }

        public string TestOutput { get; set; }
    }
}