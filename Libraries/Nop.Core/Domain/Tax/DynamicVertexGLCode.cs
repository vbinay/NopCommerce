using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Core.Domain.Tax
{
    public class DynamicVertexGLCode
    {
        public DynamicVertexGLCode(string productId, string name, string description, decimal total, string taxCode)
        {
            this.productId = productId;
            this.GlCode = name;
            this.Total = total;
            this.Description = description;
            this.taxCode = taxCode;
        }
        public String Description { get; set; }
        public string GlCode
        {
            get;
            set;
        }

        public decimal Total { get; set; }

        public string productId { get; set; }

        public string taxCode { get; set; }
    }
}
