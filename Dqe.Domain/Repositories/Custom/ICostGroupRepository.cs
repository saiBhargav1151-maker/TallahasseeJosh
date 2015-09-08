using System.Collections.Generic;
using Dqe.Domain.Model;

namespace Dqe.Domain.Repositories.Custom
{
    public interface ICostGroupRepository
    {
        CostGroup Get(long id);
        IEnumerable<CostGroup> GetAll();
        IEnumerable<CostGroup> GetAllCostGroupsWithPayItems();
        CostGroup GetByName(string name);

        CostGroupPayItem GetCostGroupPayItem(int payItemId);
    }
}