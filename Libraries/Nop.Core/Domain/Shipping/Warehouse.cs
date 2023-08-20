using Nop.Core.Domain.Stores;

namespace Nop.Core.Domain.Shipping
{
    /// <summary>
    /// Represents a fulfilment location	// NU-10
    /// </summary>
    public partial class Warehouse : BaseEntity, IStoreMappingSupported 	// NU-10
    {
        /// <summary>
        /// Gets or sets the warehouse name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the admin comment
        /// </summary>
        public string AdminComment { get; set; }

        /// <summary>
        /// Gets or sets the address identifier of the warehouse
        /// </summary>
        public int AddressId { get; set; }

		#region NU-29
        public bool LimitedToStores
        {
            get { return true; }
            set { value = true; }
        }
		#endregion

        #region NU-10
        public int MasterId { get; set; }
        public bool IsMaster { get; set; }
		#endregion

        #region NU-13
        public bool IsDelivery { get; set; }
        public bool IsPickup { get; set; }
        public bool AllowPickupTime { get; set; }
        public bool AllowDeliveryTime { get; set; }
        
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


        //Helper Methods for displaying proper dates/times
        public string DeliveryDateFormat
        {
            get
            {
                if (AllowDeliveryTime == true)
                {
                    return "MM/dd/yy hh:mm tt";
                }
                else
                {
                    return "d";
                }
            }
        }

        public string PickupDateTimeFormat
        {
            get
            {
                if (AllowPickupTime == true)
                {
                    return "MM/dd/yy hh:mm tt";
                }
                else
                {
                    return "d";
                }
            }

        }
        //end helper
        #endregion


        #region nu-90
        public decimal DeliveryFee { get; set; }
        public decimal PickupFee { get; set; }

        #endregion

        public string KitchenPrinterURL { get; set; }	// NU-84
    }
}