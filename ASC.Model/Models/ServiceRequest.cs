using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASC.Model.Models
{
    public class ServiceRequest: BaseEntity, IAuditTracker
    {
        public ServiceRequest() { }

        public ServiceRequest(string email)
        {
            this.RowKey = Guid.NewGuid().ToString();
            this.PartitionKey = email;
            //this.VehicleName = string.Empty;
            //this.VehicleType = string.Empty;
            //this.Status = string.Empty;
            //this.RequestedServices = string.Empty;
            //this.ServiceEngineer = string.Empty;
            //this.CreatedBy = string.Empty;
            //this.UpdatedBy = string.Empty;
        }

        public string VehicleName { get; set; }
        public string VehicleType { get; set; }
        public string Status { get; set; }
        public string RequestedServices { get; set; }
        public DateTime? RequestedDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public string ? ServiceEngineer { get; set; }
    }
}
