using Nop.Core.Domain.Catalog;
using System.Data.Entity.ModelConfiguration;


namespace Nop.Data.Mapping.Orders
{
    public partial class CardAccessRecordMap : NopEntityTypeConfiguration<CardAccessRecord>
    {
        public CardAccessRecordMap()
        {
            this.ToTable("CardAccessRecord");
            this.HasKey(mp => mp.Id);                 
        }
    }
}