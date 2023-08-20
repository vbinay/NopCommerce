namespace Nop.Core.Domain.Tridion
{
    /// <summary>
    /// This is an object that relates Customer Id's in Nop Commerce to identification in Tridion SODMYWAY-2956
    /// </summary>
    public class TridionProfileMapping : BaseEntity
    {
        /// <summary>
        /// The Ecommerce Store Store Id Stored In Tridion for this Site
        /// </summary>
        public virtual string TridionIdentificationSource { get; set; }
        /// <summary>
        /// The Tridion Unique Identifier for this user
        /// </summary>
        public virtual string TridionIdentificationKey { get; set; }
        /// <summary>
        /// The Customer Id that releates to this Tridion User
        /// </summary>
        public virtual int CustomerId { get; set; }
    }
}
