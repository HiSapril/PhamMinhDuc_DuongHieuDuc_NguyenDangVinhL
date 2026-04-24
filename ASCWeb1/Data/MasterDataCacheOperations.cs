using ASC.Business.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace ASCWeb1.Data
{
    public class MasterDataCacheOperations : IMasterDataCacheOperations
    {
        private readonly IDistributedCache _cache;
        private readonly IMasterDataOperations _masterData;
        private readonly string MasterDataCacheName = "MasterDataCache";

        public MasterDataCacheOperations(IDistributedCache cache, IMasterDataOperations masterData)
        {
            _cache = cache;
            _masterData = masterData;
        }

        public async Task CreateMasterDataCacheAsync()
        {
            var masterDataCache = new MasterDataCache
            {
                Keys = (await _masterData.GetAllMasterKeysAsync()).Where(p => p.IsActive == true).ToList(),
                Values = (await _masterData.GetAllMasterValuesAsync()).Where(p => p.IsActive == true).ToList()
            };
            await _cache.SetStringAsync(MasterDataCacheName, JsonConvert.SerializeObject(masterDataCache));
        }

        public async Task<MasterDataCache> GetMasterDataCacheAsync()
        {
            var data = await _cache.GetStringAsync(MasterDataCacheName);
            if (string.IsNullOrEmpty(data))
            {
                return new MasterDataCache();
            }
            return JsonConvert.DeserializeObject<MasterDataCache>(data) ?? new MasterDataCache();
        }
    }
}
