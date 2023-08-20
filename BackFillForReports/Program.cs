using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackFillForReports
{
    class Program
    {
        public static BackFillReportsScript dbEntity = new BackFillReportsScript();
        static void Main(string[] args)
        {
            var order = dbEntity.Orders.Where(x => x.CreatedOnUtc >= DateTime.Now).ToList();
            foreach (var actualOrder in order)
            {
                var orderitems = dbEntity.OrderItems.Where(x => x.OrderId == actualOrder.Id).ToList();

                foreach (var orderitem in orderitems)
                {
                    var orderDetails = new OrderDetails
                    {
                        OrderId = actualOrder.Id,
                        OrderItemId = orderitem.Id,
                        StoreId = actualOrder.StoreId,
                        IsMealPlan = orderitem.Product.IsMealPlan,
                        IsDonation = orderitem.Product.IsDonation,
                        IsGift = orderitem.Product.IsGiftCard,
                        ProductName = orderitem.Product.Name
                    };
                    ApplyJEAddressBusinessRules(orderitem, actualOrder, orderDetails);

                }
            }
        }

        public static void ApplyJEAddressBusinessRules(OrderItem items, Order order, OrderDetails orderDetails)
        {
            try
            {

                #region StoreDetails and State ProvinceDetails And Ship To Address

                var storeDetails = dbEntity.Stores.Where(x => x.Id == order.StoreId).FirstOrDefault(); // Fetching the store Details only once for item
                var stateProvinceDetails = dbEntity.StateProvinces.Where(x => x.Id == storeDetails.CompanyStateProvinceId).FirstOrDefault(); // Fetcing the state province details for the store
                var shipToAddress = dbEntity.Addresses.Where(x => x.Id == order.ShippingAddressId).FirstOrDefault(); // Recipient Address of the Customer

                // Setting the Store Address here for One time which shall be the Unit Details in the Report
                orderDetails.StoreZip = storeDetails.CompanyZipPostalCode;
                orderDetails.StoreCity = storeDetails.CompanyCity;
                orderDetails.StoreState = stateProvinceDetails.Abbreviation;
                orderDetails.StoreName = storeDetails.Name;
                orderDetails.StoreExtKey = storeDetails.ExtKey;
                orderDetails.StoreLegalEntity = storeDetails.LegalEntity;

                #endregion

                #region Vendor Products Address Configurations
                if (items.Product.VendorId != 0)
                {
                    if (items.IsShipping == true)
                    {
                        var warehouseDetails = dbEntity.Warehouses.Where(x => x.Id == items.Product.WarehouseId).FirstOrDefault(); // For vendor pulling the Warehouse details configured in the Product Config.

                        if (warehouseDetails == null) // Ship From as Store Address
                        {
                            orderDetails.ShipFromZip = storeDetails.CompanyZipPostalCode;
                            orderDetails.ShipFromCity = storeDetails.CompanyCity;
                            orderDetails.ShipFromState = stateProvinceDetails.Abbreviation; // This is the Store state details
                        }
                        else // Ship From as Warehouse Address
                        {
                            var warehouseAddressDetails = dbEntity.Addresses.Where(x => x.Id == warehouseDetails.Id).FirstOrDefault();
                            if (warehouseAddressDetails != null)
                            {
                                orderDetails.ShipFromZip = warehouseAddressDetails.ZipPostalCode;
                                orderDetails.ShipFromCity = warehouseAddressDetails.City;
                                orderDetails.ShipFromState = warehouseAddressDetails.StateProvince.Abbreviation;
                            }
                        }
                        //Set the Ship to Address Details
                        orderDetails.ShipToZip = shipToAddress.ZipPostalCode;
                        orderDetails.ShipToCity = shipToAddress.City;
                        orderDetails.ShipToState = shipToAddress.StateProvince.Abbreviation;
                        orderDetails.VertexTaxAreaId = shipToAddress.StateProvince.Abbreviation + shipToAddress.ZipPostalCode;
                    }
                }
                #endregion

                #region MealPlans Donation GiftCard
                if (items.Product.IsMealPlan || items.Product.IsDonation || items.Product.IsGiftCard)
                {
                    orderDetails.ShipFromZip = orderDetails.StoreZip;
                    orderDetails.ShipFromCity = orderDetails.StoreCity;
                    orderDetails.ShipFromState = orderDetails.StoreState;

                    orderDetails.ShipToZip = orderDetails.StoreZip;
                    orderDetails.ShipToCity = orderDetails.StoreCity;
                    orderDetails.ShipToState = orderDetails.StoreState;

                    orderDetails.VertexTaxAreaId = orderDetails.ShipToZip + orderDetails.ShipToState;
                }
                #endregion

                #region Non Meal Plan Products
                if (!items.Product.IsMealPlan && !items.Product.IsDonation && !items.Product.IsGiftCard && items.Product.VendorId == 0)
                {
                    if (items.IsDeliveryPickUp == true) // When the customer has selected Delivery option either DElIVER/IN-STORE PICKUP
                    {
                        if (items.SelectedShippingMethodName == "Delivery") // if the Product has Set Delivery as fulfillment option and has the Warehouse configured to it
                        {
                            var warehouseDetails = dbEntity.Warehouses.Where(x => x.Id == items.Product.WarehouseId).FirstOrDefault();
                            if (warehouseDetails != null)
                            {
                                var warehouseAddressDetails = dbEntity.Addresses.Where(x => x.Id == warehouseDetails.Id).FirstOrDefault();
                                orderDetails.ShipFromZip = warehouseAddressDetails.ZipPostalCode;
                                orderDetails.ShipFromCity = warehouseAddressDetails.City;
                                orderDetails.ShipFromState = warehouseAddressDetails.StateProvince.Abbreviation;
                            }
                            else // Pick up the ship from address as the store address
                            {
                                orderDetails.ShipFromZip = orderDetails.StoreZip;
                                orderDetails.ShipFromCity = orderDetails.StoreCity;
                                orderDetails.ShipFromState = orderDetails.StoreState;
                            }
                            //Set the Ship to Address Details
                            orderDetails.ShipToZip = shipToAddress.ZipPostalCode;
                            orderDetails.ShipToCity = shipToAddress.City;
                            orderDetails.ShipToState = shipToAddress.StateProvince.Abbreviation;
                            orderDetails.VertexTaxAreaId = orderDetails.ShipToState + orderDetails.ShipToZip;
                        }
                        else// If the Product has the set In-Store Pickup as fulfillment option
                        {
                            var warehouseDetails = dbEntity.Warehouses.Where(x => x.Id == Convert.ToInt32(items.SelectedFulfillmentWarehouseId)).FirstOrDefault();
                            var warehouseAddressDetails = dbEntity.Addresses.Where(x => x.Id == warehouseDetails.Id).FirstOrDefault();
                            if (warehouseAddressDetails != null)
                            {
                                orderDetails.ShipFromZip = warehouseAddressDetails.ZipPostalCode;
                                orderDetails.ShipFromCity = warehouseAddressDetails.City;
                                orderDetails.ShipFromState = warehouseAddressDetails.StateProvince.Abbreviation;

                                orderDetails.ShipToZip = warehouseAddressDetails.ZipPostalCode;
                                orderDetails.ShipToCity = warehouseAddressDetails.City;
                                orderDetails.ShipToState = warehouseAddressDetails.StateProvince.Abbreviation;

                                orderDetails.VertexTaxAreaId = orderDetails.ShipToState + orderDetails.ShipToZip;
                            }
                        }

                    }
                    if (items.IsShipping==true)
                    {
                        var warehouseDetails = dbEntity.Warehouses.Where(x=>x.Id==items.Product.WarehouseId).FirstOrDefault();
                        if (warehouseDetails != null) // Pickup from the warehouse if warehouse is configured else pick from the store address
                        {
                            var warehouseAddressDetails = dbEntity.Addresses.Where(x => x.Id == warehouseDetails.Id).FirstOrDefault();
                            orderDetails.ShipFromZip = warehouseAddressDetails.ZipPostalCode;
                            orderDetails.ShipFromCity = warehouseAddressDetails.City;
                            orderDetails.ShipFromState = warehouseAddressDetails.StateProvince.Abbreviation;
                        }
                        else // Pick up the ship from address as the store address
                        {
                            orderDetails.ShipFromZip = orderDetails.StoreZip;
                            orderDetails.ShipFromCity = orderDetails.StoreCity;
                            orderDetails.ShipFromState = orderDetails.StoreState;
                        }
                        //Set the Ship to Address Details
                        orderDetails.ShipToZip = shipToAddress.ZipPostalCode;
                        orderDetails.ShipToCity = shipToAddress.City;
                        orderDetails.ShipToState = shipToAddress.StateProvince.Abbreviation;

                        orderDetails.VertexTaxAreaId = orderDetails.ShipToState + orderDetails.ShipToZip;
                    }

                }
                #endregion

                #region Products With No FullfillmentOption and Shipping Option. Event Tickets
                if (items.Product.IsMealPlan==false && items.Product.IsDonation==false && items.Product.IsGiftCard==false //Always Sets this to store address for Event Tickets
                    && items.Product.IsLocalDelivery==false && !items.Product.IsShipEnabled && !items.Product.IsPickupEnabled)
                {
                    orderDetails.ShipFromZip = orderDetails.StoreZip;
                    orderDetails.ShipFromCity = orderDetails.StoreCity;
                    orderDetails.ShipFromState = orderDetails.StoreState;

                    orderDetails.ShipToZip = orderDetails.StoreZip;
                    orderDetails.ShipToCity = orderDetails.StoreCity;
                    orderDetails.ShipToState = orderDetails.StoreState;

                    orderDetails.VertexTaxAreaId = orderDetails.ShipToState + orderDetails.ShipToZip;
                }
                #endregion
            }
            catch (Exception ex)
            {
                throw ex.InnerException;
            }
        }

    }
}
