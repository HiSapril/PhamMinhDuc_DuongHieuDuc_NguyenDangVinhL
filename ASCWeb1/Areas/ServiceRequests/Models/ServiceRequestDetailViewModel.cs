using ASC.Model.Models;
using System.Collections.Generic;

namespace ASCWeb1.Areas.ServiceRequests.Models
{
    public class ServiceRequestDetailViewModel
    {
        public UpdateServiceRequestViewModel ServiceRequest { get; set; } = new UpdateServiceRequestViewModel();
        public List<ServiceRequest> ServiceRequestAudit { get; set; } = new List<ServiceRequest>();
    }
}
