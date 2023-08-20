using System;
using System.Collections.Generic;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;

namespace Nop.Services.Orders
{
    /// <summary>
    /// Order service interface
    /// </summary>
    public partial interface IReportingService
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceSiteId"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="os"></param>
        /// <param name="ps"></param>
        /// <param name="ss"></param>
        /// <param name="billingEmail"></param>
        /// <param name="orderGuid"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="getFutureDeliveries"></param>
        /// <returns></returns>
   
        IList<OrderExcelExport> FutureDeliveriesForExcel(int sourceSiteId, DateTime? startTime, DateTime? endTime, int pageIndex, int pageSize, bool getFutureDeliveries);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceSiteId"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="os"></param>
        /// <param name="ps"></param>
        /// <param name="ss"></param>
        /// <param name="billingEmail"></param>
        /// <param name="orderGuid"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="getFutureDeliveries"></param>
        /// <returns></returns>
        IList<OrderExcelExport> DeliveriesForExcel(int sourceSiteId, DateTime? startTime, DateTime? endTime, int pageIndex, int pageSize, bool getFutureDeliveries);
        IList<OrderExcelExport> JournalDeliveryForExcelPerStore(DateTime? startTime, DateTime? endTime, int storeId);

        IList<OrderExcelExport> JournalPaymentReportForExcelAllStore(DateTime? startTime, DateTime? endTime);

        IList<OrderExcelExport> JournalDeliveryForExcelAllStore(DateTime? startTime, DateTime? endTime);

        /// <summary>
        /// Orders for Finance Payment Report Export
        /// </summary>
        /// <param name="sourceSiteId">Id of the SourceSite</param>
        /// <param name="startTime">Order start time; null to load all orders</param>
        /// <param name="endTime">Order end time; null to load all orders</param>
        /// <param name="os">Order status; null to load all orders</param>
        /// <param name="ps">Order payment status; null to load all orders</param>
        /// <param name="ss">Order shippment status; null to load all orders</param>
        /// <param name="billingEmail">Billing email. Leave empty to load all records.</param>
        /// <param name="orderGuid">Search by order GUID (Global unique identifier) or part of GUID. Leave empty to load all records.</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Order collection</returns>
        IList<OrderExcelExport> PaymentsForExcel(int storeId, DateTime? startDate, DateTime? endDate, int pageIndex, int pageSize, TimeZoneInfo convertToTimeZone);


        /// <summary>
        /// Load all GLCodes for Order Finance Reports
        /// </summary>
        /// <returns>GLCode collection</returns>
        IList<GlCode> GetGlCodesForReport(bool isDelivered);
        /// Load all orders
        /// </summary>
        /// <param name="sourceSiteId"></param>
        /// <param name="fromDate"></param>
        /// <param name="sortingEnum">How to Sort the list</param>
        /// <returns>Order collection</returns>
       // IList<OrderItem> GetUnfulfilledOrders(int sourceSiteId, DateTime fromDate);


        IList<OrderExcelExport> JournalReportForExcel(DateTime? startTime, DateTime? endTime);
        IList<OrderExcelExport> JournalPaymentReportForExcelPerStore(DateTime? startTime, DateTime? endTime, int storeId);
        IList<OrderExcelExport> JournalDeliveryReportForExcel(DateTime? startTime, DateTime? endTime);
        IList<OrderExcelExport> JournalReportForExcel(DateTime? startTime, DateTime? endTime, int storeId);
        IList<OrderExcelExport> JournalDeliveryReportForExcel(DateTime? startTime, DateTime? endTime, int storeId);

        IList<OrderExcelExport> RefundsForExcel(int storeId, DateTime? startDate, DateTime? endDate, int pageIndex, int pageSize, TimeZoneInfo convertToTimeZone);

        IList<OrderExcelExport> RefundsAllUnitsForExcel(DateTime? startDate, DateTime? endDate, int pageIndex, int pageSize, TimeZoneInfo convertToTimeZone);

    }
}
