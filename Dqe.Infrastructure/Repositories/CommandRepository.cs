using NHibernate.Session;
using Dqe.Domain.Repositories;

namespace Dqe.Infrastructure.Repositories
{
    /// <summary>
    /// COMPONENT
    /// </summary>
    public class CommandRepository : ICommandRepository
    {
        private readonly Marshaler _marshaler;

        internal CommandRepository(Marshaler marshaler)
        {
            _marshaler = marshaler;
        }

        public void Add(object o)
        {
            _marshaler.CurrentSession.SaveOrUpdate(o);
        }

        public void Remove(object o)
        {
            _marshaler.CurrentSession.Delete(o);
        }

        public void Refresh(object o)
        {
            _marshaler.CurrentSession.Refresh(o);
        }

        public void Clear()
        {
            _marshaler.CurrentSession.Clear();
        }
    }
}