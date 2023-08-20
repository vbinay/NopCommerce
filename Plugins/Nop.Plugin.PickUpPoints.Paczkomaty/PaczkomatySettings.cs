using Nop.Core.Configuration;

namespace Nop.Plugin.PickUpPoints.Paczkomaty
{
    public class PaczkomatySettings : ISettings
    {
        public bool ShowNearestThreePoints { get; set; }
        public decimal Fee { get; set; }
        public decimal MaxWeight { get; set; }

        public string CachedPointsList { get; set; }
    }
}