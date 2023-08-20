using Nop.Core.Domain.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Services.SAP
{
    public partial interface ISAPService
    {
        void CreateEntry(OrderItem _orderItem, int _status);

        void ExportSalesJournal();

        void UploadFiles();

        string Test();

        string Count();
    }
}
