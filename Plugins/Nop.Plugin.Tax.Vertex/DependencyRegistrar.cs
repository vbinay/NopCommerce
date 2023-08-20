using Autofac;
using Autofac.Core;
using Nop.Core.Configuration;
using Nop.Core.Data;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Data;
using Nop.Plugin.Tax.Vertex.Data;
using Nop.Plugin.Tax.Vertex.Domain;
using Nop.Plugin.Tax.Vertex.Services;
using Nop.Web.Framework.Mvc;

namespace Nop.Plugin.Tax.Vertex
{
    /// <summary>
    /// Dependency registrar
    /// </summary>
    public class DependencyRegistrar : IDependencyRegistrar
    {
        /// <summary>
        /// Register services and interfaces
        /// </summary>
        /// <param name="builder">Container builder</param>
        /// <param name="typeFinder">Type finder</param>
        /// <param name="config">Config</param>
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config)
        {
            builder.RegisterType<VertexTaxRateService>().As<IVertexTaxRateService>().InstancePerLifetimeScope();

            //data context
            this.RegisterPluginDataContext<ProductTaxObjectContext>(builder, "nop_object_context_tax_vertex");

            //override required repository with our custom context
            builder.RegisterType<EfRepository<ProductTax>>()
                .As<IRepository<ProductTax>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>("nop_object_context_tax_vertex"))
                .InstancePerLifetimeScope();
        }

        /// <summary>
        /// Order of this dependency registrar implementation
        /// </summary>
        public int Order
        {
            get { return 1; }
        }
    }
}
