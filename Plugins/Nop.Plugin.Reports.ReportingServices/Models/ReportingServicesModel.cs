using System.Collections.Generic;
using System.Web.Mvc;
using Nop.Web.Framework;

namespace Nop.Plugin.Reports.ReportingServices.Models
{
    public class ReportingServicesModel
    {
        public ReportingServicesModel()
        {
        }

        [NopResourceDisplayName("Plugins.Reports.ReportingServices.SandboxEnvironment")]
        public bool SandboxEnvironment { get; set; }
    }

}