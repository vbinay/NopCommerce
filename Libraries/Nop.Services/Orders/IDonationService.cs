using System;
using System.Collections.Generic;
using Nop.Core.Domain.Orders;

namespace Nop.Services.Orders
{
	/// <summary>
	/// Donation Service Interface
	/// </summary>
	public partial interface IDonationService	/// NU-33
	{

		/// <summary>
		/// 
		/// </summary>		
		/// <param name="donationId"></param>
		/// <returns></returns>
		Donation GetDonationById( int donationId);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="storeId"></param>
        /// <param name="orderItemProductId"></param>
		/// <param name="includeProcessed"></param>
		/// <param name="startTime"></param>
		/// <param name="endTime"></param>
		/// <param name="forExport"></param>
		/// <returns></returns>
		List<Donation> GetDonations(int storeId, int? orderItemProductId, bool includeProcessed, DateTime? startTime = null, DateTime? endTime = null, bool? forExport = false);


        /// <summary>
        /// GetDonationsForExport
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="PurchasedWithOrderItemId"></param>
        /// <param name="includeProcessed"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        List<Donation> GetDonationsForExport(int storeId, int? PurchasedWithOrderItemId, bool includeProcessed, DateTime? startTime = null, DateTime? endTime = null);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="donation"></param>
        void InsertDonation(Donation donation);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="donation"></param>
		void UpdateDonation(Donation donation);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="donation"></param>
		void DeleteDonation(Donation donation);

        IList<Donation> GetDonationsByIds(int[] donationIds);
	}
}
