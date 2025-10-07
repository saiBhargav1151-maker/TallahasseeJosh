using System;
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
        DateTime? GetProjectLetting(long wtId);
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
        bool IsProjectSynced(ProjectEstimate projectEstimate);
        IEnumerable<string> GetMasterFiles();
        object GetAllBidHistory(int range);
        object GetLsDbEstimateHistory();
        //object GetBidHistory(string number, int range);
        void ConvertPayItems();
        Project ExportProjectForInitialLoad(string projectNumber);
        string UpdateProjectPrices(ProjectEstimate p, DqeUser user, bool allPrices = false);
        string UpdatePrices(Model.Proposal p, bool isOfficialEstimate, DqeUser user);
        string UpdateFixedPrices(Model.Proposal p, DqeUser user);
        string UpdateFixedPrices(ProjectEstimate p, DqeUser user);
        void UpdateRefItem(PayItemMaster payItemMaster, DqeUser user);
        IEnumerable<Letting> GetLettingNames(string number);
        Letting GetLetting(string number);
        Proposal GetProposalAndProjectHeaders(string number);
        Letting GetLettingByProposal(string number);
        IList<Proposal> GetProposalsReadyForOfficialEstimate(string proposalNumber);
        Letting GetResponsiveLettings(string number);
        bool IsProposalReadyForOfficialEstimate(string proposalNumber);
        Exception InsertRefItems(IEnumerable<PayItemMaster> payItemMasters, DqeUser user);


        void UpdateProposalReadyForDssPass(Proposal proposal);
        // SB 05/30/2025 - Added interfaces for retrieving Unit Price and Pay Item details
        IList<ProposalItemDTO> GetUnitPriceDetails(string payItem, List<string> contractType, int months, List<string> contractWorkType, DateTime? startDate, DateTime? endDate, string[] counties, string bidStatus, string[] marketCounties, decimal? minRank, decimal? maxRank, List<string> workTypeNames, string projectNumber, decimal? minBidAmount, decimal? maxBidAmount, string[]  district);
        IList<PayItemDTO> GetPayItemDetails(string input);

        //not needed since DSS is decommissioned, dont need to pass to DSS
        //void UpdateProposalReadyForDssPass(Proposal proposal);

      
    }
}