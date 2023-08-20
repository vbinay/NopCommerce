using System.Web.Mvc;
using System.Web.Routing;
using Nop.Web.Framework.Mvc.Routes;

namespace BitShift.Plugin.Payments.FirstData
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("Plugin.Payments.FirstData.Configure",
                 "Plugins/FirstData/Configure",
                 new { controller = "FirstData", action = "Configure" },
                 new[] { "BitShift.Plugin.Payments.FirstData.Controllers" }
            );

            routes.MapRoute("Plugin.Payments.FirstData.PaymentInfo",
                 "Plugins/FirstData/PaymentInfo",
                 new { controller = "FirstData", action = "PaymentInfo" },
                 new[] { "BitShift.Plugin.Payments.FirstData.Controllers" }
            );

            routes.MapRoute("Plugin.Payments.FirstData.AddKey",
                 "Plugins/FirstData/AddKey",
                 new { controller = "FirstData", action = "AddKey" },
                 new[] { "BitShift.Plugin.Payments.FirstData.Controllers" }
            );

            routes.MapRoute("Plugin.Payments.FirstData.DeleteKey",
                 "Plugins/FirstData/DeleteKey",
                 new { controller = "FirstData", action = "DeleteKey" },
                 new[] { "BitShift.Plugin.Payments.FirstData.Controllers" }
            );

            routes.MapRoute("Plugin.Payments.FirstData.SaveStoreSettings",
                 "Plugins/FirstData/SaveStoreSettings",
                 new { controller = "FirstData", action = "SaveStoreSettings" },
                 new[] { "BitShift.Plugin.Payments.FirstData.Controllers" }
            );

            routes.MapRoute("Plugin.Payments.FirstData.GetStoreSettings",
                 "Plugins/FirstData/GetStoreSettings",
                 new { controller = "FirstData", action = "GetStoreSettings" },
                 new[] { "BitShift.Plugin.Payments.FirstData.Controllers" }
            );

            routes.MapRoute("Plugin.Payments.FirstData.RevertStoreSettings",
                 "Plugins/FirstData/RevertStoreSettings",
                 new { controller = "FirstData", action = "RevertStoreSettings" },
                 new[] { "BitShift.Plugin.Payments.FirstData.Controllers" }
            );

            routes.MapRoute("Plugin.Payments.FirstData.SavedCards",
                "Plugins/FirstData/SavedCards",
                new { controller = "FirstData", action = "SavedCards" },
                new[] { "BitShift.Plugin.Payments.FirstData.Controllers" }
            );

            routes.MapRoute("Plugin.Payments.FirstData.SavedCardsTable",
                "Plugins/FirstData/SavedCardsTable",
                new { controller = "FirstData", action = "SavedCardsTable" },
                new[] { "BitShift.Plugin.Payments.FirstData.Controllers" }
            );

            routes.MapRoute("Plugin.Payments.FirstData.DeleteSavedCard",
                "Plugins/FirstData/DeleteSavedCard",
                new { controller = "FirstData", action = "DeleteSavedCard" },
                new[] { "BitShift.Plugin.Payments.FirstData.Controllers" }
            );

            routes.MapRoute("Plugin.Payments.FirstData.GetHostedPaymentForm",
                "Plugins/FirstData/GetHostedPaymentForm",
                new { controller = "FirstData", action = "GetHostedPaymentForm" },
                new[] { "BitShift.Plugin.Payments.FirstData.Controllers" }
            );

            routes.MapRoute("Plugin.Payments.FirstData.PaymentResponse",
                "Plugins/FirstData/PaymentResponse",
                new { controller = "FirstData", action = "PaymentResponse" },
                new[] { "BitShift.Plugin.Payments.FirstData.Controllers" }
            );
            routes.MapRoute("Plugin.Payments.FirstData.Version",
                "Plugins/FirstData/Version",
                new { controller = "FirstData", action = "Version" },
                new[] { "BitShift.Plugin.Payments.FirstData.Controllers" }
            );
        }

        public int Priority
        {
            get
            {
                return 0;
            }
        }
    }
}
