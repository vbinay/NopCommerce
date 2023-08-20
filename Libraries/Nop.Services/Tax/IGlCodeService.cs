using System.Collections.Generic;
using Nop.Core.Domain.Tax;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;

namespace Nop.Services.Tax
{
    /// <summary>
    /// Tax category service interface
    /// </summary>
    public partial interface IGlCodeService
    {

        /// <summary>
        /// Gets the GlCodes for the products
        /// </summary>
        /// <param name="product"></param>
        /// <returns></returns>
        List<ProductGlCode> GetProductGlCodes(Product product);


        GlCode GetGlCodeByGlcodeName(string GlcodeName);

        List<ProductGlCode> GetProductGlCodes(Product product, GLCodeType glType, GLCodeStatusType type);

        //decimal CalculateGLAmount(OrderItem item, ProductGlCode glCode);

        List<ProductGlCode> GetProductGlCodeByProductandGlcodeID(int productid, int GlcodeID);

        List<ProductGlCode> GetProductGlCodes(Product product, GLCodeStatusType type);

        List<ProductGlCode> GetProductGlCodesByProductId(int productId, GLCodeStatusType type); 

        ProductGlCode GetProductGlCodeByName(string name, List<ProductGlCode> glCodes);

        List<DynamicVertexGLCode> GetGLBreakdownfromXML(string xml);

        List<ProductGlCode> GetProductsGlByType(int type, List<ProductGlCode> glCodes, bool isPaid = false, bool isDelivery = false, bool withTaxCategory = false);

        List<DynamicVertexGLCode> GetVertexGlBreakdown(int orderId, int productId, TaxRequestType typeOfCall);
        List<DynamicVertexGLCode> GetVertexGlBreakdown(int orderId, int productId, TaxRequestType typeOfCall,int orderItemId=0);

        List<DynamicVertexGLCode> GetVertexProductGlBreakdown(List<DynamicVertexGLCode> vertexGls, string productId);

        string GetVertexXML(int orderId, string productId, TaxRequestType typeOfCall);
        string GetVertexXML(int orderId, string productId, TaxRequestType typeOfCall,int orderItemId);
        List<string> GetProductIdByOrderid(int orderid, TaxRequestType typeOfCall);
        ProductGlCode GetProductGlCodes(int GlcodeMappingID);
        /// <summary>
        /// Delete ProductGLCODE
        /// </summary>
        /// <param name="productGlCode"></param>
        void DeleteProductGlCode(ProductGlCode productGlCode);


        void UpdateProductGlCode(ProductGlCode productGlCode);


        /// <summary>
        /// insert the GlCodes for the products
        /// </summary>
        /// <param name="product"></param>
        /// <returns></returns>
        void InsertProductGlCode(ProductGlCode productGlCode);

        /// <summary>
        /// Get GL COde by id
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        GlCode GetGlCodeById(int Id);

        /// <summary>
        /// Get GlCodes
        /// </summary>
        /// <param name="isPaid"></param>
        /// <param name="isDeferred"></param>
        /// <param name="glCodeName"></param>
        /// <returns></returns>
        List<GlCode> GetGlCodes(GLCodeStatusType glCodeType, string glCodeName = "");


        /// <summary>
        /// Get GlCodes
        /// </summary>
        /// <param name="isPaid"></param>
        /// <param name="isDeferred"></param>
        /// <param name="glCodeName"></param>
        /// <returns></returns>
        List<GlCode> GetAllGlCodes();


       
    }
}
