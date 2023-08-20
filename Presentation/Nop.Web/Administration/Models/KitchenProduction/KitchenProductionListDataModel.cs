using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Nop.Admin.Models.KitchenProduction
{
    public class KitchenProductionListDataModel
    {
        public int orderid { get; set; }
        public int orderItemid { get; set; }
        public string attributes { get; set; }

        public string productName { get; set; }

        public string pickupLocation { get; set; }

        public DateTime? pickupDate { get; set; }

        public DateTime? createdOnDate { get; set; }

        public string userName { get; set; }

        public string email { get; set; }

        public string phone { get; set; }
        public string Categories { get; set; }

    }
}