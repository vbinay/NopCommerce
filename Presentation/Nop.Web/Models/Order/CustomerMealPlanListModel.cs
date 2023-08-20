using System;
using System.Collections.Generic;
using Nop.Web.Framework.Mvc;

namespace Nop.Web.Models.Orders
{
    public partial class CustomerMealPlanListModel : BaseNopModel   // NU-16
    {
        public CustomerMealPlanListModel()
        {
            MealPlans = new List<MealPlanDetailsModel>();
            CardAccessSystemRecords = new List<CardAccessSystemRecordModel>();
        }

        public IList<MealPlanDetailsModel> MealPlans { get; set; }
        public IList<CardAccessSystemRecordModel> CardAccessSystemRecords { get; set; }
         



        #region Nested classes

        public partial class MealPlanDetailsModel : BaseNopEntityModel
        {
            public int OrderId { get; set; }
            public int PurchasedWithOrderItemId { get; set; }
            public string MealPlanName { get; set; }
            public string RecipientName { get; set; }
            public string RecipientAddress { get; set; }
            public string RecipientPhone { get; set; }
            public string RecipientEmail { get; set; }
            public string RecipientAcctNum { get; set; }
            public string MealPlanAmount { get; set; }
            public string IsProcessed { get; set; }
            public int? CardAccessRecordId { get; set; }
            public string CreatedOnLocal { get; set; }
        }


        public partial class CardAccessSystemRecordModel : BaseNopEntityModel
        {
            public int CustomerId { get; set; }
            public string CardHolderID { get; set; }
            public string PlanId { get; set; }
            public string Type { get; set; }
            public string AccountId { get; set; }
            public string IssuerId { get; set; }
            public string ApplicationID { get; set; }
            public string Balance { get; set; }
            public string AppliedAmount { get; set; }
            public string Status { get; set; }
            public string Hash { get; set; }
            public DateTime CreatedOnUTC { get; set; }
            public string Error { get; set; }
        }

        #endregion
    }
}