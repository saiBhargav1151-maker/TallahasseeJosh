using Microsoft.AspNet.SignalR;

namespace Dqe.Web.Hubs
{
    public class DeferredTaskHub : Hub
    {
        public void SendMessage(string task, string message)
        {
            Clients.All.showMessage(task, message);
        }
    }
}