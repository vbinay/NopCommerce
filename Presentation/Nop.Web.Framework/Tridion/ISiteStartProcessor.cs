using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core.Domain.Customers;
using Nop.Web.Framework.Tridion.Profile;

namespace Nop.Web.Framework.Tridion
{
    /// <summary>
    ///  Interface for the Site Start Procesor Service SODMYWAY-2956
    /// </summary>
    public interface ISiteStartProcessor
    {
        /// <summary>
        /// The method that creates a new customer fromt he values passed in the Tridion Contact Information Cookie. 
        /// </summary>
        /// <param name="profile">SMW Profile from Cookie</param>
        /// <returns>Nop Commerce Customer</returns>
        Customer SetRegisteredAuthentication(SmwProfile profile);
    }
}
