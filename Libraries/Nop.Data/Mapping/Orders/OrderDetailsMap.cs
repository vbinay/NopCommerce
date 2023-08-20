using Nop.Core.Domain.Orders;


namespace Nop.Data.Mapping.Orders
{
    public partial class OrderDetailsMap: NopEntityTypeConfiguration<OrderDetails>
    {
        public OrderDetailsMap()
        {
            this.ToTable("OrderDetails");
            this.HasKey(x => x.Id);

            this.Property(x => x.OrderId);
            this.Property(x => x.OrderItemId);
            this.Property(x => x.StoreExtKey);
            this.Property(x => x.StoreName);
            this.Property(x => x.StoreLegalEntity);
            this.Property(x => x.StoreId);
            this.Property(x => x.StoreCity);
            this.Property(x => x.StoreState);
            this.Property(x => x.StoreZip);
            this.Property(x => x.ShipFromCity);
            this.Property(x => x.ShipFromState);
            this.Property(x => x.ShipFromZip);
            this.Property(x => x.ShipToCity);
            this.Property(x => x.ShipToState);
            this.Property(x => x.ShipToZip);
            this.Property(x => x.IsMealPlan);
            this.Property(x => x.IsDonation);
            this.Property(x => x.IsGift);
            this.Property(x => x.VertexTaxAreaId);
            this.Property(x => x.ProductName);
        }
    }
}
