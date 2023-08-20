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
using Nop.Core.Domain.Catalog;
using Nop.Services.Tax;
using Nop.Web.Framework.Security;
using Nop.Core.Domain.Staging;

namespace Nop.Admin.Controllers
{
    public class StagingController : BaseAdminController	// NU-32
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
        private readonly IProductService _productService;
        private readonly ITaxCategoryService _taxCategoryService;
        private readonly IGlCodeService _glcodeService;

        #endregion

        #region Constructors

        public StagingController(
            IMealPlanService mealPlanService,
            IDateTimeHelper dateTimeHelper,
            IExportManager exportManager,
            IPriceFormatter priceFormatter,
            IPermissionService permissionService,
            IWorkContext workContext,
            IStoreContext storeContext,
            ILocalizationService localizationService,
            IStoreService storeService, IProductService productService,
            ITaxCategoryService taxCategoryService,
            IGlCodeService glcodeService)
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
            _productService = productService;
            _glcodeService = glcodeService;
            _taxCategoryService = taxCategoryService;
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

            var model = new StagingListModel();

            var stores = this._storeService.GetAllStores();

            foreach (var s in _storeService.GetAllStores().Where(s => s.IsEnabled.GetValueOrDefault(true)).OrderBy(s => s.Name))
                model.AvailableStores.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString() });

            model.AvailableTaxCategories.Add(new SelectListItem { Text = "None", Value = "0" });
            foreach (var s in _taxCategoryService.GetAllTaxCategories().OrderBy(s => s.Code))
                model.AvailableTaxCategories.Add(new SelectListItem { Text = s.Code + " " + s.Name, Value = s.Id.ToString() });

            model.AvailableGLCodes.Add(new SelectListItem { Text = "None", Value = "0" });
            foreach (var s in _glcodeService.GetAllGlCodes().Where(s => s.IsDelivered == false).OrderBy(s => s.GlCodeName))
                model.AvailableGLCodes.Add(new SelectListItem { Text = s.GlCodeName, Value = s.Id.ToString() });


            model.AvailablePublish.Add(new SelectListItem { Text = "Published", Value = "1" });
            model.AvailablePublish.Add(new SelectListItem { Text = "Unpublished", Value = "" });


          


                  model.AvailableMasterMealPlans.Add(new SelectListItem { Text = "None", Value = "0" });
                  
                  
                  model.AvailableMasterMealPlans.Add(new SelectListItem { Text = "Student Mandatory Block", Value = "21098" });
                  
                  model.AvailableMasterMealPlans.Add(new SelectListItem { Text = "Student Mandatory Block + DCB", Value = "21100" });
            model.AvailableMasterMealPlans.Add(new SelectListItem { Text = "Student Mandatory Block + xFlex", Value = "21099" });
            
            model.AvailableMasterMealPlans.Add(new SelectListItem { Text = "Student Mandatory Meals", Value = "21096" });
            model.AvailableMasterMealPlans.Add(new SelectListItem { Text = "Student Mandatory Meals + Xflex", Value = "21097" });
            model.AvailableMasterMealPlans.Add(new SelectListItem { Text = "Student Mandatory xFlex/DCB", Value = "21438" });
            model.AvailableMasterMealPlans.Add(new SelectListItem { Text = "Student Mandatory xFlex/DCB + Bonus", Value = "21437" });
            model.AvailableMasterMealPlans.Add(new SelectListItem { Text = "Student Mandatory Block/Meals/Points + xFlex/DCB + Bonus", Value = "21436" });

            model.AvailableMasterMealPlans.Add(new SelectListItem { Text = "Student Voluntary DCB", Value = "21102" });
            model.AvailableMasterMealPlans.Add(new SelectListItem { Text = "Student Voluntary Flex", Value = "21101" });            
            model.AvailableMasterMealPlans.Add(new SelectListItem { Text = "Student Voluntary Meals", Value = "21104" });
            model.AvailableMasterMealPlans.Add(new SelectListItem { Text = "Student Voluntary Points", Value = "21103" });
            model.AvailableMasterMealPlans.Add(new SelectListItem { Text = "Student Voluntary xFlex/DCB + Bonus", Value = "21439" });
            model.AvailableMasterMealPlans.Add(new SelectListItem { Text = "Student Voluntary Meals/Blocks/Points + xFlex/DCB", Value = "21440" });
            model.AvailableMasterMealPlans.Add(new SelectListItem { Text = "Student Voluntary Meals/Blocks/Points + xFlex/DCB + Bonus", Value = "21442" });

            model.AvailableMasterMealPlans.Add(new SelectListItem { Text = "Faculty Block", Value = "21107" });
            model.AvailableMasterMealPlans.Add(new SelectListItem { Text = "Faculty DCB", Value = "21106" });
            model.AvailableMasterMealPlans.Add(new SelectListItem { Text = "Faculty Meals", Value = "21105" });
            model.AvailableMasterMealPlans.Add(new SelectListItem { Text = "Faculty Meals/Blocks + xFlex/DCB", Value = "21444" });
            model.AvailableMasterMealPlans.Add(new SelectListItem { Text = "Faculty DCB/Flex + Bonus", Value = "21445" });
            model.AvailableMasterMealPlans.Add(new SelectListItem { Text = "Faculty Meals/Blocks/Points + xFles/DCB + Bonus", Value = "21443" });

            return View(model);
        }



        public ActionResult MasterList()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var model = new StagingListModel();

            model.AvailableTaxCategories.Add(new SelectListItem { Text = "None", Value = "0" });
            foreach (var s in _taxCategoryService.GetAllTaxCategories().OrderBy(s => s.Code))
                model.AvailableTaxCategories.Add(new SelectListItem { Text = s.Code + " " + s.Name, Value = s.Id.ToString() });

            model.AvailableGLCodes.Add(new SelectListItem { Text = "None", Value = "0" });
            foreach (var s in _glcodeService.GetAllGlCodes().Where(s=> s.IsDelivered == true).OrderBy(s => s.GlCodeName))
                model.AvailableGLCodes.Add(new SelectListItem { Text = s.GlCodeName, Value = s.Id.ToString() });

            return View(model);
        }


        [HttpPost]
        public ActionResult StagingMasterProductList(DataSourceRequest command, StagingListModel model)
        {

            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();


            var products = _productService.SearchProducts(
               overrideHideMasterProducts: false,
               showHidden: true
              // overridePublished: false
            );


            var mealProducts = new List<Product>();


            foreach (var product in products)
            {
                if (!product.IsMealPlan)
                {
                    mealProducts.Add(product);
                }
            }

            var gridModel = new DataSourceResult
            {
                Data = mealProducts.Select(x =>
                {
                    var stagingMapping = _productService.GetStagingMapping(x.Id);
                    var m = new StagingHelperProduct();
                    m.Id = stagingMapping.Id;
                    m.Name = x.Name;
                    m.FullDescription = x.FullDescription;
                    m.Published = x.Published;
                    m.TaxCategoryId = stagingMapping.TaxCategoryId;
                    m.GLCode1Id = stagingMapping.GlCode1Id;
                    m.GLCode1Amount = stagingMapping.GlCode1Amount;

                    m.GLCode2Id = stagingMapping.GlCode2Id;
                    m.GLCode2Amount = stagingMapping.GLCode2Amount;

                    m.GLCode3Id = stagingMapping.GlCode3Id;
                    m.GLCode3Amount = stagingMapping.GLCode3Amount;

                    m.Price = x.Price;
                    m.ProductId = x.Id;
                    m.Completed = stagingMapping.Completed;
                    m.Comments = stagingMapping.Comments;
      
                    return m;
                }).OrderBy(m => m.Name),
                Total = mealProducts.Count()
            };

            return Json(gridModel);
        }


        [HttpPost]
        public ActionResult StagingProductList(DataSourceRequest command, StagingListModel model)
        {

            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();


            var products = _productService.SearchProducts(
                storeId: model.SearchStoreId,
                //overridePublished: (model.Published == "1"),
                pageIndex: command.Page - 1,
                pageSize: 100000,
                showHidden: true
             );



            var mealProducts = new List<Product>();


            foreach (var product in products)
            {
                if (product.IsMealPlan)
                {
                    mealProducts.Add(product);
                }
            }

            var gridModel = new DataSourceResult
            {
                Data = mealProducts.Select(x =>
                {
                    var stagingMapping = _productService.GetStagingMapping(x.Id);
                    var m = new StagingHelperProduct();
                    m.Name = x.Name;
                    m.Id = stagingMapping.Id;
                    m.ShortDescription = x.ShortDescription;
                    m.FullDescription = x.FullDescription;
                    m.Published = x.Published;
                    m.TaxCategoryId = stagingMapping.TaxCategoryId;

                    m.GLCode1Id = stagingMapping.GlCode1Id;
                    m.GLCode1Amount = stagingMapping.GlCode1Amount;

                    m.GLCode2Id = stagingMapping.GlCode2Id;
                    m.GLCode2Amount = stagingMapping.GLCode2Amount;

                    m.GLCode3Id = stagingMapping.GlCode3Id;
                    m.GLCode3Amount = stagingMapping.GLCode3Amount;

                    m.Price = x.Price;
                    m.ProductId = x.Id;
                    m.Completed = stagingMapping.Completed;
                    m.Comments = stagingMapping.Comments;

                    m.MasterProductId = stagingMapping.MasterProductId;
                    return m;
                }).OrderBy(m => m.Name),
                Total = mealProducts.Count()
            };

            return Json(gridModel);
        }



      
        /// <summary>
        /// 
        /// </summary>
        /// <param name="productDetailsArr"></param>
        /// <returns></returns>
        [AdminAntiForgery(true)]
        public ActionResult SaveStagingProductsInfo(StagingHelperProduct[] productDetailsArr)
        {
            if (productDetailsArr != null)
            {

                if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                    return AccessDeniedView();

                foreach (var productitems in productDetailsArr)
                {
                    if (productitems.Completed.GetValueOrDefault(false))
                    {

                        if (!_productService.StagingProductExists(productitems.ProductId))
                        {
                            var stagingProducts = new StagingProducts
                            {
                                Id = productitems.Id,
                                ProductId = productitems.ProductId,
                                TaxCategoryId = productitems.TaxCategoryId,
                                GlCode1Id = productitems.GLCode1Id,
                                GlCode1Amount = productitems.GLCode1Amount,
                                GlCode2Id = productitems.GLCode2Id,
                                GLCode2Amount = productitems.GLCode2Amount,
                                GlCode3Id = productitems.GLCode3Id,
                                GLCode3Amount = productitems.GLCode3Amount,
                                Comments = productitems.Comments,
                                Completed = productitems.Completed,
                                MasterProductId = productitems.MasterProductId
                            };
                            _productService.InsertStagingProductInfo(stagingProducts);
                        }
                        else
                        {
                            var stagingProducts = new StagingProducts
                            {
                                Id = productitems.Id,
                                ProductId = productitems.ProductId,
                                TaxCategoryId = productitems.TaxCategoryId,
                                GlCode1Id = productitems.GLCode1Id,
                                GlCode1Amount = productitems.GLCode1Amount,
                                GlCode2Id = productitems.GLCode2Id,
                                GLCode2Amount = productitems.GLCode2Amount,
                                GlCode3Id = productitems.GLCode3Id,
                                GLCode3Amount = productitems.GLCode3Amount,
                                Comments = productitems.Comments,
                                Completed = productitems.Completed,
                                MasterProductId = productitems.MasterProductId
                            };
                            _productService.UpdateStagingMapping(stagingProducts);
                        }
                    }
                }
                return Json(new { Result = "Records inserted successfully" });
            }
            else
            {
                return Json(new { Result = "Failed to inset records" });
            }
        }
        #endregion
    }


    public class StagingHelperProduct
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string Name { get; set; }
        public string ShortDescription { get; set; }
        public string FullDescription { get; set; }
        public bool Published { get; set; }
        public int? TaxCategoryId { get; set; }

        public int? GLCode1Id { get; set; }

        public decimal? GLCode1Amount { get; set; }


        public int? GLCode2Id { get; set; }
        public decimal? GLCode2Amount { get; set; }

        public int? GLCode3Id { get; set; }
        public decimal? GLCode3Amount { get; set; }


        public int? GLCodeDeliveryId { get; set; }
        public decimal? Price { get; set; }

        public bool? Completed { get; set; }
        public string Comments { get; set; }

        public int? MasterProductId { get; set; }
    }
}
