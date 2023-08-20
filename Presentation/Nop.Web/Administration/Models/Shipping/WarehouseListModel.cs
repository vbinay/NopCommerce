using System.Collections.Generic;
using System.Web.Mvc;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;

namespace Nop.Admin.Models.Catalog
{
    public partial class WarehouseListModel : BaseNopModel
    {
        public WarehouseListModel()
        {
            AvailableStores = new List<SelectListItem>();
            AvailableVendors = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.Catalog.Products.List.SearchStore")]
        public int SearchStoreId { get; set; }
        [NopResourceDisplayName("Admin.Catalog.Products.List.SearchVendor")]
        public int SearchVendorId { get; set; }

        public IList<SelectListItem> AvailableStores { get; set; }
        public IList<SelectListItem> AvailableVendors { get; set; }

        public string IsLoggedAs { get; set; }	/// SODMYWAY

        public bool IsReadOnly { get; set; }

        public string SearchBy { get; set; }
        public bool SearchByStore { get; set; }
        public bool SearchByVendor { get; set; }
    }
}