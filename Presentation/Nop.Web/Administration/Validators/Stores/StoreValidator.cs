using FluentValidation;
using FluentValidation.Results;
using Nop.Admin.Models.Stores;
using Nop.Core.Data;
using Nop.Core.Domain.Stores;
using Nop.Data;
using Nop.Services.Localization;
using Nop.Services.Stores;
using Nop.Web.Framework.Validators;

namespace Nop.Admin.Validators.Stores
{
    public partial class StoreValidator : BaseNopValidator<StoreModel>	
    {
        public StoreValidator(ILocalizationService localizationService, IDbContext dbContext,	
			IStoreService storeService)	/// SODMYWAY-
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.Configuration.Stores.Fields.Name.Required"));
            RuleFor(x => x.Url).NotEmpty().WithMessage(localizationService.GetResource("Admin.Configuration.Stores.Fields.Url.Required"));

            SetStringPropertiesMaxLength<Store>(dbContext);	

            #region SODMYWAY-
            Custom(x =>
            {
                if (storeService.IsStoreDuplicate(x.Id, x.Url))
                {
                    return new ValidationFailure("Name", localizationService.GetResource("Admin.Configuration.Stores.Fields.Url.Duplicate"));
                }
                return null;
            });
            #endregion
        }
    }
}