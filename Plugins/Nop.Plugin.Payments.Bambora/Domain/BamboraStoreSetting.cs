using Nop.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Payments.Bambora.Domain
{
   public class BamboraStoreSetting: BaseEntity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int StoreId { get; set; }

        public bool UseSandbox { get; set; }
        public int TransactionMode { get; set; }
        public string HMAC { get; set; }
        public string GatewayID { get; set; }
        public string Password { get; set; }
        public string KeyID { get; set; }
        public bool EnableRecurringPayments { get; set; }
        public bool EnableCardSaving { get; set; }
        public bool EnablePurchaseOrderNumber { get; set; }

        public string PaymentPageID { get; set; }
        public string TransactionKey { get; set; }
        public string ResponseKey { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to "additional fee" is specified as percentage. true - percentage, false - fixed value.
        /// </summary>
        public bool AdditionalFeePercentage { get; set; }
        /// <summary>
        /// Additional fee
        /// </summary>
        public decimal AdditionalFee { get; set; }

        public string MerchantId { get; set; }
        public string PaymentApiKey { get; set; }
        public string ReportingApiKey { get; set; }
        public string ProfilesApiKey { get; set; }
        public string HashKey { get; set; }
    }
}
