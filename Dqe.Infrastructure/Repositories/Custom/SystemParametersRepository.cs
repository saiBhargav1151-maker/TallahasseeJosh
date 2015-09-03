using Dqe.Domain.Model;
using Dqe.Domain.Repositories.Custom;
using NHibernate;

namespace Dqe.Infrastructure.Repositories.Custom
{
    public class SystemParametersRepository : BaseRepository, ISystemParametersRepository
    {
        public SystemParametersRepository() { }

        internal SystemParametersRepository(ISession session)
        {
            Session = session;
        }

        public SystemParameters Get()
        {
            InitializeSession();
            return Session
                .QueryOver<SystemParameters>()
                .Where(i => i.Id == 1)
                .SingleOrDefault();
        }
    }
}