namespace Dqe.Domain.Repositories
{
    /// <summary>
    /// COMPONENT
    /// </summary>
    public interface ICommandRepository
    {
        void Add(object o);
        void Remove(object o);
        void Refresh(object o);
        void Clear();
    }
}