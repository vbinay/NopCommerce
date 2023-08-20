using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPFileFeed
{
    public class ReportHelper
    {
        public ReportHelper(int productId, string name, string description, decimal total)
        {
            this.productId = productId;
            this.Name = name;
            this.Total = total;
            this.Description = description;
        }
        public String Description { get; set; }
        public string Name
        {
            get;
            set;
        }

        public decimal Total { get; set; }

        public int productId { get; set; }
    }
}
