using System.Web.Mvc;
using FluentValidation.Attributes;
using Nop.Admin.Validators.Tax;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;
using System;

namespace Nop.Admin.Models.Tax
{
    public partial class TaxResponseStorageModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Admin.Configuration.Tax.Providers.Fields.FriendlyName")]
        [AllowHtml]
        public int CustomerID { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Tax.Providers.Fields.FriendlyName")]
        [AllowHtml]
        public int? OrderID { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Tax.Providers.Fields.FriendlyName")]
        [AllowHtml]
        public String ProductID { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Tax.Providers.Fields.FriendlyName")]
        [AllowHtml]
        public int TypeOfCall { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Tax.Providers.Fields.FriendlyName")]
        [AllowHtml]
        public string XMlResponse { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Tax.Providers.Fields.FriendlyName")]
        [AllowHtml]
        public DateTime? AddedDate { get; set; }
    }
}