using Nop.Web.Framework.Mvc;
using System.Collections.Generic;

namespace Nop.Web.Models.Common
{
    public partial class BannerHeadModel : BaseNopModel
    {
        public List<string> Head { get; set; }

        public string Header { get; set; }

        public string LogoUrl { get; set; }
    }
}