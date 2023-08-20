using Nop.Core.Domain.Orders;

namespace Nop.Data.Mapping.Orders
{
    public partial class ShoppingCartItemMap : NopEntityTypeConfiguration<ShoppingCartItem>
    {
        public ShoppingCartItemMap()
        {
            this.ToTable("ShoppingCartItem");
            this.HasKey(sci => sci.Id);

            this.Property(sci => sci.CustomerEnteredPrice).HasPrecision(18, 4);
            this.Property(sci => sci.IsTieredShippingEnabled);
            this.Property(sci => sci.FlatShipping);
            this.Property(sci => sci.IsContractShippingEnabled);
            this.Property(sci => sci.RCNumber);
            this.Property(sci => sci.IsInterOfficeDeliveryEnabled);
            this.Property(sci => sci.MailStopAddress);
            this.Property(sci => sci.IsFirstCartItem);

            this.Ignore(sci => sci.ShoppingCartType);
            this.Ignore(sci => sci.IsFreeShipping);
            this.Ignore(sci => sci.IsShipEnabled);
            this.Ignore(sci => sci.AdditionalShippingCharge);
            this.Ignore(sci => sci.IsTaxExempt);

            this.HasRequired(sci => sci.Customer)
                .WithMany(c => c.ShoppingCartItems)
                .HasForeignKey(sci => sci.CustomerId);

            this.HasRequired(sci => sci.Product)
                .WithMany()
                .HasForeignKey(sci => sci.ProductId);
        }
    }
}
