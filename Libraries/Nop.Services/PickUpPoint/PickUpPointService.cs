using Nop.Core.Domain.Cms;
using Nop.Core.Domain.PickUpPoints;
using Nop.Core.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nop.Services.PickUpPoint
{
    /// <summary>
    /// Widget service
    /// </summary>
    public partial class PickUpPointService : IPickUpPointService
    {
        #region Fields

        private readonly IPluginFinder _pluginFinder;
        private readonly PickUpPointSettings _pickUpPointSettings;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="pluginFinder">Plugin finder</param>
        /// <param name="widgetSettings">Widget settings</param>
        public PickUpPointService(IPluginFinder pluginFinder,
            PickUpPointSettings pickUpPointSettings)
        {
            this._pluginFinder = pluginFinder;
            this._pickUpPointSettings = pickUpPointSettings;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Load active widgets
        /// </summary>
        /// <param name="storeId">Load records allowed only in a specified store; pass 0 to load all records</param>
        /// <returns>Widgets</returns>
        public virtual IList<IPickUpPointPlugin> LoadActivePickUpPoints(int storeId = 0)
        {
            return LoadAllPickUpPoints(storeId)
                   .Where(x => _pickUpPointSettings.ActivePickUpPointSystemNames.Contains(x.PluginDescriptor.SystemName, StringComparer.InvariantCultureIgnoreCase))
                   .OrderBy(m => m.PluginDescriptor.DisplayOrder)
                   .ToList();
        }

        /// <summary>
        /// Load widget by system name
        /// </summary>
        /// <param name="systemName">System name</param>
        /// <returns>Found widget</returns>
        public virtual IPickUpPointPlugin LoadPickUpPointBySystemName(string systemName)
        {
            var descriptor = _pluginFinder.GetPluginDescriptorBySystemName<IPickUpPointPlugin>(systemName);
            if (descriptor != null)
                return descriptor.Instance<IPickUpPointPlugin>();

            return null;
        }

        /// <summary>
        /// Load all widgets
        /// </summary>
        /// <param name="storeId">Load records allowed only in a specified store; pass 0 to load all records</param>
        /// <returns>Widgets</returns>
        public virtual IList<IPickUpPointPlugin> LoadAllPickUpPoints(int storeId = 0)
        {
            return _pluginFinder.GetPlugins<IPickUpPointPlugin>(storeId: storeId).ToList();
        }

        #endregion
    }
}