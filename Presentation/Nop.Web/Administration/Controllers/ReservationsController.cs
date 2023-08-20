using Nop.Admin.Models.Orders;
using Nop.Admin.Models.Reservations;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Services.Affiliates;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.ExportImport;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.SAP;
using Nop.Services.Security;
using Nop.Services.Shipping;
using Nop.Services.Stores;
using Nop.Services.Tax;
using Nop.Services.Vendors;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using System;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace Nop.Admin.Controllers
{
    public class ReservationsController : BaseAdminController
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
        #endregion

        #region Ctor

        public ReservationsController(IDonationService donationService,
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
            IRepository<OrderItem> orderItemRepository)
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
        }

        #endregion

        public ActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var model = new ReservationListModel();
            model.isReadOnly = _workContext.CurrentCustomer.IsReadOnly();
            model.IsLoggedInAsVendor = _workContext.CurrentVendor != null;
            model.IsLoggedAs = _workContext.CurrentCustomer.IsLoggedAs();
            
            return View(model);
        }

        public ActionResult ReservationsList(DataSourceRequest command, ReservationListModel model)
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

            //Load Reservations
            var reservations = _orderService.SearchReservations(
                                vendorId: model.VendorId,
                                orderId: model.OrderId,
                                createdFromUtc: startDateValue,
                                createdToUtc: endDateValue,
                                billingLastName: model.BillingLastName,
                                productName:model.ProductName,
                                timeSlot: model.TimeSlot,
                                status: model.Status,
                                pageIndex: command.Page - 1,
                                pageSize: command.PageSize);

            var selectedReservations = reservations.ToList();

            if (startDateValue.HasValue)
            {
                selectedReservations = reservations.Where(o => startDateValue.Value <= (DateTime?)_dateTimeHelper.ConvertToUtcTime(o.ReservationDate, _dateTimeHelper.CurrentTimeZone)).ToList();
            }

            if (endDateValue.HasValue)
            {
                selectedReservations = selectedReservations.Where(o => endDateValue.Value >= (DateTime?)_dateTimeHelper.ConvertToUtcTime(o.ReservationDate, _dateTimeHelper.CurrentTimeZone)).ToList();
            }

            if (_storeContext.CurrentStore.Id > 0)
            {
                selectedReservations = selectedReservations.Where(o => _storeContext.CurrentStore.Id == o.StoreId).ToList();
            }

            var gridModel = new DataSourceResult
            {
                Data = selectedReservations.Select(x =>
                {
                    return new Models.Reservations.ReservationGridListModel
                    {
                        OrderId = x.OrderId,
                        ReservedProductId = x.ReservedProductId,
                        ProductId = x.ProductId,
                        ReservationDate = x.ReservationDate.ToShortDateString(),
                        ReservedTimeSlot = x.ReservedTimeSlot,
                        ReservedUnits = x.ReservedUnits,
                        OrderItemId = x.OrderItemId,
                        PaymentStatus = x.PaymentStatus.GetLocalizedEnum(_localizationService, _workContext),
                        PaymentStatusId = x.PaymentStatusId,
                        CustomerId = x.CustomerId,
                        CustomerFullName = x.CustomerFullName,
                        CustomerEmail = x.CustomerEmail,
                        OrderTotal = x.OrderTotal,
                        IsFulfilled = x.IsFulfilled,
                        ProductName = x.ProductName,
                    };
                })
            };
            return new JsonResult
            {
                Data = gridModel
            };
        }

        public ActionResult FulfillOrderItem(int OrderItemId)
        {
            int orderItemId = OrderItemId;
            bool isFulfill = true;
            bool isNotify = true;
            var orderController = DependencyResolver.Current.GetService<OrderController>();
           
            var itemToFulfill = _orderService.GetOrderItemById(orderItemId);
            var order = _orderService.GetOrderById(itemToFulfill.OrderId);
           
            //get order item identifier
            if (itemToFulfill == null)
                throw new ArgumentException("No order item found with the specified id");

            #region NU-38
            decimal unitPriceInclTax = 0m, unitPriceExclTax = 0m,
                    discountInclTax = 0m, discountExclTax = 0m, 
                    priceInclTax = 0m, priceExclTax = 0m;

            int quantity = 0;
            #endregion
            if (!isFulfill) // NU-31
            {
                unitPriceInclTax = itemToFulfill.UnitPriceInclTax;
                unitPriceExclTax = itemToFulfill.UnitPriceExclTax;
                quantity = itemToFulfill.Quantity;
                discountInclTax = itemToFulfill.DiscountAmountInclTax;
                discountExclTax = itemToFulfill.DiscountAmountExclTax;
                priceInclTax = itemToFulfill.PriceInclTax;
                priceExclTax = itemToFulfill.PriceExclTax;
            }
            
            var isCancelled = order.OrderStatus == OrderStatus.Cancelled;
            if (isFulfill)
            {
                itemToFulfill.FulfillmentDateTime = DateTime.Now;
                if (itemToFulfill.Product.VendorId == 0)
                    _orderProcessingService.InsertIntoSAPSalesJournalOrderItem(order, itemToFulfill, 2);
                
                _orderProcessingService.InsertGlCodeCalcs(order, itemToFulfill, isFulfill, false);

                //Setting Shipping status if the product is partially fulfilled or completely fulfilled
                var AllunfulfilledItesmCount = _orderService.GetOrderItemsByOrderID(order.Id).Where(x => x.FulfillmentDateTime == null && x.IsReservation == true).ToList().Count;
                if (AllunfulfilledItesmCount == 0)
                    order.ShippingStatus = ShippingStatus.Delivered;
                else
                    order.ShippingStatus = ShippingStatus.PartiallyShipped;
                _orderService.UpdateOrder(order);
            }
            else if (quantity > 0)	// NU-31
            {
                int qtyDifference = itemToFulfill.Quantity - quantity;

                if (!_orderSettings.AutoUpdateOrderTotalsOnEditingOrder)
                {
                    itemToFulfill.UnitPriceInclTax = unitPriceInclTax;
                    itemToFulfill.UnitPriceExclTax = unitPriceExclTax;
                    itemToFulfill.Quantity = quantity;
                    itemToFulfill.DiscountAmountInclTax = discountInclTax;
                    itemToFulfill.DiscountAmountExclTax = discountExclTax;
                    itemToFulfill.PriceInclTax = priceInclTax;
                    itemToFulfill.PriceExclTax = priceExclTax;
                }
                _orderService.UpdateOrder(order);
                
                //adjust inventory
                _productService.AdjustInventory(itemToFulfill.Product, qtyDifference, itemToFulfill.AttributesXml);
            }
            else
            {
                //adjust inventory
                _productService.AdjustInventory(itemToFulfill.Product, itemToFulfill.Quantity, itemToFulfill.AttributesXml);

                //delete item
                _orderService.DeleteOrderItem(itemToFulfill);
            }
            #region NU-31
            UpdateOrderParameters updateOrderParameters = null;
            if (!isFulfill)
            {
                //update order totals
                updateOrderParameters = new UpdateOrderParameters
                {
                    UpdatedOrder = order,
                    UpdatedOrderItem = itemToFulfill,
                    PriceInclTax = unitPriceInclTax,
                    PriceExclTax = unitPriceExclTax,
                    DiscountAmountInclTax = discountInclTax,
                    DiscountAmountExclTax = discountExclTax,
                    SubTotalInclTax = priceInclTax,
                    SubTotalExclTax = priceExclTax,
                    Quantity = quantity,
                    RequestedFulfillmentDate = DateTime.Now.ToShortDateString()
                };
            }
            else
            {
                updateOrderParameters = new UpdateOrderParameters
                {
                    UpdatedOrder = order,
                    UpdatedOrderItem = itemToFulfill,
                    RequestedFulfillmentDate = DateTime.Now.ToShortDateString(),
                    FulfillmentDate = DateTime.Now.ToShortDateString()
                };
            }
            #endregion
            _orderProcessingService.UpdateOrderTotals(updateOrderParameters);
            //add a note
            order.OrderNotes.Add(new OrderNote
            {
                Note = "Order item has been edited",
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow
            });
            _customerActivityService.InsertActivity("EditOrder", _localizationService.GetResource("ActivityLog.EditOrder"), order.Id);

            var model = new OrderModel();
            if (isFulfill)
            {
                var orderItemList = order.OrderItems.Where(x => x.OrderId == order.Id && x.IsReservation == true).ToList();
                foreach (var item in orderItemList)
                {
                    if (item.FulfillmentDateTime == null)
                    {
                        model.OrderStatus = order.OrderStatus.GetLocalizedEnum(_localizationService, _workContext);
                        model.OrderStatusId = order.OrderStatusId;
                        break;
                    }
                    else
                    {
                        model.OrderStatusId = 30;
                    }
                }

                #region Order ststus update
                order.OrderStatusId = model.OrderStatusId;
                _orderService.UpdateOrder(order);

                //add a note
                order.OrderNotes.Add(new OrderNote
                {
                    Note = string.Format("Order status has been edited. New status: {0}", order.OrderStatus.GetLocalizedEnum(_localizationService, _workContext)),
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow
                });
                _orderService.UpdateOrder(order);

                _customerActivityService.InsertActivity("EditOrder", _localizationService.GetResource("ActivityLog.EditOrder"), order.Id);

                if (order.OrderStatusId == 30)
                {
                    orderController. SaveIntoSapJournal(order, 2);
                }
                else
                {
                    if (order.OrderStatusId == 40)
                    {
                        orderController.SaveIntoSapJournal(order, 3);
                    }
                }

                #endregion


                if (isNotify)
               {
                    //send email notification
                    int queuedEmailId = _workflowMessageService.SendFulfillementCustomerNotification(itemToFulfill, order.CustomerLanguageId);
                    if (queuedEmailId > 0)
                    {
                        order.OrderNotes.Add(new OrderNote
                        {
                            Note = string.Format("\"Fulfillement\" email (to customer) has been queued. Queued email identifier: {0}.", queuedEmailId),
                            DisplayToCustomer = false,
                            CreatedOnUtc = DateTime.UtcNow
                        });

                    }
               }

             _orderProcessingService.CheckOrderStatus(order);
            }

            if (isFulfill)
            {
                if (itemToFulfill.Product.VendorId == 0)
                    _orderProcessingService.ApplyfulfilledProductsJEGlCodeBusinessRules(itemToFulfill, order);
            }
            
            return RedirectToAction("List");
        }

        [HttpPost, ActionName("List")]
        [FormValueRequired("exportexcel-all")]
        public ActionResult ExportExcelAll(ReservationListModel model)
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

            //Load Reservations
            var reservations = _orderService.SearchReservations(
                                vendorId: model.VendorId,
                                orderId: model.OrderId,
                                createdFromUtc: startDateValue,
                                createdToUtc: endDateValue,
                                billingLastName: model.BillingLastName,
                                productName: model.ProductName,
                                timeSlot: model.TimeSlot,
                                status: model.Status);

            var selectedReservations = reservations.ToList();

            if (startDateValue.HasValue)
            {
                selectedReservations = reservations.Where(o => startDateValue.Value <= (DateTime?)_dateTimeHelper.ConvertToUtcTime(o.ReservationDate, _dateTimeHelper.CurrentTimeZone)).ToList();
            }

            if (endDateValue.HasValue)
            {
                selectedReservations = selectedReservations.Where(o => endDateValue.Value >= (DateTime?)_dateTimeHelper.ConvertToUtcTime(o.ReservationDate, _dateTimeHelper.CurrentTimeZone)).ToList();
            }

            if (_storeContext.CurrentStore.Id > 0)
            {
                selectedReservations = selectedReservations.Where(o => _storeContext.CurrentStore.Id == o.StoreId).ToList();
            }
            var gridModel = selectedReservations.Select(x =>
                {
                    return new Core.Domain.Orders.ReservationGridListModel
                    {
                        OrderId = x.OrderId,
                        ReservedProductId = x.ReservedProductId,
                        ProductId = x.ProductId,
                        ReservationDate = x.ReservationDate.ToShortDateString(),
                        ReservedTimeSlot = x.ReservedTimeSlot,
                        ReservedUnits = x.ReservedUnits,
                        OrderItemId = x.OrderItemId,
                        PaymentStatus = x.PaymentStatus.GetLocalizedEnum(_localizationService, _workContext),
                        PaymentStatusId = x.PaymentStatusId,
                        CustomerId = x.CustomerId,
                        CustomerFullName = x.CustomerFullName,
                        CustomerEmail = x.CustomerEmail,
                        OrderTotal = x.OrderTotal,
                        IsFulfilled = x.IsFulfilled,
                        ProductName = x.ProductName,
                    };
                });

            var dataToImport = gridModel.ToList();

            try
            {
                byte[] bytes = _exportManager.ReservationReportsToXlsx(dataToImport);
                return File(bytes, MimeTypes.TextXlsx, "ReservationReports.xlsx");
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("List");
            }
        }
    }
}