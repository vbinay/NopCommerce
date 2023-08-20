using System;
using System.Collections.Generic;

namespace FileFeed
{
    /// <summary>
    /// Represents a gift card
    /// </summary>
	public partial class MealPlan
    {

        public int Id;

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


  
        public virtual string Attributes { get; set; }

        public virtual string PlanName { get; set; }

		/// <summary>
        /// Gets or sets the associated order site product
        /// </summary>
        public virtual decimal Amount { get; set; }

        public virtual int OrderId { get; set; }

        public virtual int OrderItemId { get; set; }

        public virtual string CategoryName { get; set; }
    }
}
