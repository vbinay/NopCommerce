using Nop.Core.Domain.Stores;

namespace Nop.Data.Mapping.Stores
{   public partial class StoreWiseTierShippingMap : NopEntityTypeConfiguration<StoreWiseTierShipping>
    {
        public StoreWiseTierShippingMap()
        {
            this.ToTable("StoreWiseTierShipping");
            this.HasKey(s => s.Id);
            this.Property(s => s.MinPrice).IsRequired();
            this.Property(s => s.MaxPrice).IsRequired();
            this.Property(s => s.ShippingAmount).IsRequired();
            this.Property(s => s.StoreId).IsRequired();
        }
    }


}