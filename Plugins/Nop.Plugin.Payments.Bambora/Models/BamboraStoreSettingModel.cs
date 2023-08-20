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
   public class BamboraStoreSettingModel: BaseNopModel
    {
        [NopResourceDisplayName("Bambora.Plugin.Fields.StoreID")]
        public int StoreID { get; set; }

        [NopResourceDisplayName("Bambora.Plugin.Fields.StoreID")]
        public string StoreName { get; set; }

        [NopResourceDisplayName("Bambora.Plugin.Fields.UseSandbox")]
        public bool UseSandbox { get; set; }

        public int TransactModeId { get; set; }
        [NopResourceDisplayName("Bambora.Plugin.Fields.TransactModeValues")]
        public IList<SelectListItem> TransactModeValues { get; set; }

        [NopResourceDisplayName("Bambora.Plugin.Fields.HMAC")]
        public string HMAC { get; set; }

        [NopResourceDisplayName("Bambora.Plugin.Fields.GatewayID")]
        public string GatewayID { get; set; }

        [NopResourceDisplayName("Bambora.Plugin.Fields.Password")]
        public string Password { get; set; }

        [NopResourceDisplayName("Bambora.Plugin.Fields.KeyID")]
        public string KeyID { get; set; }

        [NopResourceDisplayName("Bambora.Plugin.Fields.PaymentPageID")]
        public string PaymentPageID { get; set; }
        [NopResourceDisplayName("Bambora.Plugin.Fields.TransactionKey")]
        public string TransactionKey { get; set; }
        [NopResourceDisplayName("Bambora.Plugin.Fields.ResponseKey")]
        public string ResponseKey { get; set; }

        [NopResourceDisplayName("Bambora.Plugin.Fields.AdditionalFee")]
        public decimal AdditionalFee { get; set; }

        [NopResourceDisplayName("Bambora.Plugin.Fields.AdditionalFeePercentage")]
        public bool AdditionalFeePercentage { get; set; }

        [NopResourceDisplayName("Bambora.Plugin.Fields.EnableRecurringPayments")]
        public bool EnableRecurringPayments { get; set; }

        [NopResourceDisplayName("Bambora.Plugin.Fields.EnableCardSaving")]
        public bool EnableCardSaving { get; set; }

        [NopResourceDisplayName("Bambora.Plugin.Fields.EnablePurchaseOrderNumber")]
        public bool EnablePurchaseOrderNumber { get; set; }

        public bool UseDefaultSettings { get; set; }

        public string MerchantId { get; set; }
        public string PaymentApiKey { get; set; }
        public string ReportingApiKey { get; set; }
        public string ProfilesApiKey { get; set; }
        public string HashKey { get; set; }
    }
}
