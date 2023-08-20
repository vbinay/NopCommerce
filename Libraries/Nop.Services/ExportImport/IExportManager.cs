using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.KitchenProduction;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Tax;
using Nop.Core.Domain.Vendors;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace Nop.Services.ExportImport
{
    /// <summary>
    /// Export manager interface
    /// </summary>
    public partial interface IExportManager
    {
        /// <summary>
        /// Export manufacturer list to xml
        /// </summary>
        /// <param name="manufacturers">Manufacturers</param>
        /// <returns>Result in XML format</returns>
        string ExportManufacturersToXml(IList<Manufacturer> manufacturers);
        void ExportJournalPaymentReportPerAndAllStore(string filePath, IList<OrderExcelExport> orders, DateTime? startDate, DateTime? endDate, string reportName, bool isFuture, string storeName, int storeId = 0);
        /// <summary>
        /// Export manufacturers to XLSX
        /// </summary>
        /// <param name="manufacturers">Manufactures</param>
        byte[] ExportManufacturersToXlsx(IEnumerable<Manufacturer> manufacturers);


        DataTable ToDataTable(List<KitchenProduction> kitchenProductionDataList, string name);
        /// <summary>
        /// Export category list to xml
        /// </summary>
        /// <returns>Result in XML format</returns>
        string ExportCategoriesToXml();

        /// <summary>
        /// Export categories to XLSX
        /// </summary>
        /// <param name="categories">Categories</param>
        byte[] ExportCategoriesToXlsx(IEnumerable<Category> categories);

        /// <summary>
        /// Export product list to xml
        /// </summary>
        /// <param name="products">Products</param>
        /// <returns>Result in XML format</returns>
        string ExportProductsToXml(IList<Product> products);

        /// <summary>
        /// Export products to XLSX
        /// </summary>
        /// <param name="products">Products</param>
        byte[] ExportProductsToXlsx(IEnumerable<Product> products);

        /// <summary>
        /// Export order list to xml
        /// </summary>
        /// <param name="orders">Orders</param>
        /// <returns>Result in XML format</returns>
        string ExportOrdersToXml(IList<Order> orders);

        /// <summary>
        /// Export orders to XLSX
        /// </summary>
        /// <param name="orders">Orders</param>
        byte[] ExportOrdersToXlsx(IList<Order> orders);


        byte[] ExportKitchenProdutionOrdersToXlsx(IList<KitchenProduction> kitchenProduction);

        //---START: Codechages done by (na-sdxcorp\ADas)--------------
        /// <summary>
        /// Export ShipmentReports to XLSX
        /// </summary>
        /// <param name="shipmentReports">ShipmentReports</param>
        byte[] ExportShipmentReportToXlsx(IList<ShipmentReport> shipmentReports);
        //---END: Codechages done by (na-sdxcorp\ADas)--------------

       
        /// <summary>
        /// Export ReservationReports to XLSX
        /// </summary>
        /// <param name="shipmentReports">ShipmentReports</param>
       byte[] ReservationReportsToXlsx(IList<ReservationGridListModel> reservationReports);
       

        /// <summary>
        /// Export customer list to XLSX
        /// </summary>
        /// <param name="customers">Customers</param>
        byte[] ExportCustomersToXlsx(IList<Customer> customers);

        /// <summary>
        /// Export customer list to xml
        /// </summary>
        /// <param name="customers">Customers</param>
        /// <returns>Result in XML format</returns>
        string ExportCustomersToXml(IList<Customer> customers);

        /// <summary>
        /// Export newsletter subscribers to TXT
        /// </summary>
        /// <param name="subscriptions">Subscriptions</param>
        /// <returns>Result in TXT (string) format</returns>
        string ExportNewsletterSubscribersToTxt(IList<NewsLetterSubscription> subscriptions);

        /// <summary>
        /// Export states to TXT
        /// </summary>
        /// <param name="states">States</param>
        /// <returns>Result in TXT (string) format</returns>
        string ExportStatesToTxt(IList<StateProvince> states);

        byte[] ExportMealPlansToXlsx(IList<MealPlan> mealPlans, bool exportGrouped);

        #region NU-32
        /// <summary>
        /// Export meal plan list to XLSX
        /// </summary>
        /// <param name="stream">filestream</param>
        /// <param name="mealPlanInfos">Customers</param>
        // byte[] ExportMealPlansToXlsx(IList<MealPlan> mealPlans, bool exportGrouped);
        #endregion

        #region NU-33
        /// <summary>
        /// Export Donations to Excel
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="donations"></param>
        byte[] ExportDonationsToXlsx(IList<Donation> donations, bool filterData);
        #endregion

        #region NU-64
        string ExportStoreCommissionsToXml(IList<StoreCommissionReportLineModel> commissions);

        void ExportStoreCommissionsToXlsx(Stream stream, IList<StoreCommissionReportLineModel> commissions);
        #endregion

        #region NU-66
        byte[] ExportOrdersToXlsx(IList<OrderByDeliveryReportLine> orders);
        #endregion


        #region New Vertex

        /// <summary>
        /// TAX/SHIPPING - NEW
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="orders"></param>
        /// <param name="glcodes"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="reportName"></param>
        /// <param name="isFuture"></param>
        /// <param name="storeName"></param>
        void ExportPaymentReportToXlsxVERTEX(string filePath, IList<Order> orders, DateTime? startDate, DateTime? endDate, string reportName, bool isFuture, string storeName, bool isDelivery, bool isPerstore = false, DateTime? SDate = null, DateTime? EDate = null);

        /// <summary>
        /// TAX/SHIPPING NEW!
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="orders"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="reportName"></param>
        /// <param name="isFuture"></param>
        /// <param name="storeName"></param>
        void ExportDeliveryReportToXlsxVERTEX(string filePath, IList<Order> orders, DateTime? startDate, DateTime? endDate, string reportName, bool isFuture, string storeName);
        void ExportPaymentReport(string filePath, IList<Order> orders, DateTime? startDate, DateTime? endDate, string reportName, bool isFuture, string storeName, bool isDelivery);
        #endregion


        #region Old Reporting
        /// <summary>
        /// Export orders for payment to XLSX for finance reports
        /// </summary>
        /// <param name="filePath">File path to use</param>
        /// <param name="orders">Orders</param>
        /// <param name="glcodes">GL codes used for accounting</param>
        /// <param name="startDate">Date for report header</param>
        void ExportPaymentReportToXlsx(string filePath, IList<OrderExcelExport> orders, IList<GlCode> glcodes, DateTime? startDate, DateTime? endDate, string reportName, bool isFuture, string storeName);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="orders"></param>
        /// <param name="glcodes"></param>
        /// <param name="startdate"></param>
        /// <param name="endDate"></param>
        /// <param name="storeName"></param>
        void ExportDeliveryReportToXlsx(string filePath, IList<OrderExcelExport> orders, IList<GlCode> glcodes, DateTime? startdate, DateTime? endDate, string storeName);

        /// <summary>
        /// Export orders for refund to XLSX for finance reports
        /// </summary>
        /// <param name="filePath">File path to use</param>
        /// <param name="orders">Orders</param>
        /// <param name="glcodes">GL codes used for accounting</param>
        /// <param name="startDate">Date for report header</param>
        void ExportRefundReportToXlsx(string filePath, IList<OrderExcelExport> orders, IList<GlCode> glcodes, DateTime? startDate, DateTime? endDate, string reportName, bool isFuture, string storeName);

        #endregion

        void ExportJournalReporttoXlsx(string filePath, IList<OrderExcelExport> orders, DateTime? startDate, DateTime? endDate, string reportName, bool isFuture, string storeName);

        void ExportJournalDEliveryReporttoXlsx(string filePath, IList<OrderExcelExport> orders, DateTime? startDate, DateTime? endDate, string reportName, bool isFuture, string storeName);

        void ExportJournalPaymentReportPerStoretoXlsx(string filePath, IList<OrderExcelExport> orders, DateTime? startDate, DateTime? endDate, string reportName, bool isFuture, string storeName, int storeid = 0);

        void ExportJournalDEliveryReportPerStoretoXlsx(string filePath, IList<OrderExcelExport> orders, DateTime? startDate, DateTime? endDate, string reportName, bool isFuture, string storeName);

        void ExportAllUnitsRefundReportToXlsx(string filePath, IList<OrderExcelExport> orders, List<CovidRefunds> covidOrders, IList<GlCode> glcodes, DateTime? startDate, DateTime? endDate, string reportName, bool isFuture, string storeName);

    }
}
