using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Nop.Plugin.PickUpPoints.Paczkomaty.Models
{
    public class ConfigurationModel : BaseNopModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Plugins.PickUpPoints.Paczkomaty.ShowNearestThreePoints")]
        public bool ShowNearestThreePoints { get; set; }
        public bool ShowNearestThreePoints_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.PickUpPoints.Paczkomaty.Fee")]
        public decimal Fee { get; set; }
        public bool Fee_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.PickUpPoints.Paczkomaty.MaxWeight")]
        public decimal MaxWeight { get; set; }
        public bool MaxWeight_OverrideForStore { get; set; }
    }
}