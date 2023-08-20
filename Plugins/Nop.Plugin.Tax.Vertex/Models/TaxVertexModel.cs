using System.Collections.Generic;
using System.Web.Mvc;
using Nop.Web.Framework;

namespace Nop.Plugin.Tax.Vertex.Models
{
    public class TaxVertexModel
    {
        public TaxVertexModel()
        {
            TestAddress = new TaxVertexAddressModel();
        }

        public bool IsConfigured { get; set; }

        [NopResourceDisplayName("Plugins.Tax.Vertex.Fields.AccountId")]
        public string AccountId { get; set; }

        [NopResourceDisplayName("Plugins.Tax.Vertex.Fields.LicenseKey")]
        public string LicenseKey { get; set; }

        [NopResourceDisplayName("Plugins.Tax.Vertex.Fields.CompanyCode")]
        public string CompanyCode { get; set; }

        [NopResourceDisplayName("Plugins.Tax.Vertex.Fields.IsSandboxEnvironment")]
        public bool IsSandboxEnvironment { get; set; }

        [NopResourceDisplayName("Plugins.Tax.Vertex.Fields.CommitTransactions")]
        public bool CommitTransactions { get; set; }

        [NopResourceDisplayName("Plugins.Tax.Vertex.Fields.ValidateAddresses")]
        public bool ValidateAddresses { get; set; }

        public TaxVertexAddressModel TestAddress { get; set; }

        public string TestTaxResult { get; set; }
    }

    public class TaxVertexAddressModel
    {
        public TaxVertexAddressModel()
        {
            AvailableCountries = new List<SelectListItem>();
            AvailableStates = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.Address.Fields.Country")]
        public int CountryId { get; set; }
        public IList<SelectListItem> AvailableCountries { get; set; }

        [NopResourceDisplayName("Admin.Address.Fields.StateProvince")]
        public int RegionId { get; set; }
        public IList<SelectListItem> AvailableStates { get; set; }

        [NopResourceDisplayName("Admin.Address.Fields.City")]
        public string City { get; set; }

        [NopResourceDisplayName("Admin.Address.Fields.Address1")]
        public string Address { get; set; }

        [NopResourceDisplayName("Admin.Address.Fields.ZipPostalCode")]
        public string ZipPostalCode { get; set; }
    }
}