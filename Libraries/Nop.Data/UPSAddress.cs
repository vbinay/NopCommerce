using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Data
{
    public class UPSAddress
    {
        public string AddressLine { get; set; }
        public string City { get; set; }
        public string StateProvinceCode { get; set; }
        public string PostalCode { get; set; }

        public string StateID { get; set; }
    }
}
