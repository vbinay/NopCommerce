using Nop.Plugin.Payments.Bambora.Domain;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Payments.Bambora.Data
{
    public partial class StoreLicenseKeyMap : EntityTypeConfiguration<StoreLicenseKey>
    {
        public StoreLicenseKeyMap()
        {
            this.ToTable("Bambora_StoreLicenseKey");
            this.HasKey(x => x.Id);
            this.Property(x => x.LicenseKey).HasMaxLength(1024);
        }
    }
}
