using Nop.Core.Domain.Tridion;

namespace Nop.Services.Tridion
{
    /// <summary>
    /// The Interface for the Tridion Porfile Mapping Service SODMYWAY-2956
    /// </summary>
    public interface ITridionProfileMappingService
    {
        /// <summary>
        /// Creates new Tridion Profile Mapping
        /// </summary>
        /// <param name="identificationSource">Tridion Ecomm Id Source in the Appication Settings</param>
        /// <param name="identificationKey">The Unique Identifier for the Tridion Contact</param>
        /// <param name="customerId">The Nop Commerce Customer Id</param>
        /// <returns></returns>
        TridionProfileMapping Create(string identificationSource, string identificationKey, int customerId);
        /// <summary>
        /// This returns a matching Tridion Profile Mapping based on the info from tridion.
        /// </summary>
        /// <param name="identificationSource">Tridion Ecomm Id Source in the Appication Settings</param>
        /// <param name="identificationKey">The Unique Identifier for the Tridion Contact</param>
        /// <returns></returns>
        TridionProfileMapping GetByTridionProfile(string identificationSource, string identificationKey);
    }
}
