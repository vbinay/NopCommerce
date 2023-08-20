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
using Tamir.SharpSsh.Wrappers;

namespace FileFeed.SAP
{
    class SAPFileFeed
    {
        private static readonly ILog Logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static void test(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();

            List<SAPSalesJournal> mealplans = getSalesJournalDatabaseResults();
            Logger.Info("==============================================================");
            Logger.Info("Processing " + mealplans.Count + " Meal Plans");

            if (mealplans.Count == 0)
            {
                return;
            }


            IList<FsuMealPlanRecord> list = new List<FsuMealPlanRecord>();
            List<int> ids = new List<int>();

            string priceAttributeNameToLookFor = "Price: $";

            foreach (MealPlan plan in mealplans)
            {
                string attribute = plan.Attributes;

                if (attribute.Contains(priceAttributeNameToLookFor))
                {
                    int pFrom = attribute.IndexOf(priceAttributeNameToLookFor) + priceAttributeNameToLookFor.Length;
                    int pTo = attribute.IndexOf("<br />", pFrom);

                    if (pTo == -1)
                    {
                        //no <BR /> after the price, must be end of the string
                        pTo = attribute.Length;
                    }
                    string empId = plan.RecipientAcctNum.Substring(0, plan.RecipientAcctNum.Length < 10 ? plan.RecipientAcctNum.Length : 10);
                    string mealplanAmount = attribute.Substring(pFrom, pTo - pFrom);
                    string orderNumber = plan.OrderItemId + "M" + plan.OrderId;

                    if (plan.CategoryName == "Student Fall/Spring") //need to add 2 records - one for fall this year, one for spring next year
                    {
                        var fallRecord = new FsuMealPlanRecord();
                        fallRecord.EmplId = empId;
                        fallRecord.Amount = Convert.ToString(mealplanAmount);
                        fallRecord.ReferenceNumber = orderNumber;
                        var planTerm = GetPlanTerm("Fall");
                        var dueDate = GetPlanDueDate("Fall");
                        fallRecord.Term = "2" + DateTime.Now.ToString("yy") + planTerm;
                        fallRecord.DueDate = dueDate;
                        list.Add(fallRecord);


                        var springRecord = new FsuMealPlanRecord();
                        springRecord.EmplId = empId;
                        springRecord.Amount = Convert.ToString(mealplanAmount);
                        springRecord.ReferenceNumber = orderNumber;


                        planTerm = GetPlanTerm("Spring");
                        dueDate = GetPlanDueDate("Spring");
                        springRecord.Term = "2" + DateTime.Now.AddYears(1).ToString("yy") + planTerm;
                        springRecord.DueDate = dueDate;
                        list.Add(springRecord);

                    }
                    if (plan.CategoryName == "Student Spring") //need to add 1 record only for spring.
                    {
                        var springRecord = new FsuMealPlanRecord();
                        springRecord.EmplId = empId;
                        springRecord.Amount = Convert.ToString(mealplanAmount);
                        springRecord.ReferenceNumber = orderNumber;

                        var planTerm = GetPlanTerm("Spring");
                        var dueDate = GetPlanDueDate("Spring");
                        springRecord.Term = "2" + DateTime.Now.ToString("yy") + planTerm;
                        springRecord.DueDate = dueDate;
                        list.Add(springRecord);

                    }
                    else
                    {

                        var record = new FsuMealPlanRecord();
                        record.EmplId = empId;
                        record.Amount = Convert.ToString(mealplanAmount);
                        record.ReferenceNumber = orderNumber;

                        var planTerm = GetPlanTerm(plan.CategoryName);
                        var dueDate = GetPlanDueDate(plan.CategoryName);
                        record.Term = "2" + DateTime.Now.ToString("yy") + planTerm;
                        record.DueDate = dueDate;
                        list.Add(record);
                    }
                    ids.Add(plan.Id);
                }
            }


            decimal trailerTotal = Convert.ToDecimal(0.00) + list.Sum(rec => Convert.ToDecimal(rec.Amount));
            string totalTrailer = trailerTotal.ToString("{0:#,##0.00}");
            var factory = new FixedLengthFileEngineFactory();

            var path = "";
            if (!String.IsNullOrEmpty(ConfigurationManager.AppSettings["TempFolder"]))
            {
                path = ConfigurationManager.AppSettings["TempFolder"];
            }
            var fileName = "file.txt";

            using (var stream = File.Create(path + fileName))
            {
                var recordlayout = new FsuFixedRecordLayout();
                var flatFile = factory.GetEngine(recordlayout);
                flatFile.Write<FsuMealPlanRecord>(stream, list);
                var trailerlayout = new FsuFixedTrailerLayout();
                var flatFile2 = factory.GetEngine(trailerlayout);

                IList<FsuMealPlanTrailer> trailers = new List<FsuMealPlanTrailer>();
                var trailer = new FsuMealPlanTrailer();
                trailer.ControlAmount = trailerTotal;
                trailer.ControlCount = list.Count;
                trailers.Add(trailer);

                flatFile2.Write<FsuMealPlanTrailer>(stream, trailers);
                Logger.Info("Built File: " + path + fileName);
            }

            bool fileAlreadyOnServer = false;
            bool uploadToServer = true;
            bool processMealPlanRecords = true;

            if (!String.IsNullOrEmpty(ConfigurationManager.AppSettings["ProcessMealPlanRecords"])) //app config override
            {
                processMealPlanRecords = Boolean.Parse(ConfigurationManager.AppSettings["ProcessMealPlanRecords"]);
            }

            if (!String.IsNullOrEmpty(ConfigurationManager.AppSettings["UploadToServer"])) //app config override
            {
                uploadToServer = Boolean.Parse(ConfigurationManager.AppSettings["UploadToServer"]);
            }

            if (uploadToServer)
            {
                Logger.Info("uploading to server");
                using (var ssh = new SFTPUtil("OBS-FS02.bfs.fsu.edu", "Sodexo_MP", "F$U_sdx17#"))
                {
                    IList<ChannelSftp.LsEntry> files = ssh.ListFiles("/");
                    if (files.Count > 0)
                    {
                        fileAlreadyOnServer = true; //a file is still out there
                        Logger.Info("file already on server");
                    }
                    else
                    {
                        Logger.Info("no file on server");
                        fileAlreadyOnServer = false;
                    }


                    if (!fileAlreadyOnServer)
                    {
                        Logger.Info("Uploading To Server: " + path + fileName);
                        ssh.PutFile(path + fileName, "SDXMP.dat");

                        if (processMealPlanRecords)
                        {
                            Logger.Info("Processing Meal Plans");
                            SetToProcessed(ids);
                        }
                        Logger.Info("Backing up file");
                        CreateFileBackup(path, fileName);
                    }
                }
            }
            Logger.Info("Process Complete");
            Logger.Info("==============================================================");
        }



        public static string GetPlanDueDate(string description)
        {
            if (description.Contains("Summer"))
            {
                return "20170707";
            }
            else if (description.Contains("Fall"))
            {
                return "20170908";
            }
            else if (description.Contains("Spring"))
            {
                return "20180119";
            }

            return "20170707"; //SUMMER;
        }


        public static string GetPlanTerm(string description)
        {
            if (description.Contains("Summer"))
            {
                return "6";
            }
            else if (description.Contains("Fall"))
            {
                return "9";
            }
            else if (description.Contains("Spring"))
            {
                return "1";
            }

            return "9"; //FALL
        }


        private static void CreateFileBackup(string path, string fileToCopy)
        {
            string path2 = DateTime.Now.ToString("yyyy-MM-dd HHmmss") + "-PROCESSED-" + fileToCopy;

            try
            {
                // Copy the file.
                File.Copy(path + fileToCopy, path + path2);

                Logger.Info(String.Format("{0} copied to {1}", path, path2));

                File.Delete(path + fileToCopy);

            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }
        }


        private static List<SAPSalesJournal> getSalesJournalDatabaseResults()
        {

            var connStr = ConfigurationManager.ConnectionStrings["BlueDot"].ConnectionString;
            var reportList = new List<SAPSalesJournal>();
            using (var conn = new SqlConnection(connStr))
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandType = CommandType.Text;
                var sqlSelect = string.Format(@"
                      SELECT OrderNumber, [Status], DateOfTransaction, CostCenterNumber, ProductGL, ProductAmount
                        FROM [dbo].[SAP_SalesJournal]
                    ");


                cmd.CommandText = sqlSelect;
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var salesJournal = new SAPSalesJournal();
                        salesJournal.CostCenterNumber = "1234567890";//((string)reader["1234567890"]);
                        salesJournal.DateOfTransaction = ((DateTime)reader["DateOfTransaction"]);
                        salesJournal.OrderNumber = ((string)reader["OrderNumber"]).Trim();
                        salesJournal.ProductAmount = ((string)reader["ProductAmmount"]);
                        salesJournal.ProductGL = ((string)reader["ProductGL"]);

                        //     mealPlan.Amount = (decimal)reader["PriceExclTax"];
                        /*  res.UnitId = ((string)reader["Unit Id"]).Trim();
                          res.UnitName = ((string)reader["Unit Name"]).Trim();
                          res.MasterCataLogger.ategory = ((string)reader["Master CataLogger.Category"]).Trim();
                          res.MasterCataLogger.roductId = (int)reader["Master CataLogger.Product ID"];
                          res.MasterCataLogger.roductName = ((string)reader["Master CataLogger.Product Name"]).Trim();
                          res.UniversalPlu = DBNull.Value.Equals(reader["Master CataLogger.Product SKU"]) ? "" : ((string)reader["Master CataLogger.Product SKU"]).Trim();
                          res.LocalProductName = ((string)reader["Local Product Name"]).Trim();
                          res.LocalPrice = decimal.Round((decimal)reader["Local Price"], 2);
                          res.TotalSalesExclTax = decimal.Round((decimal)reader["Total Sales Excl Tax"], 2);
                          res.TotalSalesInclTax = decimal.Round((decimal)reader["Total Sales Incl Tax"], 2);
                          res.QtySold = (int)reader["Qty Sold"];
                          res.IsPublished = ((string)reader["Is Published"]).Trim();
                          res.LocalProductDetailUrl = ((string)reader["View Local Product"]).Trim();*/

                        reportList.Add(salesJournal);
                    }
                }
            }
            return reportList;
        }

        private static void SetToProcessed(List<int> ids)
        {
            StringBuilder queryIds = new StringBuilder();
            for (int i = 0; i < ids.Count; i++)
            {
                queryIds.Append(ids[i] + (i + 1 == ids.Count ? "" : ","));
            }

            var connStr = ConfigurationManager.ConnectionStrings["BlueDot"].ConnectionString;
            using (var conn = new SqlConnection(connStr))
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandType = CommandType.Text;
                var sqlSelect = string.Format(@"
                      update mealplan set IsProcessed = 1, ProcessedDate = '{0}' where id in({1})", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), queryIds);
                cmd.CommandText = sqlSelect;
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

    }
}
