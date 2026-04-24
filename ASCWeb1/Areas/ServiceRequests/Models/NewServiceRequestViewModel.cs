using System.ComponentModel.DataAnnotations;

namespace ASCWeb1.Areas.ServiceRequests.Models
{
    public class NewServiceRequestViewModel
    {
        [Required(ErrorMessage = "The {0} field is required.")]
        [Display(Name = "Vehicle Name")]
        public string VehicleName { get; set; } = null!;

        [Required(ErrorMessage = "The {0} field is required.")]
        [Display(Name = "Vehicle Type")]
        public string VehicleType { get; set; } = null!;

        [Required(ErrorMessage = "The {0} field is required.")]
        [Display(Name = "Requested Services")]
        public string RequestedServices { get; set; } = null!;

        [Required]
        [Display(Name = "Requested Date")]
        public DateTime? RequestedDate { get; set; }
    }
}
