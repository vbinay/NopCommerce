using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Services.Events;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Tax;
using Nop.Core.Domain.Stores;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Directory;
using Nop.Services.Tax;

namespace Nop.Services.Orders
{
    /// <summary>
    /// Order service
    /// </summary>
    public partial class ReportingService : IReportingService
    {
        #region Fields

        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<OrderItem> _orderItemRepository;
        private readonly IRepository<OrderNote> _orderNoteRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<RecurringPayment> _recurringPaymentRepository;
        private readonly IRepository<Customer> _customerRepository;
        private readonly IEventPublisher _eventPublisher;



        private readonly IRepository<ProductCategory> _productCategoryMapping;
        private readonly IRepository<Category> _categoryRepository;
        private readonly IRepository<GiftCardUsageHistory> _giftCardUsageHistory;
        private readonly IRepository<MealPlan> _mealPlanRepository;
        //private readonly IRepository<CardVaultSubmission> _cardValutSubmission;
        private readonly IRepository<GlCode> _glCode;
        private readonly IRepository<Store> _storeRepository;
        private readonly IRepository<ProductGlCode> _productGlRepository;
        private readonly IRepository<GenericAttribute> _genericRepository;
        private readonly IRepository<ProductGlCodeCalculation> _productGlCodeCalculationRepository;
        private readonly IRepository<StateProvince> _stateProvinceRepository;
        private readonly IRepository<Address> _addressRepository;
        private readonly IOrderService _orderService;
        private readonly IGlCodeService _glCodeService;
        private readonly IRepository<Warehouse> _warehouseRepository;

        /// <summary>
        /// Working report item
        /// </summary>
        public OrderExcelExport UpdateDetailsWithAmountAndGlFromIncomingRequest;
        public Dictionary<int, Store> StoreCache = new Dictionary<int, Store>();

        public Store GetStore(int storeId)
        {
            if (StoreCache.Keys.Contains(storeId))
                return StoreCache[storeId];
            else
            {
                Store storeValue = _storeRepository.GetById(storeId);
                StoreCache.Add(storeId, storeValue);
                return storeValue;
            }
        }
        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="orderRepository">Order repository</param>
        /// <param name="orderItemRepository">Order item repository</param>
        /// <param name="orderNoteRepository">Order note repository</param>
        /// <param name="productRepository">Product repository</param>
        /// <param name="recurringPaymentRepository">Recurring payment repository</param>
        /// <param name="customerRepository">Customer repository</param>
        /// <param name="eventPublisher">Event published</param>
        public ReportingService(IRepository<Order> orderRepository,
            IRepository<OrderItem> orderItemRepository,
            IRepository<OrderNote> orderNoteRepository,
            IRepository<Product> productRepository,
            IRepository<RecurringPayment> recurringPaymentRepository,
            IRepository<Customer> customerRepository,
            IRepository<ProductCategory> productCategoryMapping,
            IRepository<Category> categoryRepository,
            IRepository<GiftCardUsageHistory> giftCardUsageHistory,
            IRepository<MealPlan> mealPlanRepository,
            //private readonly IRepository<CardVaultSubmission> _cardValutSubmission,
            IRepository<GlCode> glCode,
            IRepository<Store> storeRepository,
             IRepository<ProductGlCode> productGlRepository,
            IRepository<GenericAttribute> genericRepository,
            IEventPublisher eventPublisher,
            IRepository<ProductGlCodeCalculation> productGlCodeCalculationRepository,
            IRepository<StateProvince> stateProvinceRepository,
            IRepository<Address> addressRepository,
            IOrderService orderService,
            IGlCodeService glCodeService,
            IRepository<Warehouse> warehouseRepository)
        {
            this._orderRepository = orderRepository;
            this._orderItemRepository = orderItemRepository;
            this._orderNoteRepository = orderNoteRepository;
            this._productRepository = productRepository;
            this._recurringPaymentRepository = recurringPaymentRepository;
            this._customerRepository = customerRepository;
            this._eventPublisher = eventPublisher;
            this._productCategoryMapping = productCategoryMapping;
            this._categoryRepository = categoryRepository;
            this._giftCardUsageHistory = giftCardUsageHistory;
            this._mealPlanRepository = mealPlanRepository;
            this._glCode = glCode;
            this._storeRepository = storeRepository;
            this._productGlRepository = productGlRepository;
            this._genericRepository = genericRepository;
            this._productGlCodeCalculationRepository = productGlCodeCalculationRepository;
            this._stateProvinceRepository = stateProvinceRepository;
            this._addressRepository = addressRepository;
            this._orderService = orderService;
            this._glCodeService = glCodeService;
            this._warehouseRepository = warehouseRepository;
        }

        #endregion

        #region Methods



        /// <summary>
        /// Search orders
        /// </summary>
        /// <param name="sourceSiteId">Id of the SourceSite</param>
        /// <param name="startTime">Order start time; null to load all orders</param>
        /// <param name="endTime">Order end time; null to load all orders</param>
        /// <param name="os">Order status; null to load all orders</param>
        /// <param name="ps">Order payment status; null to load all orders</param>
        /// <param name="ss">Order shippment status; null to load all orders</param>
        /// <param name="billingEmail">Billing email. Leave empty to load all records.</param>
        /// <param name="orderGuid">Search by order GUID (Global unique identifier) or part of GUID. Leave empty to load all orders.</param>
        /// <param name="sortOrder">The display order in which to sort from</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="convertToTimeZone">Page size</param>
        /// <returns>Order collection</returns>
        public virtual IList<OrderExcelExport> PaymentsForExcel(int storeId, DateTime? startTime, DateTime? endTime, int pageIndex, int pageSize, TimeZoneInfo convertToTimeZone)
        {

            startTime = startTime.HasValue ? startTime : new DateTime?(DateTime.UtcNow.Date);
            endTime = endTime.HasValue ? endTime : new DateTime?(DateTime.UtcNow.Date.AddDays(1.0));


            var query1 = _orderRepository.Table.Where(o => o.StoreId == storeId);
            if (startTime.HasValue)
                query1 = query1.Where(o => startTime.Value <= o.PaidDateUtc);
            if (endTime.HasValue)
                query1 = query1.Where(o => endTime.Value >= o.PaidDateUtc);
            query1 = query1.Where(o => !o.Deleted);
            query1 = query1.OrderByDescending(o => o.PaidDateUtc);
            var query = (from o in query1
                         join osp in this._orderItemRepository.Table on o.Id equals osp.OrderId
                         join p in this._productRepository.Table on osp.ProductId equals p.Id
                         //    join p2 in this._productRepository.Table on p.MasterId equals p2.Id
                         join pcm in this._productCategoryMapping.Table on p.Id equals pcm.ProductId
                         join c in this._categoryRepository.Table on pcm.CategoryId equals c.Id
                         join pgl in this._productGlRepository.Table on p.Id equals pgl.ProductId
                         join gl in this._glCode.Table on pgl.GlCodeId equals gl.Id
                         join mp in this._mealPlanRepository.Table on osp.Id equals mp.PurchasedWithOrderItemId into m
                         join ga in this._genericRepository.Table on o.Id equals ga.EntityId into gas

                         from ga in gas.DefaultIfEmpty()
                         from mealplan in m.DefaultIfEmpty()

                         where gl.IsDelivered == (osp.FulfillmentDateTime == null ? false : true)
                         //where ga.KeyGroup == "Order"
                         //where ga.Key == "BitShift.Payments.FirstData.TransactionCardType"




                         select new OrderExcelExport()
                         {
                             OrderId = o.Id,
                             OrderItemId = osp.Id,
                             OrderGuid = o.OrderGuid,
                             CustomerId = o.CustomerId,
                             LocalProductName = p.Name,
                             Category = c.Name,
                             CategoryId = c.Id,
                             GLCode = gl.GlCodeName,
                             GLCodeId = gl.Id,
                             OrderSubtotalInclTax = o.OrderSubtotalInclTax,
                             OrderSubtotalExclTax = o.OrderSubtotalExclTax,
                             SiteProductPriceExclTax = decimal.Round(osp.PriceExclTax, 2),
                             OrderSubTotalDiscountInclTax = o.OrderSubTotalDiscountInclTax,
                             OrderSubTotalDiscountExclTax = o.OrderSubTotalDiscountExclTax,
                             OrderShippingInclTax = o.OrderShippingInclTax,
                             OrderShippingExclTax = o.OrderShippingExclTax,
                             PaymentMethodAdditionalFeeInclTax = o.PaymentMethodAdditionalFeeInclTax,
                             PaymentMethodAdditionalFeeExclTax = o.PaymentMethodAdditionalFeeExclTax,
                             OrderTax = o.OrderTax,
                             SiteProductTax = decimal.Round(osp.PriceInclTax, 2) - decimal.Round(osp.PriceExclTax, 2),
                             OrderTotal = o.OrderTotal,
                             SiteProductTotal = decimal.Round(osp.PriceInclTax, 2),
                             //GiftCardCredit = giftcard.UsedValue,
                             RefundedAmount = o.RefundedAmount,
                             OrderDiscount = o.OrderDiscount,
                             CurrencyRate = o.CurrencyRate,
                             CustomerCurrencyCode = o.CustomerCurrencyCode,

                             AffiliateId = o.AffiliateId,
                             OrderStatusId = o.OrderStatusId,
                             CardType = ga.Value,
                             PaymentMethodSystemName = o.PaymentMethodSystemName,

                             DeliveryDateUtc = osp.FulfillmentDateTime,
                             RequestedFulfillmentDate = osp.RequestedFulfillmentDateTime,
                             //DeliveryAmountExclTax = osp.Deliver,
                             //ShippingMethod = o.ShippingMethod,
                             //ShippingRateComputationMethodSystemName =
                             //o.ShippingRateComputationMethodSystemName,
                             CreatedOnUtc = o.CreatedOnUtc,
                             AccountNumber = mealplan.RecipientAcctNum
                         }
                        ).Distinct();
            var query2 = from c in query
                         orderby c.GLCode, c.CreatedOnUtc
                         select c;
            var orders = query.ToList();

            return orders.ToList();
        }


        public virtual IList<GlCode> GetGlCodesForReport(bool isDelivered)
        {
            var query = from c in _glCode.Table
                        where c.IsDelivered == isDelivered
                        select c;
            var glcodes = query.ToList();
            return glcodes;
        }


        public virtual IList<OrderExcelExport> FutureDeliveriesForExcel(int sourceSiteId, DateTime? startTime, DateTime? endTime, int pageIndex, int pageSize, bool getFutureDeliveries)
        {

            startTime = startTime.HasValue ? startTime : new DateTime?(DateTime.UtcNow.Date);

            var query1 = _orderRepository.Table.Where(o => o.StoreId == sourceSiteId);

            //get orders already delivered within the start and end date

            query1 = query1.Where(o => !o.Deleted);
            query1 = query1.OrderByDescending(o => o.CreatedOnUtc);
            var query = (from o in query1
                         join osp in this._orderItemRepository.Table on o.Id equals osp.OrderId
                         join p in this._productRepository.Table on osp.ProductId equals p.Id
                         //    join p2 in this._productRepository.Table on p.MasterId equals p2.Id
                         join pcm in this._productCategoryMapping.Table on p.Id equals pcm.ProductId
                         join c in this._categoryRepository.Table on pcm.CategoryId equals c.Id
                         join pgl in this._productGlRepository.Table on p.Id equals pgl.ProductId
                         join gl in this._glCode.Table on pgl.GlCodeId equals gl.Id
                         where osp.FulfillmentDateTime == null && osp.RequestedFulfillmentDateTime != null
                         && c.ParentCategoryId == 0 && !gl.IsDelivered
                         select new OrderExcelExport()
                         {
                             OrderId = o.Id,
                             OrderItemId = osp.Id,
                             OrderGuid = o.OrderGuid,
                             CustomerId = o.CustomerId,
                             ProductName = p.Name,
                             CardType = o.CardType,
                             Category = c.Name,
                             CategoryId = c.Id,
                             GLCode = gl.GlCodeName,
                             GLCodeId = gl.Id,
                             OrderSubtotalInclTax = o.OrderSubtotalInclTax,
                             OrderSubtotalExclTax = o.OrderSubtotalExclTax,
                             SiteProductPriceExclTax = decimal.Round(osp.PriceExclTax, 2),
                             OrderSubTotalDiscountInclTax = o.OrderSubTotalDiscountInclTax,
                             OrderSubTotalDiscountExclTax = o.OrderSubTotalDiscountExclTax,
                             OrderShippingInclTax = o.OrderShippingInclTax,
                             OrderShippingExclTax = o.OrderShippingExclTax,
                             PaymentMethodAdditionalFeeInclTax = o.PaymentMethodAdditionalFeeInclTax,
                             PaymentMethodAdditionalFeeExclTax = o.PaymentMethodAdditionalFeeExclTax,
                             OrderTax = o.OrderTax,
                             SiteProductTax = decimal.Round(osp.PriceInclTax, 2) - decimal.Round(osp.PriceExclTax, 2),
                             OrderTotal = o.OrderTotal,
                             SiteProductTotal = decimal.Round(osp.PriceInclTax, 2),
                             RefundedAmount = o.RefundedAmount,
                             OrderDiscount = o.OrderDiscount,
                             CurrencyRate = o.CurrencyRate,
                             CustomerCurrencyCode = o.CustomerCurrencyCode,
                             //OrderWeight = o.OrderWeight,
                             AffiliateId = o.AffiliateId,
                             OrderStatusId = o.OrderStatusId,
                             PaymentMethodSystemName = o.PaymentMethodSystemName,
                             //PurchaseOrderNumber = o.PurchaseOrderNumber,
                             DeliveryDateUtc = getFutureDeliveries ? osp.RequestedFulfillmentDateTime : osp.FulfillmentDateTime,
                             DeliveryAmountExclTax = osp.DeliveryAmountExclTax,
                             //       DeliveryAmountInclTax = o.OrderShippingInclTax,

                             //ShippingMethod = o.ShippingMethod,
                             //ShippingRateComputationMethodSystemName =
                             //o.ShippingRateComputationMethodSystemName,
                             CreatedOnUtc = o.CreatedOnUtc
                         }
                         ).Distinct();
            var query2 = from c in query
                         orderby c.Category, c.OrderId
                         select c;
            var orders = query2.ToList();
            return orders;
        }



        public virtual IList<OrderExcelExport> DeliveriesForExcel(int sourceSiteId, DateTime? startTime, DateTime? endTime, int pageIndex, int pageSize, bool getFutureDeliveries)
        {
            startTime = startTime.HasValue ? startTime : new DateTime?(DateTime.UtcNow.Date);
            endTime = endTime.HasValue ? endTime : new DateTime?(DateTime.UtcNow.Date.AddDays(1.0).AddTicks(-1L));

            var query1 = _orderRepository.Table.Where(o => o.StoreId == sourceSiteId);

            //get orders already delivered within the start and end date

            //       query1 =
            //           query1.Where(
            //               o =>
            //               (
            //                o.OrderStatusId == 30)); //|| ( o.OrderStatusId == 20));



            //or orders scheduled for delivery (future deliveries) within the start and end dates provided
            /* query1 =
                 query1.Where(
                     o =>
                     (( o.OrderStatusId == 20));
             */


            query1 = query1.Where(o => !o.Deleted);
            query1 = query1.OrderByDescending(o => o.CreatedOnUtc);
            var query = (from o in query1
                         join osp in this._orderItemRepository.Table on o.Id equals osp.OrderId
                         join p in this._productRepository.Table on osp.ProductId equals p.Id
                         //    join p2 in this._productRepository.Table on p.MasterId equals p2.Id
                         join pcm in this._productCategoryMapping.Table on p.Id equals pcm.ProductId
                         join c in this._categoryRepository.Table on pcm.CategoryId equals c.Id
                         join pgl in this._productGlRepository.Table on p.Id equals pgl.ProductId
                         join gl in this._glCode.Table on pgl.GlCodeId equals gl.Id
                         where gl.IsDelivered == false

                         where startTime.Value <= osp.FulfillmentDateTime && endTime.Value >= osp.FulfillmentDateTime
                         select new OrderExcelExport()
                         {
                             OrderId = o.Id,
                             OrderGuid = o.OrderGuid,
                             CustomerId = o.CustomerId,
                             ProductName = p.Name,
                             CardType = o.CardType,
                             Category = c.Name,
                             CategoryId = c.Id,
                             GLCode = gl.GlCodeName,
                             GLCodeId = gl.Id,
                             OrderSubtotalInclTax = o.OrderSubtotalInclTax,
                             OrderSubtotalExclTax = o.OrderSubtotalExclTax,
                             SiteProductPriceExclTax = decimal.Round(osp.PriceExclTax, 2),
                             OrderSubTotalDiscountInclTax = o.OrderSubTotalDiscountInclTax,
                             OrderSubTotalDiscountExclTax = o.OrderSubTotalDiscountExclTax,
                             OrderShippingInclTax = o.OrderShippingInclTax,
                             OrderShippingExclTax = o.OrderShippingExclTax,
                             PaymentMethodAdditionalFeeInclTax = o.PaymentMethodAdditionalFeeInclTax,
                             PaymentMethodAdditionalFeeExclTax = o.PaymentMethodAdditionalFeeExclTax,
                             OrderTax = o.OrderTax,
                             SiteProductTax = decimal.Round(osp.PriceInclTax, 2) - decimal.Round(osp.PriceExclTax, 2),
                             OrderTotal = o.OrderTotal,
                             SiteProductTotal = decimal.Round(osp.PriceInclTax, 2),
                             RefundedAmount = o.RefundedAmount,
                             OrderDiscount = o.OrderDiscount,
                             CurrencyRate = o.CurrencyRate,
                             CustomerCurrencyCode = o.CustomerCurrencyCode,
                             //OrderWeight = o.OrderWeight,
                             AffiliateId = o.AffiliateId,
                             OrderStatusId = o.OrderStatusId,
                             PaymentMethodSystemName = o.PaymentMethodSystemName,
                             //PurchaseOrderNumber = o.PurchaseOrderNumber,
                             DeliveryDateUtc = osp.FulfillmentDateTime,
                             DeliveryAmountExclTax = osp.DeliveryAmountExclTax,
                             RequestedFulfillmentDate = osp.RequestedFulfillmentDateTime,
                             //  DeliveryAmountInclTax = o.OrderShippingInclTax,
                             PaidDateUtc = o.PaidDateUtc,
                             //ShippingMethod = o.ShippingMethod,
                             //ShippingRateComputationMethodSystemName =
                             //o.ShippingRateComputationMethodSystemName,
                             CreatedOnUtc = o.CreatedOnUtc
                         }
                         ).Distinct();
            var query2 = from c in query
                         orderby c.Category, c.OrderId
                         select c;
            var orders = query2.ToList();
            return orders;
        }

        /// <summary>
        /// Grab every record in the datetime frame that qualifies for a JE report.
        /// </summary>
        /// <param name="startTime">Start time to look for.</param>
        /// <param name="endTime">End time.</param>
        /// <returns>The list of records from that timeframe.</returns>
        public IList<OrderExcelExport> GetAllJournalRecords(DateTime? startTime, DateTime? endTime)
        {
            var query = (from s in _storeRepository.Table
                         join Glc in this._productGlCodeCalculationRepository.Table on s.Id equals Glc.StoreId
                         join SP in this._stateProvinceRepository.Table on s.CompanyStateProvinceId equals SP.Id
                         join OD in this._orderRepository.Table on Glc.OrderId equals OD.Id
                         join AD in this._addressRepository.Table on OD.BillingAddressId equals AD.Id //change it to shipping AddressID
                         join sp1 in this._stateProvinceRepository.Table on AD.StateProvinceId equals sp1.Id
                         join P in this._productRepository.Table on Glc.ProductId equals P.Id
                         join GN in this._genericRepository.Table on Glc.OrderId equals GN.EntityId
                         join GL in this._glCode.Table on Glc.GLCodeId equals GL.Id into abc
                         from data in abc.DefaultIfEmpty()
                         where Glc.Processed == false && Glc.GlStatusType == 1 && OD.PaidDateUtc >= startTime && OD.PaidDateUtc <= endTime && !OD.Deleted
                         select new OrderExcelExport()
                         {
                             SiteExtKey = s.ExtKey, //1
                             StoreStateProvince = SP.Abbreviation,//2
                             StoreZipCode = s.CompanyZipPostalCode,//3
                             StoreCity = s.CompanyCity,//4
                             LegalEntity = s.LegalEntity,//5
                             ShiptoState = sp1.Abbreviation,//6
                             ShipToZipCode = AD.ZipPostalCode,//7
                             ShippedToCity = AD.City,//8
                             OrderId = Glc.OrderId,//9
                             ProductName = P.Name,//10
                             paymentDate = OD.PaidDateUtc,//11
                             PaymentType = GN.Value,//12
                             VertexTaxAreaId = sp1.Abbreviation + AD.ZipPostalCode,//13
                             SiteName = s.Name,
                             storeid = Glc.StoreId,
                         }
                        );
            var journalReps = query.ToList();
            return journalReps;
        }

        /// <summary>
        /// Grab every record in the datetime frame that qualifies for a JE report for a specific store.
        /// </summary>
        /// <param name="startTime">Start time to look for.</param>
        /// <param name="endTime">End time.</param>
        /// <param name="storeId">The store to look for</param>
        /// <returns>The list of records from that timeframe.</returns>
        public IList<OrderExcelExport> GetStoreSpecificJournalRecords(DateTime? startTime, DateTime? endTime, int storeId)
        {
            var query = (from s in _storeRepository.Table
                         join Glc in this._productGlCodeCalculationRepository.Table on s.Id equals Glc.StoreId
                         join SP in this._stateProvinceRepository.Table on s.CompanyStateProvinceId equals SP.Id
                         join OD in this._orderRepository.Table on Glc.OrderId equals OD.Id
                         join AD in this._addressRepository.Table on OD.BillingAddressId equals AD.Id //change it to shipping AddressID
                         join sp1 in this._stateProvinceRepository.Table on AD.StateProvinceId equals sp1.Id
                         join P in this._productRepository.Table on Glc.ProductId equals P.Id
                         join GN in this._genericRepository.Table on Glc.OrderId equals GN.EntityId
                         join GL in this._glCode.Table on Glc.GLCodeId equals GL.Id into abc
                         from data in abc.DefaultIfEmpty()
                         where Glc.Processed == false && Glc.GlStatusType == 1 && Glc.StoreId == storeId
                         && OD.PaidDateUtc >= startTime && OD.PaidDateUtc <= endTime && !OD.Deleted
                         select new OrderExcelExport()
                         {
                             SiteExtKey = s.ExtKey, //1
                             StoreStateProvince = SP.Abbreviation,//2
                             StoreZipCode = s.CompanyZipPostalCode,//3
                             StoreCity = s.CompanyCity,//4
                             LegalEntity = s.LegalEntity,//5
                             ShiptoState = sp1.Abbreviation,//6
                             ShipToZipCode = AD.ZipPostalCode,//7
                             ShippedToCity = AD.City,//8
                             OrderId = Glc.OrderId,//9
                             ProductName = P.Name,//10
                             paymentDate = OD.PaidDateUtc,//11
                             PaymentType = GN.Value,//12
                             VertexTaxAreaId = sp1.Abbreviation + AD.ZipPostalCode,//13,
                             storeid = Glc.StoreId,
                         }
             ).Distinct();
            var journalReps = query.ToList();
            return journalReps;
        }

        /// <summary>
        /// Apply the Rules to those Products which neither has any type nor does it has any fulfillment Options.
        /// </summary>
        /// <param name="productInfo"></param>
        /// <param name="orderitems"></param>
        /// <param name="OrderDetails"></param>
        /// <returns></returns>
        //public IList<OrderExcelExport> ApplyBasicProductRules(Product productInfo, OrderItem orderitems, Order OrderDetails)
        //{
        //    var returnValue = new List<OrderExcelExport>();

        //    if(!productInfo.IsShipEnabled && !productInfo.IsLocalDelivery && !productInfo.IsPickupEnabled)
        //    {
        //        if(!productInfo.IsMealPlan && !productInfo.IsDonation && !productInfo.IsDownload)
        //        {
        //            UpdateDetailsWithAmountAndGlFromIncomingRequest.GLCode = "48702040";
        //            UpdateDetailsWithAmountAndGlFromIncomingRequest.Amount = orderitems.PriceExclTax;
        //        }
        //    }
        //    return returnValue;
        //}
        /// <summary>
        /// Apply the rules specific to the vendor item.
        /// </summary>
        /// <param name="productInfo">The product info we are looking at</param>        
        /// <param name="orderitems">THe database orderitem</param>
        /// <param name="OrderDetails">The database order</param>
        /// <returns>A list containing the range of any additional records that are necessary to populate in the report.</returns>
        public IList<OrderExcelExport> ApplyVendorRules(Product productInfo, OrderItem orderitems, Order OrderDetails)
        {
            var returnValue = new List<OrderExcelExport>();
            if (productInfo.VendorId != 0) //Checking for Vendor Product
            {
                UpdateDetailsWithAmountAndGlFromIncomingRequest.GLCode = "48702041";
                UpdateDetailsWithAmountAndGlFromIncomingRequest.Amount = orderitems.PriceExclTax;
                if (orderitems.IsShipping)
                {
                    var warehouseDetails = _warehouseRepository.GetById(productInfo.WarehouseId);
                    var ShiptoAddressDetsils = _addressRepository.GetById(OrderDetails.ShippingAddressId);

                    if (warehouseDetails == null)
                    {

                        var storedetails = GetStore(OrderDetails.StoreId);
                        if (storedetails != null)
                        {
                            UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreZipCode = storedetails.CompanyZipPostalCode;
                            UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreCity = storedetails.CompanyCity;
                            var stateProvinceDetails = _stateProvinceRepository.GetById(storedetails.CompanyStateProvinceId);
                            UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreStateProvince = stateProvinceDetails.Abbreviation;
                        }
                    }
                    if (warehouseDetails != null)
                    {
                        var addressDetails = _addressRepository.GetById(warehouseDetails.AddressId);
                        if (addressDetails != null)
                        {
                            UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreStateProvince = addressDetails.StateProvince.Abbreviation;
                            UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreZipCode = addressDetails.ZipPostalCode;
                            UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreCity = addressDetails.City;
                        }
                    }

                    //UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreStateProvince = addressDetails.StateProvince.Abbreviation;
                    //UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreZipCode = addressDetails.ZipPostalCode;
                    //UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreCity = addressDetails.City;

                    UpdateDetailsWithAmountAndGlFromIncomingRequest.ShiptoState = ShiptoAddressDetsils.StateProvince.Abbreviation;
                    UpdateDetailsWithAmountAndGlFromIncomingRequest.ShipToZipCode = ShiptoAddressDetsils.ZipPostalCode;
                    UpdateDetailsWithAmountAndGlFromIncomingRequest.ShippedToCity = ShiptoAddressDetsils.City;

                }

                if (UpdateDetailsWithAmountAndGlFromIncomingRequest.Amount != 0)
                {
                    returnValue.Add(new OrderExcelExport
                    {
                        SiteExtKey = UpdateDetailsWithAmountAndGlFromIncomingRequest.SiteExtKey, //1
                        StoreStateProvince = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreStateProvince,//2
                        StoreZipCode = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreZipCode,//3
                        StoreCity = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreCity,//4
                        LegalEntity = UpdateDetailsWithAmountAndGlFromIncomingRequest.LegalEntity,//5
                        ShiptoState = UpdateDetailsWithAmountAndGlFromIncomingRequest.ShiptoState,//6
                        ShipToZipCode = UpdateDetailsWithAmountAndGlFromIncomingRequest.ShipToZipCode,//7
                        ShippedToCity = UpdateDetailsWithAmountAndGlFromIncomingRequest.ShippedToCity,//8
                        OrderId = UpdateDetailsWithAmountAndGlFromIncomingRequest.OrderId,//9
                        ProductName = productInfo.Name,//10
                        paymentDate = UpdateDetailsWithAmountAndGlFromIncomingRequest.paymentDate,//11
                        PaymentType = UpdateDetailsWithAmountAndGlFromIncomingRequest.PaymentType,//12
                        VertexTaxAreaId = UpdateDetailsWithAmountAndGlFromIncomingRequest.ShiptoState + UpdateDetailsWithAmountAndGlFromIncomingRequest.ShipToZipCode,//13
                        SiteName = UpdateDetailsWithAmountAndGlFromIncomingRequest.SiteName,
                        Amount = UpdateDetailsWithAmountAndGlFromIncomingRequest.Amount,
                        GLCode = UpdateDetailsWithAmountAndGlFromIncomingRequest.GLCode,
                        storeid = UpdateDetailsWithAmountAndGlFromIncomingRequest.storeid
                    });
                    if (OrderDetails.OrderStatusId == 40 && !orderitems.IsPartialRefund && orderitems.FulfillmentDateTime == null) // Full refund if the item is not fulfilled
                    {
                        if (UpdateDetailsWithAmountAndGlFromIncomingRequest.Amount != 0)
                        {
                            returnValue.Add(new OrderExcelExport
                            {
                                SiteExtKey = UpdateDetailsWithAmountAndGlFromIncomingRequest.SiteExtKey, //1
                                StoreStateProvince = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreStateProvince,//2
                                StoreZipCode = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreZipCode,//3
                                StoreCity = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreCity,//4
                                LegalEntity = UpdateDetailsWithAmountAndGlFromIncomingRequest.LegalEntity,//5
                                ShiptoState = UpdateDetailsWithAmountAndGlFromIncomingRequest.ShiptoState,//6
                                ShipToZipCode = UpdateDetailsWithAmountAndGlFromIncomingRequest.ShipToZipCode,//7
                                ShippedToCity = UpdateDetailsWithAmountAndGlFromIncomingRequest.ShippedToCity,//8
                                OrderId = UpdateDetailsWithAmountAndGlFromIncomingRequest.OrderId,//9
                                ProductName = productInfo.Name,//10
                                paymentDate = UpdateDetailsWithAmountAndGlFromIncomingRequest.paymentDate,//11
                                PaymentType = UpdateDetailsWithAmountAndGlFromIncomingRequest.PaymentType,//12
                                VertexTaxAreaId = UpdateDetailsWithAmountAndGlFromIncomingRequest.ShiptoState + UpdateDetailsWithAmountAndGlFromIncomingRequest.ShipToZipCode,//13
                                SiteName = UpdateDetailsWithAmountAndGlFromIncomingRequest.SiteName,
                                Amount = -UpdateDetailsWithAmountAndGlFromIncomingRequest.Amount,
                                GLCode = UpdateDetailsWithAmountAndGlFromIncomingRequest.GLCode,
                                storeid = UpdateDetailsWithAmountAndGlFromIncomingRequest.storeid
                            });
                        }
                    }
                    if (OrderDetails.OrderStatusId == 40 && !orderitems.IsPartialRefund && orderitems.FulfillmentDateTime != null)// Full Refund of the Product if Fulfilled
                    {
                        bool splitGl = productInfo.TaxCategoryId == -1 ? true : false;
                        List<ProductGlCode> PaidCodes = _glCodeService.GetProductGlCodes(productInfo, GLCodeStatusType.Paid).ToList();
                        var dis = PaidCodes.GroupBy(x => x.GlCodeId).Select(y => y.First());

                        foreach (var codes in dis)
                        {
                            if (ApplyDeliveryReportShippingRules(productInfo, orderitems, codes).FirstOrDefault().Amount != 0)
                            {
                                returnValue.Add(new OrderExcelExport
                                {
                                    SiteExtKey = UpdateDetailsWithAmountAndGlFromIncomingRequest.SiteExtKey, //1
                                    StoreStateProvince = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreStateProvince,//2
                                    StoreZipCode = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreZipCode,//3
                                    StoreCity = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreCity,//4
                                    LegalEntity = UpdateDetailsWithAmountAndGlFromIncomingRequest.LegalEntity,//5
                                    ShiptoState = UpdateDetailsWithAmountAndGlFromIncomingRequest.ShiptoState,//6
                                    ShipToZipCode = UpdateDetailsWithAmountAndGlFromIncomingRequest.ShipToZipCode,//7
                                    ShippedToCity = UpdateDetailsWithAmountAndGlFromIncomingRequest.ShippedToCity,//8
                                    OrderId = UpdateDetailsWithAmountAndGlFromIncomingRequest.OrderId,//9
                                    ProductName = productInfo.Name,//10
                                    paymentDate = UpdateDetailsWithAmountAndGlFromIncomingRequest.paymentDate,//11
                                    PaymentType = UpdateDetailsWithAmountAndGlFromIncomingRequest.PaymentType,//12
                                    VertexTaxAreaId = UpdateDetailsWithAmountAndGlFromIncomingRequest.ShiptoState + UpdateDetailsWithAmountAndGlFromIncomingRequest.ShipToZipCode,//13
                                    SiteName = UpdateDetailsWithAmountAndGlFromIncomingRequest.SiteName,
                                    Amount = -ApplyDeliveryReportShippingRules(productInfo, orderitems, codes).FirstOrDefault().Amount,
                                    GLCode = ApplyDeliveryReportShippingRules(productInfo, orderitems, codes).FirstOrDefault().GLCode,
                                    storeid = UpdateDetailsWithAmountAndGlFromIncomingRequest.storeid
                                });
                            }

                            // returnValue.AddRange(ApplyDeliveryReportShippingRules(productInfo, orderitems, codes));
                            //returnValue.AddRange(ApplyDeliveryReportDeliveryRules(productInfo, orderitems, codes, splitGl));
                        }
                    }
                    if (orderitems.IsPartialRefund) // Section to Catch Partial Refund Records for Vendor Products. For partial Refunds order cannot be of cancelled state which means they cannot ave Orde status id=40
                    {
                        returnValue.AddRange(ApplyPartialRefundBusinessRules(productInfo, orderitems, OrderDetails));
                    }
                }

            }
            return returnValue;
        }

        /// <summary>
        /// Apply the rules specific to meal plan or donation items.
        /// </summary>
        /// <param name="productInfo">The product info we are looking at</param>        
        /// <param name="orderitems">THe database orderitem</param>
        /// <param name="OrderDetails">The database order</param>
        /// <returns>A list containing the range of any additional records that are necessary to populate in the report.</returns>
        public IList<OrderExcelExport> ApplyMealPlanAndDonationRules(Product productInfo, OrderItem orderitems, Order OrderDetails)
        {
            var returnValue = new List<OrderExcelExport>();
            //if (OrderDetails.Id == 100802)
            //{

            //}
            if (productInfo.IsMealPlan || productInfo.IsDonation) //Checking for Meal Plans or Not
            {
                List<ProductGlCode> PaidCodes = _glCodeService.GetProductGlCodes(productInfo, GLCodeStatusType.Paid).ToList();
                var dis = PaidCodes.GroupBy(x => x.GlCodeId).Select(y => y.First());

                var storedetails = GetStore(OrderDetails.StoreId);
                if (storedetails != null)
                {
                    UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreZipCode = storedetails.CompanyZipPostalCode;
                    UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreCity = storedetails.CompanyCity;
                    var stateProvinceDetails = _stateProvinceRepository.GetById(storedetails.CompanyStateProvinceId);
                    UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreStateProvince = stateProvinceDetails.Abbreviation;
                }
                if (productInfo.TaxCategoryId != -1)
                {
                    foreach (var codes in dis)
                    {
                        var glcodesDetails = _glCodeService.GetGlCodeById(codes.GlCodeId);
                        UpdateDetailsWithAmountAndGlFromIncomingRequest.GLCode = glcodesDetails.GlCodeName;
                        UpdateDetailsWithAmountAndGlFromIncomingRequest.Amount = orderitems.PriceExclTax;
                        if (UpdateDetailsWithAmountAndGlFromIncomingRequest.Amount != 0)
                        {
                            returnValue.Add(new OrderExcelExport
                            {
                                SiteExtKey = UpdateDetailsWithAmountAndGlFromIncomingRequest.SiteExtKey, //1
                                StoreStateProvince = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreStateProvince,//2
                                StoreZipCode = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreZipCode,//3
                                StoreCity = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreCity,//4
                                LegalEntity = UpdateDetailsWithAmountAndGlFromIncomingRequest.LegalEntity,//5
                                ShiptoState = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreStateProvince,//6
                                ShipToZipCode = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreZipCode,//7
                                ShippedToCity = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreCity,//8
                                OrderId = UpdateDetailsWithAmountAndGlFromIncomingRequest.OrderId,//9
                                ProductName = productInfo.Name,//10
                                paymentDate = UpdateDetailsWithAmountAndGlFromIncomingRequest.paymentDate,//11
                                PaymentType = UpdateDetailsWithAmountAndGlFromIncomingRequest.PaymentType,//12
                                VertexTaxAreaId = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreStateProvince + UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreZipCode,//13
                                SiteName = UpdateDetailsWithAmountAndGlFromIncomingRequest.SiteName,
                                Amount = UpdateDetailsWithAmountAndGlFromIncomingRequest.Amount,
                                GLCode = UpdateDetailsWithAmountAndGlFromIncomingRequest.GLCode,
                                storeid = UpdateDetailsWithAmountAndGlFromIncomingRequest.storeid
                            });
                            if (OrderDetails.OrderStatusId == 40 && !orderitems.IsPartialRefund)
                            {
                                returnValue.Add(new OrderExcelExport
                                {
                                    SiteExtKey = UpdateDetailsWithAmountAndGlFromIncomingRequest.SiteExtKey, //1
                                    StoreStateProvince = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreStateProvince,//2
                                    StoreZipCode = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreZipCode,//3
                                    StoreCity = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreCity,//4
                                    LegalEntity = UpdateDetailsWithAmountAndGlFromIncomingRequest.LegalEntity,//5
                                    ShiptoState = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreStateProvince,//6
                                    ShipToZipCode = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreZipCode,//7
                                    ShippedToCity = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreCity,//8
                                    OrderId = UpdateDetailsWithAmountAndGlFromIncomingRequest.OrderId,//9
                                    ProductName = productInfo.Name,//10
                                    paymentDate = UpdateDetailsWithAmountAndGlFromIncomingRequest.paymentDate,//11
                                    PaymentType = UpdateDetailsWithAmountAndGlFromIncomingRequest.PaymentType,//12
                                    VertexTaxAreaId = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreStateProvince + UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreZipCode,//13
                                    SiteName = UpdateDetailsWithAmountAndGlFromIncomingRequest.SiteName,
                                    Amount = -UpdateDetailsWithAmountAndGlFromIncomingRequest.Amount,
                                    GLCode = UpdateDetailsWithAmountAndGlFromIncomingRequest.GLCode,
                                    storeid = UpdateDetailsWithAmountAndGlFromIncomingRequest.storeid
                                });

                            }
                        }
                    }
                    if (orderitems.IsPartialRefund)
                    {
                        returnValue.AddRange(ApplyPartialRefundBusinessRules(productInfo, orderitems, OrderDetails));
                    }
                }
                else if (productInfo.TaxCategoryId == -1)
                {
                    foreach (var codes in PaidCodes)
                    {
                        var glcodesDetails = _glCodeService.GetGlCodeById(codes.GlCodeId);
                        UpdateDetailsWithAmountAndGlFromIncomingRequest.GLCode = glcodesDetails.GlCodeName;
                        UpdateDetailsWithAmountAndGlFromIncomingRequest.Amount = codes.CalculateGLAmount(orderitems, true);
                        if (UpdateDetailsWithAmountAndGlFromIncomingRequest.Amount != 0)
                        {
                            returnValue.Add(new OrderExcelExport
                            {
                                SiteExtKey = UpdateDetailsWithAmountAndGlFromIncomingRequest.SiteExtKey, //1
                                StoreStateProvince = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreStateProvince,//2
                                StoreZipCode = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreZipCode,//3
                                StoreCity = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreCity,//4
                                LegalEntity = UpdateDetailsWithAmountAndGlFromIncomingRequest.LegalEntity,//5
                                ShiptoState = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreStateProvince,//6
                                ShipToZipCode = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreZipCode,//7
                                ShippedToCity = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreCity,//8
                                OrderId = UpdateDetailsWithAmountAndGlFromIncomingRequest.OrderId,//9
                                ProductName = productInfo.Name,//10
                                paymentDate = UpdateDetailsWithAmountAndGlFromIncomingRequest.paymentDate,//11
                                PaymentType = UpdateDetailsWithAmountAndGlFromIncomingRequest.PaymentType,//12
                                VertexTaxAreaId = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreStateProvince + UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreZipCode,//13
                                SiteName = UpdateDetailsWithAmountAndGlFromIncomingRequest.SiteName,
                                Amount = UpdateDetailsWithAmountAndGlFromIncomingRequest.Amount,
                                GLCode = UpdateDetailsWithAmountAndGlFromIncomingRequest.GLCode,
                                storeid = UpdateDetailsWithAmountAndGlFromIncomingRequest.storeid
                            });
                            if (OrderDetails.OrderStatusId == 40 && !orderitems.IsPartialRefund) // FUll Refund on the Product post to Revenue GL always as it is a Meal plan type products
                            {
                                returnValue.Add(new OrderExcelExport
                                {
                                    SiteExtKey = UpdateDetailsWithAmountAndGlFromIncomingRequest.SiteExtKey, //1
                                    StoreStateProvince = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreStateProvince,//2
                                    StoreZipCode = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreZipCode,//3
                                    StoreCity = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreCity,//4
                                    LegalEntity = UpdateDetailsWithAmountAndGlFromIncomingRequest.LegalEntity,//5
                                    ShiptoState = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreStateProvince,//6
                                    ShipToZipCode = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreZipCode,//7
                                    ShippedToCity = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreCity,//8
                                    OrderId = UpdateDetailsWithAmountAndGlFromIncomingRequest.OrderId,//9
                                    ProductName = productInfo.Name,//10
                                    paymentDate = UpdateDetailsWithAmountAndGlFromIncomingRequest.paymentDate,//11
                                    PaymentType = UpdateDetailsWithAmountAndGlFromIncomingRequest.PaymentType,//12
                                    VertexTaxAreaId = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreStateProvince + UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreZipCode,//13
                                    SiteName = UpdateDetailsWithAmountAndGlFromIncomingRequest.SiteName,
                                    Amount = -UpdateDetailsWithAmountAndGlFromIncomingRequest.Amount,
                                    GLCode = UpdateDetailsWithAmountAndGlFromIncomingRequest.GLCode,
                                    storeid = UpdateDetailsWithAmountAndGlFromIncomingRequest.storeid
                                });
                            }

                        }
                    }
                    if (orderitems.IsPartialRefund)
                    {
                        returnValue.AddRange(ApplyPartialRefundBusinessRules(productInfo, orderitems, OrderDetails));
                    }
                }
            }
            else if (!productInfo.IsMealPlan && !productInfo.IsDonation && productInfo.VendorId == 0)//For Normal Products
            {
                UpdateDetailsWithAmountAndGlFromIncomingRequest.GLCode = "48702040";
                UpdateDetailsWithAmountAndGlFromIncomingRequest.Amount = orderitems.PriceExclTax;

                if (orderitems.IsDeliveryPickUp)
                {
                    var warehouseDetails = _warehouseRepository.GetById(orderitems.SelectedFulfillmentWarehouseId);
                    if (warehouseDetails != null)
                    {
                        var addressDetails = _addressRepository.GetById(warehouseDetails.AddressId);
                        if (addressDetails != null)
                        {
                            if (orderitems.SelectedShippingMethodName == "Delivery")
                            {
                                var ShiptoAddressDetsils = _addressRepository.GetById(OrderDetails.ShippingAddressId);
                                warehouseDetails = _warehouseRepository.GetById(productInfo.WarehouseId);
                                var storedetails = GetStore(OrderDetails.StoreId);

                                if (warehouseDetails != null)
                                {
                                    addressDetails = _addressRepository.GetById(warehouseDetails.AddressId);
                                    if (addressDetails != null)
                                    {
                                        UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreStateProvince = addressDetails.StateProvince.Abbreviation;
                                        UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreZipCode = addressDetails.ZipPostalCode;
                                        UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreCity = addressDetails.City;
                                    }
                                }

                                else
                                {
                                    UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreZipCode = storedetails.CompanyZipPostalCode;
                                    UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreCity = storedetails.CompanyCity;
                                    var stateProvinceDetails = _stateProvinceRepository.GetById(storedetails.CompanyStateProvinceId);
                                    UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreStateProvince = stateProvinceDetails.Abbreviation;
                                }

                                UpdateDetailsWithAmountAndGlFromIncomingRequest.ShiptoState = ShiptoAddressDetsils.StateProvince.Abbreviation;
                                UpdateDetailsWithAmountAndGlFromIncomingRequest.ShipToZipCode = ShiptoAddressDetsils.ZipPostalCode;
                                UpdateDetailsWithAmountAndGlFromIncomingRequest.ShippedToCity = ShiptoAddressDetsils.City;
                            }
                            else
                            {
                                UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreStateProvince = addressDetails.StateProvince.Abbreviation;
                                UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreZipCode = addressDetails.ZipPostalCode;
                                UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreCity = addressDetails.City;

                                UpdateDetailsWithAmountAndGlFromIncomingRequest.ShiptoState = addressDetails.StateProvince.Abbreviation;
                                UpdateDetailsWithAmountAndGlFromIncomingRequest.ShipToZipCode = addressDetails.ZipPostalCode;
                                UpdateDetailsWithAmountAndGlFromIncomingRequest.ShippedToCity = addressDetails.City;
                            }
                        }
                    }
                }
                if (orderitems.IsShipping)
                {
                    var warehouseDetails = _warehouseRepository.GetById(productInfo.WarehouseId);
                    var ShiptoAddressDetsils = _addressRepository.GetById(OrderDetails.ShippingAddressId);

                    if (warehouseDetails == null)
                    {

                        var storedetails = GetStore(OrderDetails.StoreId);
                        if (storedetails != null)
                        {
                            UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreZipCode = storedetails.CompanyZipPostalCode;
                            UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreCity = storedetails.CompanyCity;
                            var stateProvinceDetails = _stateProvinceRepository.GetById(storedetails.CompanyStateProvinceId);
                            UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreStateProvince = stateProvinceDetails.Abbreviation;
                        }
                    }
                    if (warehouseDetails != null)
                    {
                        var addressDetails = _addressRepository.GetById(warehouseDetails.AddressId);
                        if (addressDetails != null)
                        {
                            UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreStateProvince = addressDetails.StateProvince.Abbreviation;
                            UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreZipCode = addressDetails.ZipPostalCode;
                            UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreCity = addressDetails.City;
                        }
                    }
                    UpdateDetailsWithAmountAndGlFromIncomingRequest.ShiptoState = ShiptoAddressDetsils.StateProvince.Abbreviation;
                    UpdateDetailsWithAmountAndGlFromIncomingRequest.ShipToZipCode = ShiptoAddressDetsils.ZipPostalCode;
                    UpdateDetailsWithAmountAndGlFromIncomingRequest.ShippedToCity = ShiptoAddressDetsils.City;

                }

                if (!productInfo.IsLocalDelivery && !productInfo.IsPickupEnabled && !productInfo.IsShipEnabled)
                {
                    UpdateDetailsWithAmountAndGlFromIncomingRequest.ShiptoState = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreStateProvince;
                    UpdateDetailsWithAmountAndGlFromIncomingRequest.ShipToZipCode = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreZipCode;
                    UpdateDetailsWithAmountAndGlFromIncomingRequest.ShippedToCity = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreCity;
                }

                if (UpdateDetailsWithAmountAndGlFromIncomingRequest.Amount != 0)
                {
                    returnValue.Add(new OrderExcelExport
                    {
                        SiteExtKey = UpdateDetailsWithAmountAndGlFromIncomingRequest.SiteExtKey, //1
                        StoreStateProvince = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreStateProvince,//2
                        StoreZipCode = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreZipCode,//3
                        StoreCity = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreCity,//4
                        LegalEntity = UpdateDetailsWithAmountAndGlFromIncomingRequest.LegalEntity,//5
                        ShiptoState = UpdateDetailsWithAmountAndGlFromIncomingRequest.ShiptoState,//6
                        ShipToZipCode = UpdateDetailsWithAmountAndGlFromIncomingRequest.ShipToZipCode,//7
                        ShippedToCity = UpdateDetailsWithAmountAndGlFromIncomingRequest.ShippedToCity,//8
                        OrderId = UpdateDetailsWithAmountAndGlFromIncomingRequest.OrderId,//9
                        ProductName = productInfo.Name,//10
                        paymentDate = UpdateDetailsWithAmountAndGlFromIncomingRequest.paymentDate,//11
                        PaymentType = UpdateDetailsWithAmountAndGlFromIncomingRequest.PaymentType,//12
                        VertexTaxAreaId = UpdateDetailsWithAmountAndGlFromIncomingRequest.ShiptoState + UpdateDetailsWithAmountAndGlFromIncomingRequest.ShipToZipCode,//13
                        SiteName = UpdateDetailsWithAmountAndGlFromIncomingRequest.SiteName,
                        Amount = UpdateDetailsWithAmountAndGlFromIncomingRequest.Amount,
                        GLCode = UpdateDetailsWithAmountAndGlFromIncomingRequest.GLCode,
                        storeid = UpdateDetailsWithAmountAndGlFromIncomingRequest.storeid

                    });
                    if (OrderDetails.OrderStatusId == 40 && !orderitems.IsPartialRefund && orderitems.FulfillmentDateTime == null) // Full Refund If the product is not fulfilled
                    {
                        if (UpdateDetailsWithAmountAndGlFromIncomingRequest.Amount != 0)
                        {
                            returnValue.Add(new OrderExcelExport
                            {
                                SiteExtKey = UpdateDetailsWithAmountAndGlFromIncomingRequest.SiteExtKey, //1
                                StoreStateProvince = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreStateProvince,//2
                                StoreZipCode = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreZipCode,//3
                                StoreCity = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreCity,//4
                                LegalEntity = UpdateDetailsWithAmountAndGlFromIncomingRequest.LegalEntity,//5
                                ShiptoState = UpdateDetailsWithAmountAndGlFromIncomingRequest.ShiptoState,//6
                                ShipToZipCode = UpdateDetailsWithAmountAndGlFromIncomingRequest.ShipToZipCode,//7
                                ShippedToCity = UpdateDetailsWithAmountAndGlFromIncomingRequest.ShippedToCity,//8
                                OrderId = UpdateDetailsWithAmountAndGlFromIncomingRequest.OrderId,//9
                                ProductName = productInfo.Name,//10
                                paymentDate = UpdateDetailsWithAmountAndGlFromIncomingRequest.paymentDate,//11
                                PaymentType = UpdateDetailsWithAmountAndGlFromIncomingRequest.PaymentType,//12
                                VertexTaxAreaId = UpdateDetailsWithAmountAndGlFromIncomingRequest.ShiptoState + UpdateDetailsWithAmountAndGlFromIncomingRequest.ShipToZipCode,//13
                                SiteName = UpdateDetailsWithAmountAndGlFromIncomingRequest.SiteName,
                                Amount = -UpdateDetailsWithAmountAndGlFromIncomingRequest.Amount,
                                GLCode = UpdateDetailsWithAmountAndGlFromIncomingRequest.GLCode,
                                storeid = UpdateDetailsWithAmountAndGlFromIncomingRequest.storeid

                            });
                        }
                    }
                    if (OrderDetails.OrderStatusId == 40 && !orderitems.IsPartialRefund && orderitems.FulfillmentDateTime != null)// Full Refund of the Product if Fulfilled
                    {
                        bool splitGl = productInfo.TaxCategoryId == -1 ? true : false;
                        List<ProductGlCode> PaidCodes = _glCodeService.GetProductGlCodes(productInfo, GLCodeStatusType.Paid).ToList();
                        var dis = PaidCodes.GroupBy(x => x.GlCodeId).Select(y => y.First());

                        foreach (var codes in dis)
                        {
                            if (ApplyDeliveryReportDeliveryRules(productInfo, orderitems, codes, splitGl).Any())
                            {
                                if (ApplyDeliveryReportDeliveryRules(productInfo, orderitems, codes, splitGl).FirstOrDefault().Amount != 0)
                                {
                                    returnValue.Add(new OrderExcelExport
                                    {
                                        SiteExtKey = UpdateDetailsWithAmountAndGlFromIncomingRequest.SiteExtKey, //1
                                        StoreStateProvince = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreStateProvince,//2
                                        StoreZipCode = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreZipCode,//3
                                        StoreCity = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreCity,//4
                                        LegalEntity = UpdateDetailsWithAmountAndGlFromIncomingRequest.LegalEntity,//5
                                        ShiptoState = UpdateDetailsWithAmountAndGlFromIncomingRequest.ShiptoState,//6
                                        ShipToZipCode = UpdateDetailsWithAmountAndGlFromIncomingRequest.ShipToZipCode,//7
                                        ShippedToCity = UpdateDetailsWithAmountAndGlFromIncomingRequest.ShippedToCity,//8
                                        OrderId = UpdateDetailsWithAmountAndGlFromIncomingRequest.OrderId,//9
                                        ProductName = productInfo.Name,//10
                                        paymentDate = UpdateDetailsWithAmountAndGlFromIncomingRequest.paymentDate,//11
                                        PaymentType = UpdateDetailsWithAmountAndGlFromIncomingRequest.PaymentType,//12
                                        VertexTaxAreaId = UpdateDetailsWithAmountAndGlFromIncomingRequest.ShiptoState + UpdateDetailsWithAmountAndGlFromIncomingRequest.ShipToZipCode,//13
                                        SiteName = UpdateDetailsWithAmountAndGlFromIncomingRequest.SiteName,
                                        Amount = -ApplyDeliveryReportDeliveryRules(productInfo, orderitems, codes, splitGl).FirstOrDefault().Amount,
                                        GLCode = ApplyDeliveryReportDeliveryRules(productInfo, orderitems, codes, splitGl).FirstOrDefault().GLCode,
                                        storeid = UpdateDetailsWithAmountAndGlFromIncomingRequest.storeid
                                    });
                                }
                            }
                            //returnValue.AddRange(ApplyDeliveryReportShippingRules(productInfo, orderitems, codes));
                            //returnValue.AddRange(ApplyDeliveryReportDeliveryRules(productInfo, orderitems, codes, splitGl));

                        }
                    }
                    if (orderitems.IsPartialRefund)
                    {
                        returnValue.AddRange(ApplyPartialRefundBusinessRules(productInfo, orderitems, OrderDetails));
                    }
                }
            }
            return returnValue;
        }

        /// <summary>
        /// Apply the rules specific to shipping items.
        /// </summary>
        /// <param name="productInfo">The product info we are looking at</param>        
        /// <param name="orderitems">THe database orderitem</param>
        /// <param name="OrderDetails">The database order</param>
        /// <returns>A list containing the range of any additional records that are necessary to populate in the report.</returns>
        public IList<OrderExcelExport> ApplyShippingRules(Product productInfo, OrderItem orderitems, Order OrderDetails)
        {
            var returnValue = new List<OrderExcelExport>();
            if (productInfo.IsShipEnabled && orderitems.IsShipping) //Setting The Shipping GLS
            {
                UpdateDetailsWithAmountAndGlFromIncomingRequest.GLCode = "48702040";
                UpdateDetailsWithAmountAndGlFromIncomingRequest.Amount = orderitems.ShippingFee;

                var warehouseDetails = _warehouseRepository.GetById(productInfo.WarehouseId);
                var ShiptoAddressDetsils = _addressRepository.GetById(OrderDetails.ShippingAddressId);
                if (warehouseDetails == null)
                {
                    var storedetails = GetStore(OrderDetails.StoreId);
                    if (storedetails != null)
                    {
                        UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreZipCode = storedetails.CompanyZipPostalCode;
                        UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreCity = storedetails.CompanyCity;
                        var stateProvinceDetails = _stateProvinceRepository.GetById(storedetails.CompanyStateProvinceId);
                        UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreStateProvince = stateProvinceDetails.Abbreviation;
                    }
                }

                if (warehouseDetails != null)
                {
                    var addressDetails = _addressRepository.GetById(warehouseDetails.AddressId);
                    if (addressDetails != null)
                    {
                        UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreStateProvince = addressDetails.StateProvince.Abbreviation;
                        UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreZipCode = addressDetails.ZipPostalCode;
                        UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreCity = addressDetails.City;
                    }
                }
                UpdateDetailsWithAmountAndGlFromIncomingRequest.ShiptoState = ShiptoAddressDetsils.StateProvince.Abbreviation;
                UpdateDetailsWithAmountAndGlFromIncomingRequest.ShipToZipCode = ShiptoAddressDetsils.ZipPostalCode;
                UpdateDetailsWithAmountAndGlFromIncomingRequest.ShippedToCity = ShiptoAddressDetsils.City;

                if (UpdateDetailsWithAmountAndGlFromIncomingRequest.Amount != 0)
                {
                    returnValue.Add(new OrderExcelExport
                    {
                        SiteExtKey = UpdateDetailsWithAmountAndGlFromIncomingRequest.SiteExtKey, //1
                        StoreStateProvince = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreStateProvince,//2
                        StoreZipCode = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreZipCode,//3
                        StoreCity = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreCity,//4
                        LegalEntity = UpdateDetailsWithAmountAndGlFromIncomingRequest.LegalEntity,//5
                        ShiptoState = UpdateDetailsWithAmountAndGlFromIncomingRequest.ShiptoState,//6
                        ShipToZipCode = UpdateDetailsWithAmountAndGlFromIncomingRequest.ShipToZipCode,//7
                        ShippedToCity = UpdateDetailsWithAmountAndGlFromIncomingRequest.ShippedToCity,//8
                        OrderId = UpdateDetailsWithAmountAndGlFromIncomingRequest.OrderId,//9
                        ProductName = productInfo.Name + "-Shipping",//10
                        paymentDate = UpdateDetailsWithAmountAndGlFromIncomingRequest.paymentDate,//11
                        PaymentType = UpdateDetailsWithAmountAndGlFromIncomingRequest.PaymentType,//12
                        VertexTaxAreaId = UpdateDetailsWithAmountAndGlFromIncomingRequest.ShiptoState + UpdateDetailsWithAmountAndGlFromIncomingRequest.ShipToZipCode,//13
                        SiteName = UpdateDetailsWithAmountAndGlFromIncomingRequest.SiteName,
                        Amount = UpdateDetailsWithAmountAndGlFromIncomingRequest.Amount,
                        GLCode = UpdateDetailsWithAmountAndGlFromIncomingRequest.GLCode,
                        storeid = UpdateDetailsWithAmountAndGlFromIncomingRequest.storeid

                    });
                }
                if (OrderDetails.OrderStatusId == 40 && !orderitems.IsPartialRefund && orderitems.FulfillmentDateTime == null)// Full refund if the Product is not fulfilled
                {
                    if (UpdateDetailsWithAmountAndGlFromIncomingRequest.Amount != 0)
                    {
                        returnValue.Add(new OrderExcelExport
                        {
                            SiteExtKey = UpdateDetailsWithAmountAndGlFromIncomingRequest.SiteExtKey, //1
                            StoreStateProvince = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreStateProvince,//2
                            StoreZipCode = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreZipCode,//3
                            StoreCity = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreCity,//4
                            LegalEntity = UpdateDetailsWithAmountAndGlFromIncomingRequest.LegalEntity,//5
                            ShiptoState = UpdateDetailsWithAmountAndGlFromIncomingRequest.ShiptoState,//6
                            ShipToZipCode = UpdateDetailsWithAmountAndGlFromIncomingRequest.ShipToZipCode,//7
                            ShippedToCity = UpdateDetailsWithAmountAndGlFromIncomingRequest.ShippedToCity,//8
                            OrderId = UpdateDetailsWithAmountAndGlFromIncomingRequest.OrderId,//9
                            ProductName = productInfo.Name + "-Shipping",//10
                            paymentDate = UpdateDetailsWithAmountAndGlFromIncomingRequest.paymentDate,//11
                            PaymentType = UpdateDetailsWithAmountAndGlFromIncomingRequest.PaymentType,//12
                            VertexTaxAreaId = UpdateDetailsWithAmountAndGlFromIncomingRequest.ShiptoState + UpdateDetailsWithAmountAndGlFromIncomingRequest.ShipToZipCode,//13
                            SiteName = UpdateDetailsWithAmountAndGlFromIncomingRequest.SiteName,
                            Amount = -UpdateDetailsWithAmountAndGlFromIncomingRequest.Amount,
                            GLCode = UpdateDetailsWithAmountAndGlFromIncomingRequest.GLCode,
                            storeid = UpdateDetailsWithAmountAndGlFromIncomingRequest.storeid

                        });
                    }
                }
                if (OrderDetails.OrderStatusId == 40 && !orderitems.IsPartialRefund && orderitems.FulfillmentDateTime != null)// Full Refund of the Product if Fulfilled
                {
                    bool splitGl = productInfo.TaxCategoryId == -1 ? true : false;
                    List<ProductGlCode> PaidCodes = _glCodeService.GetProductGlCodes(productInfo, GLCodeStatusType.Paid).ToList();
                    var dis = PaidCodes.GroupBy(x => x.GlCodeId).Select(y => y.First());

                    foreach (var codes in dis)
                    {
                        if (ApplyDeliveryReportShippingRules(productInfo, orderitems, codes).Any())
                        {
                            if (ApplyDeliveryReportShippingRules(productInfo, orderitems, codes).FirstOrDefault().Amount != 0)
                            {
                                returnValue.Add(new OrderExcelExport
                                {
                                    SiteExtKey = UpdateDetailsWithAmountAndGlFromIncomingRequest.SiteExtKey, //1
                                    StoreStateProvince = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreStateProvince,//2
                                    StoreZipCode = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreZipCode,//3
                                    StoreCity = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreCity,//4
                                    LegalEntity = UpdateDetailsWithAmountAndGlFromIncomingRequest.LegalEntity,//5
                                    ShiptoState = UpdateDetailsWithAmountAndGlFromIncomingRequest.ShiptoState,//6
                                    ShipToZipCode = UpdateDetailsWithAmountAndGlFromIncomingRequest.ShipToZipCode,//7
                                    ShippedToCity = UpdateDetailsWithAmountAndGlFromIncomingRequest.ShippedToCity,//8
                                    OrderId = UpdateDetailsWithAmountAndGlFromIncomingRequest.OrderId,//9
                                    ProductName = productInfo.Name + "-Shipping",//10
                                    paymentDate = UpdateDetailsWithAmountAndGlFromIncomingRequest.paymentDate,//11
                                    PaymentType = UpdateDetailsWithAmountAndGlFromIncomingRequest.PaymentType,//12
                                    VertexTaxAreaId = UpdateDetailsWithAmountAndGlFromIncomingRequest.ShiptoState + UpdateDetailsWithAmountAndGlFromIncomingRequest.ShipToZipCode,//13
                                    SiteName = UpdateDetailsWithAmountAndGlFromIncomingRequest.SiteName,
                                    Amount = -ApplyDeliveryReportShippingRules(productInfo, orderitems, codes).FirstOrDefault().Amount,
                                    GLCode = ApplyDeliveryReportShippingRules(productInfo, orderitems, codes).FirstOrDefault().GLCode,
                                    storeid = UpdateDetailsWithAmountAndGlFromIncomingRequest.storeid
                                });
                            }
                        }
                        //returnValue.AddRange(ApplyDeliveryReportShippingRules(productInfo, orderitems, codes));
                        //returnValue.AddRange(ApplyDeliveryReportDeliveryRules(productInfo, orderitems, codes, splitGl));
                    }
                }
            }

            return returnValue;
        }

        /// <summary>
        /// Apply the rules specific to delivery items.
        /// </summary>
        /// <param name="productInfo">The product info we are looking at</param>
        /// <param name="orderitems">THe database orderitem</param>
        /// <param name="OrderDetails">The database order</param>
        /// <returns>A list containing the range of any additional records that are necessary to populate in the report.</returns>
        public IList<OrderExcelExport> ApplyDeliveryRules(Product productInfo, OrderItem orderitems, Order OrderDetails)
        {
            var returnList = new List<OrderExcelExport>();
            if ((productInfo.IsLocalDelivery || productInfo.IsPickupEnabled) && orderitems.IsDeliveryPickUp) //Setting The dElivery Gls
            {
                UpdateDetailsWithAmountAndGlFromIncomingRequest.GLCode = "48702040";
                UpdateDetailsWithAmountAndGlFromIncomingRequest.Amount = orderitems.DeliveryPickupFee;

                var warehouseDetails = _warehouseRepository.GetById(orderitems.SelectedFulfillmentWarehouseId);
                if (warehouseDetails != null)
                {
                    var addressDetails = _addressRepository.GetById(warehouseDetails.AddressId);
                    if (addressDetails != null)
                    {

                        UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreStateProvince = addressDetails.StateProvince.Abbreviation;
                        UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreZipCode = addressDetails.ZipPostalCode;
                        UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreCity = addressDetails.City;

                        if (orderitems.SelectedShippingMethodName == "Delivery")
                        {
                            var ShiptoAddressDetsils = _addressRepository.GetById(OrderDetails.ShippingAddressId);
                            var storedetails = GetStore(OrderDetails.StoreId);
                            warehouseDetails = _warehouseRepository.GetById(productInfo.WarehouseId);

                            if (warehouseDetails != null)
                            {
                                addressDetails = _addressRepository.GetById(warehouseDetails.AddressId);
                                if (addressDetails != null)
                                {
                                    UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreStateProvince = addressDetails.StateProvince.Abbreviation;
                                    UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreZipCode = addressDetails.ZipPostalCode;
                                    UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreCity = addressDetails.City;
                                }
                            }
                            else
                            {
                                UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreZipCode = storedetails.CompanyZipPostalCode;
                                UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreCity = storedetails.CompanyCity;
                                var stateProvinceDetails = _stateProvinceRepository.GetById(storedetails.CompanyStateProvinceId);
                                UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreStateProvince = stateProvinceDetails.Abbreviation;
                            }
                            UpdateDetailsWithAmountAndGlFromIncomingRequest.ShiptoState = ShiptoAddressDetsils.StateProvince.Abbreviation;
                            UpdateDetailsWithAmountAndGlFromIncomingRequest.ShipToZipCode = ShiptoAddressDetsils.ZipPostalCode;
                            UpdateDetailsWithAmountAndGlFromIncomingRequest.ShippedToCity = ShiptoAddressDetsils.City;
                        }
                        else
                        {
                            UpdateDetailsWithAmountAndGlFromIncomingRequest.ShiptoState = addressDetails.StateProvince.Abbreviation;
                            UpdateDetailsWithAmountAndGlFromIncomingRequest.ShipToZipCode = addressDetails.ZipPostalCode;
                            UpdateDetailsWithAmountAndGlFromIncomingRequest.ShippedToCity = addressDetails.City;
                        }
                    }
                }
                if (UpdateDetailsWithAmountAndGlFromIncomingRequest.Amount != 0)
                {
                    returnList.Add(new OrderExcelExport
                    {
                        SiteExtKey = UpdateDetailsWithAmountAndGlFromIncomingRequest.SiteExtKey, //1
                        StoreStateProvince = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreStateProvince,//2
                        StoreZipCode = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreZipCode,//3
                        StoreCity = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreCity,//4
                        LegalEntity = UpdateDetailsWithAmountAndGlFromIncomingRequest.LegalEntity,//5
                        ShiptoState = UpdateDetailsWithAmountAndGlFromIncomingRequest.ShiptoState,//6
                        ShipToZipCode = UpdateDetailsWithAmountAndGlFromIncomingRequest.ShipToZipCode,//7
                        ShippedToCity = UpdateDetailsWithAmountAndGlFromIncomingRequest.ShippedToCity,//8
                        OrderId = UpdateDetailsWithAmountAndGlFromIncomingRequest.OrderId,//9
                        ProductName = productInfo.Name + "-DeliveryPickup",//10
                        paymentDate = UpdateDetailsWithAmountAndGlFromIncomingRequest.paymentDate,//11
                        PaymentType = UpdateDetailsWithAmountAndGlFromIncomingRequest.PaymentType,//12
                        VertexTaxAreaId = UpdateDetailsWithAmountAndGlFromIncomingRequest.ShiptoState + UpdateDetailsWithAmountAndGlFromIncomingRequest.ShipToZipCode,//13
                        SiteName = UpdateDetailsWithAmountAndGlFromIncomingRequest.SiteName,
                        Amount = UpdateDetailsWithAmountAndGlFromIncomingRequest.Amount,
                        GLCode = UpdateDetailsWithAmountAndGlFromIncomingRequest.GLCode,
                        storeid = UpdateDetailsWithAmountAndGlFromIncomingRequest.storeid

                    });
                }
                if (OrderDetails.OrderStatusId == 40 && !orderitems.IsPartialRefund && orderitems.FulfillmentDateTime == null) //Product is not fulfilled and is fully refunded
                {
                    if (UpdateDetailsWithAmountAndGlFromIncomingRequest.Amount != 0)
                    {
                        returnList.Add(new OrderExcelExport
                        {
                            SiteExtKey = UpdateDetailsWithAmountAndGlFromIncomingRequest.SiteExtKey, //1
                            StoreStateProvince = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreStateProvince,//2
                            StoreZipCode = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreZipCode,//3
                            StoreCity = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreCity,//4
                            LegalEntity = UpdateDetailsWithAmountAndGlFromIncomingRequest.LegalEntity,//5
                            ShiptoState = UpdateDetailsWithAmountAndGlFromIncomingRequest.ShiptoState,//6
                            ShipToZipCode = UpdateDetailsWithAmountAndGlFromIncomingRequest.ShipToZipCode,//7
                            ShippedToCity = UpdateDetailsWithAmountAndGlFromIncomingRequest.ShippedToCity,//8
                            OrderId = UpdateDetailsWithAmountAndGlFromIncomingRequest.OrderId,//9
                            ProductName = productInfo.Name + "-DeliveryPickup",//10
                            paymentDate = UpdateDetailsWithAmountAndGlFromIncomingRequest.paymentDate,//11
                            PaymentType = UpdateDetailsWithAmountAndGlFromIncomingRequest.PaymentType,//12
                            VertexTaxAreaId = UpdateDetailsWithAmountAndGlFromIncomingRequest.ShiptoState + UpdateDetailsWithAmountAndGlFromIncomingRequest.ShipToZipCode,//13
                            SiteName = UpdateDetailsWithAmountAndGlFromIncomingRequest.SiteName,
                            Amount = -UpdateDetailsWithAmountAndGlFromIncomingRequest.Amount,
                            GLCode = UpdateDetailsWithAmountAndGlFromIncomingRequest.GLCode,
                            storeid = UpdateDetailsWithAmountAndGlFromIncomingRequest.storeid

                        });
                    }
                }
            }
            return returnList;
        }

        #region "Vertex Rules"
        /// <summary>
        /// Entry point to apply the vertex rules to the order.
        /// </summary>
        /// <param name="vertexOrderTaxGls">The vertex related tax GLs for the order.</param>
        /// <param name="productInfo">The product info we are looking at</param>        
        /// <param name="orderitems">THe database orderitem</param>
        /// <param name="OrderDetails">The database order</param>
        /// <returns>A list containing the range of any additional records that are necessary to populate in the report.</returns>
        public IList<OrderExcelExport> ApplyVertexRelatedBatchRules(List<DynamicVertexGLCode> vertexOrderTaxGls, Product productInfo, OrderItem orderitems, Order OrderDetails)
        {
            var returnList = new List<OrderExcelExport>();
            if (vertexOrderTaxGls.Any())
            {
                //if (OrderDetails.Id == 101102)
                //{

                //}
                // UpdateDetailsWithAmountAndGlFromIncomingRequest.VertexTaxAreaId = UpdateDetailsWithAmountAndGlFromIncomingRequest.ShiptoState + UpdateDetailsWithAmountAndGlFromIncomingRequest.ShipToZipCode;
                ApplyVertexMealPlanRules(vertexOrderTaxGls, productInfo, orderitems, OrderDetails);
                ApplyVertexDeliveryRules(vertexOrderTaxGls, productInfo, orderitems, OrderDetails);
                ApplyVertexShippingRules(vertexOrderTaxGls, productInfo, orderitems, OrderDetails);
                ApplyVertexRulesForProductsWithNoOptions(vertexOrderTaxGls, productInfo, orderitems, OrderDetails);

                foreach (var vertexGl in vertexOrderTaxGls)
                {
                    UpdateDetailsWithAmountAndGlFromIncomingRequest.Amount = vertexGl.Total;
                    if (UpdateDetailsWithAmountAndGlFromIncomingRequest.Amount != 0)
                    {
                        returnList.Add(new OrderExcelExport
                        {
                            SiteExtKey = UpdateDetailsWithAmountAndGlFromIncomingRequest.SiteExtKey, //1
                            StoreStateProvince = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreStateProvince,//2
                            StoreZipCode = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreZipCode,//3
                            StoreCity = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreCity,//4
                            LegalEntity = UpdateDetailsWithAmountAndGlFromIncomingRequest.LegalEntity,//5
                            ShiptoState = UpdateDetailsWithAmountAndGlFromIncomingRequest.ShiptoState,//6
                            ShipToZipCode = UpdateDetailsWithAmountAndGlFromIncomingRequest.ShipToZipCode,//7
                            ShippedToCity = UpdateDetailsWithAmountAndGlFromIncomingRequest.ShippedToCity,//8
                            OrderId = UpdateDetailsWithAmountAndGlFromIncomingRequest.OrderId,//9
                            ProductName = (vertexGl.taxCode == "DELV" || vertexGl.taxCode == "SHIP") ? productInfo.Name + "-Delivery/Shipping Tax" : productInfo.Name + "-Product tax",//10
                            paymentDate = UpdateDetailsWithAmountAndGlFromIncomingRequest.paymentDate,//11
                            PaymentType = UpdateDetailsWithAmountAndGlFromIncomingRequest.PaymentType,//12
                            VertexTaxAreaId = productInfo.IsMealPlan ? UpdateDetailsWithAmountAndGlFromIncomingRequest.VertexTaxAreaId : UpdateDetailsWithAmountAndGlFromIncomingRequest.ShiptoState + UpdateDetailsWithAmountAndGlFromIncomingRequest.ShipToZipCode,//13,
                            SiteName = UpdateDetailsWithAmountAndGlFromIncomingRequest.SiteName,
                            Amount = UpdateDetailsWithAmountAndGlFromIncomingRequest.Amount,
                            GLCode = vertexGl.GlCode,
                            storeid = UpdateDetailsWithAmountAndGlFromIncomingRequest.storeid
                        });
                        if (OrderDetails.OrderStatusId == 40 && !orderitems.IsPartialRefund)
                        {
                            returnList.Add(new OrderExcelExport
                            {
                                SiteExtKey = UpdateDetailsWithAmountAndGlFromIncomingRequest.SiteExtKey, //1
                                StoreStateProvince = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreStateProvince,//2
                                StoreZipCode = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreZipCode,//3
                                StoreCity = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreCity,//4
                                LegalEntity = UpdateDetailsWithAmountAndGlFromIncomingRequest.LegalEntity,//5
                                ShiptoState = UpdateDetailsWithAmountAndGlFromIncomingRequest.ShiptoState,//6
                                ShipToZipCode = UpdateDetailsWithAmountAndGlFromIncomingRequest.ShipToZipCode,//7
                                ShippedToCity = UpdateDetailsWithAmountAndGlFromIncomingRequest.ShippedToCity,//8
                                OrderId = UpdateDetailsWithAmountAndGlFromIncomingRequest.OrderId,//9
                                ProductName = (vertexGl.taxCode == "DELV" || vertexGl.taxCode == "SHIP") ? productInfo.Name + "-Delivery/Shipping Tax" : productInfo.Name + "-Product tax",//10
                                paymentDate = UpdateDetailsWithAmountAndGlFromIncomingRequest.paymentDate,//11
                                PaymentType = UpdateDetailsWithAmountAndGlFromIncomingRequest.PaymentType,//12
                                VertexTaxAreaId = productInfo.IsMealPlan ? UpdateDetailsWithAmountAndGlFromIncomingRequest.VertexTaxAreaId : UpdateDetailsWithAmountAndGlFromIncomingRequest.ShiptoState + UpdateDetailsWithAmountAndGlFromIncomingRequest.ShipToZipCode,//13,
                                //VertexTaxAreaId = UpdateDetailsWithAmountAndGlFromIncomingRequest.ShiptoState + UpdateDetailsWithAmountAndGlFromIncomingRequest.ShipToZipCode,//13
                                SiteName = UpdateDetailsWithAmountAndGlFromIncomingRequest.SiteName,
                                Amount = -UpdateDetailsWithAmountAndGlFromIncomingRequest.Amount,
                                GLCode = vertexGl.GlCode,
                                storeid = UpdateDetailsWithAmountAndGlFromIncomingRequest.storeid

                            });
                        }
                    }

                }
            }
            return returnList;
        }

        public IList<OrderExcelExport> ApplyPartialRefundBusinessRules(Product productInfo, OrderItem orderitems, Order OrderDetails)
        {
            if (OrderDetails.Id == 101232 || OrderDetails.Id == 101235)
            {

            }
            var returnList = new List<OrderExcelExport>();

            if (orderitems.FulfillmentDateTime == null) // Partial refund data capture for Non-Fulfilled Order Items against the Base Gl of 2040 (Non Vendor) and 2041 (Vendor)
            {
                var partialTaxAndGlDetails = new Dictionary<string, decimal?>();
                var DeliveryTaxAndGlDetails = new Dictionary<string, decimal?>();
                decimal AmountForCost = 0;
                if (!string.IsNullOrEmpty(orderitems.TaxName1) && orderitems.TaxName1 != "None") { if (!partialTaxAndGlDetails.ContainsKey(Convert.ToString(orderitems.TaxName1.Split('-')[0]))) { partialTaxAndGlDetails.Add(Convert.ToString(orderitems.TaxName1.Split('-')[0]), orderitems.RefundedTaxAmount1); } }
                if (!string.IsNullOrEmpty(orderitems.TaxName2) && orderitems.TaxName2 != "None") { if (!partialTaxAndGlDetails.ContainsKey(Convert.ToString(orderitems.TaxName2.Split('-')[0]))) { partialTaxAndGlDetails.Add(Convert.ToString(orderitems.TaxName2.Split('-')[0]), orderitems.RefundedTaxAmount2); } }
                if (!string.IsNullOrEmpty(orderitems.TaxName3) && orderitems.TaxName3 != "None") { if (!partialTaxAndGlDetails.ContainsKey(Convert.ToString(orderitems.TaxName3.Split('-')[0]))) { partialTaxAndGlDetails.Add(Convert.ToString(orderitems.TaxName3.Split('-')[0]), orderitems.RefundedTaxAmount3); } }

                if (orderitems.IsDeliveryPickUp && orderitems.IsFullRefund) { if (orderitems.DeliveryPickupAmount != 0 && !DeliveryTaxAndGlDetails.ContainsKey("44571098")) { DeliveryTaxAndGlDetails.Add("44571098", orderitems.DelliveryTax); AmountForCost = orderitems.DeliveryPickupAmount; } }
                if (orderitems.IsShipping && orderitems.IsFullRefund) { if (orderitems.ShippingAmount != 0 && !DeliveryTaxAndGlDetails.ContainsKey("44571098")) { DeliveryTaxAndGlDetails.Add("44571098", orderitems.ShippingTax); AmountForCost = orderitems.ShippingAmount; } }

                returnList.Add(new OrderExcelExport
                {
                    SiteExtKey = UpdateDetailsWithAmountAndGlFromIncomingRequest.SiteExtKey, //1
                    StoreStateProvince = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreStateProvince,//2
                    StoreZipCode = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreZipCode,//3
                    StoreCity = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreCity,//4
                    LegalEntity = UpdateDetailsWithAmountAndGlFromIncomingRequest.LegalEntity,//5
                    ShiptoState = UpdateDetailsWithAmountAndGlFromIncomingRequest.ShiptoState,//6
                    ShipToZipCode = UpdateDetailsWithAmountAndGlFromIncomingRequest.ShipToZipCode,//7
                    ShippedToCity = UpdateDetailsWithAmountAndGlFromIncomingRequest.ShippedToCity,//8
                    OrderId = UpdateDetailsWithAmountAndGlFromIncomingRequest.OrderId,//9
                    ProductName = productInfo.Name + "-Partially-Refunded-Product Amount",//10
                    paymentDate = UpdateDetailsWithAmountAndGlFromIncomingRequest.paymentDate,//11
                    PaymentType = UpdateDetailsWithAmountAndGlFromIncomingRequest.PaymentType,//12
                    VertexTaxAreaId = UpdateDetailsWithAmountAndGlFromIncomingRequest.ShiptoState + UpdateDetailsWithAmountAndGlFromIncomingRequest.ShipToZipCode,//13
                    SiteName = UpdateDetailsWithAmountAndGlFromIncomingRequest.SiteName,
                    Amount = -(orderitems.RefundedGlAmount1),
                    GLCode = productInfo.VendorId != 0 ? "48702041" : "48702040",
                    storeid = UpdateDetailsWithAmountAndGlFromIncomingRequest.storeid
                });
                if (orderitems.IsDeliveryPickUp && orderitems.IsFullRefund)
                {
                    returnList.Add(new OrderExcelExport
                    {
                        SiteExtKey = UpdateDetailsWithAmountAndGlFromIncomingRequest.SiteExtKey, //1
                        StoreStateProvince = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreStateProvince,//2
                        StoreZipCode = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreZipCode,//3
                        StoreCity = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreCity,//4
                        LegalEntity = UpdateDetailsWithAmountAndGlFromIncomingRequest.LegalEntity,//5
                        ShiptoState = UpdateDetailsWithAmountAndGlFromIncomingRequest.ShiptoState,//6
                        ShipToZipCode = UpdateDetailsWithAmountAndGlFromIncomingRequest.ShipToZipCode,//7
                        ShippedToCity = UpdateDetailsWithAmountAndGlFromIncomingRequest.ShippedToCity,//8
                        OrderId = UpdateDetailsWithAmountAndGlFromIncomingRequest.OrderId,//9
                        ProductName = productInfo.Name + "-Partially-Refunded-Product Delivery/Shipping Amount",//10
                        paymentDate = UpdateDetailsWithAmountAndGlFromIncomingRequest.paymentDate,//11
                        PaymentType = UpdateDetailsWithAmountAndGlFromIncomingRequest.PaymentType,//12
                        VertexTaxAreaId = UpdateDetailsWithAmountAndGlFromIncomingRequest.ShiptoState + UpdateDetailsWithAmountAndGlFromIncomingRequest.ShipToZipCode,//13
                        SiteName = UpdateDetailsWithAmountAndGlFromIncomingRequest.SiteName,
                        Amount = -(orderitems.DeliveryPickupAmount),
                        GLCode = "48702040",
                        storeid = UpdateDetailsWithAmountAndGlFromIncomingRequest.storeid
                    });
                }
                if (orderitems.IsShipping && orderitems.IsFullRefund)
                {
                    returnList.Add(new OrderExcelExport
                    {
                        SiteExtKey = UpdateDetailsWithAmountAndGlFromIncomingRequest.SiteExtKey, //1
                        StoreStateProvince = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreStateProvince,//2
                        StoreZipCode = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreZipCode,//3
                        StoreCity = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreCity,//4
                        LegalEntity = UpdateDetailsWithAmountAndGlFromIncomingRequest.LegalEntity,//5
                        ShiptoState = UpdateDetailsWithAmountAndGlFromIncomingRequest.ShiptoState,//6
                        ShipToZipCode = UpdateDetailsWithAmountAndGlFromIncomingRequest.ShipToZipCode,//7
                        ShippedToCity = UpdateDetailsWithAmountAndGlFromIncomingRequest.ShippedToCity,//8
                        OrderId = UpdateDetailsWithAmountAndGlFromIncomingRequest.OrderId,//9
                        ProductName = productInfo.Name + "-Partially-Refunded-Product Delivery/Shipping Amount",//10
                        paymentDate = UpdateDetailsWithAmountAndGlFromIncomingRequest.paymentDate,//11
                        PaymentType = UpdateDetailsWithAmountAndGlFromIncomingRequest.PaymentType,//12
                        VertexTaxAreaId = UpdateDetailsWithAmountAndGlFromIncomingRequest.ShiptoState + UpdateDetailsWithAmountAndGlFromIncomingRequest.ShipToZipCode,//13
                        SiteName = UpdateDetailsWithAmountAndGlFromIncomingRequest.SiteName,
                        Amount = -(orderitems.ShippingAmount),
                        GLCode = "48702040",
                        storeid = UpdateDetailsWithAmountAndGlFromIncomingRequest.storeid
                    });
                }
                foreach (var items in partialTaxAndGlDetails)
                {
                    if (items.Value != 0)
                    {

                        returnList.Add(new OrderExcelExport
                        {
                            SiteExtKey = UpdateDetailsWithAmountAndGlFromIncomingRequest.SiteExtKey, //1
                            StoreStateProvince = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreStateProvince,//2
                            StoreZipCode = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreZipCode,//3
                            StoreCity = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreCity,//4
                            LegalEntity = UpdateDetailsWithAmountAndGlFromIncomingRequest.LegalEntity,//5
                            ShiptoState = UpdateDetailsWithAmountAndGlFromIncomingRequest.ShiptoState,//6
                            ShipToZipCode = UpdateDetailsWithAmountAndGlFromIncomingRequest.ShipToZipCode,//7
                            ShippedToCity = UpdateDetailsWithAmountAndGlFromIncomingRequest.ShippedToCity,//8
                            OrderId = UpdateDetailsWithAmountAndGlFromIncomingRequest.OrderId,//9
                            ProductName = productInfo.Name + "-Partially-Refunded-Product Tax",//10
                            paymentDate = UpdateDetailsWithAmountAndGlFromIncomingRequest.paymentDate,//11
                            PaymentType = UpdateDetailsWithAmountAndGlFromIncomingRequest.PaymentType,//12
                            VertexTaxAreaId = UpdateDetailsWithAmountAndGlFromIncomingRequest.ShiptoState + UpdateDetailsWithAmountAndGlFromIncomingRequest.ShipToZipCode,//13
                            SiteName = UpdateDetailsWithAmountAndGlFromIncomingRequest.SiteName,
                            Amount = items.Value, // setting the tax amount refunded 
                            GLCode = items.Key, // setting the tax GL against which the tax is refunded
                            storeid = UpdateDetailsWithAmountAndGlFromIncomingRequest.storeid
                        });
                    }
                }

                foreach (var delvShipTax in DeliveryTaxAndGlDetails)
                {
                    if (delvShipTax.Value != 0)
                    {

                        returnList.Add(new OrderExcelExport
                        {
                            SiteExtKey = UpdateDetailsWithAmountAndGlFromIncomingRequest.SiteExtKey, //1
                            StoreStateProvince = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreStateProvince,//2
                            StoreZipCode = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreZipCode,//3
                            StoreCity = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreCity,//4
                            LegalEntity = UpdateDetailsWithAmountAndGlFromIncomingRequest.LegalEntity,//5
                            ShiptoState = UpdateDetailsWithAmountAndGlFromIncomingRequest.ShiptoState,//6
                            ShipToZipCode = UpdateDetailsWithAmountAndGlFromIncomingRequest.ShipToZipCode,//7
                            ShippedToCity = UpdateDetailsWithAmountAndGlFromIncomingRequest.ShippedToCity,//8
                            OrderId = UpdateDetailsWithAmountAndGlFromIncomingRequest.OrderId,//9
                            ProductName = productInfo.Name + "-Partially-Refunded-Delivery/Shipping Tax",//10
                            paymentDate = UpdateDetailsWithAmountAndGlFromIncomingRequest.paymentDate,//11
                            PaymentType = UpdateDetailsWithAmountAndGlFromIncomingRequest.PaymentType,//12
                            VertexTaxAreaId = UpdateDetailsWithAmountAndGlFromIncomingRequest.ShiptoState + UpdateDetailsWithAmountAndGlFromIncomingRequest.ShipToZipCode,//13
                            SiteName = UpdateDetailsWithAmountAndGlFromIncomingRequest.SiteName,
                            Amount = delvShipTax.Value, // setting the tax amount refunded 
                            GLCode = delvShipTax.Key, // setting the tax GL against which the tax is refunded
                            storeid = UpdateDetailsWithAmountAndGlFromIncomingRequest.storeid
                        });
                    }
                }
            }
            else if (orderitems.FulfillmentDateTime != null) // Partial Refund for Items which has been Fulfuilled
            {
                var PartialRefundGlDetailsFulfilled = new Dictionary<string, decimal?>();
                var partialRefundTaxDetailsFulfilled = new Dictionary<string, decimal?>();
                var partialRefundDelvShipGlDetailsFulfilled = new Dictionary<string, decimal?>();
                var partialRefundDelvShipTaxDetailsFulfilled = new Dictionary<string, decimal?>();

                if (!string.IsNullOrEmpty(orderitems.TaxName1) && orderitems.TaxName1 != "None" && (orderitems.RefundedTaxAmount1 != null && orderitems.RefundedTaxAmount1 != 0)) { if (!partialRefundTaxDetailsFulfilled.ContainsKey(Convert.ToString(orderitems.TaxName1 + "_1"))) { partialRefundTaxDetailsFulfilled.Add(Convert.ToString(orderitems.TaxName1.Split('-')[0]), orderitems.RefundedTaxAmount1); } }
                if (!string.IsNullOrEmpty(orderitems.TaxName2) && orderitems.TaxName2 != "None" && (orderitems.RefundedTaxAmount1 != null && orderitems.RefundedTaxAmount1 != 0)) { if (!partialRefundTaxDetailsFulfilled.ContainsKey(Convert.ToString(orderitems.TaxName2 + "_2"))) { partialRefundTaxDetailsFulfilled.Add(Convert.ToString(orderitems.TaxName2.Split('-')[0]), orderitems.RefundedTaxAmount2); } }
                if (!string.IsNullOrEmpty(orderitems.TaxName3) && orderitems.TaxName3 != "None" && (orderitems.RefundedTaxAmount1 != null && orderitems.RefundedTaxAmount1 != 0)) { if (!partialRefundTaxDetailsFulfilled.ContainsKey(Convert.ToString(orderitems.TaxName3 + "_3"))) { partialRefundTaxDetailsFulfilled.Add(Convert.ToString(orderitems.TaxName3.Split('-')[0]), orderitems.RefundedTaxAmount3); } }

                if (!orderitems.Product.Name.Contains("Bonus"))
                {

                    if (!string.IsNullOrEmpty(orderitems.GLCodeName1) && orderitems.GLCodeName1 != "None" && (orderitems.RefundedGlAmount1 != null && orderitems.RefundedGlAmount1 != 0)) { if (!PartialRefundGlDetailsFulfilled.ContainsKey(Convert.ToString(orderitems.GLCodeName2 + "_1"))) { PartialRefundGlDetailsFulfilled.Add(Convert.ToString(orderitems.GLCodeName1.Split('-')[0]), orderitems.RefundedGlAmount1); } }
                    if (!string.IsNullOrEmpty(orderitems.GLCodeName2) && orderitems.GLCodeName2 != "None" && (orderitems.RefundedGlAmount2 != null && orderitems.RefundedGlAmount2 != 0)) { if (!PartialRefundGlDetailsFulfilled.ContainsKey(Convert.ToString(orderitems.GLCodeName2 + "_2"))) { PartialRefundGlDetailsFulfilled.Add(Convert.ToString(orderitems.GLCodeName2.Split('-')[0]), orderitems.RefundedGlAmount2); } }
                    if (!string.IsNullOrEmpty(orderitems.GLCodeName3) && orderitems.GLCodeName3 != "None" && (orderitems.RefundedGlAmount3 != null && orderitems.RefundedGlAmount3 != 0)) { if (!PartialRefundGlDetailsFulfilled.ContainsKey(Convert.ToString(orderitems.GLCodeName3 + "_3"))) { PartialRefundGlDetailsFulfilled.Add(Convert.ToString(orderitems.GLCodeName3.Split('-')[0]), orderitems.RefundedGlAmount3); } }

                }
                else
                {
                    if (!string.IsNullOrEmpty(orderitems.GLCodeName1) && orderitems.GLCodeName1 != "None" && (orderitems.RefundedGlAmount1 != null && orderitems.RefundedGlAmount1 != 0)) { if (!PartialRefundGlDetailsFulfilled.ContainsKey(Convert.ToString(orderitems.GLCodeName2 + "_1"))) { PartialRefundGlDetailsFulfilled.Add(Convert.ToString(orderitems.GLCodeName1.Split('-')[0]), orderitems.RefundedGlAmount1); } }
                    if (orderitems.GlcodeAmount2 > 0)
                    {
                        if (!string.IsNullOrEmpty(orderitems.GLCodeName2) && orderitems.GLCodeName2 != "None" && (orderitems.RefundedGlAmount2 != null && orderitems.RefundedGlAmount2 != 0)) { if (!PartialRefundGlDetailsFulfilled.ContainsKey(Convert.ToString(orderitems.GLCodeName2 + "_2"))) { PartialRefundGlDetailsFulfilled.Add(Convert.ToString(orderitems.GLCodeName2.Split('-')[0]), orderitems.RefundedGlAmount2); } }

                    }
                    if (orderitems.GlcodeAmount3 > 0)
                    {
                        if (!string.IsNullOrEmpty(orderitems.GLCodeName3) && orderitems.GLCodeName3 != "None" && (orderitems.RefundedGlAmount3 != null && orderitems.RefundedGlAmount3 != 0)) { if (!PartialRefundGlDetailsFulfilled.ContainsKey(Convert.ToString(orderitems.GLCodeName3 + "_3"))) { PartialRefundGlDetailsFulfilled.Add(Convert.ToString(orderitems.GLCodeName3.Split('-')[0]), orderitems.RefundedGlAmount3); } }
                    }
                    //if (!string.IsNullOrEmpty(orderitems.GLCodeName2) && orderitems.GLCodeName2 != "None" && (orderitems.GlcodeAmount2 < 0)) { if (!PartialRefundGlDetailsFulfilled.ContainsKey(Convert.ToString(orderitems.GLCodeName2 + "_2"))) { PartialRefundGlDetailsFulfilled.Add(Convert.ToString(orderitems.GLCodeName2.Split('-')[0]), orderitems.GlcodeAmount2 < 0 ? orderitems.GlcodeAmount2 : orderitems.RefundedGlAmount2); } }
                    //if (!string.IsNullOrEmpty(orderitems.GLCodeName3) && orderitems.GLCodeName3 != "None" && (orderitems.GlcodeAmount3 < 0)) { if (!PartialRefundGlDetailsFulfilled.ContainsKey(Convert.ToString(orderitems.GLCodeName3 + "_3"))) { PartialRefundGlDetailsFulfilled.Add(Convert.ToString(orderitems.GLCodeName3.Split('-')[0]), orderitems.GlcodeAmount3 < 0 ? orderitems.GlcodeAmount3 : orderitems.RefundedGlAmount3); } }

                }
                if (orderitems.IsDeliveryPickUp && orderitems.IsFullRefund) { if (Convert.ToDecimal(orderitems.DeliveryPickupAmount) != 0 && !partialRefundDelvShipTaxDetailsFulfilled.ContainsKey("44571098")) { partialRefundDelvShipTaxDetailsFulfilled.Add("44571098", Convert.ToDecimal(orderitems.DelliveryTax)); } }
                if (orderitems.IsShipping && orderitems.IsFullRefund) { if (Convert.ToDecimal(orderitems.ShippingAmount) != 0 && !partialRefundDelvShipTaxDetailsFulfilled.ContainsKey("44571098")) { partialRefundDelvShipTaxDetailsFulfilled.Add("44571098", Convert.ToDecimal(orderitems.ShippingTax)); } }

                if (orderitems.IsDeliveryPickUp && orderitems.IsFullRefund) { if (Convert.ToDecimal(orderitems.DeliveryPickupAmount) != 0 && !partialRefundDelvShipGlDetailsFulfilled.ContainsKey("70161010")) { partialRefundDelvShipGlDetailsFulfilled.Add("70161010", Convert.ToDecimal(orderitems.DeliveryPickupAmount)); } }
                if (orderitems.IsShipping && orderitems.IsFullRefund) { if (Convert.ToDecimal(orderitems.ShippingAmount) != 0 && !partialRefundDelvShipGlDetailsFulfilled.ContainsKey("62610001")) { partialRefundDelvShipGlDetailsFulfilled.Add("62610001", Convert.ToDecimal(orderitems.ShippingAmount)); } }

                foreach (var items in PartialRefundGlDetailsFulfilled) //Adding all the Revenue GL in the Report once Partially Fulfilled
                {
                    returnList.Add(new OrderExcelExport
                    {
                        SiteExtKey = UpdateDetailsWithAmountAndGlFromIncomingRequest.SiteExtKey, //1
                        StoreStateProvince = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreStateProvince,//2
                        StoreZipCode = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreZipCode,//3
                        StoreCity = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreCity,//4
                        LegalEntity = UpdateDetailsWithAmountAndGlFromIncomingRequest.LegalEntity,//5
                        ShiptoState = (productInfo.IsMealPlan || productInfo.IsDonation) ? UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreStateProvince : UpdateDetailsWithAmountAndGlFromIncomingRequest.ShiptoState,//6
                        ShipToZipCode = (productInfo.IsMealPlan || productInfo.IsDonation) ? UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreZipCode : UpdateDetailsWithAmountAndGlFromIncomingRequest.ShipToZipCode,//7
                        ShippedToCity = (productInfo.IsMealPlan || productInfo.IsDonation) ? UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreCity : UpdateDetailsWithAmountAndGlFromIncomingRequest.ShippedToCity,//8
                        OrderId = UpdateDetailsWithAmountAndGlFromIncomingRequest.OrderId,//9
                        ProductName = productInfo.Name + "-Partially-Refunded-Product Amount",//10
                        paymentDate = UpdateDetailsWithAmountAndGlFromIncomingRequest.paymentDate,//11
                        PaymentType = UpdateDetailsWithAmountAndGlFromIncomingRequest.PaymentType,//12
                        VertexTaxAreaId = (productInfo.IsMealPlan || productInfo.IsDonation) ? (UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreStateProvince + UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreZipCode) : (UpdateDetailsWithAmountAndGlFromIncomingRequest.ShiptoState + UpdateDetailsWithAmountAndGlFromIncomingRequest.ShipToZipCode),//13
                        SiteName = UpdateDetailsWithAmountAndGlFromIncomingRequest.SiteName,
                        Amount = -items.Value, // setting the tax amount refunded 
                        GLCode = items.Key.Split('-')[0], // setting the tax GL against which the tax is refunded
                        storeid = UpdateDetailsWithAmountAndGlFromIncomingRequest.storeid
                    });
                }
                foreach (var items in partialRefundTaxDetailsFulfilled)
                {
                    if (items.Value != 0)
                    {
                        returnList.Add(new OrderExcelExport
                        {
                            SiteExtKey = UpdateDetailsWithAmountAndGlFromIncomingRequest.SiteExtKey, //1
                            StoreStateProvince = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreStateProvince,//2
                            StoreZipCode = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreZipCode,//3
                            StoreCity = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreCity,//4
                            LegalEntity = UpdateDetailsWithAmountAndGlFromIncomingRequest.LegalEntity,//5
                            ShiptoState = (productInfo.IsMealPlan || productInfo.IsDonation) ? UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreStateProvince : UpdateDetailsWithAmountAndGlFromIncomingRequest.ShiptoState,//6
                            ShipToZipCode = (productInfo.IsMealPlan || productInfo.IsDonation) ? UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreZipCode : UpdateDetailsWithAmountAndGlFromIncomingRequest.ShipToZipCode,//7
                            ShippedToCity = (productInfo.IsMealPlan || productInfo.IsDonation) ? UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreCity : UpdateDetailsWithAmountAndGlFromIncomingRequest.ShippedToCity,//8
                            OrderId = UpdateDetailsWithAmountAndGlFromIncomingRequest.OrderId,//9
                            ProductName = productInfo.Name + "-Partially-Refunded-Product Tax",//10
                            paymentDate = UpdateDetailsWithAmountAndGlFromIncomingRequest.paymentDate,//11
                            PaymentType = UpdateDetailsWithAmountAndGlFromIncomingRequest.PaymentType,//12
                            VertexTaxAreaId = (productInfo.IsMealPlan || productInfo.IsDonation) ? (UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreStateProvince + UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreZipCode) : (UpdateDetailsWithAmountAndGlFromIncomingRequest.ShiptoState + UpdateDetailsWithAmountAndGlFromIncomingRequest.ShipToZipCode),//13
                            SiteName = UpdateDetailsWithAmountAndGlFromIncomingRequest.SiteName,
                            Amount = items.Value, // setting the tax amount refunded 
                            GLCode = items.Key.Split('-')[0], // setting the tax GL against which the tax is refunded
                            storeid = UpdateDetailsWithAmountAndGlFromIncomingRequest.storeid
                        });
                    }
                }

                foreach (var items in partialRefundDelvShipGlDetailsFulfilled)
                {
                    if (items.Value != 0)
                    {
                        returnList.Add(new OrderExcelExport
                        {
                            SiteExtKey = UpdateDetailsWithAmountAndGlFromIncomingRequest.SiteExtKey, //1
                            StoreStateProvince = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreStateProvince,//2
                            StoreZipCode = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreZipCode,//3
                            StoreCity = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreCity,//4
                            LegalEntity = UpdateDetailsWithAmountAndGlFromIncomingRequest.LegalEntity,//5
                            ShiptoState = (productInfo.IsMealPlan || productInfo.IsDonation) ? UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreStateProvince : UpdateDetailsWithAmountAndGlFromIncomingRequest.ShiptoState,//6
                            ShipToZipCode = (productInfo.IsMealPlan || productInfo.IsDonation) ? UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreZipCode : UpdateDetailsWithAmountAndGlFromIncomingRequest.ShipToZipCode,//7
                            ShippedToCity = (productInfo.IsMealPlan || productInfo.IsDonation) ? UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreCity : UpdateDetailsWithAmountAndGlFromIncomingRequest.ShippedToCity,//8
                            OrderId = UpdateDetailsWithAmountAndGlFromIncomingRequest.OrderId,//9
                            ProductName = productInfo.Name + "-Partially-Refunded-Product Delivery/Shipping Amount",//10
                            paymentDate = UpdateDetailsWithAmountAndGlFromIncomingRequest.paymentDate,//11
                            PaymentType = UpdateDetailsWithAmountAndGlFromIncomingRequest.PaymentType,//12
                            VertexTaxAreaId = (productInfo.IsMealPlan || productInfo.IsDonation) ? (UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreStateProvince + UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreZipCode) : (UpdateDetailsWithAmountAndGlFromIncomingRequest.ShiptoState + UpdateDetailsWithAmountAndGlFromIncomingRequest.ShipToZipCode),//13
                            SiteName = UpdateDetailsWithAmountAndGlFromIncomingRequest.SiteName,
                            Amount = -items.Value, // setting the tax amount refunded 
                            GLCode = items.Key, // setting the tax GL against which the tax is refunded
                            storeid = UpdateDetailsWithAmountAndGlFromIncomingRequest.storeid
                        });
                    }
                }
                foreach (var items in partialRefundDelvShipTaxDetailsFulfilled)
                {
                    if (items.Value != 0)
                    {
                        returnList.Add(new OrderExcelExport
                        {
                            SiteExtKey = UpdateDetailsWithAmountAndGlFromIncomingRequest.SiteExtKey, //1
                            StoreStateProvince = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreStateProvince,//2
                            StoreZipCode = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreZipCode,//3
                            StoreCity = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreCity,//4
                            LegalEntity = UpdateDetailsWithAmountAndGlFromIncomingRequest.LegalEntity,//5
                            ShiptoState = (productInfo.IsMealPlan || productInfo.IsDonation) ? UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreStateProvince : UpdateDetailsWithAmountAndGlFromIncomingRequest.ShiptoState,//6
                            ShipToZipCode = (productInfo.IsMealPlan || productInfo.IsDonation) ? UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreZipCode : UpdateDetailsWithAmountAndGlFromIncomingRequest.ShipToZipCode,//7
                            ShippedToCity = (productInfo.IsMealPlan || productInfo.IsDonation) ? UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreCity : UpdateDetailsWithAmountAndGlFromIncomingRequest.ShippedToCity,//8
                            OrderId = UpdateDetailsWithAmountAndGlFromIncomingRequest.OrderId,//9
                            ProductName = productInfo.Name + "-Partially-Refunded-Product Delivery/Shipping Tax",//10
                            paymentDate = UpdateDetailsWithAmountAndGlFromIncomingRequest.paymentDate,//11
                            PaymentType = UpdateDetailsWithAmountAndGlFromIncomingRequest.PaymentType,//12
                            VertexTaxAreaId = (productInfo.IsMealPlan || productInfo.IsDonation) ? (UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreStateProvince + UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreZipCode) : (UpdateDetailsWithAmountAndGlFromIncomingRequest.ShiptoState + UpdateDetailsWithAmountAndGlFromIncomingRequest.ShipToZipCode),//13
                            SiteName = UpdateDetailsWithAmountAndGlFromIncomingRequest.SiteName,
                            Amount = items.Value, // setting the tax amount refunded 
                            GLCode = items.Key, // setting the tax GL against which the tax is refunded
                            storeid = UpdateDetailsWithAmountAndGlFromIncomingRequest.storeid
                        });
                    }
                }
            }
            return returnList;
        }

        public void ApplyVertexMealPlanRules(List<DynamicVertexGLCode> vertexOrderTaxGls, Product productInfo, OrderItem orderitems, Order OrderDetails)
        {
            if (productInfo.IsMealPlan)
            {
                // If the product is a meal plan, we replace the store details on the report with the store location
                var storedetails = GetStore(OrderDetails.StoreId);
                if (storedetails != null)
                {
                    UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreZipCode = storedetails.CompanyZipPostalCode;
                    UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreCity = storedetails.CompanyCity;
                    var stateProvinceDetails = _stateProvinceRepository.GetById(storedetails.CompanyStateProvinceId);
                    UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreStateProvince = stateProvinceDetails.Abbreviation;

                    UpdateDetailsWithAmountAndGlFromIncomingRequest.ShiptoState = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreStateProvince;
                    UpdateDetailsWithAmountAndGlFromIncomingRequest.ShipToZipCode = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreZipCode;
                    UpdateDetailsWithAmountAndGlFromIncomingRequest.ShippedToCity = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreCity;

                    UpdateDetailsWithAmountAndGlFromIncomingRequest.VertexTaxAreaId = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreStateProvince + UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreZipCode;
                }

            }

        }

        public void ApplyVertexDeliveryRules(List<DynamicVertexGLCode> vertexOrderTaxGls, Product productInfo, OrderItem orderitems, Order OrderDetails)
        {
            if (orderitems.IsDeliveryPickUp)
            {
                var warehouseDetails = _warehouseRepository.GetById(orderitems.SelectedFulfillmentWarehouseId);
                if (warehouseDetails != null)
                {
                    var addressDetails = _addressRepository.GetById(warehouseDetails.AddressId);
                    if (addressDetails != null)
                    {

                        UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreStateProvince = addressDetails.StateProvince.Abbreviation;
                        UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreZipCode = addressDetails.ZipPostalCode;
                        UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreCity = addressDetails.City;

                        if (orderitems.SelectedShippingMethodName == "Delivery")
                        {
                            var ShiptoAddressDetsils = _addressRepository.GetById(OrderDetails.ShippingAddressId);
                            warehouseDetails = _warehouseRepository.GetById(productInfo.WarehouseId);
                            var storedetails = GetStore(OrderDetails.StoreId);

                            if (warehouseDetails != null)
                            {
                                addressDetails = _addressRepository.GetById(warehouseDetails.AddressId);
                                if (addressDetails != null)
                                {
                                    UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreStateProvince = addressDetails.StateProvince.Abbreviation;
                                    UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreZipCode = addressDetails.ZipPostalCode;
                                    UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreCity = addressDetails.City;
                                }
                            }
                            else
                            {
                                UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreZipCode = storedetails.CompanyZipPostalCode;
                                UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreCity = storedetails.CompanyCity;
                                var stateProvinceDetails = _stateProvinceRepository.GetById(storedetails.CompanyStateProvinceId);
                                UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreStateProvince = stateProvinceDetails.Abbreviation;
                            }

                            UpdateDetailsWithAmountAndGlFromIncomingRequest.ShiptoState = ShiptoAddressDetsils.StateProvince.Abbreviation;
                            UpdateDetailsWithAmountAndGlFromIncomingRequest.ShipToZipCode = ShiptoAddressDetsils.ZipPostalCode;
                            UpdateDetailsWithAmountAndGlFromIncomingRequest.ShippedToCity = ShiptoAddressDetsils.City;
                        }
                        else
                        {
                            UpdateDetailsWithAmountAndGlFromIncomingRequest.ShiptoState = addressDetails.StateProvince.Abbreviation;
                            UpdateDetailsWithAmountAndGlFromIncomingRequest.ShipToZipCode = addressDetails.ZipPostalCode;
                            UpdateDetailsWithAmountAndGlFromIncomingRequest.ShippedToCity = addressDetails.City;
                        }
                    }
                }
            }
        }

        public void ApplyVertexShippingRules(List<DynamicVertexGLCode> vertexOrderTaxGls, Product productInfo, OrderItem orderitems, Order OrderDetails)
        {
            if (orderitems.IsShipping)
            {
                var warehouseDetails = _warehouseRepository.GetById(productInfo.WarehouseId);
                var shiptoDetails = _addressRepository.GetById(OrderDetails.ShippingAddressId);

                if (warehouseDetails == null)
                {
                    var storedetails = GetStore(OrderDetails.StoreId);
                    if (storedetails != null)
                    {
                        UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreZipCode = storedetails.CompanyZipPostalCode;
                        UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreCity = storedetails.CompanyCity;
                        var stateProvinceDetails = _stateProvinceRepository.GetById(storedetails.CompanyStateProvinceId);
                        UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreStateProvince = stateProvinceDetails.Abbreviation;
                    }
                }

                if (warehouseDetails != null)
                {
                    var addressDetails = _addressRepository.GetById(warehouseDetails.AddressId);
                    if (addressDetails != null)
                    {

                        UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreStateProvince = addressDetails.StateProvince.Abbreviation;
                        UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreZipCode = addressDetails.ZipPostalCode;
                        UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreCity = addressDetails.City;
                    }
                }
                UpdateDetailsWithAmountAndGlFromIncomingRequest.ShiptoState = shiptoDetails.StateProvince.Abbreviation;
                UpdateDetailsWithAmountAndGlFromIncomingRequest.ShipToZipCode = shiptoDetails.ZipPostalCode;
                UpdateDetailsWithAmountAndGlFromIncomingRequest.ShippedToCity = shiptoDetails.City;
            }

        }
        #endregion

        public void ApplyVertexRulesForProductsWithNoOptions(List<DynamicVertexGLCode> vertexOrderTaxGls, Product productInfo, OrderItem orderitems, Order OrderDetails)
        {
            if (!productInfo.IsMealPlan && !productInfo.IsDonation && !productInfo.IsDownload)
            {
                if (!productInfo.IsLocalDelivery && !productInfo.IsPickupEnabled && !productInfo.IsShipEnabled)
                {
                    UpdateDetailsWithAmountAndGlFromIncomingRequest.ShiptoState = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreStateProvince;
                    UpdateDetailsWithAmountAndGlFromIncomingRequest.ShipToZipCode = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreZipCode;
                    UpdateDetailsWithAmountAndGlFromIncomingRequest.ShippedToCity = UpdateDetailsWithAmountAndGlFromIncomingRequest.StoreCity;
                }
            }
        }

        public IList<OrderExcelExport> ApplyJournalEntryBusinessRules(IList<OrderExcelExport> journalReps)
        {
            var disticntOrdersToProcess = journalReps.Select(x => new { x.OrderId }).Distinct(); // Fetching Disticnt orders to process the orderitems for GLs and amount.
            List<OrderExcelExport> newListToExport = new List<OrderExcelExport>();

            foreach (var itemstoProcessForReports in disticntOrdersToProcess)
            {
                var getOrderItemsbyOrdeID = _orderService.GetOrderItemsByOrderID(itemstoProcessForReports.OrderId); // Fetching all the OrderItems by order iD to process the records for JE.
                UpdateDetailsWithAmountAndGlFromIncomingRequest = journalReps.Where(x => x.OrderId == itemstoProcessForReports.OrderId).FirstOrDefault(); // Fetching the address Details to be shown for each orderitem to process 
                var OrderDetails = _orderService.GetOrderById(itemstoProcessForReports.OrderId);//Fetching the orderDetails

                if (OrderDetails.OrderStatus == OrderStatus.Pending || getOrderItemsbyOrdeID == null) // Do not Process any Records that has orderstatus as pending or Missing orderitems.
                    return null;
                //Section to Process for Reports for each orderItems identified/Retrieved by orderid
                foreach (var orderitems in getOrderItemsbyOrdeID)
                {
                    //Section to Get Tax For Each orderItems
                    var vertexOrderTaxGls = _glCodeService.GetVertexGlBreakdown(orderitems.OrderId, orderitems.ProductId, TaxRequestType.ConfirmDistributiveTax); //gets the order gls from vertex response
                    //Getting the Product Details info for Use.
                    var productInfo = _productRepository.GetById(orderitems.ProductId);

                    // process the vendor related business rules for the report.
                    newListToExport.AddRange(ApplyVendorRules(productInfo, orderitems, OrderDetails));

                    // process the meal plan and donation rules for the report.
                    newListToExport.AddRange(ApplyMealPlanAndDonationRules(productInfo, orderitems, OrderDetails));

                    // process the shipping rules for the report.
                    newListToExport.AddRange(ApplyShippingRules(productInfo, orderitems, OrderDetails));

                    // process pickup and local delivery rules for the report.
                    newListToExport.AddRange(ApplyDeliveryRules(productInfo, orderitems, OrderDetails));

                    //Section to Insert the Tax Amount From Vertex.
                    newListToExport.AddRange(ApplyVertexRelatedBatchRules(vertexOrderTaxGls, productInfo, orderitems, OrderDetails));
                }
            }
            return newListToExport.OrderByDescending(x => x.OrderId).ToList();
        }

        public IList<OrderExcelExport> JournalReportForExcel(DateTime? startTime, DateTime? endTime, int storeId)
        {
            var journalRecords = GetStoreSpecificJournalRecords(startTime, endTime, storeId);
            var preparedRecords = ApplyJournalEntryBusinessRules(journalRecords);
            return preparedRecords;
        }
        public IList<OrderExcelExport> JournalReportForExcel(DateTime? startTime, DateTime? endTime)
        {
            var journalRecords = GetAllJournalRecords(startTime, endTime);
            var preparedRecords = ApplyJournalEntryBusinessRules(journalRecords);
            return preparedRecords;
        }

        public IList<OrderExcelExport> GetAllStoresJournalDeliveryReportForExcel(DateTime? startTime, DateTime? endTime)
        {
            var query = (from s in _storeRepository.Table
                         join Glc in this._productGlCodeCalculationRepository.Table on s.Id equals Glc.StoreId
                         join SP in this._stateProvinceRepository.Table on s.CompanyStateProvinceId equals SP.Id
                         join OD in this._orderRepository.Table on Glc.OrderId equals OD.Id
                         join AD in this._addressRepository.Table on OD.BillingAddressId equals AD.Id //change it to shipping AddressID
                         join sp1 in this._stateProvinceRepository.Table on AD.StateProvinceId equals sp1.Id
                         join P in this._productRepository.Table on Glc.ProductId equals P.Id
                         join GN in this._genericRepository.Table on Glc.OrderId equals GN.EntityId
                         join OT in this._orderItemRepository.Table on new { a = Glc.ProductId, b = Glc.OrderId } equals new { a = OT.ProductId, b = OT.OrderId }
                         join GL in this._glCode.Table on Glc.GLCodeId equals GL.Id into abc
                         from data in abc.DefaultIfEmpty()
                         where Glc.Processed == false && Glc.GlStatusType == 2 && OT.FulfillmentDateTime != null &&
                         startTime.Value <= OT.FulfillmentDateTime && endTime.Value >= OT.FulfillmentDateTime && !OD.Deleted
                         select new OrderExcelExport()
                         {
                             Id = Glc.Id,
                             SiteExtKey = s.ExtKey,
                             StoreStateProvince = SP.Abbreviation,
                             StoreZipCode = s.CompanyZipPostalCode,
                             StoreCity = s.CompanyCity,
                             LegalEntity = s.LegalEntity,
                             ShiptoState = sp1.Abbreviation,
                             ShipToZipCode = AD.ZipPostalCode,
                             ShippedToCity = AD.City,
                             OrderId = Glc.OrderId,
                             ProductName = P.Name,
                             paymentDate = OD.PaidDateUtc,
                             PaymentType = GN.Value,
                             VertexTaxAreaId = "",
                             RequestedFulfillmentDate = OT.FulfillmentDateTime,
                             GLCodeId = Glc.GLCodeId,
                             ProductId = Glc.ProductId,
                             OrderStatusId = OD.OrderStatusId,
                             SiteName = s.Name,
                             storeid = Glc.StoreId
                         }
                        );
            var journalReps = query.ToList();
            // journalReps = journalReps.Where(d => d.OrderStatus != OrderStatus.Cancelled).ToList();
            // journalReps.RemoveAll(x => x.RequestedFulfillmentDate == null);
            return journalReps;
        }
        public IList<OrderExcelExport> GetStoreJournalDeliveryReportForExcel(DateTime? startTime, DateTime? endTime, int storeId)
        {
            var query = (from s in _storeRepository.Table
                         join Glc in this._productGlCodeCalculationRepository.Table on s.Id equals Glc.StoreId
                         join SP in this._stateProvinceRepository.Table on s.CompanyStateProvinceId equals SP.Id
                         join OD in this._orderRepository.Table on Glc.OrderId equals OD.Id
                         join AD in this._addressRepository.Table on OD.ShippingAddressId equals AD.Id //change it to shipping AddressID
                         join sp1 in this._stateProvinceRepository.Table on AD.StateProvinceId equals sp1.Id
                         join P in this._productRepository.Table on Glc.ProductId equals P.Id
                         join GN in this._genericRepository.Table on Glc.OrderId equals GN.EntityId
                         join OT in this._orderItemRepository.Table on new { a = Glc.ProductId, b = Glc.OrderId } equals new { a = OT.ProductId, b = OT.OrderId }
                         join GL in this._glCode.Table on Glc.GLCodeId equals GL.Id into abc
                         from data in abc.DefaultIfEmpty()
                         where Glc.Processed == false && Glc.GlStatusType == 2 && Glc.StoreId == storeId && OT.FulfillmentDateTime != null &&
                         startTime.Value <= OT.FulfillmentDateTime && endTime.Value >= OT.FulfillmentDateTime && !OD.Deleted
                         select new OrderExcelExport()
                         {
                             Id = Glc.Id,
                             SiteExtKey = s.ExtKey,
                             StoreStateProvince = SP.Abbreviation,
                             StoreZipCode = s.CompanyZipPostalCode,
                             StoreCity = s.CompanyCity,
                             LegalEntity = s.LegalEntity,
                             ShiptoState = sp1.Abbreviation,
                             ShipToZipCode = AD.ZipPostalCode,
                             ShippedToCity = AD.City,
                             OrderId = Glc.OrderId,
                             ProductName = P.Name,
                             paymentDate = OD.PaidDateUtc,
                             PaymentType = GN.Value,
                             VertexTaxAreaId = "",
                             RequestedFulfillmentDate = OT.FulfillmentDateTime,
                             GLCodeId = Glc.GLCodeId,
                             ProductId = Glc.ProductId,
                             OrderStatusId = OD.OrderStatusId,
                             SiteName = s.Name,
                             storeid = Glc.StoreId
                         }
                        );
            var journalReps = query.ToList();
            //journalReps = journalReps.Where(d => d.OrderStatus != OrderStatus.Cancelled).ToList();
            //journalReps.RemoveAll(x => x.RequestedFulfillmentDate == null);
            return journalReps;
        }

        public IList<OrderExcelExport> ApplyDeliveryReportShippingRules(Product productInfo, OrderItem orderItems, ProductGlCode codes)
        {
            var returnList = new List<OrderExcelExport>();
            if (productInfo.IsShipEnabled && codes.GlCode.GlCodeName == "62610001" && orderItems.IsShipping)
            {
                UpdateDetailsWithAmountAndGlFromIncomingRequest.Amount = orderItems.ShippingFee;
                UpdateDetailsWithAmountAndGlFromIncomingRequest.GLCode = codes.GlCode.GlCodeName;
                if (UpdateDetailsWithAmountAndGlFromIncomingRequest.Amount != 0)
                {
                    returnList.Add(new OrderExcelExport
                    {
                        SiteExtKey = UpdateDetailsWithAmountAndGlFromIncomingRequest.SiteExtKey,
                        SiteName = UpdateDetailsWithAmountAndGlFromIncomingRequest.SiteName,
                        OrderId = UpdateDetailsWithAmountAndGlFromIncomingRequest.OrderId,
                        ProductName = productInfo.Name + "-ShippingFee",
                        RequestedFulfillmentDate = orderItems.FulfillmentDateTime, // Fulfillment Date time of Each OrderItems
                        GLCode = UpdateDetailsWithAmountAndGlFromIncomingRequest.GLCode,
                        Amount = UpdateDetailsWithAmountAndGlFromIncomingRequest.Amount,
                        storeid = UpdateDetailsWithAmountAndGlFromIncomingRequest.storeid
                    });
                }
            }
            return returnList;

        }
        public IList<OrderExcelExport> ApplyDeliveryReportDeliveryRules(Product productInfo, OrderItem orderitems, ProductGlCode codes, bool splitGl)
        {
            var returnList = new List<OrderExcelExport>();
            if ((productInfo.IsPickupEnabled || productInfo.IsLocalDelivery) && codes.GlCode.GlCodeName == "70161010" && orderitems.IsDeliveryPickUp)
            {
                UpdateDetailsWithAmountAndGlFromIncomingRequest.Amount = orderitems.DeliveryPickupFee;
                UpdateDetailsWithAmountAndGlFromIncomingRequest.GLCode = codes.GlCode.GlCodeName;
                if (UpdateDetailsWithAmountAndGlFromIncomingRequest.Amount != 0)
                {
                    returnList.Add(new OrderExcelExport
                    {
                        SiteExtKey = UpdateDetailsWithAmountAndGlFromIncomingRequest.SiteExtKey,
                        SiteName = UpdateDetailsWithAmountAndGlFromIncomingRequest.SiteName,
                        OrderId = UpdateDetailsWithAmountAndGlFromIncomingRequest.OrderId,
                        ProductName = productInfo.Name + "-DeliveryPickupFee",
                        RequestedFulfillmentDate = orderitems.FulfillmentDateTime, // Fulfillment Date time of Each OrderItems
                        GLCode = UpdateDetailsWithAmountAndGlFromIncomingRequest.GLCode,
                        Amount = UpdateDetailsWithAmountAndGlFromIncomingRequest.Amount,
                        storeid = UpdateDetailsWithAmountAndGlFromIncomingRequest.storeid

                    });
                }
            }
            else if (codes.GlCode.GlCodeName != "62610001" && codes.GlCode.GlCodeName != "70161010")
            {
                UpdateDetailsWithAmountAndGlFromIncomingRequest.Amount = codes.CalculateGLAmount(orderitems, splitGl);
                UpdateDetailsWithAmountAndGlFromIncomingRequest.GLCode = codes.GlCode.GlCodeName;
                if (UpdateDetailsWithAmountAndGlFromIncomingRequest.Amount != 0)
                {
                    returnList.Add(new OrderExcelExport
                    {
                        SiteExtKey = UpdateDetailsWithAmountAndGlFromIncomingRequest.SiteExtKey,
                        SiteName = UpdateDetailsWithAmountAndGlFromIncomingRequest.SiteName,
                        OrderId = UpdateDetailsWithAmountAndGlFromIncomingRequest.OrderId,
                        ProductName = productInfo.Name,
                        RequestedFulfillmentDate = orderitems.FulfillmentDateTime, // Fulfillment Date time of Each OrderItems
                        GLCode = UpdateDetailsWithAmountAndGlFromIncomingRequest.GLCode,
                        Amount = UpdateDetailsWithAmountAndGlFromIncomingRequest.Amount,
                        storeid = UpdateDetailsWithAmountAndGlFromIncomingRequest.storeid
                    });
                }
            }

            return returnList;
        }
        public IList<OrderExcelExport> ProcessJournalDeliveryBusinessRules(IList<OrderExcelExport> journalReps)
        {
            var disticntOrdersToProcess = journalReps.Select(x => new { x.OrderId }).Distinct(); // Fetching Disticnt orders to process the orderitems for GLs and amount.
            List<OrderExcelExport> businessRulesList = new List<OrderExcelExport>();

            foreach (var itemstoProcessForReports in disticntOrdersToProcess)
            {
                var getOrderItemsbyOrdeID = _orderService.GetOrderItemsByOrderID(itemstoProcessForReports.OrderId); // Fetching all the OrderItems by order iD to process the records for JE.
                UpdateDetailsWithAmountAndGlFromIncomingRequest = journalReps.Where(x => x.OrderId == itemstoProcessForReports.OrderId).FirstOrDefault(); // Fetching the address Details to be shown for each orderitem to process 
                var OrderDetails = _orderService.GetOrderById(itemstoProcessForReports.OrderId);//Fetching the orderDetails


                //Section to Process for Reports for each orderItems identified/Retrieved by orderid
                foreach (var orderitems in getOrderItemsbyOrdeID)
                {
                    //Any Unfulfilled items, Meal Plans,Partial Refunded items and cancelled orders are filtered out
                    if (orderitems.FulfillmentDateTime == null || orderitems.Product.IsMealPlan)
                        continue;

                    //Section to Get Tax For Each orderItems
                    var vertexOrderTaxGls = _glCodeService.GetVertexGlBreakdown(orderitems.OrderId, orderitems.ProductId, TaxRequestType.ConfirmDistributiveTax); //gets the order gls from vertex response
                    //Getting the Product Details info for Use.
                    var productInfo = _productRepository.GetById(orderitems.ProductId);
                    //Checking if the Product is Split Gl or Not
                    bool splitGl = productInfo.TaxCategoryId == -1 ? true : false;
                    //Placing each orderitem in varibale to access inside the below foreach loop.
                    var individualOrderItem = orderitems;

                    //Retrieves information from the mapping table itslef product_glcode_mapping
                    List<ProductGlCode> PaidCodes = _glCodeService.GetProductGlCodes(productInfo, GLCodeStatusType.Paid).ToList();
                    var dis = PaidCodes.GroupBy(x => x.GlCodeId).Select(y => y.First());

                    foreach (var codes in dis)
                    {
                        businessRulesList.AddRange(ApplyDeliveryReportShippingRules(productInfo, individualOrderItem, codes));
                        businessRulesList.AddRange(ApplyDeliveryReportDeliveryRules(productInfo, individualOrderItem, codes, splitGl));
                    }
                }
            }
            return businessRulesList;
        }
        public IList<OrderExcelExport> JournalDeliveryReportForExcel(DateTime? startTime, DateTime? endTime)
        {
            var journalReps = GetAllStoresJournalDeliveryReportForExcel(startTime, endTime);
            var newListToExport = ProcessJournalDeliveryBusinessRules(journalReps);
            return newListToExport.OrderByDescending(x => x.OrderId).ToList();
        }

        public IList<OrderExcelExport> JournalDeliveryReportForExcel(DateTime? startTime, DateTime? endTime, int storeId)
        {
            var journalReps = GetStoreJournalDeliveryReportForExcel(startTime, endTime, storeId);
            var newListToExport = ProcessJournalDeliveryBusinessRules(journalReps);
            return newListToExport.OrderByDescending(x => x.OrderId).ToList();
        }

        public virtual IList<OrderExcelExport> RefundsForExcel(int storeId, DateTime? startTime, DateTime? endTime, int pageIndex, int pageSize, TimeZoneInfo convertToTimeZone)
        {

            startTime = startTime.HasValue ? startTime : new DateTime?(DateTime.UtcNow.Date);
            endTime = endTime.HasValue ? endTime : new DateTime?(DateTime.UtcNow.Date.AddDays(1.0));

            var query1 = _orderRepository.Table;
            //query1 = query1.Where(o => !o.Deleted);
            //query1 = query1.OrderByDescending(o => o.CreatedOnUtc);
            var query = (from o in query1
                         join orn in this._orderNoteRepository.Table on o.Id equals orn.OrderId
                         join osp in this._orderItemRepository.Table on o.Id equals osp.OrderId
                         join p in this._productRepository.Table on osp.ProductId equals p.Id
                         //  join pgl in this._productGlRepository.Table on p.Id equals pgl.ProductId
                         //                         join gl in this._glCode.Table on pgl.GlCodeId equals gl.Id
                         join ga in this._genericRepository.Table on o.Id equals ga.EntityId into gas

                         from ga in gas.DefaultIfEmpty()
                         where orn.CreatedOnUtc >= startTime.Value && orn.CreatedOnUtc <= endTime.Value
                         && orn.Note.Contains("refunded") && o.StoreId == storeId

                         select new OrderExcelExport()
                         {
                             OrderId = o.Id,
                             OrderItemId = osp.Id,
                             OrderGuid = o.OrderGuid,
                             CustomerId = o.CustomerId,
                             LocalProductName = p.Name,
                             //   Category = c.Name,
                             //   CategoryId = c.Id,
                             GLCode = "",
                             GLCodeId = 0,
                             OrderSubtotalInclTax = o.OrderSubtotalInclTax,
                             OrderSubtotalExclTax = o.OrderSubtotalExclTax,
                             SiteProductPriceExclTax = decimal.Round(osp.PriceExclTax, 2),
                             OrderSubTotalDiscountInclTax = o.OrderSubTotalDiscountInclTax,
                             OrderSubTotalDiscountExclTax = o.OrderSubTotalDiscountExclTax,
                             OrderShippingInclTax = o.OrderShippingInclTax,
                             OrderShippingExclTax = o.OrderShippingExclTax,
                             PaymentMethodAdditionalFeeInclTax = o.PaymentMethodAdditionalFeeInclTax,
                             PaymentMethodAdditionalFeeExclTax = o.PaymentMethodAdditionalFeeExclTax,
                             OrderTax = o.OrderTax,
                             SiteProductTax = decimal.Round(osp.PriceInclTax, 2) - decimal.Round(osp.PriceExclTax, 2),
                             OrderTotal = o.OrderTotal,
                             SiteProductTotal = decimal.Round(osp.PriceInclTax, 2),
                             //GiftCardCredit = giftcard.UsedValue,
                             RefundedAmount = o.RefundedAmount,
                             OrderDiscount = o.OrderDiscount,
                             CurrencyRate = o.CurrencyRate,
                             CustomerCurrencyCode = o.CustomerCurrencyCode,

                             AffiliateId = o.AffiliateId,
                             OrderStatusId = o.OrderStatusId,
                             CardType = ga.Value,
                             PaymentMethodSystemName = o.PaymentMethodSystemName,

                             DeliveryDateUtc = osp.FulfillmentDateTime,
                             RequestedFulfillmentDate = osp.RequestedFulfillmentDateTime,
                             //DeliveryAmountExclTax = osp.Deliver,
                             //ShippingMethod = o.ShippingMethod,
                             //ShippingRateComputationMethodSystemName =
                             //o.ShippingRateComputationMethodSystemName,
                             CreatedOnUtc = o.CreatedOnUtc,
                             //  AccountNumber = mealplan.RecipientAcctNum
                         }
                        ).Distinct();
            var query2 = from c in query
                         orderby c.GLCode, c.CreatedOnUtc
                         select c;
            var orders = query.ToList();

            return orders;

        }


        #endregion
    }
}
