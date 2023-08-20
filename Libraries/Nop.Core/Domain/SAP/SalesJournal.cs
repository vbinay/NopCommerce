using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Core.Domain.SAP
{
    public partial class SalesJournal : BaseEntity
    {
        public int OrderNumber { get; set; }
        public int Status { get; set; }
        public int SAPDocumentNumber { get; set; }
        public int SAPCompanyCode { get; set; }
        public int SAPFiscalYear { get; set; }
        public DateTime SAPPostingDate { get; set; }
        public DateTime SAPEntryDate { get; set; }
        public string SAPDocumentType { get; set; }
        public string CostCenterNumber { get; set; }
        public DateTime DateOfTransaction { get; set; }
        public string ProductGL { get; set; }
        public int ProductAmount { get; set; }
        public int CustomerNumber { get; set; }
        public string CustomerName { get; set; }
        public string CustomerStreetAddress { get; set; }
        public string CustomerCity { get; set; }
        public string CustomerPostalCode { get; set; }
        public string CustomerCountry { get; set; }
        public string ShipToName { get; set; }
        public string ShipToStreetAddress { get; set; }
        public string ShipToCity { get; set; }
        public string ShipToPostalCode { get; set; }
        public string ShipToCountry { get; set; }
        public string ShipToJurisdiction { get; set; }
        public string ShipFromName { get; set; }
        public string ShipFromStreetAddress { get; set; }
        public string ShipFromCity { get; set; }
        public string ShipFromPostalCode { get; set; }
        public string ShipFromCountry { get; set; }
        public string MasterProduct { get; set; }
        public string LocalProduct { get; set; }
        public bool IsProductTaxable { get; set; }
        public decimal TaxAmountImposed { get; set; }
        public decimal NonTexableAmount { get; set; }
    }
}
