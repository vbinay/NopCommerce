using Nop.Web.Framework.Mvc;

namespace Nop.Plugin.PickUpPoints.Store.Models
{
    public class PublicInfoModel : BaseNopModel
    {
        public bool ShowNearestThreePoints { get; set; }
        public decimal Fee { get; set; }
        public decimal MaxWeight { get; set; }
    }
}