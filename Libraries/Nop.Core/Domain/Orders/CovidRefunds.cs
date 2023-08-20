using System;


namespace Nop.Core.Domain.Orders
{
    public partial class CovidRefunds : BaseEntity
    {
        public int CovidRefundId { get; set; }
        public int ProductId { get; set; }
        public int OrderId { get; set; }
        public int OrderItemId { get; set; }
        public string GlCodeName1 { get; set; }
        public decimal GLAmount1 { get; set; }
        public string GlCodeName2 { get; set; }
        public decimal GLAmount2 { get; set; }
        public string GlCodeName3 { get; set; }
        public decimal GLAmount3 { get; set; }
        public decimal TaxAmount1 { get; set; }
        public string TaxName1 { get; set; }
        public decimal TaxAmount2 { get; set; }
        public string TaxName2 { get; set; }
        public decimal TaxAmount3 { get; set; }
        public string TaxName3 { get; set; }
        public decimal TotalRefund { get; set; }
        public decimal RefundedTaxAmount1 { get; set; }
        public decimal RefundedTaxAmount2 { get; set; }
        public decimal RefundedTaxAmount3 { get; set; }
        public string DeliveryTaxName { get; set; }
        public string DeliveryGLCodeName { get; set; }
        public decimal DeliveryTax { get; set; }
        public decimal DeliveryPickupAmount { get; set; }
        public decimal ShippingAmount { get; set; }
        public decimal ShippingTax { get; set; }
        public string ShippingTaxName { get; set; }
        public string Error { get; set; }
        public string Success { get; set; }

        public DateTime CreatedDateUtc { get; set; }

        public string OverriddenGlcode1 { get; set; }
        public string OverriddenGlcode2 { get; set; }
        public string OverriddenGlcode3 { get; set; }
    }
}
