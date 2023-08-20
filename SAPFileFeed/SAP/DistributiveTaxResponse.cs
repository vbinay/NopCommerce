using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPFileFeed.SAP
{
    public class DistributiveTaxResponse
    {
        public int OrderNumber { get; set; }

        public string ProductID { get; set; }

        public List<string> LineItems { get; set; }
        public List<string> CalculatedTaxAmounts { get; set; }

        public int OrderItemID { get; set; }

        public int Status { get; set; }

        public decimal InvoiceSubtotal { get; set; }

        public decimal ProductTax { get; set; }
        public decimal InvoiceTotal { get; set; }

        public decimal InvoiceTax { get; set; }

        public decimal calculatedTax { get; set; }

        public string[] LineItem { get; set; }

        public int[] LineItemQuantity { get; set; }

        public decimal[] ExtendedPrice { get; set; }

        public decimal[] InputTotaltax { get; set; }

        public string[] TaxResult { get; set; }

        public string[] Jurisdictionlevel { get; set; }

        public string[] Jurisdiction { get; set; }

        public decimal[] CalculatedTax { get; set; }

        public decimal[] EffectiveRate { get; set; }

        public decimal[] JurisdictionTaxableAmt { get; set; }

        public int[] ImpositionTypeID { get; set; }

        public decimal[] ImpositionName { get; set; }

        public int VertexCompanyCode { get; set; }

        public int VertexFiscalYear { get; set; }

        public DateTime VertexPostingDate { get; set; }

        public DateTime VertexEntryDate { get; set; }

        public string VertexDocumentType { get; set; }

        public int VertexDocumentNumber { get; set; }

        public string CostCenterNumber { get; set; }

        public DateTime DateOfTransaction { get; set; }

        public string ProductGL { get; set; }

        public decimal ProductAmount { get; set; }

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

        public string ShipFromState { get; set; }

        public string ShipFromCountry { get; set; }

        public string MasterProduct { get; set; }

        public string LocalProduct { get; set; }

        public bool IsProductTaxable { get; set; }

        public decimal TaxAmountImposed { get; set; }
        
        public decimal DeliveryTax { get; set; }

        public decimal ShippingTax { get; set; }

        public decimal NonTaxableAmount { get; set; }

        public decimal OrderAmount { get; set; }

        public bool IsExported { get; set; }



    }
}
