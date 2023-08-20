using Nop.Core.Domain.Orders;
using Nop.Web.Framework.Mvc;
using System.Collections.Generic;

namespace Nop.Web.Models.Checkout
{
    public partial class CheckoutCompletedModel : BaseNopModel
    {
        public int OrderId { get; set; }
        public bool OnePageCheckoutEnabled { get; set; }

        public IList<itemDetails> ProductItems { get; set; }

        public string Authid { get; set; }
        public string Tax { get; set; }
        public string shipping { get; set; }

        public partial class itemDetails:BaseNopEntityModel
        {
            public string ProducName { get; set; }
            public string ProductId { get; set; }
            public string price { get; set; }
            public string Quantity { get; set; }
        }
    }


   
}