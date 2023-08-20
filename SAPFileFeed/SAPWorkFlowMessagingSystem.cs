using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace SAPFileFeed
{
    public static class SAPWorkFlowMessagingSystem
    {
        public static void SendMail(string filename, int totalOrders, int processedOrders, string failedOrders)
        {
            MailMessage message = new MailMessage();
            SmtpClient smtp = new SmtpClient();
            message.From = new MailAddress("no-reply@sodexomyway.com");
            string SAPRecipients = ConfigurationManager.AppSettings["SAPRecipients"];
            foreach (var address in SAPRecipients.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
            {
                message.To.Add(address);
            }
            message.Subject = "Processing Details for the File-" + filename;
            message.IsBodyHtml = true; //to make message body as html  
            message.Body = GetSapFileProcesisngResult(totalOrders, processedOrders, failedOrders);
            smtp.Port = 587;
            smtp.Host = "smtp.sendgrid.net";
            smtp.EnableSsl = true;
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new NetworkCredential("apikey", "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");
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtp.Send(message);
        }

        public static string GetSapFileProcesisngResult(int totalOrders, int processedOrders, string failerOrders)
        {
            //totalOrders = 100;
            //processedOrders = 100;
            string sb = string.Empty; ;
            sb = string.Format(@"<html>
                      <body>
                        <table style='width:100%;' border='0'>
                            <tr style='background-color:#b9babe; text-align:center;'>
                                <th>Date</th>
                                <th>Total Orders</th>
                                <th>Orders Processed</th>
                                <th>Orders Failed</th>
                                <th>#Orders</th>
                            </tr>
                            <tr style='background-color:#ebecee; text-align:center;'>
                                <td style='padding:0.6em 0.4em; text-align:right;'>{0}</td>
                                <td style='padding:0.6em 0.4em; text-align:right;'>{1}</td>
                                <td style ='padding:0.6em 0.4em; text-align:center;'>{2}</td>
                                <td style ='padding:0.6em 0.4em; text-align:right;'>{3}</td>
                                <td style ='padding:0.6em 0.4em; text-align:right;'>{4}</td>
                            </tr>
                        </table>
                      </body>
                      </html>", DateTime.Now, totalOrders, processedOrders, (totalOrders - processedOrders), failerOrders);

            return sb;
        }
    }
}
