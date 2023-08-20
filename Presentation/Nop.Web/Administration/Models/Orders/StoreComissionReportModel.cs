using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;

namespace Nop.Admin.Models.Orders
{
    public partial class StoreCommissionReportModel : BaseNopModel
    {
        public StoreCommissionReportModel()
        {
            AvailableStores = new List<SelectListItem>();
            AvailableVendors = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.Orders.StoreCommissions.StartDate")]
        [UIHint("DateNullable")]
        public DateTime? StartDate { get; set; }

        [NopResourceDisplayName("Admin.Orders.StoreCommissions.EndDate")]
        [UIHint("DateNullable")]
        public DateTime? EndDate { get; set; }

        public int ProductId { get; set; }

        [NopResourceDisplayName("Admin.Orders.StoreCommissions.Store")]
        public int StoreId { get; set; }
        [NopResourceDisplayName("Admin.Orders.StoreCommissions.Vendor")]
        public int VendorId { get; set; }

        public IList<SelectListItem> AvailableStores { get; set; }
        public IList<SelectListItem> AvailableVendors { get; set; }

        public bool IsLoggedInAsVendor { get; set; }
    }
}