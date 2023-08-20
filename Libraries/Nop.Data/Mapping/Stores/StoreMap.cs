using Nop.Core.Domain.Stores;

namespace Nop.Data.Mapping.Stores
{
    public partial class StoreMap : NopEntityTypeConfiguration<Store>
    {
        public StoreMap()
        {
            this.ToTable("Store");
            this.HasKey(s => s.Id);
            this.Property(s => s.Name).IsRequired().HasMaxLength(400);
            this.Property(s => s.Url).IsRequired().HasMaxLength(400);
            this.Property(s => s.SecureUrl).HasMaxLength(400);
            this.Property(s => s.Hosts).HasMaxLength(1000);

            this.Property(s => s.CompanyName).HasMaxLength(1000);

            this.Property(s => s.CompanyAddress).HasMaxLength(1000);
            this.Property(s => s.CompanyAddress2).HasMaxLength(1000);
            this.Property(s => s.CompanyCity).HasMaxLength(1000);
            this.Property(s => s.CompanyZipPostalCode).HasMaxLength(100);

            this.Property(s => s.CompanyPhoneNumber).HasMaxLength(1000);
            this.Property(s => s.CompanyVat).HasMaxLength(1000);
            this.Property(s => s.TimeZoneInfoId).HasMaxLength(1000);	/// NU-29
            this.Property(s => s.ExtKey).HasMaxLength(1000);	/// NU-29
            this.Property(s => s.CurrentSeason).HasMaxLength(1000);	/// EC-5
            this.Property(s => s.DiningSiteUrl).HasMaxLength(1000);	/// NU-90
            this.Property(s => s.LegalEntity);
            this.Property(s => s.CompanyStateProvinceId);
            this.Property(s => s.IsTieredShippingEnabled);
            this.Property(s => s.IsContractShippingEnabled);
            this.Property(s => s.IsInterOfficeDeliveryEnabled);
            //this.Property(s => s.ShippingTiers);
    }
    }

    
}