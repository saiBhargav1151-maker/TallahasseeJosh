namespace Dqe.ApplicationServices
{
    public interface IDeferredTaskHubContext
    {
        void SendMessage(string message);
    }
}