using System.Web.Mvc;
using FluentValidation.Attributes;
using Nop.Admin.Validators.Tax;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;
using Nop.Admin.Validators.Messages;

namespace Nop.Admin.Models.Messages
{
    [Validator(typeof(EmailTypeValidator))]
    public partial class EmailTypeModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Admin.Configuration.EmailTypes.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }
    }
}