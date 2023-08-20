using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Payments.Bambora.Domain
{
    public class StoreLicenseKey : BaseEntity
    {
        public virtual string LicenseKey { get; set; }
    }
}
