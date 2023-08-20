using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Core.Domain.Staging
{
    public partial class StagingProducts : BaseEntity
    {

        public int ProductId { get; set; }
        public int? TaxCategoryId { get; set; }
        public int? GlCode1Id { get; set; }
        public decimal? GlCode1Amount { get; set; }
        public int? GlCode2Id { get; set; }
        public decimal? GLCode2Amount { get; set; }
        public int? GlCode3Id { get; set; }
        public decimal? GLCode3Amount { get; set; }
        public string Comments { get; set; }
        public bool? Completed { get; set; }
        public int? MasterProductId { get; set; }

    }
}
