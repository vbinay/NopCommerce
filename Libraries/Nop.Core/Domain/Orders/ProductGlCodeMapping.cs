using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Core.Domain.Orders
{
    public partial class ProductGlCodeMapping : BaseEntity
    {
        public int? OrderId { get; set; }
        public int? ProductID { get; set; }

        public String Glcode { get; set; }

        public decimal? Amount { get; set; }

        public int? GlStatusType { get; set; }
         
        public string ProductName { get; set; }
    }
}
