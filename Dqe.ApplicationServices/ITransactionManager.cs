using System;

namespace Dqe.ApplicationServices
{
    /// <summary>
    /// COMPONENT
    /// </summary>
    public interface ITransactionManager
    {
        Guid Id { get; }
        void Abort();
        void Commit();
    }
}
