using Nop.Data.Mapping;
using Nop.Plugin.Tax.Vertex.Domain;

namespace Nop.Plugin.Tax.Vertex.Data
{
    public partial class ProductTaxMap : NopEntityTypeConfiguration<ProductTax>
    {
        public ProductTaxMap()
        {
            this.ToTable("ProductTax");
        }
    }
}