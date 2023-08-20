using Nop.Core.Domain.Vendors;

namespace Nop.Data.Mapping.Vendors
{
    public partial class VendorMappingMap : NopEntityTypeConfiguration<VendorMapping>
    {
        public VendorMappingMap()
        {
            this.ToTable("VendorMapping");
            this.HasKey(sm => sm.Id);

            this.Property(sm => sm.EntityName).IsRequired().HasMaxLength(400);

            this.HasRequired(sm => sm.Vendor)
                .WithMany()
                .HasForeignKey(sm => sm.VendorId)
                .WillCascadeOnDelete(true);
        }
    }
}