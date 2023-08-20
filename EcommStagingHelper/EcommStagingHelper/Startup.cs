using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(EcommStagingHelper.Startup))]
namespace EcommStagingHelper
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
