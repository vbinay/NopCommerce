using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core.Domain.Orders;

namespace Nop.Data.Mapping.Orders
{
    public partial class ReservedProductMap : NopEntityTypeConfiguration<ReservedProduct>
    {
        public ReservedProductMap()
        {
            this.ToTable("ReservedProduct");
            this.HasKey(reservedProduct => reservedProduct.Id);

            this.Property(reservedProduct => reservedProduct.ProductId);
            this.Property(reservedProduct => reservedProduct.OrderItemId);
            this.Property(reservedProduct => reservedProduct.ReservationDate);
            this.Property(reservedProduct => reservedProduct.ReservedTimeSlot);
            this.Property(reservedProduct => reservedProduct.ReservedUnits);
        }
    }
}
