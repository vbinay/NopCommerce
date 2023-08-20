using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;

namespace Nop.Admin.Models.Orders
{
    /// <summary>
    /// Model for Donation List
    /// </summary>
    public partial class DonationListModel : BaseNopModel	/// NU-33
    {
        public DonationListModel()
        {
        }

        [NopResourceDisplayName("Admin.Orders.Donations.List.StartDate")]
        [UIHint("DateNullable")]
        public DateTime? StartDate { get; set; }

        [NopResourceDisplayName("Admin.Orders.Donations.List.EndDate")]
        [UIHint("DateNullable")]
        public DateTime? EndDate { get; set; }

        [NopResourceDisplayName("Admin.Orders.Donations.List.IncludeProcessed")]
        public bool IncludeProcessed { get; set; }

        [NopResourceDisplayName("Admin.Orders.Donations.List.ExportExcel")]
        public bool ExportExcel { get; set; }
    }
}