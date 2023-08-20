using System.Collections.Generic;
using Nop.Core;
using Nop.Core.Domain.Vendors;

namespace Nop.Services.Vendors
{
    /// <summary>
    /// Vendor mapping service interface
    /// </summary>
    public partial interface IVendorMappingService
    {
        /// <summary>
        /// Deletes a vendor mapping record
        /// </summary>
        /// <param name="vendorMapping">Vendor mapping record</param>
        void DeleteVendorMapping(VendorMapping vendorMapping);

        /// <summary>
        /// Gets a vendor mapping record
        /// </summary>
        /// <param name="vendorMappingId">Vendor mapping record identifier</param>
        /// <returns>Vendor mapping record</returns>
        VendorMapping GetVendorMappingById(int vendorMappingId);

        /// <summary>
        /// Gets vendor mapping records
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="entity">Entity</param>
        /// <returns>Vendor mapping records</returns>
        IList<VendorMapping> GetVendorMappings<T>(T entity) where T : BaseEntity;

        /// <summary>
        /// Inserts a vendor mapping record
        /// </summary>
        /// <param name="vendorMapping">Vendor mapping</param>
        void InsertVendorMapping(VendorMapping vendorMapping);

        /// <summary>
        /// Inserts a vendor mapping record
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="vendorId">Vendor id</param>
        /// <param name="entity">Entity</param>
        void InsertVendorMapping<T>(T entity, int vendorId) where T : BaseEntity;

        /// <summary>
        /// Updates the vendor mapping record
        /// </summary>
        /// <param name="vendorMapping">Vendor mapping</param>
        void UpdateVendorMapping(VendorMapping vendorMapping);

        /// <summary>
        /// Find vendor identifiers with granted access (mapped to the entity)
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="entity">Wntity</param>
        /// <returns>Vendor identifiers</returns>
        int[] GetVendorsIdsWithAccess<T>(T entity) where T : BaseEntity;

        /// <summary>
        /// Authorize whether entity could be accessed in the current vendor (mapped to this vendor)
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="entity">Wntity</param>
        /// <returns>true - authorized; otherwise, false</returns>
        bool Authorize<T>(T entity) where T : BaseEntity;

        /// <summary>
        /// Authorize whether entity could be accessed in a vendor (mapped to this vendor)
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="entity">Entity</param>
        /// <param name="vendorId">Vendor identifier</param>
        /// <returns>true - authorized; otherwise, false</returns>
        bool Authorize<T>(T entity, int vendorId) where T : BaseEntity;
    }
}