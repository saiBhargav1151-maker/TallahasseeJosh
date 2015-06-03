using System.Collections.Generic;
using Dqe.Domain.Model.Reports;
using Dqe.Domain.Model.Wt;

namespace Dqe.Domain.Repositories.Custom
{
    public interface IReportRepository
    {
        IEnumerable<ReportProposal> GetReportProposals(string proposalNumber, ReportProposalLevel proposalLevel);

        ReportProposal GetReportProposal(string proposalNumber, ReportProposalLevel proposalLevel);

        ReportLetting GetReportLettingByProposalLevel(string lettingNumber, ReportProposalLevel proposalLevel);

        IEnumerable<ReportProposal> GetProposalsInList(List<string> proposals, ReportProposalLevel proposalLevel);

        void SaveReport(Letting letting);
    }
}
