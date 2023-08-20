using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Tax;
using Nop.Core.Domain.Payments;

namespace Nop.Core.Domain.Orders
{
    public class ReservationGridListModel
    {
        public int OrderId { get; set; }
        public int ReservedProductId { get; set; }
        public int ProductId { get; set; }
        public string ReservationDate { get; set; }

        public string ReservedTimeSlot { get; set; }

        public int ReservedUnits { get; set; }

        public int OrderItemId { get; set; }

        public int PaymentStatusId { get; set; }

        public int CustomerId { get; set; }

        public string CustomerFullName { get; set; }

        public string CustomerEmail { get; set; }

        public decimal OrderTotal { get; set; }

        public string PaymentStatus { get; set; }

        public bool IsFulfilled { get; set; }

        public string ProductName { get; set; }
    }
}