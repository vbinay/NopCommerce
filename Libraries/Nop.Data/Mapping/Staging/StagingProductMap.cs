using Nop.Core.Domain.Staging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Data.Mapping.Staging
{
    public partial class StagingProductMap : NopEntityTypeConfiguration<StagingProducts>
    {
        public StagingProductMap()
        {
            this.ToTable("StagingTaxCategory");
            this.HasKey(s => s.Id);
            this.Property(s => s.Id);
            this.Property(s => s.ProductId);
            this.Property(s => s.TaxCategoryId);
            this.Property(s => s.GlCode1Id);
            this.Property(s => s.GlCode1Amount).HasPrecision(18, 4);
            this.Property(s => s.GlCode2Id);
            this.Property(s => s.GLCode2Amount).HasPrecision(18, 4);            
            this.Property(s => s.Comments).HasMaxLength(255);
            this.Property(s => s.Completed);

        }

    }
}
