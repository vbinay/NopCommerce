using System.Data.Entity.ModelConfiguration;
using BitShift.Plugin.Payments.FirstData.Domain;

namespace BitShift.Plugin.Payments.FirstData.Data
{
    public partial class StoreLicenseKeyMap : EntityTypeConfiguration<StoreLicenseKey>
    {
        public StoreLicenseKeyMap()
        {
            this.ToTable("BitShift_FirstData_StoreLicenseKey");
            this.HasKey(x => x.Id);
            this.Property(x => x.LicenseKey).HasMaxLength(1024);
        }
    }
}