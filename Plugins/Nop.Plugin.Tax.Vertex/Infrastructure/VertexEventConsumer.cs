using Nop.Core.Domain.Orders;
using Nop.Core.Events;
using Nop.Services.Events;
using Nop.Services.Tax;

namespace Nop.Plugin.Tax.Vertex.Infrastructure
{
    /// <summary>
    /// Represents Vertex event consumer (used for commit and void tax requests to Vertex service)
    /// </summary>
    public partial class VertexEventConsumer : 
        IConsumer<OrderPlacedEvent>,
        IConsumer<OrderCancelledEvent>,
        IConsumer<EntityDeleted<Order>>
    {
        #region Fields

        private readonly ITaxService _taxService;

        #endregion

        #region Ctor

        public VertexEventConsumer(ITaxService taxService)
        {
            this._taxService = taxService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Handle order placed event
        /// </summary>
        /// <param name="eventMessage">Event message</param>
        public void HandleEvent(OrderPlacedEvent eventMessage)
        {
            //ensure that Vertex tax rate provider is active
            var taxProvider = _taxService.LoadActiveTaxProvider(eventMessage.Order.StoreId) as VertexTaxProvider;
            if (taxProvider == null)
                return;

            //commit tax request
            //taxProvider.CommitTaxRequest(eventMessage.Order);
        }

        /// <summary>
        /// Handle order cancelled event
        /// </summary>
        /// <param name="eventMessage">Event message</param>
        public void HandleEvent(OrderCancelledEvent eventMessage)
        {
            //ensure that Vertex tax rate provider is active
            var taxProvider = _taxService.LoadActiveTaxProvider(eventMessage.Order.StoreId) as VertexTaxProvider;
            if (taxProvider == null)
                return;

            //void tax request
            taxProvider.VoidTaxRequest(eventMessage.Order, false);
        }

        /// <summary>
        /// Handle order deleted event
        /// </summary>
        /// <param name="eventMessage">Event message</param>
        public void HandleEvent(EntityDeleted<Order> eventMessage)
        {
            //ensure that Vertex tax rate provider is active
            var taxProvider = _taxService.LoadActiveTaxProvider(eventMessage.Entity.StoreId) as VertexTaxProvider;
            if (taxProvider == null)
                return;

            //void tax request (mark as deleted)
            taxProvider.VoidTaxRequest(eventMessage.Entity, true);
        }

        #endregion
    }
}
