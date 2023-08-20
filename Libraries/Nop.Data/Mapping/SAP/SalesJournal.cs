using Nop.Core.Domain.SAP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Data.Mapping.SAP
{
    public partial class SalesJournalMap : NopEntityTypeConfiguration<SalesJournal>
    {
        public SalesJournalMap()
        {
            this.ToTable("SAP_SalesJournal");
            this.HasKey(x => x.Id);
        }
    }
}
