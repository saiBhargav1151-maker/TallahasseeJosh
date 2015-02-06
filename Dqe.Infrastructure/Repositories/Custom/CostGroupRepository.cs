using System.Collections.Generic;
using Dqe.Domain.Model;
using Dqe.Domain.Repositories.Custom;
using NHibernate;

namespace Dqe.Infrastructure.Repositories.Custom
{
    public class CostGroupRepository : BaseRepository, ICostGroupRepository
    {
        public CostGroupRepository() { }

        internal CostGroupRepository(ISession session)
        {
            Session = session;
        }

        public IEnumerable<CostGroup> GetAll()
        {
            InitializeSession();
            return Session
                .QueryOver<CostGroup>()
                .OrderBy(i => i.Description).Asc
                .List();
        }

        public CostGroup GetByName(string name)
        {
            InitializeSession();
            return Session
                .QueryOver<CostGroup>()
                .Where(i => i.Name == name)
                .SingleOrDefault();
        }
    }
}