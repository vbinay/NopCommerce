/// SODMYWAY-2946
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;

namespace Nop.Admin.Models.Customers
{
    public partial class RegisteredCustomerReportModel : BaseNopModel
    {
        public RegisteredCustomerReportModel()
        {
            AvailableStores = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.Catalog.Products.List.SearchStore")]
        public int StoreId { get; set; }
        public IList<SelectListItem> AvailableStores { get; set; }
    }
}