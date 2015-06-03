using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dqe.Domain.Model.Reports;
using Dqe.Domain.Model.Wt;
using Dqe.Domain.Repositories.Custom;
using NHibernate;
using NHibernate.Criterion;

namespace Dqe.Infrastructure.Repositories.Custom
{
    public class ReportRepository : BaseRepository, IReportRepository
    {
        public ReportRepository() { }

        public ReportRepository(ISession session)
        {
            Session = session;
        }

        public IEnumerable<ReportProposal> GetReportProposals(string proposalNumber, ReportProposalLevel proposalLevel)
        {
            InitializeSession();

            return Session
                .QueryOver<ReportProposal>()
                .WhereRestrictionOn(i => i.ProposalNumber).IsLike(proposalNumber, MatchMode.Start)
                .Where(i => i.ProposalLevel == proposalLevel)
                .Where(i => !i.InvalidEstimate)
                .List();
        }

        public ReportProposal GetReportProposal(string proposalNumber, ReportProposalLevel proposalLevel)
        {
            InitializeSession();

            return Session
                .QueryOver<ReportProposal>()
                .Where(i => i.ProposalNumber == proposalNumber)
                .Where(i => i.ProposalLevel == proposalLevel)
                .Where(i => !i.InvalidEstimate)
                .Left.JoinQueryOver(i => i.ReportProposalItems)
                .SingleOrDefault();

        }

        public ReportLetting GetReportLettingByProposalLevel(string lettingNumber, ReportProposalLevel proposalLevel)
        {
            InitializeSession();

            ReportProposal reportProposal = null;

            return Session
                .QueryOver<ReportLetting>()
                .Where(i => i.LettingName == lettingNumber)
                .Fetch(i => i.ReportProposals).Eager
                .Left.JoinQueryOver(i => i.ReportProposals, () => reportProposal)
                .Where(i => i.ProposalLevel == proposalLevel)
                .Where(i => !i.InvalidEstimate)
                .SingleOrDefault();

        }

        public ReportProposal GetReportLettingByProposal(string proposalNumber, ReportProposalLevel proposalLevel)
        {
            InitializeSession();

            ReportProposal proposal = null;

            return Session
                .QueryOver(() => proposal)
                .Where(() => proposal.ProposalNumber == proposalNumber)
                .Where(() => proposal.ProposalLevel == proposalLevel)
                .Where(i => !i.InvalidEstimate)
                .Left.JoinQueryOver(() => proposal.MyReportLetting)
                .SingleOrDefault();
        }

        public IEnumerable<ReportProposal> GetProposalsInList(List<string> proposals, ReportProposalLevel proposalLevel)
        {
            InitializeSession();

            ReportProposal reportProposal = null;

            return Session
                .QueryOver(() => reportProposal)
                .Where(i => i.ProposalNumber.IsIn(proposals))
                .Where(i => i.ProposalLevel == proposalLevel)
                .Where(i => !i.InvalidEstimate)
                .List()
                .Distinct();
        }

        public void DeleteReportLettingData(ReportLetting reportLetting)
        {
            using (var s = Initializer.SessionFactory.OpenSession())
            {
                using (var t = s.BeginTransaction())
                {
                    try
                    {
                        var queryDeleteReportVendorBidSb = new StringBuilder();
                        queryDeleteReportVendorBidSb.Append(" Delete ReportVendorBid Where exists ( ");
                        queryDeleteReportVendorBidSb.Append("   Select 1 From ReportVendorBid As rvb Join rvb.MyReportProposalVendor As rpv Where exists ( ");
                        queryDeleteReportVendorBidSb.Append("	   Select 1 From ReportProposalVendor As rpv Join rpv.MyReportProposal As rp Where rp.Id = :proposalId )) ");

                        var queryDeleteReportProposalVendorSb = new StringBuilder();
                        queryDeleteReportProposalVendorSb.Append(" Delete ReportProposalVendor Where exists ( ");
                        queryDeleteReportProposalVendorSb.Append("  Select 1 From ReportProposalVendor As rpv Join rpv.MyReportProposal As rp Where rp.Id = :proposalId ) ");

                        var queryDeleteReportLettingSummarySb = new StringBuilder();
                        queryDeleteReportLettingSummarySb.Append(" Delete ReportLettingSummary Where exists ( ");
                        queryDeleteReportLettingSummarySb.Append("  Select 1 From ReportLettingSummary As rls Join rls.MyReportLetting As rl Where rl.Id = :lettingId ) ");

                        var queryUpdateReportProposalItemSb = new StringBuilder();
                        queryUpdateReportProposalItemSb.Append(" Update	ReportProposalItem Set AverageBidPrice = 0 Where exists ( ");
                        queryUpdateReportProposalItemSb.Append("  Select 1 From ReportProposalItem As rpi Join rpi.MyReportProposal As rp Where rp.Id = :proposalId ) ");

                        var queryDeleteVendorBid = s.CreateQuery(queryDeleteReportVendorBidSb.ToString());
                        var queryDeleteProposalVendor = s.CreateQuery(queryDeleteReportProposalVendorSb.ToString());
                        var queryDeleteReportLettingSummary = s.CreateQuery(queryDeleteReportLettingSummarySb.ToString());
                        var queryUpdateReportProposalItem = s.CreateQuery(queryUpdateReportProposalItemSb.ToString());


                        var records = queryDeleteVendorBid
                            .SetParameter("proposalId", reportLetting.ReportProposals.First().Id)
                            .ExecuteUpdate();

                        records = queryDeleteProposalVendor
                            .SetParameter("proposalId", reportLetting.ReportProposals.First().Id)
                            .ExecuteUpdate();

                        records = queryDeleteReportLettingSummary
                            .SetParameter("lettingId", reportLetting.Id)
                            .ExecuteUpdate();

                        records = queryUpdateReportProposalItem
                            .SetParameter("proposalId", reportLetting.ReportProposals.First().Id)
                            .ExecuteUpdate();

                        t.Commit();
                    }
                    catch
                    {
                        t.Rollback();
                        throw;
                    }
                }
            }
        }

        public void SaveReport(Letting letting)
        {
            InitializeSession();

            var reportLetting = GetReportLettingByProposalLevel(letting.LettingName, ReportProposalLevel.Official);

            //Must check to see if report structure needs to be rebuilt first
            if (RebuildVendorBidAndLettingSummary(letting, reportLetting))
            {
                if (reportLetting != null)
                {
                    DeleteReportLettingData(reportLetting);
                    Session.Evict(reportLetting);    
                }
                reportLetting = GetReportLettingByProposalLevel(letting.LettingName, ReportProposalLevel.Official);
            }
            else
            {
                //If the report structure did not need to be rebuilt just check if its already there.
                if (reportLetting.ReportProposals.Any(proposal => proposal.ReportProposalVendors.Any()))
                    return;
            }

            //Create report for first time or we are recreating the report at this point.
            if (reportLetting == null)
            {
                reportLetting = new ReportLetting
                {
                    LettingName = letting.LettingName,
                    LettingDate = letting.LettingDate
                };
                Session.SaveOrUpdate(reportLetting);
                Session.Flush();

                foreach (var proposal in letting.Proposals)
                {
                    var reportProposal = GetReportProposal(proposal.ProposalNumber, ReportProposalLevel.Official);
                    if (reportProposal != null)
                        reportLetting.AddReportProposal(reportProposal);
                }
            }

            foreach (var proposal in reportLetting.ReportProposals)
            {
                //TODO do I have the report Projects here?
                foreach (var project in proposal.ReportProjects)
                {
                    if (project.LettingDate == null || project.LettingDate != reportLetting.LettingDate)
                        project.LettingDate = reportLetting.LettingDate;
                }

                var wtLettingProposal = letting.Proposals.First(i => i.ProposalNumber == proposal.ProposalNumber);

                if (wtLettingProposal.ProposalVendors.Any())
                {
                    foreach (var vendor in wtLettingProposal.ProposalVendors)
                    {
                        CreateVendorData(vendor, proposal);
                    }
                    Session.Flush();
                    DetermineUnbalancedItems(proposal);
                    Session.Flush();
                    DetermineMagnitude(proposal);
                    Session.Flush();
                }
            }

            GenerateLettingSummary(reportLetting);
            Session.SaveOrUpdate(reportLetting);
            Session.Flush();
        }

        private void CreateVendorData(ProposalVendor vendor, ReportProposal proposal)
        {
            if (!vendor.Bids.Any()) return;

            var reportProposalVendor = new ReportProposalVendor
            {
                Name = vendor.MyRefVendor.VendorName,
                Awarded = vendor.Awarded,
                BidStatus = vendor.BidStatus ?? string.Empty,
                BidType = vendor.BidType ?? string.Empty,
                Total = vendor.BidTotal ?? 0
            };

            proposal.AddReportProposalVendor(reportProposalVendor);

            Session.Flush();

            foreach (var bid in vendor.Bids)
            {
                var myProposalItem = proposal.ReportProposalItems.First(i => i.LineNumber == bid.MyProposalItem.LineNumber);

                if (myProposalItem != null)
                {
                    var reportVendorBid = new ReportVendorBid
                    {
                        BidPrice =
                            bid.BidPrice != null
                                ? myProposalItem.Unit.StartsWith("LS")
                                    ? (decimal) bid.BidPrice/myProposalItem.Quantity
                                    : (decimal) bid.BidPrice
                                : 0,
                        LowCost = bid.LowCost
                    };
                    reportProposalVendor.AddReportVendorBid(reportVendorBid);
                    myProposalItem.AddReportVendorBid(reportVendorBid);
                    Session.Flush();
                }
            }
            
        }

        private static void DetermineUnbalancedItems(ReportProposal proposal)
        {
            foreach (var proposalItem in proposal.ReportProposalItems)
            {
                var unitPriceBidAverage = (proposalItem.Price + proposalItem.ReportVendorBids.Sum(i => i.BidPrice)) /
                                          (proposalItem.ReportVendorBids.Count() + 1);

                var reportVendorBid = new ReportVendorBid { BidPrice = proposalItem.Price, Tolerance = "ESTIMATE" };

                AverageItemPriceByBid(proposalItem, unitPriceBidAverage, reportVendorBid);

                var unbalanced = proposalItem.ReportVendorBids
                    .Where(i => i.BidPrice < proposalItem.AverageBidPrice * (decimal).4)
                    .ToList();
                var aboveTolerance = proposalItem.ReportVendorBids
                    .Where(i => i.BidPrice > proposalItem.AverageBidPrice * (decimal)1.6)
                    .ToList();

                foreach (var item in unbalanced)
                    item.Tolerance = "U";

                foreach (var item in aboveTolerance)
                    item.Tolerance = "A";
            }
        }

        private static void AverageItemPriceByBid(ReportProposalItem reportProposalItem, decimal unitPriceBidAverage, ReportVendorBid reportVendorBid)
        {
            var l = reportProposalItem.ReportVendorBids.ToList();
            l.Add(reportVendorBid);

            var firstCheck = l
                .Where(i => i.BidPrice > unitPriceBidAverage * (decimal).55)
                .Where(i => i.BidPrice < unitPriceBidAverage * (decimal)1.45)
                .ToList();

            if (firstCheck.Any())
            {
                reportProposalItem.AverageBidPrice = firstCheck.Sum(i => i.BidPrice) / firstCheck.Count;
                return;
            }

            var secondCheck = l
                .Where(i => i.BidPrice > unitPriceBidAverage * (decimal).1)
                .Where(i => i.BidPrice < unitPriceBidAverage * (decimal)1.9)
                .ToList();

            if (secondCheck.Any())
            {
                reportProposalItem.AverageBidPrice = secondCheck.Sum(i => i.BidPrice) / secondCheck.Count;
                return;
            }

            var thirdCheck = l
                .Where(i => i.BidPrice > unitPriceBidAverage * 0)
                .Where(i => i.BidPrice < unitPriceBidAverage * 2)
                .ToList();

            if (thirdCheck.Any())
                reportProposalItem.AverageBidPrice = thirdCheck.Sum(i => i.BidPrice) / thirdCheck.Count;
        }

        private static void DetermineMagnitude(ReportProposal reportProposal)
        {
            foreach (var proposalItem in reportProposal.ReportProposalItems)
            {
                proposalItem.Magnitude = Math.Abs(proposalItem.Total - proposalItem.ReportVendorBids.Min(i => i.BidPrice));
            }
        }

        private static void GenerateLettingSummary(ReportLetting reportLetting)
        {
            var contractRanges = ContractRange();

            for (var j = 0; j <= 5; j++)
            {
                var reportLettingSummary = new ReportLettingSummary();
                var proposals = reportLetting.ReportProposals
                    .Where(i => i.Total + i.ReportProposalMilestones.Sum(m => m.Total) >= contractRanges[j, 0] &&
                                i.Total + i.ReportProposalMilestones.Sum(m => m.Total) < contractRanges[j, 1])
                    .ToList();

                reportLettingSummary.ContractRange = j < 5
                    ? string.Format("${0} - ${1}", string.Format("{0:#,##0.00}", contractRanges[j, 0]), string.Format("{0:#,##0.00}", contractRanges[j, 1]))
                    : string.Format("${0} Or Greater", string.Format("{0:#,##0.00}", contractRanges[j, 0]));

                if (proposals.Any())
                {
                    //TODO - This needs to change after production - report vendor total + report B total.  We need to tie vendor bids to the milestone table.
                    reportLettingSummary.NumberOfContracts = proposals.Count();
                    reportLettingSummary.ValueInCategory = proposals.Sum(p => p.ReportProposalVendors.Sum(v => v.ReportVendorBids.Where(b => b.LowCost).Sum(b => b.BidPrice)));
                    reportLettingSummary.ValueOfEstimate = proposals.Sum(i => i.Total) + proposals.Sum(i => i.ReportProposalMilestones.Sum(m => m.Total));
                    reportLettingSummary.PercentageOfContracts = Math.Round((proposals.Count() / (decimal)reportLetting.ReportProposals.Count()) * 100, 2);
                    reportLettingSummary.PercentageOfLettingTotal = Math.Round((reportLettingSummary.ValueOfEstimate / (reportLetting.Total + reportLetting.ReportProposals.Sum(i => i.ReportProposalMilestones.Sum(m => m.Total)))) * 100, 2);


                    var rangeAverage = new List<decimal>();

                    foreach (var item in proposals)
                    {
                        var bidRange = (from vendor in item.ReportProposalVendors where item.ReportProposalVendors.Any()
                                        select Math.Round(((vendor.Total / (item.Total + item.ReportProposalMilestones.Sum(m => m.Total))) * 100) - 100, 2))
                                        .ToList();
                        var average = bidRange.Any() ? Math.Round(bidRange.Sum() / bidRange.Count(), 2) : 0;
                        rangeAverage.Add(average);

                        if (average > 0)
                            reportLettingSummary.NumberOfContractsAboveEstimate++;
                        else if (average < 0)
                            reportLettingSummary.NumberOfContractsBelowEstimate++;

                    }

                    var lowRanges = rangeAverage.Where(i => i < 0).ToList();
                    var highRanges = rangeAverage.Where(i => i > 0).ToList();


                    reportLettingSummary.PercentageRangeDifferenceBelow = lowRanges.Any()
                        ? Math.Abs(lowRanges.Min()) + " - " + Math.Abs(lowRanges.Max())
                        : string.Empty;
                    reportLettingSummary.AveragePercentageBelowEstimate = lowRanges.Any()
                        ? Math.Abs(lowRanges.Sum() / lowRanges.Count)
                        : 0;

                    reportLettingSummary.PercentageRangeDifferenceAbove = highRanges.Any()
                        ? highRanges.Min() + " - " + highRanges.Max()
                        : string.Empty;
                    reportLettingSummary.AveragePercentageAboveEstimate = highRanges.Any()
                        ? highRanges.Sum() / highRanges.Count
                        : 0;

                    reportLettingSummary.AverageBidsPerContract = Math.Round(proposals.Sum(i => i.ReportProposalVendors.Count() / (decimal)reportLettingSummary.NumberOfContracts), 2);
                }
                else
                {
                    reportLettingSummary.NumberOfContracts = 0;
                    reportLettingSummary.ValueInCategory = 0;
                    reportLettingSummary.ValueOfEstimate = 0;
                    reportLettingSummary.PercentageOfContracts = 0;
                    reportLettingSummary.PercentageOfLettingTotal = 0;
                    reportLettingSummary.NumberOfContractsBelowEstimate = 0;
                    reportLettingSummary.PercentageRangeDifferenceBelow = string.Empty;
                    reportLettingSummary.AveragePercentageBelowEstimate = 0;
                    reportLettingSummary.NumberOfContractsAboveEstimate = 0;
                    reportLettingSummary.PercentageRangeDifferenceAbove = string.Empty;
                    reportLettingSummary.AveragePercentageAboveEstimate = 0;
                    reportLettingSummary.AverageBidsPerContract = 0;
                }

                reportLetting.AddReportLettingSummary(reportLettingSummary);
            }
        }

        private static decimal[,] ContractRange()
        {
            return new decimal[6, 2]
            {
                {(decimal) 1, (decimal) 499999.99},
                {(decimal) 500000, (decimal) 999999.99},
                {(decimal) 1000000, (decimal) 2999999.99},
                {(decimal) 3000000, (decimal) 9999999.99},
                {(decimal) 10000000, (decimal) 49999999.99},
                {(decimal) 50000000, (decimal) 999999999}
            };
        }

        /// <summary>
        /// Compares WT proposals with Report proposals by checking to see if there are any bids tied to the proposal.
        /// </summary>
        /// <param name="wtletting"></param>
        /// <param name="reportLetting"></param>
        /// <returns></returns>
        private static bool RebuildVendorBidAndLettingSummary(Letting wtletting, ReportLetting reportLetting)
        {
            if (reportLetting == null) return true;
            if (reportLetting.ReportProposals.Any())
            {
                foreach (var proposal in reportLetting.ReportProposals)
                {
                    var wtProposal = wtletting.Proposals.FirstOrDefault(i => i.ProposalNumber == proposal.ProposalNumber);

                    if (wtProposal != null && !proposal.ReportProposalVendors.Any()) //&& wtProposal.ProposalVendors.Any()
                        return true;
                }
            }
            return false;
        }
    }
}
