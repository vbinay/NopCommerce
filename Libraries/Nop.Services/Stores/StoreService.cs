using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.Core.Domain.Stores;
using Nop.Services.Events;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Messages;

namespace Nop.Services.Stores
{
    /// <summary>
    /// Store service
    /// </summary>
    public partial class StoreService : IStoreService
    {
        #region Constants

        /// <summary>
        /// Key for caching
        /// </summary>
        private const string STORES_ALL_KEY = "Nop.stores.all";
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : store ID
        /// </remarks>
        private const string STORES_BY_ID_KEY = "Nop.stores.id-{0}";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string STORES_PATTERN_KEY = "Nop.stores.";

        #endregion
        
        #region Fields
        
        private readonly IRepository<Store> _storeRepository;
        private readonly IRepository<StoreWiseTierShipping> _tieredShippingRepository;
        private readonly IEventPublisher _eventPublisher;
        private readonly ICacheManager _cacheManager;
        private readonly IRepository<StoreMapping> _storeMappingRepository;
        private readonly IRepository<StoreContact> _storeContactRepository;
        private readonly IRepository<EmailType> _emailTypeRepository;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="cacheManager">Cache manager</param>
        /// <param name="storeRepository">Store repository</param>
        /// <param name="eventPublisher">Event published</param>
        public StoreService(ICacheManager cacheManager,
            IRepository<Store> storeRepository,
            IRepository<StoreWiseTierShipping> tieredShippingRepository,
            IEventPublisher eventPublisher,
            IRepository<StoreMapping> storeMappingRepository,	
            IRepository<StoreContact> storeContactRepository,	
            IRepository<EmailType> emailTypeRepository)	
        {
            this._cacheManager = cacheManager;
            this._storeRepository = storeRepository;
            this._eventPublisher = eventPublisher;
            this._storeMappingRepository = storeMappingRepository;	
            this._storeContactRepository = storeContactRepository;
            this._emailTypeRepository = emailTypeRepository;
            this._tieredShippingRepository = tieredShippingRepository;
    }

        #endregion

        #region Methods

        /// <summary>
        /// Deletes a store
        /// </summary>
        /// <param name="store">Store</param>
        public virtual void DeleteStore(Store store)
        {
            if (store == null)
                throw new ArgumentNullException("store");

            var allStores = GetAllStores();
            if (allStores.Count == 1)
                throw new Exception("You cannot delete the only configured store");

            _storeRepository.Delete(store);

            _cacheManager.RemoveByPattern(STORES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityDeleted(store);
        }
        
        
        /// <summary>
        /// Gets all stores
        /// </summary>
        /// <returns>Stores</returns>
        public virtual IList<Store> GetAllStores()
        {
            string key = STORES_ALL_KEY;
            return _cacheManager.Get(key, () =>
            {
                var query = from s in _storeRepository.Table
                            orderby s.Name
                            select s;
                var stores = query.ToList();
                return stores;
            });
        }

        /// <summary>
        /// Gets a store 
        /// </summary>
        /// <param name="storeId">Store identifier</param>
        /// <returns>Store</returns>
        public virtual Store GetStoreById(int storeId)
        {
            if (storeId == 0)
                return null;
            
            string key = string.Format(STORES_BY_ID_KEY, storeId);
            return _cacheManager.Get(key, () => _storeRepository.GetById(storeId));
        }

        /// <summary>
        /// Gets a store 
        /// </summary>
        /// <param name="storeId">Store identifier</param>
        /// <returns>Store</returns>
        public virtual IList<StoreWiseTierShipping> GetTieredShipping(int storeId)
        {
            if (storeId == 0)
                return null;

            var query = from s in _tieredShippingRepository.Table
                        orderby s.MinPrice
                        select s;
            var tieredShipping = query.ToList();
            return tieredShipping;
        }

        public virtual void CreateTieredShipping(StoreWiseTierShipping storeWiseTierShipping)
        {
            if (storeWiseTierShipping == null )
                throw new ArgumentNullException("tieredShippingRepository");

            _tieredShippingRepository.Insert(storeWiseTierShipping);

            //event notification
            _eventPublisher.EntityInserted(storeWiseTierShipping);
        }

        /// <summary>
        /// Inserts a store
        /// </summary>
        /// <param name="store">Store</param>
        public virtual void InsertStore(Store store)
        {
            if (store == null)
                throw new ArgumentNullException("store");

            _storeRepository.Insert(store);

            #region SODMYWAY-
            var query = from s in _storeRepository.Table
                        where s.Name == store.Name
                        && s.Url == store.Url
                        && s.SecureUrl == store.SecureUrl
                        select s;

            AddStoreContact(query.First());
            #endregion

            _cacheManager.RemoveByPattern(STORES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityInserted(store);
        }

        /// <summary>
        /// Updates the store
        /// </summary>
        /// <param name="store">Store</param>
        public virtual void UpdateStore(Store store)
        {
            if (store == null)
                throw new ArgumentNullException("store");

            _storeRepository.Update(store);

            _cacheManager.RemoveByPattern(STORES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityUpdated(store);
        }

        #region NU-75
        public virtual IList<Store> GetAllStores(Customer _customer)
        {
            if (!_customer.IsSystemGlobalAdmin())
            {
                string key = STORES_PATTERN_KEY;
                return _cacheManager.Get(key, () =>
                {
                    var query = from s in _storeRepository.Table
                                join sm in _storeMappingRepository.Table on s.Id equals sm.StoreId
                                where sm.EntityName == "Customer"
                                    && sm.EntityId == _customer.Id
                                orderby s.Name
                                select s;
                    var stores = query.ToList();
                    return stores;
                });
            }
            else
            {
                return  GetAllStores();
            }
        }
		#endregion

        #region NU-74
        public virtual IList<StoreContact> GetAllStoreContacts(int storeId)
        {
            if (storeId == 0)
                return new List<StoreContact>();

            var query = from sc in _storeContactRepository.Table
                        where sc.StoreId == storeId && sc.Deleted == false
                        select sc;

            return query.ToList();
        }

        public virtual StoreContact GetStoreContactById(int storeContactId)
        {
            if (storeContactId == 0)
                return null;

            var query = from sc in _storeContactRepository.Table
                        where sc.Id == storeContactId
                        select sc;

            return query.First();
        }

        public virtual StoreContact GetStoreContactByEmailTypeId(int emailTypeId, int storeId)
        {
            var query = from sc in _storeContactRepository.Table
                        where sc.EmailTypeId == emailTypeId && sc.StoreId == storeId
                        select sc;

            return query.First();
        }

        
        public virtual StoreWiseTierShipping GetTieredShippingById(int tieredShippingId)
        {
            if (tieredShippingId == 0)
                return null;

            var query = from sc in _tieredShippingRepository.Table
                        where sc.Id == tieredShippingId
                        select sc;

            return query.First();
        }

        public virtual void  UpdateStoreTieredShippingbyId(StoreWiseTierShipping storeWiseTierShipping)
        {
            if (storeWiseTierShipping == null)
                throw new ArgumentNullException("storeWiseTierShipping");

            StoreWiseTierShipping tierShipping = GetTieredShippingById(storeWiseTierShipping.Id);
            if (tierShipping == null)
                throw new ArgumentException("No tier shipping found with the specified id");

            if(tierShipping.Id == storeWiseTierShipping.Id)
            {
                tierShipping.MinPrice = storeWiseTierShipping.MinPrice;
                tierShipping.MaxPrice = storeWiseTierShipping.MaxPrice;
                tierShipping.ShippingAmount = storeWiseTierShipping.ShippingAmount;

                _tieredShippingRepository.Update(tierShipping);
                _eventPublisher.EntityUpdated(tierShipping);
            }
        }

        public virtual void DeleteTieredShippingbyId(StoreWiseTierShipping storeWiseTierShipping)
        {
            if (storeWiseTierShipping == null)
                throw new ArgumentNullException("storeWiseTierShipping");

            StoreWiseTierShipping tierShipping = GetTieredShippingById(storeWiseTierShipping.Id);
            if (tierShipping == null)
                throw new ArgumentException("No tier shipping found with the specified id");

            if (tierShipping.Id == storeWiseTierShipping.Id)
            {
                _tieredShippingRepository.Delete(tierShipping);
                _eventPublisher.EntityDeleted(tierShipping);
            }
        }

        public virtual void UpdateStoreContact(StoreContact storeContact)
        {
            if (storeContact == null)
                throw new ArgumentNullException("storecontact");

            _storeContactRepository.Update(storeContact);

            _eventPublisher.EntityUpdated(storeContact);
        }

        public virtual void AddStoreContact(Store store)
        {
            var query = from et in _emailTypeRepository.Table
                        where et.Deleted == false
                        select et;

            foreach(var emailType in query.ToList())
            {
                StoreContact storeContact = new StoreContact();

                storeContact.EmailTypeId = emailType.Id;
                storeContact.Email = "";
                storeContact.DisplayName = emailType.Name;
                storeContact.StoreId = store.Id;

                _storeContactRepository.Insert(storeContact);
            }
        }

        public virtual void AddStoresContact(EmailType emailType)
        {
            foreach(var store in GetAllStores())
            {
                StoreContact storeContact = new StoreContact();

                storeContact.EmailTypeId = emailType.Id;
                storeContact.Email = "";
                storeContact.DisplayName = emailType.Name;
                storeContact.StoreId = store.Id;

                _storeContactRepository.Insert(storeContact);
            }
        }

        public virtual void DeleteStoresContacts(EmailType emailType)
        {
            var query = from sc in _storeContactRepository.Table
                        where sc.EmailTypeId == emailType.Id
                        select sc;

            foreach (var contact in query.ToList())
            {
                contact.DisplayName = contact.DisplayName + " - DELETED";
                contact.Deleted = true;

                _storeContactRepository.Update(contact);
            }
        }

        public virtual Store GetStoreByUrl(string url)
        {
            var query = from s in _storeRepository.Table
                        where s.Url.Replace("/", "") == url.Replace("/", "")
                        select s;

            Store ret = null;

            if (query.Count() > 0)
                ret = query.First();
            
            return ret;
        }

        public virtual bool IsStoreDuplicate(int id, string url)
        {
            var query = from s in _storeRepository.Table
                        where s.Url.Replace("/", "") == url.Replace("/", "") 
                        && s.Id != id
                        select s;

            return (query.Count() > 0);
        }

        #endregion

        #endregion
    }
}