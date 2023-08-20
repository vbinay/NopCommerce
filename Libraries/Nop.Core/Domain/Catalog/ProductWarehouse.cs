using Nop.Core.Domain.Shipping;

namespace Nop.Core.Domain.Catalog
{
    public partial class ProductWarehouse : BaseEntity
    {
        public int ProductId { get; set; }
        public int WarehouseId { get; set; }

        public virtual Warehouse Warehouse { get; set; }

        public virtual Product Product { get; set; }

    }
}

