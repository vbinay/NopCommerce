using System.Collections.Generic;
using System.Web.Mvc;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;

namespace BitShift.Plugin.Payments.FirstData.Models
{
    public class PaymentResponseModel : BaseNopModel
    {
        public PaymentResponseModel()
        {

        }

        public bool IsSuccess { get; set; }

        public string ErrorMessage { get; set; }
    }
}