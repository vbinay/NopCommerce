using System;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using FluentValidation.Attributes;
using Nop.Admin.Models.Common;
using Nop.Admin.Validators.Shipping;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;
using System.Collections.Generic;

namespace Nop.Admin.Models.Shipping
{
    [Validator(typeof(WarehouseValidator))]
    public partial class WarehouseModel : BaseNopEntityModel
    {
        public WarehouseModel()
        {
            this.Address = new AddressModel();
            AvailableStores = new List<SelectListItem>();
            AvailableVendors = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.Configuration.Shipping.Warehouses.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }
        public string Email { get; set; }	/// SODMYWAY-

        [NopResourceDisplayName("Admin.Configuration.Shipping.Warehouses.Fields.AdminComment")]
        [AllowHtml]
        public string AdminComment { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Shipping.Warehouses.Fields.Address")]
        public AddressModel Address { get; set; }
		#region SODMYWAY-
        [NopResourceDisplayName("Is Delivery")]
        public bool IsDelivery { get; set; }
        [NopResourceDisplayName("Is Pickup")]
        public bool IsPickup { get; set; }
        [NopResourceDisplayName("Allow Pickup Time")]
        public bool AllowPickupTime { get; set; }
        [NopResourceDisplayName("Allow Delivery Time")]
        public bool AllowDeliveryTime { get; set; }
        
        
        [NopResourceDisplayName("Delivery Open Time")]
        public string DeliveryOpenTimeSun { get; set; }
        [NopResourceDisplayName("Delivery Close Time")]
        public string DeliveryCloseTimeSun { get; set; }
        [NopResourceDisplayName("Delivery Open Time")]
        public string DeliveryOpenTimeMon { get; set; }
        [NopResourceDisplayName("Delivery Close Time")]
        public string DeliveryCloseTimeMon { get; set; }
        [NopResourceDisplayName("Delivery Open Time")]
        public string DeliveryOpenTimeTues { get; set; }
        [NopResourceDisplayName("Delivery Close Time")]
        public string DeliveryCloseTimeTues { get; set; }
        [NopResourceDisplayName("Delivery Open Time")]
        public string DeliveryOpenTimeWeds { get; set; }
        [NopResourceDisplayName("Delivery Close Time")]
        public string DeliveryCloseTimeWeds { get; set; }
        [NopResourceDisplayName("Delivery Open Time")]
        public string DeliveryOpenTimeThurs { get; set; }
        [NopResourceDisplayName("Delivery Close Time")]
        public string DeliveryCloseTimeThurs { get; set; }
        [NopResourceDisplayName("Delivery Open Time")]
        public string DeliveryOpenTimeFri { get; set; }
        [NopResourceDisplayName("Delivery Close Time")]
        public string DeliveryCloseTimeFri { get; set; }
        [NopResourceDisplayName("Delivery Open Time")]
        public string DeliveryOpenTimeSat { get; set; }
        [NopResourceDisplayName("Delivery Close Time")]
        public string DeliveryCloseTimeSat { get; set; }




        [NopResourceDisplayName("Pickup Open Time")]
        public string PickupOpenTimeSun { get; set; }
        [NopResourceDisplayName("Pickup Close Time")]
        public string PickupCloseTimeSun { get; set; }
        [NopResourceDisplayName("Pickup Open Time")]
        public string PickupOpenTimeMon { get; set; }
        [NopResourceDisplayName("Pickup Close Time")]
        public string PickupCloseTimeMon { get; set; }
        [NopResourceDisplayName("Pickup Open Time")]
        public string PickupOpenTimeTues { get; set; }
        [NopResourceDisplayName("Pickup Close Time")]
        public string PickupCloseTimeTues { get; set; }
        [NopResourceDisplayName("Pickup Open Time")]
        public string PickupOpenTimeWeds { get; set; }
        [NopResourceDisplayName("Pickup Close Time")]
        public string PickupCloseTimeWeds { get; set; }
        [NopResourceDisplayName("Pickup Open Time")]
        public string PickupOpenTimeThurs { get; set; }
        [NopResourceDisplayName("Pickup Close Time")]
        public string PickupCloseTimeThurs { get; set; }
        [NopResourceDisplayName("Pickup Open Time")]
        public string PickupOpenTimeFri { get; set; }
        [NopResourceDisplayName("Pickup Close Time")]
        public string PickupCloseTimeFri { get; set; }
        [NopResourceDisplayName("Pickup Open Time")]
        public string PickupOpenTimeSat { get; set; }
        [NopResourceDisplayName("Pickup Close Time")]
        public string PickupCloseTimeSat { get; set; }

        [NopResourceDisplayName("Pickup Fee")]
        public decimal PickupFee { get; set; } //NU-90

        [NopResourceDisplayName("Delivery Fee")]
        public decimal DeliveryFee { get; set; } //NU-90

      
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
		#endregion		

        #region SODMYWAY-
        public int StoreId { get; set; }
        public IList<SelectListItem> AvailableStores { get; set; }

        public int VendorId { get; set; }
        public IList<SelectListItem> AvailableVendors { get; set; }

        public bool IsLoggedInAsVendor { get; set; }

        public bool IsAdmin { get; set; }

        public string MappedTo { get; set; }
        public bool MappedToStore { get; set; }
        public bool MappedToVendor { get; set; }

        public string IsLoggedAs { get; set; }	/// SODMYWAY-

        public bool IsReadOnly { get; set; }
        #endregion

        public string KitchenPrinterURL { get; set; }	// NU-84
    }
}