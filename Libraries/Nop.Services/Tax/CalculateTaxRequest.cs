using Nop.Core.Domain.Common;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;

namespace Nop.Services.Tax
{
    /// <summary>
    /// Represents a request for tax calculation
    /// </summary>
    public partial class CalculateTaxRequest
    {
        /// <summary>
        /// Gets or sets a customer
        /// </summary>
        public Customer Customer { get; set; }

        /// <summary>
        /// Gets or sets a product
        /// </summary>
        public Product Product { get; set; }

      
        /// <summary>
        /// Gets or sets an address
        /// </summary>
        public Address ShipToAddress { get; set; }


        public Address ShipFromAddress { get; set; }

        
        /// <summary>
        /// Gets or sets a tax category identifier
        /// </summary>
        public int TaxCategoryId { get; set; }

        /// <summary>
        /// Gets or sets a price
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Storing the request type whether Quotation or Distributive
        /// </summary>
        public int TaxRequestType { get; set; }

        /// <summary>
        /// Storing the request type whether Quotation or Distributive
        /// </summary>
        public bool OnSite { get; set; }

        public int? Quantity { get; set; }

        public bool IsTieredShippingEnabled { get; set; }

        public bool orderPlaced { get; set; }

    }
}
