using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Core.Domain.Orders
{
    public partial class ReservedProduct : BaseEntity
    {
        /// <summary>
        /// Reserved Product Id
        /// </summary>
        public int ProductId { get; set; }
        /// <summary>
        /// Reservation date
        /// </summary>
        public DateTime ReservationDate { get; set; }
        /// <summary>
        /// Reservayion Time Slot
        /// </summary>
        public string ReservedTimeSlot { get; set; }
        /// <summary>
        /// reserved Units
        /// </summary>
        public int ReservedUnits { get; set; }
        /// <summary>
        /// Order Id
        /// </summary>
        public int OrderItemId { get; set; }
    }
}
