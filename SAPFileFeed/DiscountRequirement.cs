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
    
    public partial class DiscountRequirement
    {
        public int Id { get; set; }
        public int DiscountId { get; set; }
        public string DiscountRequirementRuleSystemName { get; set; }
    
        public virtual Discount Discount { get; set; }
    }
}
