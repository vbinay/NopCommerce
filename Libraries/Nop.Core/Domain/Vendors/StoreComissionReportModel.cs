using System;
namespace Nop.Core.Domain.Vendors
{
    public partial class StoreCommissionRepModel
    {
        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public int ProductId { get; set; }

        public int StoreId { get; set; }

        public int VendorId { get; set; }

        public bool IsLoggedInAsVendor { get; set; }
    }
}