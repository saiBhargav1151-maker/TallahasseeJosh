using System.Collections.Generic;
using Dqe.Domain.Model;

namespace Dqe.Domain.Repositories.Custom
{
    public interface IProjectRepository
    {
        Project Get(long id);
        Project GetByProjectNumber(string number);
        Project GetByEstimateId(long id);
        Project GetByVersionId(long id);
        int GetMaxEstimate(string number, int version);
        int GetMaxVersion(string number);
        Project GetDetailProjectForLsBd(string number, DqeUser owner);
        ProjectEstimate GetEstimate(long id);
        IEnumerable<Project> GetByProposalId(long id);
        IEnumerable<Project> GetProjects(DqeUser dqeUser);
        ProjectItem GetProjectItem(long id);
        void Delete(long id);
    }
}