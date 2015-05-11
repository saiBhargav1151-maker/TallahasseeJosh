using System.ComponentModel.DataAnnotations;

namespace Dqe.Domain.Messaging
{
    /// <summary>
    /// COMPONENT
    /// </summary>
    public interface IMessenger
    {
        void Notify(IMessage message);
        void Notify(object o, ValidationContext validationContext);
    }
}