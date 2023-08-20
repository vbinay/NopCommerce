using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Nop.Core.Domain.Catalog
{
    public class CardAccessRecord : BaseEntity
    {
        public int CustomerId { get; set; }
        public string CardHolderID { get; set; }
        public string PlanId { get; set; }
        public string Type { get; set; }
        public string AccountId { get; set; }
        public string IssuerId { get; set; }
        public string ApplicationID { get; set; }
        public string Balance { get; set; }
        public string AppliedAmount { get; set; }
        public string Status { get; set; }
        public string Hash { get; set; }
        public DateTime CreatedOnUTC { get; set; }
        public string Error { get; set; }
    }
}