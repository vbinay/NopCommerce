using Nop.Core.Domain.Stores;

namespace Nop.Data.Mapping.Stores
{
    public partial class StoreContactMap : NopEntityTypeConfiguration<StoreContact>
    {
        public StoreContactMap()
        {
            this.ToTable("Store_Email_Mapping");
            this.HasKey(ec => ec.Id);

            this.Property(ec => ec.Email).IsRequired().HasMaxLength(255);
            this.Property(ec => ec.DisplayName).HasMaxLength(255);
        }
    }
}