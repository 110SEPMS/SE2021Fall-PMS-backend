using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace project_manage_system_backend.Hubs
{
    public class NotifyHub : Hub<INotifyHub>
    {
        public async Task SubscribeNotification(string userAccount)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, userAccount);
        }

        public async Task UnsubscribeNotification(string userAccount)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, userAccount);
        }
    }
}
