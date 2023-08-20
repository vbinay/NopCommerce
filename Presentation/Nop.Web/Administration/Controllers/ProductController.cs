using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nop.Admin.Extensions;
using Nop.Admin.Helpers;
using Nop.Admin.Infrastructure.Cache;
using Nop.Admin.Models.Catalog;
using Nop.Admin.Models.Orders;
using Nop.Admin.Models.Tax;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Logging;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Tax;
using Nop.Core.Domain.Vendors;
using Nop.Services;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.ExportImport;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Shipping;
using Nop.Services.Stores;
using Nop.Services.Tax;
using Nop.Services.Vendors;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using Nop.Web.Framework.Mvc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Nop.Admin.Controllers
{
    public partial class ProductController : BaseAdminController
    {
        #region Fields

        private readonly IProductService _productService;
        private readonly IProductTemplateService _productTemplateService;
        private readonly ICategoryService _categoryService;
        private readonly IManufacturerService _manufacturerService;
        private readonly ICustomerService _customerService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IWorkContext _workContext;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IPictureService _pictureService;
        private readonly ITaxCategoryService _taxCategoryService;
        private readonly IProductTagService _productTagService;
        private readonly ICopyProductService _copyProductService;
        private readonly IPdfService _pdfService;
        private readonly IExportManager _exportManager;
        private readonly IImportManager _importManager;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IPermissionService _permissionService;
        private readonly IAclService _aclService;
        private readonly IStoreService _storeService;
        private readonly IOrderService _orderService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IVendorService _vendorService;
        private readonly IShippingService _shippingService;
        private readonly IShipmentService _shipmentService;
        private readonly ICurrencyService _currencyService;
        private readonly CurrencySettings _currencySettings;
        private readonly IMeasureService _measureService;
        private readonly MeasureSettings _measureSettings;
        private readonly ICacheManager _cacheManager;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IDiscountService _discountService;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IBackInStockSubscriptionService _backInStockSubscriptionService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IProductAttributeFormatter _productAttributeFormatter;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IDownloadService _downloadService;
        private readonly ISettingService _settingService;
        private readonly VendorSettings _vendorSettings;
        private readonly IStoreContext _storeContext;
        private readonly IVendorMappingService _vendorMappingService;
        private readonly IGlCodeService _glCodeService; //NU-90 
        private readonly ILogger _logger;
        private readonly IProductFulfillmentService _productFulfillmentService;

        #endregion

        #region Constructors

        public ProductController(IProductService productService,
            IProductTemplateService productTemplateService,
            ICategoryService categoryService,
            IManufacturerService manufacturerService,
            ICustomerService customerService,
            IUrlRecordService urlRecordService,
            IWorkContext workContext,
            ILanguageService languageService,
            ILocalizationService localizationService,
            ILocalizedEntityService localizedEntityService,
            ISpecificationAttributeService specificationAttributeService,
            IPictureService pictureService,
            ITaxCategoryService taxCategoryService,
            IProductTagService productTagService,
            ICopyProductService copyProductService,
            IPdfService pdfService,
            IExportManager exportManager,
            IImportManager importManager,
            ICustomerActivityService customerActivityService,
            IPermissionService permissionService,
            IAclService aclService,
            IStoreService storeService,
            IOrderService orderService,
            IStoreMappingService storeMappingService,
             IVendorService vendorService,
            IShippingService shippingService,
            IShipmentService shipmentService,
            ICurrencyService currencyService,
            CurrencySettings currencySettings,
            IMeasureService measureService,
            MeasureSettings measureSettings,
            ICacheManager cacheManager,
            IDateTimeHelper dateTimeHelper,
            IDiscountService discountService,
            IProductAttributeService productAttributeService,
            IBackInStockSubscriptionService backInStockSubscriptionService,
            IShoppingCartService shoppingCartService,
            IProductAttributeFormatter productAttributeFormatter,
            IProductAttributeParser productAttributeParser,
            IDownloadService downloadService,
            ISettingService settingService,
            VendorSettings vendorSettings,
            IGlCodeService glCodeService,
            IStoreContext storeContext,
            ILogger logger,
            IVendorMappingService vendorMappingService,
            IProductFulfillmentService productFulfillmentService)
        {
            this._productService = productService;
            this._productTemplateService = productTemplateService;
            this._categoryService = categoryService;
            this._manufacturerService = manufacturerService;
            this._customerService = customerService;
            this._urlRecordService = urlRecordService;
            this._workContext = workContext;
            this._languageService = languageService;
            this._localizationService = localizationService;
            this._localizedEntityService = localizedEntityService;
            this._specificationAttributeService = specificationAttributeService;
            this._pictureService = pictureService;
            this._taxCategoryService = taxCategoryService;

            this._productTagService = productTagService;
            this._copyProductService = copyProductService;
            this._pdfService = pdfService;
            this._exportManager = exportManager;
            this._importManager = importManager;
            this._customerActivityService = customerActivityService;
            this._permissionService = permissionService;
            this._aclService = aclService;
            this._storeService = storeService;
            this._orderService = orderService;
            this._storeMappingService = storeMappingService;
            this._vendorService = vendorService;
            this._shippingService = shippingService;
            this._shipmentService = shipmentService;
            this._currencyService = currencyService;
            this._currencySettings = currencySettings;
            this._measureService = measureService;
            this._measureSettings = measureSettings;
            this._cacheManager = cacheManager;
            this._dateTimeHelper = dateTimeHelper;
            this._discountService = discountService;
            this._productAttributeService = productAttributeService;
            this._backInStockSubscriptionService = backInStockSubscriptionService;
            this._shoppingCartService = shoppingCartService;
            this._productAttributeFormatter = productAttributeFormatter;
            this._productAttributeParser = productAttributeParser;
            this._downloadService = downloadService;
            this._settingService = settingService;
            this._vendorSettings = vendorSettings;
            this._storeContext = storeContext;
            this._vendorMappingService = vendorMappingService;
            this._glCodeService = glCodeService;
            this._logger = logger;
            this._productFulfillmentService = productFulfillmentService;
        }

        #endregion

        #region Product Fulfillment calendar

        public ActionResult ProductFulfillmentCalendar()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            return View("_ProductFulfillmentCalendar");
        }

        public JsonResult GetFulfillmentAvailablility(string date, int productId)
        {
            int ProductId = 0;
            if (Request.RawUrl.IndexOf("Edit/") > 0)
            {
                int strt = Request.RawUrl.IndexOf("Edit/") + 5;
                int length = Request.RawUrl.Length - strt;
                ProductId = int.Parse(Request.RawUrl.Substring(strt, length));
            }

            if (ProductId > 0)
            {
                productId = ProductId;
            }

            var dateTime = DateTime.Parse(date.Replace("\"", ""));
            var ApptListForDate = _productFulfillmentService.GetForMonths(productId, dateTime.Year, dateTime.Month);
            var eventList = from e in ApptListForDate
                            select new
                            {
                                id = e.Id,
                                name = "Closed",
                                //event_date = e.Date.ToString("yyyy-MM-dd"),	// SODMYWAY-2780
                                startdate = e.Date.ToString("yyyy-MM-dd"),
                                enddate = e.Date.ToString("yyyy-MM-dd"),
                                color = "#d3d3d3",//e.StatusColor,
                                //baccolor = "#01DF3A",//e.StatusColor,	// SODMYWAY-2780
                                //className = e.ClassName,	// SODMYWAY-2780
                                //someKey = e.SomeImportantKeyID,	// SODMYWAY-2780
                                //allDay = true	// SODMYWAY-2780
                            };
            var rows = eventList.ToArray();
            return Json(rows, JsonRequestBehavior.AllowGet);
        }


        public JsonResult SaveEvent(string date, int productId)
        {
            if (String.IsNullOrEmpty(productId.ToString()))
                throw new Exception("Please save product  - to enable disable dates in Product Availability Calendar");

            ProductFulfillmentCalendar day = new ProductFulfillmentCalendar();
            day.Date = DateTime.Parse(date.Replace("\"", ""));

            int ProductId = 0;
            if (Request.RawUrl.IndexOf("Edit/") > 0)
            {
                int strt = Request.RawUrl.IndexOf("Edit/") + 5;
                int length = Request.RawUrl.Length - strt;
                ProductId = int.Parse(Request.RawUrl.Substring(strt, length));
            }

            if (ProductId > 0)
            {
                productId = ProductId;
            }

            var existingDay = _productFulfillmentService.GetCalDay(productId, day);

            if (existingDay != null)
            {
                _productFulfillmentService.DeleteCalDay(productId, existingDay);
            }
            else
            {
                _productFulfillmentService.InsertCalDay(productId, day);
            }
            return Json(true, JsonRequestBehavior.AllowGet); ;
        }
        #endregion

        #region Utilities

        [NonAction]
        protected virtual void UpdateLocales(Product product, ProductModel model)
        {
            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedValue(product,
                                                               x => x.Name,
                                                               localized.Name,
                                                               localized.LanguageId);
                _localizedEntityService.SaveLocalizedValue(product,
                                                               x => x.ShortDescription,
                                                               localized.ShortDescription,
                                                               localized.LanguageId);
                _localizedEntityService.SaveLocalizedValue(product,
                                                               x => x.FullDescription,
                                                               localized.FullDescription,
                                                               localized.LanguageId);
                _localizedEntityService.SaveLocalizedValue(product,
                                                               x => x.MetaKeywords,
                                                               localized.MetaKeywords,
                                                               localized.LanguageId);
                _localizedEntityService.SaveLocalizedValue(product,
                                                               x => x.MetaDescription,
                                                               localized.MetaDescription,
                                                               localized.LanguageId);
                _localizedEntityService.SaveLocalizedValue(product,
                                                               x => x.MetaTitle,
                                                               localized.MetaTitle,
                                                               localized.LanguageId);

                //search engine name
                var seName = product.ValidateSeName(localized.SeName, localized.Name, false);
                _urlRecordService.SaveSlug(product, seName, localized.LanguageId);
            }
        }

        [NonAction]
        protected virtual void UpdateLocales(ProductTag productTag, ProductTagModel model)
        {
            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedValue(productTag,
                                                               x => x.Name,
                                                               localized.Name,
                                                               localized.LanguageId);
            }
        }

        [NonAction]
        protected virtual void UpdateLocales(ProductAttributeValue pav, ProductModel.ProductAttributeValueModel model)
        {
            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedValue(pav,
                                                               x => x.Name,
                                                               localized.Name,
                                                               localized.LanguageId);
            }
        }

        [NonAction]
        protected virtual void UpdatePictureSeoNames(Product product)
        {
            foreach (var pp in product.ProductPictures)
                _pictureService.SetSeoFilename(pp.PictureId, _pictureService.GetPictureSeName(product.Name));
        }

        [NonAction]
        protected virtual void PrepareAclModel(ProductModel model, Product product, bool excludeProperties)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            if (!excludeProperties && product != null)
                model.SelectedCustomerRoleIds = _aclService.GetCustomerRoleIdsWithAccess(product).ToList();

            var allRoles = _customerService.GetAllCustomerRoles(true);
            foreach (var role in allRoles)
            {
                model.AvailableCustomerRoles.Add(new SelectListItem
                {
                    Text = role.Name,
                    Value = role.Id.ToString(),
                    Selected = model.SelectedCustomerRoleIds.Contains(role.Id)
                });
            }
        }

        [NonAction]
        protected virtual void SaveProductAcl(Product product, ProductModel model)
        {
            product.SubjectToAcl = model.SelectedCustomerRoleIds.Any();

            var existingAclRecords = _aclService.GetAclRecords(product);
            var allCustomerRoles = _customerService.GetAllCustomerRoles(true);
            foreach (var customerRole in allCustomerRoles)
            {
                if (model.SelectedCustomerRoleIds.Contains(customerRole.Id))
                {
                    //new role
                    if (existingAclRecords.Count(acl => acl.CustomerRoleId == customerRole.Id) == 0)
                        _aclService.InsertAclRecord(product, customerRole.Id);
                }
                else
                {
                    //remove role
                    var aclRecordToDelete = existingAclRecords.FirstOrDefault(acl => acl.CustomerRoleId == customerRole.Id);
                    if (aclRecordToDelete != null)
                        _aclService.DeleteAclRecord(aclRecordToDelete);
                }
            }
        }

        [NonAction]
        protected virtual void PrepareStoresMappingModel(ProductModel model, Product product, bool excludeProperties)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            if (!excludeProperties && product != null)
                model.SelectedStoreIds = _storeMappingService.GetStoresIdsWithAccess(product).ToList();

            var allStores = _storeService.GetAllStores();
            foreach (var store in allStores)
            {
                model.AvailableStores.Add(new SelectListItem
                {
                    Text = store.Name,
                    Value = store.Id.ToString(),
                    Selected = model.SelectedStoreIds.Contains(store.Id)
                });
            }
        }

        [NonAction]
        protected virtual void SaveStoreMappings(Product product, ProductModel model)
        {
            product.LimitedToStores = model.SelectedStoreIds.Any();

            var existingStoreMappings = _storeMappingService.GetStoreMappings(product);
            var allStores = _storeService.GetAllStores();
            foreach (var store in allStores)
            {
                if (model.SelectedStoreIds.Contains(store.Id))
                {

                    #region NU-10
                    //model.CopyProductModel.CopyImages = true;
                    //model.CopyProductModel.Name = product.Name + " " + store.Name;
                    //model.CopyProductModel.Published = true;

                    var copiedProduct = CopyProductFromProductModel(model, false);
                    #endregion
                    //new store
                    if (existingStoreMappings.Count(sm => sm.StoreId == store.Id) == 0)
                    {
                        #region NU-10
                        _storeMappingService.InsertStoreMapping(copiedProduct, store.Id);
                        model.IsMaster = model.IsMaster;
                        #endregion
                    }
                }
                else
                {
                    //remove store mapping
                    var storeMappingToDelete = existingStoreMappings.FirstOrDefault(sm => sm.StoreId == store.Id);
                    if (storeMappingToDelete != null)
                        _storeMappingService.DeleteStoreMapping(storeMappingToDelete);
                }
            }
        }

        [NonAction]
        protected virtual void SaveProductPickupWarehouse(Product product, ProductModel model)
        {
            var existingWarehouse = _productService.GetPickupWarehouseByProductId(product.Id);
            if (!model.SelectedWarehouseIds.Contains(0))
            {

                foreach (var data in existingWarehouse)
                {
                    if (!model.SelectedWarehouseIds.Contains(data.WarehouseId))
                    {
                        _productService.DeleteProductPickupWarehouse(data);
                    }
                }

                foreach (var warhouseData in model.SelectedWarehouseIds)
                {
                    if (!existingWarehouse.Select(c => c.WarehouseId).Contains(warhouseData))
                    {
                        _productService.UpdateProductPickupWarehouse(product.Id, warhouseData);
                    }
                }
            }

            if (model.SelectedWarehouseIds.Contains(0))
            {
                foreach (var data in existingWarehouse)
                {
                    _productService.DeleteProductPickupWarehouse(data);

                }
            }
        }


        [NonAction]
        protected virtual void PrepareCategoryMappingModel(ProductModel model, Product product, bool excludeProperties)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            if (!excludeProperties && product != null)
                model.SelectedCategoryIds = _categoryService.GetProductCategoriesByProductId(product.Id, true).Select(c => c.CategoryId).ToList();

            var allCategories = SelectListHelper.GetCategoryList(_categoryService, _cacheManager, true);
            foreach (var c in allCategories)
            {
                c.Selected = model.SelectedCategoryIds.Contains(int.Parse(c.Value));
                model.AvailableCategories.Add(c);
            }
        }



        [NonAction]
        protected virtual void SaveCategoryMappings(Product product, ProductModel model)
        {
            var existingProductCategories = _categoryService.GetProductCategoriesByProductId(product.Id, true);

            //delete categories
            foreach (var existingProductCategory in existingProductCategories)
                if (!model.SelectedCategoryIds.Contains(existingProductCategory.CategoryId))
                    _categoryService.DeleteProductCategory(existingProductCategory);

            //add categories
            foreach (var categoryId in model.SelectedCategoryIds)
                if (existingProductCategories.FindProductCategory(product.Id, categoryId) == null)
                {
                    //find next display order
                    var displayOrder = 1;
                    var existingCategoryMapping = _categoryService.GetProductCategoriesByCategoryId(categoryId, showHidden: true);
                    if (existingCategoryMapping.Any())
                        displayOrder = existingCategoryMapping.Max(x => x.DisplayOrder) + 1;
                    _categoryService.InsertProductCategory(new ProductCategory
                    {
                        ProductId = product.Id,
                        CategoryId = categoryId,
                        DisplayOrder = displayOrder
                    });
                }
        }



        [NonAction]
        protected virtual void PrepareGlCodeMappingModel(ProductModel model, Product product, bool excludeProperties)

        {
            if (model == null)
                throw new ArgumentNullException("model");

            if (!excludeProperties && product != null)
            {
                model.SelectedPaidGlCodeIds = _glCodeService.GetProductGlCodes(product).Select(c => c.GlCodeId).ToList();
                model.SelectedDeferredGlCodeIds = _glCodeService.GetProductGlCodes(product).Select(c => c.GlCodeId).ToList();
            }
            else
            {

            }

            var glCodesDeferred = _glCodeService.GetGlCodes(GLCodeStatusType.Deferred);
            var glCodesPaid = _glCodeService.GetGlCodes(GLCodeStatusType.Paid);
            var DeliveryglcodeDetails = _glCodeService.GetGlCodeByGlcodeName("70161010");
            var ShippingcodeDetails = _glCodeService.GetGlCodeByGlcodeName("62610001");
            foreach (var glcode in glCodesDeferred)
            {
                model.AvailableDeferredGLCodes.Add(new SelectListItem
                {
                    Text = glcode.GlCodeName + "-" + glcode.Description,
                    Value = glcode.Id.ToString(),
                    Selected = model.SelectedDeferredGlCodeIds.Contains(glcode.Id)
                });
            }

            glCodesPaid.RemoveAll(x => x.Id == DeliveryglcodeDetails.Id);
            glCodesPaid.RemoveAll(x => x.Id == ShippingcodeDetails.Id);

            foreach (var glcode in glCodesPaid)
            {
                model.AvailablePaidGLCodes.Add(new SelectListItem
                {
                    Text = glcode.GlCodeName + "-" + glcode.Description,
                    Value = glcode.Id.ToString(),
                    Selected = model.SelectedPaidGlCodeIds.Contains(glcode.Id)
                });
            }
        }



        [NonAction]
        protected virtual void SaveGLCodeMappings(Product product, ProductModel model)
        {
            var existingProductGlCodes = _glCodeService.GetProductGlCodes(product, GLCodeStatusType.Paid);

            var existingDeferredGlCodes = _glCodeService.GetProductGlCodes(product, GLCodeStatusType.Deferred);

            var toremove = new List<ProductGlCode>();
            var toremoveDeferred = new List<ProductGlCode>();



            foreach (var id in model.SelectedPaidGlCodeIds)
            {
                var glcodescheck = _glCodeService.GetProductGlCodeByProductandGlcodeID(product.Id, id);
                if (glcodescheck == null || !glcodescheck.Any())
                {
                    //add new Paid GLS
                    _glCodeService.InsertProductGlCode(new ProductGlCode()
                    {
                        ProductId = product.Id,
                        GlCodeId = id,
                        Percentage = 100.00M,
                        Amount = 0M
                    });
                }
            }



            foreach (var id in model.SelectedDeferredGlCodeIds)
            {
                var glcodescheck = _glCodeService.GetProductGlCodeByProductandGlcodeID(product.Id, id);
                if (glcodescheck == null || !glcodescheck.Any())
                {
                    //add new Paid GLS
                    _glCodeService.InsertProductGlCode(new ProductGlCode()
                    {
                        ProductId = product.Id,
                        GlCodeId = id,
                        Percentage = 100.00M,
                        Amount = 0M
                    });
                }
            }

            //Update paid
            foreach (var existingProductGlCode in existingProductGlCodes)
            {
                //if (model.SelectedPaidGlCodeIds.Contains(existingProductGlCode.GlCodeId))
                //{
                _glCodeService.UpdateProductGlCode(existingProductGlCode);
                toremove.Add(existingProductGlCode);
                //}

            }
            //existingProductGlCodes.RemoveAll(x => toremove.Contains(x));


            //Update deferred
            foreach (var existingProductGlCode in existingDeferredGlCodes)
            {
                //if (model.SelectedDeferredGlCodeIds.Contains(existingProductGlCode.GlCodeId))
                //{
                _glCodeService.UpdateProductGlCode(existingProductGlCode);
                toremoveDeferred.Add(existingProductGlCode);
                //}
            }

            // existingDeferredGlCodes.RemoveAll(x => toremoveDeferred.Contains(x));

            if (model.SelectedPaidGlCodeIds.Count > existingProductGlCodes.Count)
            {
                foreach (var id in model.SelectedPaidGlCodeIds)
                {
                    var glcodescheck = _glCodeService.GetProductGlCodeByProductandGlcodeID(product.Id, id);
                    if (glcodescheck == null || !glcodescheck.Any())
                    {
                        //add new Paid GLS
                        _glCodeService.InsertProductGlCode(new ProductGlCode()
                        {
                            ProductId = product.Id,
                            GlCodeId = id,
                            Percentage = 100.00M,
                            Amount = 0M
                        });
                    }
                }

            }

            if (model.SelectedPaidGlCodeIds.Count < existingProductGlCodes.Count)
            {

                foreach (var existingProductGlCode in existingProductGlCodes)
                {
                    if (!model.SelectedPaidGlCodeIds.Contains(existingProductGlCode.GlCodeId))
                    {
                        _glCodeService.DeleteProductGlCode(existingProductGlCode);
                        //toremove.Add(existingProductGlCode);
                    }
                }
            }

            if (model.SelectedPaidGlCodeIds.Count == existingProductGlCodes.Count)
            {
                foreach (var id in model.SelectedPaidGlCodeIds)
                {
                    var glcodescheck = _glCodeService.GetProductGlCodeByProductandGlcodeID(product.Id, id);
                    if (glcodescheck == null || !glcodescheck.Any())
                    {
                        //add new Paid GLS
                        _glCodeService.InsertProductGlCode(new ProductGlCode()
                        {
                            ProductId = product.Id,
                            GlCodeId = id,
                            Percentage = 100.00M,
                            Amount = 0M
                        });
                    }
                }
                foreach (var existingProductGlCode in existingProductGlCodes)
                {
                    if (!model.SelectedPaidGlCodeIds.Contains(existingProductGlCode.GlCodeId))
                    {
                        _glCodeService.DeleteProductGlCode(existingProductGlCode);
                        //toremove.Add(existingProductGlCode);
                    }
                }

            }

            if (model.SelectedDeferredGlCodeIds.Count == existingDeferredGlCodes.Count)
            {
                foreach (var id in model.SelectedDeferredGlCodeIds)
                {
                    var glcodescheck = _glCodeService.GetProductGlCodeByProductandGlcodeID(product.Id, id);
                    if (glcodescheck == null || !glcodescheck.Any())
                    {
                        //add new Paid GLS
                        _glCodeService.InsertProductGlCode(new ProductGlCode()
                        {
                            ProductId = product.Id,
                            GlCodeId = id,
                            Percentage = 100.00M,
                            Amount = 0M
                        });
                    }
                }
                foreach (var existingProductGlCode in existingDeferredGlCodes)
                {
                    if (!model.SelectedDeferredGlCodeIds.Contains(existingProductGlCode.GlCodeId))
                    {
                        _glCodeService.DeleteProductGlCode(existingProductGlCode);
                        //toremove.Add(existingProductGlCode);
                    }
                }

            }


            if (model.SelectedDeferredGlCodeIds.Count > existingDeferredGlCodes.Count)
            {
                foreach (var id in model.SelectedDeferredGlCodeIds)
                {
                    var glcodescheck = _glCodeService.GetProductGlCodeByProductandGlcodeID(product.Id, id);
                    if (glcodescheck == null || !glcodescheck.Any())
                    {
                        //add new Paid GLS
                        _glCodeService.InsertProductGlCode(new ProductGlCode()
                        {
                            ProductId = product.Id,
                            GlCodeId = id,
                            Percentage = 100.00M,
                            Amount = 0M
                        });
                    }
                }

            }

            if (model.SelectedDeferredGlCodeIds.Count < existingDeferredGlCodes.Count)
            {

                foreach (var existingProductGlCode in existingDeferredGlCodes)
                {
                    if (!model.SelectedDeferredGlCodeIds.Contains(existingProductGlCode.GlCodeId))
                    {
                        _glCodeService.DeleteProductGlCode(existingProductGlCode);
                        //toremove.Add(existingProductGlCode);
                    }
                }
            }

            if (product.IsShipEnabled) // Section to check for IF Shipping is Enabled then Push the Shipping gl into the Database of mapping table.
            {
                //Check of the Product already has the Shippping Gl in Mapping table
                var ShippingGlcodeDetails = _glCodeService.GetGlCodeByGlcodeName("62610001"); // Fetch Gl Code Details by Glcode name
                var existingShippingGlcodes = _glCodeService.GetProductGlCodeByProductandGlcodeID(product.Id, ShippingGlcodeDetails.Id);
                if (!existingShippingGlcodes.Any()) // If no existing Record is found it shall allocate and map the shipping GL to the Mapping Table
                {
                    _glCodeService.InsertProductGlCode(new ProductGlCode()
                    {
                        ProductId = product.Id,
                        GlCodeId = ShippingGlcodeDetails.Id,
                        Percentage = 100.00M,
                        Amount = 0M
                    });
                }
            }

            if (product.IsPickupEnabled || product.IsLocalDelivery) // Section to check for IF Shipping is Enabled then Push the Shipping gl into the Database of mapping table.
            {
                //Check of the Product already has the Shippping Gl in Mapping table
                var DeliveryglcodeDetails = _glCodeService.GetGlCodeByGlcodeName("70161010"); // Fetch Gl Code Details by Glcode name
                var existingDeliveryGlcodes = _glCodeService.GetProductGlCodeByProductandGlcodeID(product.Id, DeliveryglcodeDetails.Id);
                if (!existingDeliveryGlcodes.Any()) // If no existing Record is found it shall allocate and map the shipping GL to the Mapping Table
                {
                    _glCodeService.InsertProductGlCode(new ProductGlCode()
                    {
                        ProductId = product.Id,
                        GlCodeId = DeliveryglcodeDetails.Id,
                        Percentage = 100.00M,
                        Amount = 0M
                    });
                }
            }

        }

        [HttpPost]
        public ActionResult UpdateGLCodeMappings(TaxCategory model)
        {
            var data = _glCodeService.GetProductGlCodes(model.Id);
            data.Percentage = Convert.ToDecimal(model.Percentage);
            data.Amount = Convert.ToDecimal(model.Amount);
            data.TaxCategoryId = Convert.ToInt32(model.TaxCategoryId);
            data.GlCodeId = Convert.ToInt32(model.GlCodeId);

            _glCodeService.UpdateProductGlCode(data);
            return new NullJsonResult();
        }


        [NonAction]
        protected virtual void PrepareManufacturerMappingModel(ProductModel model, Product product, bool excludeProperties)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            if (!excludeProperties && product != null)
                model.SelectedManufacturerIds = _manufacturerService.GetProductManufacturersByProductId(product.Id, true).Select(c => c.ManufacturerId).ToList();

            foreach (var manufacturer in _manufacturerService.GetAllManufacturers(showHidden: true))
            {
                model.AvailableManufacturers.Add(new SelectListItem
                {
                    Text = manufacturer.Name,
                    Value = manufacturer.Id.ToString(),
                    Selected = model.SelectedManufacturerIds.Contains(manufacturer.Id)
                });
            }
        }

        [NonAction]
        protected virtual void SaveManufacturerMappings(Product product, ProductModel model)
        {
            var existingProductManufacturers = _manufacturerService.GetProductManufacturersByProductId(product.Id, true);

            //delete manufacturers
            foreach (var existingProductManufacturer in existingProductManufacturers)
                if (!model.SelectedManufacturerIds.Contains(existingProductManufacturer.ManufacturerId))
                    _manufacturerService.DeleteProductManufacturer(existingProductManufacturer);

            //add manufacturers
            foreach (var manufacturerId in model.SelectedManufacturerIds)
                if (existingProductManufacturers.FindProductManufacturer(product.Id, manufacturerId) == null)
                {
                    //find next display order
                    var displayOrder = 1;
                    var existingManufacturerMapping = _manufacturerService.GetProductManufacturersByManufacturerId(manufacturerId, showHidden: true);
                    if (existingManufacturerMapping.Any())
                        displayOrder = existingManufacturerMapping.Max(x => x.DisplayOrder) + 1;
                    _manufacturerService.InsertProductManufacturer(new ProductManufacturer()
                    {
                        ProductId = product.Id,
                        ManufacturerId = manufacturerId,
                        DisplayOrder = displayOrder
                    });
                }
        }

        [NonAction]
        protected virtual void PrepareDiscountMappingModel(ProductModel model, Product product, bool excludeProperties)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            if (!excludeProperties && product != null)
                model.SelectedDiscountIds = product.AppliedDiscounts.Select(d => d.Id).ToList();



            foreach (var discount in _discountService.GetAllDiscounts(DiscountType.AssignedToSkus, showHidden: true, storeId: _storeContext.CurrentStore.Id))
            {

                model.AvailableDiscounts.Add(new SelectListItem
                {
                    Text = discount.Name,
                    Value = discount.Id.ToString(),
                    Selected = model.SelectedDiscountIds.Contains(discount.Id)
                });
            }
        }

        [NonAction]
        protected virtual void SaveDiscountMappings(Product product, ProductModel model)
        {
            var allDiscounts = _discountService.GetAllDiscounts(DiscountType.AssignedToSkus, showHidden: true);

            foreach (var discount in allDiscounts)
            {
                if (model.SelectedDiscountIds != null && model.SelectedDiscountIds.Contains(discount.Id))
                {
                    //new discount
                    if (product.AppliedDiscounts.Count(d => d.Id == discount.Id) == 0)
                        product.AppliedDiscounts.Add(discount);
                }
                else
                {
                    //remove discount
                    if (product.AppliedDiscounts.Count(d => d.Id == discount.Id) > 0)
                        product.AppliedDiscounts.Remove(discount);
                }
            }

            _productService.UpdateProduct(product);
            _productService.UpdateHasDiscountsApplied(product);
        }

        [NonAction]
        protected virtual void PrepareAddProductAttributeCombinationModel(AddProductAttributeCombinationModel model, Product product)
        {
            if (model == null)
                throw new ArgumentNullException("model");
            if (product == null)
                throw new ArgumentNullException("product");

            model.ProductId = product.Id;
            model.StockQuantity = 10000;
            model.NotifyAdminForQuantityBelow = 1;

            var attributes = _productAttributeService.GetProductAttributeMappingsByProductId(product.Id)
                //ignore non-combinable attributes for combinations
                .Where(x => !x.IsNonCombinable())
                .ToList();
            foreach (var attribute in attributes)
            {
                var attributeModel = new AddProductAttributeCombinationModel.ProductAttributeModel
                {
                    Id = attribute.Id,
                    ProductAttributeId = attribute.ProductAttributeId,
                    Name = attribute.ProductAttribute.Name,
                    TextPrompt = attribute.TextPrompt,
                    IsRequired = attribute.IsRequired,
                    AttributeControlType = attribute.AttributeControlType
                };

                if (attribute.ShouldHaveValues())
                {
                    //values
                    var attributeValues = _productAttributeService.GetProductAttributeValues(attribute.Id);
                    foreach (var attributeValue in attributeValues)
                    {
                        var attributeValueModel = new AddProductAttributeCombinationModel.ProductAttributeValueModel
                        {
                            Id = attributeValue.Id,
                            Name = attributeValue.Name,
                            IsPreSelected = attributeValue.IsPreSelected
                        };
                        attributeModel.Values.Add(attributeValueModel);
                    }
                }

                model.ProductAttributes.Add(attributeModel);
            }
        }

        [NonAction]
        protected virtual string[] ParseProductTags(string productTags)
        {
            var result = new List<string>();
            if (!String.IsNullOrWhiteSpace(productTags))
            {
                string[] values = productTags.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string val1 in values)
                    if (!String.IsNullOrEmpty(val1.Trim()))
                        result.Add(val1.Trim());
            }
            return result.ToArray();
        }

        [NonAction]
        protected virtual void SaveProductTags(Product product, string[] productTags)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            //product tags
            var existingProductTags = product.ProductTags.ToList();
            var productTagsToRemove = new List<ProductTag>();
            foreach (var existingProductTag in existingProductTags)
            {
                bool found = false;
                foreach (string newProductTag in productTags)
                {
                    if (existingProductTag.Name.Equals(newProductTag, StringComparison.InvariantCultureIgnoreCase))
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    productTagsToRemove.Add(existingProductTag);
                }
            }
            foreach (var productTag in productTagsToRemove)
            {
                product.ProductTags.Remove(productTag);
                _productService.UpdateProduct(product);
            }
            foreach (string productTagName in productTags)
            {
                ProductTag productTag;
                var productTag2 = _productTagService.GetProductTagByName(productTagName);
                if (productTag2 == null)
                {
                    //add new product tag
                    productTag = new ProductTag
                    {
                        Name = productTagName
                    };
                    _productTagService.InsertProductTag(productTag);
                }
                else
                {
                    productTag = productTag2;
                }
                if (!product.ProductTagExists(productTag.Id))
                {
                    product.ProductTags.Add(productTag);
                    _productService.UpdateProduct(product);
                }
            }
        }

        [NonAction]
        protected virtual void PrepareProductModel(ProductModel model, Product product,
            bool setPredefinedValues, bool excludeProperties)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            if (product != null)
            {
                model.IsBundleProduct = product.IsBundleProduct;
                var parentGroupedProduct = _productService.GetProductById(product.ParentGroupedProductId);
                if (parentGroupedProduct != null)
                {
                    model.AssociatedToProductId = product.ParentGroupedProductId;
                    model.AssociatedToProductName = parentGroupedProduct.Name;
                }

            }

            model.PrimaryStoreCurrencyCode = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId).CurrencyCode;
            model.BaseWeightIn = _measureService.GetMeasureWeightById(_measureSettings.BaseWeightId).Name;
            model.BaseDimensionIn = _measureService.GetMeasureDimensionById(_measureSettings.BaseDimensionId).Name;
            if (product != null)
            {
                model.CreatedOn = _dateTimeHelper.ConvertToUserTime(product.CreatedOnUtc, DateTimeKind.Utc);
                model.UpdatedOn = _dateTimeHelper.ConvertToUserTime(product.UpdatedOnUtc, DateTimeKind.Utc);
            }

            //little performance hack here
            //there's no need to load attributes when creating a new product
            //anyway they're not used (you need to save a product before you map add them)

            //Reservation Details
            #region UI setup for reservation

            model.ReservationStartHour = new List<SelectListItem>();
            model.ReservationEndHour = new List<SelectListItem>();

            model.ReservationStartMinute = new List<SelectListItem>();
            model.ReservationEndMinute = new List<SelectListItem>();

            model.ReservationStartAMPM = new List<SelectListItem>();
            model.ReservationEndAMPM = new List<SelectListItem>();

            if (!String.IsNullOrEmpty(model.ReservationStartTime) && model.ReservationStartTime.Length <= 2)
            {
                int strtHr = Convert.ToInt32(model.ReservationStartTime);
                model.ReservationSelectedStartHour = (strtHr > 12) ? strtHr - 12 : strtHr;
                model.ReservationSelectedStartMinute = 0;
                model.ReservationSelectedStartAMPM = (strtHr >= 12) ? "PM" : "AM";
            }

            if (!String.IsNullOrEmpty(model.ReservationEndTime) && model.ReservationEndTime.Length <= 2)
            {
                int endHr = Convert.ToInt32(model.ReservationEndTime);
                model.ReservationSelectedEndHour = (endHr > 12) ? endHr - 12 : endHr;
                model.ReservationSelectedEndMinute = 0;
                model.ReservationSelectedEndAMPM = (endHr >= 12) ? "PM" : "AM";
            }

            model.LeadTime = product != null ? product.LeadTime : 0;
            if (!String.IsNullOrEmpty(model.ReservationStartTime) && model.ReservationStartTime.Length > 2)
            {
                string[] startTime = model.ReservationStartTime.Split(':');
                model.ReservationSelectedStartHour = Convert.ToInt32(startTime[0]);
                model.ReservationSelectedStartMinute = Convert.ToInt32(startTime[1].Split(' ')[0]);
                model.ReservationSelectedStartAMPM = startTime[1].Split(' ')[1];
            }

            if (!String.IsNullOrEmpty(model.ReservationEndTime) && model.ReservationEndTime.Length > 2)
            {
                string[] endTime = model.ReservationEndTime.Split(':');
                model.ReservationSelectedEndHour = Convert.ToInt32(endTime[0]);
                model.ReservationSelectedEndMinute = Convert.ToInt32(endTime[1].Split(' ')[0]);
                model.ReservationSelectedEndAMPM = endTime[1].Split(' ')[1];
            }

            for (int i = 1; i <= 12; i++)
            {
                model.ReservationStartHour.Add(new SelectListItem
                {
                    Text = i.ToString(),
                    Value = i.ToString(),
                    Selected = (!String.IsNullOrEmpty(model.ReservationSelectedStartHour.ToString())) && model.ReservationSelectedStartHour.ToString() == i.ToString() ? true : false
                });
            }

            for (int i = 1; i <= 12; i++)
            {
                model.ReservationEndHour.Add(new SelectListItem
                {
                    Text = i.ToString(),
                    Value = i.ToString(),
                    Selected = (!String.IsNullOrEmpty(model.ReservationSelectedEndHour.ToString())) && model.ReservationSelectedEndHour.ToString() == i.ToString() ? true : false
                });
            }

            model.ReservationStartMinute.Add(new SelectListItem
            {
                Text = "00",
                Value = "00",
                Selected = (!String.IsNullOrEmpty(model.ReservationSelectedStartMinute.ToString())) && (model.ReservationSelectedStartMinute == 00 || model.ReservationSelectedStartMinute == 0) ? true : false
            });
            model.ReservationStartMinute.Add(new SelectListItem
            {
                Text = "15",
                Value = "15",
                Selected = (!String.IsNullOrEmpty(model.ReservationSelectedStartMinute.ToString())) && (model.ReservationSelectedStartMinute == 15) ? true : false
            });
            model.ReservationStartMinute.Add(new SelectListItem
            {
                Text = "30",
                Value = "30",
                Selected = (!String.IsNullOrEmpty(model.ReservationSelectedStartMinute.ToString())) && (model.ReservationSelectedStartMinute == 30) ? true : false
            });

            model.ReservationEndMinute.Add(new SelectListItem
            {
                Text = "00",
                Value = "00",
                Selected = (!String.IsNullOrEmpty(model.ReservationSelectedEndMinute.ToString())) && (model.ReservationSelectedEndMinute == 00 || model.ReservationSelectedEndMinute == 0) ? true : false
            });
            model.ReservationEndMinute.Add(new SelectListItem
            {
                Text = "15",
                Value = "15",
                Selected = (!String.IsNullOrEmpty(model.ReservationSelectedEndMinute.ToString())) && (model.ReservationSelectedEndMinute == 15) ? true : false
            });
            model.ReservationEndMinute.Add(new SelectListItem
            {
                Text = "30",
                Value = "30",
                Selected = (!String.IsNullOrEmpty(model.ReservationSelectedEndMinute.ToString())) && (model.ReservationSelectedEndMinute == 30) ? true : false
            });

            model.ReservationStartAMPM.Add(new SelectListItem
            {
                Text = "AM",
                Value = "AM",
                Selected = (!String.IsNullOrEmpty(model.ReservationSelectedStartAMPM)) && model.ReservationSelectedStartAMPM == "AM" ? true : false
            });
            model.ReservationStartAMPM.Add(new SelectListItem
            {
                Text = "PM",
                Value = "PM",
                Selected = (!String.IsNullOrEmpty(model.ReservationSelectedStartAMPM)) && model.ReservationSelectedStartAMPM == "PM" ? true : false
            });

            model.ReservationEndAMPM.Add(new SelectListItem
            {
                Text = "AM",
                Value = "AM",
                Selected = (!String.IsNullOrEmpty(model.ReservationSelectedEndAMPM)) && model.ReservationSelectedEndAMPM == "AM" ? true : false
            });
            model.ReservationEndAMPM.Add(new SelectListItem
            {
                Text = "PM",
                Value = "PM",
                Selected = (!String.IsNullOrEmpty(model.ReservationSelectedEndAMPM)) && model.ReservationSelectedEndAMPM == "PM" ? true : false
            });

            #endregion

            if (product != null)
            {
                //product attributes
                foreach (var productAttribute in _productAttributeService.GetAllProductAttributes())
                {
                    model.AvailableProductAttributes.Add(new SelectListItem
                    {
                        Text = productAttribute.Name,
                        Value = productAttribute.Id.ToString()
                    });
                }

                //specification attributes
                model.AddSpecificationAttributeModel.AvailableAttributes = _cacheManager
                    .Get(ModelCacheEventConsumer.SPEC_ATTRIBUTES_MODEL_KEY, () =>
                    {
                        var availableSpecificationAttributes = new List<SelectListItem>();
                        foreach (var sa in _specificationAttributeService.GetSpecificationAttributes())
                        {
                            availableSpecificationAttributes.Add(new SelectListItem
                            {
                                Text = sa.Name,
                                Value = sa.Id.ToString()
                            });
                        }
                        return availableSpecificationAttributes;
                    });

                //options of preselected specification attribute
                if (model.AddSpecificationAttributeModel.AvailableAttributes.Any())
                {
                    var selectedAttributeId = int.Parse(model.AddSpecificationAttributeModel.AvailableAttributes.First().Value);
                    foreach (var sao in _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttribute(selectedAttributeId))
                        model.AddSpecificationAttributeModel.AvailableOptions.Add(new SelectListItem
                        {
                            Text = sao.Name,
                            Value = sao.Id.ToString()
                        });
                }
                //default specs values
                model.AddSpecificationAttributeModel.ShowOnProductPage = true;

            }


            //copy product
            if (product != null)
            {
                model.CopyProductModel.Id = product.Id;
                model.CopyProductModel.Name = "Copy of " + product.Name;
                model.CopyProductModel.Published = true;
                model.CopyProductModel.CopyImages = true;
            }

            //templates
            var templates = _productTemplateService.GetAllProductTemplates();
            foreach (var template in templates)
            {
                model.AvailableProductTemplates.Add(new SelectListItem
                {
                    Text = template.Name,
                    Value = template.Id.ToString()
                });
            }

            model.IsLoggedAs = _workContext.CurrentCustomer.IsLoggedAs();

            //vendors
            model.IsLoggedInAsVendor = _workContext.CurrentVendor != null;
            #region NU-51
            if (model.IsLoggedAs != "VENDOR")
            {
                model.AvailableVendors.Add(new SelectListItem
                {
                    Text = _localizationService.GetResource("Admin.Catalog.Products.Fields.Vendor.None"),
                    Value = "0"
                });

                var vendors = _vendorService.GetAllVendors(showHidden: true);
                foreach (var vendor in vendors)
                {
                    model.AvailableVendors.Add(new SelectListItem
                    {
                        Text = vendor.Name,
                        Value = vendor.Id.ToString()
                    });
                }
            }
            else
            {
                model.AvailableVendors.Add(new SelectListItem
                {
                    Text = _workContext.CurrentVendor.Name,
                    Value = _workContext.CurrentCustomer.Id.ToString()
                });
            }
            #endregion

            //delivery dates
            model.AvailableDeliveryDates.Add(new SelectListItem
            {
                Text = _localizationService.GetResource("Admin.Catalog.Products.Fields.DeliveryDate.None"),
                Value = "0"
            });
            var deliveryDates = _shippingService.GetAllDeliveryDates();
            foreach (var deliveryDate in deliveryDates)
            {
                model.AvailableDeliveryDates.Add(new SelectListItem
                {
                    Text = deliveryDate.Name,
                    Value = deliveryDate.Id.ToString()
                });
            }

            //warehouses
            #region NU-35
            int storeId = -1;
            int vendorId = -1;
            if (model.VendorId > 0)
            {
                vendorId = model.VendorId;
            }
            else if (_workContext.CurrentCustomer.IsVendor())
            {
                vendorId = _workContext.CurrentVendor.Id;
            }
            else if (model.Id > 0 && model.MasterId == 0)
            {
                var mapping = _storeMappingService.GetStoreMappings(product);
                if (mapping.Any())
                {
                    storeId = mapping.First().StoreId;
                }
            }
            else if (model.MasterId == 0)
            {
                vendorId = 0;
            }
            else
            {
                storeId = _storeContext.CurrentStore.Id;
            }

            var warehouses = _shippingService.GetAllWarehouses(storeId: storeId, vendorId: vendorId);

            model.AvailableWarehouses.Add(new SelectListItem
            {
                Text = _localizationService.GetResource("Admin.Catalog.Products.Fields.Warehouse.None"),
                Value = "0"
            });
            foreach (var warehouse in warehouses)
            {
                model.AvailableWarehouses.Add(new SelectListItem
                {
                    Text = warehouse.Name,
                    Value = warehouse.Id.ToString()
                });
            }

            if (product != null)
            {
                #region NU-13
                if (product.PreferredPickupWarehouseId == 0)
                {
                    model.AvailablePickupWarehouses.Add(new SelectListItem
                    {
                        Text = _localizationService.GetResource("Admin.Catalog.Products.Fields.Warehouse.None"),
                        Value = "0",
                        Selected = true
                    });
                }
                else
                {
                    model.AvailablePickupWarehouses.Add(new SelectListItem
                    {
                        Text = _localizationService.GetResource("Admin.Catalog.Products.Fields.Warehouse.None"),
                        Value = "0",
                        Selected = false
                    });
                }


                model.SelectedWarehouseIds = _productService.GetPickupWarehouseByProductId(product.Id).Select(c => c.WarehouseId).ToList();

                foreach (var warehouse in warehouses)
                {
                    if (warehouse.IsPickup)
                    {
                        if (model.PreferredPickupWarehouseId > 1 && warehouse.Id == model.PreferredPickupWarehouseId)
                        {
                            model.AvailablePickupWarehouses.Add(new SelectListItem
                            {
                                Text = warehouse.Name,
                                Value = warehouse.Id.ToString(),
                                Selected = true
                            });

                        }
                        else
                        {
                            model.AvailablePickupWarehouses.Add(new SelectListItem
                            {
                                Text = warehouse.Name,
                                Value = warehouse.Id.ToString(),
                                Selected = model.SelectedWarehouseIds.Contains(warehouse.Id)
                            });
                        }
                    }
                }
                #endregion
            }
            #endregion
            //multiple warehouses
            foreach (var warehouse in warehouses)
            {
                var pwiModel = new ProductModel.ProductWarehouseInventoryModel
                {
                    WarehouseId = warehouse.Id,
                    WarehouseName = warehouse.Name
                };
                if (product != null)
                {
                    var pwi = product.ProductWarehouseInventory.FirstOrDefault(x => x.WarehouseId == warehouse.Id);
                    if (pwi != null)
                    {
                        pwiModel.WarehouseUsed = true;
                        pwiModel.StockQuantity = pwi.StockQuantity;
                        pwiModel.ReservedQuantity = pwi.ReservedQuantity;
                        pwiModel.PlannedQuantity = _shipmentService.GetQuantityInShipments(product, pwi.WarehouseId, true, true);
                    }
                }
                model.ProductWarehouseInventoryModels.Add(pwiModel);
            }

            //product tags
            if (product != null)
            {
                var result = new StringBuilder();
                for (int i = 0; i < product.ProductTags.Count; i++)
                {
                    var pt = product.ProductTags.ToList()[i];
                    result.Append(pt.Name);
                    if (i != product.ProductTags.Count - 1)
                        result.Append(", ");
                }
                model.ProductTags = result.ToString();
                model.Url = product.Url; /// SODMYWAY-
            }

            //tax categories
            var taxCategories = _taxCategoryService.GetAllTaxCategories();
            model.AvailableTaxCategories.Add(new SelectListItem { Text = "---", Value = "0" });
            foreach (var tc in taxCategories)
                model.AvailableTaxCategories.Add(new SelectListItem { Text = tc.Name + " " + tc.Code, Value = tc.Id.ToString(), Selected = product != null && !setPredefinedValues && tc.Id == product.TaxCategoryId });

            //baseprice units
            var measureWeights = _measureService.GetAllMeasureWeights();
            foreach (var mw in measureWeights)
                model.AvailableBasepriceUnits.Add(new SelectListItem { Text = mw.Name, Value = mw.Id.ToString(), Selected = product != null && !setPredefinedValues && mw.Id == product.BasepriceUnitId });
            foreach (var mw in measureWeights)
                model.AvailableBasepriceBaseUnits.Add(new SelectListItem { Text = mw.Name, Value = mw.Id.ToString(), Selected = product != null && !setPredefinedValues && mw.Id == product.BasepriceBaseUnitId });

            //last stock quantity
            if (product != null)
            {
                model.LastStockQuantity = product.StockQuantity;
            }

            //---START: Codechages done by (na-sdxcorp\ADas)--------------
            //IsLocalDelivery
            //if (product != null)
            //{
            //    var MasterProduct = _productService.GetProductMaster(product);
            //    if (MasterProduct != null)
            //        model.IsLocalDelivery = MasterProduct.IsLocalDelivery;
            //    else
            //        model.IsLocalDelivery = product.IsLocalDelivery;

            //}
            //---END: Codechages done by (na-sdxcorp\ADas)--------------

            //default values
            if (setPredefinedValues)
            {
                model.MaximumCustomerEnteredPrice = 1000;
                model.MaxNumberOfDownloads = 10;
                model.RecurringCycleLength = 100;
                model.RecurringTotalCycles = 10;
                model.RentalPriceLength = 1;
                model.StockQuantity = 10000;
                model.NotifyAdminForQuantityBelow = 1;
                model.OrderMinimumQuantity = 1;
                model.OrderMaximumQuantity = 10000;

                model.UnlimitedDownloads = true;
                model.IsShipEnabled = false;
                model.IsPickupEnabled = false;	// NU-13
                model.AllowCustomerReviews = false; //EC-6
                model.Published = false;	// NU-41
                model.VisibleIndividually = true;
                model.IsMaster = true; /// NU-10
                model.StoreCommission = 0;  /// NU-15
            }
            else
            {
                if (product != null)
                {
                    model.IsMaster = product.IsMaster; /// NU-10
                    model.StoreCommission = product.StoreCommission;    /// NU-15
                }

            }

            //editor settings
            var productEditorSettings = _settingService.LoadSetting<ProductEditorSettings>();
            model.ProductEditorSettingsModel = productEditorSettings.ToModel();

        }

        [NonAction]
        protected virtual List<int> GetChildCategoryIds(int parentCategoryId)
        {
            var categoriesIds = new List<int>();
            var categories = _categoryService.GetAllCategoriesByParentCategoryId(parentCategoryId, true);
            foreach (var category in categories)
            {
                categoriesIds.Add(category.Id);
                categoriesIds.AddRange(GetChildCategoryIds(category.Id));
            }
            return categoriesIds;
        }

        [NonAction]
        protected virtual void SaveProductWarehouseInventory(Product product, ProductModel model)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            if (model.ManageInventoryMethodId != (int)ManageInventoryMethod.ManageStock)
                return;

            if (!model.UseMultipleWarehouses)
                return;

            var warehouses = _shippingService.GetAllWarehouses(
                _storeContext.CurrentStore.Id);	/// NU-35

            foreach (var warehouse in warehouses)
            {
                //parse stock quantity
                int stockQuantity = 0;
                foreach (string formKey in this.Request.Form.AllKeys)
                    if (formKey.Equals(string.Format("warehouse_qty_{0}", warehouse.Id), StringComparison.InvariantCultureIgnoreCase))
                    {
                        int.TryParse(this.Request.Form[formKey], out stockQuantity);
                        break;
                    }
                //parse reserved quantity
                int reservedQuantity = 0;
                foreach (string formKey in this.Request.Form.AllKeys)
                    if (formKey.Equals(string.Format("warehouse_reserved_{0}", warehouse.Id), StringComparison.InvariantCultureIgnoreCase))
                    {
                        int.TryParse(this.Request.Form[formKey], out reservedQuantity);
                        break;
                    }
                //parse "used" field
                bool used = false;
                foreach (string formKey in this.Request.Form.AllKeys)
                    if (formKey.Equals(string.Format("warehouse_used_{0}", warehouse.Id), StringComparison.InvariantCultureIgnoreCase))
                    {
                        int tmp;
                        int.TryParse(this.Request.Form[formKey], out tmp);
                        used = tmp == warehouse.Id;
                        break;
                    }

                var existingPwI = product.ProductWarehouseInventory.FirstOrDefault(x => x.WarehouseId == warehouse.Id);
                if (existingPwI != null)
                {
                    if (used)
                    {
                        //update existing record
                        existingPwI.StockQuantity = stockQuantity;
                        existingPwI.ReservedQuantity = reservedQuantity;
                        _productService.UpdateProduct(product);
                    }
                    else
                    {
                        //delete. no need to store record for qty 0
                        _productService.DeleteProductWarehouseInventory(existingPwI);
                    }
                }
                else
                {
                    if (used)
                    {
                        //no need to insert a record for qty 0
                        existingPwI = new ProductWarehouseInventory
                        {
                            WarehouseId = warehouse.Id,
                            ProductId = product.Id,
                            StockQuantity = stockQuantity,
                            ReservedQuantity = reservedQuantity
                        };
                        product.ProductWarehouseInventory.Add(existingPwI);
                        _productService.UpdateProduct(product);
                    }
                }
            }
            UpdateCopiesFromMaster(product.Id);	/// NU-10
        }

        #endregion

        #region Methods

        #region Product list / create / edit / delete

        //list products
        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        public ActionResult List(bool isMaster = false)	/// NU-10
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
            {
                return AccessDeniedView();
            }

            var model = new ProductListModel();
            //a vendor should have access only to his products
            model.IsLoggedInAsVendor = _workContext.CurrentVendor != null;
            model.IsLoggedAs = _workContext.CurrentCustomer.IsLoggedAs();	/// NU-1
            model.IsReadOnly = _workContext.CurrentCustomer.IsReadOnly();

            //is master or local catalog?
            model.IsMaster = isMaster;	/// NU-10

            //categories
            model.AvailableCategories.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            var categories = SelectListHelper.GetCategoryList(_categoryService, _cacheManager, true);
            foreach (var c in categories)
                model.AvailableCategories.Add(c);

            //manufacturers
            model.AvailableManufacturers.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            foreach (var m in _manufacturerService.GetAllManufacturers(showHidden: true).
                OrderBy(m => m.Name)) /// NU-24
                model.AvailableManufacturers.Add(new SelectListItem { Text = m.Name, Value = m.Id.ToString() });

            //stores
            model.AvailableStores.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            foreach (var s in _storeService.GetAllStores())
                model.AvailableStores.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString() });

            //warehouses
            model.AvailableWarehouses.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            #region NU-35

            int customerId = -1;
            int vendorId = -1;
            int storeId = -1;

            if (model.IsMaster)
            {
                if (_workContext.CurrentCustomer.IsVendor())
                {
                    vendorId = _workContext.CurrentCustomer.VendorId;
                }
                else
                {
                    vendorId = 0;
                }
            }
            else
            {
                if (!_workContext.CurrentCustomer.IsSystemGlobalAdmin())
                {
                    customerId = _workContext.CurrentCustomer.Id;
                }
                else if (_workContext.CurrentCustomer.IsStoreAdmin())
                {
                    storeId = _storeContext.CurrentStore.Id;
                }
                else
                {
                    storeId = 0;
                }
            }

            foreach (var wh in _shippingService.GetAllWarehouses(
                customerId: customerId,
                vendorId: vendorId,
                storeId: storeId
                ).OrderBy(w => w.Name))
                model.AvailableWarehouses.Add(new SelectListItem { Text = wh.Name, Value = wh.Id.ToString() });
            #endregion

            //vendors
            #region NU-51
            if (model.IsMaster)
            {
                if (_workContext.CurrentCustomer.IsSystemGlobalAdmin())
                {
                    model.AvailableVendors.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });

                    foreach (var v in _vendorService.GetAllVendors())
                    {
                        model.AvailableVendors.Add(new SelectListItem { Text = v.Name, Value = v.Id.ToString() });
                    }
                }
                else if (_workContext.CurrentCustomer.IsVendor())
                {
                    model.AvailableVendors.Add(new SelectListItem { Text = _workContext.CurrentVendor.Name, Value = _workContext.CurrentVendor.Id.ToString() });
                }
            }
            else
            {
                model.AvailableVendors.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
                foreach (var v in _vendorService.GetAllVendors())
                {
                    model.AvailableVendors.Add(new SelectListItem { Text = v.Name, Value = v.Id.ToString() });
                }
            }
            #endregion

            //product types
            model.AvailableProductTypes = ProductType.SimpleProduct.ToSelectList(false).ToList();
            model.AvailableProductTypes.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });

            //"published" property
            //0 - all (according to "ShowHidden" parameter)
            //1 - published only
            //2 - unpublished only
            model.AvailablePublishedOptions.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Catalog.Products.List.SearchPublished.All"), Value = "0" });
            model.AvailablePublishedOptions.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Catalog.Products.List.SearchPublished.PublishedOnly"), Value = "1" });
            model.AvailablePublishedOptions.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Catalog.Products.List.SearchPublished.UnpublishedOnly"), Value = "2" });

            model.IsMaster = isMaster;	// NU-10

            return View(model);
        }

        [HttpPost, ActionName("Helper")]
        public ActionResult Helper()	/// NU-10
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
            {
                return AccessDeniedView();
            }

            var model = new ProductListModel();
            //a vendor should have access only to his products
            model.IsLoggedInAsVendor = _workContext.CurrentVendor != null;
            model.IsLoggedAs = _workContext.CurrentCustomer.IsLoggedAs();	/// NU-1

            //is master or local catalog?
            model.IsMaster = false;	/// NU-10

            //categories
            model.AvailableCategories.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            var categories = SelectListHelper.GetCategoryList(_categoryService, _cacheManager, true);
            foreach (var c in categories)
                model.AvailableCategories.Add(c);

            //manufacturers
            model.AvailableManufacturers.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            foreach (var m in _manufacturerService.GetAllManufacturers(showHidden: true).
                OrderBy(m => m.Name)) /// NU-24
                model.AvailableManufacturers.Add(new SelectListItem { Text = m.Name, Value = m.Id.ToString() });

            //stores
            model.AvailableStores.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            foreach (var s in _storeService.GetAllStores())
                model.AvailableStores.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString() });

            //warehouses
            model.AvailableWarehouses.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            #region NU-35

            int customerId = -1;
            int vendorId = -1;
            int storeId = -1;

            if (model.IsMaster)
            {
                if (_workContext.CurrentCustomer.IsVendor())
                {
                    vendorId = _workContext.CurrentCustomer.VendorId;
                }
                else
                {
                    vendorId = 0;
                }
            }
            else
            {
                if (!_workContext.CurrentCustomer.IsSystemGlobalAdmin())
                {
                    customerId = _workContext.CurrentCustomer.Id;
                }
                else if (_workContext.CurrentCustomer.IsStoreAdmin())
                {
                    storeId = _storeContext.CurrentStore.Id;
                }
                else
                {
                    storeId = 0;
                }
            }

            foreach (var wh in _shippingService.GetAllWarehouses(
                customerId: customerId,
                vendorId: vendorId,
                storeId: storeId
                ).OrderBy(w => w.Name))
                model.AvailableWarehouses.Add(new SelectListItem { Text = wh.Name, Value = wh.Id.ToString() });
            #endregion

            //vendors
            #region NU-51
            if (model.IsMaster)
            {
                if (_workContext.CurrentCustomer.IsSystemGlobalAdmin())
                {
                    model.AvailableVendors.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });

                    foreach (var v in _vendorService.GetAllVendors())
                    {
                        model.AvailableVendors.Add(new SelectListItem { Text = v.Name, Value = v.Id.ToString() });
                    }
                }
                else if (_workContext.CurrentCustomer.IsVendor())
                {
                    model.AvailableVendors.Add(new SelectListItem { Text = _workContext.CurrentVendor.Name, Value = _workContext.CurrentVendor.Id.ToString() });
                }
            }
            else
            {
                model.AvailableVendors.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
                foreach (var v in _vendorService.GetAllVendors())
                {
                    model.AvailableVendors.Add(new SelectListItem { Text = v.Name, Value = v.Id.ToString() });
                }
            }
            #endregion

            //product types
            model.AvailableProductTypes = ProductType.SimpleProduct.ToSelectList(false).ToList();
            model.AvailableProductTypes.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });

            //"published" property
            //0 - all (according to "ShowHidden" parameter)
            //1 - published only
            //2 - unpublished only
            model.AvailablePublishedOptions.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Catalog.Products.List.SearchPublished.All"), Value = "0" });
            model.AvailablePublishedOptions.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Catalog.Products.List.SearchPublished.PublishedOnly"), Value = "1" });
            model.AvailablePublishedOptions.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Catalog.Products.List.SearchPublished.UnpublishedOnly"), Value = "2" });

            model.IsMaster = false;	// NU-10

            return View(model);
        }

        [HttpPost]
        public ActionResult ProductList(DataSourceRequest command, ProductListModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();


            #region NU-52 & NU-10
            if (model.IsMaster)
            {
                model.SearchPublishedId = 0;
                model.SearchStoreId = 0;
            }
            else
            {
                model.SearchStoreId = _storeContext.CurrentStore.Id;
            }
            #endregion

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                model.SearchVendorId = _workContext.CurrentVendor.Id;
            }

            var categoryIds = new List<int> { model.SearchCategoryId };
            //include subcategories
            if (model.SearchIncludeSubCategories && model.SearchCategoryId > 0)
                categoryIds.AddRange(GetChildCategoryIds(model.SearchCategoryId));

            //0 - all (according to "ShowHidden" parameter)
            //1 - published only
            //2 - unpublished only
            bool? overridePublished = null;
            if (model.SearchPublishedId == 1)
                overridePublished = true;
            else if (model.SearchPublishedId == 2)
                overridePublished = false;

            var products = _productService.SearchProducts(
                categoryIds: categoryIds,
                manufacturerId: model.SearchManufacturerId,
                storeId: model.SearchStoreId,
                vendorId: model.SearchVendorId,
                warehouseId: model.SearchWarehouseId,
                productType: model.SearchProductTypeId > 0 ? (ProductType?)model.SearchProductTypeId : null,
                keywords: model.SearchProductName,
                pageIndex: command.Page - 1,
                pageSize: command.PageSize,
                showHidden: true,
                overridePublished: overridePublished,
                overrideHideMasterProducts: !model.IsMaster	/// NU-10
            );

            var gridModel = new DataSourceResult();
            gridModel.Data = products.Select(x =>
            {
                var productModel = x.ToModel();
                var storeMappingsForProduct = _storeMappingService.GetStoreMappings<Product>(x);
                string name = "";
                foreach (var mapping in storeMappingsForProduct)
                {
                    if (mapping.EntityId == x.Id && mapping.EntityName == "Product")
                    {
                        name = _storeService.GetStoreById(mapping.StoreId).Name;
                        break;
                    }
                }
                productModel.StoreName = name;
                //little hack here:
                //ensure that product full descriptions are not returned
                //otherwise, we can get the following error if products have too long descriptions:
                //"Error during serialization or deserialization using the JSON JavaScriptSerializer. The length of the string exceeds the value set on the maxJsonLength property. "
                //also it improves performance
                productModel.FullDescription = "";

                //picture
                var defaultProductPicture = _pictureService.GetPicturesByProductId(x.Id, 1).FirstOrDefault();
                productModel.PictureThumbnailUrl = _pictureService.GetPictureUrl(defaultProductPicture, 75, true);
                //product type
                productModel.ProductTypeName = x.ProductType.GetLocalizedEnum(_localizationService, _workContext);
                //friendly stock qantity
                //if a simple product AND "manage inventory" is "Track inventory", then display
                if (x.ProductType == ProductType.SimpleProduct && x.ManageInventoryMethod == ManageInventoryMethod.ManageStock)
                    productModel.StockQuantityStr = x.GetTotalStockQuantity().ToString();
                return productModel;
            }).OrderBy(p => p.Name); /// NU-24
            gridModel.Total = products.TotalCount;

            return Json(gridModel);
        }

        #region NU-10
        [HttpPost, ActionName("MasterList")]
        [FormValueRequired("go-to-product-by-sku")]
        public ActionResult GoToSkuAdmin(ProductListModel model)
        {
            return GoToSku(model);
        }
        #endregion

        [HttpPost, ActionName("List")]
        [FormValueRequired("go-to-product-by-sku")]
        public ActionResult GoToSku(ProductListModel model)
        {
            string sku = model.GoDirectlyToSku;

            //try to load a product entity
            var product = _productService.GetProductBySku(sku);

            //if not found, then try to load a product attribute combination
            if (product == null)
            {
                var combination = _productAttributeService.GetProductAttributeCombinationBySku(sku);
                if (combination != null)
                {
                    product = combination.Product;
                }
            }

            if (product != null)
                return RedirectToAction("Edit", "Product", new { id = product.Id });

            //not found
            return List();
        }

        //create product
        public ActionResult Create()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //validate maximum number of products per vendor
            if (_vendorSettings.MaximumProductNumber > 0 &&
                _workContext.CurrentVendor != null &&
                _productService.GetNumberOfProductsByVendorId(_workContext.CurrentVendor.Id) >= _vendorSettings.MaximumProductNumber)
            {
                ErrorNotification(string.Format(_localizationService.GetResource("Admin.Catalog.Products.ExceededMaximumNumber"), _vendorSettings.MaximumProductNumber));
                return RedirectToAction("List");
            }

            var model = new ProductModel();

            // Convert EST To UTC
            string easternZoneId = "Eastern Standard Time";
            TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById(easternZoneId);
            model.AvailableStartDateTimeUtc = Convert.ToDateTime(TimeZoneInfo.ConvertTimeFromUtc(Convert.ToDateTime(Convert.ToDateTime("01/01/2018 12:00")), easternZone));
            model.AvailableEndDateTimeUtc = Convert.ToDateTime(TimeZoneInfo.ConvertTimeFromUtc(Convert.ToDateTime(Convert.ToDateTime("01/01/2050 12:00")), easternZone));
            model.SpecialPriceStartDateTimeUtc = Convert.ToDateTime(TimeZoneInfo.ConvertTimeFromUtc(Convert.ToDateTime(Convert.ToDateTime("12/31/1899 12:00")), easternZone));
            model.SpecialPriceEndDateTimeUtc = Convert.ToDateTime(TimeZoneInfo.ConvertTimeFromUtc(Convert.ToDateTime(Convert.ToDateTime("12/31/1899 12:00")), easternZone));


            PrepareProductModel(model, null, true, true);
            AddLocales(_languageService, model.Locales);
            PrepareAclModel(model, null, false);
            PrepareStoresMappingModel(model, null, false);
            PrepareCategoryMappingModel(model, null, false);
            PrepareManufacturerMappingModel(model, null, false);
            PrepareGlCodeMappingModel(model, null, false);
            PrepareDiscountMappingModel(model, null, false);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public ActionResult Create(ProductModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //validate maximum number of products per vendor
            if (_vendorSettings.MaximumProductNumber > 0 &&
                _workContext.CurrentVendor != null &&
                _productService.GetNumberOfProductsByVendorId(_workContext.CurrentVendor.Id) >= _vendorSettings.MaximumProductNumber)
            {
                ErrorNotification(string.Format(_localizationService.GetResource("Admin.Catalog.Products.ExceededMaximumNumber"), _vendorSettings.MaximumProductNumber));
                return RedirectToAction("List");
            }

            //---START: Codechages done by (na-sdxcorp\ADas)--------------
            //If shipping is enabled, dimensions and weight should be required. 
            if (model.IsShipEnabled && !(model.Weight > 0 && model.Height > 0 && model.Length > 0 && model.Width > 0))
            {
                ErrorNotification("Please enter valid dimensions and weight.");
                PrepareProductModel(model, null, false, true);
                PrepareAclModel(model, null, true);
                PrepareStoresMappingModel(model, null, true);
                PrepareCategoryMappingModel(model, null, true);
                PrepareManufacturerMappingModel(model, null, true);
                PrepareGlCodeMappingModel(model, null, true);
                PrepareDiscountMappingModel(model, null, true);

                return View(model);
            }
            //---END: Codechages done by (na-sdxcorp\ADas)--------------

            if (ModelState.IsValid)
            {
                int interval = 0;
                //a vendor should have access only to his products
                if (_workContext.CurrentVendor != null)
                {
                    model.VendorId = _workContext.CurrentVendor.Id;
                }
                //vendors cannot edit "Show on home page" property
                if (_workContext.CurrentVendor != null && model.ShowOnHomePage)
                {
                    model.ShowOnHomePage = false;
                }

                //product
                var product = model.ToEntity();
                // Convert EST To UTC
                string easternZoneId = "Eastern Standard Time";
                TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById(easternZoneId);
                product.AvailableStartDateTimeUtc = Convert.ToDateTime(TimeZoneInfo.ConvertTimeToUtc(Convert.ToDateTime(model.AvailableStartDateTimeUtc), easternZone));
                product.AvailableEndDateTimeUtc = Convert.ToDateTime(TimeZoneInfo.ConvertTimeToUtc(Convert.ToDateTime(model.AvailableEndDateTimeUtc), easternZone));
                product.SpecialPriceStartDateTimeUtc = Convert.ToDateTime(TimeZoneInfo.ConvertTimeToUtc(Convert.ToDateTime(model.SpecialPriceStartDateTimeUtc), easternZone));
                product.SpecialPriceEndDateTimeUtc = Convert.ToDateTime(TimeZoneInfo.ConvertTimeToUtc(Convert.ToDateTime(model.SpecialPriceEndDateTimeUtc), easternZone));

                product.CreatedOnUtc = DateTime.UtcNow;
                product.UpdatedOnUtc = DateTime.UtcNow;
                product.IsCorporateDonation = model.IsCorporateDonation;

                _productService.InsertProduct(product);

                #region reservation Config
                List<string> timeIntervals = new List<string>();
                List<string> timeSlots = new List<string>();
                List<ReservationProducts> reservationProducts = new List<ReservationProducts>();

                if (model.IsReservation)
                {
                    #region Delete existing 
                    var existingReservationDetailsforProduct = _productService.GetReservationProductsbyProductId(product.Id);
                    if (existingReservationDetailsforProduct.Count > 0)
                    {
                        foreach (ReservationProducts res in existingReservationDetailsforProduct.ToList())
                        {
                            _productService.DeleteReservationProducts(res);
                        }
                        _productService.UpdateProduct(product);
                    }
                    #endregion

                    #region Reservation TimeSlot Configuration and Product Update

                    int startHour = Convert.ToInt32(Request.Form["ReservationStartHour"]);
                    int startMin = Convert.ToInt32(Request.Form["ReservationStartMinute"]);
                    string startAMPM = Request.Form["ReservationStartAMPM"];

                    model.ReservationSelectedStartHour = Convert.ToInt32(Request.Form["ReservationStartHour"]);
                    model.ReservationSelectedStartMinute = Convert.ToInt32(Request.Form["ReservationStartMinute"]);
                    model.ReservationSelectedStartAMPM = Request.Form["ReservationStartAMPM"];

                    int endHour = Convert.ToInt32(Request.Form["ReservationEndHour"]);
                    int endMin = Convert.ToInt32(Request.Form["ReservationEndMinute"]);
                    string endAMPM = Request.Form["ReservationEndAMPM"];

                    model.ReservationSelectedEndHour = Convert.ToInt32(Request.Form["ReservationEndHour"]);
                    model.ReservationSelectedEndMinute = Convert.ToInt32(Request.Form["ReservationEndMinute"]);
                    model.ReservationSelectedEndAMPM = Request.Form["ReservationEndAMPM"];

                    interval = int.Parse(Request.Form["rdInterval"].ToString().Replace("min", ""));
                    if (interval == 1)
                    {
                        model.ReservationInterval = model.ReservationTimeInterval;
                        interval = model.ReservationTimeInterval;
                    }
                    else
                    {
                        model.ReservationInterval = interval;
                        model.ReservationTimeInterval = 0;
                    }

                    if (startAMPM == "PM" && startHour < 12)
                    {
                        startHour = startHour + 12;
                    }

                    if (endAMPM == "PM" && endHour < 12)
                    {
                        endHour = endHour + 12;
                    }

                    DateTime startDateValue = new DateTime(DateTime.Now.Year, DateTime.Now.Month,
                                            DateTime.Now.Day, startHour, startMin, 0);

                    DateTime endDateValue = new DateTime(DateTime.Now.Year, DateTime.Now.Month,
                                            DateTime.Now.Day, endHour, endMin, 0);



                    for (DateTime date = new DateTime(DateTime.Now.Year, DateTime.Now.Month,
                                            DateTime.Now.Day, startHour, startMin, 0)
                                            ; date <= endDateValue; date = date.AddMinutes(interval))
                    {
                        timeIntervals.Add(date.ToString("hh:mm tt"));
                    }

                    if (timeIntervals.Count > 0)
                    {

                        for (int i = 0; i < timeIntervals.Count - 1; i++)
                        {
                            string intervalStart = timeIntervals[i].ToString();
                            string intervalEnd = timeIntervals[i + 1].ToString();
                            timeSlots.Add(intervalStart + " - " + intervalEnd);
                        }
                    }
                    #endregion

                    #region prepare to insert list of reserved products

                    if (model.IsReservation)
                    {
                        if (timeSlots.Count > 0)
                        {
                            for (int i = 0; i <= timeSlots.Count - 1; i++)
                            {
                                ReservationProducts reservationProduct = new ReservationProducts();
                                reservationProduct.ProductId = product.Id;
                                reservationProduct.TimeSlotsConfigured = timeSlots[i];
                                reservationProduct.OccupancyUnitsAvailable = model.MaxOccupancy;
                                reservationProducts.Add(reservationProduct);
                            }
                        }
                        product.ReservationProducts = new List<ReservationProducts>();
                        product.ReservationProducts = reservationProducts;
                    }


                    if (product.ReservationProducts.Count > 0)
                    {
                        foreach (ReservationProducts res in product.ReservationProducts.ToList())
                        {
                            _productService.InsertReservationProducts(res);
                        }
                    }

                    #endregion

                    product.ReservationInterval = int.Parse(Request.Form["rdInterval"].ToString().Replace("min", ""));
                    product.ReservationStartTime = model.ReservationSelectedStartHour.ToString() + ":" + (model.ReservationSelectedStartMinute.ToString() == "0" ? "00" : model.ReservationSelectedStartMinute.ToString()) + " " + model.ReservationSelectedStartAMPM.ToString();
                    product.ReservationEndTime = model.ReservationSelectedEndHour.ToString() + ":" + (model.ReservationSelectedEndMinute.ToString() == "0" ? "00" : model.ReservationSelectedEndMinute.ToString()) + " " + model.ReservationSelectedEndAMPM.ToString();
                    product.LeadTime = model.LeadTime;
                    product.MaxWindowDays = model.MaxWindowDays;
                    product.ReservationCapPerSlot = model.ReservationCapPerSlot;

                    #region Leadtime logic

                    #region Delete Existing Unavlbl dates
                    if (model.LeadTime > 0 || model.MaxWindowDays > 0)
                    {
                        _productFulfillmentService.DeleteAllCalDays(model.MaxWindowDays);
                    }
                    #endregion

                    if (product.LeadTime > 0)
                    {
                        DateTime toDay = new DateTime(DateTime.Now.Year, DateTime.Now.Month,
                                            DateTime.Now.Day, 0, 0, 0);

                        DateTime lastDay = toDay.AddDays(product.LeadTime);

                        for (DateTime date = toDay; date <= lastDay; date = date.AddDays(1))
                        {
                            ProductFulfillmentCalendar day = new ProductFulfillmentCalendar();
                            day.Date = DateTime.Parse(date.ToString("yyyy-MM-dd"));

                            var existingDay = _productFulfillmentService.GetCalDay(product.Id, day);
                            if (existingDay == null)
                            {
                                _productFulfillmentService.InsertCalDay(product.Id, day);
                            }
                        }
                    }
                    #endregion Leadtime logic

                    #region MaxWindowDays logic
                    if (model.MaxWindowDays > 0)
                    {
                        DateTime lastWindowDay = DateTime.Now.AddDays(product.MaxWindowDays);
                        DateTime lastdayofYear = new DateTime(lastWindowDay.Year, 12, 31);

                        for (DateTime date = lastWindowDay; date <= lastdayofYear; date = date.AddDays(1))
                        {
                            ProductFulfillmentCalendar day = new ProductFulfillmentCalendar();
                            day.Date = DateTime.Parse(date.ToString("yyyy-MM-dd"));

                            var existingDay = _productFulfillmentService.GetCalDay(product.Id, day);
                            if (existingDay == null)
                            {
                                _productFulfillmentService.InsertCalDay(product.Id, day);
                            }
                        }
                    }
                    #endregion MaxWindowDays logic
                }
                else
                {
                    //Delete existing reservation config for the product
                    product.ReservationInterval = 0;
                    product.ReservationStartTime = null;
                    product.ReservationEndTime = null;
                    product.MaxOccupancy = 0;
                    product.IsReservation = false;
                    product.LeadTime = 0;

                    var existingReservationDetailsforProduct = _productService.GetReservationProductsbyProductId(product.Id);
                    if (existingReservationDetailsforProduct.Count > 0)
                    {
                        foreach (ReservationProducts res in existingReservationDetailsforProduct.ToList())
                        {
                            _productService.DeleteReservationProducts(res);
                        }
                        _productService.UpdateProduct(product);

                    }

                    //Delete all unavlbl calendar days.
                    _productFulfillmentService.DeleteAllCalDays(product.Id);
                }
                #endregion

                _productService.UpdateProduct(product);


                //search engine name
                model.SeName = product.ValidateSeName(model.SeName, product.Name, true);
                _urlRecordService.SaveSlug(product, model.SeName, 0);
                //locales
                UpdateLocales(product, model);
                //categories
                SaveCategoryMappings(product, model);
                //manufacturers
                SaveManufacturerMappings(product, model);

                SaveGLCodeMappings(product, model);
                //ACL (customer roles)
                SaveProductAcl(product, model);
                //stores
                SaveStoreMappings(product, model);
                //discounts
                SaveDiscountMappings(product, model);
                //tags
                SaveProductTags(product, ParseProductTags(model.ProductTags));
                //warehouses
                SaveProductWarehouseInventory(product, model);

                //activity log
                _customerActivityService.InsertActivity("AddNewProduct", _localizationService.GetResource("ActivityLog.AddNewProduct"), product.Name);

                SuccessNotification(_localizationService.GetResource("Admin.Catalog.Products.Added"));

                if (continueEditing)
                {
                    //selected tab
                    SaveSelectedTabName();

                    return RedirectToAction("Edit", new { id = product.Id });
                }
                return RedirectToAction(RedirectionTargetMasterLocal(product));	/// NU-10
            }

            //If we got this far, something failed, redisplay form
            PrepareProductModel(model, null, false, true);
            PrepareAclModel(model, null, true);
            PrepareStoresMappingModel(model, null, true);
            PrepareCategoryMappingModel(model, null, true);
            PrepareManufacturerMappingModel(model, null, true);
            PrepareGlCodeMappingModel(model, null, true);
            PrepareDiscountMappingModel(model, null, true);

            return View(model);
        }

        //edit product
        public ActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var product = _productService.GetProductById(id);
            if (product == null || product.Deleted)
                //No product found with the specified id
                return RedirectToAction("List");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return RedirectToAction("MasterList"); /// NU-10

            var model = product.ToModel();

            var EnableLocalAdminPublish = System.Configuration.ConfigurationManager.AppSettings["EnableLocalAdminPublish"];
            if (!string.IsNullOrEmpty(EnableLocalAdminPublish))
            {
                if (EnableLocalAdminPublish == "No")
                    model.EnableLocalAdminPublish = false;
                else
                    model.EnableLocalAdminPublish = true;
            }
            // Convert UTC To EST
            string easternZoneId = "Eastern Standard Time";
            TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById(easternZoneId);
            model.AvailableStartDateTimeUtc = Convert.ToDateTime(TimeZoneInfo.ConvertTimeFromUtc(Convert.ToDateTime(product.AvailableStartDateTimeUtc == null ? Convert.ToDateTime("01/01/2018 12:00") : product.AvailableStartDateTimeUtc), easternZone));
            model.AvailableEndDateTimeUtc = Convert.ToDateTime(TimeZoneInfo.ConvertTimeFromUtc(Convert.ToDateTime(product.AvailableEndDateTimeUtc == null ? Convert.ToDateTime("01/01/2050 12:00") : product.AvailableEndDateTimeUtc), easternZone));
            model.SpecialPriceStartDateTimeUtc = Convert.ToDateTime(TimeZoneInfo.ConvertTimeFromUtc(Convert.ToDateTime(product.SpecialPriceStartDateTimeUtc == null ? Convert.ToDateTime("12/31/1899 12:00") : product.SpecialPriceStartDateTimeUtc), easternZone));
            model.SpecialPriceEndDateTimeUtc = Convert.ToDateTime(TimeZoneInfo.ConvertTimeFromUtc(Convert.ToDateTime(product.SpecialPriceEndDateTimeUtc == null ? Convert.ToDateTime("12/31/1899 12:00") : product.SpecialPriceEndDateTimeUtc), easternZone));
            // End Convert

            model.IsBundleProduct = product.IsBundleProduct;
            PrepareProductModel(model, product, false, false);
            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = product.GetLocalized(x => x.Name, languageId, false, false);
                locale.ShortDescription = product.GetLocalized(x => x.ShortDescription, languageId, false, false);
                locale.FullDescription = product.GetLocalized(x => x.FullDescription, languageId, false, false);
                locale.MetaKeywords = product.GetLocalized(x => x.MetaKeywords, languageId, false, false);
                locale.MetaDescription = product.GetLocalized(x => x.MetaDescription, languageId, false, false);
                locale.MetaTitle = product.GetLocalized(x => x.MetaTitle, languageId, false, false);
                locale.SeName = product.GetSeName(languageId, false, false);
            });

            PrepareAclModel(model, product, false);
            PrepareStoresMappingModel(model, product, false);
            PrepareCategoryMappingModel(model, product, false);
            PrepareManufacturerMappingModel(model, product, false);
            PrepareGlCodeMappingModel(model, product, false);
            PrepareDiscountMappingModel(model, product, false);
            model.IsReadOnly = _workContext.CurrentCustomer.IsReadOnly();


            //string url = System.Web.HttpContext.Current.Request.Url.AbsoluteUri.ToString();
            //Log productlastEdited = _logger.ProductLastUpdatedBy(model.Id.ToString(), model.IsMaster ? "yes" : "no", url);
            //if (productlastEdited != null)
            //{
            //    string[] splitValue = productlastEdited.ShortMessage.Split(new[] { "ModifiedBy" }, StringSplitOptions.None);
            //    string userName = splitValue[1];
            //    model.UpdatedBy = userName;
            //}

            if (Session["OriginalProduct"] != null)
            {
                Session.Remove("OriginalProduct");
                Session["OriginalProduct"] = model;
            }
            else
            {
                Session["OriginalProduct"] = model;
            }
            return View(model);
        }



        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public ActionResult Edit(ProductModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var modelJson = Session["OriginalProduct"];

            var prevJson = JsonConvert.SerializeObject(modelJson,
                Formatting.None,
                new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });

            StringBuilder logString = new StringBuilder();
            int interval = 0;
            List<string> timeIntervals = new List<string>();
            List<string> timeSlots = new List<string>();
            List<ReservationProducts> reservationProducts = new List<ReservationProducts>();

            var product = _productService.GetProductById(model.Id);
            if (product == null || product.Deleted)
                //No product found with the specified id
                return RedirectToAction("List");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return RedirectToAction("List");

            //check if the product quantity has been changed while we were editing the product
            //and if it has been changed then we show error notification
            //and redirect on the editing page without data saving
            if (product.StockQuantity != model.LastStockQuantity)
            {
                ErrorNotification(_localizationService.GetResource("Admin.Catalog.Products.Fields.StockQuantity.ChangedWarning"));
                return RedirectToAction("Edit", new { id = product.Id });
            }
            if (model.SpecialPrice == decimal.Zero)
            {
                ErrorNotification("Special Price cannot be zero. it should either be blank or greater than zero");
                return RedirectToAction("Edit", new { id = product.Id });
            }

            //---START: Codechages done by (na-sdxcorp\ADas)--------------
            //If shipping is enabled, dimensions and weight should be required. 
            if (model.IsShipEnabled && !(model.Weight > 0 && model.Height > 0 && model.Length > 0 && model.Width > 0))
            {
                ErrorNotification("Please enter valid dimensions and weight.");
                return RedirectToAction("Edit", new { id = product.Id });
            }
            //---END: Codechages done by (na-sdxcorp\ADas)--------------


            if (product.IsMealPlan || model.IsDonation || model.IsReservation || model.IsGiftCard || model.IsDownload)
            {
                if (model.SelectedPaidGlCodeIds == null || model.SelectedPaidGlCodeIds.Count == 0)
                {
                    ErrorNotification("Please provide Payment GL code.");
                    return RedirectToAction("Edit", new { id = product.Id });
                }

            }
            else
            {
                if ((model.SelectedPaidGlCodeIds == null || model.SelectedPaidGlCodeIds.Count == 0) && (model.SelectedDeferredGlCodeIds == null || model.SelectedDeferredGlCodeIds.Count == 0))
                {
                    ErrorNotification("Please provide Payment GL code and Deferred GL code.");
                    return RedirectToAction("Edit", new { id = product.Id });
                }
                if (model.SelectedPaidGlCodeIds == null || model.SelectedPaidGlCodeIds.Count == 0)
                {
                    ErrorNotification("Please provide Payment GL code.");
                    return RedirectToAction("Edit", new { id = product.Id });
                }
                if (model.SelectedDeferredGlCodeIds == null || model.SelectedDeferredGlCodeIds.Count == 0)
                {
                    ErrorNotification("Please provide Deferred GL code.");
                    return RedirectToAction("Edit", new { id = product.Id });
                }
            }


            if (ModelState.IsValid)
            {
                //var errors = ModelState;
                bool published = false;
                var loggedinAs = _workContext.CurrentCustomer.IsLoggedAs();
                if (loggedinAs == "STORE.ADMIN")
                {
                    if (model.Published)
                    {
                        published = true;
                        model.Published = true;
                    }
                    else
                        published = false;

                    if (product.Weight > 0)
                        model.Weight = product.Weight;
                    if (product.Length > 0)
                        model.Length = product.Length;
                    if (product.Height > 0)
                        model.Height = product.Height;
                    if (product.Width > 0)
                        model.Width = product.Width;

                    if (product.IsShipEnabled)
                        model.IsShipEnabled = true;
                    else
                        model.IsShipEnabled = false;
                }

                //a vendor should have access only to his products
                if (_workContext.CurrentVendor != null)
                {
                    model.VendorId = _workContext.CurrentVendor.Id;
                }

                //we do not validate maximum number of products per vendor when editing existing products (only during creation of new products)

                //vendors cannot edit "Show on home page" property
                if (_workContext.CurrentVendor != null && model.ShowOnHomePage != product.ShowOnHomePage)
                {
                    model.ShowOnHomePage = product.ShowOnHomePage;
                }
                //some previously used values
                var prevStockQuantity = product.GetTotalStockQuantity();
                int prevDownloadId = product.DownloadId;
                int prevSampleDownloadId = product.SampleDownloadId;

                var newJson = JsonConvert.SerializeObject(model,
                Formatting.None,
                new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });
                List<string> changeditems = new List<string>();

                List<string> fields = new List<string>() {"Name","ShortDescription","FullDescription","WarehouseId",
                                        "OldPrice","Price","SpecialPrice",
                                        "SpecialPriceEndDateTimeUtc","SpecialPriceStartDateTimeUtc",
                                        "AvailableForPreOrder","IsTaxExempt", "TaxCategoryId","SelectedDeferredGlCodeIds",
                                        "SelectedPaidGlCodeIds","SelectedDiscountIds"};

                if (!JToken.DeepEquals(prevJson, newJson))
                {
                    JObject sourceJObject = JsonConvert.DeserializeObject<JObject>(prevJson);
                    JObject targetJObject = JsonConvert.DeserializeObject<JObject>(newJson);

                    foreach (var key in fields)
                    {
                        JProperty sourceProp = sourceJObject.Property(key);
                        JProperty targetProp = targetJObject.Property(key);
                        if (!JToken.DeepEquals(sourceProp.Value, targetProp.Value))
                        {
                            changeditems.Add(key);
                            logString.AppendFormat("{0}: {1} to {2} | ", key, sourceProp.Value, targetProp.Value);
                        }
                    }

                }

                //product
                product = model.ToEntity(product);

                if (!product.IsMaster)
                {
                    var MasterProduct = _productService.GetProductMaster(product);
                    if (MasterProduct != null)
                    {
                        // Don't need the if/else if this is common between the two and the only thing processed in the else.
                        // It should just always process.
                        if (MasterProduct.IsMealPlan)
                            product.IsMealPlan = true;
                        if (MasterProduct.ShowStandardMealPlanFields)
                            product.ShowStandardMealPlanFields = true;

                        bool isCurrentStoreVanguardStore = false;
                        isCurrentStoreVanguardStore = _storeContext.CurrentStore.IsContractShippingEnabled == true ||
                                                        _storeContext.CurrentStore.IsInterOfficeDeliveryEnabled == true ||
                                                         _storeContext.CurrentStore.IsTieredShippingEnabled == true;

                        if (loggedinAs == "STORE.ADMIN" && !isCurrentStoreVanguardStore)
                        {
                            if (MasterProduct.IsPickupEnabled)
                                product.IsPickupEnabled = true;
                            if (MasterProduct.IsShipEnabled)
                                product.IsShipEnabled = true;
                            if (product.IsLocalDelivery)
                                product.IsLocalDelivery = true;
                            if (published)
                            {
                                product.Published = true;
                            }
                            else
                            {
                                product.Published = false;
                            }
                        }
                        else if ((loggedinAs == "STORE.ADMIN" || loggedinAs == "ADMIN") && isCurrentStoreVanguardStore)
                        {
                            if (MasterProduct.IsPickupEnabled)
                                product.IsPickupEnabled = false;
                            if (MasterProduct.IsShipEnabled)
                                product.IsShipEnabled = true;
                            if (product.IsLocalDelivery)
                                product.IsLocalDelivery = true;
                            if (published)
                            {
                                product.Published = true;
                            }
                            else
                            {
                                product.Published = false;
                            }
                        }

                        // If the product is autofulfill, copy the master attributes for fulfillment options.
                        if (MasterProduct.IsAutoFulfill)
                        {
                            product.IsPickupEnabled = MasterProduct.IsPickupEnabled;
                            product.IsShipEnabled = MasterProduct.IsShipEnabled;
                            product.IsLocalDelivery = MasterProduct.IsLocalDelivery;
                        }
                    }
                }

                // Convert EST To UTC
                string easternZoneId = "Eastern Standard Time";
                TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById(easternZoneId);
                product.AvailableStartDateTimeUtc = Convert.ToDateTime(TimeZoneInfo.ConvertTimeToUtc(Convert.ToDateTime(model.AvailableStartDateTimeUtc == null ? Convert.ToDateTime("01/01/2018 10:01") : product.AvailableStartDateTimeUtc), easternZone));
                product.AvailableEndDateTimeUtc = Convert.ToDateTime(TimeZoneInfo.ConvertTimeToUtc(Convert.ToDateTime(model.AvailableEndDateTimeUtc == null ? Convert.ToDateTime("01/01/2050 10:01") : product.AvailableEndDateTimeUtc), easternZone));
                product.SpecialPriceStartDateTimeUtc = Convert.ToDateTime(TimeZoneInfo.ConvertTimeToUtc(Convert.ToDateTime(model.SpecialPriceStartDateTimeUtc == null ? Convert.ToDateTime("12/31/1899 10:01") : product.SpecialPriceStartDateTimeUtc), easternZone));
                product.SpecialPriceEndDateTimeUtc = Convert.ToDateTime(TimeZoneInfo.ConvertTimeToUtc(Convert.ToDateTime(model.SpecialPriceEndDateTimeUtc == null ? Convert.ToDateTime("12/31/1899 10:01") : product.SpecialPriceEndDateTimeUtc), easternZone));
                // End Convert
                product.UpdatedOnUtc = DateTime.UtcNow;
                if (model.SelectedWarehouseIds.Count > 1)
                {
                    product.PreferredPickupWarehouseId = 1;

                }
                if (model.SelectedWarehouseIds.Count == 1 && model.SelectedWarehouseIds.Contains(0))
                {
                    product.PreferredPickupWarehouseId = 0;
                }

                if (model.SelectedWarehouseIds.Count == 1 && !model.SelectedWarehouseIds.Contains(0))
                {
                    product.PreferredPickupWarehouseId = 1;
                }



                _productService.UpdateProduct(product);
                //search engine name
                model.SeName = product.ValidateSeName(model.SeName, product.Name, true);
                _urlRecordService.SaveSlug(product, model.SeName, 0);
                //locales
                UpdateLocales(product, model);
                //tags
                SaveProductTags(product, ParseProductTags(model.ProductTags));
                //warehouses
                SaveProductWarehouseInventory(product, model);
                //categories
                SaveCategoryMappings(product, model);
                //manufacturers
                SaveManufacturerMappings(product, model);

                SaveGLCodeMappings(product, model);
                //ACL (customer roles)
                SaveProductAcl(product, model);
                SaveProductPickupWarehouse(product, model);

                //stores
                if (product.IsMaster)
                {
                    model.Published = false; //NU-90
                }
                SaveStoreMappings(product, model);/// NU-10

                //discounts
                SaveDiscountMappings(product, model);
                //picture seo names
                UpdatePictureSeoNames(product);

                //back in stock notifications
                if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStock &&
                    product.BackorderMode == BackorderMode.NoBackorders &&
                    product.AllowBackInStockSubscriptions &&
                    product.GetTotalStockQuantity() > 0 &&
                    prevStockQuantity <= 0 &&
                    product.Published &&
                    !product.Deleted)
                {
                    _backInStockSubscriptionService.SendNotificationsToSubscribers(product);
                }
                //delete an old "download" file (if deleted or updated)
                if (prevDownloadId > 0 && prevDownloadId != product.DownloadId)
                {
                    var prevDownload = _downloadService.GetDownloadById(prevDownloadId);
                    if (prevDownload != null)
                        _downloadService.DeleteDownload(prevDownload);
                }
                //delete an old "sample download" file (if deleted or updated)
                if (prevSampleDownloadId > 0 && prevSampleDownloadId != product.SampleDownloadId)
                {
                    var prevSampleDownload = _downloadService.GetDownloadById(prevSampleDownloadId);
                    if (prevSampleDownload != null)
                        _downloadService.DeleteDownload(prevSampleDownload);
                }

                UpdateCopiesFromMaster(product.Id); /// NU-10

                #region reservation Config

                if (model.IsReservation)
                {
                    #region Delete existing 
                    var existingReservationDetailsforProduct = _productService.GetReservationProductsbyProductId(model.Id);
                    if (existingReservationDetailsforProduct.Count > 0)
                    {
                        foreach (ReservationProducts res in existingReservationDetailsforProduct.ToList())
                        {
                            _productService.DeleteReservationProducts(res);
                        }
                        _productService.UpdateProduct(product);
                    }
                    #endregion

                    #region Reservation TimeSlot Configuration and Product Update

                    int startHour = Convert.ToInt32(Request.Form["ReservationStartHour"]);
                    int startMin = Convert.ToInt32(Request.Form["ReservationStartMinute"]);
                    string startAMPM = Request.Form["ReservationStartAMPM"];

                    model.ReservationSelectedStartHour = Convert.ToInt32(Request.Form["ReservationStartHour"]);
                    model.ReservationSelectedStartMinute = Convert.ToInt32(Request.Form["ReservationStartMinute"]);
                    model.ReservationSelectedStartAMPM = Request.Form["ReservationStartAMPM"];

                    int endHour = Convert.ToInt32(Request.Form["ReservationEndHour"]);
                    int endMin = Convert.ToInt32(Request.Form["ReservationEndMinute"]);
                    string endAMPM = Request.Form["ReservationEndAMPM"];

                    model.ReservationSelectedEndHour = Convert.ToInt32(Request.Form["ReservationEndHour"]);
                    model.ReservationSelectedEndMinute = Convert.ToInt32(Request.Form["ReservationEndMinute"]);
                    model.ReservationSelectedEndAMPM = Request.Form["ReservationEndAMPM"];

                    if (Request.Form["rdInterval"] != null)
                    {
                        interval = int.Parse(Request.Form["rdInterval"].ToString().Replace("min", ""));
                        if (interval == 1)
                        {
                            model.ReservationInterval = model.ReservationTimeInterval;
                            interval = model.ReservationTimeInterval;
                        }
                        else
                        {
                            model.ReservationInterval = interval;
                            model.ReservationTimeInterval = 0;
                        }
                        if (startAMPM == "PM" && startHour < 12)
                        {
                            startHour = startHour + 12;
                        }

                        if (endAMPM == "PM" && endHour < 12)
                        {
                            endHour = endHour + 12;
                        }

                        DateTime startDateValue = new DateTime(DateTime.Now.Year, DateTime.Now.Month,
                                                DateTime.Now.Day, startHour, startMin, 0);

                        DateTime endDateValue = new DateTime(DateTime.Now.Year, DateTime.Now.Month,
                                                DateTime.Now.Day, endHour, endMin, 0);



                        for (DateTime date = new DateTime(DateTime.Now.Year, DateTime.Now.Month,
                                                DateTime.Now.Day, startHour, startMin, 0)
                                                ; date <= endDateValue; date = date.AddMinutes(interval))
                        {
                            timeIntervals.Add(date.ToString("hh:mm tt"));
                        }

                        if (timeIntervals.Count > 0)
                        {

                            for (int i = 0; i < timeIntervals.Count - 1; i++)
                            {
                                string intervalStart = timeIntervals[i].ToString();
                                string intervalEnd = timeIntervals[i + 1].ToString();
                                timeSlots.Add(intervalStart + " - " + intervalEnd);
                            }
                        }
                    }
                    #endregion

                    #region prepare to insert list of reserved products

                    if (model.IsReservation)
                    {
                        if (timeSlots.Count > 0)
                        {
                            for (int i = 0; i <= timeSlots.Count - 1; i++)
                            {
                                ReservationProducts reservationProduct = new ReservationProducts();
                                reservationProduct.ProductId = model.Id;
                                reservationProduct.TimeSlotsConfigured = timeSlots[i];
                                reservationProduct.OccupancyUnitsAvailable = model.MaxOccupancy;
                                reservationProducts.Add(reservationProduct);
                            }
                        }
                        model.ReservationProducts = reservationProducts;
                    }


                    if (model.ReservationProducts.Count > 0)
                    {
                        foreach (ReservationProducts res in model.ReservationProducts.ToList())
                        {
                            _productService.InsertReservationProducts(res);
                        }
                    }

                    #endregion

                    product.ReservationInterval = Request.Form["rdInterval"] != null ? int.Parse(Request.Form["rdInterval"].ToString().Replace("min", "")) : 0 ;
                    product.ReservationStartTime = model.ReservationSelectedStartHour.ToString() + ":" + (model.ReservationSelectedStartMinute.ToString() == "0" ? "00" : model.ReservationSelectedStartMinute.ToString()) + " " + model.ReservationSelectedStartAMPM.ToString();
                    product.ReservationEndTime = model.ReservationSelectedEndHour.ToString() + ":" + (model.ReservationSelectedEndMinute.ToString() == "0" ? "00" : model.ReservationSelectedEndMinute.ToString()) + " " + model.ReservationSelectedEndAMPM.ToString();
                    product.LeadTime = model.LeadTime;
                    product.MaxWindowDays = model.MaxWindowDays;
                    product.ReservationCapPerSlot = model.ReservationCapPerSlot;
                    product.ReservationTimeInterval = model.ReservationTimeInterval;

                    #region Leadtime logic

                    #region Delete Existing Unavlbl dates
                    if (model.LeadTime > 0 || model.MaxWindowDays > 0)
                    {
                        _productFulfillmentService.DeleteAllCalDays(model.MaxWindowDays);
                    }
                    #endregion
                    if (model.LeadTime > 0)
                    {
                        DateTime toDay = new DateTime(DateTime.Now.Year, DateTime.Now.Month,
                                            DateTime.Now.Day, 0, 0, 0);

                        DateTime lastDay = toDay.AddDays(product.LeadTime);

                        for (DateTime date = toDay; date <= lastDay; date = date.AddDays(1))
                        {
                            ProductFulfillmentCalendar day = new ProductFulfillmentCalendar();
                            day.Date = DateTime.Parse(date.ToString("yyyy-MM-dd"));

                            var existingDay = _productFulfillmentService.GetCalDay(product.Id, day);
                            if (existingDay == null)
                            {
                                _productFulfillmentService.InsertCalDay(product.Id, day);
                            }
                        }
                    }
                    #endregion Leadtime logic

                    #region MaxWindowDays logic
                    if (model.MaxWindowDays > 0)
                    {
                        DateTime lastWindowDay = DateTime.Now.AddDays(product.MaxWindowDays);
                        DateTime lastdayofYear = new DateTime(lastWindowDay.Year, 12, 31);

                        for (DateTime date = lastWindowDay; date <= lastdayofYear; date = date.AddDays(1))
                        {
                            ProductFulfillmentCalendar day = new ProductFulfillmentCalendar();
                            day.Date = DateTime.Parse(date.ToString("yyyy-MM-dd"));

                            var existingDay = _productFulfillmentService.GetCalDay(product.Id, day);
                            if (existingDay == null)
                            {
                                _productFulfillmentService.InsertCalDay(product.Id, day);
                            }
                        }
                    }
                    #endregion MaxWindowDays logic
                }
                else
                {
                    //Delete existing reservation config for the product
                    product.ReservationInterval = 0;
                    product.ReservationStartTime = null;
                    product.ReservationEndTime = null;
                    product.MaxOccupancy = 0;
                    product.IsReservation = false;
                    product.LeadTime = 0;

                    var existingReservationDetailsforProduct = _productService.GetReservationProductsbyProductId(product.Id);
                    if (existingReservationDetailsforProduct.Count > 0)
                    {
                        foreach (ReservationProducts res in existingReservationDetailsforProduct.ToList())
                        {
                            _productService.DeleteReservationProducts(res);
                        }
                        _productService.UpdateProduct(product);

                    }

                    //Delete all unavlbl calendar days.
                    _productFulfillmentService.DeleteAllCalDays(product.Id);
                }
                #endregion

                _productService.UpdateProduct(product);

                #endregion

                //activity log
                _customerActivityService.InsertActivity("EditProduct", _localizationService.GetResource("ActivityLog.EditProduct"), product.Name);

                SuccessNotification(_localizationService.GetResource("Admin.Catalog.Products.Updated"));
                //Session["OriginalProduct"] = product;

                if (changeditems.Count() > 0)
                {
                    _logger.InsertLog(LogLevel.Information, string.Format("ProductID {0} IsMaster {1} ModifiedBy {2}", model.Id, model.IsMaster ? "yes" : "no", _workContext.CurrentCustomer.GetFullName()), logString.ToString());
                }

                if (continueEditing)
                {
                    //selected tab
                    SaveSelectedTabName();

                    return RedirectToAction("Edit", new { id = product.Id });
                }
                return RedirectToAction(RedirectionTargetMasterLocal(product)); /// NU-10
            }

            //If we got this far, something failed, redisplay form
            PrepareProductModel(model, product, false, true);
            PrepareAclModel(model, product, true);
            PrepareStoresMappingModel(model, product, true);
            PrepareCategoryMappingModel(model, product, true);
            PrepareManufacturerMappingModel(model, product, true);
            PrepareGlCodeMappingModel(model, product, true);
            PrepareDiscountMappingModel(model, product, true);


            return View(model);
        }

        //delete product
        [HttpPost]
        public ActionResult Delete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var product = _productService.GetProductById(id);
            if (product == null)
                //No product found with the specified id
                return RedirectToAction("List");

            string redirectTarget = RedirectionTargetMasterLocal(product); /// NU-10
            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return RedirectToAction("List");

            _productService.DeleteProduct(product);
            UpdateCopiesFromMaster(product.Id);	/// NU-10

            //activity log
            _customerActivityService.InsertActivity("DeleteProduct", _localizationService.GetResource("ActivityLog.DeleteProduct"), product.Name);

            SuccessNotification(_localizationService.GetResource("Admin.Catalog.Products.Deleted"));
            return RedirectToAction(redirectTarget); /// NU-10
        }

        [HttpPost]
        public ActionResult DeleteSelected(ICollection<int> selectedIds)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            if (selectedIds != null)
            {
                #region NU-10
                foreach (var product in _productService.GetProductsByIds(selectedIds.ToArray()).Where(p => _workContext.CurrentVendor == null || p.VendorId == _workContext.CurrentVendor.Id).ToList())
                {
                    _productService.DeleteProduct(product);
                    UpdateCopiesFromMaster(product.Id);
                }
                #endregion
            }

            return Json(new { Result = true });
        }

        [HttpPost]
        public ActionResult CopyProduct(ProductModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var copyModel = model.CopyProductModel;
            try
            {
                var originalProduct = _productService.GetProductById(copyModel.Id);

                //a vendor should have access only to his products
                if (_workContext.CurrentVendor != null && originalProduct.VendorId != _workContext.CurrentVendor.Id)
                    return RedirectToAction("List");

                var newProduct = _copyProductService.CopyProduct(originalProduct,
                    copyModel.Name, copyModel.Published, copyModel.CopyImages);
                SuccessNotification("The product has been copied successfully");
                return RedirectToAction("Edit", new { id = newProduct.Id });
            }
            catch (Exception exc)
            {
                ErrorNotification(exc.Message);
                return RedirectToAction("Edit", new { id = copyModel.Id });
            }
        }


        #endregion

        #region Required products

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult LoadProductFriendlyNames(string productIds)
        {
            var result = "";

            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return Json(new { Text = result });

            if (!String.IsNullOrWhiteSpace(productIds))
            {
                var ids = new List<int>();
                var rangeArray = productIds
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim())
                    .ToList();

                foreach (string str1 in rangeArray)
                {
                    int tmp1;
                    if (int.TryParse(str1, out tmp1))
                        ids.Add(tmp1);
                }

                var products = _productService.GetProductsByIds(ids.ToArray());
                for (int i = 0; i <= products.Count - 1; i++)
                {
                    result += products[i].Name;
                    if (i != products.Count - 1)
                        result += ", ";
                }
            }

            return Json(new { Text = result });
        }

        public ActionResult RequiredProductAddPopup(string btnId, string productIdsInput)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var model = new ProductModel.AddRequiredProductModel();
            //a vendor should have access only to his products
            model.IsLoggedInAsVendor = _workContext.CurrentVendor != null;
            model.IsLoggedAs = _workContext.CurrentCustomer.IsLoggedAs();	// NU-1

            //categories
            model.AvailableCategories.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            var categories = SelectListHelper.GetCategoryList(_categoryService, _cacheManager, true);
            foreach (var c in categories)
                model.AvailableCategories.Add(c);

            //manufacturers
            model.AvailableManufacturers.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            foreach (var m in _manufacturerService.GetAllManufacturers(showHidden: true))
                model.AvailableManufacturers.Add(new SelectListItem { Text = m.Name, Value = m.Id.ToString() });

            //stores
            model.AvailableStores.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            foreach (var s in _storeService.GetAllStores())
                model.AvailableStores.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString() });

            //vendors
            model.AvailableVendors.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            foreach (var v in _vendorService.GetAllVendors(showHidden: true))
                model.AvailableVendors.Add(new SelectListItem { Text = v.Name, Value = v.Id.ToString() });

            //product types
            model.AvailableProductTypes = ProductType.SimpleProduct.ToSelectList(false).ToList();
            model.AvailableProductTypes.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });


            ViewBag.productIdsInput = productIdsInput;
            ViewBag.btnId = btnId;

            return View(model);
        }

        [HttpPost]
        public ActionResult RequiredProductAddPopupList(DataSourceRequest command, ProductModel.AddRequiredProductModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                model.SearchVendorId = _workContext.CurrentVendor.Id;
            }

            var products = _productService.SearchProducts(
                categoryIds: new List<int> { model.SearchCategoryId },
                manufacturerId: model.SearchManufacturerId,
                storeId: model.SearchStoreId,
                vendorId: model.SearchVendorId,
                productType: model.SearchProductTypeId > 0 ? (ProductType?)model.SearchProductTypeId : null,
                keywords: model.SearchProductName,
                pageIndex: command.Page - 1,
                pageSize: command.PageSize,
                showHidden: true
                );
            var gridModel = new DataSourceResult();
            gridModel.Data = products.Select(x => x.ToModel()).
                OrderBy(p => p.Name); /// NU-24
            gridModel.Total = products.TotalCount;

            return Json(gridModel);
        }

        #endregion

        #region Related products

        [HttpPost]
        public ActionResult RelatedProductList(DataSourceRequest command, int productId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                var product = _productService.GetProductById(productId);
                if (product != null && product.VendorId != _workContext.CurrentVendor.Id)
                {
                    return Content("This is not your product");
                }
            }

            var relatedProducts = _productService.GetRelatedProductsByProductId1(productId, true);
            var relatedProductsModel = relatedProducts
                .Select(x => new ProductModel.RelatedProductModel
                {
                    Id = x.Id,
                    //ProductId1 = x.ProductId1,
                    ProductId2 = x.ProductId2,
                    Product2Name = _productService.GetProductById(x.ProductId2).Name,
                    DisplayOrder = x.DisplayOrder
                })
                .ToList();

            var gridModel = new DataSourceResult
            {
                Data = relatedProductsModel.OrderBy(p => p.Product2Name), /// NU-24
                Total = relatedProductsModel.Count
            };

            return Json(gridModel);
        }

        [HttpPost]
        public ActionResult RelatedProductUpdate(ProductModel.RelatedProductModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var relatedProduct = _productService.GetRelatedProductById(model.Id);
            if (relatedProduct == null)
                throw new ArgumentException("No related product found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                var product = _productService.GetProductById(relatedProduct.ProductId1);
                if (product != null && product.VendorId != _workContext.CurrentVendor.Id)
                {
                    return Content("This is not your product");
                }
            }

            relatedProduct.DisplayOrder = model.DisplayOrder;
            _productService.UpdateRelatedProduct(relatedProduct);

            return new NullJsonResult();
        }

        [HttpPost]
        public ActionResult RelatedProductDelete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var relatedProduct = _productService.GetRelatedProductById(id);
            if (relatedProduct == null)
                throw new ArgumentException("No related product found with the specified id");

            var productId = relatedProduct.ProductId1;

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                var product = _productService.GetProductById(productId);
                if (product != null && product.VendorId != _workContext.CurrentVendor.Id)
                {
                    return Content("This is not your product");
                }
            }

            _productService.DeleteRelatedProduct(relatedProduct);

            return new NullJsonResult();
        }

        public ActionResult RelatedProductAddPopup(int productId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var model = new ProductModel.AddRelatedProductModel();
            //a vendor should have access only to his products
            model.IsLoggedInAsVendor = _workContext.CurrentVendor != null;
            model.IsLoggedAs = _workContext.CurrentCustomer.IsLoggedAs();	// NU-1

            //categories
            model.AvailableCategories.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            var categories = SelectListHelper.GetCategoryList(_categoryService, _cacheManager, true);
            foreach (var c in categories)
                model.AvailableCategories.Add(c);

            //manufacturers
            model.AvailableManufacturers.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            foreach (var m in _manufacturerService.GetAllManufacturers(showHidden: true))
                model.AvailableManufacturers.Add(new SelectListItem { Text = m.Name, Value = m.Id.ToString() });

            //stores
            model.AvailableStores.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            foreach (var s in _storeService.GetAllStores())
                model.AvailableStores.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString() });

            //vendors
            model.AvailableVendors.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            foreach (var v in _vendorService.GetAllVendors(showHidden: true))
                model.AvailableVendors.Add(new SelectListItem { Text = v.Name, Value = v.Id.ToString() });

            //product types
            model.AvailableProductTypes = ProductType.SimpleProduct.ToSelectList(false).ToList();
            model.AvailableProductTypes.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });

            return View(model);
        }

        [HttpPost]
        public ActionResult RelatedProductAddPopupList(DataSourceRequest command, ProductModel.AddRelatedProductModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                model.SearchVendorId = _workContext.CurrentVendor.Id;
            }
            var products = _productService.SearchProducts(
               categoryIds: new List<int> { model.SearchCategoryId },
               manufacturerId: model.SearchManufacturerId,
               storeId: model.SearchStoreId,
               vendorId: model.SearchVendorId,
               productType: model.SearchProductTypeId > 0 ? (ProductType?)model.SearchProductTypeId : null,
               keywords: model.SearchProductName,
               pageIndex: command.Page - 1,
               pageSize: command.PageSize,
               showHidden: true, overrideHideMasterProducts: false
               );

            //var products = _productService.SearchProducts(
            //    categoryIds: new List<int> { model.SearchCategoryId },
            //    manufacturerId: model.SearchManufacturerId,
            //    storeId: model.SearchStoreId,
            //    vendorId: model.SearchVendorId,
            //    productType: model.SearchProductTypeId > 0 ? (ProductType?)model.SearchProductTypeId : null,
            //    keywords: model.SearchProductName,
            //    pageIndex: command.Page - 1,
            //    pageSize: command.PageSize,
            //    overrideHideMasterProducts: true,
            //    showHidden: true
            //    );
            var gridModel = new DataSourceResult();
            gridModel.Data = products.Select(x => x.ToModel()).
                OrderBy(p => p.Name); /// NU-24
            gridModel.Total = products.TotalCount;

            return Json(gridModel);
        }


        [HttpPost]
        [FormValueRequired("save")]
        public ActionResult RelatedProductAddPopup(string btnId, string formId, ProductModel.AddRelatedProductModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            if (model.SelectedProductIds != null)
            {
                foreach (int id in model.SelectedProductIds)
                {
                    var product = _productService.GetProductById(id);
                    if (product != null)
                    {
                        //a vendor should have access only to his products
                        if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                            continue;

                        var existingRelatedProducts = _productService.GetRelatedProductsByProductId1(model.ProductId);
                        if (existingRelatedProducts.FindRelatedProduct(model.ProductId, id) == null)
                        {
                            _productService.InsertRelatedProduct(
                                new RelatedProduct
                                {
                                    ProductId1 = model.ProductId,
                                    ProductId2 = id,
                                    DisplayOrder = 1
                                });
                        }
                    }
                }
            }

            //a vendor should have access only to his products
            model.IsLoggedInAsVendor = _workContext.CurrentVendor != null;
            model.IsLoggedAs = _workContext.CurrentCustomer.IsLoggedAs();	// NU-1
            ViewBag.RefreshPage = true;
            ViewBag.btnId = btnId;
            ViewBag.formId = formId;
            return View(model);
        }

        #endregion

        #region Product Relation

        [HttpPost]
        public ActionResult ProductRelationLists(DataSourceRequest command, int productId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                var product = _productService.GetProductById(productId);
                if (product != null && product.VendorId != _workContext.CurrentVendor.Id)
                {
                    return Content("This is not your product");
                }
            }

            var relatedProducts = _productService.GetRelatedProductsByProductId1(productId, true);
            var relatedProductsModel = relatedProducts
                .Select(x => new ProductModel.RelatedProductModel
                {
                    Id = x.Id,
                    //ProductId1 = x.ProductId1,
                    ProductId2 = x.ProductId2,
                    Product2Name = _productService.GetProductById(x.ProductId2).Name,
                    DisplayOrder = x.DisplayOrder
                })
                .ToList();

            var gridModel = new DataSourceResult
            {
                Data = relatedProductsModel.OrderBy(p => p.Product2Name), /// NU-24
                Total = relatedProductsModel.Count
            };

            return Json(gridModel);
        }

        //[HttpPost]
        //public ActionResult RelatedProductUpdate(ProductModel.RelatedProductModel model)
        //{
        //    if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
        //        return AccessDeniedView();

        //    var relatedProduct = _productService.GetRelatedProductById(model.Id);
        //    if (relatedProduct == null)
        //        throw new ArgumentException("No related product found with the specified id");

        //    //a vendor should have access only to his products
        //    if (_workContext.CurrentVendor != null)
        //    {
        //        var product = _productService.GetProductById(relatedProduct.ProductId1);
        //        if (product != null && product.VendorId != _workContext.CurrentVendor.Id)
        //        {
        //            return Content("This is not your product");
        //        }
        //    }

        //    relatedProduct.DisplayOrder = model.DisplayOrder;
        //    _productService.UpdateRelatedProduct(relatedProduct);

        //    return new NullJsonResult();
        //}

        //[HttpPost]
        //public ActionResult RelatedProductDelete(int id)
        //{
        //    if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
        //        return AccessDeniedView();

        //    var relatedProduct = _productService.GetRelatedProductById(id);
        //    if (relatedProduct == null)
        //        throw new ArgumentException("No related product found with the specified id");

        //    var productId = relatedProduct.ProductId1;

        //    //a vendor should have access only to his products
        //    if (_workContext.CurrentVendor != null)
        //    {
        //        var product = _productService.GetProductById(productId);
        //        if (product != null && product.VendorId != _workContext.CurrentVendor.Id)
        //        {
        //            return Content("This is not your product");
        //        }
        //    }

        //    _productService.DeleteRelatedProduct(relatedProduct);

        //    return new NullJsonResult();
        //}

        public ActionResult ProductRelationAddPopup()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var model = new ProductModel.AddRelatedProductModel();
            //a vendor should have access only to his products
            model.IsLoggedInAsVendor = _workContext.CurrentVendor != null;
            model.IsLoggedAs = _workContext.CurrentCustomer.IsLoggedAs();	// NU-1

            //categories
            model.AvailableCategories.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            var categories = SelectListHelper.GetCategoryList(_categoryService, _cacheManager, true);
            foreach (var c in categories)
                model.AvailableCategories.Add(c);

            //manufacturers
            model.AvailableManufacturers.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            foreach (var m in _manufacturerService.GetAllManufacturers(showHidden: true))
                model.AvailableManufacturers.Add(new SelectListItem { Text = m.Name, Value = m.Id.ToString() });

            //stores
            model.AvailableStores.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            foreach (var s in _storeService.GetAllStores())
                model.AvailableStores.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString() });

            //vendors
            model.AvailableVendors.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            foreach (var v in _vendorService.GetAllVendors(showHidden: true))
                model.AvailableVendors.Add(new SelectListItem { Text = v.Name, Value = v.Id.ToString() });

            //product types
            model.AvailableProductTypes = ProductType.SimpleProduct.ToSelectList(false).ToList();
            model.AvailableProductTypes.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });

            return View(model);
        }

        [HttpPost]
        public ActionResult ProductRelationAddPopupList(DataSourceRequest command, ProductModel.AddRelatedProductModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                model.SearchVendorId = _workContext.CurrentVendor.Id;
            }

            var products = _productService.SearchProducts(
                categoryIds: new List<int> { model.SearchCategoryId },
                manufacturerId: model.SearchManufacturerId,
                storeId: _storeContext.CurrentStore.Id,
                vendorId: model.SearchVendorId,
                productType: model.SearchProductTypeId > 0 ? (ProductType?)model.SearchProductTypeId : null,
                keywords: model.SearchProductName,
                //pageIndex: command.Page - 1,
                //pageSize: command.PageSize,
                overrideHideMasterProducts: true,
                showHidden: true
                );
            var gridModel = new DataSourceResult();
            gridModel.Data = products.Select(x => x.ToModel()).
                OrderBy(p => p.Name); /// NU-24
            gridModel.Total = products.TotalCount;

            return Json(gridModel);
        }


        [HttpPost]
        [FormValueRequired("save")]
        public ActionResult ProductRelationAddPopup(string btnId, string formId, ProductModel.AddRelatedProductModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            if (model.SelectedProductIds != null)
            {
                foreach (int id in model.SelectedProductIds)
                {
                    var product = _productService.GetProductById(id);
                    if (product != null)
                    {
                        //a vendor should have access only to his products
                        if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                            continue;

                        //var existingRelatedProducts = _productService.GetProductRelation(model.ProductId);
                        //if (existingRelatedProducts.FindRelatedProduct(model.ProductId, id) == null)
                        //{
                        _productService.InsertProductRelation(
                            new RelatedProduct
                            {
                                ProductId1 = model.ProductId,
                                ProductId2 = id,
                                DisplayOrder = 1
                            });
                        //}
                    }
                }
            }

            //a vendor should have access only to his products
            model.IsLoggedInAsVendor = _workContext.CurrentVendor != null;
            model.IsLoggedAs = _workContext.CurrentCustomer.IsLoggedAs();	// NU-1
            ViewBag.RefreshPage = true;
            ViewBag.btnId = btnId;
            ViewBag.formId = formId;
            return View(model);
        }

        #endregion

        #region Cross-sell products

        [HttpPost]
        public ActionResult CrossSellProductList(DataSourceRequest command, int productId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                var product = _productService.GetProductById(productId);
                if (product != null && product.VendorId != _workContext.CurrentVendor.Id)
                {
                    return Content("This is not your product");
                }
            }

            var crossSellProducts = _productService.GetCrossSellProductsByProductId1(productId, true);
            var crossSellProductsModel = crossSellProducts
                .Select(x => new ProductModel.CrossSellProductModel
                {
                    Id = x.Id,
                    //ProductId1 = x.ProductId1,
                    ProductId2 = x.ProductId2,
                    Product2Name = _productService.GetProductById(x.ProductId2).Name,
                })
                .ToList();

            var gridModel = new DataSourceResult
            {
                Data = crossSellProductsModel.
                    OrderBy(p => p.Product2Name), /// NU-24
                Total = crossSellProductsModel.Count
            };

            return Json(gridModel);
        }

        [HttpPost]
        public ActionResult CrossSellProductDelete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var crossSellProduct = _productService.GetCrossSellProductById(id);
            if (crossSellProduct == null)
                throw new ArgumentException("No cross-sell product found with the specified id");

            var productId = crossSellProduct.ProductId1;

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                var product = _productService.GetProductById(productId);
                if (product != null && product.VendorId != _workContext.CurrentVendor.Id)
                {
                    return Content("This is not your product");
                }
            }

            _productService.DeleteCrossSellProduct(crossSellProduct);

            return new NullJsonResult();
        }

        public ActionResult CrossSellProductAddPopup(int productId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var model = new ProductModel.AddCrossSellProductModel();
            //a vendor should have access only to his products
            model.IsLoggedInAsVendor = _workContext.CurrentVendor != null;
            model.IsLoggedAs = _workContext.CurrentCustomer.IsLoggedAs();	// NU-1

            //categories
            model.AvailableCategories.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            var categories = SelectListHelper.GetCategoryList(_categoryService, _cacheManager, true);
            foreach (var c in categories)
                model.AvailableCategories.Add(c);

            //manufacturers
            model.AvailableManufacturers.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            foreach (var m in _manufacturerService.GetAllManufacturers(showHidden: true))
                model.AvailableManufacturers.Add(new SelectListItem { Text = m.Name, Value = m.Id.ToString() });

            //stores
            model.AvailableStores.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            foreach (var s in _storeService.GetAllStores())
                model.AvailableStores.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString() });

            //vendors
            model.AvailableVendors.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            foreach (var v in _vendorService.GetAllVendors(showHidden: true))
                model.AvailableVendors.Add(new SelectListItem { Text = v.Name, Value = v.Id.ToString() });

            //product types
            model.AvailableProductTypes = ProductType.SimpleProduct.ToSelectList(false).ToList();
            model.AvailableProductTypes.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });

            return View(model);
        }

        [HttpPost]
        public ActionResult CrossSellProductAddPopupList(DataSourceRequest command, ProductModel.AddCrossSellProductModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                model.SearchVendorId = _workContext.CurrentVendor.Id;
            }

            var products = _productService.SearchProducts(
                categoryIds: new List<int> { model.SearchCategoryId },
                manufacturerId: model.SearchManufacturerId,
                storeId: model.SearchStoreId,
                vendorId: model.SearchVendorId,
                productType: model.SearchProductTypeId > 0 ? (ProductType?)model.SearchProductTypeId : null,
                keywords: model.SearchProductName,
                pageIndex: command.Page - 1,
                pageSize: command.PageSize,
                showHidden: true
                );
            var gridModel = new DataSourceResult();
            gridModel.Data = products.Select(x => x.ToModel()).
                OrderBy(p => p.Name); /// NU-24
            gridModel.Total = products.TotalCount;

            return Json(gridModel);
        }

        [HttpPost]
        [FormValueRequired("save")]
        public ActionResult CrossSellProductAddPopup(string btnId, string formId, ProductModel.AddCrossSellProductModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            if (model.SelectedProductIds != null)
            {
                foreach (int id in model.SelectedProductIds)
                {
                    var product = _productService.GetProductById(id);
                    if (product != null)
                    {
                        //a vendor should have access only to his products
                        if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                            continue;

                        var existingCrossSellProducts = _productService.GetCrossSellProductsByProductId1(model.ProductId);
                        if (existingCrossSellProducts.FindCrossSellProduct(model.ProductId, id) == null)
                        {
                            _productService.InsertCrossSellProduct(
                                new CrossSellProduct
                                {
                                    ProductId1 = model.ProductId,
                                    ProductId2 = id,
                                });
                        }
                    }
                }
            }

            //a vendor should have access only to his products
            model.IsLoggedInAsVendor = _workContext.CurrentVendor != null;
            model.IsLoggedAs = _workContext.CurrentCustomer.IsLoggedAs();	// NU-1
            ViewBag.RefreshPage = true;
            ViewBag.btnId = btnId;
            ViewBag.formId = formId;
            return View(model);
        }

        #endregion

        #region Associated products

        [HttpPost]
        public ActionResult AssociatedProductList(DataSourceRequest command, int productId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                var product = _productService.GetProductById(productId);
                if (product != null && product.VendorId != _workContext.CurrentVendor.Id)
                {
                    return Content("This is not your product");
                }
            }

            //a vendor should have access only to his products
            var vendorId = 0;
            if (_workContext.CurrentVendor != null)
            {
                vendorId = _workContext.CurrentVendor.Id;
            }

            var associatedProducts = _productService.GetAssociatedProducts(parentGroupedProductId: productId,
                vendorId: vendorId,
                showHidden: true);
            var associatedProductsModel = associatedProducts
                .Select(x => new ProductModel.AssociatedProductModel
                {
                    Id = x.Id,
                    ProductName = x.Name,
                    DisplayOrder = x.DisplayOrder
                })
                .ToList();

            var gridModel = new DataSourceResult
            {
                Data = associatedProductsModel.
                    OrderBy(p => p.ProductName), /// NU-24
                Total = associatedProductsModel.Count
            };

            return Json(gridModel);
        }

        [HttpPost]
        public ActionResult AssociatedProductUpdate(ProductModel.AssociatedProductModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var associatedProduct = _productService.GetProductById(model.Id);
            if (associatedProduct == null)
                throw new ArgumentException("No associated product found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && associatedProduct.VendorId != _workContext.CurrentVendor.Id)
            {
                return Content("This is not your product");
            }

            associatedProduct.DisplayOrder = model.DisplayOrder;
            _productService.UpdateProduct(associatedProduct);

            return new NullJsonResult();
        }

        [HttpPost]
        public ActionResult AssociatedProductDelete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var product = _productService.GetProductById(id);
            if (product == null)
                throw new ArgumentException("No associated product found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return Content("This is not your product");

            product.ParentGroupedProductId = 0;
            _productService.UpdateProduct(product);

            return new NullJsonResult();
        }

        public ActionResult AssociatedProductAddPopup(int productId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var model = new ProductModel.AddAssociatedProductModel();
            //a vendor should have access only to his products
            model.IsLoggedInAsVendor = _workContext.CurrentVendor != null;
            model.IsLoggedAs = _workContext.CurrentCustomer.IsLoggedAs();	// NU-1

            //categories
            model.AvailableCategories.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            var categories = SelectListHelper.GetCategoryList(_categoryService, _cacheManager, true);
            foreach (var c in categories)
                model.AvailableCategories.Add(c);

            //manufacturers
            model.AvailableManufacturers.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            foreach (var m in _manufacturerService.GetAllManufacturers(showHidden: true))
                model.AvailableManufacturers.Add(new SelectListItem { Text = m.Name, Value = m.Id.ToString() });

            //stores
            model.AvailableStores.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            foreach (var s in _storeService.GetAllStores())
                model.AvailableStores.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString() });

            //vendors
            model.AvailableVendors.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            foreach (var v in _vendorService.GetAllVendors(showHidden: true))
                model.AvailableVendors.Add(new SelectListItem { Text = v.Name, Value = v.Id.ToString() });

            //product types
            model.AvailableProductTypes = ProductType.SimpleProduct.ToSelectList(false).ToList();
            model.AvailableProductTypes.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });

            return View(model);
        }

        [HttpPost]
        public ActionResult AssociatedProductAddPopupList(DataSourceRequest command, ProductModel.AddAssociatedProductModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                model.SearchVendorId = _workContext.CurrentVendor.Id;
            }

            var products = _productService.SearchProducts(
                categoryIds: new List<int> { model.SearchCategoryId },
                manufacturerId: model.SearchManufacturerId,
                storeId: _storeContext.CurrentStore.Id,
                vendorId: model.SearchVendorId,
                productType: model.SearchProductTypeId > 0 ? (ProductType?)model.SearchProductTypeId : null,
                keywords: model.SearchProductName,
                pageIndex: command.Page - 1,
                pageSize: 100,
                showHidden: true
                );
            var gridModel = new DataSourceResult();
            gridModel.Data = products.Select(x =>
            {
                var productModel = x.ToModel();
                //display already associated products
                var parentGroupedProduct = _productService.GetProductById(x.ParentGroupedProductId);
                if (parentGroupedProduct != null)
                {
                    productModel.AssociatedToProductId = x.ParentGroupedProductId;
                    productModel.AssociatedToProductName = parentGroupedProduct.Name;
                }
                return productModel;
            }).OrderBy(p => p.Name); /// NU-24
            gridModel.Total = products.TotalCount;

            return Json(gridModel);
        }

        [HttpPost]
        [FormValueRequired("save")]
        public ActionResult AssociatedProductAddPopup(string btnId, string formId, ProductModel.AddAssociatedProductModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            if (model.SelectedProductIds != null)
            {
                foreach (int id in model.SelectedProductIds)
                {
                    var product = _productService.GetProductById(id);
                    if (product != null)
                    {
                        //a vendor should have access only to his products
                        if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                            continue;

                        product.ParentGroupedProductId = model.ProductId;
                        _productService.UpdateProduct(product);
                    }
                }
            }

            //a vendor should have access only to his products
            model.IsLoggedInAsVendor = _workContext.CurrentVendor != null;
            model.IsLoggedAs = _workContext.CurrentCustomer.IsLoggedAs();	// NU-1
            ViewBag.RefreshPage = true;
            ViewBag.btnId = btnId;
            ViewBag.formId = formId;
            return View(model);
        }

        #endregion

        #region Product pictures

        [ValidateInput(false)]
        public ActionResult ProductPictureAdd(int pictureId, int displayOrder,
            string overrideAltAttribute, string overrideTitleAttribute,
            int productId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            if (pictureId == 0)
                throw new ArgumentException();

            var product = _productService.GetProductById(productId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return RedirectToAction("List");

            var picture = _pictureService.GetPictureById(pictureId);
            if (picture == null)
                throw new ArgumentException("No picture found with the specified id");

            _productService.InsertProductPicture(new ProductPicture
            {
                PictureId = pictureId,
                ProductId = productId,
                DisplayOrder = displayOrder,
            });

            _pictureService.UpdatePicture(picture.Id,
                _pictureService.LoadPictureBinary(picture),
                picture.MimeType,
                picture.SeoFilename,
                overrideAltAttribute,
                overrideTitleAttribute);

            _pictureService.SetSeoFilename(pictureId, _pictureService.GetPictureSeName(product.Name));

            UpdateCopiesFromMaster(productId);	/// NU-10
            return Json(new { Result = true }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult ProductPictureList(DataSourceRequest command, int productId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                var product = _productService.GetProductById(productId);
                if (product != null && product.VendorId != _workContext.CurrentVendor.Id)
                {
                    return Content("This is not your product");
                }
            }

            var productPictures = _productService.GetProductPicturesByProductId(productId);
            var productPicturesModel = productPictures
                .Select(x =>
                {
                    var picture = _pictureService.GetPictureById(x.PictureId);
                    if (picture == null)
                        throw new Exception("Picture cannot be loaded");
                    var m = new ProductModel.ProductPictureModel
                    {
                        Id = x.Id,
                        ProductId = x.ProductId,
                        PictureId = x.PictureId,
                        PictureUrl = _pictureService.GetPictureUrl(picture),
                        OverrideAltAttribute = picture.AltAttribute,
                        OverrideTitleAttribute = picture.TitleAttribute,
                        DisplayOrder = x.DisplayOrder
                    };
                    return m;
                })
                .ToList();

            var gridModel = new DataSourceResult
            {
                Data = productPicturesModel.
                    OrderBy(p => p.PictureUrl), /// NU-54
                Total = productPicturesModel.Count
            };

            return Json(gridModel);
        }

        [HttpPost]
        public ActionResult ProductPictureUpdate(ProductModel.ProductPictureModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var productPicture = _productService.GetProductPictureById(model.Id);
            if (productPicture == null)
                throw new ArgumentException("No product picture found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                var product = _productService.GetProductById(productPicture.ProductId);
                if (product != null && product.VendorId != _workContext.CurrentVendor.Id)
                {
                    return Content("This is not your product");
                }
            }

            productPicture.DisplayOrder = model.DisplayOrder;
            _productService.UpdateProductPicture(productPicture);

            var picture = _pictureService.GetPictureById(productPicture.PictureId);
            if (picture == null)
                throw new ArgumentException("No picture found with the specified id");

            _pictureService.UpdatePicture(picture.Id,
                _pictureService.LoadPictureBinary(picture),
                picture.MimeType,
                picture.SeoFilename,
                model.OverrideAltAttribute,
                model.OverrideTitleAttribute);

            UpdateCopiesFromMaster(model.ProductId);	/// NU-10
            return new NullJsonResult();
        }

        [HttpPost]
        public ActionResult ProductPictureDelete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var productPicture = _productService.GetProductPictureById(id);
            if (productPicture == null)
                throw new ArgumentException("No product picture found with the specified id");

            var productId = productPicture.ProductId;

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                var product = _productService.GetProductById(productId);
                if (product != null && product.VendorId != _workContext.CurrentVendor.Id)
                {
                    return Content("This is not your product");
                }
            }
            var pictureId = productPicture.PictureId;
            _productService.DeleteProductPicture(productPicture);

            var picture = _pictureService.GetPictureById(pictureId);
            if (picture == null)
                throw new ArgumentException("No picture found with the specified id");
            _pictureService.DeletePicture(picture);

            UpdateCopiesFromMaster(productId);	/// NU-10
            return new NullJsonResult();
        }

        #endregion

        #region Product specification attributes

        [ValidateInput(false)]
        public ActionResult ProductSpecificationAttributeAdd(int attributeTypeId, int specificationAttributeOptionId,
            string customValue, bool allowFiltering, bool showOnProductPage,
            int displayOrder, int productId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                var product = _productService.GetProductById(productId);
                if (product != null && product.VendorId != _workContext.CurrentVendor.Id)
                {
                    return RedirectToAction("List");
                }
            }

            //we allow filtering only for "Option" attribute type
            if (attributeTypeId != (int)SpecificationAttributeType.Option)
            {
                allowFiltering = false;
            }
            //we don't allow CustomValue for "Option" attribute type
            if (attributeTypeId == (int)SpecificationAttributeType.Option)
            {
                customValue = null;
            }

            var psa = new ProductSpecificationAttribute
            {
                AttributeTypeId = attributeTypeId,
                SpecificationAttributeOptionId = specificationAttributeOptionId,
                ProductId = productId,
                CustomValue = customValue,
                AllowFiltering = allowFiltering,
                ShowOnProductPage = showOnProductPage,
                DisplayOrder = displayOrder,
            };
            _specificationAttributeService.InsertProductSpecificationAttribute(psa);

            UpdateCopiesFromMaster(productId);	/// NU-10
            return Json(new { Result = true }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult ProductSpecAttrList(DataSourceRequest command, int productId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                var product = _productService.GetProductById(productId);
                if (product != null && product.VendorId != _workContext.CurrentVendor.Id)
                {
                    return Content("This is not your product");
                }
            }

            var productrSpecs = _specificationAttributeService.GetProductSpecificationAttributes(productId);

            var productrSpecsModel = productrSpecs
                .Select(x =>
                {
                    var psaModel = new ProductSpecificationAttributeModel
                    {
                        Id = x.Id,
                        AttributeTypeName = x.AttributeType.GetLocalizedEnum(_localizationService, _workContext),
                        AttributeName = x.SpecificationAttributeOption.SpecificationAttribute.Name,
                        AllowFiltering = x.AllowFiltering,
                        ShowOnProductPage = x.ShowOnProductPage,
                        DisplayOrder = x.DisplayOrder
                    };
                    switch (x.AttributeType)
                    {
                        case SpecificationAttributeType.Option:
                            psaModel.ValueRaw = HttpUtility.HtmlEncode(x.SpecificationAttributeOption.Name);
                            break;
                        case SpecificationAttributeType.CustomText:
                            psaModel.ValueRaw = HttpUtility.HtmlEncode(x.CustomValue);
                            break;
                        case SpecificationAttributeType.CustomHtmlText:
                            //do not encode?
                            //psaModel.ValueRaw = x.CustomValue;
                            psaModel.ValueRaw = HttpUtility.HtmlEncode(x.CustomValue);
                            break;
                        case SpecificationAttributeType.Hyperlink:
                            psaModel.ValueRaw = x.CustomValue;
                            break;
                        default:
                            break;
                    }
                    return psaModel;
                })
                .ToList();

            var gridModel = new DataSourceResult
            {
                Data = productrSpecsModel.
                    OrderBy(s => s.AttributeName), /// NU-24
                Total = productrSpecsModel.Count
            };


            return Json(gridModel);
        }

        [HttpPost]
        public ActionResult ProductSpecAttrUpdate(ProductSpecificationAttributeModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var psa = _specificationAttributeService.GetProductSpecificationAttributeById(model.Id);
            if (psa == null)
                return Content("No product specification attribute found with the specified id");

            var productId = psa.Product.Id;

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                var product = _productService.GetProductById(productId);
                if (product != null && product.VendorId != _workContext.CurrentVendor.Id)
                {
                    return Content("This is not your product");
                }
            }

            //we do not allow editing these fields anymore (when we have distinct attribute types)
            //psa.CustomValue = model.CustomValue;
            //psa.AllowFiltering = model.AllowFiltering;
            psa.ShowOnProductPage = model.ShowOnProductPage;
            psa.DisplayOrder = model.DisplayOrder;
            _specificationAttributeService.UpdateProductSpecificationAttribute(psa);

            UpdateCopiesFromMaster(productId);	/// NU-10
            return new NullJsonResult();
        }

        [HttpPost]
        public ActionResult ProductSpecAttrDelete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var psa = _specificationAttributeService.GetProductSpecificationAttributeById(id);
            if (psa == null)
                throw new ArgumentException("No specification attribute found with the specified id");

            var productId = psa.ProductId;

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                var product = _productService.GetProductById(productId);
                if (product != null && product.VendorId != _workContext.CurrentVendor.Id)
                {
                    return Content("This is not your product");
                }
            }

            _specificationAttributeService.DeleteProductSpecificationAttribute(psa);

            UpdateCopiesFromMaster(productId);	/// NU-10
            return new NullJsonResult();
        }

        #endregion

        #region Product tags

        public ActionResult ProductTags()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProductTags))
                return AccessDeniedView();

            return View();
        }

        [HttpPost]
        public ActionResult ProductTags(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProductTags))
                return AccessDeniedView();

            var tags = _productTagService.GetAllProductTags(storeId: _storeContext.CurrentStore.Id) // NU-42
                                                                                                    //order by product count
                .OrderByDescending(x => _productTagService.GetProductCount(x.Id, _storeContext.CurrentStore.Id))    // NU-42
                .Select(x => new ProductTagModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    ProductCount = _productTagService.GetProductCount(x.Id, _storeContext.CurrentStore.Id)  // NU-42
                })
                .ToList();

            var gridModel = new DataSourceResult
            {
                Data = tags.PagedForCommand(command).
                    OrderBy(t => t.Name), /// NU-24
                Total = tags.Count
            };

            return Json(gridModel);
        }

        [HttpPost]
        public ActionResult ProductTagDelete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProductTags))
                return AccessDeniedView();

            var tag = _productTagService.GetProductTagById(id);
            if (tag == null)
                throw new ArgumentException("No product tag found with the specified id");
            _productTagService.DeleteProductTag(tag);

            return new NullJsonResult();
        }

        //edit
        public ActionResult EditProductTag(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProductTags))
                return AccessDeniedView();

            var productTag = _productTagService.GetProductTagById(id);
            if (productTag == null)
                //No product tag found with the specified id
                return RedirectToAction("List");

            var model = new ProductTagModel
            {
                Id = productTag.Id,
                Name = productTag.Name,
                ProductCount = _productTagService.GetProductCount(productTag.Id, 0)
            };
            //locales
            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = productTag.GetLocalized(x => x.Name, languageId, false, false);
            });

            return View(model);
        }

        [HttpPost]
        public ActionResult EditProductTag(string btnId, string formId, ProductTagModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProductTags))
                return AccessDeniedView();

            var productTag = _productTagService.GetProductTagById(model.Id);
            if (productTag == null)
                //No product tag found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                productTag.Name = model.Name;
                _productTagService.UpdateProductTag(productTag);
                //locales
                UpdateLocales(productTag, model);

                ViewBag.RefreshPage = true;
                ViewBag.btnId = btnId;
                ViewBag.formId = formId;
                return View(model);
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }

        #endregion

        #region Purchased with order

        [HttpPost]
        public ActionResult PurchasedWithOrders(DataSourceRequest command, int productId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var product = _productService.GetProductById(productId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return Content("This is not your product");

            var orders = _orderService.SearchOrders(
                productId: productId,
                pageIndex: command.Page - 1,
                pageSize: command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = orders.Select(x =>
                {
                    var store = _storeService.GetStoreById(x.StoreId);
                    return new OrderModel
                    {
                        Id = x.Id,
                        StoreName = store != null ? store.Name : "Unknown",
                        OrderStatus = x.OrderStatus.GetLocalizedEnum(_localizationService, _workContext),
                        PaymentStatus = x.PaymentStatus.GetLocalizedEnum(_localizationService, _workContext),
                        ShippingStatus = x.ShippingStatus.GetLocalizedEnum(_localizationService, _workContext),
                        CustomerEmail = x.BillingAddress.Email,
                        CreatedOn = _dateTimeHelper.ConvertToUserTime(x.CreatedOnUtc, DateTimeKind.Utc)
                    };
                }).OrderBy(s => s.StoreName), /// NU-24
                Total = orders.TotalCount
            };

            return Json(gridModel);
        }


        [HttpPost]
        public ActionResult LocalizedProducts(DataSourceRequest command, int productId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var productIdList = _productService.GetProductChildren(_productService.GetProductById(productId));
            if (productIdList == null)
                throw new ArgumentException("No products found with the specified master product id");



            var localizedProductList = new List<ProductModel>();

            foreach (var id in productIdList)
            {
                var product = _productService.GetProductById(id);
                var storeMappings = _storeMappingService.GetStoreMappings<Product>(product);
                foreach (var mapping in storeMappings)
                {

                    ProductModel model = new ProductModel
                    {
                        Id = product.Id,
                        Name = product.Name,
                        StoreName = mapping.Store.Name,
                        Published = product.Published
                    };
                    localizedProductList.Add(model);
                }

            }

            //a vendor should have access only to his products
            //if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
            //    return Content("This is not your product");

            var products = localizedProductList;
            var gridModel = new DataSourceResult
            {
                Data = localizedProductList.Select(x =>
                {
                    //var store = _storeService.GetStoreById(x.StoreId);
                    return new ProductModel
                    {
                        Id = x.Id,
                        Name = x.Name,
                        StoreName = x.StoreName,
                        Published = x.Published
                    };
                }).OrderBy(s => s.StoreName), /// NU-24
                Total = localizedProductList.Count
            };

            return Json(gridModel);
        }

        #endregion

        #region Export / Import

        #region NU-10
        [HttpPost, ActionName("MasterList")]
        [FormValueRequired("download-catalog-pdf")]
        public ActionResult DownloadCatalogAsPdfAdmin(ProductListModel model)
        {
            return DownloadCatalogAsPdf(model);
        }
        #endregion

        [HttpPost, ActionName("List")]
        [FormValueRequired("download-catalog-pdf")]
        public ActionResult DownloadCatalogAsPdf(ProductListModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                model.SearchVendorId = _workContext.CurrentVendor.Id;
            }

            var categoryIds = new List<int> { model.SearchCategoryId };
            //include subcategories
            if (model.SearchIncludeSubCategories && model.SearchCategoryId > 0)
                categoryIds.AddRange(GetChildCategoryIds(model.SearchCategoryId));

            //0 - all (according to "ShowHidden" parameter)
            //1 - published only
            //2 - unpublished only
            bool? overridePublished = null;
            if (model.SearchPublishedId == 1)
                overridePublished = true;
            else if (model.SearchPublishedId == 2)
                overridePublished = false;

            var products = _productService.SearchProducts(
                categoryIds: categoryIds,
                manufacturerId: model.SearchManufacturerId,
                storeId: model.SearchStoreId,
                vendorId: model.SearchVendorId,
                warehouseId: model.SearchWarehouseId,
                productType: model.SearchProductTypeId > 0 ? (ProductType?)model.SearchProductTypeId : null,
                keywords: model.SearchProductName,
                showHidden: true,
                overridePublished: overridePublished
            );

            try
            {
                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    _pdfService.PrintProductsToPdf(stream, products);
                    bytes = stream.ToArray();
                }
                return File(bytes, MimeTypes.ApplicationPdf, "pdfcatalog.pdf");
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("List");
            }
        }

        #region NU-10
        [HttpPost, ActionName("MasterList")]
        [FormValueRequired("exportxml-all")]
        public ActionResult ExportXmlAllAdmin(ProductListModel model)
        {
            return ExportXmlAll(model);
        }
        #endregion

        [HttpPost, ActionName("List")]
        [FormValueRequired("exportxml-all")]
        public ActionResult ExportXmlAll(ProductListModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                model.SearchVendorId = _workContext.CurrentVendor.Id;
            }

            var categoryIds = new List<int> { model.SearchCategoryId };
            //include subcategories
            if (model.SearchIncludeSubCategories && model.SearchCategoryId > 0)
                categoryIds.AddRange(GetChildCategoryIds(model.SearchCategoryId));

            //0 - all (according to "ShowHidden" parameter)
            //1 - published only
            //2 - unpublished only
            bool? overridePublished = null;
            if (model.SearchPublishedId == 1)
                overridePublished = true;
            else if (model.SearchPublishedId == 2)
                overridePublished = false;

            var products = _productService.SearchProducts(
                categoryIds: categoryIds,
                manufacturerId: model.SearchManufacturerId,
                storeId: model.SearchStoreId,
                vendorId: model.SearchVendorId,
                warehouseId: model.SearchWarehouseId,
                productType: model.SearchProductTypeId > 0 ? (ProductType?)model.SearchProductTypeId : null,
                keywords: model.SearchProductName,
                showHidden: true,
                overridePublished: overridePublished
            );

            try
            {
                var xml = _exportManager.ExportProductsToXml(products);
                return new XmlDownloadResult(xml, "products.xml");
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("List");
            }
        }

        [HttpPost]
        public ActionResult ExportXmlSelected(string selectedIds)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var products = new List<Product>();
            if (selectedIds != null)
            {
                var ids = selectedIds
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => Convert.ToInt32(x))
                    .ToArray();
                products.AddRange(_productService.GetProductsByIds(ids));
            }
            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                products = products.Where(p => p.VendorId == _workContext.CurrentVendor.Id).ToList();
            }

            var xml = _exportManager.ExportProductsToXml(products);
            return new XmlDownloadResult(xml, "products.xml");
        }

        #region NU-10
        [HttpPost, ActionName("MasterList")]
        [FormValueRequired("exportexcel-all")]
        public ActionResult ExportExcelAllAdmin(ProductListModel model)
        {
            return ExportExcelAll(model);
        }
        #endregion

        [HttpPost, ActionName("List")]
        [FormValueRequired("exportexcel-all")]
        public ActionResult ExportExcelAll(ProductListModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                model.SearchVendorId = _workContext.CurrentVendor.Id;
            }

            var categoryIds = new List<int> { model.SearchCategoryId };
            //include subcategories
            if (model.SearchIncludeSubCategories && model.SearchCategoryId > 0)
                categoryIds.AddRange(GetChildCategoryIds(model.SearchCategoryId));

            //0 - all (according to "ShowHidden" parameter)
            //1 - published only
            //2 - unpublished only
            bool? overridePublished = null;
            if (model.SearchPublishedId == 1)
                overridePublished = true;
            else if (model.SearchPublishedId == 2)
                overridePublished = false;

            var products = _productService.SearchProducts(
                categoryIds: categoryIds,
                manufacturerId: model.SearchManufacturerId,
                storeId: model.SearchStoreId,
                vendorId: model.SearchVendorId,
                warehouseId: model.SearchWarehouseId,
                productType: model.SearchProductTypeId > 0 ? (ProductType?)model.SearchProductTypeId : null,
                keywords: model.SearchProductName,
                showHidden: true,
                overridePublished: overridePublished
            );
            try
            {
                var bytes = _exportManager.ExportProductsToXlsx(products);

                return File(bytes, MimeTypes.TextXlsx, "products.xlsx");
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("List");
            }
        }

        [HttpPost]
        public ActionResult ExportExcelSelected(string selectedIds)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var products = new List<Product>();
            if (selectedIds != null)
            {
                var ids = selectedIds
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => Convert.ToInt32(x))
                    .ToArray();
                products.AddRange(_productService.GetProductsByIds(ids));
            }
            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                products = products.Where(p => p.VendorId == _workContext.CurrentVendor.Id).ToList();
            }

            var bytes = _exportManager.ExportProductsToXlsx(products);

            return File(bytes, MimeTypes.TextXlsx, "products.xlsx");
        }

        [HttpPost]
        public ActionResult ImportExcel()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //a vendor cannot import products
            if (_workContext.CurrentVendor != null)
                return AccessDeniedView();

            try
            {
                var file = Request.Files["importexcelfile"];
                if (file != null && file.ContentLength > 0)
                {
                    _importManager.ImportProductsFromXlsx(file.InputStream);
                }
                else
                {
                    ErrorNotification(_localizationService.GetResource("Admin.Common.UploadFile"));
                    return RedirectToAction("List");
                }
                SuccessNotification(_localizationService.GetResource("Admin.Catalog.Products.Imported"));
                return RedirectToAction("List");
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("List");
            }

        }

        #endregion

        #region Low stock reports

        public ActionResult LowStockReport()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            return View();
        }
        [HttpPost]
        public ActionResult LowStockReportList(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            int vendorId = 0;
            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
                vendorId = _workContext.CurrentVendor.Id;

            #region NU-35
            IList<Product> products = _productService.GetLowStockProducts(vendorId: vendorId, storeId: _storeContext.CurrentStore.Id);
            IList<ProductAttributeCombination> combinations = _productService.GetLowStockProductCombinations(vendorId: vendorId, storeId: _storeContext.CurrentStore.Id);
            #endregion

            var models = new List<LowStockProductModel>();
            //products
            foreach (var product in products)
            {
                var lowStockModel = new LowStockProductModel
                {
                    Id = product.Id,
                    Name = product.Name,
                    ManageInventoryMethod = product.ManageInventoryMethod.GetLocalizedEnum(_localizationService, _workContext.WorkingLanguage.Id),
                    StockQuantity = product.GetTotalStockQuantity(),
                    Published = product.Published
                };
                models.Add(lowStockModel);
            }
            //combinations
            foreach (var combination in combinations)
            {
                var product = combination.Product;
                var lowStockModel = new LowStockProductModel
                {
                    Id = product.Id,
                    Name = product.Name,
                    Attributes = _productAttributeFormatter.FormatAttributes(product, combination.AttributesXml, _workContext.CurrentCustomer, "<br />", true, true, true, false),
                    ManageInventoryMethod = product.ManageInventoryMethod.GetLocalizedEnum(_localizationService, _workContext.WorkingLanguage.Id),
                    StockQuantity = combination.StockQuantity,
                    Published = product.Published
                };
                models.Add(lowStockModel);
            }
            var gridModel = new DataSourceResult
            {
                Data = models.PagedForCommand(command).
                    OrderBy(p => p.StockQuantity), /// NU-47
                Total = models.Count
            };

            return Json(gridModel);
        }









        [HttpPost]
        public ActionResult ProductPaidGlsByProductList(int productId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            int vendorId = 0;
            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
                vendorId = _workContext.CurrentVendor.Id;

            var product = _productService.GetProductById(productId);


            IList<ProductGlCode> productGls = _glCodeService.GetProductGlCodes(product, GLCodeStatusType.Paid);

            // ((List<ProductRelation>)productRelations).AddRange(_productService.GetProductRelationsByProduct(productId, 1));

            // ((List<ProductRelation>)productRelations).AddRange(_productService.GetProductRelationsByRelatedProduct(productId, 2));

            //  ((List<ProductRelation>)productRelations).AddRange(_productService.GetProductRelationsByRelatedProduct(productId, 1));//


            var models = new List<GLCodeModel>();
            //products
            foreach (var productGl in productGls)
            {

                //    var prod = _productService.GetProductById(productRelation.ProductId);
                // var relatedProd = _productService.GetProductById(productRelation.RelatedProductId);


                var glCodeModel = new Nop.Admin.Models.Tax.GLCodeModel
                {
                    GlCodeId = productGl.GlCodeId,
                    Id = productGl.Id,
                    GlCodeName = productGl.GlCode.GlCodeName,
                    Amount = productGl.Amount,
                    Percentage = productGl.Percentage,
                    IsDelivered = productGl.GlCode.IsDelivered,
                    IsPaid = productGl.GlCode.IsPaid,
                    TaxCategoryId = productGl.TaxCategoryId
                };
                models.Add(glCodeModel);
            }
            //combinations

            var gridModel = new DataSourceResult
            {
                Data = models.OrderBy(p => p.GlCodeName), /// NU-47
                Total = models.Count
            };

            return Json(gridModel);
        }


        [HttpPost]
        public ActionResult ProductDeliveredGlsByProductList(int productId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            int vendorId = 0;
            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
                vendorId = _workContext.CurrentVendor.Id;

            var product = _productService.GetProductById(productId);

            var data = new ProductModel();

            IList<ProductGlCode> productGls = _glCodeService.GetProductGlCodes(product, GLCodeStatusType.Deferred);

            // ((List<ProductRelation>)productRelations).AddRange(_productService.GetProductRelationsByProduct(productId, 1));

            // ((List<ProductRelation>)productRelations).AddRange(_productService.GetProductRelationsByRelatedProduct(productId, 2));

            //  ((List<ProductRelation>)productRelations).AddRange(_productService.GetProductRelationsByRelatedProduct(productId, 1));//


            var models = new List<GLCodeModel>();
            //products
            foreach (var productGl in productGls)
            {

                //    var prod = _productService.GetProductById(productRelation.ProductId);
                // var relatedProd = _productService.GetProductById(productRelation.RelatedProductId);


                var glCodeModel = new Nop.Admin.Models.Tax.GLCodeModel
                {
                    GlCodeId = productGl.GlCodeId,
                    Id = productGl.Id,
                    GlCodeName = productGl.GlCode.GlCodeName,
                    Amount = productGl.Amount,
                    Percentage = productGl.Percentage,
                    IsDelivered = productGl.GlCode.IsDelivered,
                    IsPaid = productGl.GlCode.IsPaid,
                    TaxCategoryId = productGl.TaxCategoryId
                };
                models.Add(glCodeModel);
            }
            //combinations

            var gridModel = new DataSourceResult
            {
                Data = models.OrderBy(p => p.GlCodeName), /// NU-47
                Total = models.Count
            };

            return Json(gridModel);
        }














        public ActionResult ProductRelation()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            return View();
        }



        [HttpPost]
        public ActionResult ProductRelationByProductList(int productId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            int vendorId = 0;
            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
                vendorId = _workContext.CurrentVendor.Id;

            IList<ProductRelation> productRelations = _productService.GetProductRelationsByProduct(productId, 2);

            // ((List<ProductRelation>)productRelations).AddRange(_productService.GetProductRelationsByProduct(productId, 1));

            // ((List<ProductRelation>)productRelations).AddRange(_productService.GetProductRelationsByRelatedProduct(productId, 2));

            //  ((List<ProductRelation>)productRelations).AddRange(_productService.GetProductRelationsByRelatedProduct(productId, 1));//


            var models = new List<ProductRelationModel>();
            //products
            foreach (var productRelation in productRelations)
            {

                var prod = _productService.GetProductById(productRelation.ProductId);
                var relatedProd = _productService.GetProductById(productRelation.RelatedProductId);


                var prodRelationModel = new ProductRelationModel
                {
                    Id = productRelation.Id,
                    ProductName = prod.Name,
                    RelatedProductName = relatedProd.Name,
                    RelatedProductId = relatedProd.Id,
                    RelationTypeId = productRelation.RelationTypeId,
                    IsActive = productRelation.IsActive
                };
                models.Add(prodRelationModel);
            }
            //combinations

            var gridModel = new DataSourceResult
            {
                Data = models.OrderBy(p => p.ProductName), /// NU-47
                Total = models.Count
            };

            return Json(gridModel);
        }


        [HttpPost]
        public ActionResult ProductRelationList(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            int vendorId = 0;
            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
                vendorId = _workContext.CurrentVendor.Id;

            IList<ProductRelation> productRelations = _productService.GetProductRelationsByStore(storeId: _storeContext.CurrentStore.Id);

            var models = new List<ProductRelationModel>();
            //products
            foreach (var productRelation in productRelations)
            {

                var prod = _productService.GetProductById(productRelation.ProductId);
                var relatedProd = _productService.GetProductById(productRelation.RelatedProductId);


                var prodRelationModel = new ProductRelationModel
                {
                    Id = productRelation.Id,
                    ProductName = prod.Name,
                    RelatedProductName = relatedProd.Name,
                    RelationTypeId = productRelation.RelationTypeId,
                    IsActive = productRelation.IsActive
                };
                models.Add(prodRelationModel);
            }
            //combinations

            var gridModel = new DataSourceResult
            {
                Data = models.PagedForCommand(command).
                    OrderBy(p => p.ProductName), /// NU-47
                Total = models.Count
            };

            return Json(gridModel);
        }

        #endregion

        #region Bulk editing

        public ActionResult BulkEdit()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var model = new BulkEditListModel();
            //categories
            model.AvailableCategories.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            var categories = SelectListHelper.GetCategoryList(_categoryService, _cacheManager, true);
            foreach (var c in categories)
                model.AvailableCategories.Add(c);

            //manufacturers
            model.AvailableManufacturers.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            foreach (var m in _manufacturerService.GetAllManufacturers(showHidden: true).
                OrderBy(m => m.Name)) /// NU-24
                model.AvailableManufacturers.Add(new SelectListItem { Text = m.Name, Value = m.Id.ToString() });

            //product types
            model.AvailableProductTypes = ProductType.SimpleProduct.ToSelectList(false).ToList();
            model.AvailableProductTypes.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });

            return View(model);
        }

        [HttpPost]
        public ActionResult BulkEditSelect(DataSourceRequest command, BulkEditListModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            int vendorId = 0;
            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
                vendorId = _workContext.CurrentVendor.Id;

            var products = _productService.SearchProducts(categoryIds: new List<int> { model.SearchCategoryId },
                manufacturerId: model.SearchManufacturerId,
                vendorId: vendorId,
                productType: model.SearchProductTypeId > 0 ? (ProductType?)model.SearchProductTypeId : null,
                keywords: model.SearchProductName,
                pageIndex: command.Page - 1,
                pageSize: command.PageSize,
                showHidden: true,
                storeId: _storeContext.CurrentStore.Id,		 /// NU-42
                overrideHideMasterProducts: true);	/// NU-10
            var gridModel = new DataSourceResult();
            gridModel.Data = products.Select(x =>
            {
                var productModel = new BulkEditProductModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    Sku = x.Sku,
                    OldPrice = x.OldPrice,
                    Price = x.Price,
                    ManageInventoryMethod = x.ManageInventoryMethod.GetLocalizedEnum(_localizationService, _workContext.WorkingLanguage.Id),
                    StockQuantity = x.StockQuantity,
                    Published = x.Published
                };

                if (x.ManageInventoryMethod == ManageInventoryMethod.ManageStock && x.UseMultipleWarehouses)
                {
                    //multi-warehouse supported
                    //TODO localize
                    productModel.ManageInventoryMethod += " (multi-warehouse)";
                }

                return productModel;
            }).OrderBy(p => p.Name); /// NU-24
            gridModel.Total = products.TotalCount;

            return Json(gridModel);
        }

        [HttpPost]
        public ActionResult BulkEditUpdate(IEnumerable<BulkEditProductModel> products)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            if (products != null)
            {
                foreach (var pModel in products)
                {
                    //update
                    var product = _productService.GetProductById(pModel.Id);
                    if (product != null)
                    {
                        //a vendor should have access only to his products
                        if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                            continue;

                        var prevStockQuantity = product.GetTotalStockQuantity();

                        product.Name = pModel.Name;
                        product.Sku = pModel.Sku;
                        product.Price = pModel.Price;
                        product.OldPrice = pModel.OldPrice;
                        product.StockQuantity = pModel.StockQuantity;
                        product.Published = pModel.Published;
                        product.UpdatedOnUtc = DateTime.UtcNow;
                        _productService.UpdateProduct(product);

                        UpdateCopiesFromMaster(product.Id);	/// NU-10
                        //back in stock notifications
                        if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStock &&
                            product.BackorderMode == BackorderMode.NoBackorders &&
                            product.AllowBackInStockSubscriptions &&
                            product.GetTotalStockQuantity() > 0 &&
                            prevStockQuantity <= 0 &&
                            product.Published &&
                            !product.Deleted)
                        {
                            _backInStockSubscriptionService.SendNotificationsToSubscribers(product);
                        }
                    }
                }
            }

            return new NullJsonResult();
        }

        [HttpPost]
        public ActionResult BulkEditDelete(IEnumerable<BulkEditProductModel> products)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            if (products != null)
            {
                foreach (var pModel in products)
                {
                    //delete
                    var product = _productService.GetProductById(pModel.Id);
                    if (product != null)
                    {
                        //a vendor should have access only to his products
                        if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                            continue;

                        UpdateCopiesFromMaster(product.Id);	/// NU-10
                        _productService.DeleteProduct(product);
                    }
                }
            }
            return new NullJsonResult();
        }

        #endregion

        #region Tier prices

        [HttpPost]
        public ActionResult TierPriceList(DataSourceRequest command, int productId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var product = _productService.GetProductById(productId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return Content("This is not your product");

            var tierPricesModel = product.TierPrices
                .OrderBy(x => x.StoreId)
                .ThenBy(x => x.Quantity)
                .ThenBy(x => x.CustomerRoleId)
                .Select(x =>
                {
                    string storeName;
                    if (x.StoreId > 0)
                    {
                        var store = _storeService.GetStoreById(x.StoreId);
                        storeName = store != null ? store.Name : "Deleted";
                    }
                    else
                    {
                        storeName = _localizationService.GetResource("Admin.Catalog.Products.TierPrices.Fields.Store.All");
                    }
                    return new ProductModel.TierPriceModel
                    {
                        Id = x.Id,
                        StoreId = x.StoreId,
                        Store = storeName,
                        CustomerRole = x.CustomerRoleId.HasValue ? _customerService.GetCustomerRoleById(x.CustomerRoleId.Value).Name : _localizationService.GetResource("Admin.Catalog.Products.TierPrices.Fields.CustomerRole.All"),
                        ProductId = x.ProductId,
                        CustomerRoleId = x.CustomerRoleId.HasValue ? x.CustomerRoleId.Value : 0,
                        Quantity = x.Quantity,
                        Price = x.Price
                    };
                })
                .ToList();

            var gridModel = new DataSourceResult
            {
                Data = tierPricesModel,
                Total = tierPricesModel.Count
            };

            return Json(gridModel);
        }

        [HttpPost]
        public ActionResult TierPriceInsert(ProductModel.TierPriceModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var product = _productService.GetProductById(model.ProductId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return Content("This is not your product");

            var tierPrice = new TierPrice
            {
                ProductId = model.ProductId,
                StoreId = model.StoreId,
                CustomerRoleId = model.CustomerRoleId > 0 ? model.CustomerRoleId : (int?)null,
                Quantity = model.Quantity,
                Price = model.Price
            };
            _productService.InsertTierPrice(tierPrice);

            //update "HasTierPrices" property
            _productService.UpdateHasTierPricesProperty(product);

            UpdateCopiesFromMaster(model.ProductId);	/// NU-10
            return new NullJsonResult();
        }

        [HttpPost]
        public ActionResult TierPriceUpdate(ProductModel.TierPriceModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var tierPrice = _productService.GetTierPriceById(model.Id);
            if (tierPrice == null)
                throw new ArgumentException("No tier price found with the specified id");

            var product = _productService.GetProductById(tierPrice.ProductId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return Content("This is not your product");

            tierPrice.StoreId = model.StoreId;
            tierPrice.CustomerRoleId = model.CustomerRoleId > 0 ? model.CustomerRoleId : (int?)null;
            tierPrice.Quantity = model.Quantity;
            tierPrice.Price = model.Price;
            _productService.UpdateTierPrice(tierPrice);

            UpdateCopiesFromMaster(model.ProductId);	/// NU-10
            return new NullJsonResult();
        }

        [HttpPost]
        public ActionResult TierPriceDelete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var tierPrice = _productService.GetTierPriceById(id);
            if (tierPrice == null)
                throw new ArgumentException("No tier price found with the specified id");

            var productId = tierPrice.ProductId;
            var product = _productService.GetProductById(productId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return Content("This is not your product");

            _productService.DeleteTierPrice(tierPrice);

            //update "HasTierPrices" property
            _productService.UpdateHasTierPricesProperty(product);

            UpdateCopiesFromMaster(productId);	/// NU-10
            return new NullJsonResult();
        }

        #endregion

        #region Product attributes

        [HttpPost]
        public ActionResult ProductAttributeMappingList(DataSourceRequest command, int productId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var product = _productService.GetProductById(productId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return Content("This is not your product");

            var attributes = _productAttributeService.GetProductAttributeMappingsByProductId(productId);
            var attributesModel = attributes
                .Select(x =>
                {
                    var attributeModel = new ProductModel.ProductAttributeMappingModel
                    {
                        Id = x.Id,
                        ProductId = x.ProductId,
                        ProductAttribute = _productAttributeService.GetProductAttributeById(x.ProductAttributeId).Name,
                        ProductAttributeId = x.ProductAttributeId,
                        TextPrompt = x.TextPrompt,
                        IsRequired = x.IsRequired,
                        AttributeControlType = x.AttributeControlType.GetLocalizedEnum(_localizationService, _workContext),
                        AttributeControlTypeId = x.AttributeControlTypeId,
                        DisplayOrder = x.DisplayOrder
                    };


                    if (x.ShouldHaveValues())
                    {
                        attributeModel.ShouldHaveValues = true;
                        attributeModel.TotalValues = x.ProductAttributeValues.Count;
                    }

                    if (x.ValidationRulesAllowed())
                    {
                        var validationRules = new StringBuilder(string.Empty);
                        attributeModel.ValidationRulesAllowed = true;
                        if (x.ValidationMinLength != null)
                            validationRules.AppendFormat("{0}: {1}<br />",
                                _localizationService.GetResource("Admin.Catalog.Products.ProductAttributes.Attributes.ValidationRules.MinLength"),
                                x.ValidationMinLength);
                        if (x.ValidationMaxLength != null)
                            validationRules.AppendFormat("{0}: {1}<br />",
                                _localizationService.GetResource("Admin.Catalog.Products.ProductAttributes.Attributes.ValidationRules.MaxLength"),
                                x.ValidationMaxLength);
                        if (!string.IsNullOrEmpty(x.ValidationFileAllowedExtensions))
                            validationRules.AppendFormat("{0}: {1}<br />",
                                _localizationService.GetResource("Admin.Catalog.Products.ProductAttributes.Attributes.ValidationRules.FileAllowedExtensions"),
                                HttpUtility.HtmlEncode(x.ValidationFileAllowedExtensions));
                        if (x.ValidationFileMaximumSize != null)
                            validationRules.AppendFormat("{0}: {1}<br />",
                                _localizationService.GetResource("Admin.Catalog.Products.ProductAttributes.Attributes.ValidationRules.FileMaximumSize"),
                                x.ValidationFileMaximumSize);
                        if (!string.IsNullOrEmpty(x.DefaultValue))
                            validationRules.AppendFormat("{0}: {1}<br />",
                                _localizationService.GetResource("Admin.Catalog.Products.ProductAttributes.Attributes.ValidationRules.DefaultValue"),
                                HttpUtility.HtmlEncode(x.DefaultValue));
                        attributeModel.ValidationRulesString = validationRules.ToString();
                    }


                    //currenty any attribute can have condition. why not?
                    attributeModel.ConditionAllowed = true;
                    var conditionAttribute = _productAttributeParser.ParseProductAttributeMappings(x.ConditionAttributeXml).FirstOrDefault();
                    var conditionValue = _productAttributeParser.ParseProductAttributeValues(x.ConditionAttributeXml).FirstOrDefault();
                    if (conditionAttribute != null && conditionValue != null)
                        attributeModel.ConditionString = string.Format("{0}: {1}",
                            HttpUtility.HtmlEncode(conditionAttribute.ProductAttribute.Name),
                            HttpUtility.HtmlEncode(conditionValue.Name));
                    else
                        attributeModel.ConditionString = string.Empty;
                    return attributeModel;
                })
                .ToList();

            var gridModel = new DataSourceResult
            {
                Data = attributesModel,
                Total = attributesModel.Count
            };

            return Json(gridModel);
        }

        [HttpPost]
        public ActionResult ProductAttributeMappingInsert(ProductModel.ProductAttributeMappingModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var product = _productService.GetProductById(model.ProductId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
            {
                return Content("This is not your product");
            }


            /*NU-90
            //ensure this attribute is not mapped yet
          //  if (_productAttributeService.GetProductAttributeMappingsByProductId(product.Id).Any(x => x.ProductAttributeId == model.ProductAttributeId))
            //{
              //  return Json(new DataSourceResult { Errors = _localizationService.GetResource("Admin.Catalog.Products.ProductAttributes.Attributes.AlreadyExists") });
            //}*/

            //insert mapping
            var productAttributeMapping = new ProductAttributeMapping
            {
                ProductId = model.ProductId,
                ProductAttributeId = model.ProductAttributeId,
                TextPrompt = model.TextPrompt,
                IsRequired = model.IsRequired,
                AttributeControlTypeId = model.AttributeControlTypeId,
                DisplayOrder = model.DisplayOrder
            };
            _productAttributeService.InsertProductAttributeMapping(productAttributeMapping);

            UpdateCopiesFromMaster(product.Id);	/// NU-10
            //predefined values
            var predefinedValues = _productAttributeService.GetPredefinedProductAttributeValues(model.ProductAttributeId);
            foreach (var predefinedValue in predefinedValues)
            {
                var pav = new ProductAttributeValue
                {
                    ProductAttributeMappingId = productAttributeMapping.Id,
                    AttributeValueType = AttributeValueType.Simple,
                    Name = predefinedValue.Name,
                    PriceAdjustment = predefinedValue.PriceAdjustment,
                    WeightAdjustment = predefinedValue.WeightAdjustment,
                    Cost = predefinedValue.Cost,
                    IsPreSelected = predefinedValue.IsPreSelected,
                    DisplayOrder = predefinedValue.DisplayOrder
                };
                _productAttributeService.InsertProductAttributeValue(pav);
                //locales
                var languages = _languageService.GetAllLanguages(true);
                //localization
                foreach (var lang in languages)
                {
                    var name = predefinedValue.GetLocalized(x => x.Name, lang.Id, false, false);
                    if (!String.IsNullOrEmpty(name))
                        _localizedEntityService.SaveLocalizedValue(pav, x => x.Name, name, lang.Id);
                }
            }

            return new NullJsonResult();
        }

        [HttpPost]
        public ActionResult ProductAttributeMappingUpdate(ProductModel.ProductAttributeMappingModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var productAttributeMapping = _productAttributeService.GetProductAttributeMappingById(model.Id);
            if (productAttributeMapping == null)
                throw new ArgumentException("No product attribute mapping found with the specified id");

            var product = _productService.GetProductById(productAttributeMapping.ProductId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return Content("This is not your product");

            productAttributeMapping.ProductAttributeId = model.ProductAttributeId;
            productAttributeMapping.TextPrompt = model.TextPrompt;
            productAttributeMapping.IsRequired = model.IsRequired;
            productAttributeMapping.AttributeControlTypeId = model.AttributeControlTypeId;
            productAttributeMapping.DisplayOrder = model.DisplayOrder;
            _productAttributeService.UpdateProductAttributeMapping(productAttributeMapping);

            UpdateCopiesFromMaster(product.Id);	/// NU-10
            return new NullJsonResult();
        }

        [HttpPost]
        public ActionResult ProductAttributeMappingDelete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var productAttributeMapping = _productAttributeService.GetProductAttributeMappingById(id);
            if (productAttributeMapping == null)
                throw new ArgumentException("No product attribute mapping found with the specified id");

            var productId = productAttributeMapping.ProductId;
            var product = _productService.GetProductById(productId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");


            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return Content("This is not your product");

            _productAttributeService.DeleteProductAttributeMapping(productAttributeMapping);

            UpdateCopiesFromMaster(product.Id);	/// NU-10
            return new NullJsonResult();
        }

        #endregion

        #region Product attributes. Validation rules

        public ActionResult ProductAttributeValidationRulesPopup(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var productAttributeMapping = _productAttributeService.GetProductAttributeMappingById(id);
            if (productAttributeMapping == null)
                //No attribute value found with the specified id
                return RedirectToAction("List", "Product");

            var product = _productService.GetProductById(productAttributeMapping.ProductId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return RedirectToAction("List", "Product");

            var model = new ProductModel.ProductAttributeMappingModel
            {
                //prepare only used properties
                Id = productAttributeMapping.Id,
                ValidationRulesAllowed = productAttributeMapping.ValidationRulesAllowed(),
                AttributeControlTypeId = productAttributeMapping.AttributeControlTypeId,
                ValidationMinLength = productAttributeMapping.ValidationMinLength,
                ValidationMaxLength = productAttributeMapping.ValidationMaxLength,
                ValidationFileAllowedExtensions = productAttributeMapping.ValidationFileAllowedExtensions,
                ValidationFileMaximumSize = productAttributeMapping.ValidationFileMaximumSize,
                DefaultValue = productAttributeMapping.DefaultValue,
            };
            return View(model);
        }
        [HttpPost]
        public ActionResult ProductAttributeValidationRulesPopup(string btnId, string formId, ProductModel.ProductAttributeMappingModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var productAttributeMapping = _productAttributeService.GetProductAttributeMappingById(model.Id);
            if (productAttributeMapping == null)
                //No attribute value found with the specified id
                return RedirectToAction("List", "Product");

            var product = _productService.GetProductById(productAttributeMapping.ProductId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return RedirectToAction("List", "Product");

            if (ModelState.IsValid)
            {
                productAttributeMapping.ValidationMinLength = model.ValidationMinLength;
                productAttributeMapping.ValidationMaxLength = model.ValidationMaxLength;
                productAttributeMapping.ValidationFileAllowedExtensions = model.ValidationFileAllowedExtensions;
                productAttributeMapping.ValidationFileMaximumSize = model.ValidationFileMaximumSize;
                productAttributeMapping.DefaultValue = model.DefaultValue;
                _productAttributeService.UpdateProductAttributeMapping(productAttributeMapping);

                ViewBag.RefreshPage = true;
                ViewBag.btnId = btnId;
                ViewBag.formId = formId;
                return View(model);
            }

            //If we got this far, something failed, redisplay form
            model.ValidationRulesAllowed = productAttributeMapping.ValidationRulesAllowed();
            model.AttributeControlTypeId = productAttributeMapping.AttributeControlTypeId;
            return View(model);
        }

        #endregion

        #region Product attributes. Condition

        public ActionResult ProductAttributeConditionPopup(string btnId, string formId, int productAttributeMappingId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var productAttributeMapping = _productAttributeService.GetProductAttributeMappingById(productAttributeMappingId);
            if (productAttributeMapping == null)
                //No attribute value found with the specified id
                return RedirectToAction("List", "Product");

            var product = _productService.GetProductById(productAttributeMapping.ProductId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return RedirectToAction("List", "Product");

            ViewBag.btnId = btnId;
            ViewBag.formId = formId;

            var model = new ProductAttributeConditionModel();
            model.ProductAttributeMappingId = productAttributeMapping.Id;
            model.EnableCondition = !String.IsNullOrEmpty(productAttributeMapping.ConditionAttributeXml);


            //pre-select attribute and values
            var selectedPva = _productAttributeParser
                .ParseProductAttributeMappings(productAttributeMapping.ConditionAttributeXml)
                .FirstOrDefault();

            var attributes = _productAttributeService.GetProductAttributeMappingsByProductId(product.Id)
                //ignore non-combinable attributes (should have selectable values)
                .Where(x => x.CanBeUsedAsCondition())
                //ignore this attribute (it cannot depend on itself)
                .Where(x => x.Id != productAttributeMapping.Id)
                .ToList();
            foreach (var attribute in attributes)
            {
                var attributeModel = new ProductAttributeConditionModel.ProductAttributeModel
                {
                    Id = attribute.Id,
                    ProductAttributeId = attribute.ProductAttributeId,
                    Name = attribute.ProductAttribute.Name,
                    TextPrompt = attribute.TextPrompt,
                    IsRequired = attribute.IsRequired,
                    AttributeControlType = attribute.AttributeControlType
                };

                if (attribute.ShouldHaveValues())
                {
                    //values
                    var attributeValues = _productAttributeService.GetProductAttributeValues(attribute.Id);
                    foreach (var attributeValue in attributeValues)
                    {
                        var attributeValueModel = new ProductAttributeConditionModel.ProductAttributeValueModel
                        {
                            Id = attributeValue.Id,
                            Name = attributeValue.Name,
                            IsPreSelected = attributeValue.IsPreSelected
                        };
                        attributeModel.Values.Add(attributeValueModel);
                    }

                    //pre-select attribute and value
                    if (selectedPva != null && attribute.Id == selectedPva.Id)
                    {
                        //attribute
                        model.SelectedProductAttributeId = selectedPva.Id;

                        //values
                        switch (attribute.AttributeControlType)
                        {
                            case AttributeControlType.DropdownList:
                            case AttributeControlType.RadioList:
                            case AttributeControlType.Checkboxes:
                            case AttributeControlType.ColorSquares:
                            case AttributeControlType.ImageSquares:
                                {
                                    if (!String.IsNullOrEmpty(productAttributeMapping.ConditionAttributeXml))
                                    {
                                        //clear default selection
                                        foreach (var item in attributeModel.Values)
                                            item.IsPreSelected = false;

                                        //select new values
                                        var selectedValues = _productAttributeParser.ParseProductAttributeValues(productAttributeMapping.ConditionAttributeXml);
                                        foreach (var attributeValue in selectedValues)
                                            foreach (var item in attributeModel.Values)
                                                if (attributeValue.Id == item.Id)
                                                    item.IsPreSelected = true;
                                    }
                                }
                                break;
                            case AttributeControlType.ReadonlyCheckboxes:
                            case AttributeControlType.TextBox:
                            case AttributeControlType.MultilineTextbox:
                            case AttributeControlType.Datepicker:
                            case AttributeControlType.FileUpload:
                            default:
                                //these attribute types are supported as conditions
                                break;
                        }
                    }
                }

                model.ProductAttributes.Add(attributeModel);
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult ProductAttributeConditionPopup(string btnId, string formId,
            ProductAttributeConditionModel model, FormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var productAttributeMapping =
                _productAttributeService.GetProductAttributeMappingById(model.ProductAttributeMappingId);
            if (productAttributeMapping == null)
                //No attribute value found with the specified id
                return RedirectToAction("List", "Product");

            var product = _productService.GetProductById(productAttributeMapping.ProductId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return RedirectToAction("List", "Product");

            string attributesXml = null;
            if (model.EnableCondition)
            {
                var attribute = _productAttributeService.GetProductAttributeMappingById(model.SelectedProductAttributeId);
                if (attribute != null)
                {
                    string controlId = string.Format("product_attribute_{0}", attribute.Id);
                    switch (attribute.AttributeControlType)
                    {
                        case AttributeControlType.DropdownList:
                        case AttributeControlType.RadioList:
                        case AttributeControlType.ColorSquares:
                        case AttributeControlType.ImageSquares:
                            {
                                var ctrlAttributes = form[controlId];
                                if (!String.IsNullOrEmpty(ctrlAttributes))
                                {
                                    int selectedAttributeId = int.Parse(ctrlAttributes);
                                    if (selectedAttributeId > 0)
                                    {
                                        attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                            attribute, selectedAttributeId.ToString());
                                    }
                                    else
                                    {
                                        //for conditions we should empty values save even when nothing is selected
                                        //otherwise "attributesXml" will be empty
                                        //hence we won't be able to find a selected attribute
                                        attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                            attribute, "");
                                    }
                                }
                                else
                                {
                                    //for conditions we should empty values save even when nothing is selected
                                    //otherwise "attributesXml" will be empty
                                    //hence we won't be able to find a selected attribute
                                    attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                        attribute, "");
                                }
                            }
                            break;
                        case AttributeControlType.Checkboxes:
                            {
                                var cblAttributes = form[controlId];
                                if (!String.IsNullOrEmpty(cblAttributes))
                                {
                                    bool anyValueSelected = false;
                                    foreach (var item in cblAttributes.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                                    {
                                        int selectedAttributeId = int.Parse(item);
                                        if (selectedAttributeId > 0)
                                        {
                                            attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                                attribute, selectedAttributeId.ToString());
                                            anyValueSelected = true;
                                        }
                                    }
                                    if (!anyValueSelected)
                                    {
                                        //for conditions we should save empty values even when nothing is selected
                                        //otherwise "attributesXml" will be empty
                                        //hence we won't be able to find a selected attribute
                                        attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                            attribute, "");
                                    }
                                }
                                else
                                {
                                    //for conditions we should save empty values even when nothing is selected
                                    //otherwise "attributesXml" will be empty
                                    //hence we won't be able to find a selected attribute
                                    attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                            attribute, "");
                                }
                            }
                            break;
                        case AttributeControlType.ReadonlyCheckboxes:
                        case AttributeControlType.TextBox:
                        case AttributeControlType.MultilineTextbox:
                        case AttributeControlType.Datepicker:
                        case AttributeControlType.FileUpload:
                        default:
                            //these attribute types are supported as conditions
                            break;
                    }
                }
            }
            productAttributeMapping.ConditionAttributeXml = attributesXml;
            _productAttributeService.UpdateProductAttributeMapping(productAttributeMapping);

            ViewBag.RefreshPage = true;
            ViewBag.btnId = btnId;
            ViewBag.formId = formId;
            return View(model);
        }

        #endregion

        #region Product attribute values

        //list
        public ActionResult EditAttributeValues(int productAttributeMappingId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var productAttributeMapping = _productAttributeService.GetProductAttributeMappingById(productAttributeMappingId);
            if (productAttributeMapping == null)
                throw new ArgumentException("No product attribute mapping found with the specified id");

            var product = _productService.GetProductById(productAttributeMapping.ProductId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return RedirectToAction("List", "Product");

            var model = new ProductModel.ProductAttributeValueListModel
            {
                ProductName = product.Name,
                ProductId = productAttributeMapping.ProductId,
                ProductAttributeName = productAttributeMapping.ProductAttribute.Name,
                ProductAttributeMappingId = productAttributeMapping.Id,
            };

            return View(model);
        }

        [HttpPost]
        public ActionResult ProductAttributeValueList(int productAttributeMappingId, DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var productAttributeMapping = _productAttributeService.GetProductAttributeMappingById(productAttributeMappingId);
            if (productAttributeMapping == null)
                throw new ArgumentException("No product attribute mapping found with the specified id");

            var product = _productService.GetProductById(productAttributeMapping.ProductId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return Content("This is not your product");

            var values = _productAttributeService.GetProductAttributeValues(productAttributeMappingId);
            var gridModel = new DataSourceResult
            {
                Data = values.Select(x =>
                {
                    Product associatedProduct = null;
                    if (x.AttributeValueType == AttributeValueType.AssociatedToProduct)
                    {
                        associatedProduct = _productService.GetProductById(x.AssociatedProductId);
                    }
                    var pictureThumbnailUrl = _pictureService.GetPictureUrl(x.PictureId, 75, false);
                    //little hack here. Grid is rendered wrong way with <img> without "src" attribute
                    if (String.IsNullOrEmpty(pictureThumbnailUrl))
                        pictureThumbnailUrl = _pictureService.GetPictureUrl(null, 1, true);
                    return new ProductModel.ProductAttributeValueModel
                    {
                        Id = x.Id,
                        ProductAttributeMappingId = x.ProductAttributeMappingId,
                        AttributeValueTypeId = x.AttributeValueTypeId,
                        AttributeValueTypeName = x.AttributeValueType.GetLocalizedEnum(_localizationService, _workContext),
                        AssociatedProductId = x.AssociatedProductId,
                        AssociatedProductName = associatedProduct != null ? associatedProduct.Name : "",
                        Name = x.ProductAttributeMapping.AttributeControlType != AttributeControlType.ColorSquares ? x.Name : string.Format("{0} - {1}", x.Name, x.ColorSquaresRgb),
                        ColorSquaresRgb = x.ColorSquaresRgb,
                        ImageSquaresPictureId = x.ImageSquaresPictureId,
                        PriceAdjustment = x.PriceAdjustment,
                        PriceAdjustmentStr = x.AttributeValueType == AttributeValueType.Simple ? x.PriceAdjustment.ToString("G29") : "",
                        WeightAdjustment = x.WeightAdjustment,
                        WeightAdjustmentStr = x.AttributeValueType == AttributeValueType.Simple ? x.WeightAdjustment.ToString("G29") : "",
                        Cost = x.Cost,
                        Quantity = x.Quantity,
                        IsPreSelected = x.IsPreSelected,
                        DisplayOrder = x.DisplayOrder,
                        PictureId = x.PictureId,
                        PictureThumbnailUrl = pictureThumbnailUrl
                    };
                }).OrderBy(av => av.DisplayOrder), /// NU-46
                Total = values.Count()
            };

            return Json(gridModel);
        }

        //create
        public ActionResult ProductAttributeValueCreatePopup(int productAttributeMappingId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var productAttributeMapping = _productAttributeService.GetProductAttributeMappingById(productAttributeMappingId);
            if (productAttributeMapping == null)
                throw new ArgumentException("No product attribute mapping found with the specified id");

            var product = _productService.GetProductById(productAttributeMapping.ProductId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return RedirectToAction("List", "Product");

            var model = new ProductModel.ProductAttributeValueModel();
            model.ProductAttributeMappingId = productAttributeMappingId;

            //color squares
            model.DisplayColorSquaresRgb = productAttributeMapping.AttributeControlType == AttributeControlType.ColorSquares;
            model.ColorSquaresRgb = "#000000";
            //image squares
            model.DisplayImageSquaresPicture = productAttributeMapping.AttributeControlType == AttributeControlType.ImageSquares;

            //default qantity for associated product
            model.Quantity = 1;

            //locales
            AddLocales(_languageService, model.Locales);

            //pictures
            model.ProductPictureModels = _productService.GetProductPicturesByProductId(product.Id)
                .Select(x => new ProductModel.ProductPictureModel
                {
                    Id = x.Id,
                    ProductId = x.ProductId,
                    PictureId = x.PictureId,
                    PictureUrl = _pictureService.GetPictureUrl(x.PictureId),
                    DisplayOrder = x.DisplayOrder
                })
                .ToList();

            return View(model);
        }

        [HttpPost]
        public ActionResult ProductAttributeValueCreatePopup(string btnId, string formId, ProductModel.ProductAttributeValueModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var productAttributeMapping = _productAttributeService.GetProductAttributeMappingById(model.ProductAttributeMappingId);
            if (productAttributeMapping == null)
                //No product attribute found with the specified id
                return RedirectToAction("List", "Product");

            var product = _productService.GetProductById(productAttributeMapping.ProductId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return RedirectToAction("List", "Product");

            if (productAttributeMapping.AttributeControlType == AttributeControlType.ColorSquares)
            {
                //ensure valid color is chosen/entered
                if (String.IsNullOrEmpty(model.ColorSquaresRgb))
                    ModelState.AddModelError("", "Color is required");
                try
                {
                    //ensure color is valid (can be instanciated)
                    System.Drawing.ColorTranslator.FromHtml(model.ColorSquaresRgb);
                }
                catch (Exception exc)
                {
                    ModelState.AddModelError("", exc.Message);
                }
            }

            //ensure a picture is uploaded
            if (productAttributeMapping.AttributeControlType == AttributeControlType.ImageSquares && model.ImageSquaresPictureId == 0)
            {
                ModelState.AddModelError("", "Image is required");
            }

            if (ModelState.IsValid)
            {
                var pav = new ProductAttributeValue
                {
                    ProductAttributeMappingId = model.ProductAttributeMappingId,
                    AttributeValueTypeId = model.AttributeValueTypeId,
                    AssociatedProductId = model.AssociatedProductId,
                    Name = model.Name,
                    ColorSquaresRgb = model.ColorSquaresRgb,
                    ImageSquaresPictureId = model.ImageSquaresPictureId,
                    PriceAdjustment = model.PriceAdjustment,
                    WeightAdjustment = model.WeightAdjustment,
                    Cost = model.Cost,
                    Quantity = model.Quantity,
                    IsPreSelected = model.IsPreSelected,
                    DisplayOrder = model.DisplayOrder,
                    PictureId = model.PictureId,
                };

                _productAttributeService.InsertProductAttributeValue(pav);
                UpdateLocales(pav, model);

                ViewBag.RefreshPage = true;
                ViewBag.btnId = btnId;
                ViewBag.formId = formId;
                return View(model);
            }

            //If we got this far, something failed, redisplay form


            //pictures
            model.ProductPictureModels = _productService.GetProductPicturesByProductId(product.Id)
                .Select(x => new ProductModel.ProductPictureModel
                {
                    Id = x.Id,
                    ProductId = x.ProductId,
                    PictureId = x.PictureId,
                    PictureUrl = _pictureService.GetPictureUrl(x.PictureId),
                    DisplayOrder = x.DisplayOrder
                })
                .ToList();

            var associatedProduct = _productService.GetProductById(model.AssociatedProductId);
            model.AssociatedProductName = associatedProduct != null ? associatedProduct.Name : "";

            return View(model);
        }

        //edit
        public ActionResult ProductAttributeValueEditPopup(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var pav = _productAttributeService.GetProductAttributeValueById(id);
            if (pav == null)
                //No attribute value found with the specified id
                return RedirectToAction("List", "Product");

            var product = _productService.GetProductById(pav.ProductAttributeMapping.ProductId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return RedirectToAction("List", "Product");

            var associatedProduct = _productService.GetProductById(pav.AssociatedProductId);

            var model = new ProductModel.ProductAttributeValueModel
            {
                ProductAttributeMappingId = pav.ProductAttributeMappingId,
                AttributeValueTypeId = pav.AttributeValueTypeId,
                AttributeValueTypeName = pav.AttributeValueType.GetLocalizedEnum(_localizationService, _workContext),
                AssociatedProductId = pav.AssociatedProductId,
                AssociatedProductName = associatedProduct != null ? associatedProduct.Name : "",
                Name = pav.Name,
                ColorSquaresRgb = pav.ColorSquaresRgb,
                DisplayColorSquaresRgb = pav.ProductAttributeMapping.AttributeControlType == AttributeControlType.ColorSquares,
                ImageSquaresPictureId = pav.ImageSquaresPictureId,
                DisplayImageSquaresPicture = pav.ProductAttributeMapping.AttributeControlType == AttributeControlType.ImageSquares,
                PriceAdjustment = pav.PriceAdjustment,
                WeightAdjustment = pav.WeightAdjustment,
                Cost = pav.Cost,
                Quantity = pav.Quantity,
                IsPreSelected = pav.IsPreSelected,
                DisplayOrder = pav.DisplayOrder,
                PictureId = pav.PictureId
            };
            if (model.DisplayColorSquaresRgb && String.IsNullOrEmpty(model.ColorSquaresRgb))
            {
                model.ColorSquaresRgb = "#000000";
            }
            //locales
            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = pav.GetLocalized(x => x.Name, languageId, false, false);
            });
            //pictures
            model.ProductPictureModels = _productService.GetProductPicturesByProductId(product.Id)
                .Select(x => new ProductModel.ProductPictureModel
                {
                    Id = x.Id,
                    ProductId = x.ProductId,
                    PictureId = x.PictureId,
                    PictureUrl = _pictureService.GetPictureUrl(x.PictureId),
                    DisplayOrder = x.DisplayOrder
                })
                .ToList();

            return View(model);
        }

        [HttpPost]
        public ActionResult ProductAttributeValueEditPopup(string btnId, string formId, ProductModel.ProductAttributeValueModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var pav = _productAttributeService.GetProductAttributeValueById(model.Id);
            if (pav == null)
                //No attribute value found with the specified id
                return RedirectToAction("List", "Product");

            var product = _productService.GetProductById(pav.ProductAttributeMapping.ProductId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return RedirectToAction("List", "Product");

            if (pav.ProductAttributeMapping.AttributeControlType == AttributeControlType.ColorSquares)
            {
                //ensure valid color is chosen/entered
                if (String.IsNullOrEmpty(model.ColorSquaresRgb))
                    ModelState.AddModelError("", "Color is required");
                try
                {
                    //ensure color is valid (can be instanciated)
                    System.Drawing.ColorTranslator.FromHtml(model.ColorSquaresRgb);
                }
                catch (Exception exc)
                {
                    ModelState.AddModelError("", exc.Message);
                }
            }

            //ensure a picture is uploaded
            if (pav.ProductAttributeMapping.AttributeControlType == AttributeControlType.ImageSquares && model.ImageSquaresPictureId == 0)
            {
                ModelState.AddModelError("", "Image is required");
            }

            if (ModelState.IsValid)
            {
                pav.AttributeValueTypeId = model.AttributeValueTypeId;
                pav.AssociatedProductId = model.AssociatedProductId;
                pav.Name = model.Name;
                pav.ColorSquaresRgb = model.ColorSquaresRgb;
                pav.ImageSquaresPictureId = model.ImageSquaresPictureId;
                pav.PriceAdjustment = model.PriceAdjustment;
                pav.WeightAdjustment = model.WeightAdjustment;
                pav.Cost = model.Cost;
                pav.Quantity = model.Quantity;
                pav.IsPreSelected = model.IsPreSelected;
                pav.DisplayOrder = model.DisplayOrder;
                pav.PictureId = model.PictureId;
                _productAttributeService.UpdateProductAttributeValue(pav);

                UpdateLocales(pav, model);

                ViewBag.RefreshPage = true;
                ViewBag.btnId = btnId;
                ViewBag.formId = formId;
                return View(model);
            }

            //If we got this far, something failed, redisplay form

            //pictures
            model.ProductPictureModels = _productService.GetProductPicturesByProductId(product.Id)
                .Select(x => new ProductModel.ProductPictureModel
                {
                    Id = x.Id,
                    ProductId = x.ProductId,
                    PictureId = x.PictureId,
                    PictureUrl = _pictureService.GetPictureUrl(x.PictureId),
                    DisplayOrder = x.DisplayOrder
                })
                .ToList();

            var associatedProduct = _productService.GetProductById(model.AssociatedProductId);
            model.AssociatedProductName = associatedProduct != null ? associatedProduct.Name : "";

            return View(model);
        }

        //delete
        [HttpPost]
        public ActionResult ProductAttributeValueDelete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var pav = _productAttributeService.GetProductAttributeValueById(id);
            if (pav == null)
                throw new ArgumentException("No product attribute value found with the specified id");

            var product = _productService.GetProductById(pav.ProductAttributeMapping.ProductId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return Content("This is not your product");

            _productAttributeService.DeleteProductAttributeValue(pav);

            return new NullJsonResult();
        }





        public ActionResult AssociateProductToAttributeValuePopup()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var model = new ProductModel.ProductAttributeValueModel.AssociateProductToAttributeValueModel();
            //a vendor should have access only to his products
            model.IsLoggedInAsVendor = _workContext.CurrentVendor != null;

            //categories
            model.AvailableCategories.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            var categories = SelectListHelper.GetCategoryList(_categoryService, _cacheManager, true);
            foreach (var c in categories)
                model.AvailableCategories.Add(c);

            //manufacturers
            model.AvailableManufacturers.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            foreach (var m in _manufacturerService.GetAllManufacturers(showHidden: true))
                model.AvailableManufacturers.Add(new SelectListItem { Text = m.Name, Value = m.Id.ToString() });

            //stores
            model.AvailableStores.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            foreach (var s in _storeService.GetAllStores())
                model.AvailableStores.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString() });

            //vendors
            model.AvailableVendors.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            foreach (var v in _vendorService.GetAllVendors(showHidden: true))
                model.AvailableVendors.Add(new SelectListItem { Text = v.Name, Value = v.Id.ToString() });

            //product types
            model.AvailableProductTypes = ProductType.SimpleProduct.ToSelectList(false).ToList();
            model.AvailableProductTypes.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });

            model.IsLoggedAs = _workContext.CurrentCustomer.IsLoggedAs();	// NU-1

            return View(model);
        }

        [HttpPost]
        public ActionResult AssociateProductToAttributeValuePopupList(DataSourceRequest command,
            ProductModel.ProductAttributeValueModel.AssociateProductToAttributeValueModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                model.SearchVendorId = _workContext.CurrentVendor.Id;
            }

            if (model.SearchStoreId == 0)
                model.SearchStoreId = _storeContext.CurrentStore.Id;

            if (command.PageSize == 0)
                command.PageSize = 100;

            var products = _productService.SearchProducts(
                categoryIds: new List<int> { model.SearchCategoryId },
                manufacturerId: model.SearchManufacturerId,
                storeId: model.SearchStoreId,
                vendorId: model.SearchVendorId,
                productType: model.SearchProductTypeId > 0 ? (ProductType?)model.SearchProductTypeId : null,
                keywords: model.SearchProductName,
                pageIndex: command.Page - 1,
                pageSize: command.PageSize,
                showHidden: true,
                overridePublished: null,
                overrideHideMasterProducts: true
                );

            var gridModel = new DataSourceResult();
            gridModel.Data = products.Select(x => x.ToModel())
                .OrderBy(p => p.Name); /// NU-24
            gridModel.Total = products.TotalCount;

            return Json(gridModel);
        }

        [HttpPost]
        [FormValueRequired("save")]
        public ActionResult AssociateProductToAttributeValuePopup(string productIdInput,
            string productNameInput, ProductModel.ProductAttributeValueModel.AssociateProductToAttributeValueModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var associatedProduct = _productService.GetProductById(model.AssociatedToProductId);
            if (associatedProduct == null)
                return Content("Cannot load a product");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && associatedProduct.VendorId != _workContext.CurrentVendor.Id)
                return Content("This is not your product");

            //a vendor should have access only to his products
            model.IsLoggedInAsVendor = _workContext.CurrentVendor != null;
            ViewBag.RefreshPage = true;
            ViewBag.productIdInput = productIdInput;
            ViewBag.productNameInput = productNameInput;
            ViewBag.productId = associatedProduct.Id;
            ViewBag.productName = associatedProduct.Name;

            model.IsLoggedAs = _workContext.CurrentCustomer.IsLoggedAs();	// NU-1
            return View(model);
        }


        #endregion

        #region Product attribute combinations

        [HttpPost]
        public ActionResult ProductAttributeCombinationList(DataSourceRequest command, int productId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var product = _productService.GetProductById(productId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return Content("This is not your product");

            var combinations = _productAttributeService.GetAllProductAttributeCombinations(productId);
            var combinationsModel = combinations
                .Select(x =>
                {
                    var pacModel = new ProductModel.ProductAttributeCombinationModel
                    {
                        Id = x.Id,
                        ProductId = x.ProductId,
                        AttributesXml = _productAttributeFormatter.FormatAttributes(x.Product, x.AttributesXml, _workContext.CurrentCustomer, "<br />", true, true, true, false),
                        StockQuantity = x.StockQuantity,
                        AllowOutOfStockOrders = x.AllowOutOfStockOrders,
                        Sku = x.Sku,
                        ManufacturerPartNumber = x.ManufacturerPartNumber,
                        Gtin = x.Gtin,
                        OverriddenPrice = x.OverriddenPrice,
                        NotifyAdminForQuantityBelow = x.NotifyAdminForQuantityBelow
                    };
                    //warnings
                    var warnings = _shoppingCartService.GetShoppingCartItemAttributeWarnings(_workContext.CurrentCustomer,
                        ShoppingCartType.ShoppingCart, x.Product, 1, x.AttributesXml, true);
                    for (int i = 0; i < warnings.Count; i++)
                    {
                        pacModel.Warnings += warnings[i];
                        if (i != warnings.Count - 1)
                            pacModel.Warnings += "<br />";
                    }

                    return pacModel;
                })
                .ToList();

            var gridModel = new DataSourceResult
            {
                Data = combinationsModel,
                Total = combinationsModel.Count
            };

            return Json(gridModel);
        }

        [HttpPost]
        public ActionResult ProductAttributeCombinationUpdate(ProductModel.ProductAttributeCombinationModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var combination = _productAttributeService.GetProductAttributeCombinationById(model.Id);
            if (combination == null)
                throw new ArgumentException("No product attribute combination found with the specified id");

            var product = _productService.GetProductById(combination.ProductId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return Content("This is not your product");

            combination.StockQuantity = model.StockQuantity;
            combination.AllowOutOfStockOrders = model.AllowOutOfStockOrders;
            combination.Sku = model.Sku;
            combination.ManufacturerPartNumber = model.ManufacturerPartNumber;
            combination.Gtin = model.Gtin;
            combination.OverriddenPrice = model.OverriddenPrice;
            combination.NotifyAdminForQuantityBelow = model.NotifyAdminForQuantityBelow;
            _productAttributeService.UpdateProductAttributeCombination(combination);

            UpdateCopiesFromMaster(product.Id);	/// NU-10
            return new NullJsonResult();
        }

        [HttpPost]
        public ActionResult ProductAttributeCombinationDelete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var combination = _productAttributeService.GetProductAttributeCombinationById(id);
            if (combination == null)
                throw new ArgumentException("No product attribute combination found with the specified id");

            var product = _productService.GetProductById(combination.ProductId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return Content("This is not your product");

            _productAttributeService.DeleteProductAttributeCombination(combination);

            UpdateCopiesFromMaster(product.Id);	/// NU-10
            return new NullJsonResult();
        }

        public ActionResult AddAttributeCombinationPopup(string btnId, string formId, int productId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var product = _productService.GetProductById(productId);
            if (product == null)
                //No product found with the specified id
                return RedirectToAction("List", "Product");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return RedirectToAction("List", "Product");

            ViewBag.btnId = btnId;
            ViewBag.formId = formId;
            var model = new AddProductAttributeCombinationModel();
            PrepareAddProductAttributeCombinationModel(model, product);
            return View(model);
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult AddAttributeCombinationPopup(string btnId, string formId, int productId,
            AddProductAttributeCombinationModel model, FormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var product = _productService.GetProductById(productId);
            if (product == null)
                //No product found with the specified id
                return RedirectToAction("List", "Product");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return RedirectToAction("List", "Product");

            ViewBag.btnId = btnId;
            ViewBag.formId = formId;

            //attributes
            string attributesXml = "";
            var warnings = new List<string>();

            #region Product attributes

            var attributes = _productAttributeService.GetProductAttributeMappingsByProductId(product.Id)
                //ignore non-combinable attributes for combinations
                .Where(x => !x.IsNonCombinable())
                .ToList();
            foreach (var attribute in attributes)
            {
                string controlId = string.Format("product_attribute_{0}", attribute.Id);
                switch (attribute.AttributeControlType)
                {
                    case AttributeControlType.DropdownList:
                    case AttributeControlType.RadioList:
                    case AttributeControlType.ColorSquares:
                    case AttributeControlType.ImageSquares:
                        {
                            var ctrlAttributes = form[controlId];
                            if (!String.IsNullOrEmpty(ctrlAttributes))
                            {
                                int selectedAttributeId = int.Parse(ctrlAttributes);
                                if (selectedAttributeId > 0)
                                    attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                        attribute, selectedAttributeId.ToString());
                            }
                        }
                        break;
                    case AttributeControlType.Checkboxes:
                        {
                            var cblAttributes = form[controlId];
                            if (!String.IsNullOrEmpty(cblAttributes))
                            {
                                foreach (var item in cblAttributes.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                                {
                                    int selectedAttributeId = int.Parse(item);
                                    if (selectedAttributeId > 0)
                                        attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                            attribute, selectedAttributeId.ToString());
                                }
                            }
                        }
                        break;
                    case AttributeControlType.ReadonlyCheckboxes:
                        {
                            //load read-only (already server-side selected) values
                            var attributeValues = _productAttributeService.GetProductAttributeValues(attribute.Id);
                            foreach (var selectedAttributeId in attributeValues
                                .Where(v => v.IsPreSelected)
                                .Select(v => v.Id)
                                .ToList())
                            {
                                attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                    attribute, selectedAttributeId.ToString());
                            }
                        }
                        break;
                    case AttributeControlType.TextBox:
                    case AttributeControlType.MultilineTextbox:
                        {
                            var ctrlAttributes = form[controlId];
                            if (!String.IsNullOrEmpty(ctrlAttributes))
                            {
                                string enteredText = ctrlAttributes.Trim();
                                attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                    attribute, enteredText);
                            }
                        }
                        break;
                    case AttributeControlType.Datepicker:
                        {
                            var date = form[controlId + "_day"];
                            var month = form[controlId + "_month"];
                            var year = form[controlId + "_year"];
                            DateTime? selectedDate = null;
                            try
                            {
                                selectedDate = new DateTime(Int32.Parse(year), Int32.Parse(month), Int32.Parse(date));
                            }
                            catch { }
                            if (selectedDate.HasValue)
                            {
                                attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                    attribute, selectedDate.Value.ToString("D"));
                            }
                        }
                        break;
                    case AttributeControlType.FileUpload:
                        {
                            var httpPostedFile = this.Request.Files[controlId];
                            if ((httpPostedFile != null) && (!String.IsNullOrEmpty(httpPostedFile.FileName)))
                            {
                                var fileSizeOk = true;
                                if (attribute.ValidationFileMaximumSize.HasValue)
                                {
                                    //compare in bytes
                                    var maxFileSizeBytes = attribute.ValidationFileMaximumSize.Value * 1024;
                                    if (httpPostedFile.ContentLength > maxFileSizeBytes)
                                    {
                                        warnings.Add(string.Format(_localizationService.GetResource("ShoppingCart.MaximumUploadedFileSize"), attribute.ValidationFileMaximumSize.Value));
                                        fileSizeOk = false;
                                    }
                                }
                                if (fileSizeOk)
                                {
                                    //save an uploaded file
                                    var download = new Download
                                    {
                                        DownloadGuid = Guid.NewGuid(),
                                        UseDownloadUrl = false,
                                        DownloadUrl = "",
                                        DownloadBinary = httpPostedFile.GetDownloadBits(),
                                        ContentType = httpPostedFile.ContentType,
                                        Filename = Path.GetFileNameWithoutExtension(httpPostedFile.FileName),
                                        Extension = Path.GetExtension(httpPostedFile.FileName),
                                        IsNew = true
                                    };
                                    _downloadService.InsertDownload(download);
                                    //save attribute
                                    attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                        attribute, download.DownloadGuid.ToString());
                                }
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
            //validate conditional attributes (if specified)
            foreach (var attribute in attributes)
            {
                var conditionMet = _productAttributeParser.IsConditionMet(attribute, attributesXml);
                if (conditionMet.HasValue && !conditionMet.Value)
                {
                    attributesXml = _productAttributeParser.RemoveProductAttribute(attributesXml, attribute);
                }
            }

            #endregion

            warnings.AddRange(_shoppingCartService.GetShoppingCartItemAttributeWarnings(_workContext.CurrentCustomer,
                ShoppingCartType.ShoppingCart, product, 1, attributesXml, true));
            if (!warnings.Any())
            {
                //save combination
                var combination = new ProductAttributeCombination
                {
                    ProductId = product.Id,
                    AttributesXml = attributesXml,
                    StockQuantity = model.StockQuantity,
                    AllowOutOfStockOrders = model.AllowOutOfStockOrders,
                    Sku = model.Sku,
                    ManufacturerPartNumber = model.ManufacturerPartNumber,
                    Gtin = model.Gtin,
                    OverriddenPrice = model.OverriddenPrice,
                    NotifyAdminForQuantityBelow = model.NotifyAdminForQuantityBelow,
                };
                _productAttributeService.InsertProductAttributeCombination(combination);

                ViewBag.RefreshPage = true;
                return View(model);
            }

            //If we got this far, something failed, redisplay form
            PrepareAddProductAttributeCombinationModel(model, product);
            model.Warnings = warnings;
            return View(model);
        }

        [HttpPost]
        public ActionResult GenerateAllAttributeCombinations(int productId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var product = _productService.GetProductById(productId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return Content("This is not your product");

            var allAttributesXml = _productAttributeParser.GenerateAllCombinations(product, true);
            foreach (var attributesXml in allAttributesXml)
            {
                var existingCombination = _productAttributeParser.FindProductAttributeCombination(product, attributesXml);

                //already exists?
                if (existingCombination != null)
                    continue;

                //new one
                var warnings = new List<string>();
                warnings.AddRange(_shoppingCartService.GetShoppingCartItemAttributeWarnings(_workContext.CurrentCustomer,
                    ShoppingCartType.ShoppingCart, product, 1, attributesXml, true));
                if (warnings.Count != 0)
                    continue;

                //save combination
                var combination = new ProductAttributeCombination
                {
                    ProductId = product.Id,
                    AttributesXml = attributesXml,
                    StockQuantity = 10000,
                    AllowOutOfStockOrders = false,
                    Sku = null,
                    ManufacturerPartNumber = null,
                    Gtin = null,
                    OverriddenPrice = null,
                    NotifyAdminForQuantityBelow = 1
                };
                _productAttributeService.InsertProductAttributeCombination(combination);
            }
            return Json(new { Success = true });
        }

        #endregion

        #region NU-10
        public string RedirectionTargetMasterLocal(Product _product)
        {
            string ret = "List";

            if (_product.IsMaster)
            {
                ret = "MasterList";
            }

            return (ret);
        }

        /// <summary>
        /// Loads the Master Catalog
        /// </summary>
        /// <returns>Action Result</returns>
        public ActionResult MasterList()
        {
            return List(true);
        }
        #endregion

        #region NU-55
        /// <summary>
        /// This method will copy the product down to the selected store's catalog
        /// </summary>
        /// <param name="model"></param>
        /// <param name="isMasterProduct"></param>
        /// <returns></returns>
        private Product CopyProductFromProductModel(ProductModel model, bool isMasterProduct)
        {
            var copyModel = model.CopyProductModel;
            try
            {
                var originalProduct = _productService.GetProductById(model.Id);
                originalProduct.IsMaster = true;

                //var newProduct = _copyProductService.CopyProduct(originalProduct,
                //    copyModel.Name, copyModel.Published, copyModel.CopyImages);

                originalProduct.IsMaster = isMasterProduct;


                //SuccessNotification("The product has been copied successfully");
                return originalProduct;
            }
            catch (Exception exc)
            {
                return null;
            }
        }
        #endregion

        #region NU-10
        private void UpdateCopiesFromMaster(int masterID)
        {
            var master = _productService.GetProductById(masterID);

            if (master.IsMaster && master.VendorId > 0)
            {
                var products = _productService.GetProductChildren(master);

                foreach (var productID in products)
                {
                    var product = _productService.GetProductById(productID);

                    product.ProductTypeId = master.ProductTypeId;
                    product.VisibleIndividually = master.VisibleIndividually;
                    product.Name = master.Name;
                    product.ShortDescription = master.ShortDescription;
                    product.FullDescription = master.FullDescription;
                    product.VendorId = master.VendorId;
                    product.ProductTemplateId = master.ProductTemplateId;
                    product.AdminComment = master.AdminComment;
                    product.ShowOnHomePage = master.ShowOnHomePage;
                    product.MetaKeywords = master.MetaKeywords;
                    product.MetaDescription = master.MetaDescription;
                    product.MetaTitle = master.MetaTitle;
                    product.AllowCustomerReviews = master.AllowCustomerReviews;
                    product.LimitedToStores = master.LimitedToStores;
                    product.Sku = master.Sku;
                    product.IsMaster = false;
                    product.MasterId = master.Id;   /// NU-10
                    product.ManufacturerPartNumber = master.ManufacturerPartNumber;
                    product.Gtin = master.Gtin;
                    product.IsGiftCard = master.IsGiftCard;
                    product.IsMealPlan = master.IsMealPlan; /// NU-16
                    product.ShowStandardMealPlanFields = master.ShowStandardMealPlanFields; //NU-90
                    product.IsDonation = master.IsDonation; /// NU-17
                    product.GiftCardTypeId = master.GiftCardTypeId;
                    product.OverriddenGiftCardAmount = master.OverriddenGiftCardAmount;
                    product.RequireOtherProducts = master.RequireOtherProducts;
                    product.RequiredProductIds = master.RequiredProductIds;
                    product.AutomaticallyAddRequiredProducts = master.AutomaticallyAddRequiredProducts;
                    product.IsDownload = master.IsDownload;
                    product.DownloadId = master.DownloadId;
                    product.UnlimitedDownloads = master.UnlimitedDownloads;
                    product.MaxNumberOfDownloads = master.MaxNumberOfDownloads;
                    product.DownloadExpirationDays = master.DownloadExpirationDays;
                    product.DownloadActivationTypeId = master.DownloadActivationTypeId;
                    product.HasSampleDownload = master.HasSampleDownload;
                    product.SampleDownloadId = master.SampleDownloadId;
                    product.HasUserAgreement = master.HasUserAgreement;
                    product.UserAgreementText = master.UserAgreementText;
                    product.IsRecurring = master.IsRecurring;
                    product.RecurringCycleLength = master.RecurringCycleLength;
                    product.RecurringCyclePeriodId = master.RecurringCyclePeriodId;
                    product.RecurringTotalCycles = master.RecurringTotalCycles;
                    product.IsRental = master.IsRental;
                    product.RentalPriceLength = master.RentalPriceLength;
                    product.RentalPricePeriodId = master.RentalPricePeriodId;
                    product.IsShipEnabled = master.IsShipEnabled;
                    product.IsFreeShipping = master.IsFreeShipping;
                    product.ShipSeparately = master.ShipSeparately;
                    product.AdditionalShippingCharge = master.AdditionalShippingCharge;
                    product.DeliveryDateId = master.DeliveryDateId;
                    product.IsTaxExempt = master.IsTaxExempt;
                    product.TaxCategoryId = master.TaxCategoryId;
                    product.IsTelecommunicationsOrBroadcastingOrElectronicServices = master.IsTelecommunicationsOrBroadcastingOrElectronicServices;
                    product.ManageInventoryMethodId = master.ManageInventoryMethodId;
                    product.UseMultipleWarehouses = master.UseMultipleWarehouses;
                    product.WarehouseId = master.WarehouseId;
                    product.StockQuantity = master.StockQuantity;
                    product.DisplayStockAvailability = master.DisplayStockAvailability;
                    product.DisplayStockQuantity = master.DisplayStockQuantity;
                    product.MinStockQuantity = master.MinStockQuantity;
                    product.LowStockActivityId = master.LowStockActivityId;
                    product.NotifyAdminForQuantityBelow = master.NotifyAdminForQuantityBelow;
                    product.BackorderModeId = master.BackorderModeId;
                    product.AllowBackInStockSubscriptions = master.AllowBackInStockSubscriptions;
                    product.OrderMinimumQuantity = master.OrderMinimumQuantity;
                    product.OrderMaximumQuantity = master.OrderMaximumQuantity;
                    product.AllowedQuantities = master.AllowedQuantities;
                    product.AllowAddingOnlyExistingAttributeCombinations = master.AllowAddingOnlyExistingAttributeCombinations;
                    product.DisableBuyButton = master.DisableBuyButton;
                    product.DisableBuyButtonForGuestUsers = master.DisableBuyButtonForGuestUsers; //NU-90
                    product.DisableWishlistButton = master.DisableWishlistButton;
                    product.AvailableForPreOrder = master.AvailableForPreOrder;
                    product.PreOrderAvailabilityStartDateTimeUtc = master.PreOrderAvailabilityStartDateTimeUtc;
                    product.CallForPrice = master.CallForPrice;
                    product.Price = master.Price;
                    product.OldPrice = master.OldPrice;
                    product.ProductCost = master.ProductCost;
                    product.SpecialPrice = master.SpecialPrice;
                    product.SpecialPriceStartDateTimeUtc = master.SpecialPriceStartDateTimeUtc;
                    product.SpecialPriceEndDateTimeUtc = master.SpecialPriceEndDateTimeUtc;
                    product.CustomerEntersPrice = master.CustomerEntersPrice;
                    product.MinimumCustomerEnteredPrice = master.MinimumCustomerEnteredPrice;
                    product.MaximumCustomerEnteredPrice = master.MaximumCustomerEnteredPrice;
                    product.BasepriceEnabled = master.BasepriceEnabled;
                    product.BasepriceAmount = master.BasepriceAmount;
                    product.BasepriceUnitId = master.BasepriceUnitId;
                    product.BasepriceBaseAmount = master.BasepriceBaseAmount;
                    product.BasepriceBaseUnitId = master.BasepriceBaseUnitId;
                    product.MarkAsNew = master.MarkAsNew;
                    product.MarkAsNewStartDateTimeUtc = master.MarkAsNewStartDateTimeUtc;
                    product.MarkAsNewEndDateTimeUtc = master.MarkAsNewEndDateTimeUtc;
                    product.Weight = master.Weight;
                    product.Length = master.Length;
                    product.Width = master.Width;
                    product.Height = master.Height;
                    product.AvailableStartDateTimeUtc = master.AvailableStartDateTimeUtc;
                    product.AvailableEndDateTimeUtc = master.AvailableEndDateTimeUtc;
                    product.DisplayOrder = master.DisplayOrder;
                    product.UpdatedOnUtc = DateTime.UtcNow;
                    product.ProductionDaysLead = master.ProductionDaysLead;
                    product.ProductionHoursLead = master.ProductionHoursLead;
                    product.ProductionMinutesLead = master.ProductionMinutesLead;
                    product.Deleted = master.Deleted;
                    product.IsPickupEnabled = master.IsPickupEnabled;
                    product.StoreCommission = master.StoreCommission;
                    //product.TaxRequestQuantity = master.TaxRequestQuantity;

                    //Reservation Setup
                    product.IsReservation = master.IsReservation;
                    product.ReservationStartTime = master.ReservationStartTime;
                    product.ReservationEndTime = master.ReservationEndTime;
                    product.MaxOccupancy = master.MaxOccupancy;
                    product.ReservationInterval = master.ReservationInterval;
                    product.ReservationProducts = master.IsReservation ? _productService.GetReservationProductsbyProductId(master.Id) : null;

                    _productService.UpdateProduct(product);

                    ///

                    foreach (var item in product.ProductPictures.ToList())
                    {
                        _productService.DeleteProductPicture(item);
                    }
                    foreach (var item in master.ProductPictures)
                    {
                        var prodPicture = new ProductPicture
                        {
                            DisplayOrder = item.DisplayOrder,
                            PictureId = item.PictureId,
                            ProductId = product.Id
                        };

                        _productService.InsertProductPicture(prodPicture);
                    }

                    ///

                    foreach (var item in product.ProductCategories.ToList())
                    {
                        _categoryService.DeleteProductCategory(item);
                    }
                    foreach (var item in master.ProductCategories)
                    {
                        var prodCategory = new ProductCategory
                        {
                            CategoryId = item.CategoryId,
                            DisplayOrder = item.DisplayOrder,
                            IsFeaturedProduct = item.IsFeaturedProduct,
                            ProductId = product.Id
                        };

                        _categoryService.InsertProductCategory(prodCategory);
                    }

                    ///

                    foreach (var item in product.ProductManufacturers.ToList())
                    {
                        _manufacturerService.DeleteProductManufacturer(item);
                    }
                    foreach (var item in master.ProductManufacturers)
                    {
                        var prodManufacturer = new ProductManufacturer
                        {
                            DisplayOrder = item.DisplayOrder,
                            IsFeaturedProduct = item.IsFeaturedProduct,
                            ManufacturerId = item.ManufacturerId,
                            ProductId = product.Id
                        };

                        _manufacturerService.InsertProductManufacturer(prodManufacturer);
                    }

                    ///

                    foreach (var item in product.ProductSpecificationAttributes.ToList())
                    {
                        _specificationAttributeService.DeleteProductSpecificationAttribute(item);
                    }
                    foreach (var item in master.ProductSpecificationAttributes)
                    {
                        var prodSpecAttribute = new ProductSpecificationAttribute
                        {
                            AllowFiltering = item.AllowFiltering,
                            AttributeTypeId = item.AttributeTypeId,
                            CustomValue = item.CustomValue,
                            DisplayOrder = item.DisplayOrder,
                            ProductId = product.Id,
                            ShowOnProductPage = item.ShowOnProductPage,
                            SpecificationAttributeOptionId = item.SpecificationAttributeOptionId
                        };

                        _specificationAttributeService.InsertProductSpecificationAttribute(item);
                    }

                    ///

                    foreach (var item in product.ProductAttributeMappings.ToList())
                    {
                        _productAttributeService.DeleteProductAttributeMapping(item);
                    }
                    foreach (var item in master.ProductAttributeMappings)
                    {
                        var prodAttribute = new ProductAttributeMapping
                        {
                            AttributeControlTypeId = item.AttributeControlTypeId,
                            ConditionAttributeXml = item.ConditionAttributeXml,
                            DefaultValue = item.DefaultValue,
                            DisplayOrder = item.DisplayOrder,
                            IsRequired = item.IsRequired,
                            ProductAttributeId = item.ProductAttributeId,
                            ProductId = product.Id,
                            TextPrompt = item.TextPrompt,
                            ValidationFileAllowedExtensions = item.ValidationFileAllowedExtensions,
                            ValidationFileMaximumSize = item.ValidationFileMaximumSize,
                            ValidationMaxLength = item.ValidationMaxLength,
                            ValidationMinLength = item.ValidationMinLength
                        };

                        _productAttributeService.InsertProductAttributeMapping(prodAttribute);
                    }

                    ///

                    foreach (var item in product.ProductAttributeCombinations.ToList())
                    {
                        _productAttributeService.DeleteProductAttributeCombination(item);
                    }
                    foreach (var item in master.ProductAttributeCombinations)
                    {
                        var prodAttrCombination = new ProductAttributeCombination
                        {
                            AllowOutOfStockOrders = item.AllowOutOfStockOrders,
                            AttributesXml = item.AttributesXml,
                            Gtin = item.Gtin,
                            ManufacturerPartNumber = item.ManufacturerPartNumber,
                            NotifyAdminForQuantityBelow = item.NotifyAdminForQuantityBelow,
                            OverriddenPrice = item.OverriddenPrice,
                            ProductId = product.Id,
                            Sku = item.Sku,
                            StockQuantity = item.StockQuantity
                        };

                        _productAttributeService.InsertProductAttributeCombination(prodAttrCombination);
                    }

                    ///
                    if (_storeMappingService.GetStoreMappings(product).Any())
                    {
                        var storeMapping = _storeMappingService.GetStoreMappings(product).First();

                        foreach (var item in product.TierPrices.ToList())
                        {
                            _productService.DeleteTierPrice(item);
                        }
                        foreach (var item in master.TierPrices)
                        {
                            var prodTier = new TierPrice
                            {
                                CustomerRoleId = item.CustomerRoleId,
                                Price = item.Price,
                                ProductId = item.ProductId,
                                Quantity = item.Quantity,
                                StoreId = storeMapping.StoreId
                            };

                            _productService.InsertTierPrice(prodTier);
                        }

                        ///
                    }

                    product.AppliedDiscounts.Clear();
                    foreach (var prodDiscount in master.AppliedDiscounts)
                    {
                        product.AppliedDiscounts.Add(prodDiscount);
                    }
                }
            }
        }
        #endregion

        #region NU-55
        public ActionResult StoreProductAddPopup()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var model = new ProductModel.AddCrossSellProductModel();
            //a vendor should have access only to his products
            model.IsLoggedAs = _workContext.CurrentCustomer.IsLoggedAs();

            //categories
            model.AvailableCategories.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            var categories = _categoryService.GetAllCategories(showHidden: true);
            foreach (var c in categories)
                model.AvailableCategories.Add(new SelectListItem { Text = c.GetFormattedBreadCrumb(categories), Value = c.Id.ToString() });

            //manufacturers
            /*model.AvailableManufacturers.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            foreach (var m in _manufacturerService.GetAllManufacturers(showHidden: true))
                model.AvailableManufacturers.Add(new SelectListItem { Text = m.Name, Value = m.Id.ToString() });*/

            //vendors
            model.AvailableVendors.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            foreach (var v in _vendorService.GetAllVendors())
                model.AvailableVendors.Add(new SelectListItem { Text = v.Name, Value = v.Id.ToString() });

            //product types
            model.AvailableProductTypes = ProductType.SimpleProduct.ToSelectList(false).ToList();
            model.AvailableProductTypes.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });

            return View(model);
        }

        [HttpPost]
        public ActionResult StoreProductAddPopupList(DataSourceRequest command, ProductModel.AddCrossSellProductModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                model.SearchVendorId = _workContext.CurrentVendor.Id;
            }

            var products = _productService.SearchProducts(
                categoryIds: new List<int> { model.SearchCategoryId },
                manufacturerId: model.SearchManufacturerId,
                storeId: model.SearchStoreId,
                vendorId: model.SearchVendorId,
                productType: model.SearchProductTypeId > 0 ? (ProductType?)model.SearchProductTypeId : null,
                keywords: model.SearchProductName,
                pageIndex: command.Page - 1,
                pageSize: command.PageSize,
                showHidden: true, overrideHideMasterProducts: false
                );

            var vendorMappings = _vendorService.GetAllVendors();

            var gridModel = new DataSourceResult();
            gridModel.Data = products.Select(x => x.ToModel()).OrderBy(p => p.Name);
            gridModel.Total = products.TotalCount;

            return Json(gridModel);
        }

        [HttpPost]
        [FormValueRequired("save")]
        public ActionResult StoreProductAddPopup(string btnId, string formId, ProductModel.AddCrossSellProductModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            if (model.SelectedProductIds != null)
            {
                foreach (int id in model.SelectedProductIds)
                {
                    var product = _productService.GetProductById(id);
                    if (product != null)
                    {
                        //a vendor should have access only to his products
                        if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                            continue;


                        var newProduct = _copyProductService.CopyProduct(product, product.Name, false, true, true, false);

                        if (newProduct.VendorId > 0)
                        {
                            var vendor = _vendorService.GetVendorById(newProduct.VendorId);

                            var customers = _customerService.GetCustomersByVendor(vendor);

                            foreach (var customer in customers)
                            {
                                bool isMapped = true;

                                var mappings = _storeMappingService.GetStoreMappings(customer);
                                foreach (var mapping in mappings)
                                {
                                    if (mapping.StoreId == _storeContext.CurrentStore.Id)
                                    {
                                        isMapped = false;
                                    }
                                }

                                if (!isMapped)
                                {
                                    _storeMappingService.InsertStoreMapping(customer, _storeContext.CurrentStore.Id);
                                }
                            }
                        }

                        _storeMappingService.InsertStoreMapping(newProduct, _storeContext.CurrentStore.Id);
                    }
                }
            }

            //a vendor should have access only to his products
            model.IsLoggedAs = _workContext.CurrentCustomer.IsLoggedAs();
            ViewBag.RefreshPage = true;
            ViewBag.btnId = btnId;
            ViewBag.formId = formId;
            return View(model);
        }
        #endregion

        [HttpPost]
        public ActionResult SaveProductEditorSettings(ProductModel model, string returnUrl = "")
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //vendors cannot manage these settings
            if (_workContext.CurrentVendor != null)
                return RedirectToAction("List");

            var productEditorSettings = _settingService.LoadSetting<ProductEditorSettings>();
            productEditorSettings = model.ProductEditorSettingsModel.ToEntity(productEditorSettings);
            _settingService.SaveSetting(productEditorSettings);

            //product list
            if (String.IsNullOrEmpty(returnUrl))
                return RedirectToAction("List");
            //prevent open redirection attack
            if (!Url.IsLocalUrl(returnUrl))
                return RedirectToAction("List");
            return Redirect(returnUrl);
        }

    }
}
