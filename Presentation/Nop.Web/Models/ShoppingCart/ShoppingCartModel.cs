using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Web.Framework.Mvc;
using Nop.Web.Models.Checkout;
using Nop.Web.Models.Common;
using Nop.Web.Models.Media;
using Nop.Core.Domain.Shipping;

namespace Nop.Web.Models.ShoppingCart
{
    public partial class ShoppingCartModel : BaseNopModel
    {
        public ShoppingCartModel()
        {
            Items = new List<ShoppingCartItemModel>();
            Warnings = new List<string>();
            EstimateShipping = new EstimateShippingModel();
            DiscountBox = new DiscountBoxModel();
            GiftCardBox = new GiftCardBoxModel();
            CheckoutAttributes = new List<CheckoutAttributeModel>();
            OrderReviewData = new OrderReviewDataModel();

            ButtonPaymentMethodActionNames = new List<string>();
            ButtonPaymentMethodControllerNames = new List<string>();
            ButtonPaymentMethodRouteValues = new List<RouteValueDictionary>();
        }

        public bool OnePageCheckoutEnabled { get; set; }

        public bool isPaypalEnabled { get; set; }
        public bool ShowSku { get; set; }
        public bool ShowProductImages { get; set; }
        public bool IsEditable { get; set; }
        public IList<ShoppingCartItemModel> Items { get; set; }

        public string CheckoutAttributeInfo { get; set; }
        public IList<CheckoutAttributeModel> CheckoutAttributes { get; set; }

        public IList<string> Warnings { get; set; }
        public string MinOrderSubtotalWarning { get; set; }
        public bool DisplayTaxShippingInfo { get; set; }
        public bool TermsOfServiceOnShoppingCartPage { get; set; }
        public bool TermsOfServiceOnOrderConfirmPage { get; set; }
        public EstimateShippingModel EstimateShipping { get; set; }
        public DiscountBoxModel DiscountBox { get; set; }
        public GiftCardBoxModel GiftCardBox { get; set; }
        public OrderReviewDataModel OrderReviewData { get; set; }

        public IList<string> ButtonPaymentMethodActionNames { get; set; }
        public IList<string> ButtonPaymentMethodControllerNames { get; set; }
        public IList<RouteValueDictionary> ButtonPaymentMethodRouteValues { get; set; }

        #region Nested Classes

        public partial class ShoppingCartItemModel : BaseNopEntityModel
        {
            public ShoppingCartItemModel()
            {
                Picture = new PictureModel();
                AllowedQuantities = new List<SelectListItem>();
                Warnings = new List<string>();
                FulfillmentModel = new ShoppingCartItemFulfillmentModel();	/// SODMYWAY-
            }
            public string Sku { get; set; }

            public string ReservedTimeSlot { get; set; }

            public DateTime ReservationDate { get; set; }

            public bool IsReservation { get; set; }

            public bool IsBundleProduct { get; set; }

            public PictureModel Picture { get; set; }

            public int ProductId { get; set; }

            public bool IsMultipleShippingFeatureEnabled { get; set; }

            public bool IsOnlyPickUpProduct { get; set; }

            public bool IsOnlyShippingEnabledProduct { get; set; }
            public bool IsTieredShippingVisible { get; set; }

            public bool IsTieredShippingEnabled { get; set; }
            public decimal FlatShipping { get; set; }

            public bool IsContractShippingEnabled { get; set; }

            public string RCNumber { get; set; }

            public bool IsInterOfficeDeliveryEnabled { get; set; }

            public string MailStopAddress { get; set; }

            public string ProductName { get; set; }

            public string ProductSeName { get; set; }

            public string UnitPrice { get; set; }

            public string SubTotal { get; set; }

            public string Discount { get; set; }

            public int Quantity { get; set; }
            public List<SelectListItem> AllowedQuantities { get; set; }

            public string AttributeInfo { get; set; }

            public string RecurringInfo { get; set; }

            public string RentalInfo { get; set; }

            public bool AllowItemEditing { get; set; }

            public IList<string> Warnings { get; set; }

            #region SODMYWAY-
            public ShoppingCartItemFulfillmentModel FulfillmentModel { get; set; }

            public DateTime RequestedFulfillmentDateTime { get; set; }

            public String RequestedFulfillmentDeliveryDateTimeDisplay
            {
                get
                {


                    if (SelectedWarehouse != null)
                    {
                        if (this.RequestedFulfillmentDateTime != null)
                        {
                            if (this.SelectedWarehouse.AllowDeliveryTime)
                            {
                                return this.RequestedFulfillmentDateTime.ToString();
                            }                          
                            else
                            {
                                return this.RequestedFulfillmentDateTime.ToShortDateString();
                            }

                        }
                    }

                    return "";
                }

            }


            public String RequestedFulfillmentPickupDateTimeDisplay
            {
                get
                {


                    if (SelectedWarehouse != null)
                    {
                        if (this.RequestedFulfillmentDateTime != null)
                        {
                           if (this.SelectedWarehouse.AllowPickupTime)
                            {
                                return this.RequestedFulfillmentDateTime.ToString();
                            }
                            else
                            {
                                return this.RequestedFulfillmentDateTime.ToShortDateString();
                            }

                        }
                    }

                    return "";
                }

            }

            public int? SelectedWarehouseId { get; set; }

            public Warehouse SelectedWarehouse { get; set; }

            public string CheckoutNotes { get; set; }
            #endregion
        }

        #region SODMYWAY-
        /// <summary>
        /// For Handling A shopping Cart Item's Fulfillment Options. SODMYWAY-2941
        /// </summary>
        public partial class ShoppingCartItemFulfillmentModel : BaseNopEntityModel
        {
            public ShoppingCartItemFulfillmentModel()
            {
                CheckoutShippingMethodModel = new CheckoutShippingMethodModel();
                ShippingMethodAllWarehouses = new List<WarehouseModel>();
                FulfillmentWarehouses = new List<Warehouse>();
            }

            public DateTime? RequestedFulfillmentDateTimeLocal { get; set; }

            public int? SelectedFullfilmentWarehouseId { get; set; }

            public string SelectedShippingMethodName { get; set; }

            public string SelectedShippingRateComputationMethodSystemName { get; set; } //SODMYWAY-3467

            public int? PreferredWarehouseLocationId { get; set; }

            public string JsExcludedDatesStr { get; set; }

            public string JsAvailableHoursStr { get; set; }

            public string JsMinDateStr { get; set; }

            public int? JsLeadTimeDays { get; set; }

            public int? JsLeadTimeHours { get; set; }

            public int? JsLeadTimeMinutes { get; set; }

            public string JsMaxDateStr { get; set; }

            public Checkout.CheckoutShippingMethodModel CheckoutShippingMethodModel { get; set; }

            public List<WarehouseModel> ShippingMethodAllWarehouses { get; set; }

            //public List<WarehouseModel> ShippingMethodPickupWarehouses {
            //    get { var list = new List<WarehouseModel>();

            //        foreach(var warehouse in this.ShippingMethodAllWarehouses)
            //        {
            //            if(warehouse.IsPickup)
            //            {
            //                list.Add(warehouse);
            //            }
            //        }
            //        return list;
            //    }
            //}            

            public List<Warehouse> FulfillmentWarehouses { get; set; }

        }
        #endregion

        #region SODMYWAY-
        public partial class WarehouseModel : BaseNopEntityModel
        {
            public WarehouseModel()
            {
                this.Address = new AddressModel();
            }

            public string Name { get; set; }
            public string Email { get; set; }
            public int AddressId { get; set; }
            public string AdminComment { get; set; }

            public AddressModel Address { get; set; }

            public bool IsDelivery { get; set; }
            public bool IsPickup { get; set; }
            public bool? AllowPickupTime { get; set; }
            public bool? AllowDeliveryTime { get; set; }
            public string DeliveryOpenTime { get; set; }
            public string DeliveryCloseTime { get; set; }
            public string PickupOpenTime { get; set; }
            public string PickupCloseTime { get; set; }





            public string DeliveryOpenTimeSun { get; set; }
            public string DeliveryCloseTimeSun { get; set; }
            public string DeliveryOpenTimeMon { get; set; }
            public string DeliveryCloseTimeMon { get; set; }
            public string DeliveryOpenTimeTues { get; set; }
            public string DeliveryCloseTimeTues { get; set; }
            public string DeliveryOpenTimeWeds { get; set; }
            public string DeliveryCloseTimeWeds { get; set; }
            public string DeliveryOpenTimeThurs { get; set; }
            public string DeliveryCloseTimeThurs { get; set; }
            public string DeliveryOpenTimeFri { get; set; }
            public string DeliveryCloseTimeFri { get; set; }
            public string DeliveryOpenTimeSat { get; set; }
            public string DeliveryCloseTimeSat { get; set; }
            public string PickupOpenTimeSun { get; set; }
            public string PickupCloseTimeSun { get; set; }
            public string PickupOpenTimeMon { get; set; }
            public string PickupCloseTimeMon { get; set; }
            public string PickupOpenTimeTues { get; set; }
            public string PickupCloseTimeTues { get; set; }
            public string PickupOpenTimeWeds { get; set; }
            public string PickupCloseTimeWeds { get; set; }
            public string PickupOpenTimeThurs { get; set; }
            public string PickupCloseTimeThurs { get; set; }
            public string PickupOpenTimeFri { get; set; }
            public string PickupCloseTimeFri { get; set; }
            public string PickupOpenTimeSat { get; set; }
            public string PickupCloseTimeSat { get; set; }
            public decimal PickupFee { get; set; }
            public decimal DeliveryFee { get; set; }



            public string GetAvailablePickupTimes()
            {
                if (PickupOpenTime == null)
                {
                    PickupOpenTime = "08:00 AM";
                }
                if (PickupOpenTime == "")
                {
                    PickupOpenTime = "08:00 AM";
                }

                if (PickupCloseTime == null)
                {
                    PickupCloseTime = "05:00 PM";
                }

                if (PickupCloseTime == "")
                {
                    PickupCloseTime = "05:00 PM";
                }

                return FormatTimes(PickupOpenTime, PickupCloseTime);
            }

            public string GetAvailableDeliveryTimes()
            {
                if (DeliveryOpenTime == null)
                {
                    DeliveryOpenTime = "08:00 AM";
                }
                if (DeliveryOpenTime == "")
                {
                    DeliveryOpenTime = "08:00 AM";
                }

                if (DeliveryCloseTime == null)
                {
                    DeliveryCloseTime = "05:00 PM";
                }

                if (DeliveryCloseTime == "")
                {
                    DeliveryCloseTime = "05:00 PM";
                }
                return FormatTimes(DeliveryOpenTime, DeliveryCloseTime);
            }

            private string FormatTimes(string openTime, string closeTime)
            {
                var startTime = DateTime.Parse(openTime);
                startTime = RoundUp(startTime, TimeSpan.FromMinutes(15));
                var endTime = DateTime.Parse(closeTime);
                endTime = RoundUp(endTime, TimeSpan.FromMinutes(15));

                double l = Math.Round(endTime.Subtract(startTime).TotalMinutes / 15.0);
                int maxHours = Convert.ToInt16(Math.Round(l, MidpointRounding.AwayFromZero));

                var clockQuery = from offset in Enumerable.Range(0, maxHours + 1) select TimeSpan.FromMinutes(15 * offset);

                var sb = new StringBuilder();
                foreach (var time in clockQuery)
                {
                    if (sb.Length > 0)
                        sb.Append(",");

                    sb.AppendFormat("'{0}'", (startTime + time).ToString("hh:mm tt"));
                }
                return sb.ToString();
            }

            DateTime RoundUp(DateTime dt, TimeSpan d)
            {
                return new DateTime(((dt.Ticks + d.Ticks - 1) / d.Ticks) * d.Ticks);
            }


        }
        #endregion

        public partial class CheckoutAttributeModel : BaseNopEntityModel
        {
            public CheckoutAttributeModel()
            {
                AllowedFileExtensions = new List<string>();
                Values = new List<CheckoutAttributeValueModel>();
            }

            public string Name { get; set; }

            public string DefaultValue { get; set; }

            public string TextPrompt { get; set; }

            public bool IsRequired { get; set; }

            /// <summary>
            /// Selected day value for datepicker
            /// </summary>
            public int? SelectedDay { get; set; }
            /// <summary>
            /// Selected month value for datepicker
            /// </summary>
            public int? SelectedMonth { get; set; }
            /// <summary>
            /// Selected year value for datepicker
            /// </summary>
            public int? SelectedYear { get; set; }

            /// <summary>
            /// Allowed file extensions for customer uploaded files
            /// </summary>
            public IList<string> AllowedFileExtensions { get; set; }

            public AttributeControlType AttributeControlType { get; set; }

            public IList<CheckoutAttributeValueModel> Values { get; set; }
        }

        public partial class CheckoutAttributeValueModel : BaseNopEntityModel
        {
            public string Name { get; set; }

            public string ColorSquaresRgb { get; set; }

            public string PriceAdjustment { get; set; }

            public bool IsPreSelected { get; set; }
        }

        public partial class DiscountBoxModel : BaseNopModel
        {
            public bool Display { get; set; }
            public string Message { get; set; }
            public string CurrentCode { get; set; }
            public bool IsApplied { get; set; }
        }

        public partial class GiftCardBoxModel : BaseNopModel
        {
            public bool Display { get; set; }
            public string Message { get; set; }
            public bool IsApplied { get; set; }
        }

        public partial class OrderReviewDataModel : BaseNopModel
        {
            public OrderReviewDataModel()
            {
                this.BillingAddress = new AddressModel();
                this.ShippingAddress = new AddressModel();
                this.PickupAddress = new AddressModel();	
                this.CustomValues = new Dictionary<string, object>();
            }
            public bool Display { get; set; }

            public AddressModel BillingAddress { get; set; }

            public bool IsShippable { get; set; }
            public AddressModel ShippingAddress { get; set; }
            public bool SelectedPickUpInStore { get; set; }
            public AddressModel PickupAddress { get; set; }	
            public string ShippingMethod { get; set; }

            public string PaymentMethod { get; set; }

            public Dictionary<string, object> CustomValues { get; set; }
        }
        #endregion
    }
}