using System;
using System.Collections.Generic;

namespace FileFeed.SAP
{
    /// <summary>
    /// Represents a gift card
    /// </summary>
	public partial class SAPSalesJournalEntry
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
		public virtual DateTime TransactionDate { get; set; }

		/// <summary>
		/// Gets or sets an account number
		/// </summary>
        public virtual string CostCenterNumber { get; set; }

		/// <summary>
        /// Gets or sets the date and time of instance creation
        /// </summary>
        public virtual string GLAccount { get; set; }
 
        public virtual string AmountSign { get; set; }

        public virtual decimal ProductAmount { get; set; }

        public virtual decimal TaxAmountImposed { get; set; }

        public virtual decimal DeliveryAmountExclTax { get; set; }

        public virtual int CustomerNumber { get; set; }

        public virtual string CustomerName { get; set; }

        public virtual string CustomerAddress { get; set; }

        public virtual string CustomerCity { get; set; }

        public virtual string CustomerState { get; set; }

        public virtual string CustomerPostalCode { get; set; }

        public virtual string CustomerCountry { get; set; }

        public virtual string CustomerJurisdictionCode { get; set; }

        public virtual string ShipToName { get; set; }

        public virtual string ShipToStreetAddress { get; set; }

        public virtual string ShipToCity { get; set; }

        public virtual string ShipToState { get; set; }

        public virtual string ShipToPostalCode { get; set; }

        public virtual string ShipToCountry { get; set; }

        public virtual string ShipToJurisdictionCode { get; set; }

        public virtual string ShipFromName { get; set; }

        public virtual string ShipFromAddress { get; set; }

        public virtual string ShipFromCity { get; set; }

        public virtual string ShipFromState { get; set; }

        public virtual string ShipFromPostalCode { get; set; }

        public virtual string ShipFromCountry { get; set; }

        public virtual string ShipFromJurisdictionCode { get; set; }

        public virtual string EcommMasterProductDescription { get; set; }

        public virtual string EcommLocalProductDescription { get; set; }



    }
}
