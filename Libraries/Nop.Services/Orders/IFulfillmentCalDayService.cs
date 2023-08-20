using System;
using System.Collections.Generic;
using Nop.Core.Domain.Orders;

namespace Nop.Services.Orders
{
	/// <summary>
	/// FulfillmentService service interface
    /// </summary>
    public partial interface IFulfillmentService
    {
		/// <summary>
		/// Gets calendar days for a source site
		/// </summary>
		/// <param name="storeId">SourceSite id</param>
		/// <param name="year">Year</param>
		/// <param name="month">Month</param>
		/// <returns>FulfillmentCalDay list</returns>
		IList<FulfillmentCalDay> GetForMonths(int storeId, int year, int month);

		IList<FulfillmentCalDay> GetForDateRange(int storeId, DateTime fromDate, DateTime toDate);

        /// <summary>
        /// Inserts a range of calendar days for a SourceSite
        /// </summary>
		/// <param name="storeId">SourceSite id</param>
		/// <param name="calDay">List of FulfillmentCalDays to insert</param>
		void InsertCalDay(int storeId, FulfillmentCalDay calDay);



        /// <summary>
        /// gets a calendar day for a SourceSite
        /// </summary>
        /// <param name="storeId">SourceSite id</param>
        /// <param name="calDay">List of FulfillmentCalDays to insert</param>
        FulfillmentCalDay GetCalDay(int storeId, FulfillmentCalDay calDay);

        /// <summary>
        /// Inserts a range of calendar days for a SourceSite
        /// </summary>
        /// <param name="storeId">SourceSite id</param>
        /// <param name="calDay">List of FulfillmentCalDays to insert</param>
        void UpdateCalDay(int storeId, FulfillmentCalDay calDay);


		/// <summary>
		/// Deletes a range of calendar days for a SourceSite
		/// </summary>
		/// <param name="storeId">SourceSite id</param>
		/// <param name="calDayIds">List of FulfillmentCalDay Id's to delete</param>
        void DeleteCalDay(int storeId, FulfillmentCalDay calDayId);
    }
}