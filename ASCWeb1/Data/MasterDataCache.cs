using ASC.Model.Models;

namespace ASCWeb1.Data
{
    public class MasterDataCache
    {
        public List<MasterDataKey> Keys { get; set; } = new List<MasterDataKey>();
        public List<MasterDataValue> Values { get; set; } = new List<MasterDataValue>();
    }
}