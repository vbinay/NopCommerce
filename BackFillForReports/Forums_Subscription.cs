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
    
    public partial class Forums_Subscription
    {
        public int Id { get; set; }
        public System.Guid SubscriptionGuid { get; set; }
        public int CustomerId { get; set; }
        public int ForumId { get; set; }
        public int TopicId { get; set; }
        public System.DateTime CreatedOnUtc { get; set; }
    
        public virtual Customer Customer { get; set; }
    }
}
