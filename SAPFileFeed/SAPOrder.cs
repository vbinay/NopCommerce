using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPFileFeed
{
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
		public virtual int StatusID { get; set; }

        public virtual int? ProductId { get; set; }

        public virtual bool? IsBundle { get; set; }

    }
}
