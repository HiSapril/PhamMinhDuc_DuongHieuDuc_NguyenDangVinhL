namespace ASCWeb1.Areas.Configuration.Models
{
    public class MasterKeysViewModel
    {
        public List<MasterDataKeyViewModel> MasterKeys { get; set; } = new();
        public MasterDataKeyViewModel? MasterKeyInContext { get; set; }
        public bool IsEdit { get; set; }
    }
}
