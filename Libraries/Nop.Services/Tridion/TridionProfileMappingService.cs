using System.Linq;
using Nop.Core.Data;
using Nop.Core.Domain.Tridion;

namespace Nop.Services.Tridion
{
    /// <summary>
    /// The Implementation for the Tridion Porfile Mapping Service SODMYWAY-2956
    /// </summary>
    public class TridionProfileMappingService : ITridionProfileMappingService
    {
        #region Fields

        private readonly IRepository<TridionProfileMapping> _tridionProfileMappingRepository;

        #endregion

        #region Ctor
        /// <summary>
        /// Service Constructor
        /// </summary>
        /// <param name="tridionProfileMappingRepository">The repository connection that will be used to connect to DB</param>
        public TridionProfileMappingService(IRepository<TridionProfileMapping> tridionProfileMappingRepository)
        {
            _tridionProfileMappingRepository = tridionProfileMappingRepository;
        }

        #endregion

        #region Methods
        /// <summary>
        /// Creates new Tridion Profile Mapping
        /// </summary>
        /// <param name="identificationSource">Tridion Ecomm Id Source in the Appication Settings</param>
        /// <param name="identificationKey">The Unique Identifier for the Tridion Contact</param>
        /// <param name="customerId">The Nop Commerce Customer Id</param>
        /// <returns></returns>
        public virtual TridionProfileMapping Create(string identificationSource, string identificationKey, int customerId)
        {
            var mapping = new TridionProfileMapping
            {
                TridionIdentificationSource = identificationSource,
                TridionIdentificationKey = identificationKey,
                CustomerId = customerId
            };

            _tridionProfileMappingRepository.Insert(mapping);

            return mapping;
        }
        /// <summary>
        /// This returns a matching Tridion Profile Mapping based on the info from tridion.
        /// </summary>
        /// <param name="identificationSource">Tridion Ecomm Id Source in the Appication Settings</param>
        /// <param name="identificationKey">The Unique Identifier for the Tridion Contact</param>
        /// <returns></returns>
        public virtual TridionProfileMapping GetByTridionProfile(string identificationSource, string identificationKey)
        {
            // Find it in the database
            return _tridionProfileMappingRepository.Table.FirstOrDefault(t => (t.TridionIdentificationSource == identificationSource && t.TridionIdentificationKey == identificationKey));
        }

        #endregion
    }
}
