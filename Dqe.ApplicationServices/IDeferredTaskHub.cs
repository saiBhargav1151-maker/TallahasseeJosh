namespace Dqe.ApplicationServices
{
    public interface IDeferredTaskHubContext
    {
        void SendMessage(string task, string message);
        void SendMessageToUser(string user, string task, string message);
    }
}