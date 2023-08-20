using System.Collections.Generic;
using FluentValidation.Attributes;
using Nop.Admin.Validators.Messages;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;

namespace Nop.Admin.Models.Messages
{
    [Validator(typeof(TestMessageTemplateValidator))]
    public partial class TestMessageTemplateModel : BaseNopEntityModel
    {
        public TestMessageTemplateModel()
        {
            Tokens = new List<string>();
            MessageTokens = new List<MessageTokenModel>(); /// SODMYWAY-
            DefaultValues = new Dictionary<string, object>();	// NU-84
        }

        public int LanguageId { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.MessageTemplates.Test.Tokens")]
        public List<string> Tokens { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.MessageTemplates.Test.SendTo")]
        public string SendTo { get; set; }

        public List<MessageTokenModel> MessageTokens { get; set; }	/// SODMYWAY-

        public Dictionary<string, object> DefaultValues = new Dictionary<string, object>();	// NU-84
    }
}