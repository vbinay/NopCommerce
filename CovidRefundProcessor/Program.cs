using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Dapper;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using System.Data.SqlClient;
using System.Net;
using Nop.Core.Domain.Stores;
using System.IO;
using System.Text;
namespace CovidRefundProcessor
{
    class Program {
        static void Main(string[] args)
        {
            var logFileName = Environment.CurrentDirectory.TrimEnd('\\') + "\\" + DateTime.UtcNow.ToShortDateString() + ".log";
            using (SqlConnection sqlCon = new SqlConnection("Data Source=DESKTOP-R8E5326;Initial Catalog=nopcommerce_prod_38;User Id=smwbluedotuser2;Password=Tahz00!;Persist Security Info=False;MultipleActiveResultSets=True"))
            {
                List<BulkRefund> refunds = sqlCon.Query<BulkRefund>("SELECT * FROM BulkRefund where IsProcessed=0").ToList();
                foreach (var individualRefund in refunds)
                {
                    /// Campus local is not secure in my local environment, so we're going to do some stuff here.
                    System.Net.ServicePointManager.ServerCertificateValidationCallback = ((sender, certificate, chain, sslPolicyErrors) => true);

                    // Get the relevant bits of data we may need for whatever reason.
                    Order individualOrder = sqlCon.Query<Order>($"SELECT * FROM [Order] where ID={individualRefund.OrderId}").FirstOrDefault();
                    OrderItem individualOrderItem = sqlCon.Query<OrderItem>($"SELECT * FROM OrderItem where ID={individualRefund.OrderItemId}").FirstOrDefault();
                    Product individualProduct = sqlCon.Query<Product>($"SELECT * FROM Product where ID={individualOrderItem.ProductId}").FirstOrDefault();
                    Store individualStore = sqlCon.Query<Store>($"SELECT * From Store where ID={individualOrder.StoreId}").FirstOrDefault();
                    // With the individual components pulled, we should be able to populate a PartiallyRefundGrid object 
                    // from NOP and pass in a normal JSON Call with it in POST. In essence, we can leave the functionality
                    // in NOP and just rely on the accessibility of the administrative post methods with a bypass password for
                    // the console to be able to utilize them without authentication.

                    // First, let's construct the URLs for this store. This'll make sure we avoid transaction tag errors and using the existing
                    // infrastructure allows us to quickly piece together this work.
                    #region "URLS"
                    var customrefundCallUrl = "";
                    if (individualStore.SslEnabled)
                    {                                                                                            
                        customrefundCallUrl = individualStore.SecureUrl.TrimEnd('/') + "/admin/order/updatecustomrefunddata";
                    }
                    else
                    {
                        customrefundCallUrl = individualStore.Url.TrimEnd('/') + "/admin/order/updatecustomrefunddata";
                    }
                    var partialrefundCallUrl = "";
                    if (individualStore.SslEnabled)
                    {
                        partialrefundCallUrl = individualStore.SecureUrl.TrimEnd('/') + "/admin/order/UpdatePartiallyRefundData";
                    }
                    else
                    {
                        partialrefundCallUrl = individualStore.Url.TrimEnd('/') + "/admin/order/UpdatePartiallyRefundData";
                    }
                    #endregion

                    WebRequest webReq = null;
                    var refundObject = new PartiallyRefundGridModel
                    {
                        DeliveryGLCodeName = individualRefund.DeliveryGLName,
                        DeliveryPickupAmount = individualRefund.DeliveryPickupAmount.HasValue ? individualRefund.DeliveryPickupAmount.Value : 0.00m,
                        DeliveryTax = individualRefund.DeliveryTax.HasValue ? individualRefund.DeliveryTax.Value : 0.00m,
                        DeliveryTaxName = "70161010",
                        Error = "",
                        GLAmount1 = individualRefund.GlAmount1,
                        GLAmount2 = individualRefund.GlAmount2.HasValue ? individualRefund.GlAmount2.Value : 0,
                        GLAmount3 = individualRefund.GlAmount3.HasValue ? individualRefund.GlAmount3.Value : 0,
                        Glcodeid1 = individualRefund.Glcodeid1,
                        Glcodeid2 = individualRefund.Glcodeid2.HasValue ? individualRefund.Glcodeid2.Value : 0,
                        Glcodeid3 = individualRefund.Glcodeid3.HasValue ? individualRefund.Glcodeid3.Value : 0,
                        GLCodeName1 = individualRefund.GLCode1,
                        GLCodeName2 = individualRefund.GLCode2,
                        GLCodeName3 = individualRefund.GLCode3,
                        OrderId = individualOrder.Id,
                        OrderItemId = individualOrderItem.Id,
                        ProductId = individualProduct.Id,
                        ProductName = individualProduct.Name,
                        RefundedTaxAmount1 = individualRefund.RefundedTaxAmount1.HasValue ? individualRefund.RefundedTaxAmount1.Value : 0.00m,
                        RefundedTaxAmount2 = individualRefund.RefundedTaxAmount2.HasValue ? individualRefund.RefundedTaxAmount2.Value : 0.00m,
                        RefundedTaxAmount3 = individualRefund.RefundedTaxAmount3.HasValue ? individualRefund.RefundedTaxAmount3.Value : 0.00m,
                        TaxAmount1 = (individualOrderItem.TaxAmount1.HasValue) ? individualOrderItem.TaxAmount1.Value : 0.00m,
                        TaxAmount2 = (individualOrderItem.TaxAmount2.HasValue) ? individualOrderItem.TaxAmount2.Value : 0.00m,
                        TaxAmount3 = (individualOrderItem.TaxAmount3.HasValue) ? individualOrderItem.TaxAmount3.Value : 0.00m,
                        ShippingAmount = individualOrderItem.ShippingAmount,
                        ShippingGlName = "62610001",
                        ShippingTax = individualOrderItem.ShippingTax,
                        ShippingTaxName = "48702001",
                        Sku = individualProduct.Sku,
                        TaxName1 = individualOrderItem.TaxName1,
                        TaxName2 = individualOrderItem.TaxName2,
                        TaxName3 = individualOrderItem.TaxName3,
                        ConsoleRefund = "Passw0rdF0rC0ns0l3"
                    };
                    var json = Newtonsoft.Json.JsonConvert.SerializeObject(refundObject);
                    try
                    {
                        if (individualRefund.ProcessVertex)
                        {
                            // We call the original partial, and if we catch an exception (at least until
                            // the business or test results dictate we should do otherwise), then we'll call custom.
                            // We need to set up a web request.
                            webReq = HttpWebRequest.Create(partialrefundCallUrl);
                            refundObject = ProcessRequest(webReq, json);
                        }
                        else
                        {
                            // We call custom every time here.
                            webReq = HttpWebRequest.Create(customrefundCallUrl);
                            refundObject = ProcessRequest(webReq, json);
                       }
                    }
                    catch (Exception ex)
                    {
                        // Originally, I had planned on having the refund processor attempt a custom
                        // in the event that an original refund failed. 
                        // After some thought and discussion, we're skipping this functionality due to the 
                        // fact that certain scenarios in the original partial refund scenario can still hit 
                        // the gateway and then fail out. This will prevent us accidentally double-refunding
                        // due to an error on partial. We can always go and flag the original refund as ProcessVertex=0 IsProcessed=0
                        // to attempt another refund through the custom processor on a later run.
                        // Note the error but do not fail!
                        //if (individualRefund.ProcessVertex)
                        //{
                        //    // try again on custom refund.
                        //    try
                        //    {
                        //        webReq = HttpWebRequest.Create(customrefundCallUrl);
                        //        refundObject = ProcessRequest(webReq, json);

                        //    }
                        //    catch (Exception customEx)
                        //    {
                        //        // fail out and save error message here!
                        //        individualRefund.IsProcessed = true;
                        //        var report = GetErrorMessages(ex, "Error in both partial and custom refund.", customEx);                                
                        //        individualRefund.Errors = report.ToString();
                        //        individualRefund.ProcessedDateUtc = null;
                        //    }
                        //}
                        //// this is a fail out on custom refund only, with no original partial failout.
                        //else
                        //{
                            // fail out and save error message.
                            individualRefund.IsProcessed = true;
                            var report = GetErrorMessages(ex,"Refund failed.");
                            individualRefund.Errors = report.ToString();
                            individualRefund.ProcessedDateUtc = null;
                        //}
                    }

                    // This truly indicates success one way or the other.
                    if (refundObject.Success == "Partially Refunded successfully" || refundObject.Success == "true")
                    {
                        individualRefund.Errors = $"No errors, partially refunded order {individualOrder.Id} order item {individualOrderItem.Id} successfully for {refundObject.TotalRefund}.";
                        individualRefund.IsProcessed = true;
                        individualRefund.ProcessedDateUtc = DateTime.UtcNow;
                    }
                    WriteTextEntry(logFileName, individualRefund.Errors);
                    SaveBulkEntry(individualRefund, sqlCon);                    
                }                
            }
        }

        public static StringBuilder GetErrorMessages(Exception ex, string Header, Exception customEx = null)
        {
            var report = new System.Text.StringBuilder();
            report.AppendLine(Header);
            if (customEx != null)
            {                 
                report.AppendLine($"Custom exception: {customEx.Message}");
            }
            report.AppendLine($"Non-custom exception: {ex.Message}");
            report.AppendLine($"Stack trace: {ex.StackTrace.ToString()}");
            report.AppendLine($"Refund not processed successfully, but no further attempts will be made via this database!");
            return report;
        }

        // Save the updates to the entry so it is marked as processed and any relevant errors are logged in place.
        public static bool SaveBulkEntry (BulkRefund refundToSave, SqlConnection sqlCon)
        {
            try { 
                using (var connection = new SqlConnection("Data Source=DESKTOP-R8E5326;Initial Catalog=nopcommerce_prod_38;User Id=smwbluedotuser2;Password=Tahz00!;Persist Security Info=False;MultipleActiveResultSets=True"))
                {
                    var sqlStatement = @"UPDATE BulkRefund SET
                    ProcessedDateUtc=@ProcessedDateUtc,
                    Errors = @Errors,
                    IsProcessed = @IsProcessed
                    where BulkRefundId=@BulkRefundId";

                    connection.Execute(sqlStatement, refundToSave);
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static void WriteTextEntry(string fileName, string logEntry)
        {
            try
            {
                var fileStream = new FileStream(fileName, FileMode.Append);
                if (fileStream.CanWrite)
                {
                    fileStream.Write(System.Text.ASCIIEncoding.UTF8.GetBytes(logEntry), 0, logEntry.Length);
                }
            }
            catch
            {

            }
        }
        /// <summary>
        /// This will actually send our request off so we don't have to repeat this bunch of code over and over.
        /// </summary>
        /// <param name="webReq"></param>
        /// <param name="jsonBody"></param>
        /// <returns></returns>
        public static PartiallyRefundGridModel ProcessRequest(WebRequest webReq, string jsonBody)
        {
            try 
            {
                webReq.Method = "POST";
                webReq.Headers.Add("consoleKey", "c0ns0l3p4ss");
                webReq.ContentType = "application/json"; 
                using (var requestStream = new StreamWriter(webReq.GetRequestStream()))
                {
                    requestStream.Write(jsonBody);
                }            
                var webResp = webReq.GetResponse();
                var responseBody = "";
                var responseStream = new StreamReader(webResp.GetResponseStream());
                responseBody = responseStream.ReadToEnd();
                var getResult = Newtonsoft.Json.JsonConvert.DeserializeObject<PartiallyRefundGridModel>(responseBody);
                return getResult;
            }
            catch (Exception ex)
            {
                var responseObject = Newtonsoft.Json.JsonConvert.DeserializeObject<PartiallyRefundGridModel>(jsonBody);
                responseObject.Success = "false";
                responseObject.Error = ex.Message;
                return responseObject;
            }
        }
    }
}
