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
    
    public partial class Product_GLCode_Calc
    {
        public Nullable<int> StoreId { get; set; }
        public Nullable<int> OrderId { get; set; }
        public Nullable<int> OrderItemId { get; set; }
        public Nullable<int> ProductId { get; set; }
        public Nullable<int> GlCodeId { get; set; }
        public Nullable<decimal> Amount { get; set; }
        public bool Processed { get; set; }
        public int Id { get; set; }
        public Nullable<int> GlStatusType { get; set; }
        public string GLCodeName { get; set; }
    }
}
