using Microsoft.Owin;
using Owin;
using Dqe.Web.Messaging;

[assembly: OwinStartup(typeof(Dqe.Web.Startup))]

namespace Dqe.Web
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR<Dispatch>("/dispatch");
            // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=316888
        }
    }
}
