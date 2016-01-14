using Dqe.Infrastructure.Providers;
using NHibernate;

namespace Dqe.Infrastructure.Repositories.Custom
{
    public abstract class BaseRepository
    {
        protected ISession Session;

        protected void InitializeSession()
        {
            if (Session == null)
            {
                Session = UnitOfWorkProvider.Marshaler.CurrentSession;
            }
        }
    }
}