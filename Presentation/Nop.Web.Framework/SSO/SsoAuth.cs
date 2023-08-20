using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using Nop.Core.Domain.Customers;
using Nop.Services.Authentication;
using Nop.Services.Customers;

namespace Nop.Web.Framework.SSO
{
    public class SsoAuth: ISsoAuth
    {

        #region Fields

        private readonly IAuthenticationService _authenticationService;
        private readonly ICustomerService _customerService;

        #endregion

        #region Ctor

        public SsoAuth (IAuthenticationService authenticationService, ICustomerService customerService)
        {
            _authenticationService = authenticationService;
            _customerService = customerService;
        }

        #endregion

        public Customer LookupSsoUser()
        {
            string user = null;
            Customer customer = null;
            //attempt to load user from the auth key set by the SSO system
            string ssoHeader = WebConfigurationManager.AppSettings["SSOHeader"];
            foreach (String header in HttpContext.Current.Request.Headers.Keys)
            {
                if (header.Equals(ssoHeader))
                {
                    user = HttpContext.Current.Request.Headers[ssoHeader];
                }
            }

            //if no sso user then try to load a debug user
            if (string.IsNullOrEmpty(user))
            {
                user = WebConfigurationManager.AppSettings["DebugUser"];
            }
            if(!string.IsNullOrEmpty(user))
            {
                customer = _customerService.GetCustomerByUsername(user);
                if (customer != null)
                {
                    _authenticationService.SignIn(customer, false);
                }
            }
            return customer;
        }
    }
}
