using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Core.Domain.Orders
{
    public partial class ShoppingCartBundleProductItem : BaseEntity
    {
        public int ShoppingCartItemId { get; set; }
        /// <summary>
        /// Parent Product Id
        /// </summary>
        public int ParentProductId { get; set; }
        /// <summary>
        /// Associated Product Id
        /// </summary>
        public int AssociatedProductId { get; set; }
        /// <summary>
        /// Quantity
        /// </summary>
        public int Quantity { get; set; }
        /// <summary>
        /// Price
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Associated Product Name
        /// </summary>
        public string AssociatedProductName { get; set; }

        /// <summary>
        /// Associated Product TaxCategoryId
        /// </summary>
        public int AssociatedProductTaxCategoryId { get; set; }

        public int OrderId { get; set; }

        public int OrderItemId { get; set; }

    }
}
