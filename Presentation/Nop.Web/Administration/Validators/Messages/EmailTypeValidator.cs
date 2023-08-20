using FluentValidation;
using FluentValidation.Results;
using Nop.Admin.Models.Messages;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Stores;
using Nop.Web.Framework.Validators;

namespace Nop.Admin.Validators.Messages
{
    public class EmailTypeValidator : BaseNopValidator<EmailTypeModel>
    {
        public EmailTypeValidator(ILocalizationService localizationService, IEmailTypeService emailTypeService)
        {
            RuleFor(x => x.Name).NotEmpty();

            Custom(x =>
            {
                var nameFound = emailTypeService.GetEmailTypeByName(x.Name);
                if (nameFound != null)
                {
                    return new ValidationFailure("Name", localizationService.GetResource("Admin.Configuration.EmailTypes.Fields.Name.Duplicate"));
                }
                return null;
            });
        }
    }
}