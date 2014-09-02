using System.Web;
using Dqe.ApplicationServices;
using Microsoft.AspNet.SignalR;

namespace Dqe.Web.Hubs
{
    public class DeferredTaskHubContext : IDeferredTaskHubContext
    {
        public void SendMessage(string task, string message)
        {
            GlobalHost.ConnectionManager.GetHubContext<DeferredTaskHub>().Clients.All.showMessage(task, message);
        }
    }
}