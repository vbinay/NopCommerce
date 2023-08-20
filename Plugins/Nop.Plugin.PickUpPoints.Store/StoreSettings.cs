using Nop.Core.Configuration;

namespace Nop.Plugin.PickUpPoints.Store
{
    public class StoreSettings : ISettings
    {
        public string Street { get; set; }
        public string BuildingNumber { get; set; }
        public string PostCode { get; set; }
        public string Town { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string Description { get; set; }
        public decimal Fee { get; set; }
    }
}