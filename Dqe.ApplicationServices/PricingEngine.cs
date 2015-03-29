using System;
using System.Collections.Generic;
using System.Linq;
using Dqe.Domain.Model;
using Dqe.Domain.Repositories.Custom;

namespace Dqe.ApplicationServices
{
    public class PricingEngine : IPricingEngine
    {
        private static IEnumerable<MarketArea> _marketAreas; 

        public PricingEngine(IMarketAreaRepository marketAreaRepository)
        {
            _marketAreas = marketAreaRepository.GetAllMarketAreas();
        }

        public BidSet CalculateStateAveragePrice(BidHistory bidHistory, PayItemMaster payItemMaster, DqeUser dqeUser)
        {
            //TODO: add the 12 month range filter
            var bids = (from proposal in bidHistory.Proposals
                from bid in proposal.Bids
                where !bid.IsBlank
                select bid).ToList();
            var bidSet = new BidSet
            {
                Bids = bids
            };
            var bs = CalculateAveragePrice(bidSet, true, 0, null);
            //persist all bids associated to the item
            foreach (var proposal in bidHistory.Proposals)
            {
                //if (!proposal.Bids.Any(i => i.Included)) continue;
                var ph = new Domain.Model.ProposalHistory();
                var pht = ph.GetTransformer();
                pht.County = proposal.County;
                pht.LettingDate = proposal.Letting;
                pht.Quantity = proposal.Quantity;
                pht.ProposalNumber = proposal.Proposal;
                pht.ContractType = string.IsNullOrWhiteSpace(proposal.ContractType) ? string.Empty : proposal.ContractType;
                pht.ContractWorkType = string.IsNullOrWhiteSpace(proposal.ContractWorkType) ? string.Empty : proposal.ContractWorkType;
                pht.ProposalType = string.IsNullOrWhiteSpace(proposal.ProposalType) ? string.Empty : proposal.ProposalType;
                pht.Duration = proposal.Duration;
                ph.Transform(pht, dqeUser);
                payItemMaster.AddProposalHistory(ph);
                foreach (var bid in proposal.Bids.Where(i => !i.IsBlank))
                {
                    var bh = new Domain.Model.BidHistory();
                    var bht = bh.GetTransformer();
                    bht.IsAwarded = bid.IsAwarded;
                    bht.BidTotal = bid.BidTotal;
                    bht.IsLowCost = bid.IsLowCost;
                    bht.Price = bid.Price;
                    bh.Transform(bht, dqeUser);
                    ph.AddBidHistory(bh);
                }
            }
            return bs;
        }

        public BidSet CalculateMarketAreaAveragePrice(BidHistory bidHistory, MarketArea marketArea)
        {
            //TODO: add the 12 month range filter
            var bids = (from proposal in bidHistory.Proposals
                where marketArea.Counties.Select(i => i.Name).Contains(proposal.County)
                from bid in proposal.Bids
                where !bid.IsBlank
                select bid);
            var bidSet = new BidSet
            {
                Bids = bids
            };
            return CalculateAveragePrice(bidSet, true, 0, null);
        }

        public BidSet CalculateCountyAveragePrice(BidHistory bidHistory, County county)
        {
            //TODO: add the 12 month range filter
            var bids = (from proposal
                in bidHistory.Proposals
                where county.Name == proposal.County
                from bid in proposal.Bids
                where !bid.IsBlank
                select bid);
            var bidSet = new BidSet
            {
                Bids = bids
            };
            return CalculateAveragePrice(bidSet, true, 0, null);
        }

        public BidSet CalculateAveragePrice(BidSet bidSet, bool resetIncludedBids, decimal refQuantity, County refCounty)
        {
            //decimal straightAveragePrice;
            decimal median;
            //decimal standardDeviation;
            decimal absoluteDeviation;
            List<Bid> includedBids;
            var bs = new BidSet();
            ResetBids(bidSet, resetIncludedBids);
            var bids = bidSet.Bids.ToList();
            if (bids.Count == 0) return bs;
            if (refQuantity == 0)
            {
                refQuantity = bids.Average(i => i.Quantity);
            }
            if (bids.Count < 11)
            {
                if (bids.Count > 5 && resetIncludedBids)
                {
                    median = CalculateMedian(bids);
                    absoluteDeviation = CalculateAbsoluteDeviation(bids, median);
                    includedBids = bids.Where(bid => bid.AbsoluteDeviation <= absoluteDeviation).ToList();
                }
                else
                {
                    includedBids = bids;
                }
                if (!includedBids.Any()) return bs;
                includedBids.ForEach(i => i.Included = true);
                bs.UnweightedAveragePrice = includedBids.Average(i => i.Price);
                bs.LocationWeightedAveragePrice = CalculateLocationWeightedAverage(includedBids, refCounty);
                bs.QuantityWeightedAveragePrice = CalculateQuantityWeightedAverage(includedBids, refQuantity);
                bs.TimeWeightedAveragePrice = CalculateTimeWeightedAverage(includedBids);
                bs.Bids = includedBids;
                return bs;
            }
            median = CalculateMedian(bids);
            absoluteDeviation = CalculateAbsoluteDeviation(bids, median);
            includedBids = resetIncludedBids
                ? bids.Where(bid => bid.AbsoluteDeviation <= absoluteDeviation).ToList()
                : bids.Where(i => i.Included).ToList();

            if (!includedBids.Any()) return bs;
            includedBids.ForEach(i => i.Included = true);
            bs.UnweightedAveragePrice = includedBids.Average(i => i.Price);
            bs.LocationWeightedAveragePrice = CalculateLocationWeightedAverage(includedBids, refCounty);
            bs.QuantityWeightedAveragePrice = CalculateQuantityWeightedAverage(includedBids, refQuantity);
            bs.TimeWeightedAveragePrice = CalculateTimeWeightedAverage(includedBids);
            bs.Bids = includedBids;
            return bs;
        }

        private static void ResetBids(BidSet bidSet, bool resetIncludedBids)
        {
            bidSet.LocationWeightedAveragePrice = 0;
            bidSet.QuantityWeightedAveragePrice = 0;
            bidSet.TimeWeightedAveragePrice = 0;
            bidSet.UnweightedAveragePrice = 0;
            var bids = bidSet.Bids.ToList();
            bids.ForEach(i =>
            {
                if (resetIncludedBids) i.Included = false;
                i.LocationWeight = 0;
                i.QuantityWeight = 0;
                i.TimeWeight = 0;
            });
        }

        private static decimal CalculateTimeWeightedAverage(IEnumerable<Bid> bids)
        {
            bids = bids.ToList();
            foreach (var bid in bids)
            {
                bid.TimeWeight = Math.Abs((DateTime.Now.Date - bid.LettingDate.Date).Days) * -1;
            }
            var zeroWeight = (bids.Min(i => i.TimeWeight) -1) * -1;
            foreach (var bid in bids)
            {
                bid.TimeWeight = bid.TimeWeight + zeroWeight;
            }
            var weightedBidTotal = bids.Sum(i => i.Price * i.TimeWeight);
            var weightTotal = bids.Sum(i => i.TimeWeight);
            var wa = weightedBidTotal / weightTotal;
            var x = 0;
            decimal? lastWeight = null;
            foreach (var bid in bids.OrderByDescending(i => i.TimeWeight))
            {
                if (!lastWeight.HasValue || bid.TimeWeight != lastWeight.Value)
                {
                    lastWeight = bid.TimeWeight;
                    x += 1;
                }
                bid.TimeWeight = x;
            }
            return wa;
        }

        private static decimal CalculateQuantityWeightedAverage(IEnumerable<Bid> bids, decimal refQuantity)
        {
            bids = bids.ToList();
            foreach (var bid in bids)
            {
                bid.QuantityWeight = Math.Abs((refQuantity - bid.Quantity)) * -1;
            }
            var zeroWeight = (bids.Min(i => i.QuantityWeight) - 1) * -1;
            foreach (var bid in bids)
            {
                bid.QuantityWeight = bid.QuantityWeight + zeroWeight;
            }
            var weightedBidTotal = bids.Sum(i => i.Price * i.QuantityWeight);
            var weightTotal = bids.Sum(i => i.QuantityWeight);
            var wa = weightedBidTotal / weightTotal;
            var x = 0;
            decimal? lastWeight = null;
            foreach (var bid in bids.OrderByDescending(i => i.QuantityWeight))
            {
                if (!lastWeight.HasValue || bid.QuantityWeight != lastWeight.Value)
                {
                    lastWeight = bid.QuantityWeight;
                    x += 1;
                }
                bid.QuantityWeight = x;
            }
            return wa;
        }

        private static decimal CalculateLocationWeightedAverage(IEnumerable<Bid> bids, County refCounty)
        {
            bids = bids.ToList();
            if (refCounty == null)
            {
                foreach (var bid in bids)
                {
                    bid.LocationWeight = 1;
                }
            }
            else
            {
                foreach (var bid in bids)
                {
                    if (bid.County == refCounty.Name)
                    {
                        bid.LocationWeight = 5;
                    }
                    else
                    {
                        var targetMarketArea = _marketAreas.FirstOrDefault(ma => ma.Counties.Any(c => c.Name == refCounty.Name));
                        bid.LocationWeight = 1;
                        if (targetMarketArea != null)
                        {
                            foreach (var c in targetMarketArea.Counties)
                            {
                                if (c.Name != bid.County) continue;
                                bid.LocationWeight = 3;
                                break;
                            }    
                        }
                    }
                }
            }
            var weightedBidTotal = bids.Sum(i => i.Price * i.LocationWeight);
            var weightTotal = bids.Sum(i => i.LocationWeight);
            var wa = weightedBidTotal / weightTotal;

            var x = 0;
            decimal? lastWeight = null;
            foreach (var bid in bids.OrderByDescending(i => i.LocationWeight))
            {
                if (!lastWeight.HasValue || bid.LocationWeight != lastWeight.Value)
                {
                    lastWeight = bid.LocationWeight;
                    x += 1;
                }
                bid.LocationWeight = x;
            }

            return wa;
        }

        private static decimal CalculateAbsoluteDeviation(IEnumerable<Bid> bids, decimal medain)
        {
            var ob = bids.OrderBy(i => i.Price).ToList();
            foreach (var bid in ob)
            {
                bid.AbsoluteDeviation = Math.Abs(bid.Price - medain);
            }
            ob = ob.OrderBy(i => i.AbsoluteDeviation).ToList();
            var size = ob.Count;
            int rem;
            var mid = Math.DivRem(size, 2, out rem) + rem;
            return (rem == 0)
                ? ob[mid].Price + ob[mid + 1].Price / 2
                : ob[mid].Price;
        }

        private static decimal CalculateMedian(IEnumerable<Bid> bids)
        {
            var ob = bids.OrderBy(i => i.Price).ToList();
            var size = ob.Count;
            int rem;
            var mid = Math.DivRem(size, 2, out rem) + rem;
            return (rem == 0)
                ? ob[mid].Price + ob[mid + 1].Price / 2
                : ob[mid].Price;
        }
    }
}