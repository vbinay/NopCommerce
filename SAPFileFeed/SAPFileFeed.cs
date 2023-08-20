using Cinchoo.PGP;
using log4net;
using Nop.Services.Orders;
using Org.BouncyCastle.Bcpg.OpenPgp;
using SAPFileFeed.SAP;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Tamir.SharpSsh.jsch;
using Tamir.SharpSsh.Wrappers;
using static SAPFileFeed.VertexResponse;

namespace SAPFileFeed
{
    class SAPFileFeed
    {
        private static readonly ILog Logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public static SapFileFeed dbEntity = new SapFileFeed();


        public static void RunSAPFeed()
        {
            try
            {
                int totalOrderstoBeProcessed = 0;
                int ProcessedOrders = 0;
                string failedOrders = string.Empty;
                //SAPWorkFlowMessagingSystem.SendMail("SAPfile_03_10_2020_22_06_09", totalOrderstoBeProcessed, ProcessedOrders);
                SAPLogger.WriteLogs(string.Format("=================START OF BATCH ON DATE TIME- {0}=====================", DateTime.Now));
                List<SAPOrder> orders = getSAPOrdersToBeProcessed();
                orders.OrderBy(x => x.OrderId).ThenBy(y => y.OrderItemId);
                totalOrderstoBeProcessed = orders.Select(x => x.OrderId).Distinct().Count();
                SAPLogger.WriteLogs("Orders received Successfully");
                //   StringBuilder SbSMWData = new StringBuilder();
                SAPFile file = new SAPFile();
                file.Builder = new StringBuilder();
                file.Header = new StringBuilder();
                file.Trailer = new SAPTrailer();
                //Header


                var header = new SAPHeader();
                header.FileDate = DateTime.Now.ToString("yyyyMMdd");
                file.Header.AppendLine(header.HeaderLiteral + "|" + header.FileDate);
                int lineItemsCount = 0;
                Dictionary<int, int> lineItemsDict = new Dictionary<int, int>();
                Dictionary<int, int> ordersProcessedAlready = new Dictionary<int, int>();
                //BODY
                foreach (var order in orders)
                {
                    SAPLogger.WriteLogs(string.Format("SAP_Processing For Order No-{0}", order.OrderId));

                    var maxSeqNumForOrders = getLastSequnceNumber(order.OrderId, 1, order.OrderItemId);
                    if (maxSeqNumForOrders == 0)
                        maxSeqNumForOrders = getLastSequnceNumber(order.OrderId, order.StatusID, order.OrderItemId);

                    if (maxSeqNumForOrders == 0)
                    {
                        if (!lineItemsDict.ContainsKey(order.OrderId))
                        {
                            lineItemsCount = 1;
                            lineItemsDict.Add(order.OrderId, order.OrderId);

                        }
                        else
                        {
                            lineItemsCount = lineItemsCount + 1;
                        }
                    }
                    else
                    {
                        lineItemsCount = maxSeqNumForOrders;
                    }

                    SAPLogger.WriteLogs(string.Format("SAP_feteched Records From Vertex for Processing for Order No- {0} ", order.OrderId));

                    List<DistributiveTaxResponse> response = getDistributiveDataResponseForOrder(order.OrderId, order.StatusID, order.OrderItemId,order.ProductId); // Last parameter checks if the product is a bundle prodct and sends the data related to the bundle productid
                    if (response.Count > 0)
                    {
                        List<SAPSalesJournalEntry> entries = new List<SAPSalesJournalEntry>();
                        SAPLogger.WriteLogs(string.Format("SAP_Inserting records in the SapSalesJurnal before exporting for Order No- {0} ", order.OrderId));
                        bool insertedintoSAPJournal = PostDistributiveTaxDataToSAPSalesJournal(response);
                        SAPLogger.WriteLogs(string.Format("SAP_Inserting records in the SapSalesJurnal  Order No- {0} is insertedintoSAPJournal-{1}", order.OrderId, insertedintoSAPJournal));
                        UpdateSAPSOrderProcess(order.Id, order.OrderItemId, order.StatusID, lineItemsCount);
                        SAPLogger.WriteLogs(string.Format("SAP_Updating SapOrderPorcess with IsProcessed as false so make it incremental for Order No- {0} ", order.OrderId));
                        if (insertedintoSAPJournal)
                        {
                            var listProducts = new List<int>();
                            var OrderInfo = dbEntity.Orders.Where(x => x.Id == order.OrderId).FirstOrDefault();
                            var orderdetails = dbEntity.OrderItems.Where(x => x.OrderId == order.OrderId && x.Id == order.OrderItemId).FirstOrDefault(); // Order Items

                            var sameProductcount = OrderInfo.OrderItems.GroupBy(x => x.ProductId)
                                 .Where(g => g.Count() > 1)
                                 .Select(y => y.Key)
                                 .ToList();
                            if (sameProductcount.Any())
                            {
                                foreach (var itemData in sameProductcount)
                                {
                                    listProducts.Add(itemData);
                                }
                            }

                            if (listProducts.Contains(orderdetails.ProductId))
                            {
                                entries = getSAPDatabaseResults(order.OrderId, order.StatusID, order.OrderItemId);
                            }
                            else
                            {
                                entries = getSAPDatabaseResults(order.OrderId, order.StatusID);
                            }

                            if (entries.Count == 0)
                            {
                                Console.WriteLine("SAP_No record found for today");
                                failedOrders = failedOrders + order.Id + ",";
                                continue;
                            }

                            if (order.StatusID == 2 || order.StatusID == 4)
                            {
                                SAPLogger.WriteLogs(string.Format("SAP_Generating Flat file for the  Products with Order No- {0} and with Status of {1}", order.OrderId, order.StatusID));
                                file = BuildFlatFile(file, entries, response, true, lineItemsCount, order.OrderId, order.OrderItemId);
                                SAPLogger.WriteLogs(string.Format("Flat file for Order No- {0} and with Status of {1} created Successfully", order.OrderId, order.StatusID));
                                if (!ordersProcessedAlready.ContainsKey(order.OrderId))
                                {
                                    ordersProcessedAlready.Add(order.OrderId, order.OrderId);
                                    ProcessedOrders++;

                                }
                            }
                            else //status 1 or 3 don't need vertex
                            {
                                SAPLogger.WriteLogs(string.Format("SAP_Generating Flat file for the  Products with Order No- {0} and with Status of {1}", order.OrderId, order.StatusID));
                                file = BuildFlatFile(file, entries, response, true, lineItemsCount, order.OrderId, order.OrderItemId);
                                SAPLogger.WriteLogs(string.Format("Flat file for Order No- {0} and with Status of {1} created Successfully", order.OrderId, order.StatusID));
                                if (!ordersProcessedAlready.ContainsKey(order.OrderId))
                                {
                                    ordersProcessedAlready.Add(order.OrderId, order.OrderId);
                                    ProcessedOrders++;

                                }


                            }
                            UpdateSAPSalesJournalOrderItem(order.Id, order.OrderItemId, order.StatusID);
                        }
                        else
                        {
                            SAPLogger.WriteLogs(string.Format("Failed to insert record For SAP Related Products with Order No- {0}", order.OrderId));
                            failedOrders = failedOrders + order.Id + ",";
                            continue;
                        }
                    }
                    else
                    {
                        failedOrders = failedOrders + order.Id + ",";
                    }
                }

                StringBuilder completeFile = new StringBuilder();
                completeFile.Append(file.Header);
                completeFile.Append(file.Builder);

                ////Trailer Record
                var trailer = file.Trailer;
                trailer.Count = file.TrailerCount.ToString().PadLeft(10, '0');
                trailer.AmountSign = (file.TrailerTotal < 0 ? "-" : "");
                trailer.Amount = Convert.ToString(file.TrailerTotal).Replace(",", "");
                completeFile.AppendLine(trailer.TrailerLiteral + "|" + trailer.Count + "|" + trailer.AmountSign + "|0");

                //  bool fileAlreadyOnServer = false;
                bool uploadToServer = true;
                string fileName = string.Empty;
                string fileNamePgp = string.Empty;

                if (uploadToServer)
                {
                    var path = "";
                    if (!String.IsNullOrEmpty(ConfigurationManager.AppSettings["TempFolder"]))
                    {
                        path = ConfigurationManager.AppSettings["TempFolder"];
                    }
                    fileName = "SAPfile_" + DateTime.Now.ToString("MM_dd_yyyy_HH_mm_ss") + ".txt";
                    fileNamePgp = "SAPfile_" + DateTime.Now.ToString("MM_dd_yyyy_HH_mm_ss") + ".txt" + ".PGP";

                    try
                    {
                        if (!Directory.Exists(path))
                        {
                            SAPLogger.WriteLogs(string.Format("SAP_Creating Folder at {0} ", path));
                            Directory.CreateDirectory(path);
                            SAPLogger.WriteLogs(String.Format("SAP_Directory Creation Successful at {0}", path));
                        }

                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"SAPFileFeed: Exception handled while attempting IO Operation to create folder path: {ex.Message}");
                    }
                    string FullPath = Path.Combine(path, fileName);
                    File.Create(FullPath).Close();
                    File.AppendAllText(FullPath, completeFile.ToString());
                    string keyName = "Sodexho_Operations_LLC2";
                    string dirpath = Directory.GetCurrentDirectory();
                    string KeyPath = Path.Combine(Directory.GetCurrentDirectory(), keyName);

                    //using (ChoPGPEncryptDecrypt pgp = new ChoPGPEncryptDecrypt())
                    //    pgp.GenerateKey(KeyPath, KeyPath1);

                    try
                    {
                        using (ChoPGPEncryptDecrypt pgp = new ChoPGPEncryptDecrypt())
                        {
                            SAPLogger.WriteLogs("SAP_initiating Encryption");
                            pgp.EncryptFile(path + fileName, path + fileName + ".PGP", KeyPath, true, false);
                            SAPLogger.WriteLogs(string.Format("SAP_encryption Successfull for File {0} with key {1} ", fileName, KeyPath));
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"SAPFileFeed: Exception handled while attempting Encryption operation: {ex.Message}");
                    }

                    try
                    {
                        SAPLogger.WriteLogs("Initiating Upload to Server");
                        using (var ssh = new SFTPUtil("nftp.mysodexo.com", "SMW-Prod", "SS9rNesw"))
                        {
                            IList<ChannelSftp.LsEntry> files = ssh.ListFiles("/");

                            try
                            {
                                SAPLogger.WriteLogs("SAP_Uploading To Server: " + path + fileName + ".PGP");
                                ssh.PutFile(path + fileName + ".PGP", fileNamePgp);
                                SAPLogger.WriteLogs("SAP_Upload complete.");
                            }
                            catch (Exception ex)
                            {
                                Logger.Error($"SAPFileFeed: Exception handled while attempting SFTP PUT operation: {ex.Message}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"SAPFileFeed: Exception handled while attempting SFTP connection operation: {ex.Message}");
                    }

                }
                SAPLogger.WriteLogs("Process Complete");
                SAPLogger.WriteLogs(string.Format("===============END OF BATCH ON DATE TIME {0}=========================", DateTime.Now));

                //Sending Mail related to files after Processing
                try
                {
                    SAPWorkFlowMessagingSystem.SendMail(fileName, totalOrderstoBeProcessed, ProcessedOrders, failedOrders);
                }
                catch (Exception)
                {

                }
            }
            catch (Exception ex)
            {
                SAPLogger.WriteLogs(string.Format("SAP_Exception Identified as {0}", ex.Message));
                Logger.Error(ex.Message);
            }

        }


        public static void CreateFlatFile(string textToAppend)
        {
            var path = "";
            if (!String.IsNullOrEmpty(ConfigurationManager.AppSettings["TempFolder"]))
            {
                path = ConfigurationManager.AppSettings["TempFolder"];
            }

            var fileName = "SAPfile_" + DateTime.Now.ToString("MM_dd_yyyy_HH_mm_ss") + ".txt";

            string FullPath = Path.Combine(path, fileName);

            File.Create(FullPath).Close();
            File.AppendAllText(FullPath, textToAppend);
        }

        public static SAPFile BuildFlatFile(SAPFile file, List<SAPSalesJournalEntry> entries, List<DistributiveTaxResponse> response, bool isVertexLineNeeded, int lineItemCount, int orderid, int orderitemid)
        {
            VertexResponse obj = new VertexResponse();
            StringBuilder sb = new StringBuilder();
            int trailerCount = 0;
            decimal trailerTotal = decimal.Zero;
            int flag = 1, m = 1, flag1 = 1, n = 1, mealplanCounter = 0, partialRefundCounter = 0;
            bool isMealPlan = false;
            //   i++;

            var Recordstatus = entries.Select(x => x.Status).Distinct().OrderBy(x => x).ToList();

            var listProducts = new List<int>();
            var OrderInfo = dbEntity.Orders.Where(x => x.Id == orderid).FirstOrDefault();
            var orderdetails = dbEntity.OrderItems.Where(x => x.OrderId == orderid && x.Id == orderitemid).FirstOrDefault(); // Order Items

            var sameProductcount = OrderInfo.OrderItems.GroupBy(x => x.ProductId)
                 .Where(g => g.Count() > 1)
                 .Select(y => y.Key)
                 .ToList();
            if (sameProductcount.Any())
            {
                foreach (var itemData in sameProductcount)
                {
                    listProducts.Add(itemData);
                }
            }


            foreach (var status in Recordstatus)
            {
                List<DynamicVertexGLCode> vertexOrderTaxGls = new List<DynamicVertexGLCode>();
                var RecordonStatus = entries.Where(x => x.Status == status).OrderBy(x => x.OrderNumber).ToList();
                foreach (SAPSalesJournalEntry plan in RecordonStatus)
                {
                    if (plan.OrderNumber == flag)
                    {
                        m++;
                    }
                    else
                    {
                        m = 1;
                        flag = plan.OrderNumber;
                    }
                    if (plan.Status == "1") // When the Records are not fulfilled and is Deferred state
                    {
                        if (plan.ProductAmount != 0)
                        {
                            // against 2004 Debit that is No Sign
                            sb.AppendLine(plan.RecordIndicator + "|" + plan.OrderNumber + "|" + plan.Status + "|" + lineItemCount.ToString().PadLeft(3, '0') + "|" + plan.TransactionDate.ToString("MMddyyyy") + "|" + plan.TransactionTime + "|" + plan.CostCenterNumber + "|" + RetrieveGLCodeByStatus(0, true, false, plan.IsMealPlan, plan.IsVendor, plan.IsDonation, plan.isDownload, plan.productId, plan.OrderNumber, true, false, plan.OrderItemId) + "|" + string.Empty + "|" + plan.OrderAmount.ToString("N2").Replace(",", ""));
                            m++;
                            trailerCount++;
                            //against 2040 /2041Credit that is Negative Sign
                            sb.AppendLine(plan.RecordIndicator + "|" + plan.OrderNumber + "|" + plan.Status + "|" + lineItemCount.ToString().PadLeft(3, '0') + "|" + plan.TransactionDate.ToString("MMddyyyy") + "|" + plan.TransactionTime + "|" + plan.CostCenterNumber + "|" + RetrieveGLCodeByStatus(1, true, false, plan.IsMealPlan, plan.IsVendor, plan.IsDonation, plan.isDownload, plan.productId, plan.OrderNumber, true, false, plan.OrderItemId) + "|" + "-" + "|" + plan.ProductAmount.ToString("N2").Replace(",", "") + "|" + plan.EcommLocalProductDescription);
                            m++;
                            trailerCount++;
                        }
                        if (plan.DeliveryPickUpFee != 0)
                        {
                            if (plan.IsDelivery)
                            {
                                //// against 2004 Debit that is No Sign
                                //sb.AppendLine(plan.RecordIndicator + "|" + plan.OrderNumber + "|" + plan.Status + "|" + m.ToString().PadLeft(3, '0') + "|" + plan.TransactionDate.ToString("MMddyyyy") + "|" + plan.TransactionTime + "|" + plan.CostCenterNumber + "|" + RetrieveGLCodeByStatus(0, true, false, plan.IsMealPlan, plan.IsVendor, plan.IsDonation, plan.isDownload, plan.productId, plan.OrderNumber, true, false) + "|" + string.Empty + "|" + plan.DeliveryPickUpFee.ToString("N2").Replace(",", ""));
                                //m++;
                                //trailerCount++;

                                //against 2040 Credit that is Negative Sign
                                sb.AppendLine(plan.RecordIndicator + "|" + plan.OrderNumber + "|" + plan.Status + "|" + lineItemCount.ToString().PadLeft(3, '0') + "|" + plan.TransactionDate.ToString("MMddyyyy") + "|" + plan.TransactionTime + "|" + plan.CostCenterNumber + "|" + RetrieveGLCodeByStatus(1, false, false, plan.IsMealPlan, plan.IsVendor, plan.IsDonation, plan.isDownload, plan.productId, plan.OrderNumber, true, false, plan.OrderItemId) + "|" + "-" + "|" + plan.DeliveryPickUpFee.ToString("N2").Replace(",", "") + "|" + plan.EcommLocalProductDescription + "-Delivery");
                                m++;
                                trailerCount++;
                            }
                        }
                        if (plan.Shippingfee != 0)
                        {
                            if (plan.IsShipping)
                            {
                                //// against 2004 Debit that is No Sign
                                //sb.AppendLine(plan.RecordIndicator + "|" + plan.OrderNumber + "|" + plan.Status + "|" + m.ToString().PadLeft(3, '0') + "|" + plan.TransactionDate.ToString("MMddyyyy") + "|" + plan.TransactionTime + "|" + plan.CostCenterNumber + "|" + RetrieveGLCodeByStatus(0, true, false, plan.IsMealPlan, plan.IsVendor, plan.IsDonation, plan.isDownload, plan.productId, plan.OrderNumber, true, false) + "|" + string.Empty + "|" + plan.Shippingfee.ToString("N2").Replace(",", ""));
                                //m++;
                                //trailerCount++;

                                //against 2040 Credit that is Negative Sign
                                sb.AppendLine(plan.RecordIndicator + "|" + plan.OrderNumber + "|" + plan.Status + "|" + lineItemCount.ToString().PadLeft(3, '0') + "|" + plan.TransactionDate.ToString("MMddyyyy") + "|" + plan.TransactionTime + "|" + plan.CostCenterNumber + "|" + RetrieveGLCodeByStatus(1, false, false, plan.IsMealPlan, plan.IsVendor, plan.IsDonation, plan.isDownload, plan.productId, plan.OrderNumber, false, true, plan.OrderItemId) + "|" + "-" + "|" + plan.Shippingfee.ToString("N2").Replace(",", "") + "|" + plan.EcommLocalProductDescription + "-Shipping");
                                m++;
                                trailerCount++;
                            }
                        }

                        if (listProducts.Contains(plan.productId))
                        {
                            vertexOrderTaxGls = obj.GetVertexGlBreakdown(plan.OrderNumber, plan.productId, 3, orderitemid);
                        }
                        else
                        {
                            vertexOrderTaxGls = obj.GetVertexGlBreakdown(plan.OrderNumber, plan.productId, 3);
                        }
                        if (vertexOrderTaxGls.Any())
                        {
                            foreach (var taxGls in vertexOrderTaxGls)
                            {
                                if (taxGls.Total != 0)
                                {
                                    sb.AppendLine(plan.RecordIndicator + "|" + plan.OrderNumber + "|" + plan.Status + "|" + lineItemCount.ToString().PadLeft(3, '0') + "|" + plan.TransactionDate.ToString("MMddyyyy") + "|" + plan.TransactionTime + "|" + plan.CostCenterNumber + "|" + taxGls.GlCode + "|" + "-" + "|" + taxGls.Total.ToString("N2").Replace(",", ""));
                                    m++;
                                    trailerCount++;
                                }
                            }
                        }
                    }
                    else if (plan.Status == "2")
                    {
                        if (plan.IsMealPlan || plan.IsDonation || plan.isDownload)
                        {
                            string datapocess = RetrieveGLCodeByStatus(2, false, false, plan.IsMealPlan, plan.IsVendor, plan.IsDonation, plan.isDownload, plan.productId, plan.OrderNumber, false, false, plan.OrderItemId);
                            if (datapocess.Contains('|'))
                            {
                                string[] glsandAmount = datapocess.Split('|');
                                var q = glsandAmount.Zip(glsandAmount.Skip(1), (Key, Value) => new { Key, Value })
                                             .Where((pair, index) => index % 2 == 0)
                                             .ToDictionary(pair => pair.Key, pair => pair.Value);
                                if (plan.OrderAmount != decimal.Zero)
                                {
                                    sb.AppendLine(plan.RecordIndicator + "|" + plan.OrderNumber + "|" + 1 + "|" + lineItemCount.ToString().PadLeft(3, '0') + "|" + plan.TransactionDate.ToString("MMddyyyy") + "|" + plan.TransactionTime + "|" + plan.CostCenterNumber + "|" + RetrieveGLCodeByStatus(0, true, false, plan.IsMealPlan, plan.IsVendor, plan.IsDonation, plan.isDownload, plan.productId, plan.OrderNumber, true, false, plan.OrderItemId) + "|" + string.Empty + "|" + plan.OrderAmount.ToString("N2").Replace(",", "") + "|" + plan.EcommLocalProductDescription);
                                    m++;
                                    trailerCount++;
                                    string amount = string.Empty;
                                    foreach (var item in q)
                                    {
                                        if (item.Key.Split('_')[0] == "64701000")
                                        {
                                            amount = item.Value.Replace("-", "");
                                            amount = Convert.ToString(Convert.ToDecimal(amount) * plan.Quantity);
                                            amount = Convert.ToString(Math.Round(Convert.ToDecimal(amount), 2));
                                            plan.AmountSign = string.Empty;
                                        }
                                        else
                                        {
                                            amount = item.Value;
                                            amount = Convert.ToString(Convert.ToDecimal(amount) * plan.Quantity);
                                            amount = Convert.ToString(Math.Round(Convert.ToDecimal(amount), 2));
                                            plan.AmountSign = "-";
                                        }
                                        // against 2004 Debit that is No Sign


                                        // Meal plans and other fulfilllable items goes as Credit with Negtaive amounts
                                        sb.AppendLine(plan.RecordIndicator + "|" + plan.OrderNumber + "|" + 1 + "|" + lineItemCount.ToString().PadLeft(3, '0') + "|" + plan.TransactionDate.ToString("MMddyyyy") + "|" + plan.TransactionTime + "|" + plan.CostCenterNumber + "|" + item.Key.Split('_')[0] + "|" + plan.AmountSign + "|" + amount.Replace(",", ""));
                                        m++;
                                        trailerCount++;
                                        mealplanCounter++;
                                    }
                                }
                            }
                            else
                            {
                                if (plan.ProductAmount != 0)
                                {
                                    // against 2004 Debit that is No Sign
                                    sb.AppendLine(plan.RecordIndicator + "|" + plan.OrderNumber + "|" + 1 + "|" + lineItemCount.ToString().PadLeft(3, '0') + "|" + plan.TransactionDate.ToString("MMddyyyy") + "|" + plan.TransactionTime + "|" + plan.CostCenterNumber + "|" + RetrieveGLCodeByStatus(0, true, false, plan.IsMealPlan, plan.IsVendor, plan.IsDonation, plan.isDownload, plan.productId, plan.OrderNumber, true, false, plan.OrderItemId) + "|" + string.Empty + "|" + plan.OrderAmount.ToString("N2").Replace(",", ""));
                                    m++;
                                    trailerCount++;

                                    sb.AppendLine(plan.RecordIndicator + "|" + plan.OrderNumber + "|" + 1 + "|" + lineItemCount.ToString().PadLeft(3, '0') + "|" + plan.TransactionDate.ToString("MMddyyyy") + "|" + plan.TransactionTime + "|" + plan.CostCenterNumber + "|" + RetrieveGLCodeByStatus(2, false, false, plan.IsMealPlan, plan.IsVendor, plan.IsDonation, plan.isDownload, plan.productId, plan.OrderNumber, false, false, plan.OrderItemId) + "|" + "-" + "|" + plan.ProductAmount.ToString("N2").Replace(",", "") + "|" + plan.EcommLocalProductDescription);
                                    m++;
                                    trailerCount++;
                                }
                            }
                        }
                        if (!plan.IsMealPlan && !plan.IsDonation && !plan.isDownload)
                        {
                            if (plan.ProductAmount != 0)
                            {
                                if (plan.IsVendor)
                                {
                                    sb.AppendLine(plan.RecordIndicator + "|" + plan.OrderNumber + "|" + plan.Status + "|" + lineItemCount.ToString().PadLeft(3, '0') + "|" + plan.TransactionDate.ToString("MMddyyyy") + "|" + plan.TransactionTime + "|" + plan.CostCenterNumber + "|" + RetrieveGLCodeByStatus(1, true, false, plan.IsMealPlan, plan.IsVendor, plan.IsDonation, plan.isDownload, plan.productId, plan.OrderNumber, true, false, plan.OrderItemId) + "|" + string.Empty + "|" + (plan.ProductAmount).ToString("N2").Replace(",", ""));
                                    m++;
                                    trailerCount++;

                                    sb.AppendLine(plan.RecordIndicator + "|" + plan.OrderNumber + "|" + plan.Status + "|" + lineItemCount.ToString().PadLeft(3, '0') + "|" + plan.TransactionDate.ToString("MMddyyyy") + "|" + plan.TransactionTime + "|" + plan.CostCenterNumber + "|" + "48702040" + "|" + string.Empty + "|" + (plan.Shippingfee).ToString("N2").Replace(",", ""));
                                    m++;
                                    trailerCount++;
                                }
                                else
                                {

                                    sb.AppendLine(plan.RecordIndicator + "|" + plan.OrderNumber + "|" + plan.Status + "|" + lineItemCount.ToString().PadLeft(3, '0') + "|" + plan.TransactionDate.ToString("MMddyyyy") + "|" + plan.TransactionTime + "|" + plan.CostCenterNumber + "|" + RetrieveGLCodeByStatus(1, true, false, plan.IsMealPlan, plan.IsVendor, plan.IsDonation, plan.isDownload, plan.productId, plan.OrderNumber, true, false, plan.OrderItemId) + "|" + string.Empty + "|" + (plan.OrderAmount).ToString("N2").Replace(",", ""));
                                    m++;
                                    trailerCount++;
                                }

                                sb.AppendLine(plan.RecordIndicator + "|" + plan.OrderNumber + "|" + plan.Status + "|" + lineItemCount.ToString().PadLeft(3, '0') + "|" + plan.TransactionDate.ToString("MMddyyyy") + "|" + plan.TransactionTime + "|" + plan.CostCenterNumber + "|" + RetrieveGLCodeByStatus(2, false, false, plan.IsMealPlan, plan.IsVendor, plan.IsDonation, plan.isDownload, plan.productId, plan.OrderNumber, false, false, plan.OrderItemId) + "|" + "-" + "|" + (plan.ProductAmount).ToString("N2").Replace(",", "") + "|" + plan.EcommLocalProductDescription);
                                m++;
                                trailerCount++;
                            }
                        }
                        if (plan.IsDelivery)
                        {
                            if (plan.DeliveryPickUpFee != 0)
                            {
                                //sb.AppendLine(plan.RecordIndicator + "|" + plan.OrderNumber + "|" + plan.Status + "|" + m.ToString().PadLeft(3, '0') + "|" + plan.TransactionDate.ToString("MMddyyyy") + "|" + plan.TransactionTime + "|" + plan.CostCenterNumber + "|" + RetrieveGLCodeByStatus(1, false, false, plan.IsMealPlan, plan.IsVendor, plan.IsDonation, plan.isDownload, plan.productId, plan.OrderNumber, true, false) + "|" + "-" + "|" + plan.DeliveryPickUpFee.ToString("N2").Replace(",", ""));
                                //m++;
                                //trailerCount++;

                                sb.AppendLine(plan.RecordIndicator + "|" + plan.OrderNumber + "|" + plan.Status + "|" + lineItemCount.ToString().PadLeft(3, '0') + "|" + plan.TransactionDate.ToString("MMddyyyy") + "|" + plan.TransactionTime + "|" + plan.CostCenterNumber + "|" + RetrieveGLCodeByStatus(2, true, false, plan.IsMealPlan, plan.IsVendor, plan.IsDonation, plan.isDownload, plan.productId, plan.OrderNumber, true, false, plan.OrderItemId) + "|" + "-" + "|" + plan.DeliveryPickUpFee.ToString("N2").Replace(",", "") + "|" + plan.EcommLocalProductDescription + "-Delivery"); //Delivery line
                                m++;
                                trailerCount++;
                            }
                        }
                        if (plan.IsShipping)
                        {
                            if (plan.Shippingfee != 0)
                            {
                                //sb.AppendLine(plan.RecordIndicator + "|" + plan.OrderNumber + "|" + plan.Status + "|" + m.ToString().PadLeft(3, '0') + "|" + plan.TransactionDate.ToString("MMddyyyy") + "|" + plan.TransactionTime + "|" + plan.CostCenterNumber + "|" + RetrieveGLCodeByStatus(1, false, false, plan.IsMealPlan, plan.IsVendor, plan.IsDonation, plan.isDownload, plan.productId, plan.OrderNumber, false, true) + "|" + "-" + "|" + plan.Shippingfee.ToString("N2").Replace(",", ""));
                                //m++;
                                //trailerCount++;

                                sb.AppendLine(plan.RecordIndicator + "|" + plan.OrderNumber + "|" + plan.Status + "|" + lineItemCount.ToString().PadLeft(3, '0') + "|" + plan.TransactionDate.ToString("MMddyyyy") + "|" + plan.TransactionTime + "|" + plan.CostCenterNumber + "|" + RetrieveGLCodeByStatus(2, true, false, plan.IsMealPlan, plan.IsVendor, plan.IsDonation, plan.isDownload, plan.productId, plan.OrderNumber, false, true, plan.OrderItemId) + "|" + "-" + "|" + plan.Shippingfee.ToString("N2").Replace(",", "") + "|" + plan.EcommLocalProductDescription + "-Shipping"); //Shippping line
                                m++;
                                trailerCount++;
                            }
                        }
                        if (plan.IsMealPlan || plan.IsDonation || plan.IsDonation)
                        {
                            if (listProducts.Contains(plan.productId))
                            {
                                vertexOrderTaxGls = obj.GetVertexGlBreakdown(plan.OrderNumber, plan.productId, 3, orderitemid);
                            }
                            else
                            {
                                vertexOrderTaxGls = obj.GetVertexGlBreakdown(plan.OrderNumber, plan.productId, 3);
                            }
                            if (vertexOrderTaxGls.Any())
                            {
                                foreach (var taxGls in vertexOrderTaxGls)
                                {
                                    if (taxGls.Total != 0)
                                    {
                                        sb.AppendLine(plan.RecordIndicator + "|" + plan.OrderNumber + "|" + (plan.IsMealPlan ? "1" : plan.Status) + "|" + lineItemCount.ToString().PadLeft(3, '0') + "|" + plan.TransactionDate.ToString("MMddyyyy") + "|" + plan.TransactionTime + "|" + plan.CostCenterNumber + "|" + taxGls.GlCode + "|" + "-" + "|" + taxGls.Total.ToString("N2").Replace(",", ""));
                                        m++;
                                        trailerCount++;
                                    }
                                }
                            }
                        }

                    }
                    else if (plan.Status == "3")
                    {
                        if (string.IsNullOrEmpty(plan.FulfillmentDateTime))
                        {
                            if (plan.ProductAmount != 0)
                            {
                                //Credit to the 2004 account with negative sign
                                sb.AppendLine(plan.RecordIndicator + "|" + plan.OrderNumber + "|" + 3 + "|" + lineItemCount.ToString().PadLeft(3, '0') + "|" + plan.TransactionDate.ToString("MMddyyyy") + "|" + plan.TransactionTime + "|" + plan.CostCenterNumber + "|" + RetrieveGLCodeByStatus(0, true, false, plan.IsMealPlan, plan.IsVendor, plan.IsDonation, plan.isDownload, plan.productId, plan.OrderNumber, true, false, plan.OrderItemId) + "|" + "-" + "|" + plan.OrderAmount.ToString("N2").Replace(",", ""));
                                m++;
                                trailerCount++;

                                sb.AppendLine(plan.RecordIndicator + "|" + plan.OrderNumber + "|" + 3 + "|" + lineItemCount.ToString().PadLeft(3, '0') + "|" + plan.TransactionDate.ToString("MMddyyyy") + "|" + plan.TransactionTime + "|" + plan.CostCenterNumber + "|" + RetrieveGLCodeByStatus(1, true, false, plan.IsMealPlan, plan.IsVendor, plan.IsDonation, plan.isDownload, plan.productId, plan.OrderNumber, true, false, plan.OrderItemId) + "|" + string.Empty + "|" + plan.ProductAmount.ToString("N2").Replace(",", "") + "|" + plan.EcommLocalProductDescription);
                                m++;
                                trailerCount++;
                            }
                            if (plan.DeliveryPickUpFee != 0)
                            {
                                if (plan.IsDelivery)
                                {
                                    sb.AppendLine(plan.RecordIndicator + "|" + plan.OrderNumber + "|" + 3 + "|" + lineItemCount.ToString().PadLeft(3, '0') + "|" + plan.TransactionDate.ToString("MMddyyyy") + "|" + plan.TransactionTime + "|" + plan.CostCenterNumber + "|" + RetrieveGLCodeByStatus(1, false, false, plan.IsMealPlan, plan.IsVendor, plan.IsDonation, plan.isDownload, plan.productId, plan.OrderNumber, true, false, plan.OrderItemId) + "|" + string.Empty + "|" + plan.DeliveryPickUpFee.ToString("N2").Replace(",", "") + "|" + plan.EcommLocalProductDescription + "-Delivery");
                                    m++;
                                    trailerCount++;
                                }
                            }
                            if (plan.Shippingfee != 0)
                            {
                                if (plan.IsShipping)
                                {
                                    sb.AppendLine(plan.RecordIndicator + "|" + plan.OrderNumber + "|" + 3 + "|" + lineItemCount.ToString().PadLeft(3, '0') + "|" + plan.TransactionDate.ToString("MMddyyyy") + "|" + plan.TransactionTime + "|" + plan.CostCenterNumber + "|" + RetrieveGLCodeByStatus(1, false, false, plan.IsMealPlan, plan.IsVendor, plan.IsDonation, plan.isDownload, plan.productId, plan.OrderNumber, false, true, plan.OrderItemId) + "|" + string.Empty + "|" + plan.Shippingfee.ToString("N2").Replace(",", "") + "|" + plan.EcommLocalProductDescription + "-Shipping");
                                    m++;
                                    trailerCount++;
                                }
                            }
                        }
                        else
                        {
                            if (plan.IsMealPlan || plan.IsDonation || plan.isDownload)
                            {
                                string datapocess = RetrieveGLCodeByStatus(2, false, false, plan.IsMealPlan, plan.IsVendor, plan.IsDonation, plan.isDownload, plan.productId, plan.OrderNumber, false, false, plan.OrderItemId);
                                if (datapocess.Contains('|'))
                                {
                                    string[] glsandAmount = datapocess.Split('|');
                                    var q = glsandAmount.Zip(glsandAmount.Skip(1), (Key, Value) => new { Key, Value })
                                                 .Where((pair, index) => index % 2 == 0)
                                                 .ToDictionary(pair => pair.Key, pair => pair.Value);
                                    if (plan.OrderAmount != decimal.Zero)
                                    {
                                        //Credit to the 2004 account with negative sign
                                        sb.AppendLine(plan.RecordIndicator + "|" + plan.OrderNumber + "|" + 3 + "|" + lineItemCount.ToString().PadLeft(3, '0') + "|" + plan.TransactionDate.ToString("MMddyyyy") + "|" + plan.TransactionTime + "|" + plan.CostCenterNumber + "|" + RetrieveGLCodeByStatus(0, true, false, plan.IsMealPlan, plan.IsVendor, plan.IsDonation, plan.isDownload, plan.productId, plan.OrderNumber, true, false, plan.OrderItemId) + "|" + "-" + "|" + plan.OrderAmount.ToString("N2").Replace(",", ""));
                                        m++;
                                        trailerCount++;
                                        string amount = string.Empty;
                                        foreach (var item in q)
                                        {
                                            if (item.Key.Split('_')[0] == "64701000")
                                            {

                                                amount = item.Value.Replace("-", "");
                                                amount = Convert.ToString(Convert.ToDecimal(amount) * plan.Quantity);
                                                amount = Convert.ToString(Math.Round(Convert.ToDecimal(amount), 2));
                                                plan.AmountSign = "-";
                                            }
                                            else
                                            {
                                                amount = item.Value;
                                                amount = Convert.ToString(Convert.ToDecimal(amount) * plan.Quantity);
                                                amount = Convert.ToString(Math.Round(Convert.ToDecimal(amount), 2));
                                                plan.AmountSign = string.Empty;
                                            }
                                            sb.AppendLine(plan.RecordIndicator + "|" + plan.OrderNumber + "|" + 3 + "|" + lineItemCount.ToString().PadLeft(3, '0') + "|" + plan.TransactionDate.ToString("MMddyyyy") + "|" + plan.TransactionTime + "|" + plan.CostCenterNumber + "|" + item.Key.Split('_')[0] + "|" + plan.AmountSign + "|" + amount.Replace(",", "") + "|" + plan.EcommLocalProductDescription);
                                            m++;
                                            trailerCount++;
                                            mealplanCounter++;
                                        }
                                    }
                                }
                                else
                                {
                                    if (plan.ProductAmount != 0)
                                    {
                                        //Credit to the 2004 account with negative sign
                                        sb.AppendLine(plan.RecordIndicator + "|" + plan.OrderNumber + "|" + 3 + "|" + lineItemCount.ToString().PadLeft(3, '0') + "|" + plan.TransactionDate.ToString("MMddyyyy") + "|" + plan.TransactionTime + "|" + plan.CostCenterNumber + "|" + RetrieveGLCodeByStatus(0, true, false, plan.IsMealPlan, plan.IsVendor, plan.IsDonation, plan.isDownload, plan.productId, plan.OrderNumber, true, false, plan.OrderItemId) + "|" + "-" + "|" + plan.OrderAmount.ToString("N2").Replace(",", ""));
                                        m++;
                                        trailerCount++;

                                        sb.AppendLine(plan.RecordIndicator + "|" + plan.OrderNumber + "|" + 3 + "|" + lineItemCount.ToString().PadLeft(3, '0') + "|" + plan.TransactionDate.ToString("MMddyyyy") + "|" + plan.TransactionTime + "|" + plan.CostCenterNumber + "|" + RetrieveGLCodeByStatus(2, false, false, plan.IsMealPlan, plan.IsVendor, plan.IsDonation, plan.isDownload, plan.productId, plan.OrderNumber, false, false, plan.OrderItemId) + "|" + "" + "|" + plan.ProductAmount.ToString("N2").Replace(",", "") + "|" + plan.EcommLocalProductDescription);
                                        m++;
                                        trailerCount++;
                                    }
                                }
                            }
                            if (!plan.IsMealPlan && !plan.IsDonation && !plan.isDownload)
                            {
                                if (plan.ProductAmount != 0)
                                {

                                    //Credit to the 2004 account with negative sign
                                    sb.AppendLine(plan.RecordIndicator + "|" + plan.OrderNumber + "|" + 3 + "|" + lineItemCount.ToString().PadLeft(3, '0') + "|" + plan.TransactionDate.ToString("MMddyyyy") + "|" + plan.TransactionTime + "|" + plan.CostCenterNumber + "|" + RetrieveGLCodeByStatus(0, true, false, plan.IsMealPlan, plan.IsVendor, plan.IsDonation, plan.isDownload, plan.productId, plan.OrderNumber, true, false, plan.OrderItemId) + "|" + "-" + "|" + plan.OrderAmount.ToString("N2").Replace(",", ""));
                                    m++;
                                    trailerCount++;

                                    sb.AppendLine(plan.RecordIndicator + "|" + plan.OrderNumber + "|" + 3 + "|" + lineItemCount.ToString().PadLeft(3, '0') + "|" + plan.TransactionDate.ToString("MMddyyyy") + "|" + plan.TransactionTime + "|" + plan.CostCenterNumber + "|" + RetrieveGLCodeByStatus(2, false, false, plan.IsMealPlan, plan.IsVendor, plan.IsDonation, plan.isDownload, plan.productId, plan.OrderNumber, false, false, plan.OrderItemId) + "|" + string.Empty + "|" + (plan.ProductAmount).ToString("N2").Replace(",", "") + "|" + plan.EcommLocalProductDescription);
                                    m++;
                                    trailerCount++;
                                }
                            }
                            if (plan.IsDelivery)
                            {
                                if (plan.DeliveryPickUpFee != 0)
                                {
                                    //sb.AppendLine(plan.RecordIndicator + "|" + plan.OrderNumber + "|" + plan.Status + "|" + m.ToString().PadLeft(3, '0') + "|" + plan.TransactionDate.ToString("MMddyyyy") + "|" + plan.TransactionTime + "|" + plan.CostCenterNumber + "|" + RetrieveGLCodeByStatus(1, false, false, plan.IsMealPlan, plan.IsVendor, plan.IsDonation, plan.isDownload, plan.productId, plan.OrderNumber, true, false) + "|" + "-" + "|" + plan.DeliveryPickUpFee.ToString("N2").Replace(",", ""));
                                    //m++;
                                    //trailerCount++;

                                    sb.AppendLine(plan.RecordIndicator + "|" + plan.OrderNumber + "|" + 3 + "|" + lineItemCount.ToString().PadLeft(3, '0') + "|" + plan.TransactionDate.ToString("MMddyyyy") + "|" + plan.TransactionTime + "|" + plan.CostCenterNumber + "|" + RetrieveGLCodeByStatus(2, true, false, plan.IsMealPlan, plan.IsVendor, plan.IsDonation, plan.isDownload, plan.productId, plan.OrderNumber, true, false, plan.OrderItemId) + "|" + string.Empty + "|" + plan.DeliveryPickUpFee.ToString("N2").Replace(",", "") + "|" + plan.EcommLocalProductDescription + "-Delivery"); //Delivery line
                                    m++;
                                    trailerCount++;
                                }
                            }
                            if (plan.IsShipping)
                            {
                                if (plan.Shippingfee != 0)
                                {
                                    //sb.AppendLine(plan.RecordIndicator + "|" + plan.OrderNumber + "|" + plan.Status + "|" + m.ToString().PadLeft(3, '0') + "|" + plan.TransactionDate.ToString("MMddyyyy") + "|" + plan.TransactionTime + "|" + plan.CostCenterNumber + "|" + RetrieveGLCodeByStatus(1, false, false, plan.IsMealPlan, plan.IsVendor, plan.IsDonation, plan.isDownload, plan.productId, plan.OrderNumber, false, true) + "|" + "-" + "|" + plan.Shippingfee.ToString("N2").Replace(",", ""));
                                    //m++;
                                    //trailerCount++;

                                    sb.AppendLine(plan.RecordIndicator + "|" + plan.OrderNumber + "|" + 3 + "|" + lineItemCount.ToString().PadLeft(3, '0') + "|" + plan.TransactionDate.ToString("MMddyyyy") + "|" + plan.TransactionTime + "|" + plan.CostCenterNumber + "|" + RetrieveGLCodeByStatus(2, true, false, plan.IsMealPlan, plan.IsVendor, plan.IsDonation, plan.isDownload, plan.productId, plan.OrderNumber, false, true, plan.OrderItemId) + "|" + string.Empty + "|" + plan.Shippingfee.ToString("N2").Replace(",", "") + "|" + plan.EcommLocalProductDescription + "-Shipping"); //Shippping line
                                    m++;
                                    trailerCount++;

                                }
                            }
                        }
                        if (listProducts.Contains(plan.productId))
                        {
                            vertexOrderTaxGls = obj.GetVertexGlBreakdown(plan.OrderNumber, plan.productId, 9, orderitemid);
                        }
                        else
                        {
                            vertexOrderTaxGls = obj.GetVertexGlBreakdown(plan.OrderNumber, plan.productId, 9);
                        };// Getting the Refund Resposne
                        if (!vertexOrderTaxGls.Any())
                        {
                            if (listProducts.Contains(plan.productId))
                            {
                                vertexOrderTaxGls = obj.GetVertexGlBreakdown(plan.OrderNumber, plan.productId, 3, orderitemid);
                            }
                            else
                            {
                                vertexOrderTaxGls = obj.GetVertexGlBreakdown(plan.OrderNumber, plan.productId, 3);
                            }
                        }
                        if (vertexOrderTaxGls.Any())
                        {
                            decimal tax = decimal.Zero;
                            foreach (var taxGls in vertexOrderTaxGls)
                            {
                                if (taxGls.Total != 0)
                                {

                                    if (taxGls.Total < 0)
                                        tax = taxGls.Total * (-1);
                                    else
                                        tax = taxGls.Total;
                                    sb.AppendLine(plan.RecordIndicator + "|" + plan.OrderNumber + "|" + 3 + "|" + lineItemCount.ToString().PadLeft(3, '0') + "|" + plan.TransactionDate.ToString("MMddyyyy") + "|" + plan.TransactionTime + "|" + plan.CostCenterNumber + "|" + taxGls.GlCode + "|" + string.Empty + "|" + tax.ToString("N2").Replace(",", ""));
                                    m++;
                                    trailerCount++;
                                }
                            }
                        }
                    }
                    else if (plan.Status == "4")
                    {
                        sb.AppendLine(plan.RecordIndicator + "|" + plan.OrderNumber + "|" + 3 + "|" + lineItemCount.ToString().PadLeft(3, '0') + "|" + plan.TransactionDate.ToString("MMddyyyy") + "|" + plan.TransactionTime + "|" + plan.CostCenterNumber + "|" + "41102004" + "|" + plan.AmountSign + "|" + plan.OrderAmount.ToString("N2").Replace(",", ""));
                        m++;
                        trailerCount++;

                        var refundedDataDetails = RetrievePartialRefundGls(plan.OrderItemId);

                        if (plan.FulfillmentDateTime != null)
                        {
                            foreach (var itemsinDictionary in refundedDataDetails)
                            {
                                var amount = Convert.ToString(Math.Round(Convert.ToDecimal(itemsinDictionary.Value), 2));
                                sb.AppendLine(plan.RecordIndicator + "|" + plan.OrderNumber + "|" + 3 + "|" + lineItemCount.ToString().PadLeft(3, '0') + "|" + plan.TransactionDate.ToString("MMddyyyy") + "|" + plan.TransactionTime + "|" + plan.CostCenterNumber + "|" + (itemsinDictionary.Key.Contains("_") ? itemsinDictionary.Key.Split('_')[0] : itemsinDictionary.Key) + "|" + string.Empty + "|" + amount.Replace(",", ""));
                                m++;
                                trailerCount++;
                                partialRefundCounter++;
                            }
                        }
                        else
                        {
                            foreach (var itemsinDictionary in refundedDataDetails)
                            {
                                var amount = Convert.ToString(Math.Round(Convert.ToDecimal(itemsinDictionary.Value), 2));
                                sb.AppendLine(plan.RecordIndicator + "|" + plan.OrderNumber + "|" + 3 + "|" + lineItemCount.ToString().PadLeft(3, '0') + "|" + plan.TransactionDate.ToString("MMddyyyy") + "|" + plan.TransactionTime + "|" + plan.CostCenterNumber + "|" + (itemsinDictionary.Key.Contains("_") ? itemsinDictionary.Key.Split('_')[0] : itemsinDictionary.Key) + "|" + string.Empty + "|" + amount.Replace(",", ""));
                                m++;
                                trailerCount++;
                                partialRefundCounter++;
                            }
                        }

                    }
                    else
                        sb.AppendLine(plan.RecordIndicator + "|" + plan.OrderNumber + "|" + plan.Status + "|" + m.ToString().PadLeft(3, '0') + "|" + plan.TransactionDate.ToString("MMddyyyy") + "|" + plan.TransactionTime + "|" + plan.CostCenterNumber + "|" + RetrieveGLCodeByStatus(1, true) + "|" + plan.AmountSign + "|" + plan.ProductAmount.ToString("N2").Replace(".", ""));


                    trailerCount++;

                    if (plan.Status == "1")
                    {
                        var recordStatus2 = entries.Where(x => x.Status == "2").ToList();
                        if (recordStatus2.FindIndex(x => x.OrderNumber == plan.OrderNumber) < 0)
                            trailerTotal = trailerTotal + Convert.ToDecimal(plan.ProductAmount.ToString("N2"));
                    }

                    if (plan.Status == "2" || plan.Status == "3")
                    {
                        trailerTotal = trailerTotal + Convert.ToDecimal(plan.ProductAmount.ToString("N2"));
                        UpdateSAPJournal(plan.OrderNumber);
                    }
                }

                foreach (SAPSalesJournalEntry plan in RecordonStatus)
                {
                    //Sales Journal Record
                    if (plan.OrderNumber == flag1)
                    {
                        n++;
                    }
                    else
                    {
                        n = 1;
                        flag1 = plan.OrderNumber;
                    }

                    if (plan.Status == "1")
                    {
                        if (plan.ProductAmount != 0)
                        {
                            sb.AppendLine("S" + "|" + plan.OrderNumber + "|" + plan.Status + "|" + lineItemCount.ToString().PadLeft(3, '0') + "|" + plan.CustomerNumber + "|" + plan.CustomerName + "|" + plan.CustomerAddress + "|" + plan.CustomerCity + "|" + plan.CustomerState + "|" + plan.CustomerPostalCode + "|" + plan.CustomerCountry + "|" + plan.CustomerJurisdictionCode + "|" + plan.ShipToName + "|" + plan.ShipToStreetAddress + "|" + plan.ShipToCity + "|" + plan.ShipToState + "|" + plan.ShipToPostalCode + "|" + plan.ShipToCountry + "|" + plan.ShipToJurisdictionCode + "|" + plan.ShipFromName + "|" + plan.ShipFromAddress + "|" + plan.ShipFromCity + "|" + plan.ShipFromState + "|" + plan.ShipFromPostalCode + "|" + plan.ShipFromCountry + "|" + plan.ShipFromJurisdictionCode + "|" + plan.EcommMasterProductDescription + "|" + plan.EcommLocalProductDescription);
                            trailerCount++;
                            n++;
                            sb.AppendLine("S" + "|" + plan.OrderNumber + "|" + plan.Status + "|" + lineItemCount.ToString().PadLeft(3, '0') + "|" + plan.CustomerNumber + "|" + plan.CustomerName + "|" + plan.CustomerAddress + "|" + plan.CustomerCity + "|" + plan.CustomerState + "|" + plan.CustomerPostalCode + "|" + plan.CustomerCountry + "|" + plan.CustomerJurisdictionCode + "|" + plan.ShipToName + "|" + plan.ShipToStreetAddress + "|" + plan.ShipToCity + "|" + plan.ShipToState + "|" + plan.ShipToPostalCode + "|" + plan.ShipToCountry + "|" + plan.ShipToJurisdictionCode + "|" + plan.ShipFromName + "|" + plan.ShipFromAddress + "|" + plan.ShipFromCity + "|" + plan.ShipFromState + "|" + plan.ShipFromPostalCode + "|" + plan.ShipFromCountry + "|" + plan.ShipFromJurisdictionCode + "|" + plan.EcommMasterProductDescription + "|" + plan.EcommLocalProductDescription);
                            trailerCount++;
                            n++;
                        }
                        if (plan.DeliveryPickUpFee != 0)
                        {
                            if (plan.IsDelivery)
                            {
                                //sb.AppendLine("S" + "|" + plan.OrderNumber + "|" + plan.Status + "|" + n.ToString().PadLeft(3, '0') + "|" + plan.CustomerNumber + "|" + plan.CustomerName + "|" + plan.CustomerAddress + "|" + plan.CustomerCity + "|" + plan.CustomerState + "|" + plan.CustomerPostalCode + "|" + plan.CustomerCountry + "|" + plan.CustomerJurisdictionCode + "|" + plan.ShipToName + "|" + plan.ShipToStreetAddress + "|" + plan.ShipToCity + "|" + plan.ShipToState + "|" + plan.ShipToPostalCode + "|" + plan.ShipToCountry + "|" + plan.ShipToJurisdictionCode + "|" + plan.ShipFromName + "|" + plan.ShipFromAddress + "|" + plan.ShipFromCity + "|" + plan.ShipFromState + "|" + plan.ShipFromPostalCode + "|" + plan.ShipFromCountry + "|" + plan.ShipFromJurisdictionCode + "|" + plan.EcommMasterProductDescription + "|" + plan.EcommLocalProductDescription);
                                //trailerCount++;
                                //n++;
                                sb.AppendLine("S" + "|" + plan.OrderNumber + "|" + plan.Status + "|" + lineItemCount.ToString().PadLeft(3, '0') + "|" + plan.CustomerNumber + "|" + plan.CustomerName + "|" + plan.CustomerAddress + "|" + plan.CustomerCity + "|" + plan.CustomerState + "|" + plan.CustomerPostalCode + "|" + plan.CustomerCountry + "|" + plan.CustomerJurisdictionCode + "|" + plan.ShipToName + "|" + plan.ShipToStreetAddress + "|" + plan.ShipToCity + "|" + plan.ShipToState + "|" + plan.ShipToPostalCode + "|" + plan.ShipToCountry + "|" + plan.ShipToJurisdictionCode + "|" + plan.ShipFromName + "|" + plan.ShipFromAddress + "|" + plan.ShipFromCity + "|" + plan.ShipFromState + "|" + plan.ShipFromPostalCode + "|" + plan.ShipFromCountry + "|" + plan.ShipFromJurisdictionCode + "|" + plan.EcommMasterProductDescription + "|" + plan.EcommLocalProductDescription);
                                trailerCount++;
                                n++;
                            }
                        }
                        if (plan.Shippingfee != 0)
                        {
                            if (plan.IsShipping)
                            {
                                //sb.AppendLine("S" + "|" + plan.OrderNumber + "|" + plan.Status + "|" + n.ToString().PadLeft(3, '0') + "|" + plan.CustomerNumber + "|" + plan.CustomerName + "|" + plan.CustomerAddress + "|" + plan.CustomerCity + "|" + plan.CustomerState + "|" + plan.CustomerPostalCode + "|" + plan.CustomerCountry + "|" + plan.CustomerJurisdictionCode + "|" + plan.ShipToName + "|" + plan.ShipToStreetAddress + "|" + plan.ShipToCity + "|" + plan.ShipToState + "|" + plan.ShipToPostalCode + "|" + plan.ShipToCountry + "|" + plan.ShipToJurisdictionCode + "|" + plan.ShipFromName + "|" + plan.ShipFromAddress + "|" + plan.ShipFromCity + "|" + plan.ShipFromState + "|" + plan.ShipFromPostalCode + "|" + plan.ShipFromCountry + "|" + plan.ShipFromJurisdictionCode + "|" + plan.EcommMasterProductDescription + "|" + plan.EcommLocalProductDescription);
                                //trailerCount++;
                                //n++;
                                sb.AppendLine("S" + "|" + plan.OrderNumber + "|" + plan.Status + "|" + lineItemCount.ToString().PadLeft(3, '0') + "|" + plan.CustomerNumber + "|" + plan.CustomerName + "|" + plan.CustomerAddress + "|" + plan.CustomerCity + "|" + plan.CustomerState + "|" + plan.CustomerPostalCode + "|" + plan.CustomerCountry + "|" + plan.CustomerJurisdictionCode + "|" + plan.ShipToName + "|" + plan.ShipToStreetAddress + "|" + plan.ShipToCity + "|" + plan.ShipToState + "|" + plan.ShipToPostalCode + "|" + plan.ShipToCountry + "|" + plan.ShipToJurisdictionCode + "|" + plan.ShipFromName + "|" + plan.ShipFromAddress + "|" + plan.ShipFromCity + "|" + plan.ShipFromState + "|" + plan.ShipFromPostalCode + "|" + plan.ShipFromCountry + "|" + plan.ShipFromJurisdictionCode + "|" + plan.EcommMasterProductDescription + "|" + plan.EcommLocalProductDescription);
                                trailerCount++;
                                n++;
                            }
                        }
                        if (vertexOrderTaxGls.Any())
                        {
                            foreach (var taxGls in vertexOrderTaxGls)
                            {
                                if (taxGls.Total != 0)
                                {
                                    sb.AppendLine("S" + "|" + plan.OrderNumber + "|" + plan.Status + "|" + lineItemCount.ToString().PadLeft(3, '0') + "|" + plan.CustomerNumber + "|" + plan.CustomerName + "|" + plan.CustomerAddress + "|" + plan.CustomerCity + "|" + plan.CustomerState + "|" + plan.CustomerPostalCode + "|" + plan.CustomerCountry + "|" + plan.CustomerJurisdictionCode + "|" + plan.ShipToName + "|" + plan.ShipToStreetAddress + "|" + plan.ShipToCity + "|" + plan.ShipToState + "|" + plan.ShipToPostalCode + "|" + plan.ShipToCountry + "|" + plan.ShipToJurisdictionCode + "|" + plan.ShipFromName + "|" + plan.ShipFromAddress + "|" + plan.ShipFromCity + "|" + plan.ShipFromState + "|" + plan.ShipFromPostalCode + "|" + plan.ShipFromCountry + "|" + plan.ShipFromJurisdictionCode + "|" + plan.EcommMasterProductDescription + "|" + plan.EcommLocalProductDescription);
                                    trailerCount++;
                                    n++;
                                }
                            }
                        }

                    }
                    else if (plan.Status == "2")
                    {
                        if (plan.IsMealPlan || plan.IsDonation || plan.isDownload)
                        {
                            if (mealplanCounter != 0)
                            {
                                sb.AppendLine("S" + "|" + plan.OrderNumber + "|" + 1 + "|" + lineItemCount.ToString().PadLeft(3, '0') + "|" + plan.CustomerNumber + "|" + plan.CustomerName + "|" + plan.CustomerAddress + "|" + plan.CustomerCity + "|" + plan.CustomerState + "|" + plan.CustomerPostalCode + "|" + plan.CustomerCountry + "|" + plan.CustomerJurisdictionCode + "|" + plan.ShipToName + "|" + plan.ShipToStreetAddress + "|" + plan.ShipToCity + "|" + plan.ShipToState + "|" + plan.ShipToPostalCode + "|" + plan.ShipToCountry + "|" + plan.ShipToJurisdictionCode + "|" + plan.ShipFromName + "|" + plan.ShipFromAddress + "|" + plan.ShipFromCity + "|" + plan.ShipFromState + "|" + plan.ShipFromPostalCode + "|" + plan.ShipFromCountry + "|" + plan.ShipFromJurisdictionCode + "|" + plan.EcommMasterProductDescription + "|" + plan.EcommLocalProductDescription);
                                trailerCount++;
                                n++;
                                for (int i = 0; i < mealplanCounter; i++)
                                {
                                    sb.AppendLine("S" + "|" + plan.OrderNumber + "|" + 1 + "|" + lineItemCount.ToString().PadLeft(3, '0') + "|" + plan.CustomerNumber + "|" + plan.CustomerName + "|" + plan.CustomerAddress + "|" + plan.CustomerCity + "|" + plan.CustomerState + "|" + plan.CustomerPostalCode + "|" + plan.CustomerCountry + "|" + plan.CustomerJurisdictionCode + "|" + plan.ShipToName + "|" + plan.ShipToStreetAddress + "|" + plan.ShipToCity + "|" + plan.ShipToState + "|" + plan.ShipToPostalCode + "|" + plan.ShipToCountry + "|" + plan.ShipToJurisdictionCode + "|" + plan.ShipFromName + "|" + plan.ShipFromAddress + "|" + plan.ShipFromCity + "|" + plan.ShipFromState + "|" + plan.ShipFromPostalCode + "|" + plan.ShipFromCountry + "|" + plan.ShipFromJurisdictionCode + "|" + plan.EcommMasterProductDescription + "|" + plan.EcommLocalProductDescription);
                                    trailerCount++;
                                    n++;
                                }
                            }
                            else
                            {
                                if (plan.ProductAmount != 0)
                                {
                                    sb.AppendLine("S" + "|" + plan.OrderNumber + "|" + 1 + "|" + lineItemCount.ToString().PadLeft(3, '0') + "|" + plan.CustomerNumber + "|" + plan.CustomerName + "|" + plan.CustomerAddress + "|" + plan.CustomerCity + "|" + plan.CustomerState + "|" + plan.CustomerPostalCode + "|" + plan.CustomerCountry + "|" + plan.CustomerJurisdictionCode + "|" + plan.ShipToName + "|" + plan.ShipToStreetAddress + "|" + plan.ShipToCity + "|" + plan.ShipToState + "|" + plan.ShipToPostalCode + "|" + plan.ShipToCountry + "|" + plan.ShipToJurisdictionCode + "|" + plan.ShipFromName + "|" + plan.ShipFromAddress + "|" + plan.ShipFromCity + "|" + plan.ShipFromState + "|" + plan.ShipFromPostalCode + "|" + plan.ShipFromCountry + "|" + plan.ShipFromJurisdictionCode + "|" + plan.EcommMasterProductDescription + "|" + plan.EcommLocalProductDescription);
                                    trailerCount++;
                                    n++;

                                    sb.AppendLine("S" + "|" + plan.OrderNumber + "|" + 1 + "|" + lineItemCount.ToString().PadLeft(3, '0') + "|" + plan.CustomerNumber + "|" + plan.CustomerName + "|" + plan.CustomerAddress + "|" + plan.CustomerCity + "|" + plan.CustomerState + "|" + plan.CustomerPostalCode + "|" + plan.CustomerCountry + "|" + plan.CustomerJurisdictionCode + "|" + plan.ShipToName + "|" + plan.ShipToStreetAddress + "|" + plan.ShipToCity + "|" + plan.ShipToState + "|" + plan.ShipToPostalCode + "|" + plan.ShipToCountry + "|" + plan.ShipToJurisdictionCode + "|" + plan.ShipFromName + "|" + plan.ShipFromAddress + "|" + plan.ShipFromCity + "|" + plan.ShipFromState + "|" + plan.ShipFromPostalCode + "|" + plan.ShipFromCountry + "|" + plan.ShipFromJurisdictionCode + "|" + plan.EcommMasterProductDescription + "|" + plan.EcommLocalProductDescription);
                                    trailerCount++;
                                    n++;
                                }
                            }
                        }
                        if (!plan.IsMealPlan && !plan.isDownload && !plan.IsDonation)
                        {
                            if (plan.ProductAmount != 0)
                            {
                                if (plan.IsVendor)
                                {
                                    sb.AppendLine("S" + "|" + plan.OrderNumber + "|" + plan.Status + "|" + lineItemCount.ToString().PadLeft(3, '0') + "|" + plan.CustomerNumber + "|" + plan.CustomerName + "|" + plan.CustomerAddress + "|" + plan.CustomerCity + "|" + plan.CustomerState + "|" + plan.CustomerPostalCode + "|" + plan.CustomerCountry + "|" + plan.CustomerJurisdictionCode + "|" + plan.ShipToName + "|" + plan.ShipToStreetAddress + "|" + plan.ShipToCity + "|" + plan.ShipToState + "|" + plan.ShipToPostalCode + "|" + plan.ShipToCountry + "|" + plan.ShipToJurisdictionCode + "|" + plan.ShipFromName + "|" + plan.ShipFromAddress + "|" + plan.ShipFromCity + "|" + plan.ShipFromState + "|" + plan.ShipFromPostalCode + "|" + plan.ShipFromCountry + "|" + plan.ShipFromJurisdictionCode + "|" + plan.EcommMasterProductDescription + "|" + plan.EcommLocalProductDescription);
                                    trailerCount++;
                                    n++;

                                    sb.AppendLine("S" + "|" + plan.OrderNumber + "|" + plan.Status + "|" + lineItemCount.ToString().PadLeft(3, '0') + "|" + plan.CustomerNumber + "|" + plan.CustomerName + "|" + plan.CustomerAddress + "|" + plan.CustomerCity + "|" + plan.CustomerState + "|" + plan.CustomerPostalCode + "|" + plan.CustomerCountry + "|" + plan.CustomerJurisdictionCode + "|" + plan.ShipToName + "|" + plan.ShipToStreetAddress + "|" + plan.ShipToCity + "|" + plan.ShipToState + "|" + plan.ShipToPostalCode + "|" + plan.ShipToCountry + "|" + plan.ShipToJurisdictionCode + "|" + plan.ShipFromName + "|" + plan.ShipFromAddress + "|" + plan.ShipFromCity + "|" + plan.ShipFromState + "|" + plan.ShipFromPostalCode + "|" + plan.ShipFromCountry + "|" + plan.ShipFromJurisdictionCode + "|" + plan.EcommMasterProductDescription + "|" + plan.EcommLocalProductDescription);
                                    trailerCount++;
                                    n++;
                                }
                                else
                                {
                                    sb.AppendLine("S" + "|" + plan.OrderNumber + "|" + plan.Status + "|" + lineItemCount.ToString().PadLeft(3, '0') + "|" + plan.CustomerNumber + "|" + plan.CustomerName + "|" + plan.CustomerAddress + "|" + plan.CustomerCity + "|" + plan.CustomerState + "|" + plan.CustomerPostalCode + "|" + plan.CustomerCountry + "|" + plan.CustomerJurisdictionCode + "|" + plan.ShipToName + "|" + plan.ShipToStreetAddress + "|" + plan.ShipToCity + "|" + plan.ShipToState + "|" + plan.ShipToPostalCode + "|" + plan.ShipToCountry + "|" + plan.ShipToJurisdictionCode + "|" + plan.ShipFromName + "|" + plan.ShipFromAddress + "|" + plan.ShipFromCity + "|" + plan.ShipFromState + "|" + plan.ShipFromPostalCode + "|" + plan.ShipFromCountry + "|" + plan.ShipFromJurisdictionCode + "|" + plan.EcommMasterProductDescription + "|" + plan.EcommLocalProductDescription);
                                    m++;
                                    trailerCount++;
                                }

                                sb.AppendLine("S" + "|" + plan.OrderNumber + "|" + plan.Status + "|" + lineItemCount.ToString().PadLeft(3, '0') + "|" + plan.CustomerNumber + "|" + plan.CustomerName + "|" + plan.CustomerAddress + "|" + plan.CustomerCity + "|" + plan.CustomerState + "|" + plan.CustomerPostalCode + "|" + plan.CustomerCountry + "|" + plan.CustomerJurisdictionCode + "|" + plan.ShipToName + "|" + plan.ShipToStreetAddress + "|" + plan.ShipToCity + "|" + plan.ShipToState + "|" + plan.ShipToPostalCode + "|" + plan.ShipToCountry + "|" + plan.ShipToJurisdictionCode + "|" + plan.ShipFromName + "|" + plan.ShipFromAddress + "|" + plan.ShipFromCity + "|" + plan.ShipFromState + "|" + plan.ShipFromPostalCode + "|" + plan.ShipFromCountry + "|" + plan.ShipFromJurisdictionCode + "|" + plan.EcommMasterProductDescription + "|" + plan.EcommLocalProductDescription);
                                m++;
                                trailerCount++;
                            }
                        }
                        if (plan.IsDelivery)
                        {
                            if (plan.DeliveryPickUpFee != 0)
                            {
                                sb.AppendLine("S" + "|" + plan.OrderNumber + "|" + plan.Status + "|" + lineItemCount.ToString().PadLeft(3, '0') + "|" + plan.CustomerNumber + "|" + plan.CustomerName + "|" + plan.CustomerAddress + "|" + plan.CustomerCity + "|" + plan.CustomerState + "|" + plan.CustomerPostalCode + "|" + plan.CustomerCountry + "|" + plan.CustomerJurisdictionCode + "|" + plan.ShipToName + "|" + plan.ShipToStreetAddress + "|" + plan.ShipToCity + "|" + plan.ShipToState + "|" + plan.ShipToPostalCode + "|" + plan.ShipToCountry + "|" + plan.ShipToJurisdictionCode + "|" + plan.ShipFromName + "|" + plan.ShipFromAddress + "|" + plan.ShipFromCity + "|" + plan.ShipFromState + "|" + plan.ShipFromPostalCode + "|" + plan.ShipFromCountry + "|" + plan.ShipFromJurisdictionCode + "|" + plan.EcommMasterProductDescription + "|" + plan.EcommLocalProductDescription);
                                m++;
                                trailerCount++;

                            }
                        }
                        if (plan.IsShipping)
                        {
                            if (plan.Shippingfee != 0)
                            {
                                sb.AppendLine("S" + "|" + plan.OrderNumber + "|" + plan.Status + "|" + lineItemCount.ToString().PadLeft(3, '0') + "|" + plan.CustomerNumber + "|" + plan.CustomerName + "|" + plan.CustomerAddress + "|" + plan.CustomerCity + "|" + plan.CustomerState + "|" + plan.CustomerPostalCode + "|" + plan.CustomerCountry + "|" + plan.CustomerJurisdictionCode + "|" + plan.ShipToName + "|" + plan.ShipToStreetAddress + "|" + plan.ShipToCity + "|" + plan.ShipToState + "|" + plan.ShipToPostalCode + "|" + plan.ShipToCountry + "|" + plan.ShipToJurisdictionCode + "|" + plan.ShipFromName + "|" + plan.ShipFromAddress + "|" + plan.ShipFromCity + "|" + plan.ShipFromState + "|" + plan.ShipFromPostalCode + "|" + plan.ShipFromCountry + "|" + plan.ShipFromJurisdictionCode + "|" + plan.EcommMasterProductDescription + "|" + plan.EcommLocalProductDescription);
                                m++;
                                trailerCount++;
                            }
                        }
                        if (plan.IsMealPlan || plan.IsDonation || plan.IsDonation)
                        {
                            if (vertexOrderTaxGls.Any())
                            {
                                foreach (var taxGls in vertexOrderTaxGls)
                                {
                                    if (taxGls.Total != 0)
                                    {
                                        sb.AppendLine("S" + "|" + plan.OrderNumber + "|" + (plan.IsMealPlan ? "1" : plan.Status) + "|" + lineItemCount.ToString().PadLeft(3, '0') + "|" + plan.CustomerNumber + "|" + plan.CustomerName + "|" + plan.CustomerAddress + "|" + plan.CustomerCity + "|" + plan.CustomerState + "|" + plan.CustomerPostalCode + "|" + plan.CustomerCountry + "|" + plan.CustomerJurisdictionCode + "|" + plan.ShipToName + "|" + plan.ShipToStreetAddress + "|" + plan.ShipToCity + "|" + plan.ShipToState + "|" + plan.ShipToPostalCode + "|" + plan.ShipToCountry + "|" + plan.ShipToJurisdictionCode + "|" + plan.ShipFromName + "|" + plan.ShipFromAddress + "|" + plan.ShipFromCity + "|" + plan.ShipFromState + "|" + plan.ShipFromPostalCode + "|" + plan.ShipFromCountry + "|" + plan.ShipFromJurisdictionCode + "|" + plan.EcommMasterProductDescription + "|" + plan.EcommLocalProductDescription);
                                        trailerCount++;
                                        n++;
                                    }
                                }
                            }
                        }
                    }
                    else if (plan.Status == "3")
                    {
                        if (mealplanCounter != 0)
                        {
                            for (int i = 0; i < mealplanCounter; i++)
                            {
                                sb.AppendLine("S" + "|" + plan.OrderNumber + "|" + 3 + "|" + lineItemCount.ToString().PadLeft(3, '0') + "|" + plan.CustomerNumber + "|" + plan.CustomerName + "|" + plan.CustomerAddress + "|" + plan.CustomerCity + "|" + plan.CustomerState + "|" + plan.CustomerPostalCode + "|" + plan.CustomerCountry + "|" + plan.CustomerJurisdictionCode + "|" + plan.ShipToName + "|" + plan.ShipToStreetAddress + "|" + plan.ShipToCity + "|" + plan.ShipToState + "|" + plan.ShipToPostalCode + "|" + plan.ShipToCountry + "|" + plan.ShipToJurisdictionCode + "|" + plan.ShipFromName + "|" + plan.ShipFromAddress + "|" + plan.ShipFromCity + "|" + plan.ShipFromState + "|" + plan.ShipFromPostalCode + "|" + plan.ShipFromCountry + "|" + plan.ShipFromJurisdictionCode + "|" + plan.EcommMasterProductDescription + "|" + plan.EcommLocalProductDescription);
                                trailerCount++;
                                n++;
                            }
                            sb.AppendLine("S" + "|" + plan.OrderNumber + "|" + 3 + "|" + lineItemCount.ToString().PadLeft(3, '0') + "|" + plan.CustomerNumber + "|" + plan.CustomerName + "|" + plan.CustomerAddress + "|" + plan.CustomerCity + "|" + plan.CustomerState + "|" + plan.CustomerPostalCode + "|" + plan.CustomerCountry + "|" + plan.CustomerJurisdictionCode + "|" + plan.ShipToName + "|" + plan.ShipToStreetAddress + "|" + plan.ShipToCity + "|" + plan.ShipToState + "|" + plan.ShipToPostalCode + "|" + plan.ShipToCountry + "|" + plan.ShipToJurisdictionCode + "|" + plan.ShipFromName + "|" + plan.ShipFromAddress + "|" + plan.ShipFromCity + "|" + plan.ShipFromState + "|" + plan.ShipFromPostalCode + "|" + plan.ShipFromCountry + "|" + plan.ShipFromJurisdictionCode + "|" + plan.EcommMasterProductDescription + "|" + plan.EcommLocalProductDescription);
                            trailerCount++;
                            n++;
                        }
                        else
                        {
                            if (plan.ProductAmount != 0)
                            {
                                sb.AppendLine("S" + "|" + plan.OrderNumber + "|" + 3 + "|" + lineItemCount.ToString().PadLeft(3, '0') + "|" + plan.CustomerNumber + "|" + plan.CustomerName + "|" + plan.CustomerAddress + "|" + plan.CustomerCity + "|" + plan.CustomerState + "|" + plan.CustomerPostalCode + "|" + plan.CustomerCountry + "|" + plan.CustomerJurisdictionCode + "|" + plan.ShipToName + "|" + plan.ShipToStreetAddress + "|" + plan.ShipToCity + "|" + plan.ShipToState + "|" + plan.ShipToPostalCode + "|" + plan.ShipToCountry + "|" + plan.ShipToJurisdictionCode + "|" + plan.ShipFromName + "|" + plan.ShipFromAddress + "|" + plan.ShipFromCity + "|" + plan.ShipFromState + "|" + plan.ShipFromPostalCode + "|" + plan.ShipFromCountry + "|" + plan.ShipFromJurisdictionCode + "|" + plan.EcommMasterProductDescription + "|" + plan.EcommLocalProductDescription);
                                trailerCount++;
                                n++;
                                sb.AppendLine("S" + "|" + plan.OrderNumber + "|" + 3 + "|" + lineItemCount.ToString().PadLeft(3, '0') + "|" + plan.CustomerNumber + "|" + plan.CustomerName + "|" + plan.CustomerAddress + "|" + plan.CustomerCity + "|" + plan.CustomerState + "|" + plan.CustomerPostalCode + "|" + plan.CustomerCountry + "|" + plan.CustomerJurisdictionCode + "|" + plan.ShipToName + "|" + plan.ShipToStreetAddress + "|" + plan.ShipToCity + "|" + plan.ShipToState + "|" + plan.ShipToPostalCode + "|" + plan.ShipToCountry + "|" + plan.ShipToJurisdictionCode + "|" + plan.ShipFromName + "|" + plan.ShipFromAddress + "|" + plan.ShipFromCity + "|" + plan.ShipFromState + "|" + plan.ShipFromPostalCode + "|" + plan.ShipFromCountry + "|" + plan.ShipFromJurisdictionCode + "|" + plan.EcommMasterProductDescription + "|" + plan.EcommLocalProductDescription);
                                trailerCount++;
                                n++;

                            }
                            if (plan.DeliveryPickUpFee != 0)
                            {
                                //sb.AppendLine("S" + "|" + plan.OrderNumber + "|" + plan.Status + "|" + n.ToString().PadLeft(3, '0') + "|" + plan.CustomerNumber + "|" + plan.CustomerName + "|" + plan.CustomerAddress + "|" + plan.CustomerCity + "|" + plan.CustomerState + "|" + plan.CustomerPostalCode + "|" + plan.CustomerCountry + "|" + plan.CustomerJurisdictionCode + "|" + plan.ShipToName + "|" + plan.ShipToStreetAddress + "|" + plan.ShipToCity + "|" + plan.ShipToState + "|" + plan.ShipToPostalCode + "|" + plan.ShipToCountry + "|" + plan.ShipToJurisdictionCode + "|" + plan.ShipFromName + "|" + plan.ShipFromAddress + "|" + plan.ShipFromCity + "|" + plan.ShipFromState + "|" + plan.ShipFromPostalCode + "|" + plan.ShipFromCountry + "|" + plan.ShipFromJurisdictionCode + "|" + plan.EcommMasterProductDescription + "|" + plan.EcommLocalProductDescription);
                                //trailerCount++;
                                //n++;
                                sb.AppendLine("S" + "|" + plan.OrderNumber + "|" + 3 + "|" + lineItemCount.ToString().PadLeft(3, '0') + "|" + plan.CustomerNumber + "|" + plan.CustomerName + "|" + plan.CustomerAddress + "|" + plan.CustomerCity + "|" + plan.CustomerState + "|" + plan.CustomerPostalCode + "|" + plan.CustomerCountry + "|" + plan.CustomerJurisdictionCode + "|" + plan.ShipToName + "|" + plan.ShipToStreetAddress + "|" + plan.ShipToCity + "|" + plan.ShipToState + "|" + plan.ShipToPostalCode + "|" + plan.ShipToCountry + "|" + plan.ShipToJurisdictionCode + "|" + plan.ShipFromName + "|" + plan.ShipFromAddress + "|" + plan.ShipFromCity + "|" + plan.ShipFromState + "|" + plan.ShipFromPostalCode + "|" + plan.ShipFromCountry + "|" + plan.ShipFromJurisdictionCode + "|" + plan.EcommMasterProductDescription + "|" + plan.EcommLocalProductDescription);
                                trailerCount++;
                                n++;
                            }
                            if (plan.Shippingfee != 0)
                            {
                                //sb.AppendLine("S" + "|" + plan.OrderNumber + "|" + plan.Status + "|" + n.ToString().PadLeft(3, '0') + "|" + plan.CustomerNumber + "|" + plan.CustomerName + "|" + plan.CustomerAddress + "|" + plan.CustomerCity + "|" + plan.CustomerState + "|" + plan.CustomerPostalCode + "|" + plan.CustomerCountry + "|" + plan.CustomerJurisdictionCode + "|" + plan.ShipToName + "|" + plan.ShipToStreetAddress + "|" + plan.ShipToCity + "|" + plan.ShipToState + "|" + plan.ShipToPostalCode + "|" + plan.ShipToCountry + "|" + plan.ShipToJurisdictionCode + "|" + plan.ShipFromName + "|" + plan.ShipFromAddress + "|" + plan.ShipFromCity + "|" + plan.ShipFromState + "|" + plan.ShipFromPostalCode + "|" + plan.ShipFromCountry + "|" + plan.ShipFromJurisdictionCode + "|" + plan.EcommMasterProductDescription + "|" + plan.EcommLocalProductDescription);
                                //trailerCount++;
                                //n++;
                                sb.AppendLine("S" + "|" + plan.OrderNumber + "|" + 3 + "|" + lineItemCount.ToString().PadLeft(3, '0') + "|" + plan.CustomerNumber + "|" + plan.CustomerName + "|" + plan.CustomerAddress + "|" + plan.CustomerCity + "|" + plan.CustomerState + "|" + plan.CustomerPostalCode + "|" + plan.CustomerCountry + "|" + plan.CustomerJurisdictionCode + "|" + plan.ShipToName + "|" + plan.ShipToStreetAddress + "|" + plan.ShipToCity + "|" + plan.ShipToState + "|" + plan.ShipToPostalCode + "|" + plan.ShipToCountry + "|" + plan.ShipToJurisdictionCode + "|" + plan.ShipFromName + "|" + plan.ShipFromAddress + "|" + plan.ShipFromCity + "|" + plan.ShipFromState + "|" + plan.ShipFromPostalCode + "|" + plan.ShipFromCountry + "|" + plan.ShipFromJurisdictionCode + "|" + plan.EcommMasterProductDescription + "|" + plan.EcommLocalProductDescription);
                                trailerCount++;
                                n++;
                            }
                        }
                        if (vertexOrderTaxGls.Any())
                        {
                            foreach (var taxGls in vertexOrderTaxGls)
                            {
                                if (taxGls.Total != 0)
                                {
                                    sb.AppendLine("S" + "|" + plan.OrderNumber + "|" + 3 + "|" + lineItemCount.ToString().PadLeft(3, '0') + "|" + plan.CustomerNumber + "|" + plan.CustomerName + "|" + plan.CustomerAddress + "|" + plan.CustomerCity + "|" + plan.CustomerState + "|" + plan.CustomerPostalCode + "|" + plan.CustomerCountry + "|" + plan.CustomerJurisdictionCode + "|" + plan.ShipToName + "|" + plan.ShipToStreetAddress + "|" + plan.ShipToCity + "|" + plan.ShipToState + "|" + plan.ShipToPostalCode + "|" + plan.ShipToCountry + "|" + plan.ShipToJurisdictionCode + "|" + plan.ShipFromName + "|" + plan.ShipFromAddress + "|" + plan.ShipFromCity + "|" + plan.ShipFromState + "|" + plan.ShipFromPostalCode + "|" + plan.ShipFromCountry + "|" + plan.ShipFromJurisdictionCode + "|" + plan.EcommMasterProductDescription + "|" + plan.EcommLocalProductDescription);
                                    trailerCount++;
                                    n++;
                                }
                            }
                        }
                    }

                    else if (plan.Status == "4")
                    {
                        sb.AppendLine("S" + "|" + plan.OrderNumber + "|" + 3 + "|" + lineItemCount.ToString().PadLeft(3, '0') + "|" + plan.CustomerNumber + "|" + plan.CustomerName + "|" + plan.CustomerAddress + "|" + plan.CustomerCity + "|" + plan.CustomerState + "|" + plan.CustomerPostalCode + "|" + plan.CustomerCountry + "|" + plan.CustomerJurisdictionCode + "|" + plan.ShipToName + "|" + plan.ShipToStreetAddress + "|" + plan.ShipToCity + "|" + plan.ShipToState + "|" + plan.ShipToPostalCode + "|" + plan.ShipToCountry + "|" + plan.ShipToJurisdictionCode + "|" + plan.ShipFromName + "|" + plan.ShipFromAddress + "|" + plan.ShipFromCity + "|" + plan.ShipFromState + "|" + plan.ShipFromPostalCode + "|" + plan.ShipFromCountry + "|" + plan.ShipFromJurisdictionCode + "|" + plan.EcommMasterProductDescription + "|" + plan.EcommLocalProductDescription);
                        trailerCount++;
                        n++;
                        if (partialRefundCounter != 0)
                        {
                            for (int i = 0; i < partialRefundCounter; i++)
                            {
                                sb.AppendLine("S" + "|" + plan.OrderNumber + "|" + 3 + "|" + lineItemCount.ToString().PadLeft(3, '0') + "|" + plan.CustomerNumber + "|" + plan.CustomerName + "|" + plan.CustomerAddress + "|" + plan.CustomerCity + "|" + plan.CustomerState + "|" + plan.CustomerPostalCode + "|" + plan.CustomerCountry + "|" + plan.CustomerJurisdictionCode + "|" + plan.ShipToName + "|" + plan.ShipToStreetAddress + "|" + plan.ShipToCity + "|" + plan.ShipToState + "|" + plan.ShipToPostalCode + "|" + plan.ShipToCountry + "|" + plan.ShipToJurisdictionCode + "|" + plan.ShipFromName + "|" + plan.ShipFromAddress + "|" + plan.ShipFromCity + "|" + plan.ShipFromState + "|" + plan.ShipFromPostalCode + "|" + plan.ShipFromCountry + "|" + plan.ShipFromJurisdictionCode + "|" + plan.EcommMasterProductDescription + "|" + plan.EcommLocalProductDescription);
                                trailerCount++;
                                n++;
                            }
                        }
                    }

                    isMealPlan = plan.IsMealPlan;

                }

                if (isVertexLineNeeded)
                {
                    int vertexStatus = Convert.ToInt32(status);


                    //Vertex Record
                    var VertexRecordonStatus = response.Where(x => x.Status == vertexStatus).OrderBy(x => x.OrderNumber).ToList();
                    foreach (DistributiveTaxResponse plan in VertexRecordonStatus)
                    {
                        if (plan.OrderAmount != decimal.Zero)
                        {
                            int j = 1;
                            int count = 0;
                            foreach (var lineItem in plan.LineItems)
                            {

                                string productId = lineItem.Split('|').ToArray()[0];

                                if (productId.Contains("-"))
                                    productId = productId.Split('-')[0];
                                if (plan.ProductID == productId)
                                {
                                    decimal deliverytax = plan.DeliveryTax;
                                    decimal shippingTax = plan.ShippingTax;

                                    string lineItemData = lineItem.Replace(lineItem.Split('|').First() + "|", "");
                                    if (plan.Status == 3 || plan.Status == 4)
                                    {
                                        sb.AppendLine("V" + "|" + plan.OrderNumber + "|" + 3 + "|" + lineItemCount.ToString().PadLeft(3, '0') + "|" + plan.VertexPostingDate.ToString("MMddyyyy") + "|" + (plan.InvoiceSubtotal < decimal.Zero ? -plan.InvoiceSubtotal : plan.InvoiceSubtotal) + "|" + (plan.InvoiceTotal < decimal.Zero ? -plan.InvoiceTotal : plan.InvoiceTotal) + "|" + plan.CalculatedTaxAmounts[count] + "|" + lineItemData);
                                        trailerCount++;
                                    }
                                    else
                                    {
                                        sb.AppendLine("V" + "|" + plan.OrderNumber + "|" + (isMealPlan ? 1 : plan.Status) + "|" + lineItemCount.ToString().PadLeft(3, '0') + "|" + plan.VertexPostingDate.ToString("MMddyyyy") + "|" + plan.InvoiceSubtotal + "|" + plan.InvoiceTotal + "|" + plan.CalculatedTaxAmounts[count] + "|" + lineItemData);
                                        trailerCount++;
                                    }
                                    //}

                                }
                                j++;
                                count++;
                            }
                        }
                    }
                }
                // sb.AppendLine("\n");
            }


            file.TrailerCount += trailerCount;
            file.TrailerTotal += trailerTotal;

            file.Builder.Append(sb);



            return file;
        }


        private static List<SAPSalesJournalEntry> getSAPDatabaseResults(int orderNumber, int statusId)
        {
            var reportList = new List<SAPSalesJournalEntry>();

            try
            {
                var connStr = ConfigurationManager.ConnectionStrings["BlueDot"].ConnectionString;

                using (var conn = new SqlConnection(connStr))
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    var sqlSelect = string.Format(@"
                       select s.extkey, oi.PriceExclTax, oi.TotalRefundedAmount, oi.DeliveryAmountExclTax, sap.*, o.*,p.*,p.Id As [ProductId],oi.* from sap_salesjournal sap
                        inner join [Order] o
                        on o.id = sap.OrderNumber
                        inner join OrderItem oi
                        on oi.id = sap.OrderItemId
                        inner join Store s
                        on o.storeid = s.id
                        inner join Product P
                        on oi.Productid=P.id
                        where sap.IsExported = 0 and sap.OrderNumber = {0} and sap.status = {1} ", orderNumber, statusId);
                    cmd.CommandText = sqlSelect;
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var entry = new SAPSalesJournalEntry();
                            entry.Id = ((int)reader["Id"]);
                            entry.Status = Convert.ToString(reader["Status"] == DBNull.Value ? "" : reader["Status"]);
                            entry.OrderItemId = ((int)reader["OrderItemId"]);
                            entry.OrderNumber = ((int)reader["OrderNumber"]);
                            entry.productId = ((int)reader["ProductId"]);
                            if (Convert.ToBoolean(reader["IsMealPlan"]))
                                entry.IsMealPlan = true;
                            else
                                entry.IsMealPlan = false;

                            if (Convert.ToInt32(reader["VendorId"]) != 0)
                                entry.IsVendor = true;
                            else
                                entry.IsVendor = false;
                            entry.IsDonation = Convert.ToBoolean(reader["IsDonation"]) ? true : false;
                            entry.isDownload = Convert.ToBoolean(reader["IsDownload"]) ? true : false;
                            entry.IsDelivery = Convert.ToBoolean(reader["IsDeliveryPickUp"]) ? true : false;
                            entry.IsShipping = Convert.ToBoolean(reader["IsShipping"]) ? true : false;
                            entry.FulfillmentDateTime = Convert.ToString(reader["FulfillmentDateTime"]);
                            entry.Quantity = ((int)reader["Quantity"]);
                            if (entry.IsDelivery)
                                entry.DeliveryPickUpFee = Convert.ToDecimal(reader["DeliveryPickupFee"]);
                            if (entry.IsShipping)
                                entry.Shippingfee = Convert.ToDecimal(reader["ShippingFee"]);

                            if (entry.Status == "3" || entry.Status == "4") //refunds & cancels
                            {
                                var data = dbEntity.Orders.Where(x => x.Id == entry.OrderNumber).FirstOrDefault();
                                var dataItem = data.OrderItems.Where(x => x.Id == entry.OrderItemId).FirstOrDefault();
                                entry.TransactionDate = Convert.ToDateTime(reader["DateOfRefund"]);
                                entry.TransactionTime = entry.TransactionDate.ToString("HHmmss");

                                if (data.OrderStatusId == 40)
                                {
                                    if (dataItem.IsPartialRefund == false)
                                    {
                                        entry.AmountSign = "-";
                                        entry.ProductAmount = Convert.ToDecimal(reader["PriceExclTax"]);

                                        entry.OrderAmount = ((decimal)reader["PriceInclTax"]);

                                        if (entry.IsShipping)
                                        {
                                            entry.OrderAmount = Convert.ToDecimal(reader["PriceInclTax"]) + Convert.ToDecimal(reader["ShippingFee"]);
                                        }
                                        if (entry.IsDelivery)
                                        {
                                            entry.OrderAmount = Convert.ToDecimal(reader["PriceInclTax"]) + Convert.ToDecimal(reader["DeliveryPickupFee"]);
                                        }
                                    }
                                }
                                if (data.OrderStatusId != 40)
                                {
                                    entry.AmountSign = "-";
                                    entry.OrderAmount = ((decimal)reader["TotalRefundedAmount"]);

                                }

                                if (data.OrderStatusId == 40)
                                {
                                    if (dataItem.IsPartialRefund == true)
                                    {
                                        entry.AmountSign = "-";
                                        entry.OrderAmount = ((decimal)reader["TotalRefundedAmount"]);
                                    }
                                }

                                //entry.AmountSign = "-";
                                //entry.ProductAmount = ((int)reader["ProductAmount"] * (-1));
                            }
                            else if (entry.Status == "1") //should be total order.
                            {
                                entry.AmountSign = null;
                                entry.ProductAmount = Convert.ToDecimal(reader["PriceExclTax"]);
                                entry.TransactionDate = Convert.ToDateTime(reader["createdonutc"]);
                                entry.TransactionTime = entry.TransactionDate.ToString("HHmmss");
                                entry.OrderAmount = ((decimal)reader["PriceInclTax"]);

                                if (entry.IsShipping)
                                {
                                    entry.OrderAmount = Convert.ToDecimal(reader["PriceInclTax"]) + Convert.ToDecimal(reader["ShippingFee"]);
                                }
                                if (entry.IsDelivery)
                                {
                                    entry.OrderAmount = Convert.ToDecimal(reader["PriceInclTax"]) + Convert.ToDecimal(reader["DeliveryPickupFee"]);
                                }
                            }
                            else
                            {
                                entry.AmountSign = null;
                                // entry.ProductAmount = ((int)reader["ProductAmount"]);
                                if (entry.IsMealPlan || entry.IsDonation || entry.isDownload)
                                {
                                    entry.OrderAmount = ((decimal)reader["PriceInclTax"]);
                                }
                                else
                                {
                                    entry.OrderAmount = ((decimal)reader["PriceExclTax"]);
                                }
                                entry.ProductAmount = ((decimal)reader["PriceExclTax"]);
                                if (entry.IsMealPlan || entry.IsDonation || entry.isDownload)
                                {
                                    entry.TransactionDate = Convert.ToDateTime(reader["createdonutc"]);
                                    entry.TransactionTime = entry.TransactionDate.ToString("HHmmss");
                                }
                                else
                                {
                                    entry.TransactionDate = Convert.ToDateTime(reader["FulfillmentDateTime"]);
                                    entry.TransactionTime = entry.TransactionDate.ToString("HHmmss");
                                }

                                if (entry.IsMealPlan || entry.IsDonation || entry.isDownload)
                                {
                                    if (entry.IsShipping)
                                    {
                                        entry.OrderAmount = Convert.ToDecimal(reader["PriceInclTax"]) + Convert.ToDecimal(reader["ShippingFee"]);
                                    }
                                    if (entry.IsDelivery)
                                    {
                                        entry.OrderAmount = Convert.ToDecimal(reader["PriceInclTax"]) + Convert.ToDecimal(reader["DeliveryPickupFee"]);
                                    }
                                }
                                else
                                {
                                    if (entry.IsShipping)
                                    {
                                        entry.OrderAmount = Convert.ToDecimal(reader["PriceExclTax"]) + Convert.ToDecimal(reader["ShippingFee"]);
                                    }
                                    if (entry.IsDelivery)
                                    {
                                        entry.OrderAmount = Convert.ToDecimal(reader["PriceExclTax"]) + Convert.ToDecimal(reader["DeliveryPickupFee"]);
                                    }
                                }
                            }

                            entry.CostCenterNumber = (reader["CostCenterNumber"] == DBNull.Value ? "" : (string)reader["CostCenterNumber"]).Trim();
                            entry.GLAccount = (reader["ProductGL"] == DBNull.Value ? "" : (string)reader["ProductGL"]);
                            entry.OrderNumber = ((int)reader["OrderNumber"]);

                            //xmlDict.Add("customertaxareaID", customertaxareaID);
                            //xmlDict.Add("CustomerNumber", CustomerNumber);
                            //xmlDict.Add("customerStreetAddress1", customerStreetAddress1);
                            //xmlDict.Add("customerStreetAddress2", customerStreetAddress2);
                            //xmlDict.Add("customerCity", customerCity);
                            //xmlDict.Add("customerState", customerState);
                            //xmlDict.Add("customerPostalCode", customerPostalCode);
                            //xmlDict.Add("customerCountry", customerCountry);
                            //continue;
                            // mealPlan.LineNumber = i.ToString();                                                       
                            entry.RecordIndicator = "F";
                            // entry.TransactionDate = ((DateTime)reader["SAPEntryDate"]);
                            entry.TaxAmountImposed = ((decimal)reader["TaxAmountImposed"]);
                            entry.DeliveryAmountExclTax = ((decimal)reader["DeliveryAmountExclTax"]);
                            entry.CustomerNumber = ((int)reader["CustomerNumber"]);
                            entry.CustomerName = reader["CustomerName"].ToString();
                            entry.CustomerAddress = reader["CustomerStreetAddress"].ToString();
                            entry.CustomerCity = reader["CustomerCity"].ToString();
                            entry.CustomerPostalCode = reader["CustomerPostalCode"].ToString();
                            entry.CustomerState = reader["ShipToJurisdiction"].ToString();
                            entry.CustomerCountry = reader["CustomerCountry"].ToString();
                            entry.CustomerJurisdictionCode = reader["ShipToJurisdiction"].ToString();
                            entry.ShipToName = reader["ShipToName"].ToString();
                            entry.ShipToStreetAddress = reader["ShipToStreetAddress"].ToString();
                            entry.ShipToCity = reader["ShipToCity"].ToString();
                            entry.ShipToState = reader["ShipToJurisdiction"].ToString();
                            entry.ShipToPostalCode = reader["ShipToPostalCode"].ToString();
                            entry.ShipToCountry = reader["ShipToCountry"].ToString();
                            entry.ShipToJurisdictionCode = reader["ShipToJurisdiction"].ToString();
                            entry.ShipFromName = reader["ShipFromName"].ToString();
                            entry.ShipFromAddress = reader["ShipFromStreetAddress"].ToString();
                            entry.ShipFromCity = reader["ShipFromCity"].ToString();
                            entry.ShipFromState = reader["ShipFromState"].ToString();
                            entry.ShipFromPostalCode = reader["ShipFromPostalCode"].ToString();
                            entry.ShipFromCountry = reader["ShipFromCountry"].ToString();
                            entry.ShipFromJurisdictionCode = reader["ShipFromState"].ToString();
                            entry.CreatedOnUtc = ((DateTime)reader["CreatedOnUtc"]);
                            entry.EcommMasterProductDescription = reader["MasterProduct"].ToString();
                            entry.EcommLocalProductDescription = reader["LocalProduct"].ToString();
                            reportList.Add(entry);
                        }
                    }
                }

                return reportList;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }
            return reportList;

        }

        private static List<SAPSalesJournalEntry> getSAPDatabaseResults(int orderNumber, int statusId, int orderItemId)
        {
            var reportList = new List<SAPSalesJournalEntry>();

            try
            {
                var connStr = ConfigurationManager.ConnectionStrings["BlueDot"].ConnectionString;

                using (var conn = new SqlConnection(connStr))
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    var sqlSelect = string.Format(@"
                       select s.extkey, oi.PriceExclTax, oi.TotalRefundedAmount, oi.DeliveryAmountExclTax, sap.*, o.*,p.*,p.Id As [ProductId],oi.* from sap_salesjournal sap
                        inner join [Order] o
                        on o.id = sap.OrderNumber
                        inner join OrderItem oi
                        on oi.id = sap.OrderItemId
                        inner join Store s
                        on o.storeid = s.id
                        inner join Product P
                        on oi.Productid=P.id
                        where sap.IsExported = 0 and sap.OrderNumber = {0} and sap.status = {1}  and sap.OrderItemId={2}", orderNumber, statusId, orderItemId);
                    cmd.CommandText = sqlSelect;
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var entry = new SAPSalesJournalEntry();
                            entry.Id = ((int)reader["Id"]);
                            entry.Status = Convert.ToString(reader["Status"] == DBNull.Value ? "" : reader["Status"]);
                            entry.OrderItemId = ((int)reader["OrderItemId"]);
                            entry.OrderNumber = ((int)reader["OrderNumber"]);
                            entry.productId = ((int)reader["ProductId"]);
                            if (Convert.ToBoolean(reader["IsMealPlan"]))
                                entry.IsMealPlan = true;
                            else
                                entry.IsMealPlan = false;

                            if (Convert.ToInt32(reader["VendorId"]) != 0)
                                entry.IsVendor = true;
                            else
                                entry.IsVendor = false;
                            entry.IsDonation = Convert.ToBoolean(reader["IsDonation"]) ? true : false;
                            entry.isDownload = Convert.ToBoolean(reader["IsDownload"]) ? true : false;
                            entry.IsDelivery = Convert.ToBoolean(reader["IsDeliveryPickUp"]) ? true : false;
                            entry.IsShipping = Convert.ToBoolean(reader["IsShipping"]) ? true : false;
                            entry.FulfillmentDateTime = Convert.ToString(reader["FulfillmentDateTime"]);
                            entry.Quantity = ((int)reader["Quantity"]);
                            if (entry.IsDelivery)
                                entry.DeliveryPickUpFee = Convert.ToDecimal(reader["DeliveryPickupFee"]);
                            if (entry.IsShipping)
                                entry.Shippingfee = Convert.ToDecimal(reader["ShippingFee"]);

                            if (entry.Status == "3" || entry.Status == "4") //refunds & cancels
                            {
                                var data = dbEntity.Orders.Where(x => x.Id == entry.OrderNumber).FirstOrDefault();
                                var dataItem = data.OrderItems.Where(x => x.Id == entry.OrderItemId).FirstOrDefault();
                                entry.TransactionDate = Convert.ToDateTime(reader["DateOfRefund"]);
                                entry.TransactionTime = entry.TransactionDate.ToString("HHmmss");

                                if (data.OrderStatusId == 40)
                                {
                                    if (dataItem.IsPartialRefund == false)
                                    {
                                        entry.AmountSign = "-";
                                        entry.ProductAmount = Convert.ToDecimal(reader["PriceExclTax"]);

                                        entry.OrderAmount = ((decimal)reader["PriceInclTax"]);

                                        if (entry.IsShipping)
                                        {
                                            entry.OrderAmount = Convert.ToDecimal(reader["PriceInclTax"]) + Convert.ToDecimal(reader["ShippingFee"]);
                                        }
                                        if (entry.IsDelivery)
                                        {
                                            entry.OrderAmount = Convert.ToDecimal(reader["PriceInclTax"]) + Convert.ToDecimal(reader["DeliveryPickupFee"]);
                                        }
                                    }
                                }
                                if (data.OrderStatusId != 40)
                                {
                                    entry.AmountSign = "-";
                                    entry.OrderAmount = ((decimal)reader["TotalRefundedAmount"]);

                                }

                                if (data.OrderStatusId == 40)
                                {
                                    if (dataItem.IsPartialRefund == true)
                                    {
                                        entry.AmountSign = "-";
                                        entry.OrderAmount = ((decimal)reader["TotalRefundedAmount"]);
                                    }
                                }

                                //entry.AmountSign = "-";
                                //entry.ProductAmount = ((int)reader["ProductAmount"] * (-1));
                            }
                            else if (entry.Status == "1") //should be total order.
                            {
                                entry.AmountSign = null;
                                entry.ProductAmount = Convert.ToDecimal(reader["PriceExclTax"]);
                                entry.TransactionDate = Convert.ToDateTime(reader["createdonutc"]);
                                entry.TransactionTime = entry.TransactionDate.ToString("HHmmss");
                                entry.OrderAmount = ((decimal)reader["PriceInclTax"]);

                                if (entry.IsShipping)
                                {
                                    entry.OrderAmount = Convert.ToDecimal(reader["PriceInclTax"]) + Convert.ToDecimal(reader["ShippingFee"]);
                                }
                                if (entry.IsDelivery)
                                {
                                    entry.OrderAmount = Convert.ToDecimal(reader["PriceInclTax"]) + Convert.ToDecimal(reader["DeliveryPickupFee"]);
                                }
                            }
                            else
                            {
                                entry.AmountSign = null;
                                // entry.ProductAmount = ((int)reader["ProductAmount"]);
                                if (entry.IsMealPlan || entry.IsDonation || entry.isDownload)
                                {
                                    entry.OrderAmount = ((decimal)reader["PriceInclTax"]);
                                }
                                else
                                {
                                    entry.OrderAmount = ((decimal)reader["PriceExclTax"]);
                                }
                                entry.ProductAmount = ((decimal)reader["PriceExclTax"]);
                                if (entry.IsMealPlan)
                                {
                                    entry.TransactionDate = Convert.ToDateTime(reader["createdonutc"]);
                                    entry.TransactionTime = entry.TransactionDate.ToString("HHmmss");
                                }
                                else
                                {
                                    entry.TransactionDate = Convert.ToDateTime(reader["FulfillmentDateTime"]);
                                    entry.TransactionTime = entry.TransactionDate.ToString("HHmmss");
                                }

                                if (entry.IsMealPlan || entry.IsDonation || entry.isDownload)
                                {
                                    if (entry.IsShipping)
                                    {
                                        entry.OrderAmount = Convert.ToDecimal(reader["PriceInclTax"]) + Convert.ToDecimal(reader["ShippingFee"]);
                                    }
                                    if (entry.IsDelivery)
                                    {
                                        entry.OrderAmount = Convert.ToDecimal(reader["PriceInclTax"]) + Convert.ToDecimal(reader["DeliveryPickupFee"]);
                                    }
                                }
                                else
                                {
                                    if (entry.IsShipping)
                                    {
                                        entry.OrderAmount = Convert.ToDecimal(reader["PriceExclTax"]) + Convert.ToDecimal(reader["ShippingFee"]);
                                    }
                                    if (entry.IsDelivery)
                                    {
                                        entry.OrderAmount = Convert.ToDecimal(reader["PriceExclTax"]) + Convert.ToDecimal(reader["DeliveryPickupFee"]);
                                    }
                                }
                            }

                            entry.CostCenterNumber = (reader["CostCenterNumber"] == DBNull.Value ? "" : (string)reader["CostCenterNumber"]).Trim();
                            entry.GLAccount = (reader["ProductGL"] == DBNull.Value ? "" : (string)reader["ProductGL"]);
                            entry.OrderNumber = ((int)reader["OrderNumber"]);

                            //xmlDict.Add("customertaxareaID", customertaxareaID);
                            //xmlDict.Add("CustomerNumber", CustomerNumber);
                            //xmlDict.Add("customerStreetAddress1", customerStreetAddress1);
                            //xmlDict.Add("customerStreetAddress2", customerStreetAddress2);
                            //xmlDict.Add("customerCity", customerCity);
                            //xmlDict.Add("customerState", customerState);
                            //xmlDict.Add("customerPostalCode", customerPostalCode);
                            //xmlDict.Add("customerCountry", customerCountry);
                            //continue;
                            // mealPlan.LineNumber = i.ToString();                                                       
                            entry.RecordIndicator = "F";
                            // entry.TransactionDate = ((DateTime)reader["SAPEntryDate"]);
                            entry.TaxAmountImposed = ((decimal)reader["TaxAmountImposed"]);
                            entry.DeliveryAmountExclTax = ((decimal)reader["DeliveryAmountExclTax"]);
                            entry.CustomerNumber = ((int)reader["CustomerNumber"]);
                            entry.CustomerName = reader["CustomerName"].ToString();
                            entry.CustomerAddress = reader["CustomerStreetAddress"].ToString();
                            entry.CustomerCity = reader["CustomerCity"].ToString();
                            entry.CustomerPostalCode = reader["CustomerPostalCode"].ToString();
                            entry.CustomerState = reader["ShipToJurisdiction"].ToString();
                            entry.CustomerCountry = reader["CustomerCountry"].ToString();
                            entry.CustomerJurisdictionCode = reader["ShipToJurisdiction"].ToString();
                            entry.ShipToName = reader["ShipToName"].ToString();
                            entry.ShipToStreetAddress = reader["ShipToStreetAddress"].ToString();
                            entry.ShipToCity = reader["ShipToCity"].ToString();
                            entry.ShipToState = reader["ShipToJurisdiction"].ToString();
                            entry.ShipToPostalCode = reader["ShipToPostalCode"].ToString();
                            entry.ShipToCountry = reader["ShipToCountry"].ToString();
                            entry.ShipToJurisdictionCode = reader["ShipToJurisdiction"].ToString();
                            entry.ShipFromName = reader["ShipFromName"].ToString();
                            entry.ShipFromAddress = reader["ShipFromStreetAddress"].ToString();
                            entry.ShipFromCity = reader["ShipFromCity"].ToString();
                            entry.ShipFromState = reader["ShipFromState"].ToString();
                            entry.ShipFromPostalCode = reader["ShipFromPostalCode"].ToString();
                            entry.ShipFromCountry = reader["ShipFromCountry"].ToString();
                            entry.ShipFromJurisdictionCode = reader["ShipFromState"].ToString();
                            entry.CreatedOnUtc = ((DateTime)reader["CreatedOnUtc"]);
                            entry.EcommMasterProductDescription = reader["MasterProduct"].ToString();
                            entry.EcommLocalProductDescription = reader["LocalProduct"].ToString();
                            reportList.Add(entry);
                        }
                    }
                }

                return reportList;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }
            return reportList;

        }

        public static List<DistributiveTaxResponse> getDistributiveDataResponseForOrder(int orderId, int status, int orderitemid,int? productid=null)
        {
            List<DistributiveTaxResponse> responseList = new List<DistributiveTaxResponse>();
            try
            {
                List<SAPTaxResponseStorage> response = new List<SAPTaxResponseStorage>();
                var listProducts = new List<int>();
                var orderdetails = dbEntity.OrderItems.Where(x => x.OrderId == orderId && x.Id == orderitemid).FirstOrDefault(); // Order Items
                var orderDetails = dbEntity.Orders.Where(x => x.Id == orderId).FirstOrDefault();

                var sameProductcount = orderDetails.OrderItems.GroupBy(x => x.ProductId)
                 .Where(g => g.Count() > 1)
                 .Select(y => y.Key)
                 .ToList();
                if (sameProductcount.Any())
                {
                    foreach (var itemData in sameProductcount)
                    {
                        listProducts.Add(itemData);
                    }
                }

                if (listProducts.Contains(orderdetails.ProductId))
                {
                    response = getTaxResponseDataFromDB(orderId, orderdetails.ProductId, status, orderitemid);
                }
                else
                {
                    //if (orderdetails.Product.IsBundleProduct == true)
                    //    response = getTaxResponseDataFromDB(orderId, orderdetails.ProductId, status);
                    //else
                        response = getTaxResponseDataFromDB(orderId, orderdetails.ProductId, status);
                }
                var productDetails = dbEntity.Products.Where(x => x.Id == orderdetails.ProductId).FirstOrDefault();

                var storeDetails = dbEntity.Stores.Where(x => x.Id == orderDetails.StoreId).FirstOrDefault();

                var shiptoAddress = dbEntity.Addresses.Where(x => x.Id == orderDetails.ShippingAddressId).FirstOrDefault();

                // Section where i am checking if this product has multiple attributes but with the same product id in the order.

                if (response.Count > 0)
                {
                    string[] ProductID = response[0].ProductId.Split('|');

                    string responseXml = response[0].XMLResponse.ToString();

                    Dictionary<string, string> dictResponse = getDataFromResponseXML(responseXml, productDetails.Name);

                    for (int i = 0; i < ProductID.Count(); i++)
                    {
                        DistributiveTaxResponse listdata = new DistributiveTaxResponse();

                        listdata.OrderAmount = orderdetails.PriceExclTax;
                        listdata.OrderItemID = orderitemid;
                        listdata.CustomerNumber = response[0].CustomerId;
                        listdata.OrderNumber = Convert.ToInt32(response[0].OrderId);
                        listdata.ProductID = response[0].ProductId.Split('|')[i];
                        listdata.VertexPostingDate = response[0].AddedDate;
                        listdata.VertexEntryDate = response[0].AddedDate;
                        string lineProductId = "LineItem_" + listdata.ProductID;
                        string calculatedTaxResultOutput = "calculatedTaxResultOutput_" + listdata.ProductID;
                        //var lineItemData = dictResponse.Where(kvp => kvp.Key.Contains(lineProductId)).ToList();
                        var lineItemData = dictResponse.Where(kvp => kvp.Key.Contains("LineItem_")).ToList();
                        var calculatedTaxDataAmount = dictResponse.Where(kvp => kvp.Key.Contains("calculatedTaxResultOutput_")).ToList();
                        if (listdata.LineItems == null)
                        {
                            listdata.LineItems = new List<string>();
                        }
                        foreach (var item in lineItemData)
                        {
                            listdata.LineItems.Add(item.Value);
                        }
                        if (listdata.CalculatedTaxAmounts == null)
                        {
                            listdata.CalculatedTaxAmounts = new List<string>();
                        }
                        foreach (var items in calculatedTaxDataAmount)
                        {
                            listdata.CalculatedTaxAmounts.Add(items.Value);
                        }
                        listdata.VertexDocumentNumber = Convert.ToInt32(dictResponse["documentNumber"]);
                        listdata.VertexPostingDate = Convert.ToDateTime(dictResponse["documentDate"]);


                        listdata.ShipFromName = dbEntity.Stores.Where(x => x.Id == orderDetails.StoreId).FirstOrDefault().Name; //_storeContext.CurrentStore.Name;
                        listdata.ShipFromStreetAddress = Convert.ToString(dictResponse["sellerStreetAddress"]);
                        listdata.ShipFromCity = Convert.ToString(dictResponse["sellerCity"]);
                        listdata.ShipFromPostalCode = Convert.ToString(dictResponse["sellerPostalCode"]);
                        listdata.ShipFromCountry = Convert.ToString(dictResponse["sellerCountry"]);
                        listdata.ShipFromState = Convert.ToString(dictResponse["sellerState"]);

                        listdata.ShipToName = (dbEntity.Customers.Where(x => x.Id == orderDetails.CustomerId).FirstOrDefault().Username == null ? "" : dbEntity.Customers.Where(x => x.Id == orderDetails.CustomerId).FirstOrDefault().Username);
                        listdata.CustomerName = listdata.ShipToName;

                        listdata.ShipToStreetAddress = String.Concat(Convert.ToString(dictResponse["customerStreetAddress1"]), " ", Convert.ToString(dictResponse["customerStreetAddress2"]));
                        listdata.CustomerStreetAddress = listdata.ShipToStreetAddress;
                        listdata.ShipToCity = Convert.ToString(dictResponse["customerCity"]);
                        listdata.CustomerCity = Convert.ToString(dictResponse["customerCity"]);
                        listdata.ShipToJurisdiction = Convert.ToString(dictResponse["customerState"]);
                        listdata.ShipToPostalCode = Convert.ToString(dictResponse["customerPostalCode"]);
                        listdata.CustomerPostalCode = Convert.ToString(dictResponse["customerPostalCode"]);

                        listdata.ShipToCountry = Convert.ToString(dictResponse["customerCountry"]);
                        listdata.CustomerCountry = Convert.ToString(dictResponse["customerCountry"]);
                        listdata.ProductAmount = Convert.ToDecimal(dictResponse["ProductAmt"]);
                        listdata.CostCenterNumber = dbEntity.Stores.Where(x => x.Id == orderDetails.StoreId).FirstOrDefault().ExtKey;//Convert.ToString(_storeContext.CurrentStore.ExtKey);
                        listdata.Status = status;
                        listdata.VertexCompanyCode = 0;
                        listdata.VertexFiscalYear = response[0].AddedDate.Year;
                        listdata.VertexDocumentType = "eCommerce";
                        listdata.DateOfTransaction = response[0].AddedDate;
                        listdata.TaxAmountImposed = Convert.ToDecimal(dictResponse["totalTax"]);
                        if (dictResponse.ContainsKey("DeliveryTax"))
                            listdata.DeliveryTax = Convert.ToDecimal(dictResponse["DeliveryTax"]);
                        if (dictResponse.ContainsKey("ShippingTax"))
                            listdata.ShippingTax = Convert.ToDecimal(dictResponse["ShippingTax"]);
                        //if (dictResponse.ContainsKey("ProductTax"))
                        //    listdata.ProductTax = Convert.ToDecimal(dictResponse["ProductTax"]);
                        listdata.InvoiceTotal = Convert.ToDecimal(dictResponse["totalAmt"]);
                        listdata.InvoiceTax = Convert.ToDecimal(dictResponse["totalTax"]);

                        responseList.Add(listdata);
                    }
                }
                else
                {
                    DistributiveTaxResponse listdata = new DistributiveTaxResponse();


                    listdata.CustomerNumber = dbEntity.Orders.Where(x => x.Id == orderId).FirstOrDefault().CustomerId;
                    listdata.OrderNumber = Convert.ToInt32(orderId);
                    listdata.ProductID = Convert.ToString(productDetails.Id);
                    //listdata.VertexPostingDate = response[0].AddedDate;// Not available
                    //listdata.VertexEntryDate = response[0].AddedDate;// Not available
                    string lineProductId = "LineItem_" + listdata.ProductID;
                    //string calculatedTaxResultOutput = "calculatedTaxResultOutput_" + listdata.ProductID; // Not available
                    //var lineItemData = dictResponse.Where(kvp => kvp.Key.Contains(lineProductId)).ToList(); // Not available
                    //var lineItemData = dictResponse.Where(kvp => kvp.Key.Contains("LineItem_")).ToList(); // Not available
                    //var calculatedTaxDataAmount = dictResponse.Where(kvp => kvp.Key.Contains("calculatedTaxResultOutput_")).ToList(); // Not available
                    if (listdata.LineItems == null)
                    {
                        listdata.LineItems = new List<string>();
                    }
                    //foreach (var item in lineItemData)   
                    //{
                    //    listdata.LineItems.Add(item.Value);
                    //}
                    //if (listdata.CalculatedTaxAmounts == null)
                    //{
                    //    listdata.CalculatedTaxAmounts = new List<string>();
                    //}
                    //foreach (var items in calculatedTaxDataAmount)
                    //{
                    //    listdata.CalculatedTaxAmounts.Add(items.Value);
                    //}
                    //listdata.VertexDocumentNumber = Convert.ToInt32(dictResponse["documentNumber"]); // Not available
                    //  listdata.VertexPostingDate = Convert.ToDateTime(dictResponse["documentDate"]); // Not available
                    //var orderDetails = dbEntity.Orders.Where(x => x.Id == listdata.OrderNumber).FirstOrDefault();

                    listdata.ShipFromName = storeDetails.Name; //_storeContext.CurrentStore.Name;
                    listdata.ShipFromStreetAddress = storeDetails.CompanyAddress;
                    listdata.ShipFromCity = storeDetails.CompanyCity;
                    listdata.ShipFromPostalCode = storeDetails.CompanyZipPostalCode;
                    listdata.ShipFromCountry = storeDetails.CompanyCountryId == 1 ? "Us" : "Canada";
                    listdata.ShipFromState = dbEntity.StateProvinces.Where(x => x.Id == storeDetails.CompanyStateProvinceId).FirstOrDefault().Name;

                    listdata.ShipToName = (dbEntity.Customers.Where(x => x.Id == orderDetails.CustomerId).FirstOrDefault().Username == null ? "" : dbEntity.Customers.Where(x => x.Id == orderDetails.CustomerId).FirstOrDefault().Username);
                    listdata.CustomerName = listdata.ShipToName;

                    listdata.ShipToStreetAddress = shiptoAddress.Address1 + shiptoAddress.Address2;
                    listdata.CustomerStreetAddress = shiptoAddress.Address1 + shiptoAddress.Address2;
                    listdata.ShipToCity = shiptoAddress.City;
                    listdata.CustomerCity = shiptoAddress.City;
                    listdata.ShipToJurisdiction = shiptoAddress.StateProvince.Name;
                    listdata.ShipToPostalCode = shiptoAddress.ZipPostalCode;
                    listdata.CustomerPostalCode = shiptoAddress.ZipPostalCode;

                    listdata.ShipToCountry = shiptoAddress.Country.Name;
                    listdata.CustomerCountry = shiptoAddress.Country.Name;
                    listdata.ProductAmount = Convert.ToDecimal(orderdetails.PriceInclTax);
                    listdata.CostCenterNumber = dbEntity.Stores.Where(x => x.Id == orderDetails.StoreId).FirstOrDefault().ExtKey;//Convert.ToString(_storeContext.CurrentStore.ExtKey);
                    listdata.Status = status;
                    listdata.VertexCompanyCode = 0;
                    // listdata.VertexFiscalYear = response[0].AddedDate.Year; Not available
                    listdata.VertexDocumentType = "eCommerce";
                    listdata.DateOfTransaction = orderDetails.CreatedOnUtc;
                    // listdata.TaxAmountImposed = Convert.ToDecimal(dictResponse["totalTax"]); Not available
                    //if (dictResponse.ContainsKey("DeliveryTax"))
                    //    listdata.DeliveryTax = Convert.ToDecimal(dictResponse["DeliveryTax"]);
                    //if (dictResponse.ContainsKey("ShippingTax"))
                    //    listdata.ShippingTax = Convert.ToDecimal(dictResponse["ShippingTax"]);
                    //if (dictResponse.ContainsKey("ProductTax"))
                    //    listdata.ProductTax = Convert.ToDecimal(dictResponse["ProductTax"]);
                    listdata.InvoiceTotal = Convert.ToDecimal(orderdetails.PriceInclTax);
                    //listdata.InvoiceTax = Convert.ToDecimal(dictResponse["totalTax"]);

                    responseList.Add(listdata);
                }

            }
            catch (Exception ex)
            {
                SAPLogger.WriteLogs(string.Format("Failed to process records at getDistributiveDataResponseForOrder Method for Order No- {0} by exception-{1}", orderId, ex.Message));
            }

            return responseList;
        }

        private static List<SAPOrder> getSAPOrdersToBeProcessed()
        {

            var connStr = ConfigurationManager.ConnectionStrings["BlueDot"].ConnectionString;
            var reportList = new List<SAPOrder>();
            using (var conn = new SqlConnection(connStr))
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandType = CommandType.Text;

                //Section to fetch records that are fulfilled and Purchased after cutoff date.
                var sqlSelect = string.Format(@"select DISTINCT  O.ID,
                                                O.ID AS OrderId,
                                                sap.orderItemId,
                                                sap.StatusId,
                                                sap.IsProcessed,
                                                sap.SequenceNumber
                                                from SAPOrderProcess sap
                                                left outer join [order] o on o.id = sap.orderid
                                                inner join TaxResponseStorage TS on O.id=TS.OrderID
                                                where
                                                createdonutc >= '2020-02-01'
                                                and isprocessed = 0 and o.deleted=0 and o.storeid!=404 and o.storeid!=12");//and sap.orderid= 153868


                //Section to fetch records that are purchased before cutoff but fulfilled after cutoff.
                var sqlSelectFulfilled = (@"select DISTINCT O.ID,
                                            O.ID AS OrderId,
                                            sap.orderItemId,
                                            sap.StatusId,
                                            sap.IsProcessed,
                                            sap.SequenceNumber
                                            from SAPOrderProcess sap
                                            left outer join [order] o on o.id = sap.orderid
                                            inner join TaxResponseStorage TS on O.id=TS.OrderID
                                            inner join OrderItem OT on sap.OrderId=OT.OrderId and sap.OrderItemId=OT.Id 
                                            where
                                            O.createdonutc < '2020-02-01' and OT.FulfillmentDateTime>='2020-02-01' and sap.StatusId=2
                                            and isprocessed = 0 and o.deleted=0 and o.storeid!=404 and o.storeid!=12");

                //Section to fetch records that are Refunded before cutoff but fulfilled after cutoff.
                var sqlSelectRefund = (@"select DISTINCT O.ID,
                                        O.ID AS OrderId,
                                        sap.orderItemId,
                                        sap.StatusId,
                                        sap.IsProcessed,
                                        sap.SequenceNumber
                                        from SAPOrderProcess sap
                                        left outer join [order] o on o.id = sap.orderid
                                        inner join TaxResponseStorage TS on O.id=TS.OrderID
                                        inner join OrderItem OT on sap.OrderId=OT.OrderId and sap.OrderItemId=OT.Id 
                                        where
                                        O.createdonutc < '2020-02-01' and OT.DateOfRefund>='2020-02-01' and (sap.StatusId=3 or sap.StatusId=4)
                                        and isprocessed = 0 and o.deleted=0 and  o.storeid!=404 and o.storeid!=12");

                cmd.CommandText = sqlSelect;
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var order = new SAPOrder();
                        order.Id = ((int)reader["Id"]);
                        order.OrderId = ((int)reader["OrderId"]);
                        order.OrderItemId = ((int)reader["OrderItemId"]);
                        order.StatusID = ((int)reader["StatusId"]);
                        reportList.Add(order);
                    }
                   // return reportList;
                }
                cmd.CommandText = sqlSelectFulfilled;
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var order = new SAPOrder();
                        order.Id = ((int)reader["Id"]);
                        order.OrderId = ((int)reader["OrderId"]);
                        order.OrderItemId = ((int)reader["OrderItemId"]);
                        order.StatusID = ((int)reader["StatusId"]);

                        reportList.Add(order);
                    }
                }
                cmd.CommandText = sqlSelectRefund;
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var order = new SAPOrder();
                        order.Id = ((int)reader["Id"]);
                        order.OrderId = ((int)reader["OrderId"]);
                        order.OrderItemId = ((int)reader["OrderItemId"]);
                        order.StatusID = ((int)reader["StatusId"]);

                        reportList.Add(order);
                    }
                }
            }
            return reportList;
        }

        private static int getLastSequnceNumber(int orderid, int statusid, int orderitemid)
        {
            int seqNum = 0;
            var connStr = ConfigurationManager.ConnectionStrings["BlueDot"].ConnectionString;
            var reportList = new List<SAPOrder>();
            using (var conn = new SqlConnection(connStr))
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandType = CommandType.Text;
                var sqlSelect = string.Format(@"Select  SequenceNumber from SAPOrderProcess where OrderId={0} and StatusId={1} and OrderItemId={2}", orderid, statusid, orderitemid);
                cmd.CommandText = sqlSelect;
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader["SequenceNumber"] is DBNull)
                            seqNum = 0;
                        else
                            seqNum = Convert.ToInt32(reader["SequenceNumber"]);
                    }
                }
            }
            return seqNum;
        }

        public static List<SAPTaxResponseStorage> getTaxResponseDataFromDB(int orderId, int ProductId, int status)
        {
            List<SAPTaxResponseStorage> taxResponses = new List<SAPTaxResponseStorage>();


            var connStr = ConfigurationManager.ConnectionStrings["BlueDot"].ConnectionString;
            var reportList = new List<SAPOrder>();
            using (var conn = new SqlConnection(connStr))
            using (var cmd = conn.CreateCommand())
            {
                string sqlSelect = string.Empty;
                cmd.CommandType = CommandType.Text;
                if (status == 4 || status == 3)
                {
                    sqlSelect = string.Format(@"select * from TaxResponseStorage where  typeofCall = 9 AND  orderId = {0} AND ProductID={1}", orderId, ProductId);
                }
                else
                {
                    sqlSelect = string.Format(@"select * from TaxResponseStorage where  typeofCall = 3 AND  orderId = {0} AND ProductID={1}", orderId, ProductId);
                }
                cmd.CommandText = sqlSelect;
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var taxResponse = new SAPTaxResponseStorage();
                        taxResponse.CustomerId = ((int)reader["CustomerId"]);
                        taxResponse.OrderId = ((int)reader["OrderId"]);
                        taxResponse.ProductId = ((string)reader["ProductId"]);
                        taxResponse.XMLResponse = ((string)reader["XMLResponse"]);
                        taxResponse.AddedDate = ((DateTime)reader["AddedDate"]);
                        taxResponses.Add(taxResponse);
                    }
                }
            }
            return taxResponses;
        }

       

        public static List<SAPTaxResponseStorage> getTaxResponseDataFromDB(int orderId, int ProductId, int status, int orderItemId)
        {
            List<SAPTaxResponseStorage> taxResponses = new List<SAPTaxResponseStorage>();


            var connStr = ConfigurationManager.ConnectionStrings["BlueDot"].ConnectionString;
            var reportList = new List<SAPOrder>();
            using (var conn = new SqlConnection(connStr))
            using (var cmd = conn.CreateCommand())
            {
                string sqlSelect = string.Empty;
                cmd.CommandType = CommandType.Text;
                if (status == 4 || status == 3)
                {
                    sqlSelect = string.Format(@"select * from TaxResponseStorage where  typeofCall = 9 AND  orderId = {0} AND ProductID={1} AND OrderItemId={2}", orderId, ProductId, orderItemId);
                }
                else
                {
                    sqlSelect = string.Format(@"select * from TaxResponseStorage where  typeofCall = 3 AND  orderId = {0} AND ProductID={1} AND OrderItemId={2}", orderId, ProductId, orderItemId);
                }
                cmd.CommandText = sqlSelect;
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var taxResponse = new SAPTaxResponseStorage();
                        taxResponse.CustomerId = ((int)reader["CustomerId"]);
                        taxResponse.OrderId = ((int)reader["OrderId"]);
                        taxResponse.ProductId = ((string)reader["ProductId"]);
                        taxResponse.XMLResponse = ((string)reader["XMLResponse"]);
                        taxResponse.AddedDate = ((DateTime)reader["AddedDate"]);
                        taxResponses.Add(taxResponse);
                    }
                }
            }
            return taxResponses;
        }


        public static void UpdateSAPSOrderProcess(int orderid, int itemid, int status, int SequenceNum)
        {

            var connStr = ConfigurationManager.ConnectionStrings["BlueDot"].ConnectionString;
            using (var conn = new SqlConnection(connStr))
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandType = CommandType.Text;
                var sqlSelect = string.Format(@"
                       Update SAPOrderProcess set isProcessed = 1,SequenceNumber=" + SequenceNum + " where OrderId ={0} and OrderItemId={1} and StatusId={2}", orderid, itemid, status);
                cmd.CommandText = sqlSelect;
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public static void UpdateSAPSalesJournalOrderItem(int orderid, int itemid, int status)
        {

            var connStr = ConfigurationManager.ConnectionStrings["BlueDot"].ConnectionString;
            using (var conn = new SqlConnection(connStr))
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandType = CommandType.Text;
                var sqlSelect = string.Format(@"
                       Update SAP_SalesJournal set isExported = 1 where OrderNumber ={0} and OrderItemId={1} and Status={2}", orderid, itemid, status);
                cmd.CommandText = sqlSelect;
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }


        public static Dictionary<string, string> getDataFromResponseXML(string responseXml, string productName)
        {
            Dictionary<string, string> xmlDict = new Dictionary<string, string>();

            Dictionary<string, string> taxResultData = new Dictionary<string, string>();

            string ProductID = string.Empty;

            int i = 1;

            decimal quantity = decimal.Zero;

            decimal ExtendedPrice = decimal.Zero;

            decimal InputTotaltax = decimal.Zero;

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(responseXml);

            XmlNodeList nodeitem = xmlDoc.GetElementsByTagName("DistributeTaxResponse");

            foreach (XmlNode xmlnode in nodeitem)
            {
                string documentNo = xmlnode.Attributes["documentNumber"].Value;
                string docDate = xmlnode.Attributes["documentDate"].Value;
                xmlDict.Add("documentNumber", documentNo);
                xmlDict.Add("documentDate", docDate);
                int prodtax = 0;

                foreach (XmlNode childnode in xmlnode.ChildNodes)
                {

                    if (childnode.Name == "Seller")
                    {
                        string sellertaxareaID = childnode.FirstChild.NextSibling.NextSibling.Attributes["taxAreaId"].Value == null ? string.Empty : Convert.ToString(childnode.FirstChild.NextSibling.NextSibling.Attributes["taxAreaId"].Value);
                        string sellerStreetAddress = childnode.FirstChild.NextSibling.NextSibling.FirstChild.InnerText == null ? string.Empty : Convert.ToString(childnode.FirstChild.NextSibling.NextSibling.FirstChild.InnerText);
                        string sellerCity = childnode.FirstChild.NextSibling.NextSibling.FirstChild.NextSibling.InnerText == null ? string.Empty : Convert.ToString(childnode.FirstChild.NextSibling.NextSibling.FirstChild.NextSibling.InnerText);
                        string sellerState = childnode.FirstChild.NextSibling.NextSibling.FirstChild.NextSibling.NextSibling.InnerText == null ? string.Empty : Convert.ToString(childnode.FirstChild.NextSibling.NextSibling.FirstChild.NextSibling.NextSibling.InnerText);
                        string sellerPostalCode = childnode.FirstChild.NextSibling.NextSibling.FirstChild.NextSibling.NextSibling.NextSibling.InnerText == null ? string.Empty : Convert.ToString(childnode.FirstChild.NextSibling.NextSibling.FirstChild.NextSibling.NextSibling.NextSibling.InnerText);
                        string sellerCountry = childnode.FirstChild.NextSibling.NextSibling.FirstChild.NextSibling.NextSibling.NextSibling.NextSibling.InnerText == null ? string.Empty : Convert.ToString(childnode.FirstChild.NextSibling.NextSibling.FirstChild.NextSibling.NextSibling.NextSibling.NextSibling.InnerText);

                        xmlDict.Add("sellertaxareaID", sellertaxareaID);
                        xmlDict.Add("sellerStreetAddress", sellerStreetAddress);
                        xmlDict.Add("sellerCity", sellerCity);
                        xmlDict.Add("sellerState", sellerState);
                        xmlDict.Add("sellerPostalCode", sellerPostalCode);
                        xmlDict.Add("sellerCountry", sellerCountry);
                        continue;
                    }

                    if (childnode.Name == "Customer")
                    {
                        string customertaxareaID = childnode.FirstChild.NextSibling.Attributes["taxAreaId"] == null ? string.Empty : Convert.ToString(childnode.FirstChild.NextSibling.Attributes["taxAreaId"].Value);
                        string CustomerNumber = childnode.FirstChild.InnerText == null ? string.Empty : Convert.ToString(childnode.FirstChild.InnerText);
                        string customerStreetAddress1 = childnode.LastChild["StreetAddress1"] == null ? string.Empty : Convert.ToString(childnode.LastChild["StreetAddress1"].InnerText);
                        string customerStreetAddress2 = childnode.LastChild["StreetAddress2"] == null ? string.Empty : childnode.LastChild["StreetAddress2"].InnerText;
                        string customerCity = childnode.LastChild["City"] == null ? string.Empty : Convert.ToString(childnode.LastChild["City"].InnerText);
                        string customerState = childnode.LastChild["MainDivision"] == null ? string.Empty : Convert.ToString(childnode.LastChild["MainDivision"].InnerText);
                        string customerPostalCode = childnode.LastChild["PostalCode"] == null ? string.Empty : Convert.ToString(childnode.LastChild["PostalCode"].InnerText);
                        string customerCountry = childnode.LastChild["Country"] == null ? string.Empty : Convert.ToString(childnode.LastChild["Country"].InnerText);

                        xmlDict.Add("customertaxareaID", customertaxareaID);
                        xmlDict.Add("CustomerNumber", CustomerNumber);
                        xmlDict.Add("customerStreetAddress1", customerStreetAddress1);
                        xmlDict.Add("customerStreetAddress2", customerStreetAddress2);
                        xmlDict.Add("customerCity", customerCity);
                        xmlDict.Add("customerState", customerState);
                        xmlDict.Add("customerPostalCode", customerPostalCode);
                        xmlDict.Add("customerCountry", customerCountry);
                        continue;
                    }

                    if (childnode.Name == "SubTotal")
                    {
                        string ProductAmt = Convert.ToString(childnode.InnerText);
                        xmlDict.Add("ProductAmt", ProductAmt);
                        continue;
                    }

                    if (childnode.Name == "TotalTax")
                    {
                        string totalTax = Convert.ToString(childnode.InnerText);
                        xmlDict.Add("totalTax", totalTax);
                        continue;
                    }

                    if (childnode.Name == "Total")
                    {
                        string totalAmt = Convert.ToString(childnode.InnerText);
                        xmlDict.Add("totalAmt", totalAmt);

                    }

                    if (childnode.Name == "LineItem")
                    {

                        var vertexLineItemNumber = childnode.Attributes["lineItemNumber"].Value;


                        string subTaxResultData = "";
                        string calculatedTaxResultOutput = string.Empty;
                        string itemName = string.Empty;
                        int count = 0;
                        foreach (XmlNode xmlchildNode in childnode.ChildNodes)
                        {
                            if (xmlchildNode.Name == "Product")
                            {

                                ProductID = childnode.Attributes["lineItemId"] == null ? string.Empty : childnode.Attributes["lineItemId"].Value;

                            }

                            if (xmlchildNode.Name == "Quantity")
                            {
                                quantity = Convert.ToDecimal(xmlchildNode.InnerText);
                            }

                            if (xmlchildNode.Name == "ExtendedPrice")
                            {
                                ExtendedPrice = Convert.ToDecimal(xmlchildNode.InnerText);
                            }

                            if (xmlchildNode.Name == "InputTotalTax")
                            {
                                InputTotaltax = Convert.ToDecimal(xmlchildNode.InnerText);
                                if (childnode.InnerXml.Contains("DELV"))
                                {
                                    xmlDict.Add("DeliveryTax", Convert.ToString(InputTotaltax));
                                    itemName = "|" + productName + "-Delivery";
                                }
                                if (childnode.InnerXml.Contains("SHIP"))
                                {
                                    xmlDict.Add("ShippingTax", Convert.ToString(InputTotaltax));
                                    itemName = "|" + productName + "-Shipping";
                                }
                                else if (!childnode.InnerXml.Contains("DELV") && !childnode.InnerXml.Contains("SHIP"))
                                {
                                    //xmlDict.Add("ProductTax" + "_" + prodtax, Convert.ToString(InputTotaltax));
                                    //itemName = "|" + productName + "-Product";
                                    //prodtax++;
                                }
                            }

                            if (xmlchildNode.Name == "Taxes")
                            {
                                string taxResult = xmlchildNode.Attributes["taxResult"].Value;
                                string taxType = xmlchildNode.Attributes["taxType"].Value; // capturing taxtype
                                string impositionName = "";
                                string glCode = "";
                                string SummaryInvoiceText = string.Empty;
                                string jurisDictionId = string.Empty;

                                foreach (XmlNode taxNode in xmlchildNode.ChildNodes)
                                {
                                    string jurisdictionLevel = "";
                                    if (taxNode.Name == "Jurisdiction")
                                    {
                                        jurisdictionLevel = taxNode.Attributes["jurisdictionLevel"].Value;
                                        string jurisdiction = Convert.ToString(taxNode.InnerText);
                                        subTaxResultData = String.Concat(subTaxResultData, jurisdictionLevel + "|" + jurisdiction);
                                        jurisDictionId = taxNode.Attributes["jurisdictionId"].Value;
                                    }

                                    if (taxNode.Name == "CalculatedTax")
                                    {
                                        string calculatedTax = Convert.ToString(taxNode.InnerText);
                                        var ActualcalculatedTax = Convert.ToDecimal(calculatedTax);
                                        ActualcalculatedTax = ActualcalculatedTax < decimal.Zero ? -ActualcalculatedTax : ActualcalculatedTax;
                                        subTaxResultData = String.Concat(subTaxResultData, "|", ActualcalculatedTax);
                                        calculatedTaxResultOutput = Convert.ToString(ActualcalculatedTax);
                                        count++;
                                    }

                                    if (taxNode.Name == "EffectiveRate")
                                    {
                                        string effectiveRate = Convert.ToString(taxNode.InnerText);
                                        var ActualEffictiveRate = Convert.ToDecimal(effectiveRate);
                                        ActualEffictiveRate = ActualEffictiveRate < decimal.Zero ? -ActualEffictiveRate : ActualEffictiveRate;
                                        subTaxResultData = String.Concat(subTaxResultData, "|", ActualEffictiveRate);
                                    }

                                    if (taxNode.Name == "Taxable")
                                    {
                                        string taxableAmt = Convert.ToString(taxNode.InnerText);
                                        var ActualTaxableAmount = Convert.ToDecimal(taxableAmt);
                                        ActualTaxableAmount = ActualTaxableAmount < decimal.Zero ? -ActualTaxableAmount : ActualTaxableAmount;
                                        subTaxResultData = String.Concat(subTaxResultData, "|", ActualTaxableAmount, "|", taxResult);
                                    }

                                    if (taxNode.Name == "ImpositionType")
                                    {
                                        int impositionTypeId = Convert.ToInt32(taxNode.Attributes["impositionTypeId"].Value);
                                        subTaxResultData = String.Concat(subTaxResultData, "|", impositionTypeId, "|", impositionName);
                                        //  subTaxResultData = String.Concat(subTaxResultData, "|", impositionName);
                                    }

                                    if (taxNode.Name == "Imposition")
                                    {
                                        impositionName = Convert.ToString(taxNode.InnerText);
                                    }
                                    if (taxNode.Name == "SummaryInvoiceText")
                                    {
                                        SummaryInvoiceText = taxNode.InnerText;
                                    }
                                    if (taxNode.Name == "AssistedParameters") //Ryan added
                                    {

                                        foreach (XmlNode parameters in taxNode.ChildNodes)
                                        {
                                            if (SummaryInvoiceText == parameters.InnerText)
                                                glCode = Convert.ToString(parameters.InnerText);

                                        }
                                    }
                                }
                                xmlDict.Add("LineItem_" + ProductID + "_" + i, ProductID + "|" + vertexLineItemNumber + "|" + quantity + "|" + (ExtendedPrice < decimal.Zero ? -ExtendedPrice : ExtendedPrice) + "|" + (InputTotaltax < decimal.Zero ? -InputTotaltax : InputTotaltax) + "|" + subTaxResultData + "|" + glCode.Replace(".", "") + "|" + taxType + "|" + jurisDictionId + itemName);
                                xmlDict.Add("calculatedTaxResultOutput_" + ProductID + "_" + i, calculatedTaxResultOutput);
                                subTaxResultData = "";
                                i++;
                            }
                        }

                        i = 1;

                    }
                }
            }


            return xmlDict;
        }

        public static bool PostDistributiveTaxDataToSAPSalesJournal(List<DistributiveTaxResponse> response)
        {
            SapFileFeed DBEntity = new SapFileFeed();

            SAP_SalesJournal DBSPJournal = new SAP_SalesJournal();

            try
            {
                foreach (var responseItem in response)
                {
                    DBSPJournal.OrderNumber = responseItem.OrderNumber; //
                    int ProductId = Convert.ToInt32(responseItem.ProductID);
                    var orderItem = DBEntity.OrderItems.Where(x => x.OrderId == DBSPJournal.OrderNumber && x.ProductId == ProductId).FirstOrDefault();
                    if (orderItem != null)
                        DBSPJournal.OrderItemID = responseItem.OrderItemID;
                    else
                        DBSPJournal.OrderItemID = null;
                    DBSPJournal.CustomerNumber = responseItem.CustomerNumber;
                    DBSPJournal.Status = responseItem.Status;
                    DBSPJournal.SAPDocumentNumber = responseItem.VertexDocumentNumber;
                    DBSPJournal.SAPCompanyCode = responseItem.VertexCompanyCode;
                    DBSPJournal.SAPFiscalYear = responseItem.VertexFiscalYear;
                    DBSPJournal.SAPPostingDate = responseItem.VertexPostingDate;
                    DBSPJournal.SAPEntryDate = responseItem.VertexEntryDate;
                    DBSPJournal.SAPDocumentType = responseItem.VertexDocumentType;
                    DBSPJournal.CostCenterNumber = responseItem.CostCenterNumber;
                    DBSPJournal.DateOfTransaction = responseItem.DateOfTransaction;

                    DBSPJournal.ProductGL = RetrieveGLCode(Convert.ToString(ProductId));
                    DBSPJournal.ProductAmount = Convert.ToInt32(Math.Round(responseItem.ProductAmount, MidpointRounding.AwayFromZero));
                    DBSPJournal.CustomerNumber = responseItem.CustomerNumber;
                    DBSPJournal.CustomerName = responseItem.CustomerName;
                    DBSPJournal.CustomerStreetAddress = responseItem.CustomerStreetAddress;
                    DBSPJournal.CustomerCity = responseItem.CustomerCity;
                    DBSPJournal.CustomerPostalCode = responseItem.CustomerPostalCode;
                    DBSPJournal.CustomerCountry = responseItem.CustomerCountry;
                    DBSPJournal.ShipToName = responseItem.ShipToName;
                    DBSPJournal.ShipToStreetAddress = responseItem.ShipToStreetAddress;
                    DBSPJournal.ShipToCity = responseItem.ShipToCity;
                    DBSPJournal.ShipToPostalCode = responseItem.ShipToPostalCode;
                    DBSPJournal.ShipToCountry = responseItem.ShipToCountry;
                    DBSPJournal.ShipToJurisdiction = responseItem.ShipToJurisdiction;
                    DBSPJournal.ShipFromName = responseItem.ShipFromName;
                    DBSPJournal.ShipFromStreetAddress = responseItem.ShipFromStreetAddress;
                    DBSPJournal.ShipFromCity = responseItem.ShipFromCity;
                    DBSPJournal.ShipFromPostalCode = responseItem.ShipFromPostalCode;
                    DBSPJournal.ShipFromState = responseItem.ShipFromState;
                    DBSPJournal.ShipFromCountry = responseItem.ShipFromCountry;
                    var ProductDetail = DBEntity.Products.Where(x => x.Id == ProductId).FirstOrDefault();
                    DBSPJournal.LocalProduct = ProductDetail.Name;
                    if (ProductDetail.MasterId != 0)
                        DBSPJournal.MasterProduct = (DBEntity.Products.Where(x => x.Id == ProductDetail.MasterId).FirstOrDefault().Name == null ? "" : DBEntity.Products.Where(x => x.Id == ProductDetail.MasterId).FirstOrDefault().Name);
                    else
                        DBSPJournal.MasterProduct = "";
                    DBSPJournal.IsProductTaxable = ProductDetail.IsTaxExempt;
                    DBSPJournal.TaxAmountImposed = responseItem.TaxAmountImposed;
                    DBSPJournal.NonTexableAmount = 0; //Need to ask 
                    DBSPJournal.IsExported = false; //Need to ask
                    DBEntity.SAP_SalesJournal.Add(DBSPJournal);
                    DBEntity.SaveChanges();


                }

                return true;
            }
            catch (Exception ex)
            {
                SAPLogger.WriteLogs(String.Format("Failed to Process the Method PostDistributiveTaxDataToSAPSalesJournal for Order No- {0} for exception-{1}", DBSPJournal.OrderNumber, ex.Message));
                return false;
            }
        }

        public static void ConvertDtToFlatFileAndSend(DataTable dt)
        {
            StringBuilder data = ConvertDataTableToCsvFile(dt);
            CreateFolderForCSV(data);
        }

        public static StringBuilder ConvertDataTableToCsvFile(DataTable dtData)
        {
            StringBuilder data = new StringBuilder();

            //Taking the column names.
            for (int column = 0; column < dtData.Columns.Count; column++)
            {
                //Making sure that end of the line, shoould not have comma delimiter.
                if (column == dtData.Columns.Count - 1)
                    data.Append(dtData.Columns[column].ColumnName.ToString().Replace(",", ";"));
                else
                    data.Append(dtData.Columns[column].ColumnName.ToString().Replace(",", ";") + ',');
            }

            data.Append(Environment.NewLine);//New line after appending columns.

            for (int row = 0; row < dtData.Rows.Count; row++)
            {
                for (int column = 0; column < dtData.Columns.Count; column++)
                {
                    ////Making sure that end of the line, shoould not have comma delimiter.
                    if (column == dtData.Columns.Count - 1)
                        data.Append(dtData.Rows[row][column].ToString().Replace(",", ";"));
                    else
                        data.Append(dtData.Rows[row][column].ToString().Replace(",", ";") + ',');
                }

                //Making sure that end of the file, should not have a new line.
                if (row != dtData.Rows.Count - 1)
                    data.Append(Environment.NewLine);
            }
            return data;
        }

        public static void CreateFolderForCSV(StringBuilder data)
        {
            try
            {
                string appPath = ConfigurationManager.AppSettings["Filepath"];
                string FileName = "SAPJournalData_" + DateTime.Now.ToString("MM_dd_yyyy_HH_mm_ss") + ".csv";
                string FullPath = Path.Combine(appPath, FileName);

                File.Create(FullPath).Close();
                File.AppendAllText(FullPath, data.ToString());
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }
        }

        public static string RetrieveGLCode(string ProductID)
        {
            string GlCodeName = "";

            string query = "Select GlCodeName from [dbo].[GLCode] glc,[dbo].[Product_Category_Mapping] pcm,[dbo].[Category] c where pcm.ProductId = @productID AND c.Id = pcm.CategoryId AND glc.Description = c.Name AND glc.IsDelivered = 0";

            query = query.Replace("@productID", ProductID);

            var connStr = ConfigurationManager.ConnectionStrings["BlueDot"].ConnectionString;

            SqlConnection scon = new SqlConnection(connStr);

            SqlCommand cmd = new SqlCommand(query, scon);

            scon.Open();

            GlCodeName = Convert.ToString(cmd.ExecuteScalar());

            scon.Close();

            return GlCodeName;
        }

        public static string RetrieveGLCodeByStatus(int statusId, bool first = true, bool isTaxGl = false, bool isMealPlan = false,
            bool isVendor = false, bool IsDonation = false, bool IsDownload = false, int productId = 0, int orderid = 0, bool isDelivery = false, bool isShipping = false, int orderitemId = 0)
        {
            if (statusId == 0)
            {
                return "41102004";
            }

            if (statusId == 1)
            {

                if (first)
                {
                    return isVendor ? "48702041" : "48702040";
                }
                else
                {
                    if (isDelivery)
                        return "48702040";
                    if (isShipping)
                        return "48702040";
                }
            }

            if (statusId == 2)
            {
                if (isTaxGl)
                {
                    return "44571098";
                }
                if (isMealPlan || IsDonation || IsDownload)
                {
                    if (first)
                    {
                        return "64701000";
                    }
                    else
                    {
                        var orderitemDetails = dbEntity.OrderItems.Where(x => x.Id == orderid && x.ProductId == productId);
                        var itemsinPaidCode = dbEntity.Product_GlCode_Mapping.Where(x => x.ProductId == productId).ToList();
                        //USING THE JE TABLE SO THAT THE RECORDS TO STAY CONSISTENT
                        //var itemsinPaidCode = dbEntity.ProductGlCodeMappings.Where(x => x.OrderId == orderid && x.ProductID == orderitemId && x.Amount > decimal.Zero).ToList();
                        var ProductDetails = dbEntity.Products.Where(x => x.Id == productId).FirstOrDefault();
                        if (ProductDetails.TaxCategoryId != -1)
                        {
                            foreach (var items in itemsinPaidCode)
                            {
                                var glcodeDetails = dbEntity.GLCodes.Where(x => x.Id == items.GlCodeId).FirstOrDefault();
                                if (glcodeDetails == null)
                                    continue;
                                return glcodeDetails.GlCodeName;
                            }
                        }
                        else
                        {
                            int count = 0;
                            string glcodeDetailstoreturn = string.Empty;
                            foreach (var items in itemsinPaidCode)
                            {
                                count++;
                                var glcodeDetails = dbEntity.GLCodes.Where(x => x.Id == items.GlCodeId).FirstOrDefault();
                                glcodeDetailstoreturn = glcodeDetailstoreturn + glcodeDetails.GlCodeName + "_" + count + "|" + items.Amount + "|";
                            }
                            return glcodeDetailstoreturn.TrimEnd('|');
                        }

                    }
                }
                else if (!isMealPlan && !IsDownload && !IsDonation)
                {
                    //USING THE JE TABLE SO THAT THE RECORDS TO STAY CONSISTENT
                    var itemsinPaidCode = dbEntity.ProductGlCodeMappings.Where(x => x.OrderId == orderid && x.ProductID == orderitemId && x.Amount > decimal.Zero).ToList();
                    //var itemsinPaidCode = dbEntity.Product_GlCode_Mapping.Where(x => x.ProductId == productId).ToList();
                    var ProductDetails = dbEntity.Products.Where(x => x.Id == productId).FirstOrDefault();
                    if (!isDelivery && !isShipping)
                    {
                        if (ProductDetails.TaxCategoryId != -1)
                        {
                            foreach (var items in itemsinPaidCode)
                            {
                                // var glcodeDetails = dbEntity.GLCodes.Where(x => x.Id == items.GlCodeId && x.IsPaid == true && x.IsDelivered == false && x.GlCodeName != "70161010" && x.GlCodeName != "62610001").FirstOrDefault();
                                var glcodeDetails = items.Glcode;
                                if (glcodeDetails == null)
                                    continue;
                                return glcodeDetails;
                            }
                        }
                    }
                    if (isDelivery)
                    {
                        return "70161010";
                    }
                    if (isShipping)
                    {
                        return "62610001";
                    }
                }
                //else if (first)
                //{
                //    return "70895010";
                //}

                else
                {
                    return "48702040";
                }
            }

            return "";
            //string GlCodeName = "";

            //string query = "Select GlCodeName from [dbo].[GLCode] glc,[dbo].[Product_Category_Mapping] pcm,[dbo].[Category] c where pcm.ProductId = @productID AND c.Id = pcm.CategoryId AND glc.Description = c.Name AND glc.IsDelivered = 0";

            //query = query.Replace("@productID", ProductID);

            //var connStr = ConfigurationManager.ConnectionStrings["BlueDot"].ConnectionString;

            //SqlConnection scon = new SqlConnection(connStr);

            //SqlCommand cmd = new SqlCommand(query, scon);

            //scon.Open();

            //GlCodeName = Convert.ToString(cmd.ExecuteScalar());

            //scon.Close();

            //return GlCodeName;
        }

        public static Dictionary<string, decimal?> RetrievePartialRefundGls(int itemid)
        {
            var orderItemsDetails = dbEntity.OrderItems.Where(x => x.Id == itemid).FirstOrDefault();

            try
            {
                if (orderItemsDetails.FulfillmentDateTime == null)
                {

                    var partialTaxAndGlDetails = new Dictionary<string, decimal?>();
                    var DeliveryTaxAndGlDetails = new Dictionary<string, decimal?>();
                    decimal? AmountForCost = 0;

                    if (orderItemsDetails.Product.VendorId != 0)
                    {
                        if (!string.IsNullOrEmpty(orderItemsDetails.GLCodeName1) && orderItemsDetails.GLCodeName1 != "None" && (orderItemsDetails.RefundedGlAmount1 != null && orderItemsDetails.RefundedGlAmount1 != 0)) { partialTaxAndGlDetails.Add("48702041", orderItemsDetails.RefundedGlAmount1); }

                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(orderItemsDetails.GLCodeName1) && orderItemsDetails.GLCodeName1 != "None" && (orderItemsDetails.RefundedGlAmount1 != null && orderItemsDetails.RefundedGlAmount1 != 0)) { partialTaxAndGlDetails.Add("48702040", orderItemsDetails.RefundedGlAmount1); }
                    }
                    if (orderItemsDetails.IsDeliveryPickUp == true && orderItemsDetails.IsFullRefund == true) { if (Convert.ToDecimal(orderItemsDetails.DeliveryPickupAmount) != 0) { partialTaxAndGlDetails.Add("48702040" + "_D", Convert.ToDecimal(orderItemsDetails.DeliveryPickupAmount)); } }
                    if (orderItemsDetails.IsShipping == true && orderItemsDetails.IsFullRefund == true) { if (Convert.ToDecimal(orderItemsDetails.ShippingAmount) != 0) { partialTaxAndGlDetails.Add("48702040" + "_S", Convert.ToDecimal(orderItemsDetails.ShippingAmount)); } }


                    if (!string.IsNullOrEmpty(orderItemsDetails.TaxName1) && orderItemsDetails.TaxName1 != "None") { if (!partialTaxAndGlDetails.ContainsKey(Convert.ToString(orderItemsDetails.TaxName1.Split('-')[0]))) { partialTaxAndGlDetails.Add(Convert.ToString(orderItemsDetails.TaxName1.Split('-')[0]), -orderItemsDetails.RefundedTaxAmount1); } }
                    if (!string.IsNullOrEmpty(orderItemsDetails.TaxName2) && orderItemsDetails.TaxName2 != "None") { if (!partialTaxAndGlDetails.ContainsKey(Convert.ToString(orderItemsDetails.TaxName2.Split('-')[0]))) { partialTaxAndGlDetails.Add(Convert.ToString(orderItemsDetails.TaxName2.Split('-')[0]), -orderItemsDetails.RefundedTaxAmount2); } }
                    if (!string.IsNullOrEmpty(orderItemsDetails.TaxName3) && orderItemsDetails.TaxName3 != "None") { if (!partialTaxAndGlDetails.ContainsKey(Convert.ToString(orderItemsDetails.TaxName3.Split('-')[0]))) { partialTaxAndGlDetails.Add(Convert.ToString(orderItemsDetails.TaxName3.Split('-')[0]), -orderItemsDetails.RefundedTaxAmount3); } }



                    if (orderItemsDetails.IsDeliveryPickUp == true && orderItemsDetails.IsFullRefund == true) { if (orderItemsDetails.DeliveryPickupAmount != 0 && !DeliveryTaxAndGlDetails.ContainsKey("44571098")) { partialTaxAndGlDetails.Add("44571098" + "_D", -orderItemsDetails.DelliveryTax); AmountForCost = -orderItemsDetails.DelliveryTax; } }
                    if (orderItemsDetails.IsShipping == true && orderItemsDetails.IsFullRefund == true) { if (orderItemsDetails.ShippingAmount != 0 && !DeliveryTaxAndGlDetails.ContainsKey("44571098")) { partialTaxAndGlDetails.Add("44571098" + "_S", -orderItemsDetails.ShippingTax); AmountForCost = -orderItemsDetails.ShippingTax; } }

                    return partialTaxAndGlDetails;
                }
                else
                {
                    var PartialRefundTaxAndGlDetailsFulfilled = new Dictionary<string, decimal?>();


                    if (!string.IsNullOrEmpty(orderItemsDetails.GLCodeName1) && orderItemsDetails.GLCodeName1 != "None" && (orderItemsDetails.RefundedGlAmount1 != null && orderItemsDetails.RefundedGlAmount1 != 0)) { if (!PartialRefundTaxAndGlDetailsFulfilled.ContainsKey(Convert.ToString(orderItemsDetails.GLCodeName2 + "-1"))) { PartialRefundTaxAndGlDetailsFulfilled.Add(Convert.ToString(orderItemsDetails.GLCodeName1.Split('-')[0] + "_1"), orderItemsDetails.RefundedGlAmount1); } }
                    if (!string.IsNullOrEmpty(orderItemsDetails.GLCodeName2) && orderItemsDetails.GLCodeName2 != "None" && (orderItemsDetails.RefundedGlAmount2 != null && orderItemsDetails.RefundedGlAmount2 != 0)) { if (!PartialRefundTaxAndGlDetailsFulfilled.ContainsKey(Convert.ToString(orderItemsDetails.GLCodeName2 + "-2"))) { PartialRefundTaxAndGlDetailsFulfilled.Add(Convert.ToString(orderItemsDetails.GLCodeName2.Split('-')[0] + "_2"), orderItemsDetails.RefundedGlAmount2); } }
                    if (!string.IsNullOrEmpty(orderItemsDetails.GLCodeName3) && orderItemsDetails.GLCodeName3 != "None" && (orderItemsDetails.RefundedGlAmount3 != null && orderItemsDetails.RefundedGlAmount3 != 0)) { if (!PartialRefundTaxAndGlDetailsFulfilled.ContainsKey(Convert.ToString(orderItemsDetails.GLCodeName3 + "-3"))) { PartialRefundTaxAndGlDetailsFulfilled.Add(Convert.ToString(orderItemsDetails.GLCodeName3.Split('-')[0] + "_3"), orderItemsDetails.RefundedGlAmount3); } }


                    if (orderItemsDetails.IsDeliveryPickUp == true && orderItemsDetails.IsFullRefund == true) { if (Convert.ToDecimal(orderItemsDetails.DeliveryPickupAmount) != 0 && !PartialRefundTaxAndGlDetailsFulfilled.ContainsKey("70161010")) { PartialRefundTaxAndGlDetailsFulfilled.Add("70161010", Convert.ToDecimal(orderItemsDetails.DeliveryPickupAmount)); } }
                    if (orderItemsDetails.IsShipping == true && orderItemsDetails.IsFullRefund == true) { if (Convert.ToDecimal(orderItemsDetails.ShippingAmount) != 0 && !PartialRefundTaxAndGlDetailsFulfilled.ContainsKey("62610001")) { PartialRefundTaxAndGlDetailsFulfilled.Add("62610001", Convert.ToDecimal(orderItemsDetails.ShippingAmount)); } }


                    if (!string.IsNullOrEmpty(orderItemsDetails.TaxName1) && orderItemsDetails.TaxName1 != "None" && (orderItemsDetails.RefundedTaxAmount1 != null && orderItemsDetails.RefundedTaxAmount1 != 0)) { if (!PartialRefundTaxAndGlDetailsFulfilled.ContainsKey(Convert.ToString(orderItemsDetails.TaxName1 + "-1"))) { PartialRefundTaxAndGlDetailsFulfilled.Add(Convert.ToString(orderItemsDetails.TaxName1.Split('-')[0] + "_1"), -orderItemsDetails.RefundedTaxAmount1); } }
                    if (!string.IsNullOrEmpty(orderItemsDetails.TaxName2) && orderItemsDetails.TaxName2 != "None" && (orderItemsDetails.RefundedTaxAmount2 != null && orderItemsDetails.RefundedTaxAmount2 != 0)) { if (!PartialRefundTaxAndGlDetailsFulfilled.ContainsKey(Convert.ToString(orderItemsDetails.TaxName2 + "-2"))) { PartialRefundTaxAndGlDetailsFulfilled.Add(Convert.ToString(orderItemsDetails.TaxName2.Split('-')[0] + "_2"), -orderItemsDetails.RefundedTaxAmount2); } }
                    if (!string.IsNullOrEmpty(orderItemsDetails.TaxName3) && orderItemsDetails.TaxName3 != "None" && (orderItemsDetails.RefundedTaxAmount3 != null && orderItemsDetails.RefundedTaxAmount3 != 0)) { if (!PartialRefundTaxAndGlDetailsFulfilled.ContainsKey(Convert.ToString(orderItemsDetails.TaxName3 + "-3"))) { PartialRefundTaxAndGlDetailsFulfilled.Add(Convert.ToString(orderItemsDetails.TaxName3.Split('-')[0] + "_3"), -orderItemsDetails.RefundedTaxAmount3); } }


                    if (orderItemsDetails.IsDeliveryPickUp == true && orderItemsDetails.IsFullRefund == true) { if (Convert.ToDecimal(orderItemsDetails.DeliveryPickupAmount) != 0 && !PartialRefundTaxAndGlDetailsFulfilled.ContainsKey("44571098" + "_1")) { PartialRefundTaxAndGlDetailsFulfilled.Add("44571098" + "_1", Convert.ToDecimal(-orderItemsDetails.DelliveryTax)); } }
                    if (orderItemsDetails.IsShipping == true && orderItemsDetails.IsFullRefund == true) { if (Convert.ToDecimal(orderItemsDetails.ShippingAmount) != 0 && !PartialRefundTaxAndGlDetailsFulfilled.ContainsKey("44571098" + "_2")) { PartialRefundTaxAndGlDetailsFulfilled.Add("44571098" + "_2", Convert.ToDecimal(-orderItemsDetails.ShippingTax)); } }

                    return PartialRefundTaxAndGlDetailsFulfilled;
                }
            }
            catch (Exception)
            {

                SAPLogger.WriteLogs(string.Format("Failed to process record For SAP Related Products with Order No- {0} for Partial refund", orderItemsDetails.OrderId));

            }
            return null;
        }


        public static void UpdateSAPJournal(int OrderId)
        {
            // SAP_SalesJournal DBSPJournal = new SAP_SalesJournal();
            var connStr = ConfigurationManager.ConnectionStrings["BlueDot"].ConnectionString;
            using (var conn = new SqlConnection(connStr))
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandType = CommandType.Text;
                var sqlSelect = string.Format(@"
                       Update SAP_SalesJournal set IsExported = 1 where OrderNumber = " + OrderId);
                cmd.CommandText = sqlSelect;
                cmd.Parameters.Add("@CSVProcessedDate", SqlDbType.DateTime2).Value = DateTime.Now;
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }
}
