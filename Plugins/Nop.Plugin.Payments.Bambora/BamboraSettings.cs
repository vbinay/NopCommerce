using Nop.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Payments.Bambora
{
    public class BamboraSettings: ISettings
    {
        public string SandboxURL { get; set; }
        public string ProductionURL { get; set; }


        public string EncryptedSecureKey { get; set; }
        public string ApprovedRedirectUrl { get; set; }
        public string RejectedRedirectUrl { get; set; }
    }
}
