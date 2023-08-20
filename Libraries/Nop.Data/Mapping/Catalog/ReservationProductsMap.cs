using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;

namespace Nop.Data.Mapping.Catalog
{
      public partial class ReservationProductsMap : NopEntityTypeConfiguration<ReservationProducts>
    {
        public ReservationProductsMap()
        {
            this.ToTable(" ReservationProducts");
            this.HasKey(reservationProducts => reservationProducts.Id);

            this.Property(reservationProducts => reservationProducts.ProductId);
            this.Property(reservationProducts => reservationProducts.TimeSlotsConfigured);
            this.Property(reservationProducts => reservationProducts.OccupancyUnitsAvailable);
        }
    }
}
