using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using Nop.Core.Domain.Affiliates;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;


namespace Nop.Core.Domain.Orders
{
    public partial class OrderExcelExport : BaseEntity
    {
        #region Properties

        public virtual int storeid { get; set; }

        public virtual bool isPartialRefund { get; set; }

        public virtual bool isFullRefund { get; set; }
        public string storeName { get; set; }

        public string storeSate { get; set; }

        public string storeZip { get; set; }

        public string storeCityAb { get; set; }
        /// <summary>
        /// Gets or sets the source site identifier
        /// </summary>
        public virtual int OrderId { get; set; }


        /// <summary>
        /// Gets or sets the source site identifier
        /// </summary>
        public virtual int OrderItemId { get; set; }

        /// <summary>
        /// Gets or sets the source site identifier
        /// </summary>
        public virtual int SourceSiteId { get; set; }

        /// <summary>
        /// Gets or sets the order identifier
        /// </summary>
        public virtual Guid OrderGuid { get; set; }

        /// <summary>
        /// Gets or sets the customer identifier
        /// </summary>
        public virtual int CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the source site identifier
        /// </summary>
        public virtual int ProductId { get; set; }

        /// <summary>
        /// Gets or sets the order product name
        /// </summary>
        public virtual string ProductName { get; set; }


        /// <summary>
        /// Gets or sets the order product name
        /// </summary>
        public virtual string LocalProductName { get; set; }

        /// <summary>
        /// Gets or sets the order category name
        /// </summary>
        public virtual string Category { get; set; }

        /// <summary>
        /// Gets or sets the CategoryId
        /// </summary>
        public virtual int CategoryId { get; set; }

        /// <summary>
        /// Gets or sets the order G/L Code name
        /// </summary>
        public virtual string GLCode { get; set; }

        /// <summary>
        /// Gets or sets the order G/L Code Id
        /// </summary>
        public virtual int GLCodeId { get; set; }

        /// <summary>
        /// Gets or sets the billing address identifier
        /// </summary>
        public virtual int BillingAddressId { get; set; }

        /// <summary>
        /// Gets or sets the shipping address identifier
        /// </summary>
        public virtual int? ShippingAddressId { get; set; }

        /// <summary>
        /// Gets or sets an order status identifer
        /// </summary>
        public virtual int OrderStatusId { get; set; }

        /// <summary>
        /// Gets or sets the shipping status identifier
        /// </summary>
        public virtual int ShippingStatusId { get; set; }

        /// <summary>
        /// Gets or sets the payment status identifier
        /// </summary>
        public virtual int PaymentStatusId { get; set; }

        /// <summary>
        /// Gets or sets the payment method system name
        /// </summary>
        public virtual string PaymentMethodSystemName { get; set; }

        /// <summary>
        /// Gets or sets the customer currency code (at the moment of order placing)
        /// </summary>
        public virtual string CustomerCurrencyCode { get; set; }

        /// <summary>
        /// Gets or sets the currency rate
        /// </summary>
        public virtual decimal CurrencyRate { get; set; }

        /// <summary>
        /// Gets or sets the customer tax display type identifier
        /// </summary>
        public virtual int CustomerTaxDisplayTypeId { get; set; }

        /// <summary>
        /// Gets or sets the VAT number (the European Union Value Added Tax)
        /// </summary>
        public virtual string VatNumber { get; set; }

        /// <summary>
        /// Gets or sets the order subtotal (incl tax)
        /// </summary>
        public virtual decimal OrderSubtotalInclTax { get; set; }

        /// <summary>
        /// Gets or sets the order subtotal (excl tax)
        /// </summary>
        public virtual decimal OrderSubtotalExclTax { get; set; }

        /// <summary>
        /// Gets or sets the order subtotal (excl tax)
        /// </summary>
        public virtual decimal SiteProductPriceExclTax { get; set; }

        /// <summary>
        /// Gets or sets the order subtotal discount (incl tax)
        /// </summary>
        public virtual decimal OrderSubTotalDiscountInclTax { get; set; }

        /// <summary>
        /// Gets or sets the order subtotal discount (excl tax)
        /// </summary>
        public virtual decimal OrderSubTotalDiscountExclTax { get; set; }

        /// <summary>
        /// Gets or sets the order shipping (incl tax)
        /// </summary>
        public virtual decimal OrderShippingInclTax { get; set; }

        /// <summary>
        /// Gets or sets the order shipping (excl tax)
        /// </summary>
        public virtual decimal OrderShippingExclTax { get; set; }

        /// <summary>
        /// Gets or sets the payment method additional fee (incl tax)
        /// </summary>
        public virtual decimal PaymentMethodAdditionalFeeInclTax { get; set; }

        /// <summary>
        /// Gets or sets the payment method additional fee (excl tax)
        /// </summary>
        public virtual decimal PaymentMethodAdditionalFeeExclTax { get; set; }

        /// <summary>
        /// Gets or sets the tax rates
        /// </summary>
        public virtual string TaxRates { get; set; }

        /// <summary>
        /// Gets or sets the order tax
        /// </summary>
        public virtual decimal OrderTax { get; set; }

        /// <summary>
        /// Gets or sets the siteproduct tax
        /// </summary>
        public virtual decimal SiteProductTax { get; set; }

        /// <summary>
        /// Gets or sets the order discount (applied to order total)
        /// </summary>
        public virtual decimal OrderDiscount { get; set; }

        /// <summary>
        /// Gets or sets the order total
        /// </summary>
        public virtual decimal OrderTotal { get; set; }

        /// <summary>
        /// Gets or sets the order total
        /// </summary>
        public virtual decimal SiteProductTotal { get; set; }

        /// <summary>
        /// Gets or sets the gift card amount applied to order
        /// </summary>
        public virtual decimal? GiftCardCredit { get; set; }

        /// <summary>
        /// Gets or sets the refunded amount
        /// </summary>
        public virtual decimal RefundedAmount { get; set; }

        /// <summary>
        /// Gets or sets the value indicating whether reward points were earned for this order
        /// </summary>
        public virtual bool RewardPointsWereAdded { get; set; }

        /// <summary>
        /// Gets or sets the checkout attribute description
        /// </summary>
        public virtual string CheckoutAttributeDescription { get; set; }

        /// <summary>
        /// Gets or sets the checkout attributes in XML format
        /// </summary>
        public virtual string CheckoutAttributesXml { get; set; }

        /// <summary>
        /// Gets or sets the customer language identifier
        /// </summary>
        public virtual int CustomerLanguageId { get; set; }

        /// <summary>
        /// Gets or sets the affiliate identifier
        /// </summary>
        public virtual int? AffiliateId { get; set; }

        /// <summary>
        /// Gets or sets the customer IP address
        /// </summary>
        public virtual string CustomerIp { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether storing of credit card number is allowed
        /// </summary>
        public virtual bool AllowStoringCreditCardNumber { get; set; }

        /// <summary>
        /// Gets or sets the card type
        /// </summary>
        public virtual string CardType { get; set; }

        /// <summary>
        /// Gets or sets the card name
        /// </summary>
        public virtual string CardName { get; set; }

        /// <summary>
        /// Gets or sets the card number
        /// </summary>
        public virtual string CardNumber { get; set; }

        /// <summary>
        /// Gets or sets the masked credit card number
        /// </summary>
        public virtual string MaskedCreditCardNumber { get; set; }

        /// <summary>
        /// Gets or sets the card CVV2
        /// </summary>
        public virtual string CardCvv2 { get; set; }

        /// <summary>
        /// Gets or sets the card expiration month
        /// </summary>
        public virtual string CardExpirationMonth { get; set; }

        /// <summary>
        /// Gets or sets the card expiration year
        /// </summary>
        public virtual string CardExpirationYear { get; set; }

        /// <summary>
        /// Gets or sets the authorization transaction identifier
        /// </summary>
        public virtual string AuthorizationTransactionId { get; set; }

        /// <summary>
        /// Gets or sets the authorization transaction code
        /// </summary>
        public virtual string AuthorizationTransactionCode { get; set; }

        /// <summary>
        /// Gets or sets the authorization transaction result
        /// </summary>
        public virtual string AuthorizationTransactionResult { get; set; }

        /// <summary>
        /// Gets or sets the capture transaction identifier
        /// </summary>
        public virtual string CaptureTransactionId { get; set; }

        /// <summary>
        /// Gets or sets the capture transaction result
        /// </summary>
        public virtual string CaptureTransactionResult { get; set; }

        /// <summary>
        /// Gets or sets the subscription transaction identifier
        /// </summary>
        public virtual string SubscriptionTransactionId { get; set; }

        /// <summary>
        /// Gets or sets the purchase order number
        /// </summary>
        public virtual string PurchaseOrderNumber { get; set; }

        /// <summary>
        /// Gets or sets the paid date and time
        /// </summary>
        public virtual DateTime? PaidDateUtc { get; set; }

        /// <summary>
        /// Gets or sets the shipping method
        /// </summary>
     //   public virtual string ShippingMethod { get; set; }

        /// <summary>
        /// Gets or sets the shipping rate computation method identifier
        /// </summary>
       // public virtual string ShippingRateComputationMethodSystemName { get; set; }

        /// <summary>
        /// Gets or sets the shipped date and time
        /// </summary>
        public virtual DateTime? ShippedDateUtc { get; set; }

        /// <summary>
        /// Gets or sets the delivery date and time
        /// </summary>
        public virtual DateTime? DeliveryDateUtc { get; set; }

        /// <summary>
        /// Gets or sets the delivery date and time
        /// </summary>
        public virtual decimal DeliveryAmountInclTax { get; set; }

        public virtual decimal DeliveryAmountExclTax { get; set; }

        /// <summary>
        /// Gets or sets the order weight
        /// </summary>
        public virtual decimal OrderWeight { get; set; }

        /// <summary>
        /// Gets or sets the tracking number of current order
        /// </summary>
        public virtual string TrackingNumber { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entity has been deleted
        /// </summary>
        public virtual bool Deleted { get; set; }

        /// <summary>
        /// Gets or sets the date and time of order creation
        /// </summary>
        public virtual DateTime CreatedOnUtc { get; set; }

        /// <summary>
        /// Gets or sets the requested delivery date
        /// </summary>
        public virtual DateTime? RequestedFulfillmentDate { get; set; }

        /// <summary>
        /// Gets or sets the OrderSessionKey
        /// </summary>
        public virtual string OrderSessionKey { get; set; }

        /// <summary>
        /// Gets or sets the site ExternalKey
        /// </summary>
        public virtual string SiteExtKey { get; set; }

        /// <summary>
        /// Gets or sets the site Name
        /// </summary>
        public virtual string SiteName { get; set; }


        /// <summary>
        /// Gets or sets the accountnumber
        /// </summary>
        public virtual string AccountNumber { get; set; }
        public virtual string StoreStateProvince { get; set; }
        public virtual string StoreZipCode { get; set; }
        public virtual string StoreCity { get; set; }
        public virtual string LegalEntity { get; set; }
        public virtual string ShipFromState { get; set; }
        public virtual string ShipFromZipCode { get; set; }
        public virtual string ShipFromCity { get; set; }

        public virtual string ShiptoState { get; set; }
        public virtual string ShipToZipCode { get; set; }
        public virtual string ShippedToCity { get; set; }
        public virtual decimal? Amount { get; set; }
        public virtual DateTime? paymentDate { get; set; }
        public string PaidDate { get; set; }
        public string RefundedDate { get; set;}
        public virtual DateTime? DateOfRefund { get; set; }
        public virtual string PaymentType { get; set; }
        public virtual string VertexTaxAreaId { get; set; }

        public virtual decimal? CovidGLAmount1 { get; set; }
        public virtual decimal? CovidGLAmount2 { get; set; }
        public virtual decimal? CovidGLAmount3 { get; set; }
        public virtual decimal? CovidTaxAmount1 { get; set; }
        public virtual decimal? CovidTaxAmount2 { get; set; }
        public virtual decimal? CovidTaxAmount3 { get; set; }

        #endregion

        #region Custom properties
        //TODO: Not sure if I need these below may need to remove later.
        /// <summary>
        /// Gets or sets the order status
        /// </summary>
        public virtual OrderStatus OrderStatus
        {
            get
            {
                return (OrderStatus)this.OrderStatusId;
            }
            set
            {
                this.OrderStatusId = (int)value;
            }
        }

        /// <summary>
        /// Gets or sets the payment status
        /// </summary>
        public virtual PaymentStatus PaymentStatus
        {
            get
            {
                return (PaymentStatus)this.PaymentStatusId;
            }
            set
            {
                this.PaymentStatusId = (int)value;
            }
        }

        /// <summary>
        /// Gets or sets the shipping status
        /// </summary>
        public virtual ShippingStatus ShippingStatus
        {
            get
            {
                return (ShippingStatus)this.ShippingStatusId;
            }
            set
            {
                this.ShippingStatusId = (int)value;
            }
        }




        #endregion
    }

    public partial class NewOrderSiteProductExcelExport : BaseEntity
    {

    }

    public partial class NewOrderExcelExport : BaseEntity
    {
        #region Properties
        /// <summary>
        /// Gets or sets the source site identifier
        /// </summary>
        public virtual int OrderId { get; set; }

        /// <summary>
        /// Gets or sets the source site identifier
        /// </summary>
        public virtual int SourceSiteId { get; set; }

        /// <summary>
        /// Gets or sets the order identifier
        /// </summary>
        public virtual Guid OrderGuid { get; set; }

        /// <summary>
        /// Gets or sets the customer identifier
        /// </summary>
        public virtual int CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the source site identifier
        /// </summary>
        public virtual int ProductId { get; set; }

        /// <summary>
        /// Gets or sets the order product name
        /// </summary>
        public virtual string ProductName { get; set; }


        /// <summary>
        /// Gets or sets the order product name
        /// </summary>
        public virtual string SiteProductName { get; set; }

        /// <summary>
        /// Gets or sets the order category name
        /// </summary>
        public virtual string Category { get; set; }

        /// <summary>
        /// Gets or sets the CategoryId
        /// </summary>
        public virtual int CategoryId { get; set; }

        /// <summary>
        /// Gets or sets the order G/L Code name
        /// </summary>
        public virtual string GLCode { get; set; }

        /// <summary>
        /// Gets or sets the order G/L Code Id
        /// </summary>
        public virtual int GLCodeId { get; set; }

        /// <summary>
        /// Gets or sets the billing address identifier
        /// </summary>
        public virtual int BillingAddressId { get; set; }

        /// <summary>
        /// Gets or sets the shipping address identifier
        /// </summary>
        public virtual int? ShippingAddressId { get; set; }

        /// <summary>
        /// Gets or sets an order status identifer
        /// </summary>
        public virtual int OrderStatusId { get; set; }

        /// <summary>
        /// Gets or sets the shipping status identifier
        /// </summary>
        public virtual int ShippingStatusId { get; set; }

        /// <summary>
        /// Gets or sets the payment status identifier
        /// </summary>
        public virtual int PaymentStatusId { get; set; }

        /// <summary>
        /// Gets or sets the payment method system name
        /// </summary>
        public virtual string PaymentMethodSystemName { get; set; }

        /// <summary>
        /// Gets or sets the customer currency code (at the moment of order placing)
        /// </summary>
        public virtual string CustomerCurrencyCode { get; set; }

        /// <summary>
        /// Gets or sets the currency rate
        /// </summary>
        public virtual decimal CurrencyRate { get; set; }

        /// <summary>
        /// Gets or sets the customer tax display type identifier
        /// </summary>
        public virtual int CustomerTaxDisplayTypeId { get; set; }

        /// <summary>
        /// Gets or sets the VAT number (the European Union Value Added Tax)
        /// </summary>
        public virtual string VatNumber { get; set; }

        /// <summary>
        /// Gets or sets the order subtotal (incl tax)
        /// </summary>
        public virtual decimal OrderSubtotalInclTax { get; set; }

        /// <summary>
        /// Gets or sets the order subtotal (excl tax)
        /// </summary>
        public virtual decimal OrderSubtotalExclTax { get; set; }

        /// <summary>
        /// Gets or sets the order subtotal (excl tax)
        /// </summary>
        public virtual decimal SiteProductPriceExclTax { get; set; }

        /// <summary>
        /// Gets or sets the order subtotal discount (incl tax)
        /// </summary>
        public virtual decimal OrderSubTotalDiscountInclTax { get; set; }

        /// <summary>
        /// Gets or sets the order subtotal discount (excl tax)
        /// </summary>
        public virtual decimal OrderSubTotalDiscountExclTax { get; set; }

        /// <summary>
        /// Gets or sets the order shipping (incl tax)
        /// </summary>
        public virtual decimal OrderShippingInclTax { get; set; }

        /// <summary>
        /// Gets or sets the order shipping (excl tax)
        /// </summary>
        public virtual decimal OrderShippingExclTax { get; set; }

        /// <summary>
        /// Gets or sets the payment method additional fee (incl tax)
        /// </summary>
        public virtual decimal PaymentMethodAdditionalFeeInclTax { get; set; }

        /// <summary>
        /// Gets or sets the payment method additional fee (excl tax)
        /// </summary>
        public virtual decimal PaymentMethodAdditionalFeeExclTax { get; set; }

        /// <summary>
        /// Gets or sets the tax rates
        /// </summary>
        public virtual string TaxRates { get; set; }

        /// <summary>
        /// Gets or sets the order tax
        /// </summary>
        public virtual decimal OrderTax { get; set; }

        /// <summary>
        /// Gets or sets the siteproduct tax
        /// </summary>
        public virtual decimal SiteProductTax { get; set; }

        /// <summary>
        /// Gets or sets the order discount (applied to order total)
        /// </summary>
        public virtual decimal OrderDiscount { get; set; }

        /// <summary>
        /// Gets or sets the order total
        /// </summary>
        public virtual decimal OrderTotal { get; set; }

        /// <summary>
        /// Gets or sets the order total
        /// </summary>
        public virtual decimal SiteProductTotal { get; set; }

        /// <summary>
        /// Gets or sets the gift card amount applied to order
        /// </summary>
        public virtual decimal? GiftCardCredit { get; set; }

        /// <summary>
        /// Gets or sets the refunded amount
        /// </summary>
        public virtual decimal RefundedAmount { get; set; }

        /// <summary>
        /// Gets or sets the value indicating whether reward points were earned for this order
        /// </summary>
        public virtual bool RewardPointsWereAdded { get; set; }

        /// <summary>
        /// Gets or sets the checkout attribute description
        /// </summary>
        public virtual string CheckoutAttributeDescription { get; set; }

        /// <summary>
        /// Gets or sets the checkout attributes in XML format
        /// </summary>
        public virtual string CheckoutAttributesXml { get; set; }

        /// <summary>
        /// Gets or sets the customer language identifier
        /// </summary>
        public virtual int CustomerLanguageId { get; set; }

        /// <summary>
        /// Gets or sets the affiliate identifier
        /// </summary>
        public virtual int? AffiliateId { get; set; }

        /// <summary>
        /// Gets or sets the customer IP address
        /// </summary>
        public virtual string CustomerIp { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether storing of credit card number is allowed
        /// </summary>
        public virtual bool AllowStoringCreditCardNumber { get; set; }

        /// <summary>
        /// Gets or sets the card type
        /// </summary>
        public virtual string CardType { get; set; }

        /// <summary>
        /// Gets or sets the card name
        /// </summary>
        public virtual string CardName { get; set; }

        /// <summary>
        /// Gets or sets the card number
        /// </summary>
        public virtual string CardNumber { get; set; }

        /// <summary>
        /// Gets or sets the masked credit card number
        /// </summary>
        public virtual string MaskedCreditCardNumber { get; set; }

        /// <summary>
        /// Gets or sets the card CVV2
        /// </summary>
        public virtual string CardCvv2 { get; set; }

        /// <summary>
        /// Gets or sets the card expiration month
        /// </summary>
        public virtual string CardExpirationMonth { get; set; }

        /// <summary>
        /// Gets or sets the card expiration year
        /// </summary>
        public virtual string CardExpirationYear { get; set; }

        /// <summary>
        /// Gets or sets the authorization transaction identifier
        /// </summary>
        public virtual string AuthorizationTransactionId { get; set; }

        /// <summary>
        /// Gets or sets the authorization transaction code
        /// </summary>
        public virtual string AuthorizationTransactionCode { get; set; }

        /// <summary>
        /// Gets or sets the authorization transaction result
        /// </summary>
        public virtual string AuthorizationTransactionResult { get; set; }

        /// <summary>
        /// Gets or sets the capture transaction identifier
        /// </summary>
        public virtual string CaptureTransactionId { get; set; }

        /// <summary>
        /// Gets or sets the capture transaction result
        /// </summary>
        public virtual string CaptureTransactionResult { get; set; }

        /// <summary>
        /// Gets or sets the subscription transaction identifier
        /// </summary>
        public virtual string SubscriptionTransactionId { get; set; }

        /// <summary>
        /// Gets or sets the purchase order number
        /// </summary>
        public virtual string PurchaseOrderNumber { get; set; }

        /// <summary>
        /// Gets or sets the paid date and time
        /// </summary>
        public virtual DateTime? PaidDateUtc { get; set; }

        /// <summary>
        /// Gets or sets the shipping method
        /// </summary>
        //   public virtual string ShippingMethod { get; set; }

        /// <summary>
        /// Gets or sets the shipping rate computation method identifier
        /// </summary>
        // public virtual string ShippingRateComputationMethodSystemName { get; set; }

        /// <summary>
        /// Gets or sets the shipped date and time
        /// </summary>
        public virtual DateTime? ShippedDateUtc { get; set; }

        /// <summary>
        /// Gets or sets the delivery date and time
        /// </summary>
        public virtual DateTime? DeliveryDateUtc { get; set; }

        /// <summary>
        /// Gets or sets the order weight
        /// </summary>
        public virtual decimal OrderWeight { get; set; }

        /// <summary>
        /// Gets or sets the tracking number of current order
        /// </summary>
        public virtual string TrackingNumber { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entity has been deleted
        /// </summary>
        public virtual bool Deleted { get; set; }

        /// <summary>
        /// Gets or sets the date and time of order creation
        /// </summary>
        public virtual DateTime CreatedOnUtc { get; set; }

        /// <summary>
        /// Gets or sets the requested delivery date
        /// </summary>
        public virtual DateTime? RequestedFulfillmentDate { get; set; }

        /// <summary>
        /// Gets or sets the OrderSessionKey
        /// </summary>
        public virtual string OrderSessionKey { get; set; }

        /// <summary>
        /// Gets or sets the site ExternalKey
        /// </summary>
        public virtual string SiteExtKey { get; set; }

        /// <summary>
        /// Gets or sets the site Name
        /// </summary>
        public virtual string SiteName { get; set; }


        /// <summary>
        /// Gets or sets the accountnumber
        /// </summary>
        public virtual string AccountNumber { get; set; }

        #endregion

        #region Custom properties
        //TODO: Not sure if I need these below may need to remove later.
        /// <summary>
        /// Gets or sets the order status
        /// </summary>
        public virtual OrderStatus OrderStatus
        {
            get
            {
                return (OrderStatus)this.OrderStatusId;
            }
            set
            {
                this.OrderStatusId = (int)value;
            }
        }

        /// <summary>
        /// Gets or sets the payment status
        /// </summary>
        public virtual PaymentStatus PaymentStatus
        {
            get
            {
                return (PaymentStatus)this.PaymentStatusId;
            }
            set
            {
                this.PaymentStatusId = (int)value;
            }
        }

        /// <summary>
        /// Gets or sets the shipping status
        /// </summary>
        public virtual ShippingStatus ShippingStatus
        {
            get
            {
                return (ShippingStatus)this.ShippingStatusId;
            }
            set
            {
                this.ShippingStatusId = (int)value;
            }
        }




        #endregion
    }

    public class AttributeReportExcelRow
    {



        public string UnitName { get; set; }
        public string ProductName { get; set; }
        public string AttributeType { get; set; }
        public string AttributeName { get; set; }
        public decimal Total { get; set; }
        public int LocalProductId { get; set; }



    }
}
