namespace ASCWeb1.Areas.Configuration.Models
{
    public class MasterValuesViewModel
    {
        public List<MasterDataValueViewModel> MasterValues { get; set; } = new();
        public MasterDataValueViewModel? MasterValueInContext { get; set; }
        public bool IsEdit { get; set; }
    }
}
