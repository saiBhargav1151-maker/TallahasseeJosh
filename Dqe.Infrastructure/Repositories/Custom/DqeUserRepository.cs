using System.Collections.Generic;
using Dqe.Domain.Model;
using Dqe.Domain.Repositories.Custom;
using Dqe.Infrastructure.Providers;

namespace Dqe.Infrastructure.Repositories.Custom
{
    public class DqeUserRepository : IDqeUserRepository
    {
        public IEnumerable<DqeUser> GetAll()
        {
            return UnitOfWorkProvider
                .Marshaler
                .CurrentSession
                .QueryOver<DqeUser>()
                .Where(i => i.IsActive)
                .List();
        }

        public DqeUser Get(int id)
        {
            return UnitOfWorkProvider
                .Marshaler
                .CurrentSession
                .Get<DqeUser>(id);
        }

        public DqeUser GetBySrsId(int id)
        {
            return UnitOfWorkProvider
                .Marshaler
                .CurrentSession
                .QueryOver<DqeUser>()
                .Where(i => i.SrsId == id)
                .Where(i => i.IsActive)
                .SingleOrDefault();
        }
    }
}