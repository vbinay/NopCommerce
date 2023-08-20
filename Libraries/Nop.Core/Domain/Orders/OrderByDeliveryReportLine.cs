using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using System;
namespace Nop.Core.Domain.Orders
{
    public partial class OrderByDeliveryReportLine
    {
        public virtual string Unit { get; set; }
        public virtual string Category { get; set; }
        public virtual string GLCode { get; set; }
        public virtual int OrderId { get; set; }
        public virtual DateTime? OrdItmDeliveryDate { get; set; }
        public virtual decimal OrderSubTotalExclTax { get; set; }
        public virtual decimal OrderTax { get; set; }
        public virtual decimal OrderSubtotalInclTax { get; set; }
        public virtual string ExtKey { get; set; }
    }
}
