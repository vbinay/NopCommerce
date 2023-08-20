using System.Collections.Generic;
using System.Web.Mvc;
using FluentValidation.Attributes;
using Nop.Admin.Validators.Stores;
using Nop.Web.Framework;
using Nop.Web.Framework.Localization;
using Nop.Web.Framework.Mvc;
using System;

namespace Nop.Admin.Models.Stores
{
    [Validator(typeof(StoreValidator))]
    public partial class StoreModel : BaseNopEntityModel, ILocalizedModel<StoreLocalizedModel>
    {
        public StoreModel()
        {
            Locales = new List<StoreLocalizedModel>();
            AvailableLanguages = new List<SelectListItem>();
            AvailableStates = new List<SelectListItem>();
            TieredShipping = new List<TieredShippingModel>();
        }

        [NopResourceDisplayName("Admin.Configuration.Stores.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Stores.Fields.Url")]
        [AllowHtml]
        public string Url { get; set; }

        public string ShipperId { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Stores.Fields.SslEnabled")]
        public virtual bool SslEnabled { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Stores.Fields.IsTieredShippingEnabled")]
        public virtual bool IsTieredShippingEnabled { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Stores.Fields.IsInterOfficeDeliveryEnabled")]
        public virtual bool IsInterOfficeDeliveryEnabled { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Stores.Fields.IsContractShippingEnabled")]
        public virtual bool IsContractShippingEnabled { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Stores.Fields.SecureUrl")]
        [AllowHtml]
        public virtual string SecureUrl { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Stores.Fields.Hosts")]
        [AllowHtml]
        public string Hosts { get; set; }

        //default language
        [NopResourceDisplayName("Admin.Configuration.Stores.Fields.DefaultLanguage")]
        [AllowHtml]
        public int DefaultLanguageId { get; set; }
        public IList<SelectListItem> AvailableLanguages { get; set; }


        public IList<SelectListItem> AvailableStates { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Stores.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Stores.Fields.CompanyName")]
        [AllowHtml]
        public string CompanyName { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Stores.Fields.CompanyAddress")]
        [AllowHtml]
        public string CompanyAddress { get; set; }

        public bool IsReadOnly { get; set; }

        //NU-90
        [NopResourceDisplayName("Admin.Configuration.Stores.Fields.CompanyAddress2")]
        [AllowHtml]
        public string CompanyAddress2 { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Stores.Fields.CompanyCity")]
        [AllowHtml]
        public string CompanyCity { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Stores.Fields.CompanyZipPostalCode")]
        [AllowHtml]
        public string CompanyZipPostalCode { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Stores.Fields.CompanyStateProvinceId")]
        [AllowHtml]
        public int CompanyStateProvinceId { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Stores.Fields.CompanyCountryId")]
        [AllowHtml]
        public int CompanyCountryId { get; set; }
        //End NU-90

        [NopResourceDisplayName("Admin.Configuration.Stores.Fields.CompanyLegalEntity")]
        [AllowHtml]
        public string LegalEntity { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Stores.Fields.CompanyPhoneNumber")]
        [AllowHtml]
        public string CompanyPhoneNumber { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Stores.Fields.CompanyVat")]
        [AllowHtml]
        public string CompanyVat { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Stores.Fields.DiningSiteUrl")]
        [AllowHtml]
        public string DiningSiteUrl { get; set; } //NU-90


        [NopResourceDisplayName("Admin.Configuration.Stores.Fields.ExtKey")]
        public string ExtKey { get; set; }


        [NopResourceDisplayName("Admin.Configuration.Stores.Fields.CurrentSeason")]
        public string CurrentSeason { get; set; }  //EC-5

        [NopResourceDisplayName("Admin.Configuration.Stores.Fields.IsEnabled")]
        public bool? IsEnabled { get; set; }  //EC-5

        [NopResourceDisplayName("Admin.Configuration.Stores.Fields.InActiveDate")]
        public DateTime InactiveDate { get; set; }


        public IList<StoreLocalizedModel> Locales { get; set; }

        public IList<TieredShippingModel> TieredShipping { get; set; }

        #region Nested classes
        #region SODMYWAY-
        public partial class StoreContactModel : BaseNopEntityModel
        {
            public string EmailType { get; set; }

            public int StoreId { get; set; }

            public int EmailTypeId { get; set; }

            public string Email { get; set; }

            public string DisplayName { get; set; }
        }
        #endregion
        #endregion
    }

    public partial class StoreLocalizedModel : ILocalizedModelLocal
    {
        public int LanguageId { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Stores.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }
    }

    public partial class TieredShippingModel : BaseNopEntityModel
    {
        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }
        public decimal ShippingAmount { get; set; }
        public int StoreId { get; set; }
    }

}