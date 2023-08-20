using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Tax.Vertex.Domain
{
    public class ProductTax : BaseEntity
    {
        /// <summary>
        /// Gets or sets the store identifier
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// Gets or sets the zip
        /// </summary>
        public string SessionId { get; set; }

        public int GlCodeId { get; set; }

        public int OrderId { get; set; }

        /// <summary>
        /// Gets or sets the percentage
        /// </summary>
        public decimal Tax { get; set; }

        public string TaxCode { get; set; }
    }
}
