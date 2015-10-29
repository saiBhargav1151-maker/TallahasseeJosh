using System.Collections.Generic;
using Dqe.Domain.Model.Reports;
using Dqe.Domain.Model.Wt;

namespace Dqe.Domain.Repositories.Custom
{
    public interface IReportRepository
    {
        IEnumerable<ReportProposal> GetReportProposals(string proposalNumber, ReportProposalLevel proposalLevel);

        ReportProposal GetReportProposal(string proposalNumber, ReportProposalLevel proposalLevel);

        ReportProposal GetReportProposalAndItems(string proposalNumber, ReportProposalLevel proposalLevel);

        ReportLetting GetReportLettingByProposalLevel(string lettingNumber, ReportProposalLevel proposalLevel);

        ReportProposal GetReportLettingByProposal(string proposalNumber, ReportProposalLevel proposalLevel);

        IEnumerable<ReportProposal> GetProposalsInList(List<string> proposals, ReportProposalLevel proposalLevel);

        void DeleteEmptyLettings();

        void DeleteLettingByName(string name);

        void DeleteLettingData(Letting letting, bool rebuildVendorBidData);

        void RebuildReportStructure(Letting letting, List<ReportProposal> officialProposals, List<ReportProposal> authorizedProposals, bool rebuildVendorBidData, List<Model.PayItemMaster> frontLoadedPayItems);
    }
}
