using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Nop.Plugin.Payments.Bambora.Models
{
    public class ConfigurationModel : BaseNopModel
    {
        [NopResourceDisplayName("Bambora.Plugin.Fields.SandboxURL")]
        public string SandboxURL { get; set; }

        [NopResourceDisplayName("Bambora.Plugin.Fields.ProductionURL")]
        public string ProductionURL { get; set; }

        [NopResourceDisplayName("Bambora.Plugin.Fields.MerchantId")] ///For Bambora 
        public string MerchantId { get; set; }                       ///For Bambora 
        public bool MerchantId_OverrideForStore { get; set; }        ///For Bambora 
                                                                     ///For Bambora 
        [NopResourceDisplayName("Bambora.Plugin.Fields.HashKey")]    ///For Bambora 
        public string HashKey { get; set; }                          ///For Bambora
        public bool HashKey_OverrideForStore { get; set; }           ///For Bambora

        [NopResourceDisplayName("Bambora.Plugin.Fields.EncryptedSecureKey")]    ///For Bambora 
        public string EncryptedSecureKey { get; set; }                          ///For Bambora


        [NopResourceDisplayName("Bambora.Plugin.Fields.LicenseKey")]
        public IList<StoreLicenseKeyModel> LicenseKeys { get; set; }

        public IList<SelectListItem> Stores { get; set; }

        public bool SavedSuccessfully { get; set; }

        public string SaveMessage { get; set; }

        public string ApprovedRedirectUrl { get; set; }
        public string RejectedRedirectUrl { get; set; }
    }
}
