using Nop.Core.Domain.Stores;
using Nop.Web.Framework.Mvc;
using System.Collections.Generic;

namespace Nop.Plugin.Widgets.AdminSecurity.Models
{
    public class PublicInfoModel : BaseNopModel
    {
        public bool hasAccess = false;
    }
}