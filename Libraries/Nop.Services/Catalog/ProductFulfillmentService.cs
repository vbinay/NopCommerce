using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Nop.Core;
using Nop.Core.Data;
using Nop.Core.Events;
using Nop.Core.Domain.Catalog;
using System.Globalization;

namespace Nop.Services.Catalog
{
    /// <summary>
    // Site service
    /// </summary>
    public partial class ProductFulfillmentService : IProductFulfillmentService
    {
        #region Fields

        private readonly IRepository<ProductFulfillmentCalendar> _fulfillmentCalRepository;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="siteRepository">Fulfillment Calendar repository</param>
        /// <param name="eventPublisher"></param>
        public ProductFulfillmentService(IRepository<ProductFulfillmentCalendar> fulfillmentCalRepository)
        {
            _fulfillmentCalRepository = fulfillmentCalRepository;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets excluded fulfillment days for the given month
        /// </summary>
        /// <param name="productId">Store id</param>
        /// <param name="year">Year</param>
        /// <param name="month">Month</param>
        /// <returns>FulfillmentCalDay list</returns>
        public virtual IList<ProductFulfillmentCalendar> GetForMonths(int productId, int year, int month)
        {
            var fromDate = new DateTime(year, month, 1);
            var toDate = fromDate.AddMonths(1);

            var calDays = from c in _fulfillmentCalRepository.Table
                          where
                              c.ProductId == productId &&
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
        /// <param name="productId"></param>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <returns>FulfillmentCalDay list</returns>
        public virtual IList<ProductFulfillmentCalendar> GetForDateRange(int productId, DateTime fromDate, DateTime toDate)
        {
            var calDays = from c in _fulfillmentCalRepository.Table
                          where
                              c.ProductId == productId &&
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
        /// <param name="productId">Store id</param>
        /// <param name="calDay">List of FulfillmentCalDays to insert</param>
        public void InsertCalDay(int productId, ProductFulfillmentCalendar calDay)
        {
            calDay.ProductId = productId;
            _fulfillmentCalRepository.Insert(calDay);
        }



        /// <summary>
        /// Inserts a range of calendar days for a Store
        /// </summary>
        /// <param name="productId">Store id</param>
        /// <param name="calDay">List of FulfillmentCalDays to insert</param>
        public ProductFulfillmentCalendar GetCalDay(int productId, ProductFulfillmentCalendar calDay)
        {
            
             var tmpCalDays = from c in _fulfillmentCalRepository.Table
                          where
                              c.ProductId == productId &&
                              c.Date == calDay.Date
                          select c;

             return tmpCalDays.FirstOrDefault();
        }

        /// <summary>
        /// Inserts a range of calendar days for a Store
        /// </summary>
        /// <param name="productId">Store id</param>
        /// <param name="calDay">List of FulfillmentCalDays to insert</param>
        public void UpdateCalDay(int productId, ProductFulfillmentCalendar calDay)
        {
            _fulfillmentCalRepository.Update(calDay);
        }

        /// <summary>
        /// Deletes a range of calendar days for a Store
        /// </summary>
        /// <param name="productId">Store id</param>
        /// <param name="calDayIds">List of FulfillmentCalDay Id's to delete</param>
        public void DeleteCalDay(int productId, ProductFulfillmentCalendar calDay)
        {
         
                
                var calDays = from c in _fulfillmentCalRepository.Table
                             where
                                 c.ProductId == productId &&
                                 c.Date == calDay.Date
                             select c;

                var tmpCalDay = calDays.FirstOrDefault();
                
                if (calDay != null)
                    _fulfillmentCalRepository.Delete(calDay);
            
        }

        /// <summary>
        /// Deletes a range of calendar days for a Store
        /// </summary>
        /// <param name="productId">Store id</param>
        public void DeleteAllCalDays(int productId)
        {
            var calDays = from c in _fulfillmentCalRepository.Table
                          where
                              c.ProductId == productId
                          select c;
            if (calDays != null)
            {
                foreach (var calDay in calDays)
                _fulfillmentCalRepository.Delete(calDay);
            }
        }

        

        #endregion
    }
}