using Nop.Core.Domain.Tax;
using System.Data.Entity.ModelConfiguration;


namespace Nop.Data.Mapping.Tax
{
    public partial class TaxresponseStorageMap : NopEntityTypeConfiguration<TaxResponseStorage> /// NU-16
    {
        public TaxresponseStorageMap()
        {
            this.ToTable("TaxResponseStorage");
            this.HasKey(mp => mp.Id);
        }
    }

    public partial class ProductTaxMap : NopEntityTypeConfiguration<ProductTax> 
    {
        public ProductTaxMap()
        {
            this.ToTable("ProductTax");
            this.HasKey(mp => mp.Id);
        }
    }
}