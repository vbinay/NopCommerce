using System;
using Nop.Web.Framework.Mvc;

namespace Nop.Admin.Models.Orders
{
    public class MealPlanModel : BaseNopEntityModel /// NU-32
    {
        public int PurchasedWithOrderItemId { get; set; }
        public string MealPlanName { get; set; }
        public string RecipientName { get; set; }
        public string RecipientAddress { get; set; }
        public string RecipientPhone { get; set; }
        public string RecipientEmail { get; set; }
        public string RecipientAcctNum { get; set; }
        public string MealPlanAmount { get; set; }
        public bool IsProcessed { get; set; }
        public string Attributes { get; set; } //NU-83
        public int OrderId { get; set; } //NU-83
        public string CreatedOnLocal { get; set; }

    }
}