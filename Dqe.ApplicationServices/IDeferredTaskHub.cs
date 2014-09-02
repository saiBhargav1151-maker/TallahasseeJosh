namespace Dqe.ApplicationServices
{
    public interface IDeferredTaskHubContext
    {
        void SendMessage(string task, string message);
    }
}