namespace Dqe.Domain.Messaging
{
    /// <summary>
    /// COMPONENT TEMPLATE
    /// </summary>
    public interface IMessage
    {
        string Id { get; }
        bool IsCancelable { get; }
    }
}