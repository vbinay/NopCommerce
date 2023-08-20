using FlatFile.FixedLength.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPFileFeed.SAP
{
    [FixedLengthFile]
    public class SAPRecord
    {
        public int Id;

        /// <summary>
        /// Gets or sets the recipient name
        /// </summary>
        public virtual string RecordIndicator { get; set; }

        /// <summary>
        /// Gets or sets a recipient address
        /// </summary>
        public virtual int OrderNumber { get; set; }

        /// <summary>
        /// Gets or sets a recipient phone number
        /// </summary>
        public virtual string Status { get; set; }

        /// <summary>
        /// Gets or sets a recipient email
        /// </summary>
        public virtual string LineNumber { get; set; }

        /// <summary>
        /// Gets or sets an account number
        /// </summary>
        public virtual String TransactionDate { get; set; }

        /// <summary>
        /// Gets or sets an account number
        /// </summary>
        public virtual string CostCenterNumber { get; set; }

        /// <summary>
        /// Gets or sets the date and time of instance creation
        /// </summary>
        public virtual string GLAccount { get; set; }


        public virtual string AmountSign { get; set; }

        public virtual string ProductAmount { get; set; }

    }
}
