using System;
using System.Linq;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Stores;
using System.DirectoryServices;
using System.DirectoryServices.Protocols;
using System.Net;
using Nop.Services.Authentication.External;
using Nop.Services.Common;
using System.Web.Configuration;
using Nop.Services.Logging;
using Newtonsoft.Json;

namespace Nop.Services.Customers
{
    /// <summary>
    /// Customer registration service
    /// </summary>
    public partial class CustomerRegistrationService : ICustomerRegistrationService
    {
        #region Fields

        private readonly ICustomerService _customerService;
        private readonly IEncryptionService _encryptionService;
        private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
        private readonly ILocalizationService _localizationService;
        private readonly IStoreService _storeService;
        private readonly IRewardPointService _rewardPointService;
        private readonly RewardPointsSettings _rewardPointsSettings;
        private readonly CustomerSettings _customerSettings;
        private readonly IGenericAttributeService _genericAttributeService;  //NU-90
        private readonly IStoreMappingService _storeMappingService; //NU-90
        private readonly ILogger _logger;



        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="customerService">Customer service</param>
        /// <param name="encryptionService">Encryption service</param>
        /// <param name="newsLetterSubscriptionService">Newsletter subscription service</param>
        /// <param name="localizationService">Localization service</param>
        /// <param name="storeService">Store service</param>
        /// <param name="rewardPointService">Reward points service</param>
        /// <param name="rewardPointsSettings">Reward points settings</param>
        /// <param name="customerSettings">Customer settings</param>
        public CustomerRegistrationService(ICustomerService customerService,
            IEncryptionService encryptionService,
            INewsLetterSubscriptionService newsLetterSubscriptionService,
            ILocalizationService localizationService,
            IStoreService storeService,
            IRewardPointService rewardPointService,
            IGenericAttributeService genericAttributeService,//NU-90
            IStoreMappingService storeMappingService, //NU-90
            RewardPointsSettings rewardPointsSettings,
            CustomerSettings customerSettings,
            ILogger logger)
        {
            this._customerService = customerService;
            this._encryptionService = encryptionService;
            this._newsLetterSubscriptionService = newsLetterSubscriptionService;
            this._localizationService = localizationService;
            this._storeService = storeService;
            this._genericAttributeService = genericAttributeService;//NU-90
            this._storeMappingService = storeMappingService;//NU-90
            this._rewardPointService = rewardPointService;
            this._rewardPointsSettings = rewardPointsSettings;
            this._customerSettings = customerSettings;
            this._logger = logger;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Validate customer
        /// </summary>
        /// <param name="usernameOrEmail">Username or email</param>
        /// <param name="password">Password</param>
        /// <returns>Result</returns>
        public virtual CustomerLoginResults ValidateCustomer(string usernameOrEmail, string password)
        {
            var customer = _customerSettings.UsernamesEnabled ?
                _customerService.GetCustomerByUsername(usernameOrEmail) :
                _customerService.GetCustomerByEmail(usernameOrEmail);

            if (customer == null)
                return CustomerLoginResults.CustomerNotExist;
            if (customer.Deleted)
                return CustomerLoginResults.Deleted;
            if (!customer.Active)
                return CustomerLoginResults.NotActive;
            //only registered can login
            if (!customer.IsRegistered())
                return CustomerLoginResults.NotRegistered;

            string pwd;
            switch (customer.PasswordFormat)
            {
                case PasswordFormat.Encrypted:
                    pwd = _encryptionService.EncryptText(password);
                    break;
                case PasswordFormat.Hashed:
                    pwd = _encryptionService.CreatePasswordHash(password, customer.PasswordSalt, _customerSettings.HashedPasswordFormat);
                    break;
                default:
                    pwd = password;
                    break;
            }

            bool isValid = pwd == customer.Password;
            if (!isValid)
                return CustomerLoginResults.WrongPassword;

            //save last login date
            customer.LastLoginDateUtc = DateTime.UtcNow;
            _customerService.UpdateCustomer(customer);
            return CustomerLoginResults.Successful;
        }


        /// <summary>
        /// Validate customer
        /// </summary>
        /// <param name="usernameOrEmail">Username or email</param>
        /// <param name="password">Password</param>
        /// NU-90
        /// <returns>Result</returns>
        public virtual CustomerLoginResults ValidateCustomerSSO(Customer currentCustomer, string usernameOrEmail, string password, int storeId)
        {
            var customer = _customerSettings.UsernamesEnabled ? _customerService.GetCustomerByUsername(usernameOrEmail) : _customerService.GetCustomerByEmail(usernameOrEmail);

            //if (customer == null)
            //return CustomerLoginResults.CustomerNotExist;
            //if (customer.Deleted)
            //    //return CustomerLoginResults.Deleted;
            //if (!customer.Active)
            //    //return CustomerLoginResults.NotActive;
            ////only registered can login
            //if (!customer.IsRegistered())
            //return CustomerLoginResults.NotRegistered;

            if (customer == null && !_storeMappingService.Authorize<Customer>(customer)) //validate against the SSO and register the user if ok
            {

                string ldapserver = WebConfigurationManager.AppSettings["FSULDAPServer"];
                string ldapserverpass = WebConfigurationManager.AppSettings["FSULDAPPassword"];

                LdapDirectoryIdentifier id = new LdapDirectoryIdentifier(ldapserver, 636);
                LdapConnection _connection = new LdapConnection(id);
                //_connection.SessionOptions.SecureSocketLayer = secureConnection;


                _logger.Information("User:  Trying to log in as user: " + usernameOrEmail);

                _connection.AuthType = AuthType.Basic;
                _connection.Credential = new NetworkCredential("uid=" + usernameOrEmail + ",ou=people,dc=fsu,dc=edu", password);
                try
                {
                    _logger.Information("User: Trying User bind: " + usernameOrEmail);
                    _connection.Bind();
                }
                catch (Exception e)
                {
                    _logger.Information("User:  User couldn't Bind: " + usernameOrEmail + " " + e.Message + " " + e.StackTrace);
                    return CustomerLoginResults.CustomerNotExist;
                }
                finally
                {

                    _logger.Information("User:  Killing user bind connection");
                    _connection.Dispose();
                }




                _logger.Information("Proxy:  Trying to bind as proxy user");

                LdapDirectoryIdentifier id2 = new LdapDirectoryIdentifier(ldapserver, 636);
                LdapConnection _connection2 = new LdapConnection(id2);
                //_connection.SessionOptions.SecureSocketLayer = secureConnection;
                _connection2.AuthType = AuthType.Basic;
                _connection2.Credential = new NetworkCredential("cn=obs-sodexo-proxy,ou=proxy-users,dc=fsu,dc=edu", ldapserverpass);

                try
                {
                    _logger.Information("Proxy: User bind");
                    _connection2.Bind();




                    String _target = "ou=people,dc=fsu,dc=edu";

                    String filter = "(uid=" + usernameOrEmail + ")";
                    String[] attributesToReturn = { "fsuEduLastName", "fsuEduPreferredEmail", "uid", "fsuEduEMPLID", "fsuEduFirstName", "fsuEduMiddleName" };

                    SearchRequest searchRequest = new SearchRequest(_target, filter, System.DirectoryServices.Protocols.SearchScope.Subtree, attributesToReturn);
                    SearchResponse response = (SearchResponse)_connection2.SendRequest(searchRequest);

                    _logger.Information("Proxy:  trying to find uid " + usernameOrEmail + " through proxy");

                    if (response.Entries.Count > 0) //we find it?!
                    {
                        //We did!

                        _logger.Information("Proxy:  Found uid " + usernameOrEmail + " through proxy getting responses");


                        SearchResultEntry entry = response.Entries[0];

                        string ssoEmail = entry.Attributes["fsuEduPreferredEmail"][0].ToString();
                        string ssoFName = entry.Attributes["fsuEduFirstName"][0].ToString();
                        string ssoLastName = entry.Attributes["fsuEduLastName"][0].ToString();

                        //Create User if one isn't already created

                        _logger.Information("Proxy: Building registration for: " + usernameOrEmail);


                        bool isApproved = _customerSettings.UserRegistrationType == UserRegistrationType.Standard;
                        var registrationRequest = new CustomerRegistrationRequest(currentCustomer,
                            ssoEmail,
                            usernameOrEmail,
                            password, //NO Idea if this should work like this.
                            _customerSettings.DefaultPasswordFormat,
                            storeId, isApproved);


                       // _logger.Information("CustomerRegistrationService: Setting up registrationRequest ssoEmail: " + ssoEmail + " usernameOrEmail: " + usernameOrEmail + " storeId: " + storeId );


                        CustomerRegistrationResult result = this.RegisterCustomer(registrationRequest);                        

                        _logger.Information("CustomerRegistrationService: Result.Success: " + result.Success);

                        if (result.Success)
                        {
                           

                            if (!String.IsNullOrEmpty(ssoFName))
                                _genericAttributeService.SaveAttribute(currentCustomer, SystemCustomerAttributeNames.FirstName, ssoFName);
                            if (!String.IsNullOrEmpty(ssoLastName))
                                _genericAttributeService.SaveAttribute(currentCustomer, SystemCustomerAttributeNames.LastName, ssoLastName);

                            _genericAttributeService.SaveAttribute<string>(currentCustomer, "IsSSOUser", "true", storeId);


                            foreach (var attr in entry.Attributes.AttributeNames)
                            {
                                string row = attr.ToString();

                                string attrstr = entry.Attributes[attr.ToString()][0].ToString();

                                _genericAttributeService.SaveAttribute<string>(registrationRequest.Customer, row, attrstr, storeId);

                            }

                            customer = registrationRequest.Customer;
                        }
                    }
                    else //can't find FSU email through SSO
                    {

                        _logger.Information("CustomerRegistrationService: Customer Not Exists");
                        return CustomerLoginResults.CustomerNotExist;
                    }
                }
                catch (Exception e)
                {
                    _logger.Information("Proxy: User Bind Issue: " + e.Message + " " + e.StackTrace);
                    return CustomerLoginResults.CustomerNotExist;
                }
                finally
                {

                    _logger.Information("Proxy:  Killing user bind connection");
                    _connection2.Dispose();
                }

            }


            //save last login date
            customer.LastLoginDateUtc = DateTime.UtcNow;
            _customerService.UpdateCustomer(customer);
            _logger.Information("CustomerRegistrationService: Successful!!");
            return CustomerLoginResults.Successful;
        }




        /// <summary>
        /// Register customer
        /// </summary>
        /// <param name="request">Request</param>
        /// <returns>Result</returns>
        public virtual CustomerRegistrationResult RegisterCustomer(CustomerRegistrationRequest request)
        {
            if (request == null)
                throw new ArgumentNullException("request");

            if (request.Customer == null)
            {
                _logger.Information("RegisterCustomer: can't load current customer");
                throw new ArgumentException("Can't load current customer");
            }

            var result = new CustomerRegistrationResult();
            if (request.Customer.IsSearchEngineAccount())
            {
                _logger.Information("RegisterCustomer: Search engine can't be registered");
                result.AddError("Search engine can't be registered");
                return result;
            }
            if (request.Customer.IsBackgroundTaskAccount())
            {
                _logger.Information("RegisterCustomer: Is background task");
                result.AddError("Background task account can't be registered");
                return result;
            }
            if (request.Customer.IsRegistered())
            {
                _logger.Information("RegisterCustomer:  is registered");
                result.AddError("Current customer is already registered");
                return result;
            }
            if (String.IsNullOrEmpty(request.Email))
            {
                _logger.Information("RegisterCustomer:  No Email Address");
                result.AddError(_localizationService.GetResource("Account.Register.Errors.EmailIsNotProvided"));
                return result;
            }
            if (!CommonHelper.IsValidEmail(request.Email))
            {
                _logger.Information("RegisterCustomer:  Not valid Email Address");
                result.AddError(_localizationService.GetResource("Common.WrongEmail"));
                return result;
            }
            if (String.IsNullOrWhiteSpace(request.Password))
            {
                _logger.Information("RegisterCustomer:  No password");
                result.AddError(_localizationService.GetResource("Account.Register.Errors.PasswordIsNotProvided"));
                return result;
            }
            if (_customerSettings.UsernamesEnabled)
            {

                _logger.Information("RegisterCustomer:  usernames enabled");
                if (String.IsNullOrEmpty(request.Username))
                {
                    _logger.Information("RegisterCustomer:  no username");
                    result.AddError(_localizationService.GetResource("Account.Register.Errors.UsernameIsNotProvided"));
                    return result;
                }
            }

            //validate unique user

            _logger.Information("RegisterCustomer:  Grab customer by email");
            if (_customerService.GetCustomerByEmail(request.Email) != null)
            {
                _logger.Information("RegisterCustomer:  customer already exists by email");
                result.AddError(_localizationService.GetResource("Account.Register.Errors.EmailAlreadyExists"));
                return result;
            }

            _logger.Information("RegisterCustomer:  Grab customer by username");
            if (_customerSettings.UsernamesEnabled)
            {
                _logger.Information("RegisterCustomer:  username enabled");
                if (_customerService.GetCustomerByUsername(request.Username) != null)
                {
                    _logger.Information("RegisterCustomer:  username already exists");
                    result.AddError(_localizationService.GetResource("Account.Register.Errors.UsernameAlreadyExists"));
                    return result;
                }
            }


            _logger.Information("RegisterCustomer:  request valid at this point.");
            //at this point request is valid
            request.Customer.Username = request.Username;
            request.Customer.Email = request.Email;
            request.Customer.PasswordFormat = request.PasswordFormat;


            _logger.Information("RegisterCustomer:  Request UserName: " + request.Username);
            _logger.Information("RegisterCustomer:  Request email: " + request.Email);

            switch (request.PasswordFormat)
            {
                case PasswordFormat.Clear:
                    {
                        request.Customer.Password = request.Password;
                    }
                    break;
                case PasswordFormat.Encrypted:
                    {
                        request.Customer.Password = _encryptionService.EncryptText(request.Password);
                    }
                    break;
                case PasswordFormat.Hashed:
                    {
                        string saltKey = _encryptionService.CreateSaltKey(5);
                        request.Customer.PasswordSalt = saltKey;
                        request.Customer.Password = _encryptionService.CreatePasswordHash(request.Password, saltKey, _customerSettings.HashedPasswordFormat);
                    }
                    break;
                default:
                    break;
            }

            request.Customer.Active = request.IsApproved;

            _logger.Information("RegisterCustomer:  Request isapproved: " + request.IsApproved);

            //add to 'Registered' role
            var registeredRole = _customerService.GetCustomerRoleBySystemName(SystemCustomerRoleNames.Registered);
            if (registeredRole == null)
                throw new NopException("'Registered' role could not be loaded");


            _logger.Information("RegisterCustomer:  Change to registered role");
            request.Customer.CustomerRoles.Add(registeredRole);
            //remove from 'Guests' role
            var guestRole = request.Customer.CustomerRoles.FirstOrDefault(cr => cr.SystemName == SystemCustomerRoleNames.Guests);
            if (guestRole != null)
                request.Customer.CustomerRoles.Remove(guestRole);

            //Add reward points for customer registration (if enabled)
            if (_rewardPointsSettings.Enabled &&
                _rewardPointsSettings.PointsForRegistration > 0)
            {
                _rewardPointService.AddRewardPointsHistoryEntry(request.Customer,
                    _rewardPointsSettings.PointsForRegistration,
                    request.StoreId,
                    _localizationService.GetResource("RewardPoints.Message.EarnedForRegistration"));
            }


            //NU-90

            _logger.Information("Insert Store mapping, Customer Id" + request.Customer.Id + " store id: " + request.StoreId);
            _storeMappingService.InsertStoreMapping(request.Customer, request.StoreId);  //TODO:  I think this is required on all customer registrations now

            _logger.Information("updating customer in table");
            _customerService.UpdateCustomer(request.Customer);

            _logger.Information("Returning result " + JsonConvert.SerializeObject(request.Customer, Formatting.Indented,
                                new JsonSerializerSettings
                                {
                                    PreserveReferencesHandling = PreserveReferencesHandling.Objects
                                }
                                ));
            return result;
        }

        /// <summary>
        /// Change password
        /// </summary>
        /// <param name="request">Request</param>
        /// <returns>Result</returns>
        public virtual ChangePasswordResult ChangePassword(ChangePasswordRequest request)
        {
            if (request == null)
                throw new ArgumentNullException("request");

            var result = new ChangePasswordResult();
            if (String.IsNullOrWhiteSpace(request.Email))
            {
                result.AddError(_localizationService.GetResource("Account.ChangePassword.Errors.EmailIsNotProvided"));
                return result;
            }
            if (String.IsNullOrWhiteSpace(request.NewPassword))
            {
                result.AddError(_localizationService.GetResource("Account.ChangePassword.Errors.PasswordIsNotProvided"));
                return result;
            }

            var customer = _customerService.GetCustomerByEmail(request.Email);
            if (customer == null)
            {
                result.AddError(_localizationService.GetResource("Account.ChangePassword.Errors.EmailNotFound"));
                return result;
            }


            var requestIsValid = false;
            if (request.ValidateRequest)
            {
                //password
                string oldPwd;
                switch (customer.PasswordFormat)
                {
                    case PasswordFormat.Encrypted:
                        oldPwd = _encryptionService.EncryptText(request.OldPassword);
                        break;
                    case PasswordFormat.Hashed:
                        oldPwd = _encryptionService.CreatePasswordHash(request.OldPassword, customer.PasswordSalt, _customerSettings.HashedPasswordFormat);
                        break;
                    default:
                        oldPwd = request.OldPassword;
                        break;
                }

                bool oldPasswordIsValid = oldPwd == customer.Password;
                if (!oldPasswordIsValid)
                    result.AddError(_localizationService.GetResource("Account.ChangePassword.Errors.OldPasswordDoesntMatch"));

                if (oldPasswordIsValid)
                    requestIsValid = true;
            }
            else
                requestIsValid = true;


            //at this point request is valid
            if (requestIsValid)
            {
                switch (request.NewPasswordFormat)
                {
                    case PasswordFormat.Clear:
                        {
                            customer.Password = request.NewPassword;
                        }
                        break;
                    case PasswordFormat.Encrypted:
                        {
                            customer.Password = _encryptionService.EncryptText(request.NewPassword);
                        }
                        break;
                    case PasswordFormat.Hashed:
                        {
                            string saltKey = _encryptionService.CreateSaltKey(5);
                            customer.PasswordSalt = saltKey;
                            customer.Password = _encryptionService.CreatePasswordHash(request.NewPassword, saltKey, _customerSettings.HashedPasswordFormat);
                        }
                        break;
                    default:
                        break;
                }
                customer.PasswordFormat = request.NewPasswordFormat;
                _customerService.UpdateCustomer(customer);
            }

            return result;
        }

        /// <summary>
        /// Sets a user email
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="newEmail">New email</param>
        public virtual void SetEmail(Customer customer, string newEmail)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            if (newEmail == null)
                throw new NopException("Email cannot be null");

            newEmail = newEmail.Trim();
            string oldEmail = customer.Email;

            if (!CommonHelper.IsValidEmail(newEmail))
                throw new NopException(_localizationService.GetResource("Account.EmailUsernameErrors.NewEmailIsNotValid"));

            if (newEmail.Length > 100)
                throw new NopException(_localizationService.GetResource("Account.EmailUsernameErrors.EmailTooLong"));

            var customer2 = _customerService.GetCustomerByEmail(newEmail);
            if (customer2 != null && customer.Id != customer2.Id)
                throw new NopException(_localizationService.GetResource("Account.EmailUsernameErrors.EmailAlreadyExists"));

            customer.Email = newEmail;
            _customerService.UpdateCustomer(customer);

            //update newsletter subscription (if required)
            if (!String.IsNullOrEmpty(oldEmail) && !oldEmail.Equals(newEmail, StringComparison.InvariantCultureIgnoreCase))
            {
                foreach (var store in _storeService.GetAllStores())
                {
                    var subscriptionOld = _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmailAndStoreId(oldEmail, store.Id);
                    if (subscriptionOld != null)
                    {
                        subscriptionOld.Email = newEmail;
                        _newsLetterSubscriptionService.UpdateNewsLetterSubscription(subscriptionOld);
                    }
                }
            }
        }

        /// <summary>
        /// Sets a customer username
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="newUsername">New Username</param>
        public virtual void SetUsername(Customer customer, string newUsername)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            if (!_customerSettings.UsernamesEnabled)
                throw new NopException("Usernames are disabled");

            if (!_customerSettings.AllowUsersToChangeUsernames)
                throw new NopException("Changing usernames is not allowed");

            newUsername = newUsername.Trim();

            if (newUsername.Length > 100)
                throw new NopException(_localizationService.GetResource("Account.EmailUsernameErrors.UsernameTooLong"));

            var user2 = _customerService.GetCustomerByUsername(newUsername);
            if (user2 != null && customer.Id != user2.Id)
                throw new NopException(_localizationService.GetResource("Account.EmailUsernameErrors.UsernameAlreadyExists"));

            customer.Username = newUsername;
            _customerService.UpdateCustomer(customer);
        }

        #endregion
    }
}