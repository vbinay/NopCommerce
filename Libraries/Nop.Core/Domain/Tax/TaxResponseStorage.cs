using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Core.Domain.Tax
{
    public partial class TaxResponseStorage : BaseEntity
    {
        public int CustomerID { get; set; }
        public int? OrderID { get; set; }
        public string ProductID { get; set; }
        public int TypeOfCall { get; set; }
        public string XMlResponse { get; set; }
        public DateTime? AddedDate { get; set; }
    }

    public class ProductTax : BaseEntity
    {
        
        public int ProductId { get; set; }

        public string SessionId { get; set; }

        public int GlCodeId { get; set; }

        public int OrderId { get; set; }

        public decimal Tax { get; set; }

        public string TaxCode { get; set; }
    }


}
