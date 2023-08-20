using System;
using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework;
using System.Web.Mvc;
using System.Collections.Generic;

namespace Nop.Admin.Models.Orders
{
    public class StagingListModel : BaseNopModel   // NU-32
    {
        public StagingListModel()
        {
            AvailableStores = new List<SelectListItem>();
            AvailableTaxCategories = new List<SelectListItem>();
            AvailableGLCodes = new List<SelectListItem>();
            AvailablePublish = new List<SelectListItem>();
            AvailableMaster = new List<SelectListItem>();
            AvailableMasterMealPlans = new List<SelectListItem>();

        }

        public int SearchStoreId { get; set; }

        public string Published { get; set; }

        public IList<SelectListItem> AvailableStores { get; set; }


        //product attributes
        public IList<SelectListItem> AvailableTaxCategories { get; set; }
            
        //product attributes
        public IList<SelectListItem> AvailableGLCodes { get; set; }

        public IList<SelectListItem> AvailablePublish { get; set; }

        public IList<SelectListItem> AvailableMaster { get; set; }


        public IList<SelectListItem> AvailableMasterMealPlans { get; set; }



    }
}