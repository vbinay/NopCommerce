using Nop.Core.Configuration;

namespace Nop.Plugin.Tax.Vertex
{
    /// <summary>
    /// Represents settings for the Vertex tax provider 
    /// </summary>
    public class VertexTaxSettings : ISettings
    {
        /// <summary>
        /// Gets or sets Vertex account ID
        /// </summary>
        public string AccountId { get; set; }

        /// <summary>
        /// Gets or sets Vertex account license key
        /// </summary>
        public string LicenseKey { get; set; }

        /// <summary>
        /// Gets or sets company code
        /// </summary>
        public string CompanyCode { get; set; }

        /// <summary>
        /// Gets or sets division code
        /// </summary>
        public string Division { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use sandbox (testing environment)
        /// </summary>
        public bool IsSandboxEnvironment { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to commit tax transactions (recorded in the history on your Vertex account)
        /// </summary>
        public bool CommitTransactions { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to validate addresses before tax requesting (only for US or Canadian address)
        /// </summary>
        public bool ValidateAddresses { get; set; }
    }
}