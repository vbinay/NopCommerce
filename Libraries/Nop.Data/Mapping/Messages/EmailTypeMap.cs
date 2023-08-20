using Nop.Core.Domain.Messages;

namespace Nop.Data.Mapping.Messages
{
    public partial class EmailTypeMap : NopEntityTypeConfiguration<EmailType>
    {
        public EmailTypeMap()
        {
            this.ToTable("EmailType");
            this.HasKey(et => et.Id);

            this.Property(et => et.Name).IsRequired();
        }
    }
}