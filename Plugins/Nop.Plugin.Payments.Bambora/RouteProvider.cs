using Nop.Web.Framework.Mvc.Routes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Routing;

namespace Nop.Plugin.Payments.Bambora
{
    public partial class RouteProvider : IRouteProvider
    {
        public int Priority
        {
            get
            {
                return 0;
            }
        }
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("Plugins.Payments.Bambora.Configure",
               "Plugins/BamboraPayment/Configure",
               new { controller = "BamboraPayment", action = "Configure" },
               new[] { "Nop.Plugin.Payments.Bambora.Controllers" }
          );

            routes.MapRoute("Plugins.Payments.Bambora.GetStoreSettings",
                 "Plugins/BamboraPayment/GetStoreSettings",
                 new { controller = "BamboraPayment", action = "GetStoreSettings" },
                 new[] { "Nop.Plugin.Payments.Bambora.Controllers" }
            );

            routes.MapRoute("Plugins.Payments.Bambora.PaymentInfo",
                "Plugins/BamboraPayment/PaymentInfo",
                new { controller = "BamboraPayment", action = "PaymentInfo" },
                new[] { "Nop.Plugin.Payments.Bambora.Controllers" }
           );


            routes.MapRoute("Plugins.Payments.Bambora.GetHostedPaymentForm",
                "Plugins/BamboraPayment/GetHostedPaymentForm",
                new { controller = "BamboraPayment", action = "GetHostedPaymentForm" },
                new[] { "Nop.Plugin.Payments.Bambora.Controllers" }
           );

            routes.MapRoute("Plugins.Payments.Bambora.HandleCallback",
                "Plugins/BamboraPayment/HandleCallback",
                new { controller = "BamboraPayment", action = "HandleCallback" },
                new[] { "Nop.Plugin.Payments.Bambora.Controllers" }
           );

            routes.MapRoute("Plugin.Payments.Bambora.SaveStoreSettings",
                "Plugins/BamboraPayment/SaveStoreSettings",
                new { controller = "BamboraPayment", action = "SaveStoreSettings" },
                new[] { "Nop.Plugin.Payments.Bambora.Controllers" }
           );

            routes.MapRoute("Plugin.Payments.Bambora.GetStoreSettings",
                "Plugins/BamboraPayment/GetStoreSettings",
                new { controller = "BamboraPayment", action = "GetStoreSettings" },
                new[] { "Nop.Plugin.Payments.Bambora.Controllers" }
           );
        }
    }
}
