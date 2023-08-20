using System;
using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework;
using System.Web.Mvc;
using System.Collections.Generic;

namespace Nop.Admin.Models.Orders
{
    public class MealPlanListModel : BaseNopModel   // NU-32
    {
        public MealPlanListModel()
        {
        }

        [NopResourceDisplayName("Admin.Catalog.MealPlans.List.StartDate")]
        [UIHint("DateNullable")]
        public DateTime? StartDate { get; set; }

        [NopResourceDisplayName("Admin.Catalog.MealPlans.List.EndDate")]
        [UIHint("DateNullable")]
        public DateTime? EndDate { get; set; }

        [NopResourceDisplayName("Admin.Catalog.MealPlans.List.IncludeProcessed")]
        public bool IncludeProcessed { get; set; }

        [NopResourceDisplayName("Admin.Catalog.MealPlans.List.ExportExcel")]
        public bool ExportExcel { get; set; }

        public int SearchStoreId { get; set; }
    }
}