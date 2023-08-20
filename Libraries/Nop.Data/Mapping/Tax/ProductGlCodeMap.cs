using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Tax;

namespace Nop.Data.Mapping.Catalog
{
    public partial class ProductGlCodeMap : NopEntityTypeConfiguration<ProductGlCode>
    {
      public ProductGlCodeMap()
        {
            this.ToTable("Product_GlCode_Mapping");
            this.HasKey(pc => pc.Id);
            
            this.HasRequired(pc => pc.GlCode)
                .WithMany()
                .HasForeignKey(pc => pc.GlCodeId);


            this.HasRequired(pc => pc.Product)
                .WithMany(p => p.ProductGlCodes)
                .HasForeignKey(pc => pc.ProductId);
        }
    }
}