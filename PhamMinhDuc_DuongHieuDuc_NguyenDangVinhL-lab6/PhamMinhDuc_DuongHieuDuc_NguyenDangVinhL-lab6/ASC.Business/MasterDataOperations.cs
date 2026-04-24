using ASC.Business.Interfaces;
using ASC.DataAccess.Interfaces;
using ASC.Model.Models;

namespace ASC.Business
{
    public class MasterDataOperations : IMasterDataOperations
    {
        private readonly IUnitOfWork _unitOfWork;

        public MasterDataOperations(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<MasterDataKey>> GetAllMasterKeysAsync()
        {
            var masterKeys = await _unitOfWork.Repository<MasterDataKey>().FindAllAsync();
            return masterKeys.ToList();
        }

        public async Task<List<MasterDataKey>> GetMasterKeyByNameAsync(string name)
        {
            var masterKeys = await _unitOfWork.Repository<MasterDataKey>().FindAllByPartitionKeyAsync(name);
            return masterKeys.ToList();
        }

        public async Task<bool> InsertMasterKeyAsync(MasterDataKey key)
        {
            await _unitOfWork.Repository<MasterDataKey>().AddAsync(key);
            _unitOfWork.CommitTransaction();
            return true;
        }

        public async Task<List<MasterDataValue>> GetAllMasterValuesByKeyAsync(string key)
        {
            try
            {
                var masterKeys = await _unitOfWork.Repository<MasterDataValue>().FindAllByPartitionKeyAsync(key);
                return masterKeys.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return new List<MasterDataValue>();
        }

        public async Task<MasterDataValue?> GetMasterValueByNameAsync(string key, string name)
        {
            var masterValues = await _unitOfWork.Repository<MasterDataValue>().FindAsync(key, name);
            return masterValues;
        }

        public async Task<bool> InsertMasterValueAsync(MasterDataValue value)
        {
            await _unitOfWork.Repository<MasterDataValue>().AddAsync(value);
            _unitOfWork.CommitTransaction();
            return true;
        }

        public async Task<List<MasterDataValue>> GetAllMasterValuesAsync()
        {
            var masterValues = await _unitOfWork.Repository<MasterDataValue>().FindAllAsync();
            return masterValues.ToList();
        }

        public async Task<bool> UpdateMasterKeyAsync(string originalPartitionKey, MasterDataKey key)
        {
            var masterKey = await _unitOfWork.Repository<MasterDataKey>().FindAsync(originalPartitionKey, key.RowKey);
            if (masterKey != null)
            {
                masterKey.IsActive = key.IsActive;
                masterKey.IsDeleted = key.IsDeleted;
                masterKey.Name = key.Name;
                _unitOfWork.Repository<MasterDataKey>().Update(masterKey);
                _unitOfWork.CommitTransaction();
            }
            return true;
        }

        public async Task<bool> UpdateMasterValueAsync(string originalPartitionKey, string originalRowKey, MasterDataValue value)
        {
            var masterValue = await _unitOfWork.Repository<MasterDataValue>().FindAsync(originalPartitionKey, originalRowKey);
            if (masterValue != null)
            {
                masterValue.IsActive = value.IsActive;
                masterValue.IsDeleted = value.IsDeleted;
                masterValue.Name = value.Name;
                _unitOfWork.Repository<MasterDataValue>().Update(masterValue);
                _unitOfWork.CommitTransaction();
            }
            return true;
        }

        public async Task<bool> UploadBulkMasterData(List<MasterDataValue> values)
        {
            foreach (var value in values)
            {
                // Find, if null insert MasterKey
                var masterKey = await GetMasterKeyByNameAsync(value.PartitionKey);
                if (!masterKey.Any())
                {
                    await _unitOfWork.Repository<MasterDataKey>().AddAsync(new MasterDataKey()
                    {
                        Name = value.PartitionKey,
                        RowKey = Guid.NewGuid().ToString(),
                        PartitionKey = value.PartitionKey,
                        CreatedBy = value.CreatedBy,
                        UpdatedBy = value.UpdatedBy
                    });
                }
                // Find, if null Insert MasterValue
                var masterValuesByKey = await GetAllMasterValuesByKeyAsync(value.PartitionKey);
                var masterValue = masterValuesByKey.FirstOrDefault(p => p.Name == value.Name);
                if (masterValue == null)
                {
                    await _unitOfWork.Repository<MasterDataValue>().AddAsync(value);
                }
                else
                {
                    masterValue.IsActive = value.IsActive;
                    masterValue.IsDeleted = value.IsDeleted;
                    masterValue.Name = value.Name;
                    _unitOfWork.Repository<MasterDataValue>().Update(masterValue);
                }
            }
            _unitOfWork.CommitTransaction();
            return true;
        }
    }
}
