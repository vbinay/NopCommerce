using Autofac;
using Autofac.Core;
using Nop.Core.Caching;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Plugin.PickUpPoints.Store.Controllers;

namespace Nop.Plugin.PickUpPoints.Store.Infrastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder,  NopConfig config)
        {
            //we cache presentation models between requests
            builder.RegisterType<PickUpPointsStoreController>()
                .WithParameter(ResolvedParameter.ForNamed<ICacheManager>("nop_cache_static"));
        }

        public int Order
        {
            get { return 2; }
        }
    }
}