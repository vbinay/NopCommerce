//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SAPFileFeed
{
    using System;
    using System.Collections.Generic;
    
    public partial class Store
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Store()
        {
            this.FulfillmentCalDays = new HashSet<FulfillmentCalDay>();
            this.ProductReviews = new HashSet<ProductReview>();
            this.Store_Email_Mapping = new HashSet<Store_Email_Mapping>();
            this.StoreCommissions = new HashSet<StoreCommission>();
            this.StoreMappings = new HashSet<StoreMapping>();
        }
    
        public int Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public bool SslEnabled { get; set; }
        public string SecureUrl { get; set; }
        public string Hosts { get; set; }
        public int DefaultLanguageId { get; set; }
        public int DisplayOrder { get; set; }
        public string CompanyName { get; set; }
        public string CompanyAddress { get; set; }
        public string CompanyPhoneNumber { get; set; }
        public string CompanyVat { get; set; }
        public string TimeZoneInfoId { get; set; }
        public string ExtKey { get; set; }
        public string DiningSiteURL { get; set; }
        public Nullable<bool> IsEnabled { get; set; }
        public Nullable<System.DateTime> CreationDate { get; set; }
        public string CurrentSeason { get; set; }
        public string diningsiteurlbkp { get; set; }
        public Nullable<System.DateTime> DeactivatedDate { get; set; }
        public string CompanyAddress2 { get; set; }
        public string CompanyCity { get; set; }
        public string CompanyZipPostalCode { get; set; }
        public Nullable<int> CompanyCountryId { get; set; }
        public Nullable<int> CompanyStateProvinceId { get; set; }
        public string LegalEntity { get; set; }
        public Nullable<System.DateTime> InactiveDate { get; set; }
        public string ShipperId { get; set; }
        public Nullable<bool> IsTieredShippingEnabled { get; set; }
        public Nullable<bool> IsContractShippingEnabled { get; set; }
        public Nullable<bool> IsInterOfficeDeliveryEnabled { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<FulfillmentCalDay> FulfillmentCalDays { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProductReview> ProductReviews { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Store_Email_Mapping> Store_Email_Mapping { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<StoreCommission> StoreCommissions { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<StoreMapping> StoreMappings { get; set; }
    }
}
