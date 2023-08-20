using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Services.Events;
using System.Collections;
using Nop.Services.Helpers;
using Nop.Services.Stores;

namespace Nop.Services.Orders
{
    /// <summary>
    /// Meal plan service
    /// </summary>
    public partial class MealPlanService : IMealPlanService	// NU-32
    {
        #region Fields

        private readonly IRepository<MealPlan> _mealPlanRepository;
        private readonly IEventPublisher _eventPublisher;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IStoreMappingService _storeMappingService;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="mealPlanRepository">Meal plan context</param>
        /// <param name="eventPublisher"></param>
        /// <param name="storeMappingServices"></param>
        public MealPlanService(IRepository<MealPlan> mealPlanRepository, IEventPublisher eventPublisher, IDateTimeHelper dateTimeHelper, IStoreMappingService storeMappingService)
        {
            this._mealPlanRepository = mealPlanRepository;
            _eventPublisher = eventPublisher;
            this._dateTimeHelper = dateTimeHelper;
            this._storeMappingService = storeMappingService;
        }

        #endregion

        #region Methods


        /// <summary>
        /// Gets all meal plans
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="purchasedWithOrderItemId">Associated order ID; null to load all records</param>
        /// <param name="includeProcessed"></param>
        /// <param name="startTime">Order start time; null to load all records</param>
        /// <param name="endTime">Order end time; null to load all records</param>
        /// <returns>Meal plans</returns>
        public virtual List<MealPlan> GetMealPlans(int storeId, int? customerId, int? purchasedWithOrderItemId, bool includeProcessed, DateTime? startTime = null, DateTime? endTime = null, Boolean? forExport = false, Boolean? ExcelGrouped = false)
        {

            var query = this._mealPlanRepository.Table;

            if (customerId.HasValue)
            {
                query = query.Where(mp =>
                 mp.PurchasedWithOrderItem != null &&
                 mp.PurchasedWithOrderItem.Order.CustomerId == customerId.Value);
            }


            if (startTime != null)
                startTime = new DateTime(startTime.Value.Year, startTime.Value.Month, startTime.Value.Day, 0, 0, 0);

            if (endTime != null)
                endTime = new DateTime(endTime.Value.Year, endTime.Value.Month, endTime.Value.Day, 23, 59, 59);

            if (purchasedWithOrderItemId.HasValue)
                query = query.Where(mp =>
                    mp.PurchasedWithOrderItem != null &&
                    mp.PurchasedWithOrderItem.OrderId == purchasedWithOrderItemId.Value);


            if (ExcelGrouped == true)
                query = query.GroupBy(c => new { c.PurchasedWithOrderItem.Id, c.RecipientAcctNum }, (key, c) => c.FirstOrDefault());

            if (!includeProcessed)
                query = query.Where(mp => mp.IsProcessed == false);

            if (startTime.HasValue)
                query = query.Where(mp => startTime.Value <= mp.CreatedOnUtc);

            if (endTime.HasValue)
                query = query.Where(mp => endTime.Value >= mp.CreatedOnUtc);



            List<MealPlan> mealPlanList = new List<MealPlan>();


            foreach (var mp in query.ToList())
            {
                if (_storeMappingService.Authorize(mp.PurchasedWithOrderItem.Product, storeId))
                {
                    mealPlanList.Add(mp);
                }
            }

            if (forExport == true)
            {
                Hashtable hashtable = new Hashtable();

                foreach (MealPlan plan in mealPlanList)
                {



                    String acctAndOrderNumber = plan.RecipientAcctNum.Trim().ToLower() + " " + plan.PurchasedWithOrderItem.Product.Name + " " + _dateTimeHelper.ConvertToUserTime(plan.CreatedOnUtc, DateTimeKind.Utc).ToString("d");
                    if (hashtable.ContainsKey(acctAndOrderNumber))
                    {

                        MealPlan innerPlan = (MealPlan)hashtable[acctAndOrderNumber];
                        decimal plan1Value = plan.PurchasedWithOrderItem.UnitPriceExclTax;
                        decimal plan2Value = innerPlan.PurchasedWithOrderItem.UnitPriceExclTax;
                        innerPlan.PurchasedWithOrderItem.UnitPriceExclTax = Decimal.Add(plan1Value, plan2Value);
                        hashtable.Remove(acctAndOrderNumber);
                        hashtable.Add(acctAndOrderNumber, innerPlan);
                    }
                    else
                    {

                        MealPlan newPlanCopy = new MealPlan();
                        var orderItem = new OrderItem();
                        orderItem.PriceExclTax = plan.PurchasedWithOrderItem.PriceExclTax;
                        orderItem.UnitPriceExclTax = plan.PurchasedWithOrderItem.UnitPriceExclTax;
                        orderItem.Product = plan.PurchasedWithOrderItem.Product;

                        newPlanCopy.Id = plan.Id;
                        newPlanCopy.PurchasedWithOrderItem = orderItem;
                        newPlanCopy.PurchasedWithOrderItem.Id = orderItem.Id;
                        newPlanCopy.RecipientAcctNum = plan.RecipientAcctNum;
                        newPlanCopy.RecipientAddress = plan.RecipientAddress;
                        newPlanCopy.RecipientEmail = plan.RecipientEmail;
                        newPlanCopy.RecipientName = plan.RecipientName;
                        newPlanCopy.RecipientPhone = plan.RecipientPhone;
                        newPlanCopy.CreatedOnUtc = plan.CreatedOnUtc;
                        newPlanCopy.IsProcessed = plan.IsProcessed;

                        hashtable.Add(acctAndOrderNumber, newPlanCopy);
                    }
                }
                return hashtable.Values.Cast<MealPlan>().ToList();
            }
            else
            {
                return mealPlanList;
            }

        }

        /// <summary>
        /// Deletes a meal plan
        /// </summary>
        /// <param name="mealPlan">Meal plan</param>
        public virtual void DeleteMealPlan(MealPlan mealPlan)
        {
            if (mealPlan == null)
                throw new ArgumentNullException("mealPlan");

            this._mealPlanRepository.Delete(mealPlan);

            //event notification
            _eventPublisher.EntityDeleted(mealPlan);
        }

        /// <summary>
        /// Gets a meal plan
        /// </summary>
        /// /// <param name="sourceSiteId"></param>
        /// <param name="mealPlanId">Meal plan identifier</param>
        /// <returns>Meal plan entry</returns>
        public virtual MealPlan GetMealPlanById(int storeId, int mealPlanId)
        {
            if (mealPlanId == 0)
                return null;

            var mealPlan = this._mealPlanRepository.Table.FirstOrDefault(gc => gc.Id == mealPlanId);

            return mealPlan;
        }

        /// <summary>
        /// Gets all meal plans
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="purchasedWithOrderItemId">Associated order ID; null to load all records</param>
        /// <param name="includeProcessed"></param>
        /// <param name="startTime">Order start time; null to load all records</param>
        /// <param name="endTime">Order end time; null to load all records</param>
        /// <returns>Meal plans</returns>
        public virtual List<MealPlan> GetMealPlans(int storeId, int? customerId, int? purchasedWithOrderItemId, bool includeProcessed, DateTime? startTime = null, DateTime? endTime = null, Boolean? forExport = false)
        {

            var query = this._mealPlanRepository.Table;

            if (customerId.HasValue)
            {
                query = query.Where(mp =>
                 mp.PurchasedWithOrderItem != null &&
                 mp.PurchasedWithOrderItem.Order.CustomerId == customerId.Value);
            }


            if (startTime != null)
                startTime = new DateTime(startTime.Value.Year, startTime.Value.Month, startTime.Value.Day, 0, 0, 0);

            if (endTime != null)
                endTime = new DateTime(endTime.Value.Year, endTime.Value.Month, endTime.Value.Day, 23, 59, 59);

            if (purchasedWithOrderItemId.HasValue)
                query = query.Where(mp =>
                    mp.PurchasedWithOrderItem != null &&
                    mp.PurchasedWithOrderItem.OrderId == purchasedWithOrderItemId.Value);

            if (!includeProcessed)
                query = query.Where(mp => mp.IsProcessed == false);

            if (startTime.HasValue)
                query = query.Where(mp => startTime.Value <= mp.CreatedOnUtc);

            if (endTime.HasValue)
                query = query.Where(mp => endTime.Value >= mp.CreatedOnUtc);

            List<MealPlan> mealPlanList = new List<MealPlan>();


            foreach (var mp in query.ToList())
            {
                if (_storeMappingService.Authorize(mp.PurchasedWithOrderItem.Product, storeId))
                {
                    mealPlanList.Add(mp);
                }
            }

            if (forExport == true)
            {
                Hashtable hashtable = new Hashtable();

                foreach (MealPlan plan in mealPlanList)
                {



                    String acctAndOrderNumber = plan.RecipientAcctNum.Trim().ToLower() + " " + plan.PurchasedWithOrderItem.Product.Name + " " + _dateTimeHelper.ConvertToUserTime(plan.CreatedOnUtc, DateTimeKind.Utc).ToString("d");
                    if (hashtable.ContainsKey(acctAndOrderNumber))
                    {

                        MealPlan innerPlan = (MealPlan)hashtable[acctAndOrderNumber];
                        decimal plan1Value = plan.PurchasedWithOrderItem.UnitPriceExclTax;
                        decimal plan2Value = innerPlan.PurchasedWithOrderItem.UnitPriceExclTax;
                        innerPlan.PurchasedWithOrderItem.UnitPriceExclTax = Decimal.Add(plan1Value, plan2Value);
                        hashtable.Remove(acctAndOrderNumber);
                        hashtable.Add(acctAndOrderNumber, innerPlan);
                    }
                    else
                    {

                        MealPlan newPlanCopy = new MealPlan();
                        var orderItem = new OrderItem();
                        orderItem.PriceExclTax = plan.PurchasedWithOrderItem.PriceExclTax;
                        orderItem.UnitPriceExclTax = plan.PurchasedWithOrderItem.UnitPriceExclTax;
                        orderItem.Product = plan.PurchasedWithOrderItem.Product;

                        newPlanCopy.Id = plan.Id;
                        newPlanCopy.PurchasedWithOrderItem = orderItem;
                        newPlanCopy.PurchasedWithOrderItem.Id = orderItem.Id;
                        newPlanCopy.RecipientAcctNum = plan.RecipientAcctNum;
                        newPlanCopy.RecipientAddress = plan.RecipientAddress;
                        newPlanCopy.RecipientEmail = plan.RecipientEmail;
                        newPlanCopy.RecipientName = plan.RecipientName;
                        newPlanCopy.RecipientPhone = plan.RecipientPhone;
                        newPlanCopy.CreatedOnUtc = plan.CreatedOnUtc;
                        newPlanCopy.IsProcessed = plan.IsProcessed;

                        hashtable.Add(acctAndOrderNumber, newPlanCopy);
                    }
                }
                return hashtable.Values.Cast<MealPlan>().ToList();
            }
            else
            {
                return mealPlanList;
            }


        }


        /// <summary>
        /// Gets all meal plans
        /// </summary>
        /// <param name="sourceSiteId"></param>
        /// <param name="PurchasedWithOrderItemId">Associated order ID; null to load all records</param>
        /// <param name="includeProcessed"></param>
        /// <param name="startTime">Order start time; null to load all records</param>
        /// <param name="endTime">Order end time; null to load all records</param>
        /// <returns>Meal plans</returns>
        public virtual List<MealPlan> GetMealPlansForExport(int storeId, int? PurchasedWithOrderItemId, bool includeProcessed, MealPlanSortingEnum sortingEnum, DateTime? startTime = null, DateTime? endTime = null)
        {

            var query = this._mealPlanRepository.Table;//.Where(mp => mp.PurchasedWithOrderItem.SiteProduct.SourceSiteId == sourceSiteId);

            if (PurchasedWithOrderItemId.HasValue)
                query = query.Where(mp =>
                    mp.PurchasedWithOrderItem != null &&
                    mp.PurchasedWithOrderItem.OrderId == PurchasedWithOrderItemId.Value);

            if (!includeProcessed)
                query = query.Where(mp => mp.IsProcessed == false);

            if (startTime.HasValue)
                query = query.Where(mp => startTime.Value <= mp.CreatedOnUtc);

            if (endTime.HasValue)
                query = query.Where(mp => endTime.Value >= mp.CreatedOnUtc);


            List<MealPlan> listOfMealPlans = SortData(sortingEnum, query).ToList();




            return SortData(sortingEnum, query).ToList();
        }



        /// <summary>
        /// Inserts a meal plan
        /// </summary>
        /// <param name="mealPlan">Meal plan</param>
        public virtual void InsertMealPlan(MealPlan mealPlan)
        {
            if (mealPlan == null)
                throw new ArgumentNullException("mealPlan");

            this._mealPlanRepository.Insert(mealPlan);


            //event notification
            _eventPublisher.EntityInserted(mealPlan);
        }

        /// <summary>
        /// Updates the meal plan
        /// </summary>
        /// <param name="mealPlan">Meal plan</param>
        public virtual void UpdateMealPlan(MealPlan mealPlan)
        {
            if (mealPlan == null)
                throw new ArgumentNullException("mealPlan");

            this._mealPlanRepository.Update(mealPlan);

            //event notification
            _eventPublisher.EntityUpdated(mealPlan);
        }



        private IQueryable<MealPlan> SortData(MealPlanSortingEnum sortOrder, IQueryable<MealPlan> mealPlans)
        {
            //Temporary list for dealing with certain columns
            var tempOrders = new List<MealPlan>();
            //Dictionary of Customers
            var orderDictionary = new Dictionary<int, string>();
            //Switch statement on Sort Order for grid
            switch (sortOrder)
            {
                case MealPlanSortingEnum.MealPlanNameAsc:
                    mealPlans = mealPlans.OrderBy(t => t.PurchasedWithOrderItem.Product.Name);
                    break;
                case MealPlanSortingEnum.MealPlanNameDesc:
                    mealPlans = mealPlans.OrderByDescending(t => t.PurchasedWithOrderItem.Product.Name);
                    break;
                case MealPlanSortingEnum.RecipientNameAsc:
                    mealPlans = mealPlans.OrderBy(t => t.RecipientName);
                    break;
                case MealPlanSortingEnum.RecipientNameDesc:
                    mealPlans = mealPlans.OrderByDescending(t => t.RecipientName);
                    break;
                case MealPlanSortingEnum.RecipientAcctNumAsc:
                    mealPlans = mealPlans.OrderBy(t => t.RecipientAcctNum);
                    break;
                case MealPlanSortingEnum.RecipientAcctNumDesc:
                    mealPlans = mealPlans.OrderByDescending(t => t.RecipientAcctNum);
                    break;
                case MealPlanSortingEnum.RecipientAddressAsc:
                    mealPlans = mealPlans.OrderBy(t => t.RecipientAddress);
                    break;
                case MealPlanSortingEnum.RecipientAddressDesc:
                    mealPlans = mealPlans.OrderByDescending(t => t.RecipientAddress);
                    break;
                case MealPlanSortingEnum.RecipientEmailAsc:
                    mealPlans = mealPlans.OrderBy(t => t.RecipientEmail);
                    break;
                case MealPlanSortingEnum.RecipientEmailDesc:
                    mealPlans = mealPlans.OrderByDescending(t => t.RecipientEmail);
                    break;
                case MealPlanSortingEnum.RecipientPhoneAsc:
                    mealPlans = mealPlans.OrderBy(t => t.RecipientPhone);
                    break;
                case MealPlanSortingEnum.RecipientPhoneDesc:
                    mealPlans = mealPlans.OrderByDescending(t => t.RecipientPhone);
                    break;
                case MealPlanSortingEnum.CreatedOnUtcAsc:
                    mealPlans = mealPlans.OrderBy(t => t.CreatedOnUtc);
                    break;
                case MealPlanSortingEnum.CreatedOnUtcDesc:
                    mealPlans = mealPlans.OrderByDescending(t => t.CreatedOnUtc);
                    break;
                case MealPlanSortingEnum.IsProcessedAsc:
                    mealPlans = mealPlans.OrderBy(t => t.IsProcessed);
                    break;
                case MealPlanSortingEnum.IsProcessedDesc:
                    mealPlans = mealPlans.OrderByDescending(t => t.IsProcessed);
                    break;
                case MealPlanSortingEnum.AmountAsc:
                    mealPlans = mealPlans.OrderBy(t => t.PurchasedWithOrderItem.Order.OrderSubtotalExclTax);
                    break;
                case MealPlanSortingEnum.AmountDesc:
                    mealPlans = mealPlans.OrderByDescending(t => t.PurchasedWithOrderItem.Order.OrderSubtotalExclTax);
                    break;
            }

            return mealPlans;
        }

        public virtual IList<MealPlan> GeMealPlansByIds(int[] mealplanIds)
        {
            if (mealplanIds == null || mealplanIds.Length == 0)
                return new List<MealPlan>();

            var query = from pr in _mealPlanRepository.Table
                        where mealplanIds.Contains(pr.Id)
                        select pr;
            var mealplans = query.ToList();
            //sort by passed identifiers
            var sortedMealplans = new List<MealPlan>();
            foreach (int id in mealplanIds)
            {
                var mealplan = mealplans.Find(x => x.Id == id);
                if (mealplan != null)
                    sortedMealplans.Add(mealplan);
            }
            return sortedMealplans;
        }

        #endregion
    }
}
