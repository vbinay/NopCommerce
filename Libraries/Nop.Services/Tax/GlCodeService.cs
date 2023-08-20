using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.Core.Domain.Tax;
using Nop.Services.Events;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using System.Data.SqlClient;
using System.Data;
using System.Xml;

namespace Nop.Services.Tax
{
    /// <summary>
    /// Tax category service
    /// </summary>
    public partial class GlCodeService : IGlCodeService
    {


        #region Fields

        private readonly IRepository<GlCode> _glCodeRepository;
        private readonly IRepository<ProductGlCode> _productGlCodeRepository;
        private readonly IEventPublisher _eventPublisher;
        private readonly ICacheManager _cacheManager;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="cacheManager">Cache manager</param>
        /// <param name="taxCategoryRepository">Tax category repository</param>
        /// <param name="eventPublisher">Event published</param>
        public GlCodeService(ICacheManager cacheManager,
            IRepository<GlCode> glCodeRepository,
            IRepository<ProductGlCode> productGlCodeRepository,
            IEventPublisher eventPublisher)
        {
            _cacheManager = cacheManager;
            _glCodeRepository = glCodeRepository;
            _productGlCodeRepository = productGlCodeRepository;
            _eventPublisher = eventPublisher;
        }

        #endregion

        #region Methods

        #region nu-90

        /// <summary>
        /// Delete ProductGlCode
        /// </summary>
        /// <param name="productGlCode"></param>
        public void DeleteProductGlCode(ProductGlCode productGlCode)
        {
            _productGlCodeRepository.Delete(productGlCode);
        }

        public void UpdateProductGlCode(ProductGlCode productGlCode)
        {
            _productGlCodeRepository.Update(productGlCode);
        }


        /// <summary>
        /// insert the GlCodes for the products
        /// </summary>
        /// <param name="product"></param>
        /// <returns></returns>
        public void InsertProductGlCode(ProductGlCode productGlCode)
        {
            _productGlCodeRepository.Insert(productGlCode);
        }



        public List<ProductGlCode> GetProductGlCodes(Product product)
        {
            var query = from tc in _productGlCodeRepository.Table
                        where tc.ProductId == product.Id
                        select tc;

            return query.ToList();
        }

        public List<ProductGlCode> GetProductGlCodes(Product product, GLCodeType glType, GLCodeStatusType type = GLCodeStatusType.All)
        {
            if ((int)type == 1) //paid
            {
                var query = from tc in _productGlCodeRepository.Table
                            where tc.ProductId == product.Id
                            && tc.GlCode.GlCodeTypeId == (int)glType
                            && tc.GlCode.IsPaid == true
                            orderby tc.GlCode.GlCodeName
                            select tc;
                return query.ToList();
            }
            else if ((int)type == 2) //deferred
            {
                var query = from tc in _productGlCodeRepository.Table
                            where tc.ProductId == product.Id
                            && tc.GlCode.GlCodeTypeId == (int)glType
                            && tc.GlCode.IsDelivered == true
                            orderby tc.GlCode.GlCodeName
                            select tc;
                return query.ToList();
            }
            else //All
            {
                var query = from tc in _productGlCodeRepository.Table
                            where tc.ProductId == product.Id
                            && tc.GlCode.GlCodeTypeId == (int)glType
                            orderby tc.GlCode.GlCodeName
                            select tc;
                return query.ToList();
            }
        }

        public ProductGlCode GetProductGlCodeByName(string name, List<ProductGlCode> glCodes)
        {
            foreach (var glcode in glCodes)
            {
                if (glcode.GlCode.Description == name)
                {
                    return glcode;
                }
            }
            return null;
        }

        public GlCode GetGlCodeByGlcodeName(string GlcodeName)
        {
            if (!string.IsNullOrEmpty(GlcodeName))
            {
                var query = from tc in _glCodeRepository.Table
                            where tc.GlCodeName == GlcodeName
                            select tc;

                return query.FirstOrDefault();
            }
            else
            { return null; }
        }

        public List<ProductGlCode> GetProductsGlByType(int type, List<ProductGlCode> glCodes, bool isPaid = false, bool isDelivery = false, bool withTaxCategory = false)
        {

            List<ProductGlCode> matchingGls = new List<ProductGlCode>();
            foreach (var glcode in glCodes)
            {
                if (glcode.GlCode.GlCodeTypeId == type && glcode.GlCode.IsDelivered == isDelivery && glcode.GlCode.IsPaid == isPaid)
                {
                    if (withTaxCategory)
                    {
                        if (glcode.TaxCategoryId != 0)
                        {
                            matchingGls.Add(glcode);
                        }

                    }
                    else
                    {
                        matchingGls.Add(glcode);
                    }

                }
            }
            return matchingGls;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="product"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public List<ProductGlCode> GetProductGlCodes(Product product, GLCodeStatusType type)
        {
            if ((int)type == 1) //paid
            {
                var query = from tc in _productGlCodeRepository.Table
                            where tc.ProductId == product.Id
                            && tc.GlCode.IsPaid == true
                            select tc;
                return query.ToList();
            }
            else if ((int)type == 2) //deferred
            {
                var query = from tc in _productGlCodeRepository.Table
                            where tc.ProductId == product.Id
                            && tc.GlCode.IsDelivered == true
                            select tc;
                return query.ToList();
            }
            else //All
            {
                var query = from tc in _productGlCodeRepository.Table
                            where tc.ProductId == product.Id
                            select tc;
                return query.ToList();
            }
        }

        public List<ProductGlCode> GetProductGlCodesByProductId(int productId, GLCodeStatusType type)
        {
            if ((int)type == 1) //paid
            {
                var query = from tc in _productGlCodeRepository.Table
                            where tc.ProductId == productId
                            && tc.GlCode.IsPaid == true
                            select tc;
                return query.ToList();
            }
            else if ((int)type == 2) //deferred
            {
                var query = from tc in _productGlCodeRepository.Table
                            where tc.ProductId == productId
                            && tc.GlCode.IsDelivered == true
                            select tc;
                return query.ToList();
            }
            else //All
            {
                var query = from tc in _productGlCodeRepository.Table
                            where tc.ProductId == productId
                            select tc;
                return query.ToList();
            }
        }

        public List<ProductGlCode> GetProductGlCodeByProductandGlcodeID(int productid, int GlcodeID)
        {
            var query = from tc in _productGlCodeRepository.Table
                        where tc.ProductId == productid && tc.GlCodeId == GlcodeID
                        select tc;
            return query.ToList();
        }

        public ProductGlCode GetProductGlCodes(int GlcodeMappingID)
        {
            var query = from tc in _productGlCodeRepository.Table
                        where tc.Id == GlcodeMappingID
                        select tc;
            return query.FirstOrDefault();
        }





        /// <summary>
        /// Get GlCodes
        /// </summary>
        /// <param name="isPaid"></param>
        /// <param name="isDeferred"></param>
        /// <param name="glCodeName"></param>
        /// <returns></returns>
        public List<GlCode> GetGlCodes(GLCodeStatusType glCodeType, string glCodeName = "")
        {

            if ((int)glCodeType == 1) //paid
            {
                var query = from tc in _glCodeRepository.Table
                            where tc.IsPaid == true
                            select tc;
                return query.ToList();
            }
            else if ((int)glCodeType == 2) //deferred
            {
                var query = from tc in _glCodeRepository.Table
                            where tc.IsDelivered == true
                            select tc;
                return query.ToList();
            }
            else //All
            {
                var query = from tc in _glCodeRepository.Table
                            select tc;
                return query.ToList();
            }


        }

        /// <summary>
        /// Get GlCodes
        /// </summary>
        /// <param name="isPaid"></param>
        /// <param name="isDeferred"></param>
        /// <param name="glCodeName"></param>
        /// <returns></returns>
        public GlCode GetGlCodeById(int Id)
        {
            var query = from tc in _glCodeRepository.Table
                        where tc.Id == Id
                        select tc;

            return query.ToList().FirstOrDefault();
        }
        #endregion
        #endregion


        public List<GlCode> GetAllGlCodes()
        {
            var query = from tc in _glCodeRepository.Table
                        select tc;

            return query.ToList();
        }


        public string GetVertexXML(int orderId, string productId, TaxRequestType typeOfCall)
        {
            var connStr = new DataSettingsManager().LoadSettings().DataConnectionString;
            int glcodeTypeOfCall = Convert.ToInt32(typeOfCall);
            string xmlResponse = "";
            //   var reportList = new List<SAPOrder>();
            using (var conn = new SqlConnection(connStr))
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandType = CommandType.Text;
                var sqlSelect = string.Format(@"select top 1 * from TaxResponseStorage where typeofcall = " + glcodeTypeOfCall + " and orderId = " + orderId
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

        public string GetVertexXML(int orderId, string productId, TaxRequestType typeOfCall, int orderItemId)
        {
            var connStr = new DataSettingsManager().LoadSettings().DataConnectionString;
            int glcodeTypeOfCall = Convert.ToInt32(typeOfCall);
            string xmlResponse = "";
            //   var reportList = new List<SAPOrder>();
            using (var conn = new SqlConnection(connStr))
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandType = CommandType.Text;
                var sqlSelect = string.Format(@"select top 1 * from TaxResponseStorage where typeofcall = " + glcodeTypeOfCall + " and orderId = " + orderId
                     + " and productId = '" + productId.ToString() + "'  and OrderItemId='" + orderItemId + "' order by addeddate desc");
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
        public List<string> GetProductIdByOrderid(int orderid, TaxRequestType typeOfCall)
        {

            var connStr = new DataSettingsManager().LoadSettings().DataConnectionString;
            int glcodeTypeOfCall = Convert.ToInt32(typeOfCall);
            var reportList = new List<string>();
            //   var reportList = new List<SAPOrder>();
            using (var conn = new SqlConnection(connStr))
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandType = CommandType.Text;
                var sqlSelect = string.Format(@"select  * from TaxResponseStorage where typeofcall = " + glcodeTypeOfCall + " and orderId = " + orderid);
                cmd.CommandText = sqlSelect;
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        reportList.Add(Convert.ToString(reader["ProductId"]));
                    }
                }
            }
            return reportList;
        }

        public List<DynamicVertexGLCode> GetVertexGlBreakdown(int orderId, int productId, TaxRequestType typeOfCall)
        {

            string xmlResponse = GetVertexXML(orderId, Convert.ToString(productId), typeOfCall);

            return this.GetGLBreakdownfromXML(xmlResponse);
        }

        public List<DynamicVertexGLCode> GetVertexGlBreakdown(int orderId, int productId, TaxRequestType typeOfCall, int orderItemId = 0)
        {
            string xmlResponse = GetVertexXML(orderId, Convert.ToString(productId), typeOfCall, orderItemId);

            return this.GetGLBreakdownfromXML(xmlResponse);
        }

        public List<DynamicVertexGLCode> GetVertexProductGlBreakdown(List<DynamicVertexGLCode> vertexGls, string productId)
        {
            List<DynamicVertexGLCode> productGls = new List<DynamicVertexGLCode>();

            foreach (var gl in vertexGls)
            {
                if (gl.productId.Contains(productId))
                {
                    bool found = false;
                    foreach (var prodGl in productGls)
                    {
                        if (prodGl.GlCode == gl.GlCode)
                        {
                            found = true;
                            prodGl.Total += gl.Total;
                        }
                    }
                    if (!found)
                    {
                        productGls.Add(gl);
                    }

                }
            }
            return productGls;
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
