using Nop.Core.Configuration;

namespace Nop.Plugin.Reports.ReportingServices
{
    public class ReportingServicesSettings : ISettings
    {
        public bool SandboxEnvironment { get; set; }
    }
}