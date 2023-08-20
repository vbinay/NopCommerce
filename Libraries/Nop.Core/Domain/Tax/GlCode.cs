
namespace Nop.Core.Domain.Tax
{
    /// <summary>
    /// </summary>
    public partial class GlCode : BaseEntity
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


        public int GlCodeTypeId { get; set; } //RMARKLE
    }
}
