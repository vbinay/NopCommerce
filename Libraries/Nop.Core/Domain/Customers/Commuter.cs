using System.Collections.Generic;
using Nop.Core.Domain.Security;

namespace Nop.Core.Domain.Customers
{
    /// <summary>
    /// Represents a customer role
    /// </summary>
    public partial class Commuter : BaseEntity
    {
        /// <summary>
        /// 
        /// </summary>
        public string StudentEmplId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string StudentFirstName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string StudentLastName { get; set; }


        /// <summary>
        /// 
        /// </summary>
        public int StoreId { get; set; }
    }

}