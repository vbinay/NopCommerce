using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Core.Domain.Catalog
{
    public partial class ReservationProducts : BaseEntity
    {
        //public  int Id { get; set; }

        public  int ProductId { get; set; }

        public  string TimeSlotsConfigured { get; set; }

        public  int OccupancyUnitsAvailable { get; set; }
    }
}
