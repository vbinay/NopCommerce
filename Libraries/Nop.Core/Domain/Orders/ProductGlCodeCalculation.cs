using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Core.Domain.Orders
{
   public class ProductGlCodeCalculation: BaseEntity
    {
        public int StoreId { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int GLCodeId { get; set; }
        public string GLCodeName { get; set; }
        public decimal? Amount { get; set; }
        public bool Processed { get; set; }

        public int GlStatusType { get; set; }
    }
}
