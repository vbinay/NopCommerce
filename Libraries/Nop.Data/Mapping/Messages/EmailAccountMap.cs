using Nop.Core.Domain.Messages;

namespace Nop.Data.Mapping.Messages
{
    public partial class EmailAccountMap : NopEntityTypeConfiguration<EmailAccount>
    {
        public EmailAccountMap()
        {
            this.ToTable("EmailAccount");
            this.HasKey(ea => ea.Id);
            this.Ignore(ea => ea.DisplayName);
            this.Ignore(ea => ea.Email);
            this.Property(ea => ea.Host).IsRequired().HasMaxLength(255);
            this.Property(ea => ea.Username).IsRequired().HasMaxLength(255);
            this.Property(ea => ea.Password).IsRequired().HasMaxLength(255);

        }
    }
}