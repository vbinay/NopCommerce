using System.Collections.Generic;
using System.Web.Mvc;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;

namespace Nop.Admin.Models.Catalog
{
    #region SODMYWAY-2946
    public partial class ProductTagListModel : BaseNopModel
    {
        public ProductTagListModel()
        {
            AvailableStores = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.Catalog.Products.List.SearchStore")]
        public int StoreId { get; set; }
        public IList<SelectListItem> AvailableStores { get; set; }
    }
    #endregion
}