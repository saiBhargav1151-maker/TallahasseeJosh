using System;
using System.Collections.Generic;
using System.Linq;
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
                .SingleOrDefault();
        }

        public ReportProposal GetReportProposalAndItems(string proposalNumber, ReportProposalLevel proposalLevel)
        {
            InitializeSession();

            return Session
                .QueryOver<ReportProposal>()
                .Where(i => i.ProposalNumber == proposalNumber)
                .Where(i => i.ProposalLevel == proposalLevel)
                //.Where(i => !i.InvalidEstimate)
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

        public void DeleteEmptyLettings()
        {
            InitializeSession();
            //delete orphaned letting summaries
            Session.CreateQuery("delete from ReportLettingSummary orls where orls.Id in (select rls.Id from ReportLettingSummary as rls join rls.MyReportLetting as rl left join rl.ReportProposals as rp group by rls.Id having count(rp) = 0)").ExecuteUpdate();
            //delete orphaned lettings
            Session.CreateQuery("delete from ReportLetting orl where orl.Id in (select rl.Id from ReportLetting as rl left join rl.ReportProposals as rp group by rl.Id having count(rp) = 0)").ExecuteUpdate();
        }

        public void DeleteLettingData(Letting letting, bool rebuildVendorBidData)
        {
            InitializeSession();
            //get the distinct lettings in DQE based on the proposals attached to the wT letting
            var wtProposals = letting.Proposals.ToList();
            var lettingNames = new List<string> { letting.LettingName };
            foreach (var wtProposal in wtProposals)
            {
                var proposal = wtProposal;
                var rpl = Session.QueryOver<ReportProposal>()
                    .Where(i => i.ProposalNumber == proposal.ProposalNumber)
                    .List();
                foreach (var rp in rpl)
                {
                    if (rp.MyReportLetting != null)
                    {
                        lettingNames.Add(rp.MyReportLetting.LettingName);
                    }
                    //remove bids
                    if (rebuildVendorBidData)
                    {
                        rp.MyReportLetting = null;
                        rp.ClearProposalVendors();
                    }
                }
            }
            //remove letting from orphaned DQE proposals (those not in the proposal list from wT)
            Session.Flush();
            var orpl = Session.QueryOver<ReportProposal>()
                    .JoinQueryOver(i => i.MyReportLetting)
                    .Where(i => i.LettingName == letting.LettingName)
                    .List();
            foreach (var rp in orpl)
            {
                //remove bids
                rp.MyReportLetting = null;
                rp.ClearProposalVendors();
            }
            var distinctLettingNames = lettingNames.Distinct().ToList();
            //remove the lettings and summaries from DQE
            foreach (var l in distinctLettingNames)
            {
                var l1 = l;
                var rpl = Session.QueryOver<ReportLetting>()
                    .Where(i => i.LettingName == l1)
                    .List();
                foreach (var reportLetting in rpl)
                {
                    Session.Delete(reportLetting);
                }
            }
            Session.Flush();
            DeleteEmptyLettings();
        }

        public void DeleteLettingByName(string name)
        {
            InitializeSession();
            var lettings = Session.QueryOver<ReportLetting>()
                .Where(i => i.LettingName == name)
                .Left.JoinQueryOver(i => i.ReportLettingSummaries)
                .Where(i => i.Id == null)
                .List();

            foreach (var reportLetting in lettings)
            {
                Session.Delete(reportLetting);
            }
        }

        public void RebuildReportStructure(Letting letting, List<ReportProposal> officialProposals, List<ReportProposal> authorizedProposals, bool rebuildVendorBidData, List<Domain.Model.PayItemMaster> payItems)
        {
            RebuildReportStructureForAuthorization(letting, authorizedProposals);

            RebuildReportStructureForOfficial(letting, officialProposals, rebuildVendorBidData, payItems);
        }

        public void RebuildReportStructureForOfficial(Letting letting, List<ReportProposal> officialProposals, bool rebuildVendorBidData, List<Domain.Model.PayItemMaster> payItems)
        {
            InitializeSession();

            var reportLetting = new ReportLetting
            {
                LettingName = letting.LettingName,
                LettingDate = letting.LettingDate
            };

            if (officialProposals == null)
                officialProposals = GetProposalsInList(letting.Proposals.Select(i => i.ProposalNumber).ToList(), ReportProposalLevel.Official).ToList();

            foreach (var proposal in letting.Proposals)
            {
                var offP = officialProposals.FirstOrDefault(a => a.ProposalNumber == proposal.ProposalNumber);
                if (offP != null)
                    reportLetting.AddReportProposal(offP);
            }

            var vendorData = false;

            foreach (var proposal in reportLetting.ReportProposals)
            {
                var proposalVendorData = false;
                var wtLettingProposal = letting.Proposals.First(i => i.ProposalNumber == proposal.ProposalNumber);

                foreach (var project in proposal.ReportProjects)
                {
                    project.LettingDate = wtLettingProposal.Projects.First(p => p.ProjectNumber == project.ProjectNumber).LettingDate;
                }

                if (rebuildVendorBidData && wtLettingProposal.ProposalVendors.Any() && wtLettingProposal.ProposalVendors.Any(b => b.Bids.Any()))
                {
                    foreach (var vendor in wtLettingProposal.ProposalVendors.Where(b => b.Bids.Any()))
                    {
                        CreateVendorData(vendor, proposal);
                        vendorData = true;
                        proposalVendorData = true;
                    }
                    if (proposalVendorData)
                    {
                        SetLowBid(proposal);
                        DetermineUnbalancedItems(proposal, payItems);
                        DetermineMagnitude(proposal);
                    }
                }
            }

            if (vendorData)
                GenerateLettingSummary(reportLetting);

            Session.SaveOrUpdate(reportLetting);
            Session.Flush();
        }

        public void RebuildReportStructureForAuthorization(Letting letting, List<ReportProposal> authorizedProposals)
        {
            InitializeSession();

            var reportLetting = new ReportLetting
            {
                LettingName = letting.LettingName,
                LettingDate = letting.LettingDate
            };

            if (authorizedProposals == null)
                authorizedProposals = GetProposalsInList(letting.Proposals.Select(i => i.ProposalNumber).ToList(), ReportProposalLevel.Authorization).ToList();

            foreach (var proposal in letting.Proposals)
            {
                var authP = authorizedProposals.FirstOrDefault(a => a.ProposalNumber == proposal.ProposalNumber);
                if (authP != null)
                    reportLetting.AddReportProposal(authP);
            }

            foreach (var proposal in reportLetting.ReportProposals)
            {
                var wtLettingProposal = letting.Proposals.First(i => i.ProposalNumber == proposal.ProposalNumber);

                foreach (var project in proposal.ReportProjects)
                {
                    project.LettingDate = wtLettingProposal.Projects.First(p => p.ProjectNumber == project.ProjectNumber).LettingDate;
                }
            }
            Session.SaveOrUpdate(reportLetting);
            Session.Flush();
        }

        private static void SetLowBid(ReportProposal proposal)
        {
            foreach (var proposalItem in proposal.ReportProposalItems)
            {
                //var lowBid = proposalItem.ReportVendorBids.OrderBy(b => b.BidPrice).First();

                var lowBid = proposalItem.ReportVendorBids.OrderBy(b => b.BidPrice).FirstOrDefault();
                if (lowBid == null) continue; 

                foreach (var vendorBid in proposalItem.ReportVendorBids)
                    vendorBid.LowCost = vendorBid == lowBid;
            }

            foreach (var milestone in proposal.ReportProposalMilestones.Where(m => m.CostPerDay > 0))
            {
                var lowBid = milestone.ReportMilestoneBids.OrderBy(b => b.NumberOfDaysBid).First();
                foreach (var milestoneBid in milestone.ReportMilestoneBids)
                    milestoneBid.LowCost = milestoneBid == lowBid;
            }
        }

        private static void CreateVendorData(ProposalVendor vendor, ReportProposal proposal)
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
                                    ? (Decimal.Round((decimal)bid.BidPrice / myProposalItem.Quantity, 5))
                                    : (Decimal.Round((decimal)bid.BidPrice, 5))
                                : 0,
                        LowCost = bid.LowCost
                    };
                    reportProposalVendor.AddReportVendorBid(reportVendorBid);
                    myProposalItem.AddReportVendorBid(reportVendorBid);
                }
            }

            foreach (var bid in vendor.BidTimes)
            {
                var myMilestone = proposal.ReportProposalMilestones.First(i => i.WtId == bid.MyMilestone.Id);

                var reportMilestoneBid = new ReportMilestoneBid
                {
                    NumberOfDaysBid = bid.NumberOfUnits,
                    BidPrice = bid.NumberOfUnits > 0 ? bid.CalculatedPrice / bid.NumberOfUnits : bid.CalculatedPrice
                };

                reportProposalVendor.AddReportMilestoneBid(reportMilestoneBid);
                myMilestone.AddReportMilestoneBid(reportMilestoneBid);
            }
        }

        private static void DetermineUnbalancedItems(ReportProposal proposal, List<Domain.Model.PayItemMaster> payItems)
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
                {
                    var ri = payItems.FirstOrDefault(i => i.IsFrontLoadedItem && i.RefItemName == item.MyReportProposalItem.ItemNumber);

                    if (ri != null)
                        item.Tolerance = "F";
                    else
                        item.Tolerance = "A";
                }
            }

            foreach (var milestone in proposal.ReportProposalMilestones.Where(m => m.CostPerDay > 0))
            {
                var averageDaysBid = (milestone.ConstructionDays + milestone.ReportMilestoneBids.Sum(i => i.NumberOfDaysBid)) /
                                     (milestone.ReportMilestoneBids.Count() + 1);

                var reportMilestoneBid = new ReportMilestoneBid { BidPrice = milestone.CostPerDay, NumberOfDaysBid = (int)milestone.ConstructionDays, Tolerance = "ESTIMATE" };

                AverageMilestonePriceByBid(milestone, (int)averageDaysBid, reportMilestoneBid);

                var unbalanced = milestone.ReportMilestoneBids
                     .Where(i => i.NumberOfDaysBid < milestone.AverageBidPrice * (decimal).4)
                    .ToList();
                var aboveTolerance = milestone.ReportMilestoneBids
                    .Where(i => i.NumberOfDaysBid > milestone.AverageBidPrice * (decimal)1.6)
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

        private static void AverageMilestonePriceByBid(ReportProposalMilestone reportProposalMilestone, int averageDaysBid, ReportMilestoneBid reportMilestoneBid)
        {
            var l = reportProposalMilestone.ReportMilestoneBids.ToList();
            l.Add(reportMilestoneBid);

            var firstCheck = l
                .Where(i => i.NumberOfDaysBid > averageDaysBid * (decimal).55)
                .Where(i => i.NumberOfDaysBid < averageDaysBid * (decimal)1.45)
                .ToList();

            if (firstCheck.Any())
            {
                reportProposalMilestone.AverageBidPrice = firstCheck.Sum(i => i.NumberOfDaysBid) / (decimal)firstCheck.Count;
                return;
            }

            var secondCheck = l
                .Where(i => i.NumberOfDaysBid > averageDaysBid * (decimal).1)
                .Where(i => i.NumberOfDaysBid < averageDaysBid * (decimal)1.9)
                .ToList();

            if (secondCheck.Any())
            {
                reportProposalMilestone.AverageBidPrice = secondCheck.Sum(i => i.NumberOfDaysBid) / (decimal)secondCheck.Count;
                return;
            }

            var thirdCheck = l
                .Where(i => i.NumberOfDaysBid > averageDaysBid * 0)
                .Where(i => i.NumberOfDaysBid < averageDaysBid * 2)
                .ToList();

            if (thirdCheck.Any())
                reportProposalMilestone.AverageBidPrice = thirdCheck.Sum(i => i.NumberOfDaysBid) / (decimal)thirdCheck.Count;
        }

        private static void DetermineMagnitude(ReportProposal reportProposal)
        {
            var lowestVendor = reportProposal.ReportProposalVendors
                .Where(i => i.Total > 0)
                .OrderBy(i => i.Total)
                .FirstOrDefault();

            if (lowestVendor != null)
            {
                foreach (var proposalItem in reportProposal.ReportProposalItems)
                {
                    var vendorItemBid = lowestVendor.ReportVendorBids.FirstOrDefault(i => i.MyReportProposalItem.Id == proposalItem.Id);
                    proposalItem.Magnitude = vendorItemBid != null ? Math.Abs(proposalItem.Total - vendorItemBid.BidPrice * proposalItem.Quantity) : 0;
                }

                foreach (var milestone in reportProposal.ReportProposalMilestones.Where(i => i.Total != 0))
                {
                    var vendorMilestoneBid = lowestVendor.ReportMilestoneBids.FirstOrDefault(i => i.MyReportProposalMilestone.Id == milestone.Id);
                    milestone.Magnitude = vendorMilestoneBid != null ? Math.Abs(milestone.Total - vendorMilestoneBid.BidPrice*vendorMilestoneBid.NumberOfDaysBid) : 0;
                }
            }
            else
            {
                foreach (var proposalItem in reportProposal.ReportProposalItems)
                    proposalItem.Magnitude = 0;

                foreach (var milestone in reportProposal.ReportProposalMilestones.Where(i => i.Total != 0))
                    milestone.Magnitude = 0;
            }
        }

        private static void GenerateLettingSummary(ReportLetting reportLetting)
        {
            var contractRanges = ContractRange();

            for (var j = 0; j <= 5; j++)
            {
                var reportLettingSummary = new ReportLettingSummary();
                var proposals = reportLetting.ReportProposals
                    .Where(i => i.ReportProposalVendors.Any() &&
                                i.ReportProposalVendors.Min(v => v.Total) >= contractRanges[j, 0] &&
                                i.ReportProposalVendors.Min(v => v.Total) < contractRanges[j, 1])
                    .ToList();

                reportLettingSummary.ContractRange = j < 5
                    ? string.Format("${0} - ${1}", string.Format("{0:#,##0.00}", contractRanges[j, 0]), string.Format("{0:#,##0.00}", contractRanges[j, 1]))
                    : string.Format("${0} Or Greater", string.Format("{0:#,##0.00}", contractRanges[j, 0]));

                if (proposals.Any())
                {
                    reportLettingSummary.NumberOfContracts = proposals.Count();
                    reportLettingSummary.ValueInCategory = proposals.Sum(p => p.ReportProposalVendors.Min(v => v.Total));
                    reportLettingSummary.ValueOfEstimate = proposals.Sum(i => i.Total) + proposals.Sum(i => i.ReportProposalMilestones.Sum(m => m.Total));
                    reportLettingSummary.PercentageOfContracts = Math.Round((proposals.Count() / (decimal)reportLetting.ReportProposals.Count()) * 100, 2);

                    reportLettingSummary.PercentageOfLettingTotal =
                        Math.Round(
                            (reportLettingSummary.ValueOfEstimate /
                             (reportLetting.Total + reportLetting.ReportProposals.Sum(i => i.ReportProposalMilestones.Sum(m => m.Total))))
                             * 100, 2);

                    var countOfLowest = 0;
                    var rangeOfLowest = new List<decimal>();
                    var countOfAbove = 0;
                    var rangeOfAbove = new List<decimal>();

                    foreach (var proposal in proposals)
                    {
                        var lowestVendor = proposal.ReportProposalVendors.Min(i => i.Total);
                        var range = Math.Round((lowestVendor / (proposal.Total + proposal.ReportProposalMilestones.Sum(m => m.Total))) * 100 - 100, 2);

                        if (lowestVendor <= proposal.Total + proposal.ReportProposalMilestones.Sum(m => m.Total))
                        {
                            countOfLowest++;
                            rangeOfLowest.Add(range);
                        }
                        else
                        {
                            countOfAbove++;
                            rangeOfAbove.Add(range);
                        }
                    }

                    reportLettingSummary.NumberOfContractsBelowEstimate = countOfLowest;
                    reportLettingSummary.PercentageRangeDifferenceBelow = rangeOfLowest.Any()
                        ? Math.Abs(rangeOfLowest.Min()) + " - " + Math.Abs(rangeOfLowest.Max())
                        : string.Empty;
                    reportLettingSummary.AveragePercentageBelowEstimate = rangeOfLowest.Any()
                        ? Math.Abs(rangeOfLowest.Sum() / rangeOfLowest.Count)
                        : 0;

                    reportLettingSummary.NumberOfContractsAboveEstimate = countOfAbove;
                    reportLettingSummary.PercentageRangeDifferenceAbove = rangeOfAbove.Any()
                        ? Math.Abs(rangeOfAbove.Min()) + " - " + Math.Abs(rangeOfAbove.Max())
                        : string.Empty;
                    reportLettingSummary.AveragePercentageAboveEstimate = rangeOfAbove.Any()
                        ? Math.Abs(rangeOfAbove.Sum() / rangeOfAbove.Count)
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
    }
}
