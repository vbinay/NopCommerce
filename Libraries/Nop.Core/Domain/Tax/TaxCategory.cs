
namespace Nop.Core.Domain.Tax
{
    /// <summary>
    /// Represents a tax category
    /// </summary>
    public partial class TaxCategory : BaseEntity
    {
        /// <summary>
        /// Gets or sets the name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the display order
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Code { get; set; }

        public decimal? Amount { get; set; }

        public  decimal? Percentage { get; set; }

        public string TaxCategoryId { get; set; }

        public string GlCodeId { get; set; }
    }

}
