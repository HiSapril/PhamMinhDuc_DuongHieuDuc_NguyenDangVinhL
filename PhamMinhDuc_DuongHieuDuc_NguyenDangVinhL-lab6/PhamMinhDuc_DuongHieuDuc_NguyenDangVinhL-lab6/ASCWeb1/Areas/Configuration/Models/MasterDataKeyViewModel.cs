using System.ComponentModel.DataAnnotations;

namespace ASCWeb1.Areas.Configuration.Models
{
    public class MasterDataKeyViewModel
    {
        public string? RowKey { get; set; }
        public string? PartitionKey { get; set; }
        public bool IsActive { get; set; }
        [Required(ErrorMessage = "The Name field is required")]
        public string Name { get; set; } = string.Empty;
    }
}
