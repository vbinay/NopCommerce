using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;

namespace Nop.Admin.Models.Catalog
{
    public partial class ProductRelationModel : BaseNopEntityModel
    {
        /// <summary>
        /// 
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int RelatedProductId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string RelatedProductName { get; set; }

        /// <summary>s
        /// 
        /// </summary>
        public int RelationTypeId { get; set; }

        public string RelationTypeName { 
            get
            {
                switch(RelationTypeId)
                {
                    case 1:                        
                        return "Can Be Sold With";                               
                    case 2:
                        return "Cannot Be Sold With";

                    default:
                        return "Not Sure";

                }
            }
         }

        public int StoreId { get; set; }

        public bool IsActive { get; set; }
    }


    public enum RelationTypeEnum
    {
        CanBeSoldWith = 1,
        CannotBeSoldWith = 2,      
    }
}