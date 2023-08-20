using Nop.Core.Domain.Orders;

namespace Nop.Data.Mapping.Orders
{
    public partial class OrderItemMap : NopEntityTypeConfiguration<OrderItem>
    {
        public OrderItemMap()
        {
            this.ToTable("OrderItem");
            this.HasKey(orderItem => orderItem.Id);

            this.Property(orderItem => orderItem.UnitPriceInclTax).HasPrecision(18, 4);
            this.Property(orderItem => orderItem.UnitPriceExclTax).HasPrecision(18, 4);
            this.Property(orderItem => orderItem.PriceInclTax).HasPrecision(18, 4);
            this.Property(orderItem => orderItem.PriceExclTax).HasPrecision(18, 4);
            this.Property(orderItem => orderItem.DiscountAmountInclTax).HasPrecision(18, 4);
            this.Property(orderItem => orderItem.DiscountAmountExclTax).HasPrecision(18, 4);
            this.Property(orderItem => orderItem.OriginalProductCost).HasPrecision(18, 4);
            this.Property(orderItem => orderItem.ItemWeight).HasPrecision(18, 4);
            this.Property(orderitem => orderitem.GlcodeAmount1).HasPrecision(18, 4);
            this.Property(orderitem => orderitem.TaxAmount1).HasPrecision(18, 4);
            this.Property(orderitem => orderitem.GlcodeAmount2).HasPrecision(18, 4);
            this.Property(orderitem => orderitem.TaxAmount2).HasPrecision(18, 4);
            this.Property(orderitem => orderitem.GlcodeAmount3).HasPrecision(18, 4);
            this.Property(orderitem => orderitem.TaxAmount3).HasPrecision(18, 4);
            this.Property(orderitem => orderitem.TotalRefundedAmount).HasPrecision(18, 4);
            this.Property(orderitem => orderitem.RefundedTaxAmount1).HasPrecision(18, 4);
            this.Property(orderitem => orderitem.RefundedGlAmount1).HasPrecision(18, 4);
            this.Property(orderitem => orderitem.RefundedTaxAmount2).HasPrecision(18, 4);
            this.Property(orderitem => orderitem.RefundedGlAmount2).HasPrecision(18, 4);
            this.Property(orderitem => orderitem.RefundedTaxAmount3).HasPrecision(18, 4);
            this.Property(orderitem => orderitem.DeliveryPickupFee).HasPrecision(18, 4);
            this.Property(orderitem => orderitem.ShippingFee).HasPrecision(18, 4);
            this.Property(orderitem => orderitem.IsPartialRefund);
            this.Property(orderitem => orderitem.IsFullRefund);
            this.Property(orderitem => orderitem.GLCodeName1);
            this.Property(orderitem => orderitem.GLCodeName2);
            this.Property(orderitem => orderitem.GLCodeName3);
            this.Property(orderitem => orderitem.TaxAmount1);
            this.Property(orderitem => orderitem.TaxAmount2);
            this.Property(orderitem => orderitem.TaxAmount3);
            this.Property(orderitem => orderitem.IsDeliveryPickUp);
            this.Property(orderitem => orderitem.IsShipping);
            this.Property(orderitem => orderitem.PartialRefundOrderAmount).HasPrecision(18, 4);
            this.Property(orderitem => orderitem.DelliveryTax).HasPrecision(18, 4);
            this.Property(orderitem => orderitem.DeliveryPickupAmount).HasPrecision(18, 4);
            this.Property(orderitem => orderitem.ShippingTax).HasPrecision(18, 4);
            this.Property(orderitem => orderitem.ShippingAmount).HasPrecision(18, 4);
            this.Property(orderitem => orderitem.DateOfRefund);
            this.Property(orderitem => orderitem.IsReservation);
            this.Property(o => o.MailStopAddress);
            this.Property(o => o.RCNumber);

            this.HasRequired(orderItem => orderItem.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(orderItem => orderItem.OrderId);

            this.HasRequired(orderItem => orderItem.Product)
                .WithMany()
                .HasForeignKey(orderItem => orderItem.ProductId);
        }
    }
}