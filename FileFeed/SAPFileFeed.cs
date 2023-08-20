using FileFeed.SAP;
using FlatFile.FixedLength.Implementation;
using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tamir.SharpSsh.jsch;
using Tamir.SharpSsh;
using System.Xml;
using Tamir.SharpSsh.Wrappers;

using Nop.Plugin.Tax.Vertex.Helpers;

namespace FileFeed
{
    class SAPFileFeed
    {
        private static readonly ILog Logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public static SAPDBEntities dbEntity = new SAPDBEntities();

        public static void RunSAPFeed()
        {
            try
            {
                List<SAPOrder> orders = getSAPOrdersToBeProcessed();
                //   StringBuilder SbSMWData = new StringBuilder();
                SAPFile file = new SAPFile();
                file.Builder = new StringBuilder();
                file.Header = new StringBuilder();
                file.Trailer = new SAPTrailer();
                //Header


                var header = new SAPHeader();
                header.FileDate = DateTime.Now.ToString("yyyyMMdd");
                file.Header.AppendLine(header.HeaderLiteral + "|" + header.FileDate);

                //BODY
                foreach (var order in orders)
                {


                    List<DistributiveTaxResponse> response = getDistributiveDataResponseForOrder(order.OrderId, order.StatusID);
                    if (response.Count > 0)
                    {

                        bool insertedintoSAPJournal = PostDistributiveTaxDataToSAPSalesJournal(response);
                        if (insertedintoSAPJournal)
                        {
                            List<SAPSalesJournalEntry> entries = getSAPDatabaseResults(order.OrderId, order.StatusID);

                            if (entries.Count == 0)
                            {
                                Console.WriteLine("No record found for today");
                                continue;
                            }

                            if (order.StatusID == 2 || order.StatusID == 4)
                            {
                                file = BuildFlatFile(file, entries, response, true);
                            }
                            else //status 1 or 3 don't need vertex
                            {
                                file = BuildFlatFile(file, entries, response, false);
                            }
                        }
                        else
                        {
                            Console.WriteLine("Error in inserting record into SAP");
                            continue;
                        }
                    }

                }




                StringBuilder completeFile = new StringBuilder();
                completeFile.Append(file.Header);
                completeFile.Append(file.Builder);



                ////Trailer Record
                var trailer = file.Trailer;
                trailer.Count = file.TrailerCount.ToString().PadLeft(10, '0');
                trailer.AmountSign = (file.TrailerTotal < 0 ? "-" : "");
                trailer.Amount = Convert.ToString(file.TrailerTotal).Replace(".", "");
                completeFile.AppendLine(trailer.TrailerLiteral + "|" + trailer.Count + "|" + trailer.AmountSign + "|0");


                CreateFlatFile(completeFile.ToString());
            }
            catch (Exception ex)
            {
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

        public static SAPFile BuildFlatFile(SAPFile file, List<SAPSalesJournalEntry> entries, List<DistributiveTaxResponse> response, bool isVertexLineNeeded)
        {
            StringBuilder sb = new StringBuilder();
            int trailerCount = 0;
            decimal trailerTotal = decimal.Zero;
            int flag = 1, m = 1, flag1 = 1, n = 1;
            //   i++;

            var Recordstatus = entries.Select(x => x.Status).Distinct().OrderBy(x => x).ToList();

            foreach (var status in Recordstatus)
            {
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
                    //Financial Record
                    if (plan.Status == "3")
                    {
                        sb.AppendLine(plan.RecordIndicator + "|" + plan.OrderNumber + "|" + plan.Status + "|" + m.ToString().PadLeft(3, '0') + "|" + plan.TransactionDate.ToString("MMddyyyy") + "|" + plan.CostCenterNumber + "|" + RetrieveGLCodeByStatus(3, true) + "|" + plan.AmountSign + "|" + decimal.Negate(plan.ProductAmount).ToString("N2").Replace(".", ""));
                    }
                    else if (plan.Status == "1")
                    {
                        sb.AppendLine(plan.RecordIndicator + "|" + plan.OrderNumber + "|" + plan.Status + "|" + m.ToString().PadLeft(3, '0') + "|" + plan.TransactionDate.ToString("MMddyyyy") + "|" + plan.CostCenterNumber + "|" + RetrieveGLCodeByStatus(1, true) + "|" + plan.AmountSign + "|" + plan.ProductAmount.ToString("N2").Replace(".", ""));
                        m++;
                        sb.AppendLine(plan.RecordIndicator + "|" + plan.OrderNumber + "|" + plan.Status + "|" + m.ToString().PadLeft(3, '0') + "|" + plan.TransactionDate.ToString("MMddyyyy") + "|" + plan.CostCenterNumber + "|" + RetrieveGLCodeByStatus(1, false) + "|-|" + plan.ProductAmount.ToString("N2").Replace(".", ""));
                        trailerCount++;
                    }
                    else if (plan.Status == "2")
                    {
                        sb.AppendLine(plan.RecordIndicator + "|" + plan.OrderNumber + "|" + plan.Status + "|" + m.ToString().PadLeft(3, '0') + "|" + plan.TransactionDate.ToString("MMddyyyy") + "|" + plan.CostCenterNumber + "|" + RetrieveGLCodeByStatus(2, true) + "|" + plan.AmountSign + "|" + plan.ProductAmount.ToString("N2").Replace(".", ""));
                        m++;
                        trailerCount++;
                        if (plan.TaxAmountImposed > 0)
                        {
                            sb.AppendLine(plan.RecordIndicator + "|" + plan.OrderNumber + "|" + plan.Status + "|" + m.ToString().PadLeft(3, '0') + "|" + plan.TransactionDate.ToString("MMddyyyy") + "|" + plan.CostCenterNumber + "|" + RetrieveGLCodeByStatus(2, true, true) + "|" + plan.AmountSign + "|" + plan.TaxAmountImposed.ToString("N2").Replace(".", "")); //tax line
                            m++;
                            trailerCount++;
                        }
                        sb.AppendLine(plan.RecordIndicator + "|" + plan.OrderNumber + "|" + plan.Status + "|" + m.ToString().PadLeft(3, '0') + "|" + plan.TransactionDate.ToString("MMddyyyy") + "|" + plan.CostCenterNumber + "|" + RetrieveGLCodeByStatus(2, false) + "|-|" + (plan.ProductAmount + plan.TaxAmountImposed).ToString("N2").Replace(".", ""));
                    }
                    else if (plan.Status == "3")
                    {
                        sb.AppendLine(plan.RecordIndicator + "|" + plan.OrderNumber + "|" + plan.Status + "|" + m.ToString().PadLeft(3, '0') + "|" + plan.TransactionDate.ToString("MMddyyyy") + "|" + plan.CostCenterNumber + "|" + RetrieveGLCodeByStatus(1, false) + "|" + plan.AmountSign + "|" + plan.ProductAmount.ToString("N2").Replace(".", ""));
                        m++;
                        sb.AppendLine(plan.RecordIndicator + "|" + plan.OrderNumber + "|" + plan.Status + "|" + m.ToString().PadLeft(3, '0') + "|" + plan.TransactionDate.ToString("MMddyyyy") + "|" + plan.CostCenterNumber + "|" + RetrieveGLCodeByStatus(1, true) + "|-|" + plan.ProductAmount.ToString("N2").Replace(".", ""));
                        trailerCount++;
                    }
                    else if (plan.Status == "4")
                    {
                        sb.AppendLine(plan.RecordIndicator + "|" + plan.OrderNumber + "|" + plan.Status + "|" + m.ToString().PadLeft(3, '0') + "|" + plan.TransactionDate.ToString("MMddyyyy") + "|" + plan.CostCenterNumber + "|" + RetrieveGLCodeByStatus(2, true) + "|" + plan.AmountSign + "|" + plan.ProductAmount.ToString("N2").Replace(".", ""));
                        m++;
                        trailerCount++;
                        if (plan.TaxAmountImposed > 0)
                        {
                            sb.AppendLine(plan.RecordIndicator + "|" + plan.OrderNumber + "|" + plan.Status + "|" + m.ToString().PadLeft(3, '0') + "|" + plan.TransactionDate.ToString("MMddyyyy") + "|" + plan.CostCenterNumber + "|" + RetrieveGLCodeByStatus(2, true, true) + "|" + plan.AmountSign + "|" + plan.TaxAmountImposed.ToString("N2").Replace(".", ""));
                            m++;
                            trailerCount++;
                        }
                        sb.AppendLine(plan.RecordIndicator + "|" + plan.OrderNumber + "|" + plan.Status + "|" + m.ToString().PadLeft(3, '0') + "|" + plan.TransactionDate.ToString("MMddyyyy") + "|" + plan.CostCenterNumber + "|" + RetrieveGLCodeByStatus(2, false) + "|-|" + (plan.ProductAmount + plan.TaxAmountImposed).ToString("N2").Replace(".", ""));
                    }
                    else
                        sb.AppendLine(plan.RecordIndicator + "|" + plan.OrderNumber + "|" + plan.Status + "|" + m.ToString().PadLeft(3, '0') + "|" + plan.TransactionDate.ToString("MMddyyyy") + "|" + plan.CostCenterNumber + "|" + RetrieveGLCodeByStatus(1, true) + "|" + plan.AmountSign + "|" + plan.ProductAmount.ToString("N2").Replace(".", ""));


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
                        sb.AppendLine("S" + "|" + plan.OrderNumber + "|" + plan.Status + "|" + n.ToString().PadLeft(3, '0') + "|" + plan.CustomerNumber + "|" + plan.CustomerName + "|" + plan.CustomerAddress + "|" + plan.CustomerCity + "|" + plan.CustomerState + "|" + plan.CustomerPostalCode + "|" + plan.CustomerCountry + "|" + plan.CustomerJurisdictionCode + "|" + plan.ShipToName + "|" + plan.ShipToStreetAddress + "|" + plan.ShipToCity + "|" + plan.ShipToState + "|" + plan.ShipToPostalCode + "|" + plan.ShipToCountry + "|" + plan.ShipToJurisdictionCode + "|" + plan.ShipFromName + "|" + plan.ShipFromAddress + "|" + plan.ShipFromCity + "|" + plan.ShipFromState + "|" + plan.ShipFromPostalCode + "|" + plan.ShipFromCountry + "|" + plan.ShipFromJurisdictionCode + "|" + plan.EcommMasterProductDescription + "|" + plan.EcommLocalProductDescription);
                        trailerCount++;
                        n++;
                        sb.AppendLine("S" + "|" + plan.OrderNumber + "|" + plan.Status + "|" + n.ToString().PadLeft(3, '0') + "|" + plan.CustomerNumber + "|" + plan.CustomerName + "|" + plan.CustomerAddress + "|" + plan.CustomerCity + "|" + plan.CustomerState + "|" + plan.CustomerPostalCode + "|" + plan.CustomerCountry + "|" + plan.CustomerJurisdictionCode + "|" + plan.ShipToName + "|" + plan.ShipToStreetAddress + "|" + plan.ShipToCity + "|" + plan.ShipToState + "|" + plan.ShipToPostalCode + "|" + plan.ShipToCountry + "|" + plan.ShipToJurisdictionCode + "|" + plan.ShipFromName + "|" + plan.ShipFromAddress + "|" + plan.ShipFromCity + "|" + plan.ShipFromState + "|" + plan.ShipFromPostalCode + "|" + plan.ShipFromCountry + "|" + plan.ShipFromJurisdictionCode + "|" + plan.EcommMasterProductDescription + "|" + plan.EcommLocalProductDescription);
                        trailerCount++;
                    }
                    else if (plan.Status == "2")
                    {
                        sb.AppendLine("S" + "|" + plan.OrderNumber + "|" + plan.Status + "|" + n.ToString().PadLeft(3, '0') + "|" + plan.CustomerNumber + "|" + plan.CustomerName + "|" + plan.CustomerAddress + "|" + plan.CustomerCity + "|" + plan.CustomerState + "|" + plan.CustomerPostalCode + "|" + plan.CustomerCountry + "|" + plan.CustomerJurisdictionCode + "|" + plan.ShipToName + "|" + plan.ShipToStreetAddress + "|" + plan.ShipToCity + "|" + plan.ShipToState + "|" + plan.ShipToPostalCode + "|" + plan.ShipToCountry + "|" + plan.ShipToJurisdictionCode + "|" + plan.ShipFromName + "|" + plan.ShipFromAddress + "|" + plan.ShipFromCity + "|" + plan.ShipFromState + "|" + plan.ShipFromPostalCode + "|" + plan.ShipFromCountry + "|" + plan.ShipFromJurisdictionCode + "|" + plan.EcommMasterProductDescription + "|" + plan.EcommLocalProductDescription);
                        trailerCount++;
                        n++;

                        if (plan.TaxAmountImposed > 0)
                        {
                            sb.AppendLine("S" + "|" + plan.OrderNumber + "|" + plan.Status + "|" + n.ToString().PadLeft(3, '0') + "|" + plan.CustomerNumber + "|" + plan.CustomerName + "|" + plan.CustomerAddress + "|" + plan.CustomerCity + "|" + plan.CustomerState + "|" + plan.CustomerPostalCode + "|" + plan.CustomerCountry + "|" + plan.CustomerJurisdictionCode + "|" + plan.ShipToName + "|" + plan.ShipToStreetAddress + "|" + plan.ShipToCity + "|" + plan.ShipToState + "|" + plan.ShipToPostalCode + "|" + plan.ShipToCountry + "|" + plan.ShipToJurisdictionCode + "|" + plan.ShipFromName + "|" + plan.ShipFromAddress + "|" + plan.ShipFromCity + "|" + plan.ShipFromState + "|" + plan.ShipFromPostalCode + "|" + plan.ShipFromCountry + "|" + plan.ShipFromJurisdictionCode + "|Tax|Tax");
                            trailerCount++;
                            n++;
                        }


                        sb.AppendLine("S" + "|" + plan.OrderNumber + "|" + plan.Status + "|" + n.ToString().PadLeft(3, '0') + "|" + plan.CustomerNumber + "|" + plan.CustomerName + "|" + plan.CustomerAddress + "|" + plan.CustomerCity + "|" + plan.CustomerState + "|" + plan.CustomerPostalCode + "|" + plan.CustomerCountry + "|" + plan.CustomerJurisdictionCode + "|" + plan.ShipToName + "|" + plan.ShipToStreetAddress + "|" + plan.ShipToCity + "|" + plan.ShipToState + "|" + plan.ShipToPostalCode + "|" + plan.ShipToCountry + "|" + plan.ShipToJurisdictionCode + "|" + plan.ShipFromName + "|" + plan.ShipFromAddress + "|" + plan.ShipFromCity + "|" + plan.ShipFromState + "|" + plan.ShipFromPostalCode + "|" + plan.ShipFromCountry + "|" + plan.ShipFromJurisdictionCode + "|" + plan.EcommMasterProductDescription + "|" + plan.EcommLocalProductDescription);
                        trailerCount++;
                    }
                    else if (plan.Status == "3")
                    {
                        sb.AppendLine("S" + "|" + plan.OrderNumber + "|" + plan.Status + "|" + n.ToString().PadLeft(3, '0') + "|" + plan.CustomerNumber + "|" + plan.CustomerName + "|" + plan.CustomerAddress + "|" + plan.CustomerCity + "|" + plan.CustomerState + "|" + plan.CustomerPostalCode + "|" + plan.CustomerCountry + "|" + plan.CustomerJurisdictionCode + "|" + plan.ShipToName + "|" + plan.ShipToStreetAddress + "|" + plan.ShipToCity + "|" + plan.ShipToState + "|" + plan.ShipToPostalCode + "|" + plan.ShipToCountry + "|" + plan.ShipToJurisdictionCode + "|" + plan.ShipFromName + "|" + plan.ShipFromAddress + "|" + plan.ShipFromCity + "|" + plan.ShipFromState + "|" + plan.ShipFromPostalCode + "|" + plan.ShipFromCountry + "|" + plan.ShipFromJurisdictionCode + "|" + plan.EcommMasterProductDescription + "|" + plan.EcommLocalProductDescription);
                        trailerCount++;
                        n++;
                        sb.AppendLine("S" + "|" + plan.OrderNumber + "|" + plan.Status + "|" + n.ToString().PadLeft(3, '0') + "|" + plan.CustomerNumber + "|" + plan.CustomerName + "|" + plan.CustomerAddress + "|" + plan.CustomerCity + "|" + plan.CustomerState + "|" + plan.CustomerPostalCode + "|" + plan.CustomerCountry + "|" + plan.CustomerJurisdictionCode + "|" + plan.ShipToName + "|" + plan.ShipToStreetAddress + "|" + plan.ShipToCity + "|" + plan.ShipToState + "|" + plan.ShipToPostalCode + "|" + plan.ShipToCountry + "|" + plan.ShipToJurisdictionCode + "|" + plan.ShipFromName + "|" + plan.ShipFromAddress + "|" + plan.ShipFromCity + "|" + plan.ShipFromState + "|" + plan.ShipFromPostalCode + "|" + plan.ShipFromCountry + "|" + plan.ShipFromJurisdictionCode + "|" + plan.EcommMasterProductDescription + "|" + plan.EcommLocalProductDescription);
                        trailerCount++;
                    }

                    else if (plan.Status == "4")
                    {
                        sb.AppendLine("S" + "|" + plan.OrderNumber + "|" + plan.Status + "|" + n.ToString().PadLeft(3, '0') + "|" + plan.CustomerNumber + "|" + plan.CustomerName + "|" + plan.CustomerAddress + "|" + plan.CustomerCity + "|" + plan.CustomerState + "|" + plan.CustomerPostalCode + "|" + plan.CustomerCountry + "|" + plan.CustomerJurisdictionCode + "|" + plan.ShipToName + "|" + plan.ShipToStreetAddress + "|" + plan.ShipToCity + "|" + plan.ShipToState + "|" + plan.ShipToPostalCode + "|" + plan.ShipToCountry + "|" + plan.ShipToJurisdictionCode + "|" + plan.ShipFromName + "|" + plan.ShipFromAddress + "|" + plan.ShipFromCity + "|" + plan.ShipFromState + "|" + plan.ShipFromPostalCode + "|" + plan.ShipFromCountry + "|" + plan.ShipFromJurisdictionCode + "|" + plan.EcommMasterProductDescription + "|" + plan.EcommLocalProductDescription);
                        trailerCount++;
                        n++;
                        sb.AppendLine("S" + "|" + plan.OrderNumber + "|" + plan.Status + "|" + n.ToString().PadLeft(3, '0') + "|" + plan.CustomerNumber + "|" + plan.CustomerName + "|" + plan.CustomerAddress + "|" + plan.CustomerCity + "|" + plan.CustomerState + "|" + plan.CustomerPostalCode + "|" + plan.CustomerCountry + "|" + plan.CustomerJurisdictionCode + "|" + plan.ShipToName + "|" + plan.ShipToStreetAddress + "|" + plan.ShipToCity + "|" + plan.ShipToState + "|" + plan.ShipToPostalCode + "|" + plan.ShipToCountry + "|" + plan.ShipToJurisdictionCode + "|" + plan.ShipFromName + "|" + plan.ShipFromAddress + "|" + plan.ShipFromCity + "|" + plan.ShipFromState + "|" + plan.ShipFromPostalCode + "|" + plan.ShipFromCountry + "|" + plan.ShipFromJurisdictionCode + "|" + plan.EcommMasterProductDescription + "|" + plan.EcommLocalProductDescription);
                        trailerCount++;
                    }



                }

                if (isVertexLineNeeded)
                {
                    int vertexStatus = Convert.ToInt32(status);

                    //Vertex Record
                    var VertexRecordonStatus = response.Where(x => x.Status == vertexStatus).OrderBy(x => x.OrderNumber).ToList();
                    foreach (DistributiveTaxResponse plan in VertexRecordonStatus)
                    {
                        int j = 1;
                        foreach (var lineItem in plan.LineItems)
                        {
                            string productId = lineItem.Split('|').ToArray()[0];
                            if (plan.ProductID == productId)
                            {
                                string lineItemData = lineItem.Replace(lineItem.Split('|').First() + "|", "");
                                sb.AppendLine("V" + "|" + plan.OrderNumber + "|" + plan.Status + "|" + j.ToString().PadLeft(3, '0') + "|" + plan.VertexPostingDate.ToString("MMddyyyy") + "|" + plan.InvoiceSubtotal + "|" + plan.InvoiceTotal + "|" + plan.InvoiceTax + "|" + lineItemData);
                                trailerCount++;
                            }
                            j++;
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
                       select s.extkey, oi.PriceExclTax, oi.DeliveryAmountExclTax, sap.*, o.* from sap_salesjournal sap
                        inner join [Order] o
                        on o.id = sap.OrderNumber
                        inner join OrderItem oi
                        on oi.id = sap.OrderItemId
                        inner join Store s
                        on o.storeid = s.id
                        
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
                            if (entry.Status == "3" || entry.Status == "4") //refunds & cancels
                            {
                                entry.AmountSign = "-";
                                entry.ProductAmount = ((int)reader["ProductAmount"] * (-1));
                            }
                            else if (entry.Status == "1") //should be total order.
                            {
                                entry.AmountSign = null;
                                entry.ProductAmount = ((decimal)reader["OrderTotal"]);
                            }
                            else
                            {
                                entry.AmountSign = null;
                                entry.ProductAmount = ((int)reader["ProductAmount"]);
                            }

                            entry.CostCenterNumber = (reader["CostCenterNumber"] == DBNull.Value ? "" : (string)reader["CostCenterNumber"]).Trim();
                            entry.GLAccount = (reader["ProductGL"] == DBNull.Value ? "" : (string)reader["ProductGL"]);
                            entry.OrderNumber = ((int)reader["OrderNumber"]);


                            // mealPlan.LineNumber = i.ToString();                                                       
                            entry.RecordIndicator = "F";
                            entry.TransactionDate = ((DateTime)reader["SAPEntryDate"]);
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
                            //entry.ShipFromState = reader["ShipFromState"].ToString();
                            entry.ShipFromPostalCode = reader["ShipFromPostalCode"].ToString();
                            entry.ShipFromCountry = reader["ShipFromCountry"].ToString();
                            // entry.ShipFromJurisdictionCode = reader["ShipFromState"].ToString();
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

        public static List<DistributiveTaxResponse> getDistributiveDataResponseForOrder(int orderId, int status)
        {
            List<DistributiveTaxResponse> responseList = new List<DistributiveTaxResponse>();
            try
            {

                var response = getTaxResponseDataFromDB(orderId);

                if (response.Count > 0)
                {
                    string[] ProductID = response[0].ProductId.Split('|');

                    string responseXml = response[0].XMLResponse.ToString();

                    Dictionary<string, string> dictResponse = getDataFromResponseXML(responseXml);

                    for (int i = 0; i < ProductID.Count(); i++)
                    {
                        DistributiveTaxResponse listdata = new DistributiveTaxResponse();

                        listdata.CustomerNumber = response[0].CustomerId;
                        listdata.OrderNumber = Convert.ToInt32(response[0].OrderId);
                        listdata.ProductID = response[0].ProductId.Split('|')[i];
                        listdata.VertexPostingDate = response[0].AddedDate;
                        listdata.VertexEntryDate = response[0].AddedDate;
                        string lineProductId = "LineItem_" + listdata.ProductID;
                        //var lineItemData = dictResponse.Where(kvp => kvp.Key.Contains(lineProductId)).ToList();
                        var lineItemData = dictResponse.Where(kvp => kvp.Key.Contains(listdata.ProductID)).ToList();
                        if (listdata.LineItems == null)
                        {
                            listdata.LineItems = new List<string>();
                        }
                        foreach (var item in lineItemData)
                        {
                            listdata.LineItems.Add(item.Value);
                        }

                        listdata.VertexDocumentNumber = 12345678;//Convert.ToInt32(dictResponse["documentNumber"]);
                        listdata.VertexPostingDate = Convert.ToDateTime(dictResponse["documentDate"]);
                        var orderDetails = dbEntity.Orders.Where(x => x.Id == listdata.OrderNumber).FirstOrDefault();

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


                        listdata.InvoiceSubtotal = Convert.ToDecimal(dictResponse["ProductAmt"]);
                        listdata.InvoiceTotal = Convert.ToDecimal(dictResponse["totalAmt"]);
                        listdata.InvoiceTax = Convert.ToDecimal(dictResponse["totalTax"]);


                        responseList.Add(listdata);
                    }
                }

            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
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
                var sqlSelect = string.Format(@"select o.id, * from SAPOrderProcess sap
left outer join [order] o on o.id = sap.orderid
where createdonutc > '2019-01-01' and isprocessed = 0");

                //                var sqlSelect = string.Format(@"select DISTINCT O.ID, O.ID AS OrderId,sap.orderItemId,sap.StatusId from SAPOrderProcess sap
                //left outer join [order] o on o.id = sap.orderid
                //inner join TaxResponseStorage TS on O.id=TS.OrderID
                //where createdonutc > '2019-04-12' and isprocessed = 0  ");
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
                }
            }
            return reportList;
        }

        public static List<SAPTaxResponseStorage> getTaxResponseDataFromDB(int orderId)
        {
            List<SAPTaxResponseStorage> taxResponses = new List<SAPTaxResponseStorage>();


            var connStr = ConfigurationManager.ConnectionStrings["BlueDot"].ConnectionString;
            var reportList = new List<SAPOrder>();
            using (var conn = new SqlConnection(connStr))
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandType = CommandType.Text;
                var sqlSelect = string.Format(@"select * from TaxResponseStorage where  typeofCall = 3 AND  orderId = " + orderId);
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

        public static Dictionary<string, string> getDataFromResponseXML(string responseXml)
        {
            Dictionary<string, string> xmlDict = new Dictionary<string, string>();

            Dictionary<string, string> taxResultData = new Dictionary<string, string>();

            int ProductID = 0;

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
                foreach (XmlNode childnode in xmlnode.ChildNodes)
                {
                    if (childnode.Name == "Seller")
                    {
                        string sellertaxareaID = childnode.FirstChild.Attributes["taxAreaId"] == null ? string.Empty : Convert.ToString(childnode.FirstChild.Attributes["taxAreaId"].Value);
                        string sellerStreetAddress = childnode.FirstChild.ChildNodes[0] == null ? string.Empty : Convert.ToString(childnode.FirstChild.ChildNodes[0].InnerText);
                        string sellerCity = childnode.FirstChild.ChildNodes[1] == null ? string.Empty : Convert.ToString(childnode.FirstChild.ChildNodes[1].InnerText);
                        string sellerState = childnode.FirstChild.ChildNodes[2] == null ? string.Empty : Convert.ToString(childnode.FirstChild.ChildNodes[2].InnerText);
                        string sellerPostalCode = childnode.FirstChild.ChildNodes[3] == null ? string.Empty : Convert.ToString(childnode.FirstChild.ChildNodes[3].InnerText);
                        string sellerCountry = childnode.FirstChild.ChildNodes[4] == null ? string.Empty : Convert.ToString(childnode.FirstChild.ChildNodes[4].InnerText);

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
                        foreach (XmlNode xmlchildNode in childnode.ChildNodes)
                        {
                            if (xmlchildNode.Name == "Product")
                            {

                                ProductID = childnode.Attributes["lineItemId"] == null ? 0 : Convert.ToInt32(childnode.Attributes["lineItemId"].Value);
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
                            }

                            if (xmlchildNode.Name == "Taxes")
                            {
                                string taxResult = xmlchildNode.Attributes["taxResult"].Value;
                                string impositionName = "";
                                string glCode = "";

                                foreach (XmlNode taxNode in xmlchildNode.ChildNodes)
                                {
                                    string jurisdictionLevel = "";
                                    if (taxNode.Name == "Jurisdiction")
                                    {
                                        jurisdictionLevel = taxNode.Attributes["jurisdictionLevel"].Value;
                                        string jurisdiction = Convert.ToString(taxNode.InnerText);
                                        subTaxResultData = String.Concat(subTaxResultData, jurisdictionLevel + "|" + jurisdiction);
                                    }

                                    if (taxNode.Name == "CalculatedTax")
                                    {
                                        string calculatedTax = Convert.ToString(taxNode.InnerText);
                                        subTaxResultData = String.Concat(subTaxResultData, "|", calculatedTax);
                                    }

                                    if (taxNode.Name == "EffectiveRate")
                                    {
                                        string effectiveRate = Convert.ToString(taxNode.InnerText);
                                        subTaxResultData = String.Concat(subTaxResultData, "|", effectiveRate);
                                    }

                                    if (taxNode.Name == "Taxable")
                                    {
                                        string taxableAmt = Convert.ToString(taxNode.InnerText);
                                        subTaxResultData = String.Concat(subTaxResultData, "|", taxableAmt, "|", taxResult);
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

                                    if (taxNode.Name == "AssistedParameters") //Ryan added
                                    {
                                        foreach (XmlNode parameters in taxNode.ChildNodes)
                                        {

                                            glCode = Convert.ToString(taxNode.InnerText);
                                        }
                                    }
                                }
                                xmlDict.Add("LineItem_" + ProductID + "_" + i, ProductID + "|" + vertexLineItemNumber + "|" + quantity + "|" + ExtendedPrice + "|" + InputTotaltax + "|" + subTaxResultData + "|" + glCode.Replace(".", ""));
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
            SAPDBEntities DBEntity = new SAPDBEntities();

            SAP_SalesJournal DBSPJournal = new SAP_SalesJournal();

            try
            {
                foreach (var responseItem in response)
                {
                    DBSPJournal.OrderNumber = responseItem.OrderNumber; //
                    int ProductId = Convert.ToInt32(responseItem.ProductID);
                    var orderItem = DBEntity.OrderItems.Where(x => x.OrderId == DBSPJournal.OrderNumber && x.ProductId == ProductId).FirstOrDefault();
                    if (orderItem != null)
                        DBSPJournal.OrderItemID = orderItem.Id;
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
                        DBSPJournal.MasterProduct = ""; // (DBEntity.Products.Where(x => x.Id == ProductDetail.MasterId).FirstOrDefault().Name == null ? "" : DBEntity.Products.Where(x => x.Id == ProductDetail.MasterId).FirstOrDefault().Name);
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
                Logger.Error(ex.StackTrace);
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

        public static string RetrieveGLCodeByStatus(int statusId, bool first = true, bool isTaxGl = false)
        {

            if (statusId == 1)
            {

                if (first)
                {
                    return "48702040";
                }
                else
                {
                    return "41102004";
                }
            }

            if (statusId == 2)
            {
                if (isTaxGl)
                {
                    return "44571098";
                }

                if (first)
                {
                    return "70895010";
                }
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
