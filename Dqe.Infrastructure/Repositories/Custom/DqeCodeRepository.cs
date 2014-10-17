using System.Collections.Generic;
using Dqe.Domain.Model;
using Dqe.Domain.Repositories.Custom;
using NHibernate;

namespace Dqe.Infrastructure.Repositories.Custom
{
    public class DqeCodeRepository : BaseRepository, IDqeCodeRepository
    {
        public DqeCodeRepository() { }

        internal DqeCodeRepository(ISession session)
        {
            Session = session;
        }

        public DqeCode Get(int id)
        {
            InitializeSession();
            return Session.Get<DqeCode>(id);
        }

        public IEnumerable<CostGroup> GetCostGroups()
        {
            InitializeSession();
            return Session
                .QueryOver<CostGroup>()
                .OrderBy(i => i.Name).Asc
                .List();
        }

        public IEnumerable<CaddSummary> GetCaddSummaries()
        {
            InitializeSession();
            return Session
                .QueryOver<CaddSummary>()
                .OrderBy(i => i.Name).Asc
                .List();
        }
    }
}