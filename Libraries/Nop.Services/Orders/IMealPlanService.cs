using System;
using System.Collections.Generic;
using Nop.Core.Domain.Orders;

namespace Nop.Services.Orders
{
    /// <summary>
    /// Gift card service interface
    /// </summary>
    public partial interface IMealPlanService	// NU-16
    {
        /// <summary>
        /// Gets a gift card
        /// </summary>
        /// /// <param name="sourceSiteId"></param>
        /// <param name="mealPlanId">Gift card identifier</param>
        /// <returns>Gift card entry</returns>
        MealPlan GetMealPlanById(int sourceSiteId, int mealPlanId);


        List<MealPlan> GetMealPlans(int sourceSiteId, int? customerId,
           int? purchasedWithOrderItemId, bool includeProcessed, DateTime? startTime = null, DateTime? endTime = null, bool? forExport = false, bool? excelgrouped = false);
        /// <summary>
        /// Gets all meal plans
        /// </summary>
        /// <param name="sourceSiteId"></param>
        /// <param name="purchasedWithOrderItemId">Associated order ID; null to load all records</param>
        /// <param name="includeProcessed"></param>
        /// <param name="startTime">Order start time; null to load all records</param>
        /// <param name="endTime">Order end time; null to load all records</param>        
        /// <returnsMeal Planss</returns>
        List<MealPlan> GetMealPlans(int sourceSiteId, int? customerId, 
            int? purchasedWithOrderItemId, bool includeProcessed, DateTime? startTime = null, DateTime? endTime = null, bool? forExport = false);

        /// <summary>
        /// Inserts a meal plan
        /// </summary>
        /// <param name="mealPlan">Meal plan</param>
        void InsertMealPlan(MealPlan mealPlan);

        /// <summary>
        /// Updates the gift card
        /// </summary>
        /// <param name="mealPlan">Gift card</param>
        void UpdateMealPlan(MealPlan mealPlan);

        /// <summary>
        /// Deletes a gift card
        /// </summary>
        /// <param name="mealPlan">Gift card</param>
        void DeleteMealPlan(MealPlan mealPlan);

        IList<MealPlan> GeMealPlansByIds(int[] mealplanIds);
    }
}
