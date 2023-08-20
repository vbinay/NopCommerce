using Nop.Plugin.Payments.Bambora.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Payments.Bambora.Services
{
    public interface IStoreLicenseKeyService
    {
        StoreLicenseKey GetById(int id);
        IList<StoreLicenseKey> GetAll();
        void Delete(StoreLicenseKey key);
        void Update(StoreLicenseKey key);
        void Insert(StoreLicenseKey key);

        string GetLicenseType(string encodedLicense);
        string GetLicenseHost(string encodedLicense);
        bool IsLicensed(string host);
    }
}
