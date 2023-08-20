
namespace Nop.Core.Domain.Tax
{
    /// <summary>
    /// Represents the VAT number status enumeration
    /// </summary>
    public enum GLCodeType
    {
        /// <summary>
        /// Unknown
        /// </summary>
        Revenue = 1,

        /// <summary>
        /// Empty
        /// </summary>
        Tax = 4,
        /// <summary>
        /// Valid
        /// </summary>
        Delivery = 3,
        /// <summary>
        /// Invalid
        /// </summary>
        Specialty = 2
    }


    /// <summary>
    /// Represents the VAT number status enumeration
    /// </summary>
    public enum GLCodeStatusType
    {
        /// <summary>
        /// Unknown
        /// </summary>
        Paid = 1,

        Deferred = 2,

        All = 0
    }
}
