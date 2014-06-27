using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Dqe.Web.Services;

namespace Dqe.Web.Messaging
{
    public class Dispatch : PersistentConnection
    {
        private static readonly ConcurrentDictionary<string, IList<ClientMessage>> Messages = new ConcurrentDictionary<string, IList<ClientMessage>>();

        public static void AddMessage(string clientId, ClientMessage value)
        {
            var clientMessages = Messages.GetOrAdd(clientId, new List<ClientMessage>());
            clientMessages.Add(value);
        }

        protected override Task OnConnected(IRequest request, string connectionId)
        {
            var task = base.OnConnected(request, connectionId);
            var cookie = request.Cookies[ContextService.ClientIdKey];
            if (cookie != null)
            {
                var clientId = cookie.Value;
                if (!string.IsNullOrEmpty(clientId))
                {
                    var clientMessages = Messages.GetOrAdd(clientId, new List<ClientMessage>());
                    foreach (var clientMessage in clientMessages)
                    {
                        Connection.Send(connectionId, clientMessage);
                    }
                    clientMessages.Clear();
                }
            }
            Connection.Send(connectionId, new ClientMessage { MessageHeader = "Disconnect" });
            return task;
        }
    }
}