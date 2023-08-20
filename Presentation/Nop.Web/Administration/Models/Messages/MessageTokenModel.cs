using System.Web.Mvc;
using FluentValidation.Attributes;
using Nop.Admin.Validators.Tax;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;
using Nop.Admin.Validators.Messages;

namespace Nop.Admin.Models.Messages
{
    public partial class MessageTokenModel : BaseNopEntityModel
    {
        public string Name { get; set; }

        public string Value { get; set; }

        public string ReadOnly { get; set; } 
    }
}