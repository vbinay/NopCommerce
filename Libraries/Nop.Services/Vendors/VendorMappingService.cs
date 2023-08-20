using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Vendors;
using Nop.Services.Events;

namespace Nop.Services.Vendors
{
    /// <summary>
    /// Vendor mapping service
    /// </summary>
    public partial class VendorMappingService : IVendorMappingService
    {
        #region Constants

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : entity ID
        /// {1} : entity name
        /// </remarks>
        private const string VENDORMAPPING_BY_ENTITYID_NAME_KEY = "Nop.vendormapping.entityid-name-{0}-{1}";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string VENDORMAPPING_PATTERN_KEY = "Nop.vendormapping.";

        #endregion

        #region Fields

        private readonly IRepository<VendorMapping> _vendorMappingRepository;
        private readonly IWorkContext _workContext;
        private readonly ICacheManager _cacheManager;
        private readonly IEventPublisher _eventPublisher;
        private readonly CatalogSettings _catalogSettings;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="cacheManager">Cache manager</param>
        /// <param name="vendorContext">Vendor context</param>
        /// <param name="vendorMappingRepository">Vendor mapping repository</param>
        /// <param name="catalogSettings">Catalog settings</param>
        /// <param name="eventPublisher">Event publisher</param>
        public VendorMappingService(ICacheManager cacheManager, 
            IWorkContext vendorContext,
            IRepository<VendorMapping> vendorMappingRepository,
            CatalogSettings catalogSettings,
            IEventPublisher eventPublisher)
        {
            this._cacheManager = cacheManager;
            this._workContext = vendorContext;
            this._vendorMappingRepository = vendorMappingRepository;
            this._catalogSettings = catalogSettings;
            this._eventPublisher = eventPublisher;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Deletes a vendor mapping record
        /// </summary>
        /// <param name="vendorMapping">Vendor mapping record</param>
        public virtual void DeleteVendorMapping(VendorMapping vendorMapping)
        {
            if (vendorMapping == null)
                throw new ArgumentNullException("vendorMapping");

            _vendorMappingRepository.Delete(vendorMapping);

            //cache
            _cacheManager.RemoveByPattern(VENDORMAPPING_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityDeleted(vendorMapping);
        }

        /// <summary>
        /// Gets a vendor mapping record
        /// </summary>
        /// <param name="vendorMappingId">Vendor mapping record identifier</param>
        /// <returns>Vendor mapping record</returns>
        public virtual VendorMapping GetVendorMappingById(int vendorMappingId)
        {
            if (vendorMappingId == 0)
                return null;

            return _vendorMappingRepository.GetById(vendorMappingId);
        }

        /// <summary>
        /// Gets vendor mapping records
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="entity">Entity</param>
        /// <returns>Vendor mapping records</returns>
        public virtual IList<VendorMapping> GetVendorMappings<T>(T entity) where T : BaseEntity
        {
            if (entity == null)
                throw new ArgumentNullException("entity");

            int entityId = entity.Id;
            string entityName = typeof(T).Name;

            var query = from sm in _vendorMappingRepository.Table
                        where sm.EntityId == entityId &&
                        sm.EntityName == entityName
                        select sm;
            var vendorMappings = query.ToList();
            return vendorMappings;
        }

        /// <summary>
        /// Gets vendor mapping records
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="entity">Entity</param>
		/// SODMYWAY-2945
        /// <returns>Vendor mapping records</returns>
        public virtual IList<VendorMapping> GetVendorMappingsByMasterId<T>(T entity) where T : BaseEntity
        {
            if (entity == null)
                throw new ArgumentNullException("entity");

            int entityId = entity.Id;
            string entityName = typeof(T).Name;

            var query = from sm in _vendorMappingRepository.Table
                        where sm.EntityId == entityId &&
                        sm.EntityName == entityName
                        select sm;
            var vendorMappings = query.ToList();
            return vendorMappings; 
        }

        /// <summary>
        /// Inserts a vendor mapping record
        /// </summary>
        /// <param name="vendorMapping">Vendor mapping</param>
        public virtual void InsertVendorMapping(VendorMapping vendorMapping)
        {
            if (vendorMapping == null)
                throw new ArgumentNullException("vendorMapping");

            _vendorMappingRepository.Insert(vendorMapping);

            //cache
            _cacheManager.RemoveByPattern(VENDORMAPPING_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityInserted(vendorMapping);
        }

        /// <summary>
        /// Inserts a vendor mapping record
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="vendorId">Vendor id</param>
        /// <param name="entity">Entity</param>
        public virtual void InsertVendorMapping<T>(T entity, int vendorId) where T : BaseEntity
        {
            if (entity == null)
                throw new ArgumentNullException("entity");

            if (vendorId == 0)
                throw new ArgumentOutOfRangeException("vendorId");

            int entityId = entity.Id;
            string entityName = typeof(T).Name;

            var vendorMapping = new VendorMapping
            {
                EntityId = entityId,
                EntityName = entityName,
                VendorId = vendorId
            };

            InsertVendorMapping(vendorMapping);
        }

        /// <summary>
        /// Updates the vendor mapping record
        /// </summary>
        /// <param name="vendorMapping">Vendor mapping</param>
        public virtual void UpdateVendorMapping(VendorMapping vendorMapping)
        {
            if (vendorMapping == null)
                throw new ArgumentNullException("vendorMapping");

            _vendorMappingRepository.Update(vendorMapping);

            //cache
            _cacheManager.RemoveByPattern(VENDORMAPPING_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityUpdated(vendorMapping);
        }

        /// <summary>
        /// Find vendor identifiers with granted access (mapped to the entity)
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="entity">Wntity</param>
        /// <returns>Vendor identifiers</returns>
        public virtual int[] GetVendorsIdsWithAccess<T>(T entity) where T : BaseEntity
        {
            if (entity == null)
                throw new ArgumentNullException("entity");

            int entityId = entity.Id;
            string entityName = typeof(T).Name;

            string key = string.Format(VENDORMAPPING_BY_ENTITYID_NAME_KEY, entityId, entityName);
            return _cacheManager.Get(key, () =>
            {
                var query = from sm in _vendorMappingRepository.Table
                            where sm.EntityId == entityId &&
                            sm.EntityName == entityName
                            select sm.VendorId;
                return query.ToArray();
            });
        }

        /// <summary>
        /// Authorize whether entity could be accessed in the current vendor (mapped to this vendor)
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="entity">Wntity</param>
        /// <returns>true - authorized; otherwise, false</returns>
        public virtual bool Authorize<T>(T entity) where T : BaseEntity
        {
            return Authorize(entity, _workContext.CurrentVendor.Id);
        }

        /// <summary>
        /// Authorize whether entity could be accessed in a vendor (mapped to this vendor)
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="entity">Entity</param>
        /// <param name="vendorId">Vendor identifier</param>
        /// <returns>true - authorized; otherwise, false</returns>
        public virtual bool Authorize<T>(T entity, int vendorId) where T : BaseEntity
        {
            if (entity == null)
                return false;

            if (vendorId == 0)
                //return true if no vendor specified/found
                return true;

            foreach (var vendorIdWithAccess in GetVendorsIdsWithAccess(entity))
                if (vendorId == vendorIdWithAccess)
                    //yes, we have such permission
                    return true;

            //no permission found
            return false;
        }

        #endregion
    }
}