using Nop.Web.Framework.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Payments.Bambora.Models
{
   public class PaymentInfoModel:BaseNopModel
    {
        public bool UseHostedPage { get; set; }
        public string Url { get; set; }

        public bool LoadConfirmOrder { get; set; }
        public bool LoadPayemntError { get; set; }

        public string ErrorMessage { get; set; }
    }
}
