using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASC.Model.Models
{
    public class ServiceRequestMessage : BaseEntity
    {
        public ServiceRequestMessage()
        {
        }

        public ServiceRequestMessage(string serviceRequestId)
        {
            this.RowKey = Guid.NewGuid().ToString();
            this.PartitionKey = serviceRequestId;
        }

        public string? FromDisplayName { get; set; }
        public string? FromEmail { get; set; }
        public string? Message { get; set; }
        public DateTime MessageDate { get; set; }
    }
}
