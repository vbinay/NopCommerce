using Nop.Web.Framework;

namespace Nop.Admin.Models.Reservations
{
    public class ReservationGridListModel
    {
        public int OrderId { get; set; }
        public int ReservedProductId { get; set; }
        public int ProductId { get; set; }
        [NopResourceDisplayName("Admin.Reservations.Fields.ReservationDate")]
        public string ReservationDate { get; set; }

        [NopResourceDisplayName("Admin.Reservations.Fields.ReservedTimeSlot")]
        public string ReservedTimeSlot { get; set; }

        [NopResourceDisplayName("Admin.Reservations.Fields.ReservedUnits")]
        public int ReservedUnits { get; set; }

        [NopResourceDisplayName("Admin.Reservations.Fields.OrderItemId")]
        public int OrderItemId { get; set; }

        public int PaymentStatusId { get; set; }

        [NopResourceDisplayName("Admin.Reservations.Fields.Customer")]
        public int CustomerId { get; set; }

        [NopResourceDisplayName("Admin.Reservations.Fields.Customer")]
        public string CustomerFullName { get; set; }

        [NopResourceDisplayName("Admin.Reservations.Fields.CustomerEmail")]
        public string CustomerEmail { get; set; }

        [NopResourceDisplayName("Admin.Reservations.Fields.OrderTotal")]
        public decimal OrderTotal { get; set; }

        public string PaymentStatus { get; set; }

        public bool IsFulfilled { get; set; }

        public string ProductName { get; set; }
    }
}