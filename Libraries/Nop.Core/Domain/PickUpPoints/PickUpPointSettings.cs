using Nop.Core.Configuration;
using System.Collections.Generic;

namespace Nop.Core.Domain.PickUpPoints
{
    public class PickUpPointSettings : ISettings
    {
        public PickUpPointSettings()
        {
            ActivePickUpPointSystemNames = new List<string>();
        }

        /// <summary>
        /// Gets or sets a system names of active widgets
        /// </summary>
        public List<string> ActivePickUpPointSystemNames { get; set; }
    }
}