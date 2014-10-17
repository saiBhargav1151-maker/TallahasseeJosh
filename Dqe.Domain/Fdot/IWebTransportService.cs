using System.Collections.Generic;
using Dqe.Domain.Model.Wt;

namespace Dqe.Domain.Fdot
{
    public interface IWebTransportService
    {
        IEnumerable<CodeTable> GetCodeTables();
        CodeTable GetCodeTable(string codeType);
        IEnumerable<RefItem> GetRefItems();
        IEnumerable<RefItem> GetRefItemsBySpecYear(int specYear);
        IEnumerable<Project> GetProjects(string number);
        Project GetProject(string number);
        IEnumerable<Project> GetProjectsByProposalId(long id);
        IEnumerable<Proposal> GetProposals(string number);
        Proposal GetProposal(string number);
        Project ExportProject(string projectNumber);
        Proposal ExportProposal(string proposalNumber);
        bool IsProjectSynced(Model.ProjectEstimate projectEstimate);
    }
}