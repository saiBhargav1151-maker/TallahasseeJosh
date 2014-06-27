namespace Dqe.Web.Messaging
{
    public class ClientMessage
    {
        public string MessageHeader { get; set; }

        public string MessageBody { get; set; }

        public bool PersistMessage { get; set; }
    }
}