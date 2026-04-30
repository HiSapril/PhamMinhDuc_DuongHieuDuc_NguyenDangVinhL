namespace ASCWeb1.Areas.ServiceRequests.Models
{
    public class CreateServiceRequestMessageViewModel
    {
        public string PartitionKey { get; set; } = string.Empty;
        public string FromDisplayName { get; set; } = string.Empty;
        public string FromEmail { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
