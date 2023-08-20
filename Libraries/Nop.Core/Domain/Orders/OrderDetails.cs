using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Core.Domain.Orders
{
    public partial class OrderDetails : BaseEntity
    {

        public int OrderId { get; set; }
        public int OrderItemId { get; set; }
        public int StoreId { get; set; }

        public string StoreName { get; set; }
        public string StoreExtKey { get; set; }

        public string StoreLegalEntity { get; set; }

        public string StoreState { get; set; }
        public string StoreZip { get; set; }
        public string StoreCity { get; set; }
        public string ShipFromCity { get; set; }
        public string ShipFromState { get; set; }
        public string ShipFromZip { get; set; }
        public string ShipToCity { get; set; }
        public string ShipToState { get; set; }
        public string ShipToZip { get; set; }
        public bool IsMealPlan { get; set; }
        public bool IsDonation { get; set; }
        public bool IsGift { get; set; }
        public string VertexTaxAreaId { get; set; }

        public string ProductName { get; set; }
    }
}
