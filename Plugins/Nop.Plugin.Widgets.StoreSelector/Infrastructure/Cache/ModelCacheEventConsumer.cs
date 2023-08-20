using Nop.Core.Caching;
using Nop.Core.Domain.Configuration;
using Nop.Core.Events;
using Nop.Core.Infrastructure;
using Nop.Services.Events;

namespace Nop.Plugin.Widgets.StoreSelector.Infrastructure.Cache
{
    /// <summary>
    /// Model cache event consumer (used for caching of presentation layer models)
    /// </summary>
    public partial class ModelCacheEventConsumer: 
        IConsumer<EntityInserted<Setting>>,
        IConsumer<EntityUpdated<Setting>>,
        IConsumer<EntityDeleted<Setting>>
    {
        private readonly ICacheManager _cacheManager;

        public ModelCacheEventConsumer()
        {
            //TODO inject static cache manager using constructor
            this._cacheManager = EngineContext.Current.ContainerManager.Resolve<ICacheManager>("nop_cache_static");
        }

        public void HandleEvent(EntityInserted<Setting> eventMessage)
        {
        }
        public void HandleEvent(EntityUpdated<Setting> eventMessage)
        {
        }
        public void HandleEvent(EntityDeleted<Setting> eventMessage)
        {
        }

    }
}
