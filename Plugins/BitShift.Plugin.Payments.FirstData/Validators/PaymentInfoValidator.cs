using System;
using FluentValidation;
using BitShift.Plugin.Payments.FirstData.Models;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace BitShift.Plugin.Payments.FirstData.Validators
{
    public class PaymentInfoValidator : AbstractValidator<PaymentInfoModel>
    {
        public PaymentInfoValidator(ILocalizationService localizationService)
        {
            //useful links:
            //http://fluentvalidation.codeplex.com/wikipage?title=Custom&referringTitle=Documentation&ANCHOR#CustomValidator
            //http://benjii.me/2010/11/credit-card-validator-attribute-for-asp-net-mvc-3/

            //RuleFor(x => x.CardNumber).NotEmpty().WithMessage(localizationService.GetResource("Payment.CardNumber.Required"));
            //RuleFor(x => x.CardCode).NotEmpty().WithMessage(localizationService.GetResource("Payment.CardCode.Required"));

            RuleFor(x => x.CardholderName).NotEmpty().WithMessage(localizationService.GetResource("Payment.CardholderName.Required"));
            RuleFor(x => x.CardNumber).CreditCard().WithMessage(localizationService.GetResource("Payment.CardNumber.Wrong"));
            RuleFor(x => x.CardCode).Matches(@"^[0-9]{3,4}$").WithMessage(localizationService.GetResource("Payment.CardCode.Wrong"));
            RuleFor(x => x.ExpireMonth).NotEmpty().WithMessage(localizationService.GetResource("Payment.ExpireMonth.Required"));
            RuleFor(x => x.ExpireYear).NotEmpty().WithMessage(localizationService.GetResource("Payment.ExpireYear.Required"));
            RuleFor(x => x.ExpiryDate).Must(BeLaterThanToday).WithMessage(localizationService.GetResource("BitShift.Plugin.FirstData.ExpiryDateError"));
        }

        private bool BeLaterThanToday(string expiryDate)
        {
            int month = Convert.ToInt32(expiryDate.Substring(0, 2));
            int year = Convert.ToInt32(expiryDate.Substring(2, 4));

            if (year < DateTime.Today.Year || (year == DateTime.Today.Year && month < DateTime.Today.Month))
                return false;
            else
                return true;
        }
    }
}