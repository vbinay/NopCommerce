using Nop.Core;

namespace BitShift.Plugin.Payments.FirstData.Domain
{
    public class StoreLicenseKey : BaseEntity
    {
        public virtual string LicenseKey { get; set; }
    }
}
