using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core.Domain.Customers;

namespace Nop.Web.Framework.SSO
{
    public interface ISsoAuth
    {
        Customer LookupSsoUser();
    }
}
