using Nop.Web.Framework.Mvc;

namespace Nop.Web.Models.Common
{
    public partial class HeaderLinksModel : BaseNopModel
    {
        public bool IsAuthenticated { get; set; }
        public string CustomerEmailUsername { get; set; }
        
        public bool ShoppingCartEnabled { get; set; }
        public int ShoppingCartItems { get; set; }
        
        public bool WishlistEnabled { get; set; }
        public int WishlistItems { get; set; }

        public bool AllowPrivateMessages { get; set; }
        public bool AllowRegistrations { get; set; } //NU-90
        public string UnreadPrivateMessages { get; set; }
        public string AlertMessage { get; set; }
    }
}