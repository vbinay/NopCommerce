using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Nop.Plugin.PickUpPoints.Store.Models
{
    public class ConfigurationModel : BaseNopModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Plugins.PickUpPoints.Store.Street")]
        public string Street { get; set; }
        public bool Street_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.PickUpPoints.Store.BuildingNumber")]
        public string BuildingNumber { get; set; }
        public bool BuildingNumber_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.PickUpPoints.Store.PostCode")]
        public string PostCode { get; set; }
        public bool PostCode_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.PickUpPoints.Store.Town")]
        public string Town { get; set; }
        public bool Town_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.PickUpPoints.Store.Latitude")]
        public string Latitude { get; set; }
        public bool Latitude_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.PickUpPoints.Store.Longitude")]
        public string Longitude { get; set; }
        public bool Longitude_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.PickUpPoints.Store.Description")]
        public string Description { get; set; }
        public bool Description_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.PickUpPoints.Store.Fee")]
        public decimal Fee { get; set; }
        public bool Fee_OverrideForStore { get; set; }
    }
}