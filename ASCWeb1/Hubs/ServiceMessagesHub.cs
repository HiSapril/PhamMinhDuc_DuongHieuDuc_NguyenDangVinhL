using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace ASCWeb1.Hubs
{
    public class ServiceMessagesHub : Hub
    {
        public async Task SendMessage(string fromDisplayName, string fromEmail, string message, string serviceRequestId)
        {
            // Broadcast message to all clients in the service request group
            await Clients.Group(serviceRequestId).SendAsync("ReceiveMessage", fromDisplayName, fromEmail, message);
        }

        public async Task JoinGroup(string serviceRequestId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, serviceRequestId);
        }

        public async Task LeaveGroup(string serviceRequestId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, serviceRequestId);
        }
    }
}
