using System.Collections.Generic;
using Dqe.Domain.Model;
using Dqe.Domain.Model.Wt;
using Project = Dqe.Domain.Model.Wt.Project;
using Proposal = Dqe.Domain.Model.Wt.Proposal;

namespace Dqe.Domain.Fdot
{
    public interface IWebTransportService
    {
        IEnumerable<CodeTable> GetCodeTables();
        CodeTable GetCodeTable(string codeType);
        IEnumerable<RefItem> GetRefItems();
        IEnumerable<RefItem> GetRefItems(string number);
        IEnumerable<string> GetDistinctRefItemNumbers();
        IEnumerable<RefItem> GetRefItemsBySpecYear(int specYear);
        IEnumerable<Project> GetProjects(string number);
        Project GetProject(string number);
        IEnumerable<Project> GetProjectsByProposalId(long id);
        IEnumerable<Proposal> GetProposals(string number);
        Proposal GetProposal(string number);
        Project ExportProject(string projectNumber);
        Proposal ExportProposal(string proposalNumber);
        bool IsProjectSynced(Model.ProjectEstimate projectEstimate);
        IEnumerable<string> GetMasterFiles();
        object GetAllBidHistory(int range);
        //object GetBidHistory(string number, int range);
        void ConvertPayItems();
        Project ExportProjectForInitialLoad(string projectNumber);
        void UpdatePrices(Model.Proposal p, DqeUser user);
        IEnumerable<Letting> GetLettingNames(string number);
        Proposal GetProposalAndProjectHeaders(string number);
    }
}