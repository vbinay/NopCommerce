using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Stores;
using Nop.Services.Events;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Common;

namespace Nop.Services.Orders
{
    /// <summary>
    /// Order service
    /// </summary>
    public partial class OrderService : IOrderService
    {
        #region Fields

        private readonly IRepository<ProductGlCodeCalculation> _productGlcodecalcrepo;
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<OrderItem> _orderItemRepository;
        private readonly IRepository<OrderNote> _orderNoteRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<RecurringPayment> _recurringPaymentRepository;
        private readonly IRepository<Customer> _customerRepository;
        private readonly IEventPublisher _eventPublisher;
        private readonly IRepository<ProductGlCodeMapping> _productGlcodeMappingRepo;
        private readonly IRepository<OrderDetails> _orderDetailsRepo;
        private readonly IRepository<ReservedProduct> _reservedProductRepo;
        private readonly IRepository<Address> _addressRepo;
        private readonly IRepository<StoreMapping> _storeMappingRepo;
        private readonly Helpers.IDateTimeHelper _dateTimeHelper;

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
        public OrderService(IRepository<Order> orderRepository,
            IRepository<OrderItem> orderItemRepository,
            IRepository<OrderNote> orderNoteRepository,
            IRepository<Product> productRepository,
            IRepository<RecurringPayment> recurringPaymentRepository,
            IRepository<Customer> customerRepository,
            IEventPublisher eventPublisher, IRepository<ProductGlCodeCalculation> productGlcodecalcrepo,
            IRepository<ProductGlCodeMapping> productGlcodeMappingRepo,
            IRepository<OrderDetails> orderDetailsRepo,
            Helpers.IDateTimeHelper dateTimeHelper,
            IRepository<ReservedProduct> reservedProductsRepo,
            IRepository<Address> addressRepo,
            IRepository<StoreMapping> storeMappingRepo)
        {
            this._orderRepository = orderRepository;
            this._orderItemRepository = orderItemRepository;
            this._orderNoteRepository = orderNoteRepository;
            this._productRepository = productRepository;
            this._recurringPaymentRepository = recurringPaymentRepository;
            this._customerRepository = customerRepository;
            this._eventPublisher = eventPublisher;
            this._productGlcodecalcrepo = productGlcodecalcrepo;
            this._productGlcodeMappingRepo = productGlcodeMappingRepo;
            this._orderDetailsRepo = orderDetailsRepo;
            this._reservedProductRepo = reservedProductsRepo;
            this._addressRepo = addressRepo;
            this._storeMappingRepo = storeMappingRepo;
            this._dateTimeHelper = dateTimeHelper;
        }

        #endregion

        #region Methods

        #region Orders

        /// <summary>
        /// Gets an order
        /// </summary>
        /// <param name="orderId">The order identifier</param>
        /// <returns>Order</returns>
        public virtual Order GetOrderById(int orderId)
        {
            if (orderId == 0)
                return null;

            return _orderRepository.GetById(orderId);
        }


        /// <summary>
        /// Get orders by identifiers
        /// </summary>
        /// <param name="orderIds">Order identifiers</param>
        /// <returns>Order</returns>
        public virtual IList<Order> GetOrdersByIds(int[] orderIds)
        {
            if (orderIds == null || orderIds.Length == 0)
                return new List<Order>();

            var query = from o in _orderRepository.Table
                        where orderIds.Contains(o.Id)
                        select o;
            var orders = query.ToList();
            //sort by passed identifiers
            var sortedOrders = new List<Order>();
            foreach (int id in orderIds)
            {
                var order = orders.Find(x => x.Id == id);
                if (order != null)
                    sortedOrders.Add(order);
            }
            return sortedOrders;
        }

        /// <summary>
        /// Gets an order
        /// </summary>
        /// <param name="orderGuid">The order identifier</param>
        /// <returns>Order</returns>
        public virtual Order GetOrderByGuid(Guid orderGuid)
        {
            if (orderGuid == Guid.Empty)
                return null;

            var query = from o in _orderRepository.Table
                        where o.OrderGuid == orderGuid
                        select o;
            var order = query.FirstOrDefault();
            return order;
        }

        public virtual Order GetOrderByOrderItemId(int orderItemId)
        {
            if (orderItemId < 0)
                return null;
            var query = from o in _orderItemRepository.Table
                        where o.Id == orderItemId
                        select o;
            int orderId = query.FirstOrDefault().OrderId;

            var query2 = from or in _orderRepository.Table
                        where or.Id == orderId
                        select or;

            var order = query2.FirstOrDefault();
            return order;
        }

        /// <summary>
        /// Deletes an order
        /// </summary>
        /// <param name="order">The order</param>
        public virtual void DeleteOrder(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            order.Deleted = true;
            UpdateOrder(order);
        }


        /// <summary>
        /// Search orders
        /// </summary>
        /// <param name="storeId">Store identifier; 0 to load all orders</param>
        /// <param name="vendorId">Vendor identifier; null to load all orders</param>
        /// <param name="customerId">Customer identifier; 0 to load all orders</param>
        /// <param name="productId">Product identifier which was purchased in an order; 0 to load all orders</param>
        /// <param name="affiliateId">Affiliate identifier; 0 to load all orders</param>
        /// <param name="billingCountryId">Billing country identifier; 0 to load all orders</param>
        /// <param name="warehouseId">Warehouse identifier, only orders with products from a specified warehouse will be loaded; 0 to load all orders</param>
        /// <param name="paymentMethodSystemName">Payment method system name; null to load all records</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="osIds">Order status identifiers; null to load all orders</param>
        /// <param name="psIds">Payment status identifiers; null to load all orders</param>
        /// <param name="ssIds">Shipping status identifiers; null to load all orders</param>
        /// <param name="billingEmail">Billing email. Leave empty to load all records.</param>
        /// <param name="billingLastName">Billing last name. Leave empty to load all records.</param>
        /// <param name="orderNotes">Search in order notes. Leave empty to load all records.</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Orders</returns>
        public virtual IPagedList<Order> SearchUnfulfilledOrders(int storeId = 0,
            int vendorId = 0, int customerId = 0,
            int productId = 0, int affiliateId = 0, int warehouseId = 0,
            int billingCountryId = 0, string paymentMethodSystemName = null,
            DateTime? createdFromUtc = null, DateTime? createdToUtc = null,
            List<int> osIds = null, List<int> psIds = null, List<int> ssIds = null,
            string billingEmail = null, string billingLastName = "", bool onlyUnfulfilled = false,
            string orderNotes = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = _orderRepository.Table;
            if (storeId > 0)
                query = query.Where(o => o.StoreId == storeId);
            if (vendorId > 0)
            {
                query = query
                    .Where(o => o.OrderItems
                    .Any(orderItem => orderItem.Product.VendorId == vendorId));
            }
            if (customerId > 0)
                query = query.Where(o => o.CustomerId == customerId);
            if (productId > 0)
            {
                query = query
                    .Where(o => o.OrderItems
                    .Any(orderItem => orderItem.Product.Id == productId));
            }
            if (warehouseId > 0)
            {
                var manageStockInventoryMethodId = (int)ManageInventoryMethod.ManageStock;
                query = query
                    .Where(o => o.OrderItems
                    .Any(orderItem =>
                        //"Use multiple warehouses" enabled
                        //we search in each warehouse
                        (orderItem.Product.ManageInventoryMethodId == manageStockInventoryMethodId &&
                        orderItem.Product.UseMultipleWarehouses &&
                        orderItem.Product.ProductWarehouseInventory.Any(pwi => pwi.WarehouseId == warehouseId))
                        ||
                        //"Use multiple warehouses" disabled
                        //we use standard "warehouse" property
                        ((orderItem.Product.ManageInventoryMethodId != manageStockInventoryMethodId ||
                        !orderItem.Product.UseMultipleWarehouses) &&
                        orderItem.Product.WarehouseId == warehouseId))
                        );
            }
            if (billingCountryId > 0)
                query = query.Where(o => o.BillingAddress != null && o.BillingAddress.CountryId == billingCountryId);
            if (!String.IsNullOrEmpty(paymentMethodSystemName))
                query = query.Where(o => o.PaymentMethodSystemName == paymentMethodSystemName);
            if (affiliateId > 0)
                query = query.Where(o => o.AffiliateId == affiliateId);
            if (createdFromUtc.HasValue)
                query = query.Where(o => createdFromUtc.Value <= o.CreatedOnUtc);
            if (createdToUtc.HasValue)
                query = query.Where(o => createdToUtc.Value >= o.CreatedOnUtc);
            if (osIds != null && osIds.Any())
                query = query.Where(o => osIds.Contains(o.OrderStatusId));
            if (psIds != null && psIds.Any())
                query = query.Where(o => psIds.Contains(o.PaymentStatusId));
            if (ssIds != null && ssIds.Any())
                query = query.Where(o => ssIds.Contains(o.ShippingStatusId));
            if (!String.IsNullOrEmpty(billingEmail))
                query = query.Where(o => o.BillingAddress != null && !String.IsNullOrEmpty(o.BillingAddress.Email) && o.BillingAddress.Email.Contains(billingEmail));
            if (!String.IsNullOrEmpty(billingLastName))
                query = query.Where(o => o.BillingAddress != null && !String.IsNullOrEmpty(o.BillingAddress.LastName) && o.BillingAddress.LastName.Contains(billingLastName));
            if (!String.IsNullOrEmpty(orderNotes))
                query = query.Where(o => o.OrderNotes.Any(on => on.Note.Contains(orderNotes)));



            query = query.Where(o => !o.Deleted);
            query = query.OrderByDescending(o => o.CreatedOnUtc);

            //database layer paging
            return new PagedList<Order>(query, pageIndex, pageSize);
        }



        /// <summary>
        /// Search orders
        /// </summary>
        /// <param name="storeId">Store identifier; 0 to load all orders</param>
        /// <param name="vendorId">Vendor identifier; null to load all orders</param>
        /// <param name="customerId">Customer identifier; 0 to load all orders</param>
        /// <param name="productId">Product identifier which was purchased in an order; 0 to load all orders</param>
        /// <param name="affiliateId">Affiliate identifier; 0 to load all orders</param>
        /// <param name="billingCountryId">Billing country identifier; 0 to load all orders</param>
        /// <param name="warehouseId">Warehouse identifier, only orders with products from a specified warehouse will be loaded; 0 to load all orders</param>
        /// <param name="paymentMethodSystemName">Payment method system name; null to load all records</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="osIds">Order status identifiers; null to load all orders</param>
        /// <param name="psIds">Payment status identifiers; null to load all orders</param>
        /// <param name="ssIds">Shipping status identifiers; null to load all orders</param>
        /// <param name="billingEmail">Billing email. Leave empty to load all records.</param>
        /// <param name="billingLastName">Billing last name. Leave empty to load all records.</param>
        /// <param name="orderNotes">Search in order notes. Leave empty to load all records.</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Orders</returns>
        public virtual IPagedList<Order> SearchOrders(int storeId = 0,
            int vendorId = 0, int customerId = 0,
            int productId = 0, int affiliateId = 0, int warehouseId = 0,
            int billingCountryId = 0, string paymentMethodSystemName = null,
            DateTime? createdFromUtc = null, DateTime? createdToUtc = null,
            List<int> osIds = null, List<int> psIds = null, List<int> ssIds = null,
            string billingEmail = null, string billingLastName = "",
            string orderNotes = null, bool deliveryDate = false, bool onlyUnfulfilled = false, int pageIndex = 0, int pageSize = int.MaxValue,int orderid=0)
        {
            var query = _orderRepository.Table;
            if (storeId > 0)
                query = query.Where(o => o.StoreId == storeId);
            if (vendorId > 0)
            {
                query = query
                    .Where(o => o.OrderItems
                    .Any(orderItem => orderItem.Product.VendorId == vendorId));
            }
            if (customerId > 0)
                query = query.Where(o => o.CustomerId == customerId);


            if (productId > 0)
            {
                query = query
                    .Where(o => o.OrderItems
                    .Any(orderItem => orderItem.Product.Id == productId));
            }
            if (warehouseId > 0)
            {
                var manageStockInventoryMethodId = (int)ManageInventoryMethod.ManageStock;
                query = query
                    .Where(o => o.OrderItems
                    .Any(orderItem =>
                        //"Use multiple warehouses" enabled
                        //we search in each warehouse
                        (orderItem.Product.ManageInventoryMethodId == manageStockInventoryMethodId &&
                        orderItem.Product.UseMultipleWarehouses &&
                        orderItem.Product.ProductWarehouseInventory.Any(pwi => pwi.WarehouseId == warehouseId))
                        ||
                        //"Use multiple warehouses" disabled
                        //we use standard "warehouse" property
                        ((orderItem.Product.ManageInventoryMethodId != manageStockInventoryMethodId ||
                        !orderItem.Product.UseMultipleWarehouses) &&
                        orderItem.Product.WarehouseId == warehouseId))
                        );
            }



            if (billingCountryId > 0)
                query = query.Where(o => o.BillingAddress != null && o.BillingAddress.CountryId == billingCountryId);
            if (!String.IsNullOrEmpty(paymentMethodSystemName))
                query = query.Where(o => o.PaymentMethodSystemName == paymentMethodSystemName);
            if (affiliateId > 0)
                query = query.Where(o => o.AffiliateId == affiliateId);
            if (createdFromUtc.HasValue && !deliveryDate)
                query = query.Where(o => createdFromUtc.Value <= o.CreatedOnUtc);
            if (createdToUtc.HasValue && !deliveryDate)
                query = query.Where(o => createdToUtc.Value >= o.CreatedOnUtc);

            if (createdFromUtc.HasValue && deliveryDate)
                query = query.Where(o => o.OrderItems.Any(oi => createdFromUtc.Value <= oi.FulfillmentDateTime));
            if (createdToUtc.HasValue && deliveryDate)
                query = query.Where(o => o.OrderItems.Any(oi => createdToUtc.Value >= oi.FulfillmentDateTime));

            if (onlyUnfulfilled)
            {
                query = query.Where(o => o.OrderItems.Any(orderItem => orderItem.FulfillmentDateTime == null));
            }


            if (osIds != null && osIds.Any())
                query = query.Where(o => osIds.Contains(o.OrderStatusId));
            if (psIds != null && psIds.Any())
                query = query.Where(o => psIds.Contains(o.PaymentStatusId));
            if (ssIds != null && ssIds.Any())
                query = query.Where(o => ssIds.Contains(o.ShippingStatusId));
            if (!String.IsNullOrEmpty(billingEmail))
                query = query.Where(o => o.BillingAddress != null && !String.IsNullOrEmpty(o.BillingAddress.Email) && o.BillingAddress.Email.Contains(billingEmail));
            if (!String.IsNullOrEmpty(billingLastName))
                query = query.Where(o => o.BillingAddress != null && !String.IsNullOrEmpty(o.BillingAddress.LastName) && o.BillingAddress.LastName.Contains(billingLastName));
            if (!String.IsNullOrEmpty(orderNotes))
                query = query.Where(o => o.OrderNotes.Any(on => on.Note.Contains(orderNotes)));

            if (orderid != 0)
                query = query.Where(o => o.Id == orderid);

            query = query.Where(o => !o.Deleted);
            query = query.OrderByDescending(o => o.CreatedOnUtc);
            //database layer paging
            return new PagedList<Order>(query, pageIndex, pageSize);
        }

        public virtual IPagedList<ReservationModel> SearchReservations(
            int vendorId = 0, string orderId = "",
            DateTime? createdFromUtc = null, DateTime? createdToUtc = null, string billingLastName = "", string productName = "",
            string timeSlot = "", string status = "", int pageIndex = 0, int pageSize = int.MaxValue)
        {
            //var queryReserved = _reservedProductRepo.Table;
            var query = (from OR in _orderRepository.Table
                         join OIR in _orderItemRepository.Table on OR.Id equals OIR.OrderId
                         join RPR in _reservedProductRepo.Table on OIR.Id equals RPR.OrderItemId
                         join PR in _productRepository.Table on   RPR.ProductId equals PR.Id
                         join BAR in _addressRepo.Table on OR.BillingAddressId equals BAR.Id
                         join SM in _storeMappingRepo.Table on RPR.ProductId equals SM.EntityId 
                         orderby RPR.ReservationDate descending
                         where SM.EntityName == "Product"
                         select new ReservationModel()
                         {
                             OrderId = OR.Id,
                             ReservedProductId = RPR.Id,
                             ProductId = OIR.ProductId,
                             ReservationDate = RPR.ReservationDate,
                             ReservedTimeSlot = RPR.ReservedTimeSlot,
                             ReservedUnits = RPR.ReservedUnits,
                             OrderItemId = OIR.Id,
                             PaymentStatusId = OR.PaymentStatusId,
                             CustomerId = OR.CustomerId,
                             CustomerFullName = BAR.FirstName + " " + BAR.LastName,
                             BillingLastName = BAR.LastName,
                             CustomerEmail = OR.Customer.Email,
                             OrderTotal = OR.OrderTotal,
                             IsFulfilled = OIR.FulfillmentDateTime.HasValue ? true : false,
                             StoreId = SM.StoreId,
                             ProductName = PR.Name
                         });

            //Filter for billing last name
            if (!String.IsNullOrEmpty(billingLastName))
                query = query.Where(o => o.BillingLastName != null && !String.IsNullOrEmpty(o.BillingLastName) && o.BillingLastName.Contains(billingLastName));

            //filter for order id
            if (!String.IsNullOrEmpty(orderId))
            {
                int orderid = int.Parse(orderId);
                query = query.Where(o=> o.OrderId == orderid);
            }

            if (!String.IsNullOrEmpty(productName))
            {
                query = query.Where(o => o.ProductName != null && !String.IsNullOrEmpty(o.ProductName) && o.ProductName.Contains(productName));
            }

            if (!String.IsNullOrEmpty(timeSlot))
                query = query.Where(o => o.ReservedTimeSlot != null && !String.IsNullOrEmpty(o.ReservedTimeSlot) && o.ReservedTimeSlot.Contains(timeSlot));

            if (!String.IsNullOrEmpty(status))
            {
                if(status == "Fulfilled")
                query = query.Where(o =>o.IsFulfilled);

                if (status == "Check In")
                query = query.Where(o => !o.IsFulfilled);
            }

            return new PagedList<ReservationModel>(query, pageIndex, pageSize);
        }

        public virtual IPagedList<Order> SearchOrdersForFutureDeliveryPerStore(int storeId = 0,
            int vendorId = 0, int customerId = 0,
            int productId = 0, int affiliateId = 0, int warehouseId = 0,
            int billingCountryId = 0, string paymentMethodSystemName = null,
            DateTime? createdFromUtc = null, DateTime? createdToUtc = null,
            List<int> osIds = null, List<int> psIds = null, List<int> ssIds = null,
            string billingEmail = null, string billingLastName = "",
            string orderNotes = null, bool deliveryDate = false, bool onlyUnfulfilled = false, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = _orderRepository.Table;
            if (storeId > 0)
                query = query.Where(o => o.StoreId == storeId);
            if (vendorId > 0)
            {
                query = query
                    .Where(o => o.OrderItems
                    .Any(orderItem => orderItem.Product.VendorId == vendorId));
            }
            if (customerId > 0)
                query = query.Where(o => o.CustomerId == customerId);


            if (productId > 0)
            {
                query = query
                    .Where(o => o.OrderItems
                    .Any(orderItem => orderItem.Product.Id == productId));
            }
            if (warehouseId > 0)
            {
                var manageStockInventoryMethodId = (int)ManageInventoryMethod.ManageStock;
                query = query
                    .Where(o => o.OrderItems
                    .Any(orderItem =>
                        //"Use multiple warehouses" enabled
                        //we search in each warehouse
                        (orderItem.Product.ManageInventoryMethodId == manageStockInventoryMethodId &&
                        orderItem.Product.UseMultipleWarehouses &&
                        orderItem.Product.ProductWarehouseInventory.Any(pwi => pwi.WarehouseId == warehouseId))
                        ||
                        //"Use multiple warehouses" disabled
                        //we use standard "warehouse" property
                        ((orderItem.Product.ManageInventoryMethodId != manageStockInventoryMethodId ||
                        !orderItem.Product.UseMultipleWarehouses) &&
                        orderItem.Product.WarehouseId == warehouseId))
                        );
            }



            if (billingCountryId > 0)
                query = query.Where(o => o.BillingAddress != null && o.BillingAddress.CountryId == billingCountryId);
            if (!String.IsNullOrEmpty(paymentMethodSystemName))
                query = query.Where(o => o.PaymentMethodSystemName == paymentMethodSystemName);
            if (affiliateId > 0)
                query = query.Where(o => o.AffiliateId == affiliateId);

            if (createdFromUtc.HasValue && !deliveryDate)
                query = query.Where(o => createdFromUtc.Value <= o.CreatedOnUtc);

            if (createdToUtc.HasValue && !deliveryDate)
                query = query.Where(o => createdToUtc.Value >= o.CreatedOnUtc);

            if (createdFromUtc.HasValue && deliveryDate)
                query = query.Where(o => o.OrderItems.Any(oi => createdFromUtc.Value <= oi.FulfillmentDateTime));
            if (createdToUtc.HasValue && deliveryDate)
                query = query.Where(o => o.OrderItems.Any(oi => createdToUtc.Value >= oi.FulfillmentDateTime));

            if (onlyUnfulfilled)
            {
                query = query.Where(o => o.OrderItems.Any(orderItem => orderItem.FulfillmentDateTime == null));
            }


            if (osIds != null && osIds.Any())
                query = query.Where(o => osIds.Contains(o.OrderStatusId));
            if (psIds != null && psIds.Any())
                query = query.Where(o => psIds.Contains(o.PaymentStatusId));
            if (ssIds != null && ssIds.Any())
                query = query.Where(o => ssIds.Contains(o.ShippingStatusId));
            if (!String.IsNullOrEmpty(billingEmail))
                query = query.Where(o => o.BillingAddress != null && !String.IsNullOrEmpty(o.BillingAddress.Email) && o.BillingAddress.Email.Contains(billingEmail));
            if (!String.IsNullOrEmpty(billingLastName))
                query = query.Where(o => o.BillingAddress != null && !String.IsNullOrEmpty(o.BillingAddress.LastName) && o.BillingAddress.LastName.Contains(billingLastName));
            if (!String.IsNullOrEmpty(orderNotes))
                query = query.Where(o => o.OrderNotes.Any(on => on.Note.Contains(orderNotes)));



            query = query.Where(o => !o.Deleted);
            query = query.OrderByDescending(o => o.CreatedOnUtc);

            //database layer paging
            return new PagedList<Order>(query, pageIndex, pageSize);
        }


        /// <summary>
        /// Inserts an order
        /// </summary>
        /// <param name="order">Order</param>
        public virtual void InsertOrder(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            _orderRepository.Insert(order);

            //event notification
            _eventPublisher.EntityInserted(order);
        }

         /// <summary>
        /// Inserts an order
        /// </summary>
        /// <param name="Insert Reserved Product">Order</param>
        public virtual void InsertReservedProduct(ReservedProduct reservedProduct)
        {
            if (reservedProduct == null)
                throw new ArgumentNullException("reservedProducts");
            
                _reservedProductRepo.Insert(reservedProduct);
                //event notification
                _eventPublisher.EntityInserted(reservedProduct);
        }

        /// <summary>
        /// Updates the order
        /// </summary>
        /// <param name="order">The order</param>
        public virtual void UpdateOrder(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            _orderRepository.Update(order);

            //event notification
            _eventPublisher.EntityUpdated(order);
        }

        public void InsertJEBusinessLogicOrderDetails(OrderDetails orderDetails)
        {
            if (orderDetails == null)
                throw new ArgumentException("orderdetails");

            _orderDetailsRepo.Insert(orderDetails);
        }



        public List<ProductGlCodeMapping> GetDetailsFromProductGlcodeMapping(int? orderId, int? orderItemId, int GlstatusType = 1)
        {
            var query = _productGlcodeMappingRepo.Table.Where(x => x.OrderId == orderId && x.ProductID == orderItemId && x.GlStatusType == GlstatusType).ToList();
            return query;
        }

        public void InsertProductCalculatedGlCodes(ProductGlCodeCalculation ProductGlCodeCalculation)
        {
            if (ProductGlCodeCalculation == null)
                throw new ArgumentNullException("ProductGlCodeCalculation");

            _productGlcodecalcrepo.Insert(ProductGlCodeCalculation);
            // _eventPublisher.EntityInserted(ProductGlCodeCalculation);

        }

        public void UpdateProductCalculatedGlCodes(ProductGlCodeCalculation ProductGlCodeCalculation)
        {
            if (ProductGlCodeCalculation == null)
                throw new ArgumentNullException("ProductGlCodeCalculation");

            _productGlcodecalcrepo.Update(ProductGlCodeCalculation);
            _eventPublisher.EntityUpdated(ProductGlCodeCalculation);
        }


        /// <summary>
        /// Get an order by authorization transaction ID and payment method system name
        /// </summary>
        /// <param name="authorizationTransactionId">Authorization transaction ID</param>
        /// <param name="paymentMethodSystemName">Payment method system name</param>
        /// <returns>Order</returns>
        public virtual Order GetOrderByAuthorizationTransactionIdAndPaymentMethod(string authorizationTransactionId,
            string paymentMethodSystemName)
        {
            var query = _orderRepository.Table;
            if (!String.IsNullOrWhiteSpace(authorizationTransactionId))
                query = query.Where(o => o.AuthorizationTransactionId == authorizationTransactionId);

            if (!String.IsNullOrWhiteSpace(paymentMethodSystemName))
                query = query.Where(o => o.PaymentMethodSystemName == paymentMethodSystemName);

            query = query.OrderByDescending(o => o.CreatedOnUtc);
            var order = query.FirstOrDefault();
            return order;
        }

        #region NU-84
        public virtual Order GetLatestOrder()
        {
            var query = _orderRepository.Table;

            query = query.OrderByDescending(o => o.Id);

            var order = query.FirstOrDefault();
            return order;
        }
        #endregion

        #endregion

        #region Orders items

        /// <summary>
        /// Gets an order item
        /// </summary>
        /// <param name="orderItemId">Order item identifier</param>
        /// <returns>Order item</returns>
        public virtual OrderItem GetOrderItemById(int orderItemId)
        {
            if (orderItemId == 0)
                return null;

            return _orderItemRepository.GetById(orderItemId);
        }

        public List<OrderItem> GetOrderItemsByOrderID(int orderID)
        {
            if (orderID == 0)
                return null;

            var query = from orderitem in _orderItemRepository.Table
                        where orderitem.OrderId == orderID
                        select orderitem;
            return query.ToList();
        }
        /// <summary>
        /// Gets an item
        /// </summary>
        /// <param name="orderItemGuid">Order identifier</param>
        /// <returns>Order item</returns>
        public virtual OrderItem GetOrderItemByGuid(Guid orderItemGuid)
        {
            if (orderItemGuid == Guid.Empty)
                return null;

            var query = from orderItem in _orderItemRepository.Table
                        where orderItem.OrderItemGuid == orderItemGuid
                        select orderItem;
            var item = query.FirstOrDefault();
            return item;
        }

        /// <summary>
        /// Gets all downloadable order items
        /// </summary>
        /// <param name="customerId">Customer identifier; null to load all records</param>
        /// <returns>Order items</returns>
        public virtual IList<OrderItem> GetDownloadableOrderItems(int customerId)
        {
            if (customerId == 0)
                throw new ArgumentOutOfRangeException("customerId");

            var query = from orderItem in _orderItemRepository.Table
                        join o in _orderRepository.Table on orderItem.OrderId equals o.Id
                        join p in _productRepository.Table on orderItem.ProductId equals p.Id
                        where customerId == o.CustomerId &&
                        p.IsDownload &&
                        !o.Deleted
                        orderby o.CreatedOnUtc descending, orderItem.Id
                        select orderItem;

            var orderItems = query.ToList();
            return orderItems;
        }

        /// <summary>
        /// Delete an order item
        /// </summary>
        /// <param name="orderItem">The order item</param>
        public virtual void DeleteOrderItem(OrderItem orderItem)
        {
            if (orderItem == null)
                throw new ArgumentNullException("orderItem");

            _orderItemRepository.Delete(orderItem);

            //event notification
            _eventPublisher.EntityDeleted(orderItem);
        }

        public virtual ReservedProduct GetReservedProductbyOrderItemId(int orderItemId)
        {
            var query = from p in _reservedProductRepo.Table
                        orderby p.Id
                        where p.OrderItemId == orderItemId
                        select p;
            var reservedProduct = query.ToList().FirstOrDefault();
            return reservedProduct;
        }

        #endregion

        #region Orders notes

        /// <summary>
        /// Gets an order note
        /// </summary>
        /// <param name="orderNoteId">The order note identifier</param>
        /// <returns>Order note</returns>
        public virtual OrderNote GetOrderNoteById(int orderNoteId)
        {
            if (orderNoteId == 0)
                return null;

            return _orderNoteRepository.GetById(orderNoteId);
        }

        /// <summary>
        /// Deletes an order note
        /// </summary>
        /// <param name="orderNote">The order note</param>
        public virtual void DeleteOrderNote(OrderNote orderNote)
        {
            if (orderNote == null)
                throw new ArgumentNullException("orderNote");

            _orderNoteRepository.Delete(orderNote);

            //event notification
            _eventPublisher.EntityDeleted(orderNote);
        }

        #endregion

        #region Recurring payments

        /// <summary>
        /// Deletes a recurring payment
        /// </summary>
        /// <param name="recurringPayment">Recurring payment</param>
        public virtual void DeleteRecurringPayment(RecurringPayment recurringPayment)
        {
            if (recurringPayment == null)
                throw new ArgumentNullException("recurringPayment");

            recurringPayment.Deleted = true;
            UpdateRecurringPayment(recurringPayment);
        }

        /// <summary>
        /// Gets a recurring payment
        /// </summary>
        /// <param name="recurringPaymentId">The recurring payment identifier</param>
        /// <returns>Recurring payment</returns>
        public virtual RecurringPayment GetRecurringPaymentById(int recurringPaymentId)
        {
            if (recurringPaymentId == 0)
                return null;

            return _recurringPaymentRepository.GetById(recurringPaymentId);
        }

        /// <summary>
        /// Inserts a recurring payment
        /// </summary>
        /// <param name="recurringPayment">Recurring payment</param>
        public virtual void InsertRecurringPayment(RecurringPayment recurringPayment)
        {
            if (recurringPayment == null)
                throw new ArgumentNullException("recurringPayment");

            _recurringPaymentRepository.Insert(recurringPayment);

            //event notification
            _eventPublisher.EntityInserted(recurringPayment);
        }

        /// <summary>
        /// Updates the recurring payment
        /// </summary>
        /// <param name="recurringPayment">Recurring payment</param>
        public virtual void UpdateRecurringPayment(RecurringPayment recurringPayment)
        {
            if (recurringPayment == null)
                throw new ArgumentNullException("recurringPayment");

            _recurringPaymentRepository.Update(recurringPayment);

            //event notification
            _eventPublisher.EntityUpdated(recurringPayment);
        }

        /// <summary>
        /// Search recurring payments
        /// </summary>
        /// <param name="storeId">The store identifier; 0 to load all records</param>
        /// <param name="customerId">The customer identifier; 0 to load all records</param>
        /// <param name="initialOrderId">The initial order identifier; 0 to load all records</param>
        /// <param name="initialOrderStatus">Initial order status identifier; null to load all records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Recurring payments</returns>
        public virtual IPagedList<RecurringPayment> SearchRecurringPayments(int storeId = 0,
            int customerId = 0, int initialOrderId = 0, OrderStatus? initialOrderStatus = null,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false)
        {
            int? initialOrderStatusId = null;
            if (initialOrderStatus.HasValue)
                initialOrderStatusId = (int)initialOrderStatus.Value;

            var query1 = from rp in _recurringPaymentRepository.Table
                         join c in _customerRepository.Table on rp.InitialOrder.CustomerId equals c.Id
                         where
                         (!rp.Deleted) &&
                         (showHidden || !rp.InitialOrder.Deleted) &&
                         (showHidden || !c.Deleted) &&
                         (showHidden || rp.IsActive) &&
                         (customerId == 0 || rp.InitialOrder.CustomerId == customerId) &&
                         (storeId == 0 || rp.InitialOrder.StoreId == storeId) &&
                         (initialOrderId == 0 || rp.InitialOrder.Id == initialOrderId) &&
                         (!initialOrderStatusId.HasValue || initialOrderStatusId.Value == 0 || rp.InitialOrder.OrderStatusId == initialOrderStatusId.Value)
                         select rp.Id;

            var query2 = from rp in _recurringPaymentRepository.Table
                         where query1.Contains(rp.Id)
                         orderby rp.StartDateUtc, rp.Id
                         select rp;

            var recurringPayments = new PagedList<RecurringPayment>(query2, pageIndex, pageSize);
            return recurringPayments;
        }

        #endregion

        #endregion
    }
}
