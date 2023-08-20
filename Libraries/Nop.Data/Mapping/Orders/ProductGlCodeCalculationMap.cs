using Nop.Core.Domain.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Data.Mapping.Orders
{
    public partial class ProductGlCodeCalculationMap: NopEntityTypeConfiguration<ProductGlCodeCalculation>
    {
        public ProductGlCodeCalculationMap()
        {
            this.ToTable("Product_GLCode_Calc");
            this.HasKey(productGlCodeCalculation => productGlCodeCalculation.Id);
            this.Property(productGlCodeCalculation => productGlCodeCalculation.StoreId);
            this.Property(productGlCodeCalculation => productGlCodeCalculation.OrderId);
            this.Property(productGlCodeCalculation => productGlCodeCalculation.ProductId);
            this.Property(productGlCodeCalculation => productGlCodeCalculation.GLCodeId);
            this.Property(productGlCodeCalculation => productGlCodeCalculation.Amount).HasPrecision(18, 4); ;
            this.Property(productGlCodeCalculation => productGlCodeCalculation.Processed);
            this.Property(productGlCodeCalculation => productGlCodeCalculation.GlStatusType);
        }
    }
}
