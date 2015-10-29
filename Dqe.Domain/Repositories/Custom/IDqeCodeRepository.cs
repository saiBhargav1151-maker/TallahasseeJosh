using System.Collections.Generic;
using Dqe.Domain.Model;

namespace Dqe.Domain.Repositories.Custom
{
    public interface IDqeCodeRepository
    {
        DqeCode Get(long id);
        //IEnumerable<CostGroup> GetCostGroups();
        IEnumerable<CaddSummary> GetCaddSummaries();
    }
}