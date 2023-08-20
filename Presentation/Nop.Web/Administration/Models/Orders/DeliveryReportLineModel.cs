using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;

namespace Nop.Admin.Models.Orders
{
    public partial class DeliveryReportLineModel : BaseNopModel
    {
        [NopResourceDisplayName("Admin.SalesReport.Delivery.Fields.OrderID")]
        public string OrderID { get; set; }

        [NopResourceDisplayName("Admin.SalesReport.Delivery.Fields.UnitKey")]
        public string UnitKey { get; set; }

        [NopResourceDisplayName("Admin.SalesReport.Delivery.Fields.UnitName")]
        public string UnitName { get; set; }

        [NopResourceDisplayName("Admin.SalesReport.Delivery.Fields.FulfillmentDate")]
        public string FulfillmentDate { get; set; }

        [NopResourceDisplayName("Admin.SalesReport.Delivery.Fields.OrderTotal")]
        public string OrderTotal { get; set; }

        [NopResourceDisplayName("Admin.SalesReport.Delivery.Fields.OrderStatus")]
        public string OrderStatus { get; set; }

        [NopResourceDisplayName("Admin.SalesReport.Delivery.Fields.CreatedOn")]
        public string CreatedOn { get; set; }
    }
}