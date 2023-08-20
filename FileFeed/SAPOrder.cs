using System;
using System.Collections.Generic;

namespace FileFeed
{
    /// <summary>
    /// Represents a gift card
    /// </summary>
	public partial class SAPOrder
    {

        public int Id;

        /// <summary>
        /// Gets or sets the recipient name
        /// </summary>
		public virtual int OrderId { get; set; }

        /// <summary>
        /// Gets or sets a recipient address
        /// </summary>
        public virtual int OrderItemId { get; set; }

        /// <summary>
        /// Gets or sets a recipient phone number
        /// </summary>
		public virtual int StatusID{ get; set; }

    }
}
