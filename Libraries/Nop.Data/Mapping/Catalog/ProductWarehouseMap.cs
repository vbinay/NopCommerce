using Nop.Core.Domain.Catalog;

namespace Nop.Data.Mapping.Catalog
{
    public partial class ProductWarehouseMap : NopEntityTypeConfiguration<ProductWarehouse>
    {
        public ProductWarehouseMap()
        {
            this.ToTable("Product_PickupWarehouses_Mapping");

            this.HasKey(pc => pc.Id);

            this.HasRequired(pc => pc.Warehouse)
                .WithMany()
                .HasForeignKey(pc => pc.WarehouseId);


            this.HasRequired(pc => pc.Product)
                .WithMany()
                .HasForeignKey(pc => pc.ProductId);
        }
    }
}
