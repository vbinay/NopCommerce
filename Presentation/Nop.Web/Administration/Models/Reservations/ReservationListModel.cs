using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;

namespace Nop.Admin.Models.Reservations
{
    public partial class ReservationListModel : BaseNopModel
    {
        public bool isReadOnly { get; set; }

        [NopResourceDisplayName("Admin.Orders.List.StartDate")]
        [UIHint("DateNullable")]
        public DateTime? StartDate { get; set; }

        [NopResourceDisplayName("Admin.Orders.List.EndDate")]
        [UIHint("DateNullable")]
        public DateTime? EndDate { get; set; }

        [NopResourceDisplayName("Admin.Orders.List.OrderId")]
        public string OrderId { get; set; }

        [NopResourceDisplayName("Admin.Orders.List.BillingLastName")]
        [AllowHtml]
        public string BillingLastName { get; set; }

        public bool IsLoggedInAsVendor { get; set; }
        public string IsLoggedAs { get; set; }

        [NopResourceDisplayName("Admin.Orders.List.Vendor")]
        public int VendorId { get; set; }

        public string ProductName { get; set; }

        public string TimeSlot { get; set; }

        public string Status { get; set; }
    }
}