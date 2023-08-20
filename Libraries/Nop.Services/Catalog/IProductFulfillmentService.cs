using System;
using System.Collections.Generic;
using Nop.Core.Domain.Catalog;

namespace Nop.Services.Catalog
{
	/// <summary>
	/// FulfillmentService service interface
    /// </summary>
    public partial interface IProductFulfillmentService
    {
        /// <summary>
        /// Gets calendar days for a source site
        /// </summary>
        /// <param name="productId">SourceSite id</param>
        /// <param name="year">Year</param>
        /// <param name="month">Month</param>
        /// <returns>FulfillmentCalDay list</returns>
        IList<ProductFulfillmentCalendar> GetForMonths(int productId, int year, int month);

		IList<ProductFulfillmentCalendar> GetForDateRange(int productId, DateTime fromDate, DateTime toDate);

        /// <summary>
        /// Inserts a range of calendar days for a SourceSite
        /// </summary>
		/// <param name="productId">SourceSite id</param>
		/// <param name="calDay">List of FulfillmentCalDays to insert</param>
		void InsertCalDay(int productId, ProductFulfillmentCalendar calDay);



        /// <summary>
        /// gets a calendar day for a SourceSite
        /// </summary>
        /// <param name="productId">SourceSite id</param>
        /// <param name="calDay">List of FulfillmentCalDays to insert</param>
        ProductFulfillmentCalendar GetCalDay(int productId, ProductFulfillmentCalendar calDay);

        /// <summary>
        /// Inserts a range of calendar days for a SourceSite
        /// </summary>
        /// <param name="productId">SourceSite id</param>
        /// <param name="calDay">List of FulfillmentCalDays to insert</param>
        void UpdateCalDay(int productId, ProductFulfillmentCalendar calDay);


        /// <summary>
        /// Deletes a range of calendar days for a SourceSite
        /// </summary>
        /// <param name="productId">SourceSite id</param>
        /// <param name="calDayIds">List of FulfillmentCalDay Id's to delete</param>
        void DeleteCalDay(int productId, ProductFulfillmentCalendar calDayId);

        /// <summary>
        /// Deletes all unavlbl days wrt given product id
        /// </summary>
        /// <param name="productId">SourceSite id</param>
        void DeleteAllCalDays(int productId);
    }
}