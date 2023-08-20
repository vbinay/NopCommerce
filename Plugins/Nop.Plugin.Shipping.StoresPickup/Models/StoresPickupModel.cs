using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;

namespace Nop.Plugin.Shipping.StoresPickup.Models
{
    public class StoresPickupModel
    {
        [NopResourceDisplayName("Plugins.Shipping.StoresPickup.Fields.Stores")]
        public string Stores { get; set; }
        [NopResourceDisplayName("Plugins.Shipping.StoresPickup.Fields.type")]
        public string Type { get; set; }
        [NopResourceDisplayName("Plugins.Shipping.StoresPickup.Fields.get")]
        public bool PickupStore { get; set; }

    }
}
