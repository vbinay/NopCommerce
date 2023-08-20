using System.Data.Entity.ModelConfiguration;
using BitShift.Plugin.Payments.FirstData.Domain;

namespace BitShift.Plugin.Payments.FirstData.Data
{
    public partial class FirstDataStoreSettingMap : EntityTypeConfiguration<FirstDataStoreSetting>
    {
        public FirstDataStoreSettingMap()
        {
            ToTable("BitShift_FirstData_StoreSetting");
            HasKey(x => x.StoreId);
            Ignore(x => x.Id);
            Property(x => x.HMAC).HasMaxLength(128);
            Property(x => x.GatewayID).HasMaxLength(128);
            Property(x => x.Password).HasMaxLength(128);
            Property(x => x.KeyID).HasMaxLength(128);
            Property(x => x.AdditionalFee).HasPrecision(9, 4);
        }
    }
}