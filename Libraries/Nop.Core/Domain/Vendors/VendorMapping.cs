namespace Nop.Core.Domain.Vendors
{
    /// <summary>
    /// Represents a vendor mapping record
    /// </summary>
    public partial class VendorMapping : BaseEntity
    {
        /// <summary>
        /// Gets or sets the entity identifier
        /// </summary>
        public int EntityId { get; set; }

        /// <summary>
        /// Gets or sets the entity name
        /// </summary>
        public string EntityName { get; set; }

        /// <summary>
        /// Gets or sets the store identifier
        /// </summary>
        public int VendorId { get; set; }

        /// <summary>
        /// Gets or sets the vendor
        /// </summary>
        public virtual Vendor Vendor { get; set; }
    }
}
