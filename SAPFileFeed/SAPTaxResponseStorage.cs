using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPFileFeed
{
    public partial class SAPTaxResponseStorage
    {

        public int CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the recipient name
        /// </summary>
		public virtual int OrderId { get; set; }

        /// <summary>
        /// Gets or sets a recipient address
        /// </summary>
        public virtual string ProductId { get; set; }

        /// <summary>
        /// Gets or sets a recipient phone number
        /// </summary>
		public virtual string XMLResponse { get; set; }

        public virtual DateTime AddedDate { get; set; }

        public virtual int? orderitemid { get; set; }
    }
}
