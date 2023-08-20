using System.Data.Entity.ModelConfiguration;
using BitShift.Plugin.Payments.FirstData.Domain;

namespace BitShift.Plugin.Payments.FirstData.Data
{
    public partial class SavedCardMap : EntityTypeConfiguration<SavedCard>
    {
        public SavedCardMap()
        {
            this.ToTable("BitShift_FirstData_SavedCard");
            this.HasKey(x => x.Id);
            this.Property(x => x.Token).HasMaxLength(64);
            this.Property(x => x.CardholderName).HasMaxLength(256);
            this.Property(x => x.CardType).HasMaxLength(64);
        }
    }
}