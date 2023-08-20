using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Nop.Admin.Extensions;
using Nop.Core.Domain.Stores;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Kendoui;
using Nop.Admin.Models.Orders;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.ExportImport;
using Nop.Services.Helpers;
using Nop.Services.Orders;
using Nop.Services.Localization;
using Nop.Services.Stores;
using Nop.Core.Domain.Customers;
using Nop.Web.Framework.Controllers;

namespace Nop.Admin.Controllers
{
    public class MealPlanController : BaseAdminController	// NU-32
    {
        #region Fields
        
        private readonly IMealPlanService _mealPlanService;
        private readonly IExportManager _exportManager;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IPermissionService _permissionService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly ILocalizationService _localizationService;
        private readonly IStoreService _storeService;

        #endregion

        #region Constructors

        public MealPlanController(
            IMealPlanService mealPlanService,
            IDateTimeHelper dateTimeHelper,
            IExportManager exportManager,            
            IPriceFormatter priceFormatter,
            IPermissionService permissionService,
            IWorkContext workContext,
            IStoreContext storeContext,
            ILocalizationService localizationService,
            IStoreService storeService)
        {
            _mealPlanService = mealPlanService;
            _exportManager = exportManager;
            _dateTimeHelper = dateTimeHelper;           
            _priceFormatter = priceFormatter;
            _permissionService = permissionService;

            _workContext = workContext;
            _storeContext = storeContext;
            _localizationService = localizationService;
            _storeService = storeService;
        }

        #endregion

        #region Methods

        // List
        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        public ActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();
            
            var model = new MealPlanListModel();

            model.SearchStoreId = _storeContext.CurrentStore.Id;

            return View(model);
        }

        [HttpPost, ActionName("List")]
        [FormValueRequired("exportexcel-grouped")]
        public ActionResult ExportExcelGrouped(MealPlanListModel model)
        {
            var mealplans = this.LoadMealPlans(model, false, true);
            try
            {
                var bytes = _exportManager.ExportMealPlansToXlsx(mealplans, true);

                return File(bytes, MimeTypes.TextXlsx, "MealplansGrouped.xlsx");
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("List");
            }
        }

        private List<MealPlan> LoadMealPlans(MealPlanListModel model, bool forExport, bool excelgrouped)
        {
            var startDate = (model.StartDate == null) ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.StartDate.Value, _dateTimeHelper.CurrentTimeZone);
            var endDate = (model.EndDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.EndDate.Value, _dateTimeHelper.CurrentTimeZone);
            var includeProcessed = model.IncludeProcessed;


            return _mealPlanService.GetMealPlans(_storeContext.CurrentStore.Id, null, null, includeProcessed, startDate, endDate, forExport, excelgrouped);

        }


        [HttpPost]
        public ActionResult MealPlanList(DataSourceRequest command, MealPlanListModel model)
        {

            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var mealPlans = this.LoadMealPlans(model, false);
           
            var gridModel = new DataSourceResult
            {
                Data = mealPlans.PagedForCommand(command).Select(x =>
                    {
                        var m = x.ToModel();
                        m.MealPlanAmount = _priceFormatter.FormatPrice(x.PurchasedWithOrderItem.UnitPriceInclTax, true, false);
                        m.MealPlanName = x.PurchasedWithOrderItem.Product.Name;
                        m.CreatedOnLocal = _dateTimeHelper.ConvertToUserTime(x.CreatedOnUtc, DateTimeKind.Utc).ToString("MM/dd/yy hh:mm tt");
                        m.Attributes = x.PurchasedWithOrderItem.AttributeDescription;
                        m.OrderId = x.PurchasedWithOrderItem.OrderId;
                        return m;
                    }).OrderBy(m => m.MealPlanName), 
                Total = mealPlans.Count()
            };



            return Json(gridModel);
        }




       [HttpPost]
        public ActionResult ProcessSelected(ICollection<int> selectedIds)
        {
            if (selectedIds != null)
            {
                var mealplans = _mealPlanService.GeMealPlansByIds(selectedIds.ToArray());
                foreach (var mealplan in mealplans)
                {
                    mealplan.IsProcessed = true;
                    _mealPlanService.UpdateMealPlan(mealplan);
                }
            }

            return Json(new { Result = true });
        }

      

        [HttpPost, ActionName("List")]
        [FormValueRequired("exportexcel-all")]
        public ActionResult ExportExcelAll(MealPlanListModel model)
        {
            var mealplans = this.LoadMealPlans(model, false);
            try
            {
                var bytes = _exportManager.ExportMealPlansToXlsx(mealplans, false);

                return File(bytes, MimeTypes.TextXlsx, "mealplans.xlsx");
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("List");
            }
        }

        private List<MealPlan> LoadMealPlans(MealPlanListModel model, bool forExport)
        {
            var startDate = (model.StartDate == null) ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.StartDate.Value, _dateTimeHelper.CurrentTimeZone);
            var endDate = (model.EndDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.EndDate.Value, _dateTimeHelper.CurrentTimeZone);
            var includeProcessed = model.IncludeProcessed;

            return _mealPlanService.GetMealPlans(_storeContext.CurrentStore.Id, null, null, includeProcessed, startDate, endDate, forExport);
        }



     /*   private MealPlanInfo ToMealPlanInfo(MealPlan mealPlan)
        {
            return new MealPlanInfo
                {
                    Id = mealPlan.Id,
                    CreatedOnLocal = _dateTimeHelper.ConvertToUserTime(mealPlan.CreatedOnUtc, DateTimeKind.Utc).ToString("d"),
                    IsProcessed = mealPlan.IsProcessed,
                    MealPlanName = mealPlan.PurchasedWithOrderSiteProduct.SiteProduct.Name,
                    RecipientAcctNum = mealPlan.RecipientAcctNum,
                    RecipientAddress = mealPlan.RecipientAddress,
                    RecipientEmail = mealPlan.RecipientEmail,
                    RecipientName = mealPlan.RecipientName,
                    RecipientPhone = mealPlan.RecipientPhone,
                    MealPlanAmount = _priceFormatter.FormatPrice(mealPlan.PurchasedWithOrderSiteProduct.UnitPriceExclTax, true, false)
                };
        }*/

        #endregion

        /*
        /// <summary>
        /// Added For Sorting the data
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [NonAction]
       protected MealPlanSortingEnum GetSortOrder(GridCommand command)
        {
            if (command.SortDescriptors.Any())
            {
                var sortDescriptors = command.SortDescriptors.FirstOrDefault();
                var sortOrder = new MealPlanSortingEnum();

                switch (sortDescriptors.Member)
                {
                    case "MealPlanName":
                        sortOrder = sortDescriptors.SortDirection == 0 ? MealPlanSortingEnum.MealPlanNameAsc : MealPlanSortingEnum.MealPlanNameDesc;
                        break;
                    case "RecipientAcctNum":
                        sortOrder = sortDescriptors.SortDirection == 0 ? MealPlanSortingEnum.RecipientAcctNumAsc : MealPlanSortingEnum.RecipientAcctNumDesc;
                        break;
                    case "RecipientName":
                        sortOrder = sortDescriptors.SortDirection == 0 ? MealPlanSortingEnum.RecipientNameAsc : MealPlanSortingEnum.RecipientNameDesc;
                        break;
                    case "RecipientEmail":
                        sortOrder = sortDescriptors.SortDirection == 0 ? MealPlanSortingEnum.RecipientEmailAsc : MealPlanSortingEnum.RecipientEmailDesc;
                        break;
                    case "RecipientPhone":
                        sortOrder = sortDescriptors.SortDirection == 0 ? MealPlanSortingEnum.RecipientPhoneAsc : MealPlanSortingEnum.RecipientPhoneDesc;
                        break;
                    case "CreatedOnLocal":
                        sortOrder = sortDescriptors.SortDirection == 0 ? MealPlanSortingEnum.CreatedOnLocalAsc : MealPlanSortingEnum.CreatedOnLocalDesc;
                        break;
                    case "IsProcessed":
                        sortOrder = sortDescriptors.SortDirection == 0 ? MealPlanSortingEnum.IsProcessedAsc : MealPlanSortingEnum.IsProcessedDesc;
                        break;
                    case "Amount":
                        sortOrder = sortDescriptors.SortDirection == 0 ? MealPlanSortingEnum.IsProcessedAsc : MealPlanSortingEnum.AmountDesc;
                        break;
                    default:
                        sortOrder = MealPlanSortingEnum.CreatedOnUtcDesc;
                        break;
                }

                return sortOrder;
            }
            return MealPlanSortingEnum.CreatedOnUtcDesc;
        }*/


    }
}
