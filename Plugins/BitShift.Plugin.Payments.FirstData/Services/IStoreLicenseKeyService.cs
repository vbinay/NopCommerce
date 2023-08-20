using System.Collections.Generic;
using BitShift.Plugin.Payments.FirstData.Domain;

namespace BitShift.Plugin.Payments.FirstData.Services
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
