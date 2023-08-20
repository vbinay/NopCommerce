using System;
using System.Collections.Generic;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Seo;
using Nop.Core.Domain.Stores;
using Nop.Core.Domain.Tax;

namespace Nop.Core.Domain.Catalog
{
    /// <summary>
    /// Represents a product
    /// </summary>
    public partial class ProductRelation : BaseEntity, ILocalizedEntity
    {
   
        /// <summary>
        /// 
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Product Product{get; set;}

        /// <summary>
        /// 
        /// </summary>
        public int RelatedProductId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Product RelatedProduct { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int RelationTypeId { get; set; }

        public int StoreId { get; set; }

        public bool IsActive { get; set; }

    }
}