using Nop.Core.Domain.Tax;

namespace Nop.Data.Mapping.Tax
{
    public class GlCodeMap : NopEntityTypeConfiguration<GlCode>
    {
        public GlCodeMap()
        {
            this.ToTable("GLCode");
            this.HasKey(tc => tc.Id);
        }
    }
}
