using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Core.Domain.Orders
{
    public partial class ShipmentReport : BaseEntity
    {
        public int OrderId { get; set; }

        public int StoreId { get; set; }

        public string OrderStatus { get; set; }

        public decimal OrderTotal { get; set; }

        public int ShipmentID { get; set; }

        public string ShippingMethod { get; set; }

        public string ShipmentTrackingNumber { get; set; }

        public decimal ShippingCost { get; set; }

        public string ShippingStatus { get; set; }

        public string RecipientContactInfo { get; set; }

        public string RecipientName { get; set; }

        public string RecipientCountry { get; set; }

        public string RecipientStateProvince { get; set; }

        public string RecipientCity { get; set; }

        public string RecipientAddress1 { get; set; }

        public string RecipientAddress2 { get; set; }

        public string RecipientZipPostalCode { get; set; }




    }
}
