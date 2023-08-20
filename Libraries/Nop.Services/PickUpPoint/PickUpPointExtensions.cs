using Nop.Core.Domain.Cms;
using Nop.Core.Domain.PickUpPoints;
using Nop.Services.PickUpPoint;
using System;

namespace Nop.Services.PickUpPoint
{
    public static class PickUpPointExtensions
    {
        public static bool IsPickUpPointActive(this IPickUpPointPlugin pickUpPoint,
            PickUpPointSettings pickUpPointSettings)
        {
            if (pickUpPoint == null)
                throw new ArgumentNullException("pickUpPoint");

            if (pickUpPointSettings == null)
                throw new ArgumentNullException("pickUpPointSettings");

            if (pickUpPointSettings.ActivePickUpPointSystemNames == null)
                return false;
            foreach (string activeMethodSystemName in pickUpPointSettings.ActivePickUpPointSystemNames)
                if (pickUpPoint.PluginDescriptor.SystemName.Equals(activeMethodSystemName, StringComparison.InvariantCultureIgnoreCase))
                    return true;
            return false;
        }
    }
}