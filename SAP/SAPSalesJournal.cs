using System;
using System.Collections.Generic;

namespace FileFeed.SAP
{
    /// <summary>
    /// Represents a gift card
    /// </summary>
	public partial class SAPSalesJournal
    {

        public int OrderNumber { get; set; }

        public int OrderItemId { get; set; }
        public int Status { get; set; }
        

        public DateTime DateOfTransaction { get; set; }

        public string CostCenterNumber { get; set; }

        public string ProductGL { get; set; }
                         
        public string ProductAmount { get; set; }

        public string AmountSign { get; set; }
        
    }
}
