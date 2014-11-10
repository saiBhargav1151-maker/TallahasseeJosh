using System.Collections.Generic;
using Dqe.Domain.Model;

namespace Dqe.Domain.Repositories.Custom
{
    public interface IProjectRepository
    {
        Project Get(int id);
        Project GetByProjectNumber(string number);
        Project GetByEstimateId(int id);
        Project GetByVersionId(int id);
        int GetMaxEstimate(string number, int version);
        int GetMaxVersion(string number);
    }
}