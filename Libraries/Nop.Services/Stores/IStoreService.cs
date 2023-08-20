using System.Collections.Generic;
using Nop.Core.Domain.Stores;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Messages;

namespace Nop.Services.Stores
{
    /// <summary>
    /// Store service interface
    /// </summary>
    public partial interface IStoreService
    {
        /// <summary>
        /// Deletes a store
        /// </summary>
        /// <param name="store">Store</param>
        void DeleteStore(Store store);

        void DeleteTieredShippingbyId(StoreWiseTierShipping storeWiseTierShipping);

        /// <summary>
        /// Gets all stores
        /// </summary>
        /// <returns>Stores</returns>
        IList<Store> GetAllStores();

        /// <summary>
        /// Gets a store 
        /// </summary>
        /// <param name="storeId">Store identifier</param>
        /// <returns>Store</returns>
        Store GetStoreById(int storeId);

        /// <summary>
        /// Inserts a store
        /// </summary>
        /// <param name="store">Store</param>
        void InsertStore(Store store);

        /// <summary>
        /// Updates the store
        /// </summary>
        /// <param name="store">Store</param>
        void UpdateStore(Store store);

        IList<Store> GetAllStores(Customer _customer);	/// SODMYWAY-

        IList<StoreContact> GetAllStoreContacts(int storeId);   // NU-74
        #region SODMYWAY-

        StoreContact GetStoreContactById(int storeContactId);

        void UpdateStoreContact(StoreContact storeContact);

        void UpdateStoreTieredShippingbyId(StoreWiseTierShipping storeWiseTierShipping);

        void AddStoresContact(EmailType emailType);

        void DeleteStoresContacts(EmailType emailType);

        Store GetStoreByUrl(string url);

        bool IsStoreDuplicate(int id, string url);

        StoreContact GetStoreContactByEmailTypeId(int emailTypeId, int storeId);

        IList<StoreWiseTierShipping> GetTieredShipping(int storeId);

        StoreWiseTierShipping GetTieredShippingById(int Id);

        void CreateTieredShipping(StoreWiseTierShipping storeWiseTierShipping);
        #endregion
    }
}