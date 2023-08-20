using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Core.Domain.Orders
{
    public partial class PartiallyRefundGridModel : BaseEntity
    {
        public int ProductId { get; set; }
        public int OrderId { get; set; }

        public int OrderItemId { get; set; }
        public string ProductName { get; set; }
        public string Sku { get; set; }
        public string GLCodeName1 { get; set; }
        public decimal GLAmount1 { get; set; }
        public string GLCodeName2 { get; set; }
        public decimal GLAmount2 { get; set; }
        public string GLCodeName3 { get; set; }
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
        public string ShippingGlName { get; set; }
        public string Error { get; set; }

        public string Success { get; set; }

        public int Glcodeid1 { get; set; }
        public int Glcodeid2 { get; set; }
        public int Glcodeid3 { get; set; }
        public string ConsoleRefund { get; set; }
        public PartiallyRefundGridModel()
        {
            GLCodeName1 = "None";
            GLCodeName2 = "None";
            GLCodeName3 = "None";
            TaxName1 = "None";
            TaxName2 = "None";
            TaxName3 = "None";

        }



    }

    public partial class PartiallyRefundGridModel_SB : BaseEntity
    {
        public int ProductId_SB { get; set; }
        public int OrderItemId_SB { get; set; }
        public string ProductName_SB { get; set; }
        public string Sku_SB { get; set; }
        public string GLCodeName1_SB { get; set; }
        public decimal GLAmount1_SB { get; set; }
        public string GLCodeName2_SB { get; set; }
        public decimal GLAmount2_SB { get; set; }
        public string GLCodeName3_SB { get; set; }
        public decimal GLAmount3_SB { get; set; }

        public decimal TaxAmount1_SB { get; set; }

        public string TaxName1_SB { get; set; }

        public decimal TaxAmount2_SB { get; set; }

        public string TaxName2_SB { get; set; }

        public decimal TaxAmount3_SB { get; set; }
        public string TaxName3_SB { get; set; }

        public decimal TotalRefund_SB { get; set; }

        public decimal RefundedTaxAmount1_SB { get; set; }
        public decimal RefundedTaxAmount2_SB { get; set; }
        public decimal RefundedTaxAmount3_SB { get; set; }

        public string DeliveryTaxName_SB { get; set; }
        public string DeliveryGLCodeName_SB { get; set; }

        public decimal DeliveryTax_SB { get; set; }

        public decimal DeliveryPickupAmount_SB { get; set; }
        public decimal ShippingAmount_SB { get; set; }

        public decimal ShippingTax_SB { get; set; }
        public string ShippingTaxName_SB { get; set; }
        public string ShippingGlName_SB { get; set; }
        public string Error_SB { get; set; }

        public string Success_SB { get; set; }

        public int Glcodeid1_SB { get; set; }
        public int Glcodeid2_SB { get; set; }
        public int Glcodeid3_SB { get; set; }
        public string ConsoleRefund_SB { get; set; }

        public PartiallyRefundGridModel_SB()
        {
            GLCodeName1_SB = "None";
            GLCodeName2_SB = "None";
            GLCodeName3_SB = "None";
            TaxName1_SB = "None";
            TaxName2_SB = "None";
            TaxName3_SB = "None";

        }
    }
}
