//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SAPFileFeed
{
    using System;
    using System.Collections.Generic;
    
    public partial class Donation
    {
        public int Id { get; set; }
        public int PurchasedWithOrderItemId { get; set; }
        public decimal Amount { get; set; }
        public string DonorFirstName { get; set; }
        public string DonorLastName { get; set; }
        public string DonorAddress { get; set; }
        public string DonorAddress2 { get; set; }
        public string DonorCity { get; set; }
        public string DonorState { get; set; }
        public string DonorZip { get; set; }
        public string DonorPhone { get; set; }
        public string DonorCountry { get; set; }
        public string Comments { get; set; }
        public string NotificationEmail { get; set; }
        public string OnBehalfOfFullName { get; set; }
        public bool IsProcessed { get; set; }
        public bool IncludeGiftAmount { get; set; }
        public System.DateTime CreatedOnUtc { get; set; }
        public string DonorCompany { get; set; }
    
        public virtual OrderItem OrderItem { get; set; }
    }
}
