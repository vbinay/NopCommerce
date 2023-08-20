using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CovidRefundProcessor
{
    public class BulkRefund
    {
        // Identifiers
        public int BulkRefundId { get; set; }

        public int OrderId { get; set; }
        public int OrderItemId { get; set; }

        // Bits
        public bool IsVertex { get; set; }
        public bool ProcessVertex { get; set; }
        public bool IsProcessed { get; set; }

        // Total
        public decimal TotalRefund { get; set; }

        // GL Code data 
        public string GLCode1 { get; set; }
        public string GLCode2 { get; set; }
        public string GLCode3 { get; set; }        
        public decimal GlAmount1 { get; set; }
        public decimal? GlAmount2 { get; set; }
        public decimal? GlAmount3 { get; set; }
        public int Glcodeid1 { get; set; }
        public int? Glcodeid2 { get; set; }
        public int? Glcodeid3 { get; set; }
        public decimal? RefundedTaxAmount1 { get; set; }
        public decimal? RefundedTaxAmount2 { get; set; }
        public decimal? RefundedTaxAmount3 { get; set; }
        public string DeliveryGLName { get; set; }
        public decimal? DeliveryPickupAmount { get; set; }
        public decimal? DeliveryTax { get; set; }

        // summary referential data        
        public DateTime CreatedDateUtc { get; set; }
        public DateTime? ProcessedDateUtc { get; set; }
        public string Errors { get; set; }        
    }
}
