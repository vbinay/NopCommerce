using System;
using System.Collections.Generic;
using Nop.Core.Domain.Catalog;

namespace Nop.Core.Domain.Orders
{
    /// <summary>
    /// Represents an order item
    /// </summary>
    public partial class OrderItem : BaseEntity
    {
        private ICollection<GiftCard> _associatedGiftCards;

        /// <summary>
        /// Gets or sets the order item identifier
        /// </summary>
        public Guid OrderItemGuid { get; set; }

        public decimal PartialRefundOrderAmount { get; set; }
        /// <summary>
        /// Gets or sets the order identifier
        /// </summary>
        public int OrderId { get; set; }

        /// <summary>
        /// Gets or sets the product identifier
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// Gets or sets the quantity
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Gets or sets the unit price in primary store currency (incl tax)
        /// </summary>
        public decimal UnitPriceInclTax { get; set; }

        /// <summary>
        /// Gets or sets the unit price in primary store currency (excl tax)
        /// </summary>
        public decimal UnitPriceExclTax { get; set; }

        /// <summary>
        /// Gets or sets the price in primary store currency (incl tax)
        /// </summary>
        public decimal PriceInclTax { get; set; }

        /// <summary>
        /// Gets or sets the price in primary store currency (excl tax)
        /// </summary>
        public decimal PriceExclTax { get; set; }

        /// <summary>
        /// Gets or sets the discount amount (incl tax)
        /// </summary>
        public decimal DiscountAmountInclTax { get; set; }

        /// <summary>
        /// Gets or sets the discount amount (excl tax)
        /// </summary>
        public decimal DiscountAmountExclTax { get; set; }

        /// <summary>
        /// Gets or sets the original cost of this order item (when an order was placed), qty 1
        /// </summary>
        public decimal OriginalProductCost { get; set; }

        /// <summary>
        /// Gets or sets the attribute description
        /// </summary>
        public string AttributeDescription { get; set; }

        /// <summary>
        /// Gets or sets the product attributes in XML format
        /// </summary>
        public string AttributesXml { get; set; }

        /// <summary>
        /// Gets or sets the download count
        /// </summary>
        public int DownloadCount { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether download is activated
        /// </summary>
        public bool IsDownloadActivated { get; set; }

        /// <summary>
        /// Gets or sets a license download identifier (in case this is a downloadable product)
        /// </summary>
        public int? LicenseDownloadId { get; set; }

        /// <summary>
        /// Gets or sets the total weight of one item
        /// It's nullable for compatibility with the previous version of nopCommerce where was no such property
        /// </summary>
        public decimal? ItemWeight { get; set; }

        /// <summary>
        /// Gets or sets the rental product start date (null if it's not a rental product)
        /// </summary>
        public DateTime? RentalStartDateUtc { get; set; }

        /// <summary>
        /// Gets or sets the rental product end date (null if it's not a rental product)
        /// </summary>
        public DateTime? RentalEndDateUtc { get; set; }

        /// <summary>
        /// Gets the order
        /// </summary>
        public virtual Order Order { get; set; }

        /// <summary>
        /// Gets the product
        /// </summary>
        public virtual Product Product { get; set; }

        /// <summary>
        /// Gets or sets the associated gift card
        /// </summary>
        public virtual ICollection<GiftCard> AssociatedGiftCards
        {
            get { return _associatedGiftCards ?? (_associatedGiftCards = new List<GiftCard>()); }
            protected set { _associatedGiftCards = value; }
        }

        #region NU-31
        /// <summary>
        /// Gets or sets the date and time of requested fulfillment date (local)
        /// </summary>
        public virtual DateTime? RequestedFulfillmentDateTime { get; set; }

        public virtual DateTime? FulfillmentDateTime { get; set; }

        /// <summary>
        /// Gets,Sets the selected warehouse to do the fulfillment
        /// </summary>
        public virtual int? SelectedFulfillmentWarehouseId { get; set; }

        /// <summary>
        /// Gets, Set the selected shipping method name (used for displaying on notifications)
        /// </summary> 
        public virtual string SelectedShippingMethodName { get; set; }

        public virtual string SelectedShippingRateComputationMethodSystemName { get; set; }


        public virtual Nop.Core.Domain.Shipping.Warehouse SelectedFulfillmentWarehouse { get; set; }
        #endregion

        public decimal? StoreCommission { get; set; } /// NU-15
                                                      /// 
        public virtual decimal DeliveryAmountExclTax { get; set; } //NU-90

        public decimal? GlcodeAmount1 { get; set; }

        public decimal? TaxAmount1 { get; set; }

        public decimal? GlcodeAmount2 { get; set; }

        public decimal? TaxAmount2 { get; set; }

        public decimal? GlcodeAmount3 { get; set; }

        public decimal? TaxAmount3 { get; set; }

        public decimal? TotalRefundedAmount { get; set; }

        public bool IsPartialRefund { get; set; }
        public bool IsFullRefund { get; set; }

        public decimal DelliveryTax { get; set; }
        public decimal DeliveryPickupAmount { get; set; }
        public decimal ShippingTax { get; set; }
        public decimal ShippingAmount { get; set; }
        public decimal? RefundedTaxAmount1 { get; set; }
        public decimal? RefundedGlAmount1 { get; set; }

        public decimal? RefundedTaxAmount2 { get; set; }
        public decimal? RefundedGlAmount2 { get; set; }

        public decimal? RefundedTaxAmount3 { get; set; }
        public decimal? RefundedGlAmount3 { get; set; }

        public decimal? DeliveryPickupFee { get; set; }
        public decimal? ShippingFee { get; set; }

        public string GLCodeName1 { get; set; }
        public string GLCodeName2 { get; set; }
        public string GLCodeName3 { get; set; }
        public string TaxName1 { get; set; }
        public string TaxName2 { get; set; }
        public string TaxName3 { get; set; }

        public bool IsDeliveryPickUp { get; set; }
        public bool IsShipping { get; set; }

        public bool IsReservation { get; set; }

        public bool IsBundleProduct { get; set; }

        public DateTime? DateOfRefund { get; set; }

        public string RCNumber { get; set; }
        public string MailStopAddress { get; set; }
    }
}
