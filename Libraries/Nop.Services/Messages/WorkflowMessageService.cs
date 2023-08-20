using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core;
using Nop.Core.Domain.Blogs;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.News;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Stores;
using Nop.Core.Domain.Vendors;
using Nop.Services.Customers;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Stores;
using System.Xml;
using System.Net;
using System.Xml.Linq;
using System.Security;
using Nop.Services.Common;
using Nop.Core.Data;

namespace Nop.Services.Messages
{
    public partial class WorkflowMessageService : IWorkflowMessageService
    {
        #region Fields

        private readonly IMessageTemplateService _messageTemplateService;
        private readonly IQueuedEmailService _queuedEmailService;
        private readonly ILanguageService _languageService;
        private readonly ITokenizer _tokenizer;
        private readonly IEmailAccountService _emailAccountService;
        private readonly IMessageTokenProvider _messageTokenProvider;
        private readonly IStoreService _storeService;
        private readonly IStoreContext _storeContext;
        private readonly EmailAccountSettings _emailAccountSettings;
        private readonly IEventPublisher _eventPublisher;
        private readonly IDonationService _donationService;
        private readonly IOrderService _orderService;
        private readonly IAddressService _addressService;
        private readonly IRepository<OrderItem> _orderItemRepository;

        #endregion

        #region Ctor

        public WorkflowMessageService(IMessageTemplateService messageTemplateService,
            IQueuedEmailService queuedEmailService,
            ILanguageService languageService,
            ITokenizer tokenizer,
            IEmailAccountService emailAccountService,
            IMessageTokenProvider messageTokenProvider,
            IStoreService storeService,
            IDonationService donationService,
            IStoreContext storeContext,
            IAddressService addressService,
            EmailAccountSettings emailAccountSettings,
            IEventPublisher eventPublisher,
            IOrderService orderService,
            IRepository<OrderItem> orderItemRepository)
        {
            this._messageTemplateService = messageTemplateService;
            this._queuedEmailService = queuedEmailService;
            this._languageService = languageService;
            this._tokenizer = tokenizer;
            this._emailAccountService = emailAccountService;
            this._messageTokenProvider = messageTokenProvider;
            this._storeService = storeService;
            this._addressService = addressService;
            this._storeContext = storeContext;
            this._emailAccountSettings = emailAccountSettings;
            this._eventPublisher = eventPublisher;
            this._donationService = donationService;
            this._orderService = orderService;
            this._orderItemRepository = orderItemRepository;
        }

        #endregion

        #region Utilities

        protected virtual int SendNotification(MessageTemplate messageTemplate,
            EmailAccount emailAccount, int languageId, IEnumerable<Token> tokens,
            string toEmailAddress, string toName,
            string attachmentFilePath = null, string attachmentFileName = null,
            string replyToEmailAddress = null, string replyToName = null,
            bool isStoreEmail = false)	/// NU-70
        {
            if (messageTemplate == null)
                throw new ArgumentNullException("messageTemplate");
            if (emailAccount == null)
                throw new ArgumentNullException("emailAccount");

            //retrieve localized message template data
            var bcc = messageTemplate.GetLocalized(mt => mt.BccEmailAddresses, languageId);
            var subject = messageTemplate.GetLocalized(mt => mt.Subject, languageId);
            var body = messageTemplate.GetLocalized(mt => mt.Body, languageId);

            //Replace subject and body tokens 
            var subjectReplaced = _tokenizer.Replace(subject, tokens, false);
            var bodyReplaced = _tokenizer.Replace(body, tokens, true);

            //limit name length
            toName = CommonHelper.EnsureMaximumLength(toName, 300);

            #region NU-70
            int emailId = -1;

            String[] fromEmails = emailAccount.Email.Split(new char[] { ',', ';' },
                                                             StringSplitOptions.RemoveEmptyEntries);
            String[] toEmails = toEmailAddress.Split(new char[] { ',', ';' },
                                                             StringSplitOptions.RemoveEmptyEntries);

            if (isStoreEmail)
            {
                foreach (String toEmail in toEmails)
                {
                    var email = new QueuedEmail
                    {
                        Priority = QueuedEmailPriority.High,
                        From = fromEmails[0],
                        FromName = emailAccount.DisplayName,
                        To = toEmail,
                        ToName = toName,
                        ReplyTo = fromEmails[0],
                        ReplyToName = replyToName,
                        CC = string.Empty,
                        Bcc = bcc,
                        Subject = subjectReplaced,
                        Body = bodyReplaced,
                        AttachmentFilePath = attachmentFilePath,
                        AttachmentFileName = attachmentFileName,
                        AttachedDownloadId = messageTemplate.AttachedDownloadId,
                        CreatedOnUtc = DateTime.UtcNow,
                        EmailAccountId = emailAccount.Id,
                        DontSendBeforeDateUtc = !messageTemplate.DelayBeforeSend.HasValue ? null
                        : (DateTime?)(DateTime.UtcNow + TimeSpan.FromHours(messageTemplate.DelayPeriod.ToHours(messageTemplate.DelayBeforeSend.Value)))
                    };

                    _queuedEmailService.InsertQueuedEmail(email);

                    emailId = email.Id;
                }
            }
            else
            {
                var email = new QueuedEmail
                {
                    Priority = QueuedEmailPriority.High,
                    From = fromEmails[0],
                    FromName = emailAccount.DisplayName,
                    To = toEmailAddress,
                    ToName = toName,
                    ReplyTo = fromEmails[0],
                    ReplyToName = replyToName,
                    CC = string.Empty,
                    Bcc = bcc,
                    Subject = subjectReplaced,
                    Body = bodyReplaced,
                    AttachmentFilePath = attachmentFilePath,
                    AttachmentFileName = attachmentFileName,
                    AttachedDownloadId = messageTemplate.AttachedDownloadId,
                    CreatedOnUtc = DateTime.UtcNow,
                    EmailAccountId = emailAccount.Id,
                    DontSendBeforeDateUtc = !messageTemplate.DelayBeforeSend.HasValue ? null
                        : (DateTime?)(DateTime.UtcNow + TimeSpan.FromHours(messageTemplate.DelayPeriod.ToHours(messageTemplate.DelayBeforeSend.Value)))
                };

                _queuedEmailService.InsertQueuedEmail(email);

                emailId = email.Id;
            }

            return emailId;
            #endregion
        }

        protected virtual MessageTemplate GetActiveMessageTemplate(string messageTemplateName, int storeId)
        {
            var messageTemplate = _messageTemplateService.GetMessageTemplateByName(messageTemplateName, storeId);

            //no template found
            if (messageTemplate == null)
                return null;

            //ensure it's active
            var isActive = messageTemplate.IsActive;
            if (!isActive)
                return null;

            return messageTemplate;
        }

        protected virtual EmailAccount GetEmailAccountOfMessageTemplate(MessageTemplate messageTemplate, int languageId)
        {
            var emailAccountId = messageTemplate.GetLocalized(mt => mt.EmailAccountId, languageId);
            //some 0 validation (for localizable "Email account" dropdownlist which saves 0 if "Standard" value is chosen)
            if (emailAccountId == 0)
                emailAccountId = messageTemplate.EmailAccountId;

            var emailAccount = _emailAccountService.GetEmailAccountById(emailAccountId);
            if (emailAccount == null)
                emailAccount = _emailAccountService.GetEmailAccountById(_emailAccountSettings.DefaultEmailAccountId);
            //if (emailAccount == null)
            //    emailAccount = _emailAccountService.GetAllEmailAccounts().FirstOrDefault();

            #region SODMYWAY-2949 - Ecomm Upgrade - Email Accounts
            var storeContact = _storeService.GetStoreContactByEmailTypeId(messageTemplate.EmailTypeId, _storeContext.CurrentStore.Id);


            emailAccount.DisplayName = storeContact.DisplayName;
            emailAccount.Email = storeContact.Email;
            #endregion

            return emailAccount;

        }

        protected virtual int EnsureLanguageIsActive(int languageId, int storeId)
        {
            //load language by specified ID
            var language = _languageService.GetLanguageById(languageId);

            if (language == null || !language.Published)
            {
                //load any language from the specified store
                language = _languageService.GetAllLanguages(storeId: storeId).FirstOrDefault();
            }
            if (language == null || !language.Published)
            {
                //load any language
                language = _languageService.GetAllLanguages().FirstOrDefault();
            }

            if (language == null)
                throw new Exception("No active language could be loaded");
            return language.Id;
        }

        #endregion

        #region Methods

        #region Customer workflow

        /// <summary>
        /// Sends 'New customer' notification message to a store owner
        /// </summary>
        /// <param name="customer">Customer instance</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendCustomerRegisteredNotificationMessage(Customer customer, int languageId)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            var store = _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = GetActiveMessageTemplate("NewCustomer.Notification", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            _messageTokenProvider.AddCustomerTokens(tokens, customer);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var toEmail = emailAccount.Email;
            var toName = emailAccount.DisplayName;
            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
                toEmail, toName);
        }

        /// <summary>
        /// Sends a welcome message to a customer
        /// </summary>
        /// <param name="customer">Customer instance</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendCustomerWelcomeMessage(Customer customer, int languageId)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            var store = _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = GetActiveMessageTemplate("Customer.WelcomeMessage", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            _messageTokenProvider.AddCustomerTokens(tokens, customer);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var toEmail = customer.Email;
            var toName = customer.GetFullName();
            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
                toEmail, toName);
        }

        /// <summary>
        /// Sends an email validation message to a customer
        /// </summary>
        /// <param name="customer">Customer instance</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendCustomerEmailValidationMessage(Customer customer, int languageId)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            var store = _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = GetActiveMessageTemplate("Customer.EmailValidationMessage", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            _messageTokenProvider.AddCustomerTokens(tokens, customer);


            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var toEmail = customer.Email;
            var toName = customer.GetFullName();
            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
                toEmail, toName);
        }

        /// <summary>
        /// Sends password recovery message to a customer
        /// </summary>
        /// <param name="customer">Customer instance</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendCustomerPasswordRecoveryMessage(Customer customer, int languageId)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            var store = _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = GetActiveMessageTemplate("Customer.PasswordRecovery", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            _messageTokenProvider.AddCustomerTokens(tokens, customer);


            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var toEmail = customer.Email;
            var toName = customer.GetFullName();
            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
                toEmail, toName);
        }

        #endregion

        #region Order workflow
        /// <summary>
        /// Sends an order placed notification to a vendor
        /// </summary>
        /// <param name="order">Order instance</param>
        /// <param name="vendor">Vendor instance</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendOrderPlacedVendorNotification(Order order, Vendor vendor, int languageId)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            if (vendor == null)
                throw new ArgumentNullException("vendor");

            var store = _storeService.GetStoreById(order.StoreId) ?? _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = GetActiveMessageTemplate("OrderPlaced.VendorNotification", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            _messageTokenProvider.AddOrderTokens(tokens, order, languageId, vendor.Id);
            _messageTokenProvider.AddCustomerTokens(tokens, order.Customer);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var toEmail = vendor.Email;
            var toName = vendor.Name;
            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
                toEmail, toName);
        }

        /// <summary>
        /// Sends an order placed notification to a store owner
        /// </summary>
        /// <param name="order">Order instance</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual string SendOrderPlacedorDonationStoreOwnerNotification(int orderId, int languageId)
        {
            if (orderId < 0)
                throw new ArgumentNullException("orderId not available");

            Order order = _orderService.GetOrderById(orderId);
            List<int> queuedEmailIDList = new List<int>();
            int EmailID = 0;
            string queuedEmailID = "";

            var store = _storeService.GetStoreById(order.StoreId) ?? _storeContext.CurrentStore;
            var toEmail = "";
            languageId = EnsureLanguageIsActive(languageId, store.Id);

            List<OrderItem> donationOrderItems = new List<OrderItem>();
            List<OrderItem> orderItems = new List<OrderItem>();

            orderItems = _orderService.GetOrderItemsByOrderID(orderId);  //order.OrderItems.ToList();

            foreach (OrderItem orderItem in orderItems)
            {
                if (orderItem.Product.IsDonation)
                {
                    donationOrderItems.Add(orderItem);
                }
            }

            // for donation order placed  notification

            if (donationOrderItems.Count > 0)
            {
                foreach (OrderItem product in donationOrderItems)
                {
                    List<Donation> donations = _donationService.GetDonations(store.Id, product.Id, false, null, null, null);
                    string templateName = "DonationOrderPlaced.StoreOwnerNotification";
                    MessageTemplate messageTemplate = this.GetActiveMessageTemplate(templateName, store.Id);

                    if (messageTemplate == null)
                    {
                        return null;
                    }

                    var emailAccount = this.GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

                    //tokens
                    var tokens = new List<Token>();
                    _messageTokenProvider.AddDonationTokens(tokens, order, donations, languageId, true);
                    _messageTokenProvider.AddOrderTokens(tokens, order, languageId, 0);
                    _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);

                    //event notification
                    _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

                    if (product.SelectedFulfillmentWarehouse != null)
                    {
                        int PickupaddressId = product.SelectedFulfillmentWarehouse.AddressId;
                        var address = _addressService.GetAddressById(PickupaddressId);
                        if (address.Email != null)
                            toEmail = address.Email;
                        else
                            toEmail = emailAccount.Email;
                    }
                    else
                    {
                        toEmail = emailAccount.Email;
                    }

                    string toName = string.Format("{0}", emailAccount.DisplayName);

                    int id = this.SendNotification(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
                    queuedEmailIDList.Add(id);

                }
            }
            else
            {
                var messageTemplate = GetActiveMessageTemplate("OrderPlaced.StoreOwnerNotification", store.Id);
                if (messageTemplate == null)
                    return null;

                //email account
                var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);
                var toName = emailAccount.DisplayName;
                //tokens
                var tokens = new List<Token>();
                _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
                _messageTokenProvider.AddOrderTokens(tokens, order, languageId);
                _messageTokenProvider.AddCustomerTokens(tokens, order.Customer);

                //event notification
                _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

                for (int i = 0; i <= orderItems.Count - 1; i++)
                {
                    var orderItem = orderItems[i];

                    if (orderItem.SelectedFulfillmentWarehouse != null)
                    {
                        int PickupaddressId = orderItem.SelectedFulfillmentWarehouse.AddressId;
                        var address = _addressService.GetAddressById(PickupaddressId);
                        if (address.Email != null)
                            toEmail = address.Email;
                        else
                            toEmail = emailAccount.Email;
                    }
                    else
                    {
                        toEmail = emailAccount.Email;
                    }
                    //_orderItemRepository.Detach(orderItem);
                }


                EmailID = SendNotification(messageTemplate, emailAccount,
                            languageId, tokens,
                            toEmail, toName,
                            isStoreEmail: true);

                queuedEmailIDList.Add(EmailID);

                /// NU-70
            }

            if (queuedEmailIDList.Count > 1)
            {
                queuedEmailID = string.Join(",", queuedEmailIDList.Select(n => n.ToString()).ToArray());
            }
            else
            {
                queuedEmailID = queuedEmailIDList.FirstOrDefault().ToString();
            }

            return queuedEmailID;
        }

        /// <summary>
        /// Sends an order placed notification to a store owner
        /// </summary>
        /// <param name="order">Order instance</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendOrderPlacedStoreOwnerNotification(Order order, int languageId)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            var store = _storeService.GetStoreById(order.StoreId) ?? _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = GetActiveMessageTemplate("OrderPlaced.StoreOwnerNotification", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            _messageTokenProvider.AddOrderTokens(tokens, order, languageId);
            _messageTokenProvider.AddCustomerTokens(tokens, order.Customer);




            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var toEmail = emailAccount.Email;
            var toName = emailAccount.DisplayName;
            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
                toEmail, toName,
                isStoreEmail: true); /// NU-70
        }

        /// <summary>
        /// Sends an order paid notification to a store owner
        /// </summary>
        /// <param name="order">Order instance</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendOrderPaidStoreOwnerNotification(Order order, int languageId)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            var store = _storeService.GetStoreById(order.StoreId) ?? _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = GetActiveMessageTemplate("OrderPaid.StoreOwnerNotification", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            _messageTokenProvider.AddOrderTokens(tokens, order, languageId);
            _messageTokenProvider.AddCustomerTokens(tokens, order.Customer);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var toEmail = emailAccount.Email;
            var toName = emailAccount.DisplayName;
            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
                toEmail, toName,
                isStoreEmail: true); /// NU-70
        }

        /// <summary>
        /// Sends an order paid notification to a customer
        /// </summary>
        /// <param name="order">Order instance</param>
        /// <param name="languageId">Message language identifier</param>
        /// <param name="attachmentFilePath">Attachment file path</param>
        /// <param name="attachmentFileName">Attachment file name. If specified, then this file name will be sent to a recipient. Otherwise, "AttachmentFilePath" name will be used.</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendOrderPaidCustomerNotification(Order order, int languageId,
            string attachmentFilePath = null, string attachmentFileName = null)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            var store = _storeService.GetStoreById(order.StoreId) ?? _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = GetActiveMessageTemplate("OrderPaid.CustomerNotification", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            _messageTokenProvider.AddOrderTokens(tokens, order, languageId);
            _messageTokenProvider.AddCustomerTokens(tokens, order.Customer);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var toEmail = order.BillingAddress.Email;
            var toName = string.Format("{0} {1}", order.BillingAddress.FirstName, order.BillingAddress.LastName);
            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
                toEmail, toName,
                attachmentFilePath,
                attachmentFileName);
        }

        /// <summary>
        /// Sends an order paid notification to a vendor
        /// </summary>
        /// <param name="order">Order instance</param>
        /// <param name="vendor">Vendor instance</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendOrderPaidVendorNotification(Order order, Vendor vendor, int languageId)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            if (vendor == null)
                throw new ArgumentNullException("vendor");

            var store = _storeService.GetStoreById(order.StoreId) ?? _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = GetActiveMessageTemplate("OrderPaid.VendorNotification", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            _messageTokenProvider.AddOrderTokens(tokens, order, languageId, vendor.Id);
            _messageTokenProvider.AddCustomerTokens(tokens, order.Customer);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var toEmail = vendor.Email;
            var toName = vendor.Name;
            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
                toEmail, toName);
        }

        /// <summary>
        /// Sends an order placed notification to a customer
        /// </summary>
        /// <param name="order">Order instance</param>
        /// <param name="languageId">Message language identifier</param>
        /// <param name="attachmentFilePath">Attachment file path</param>
        /// <param name="attachmentFileName">Attachment file name. If specified, then this file name will be sent to a recipient. Otherwise, "AttachmentFilePath" name will be used.</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendOrderPlacedCustomerNotification(Order order, int languageId,
            string attachmentFilePath = null, string attachmentFileName = null)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            var store = _storeService.GetStoreById(order.StoreId) ?? _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = GetActiveMessageTemplate("OrderPlaced.CustomerNotification", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            _messageTokenProvider.AddOrderTokens(tokens, order, languageId);
            _messageTokenProvider.AddCustomerTokens(tokens, order.Customer);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var toEmail = order.BillingAddress.Email;
            var toName = string.Format("{0} {1}", order.BillingAddress.FirstName, order.BillingAddress.LastName);
            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
                toEmail, toName,
                attachmentFilePath,
                attachmentFileName);
        }
        /// <summary>
        /// Sends an order placed notification to a customer
        /// </summary>
        /// <param name="order">Order instance</param>
        /// <param name="languageId">Message language identifier</param>
        /// <param name="attachmentFilePath">Attachment file path</param>
        /// <param name="attachmentFileName">Attachment file name. If specified, then this file name will be sent to a recipient. Otherwise, "AttachmentFilePath" name will be used.</param>
        /// <returns>Queued email identifier</returns>
        public virtual List<int> SendOrderPlacedCustomerOrDonationCustomerNotification(Order order, int languageId,
            string attachmentFilePath = null, string attachmentFileName = null)
        {
            List<int> returnIds = new List<int>();

            if (order == null)
            {
                throw new ArgumentNullException("order");
            }

            var store = _storeService.GetStoreById(order.StoreId) ?? _storeContext.CurrentStore;

            languageId = this.EnsureLanguageIsActive(languageId, store.Id);

            List<OrderItem> donationOrderItems = new List<OrderItem>();
            List<OrderItem> orderItems = new List<OrderItem>();
            orderItems = _orderService.GetOrderItemsByOrderID(order.Id).ToList();

            foreach (OrderItem orderItem in orderItems)
            {
                if (orderItem.Product.IsDonation)
                {
                    donationOrderItems.Add(orderItem);
                }

            }
           
            // for donation order placed  notification

            if (donationOrderItems.Count > 0)
            {

                int id = SendDonationOrderPlacedCustomerNotification(order, languageId);

                returnIds.Add(id);
            }
            else
            {
                var messageTemplate = GetActiveMessageTemplate("OrderPlaced.CustomerNotification", store.Id);
                if (messageTemplate == null)
                    return null;

                //email account
                var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

                //tokens
                var tokens = new List<Token>();
                _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
                _messageTokenProvider.AddOrderTokens(tokens, order, languageId);
                _messageTokenProvider.AddCustomerTokens(tokens, order.Customer);

                //event notification
                _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

                var toEmail = order.BillingAddress.Email;
                var toName = string.Format("{0} {1}", order.BillingAddress.FirstName, order.BillingAddress.LastName);

                int id = SendNotification(messageTemplate, emailAccount,
                    languageId, tokens,
                    toEmail, toName,
                    attachmentFilePath,
                    attachmentFileName);
                returnIds.Add(id);
            }


            return returnIds;


        }


        /// <summary>
        /// For Corporate Notification
        /// </summary>
        /// <param name="order"></param>
        /// <param name="languageId"></param>
        /// <returns></returns>
        public virtual int SendDonationPlacedForCorporateNotification(Order order, int languageId)
        {
            if (order == null)
            {
                throw new ArgumentNullException("order");
            }

            var store = _storeService.GetStoreById(order.StoreId) ?? _storeContext.CurrentStore;


            languageId = this.EnsureLanguageIsActive(languageId, store.Id);
            List<Donation> allDonations = new List<Donation>();
            foreach (OrderItem orderItem in order.OrderItems)
            {
                if (orderItem.Product.IsCorporateDonation)
                {
                    List<Donation> donations = _donationService.GetDonations(store.Id, orderItem.Id, false, null, null, null);

                    foreach (Donation donation in donations)
                    {
                        allDonations.Add(donation);
                    }
                }
            }


            if (allDonations.Count > 0)
            {

                var messageTemplate = this.GetActiveMessageTemplate("DonationOrderPlaced.CorporationNotification", store.Id);
                if (messageTemplate == null)
                {
                    return 0;
                }

                var emailAccount = this.GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

                //tokens
                var tokens = new List<Token>();
                _messageTokenProvider.AddDonationTokens(tokens, order, allDonations, languageId, true);
                _messageTokenProvider.AddOrderTokens(tokens, order, languageId, 0);
                _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);

                //event notification
                _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

                string toEmail = order.BillingAddress.Email;
                string toName = string.Format("{0} {1}", order.BillingAddress.FirstName, order.BillingAddress.LastName);
                return this.SendNotification(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
            }
            return 0;
        }


        //---START: Codechages done by (na-sdxcorp\ADas)--------------

        /// <summary>
        /// Sends a shipment sent notification to store owner
        /// </summary>
        /// <param name="shipment">Shipment</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendShipmentSentStoreOwnerNotification(Shipment shipment, int languageId)
        {
            if (shipment == null)
                throw new ArgumentNullException("shipment");

            var order = shipment.Order;
            if (order == null)
                throw new Exception("Order cannot be loaded");

            var store = _storeService.GetStoreById(order.StoreId) ?? _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = GetActiveMessageTemplate("OrderDelivered.StoreOwnerNotification", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            _messageTokenProvider.AddShipmentTokens(tokens, shipment, languageId);
            _messageTokenProvider.AddOrderTokens(tokens, shipment.Order, languageId);
            _messageTokenProvider.AddCustomerTokens(tokens, shipment.Order.Customer);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var toEmail = emailAccount.Email;
            var toName = emailAccount.DisplayName;
            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
                toEmail, toName);
        }

        /// <summary>
        /// Sends a shipment delivered notification to store owner
        /// </summary>
        /// <param name="shipment">Shipment</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendShipmentDeliveredStoreOwnerNotification(Shipment shipment, int languageId)
        {
            if (shipment == null)
                throw new ArgumentNullException("shipment");

            var order = shipment.Order;
            if (order == null)
                throw new Exception("Order cannot be loaded");

            var store = _storeService.GetStoreById(order.StoreId) ?? _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = GetActiveMessageTemplate("ShipmentDelivered.CustomerNotification", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            _messageTokenProvider.AddShipmentTokens(tokens, shipment, languageId);
            _messageTokenProvider.AddOrderTokens(tokens, shipment.Order, languageId);
            _messageTokenProvider.AddCustomerTokens(tokens, shipment.Order.Customer);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var toEmail = emailAccount.Email;
            var toName = emailAccount.DisplayName;
            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
                toEmail, toName);
        }




        //---END: Codechages done by (na-sdxcorp\ADas)--------------

        /// <summary>
        /// Sends a shipment sent notification to a customer
        /// </summary>
        /// <param name="shipment">Shipment</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendShipmentSentCustomerNotification(Shipment shipment, int languageId)
        {
            if (shipment == null)
                throw new ArgumentNullException("shipment");

            var order = shipment.Order;
            if (order == null)
                throw new Exception("Order cannot be loaded");

            var store = _storeService.GetStoreById(order.StoreId) ?? _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

            //---START: Codechages done by (na-sdxcorp\ADas)--------------
            //--No entry found for "ShipmentSent.CustomerNotification" in MessageTemplate table
            //--Instead found "OrderShipped.CustomerNotification"
            var messageTemplate = GetActiveMessageTemplate("OrderShipped.CustomerNotification"/*"ShipmentSent.CustomerNotification"*/, store.Id);
            //---END: Codechages done by (na-sdxcorp\ADas)--------------

            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            _messageTokenProvider.AddShipmentTokens(tokens, shipment, languageId);
            _messageTokenProvider.AddOrderTokens(tokens, shipment.Order, languageId);
            _messageTokenProvider.AddCustomerTokens(tokens, shipment.Order.Customer);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var toEmail = order.BillingAddress.Email;
            var toName = string.Format("{0} {1}", order.BillingAddress.FirstName, order.BillingAddress.LastName);
            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
                toEmail, toName);
        }


        #region SODMYWAY-3467
        /// <summary>
        /// 	Sends an product delivered notification to a customer
        /// </summary>
        /// <param name = "sourceSiteId"></param>
        /// <param name = "order">Order instance</param>
        /// <param name = "languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendFulfillementCustomerNotification(OrderItem orderItem, int languageId)
        {


            if (orderItem == null)
                throw new ArgumentNullException("orderItem");

            var order = _orderService.GetOrderByOrderItemId(orderItem.Id);

            var store = _storeService.GetStoreById(order.StoreId) ?? _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = GetActiveMessageTemplate("Fulfilled.CustomerNotification", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            _messageTokenProvider.AddOrderTokens(tokens, orderItem.Order, languageId);
            _messageTokenProvider.AddCustomerTokens(tokens, orderItem.Order.Customer);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var toEmail = order.BillingAddress.Email;
            var toName = string.Format("{0} {1}", order.BillingAddress.FirstName, order.BillingAddress.LastName);
            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
                toEmail, toName);

        }
        #endregion

        #region Customer Creation Mail
        public virtual int SendCustomerCreationNotification(Customer customer, int languageId)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");
            var store = _storeService.GetStoreById(_storeContext.CurrentStore.Id) ?? _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, _storeContext.CurrentStore.Id);

            var messageTemplate = GetActiveMessageTemplate("Customer.NewCustomerCreatedNotification", _storeContext.CurrentStore.Id);
            if (messageTemplate == null)
                return 0;
            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            _messageTokenProvider.AddCustomerTokens(tokens, customer);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);


            var toEmail = emailAccount.Email;
            var toName = string.Format("{0}", customer.Username);
            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
               toEmail, toName);


        }
        #endregion
        /// <summary>
        /// Sends a shipment delivered notification to a customer
        /// </summary>
        /// <param name="shipment">Shipment</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendShipmentDeliveredCustomerNotification(Shipment shipment, int languageId)
        {
            if (shipment == null)
                throw new ArgumentNullException("shipment");

            var order = shipment.Order;
            if (order == null)
                throw new Exception("Order cannot be loaded");

            var store = _storeService.GetStoreById(order.StoreId) ?? _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

            //---START: Codechages done by (na-sdxcorp\ADas)--------------
            //--No entry found for "ShipmentSent.CustomerNotification" in MessageTemplate table
            //--Instead found "OrderDelivered.CustomerNotification"
            var messageTemplate = GetActiveMessageTemplate("OrderDelivered.CustomerNotification"/*"ShipmentDelivered.CustomerNotification"*/, store.Id);
            //---END: Codechages done by (na-sdxcorp\ADas)--------------
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            _messageTokenProvider.AddShipmentTokens(tokens, shipment, languageId);
            _messageTokenProvider.AddOrderTokens(tokens, shipment.Order, languageId);
            _messageTokenProvider.AddCustomerTokens(tokens, shipment.Order.Customer);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var toEmail = order.BillingAddress.Email;
            var toName = string.Format("{0} {1}", order.BillingAddress.FirstName, order.BillingAddress.LastName);
            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
                toEmail, toName);
        }

        /// <summary>
        /// Sends an order completed notification to a customer
        /// </summary>
        /// <param name="order">Order instance</param>
        /// <param name="languageId">Message language identifier</param>
        /// <param name="attachmentFilePath">Attachment file path</param>
        /// <param name="attachmentFileName">Attachment file name. If specified, then this file name will be sent to a recipient. Otherwise, "AttachmentFilePath" name will be used.</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendOrderCompletedCustomerNotification(Order order, int languageId,
            string attachmentFilePath = null, string attachmentFileName = null)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            var store = _storeService.GetStoreById(order.StoreId) ?? _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = GetActiveMessageTemplate("OrderCompleted.CustomerNotification", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            _messageTokenProvider.AddOrderTokens(tokens, order, languageId);
            _messageTokenProvider.AddCustomerTokens(tokens, order.Customer);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var toEmail = order.BillingAddress.Email;
            var toName = string.Format("{0} {1}", order.BillingAddress.FirstName, order.BillingAddress.LastName);
            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
                toEmail, toName,
                attachmentFilePath,
                attachmentFileName);
        }

        /// <summary>
        /// Sends an order cancelled notification to a customer
        /// </summary>
        /// <param name="order">Order instance</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendOrderCancelledCustomerNotification(Order order, int languageId)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            var store = _storeService.GetStoreById(order.StoreId) ?? _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = GetActiveMessageTemplate("OrderCancelled.CustomerNotification", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            _messageTokenProvider.AddOrderTokens(tokens, order, languageId);
            _messageTokenProvider.AddCustomerTokens(tokens, order.Customer);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var toEmail = order.BillingAddress.Email;
            var toName = string.Format("{0} {1}", order.BillingAddress.FirstName, order.BillingAddress.LastName);
            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
                toEmail, toName);
        }

        /// <summary>
        /// Sends an order refunded notification to a store owner
        /// </summary>
        /// <param name="order">Order instance</param>
        /// <param name="refundedAmount">Amount refunded</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendOrderRefundedStoreOwnerNotification(Order order, decimal refundedAmount, int languageId)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            var store = _storeService.GetStoreById(order.StoreId) ?? _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = GetActiveMessageTemplate("OrderRefunded.StoreOwnerNotification", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            _messageTokenProvider.AddOrderTokens(tokens, order, languageId);
            _messageTokenProvider.AddOrderRefundedTokens(tokens, order, refundedAmount);
            _messageTokenProvider.AddCustomerTokens(tokens, order.Customer);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var toEmail = emailAccount.Email;
            var toName = emailAccount.DisplayName;
            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
                toEmail, toName,
                isStoreEmail: true); /// NU-70
        }

        /// <summary>
        /// Sends an order refunded notification to a customer
        /// </summary>
        /// <param name="order">Order instance</param>
        /// <param name="refundedAmount">Amount refunded</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendOrderRefundedCustomerNotification(Order order, decimal refundedAmount, int languageId)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            var store = _storeService.GetStoreById(order.StoreId) ?? _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = GetActiveMessageTemplate("OrderRefund.CustomerNotification", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            _messageTokenProvider.AddOrderTokens(tokens, order, languageId);
            _messageTokenProvider.AddOrderRefundedTokens(tokens, order, refundedAmount);
            _messageTokenProvider.AddCustomerTokens(tokens, order.Customer);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var toEmail = order.BillingAddress.Email;
            var toName = string.Format("{0} {1}", order.BillingAddress.FirstName, order.BillingAddress.LastName);
            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
                toEmail, toName);
        }

        /// <summary>
        /// Sends a new order note added notification to a customer
        /// </summary>
        /// <param name="orderNote">Order note</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendNewOrderNoteAddedCustomerNotification(OrderNote orderNote, int languageId)
        {
            if (orderNote == null)
                throw new ArgumentNullException("orderNote");

            var order = orderNote.Order;

            var store = _storeService.GetStoreById(order.StoreId) ?? _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = GetActiveMessageTemplate("Customer.NewOrderNote", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            _messageTokenProvider.AddOrderNoteTokens(tokens, orderNote);
            _messageTokenProvider.AddOrderTokens(tokens, orderNote.Order, languageId);
            _messageTokenProvider.AddCustomerTokens(tokens, orderNote.Order.Customer);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var toEmail = order.BillingAddress.Email;
            var toName = string.Format("{0} {1}", order.BillingAddress.FirstName, order.BillingAddress.LastName);
            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
                toEmail, toName);
        }

        /// <summary>
        /// Sends a "Recurring payment cancelled" notification to a store owner
        /// </summary>
        /// <param name="recurringPayment">Recurring payment</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendRecurringPaymentCancelledStoreOwnerNotification(RecurringPayment recurringPayment, int languageId)
        {
            if (recurringPayment == null)
                throw new ArgumentNullException("recurringPayment");

            var store = _storeService.GetStoreById(recurringPayment.InitialOrder.StoreId) ?? _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = GetActiveMessageTemplate("RecurringPaymentCancelled.StoreOwnerNotification", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            _messageTokenProvider.AddOrderTokens(tokens, recurringPayment.InitialOrder, languageId);
            _messageTokenProvider.AddCustomerTokens(tokens, recurringPayment.InitialOrder.Customer);
            _messageTokenProvider.AddRecurringPaymentTokens(tokens, recurringPayment);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var toEmail = emailAccount.Email;
            var toName = emailAccount.DisplayName;
            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
                toEmail, toName,
                isStoreEmail: true); /// NU-70
        }

        #endregion

        #region Newsletter workflow

        /// <summary>
        /// Sends a newsletter subscription activation message
        /// </summary>
        /// <param name="subscription">Newsletter subscription</param>
        /// <param name="languageId">Language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendNewsLetterSubscriptionActivationMessage(NewsLetterSubscription subscription,
            int languageId)
        {
            if (subscription == null)
                throw new ArgumentNullException("subscription");

            var store = _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = GetActiveMessageTemplate("NewsLetterSubscription.ActivationMessage", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            _messageTokenProvider.AddNewsLetterSubscriptionTokens(tokens, subscription);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var toEmail = subscription.Email;
            var toName = "";
            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
                toEmail, toName);
        }

        /// <summary>
        /// Sends a newsletter subscription deactivation message
        /// </summary>
        /// <param name="subscription">Newsletter subscription</param>
        /// <param name="languageId">Language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendNewsLetterSubscriptionDeactivationMessage(NewsLetterSubscription subscription,
            int languageId)
        {
            if (subscription == null)
                throw new ArgumentNullException("subscription");

            var store = _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = GetActiveMessageTemplate("NewsLetterSubscription.DeactivationMessage", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            _messageTokenProvider.AddNewsLetterSubscriptionTokens(tokens, subscription);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var toEmail = subscription.Email;
            var toName = "";
            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
                toEmail, toName);
        }

        #endregion

        #region Send a message to a friend

        /// <summary>
        /// Sends "email a friend" message
        /// </summary>
        /// <param name="customer">Customer instance</param>
        /// <param name="languageId">Message language identifier</param>
        /// <param name="product">Product instance</param>
        /// <param name="customerEmail">Customer's email</param>
        /// <param name="friendsEmail">Friend's email</param>
        /// <param name="personalMessage">Personal message</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendProductEmailAFriendMessage(Customer customer, int languageId,
            Product product, string customerEmail, string friendsEmail, string personalMessage)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            if (product == null)
                throw new ArgumentNullException("product");

            var store = _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = GetActiveMessageTemplate("Service.EmailAFriend", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            _messageTokenProvider.AddCustomerTokens(tokens, customer);
            _messageTokenProvider.AddProductTokens(tokens, product, languageId);
            tokens.Add(new Token("EmailAFriend.PersonalMessage", personalMessage, true));
            tokens.Add(new Token("EmailAFriend.Email", customerEmail));

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var toEmail = friendsEmail;
            var toName = "";
            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
                toEmail, toName);
        }

        /// <summary>
        /// Sends wishlist "email a friend" message
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="languageId">Message language identifier</param>
        /// <param name="customerEmail">Customer's email</param>
        /// <param name="friendsEmail">Friend's email</param>
        /// <param name="personalMessage">Personal message</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendWishlistEmailAFriendMessage(Customer customer, int languageId,
             string customerEmail, string friendsEmail, string personalMessage)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            var store = _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = GetActiveMessageTemplate("Wishlist.EmailAFriend", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            _messageTokenProvider.AddCustomerTokens(tokens, customer);
            tokens.Add(new Token("Wishlist.PersonalMessage", personalMessage, true));
            tokens.Add(new Token("Wishlist.Email", customerEmail));

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var toEmail = friendsEmail;
            var toName = "";
            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
                toEmail, toName);
        }

        #endregion

        #region Return requests

        /// <summary>
        /// Sends 'New Return Request' message to a store owner
        /// </summary>
        /// <param name="returnRequest">Return request</param>
        /// <param name="orderItem">Order item</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendNewReturnRequestStoreOwnerNotification(ReturnRequest returnRequest, OrderItem orderItem, int languageId)
        {
            if (returnRequest == null)
                throw new ArgumentNullException("returnRequest");

            var store = _storeService.GetStoreById(orderItem.Order.StoreId) ?? _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = GetActiveMessageTemplate("NewReturnRequest.StoreOwnerNotification", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            _messageTokenProvider.AddCustomerTokens(tokens, returnRequest.Customer);
            _messageTokenProvider.AddReturnRequestTokens(tokens, returnRequest, orderItem);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var toEmail = emailAccount.Email;
            var toName = emailAccount.DisplayName;
            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
                toEmail, toName,
                isStoreEmail: true); /// NU-70
        }

        /// <summary>
        /// Sends 'Return Request status changed' message to a customer
        /// </summary>
        /// <param name="returnRequest">Return request</param>
        /// <param name="orderItem">Order item</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendReturnRequestStatusChangedCustomerNotification(ReturnRequest returnRequest, OrderItem orderItem, int languageId)
        {
            if (returnRequest == null)
                throw new ArgumentNullException("returnRequest");

            var store = _storeService.GetStoreById(orderItem.Order.StoreId) ?? _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = GetActiveMessageTemplate("ReturnRequestStatusChanged.CustomerNotification", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            _messageTokenProvider.AddCustomerTokens(tokens, returnRequest.Customer);
            _messageTokenProvider.AddReturnRequestTokens(tokens, returnRequest, orderItem);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            string toEmail = returnRequest.Customer.IsGuest() ?
                orderItem.Order.BillingAddress.Email :
                returnRequest.Customer.Email;
            var toName = returnRequest.Customer.IsGuest() ?
                orderItem.Order.BillingAddress.FirstName :
                returnRequest.Customer.GetFullName();
            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
                toEmail, toName);
        }

        #endregion

        #region Forum Notifications

        /// <summary>
        /// Sends a forum subscription message to a customer
        /// </summary>
        /// <param name="customer">Customer instance</param>
        /// <param name="forumTopic">Forum Topic</param>
        /// <param name="forum">Forum</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public int SendNewForumTopicMessage(Customer customer,
            ForumTopic forumTopic, Forum forum, int languageId)
        {
            if (customer == null)
            {
                throw new ArgumentNullException("customer");
            }
            var store = _storeContext.CurrentStore;

            var messageTemplate = GetActiveMessageTemplate("Forums.NewForumTopic", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            _messageTokenProvider.AddForumTopicTokens(tokens, forumTopic);
            _messageTokenProvider.AddForumTokens(tokens, forumTopic.Forum);
            _messageTokenProvider.AddCustomerTokens(tokens, customer);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var toEmail = customer.Email;
            var toName = customer.GetFullName();

            return SendNotification(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
        }

        /// <summary>
        /// Sends a forum subscription message to a customer
        /// </summary>
        /// <param name="customer">Customer instance</param>
        /// <param name="forumPost">Forum post</param>
        /// <param name="forumTopic">Forum Topic</param>
        /// <param name="forum">Forum</param>
        /// <param name="friendlyForumTopicPageIndex">Friendly (starts with 1) forum topic page to use for URL generation</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public int SendNewForumPostMessage(Customer customer,
            ForumPost forumPost, ForumTopic forumTopic,
            Forum forum, int friendlyForumTopicPageIndex, int languageId)
        {
            if (customer == null)
            {
                throw new ArgumentNullException("customer");
            }

            var store = _storeContext.CurrentStore;

            var messageTemplate = GetActiveMessageTemplate("Forums.NewForumPost", store.Id);
            if (messageTemplate == null)
            {
                return 0;
            }

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            _messageTokenProvider.AddForumPostTokens(tokens, forumPost);
            _messageTokenProvider.AddForumTopicTokens(tokens, forumPost.ForumTopic,
                friendlyForumTopicPageIndex, forumPost.Id);
            _messageTokenProvider.AddForumTokens(tokens, forumPost.ForumTopic.Forum);
            _messageTokenProvider.AddCustomerTokens(tokens, customer);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var toEmail = customer.Email;
            var toName = customer.GetFullName();

            return SendNotification(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
        }

        /// <summary>
        /// Sends a private message notification
        /// </summary>
        /// <param name="privateMessage">Private message</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public int SendPrivateMessageNotification(PrivateMessage privateMessage, int languageId)
        {
            if (privateMessage == null)
            {
                throw new ArgumentNullException("privateMessage");
            }

            var store = _storeService.GetStoreById(privateMessage.StoreId) ?? _storeContext.CurrentStore;

            var messageTemplate = GetActiveMessageTemplate("Customer.NewPM", store.Id);
            if (messageTemplate == null)
            {
                return 0;
            }

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            _messageTokenProvider.AddPrivateMessageTokens(tokens, privateMessage);
            _messageTokenProvider.AddCustomerTokens(tokens, privateMessage.ToCustomer);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var toEmail = privateMessage.ToCustomer.Email;
            var toName = privateMessage.ToCustomer.GetFullName();

            return SendNotification(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
        }

        #endregion

        #region Misc

        /// <summary>
        /// Sends 'New vendor account submitted' message to a store owner
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="vendor">Vendor</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendNewVendorAccountApplyStoreOwnerNotification(Customer customer, Vendor vendor, int languageId)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            if (vendor == null)
                throw new ArgumentNullException("vendor");

            var store = _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = GetActiveMessageTemplate("VendorAccountApply.StoreOwnerNotification", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            _messageTokenProvider.AddCustomerTokens(tokens, customer);
            _messageTokenProvider.AddVendorTokens(tokens, vendor);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var toEmail = emailAccount.Email;
            var toName = emailAccount.DisplayName;
            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
                toEmail, toName,
                isStoreEmail: true); /// NU-70
        }

        /// <summary>
        /// Sends 'Vendor information changed' message to a store owner
        /// </summary>
        /// <param name="vendor">Vendor</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendVendorInformationChangeNotification(Vendor vendor, int languageId)
        {
            if (vendor == null)
                throw new ArgumentNullException("vendor");

            var store = _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = GetActiveMessageTemplate("VendorInformationChange.StoreOwnerNotification", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            _messageTokenProvider.AddVendorTokens(tokens, vendor);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var toEmail = emailAccount.Email;
            var toName = emailAccount.DisplayName;
            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
                toEmail, toName);
        }

        /// <summary>
        /// Sends a gift card notification
        /// </summary>
        /// <param name="giftCard">Gift card</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendGiftCardNotification(GiftCard giftCard, int languageId)
        {
            if (giftCard == null)
                throw new ArgumentNullException("giftCard");

            Store store = null;
            var order = giftCard.PurchasedWithOrderItem != null ?
                giftCard.PurchasedWithOrderItem.Order :
                null;
            if (order != null)
                store = _storeService.GetStoreById(order.StoreId);
            if (store == null)
                store = _storeContext.CurrentStore;

            languageId = EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = GetActiveMessageTemplate("GiftCard.Notification", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            _messageTokenProvider.AddGiftCardTokens(tokens, giftCard);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);
            var toEmail = giftCard.RecipientEmail;
            var toName = giftCard.RecipientName;
            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
                toEmail, toName);
        }

        /// <summary>
        /// Sends a product review notification message to a store owner
        /// </summary>
        /// <param name="productReview">Product review</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendProductReviewNotificationMessage(ProductReview productReview,
            int languageId)
        {
            if (productReview == null)
                throw new ArgumentNullException("productReview");

            var store = _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = GetActiveMessageTemplate("Product.ProductReview", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            _messageTokenProvider.AddProductReviewTokens(tokens, productReview);
            _messageTokenProvider.AddCustomerTokens(tokens, productReview.Customer);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var toEmail = emailAccount.Email;
            var toName = emailAccount.DisplayName;
            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
                toEmail, toName);
        }

        /// <summary>
        /// Sends a "quantity below" notification to a store owner
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendQuantityBelowStoreOwnerNotification(Product product, int languageId)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            var store = _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = GetActiveMessageTemplate("QuantityBelow.StoreOwnerNotification", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            _messageTokenProvider.AddProductTokens(tokens, product, languageId);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var toEmail = emailAccount.Email;
            var toName = emailAccount.DisplayName;
            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
                toEmail, toName,
                isStoreEmail: true); /// NU-70
        }

        /// <summary>
        /// Sends a "quantity below" notification to a store owner
        /// </summary>
        /// <param name="combination">Attribute combination</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendQuantityBelowStoreOwnerNotification(ProductAttributeCombination combination, int languageId)
        {
            if (combination == null)
                throw new ArgumentNullException("combination");

            var store = _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = GetActiveMessageTemplate("QuantityBelow.AttributeCombination.StoreOwnerNotification", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            var product = combination.Product;

            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            _messageTokenProvider.AddProductTokens(tokens, product, languageId);
            _messageTokenProvider.AddAttributeCombinationTokens(tokens, combination, languageId);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var toEmail = emailAccount.Email;
            var toName = emailAccount.DisplayName;
            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
                toEmail, toName,
                isStoreEmail: true); /// NU-70
        }

        /// <summary>
        /// Sends a "new VAT submitted" notification to a store owner
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="vatName">Received VAT name</param>
        /// <param name="vatAddress">Received VAT address</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendNewVatSubmittedStoreOwnerNotification(Customer customer,
            string vatName, string vatAddress, int languageId)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            var store = _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = GetActiveMessageTemplate("NewVATSubmitted.StoreOwnerNotification", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            _messageTokenProvider.AddCustomerTokens(tokens, customer);
            tokens.Add(new Token("VatValidationResult.Name", vatName));
            tokens.Add(new Token("VatValidationResult.Address", vatAddress));

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var toEmail = emailAccount.Email;
            var toName = emailAccount.DisplayName;
            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
                toEmail, toName,
                isStoreEmail: true); /// NU-70
        }

        /// <summary>
        /// Sends a blog comment notification message to a store owner
        /// </summary>
        /// <param name="blogComment">Blog comment</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendBlogCommentNotificationMessage(BlogComment blogComment, int languageId)
        {
            if (blogComment == null)
                throw new ArgumentNullException("blogComment");

            var store = _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = GetActiveMessageTemplate("Blog.BlogComment", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            _messageTokenProvider.AddBlogCommentTokens(tokens, blogComment);
            _messageTokenProvider.AddCustomerTokens(tokens, blogComment.Customer);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var toEmail = emailAccount.Email;
            var toName = emailAccount.DisplayName;
            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
                toEmail, toName);
        }

        /// <summary>
        /// Sends a news comment notification message to a store owner
        /// </summary>
        /// <param name="newsComment">News comment</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendNewsCommentNotificationMessage(NewsComment newsComment, int languageId)
        {
            if (newsComment == null)
                throw new ArgumentNullException("newsComment");

            var store = _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = GetActiveMessageTemplate("News.NewsComment", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            _messageTokenProvider.AddNewsCommentTokens(tokens, newsComment);
            _messageTokenProvider.AddCustomerTokens(tokens, newsComment.Customer);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var toEmail = emailAccount.Email;
            var toName = emailAccount.DisplayName;
            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
                toEmail, toName);
        }

        /// <summary>
        /// Sends a 'Back in stock' notification message to a customer
        /// </summary>
        /// <param name="subscription">Subscription</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendBackInStockNotification(BackInStockSubscription subscription, int languageId)
        {
            if (subscription == null)
                throw new ArgumentNullException("subscription");

            var store = _storeService.GetStoreById(subscription.StoreId) ?? _storeContext.CurrentStore;
            languageId = EnsureLanguageIsActive(languageId, store.Id);

            var messageTemplate = GetActiveMessageTemplate("Customer.BackInStock", store.Id);
            if (messageTemplate == null)
                return 0;

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            _messageTokenProvider.AddCustomerTokens(tokens, subscription.Customer);
            _messageTokenProvider.AddBackInStockTokens(tokens, subscription);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var customer = subscription.Customer;
            var toEmail = customer.Email;
            var toName = customer.GetFullName();
            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
                toEmail, toName);
        }

        /// <summary>
        /// Sends a test email
        /// </summary>
        /// <param name="messageTemplateId">Message template identifier</param>
        /// <param name="sendToEmail">Send to email</param>
        /// <param name="tokens">Tokens</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendTestEmail(int messageTemplateId, string sendToEmail,
            List<Token> tokens, int languageId)
        {
            var messageTemplate = _messageTemplateService.GetMessageTemplateById(messageTemplateId);
            if (messageTemplate == null)
                throw new ArgumentException("Template cannot be loaded");

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
                sendToEmail, null);
        }

        #endregion

        #region NU-17
        /// <summary>
        /// 
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="order"></param>
        /// <param name="donation"></param>
        /// <param name="languageId"></param>
        /// <returns></returns>
        public virtual int SendDonationOrderPlacedCustomerNotification(Order order, int languageId)
        {
            if (order == null)
            {
                throw new ArgumentNullException("order");
            }

            var store = _storeService.GetStoreById(order.StoreId) ?? _storeContext.CurrentStore;


            languageId = this.EnsureLanguageIsActive(languageId, store.Id);
            List<Donation> allDonations = new List<Donation>();
            foreach (OrderItem orderItem in order.OrderItems)
            {
                if (orderItem.Product.IsDonation)
                {
                    List<Donation> donations = _donationService.GetDonations(store.Id, orderItem.Id, false, null, null, null);

                    foreach (Donation donation in donations)
                    {
                        allDonations.Add(donation);
                    }
                }
            }


            if (allDonations.Count > 0)
            {

                var messageTemplate = this.GetActiveMessageTemplate("DonationOrderPlaced.CustomerNotification", store.Id);
                if (messageTemplate == null)
                {
                    return 0;
                }

                var emailAccount = this.GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

                //tokens
                var tokens = new List<Token>();
                _messageTokenProvider.AddDonationTokens(tokens, order, allDonations, languageId, true);
                _messageTokenProvider.AddOrderTokens(tokens, order, languageId, 0);
                _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);

                //event notification
                _eventPublisher.MessageTokensAdded(messageTemplate, tokens);



                string toEmail = order.BillingAddress.Email;
                string toName = string.Format("{0} {1}", order.BillingAddress.FirstName, order.BillingAddress.LastName);
                return this.SendNotification(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
            }
            return 0;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceSiteId"></param>
        /// <param name="order"></param>
        /// <param name="donation"></param>
        /// <param name="languageId"></param>
        /// <returns></returns>
        public virtual List<int> SendDonationOrderPlacedOnBehalfOfNotifications(Order order, int languageId)
        {
            List<int> returnIds = new List<int>();
            if (order == null)
            {
                throw new ArgumentNullException("order");
            }

            var store = _storeService.GetStoreById(order.StoreId) ?? _storeContext.CurrentStore;

            languageId = this.EnsureLanguageIsActive(languageId, store.Id);


            string templateName = "DonationOrderPlaced.OnBehalfOfNotification";

            List<OrderItem> donationOrderItems = new List<OrderItem>();

            foreach (OrderItem orderItem in order.OrderItems)
            {
                if (orderItem.Product.IsDonation)
                {
                    donationOrderItems.Add(orderItem);
                }

                //overridden in admin of product variant
                //if (product.SiteProduct.ProductVariant.OrderPlacedCustomerOnBehalfOfMessageTemplateName != null)
                //{
                //    if (product.SiteProduct.ProductVariant.OrderPlacedCustomerOnBehalfOfMessageTemplateName != "")
                //    {
                //        templateName = product.SiteProduct.ProductVariant.OrderPlacedCustomerOnBehalfOfMessageTemplateName;
                //    }
                // }
            }


            MessageTemplate messageTemplate = this.GetActiveMessageTemplate(templateName, store.Id);
            if (messageTemplate == null)
            {
                return new List<int>();
            }

            if (donationOrderItems.Count > 0)
            {


                foreach (OrderItem product in donationOrderItems)
                {

                    List<Donation> donations = _donationService.GetDonations(store.Id,
                                                                            product.Id, false,
                                                                            null, null, null);

                    if (donations[0].NotificationEmail != null)
                    {

                        bool sendEmail = donations[0].NotificationEmail == "" ? false : true;

                        if (sendEmail)
                        {

                            var emailAccount = this.GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

                            //tokens
                            var tokens = new List<Token>();
                            _messageTokenProvider.AddDonationTokens(tokens, order, donations, languageId, true);
                            _messageTokenProvider.AddOrderTokens(tokens, order, languageId, 0);
                            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);

                            //event notification
                            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

                            //          IEnumerable<Token> donationTokens = GenerateTokens(order, donations, languageId);
                            //          IEnumerable<Token> orderTokens = GenerateTokens(order, languageId);
                            //          IEnumerable<Token> tokens = donationTokens.Concat(orderTokens);
                            //          EmailAccount emailAccount = this.GetEmailAccountOfMessageTemplate(sourceSiteId,
                            //  messageTemplate);
                            string toEmail = donations[0].NotificationEmail;
                            string toName = string.Format("{0}", donations[0].OnBehalfOfFullName);

                            int id = this.SendNotification(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
                            returnIds.Add(id);
                        }
                    }
                }
            }

            return returnIds;

        }


        /// <summary>
        /// Sends an email to the email of the shipping address (aka recipient)
        /// </summary>
        /// <param name="sourceSiteId"></param>
        /// <param name="order"></param>
        /// <param name="languageId"></param>
        /// <returns></returns>
        public virtual int SendCustomerOrderGiftNotification(Order order, int languageId, bool includePrice = true)
        {
            if (order == null)
            {
                throw new ArgumentNullException("order");
            }
            var store = _storeService.GetStoreById(order.StoreId) ?? _storeContext.CurrentStore;
            MessageTemplate messageTemplate = null;

            if (!includePrice)
            {
                messageTemplate = this.GetActiveMessageTemplate("OrderPlaced.GiftNotificationNoPrice", store.Id);
            }
            else
            {
                messageTemplate = this.GetActiveMessageTemplate("OrderPlaced.GiftNotification", store.Id);
            }
            if (messageTemplate == null || order.ShippingAddress == null)
            {
                return 0;
            }

            //email account
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);

            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            _messageTokenProvider.AddOrderTokens(tokens, order, languageId, 0);
            _messageTokenProvider.AddCustomerTokens(tokens, order.Customer);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            Customer customer = order.Customer;
            string toEmail = order.ShippingAddress.Email;
            string toName = order.ShippingAddress.FirstName + " " + order.ShippingAddress.LastName;

            int id = this.SendNotification(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
            // return this.SendNotification(messageTemplate, emailAccount, languageId, orderTokens, toEmail, toName, false); TODO:  Implement delivery/pickup email notifications "false".
            return id;
        }



        #endregion

        #region NU-84
        public virtual int SendTestReceipt(int messageTemplateId, int languageId, string kitchenPrinterURL, string orderNumber)
        {
            var messageTemplate = _messageTemplateService.GetMessageTemplateById(messageTemplateId);
            if (messageTemplate == null)
                throw new ArgumentException("Template cannot be loaded");

            var order = _orderService.GetOrderById(int.Parse(orderNumber));

            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddOrderTokens(tokens, order, order.CustomerLanguageId);

            //warehouses
            List<int> warehouses = new List<int>();
            foreach (var orderItem in order.OrderItems)
            {
                if (orderItem.SelectedFulfillmentWarehouse.KitchenPrinterURL != null && orderItem.SelectedFulfillmentWarehouse.KitchenPrinterURL != "")
                    warehouses.Add(orderItem.SelectedFulfillmentWarehouseId.Value);
            }

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            int ret = 0;
            foreach (int warehouseID in warehouses.Distinct())
            {
                int status = SendReceipt(messageTemplate, languageId, tokens, warehouseID, order);
                if (status > 0) ret = status;

            }
            return ret;
        }

        public virtual int SendOrderReceipts(int languageId, Order order)
        {
            var messageTemplate = _messageTemplateService.GetMessageTemplateById(30);
            if (messageTemplate == null)
                throw new ArgumentException("Template cannot be loaded");

            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddOrderTokens(tokens, order, order.CustomerLanguageId);

            //warehouses
            List<int> warehouses = new List<int>();
            foreach (var orderItem in order.OrderItems)
            {
                if (orderItem.SelectedFulfillmentWarehouse.KitchenPrinterURL.Length > 0)
                    warehouses.Add(orderItem.SelectedFulfillmentWarehouseId.Value);
            }

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            int ret = 0;
            foreach (int warehouseID in warehouses.Distinct())
            {
                int status = SendReceipt(messageTemplate, languageId, tokens, warehouseID, order);
                if (status > 0) ret = status;

            }
            return ret;
        }

        public virtual int SendReceipt(MessageTemplate messageTemplate,
            int languageId, IList<Token> tokens, int warehouseID,
            Order order)
        {
            if (messageTemplate == null)
                throw new ArgumentNullException("messageTemplate");

            // warehouse tokens
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, order.CustomerLanguageId);
            _messageTokenProvider.AddWarehouseTokens(tokens, warehouseID, emailAccount);

            var body = messageTemplate.GetLocalized(mt => mt.Body, languageId);

            var bodyReplaced = _tokenizer.Replace(body, tokens, true);

            // <orderDetails />
            string orderDetails = "";
            foreach (OrderItem item in order.OrderItems.Where(i => i.SelectedFulfillmentWarehouseId == warehouseID))
            {
                orderDetails += "<text>(x" + item.Quantity + ") " + SecurityElement.Escape(item.Product.Name) + "</text>" +
                                "<text x=\"420\" />" +
                                "<text>" + item.PriceInclTax + "&#10;</text>" +
                                "<text>Requested for: " + item.RequestedFulfillmentDateTime + "&#10;</text>" +
                                "<text>Delivery method: " + item.SelectedShippingMethodName + "&#10;</text>";
            }
            orderDetails += "<text>--------------------------------------------------------&#10;</text>";
            bodyReplaced = bodyReplaced.Replace("<orderDetails />", orderDetails);

            // <comments/>
            string comments = "";
            if (order.CheckoutAttributesXml != null)
            {
                var checkoutAttributeCollection = (from nodes in XDocument.Parse(order.CheckoutAttributesXml).Descendants("CheckoutAttribute")
                                                   select nodes);
                foreach (var checkoutAttribute in checkoutAttributeCollection)
                {
                    if (checkoutAttribute.Attribute("ID").Value == "2")
                    {
                        comments = SecurityElement.Escape(checkoutAttribute.Value);
                    }
                }
            }
            bodyReplaced = bodyReplaced.Replace("<comments />", comments);

            string xmlDocument = "<s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\">" +
                        "<s:Body>" +
                            bodyReplaced +
                        "</s:Body>" +
                      "</s:Envelope>";

            XmlDocument xml = new XmlDocument();
            xml.LoadXml(xmlDocument);
            xml.Save(@"C:\workspace\KitchenPrinterXML" + order.Id + "#" + warehouseID + ".xml");

            /*
            using (WebClient wc = new WebClient())
            {
                Uri url = new Uri(kitchenPrinterURL);

                wc.Headers["Content-Type"] = "text/xml; charset=utf-8";

                wc.Headers["SOAPAction"] = "";

                // wc.UploadStringCompleted += new UploadStringCompletedEventHandler(SendCompleted);

                string ret = wc.UploadString(url, xmlDocument);
            }
            */
            return 0;
        }
        #endregion

        #endregion
    }
}
