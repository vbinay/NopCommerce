﻿using FluentValidation;
using Nop.Admin.Models.Directory;
using Nop.Core.Domain.Directory;
using Nop.Data;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace Nop.Admin.Validators.Directory
{
    public partial class StateProvinceValidator : BaseNopValidator<StateProvinceModel>	
    {
        public StateProvinceValidator(ILocalizationService localizationService, IDbContext dbContext)	
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.Configuration.Countries.States.Fields.Name.Required"));

            SetStringPropertiesMaxLength<StateProvince>(dbContext);	
        }
    }
}