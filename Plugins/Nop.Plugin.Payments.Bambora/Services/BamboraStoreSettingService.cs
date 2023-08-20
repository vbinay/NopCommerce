using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Plugin.Payments.Bambora.Domain;
using Nop.Core.Data;

namespace Nop.Plugin.Payments.Bambora.Services
{
    public class BamboraStoreSettingService : IBamboraStoreSettingService
    {
        readonly IRepository<BamboraStoreSetting> _settingRepository;

        public BamboraStoreSettingService(IRepository<BamboraStoreSetting> settingRepository)
        {
            _settingRepository = settingRepository;
        }
        public BamboraStoreSetting GetByStore(int storeId, bool fallbackToDefault = true)
        {
            var setting = _settingRepository.Table.Where(s => s.StoreId == storeId).FirstOrDefault();
            if (setting == null && fallbackToDefault)
            {
                setting = _settingRepository.Table.Where(s => s.StoreId == 0).FirstOrDefault();
            }

            return setting;
        }
        public void Delete(BamboraStoreSetting setting)
        {
            _settingRepository.Delete(setting);
        }

       
        public void Insert(BamboraStoreSetting setting)
        {
            _settingRepository.Insert(setting);
        }

        public void Update(BamboraStoreSetting setting)
        {
            _settingRepository.Update(setting);
        }
    }
}
