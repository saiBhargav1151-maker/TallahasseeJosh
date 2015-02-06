using System.Collections.Generic;
using Dqe.Domain.Model;

namespace Dqe.Domain.Repositories.Custom
{
    public interface ICostGroupRepository
    {
        IEnumerable<CostGroup> GetAll();
        CostGroup GetByName(string name);
    }
}