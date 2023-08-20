using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Services.PickUpPoint
{
    public class PickupPointShippingMethodModel
    {
        public string ShippingSystemName { get; set; }
        public string FriendlyName { get; set; }
        public string Description { get; set; }
        public string Fee { get; set; }
        public IList<PickUpPointItemModel> Points { get; set; }
    }
}