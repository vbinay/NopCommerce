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
    
    public partial class CardAccessRecord
    {
        public int Id { get; set; }
        public Nullable<int> CustomerId { get; set; }
        public string CardHolderId { get; set; }
        public string PlanId { get; set; }
        public string IssuerId { get; set; }
        public string ApplicationID { get; set; }
        public string Type { get; set; }
        public string AccountId { get; set; }
        public string Balance { get; set; }
        public string Status { get; set; }
        public string HashUsed { get; set; }
        public Nullable<System.DateTime> CreatedOnUtc { get; set; }
        public string ReferenceID { get; set; }
        public string AppliedAmount { get; set; }
        public string Hash { get; set; }
        public string Error { get; set; }
    }
}
