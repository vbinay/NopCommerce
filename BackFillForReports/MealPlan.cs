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
    
    public partial class MealPlan
    {
        public int Id { get; set; }
        public int PurchasedWithOrderItemId { get; set; }
        public string RecipientName { get; set; }
        public string RecipientAddress { get; set; }
        public string RecipientPhone { get; set; }
        public string RecipientEmail { get; set; }
        public string RecipientAcctNum { get; set; }
        public bool IsProcessed { get; set; }
        public System.DateTime CreatedOnUtc { get; set; }
        public Nullable<int> CardAccessRecordId { get; set; }
        public Nullable<System.DateTime> ProcessedDate { get; set; }
    
        public virtual OrderItem OrderItem { get; set; }
    }
}
