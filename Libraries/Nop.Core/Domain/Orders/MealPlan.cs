using System;
using System.Collections.Generic;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Seo;
using Nop.Core.Domain.Stores;
namespace Nop.Core.Domain.Orders
{
    /// <summary>
    /// Represents a gift card
    /// </summary>
    public partial class MealPlan : BaseEntity  // NU-16
    {
        /// <summary>
        /// Gets or sets the Id of the SiteProduct
        /// </summary>
        public virtual int PurchasedWithOrderItemId { get; set; }

        /// <summary>
        /// Gets or sets the recipient name
        /// </summary>
        public virtual string RecipientName { get; set; }

        /// <summary>
        /// Gets or sets a recipient address
        /// </summary>
        public virtual string RecipientAddress { get; set; }

        /// <summary>
        /// Gets or sets a recipient phone number
        /// </summary>
        public virtual string RecipientPhone { get; set; }

        /// <summary>
        /// Gets or sets a recipient email
        /// </summary>
        public virtual string RecipientEmail { get; set; }

        /// <summary>
        /// Gets or sets an account number
        /// </summary>
        public virtual string RecipientAcctNum { get; set; }

        /// <summary>
        /// Gets or sets an account number
        /// </summary>
        public virtual bool IsProcessed { get; set; }

        /// <summary>
        /// Gets or sets the date and time of instance creation
        /// </summary>
        public virtual DateTime CreatedOnUtc { get; set; }

        /// <summary>
        /// Reference ID from the card system
        /// </summary>
        public virtual int? CardAccessRecordId { get; set; }


        /// <summary>
        /// Gets or sets the associated order item
        /// </summary>
        public virtual OrderItem PurchasedWithOrderItem { get; set; }

      
    }
}
