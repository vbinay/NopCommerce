using System.Collections.Generic;

namespace Nop.Services.PickUpPoint
{
    /// <summary>
    /// Widget service interface
    /// </summary>
    public partial interface IPickUpPointService
    {
        /// <summary>
        /// Load active widgets
        /// </summary>
        /// <param name="storeId">Load records allowed only in a specified store; pass 0 to load all records</param>
        /// <returns>Widgets</returns>
        IList<IPickUpPointPlugin> LoadActivePickUpPoints(int storeId = 0);

        /// <summary>
        /// Load widget by system name
        /// </summary>
        /// <param name="systemName">System name</param>
        /// <returns>Found widget</returns>
        IPickUpPointPlugin LoadPickUpPointBySystemName(string systemName);

        /// <summary>
        /// Load all widgets
        /// </summary>
        /// <param name="storeId">Load records allowed only in a specified store; pass 0 to load all records</param>
        /// <returns>Widgets</returns>
        IList<IPickUpPointPlugin> LoadAllPickUpPoints(int storeId = 0);
    }
}