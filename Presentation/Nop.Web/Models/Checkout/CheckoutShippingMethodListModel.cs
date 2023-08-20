using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Nop.Web.Models.ShoppingCart;
using Nop.Web.Framework.Mvc;

namespace Nop.Web.Models.Checkout
{
    public class CheckoutShippingMethodListModel : BaseNopModel
    {

        public CheckoutShippingMethodListModel()
        {
            ShoppingCartItemModels = new List<ShoppingCartModel.ShoppingCartItemModel>();
        }

        public bool IsMultipleShippingFeatureEnabled { get; set; }

        public bool IsTieredShippingEnabled { get; set; }
        public decimal FlatShipping { get; set; }

        public bool IsContractShippingEnabled { get; set; }

        public string RCNumber { get; set; }

        public bool IsInterOfficeDeliveryEnabled { get; set; }

        public string MailStopAddress { get; set; }

        public IList<ShoppingCartModel.ShoppingCartItemModel> ShoppingCartItemModels { get; set; }
    }

}