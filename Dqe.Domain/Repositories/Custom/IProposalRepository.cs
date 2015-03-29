using System.Collections.Generic;
using Dqe.Domain.Fdot;
using Dqe.Domain.Model;

namespace Dqe.Domain.Repositories.Custom
{
    public interface IProposalRepository
    {
        IEnumerable<Proposal> GetByNumber(string number);
        Proposal GetWtByNumber(string number);
        ProjectItem GetProjectItemByWtId(long id, DqeUser custodyUser);
        Proposal GetById(long id);
        void DeleteProposalStructure(long id);
        IEnumerable<ProjectItem> GetDqeProjectItemsForProposal(DqeUser custodyUser, string[] projects);

        IEnumerable<Proposal> GetProposalByEstimateTypeAndCategory(string proposalNumber, int estimateType);

        void BuildReportProposal(long proposalId, DqeUser owner, IPayItemMasterRepository payItemMasterRepository, IReportRepository reportRepository, IWebTransportService webTransportService);
    }
}