using System.Collections.Generic;
using Dqe.Domain.Model;

namespace Dqe.Domain.Repositories.Custom
{
    public interface IProjectRepository
    {
        Project Get(int id);
        Project GetByProjectNumber(string number);
        Project GetBySnapshotId(int id);
        int GetMaxSnapshot(string number, int version);
        int GetMaxVersion(string number);
    }
}