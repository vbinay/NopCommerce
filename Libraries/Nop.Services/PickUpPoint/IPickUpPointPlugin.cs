using Nop.Core.Plugins;
using System.Collections.Generic;
using System.Web.Routing;

namespace Nop.Services.PickUpPoint
{
    /// <summary>
    /// Provides an interface for creating tax providers
    /// </summary>
    public partial interface IPickUpPointPlugin : IPlugin
    {
        /// <summary>
        /// Gets a route for plugin configuration
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues);

        IList<PickUpPointItemModel> GetPickUpPointsList(string postcode);

        decimal GetShippingFee(decimal? shippingWeight);

        string GetPointDescription(string pointName);

        string GetDescription();
    }
}