using Nop.Core;
using Nop.Plugin.Tax.Vertex.Domain;
using System.Collections.Generic;

namespace Nop.Plugin.Tax.Vertex.Services
{
    /// <summary>
    /// Tax rate service interface
    /// </summary>
    public partial interface IVertexTaxRateService
    {
        /// <summary>
        /// Deletes a tax rate
        /// </summary>
        /// <param name="taxRate">Tax rate</param>
        void DeleteTaxRate(ProductTax taxRate);

        void DeleteTaxRatesForProduct(int productId, string sessionId);

        /// <summary>
        /// Gets all tax rates
        /// </summary>
        /// <returns>Tax rates</returns>
        IList<ProductTax> GetAllProductTax(string sessionId,int productId = 0,  int orderId = 0);

        ProductTax CreateOrUpdateProductTax(int productId,string taxCode, string sessionId, decimal tax, int glCodeId = 0, int orderId = 0);

        /// <summary>
        /// Gets a tax rate
        /// </summary>
        /// <param name="taxRateId">Tax rate identifier</param>
        /// <returns>Tax rate</returns>
        ProductTax GetTaxRateById(int taxRateId);

        /// <summary>
        /// Inserts a tax rate
        /// </summary>
        /// <param name="taxRate">Tax rate</param>
        void InsertTaxRate(ProductTax taxRate);

        /// <summary>
        /// Updates the tax rate
        /// </summary>
        /// <param name="taxRate">Tax rate</param>
        void UpdateTaxRate(ProductTax taxRate);

        /// <summary>
        /// Returns the tax rate row based on sessionid/taxcode and product id
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="taxCode"></param>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        ProductTax GetTaxRateBySessionIdProductIdTaxCode(int productId, string taxCode, string sessionId);
    }
}
