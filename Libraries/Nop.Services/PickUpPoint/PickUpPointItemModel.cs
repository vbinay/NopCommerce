using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Services.PickUpPoint
{
    public class PickUpPointItemModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public bool IsPaymentAvailable { get; set; }
        public string Street { get; set; }
        public string BuildingNumber { get; set; }
        public string PostCode { get; set; }
        public string Province { get; set; }
        public string Town { get; set; }
    }
}