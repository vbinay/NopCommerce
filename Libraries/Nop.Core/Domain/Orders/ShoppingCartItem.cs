using System;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Shipping;

namespace Nop.Core.Domain.Orders
{
    /// <summary>
    /// Represents a shopping cart item
    /// </summary>
    public partial class ShoppingCartItem : BaseEntity
    {
        /// <summary>
        /// Gets or sets the store identifier
        /// </summary>
        public int StoreId { get; set; }


        public string ReservedTimeSlot { get; set; }

        public DateTime ReservationDate { get; set; }

        public bool IsReservation { get; set; }

        public bool IsBundleProduct { get; set; }

        public bool IsTieredShippingEnabled { get; set; }
        public decimal FlatShipping { get; set; }

        public bool IsContractShippingEnabled { get; set; }

        public string RCNumber { get; set; }

        public bool IsInterOfficeDeliveryEnabled { get; set; }

        public string MailStopAddress { get; set; }

        public bool IsFirstCartItem { get; set; }
        /// <summary>
        /// Gets or sets the shopping cart type identifier
        /// </summary>
        public int ShoppingCartTypeId { get; set; }

        /// <summary>
        /// Gets or sets the customer identifier
        /// </summary>
        public int CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the product identifier
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// Gets or sets the product attributes in XML format
        /// </summary>
        public string AttributesXml { get; set; }

        /// <summary>
        /// Gets or sets the price enter by a customer
        /// </summary>
        public decimal CustomerEnteredPrice { get; set; }

        /// <summary>
        /// Gets or sets the quantity
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Gets or sets the rental product start date (null if it's not a rental product)
        /// </summary>
        public DateTime? RentalStartDateUtc { get; set; }

        /// <summary>
        /// Gets or sets the rental product end date (null if it's not a rental product)
        /// </summary>
        public DateTime? RentalEndDateUtc { get; set; }

        /// <summary>
        /// Gets or sets the date and time of instance creation
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }

        /// <summary>
        /// Gets or sets the date and time of instance update
        /// </summary>
        public DateTime UpdatedOnUtc { get; set; }

        /// <summary>
        /// Gets the log type
        /// </summary>
        public ShoppingCartType ShoppingCartType
        {
            get
            {
                return (ShoppingCartType)this.ShoppingCartTypeId;
            }
            set
            {
                this.ShoppingCartTypeId = (int)value;
            }
        }

        /// <summary>
        /// Gets or sets the product
        /// </summary>
        public virtual Product Product { get; set; }

        /// <summary>
        /// Gets or sets the customer
        /// </summary>
        public virtual Customer Customer { get; set; }

        /// <summary>
        /// Gets a value indicating whether the shopping cart item is free shipping
        /// </summary>
        public bool IsFreeShipping
        {
            get
            {
                var product = this.Product;
                if (product != null)
                    return product.IsFreeShipping;
                return true;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the shopping cart item is ship enabled
        /// </summary>
        public bool IsShipEnabled
        {
            get
            {
                var product = this.Product;
                if (product != null)
                {
					#region NU-16
                    if(product.IsMealPlan)
                    {
                        return false;
                    }
					#endregion
                    return product.IsShipEnabled;
                }
                return false;
            }
        }

        /// <summary>
        /// Gets the additional shipping charge
        /// </summary> 
        public decimal AdditionalShippingCharge
        {
            get
            {
                decimal additionalShippingCharge = decimal.Zero;
                var product = this.Product;
                if (product != null)
                    additionalShippingCharge = product.AdditionalShippingCharge * Quantity;
                return additionalShippingCharge;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the shopping cart item is tax exempt
        /// </summary>
        public bool IsTaxExempt
        {
            get
            {
                var product = this.Product;
                if (product != null)
                    return product.IsTaxExempt;
                return false;
            }
        }

		#region NU-31
        /// <summary>
        /// Gets or sets the date and time of instance update
        /// </summary>
        public DateTime? RequestedFullfilmentDateTime{ get; set; }

        /// <summary>
        ///Selected Warehouse Id
        /// </summary>
        public virtual int? SelectedWarehouseId { get; set; }

        /// <summary>
        ///Selected Warehouse
        /// </summary>
        public virtual Warehouse SelectedWarehouse { get; set; }

        /// <summary>
        /// Gets or sets the ShippingMethod
        /// </summary>
        public virtual string SelectedShippingMethodName { get; set; }

        /// <summary>
        /// Gets or sets the ShippingMethodRateCompName
        /// SODMYWAY-3467
        /// </summary>
        public virtual string SelectedShippingRateComputationMethodSystemName { get; set; }
		#endregion
    }
}
