using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Nop.Web.Framework.Mvc;


namespace Nop.Web.Models.Catalog
{
    public partial class ReservationProductDetailsModel : BaseNopEntityModel
    {

        public ReservationProductDetailsModel()
        {
            this.TimeslotsConfigured = new List<SelectListItem>(); 
        }

        public int ProductId { get; set; }

        public IList<SelectListItem> TimeslotsConfigured { get; set; }

        public int OccupancyUnitsAvailable { get; set; }

        public int OccupancyUnitsAvailableForReservation { get; set; }

        public int UnitsReserved { get; set; }

        public string JsExcludedDatesStr { get; set; }

        public string JsMinDateStr { get; set; }

        public string JsMaxDateStr { get; set; }

        public int ReservationCapPerSlot { get; set; }
    }
}