using System.Web.Mvc;
using FluentValidation.Attributes;
using Nop.Admin.Validators.Tax;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;

namespace Nop.Admin.Models.Tax
{
    [Validator(typeof(TaxCategoryValidator))]
    public partial class GLCodeModel : BaseNopEntityModel
    {
        /// <summary>
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// </summary>
        public bool IsPaid { get; set; }

        /// <summary>
        /// </summary>
        public bool IsDelivered { get; set; }

        /// <summary>
        /// </summary>
        public string GlCodeName { get; set; }


        public decimal? Amount { get; set; }

        public decimal? Percentage { get; set; }

        public int ProductId { get; set; }

        public int TaxCategoryId { get; set; }

        public int GlCodeId { get; set; }
    }
}