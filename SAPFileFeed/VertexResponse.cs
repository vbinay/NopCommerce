using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SAPFileFeed
{
    public class VertexResponse
    {
        public class DynamicVertexGLCode
        {
            public DynamicVertexGLCode(string productId, string name, string description, decimal total, string taxCode)
            {
                this.productId = productId;
                this.GlCode = name;
                this.Total = total;
                this.Description = description;
                this.taxCode = taxCode;
            }
            public String Description { get; set; }
            public string GlCode
            {
                get;
                set;
            }

            public decimal Total { get; set; }

            public string productId { get; set; }

            public string taxCode { get; set; }
        }

        public List<DynamicVertexGLCode> GetVertexGlBreakdown(int orderId, int productId, int typeOfCall)
        {

            string xmlResponse = GetVertexXML(orderId, productId, typeOfCall);

            return this.GetGLBreakdownfromXML(xmlResponse);
        }
        public List<DynamicVertexGLCode> GetVertexGlBreakdown(int orderId, int productId, int typeOfCall, int orderitemId)
        {

            string xmlResponse = GetVertexXML(orderId, productId, typeOfCall, orderitemId);

            return this.GetGLBreakdownfromXML(xmlResponse);
        }

        public string GetVertexXML(int orderId, int productId, int typeOfCall)
        {
            var connStr = ConfigurationManager.ConnectionStrings["BlueDot"].ConnectionString;
            string sqlSelect = string.Empty;
            int glcodeTypeOfCall = Convert.ToInt32(typeOfCall);
            string xmlResponse = "";
            string productID = string.Empty;
            //   var reportList = new List<SAPOrder>();
            using (var conn = new SqlConnection(connStr))
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandType = CommandType.Text;
                sqlSelect = string.Format(@"select top 1 * from TaxResponseStorage where typeofcall = " + glcodeTypeOfCall + " and orderId = " + orderId
                    + " and productId = '" + productId.ToString() + "' order by addeddate desc");
                cmd.CommandText = sqlSelect;
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        // var vertexGLCode = new DynamicVertexGLCode();
                        xmlResponse = ((string)reader["XMLResponse"]);
                        //  order.OrderId = ((int)reader["OrderId"]);
                        //  order.OrderItemId = ((int)reader["OrderItemId"]);
                        //  order.StatusID = ((int)reader["StatusId"]);

                        // reportList.Add(order);
                    }
                }
            }
            return xmlResponse;
        }

        public string GetVertexXML(int orderId, int productId, int typeOfCall, int orderitemId)
        {
            var connStr = ConfigurationManager.ConnectionStrings["BlueDot"].ConnectionString;
            string sqlSelect = string.Empty;
            int glcodeTypeOfCall = Convert.ToInt32(typeOfCall);
            string xmlResponse = "";
            string productID = string.Empty;
            //   var reportList = new List<SAPOrder>();
            using (var conn = new SqlConnection(connStr))
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandType = CommandType.Text;
                sqlSelect = string.Format(@"select top 1 * from TaxResponseStorage where typeofcall = " + glcodeTypeOfCall + " and orderId = " + orderId
                    + " and productId = '" + productId.ToString() + "' and OrderItemId='" + orderitemId + "' order by addeddate desc");
                cmd.CommandText = sqlSelect;
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        // var vertexGLCode = new DynamicVertexGLCode();
                        xmlResponse = ((string)reader["XMLResponse"]);
                        //  order.OrderId = ((int)reader["OrderId"]);
                        //  order.OrderItemId = ((int)reader["OrderItemId"]);
                        //  order.StatusID = ((int)reader["StatusId"]);

                        // reportList.Add(order);
                    }
                }
            }
            return xmlResponse;
        }

        public List<DynamicVertexGLCode> GetGLBreakdownfromXML(string xml)
        {
            XmlDocument xmlDoc = new XmlDocument();
            List<DynamicVertexGLCode> reportHelpers = new List<DynamicVertexGLCode>();
            if (!xml.Contains("The requested URL was rejected. Please consult with your administrator."))
            {

                if (xml == "")
                {
                    return new List<DynamicVertexGLCode>();
                }
                xmlDoc.LoadXml(xml);

                XmlNodeList nodeitem = xmlDoc.GetElementsByTagName("DistributeTaxResponse");



                foreach (XmlNode xmlnode in nodeitem)
                {
                    for (int i = 0; i < xmlnode.ChildNodes.Count; i++)
                    {

                        XmlNode childnode = xmlnode.ChildNodes[i];

                        System.Diagnostics.Debug.WriteLine(childnode.Name);

                        if (childnode.Name == "LineItem")
                        {

                            string productId = childnode.Attributes["lineItemId"].Value;

                            string productClass = string.Empty;

                            foreach (XmlNode lineItemNode in childnode.ChildNodes)
                            {
                                if (lineItemNode.Name == "Product")
                                {
                                    productClass = lineItemNode.Attributes["productClass"].Value;
                                }

                                if (lineItemNode.Name == "Taxes")
                                {
                                    XmlNode taxNode = lineItemNode;
                                    decimal calculatedTax = Convert.ToDecimal(taxNode["CalculatedTax"].InnerText);
                                    string ruleName = string.Empty;
                                    string glcode = string.Empty;
                                    string SummaryInvoiceText = taxNode["SummaryInvoiceText"].InnerText;
                                    foreach (XmlNode nodedata in taxNode["AssistedParameters"])
                                    {
                                        if (SummaryInvoiceText == nodedata.InnerText)
                                        {
                                            ruleName = nodedata.Attributes["ruleName"].Value;
                                            glcode = nodedata.InnerText;
                                        }
                                    }


                                    //var assistedParameter = taxNode["AssistedParameters"].ChildNodes[0];


                                    //ruleName = assistedParameter.Attributes["ruleName"].Value;
                                    //glcode = assistedParameter.InnerText;


                                    bool glfound = false;


                                    foreach (DynamicVertexGLCode helper in reportHelpers)
                                    {
                                        if (helper.GlCode == glcode && helper.productId == productId)
                                        {
                                            glfound = true;
                                            helper.Total += calculatedTax;
                                            break;
                                        }
                                    }

                                    if (!glfound)
                                    {
                                        reportHelpers.Add(new DynamicVertexGLCode(productId, glcode, ruleName, calculatedTax, productClass));
                                    }
                                }
                            }

                        }
                    }
                }

                return reportHelpers;
            }
            else
            {
                return reportHelpers;
            }

        }
    }
}

