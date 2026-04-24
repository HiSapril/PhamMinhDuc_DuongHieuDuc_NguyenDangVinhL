using ASC.Model.Models;

namespace ASCWeb1.Areas.ServiceRequests.Models
{
    public class DashboardViewModel
    {
        public List<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();
    }
}
