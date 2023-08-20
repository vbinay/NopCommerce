using Nop.Core.Domain.Localization;

namespace Nop.Core.Domain.Catalog
{
    /// <summary>
    /// Represents a product attribute
    /// </summary>
    public partial class LimitPurchaseResponse
    {
        public LimitPurchaseResponse()
        {

        }
        public string LimitMessage { get; set; }

        public bool LimitPurchase { get; set; }
    }
}
