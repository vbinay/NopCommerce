using System;
using System.Linq;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.Plugin.Tax.Vertex.Domain;
using Nop.Services.Events;
using System.Collections.Generic;

namespace Nop.Plugin.Tax.Vertex.Services
{
    /// <summary>
    /// Tax rate service
    /// </summary>
    public partial class VertexTaxRateService : IVertexTaxRateService
    {


        #region Fields

        private readonly IEventPublisher _eventPublisher;
        private readonly IRepository<ProductTax> _taxRateRepository;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="eventPublisher">Event publisher</param>
        /// <param name="cacheManager">Cache manager</param>
        /// <param name="taxRateRepository">Tax rate repository</param>
        public VertexTaxRateService(IEventPublisher eventPublisher,
            IRepository<ProductTax> taxRateRepository)
        {
            this._eventPublisher = eventPublisher;
            this._taxRateRepository = taxRateRepository;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Deletes a tax rate
        /// </summary>
        /// <param name="taxRate">Tax rate</param>
        public virtual void DeleteTaxRate(ProductTax taxRate)
        {
            if (taxRate == null)
                throw new ArgumentNullException("taxRate");

            _taxRateRepository.Delete(taxRate);

            //event notification
            _eventPublisher.EntityDeleted(taxRate);
        }

        /// <summary>
        /// Deletes a tax rate
        /// </summary>
        /// <param name="taxRate">Tax rate</param>
        public virtual void DeleteTaxRatesForProduct(int productId, string sessionId)
        {

            foreach (var productTax in GetAllProductTax(sessionId, productId))
            {
                DeleteTaxRate(productTax);
            }

            //event notification

        }

        public virtual ProductTax CreateOrUpdateProductTax(int productId, string taxCode, string sessionId, decimal tax, int glCodeId = 0, int orderId = 0)
        {
            bool found = false;
           
            foreach (var productTax in GetAllProductTax(sessionId, productId, orderId))
            {
                System.Web.HttpContext.Current.Session["VertexCurrentSessionId"] = sessionId;
                if (productTax.ProductId == productId && productTax.SessionId == sessionId && productTax.GlCodeId == glCodeId && productTax.TaxCode == taxCode)
                {
                    productTax.Tax = tax;
                    productTax.TaxCode = taxCode;
                    this.UpdateTaxRate(productTax);
                    return productTax;
                }
            }

            if (!found)
            {
                var productTax = new ProductTax();
                productTax.GlCodeId = glCodeId;
                productTax.ProductId = productId;
                productTax.Tax = tax;
                productTax.SessionId = sessionId;
                productTax.OrderId = orderId;
                productTax.TaxCode = taxCode;
                this.InsertTaxRate(productTax);
                return productTax;
            }
            return null;

        }

        public virtual void UpdateProductTax(int productId, string taxCode, string sessionId, decimal tax, int glCodeId = 0, int orderId = 0)
        {
            foreach (var productTax in GetAllProductTax(sessionId, productId, orderId))
            {
                if (productTax.ProductId == productId && productTax.SessionId == sessionId && productTax.GlCodeId == glCodeId && productTax.TaxCode == taxCode)
                {
                    productTax.Tax = tax;
                    productTax.TaxCode = taxCode;
                    this.UpdateTaxRate(productTax);
                }
            }
        }

        /// <summary>
        /// Gets all tax rates
        /// </summary>
        /// <returns>Tax rates</returns>
        public virtual IList<ProductTax> GetAllProductTax(string sessionId, int productId = 0, int orderId = 0)
        {
            var query = from tr in _taxRateRepository.Table
                        where tr.SessionId == sessionId
                        select tr;

            if (productId != 0)
            {
                query = query.Where(tr => tr.ProductId == productId);
            }
            if (orderId != 0)
            {
                query = query.Where(tr => tr.OrderId == orderId);
            }

            var records = query.ToList<ProductTax>();
            return records;
        }

        /// <summary>
        /// Gets a tax rate
        /// </summary>
        /// <param name="taxRateId">Tax rate identifier</param>
        /// <returns>Tax rate</returns>
        public virtual ProductTax GetTaxRateById(int taxRateId)
        {
            if (taxRateId == 0)
                return null;

            return _taxRateRepository.GetById(taxRateId);
        }

        /// <summary>
        /// Inserts a tax rate
        /// </summary>
        /// <param name="taxRate">Tax rate</param>
        public virtual void InsertTaxRate(ProductTax taxRate)
        {
            if (taxRate == null)
                throw new ArgumentNullException("taxRate");

            _taxRateRepository.Insert(taxRate);

            //event notification
            _eventPublisher.EntityInserted(taxRate);
        }

        /// <summary>
        /// Updates the tax rate
        /// </summary>
        /// <param name="taxRate">Tax rate</param>
        public virtual void UpdateTaxRate(ProductTax taxRate)
        {
            if (taxRate == null)
                throw new ArgumentNullException("taxRate");

            _taxRateRepository.Update(taxRate);


            //event notification
            _eventPublisher.EntityUpdated(taxRate);
        }

        public virtual ProductTax GetTaxRateBySessionIdProductIdTaxCode(int productId, string taxCode, string sessionId)
        {
            if (productId == 0 || String.IsNullOrEmpty(taxCode) || String.IsNullOrEmpty(sessionId))
                return null;
            var query = from tr in _taxRateRepository.Table
                        where tr.SessionId == sessionId &&
                              tr.ProductId == productId &&
                              tr.TaxCode == taxCode
                        select tr;

            return query.FirstOrDefault();
         }
        #endregion
    }
}
