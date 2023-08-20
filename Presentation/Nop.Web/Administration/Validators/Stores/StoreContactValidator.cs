using FluentValidation;
using Nop.Admin.Models.Messages;
using Nop.Admin.Models.Stores;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace Nop.Admin.Validators.Stores
{
    public class StoreContactValidator : BaseNopValidator<StoreContactModel>
    {
        public StoreContactValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Email).NotEmpty();
            RuleFor(x => x.Email).EmailAddress().WithMessage(localizationService.GetResource("Admin.Common.WrongEmail"));
            
            RuleFor(x => x.DisplayName).NotEmpty();
        }
    }
}