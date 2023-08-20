using System;
using Nop.Services.Logging;
using Nop.Services.Tasks;
using Nop.Services.Orders;
using Nop.Services.Shipping;
using Nop.Core;
using Nop.Core.Domain.Shipping;
using Nop.Plugin.Shipping.UPS;
using Nop.Services.Localization;
using Nop.Services.Shipping.Tracking;
using System.Collections.Generic;

namespace Nop.Plugin.Shipping.UPS
{
    /// <summary>
    /// Represents a task for sending queued message 
    /// </summary>
    public partial class UPSShipmentTrackerTask : ITask
    {
        private readonly ILogger _logger;
        private IShipmentService _shipmentService;
        private readonly UPSSettings _upsSettings;
        private readonly ILocalizationService _localizationService;
        private readonly IOrderProcessingService _orderProcessingService;

        public UPSShipmentTrackerTask(IShipmentService shipmentService, UPSSettings upsSettings, ILocalizationService localizationService, IOrderProcessingService orderProcessingService, ILogger logger)
        {
            this._shipmentService = shipmentService;
            this._logger = logger;
            this._upsSettings = upsSettings;
            this._localizationService = localizationService;
            this._orderProcessingService = orderProcessingService;
        }

        /// <summary>
        /// Executes a task
        /// </summary>
        public virtual void Execute()
        {

            IPagedList<Shipment> shipments = _shipmentService.GetAllShipments(); //get all the shipments

            UPSShipmentTracker tracker = new UPSShipmentTracker(this._logger, this._localizationService, this._upsSettings); //Load the tracker

           
            //Run through all shipments and pass in Tracking number (from shipment)
            foreach (Shipment shipment in shipments)
            {
                //---START: Codechages done by (na-sdxcorp\ADas)--------------
                //If Shipment already delivered, then skip it
                if (shipment.DeliveryDateUtc != null)
                    continue;

                if(shipment.Id==62)
                {

                }
                //---END: Codechages done by (na-sdxcorp\ADas)--------------

                IList<ShipmentStatusEvent> events = tracker.GetShipmentEvents(shipment.TrackingNumber);

                foreach (ShipmentStatusEvent shipEvent in events)
                {
                    if (shipEvent.EventName == _localizationService.GetResource("Plugins.Shipping.UPS.Tracker.Departed"))
                    {
                        _orderProcessingService.Ship(shipment, true /*---START: Codechages done by (na-sdxcorp\ADas)--------------*/, true, shipEvent.Date /*---END: Codechages done by (na-sdxcorp\ADas)--------------*/);
                    }
                    if (shipEvent.EventName == _localizationService.GetResource("Plugins.Shipping.UPS.Tracker.Delivered"))
                    {
                        if (shipment.DeliveryDateUtc == null)
                        {
                            _orderProcessingService.Deliver(shipment, true /*---START: Codechages done by (na-sdxcorp\ADas)--------------*/, true, shipEvent.Date /*---END: Codechages done by (na-sdxcorp\ADas)--------------*/);
                        }
                    }

                }

                _shipmentService.UpdateShipment(shipment);


            }

        }
    }
}
