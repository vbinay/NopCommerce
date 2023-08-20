using Nop.Core.Domain.Localization;
using System;
using System.Collections.Generic;

namespace Nop.Core.Domain.Stores
{
    /// <summary>
    /// Represents a store
    /// </summary>
    public partial class Store : BaseEntity, ILocalizedEntity
    {

        public Store()
        {
            ShippingTiers = new List<StoreWiseTierShipping>();
        }
        /// <summary>
        /// Gets or sets the store name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the store URL
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether SSL is enabled
        /// </summary>
        public bool SslEnabled { get; set; }

        /// <summary>
        /// Gets or sets the store secure URL (HTTPS)
        /// </summary>
        public string SecureUrl { get; set; }

        /// <summary>
        /// Gets or sets the comma separated list of possible HTTP_HOST values
        /// </summary>
        public string Hosts { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the default language for this store; 0 is set when we use the default language display order
        /// </summary>
        public int DefaultLanguageId { get; set; }

        /// <summary>
        /// Gets or sets the display order
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Gets or sets the company name
        /// </summary>
        public string CompanyName { get; set; }

        /// <summary>
        /// Gets or sets the company address
        /// </summary>
        public string CompanyAddress { get; set; }

        /// <summary>
        /// Gets or sets the company address
        /// </summary>
        public string CompanyAddress2 { get; set; }

        /// <summary>
        /// Gets or sets the company address
        /// </summary>
        public string CompanyCity { get; set; }

        /// <summary>
        /// Gets or sets the company address
        /// </summary>
        public int? CompanyCountryId { get; set; }

        public int? CompanyStateProvinceId { get; set; }

        public string CompanyZipPostalCode { get; set; }

        public string LegalEntity { get; set; }
        

        /// <summary>
        /// Gets or sets the store phone number
        /// </summary>
        public string CompanyPhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the company VAT (used in Europe Union countries)
        /// </summary>
        public string CompanyVat { get; set; }

        public string TimeZoneInfoId { get; set; }	/// NU-29
	
        public string ExtKey { get; set; }	/// NU-29
                                            /// 
        public string CurrentSeason { get; set; }	/// EC-5
        
        public string DiningSiteUrl { get; set; }	/// NU-90

        public bool? IsEnabled { get; set; }

        public bool? IsTieredShippingEnabled { get; set; }

        public bool IsContractShippingEnabled { get; set; }

        public bool IsInterOfficeDeliveryEnabled { get; set; }

        public DateTime? InactiveDate { get; set; }

        public string ShipperId { get; set; }

        public IList<StoreWiseTierShipping> ShippingTiers { get; set; }
    } 

    public partial class StoreWiseTierShipping : BaseEntity
    {
        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }
        public decimal ShippingAmount { get; set; }
        public int StoreId { get; set; }
    }
}


