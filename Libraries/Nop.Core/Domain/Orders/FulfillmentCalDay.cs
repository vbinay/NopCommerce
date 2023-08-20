using System;
using Nop.Core.Domain.Stores;

namespace Nop.Core.Domain.Orders
{
    /// <summary>
	/// Represents a Fulfillment Calendar
    /// </summary>
	public partial class FulfillmentCalDay : BaseEntity
    {
		/// <summary>
        /// Gets or sets Source site ID
		/// </summary>
        public virtual int StoreId { get; set; }

		/// <summary>
		/// Gets or sets the name
		/// </summary>
		public virtual DateTime Date { get; set; }

        #region Navigation properties


        #endregion

        public virtual Store Store { get; set;  }


        public string Title;
        public int SomeImportantKeyID;
        public string StartDateString;
        public string EndDateString;
        public string StatusString;
        public string StatusColor;
        public string ClassName;

    }
}
