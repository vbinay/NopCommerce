using Nop.Core.Domain.Customers;

namespace Nop.Data.Mapping.Customers
{
    public partial class CommuterMap : NopEntityTypeConfiguration<Commuter>
    {
        public CommuterMap()
        {
            this.ToTable("Commuter");         
        }
    }
}