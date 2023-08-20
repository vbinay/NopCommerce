using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PreSalesNotifier
{
    public partial class PreSalesDataModel
    {
        public int productId { get; set; }
        public string productName { get; set; }

        public bool isPublished { get; set; }

        public bool isDeleted { get; set; }

        public bool IsMaster { get; set; }
        public DateTime PreorderAvaibilityStartDate { get; set; }
        public bool isPreorder { get; set; }

        public string StoreName { get; set; }
        public int storeid { get; set; }

        
        
    }
}
