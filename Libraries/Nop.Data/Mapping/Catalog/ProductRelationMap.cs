using Nop.Core.Domain.Catalog;

namespace Nop.Data.Mapping.Catalog
{
    public partial class ProductRelationMap : NopEntityTypeConfiguration<ProductRelation>
    {
        public ProductRelationMap()
        {
            this.ToTable("ProductRelation");
            this.Ignore(ea => ea.Product);
            this.Ignore(ea => ea.RelatedProduct);
            this.HasKey(pc => pc.Id);

        }
    }
}