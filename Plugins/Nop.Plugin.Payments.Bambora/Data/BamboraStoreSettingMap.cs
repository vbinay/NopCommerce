using Nop.Plugin.Payments.Bambora.Domain;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Payments.Bambora.Data
{
   public partial class BamboraStoreSettingMap : EntityTypeConfiguration<BamboraStoreSetting>
    {
        public BamboraStoreSettingMap()
        {
            ToTable("Bambora_StoreSetting");
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
