using System;
using Nop.Core.Domain.Payments;

namespace Nop.Services.Orders
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

        public string ProductName { get; set; }

        public PaymentStatus PaymentStatus 
        {
            get
            {
                return (PaymentStatus)this.PaymentStatusId;
            }
            set
            {
                this.PaymentStatusId = (int)value;
            }
        }
        public int CustomerId { get; set; }

        public string CustomerFullName { get; set; }

        public string BillingLastName { get; set; }

        public string CustomerEmail { get; set; }

        public decimal OrderTotal { get; set; }

        public bool IsFulfilled { get; set; }

        public int StoreId { get; set; }

    }
}