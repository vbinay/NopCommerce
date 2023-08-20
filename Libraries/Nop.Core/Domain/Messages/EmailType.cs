
namespace Nop.Core.Domain.Messages
{
    public partial class EmailType : BaseEntity
    {
        public string Name { get; set; }

        public bool Deleted { get; set; }
    }
}
