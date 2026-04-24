namespace ASCWeb1.Data
{
    public interface IMasterDataCacheOperations
    {
        Task<MasterDataCache> GetMasterDataCacheAsync();
        Task CreateMasterDataCacheAsync();
    }
}
