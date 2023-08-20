using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Nop.Plugin.Reports.ReportingServices
{
    public class ReportingServicesRequest
    {
        public string start { get; set; }

        public string end { get; set; }
    }

    public class ReportingServicesResult
    {
        public string UnitId { get; set; }

        public string UnitName { get; set; }

        public string UnitStatus { get; set; }

        public string MasterCatalogCategory { get; set; }

        public string MasterCatalogProductId { get; set; }

        public string MasterCatalogProductName { get; set; }

        public string UniversalPlu { get; set; }

        public string LocalProductName { get; set; }

        public string LocalProductDetailUrl { get; set; }

        public string LocalPrice { get; set; }

        public string TotalSalesExclTax { get; set; }

        public string TotalSalesInclTax { get; set; }

        public string QtySold { get; set; }

        public string Id { get; set; }

        public string IsPublished { get; set; }

        public string SMM { get; set; }
    }
}
