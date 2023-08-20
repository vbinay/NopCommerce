using System;
using System.Collections.Generic;

namespace Nop.Core.Domain.Orders
{
    public class ReservationModel
    {
        public int OrderId { get; set; }
        public int ReservedProductId { get; set; }
        public int ProductId { get; set; }
        public DateTime ReservationDate { get; set; }

        public string ReservedTimeSlot { get; set; }

        public int ReservedUnits { get; set; }

        public int OrderItemId { get; set; }

        public int PaymentStatusId { get; set; }

        public string PaymentStatus { get; set; }

        public int CustomerId { get; set; }

        public string CustomerFullName { get; set; }

        public string CustomerEmail { get; set; }

        public decimal OrderTotal { get; set; }

    }
}