using System.Data.Entity.ModelConfiguration.Configuration;
using Nop.Core.Domain.Tridion;

namespace Nop.Data.Mapping.Tridion
{
    /// <summary>
    /// Class for Mapping the Tridion Profile Mapping Object from the DB to the application SODMYWAY-2956
    /// </summary>
    public partial class TridionProfileMappingMap : NopEntityTypeConfiguration<TridionProfileMapping>
    {
        public ForeignKeyAssociationMappingConfiguration ForeignKeyAssociationMappingConfiguration { get; set; }

        public TridionProfileMappingMap()
        {
            this.ToTable("TridionProfileMapping");
            this.HasKey(x => x.Id);
            this.Property(x => x.TridionIdentificationSource).IsRequired();
            this.Property(x => x.TridionIdentificationKey).IsRequired();
            this.Property(x => x.CustomerId).IsRequired();
        }
    }
}
