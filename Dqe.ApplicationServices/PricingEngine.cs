using System;
using System.Collections.Generic;
using System.Linq;

namespace Dqe.ApplicationServices
{
    public class PricingEngine : IPricingEngine
    {
        public void CalculateAveragePrice(BidSet bidSet)
        {
            var bids = bidSet.Bids.ToList();
            if (bids.Count == 0) return;
            if (bids.Count < 11)
            {
                if (bids.Count > 4)
                {
                    //omit high and low
                    var low = bids.First(i => i.Price == bids.Min(ii => ii.Price));
                    var high = bids.First(i => i.Price == bids.Max(ii => ii.Price));
                    bids.Remove(low);
                    bids.Remove(high);
                }
                //time weighted average of bids
                SetTimeWeights(bids);
                bidSet.AveragePrice = CalculateTimeWeightedAverage(bids);
                bids.ForEach(i => i.Included = true);
            }
            else
            {
                //time and quantity weighted standard deviation of bids
                var straightAveragePrice = bids.Average(i => i.Price);
                var standardDeviation = CalculateStandardDeviation(bids);
                var includedBids = bids.Where(bid => Math.Abs(bid.Price - straightAveragePrice) <= 1 * standardDeviation).ToList();
                SetTimeWeights(includedBids);
                bidSet.AveragePrice = CalculateTimeWeightedAverage(includedBids);
                includedBids.ForEach(i => i.Included = true);
            }
        }

        private static void SetTimeWeights(IEnumerable<Bid> bids)
        {
            var b = bids.ToList();
            foreach (var bid in b)
            {
                if (bid.LettingDate.Date.AddMonths(3) >= DateTime.Now.Date)
                {
                    bid.TimeFactor = (decimal).35;
                }
                else if (bid.LettingDate.Date.AddMonths(6) >= DateTime.Now.Date)
                {
                    bid.TimeFactor = (decimal).30;
                }
                else if (bid.LettingDate.Date.AddMonths(12) >= DateTime.Now.Date)
                {
                    bid.TimeFactor = (decimal).20;
                }
                else
                {
                    bid.TimeFactor = (decimal).15;
                }
            }
            var totalTime = b.Sum(i => i.TimeFactor);
            if (totalTime == 0) return;
            foreach (var bid in b)
            {
                bid.TimeWeight = bid.TimeFactor / totalTime;
            }
        }

        private static decimal CalculateTimeWeightedAverage(IEnumerable<Bid> bids)
        {
            return bids.Sum(i => i.Price * i.TimeWeight);
        }

        private static decimal CalculateStandardDeviation(IEnumerable<Bid> bids)
        {
            var b = bids.ToList();
            var avg = Convert.ToDouble(b.Average(i => i.Price));
            return Convert.ToDecimal(Math.Sqrt(b.Select(i => i.Price).Average(v => Math.Pow(Convert.ToDouble(v) - avg, 2))));
        }
    }
}