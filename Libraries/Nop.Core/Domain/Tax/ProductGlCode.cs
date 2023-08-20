using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
namespace Nop.Core.Domain.Tax
{
    /// <summary>
    /// Represents a product category mapping
    /// </summary>
    public partial class ProductGlCode : BaseEntity
    {
        /// <summary>
        /// Gets or sets the product identifier
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// Gets or sets the category identifier
        /// </summary>
        public int GlCodeId { get; set; }

        ///// <summary>
        ///// 
        ///// </summary>
        //public int GlCodeTypeId { get; set; }

        public decimal Percentage { get; set; }


        public decimal Amount { get; set; }

        /// <summary>
        /// Gets the category
        /// </summary>
        public virtual GlCode GlCode { get; set; }

        /// <summary>
        /// Gets the product
        /// </summary>
        public virtual Product Product { get; set; }

        /// <summary>
        /// tax category for the split.
        /// </summary>
        public virtual int TaxCategoryId { get; set; }


        public decimal CalculateGLAmount(OrderItem item)
        {

            decimal amount = 0;

            if (this.GlCode.GlCodeTypeId == 1)//Split
            {
                if (this.Percentage != 0)
                {
                    amount = (item.UnitPriceExclTax * (this.Percentage * .01m) * item.Quantity);
                }
                if (this.Amount != 0)
                {
                    amount = this.Amount * item.Quantity;
                }

            }
            else if (this.GlCode.GlCodeTypeId == 2) //Specialty
            {
                //get from vetex
            }
            else if (this.GlCode.GlCodeTypeId == 3) //Delivery
            {
                amount = item.DeliveryAmountExclTax;
            }
            else if (this.GlCode.GlCodeTypeId == 4) //Tax
            {
                amount = item.PriceInclTax - item.PriceExclTax;
            }
            return amount;

        }


        public decimal? CalculateGLAmount(OrderItem item, bool isSplit)
        {
            decimal? amount = 0;

            if (isSplit)
            {
                if (this.Percentage != 0)
                {
                    amount = (item.UnitPriceExclTax * (this.Percentage * .01m) * item.Quantity);
                }
                else
                {
                    amount = this.Amount * item.Quantity;
                }
            }
            if (!isSplit)
            {
                if (item.Product.SpecialPrice == null || item.Product.SpecialPrice == decimal.Zero)
                    amount = item.UnitPriceExclTax * item.Quantity;
                else
                    amount = item.Product.SpecialPrice * item.Quantity;
            }

            return amount;
        }

    }

}
