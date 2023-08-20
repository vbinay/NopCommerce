using Nop.Core.Domain.Orders;
using Nop.Data.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Data.Mapping.Orders
{
   public partial class ProductGlCodeMappingMap: NopEntityTypeConfiguration<ProductGlCodeMapping>
    {
        public ProductGlCodeMappingMap()
        {
            this.ToTable("ProductGlCodeMapping");
            this.HasKey(x => x.Id);

            this.Property(x => x.OrderId);
            this.Property(x => x.ProductID);
            this.Property(x => x.Glcode);
            this.Property(x => x.Amount).HasPrecision(18, 4);
            this.Property(x => x.GlStatusType);
            this.Property(x => x.ProductName);
        }
    }
}
