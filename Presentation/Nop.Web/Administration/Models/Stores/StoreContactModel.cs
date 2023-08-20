using System.Web.Mvc;
using FluentValidation.Attributes;
using Nop.Admin.Validators.Messages;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;
using Nop.Admin.Validators.Stores;

namespace Nop.Admin.Models.Stores
{
    [Validator(typeof(StoreContactValidator))]
    public partial class StoreContactModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Admin.Configuration.StoreContactModel.Fields.EmailTypeId")]
        [AllowHtml]
        public int EmailTypeId { get; set; }

        [NopResourceDisplayName("Admin.Configuration.StoreContactModel.Fields.Email")]
        [AllowHtml]
        public string Email { get; set; }

        [NopResourceDisplayName("Admin.Configuration.StoreContactModel.Fields.DisplayName")]
        [AllowHtml]
        public string DisplayName { get; set; }
    }
}