using System.Threading.Tasks;

namespace project_manage_system_backend.Hubs
{
    public interface INotifyHub
    {
        Task ReceiveNotification();

        Task SubscribeNotification();

        Task UnsubscribeNotification();
    }
}
