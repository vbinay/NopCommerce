using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;

namespace Nop.Admin.Models.Orders
{
    public partial class DeliveryReportModel : BaseNopModel
    {
        public DeliveryReportModel()
        {
        }

        [NopResourceDisplayName("Admin.SalesReport.Delivery.StartDate")]
        [UIHint("DateNullable")]
        public DateTime? StartDate { get; set; }

        [NopResourceDisplayName("Admin.SalesReport.Delivery.EndDate")]
        [UIHint("DateNullable")]
        public DateTime? EndDate { get; set; }

        [NopResourceDisplayName("Admin.SalesReport.Delivery.UnitNumber")]
        public string UnitNumber { get; set; }
    }
}