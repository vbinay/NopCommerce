using Nop.Services.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Nop.Core.Data;
using Nop.Services.Catalog;
using Nop.Services.Tax;
using Nop.Services.Helpers;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Core;
using Nop.Services.Directory;
using Nop.Services.Security;
using Nop.Services.Payments;
using Nop.Services.Common;
using Nop.Services.ExportImport;
using Nop.Services.Messages;
using Nop.Services.Media;
using Nop.Services.Shipping;
using Nop.Services.Stores;
using Nop.Services.Vendors;
using Nop.Services.Affiliates;
using Nop.Services.Logging;
using Nop.Core.Caching;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Tax;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Shipping;
using Nop.Services.SAP;
using Nop.Admin.Models.KitchenProduction;
using Nop.Web.Framework.Kendoui;
using Nop.Web.Framework.Controllers;
using Nop.Core.Domain.KitchenProduction;
using Nop.Web.Framework.Mvc;
using Nop.Admin.Helpers;
using System.Data;
using System.ComponentModel;
using ClosedXML.Excel;
using System.IO;

namespace Nop.Admin.Controllers
{
    public class KitchenProductionController : BaseAdminController
    {
        #region Fields

        private readonly IDonationService _donationService;
        private readonly IOrderService _orderService;
        private readonly IOrderReportService _orderReportService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IReturnRequestService _returnRequestService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly ITaxService _taxService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IDiscountService _discountService;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
        private readonly ICurrencyService _currencyService;
        private readonly IEncryptionService _encryptionService;
        private readonly IPaymentService _paymentService;
        private readonly IMeasureService _measureService;
        private readonly IPdfService _pdfService;
        private readonly IAddressService _addressService;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IProductService _productService;
        private readonly IExportManager _exportManager;
        private readonly IPermissionService _permissionService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly ICategoryService _categoryService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IProductAttributeFormatter _productAttributeFormatter;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IGiftCardService _giftCardService;
        private readonly IDownloadService _downloadService;
        private readonly IShipmentService _shipmentService;
        private readonly IShippingService _shippingService;
        private readonly IStoreService _storeService;
        private readonly IVendorService _vendorService;
        private readonly IAddressAttributeParser _addressAttributeParser;
        private readonly IAddressAttributeService _addressAttributeService;
        private readonly IAddressAttributeFormatter _addressAttributeFormatter;
        private readonly IAffiliateService _affiliateService;
        private readonly IPictureService _pictureService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ICacheManager _cacheManager;

        private readonly OrderSettings _orderSettings;
        private readonly CurrencySettings _currencySettings;
        private readonly TaxSettings _taxSettings;
        private readonly MeasureSettings _measureSettings;
        private readonly AddressSettings _addressSettings;
        private readonly ShippingSettings _shippingSettings;

        private readonly IReportingService _reportingService;//NU-90           

        private readonly IStoreContext _storeContext;

        private readonly ISAPService _SAPService;
        private readonly HttpContextBase _httpContext;
        private readonly IGlCodeService _glCodeService;
        private readonly ITaxCategoryService _taxCategoryService;
        private readonly IRepository<OrderItem> _orderItemRepository;
        private readonly IRepository<Warehouse> _warehouseRepository;
        #endregion

        #region Ctor

        public KitchenProductionController(IDonationService donationService,
            IOrderService orderService,
            IOrderReportService orderReportService,
            IOrderProcessingService orderProcessingService,
            IReturnRequestService returnRequestService,
            IPriceCalculationService priceCalculationService,
            ITaxService taxService,
            IDateTimeHelper dateTimeHelper,
            IPriceFormatter priceFormatter,
            IDiscountService discountService,
            ILocalizationService localizationService,
            IWorkContext workContext,
            ICurrencyService currencyService,
            IEncryptionService encryptionService,
            IPaymentService paymentService,
            IMeasureService measureService,
            IPdfService pdfService,
            IAddressService addressService,
            ICountryService countryService,
            IStateProvinceService stateProvinceService,
            IProductService productService,
            IExportManager exportManager,
            IPermissionService permissionService,
            IWorkflowMessageService workflowMessageService,
            ICategoryService categoryService,
            IManufacturerService manufacturerService,
            IProductAttributeService productAttributeService,
            IProductAttributeParser productAttributeParser,
            IProductAttributeFormatter productAttributeFormatter,
            IShoppingCartService shoppingCartService,
            IGiftCardService giftCardService,
            IDownloadService downloadService,
            IShipmentService shipmentService,
            IShippingService shippingService,
            IStoreService storeService,
            IVendorService vendorService,
            IAddressAttributeParser addressAttributeParser,
            IAddressAttributeService addressAttributeService,
            IAddressAttributeFormatter addressAttributeFormatter,
            IAffiliateService affiliateService,
            IPictureService pictureService,
            ICustomerActivityService customerActivityService,
            ICacheManager cacheManager,
            OrderSettings orderSettings,
            CurrencySettings currencySettings,
            TaxSettings taxSettings,
            MeasureSettings measureSettings,
            AddressSettings addressSettings,
            ShippingSettings shippingSettings,
            IStoreContext storeContext,
            ISAPService SAPService,
            IReportingService reportingService,
            HttpContextBase httpContext,
            IGlCodeService glCodeService,
            ITaxCategoryService taxCategoryService,
            IRepository<OrderItem> orderItemRepository,
            IRepository<Warehouse> warehouseRepository)
        {
            this._donationService = donationService;
            this._orderService = orderService;
            this._orderReportService = orderReportService;
            this._orderProcessingService = orderProcessingService;
            this._returnRequestService = returnRequestService;
            this._priceCalculationService = priceCalculationService;
            this._taxService = taxService;
            this._dateTimeHelper = dateTimeHelper;
            this._priceFormatter = priceFormatter;
            this._discountService = discountService;
            this._localizationService = localizationService;
            this._workContext = workContext;
            this._currencyService = currencyService;
            this._encryptionService = encryptionService;
            this._paymentService = paymentService;
            this._measureService = measureService;
            this._pdfService = pdfService;
            this._addressService = addressService;
            this._countryService = countryService;
            this._stateProvinceService = stateProvinceService;
            this._productService = productService;
            this._exportManager = exportManager;
            this._permissionService = permissionService;
            this._workflowMessageService = workflowMessageService;
            this._categoryService = categoryService;
            this._manufacturerService = manufacturerService;
            this._productAttributeService = productAttributeService;
            this._productAttributeParser = productAttributeParser;
            this._productAttributeFormatter = productAttributeFormatter;
            this._shoppingCartService = shoppingCartService;
            this._giftCardService = giftCardService;
            this._downloadService = downloadService;
            this._shipmentService = shipmentService;
            this._shippingService = shippingService;
            this._storeService = storeService;
            this._vendorService = vendorService;
            this._addressAttributeParser = addressAttributeParser;
            this._addressAttributeService = addressAttributeService;
            this._addressAttributeFormatter = addressAttributeFormatter;
            this._affiliateService = affiliateService;
            this._pictureService = pictureService;
            this._customerActivityService = customerActivityService;
            this._cacheManager = cacheManager;
            this._orderSettings = orderSettings;
            this._currencySettings = currencySettings;
            this._taxSettings = taxSettings;
            this._measureSettings = measureSettings;
            this._addressSettings = addressSettings;
            this._shippingSettings = shippingSettings;
            this._storeContext = storeContext;
            this._SAPService = SAPService;
            this._reportingService = reportingService;
            this._httpContext = httpContext;
            this._glCodeService = glCodeService;
            this._taxCategoryService = taxCategoryService;
            this._orderItemRepository = orderItemRepository;
            this._warehouseRepository = warehouseRepository;
        }

        #endregion
        public ActionResult List([ModelBinder(typeof(CommaSeparatedModelBinder))] List<string> categoriesIds = null)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var model = new KitchenProductionSearchDataModel();

            model.AvailablePaymentMethods.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "" });
            foreach (var pm in _paymentService.LoadAllPaymentMethods()
                .OrderBy(p => p.PluginDescriptor.FriendlyName)) /// NU-34
                model.AvailablePaymentMethods.Add(new SelectListItem { Text = pm.PluginDescriptor.FriendlyName, Value = pm.PluginDescriptor.SystemName });


            var allCategories = SelectListHelper.GetCategoryList(_categoryService, _cacheManager, true);
            foreach (var c in allCategories)
                model.AvailableCategories.Add(c);

            model.IsLoggedInAsVendor = _workContext.CurrentVendor != null;
            return View(model);
        }

        public ActionResult KitchenProdcutionList(DataSourceRequest command, KitchenProductionSearchDataModel model)
        {

            try
            {
                if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                    return AccessDeniedView();

                //A vendor should have access only to his products
                if (_workContext.CurrentVendor != null)
                {
                    model.VendorId = _workContext.CurrentVendor.Id;
                }

                DateTime? startDateValue = (model.StartDate == null) ? null
                                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.StartDate.Value, _dateTimeHelper.CurrentTimeZone);

                DateTime? endDateValue = (model.EndDate == null) ? null
                                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.EndDate.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);

                var filterByProductId = 0;
                //var product = _productService.GetProductById(model.ProductId);
                //if (product != null && HasAccessToProduct(product))
                //    filterByProductId = model.ProductId;

                int storeID = model.VendorId != 0 ? 0 : _storeContext.CurrentStore.Id;
                var categoriesID = !model.CategoriesIds.Contains(0) ? model.CategoriesIds : null;

                var orders = _orderService.SearchOrders(storeId: storeID, /// NU-36
                vendorId: model.VendorId,
                   productId: filterByProductId,
                   warehouseId: 0,
                   paymentMethodSystemName: model.PaymentMethodSystemName,
                   createdFromUtc: startDateValue,
                   createdToUtc: endDateValue,
                   osIds: null,
                   psIds: null,
                   ssIds: null,
                   billingEmail: null,
                   billingLastName: model.BillingLastName,
                   billingCountryId: 0,
                   onlyUnfulfilled: false,
                   orderNotes: null,
                   pageIndex: command.Page - 1,
                   pageSize: command.PageSize, 
                   orderid: Convert.ToInt32(model.OrderId));

                List<KitchenProductionListDataModel> kitchenProductionDataList = new List<KitchenProductionListDataModel>();

                foreach (var order in orders)
                {
                    foreach (var items in order.OrderItems)
                    {
                        string categories = string.Empty;

                        if (items.Product.IsMealPlan || items.Product.IsDonation || items.Product.IsGiftCard || items.Product.IsDownload || items.Product.IsReservation)
                        {
                            continue;
                        }
                        string warehouse = string.Empty;
                        if (items.SelectedFulfillmentWarehouseId != null)
                        {
                            var warehouseDetails = _warehouseRepository.GetById(items.SelectedFulfillmentWarehouseId);

                            if (warehouseDetails != null)
                                warehouse = _warehouseRepository.GetById(items.SelectedFulfillmentWarehouseId).Name == null ? "N/A" : _warehouseRepository.GetById(items.SelectedFulfillmentWarehouseId).Name;

                            else
                                warehouse = "N/A";
                        }
                        else
                        {
                            warehouse = "N/A";
                        }
                        foreach (var cat in items.Product.ProductCategories)
                        {
                            categories = categories + (cat.Category.Name + ",");
                        }
                        KitchenProductionListDataModel dataModel = new KitchenProductionListDataModel
                        {
                            orderid = items.OrderId,
                            orderItemid = items.Id,
                            attributes = items.AttributeDescription.Replace("<br />", Environment.NewLine),
                            pickupDate = items.RequestedFulfillmentDateTime,
                            Categories = categories.TrimEnd(','),
                            pickupLocation = warehouse,
                            productName = items.Product.Name,
                            //createdOnDate = _dateTimeHelper.ConvertToUserTime(items.Order.CreatedOnUtc, DateTimeKind.Utc),
                            createdOnDate = items.Order.CreatedOnUtc,
                            email = items.Order.ShippingAddress == null ? items.Order.BillingAddress.Email : items.Order.ShippingAddress.Email,
                            phone = items.Order.ShippingAddress == null ? items.Order.BillingAddress.PhoneNumber : items.Order.ShippingAddress.PhoneNumber,
                            userName = items.Order.ShippingAddress == null ? (items.Order.BillingAddress.FirstName + " " + items.Order.BillingAddress.LastName) : (items.Order.ShippingAddress.FirstName + " " + items.Order.ShippingAddress.LastName)

                        };
                        kitchenProductionDataList.Add(dataModel);
                    }
                }

                var gridModel = new DataSourceResult
                {
                    Data = kitchenProductionDataList
                };

                return new JsonResult
                {
                    Data = gridModel
                };
            }
            catch (Exception ex)
            {

                throw;
            }

        }


        [HttpPost, ActionName("List")]
        [FormValueRequired("production-report-all")]
        public ActionResult ExportExcelAll(KitchenProductionSearchDataModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            //a vendor cannot export orders
            if (_workContext.CurrentVendor != null)
                return AccessDeniedView();

            DateTime? startDateValue = (model.StartDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.StartDate.Value, _dateTimeHelper.CurrentTimeZone);

            DateTime? endDateValue = (model.EndDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.EndDate.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);

            var filterByProductId = 0;
            int storeID = model.VendorId != 0 ? 0 : _storeContext.CurrentStore.Id;

            var orders = _orderService.SearchOrders(storeId: storeID, /// NU-36
               vendorId: model.VendorId,
               productId: filterByProductId,
               warehouseId: 0,
               paymentMethodSystemName: model.PaymentMethodSystemName,
               createdFromUtc: startDateValue,
               createdToUtc: endDateValue,
               osIds: null,
               psIds: null,
               ssIds: null,
               billingEmail: null,
               billingLastName: model.BillingLastName,
               billingCountryId: 0,
               orderNotes: null, orderid: Convert.ToInt32(model.OrderId));

            List<KitchenProduction> kitchenProductionDataList = new List<KitchenProduction>();
            string[] separator = { "<br />" };
            foreach (var order in orders)
            {
                foreach (var items in order.OrderItems)
                {
                    string categories = string.Empty;
                    if (items.Product.IsMealPlan || items.Product.IsDonation || items.Product.IsGiftCard || items.Product.IsDownload || items.Product.IsReservation)
                    {
                        continue;
                    }
                    string warehouse = string.Empty;
                    if (items.SelectedFulfillmentWarehouseId != null)
                    {
                        var warehouseDetails = _warehouseRepository.GetById(items.SelectedFulfillmentWarehouseId);

                        if (warehouseDetails != null)
                            warehouse = _warehouseRepository.GetById(items.SelectedFulfillmentWarehouseId).Name == null ? "N/A" : _warehouseRepository.GetById(items.SelectedFulfillmentWarehouseId).Name;

                        else
                            warehouse = "N/A";
                    }
                    else
                    {
                        warehouse = "N/A";
                    }
                    foreach (var cat in items.Product.ProductCategories)
                    {
                        categories = categories + (cat.Category.Name + ",");
                    }
                    KitchenProduction dataModel = new KitchenProduction
                    {
                        orderid = items.OrderId,
                        orderItemid = items.Id,
                        Categories = categories.TrimEnd(','),
                        attributes = items.AttributeDescription,
                        attributesarr = items.AttributeDescription.Split(separator, System.StringSplitOptions.RemoveEmptyEntries),
                        pickupDate = items.RequestedFulfillmentDateTime,
                        pickupLocation = warehouse,
                        productName = items.Product.Name,
                        createdOnDate = items.Order.CreatedOnUtc,
                        email = items.Order.ShippingAddress == null ? items.Order.BillingAddress.Email : items.Order.ShippingAddress.Email,
                        phone = items.Order.ShippingAddress == null ? items.Order.BillingAddress.PhoneNumber : items.Order.ShippingAddress.PhoneNumber,
                        userName = items.Order.ShippingAddress == null ? (items.Order.BillingAddress.FirstName + " " + items.Order.BillingAddress.LastName) : (items.Order.ShippingAddress.FirstName + " " + items.Order.ShippingAddress.LastName),
                        maxAttributes = items.AttributeDescription.Split(separator, System.StringSplitOptions.RemoveEmptyEntries).Count()
                    };
                    kitchenProductionDataList.Add(dataModel);
                }
            }

            var tableToExport = _exportManager.ToDataTable(kitchenProductionDataList, "KitchenProduction");

            try
            {
                using (XLWorkbook wb = new XLWorkbook())
                {
                    wb.Worksheets.Add(tableToExport);
                    using (MemoryStream stream = new MemoryStream())
                    {
                        wb.SaveAs(stream);
                        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "KitchenProductionReport-" + DateTime.Now + ".xlsx");
                    }
                }
                //byte[] bytes = _exportManager.ExportKitchenProdutionOrdersToXlsx(kitchenProductionDataList);
                //return File(bytes, MimeTypes.TextXlsx, "KitchenProduction.xlsx");
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("List");
            }
        }


    }
}