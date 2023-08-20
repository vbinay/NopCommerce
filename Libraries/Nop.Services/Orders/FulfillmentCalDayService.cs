using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Nop.Core;
using Nop.Core.Data;
using Nop.Core.Events;
using Nop.Core.Domain.Orders;
using System.Globalization;

namespace Nop.Services.Orders
{
    /// <summary>
    /// Site service
    /// </summary>
    public partial class FulfillmentService : IFulfillmentService
    {
        #region Fields

        private readonly IRepository<FulfillmentCalDay> _fulfillmentCalRepository;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="siteRepository">Fulfillment Calendar repository</param>
        /// <param name="eventPublisher"></param>
        public FulfillmentService(IRepository<FulfillmentCalDay> fulfillmentCalRepository)
        {
            _fulfillmentCalRepository = fulfillmentCalRepository;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets excluded fulfillment days for the given month
        /// </summary>
        /// <param name="storeId">Store id</param>
        /// <param name="year">Year</param>
        /// <param name="month">Month</param>
        /// <returns>FulfillmentCalDay list</returns>
        public virtual IList<FulfillmentCalDay> GetForMonths(int storeId, int year, int month)
        {
            var fromDate = new DateTime(year, month, 1);
            var toDate = fromDate.AddMonths(1);

            var calDays = from c in _fulfillmentCalRepository.Table
                          where
                              c.StoreId == storeId &&
                              c.Date >= fromDate &&
                              c.Date < toDate
                          orderby
                              c.Date
                          select c;

            return calDays.ToList();
        }

        /// <summary>
        /// Gets excluded fulfillment days for a date range
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <returns>FulfillmentCalDay list</returns>
        public virtual IList<FulfillmentCalDay> GetForDateRange(int storeId, DateTime fromDate, DateTime toDate)
        {
            var calDays = from c in _fulfillmentCalRepository.Table
                          where
                              c.StoreId == storeId &&
                              c.Date >= fromDate &&
                              c.Date <= toDate
                          orderby
                              c.Date
                          select c;

            return calDays.ToList();
        }

        /// <summary>
        /// Inserts a range of calendar days for a Store
        /// </summary>
        /// <param name="storeId">Store id</param>
        /// <param name="calDay">List of FulfillmentCalDays to insert</param>
        public void InsertCalDay(int storeId, FulfillmentCalDay calDay)
        {
            calDay.StoreId = storeId;
            _fulfillmentCalRepository.Insert(calDay);
        }



        /// <summary>
        /// Inserts a range of calendar days for a Store
        /// </summary>
        /// <param name="storeId">Store id</param>
        /// <param name="calDay">List of FulfillmentCalDays to insert</param>
        public FulfillmentCalDay GetCalDay(int storeId, FulfillmentCalDay calDay)
        {
            
             var tmpCalDays = from c in _fulfillmentCalRepository.Table
                          where
                              c.StoreId == storeId &&
                              c.Date == calDay.Date
                          select c;

             return tmpCalDays.FirstOrDefault();
        }

        /// <summary>
        /// Inserts a range of calendar days for a Store
        /// </summary>
        /// <param name="storeId">Store id</param>
        /// <param name="calDay">List of FulfillmentCalDays to insert</param>
        public void UpdateCalDay(int storeId, FulfillmentCalDay calDay)
        {
            _fulfillmentCalRepository.Update(calDay);
        }

        /// <summary>
        /// Deletes a range of calendar days for a Store
        /// </summary>
        /// <param name="storeId">Store id</param>
        /// <param name="calDayIds">List of FulfillmentCalDay Id's to delete</param>
        public void DeleteCalDay(int storeId, FulfillmentCalDay calDay)
        {
         
                
                var calDays = from c in _fulfillmentCalRepository.Table
                             where
                                 c.StoreId == storeId &&
                                 c.Date == calDay.Date
                             select c;

                var tmpCalDay = calDays.FirstOrDefault();
                
                if (calDay != null)
                    _fulfillmentCalRepository.Delete(calDay);
            
        }

        #endregion
    }
}