//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace BackFillForReports
{
    using System;
    using System.Collections.Generic;
    
    public partial class BitShift_FirstData_StoreSetting
    {
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
        public bool AdditionalFeePercentage { get; set; }
        public decimal AdditionalFee { get; set; }
        public string PaymentPageID { get; set; }
        public string TransactionKey { get; set; }
        public string ResponseKey { get; set; }
    }
}
