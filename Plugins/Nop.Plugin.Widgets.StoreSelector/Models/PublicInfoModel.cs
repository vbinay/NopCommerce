using Nop.Core.Domain.Stores;
using Nop.Web.Framework.Mvc;
using System.Collections.Generic;

namespace Nop.Plugin.Widgets.StoreSelector.Models
{
    public class PublicInfoModel : BaseNopModel
    {
        public List<Store> availableStores = new List<Store>();
        
        public Store currentStore = null;
        public string currentURLTemplate = "";
    }
}