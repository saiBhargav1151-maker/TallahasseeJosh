using System.Collections.Generic;
using Dqe.Domain.Fdot;
using Dqe.Domain.Model;
using Dqe.Domain.Model.Reports;

namespace Dqe.Domain.Repositories.Custom
{
    public interface IProposalRepository
    {
        IEnumerable<Proposal> GetByNumber(string number);
        Proposal GetWtByNumber(string number);
        ProjectItem GetProjectItemByWtId(long id, DqeUser custodyUser);
        Proposal GetById(int id);
        void DeleteProposalStructure(int id);
        IEnumerable<ProjectItem> GetDqeProjectItemsForProposal(DqeUser custodyUser, string[] projects);

        IEnumerable<Proposal> GetProposalByEstimateTypeAndCategory(string proposalNumber, int estimateType);

        void BuildReportProposal(int proposalId, DqeUser owner, IPayItemMasterRepository payItemMasterRepository, IWebTransportService webTransportService);
    }
}