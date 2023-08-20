using System;

namespace Nop.Core.Domain.Stores
{
    public partial class StoreContact : BaseEntity
    {
        public int EmailTypeId { get; set; }

        public int StoreId { get; set; }

        public string Email { get; set; }

        public string DisplayName { get; set; }

        public bool Deleted { get; set; }
    }
}
