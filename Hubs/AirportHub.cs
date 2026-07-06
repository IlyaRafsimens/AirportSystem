using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace AirportSystem.Hubs
{
    public class AirportHub : Hub
    {
        public async Task SendUpdate() => await Clients.All.SendAsync("ReceiveUpdate");

        // Новое: Отправка пуш-уведомления всем
        public async Task SendAlert(string title, string message)
        {
            await Clients.All.SendAsync("ReceiveNotification", title, message);
        }
    }
}