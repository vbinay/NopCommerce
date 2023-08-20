using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Nop.Admin.Models.KitchenProduction
{
    public class KitchenProductionSearchDataModel:BaseNopModel
    {
        public KitchenProductionSearchDataModel()
        {
            CategoriesIds = new List<int>();
            AvailablePaymentMethods = new List<SelectListItem>(); ;
            AvailableCategories = new List<SelectListItem>();
        }

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

        public IList<SelectListItem> AvailablePaymentMethods { get; set; }

        public IList<SelectListItem> AvailableCategories { get; set; }

        [NopResourceDisplayName("Admin.Kitchen.List.Category")]
        [UIHint("MultiSelect")]
        public List<int> CategoriesIds { get; set; }


        [NopResourceDisplayName("Admin.Orders.List.PaymentMethod")]
        public string PaymentMethodSystemName { get; set; }
    }
}