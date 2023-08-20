namespace Nop.Core.Domain.Vendors
{
    public partial class StoreCommissionReportLineModel
    {
        public int OrderId { get; set; }

        public string StoreName { get; set; }

        public string ProductName { get; set; }

        public decimal Quantity { get; set; }

        public decimal? Rate { get; set; }

        public string Commission { get; set; }

        public string Earned { get; set; }

        public string Total { get; set; }

        public string Date { get; set; }

        public string VendorName { get; set; }

        public string UnitNumber { get; set; }
    }
}