using Autofac;
using Autofac.Core;
using Nop.Core.Configuration;
using Nop.Core.Data;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Data;
using BitShift.Plugin.Payments.FirstData.Data;
using BitShift.Plugin.Payments.FirstData.Domain;
using BitShift.Plugin.Payments.FirstData.Services;

namespace BitShift.Plugin.Payments.FirstData
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config)
        {
            builder.RegisterType<StoreLicenseKeyService>().As<IStoreLicenseKeyService>().InstancePerLifetimeScope();
            builder.RegisterType<SavedCardService>().As<ISavedCardService>().InstancePerLifetimeScope();
            builder.RegisterType<FirstDataStoreSettingService>().As<IFirstDataStoreSettingService>().InstancePerLifetimeScope();

            //data layer
            var dataSettingsManager = new DataSettingsManager();
            var dataProviderSettings = dataSettingsManager.LoadSettings();

            if (dataProviderSettings != null && dataProviderSettings.IsValid())
            {
                //register named context
                builder.Register<IDbContext>(c => new FirstDataObjectContext(dataProviderSettings.DataConnectionString))
                    .Named<IDbContext>("nop_object_context_bitshift_firstdata")
                    .InstancePerLifetimeScope();

                builder.Register<FirstDataObjectContext>(c => new FirstDataObjectContext(dataProviderSettings.DataConnectionString))
                    .InstancePerLifetimeScope();
            }
            else
            {
                //register named context
                builder.Register<IDbContext>(c => new FirstDataObjectContext(c.Resolve<DataSettings>().DataConnectionString))
                    .Named<IDbContext>("nop_object_context_bitshift_firstdata")
                    .InstancePerLifetimeScope();

                builder.Register(c => new FirstDataObjectContext(c.Resolve<DataSettings>().DataConnectionString))
                    .InstancePerLifetimeScope();
            }

            //override required repository with our custom context
            builder.RegisterType<EfRepository<StoreLicenseKey>>()
                .As<IRepository<StoreLicenseKey>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>("nop_object_context_bitshift_firstdata"))
                .InstancePerLifetimeScope();
            builder.RegisterType<EfRepository<SavedCard>>()
                .As<IRepository<SavedCard>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>("nop_object_context_bitshift_firstdata"))
                .InstancePerLifetimeScope();
            builder.RegisterType<EfRepository<FirstDataStoreSetting>>()
                .As<IRepository<FirstDataStoreSetting>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>("nop_object_context_bitshift_firstdata"))
                .InstancePerLifetimeScope();
        }

        public int Order
        {
            get { return 1; }
        }
    }
}
