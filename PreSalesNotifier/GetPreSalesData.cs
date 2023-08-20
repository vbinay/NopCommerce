using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace PreSalesNotifier
{
    class GetPreSalesData
    {
        public static void RunPreSalesRecordsReciever()
        {
            var connStr = ConfigurationManager.ConnectionStrings["BlueDot"].ConnectionString;
            var GetPreSalesProductInfo = GetPreSalesDataForAllUnits(connStr);
            var GetStoreids = GetAllStoreId(connStr);

            List<PreSalesDataModel> dataTobeGoingForSaleFuture = new List<PreSalesDataModel>();

            //This Section returns the Record details for each store and shall return them to the corresponding store owner.
            foreach (var storeid in GetStoreids)
            {
                var info = GetPreSalesProductInfo.Where(x => x.storeid == storeid && DateTime.Now.AddDays(7) <= x.PreorderAvaibilityStartDate).ToList();
                if (info.Any())
                    dataTobeGoingForSaleFuture.AddRange(info);
                var orderCounts = GetorderCountsPreSale(connStr, storeid);

                SendMail(info, orderCounts);
            }

        }

        private static List<PreSalesDataModel> GetPreSalesDataForAllUnits(string connStr)
        {


            var preSalesData = new List<PreSalesDataModel>();
            using (var conn = new SqlConnection(connStr))
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandType = CommandType.Text;
                //Section to fetch records that are fulfilled and Purchased after cutoff date.

                var sqlSelect = string.Format(@"select P.Name,p.Id,p.AvailableForPreOrder,
                                                p.AvailableStartDateTimeUtc,S.Id,S.Name,
                                                P.Deleted,p.IsMaster,p.Published
                                                from product P inner Join storeMapping SM
                                                on P.Id=SM.EntityId Inner Join Store S
                                                on SM.StoreId=S.Id where SM.EntityName='Product' and 
                                                p.AvailableForPreOrder=1 and p.IsMaster=0 and p.Deleted=0");

                cmd.CommandText = sqlSelect;
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var preSalesInfo = new PreSalesDataModel();
                        preSalesInfo.productId = ((int)reader["Id"]);
                        preSalesInfo.productName = ((string)reader["Name"]);
                        preSalesInfo.isDeleted = ((bool)reader["OrderItemId"]);
                        preSalesInfo.IsMaster = ((bool)reader["StatusId"]);
                        preSalesInfo.isPreorder = ((bool)reader["StatusId"]); ;
                        preSalesInfo.PreorderAvaibilityStartDate = ((DateTime)reader["StatusId"]); ;
                        preSalesInfo.isPublished = ((bool)reader["StatusId"]); ;
                        preSalesInfo.storeid = ((int)reader["StatusId"]); ;
                        preSalesInfo.StoreName = ((string)reader["StatusId"]); ;

                        preSalesData.Add(preSalesInfo);
                    }
                }

            }
            return preSalesData;
        }

        private static List<int> GetAllStoreId(string connStr)
        {
            var storeids = new List<int>();
            using (var conn = new SqlConnection(connStr))
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandType = CommandType.Text;
                //Section to fetch records that are fulfilled and Purchased after cutoff date.

                var sqlSelect = string.Format(@"select id from Store");

                cmd.CommandText = sqlSelect;
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        dynamic preSalesInfo = ((int)reader["Id"]);
                        storeids.Add(preSalesInfo);
                    }
                }
            }
            return storeids;
        }

        private static int GetorderCountsPreSale(string connStr, int storeId)
        {
            int count = 0;
            using (var conn = new SqlConnection(connStr))
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandType = CommandType.Text;
                //Section to fetch records that are fulfilled and Purchased after cutoff date.

                var sqlSelect = string.Format(@"select O.*,P.AvailableForPreOrder,O.CreatedOnUtc from [Order] O inner Join OrderItem OT
                                                on O.id=OT.OrderId
                                                inner join Product P
                                                on OT.ProductId=P.Id
                                                where p.AvailableForPreOrder=1 and O.StoreId=" + storeId + " and O.CreatedOnUtc=GETDATE()");

                cmd.CommandText = sqlSelect;
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        count++;
                    }
                }
            }
            return count;
        }

        public static void SendMail(List<PreSalesDataModel> preSalesData, int orderCounts)
        {
            MailMessage message = new MailMessage();
            SmtpClient smtp = new SmtpClient();
            message.From = new MailAddress("no-reply@sodexomyway.com");
            string SAPRecipients = ConfigurationManager.AppSettings["SAPRecipients"];
            foreach (var address in SAPRecipients.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
            {
                message.To.Add(address);
            }
            message.Subject = "PreSale Details-" + DateTime.Now;
            message.IsBodyHtml = true; //to make message body as html  
            message.Body = CreateMailerInfoDetailsForPresales(preSalesData, orderCounts);
            smtp.Port = 587;
            smtp.Host = "smtp.sendgrid.net";
            smtp.EnableSsl = true;
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new NetworkCredential("apikey", "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");
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtp.Send(message);
        }


        public static string CreateMailerInfoDetailsForPresales(List<PreSalesDataModel> preSalesData, int orderCounts)
        {

            string sb = string.Empty;
            string ProductName = string.Empty;
            foreach (var items in preSalesData)
            {
                ProductName = ProductName + "" + items.productName;
            }

            sb = string.Format(@"<html>
                      <body>
                        <table style='width:100%;' border='0'>
                            <tr style='background-color:#b9babe; text-align:center;'>
                                <th>Date</th>
                                <th>Store Name</th>
                                <th>Products Going For PreSale</th>
                                <th>Purchassed Orders Count on PreSale</th>
                            </tr>
                            <tr style='background-color:#ebecee; text-align:center;'>
                                <td style='padding:0.6em 0.4em; text-align:right;'>{0}</td>
                                <td style='padding:0.6em 0.4em; text-align:right;'>{1}</td>
                                <td style ='padding:0.6em 0.4em; text-align:center;'>{2}</td>
                                <td style ='padding:0.6em 0.4em; text-align:right;'>{3}</td>
                            </tr>
                        </table>
                      </body>
                      </html>", DateTime.Now, preSalesData[0].StoreName, ProductName, orderCounts);

            return sb;
        }
    }
}
