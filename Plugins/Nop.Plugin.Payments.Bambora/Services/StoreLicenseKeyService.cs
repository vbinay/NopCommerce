using Nop.Core.Data;
using Nop.Plugin.Payments.Bambora.Domain;
using Nop.Services.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Payments.Bambora.Services
{
    public class StoreLicenseKeyService : IStoreLicenseKeyService
    {
        private IRepository<StoreLicenseKey> _storeLicenseKeyRepository;
        private IEncryptionService _encryptionService;

        public StoreLicenseKeyService(IRepository<StoreLicenseKey> storeLicenseKeyRepository, IEncryptionService encryptionService)
        {
            _storeLicenseKeyRepository = storeLicenseKeyRepository;
            _encryptionService = encryptionService;
        }

        public StoreLicenseKey GetById(int id)
        {
            return _storeLicenseKeyRepository.GetById(id);
        }

        public IList<StoreLicenseKey> GetAll()
        {
            return _storeLicenseKeyRepository.Table.ToList();
        }

        public void Delete(StoreLicenseKey key)
        {
            if (key == null)
                throw new ArgumentNullException("storeLicenseKey");

            _storeLicenseKeyRepository.Delete(key);
        }

        public void Update(StoreLicenseKey key)
        {
            if (key == null)
                throw new ArgumentNullException("storeLicenseKey");

            _storeLicenseKeyRepository.Update(key);
        }

        public void Insert(StoreLicenseKey key)
        {
            if (key == null)
                throw new ArgumentNullException("storeLicenseKey");

            _storeLicenseKeyRepository.Insert(key);
        }

        public string GetLicenseType(string encodedLicense)
        {
            string result = "";
            try
            {
                //var decryptedText = _encryptionService.DecryptText(encodedLicense, Constants.LicenseKeySeed);
                var decryptedText = "nasasdhaksghdh87897";
                if (decryptedText.Length > 0)
                {
                    switch (decryptedText[0])
                    {
                        case 'U':
                            result = "URL";
                            break;
                        case 'D':
                            result = "Domain";
                            break;
                        default:
                            result = "Invalid";
                            break;
                    }
                }
                else
                {
                    result = "Invalid";
                }
            }
            catch
            {
                result = "Invalid";
            }

            return result;
        }

        public string GetLicenseHost(string encodedLicense)
        {
            string result = "";
            try
            {
                //var decryptedText = _encryptionService.DecryptText(encodedLicense, Constants.LicenseKeySeed);
                var decryptedText = "nasasdhaksghdh87897";
                if (decryptedText.Length > 1)
                {
                    result = decryptedText.Substring(1);
                }
            }
            catch
            {
                result = "Error decrypting license key";
            }

            return result;
        }

        public bool IsLicensed(string host)
        {
            var keys = GetAll();

            foreach (var key in keys)
            {
                try
                {
                    host = host.ToLower();
                    //string decryptedKey = _encryptionService.DecryptText(key.LicenseKey, Constants.LicenseKeySeed);
                    string decryptedKey = "hkhdakshdkjhaksdhjk";
                    string licensedHost = decryptedKey.Substring(1).ToLower();
                    if (decryptedKey[0] == 'U')
                    {
                        if (host == licensedHost || host == "www." + licensedHost)
                            return true;
                    }
                    else if (decryptedKey[0] == 'D')
                    {
                        if (host.EndsWith(licensedHost))
                            return true;
                    }
                }
                catch { }
            }

            return false;
        }
    }
}
