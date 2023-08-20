using System;
using System.Collections.Generic;
using Nop.Core.Domain.Catalog;

namespace Nop.Core.Domain.Orders
{
	/// <summary>
	/// Represents a Donation card
	/// </summary>
	public partial class Donation : BaseEntity  /// NU-17
	{


		/// <summary>
		/// Gets or sets the amount
		/// </summary>
		public virtual decimal Amount { get; set; }

		/// <summary>
		/// Gets or sets the recipient name
		/// </summary>
		public virtual string DonorFirstName { get; set; }

		/// <summary>
		/// Gets or sets the recipient name
		/// </summary>
		public virtual string DonorLastName { get; set; }

		public virtual string DonorCompany { get; set; }

		/// <summary>
		/// Gets or sets a recipient address
		/// </summary>
		public virtual string DonorAddress { get; set; }
		public virtual string DonorAddress2 { get; set; }

		public virtual string DonorCity { get; set; }
		public virtual string DonorState { get; set; }
		public virtual string DonorZip { get; set; }
		public virtual string DonorPhone { get; set; }
		public virtual string DonorCountry { get; set; }


		/// <summary>
		/// Gets or sets a recipient phone number
		/// </summary>
		public virtual string OnBehalfOfFullName { get; set; }

		/// <summary>
		/// Gets or sets a recipient email
		/// </summary>
		public virtual string NotificationEmail { get; set; }


		public virtual bool IncludeGiftAmount { get; set; }

		public virtual string Comments { get; set; }

		/// <summary>
		/// Gets or sets an account number
		/// </summary>
		public virtual bool IsProcessed { get; set; }

		/// <summary>
		/// Gets or sets the date and time of instance creation
		/// </summary>
		public virtual DateTime CreatedOnUtc { get; set; }



		/// <summary>
		/// Gets or sets the Id of the Order Item
		/// </summary>
		public virtual int PurchasedWithOrderItemId { get; set; }


		/// <summary>
		/// Gets or sets the associated order item
		/// </summary>
		public virtual OrderItem PurchasedWithOrderItem { get; set; }
	}
  
}
