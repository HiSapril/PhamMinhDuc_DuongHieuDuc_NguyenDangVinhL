using ASC.Model.Models;

namespace ASCWeb1.Areas.ServiceRequests.Models
{
    public class DashboardViewModel
    {
        public List<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();
        public List<ServiceRequest> RecentUpdates { get; set; } = new List<ServiceRequest>();
        public Dictionary<string, int> ActiveServiceRequests { get; set; } = new Dictionary<string, int>();
    }
}
