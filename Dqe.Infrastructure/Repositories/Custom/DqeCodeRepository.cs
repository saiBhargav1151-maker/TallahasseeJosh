using System.Collections.Generic;
using Dqe.Domain.Model;
using Dqe.Domain.Repositories.Custom;
using Dqe.Infrastructure.Providers;

namespace Dqe.Infrastructure.Repositories.Custom
{
    public class DqeCodeRepository : IDqeCodeRepository
    {
        public DqeCode Get(int id)
        {
            return UnitOfWorkProvider
                .Marshaler
                .CurrentSession
                .Get<DqeCode>(id);
        }

        public IEnumerable<CostGroup> GetCostGroups()
        {
            return UnitOfWorkProvider
                .Marshaler
                .CurrentSession
                .QueryOver<CostGroup>()
                .OrderBy(i => i.Name).Asc
                .List();
        }

        public IEnumerable<CaddSummary> GetCaddSummaries()
        {
            return UnitOfWorkProvider
                .Marshaler
                .CurrentSession
                .QueryOver<CaddSummary>()
                .OrderBy(i => i.Name).Asc
                .List();
        }
    }
}