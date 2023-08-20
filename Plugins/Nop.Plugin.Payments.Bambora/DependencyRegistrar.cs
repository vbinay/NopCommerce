using Autofac;
using Autofac.Core;
using Nop.Core.Configuration;
using Nop.Core.Data;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Data;
using Nop.Plugin.Payments.Bambora.Data;
using Nop.Plugin.Payments.Bambora.Domain;
using Nop.Plugin.Payments.Bambora.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Payments.Bambora
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config)
        {
            builder.RegisterType<StoreLicenseKeyService>().As<IStoreLicenseKeyService>().InstancePerLifetimeScope();
            //builder.RegisterType<SavedCardService>().As<ISavedCardService>().InstancePerLifetimeScope();
            builder.RegisterType<BamboraStoreSettingService>().As<IBamboraStoreSettingService>().InstancePerLifetimeScope();

            //data layer
            var dataSettingsManager = new DataSettingsManager();
            var dataProviderSettings = dataSettingsManager.LoadSettings();

            if (dataProviderSettings != null && dataProviderSettings.IsValid())
            {
                //register named context
                builder.Register<IDbContext>(c => new BamboraObjectContext(dataProviderSettings.DataConnectionString))
                    .Named<IDbContext>("nop_object_context_bambora")
                    .InstancePerLifetimeScope();

                builder.Register<BamboraObjectContext>(c => new BamboraObjectContext(dataProviderSettings.DataConnectionString))
                    .InstancePerLifetimeScope();
            }
            else
            {
                //register named context
                builder.Register<IDbContext>(c => new BamboraObjectContext(c.Resolve<DataSettings>().DataConnectionString))
                    .Named<IDbContext>("nop_object_context_bambora")
                    .InstancePerLifetimeScope();

                builder.Register(c => new BamboraObjectContext(c.Resolve<DataSettings>().DataConnectionString))
                    .InstancePerLifetimeScope();
            }

            //override required repository with our custom context
            builder.RegisterType<EfRepository<StoreLicenseKey>>()
                .As<IRepository<StoreLicenseKey>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>("nop_object_context_bambora"))
                .InstancePerLifetimeScope();
            //builder.RegisterType<EfRepository<SavedCard>>()
            //    .As<IRepository<SavedCard>>()
            //    .WithParameter(ResolvedParameter.ForNamed<IDbContext>("nop_object_context_bitshift_firstdata"))
            //    .InstancePerLifetimeScope();
            builder.RegisterType<EfRepository<BamboraStoreSetting>>()
                .As<IRepository<BamboraStoreSetting>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>("nop_object_context_bambora"))
                .InstancePerLifetimeScope();
        }

        public int Order
        {
            get { return 1; }
        }
    }
}
