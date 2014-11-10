using System.Collections.Generic;
using Dqe.Domain.Model;

namespace Dqe.Domain.Repositories.Custom
{
    public interface IProposalRepository
    {
        IEnumerable<Proposal> GetByNumber(string number);
        Proposal GetWtByNumber(string number);
        ProjectItem GetProjectItemByWtId(long id, DqeUser custodyUser);
    }
}