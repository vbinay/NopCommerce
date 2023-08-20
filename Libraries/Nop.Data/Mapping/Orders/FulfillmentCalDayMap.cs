using System.Data.Entity.ModelConfiguration;
using Nop.Core.Domain.Orders;


namespace Nop.Data.Mapping.Orders
{
	public partial class FulfillmentCalDayMap :  NopEntityTypeConfiguration<FulfillmentCalDay>
    {
		public FulfillmentCalDayMap()
        {
			this.ToTable("FulfillmentCalDay");
			this.HasKey(a => a.Id);
			this.Property(s => s.Date).IsRequired();

			this.HasRequired(ss => ss.Store)
                .WithMany()
                .HasForeignKey(ss => ss.StoreId).WillCascadeOnDelete(false);
        }
    }
}