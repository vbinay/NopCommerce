using System;
using System.Web;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Services.Authentication;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Stores;
using Nop.Services.Tridion;
using Nop.Web.Framework.Tridion.AppSettings;
using Nop.Web.Framework.Tridion.Profile;

namespace Nop.Web.Framework.Tridion
{
    /// <summary>
    ///  Implementation of the Site Start Procesor Service SODMYWAY-2956
    /// </summary>
    public class SiteStartProcessor: ISiteStartProcessor
    {

        #region Fields

        private readonly ICustomerService _customerService;
        private readonly IAuthenticationService _authenticationService;
        private readonly ITridionProfileMappingService _tridionProfileMappingService;
        private readonly IGenericAttributeService _genericAttributeService;

        #endregion

        #region Constructors / Initialization / Finalization

        public SiteStartProcessor( ICustomerService customerService, IAuthenticationService authenticationService, ITridionProfileMappingService tridionProfileMappingService, IGenericAttributeService genericAttributeService)
        {
            _customerService = customerService;
            _authenticationService = authenticationService;
            //_workContext = workContext;
            _tridionProfileMappingService = tridionProfileMappingService;
            _genericAttributeService = genericAttributeService;
        }

        #endregion
        /// <summary>
        /// The method that creates a new customer fromt he values passed in the Tridion Contact Information Cookie. 
        /// </summary>
        /// <param name="profile">SMW Profile from Cookie</param>
        /// <returns>Nop Commerce Customer</returns>
        public Customer SetRegisteredAuthentication(SmwProfile profile)
        {
            try
            {
                var storeId = SmwUtils.Instance.EcommerceSiteKey;
                var mapping = _tridionProfileMappingService.GetByTridionProfile(storeId, profile.Id);
                Customer customer = null;

                if (storeId == null)
                {
                    return null;
                }

                if (mapping != null)
                {
                    customer = _customerService.GetCustomerById(mapping.CustomerId);

                    _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.FirstName, profile.Firstname);
                    _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.LastName, profile.Lastname);
                    customer.Email = profile.Email;

                    _customerService.UpdateCustomer(customer);
                }
                else
                {

                    customer = new Customer
                    {
                        CustomerGuid = Guid.NewGuid(),
                        Email = null,
                        Username = null,
                        VendorId = 0,
                        AdminComment = null,
                        IsTaxExempt = false,
                        Active = true,
                        CreatedOnUtc = DateTime.UtcNow,
                        LastActivityDateUtc = DateTime.UtcNow,
                    };

                    var registeredRole = _customerService.GetCustomerRoleBySystemName(SystemCustomerRoleNames.Registered);
                    customer.CustomerRoles.Add(registeredRole);
                    _customerService.InsertCustomer(customer);

                    _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.FirstName, profile.Firstname);
                    _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.LastName, profile.Lastname);

                    _tridionProfileMappingService.Create(storeId, profile.Id, customer.Id);
                }

                _authenticationService.SignIn(customer, false);

                return customer;

            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
