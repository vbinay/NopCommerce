using Nop.Plugin.Payments.Bambora.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Payments.Bambora.Services
{
    public interface IBamboraStoreSettingService
    {
        BamboraStoreSetting GetByStore(int storeId, bool fallbackToDefault = true);
        void Insert(BamboraStoreSetting setting);
        void Update(BamboraStoreSetting setting);
        void Delete(BamboraStoreSetting setting);
    }
}
