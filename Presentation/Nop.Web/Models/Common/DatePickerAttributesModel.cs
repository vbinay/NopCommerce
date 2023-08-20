using System.Collections.Generic;
using Nop.Core.Domain.Catalog;
using Nop.Web.Framework.Mvc;
using System;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;
using Nop.Core.Domain.Common;
using Nop.Web.Models.Checkout;
using Nop.Web.Models.Common;
using Nop.Web.Models.Media;
using Nop.Core.Domain.Shipping;

namespace Nop.Web.Models.Common
{
    public partial class DatePickerAttributesModel : BaseNopEntityModel
    {
        public DateTime? JsRequestedsDateTimeLocal { get; set; }

        public string JsExcludedDatesStr { get; set; }

        public string JsAvailableHoursStr { get; set; }

        public string JsMinDateStr { get; set; }

        public int? JsLeadTimeDays { get; set; }

        public int? JsLeadTimeHours { get; set; }

        public int? JsLeadTimeMinutes { get; set; }

        public string JsMaxDateStr { get; set; }

    }
}