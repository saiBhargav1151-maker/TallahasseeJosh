using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dqe.Domain.Tests
{
    [TestClass]
// ReSharper disable InconsistentNaming
    public class When_Using_Dqe_Formula
// ReSharper restore InconsistentNaming
    {

        public class Bid
        {
            public DateTime LettingDate { get; set; }
            public bool IsAwarded { get; set; }
            public decimal Quantity { get; set; }
            public decimal Price { get; set; }
            public decimal TimeFactor { get; set; }
            public decimal TimeWeight { get; set; }
            public bool Included { get; set; }
        }

        public class BidSet
        {
            public IEnumerable<Bid> Bids { get; set; }
            public decimal AveragePrice { get; set; }
        }

        private BidSet _bidSet = new BidSet(); 
        
        [TestInitialize]
        public void SetUp()
        {
            _bidSet = new BidSet
            {
                Bids = new List<Bid>
                {
                    new Bid
                    {
                        IsAwarded = true,
                        LettingDate = DateTime.Now.Date.AddDays(-46),
                        Price = 10,
                        Quantity = 100
                    },
                    new Bid
                    {
                        IsAwarded = true,
                        LettingDate = DateTime.Now.Date.AddDays(-146),
                        Price = (decimal) 10.34,
                        Quantity = 150
                    },
                    new Bid
                    {
                        IsAwarded = true,
                        LettingDate = DateTime.Now.Date.AddDays(-22),
                        Price = (decimal) 11.01,
                        Quantity = 55
                    },
                    new Bid
                    {
                        IsAwarded = true,
                        LettingDate = DateTime.Now.Date.AddDays(-96),
                        Price = (decimal) 10.57,
                        Quantity = 300
                    },
                    new Bid
                    {
                        IsAwarded = true,
                        LettingDate = DateTime.Now.Date.AddDays(-246),
                        Price = (decimal) 10.03,
                        Quantity = 30
                    },
                    new Bid
                    {
                        IsAwarded = true,
                        LettingDate = DateTime.Now.Date.AddDays(-346),
                        Price = (decimal) 11.56,
                        Quantity = 100
                    },
                    new Bid
                    {
                        IsAwarded = true,
                        LettingDate = DateTime.Now.Date.AddDays(-546),
                        Price = (decimal) 12.02,
                        Quantity = 500
                    },
                    new Bid
                    {
                        IsAwarded = true,
                        LettingDate = DateTime.Now.Date.AddDays(-446),
                        Price = (decimal) 22.34,
                        Quantity = 1000
                    },
                    new Bid
                    {
                        IsAwarded = true,
                        LettingDate = DateTime.Now.Date.AddDays(-406),
                        Price = 16,
                        Quantity = 367
                    },
                    new Bid
                    {
                        IsAwarded = true,
                        LettingDate = DateTime.Now.Date.AddDays(-176),
                        Price = (decimal) 13.25,
                        Quantity = 1000
                    },
                    new Bid
                    {
                        IsAwarded = true,
                        LettingDate = DateTime.Now.Date.AddDays(-16),
                        Price = (decimal) 4.23,
                        Quantity = 220
                    },
                    new Bid
                    {
                        IsAwarded = true,
                        LettingDate = DateTime.Now.Date.AddDays(-96),
                        Price = (decimal) 11.45,
                        Quantity = 678
                    },
                }
            };
        }

        [TestMethod]
        public void Average_Price_Is_Calculated()
        {
            CalculateAveragePrice(_bidSet);
        }

        public static void CalculateAveragePrice(BidSet bidSet)
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

        public static void SetTimeWeights(IEnumerable<Bid> bids)
        {
            var b = bids.ToList();
            foreach (var bid in b)
            {
                if (bid.LettingDate.Date.AddMonths(3) >= DateTime.Now.Date)
                {
                    bid.TimeFactor = (decimal) .35;
                }
                else if (bid.LettingDate.Date.AddMonths(6) >= DateTime.Now.Date)
                {
                    bid.TimeFactor = (decimal) .30;
                }
                else if (bid.LettingDate.Date.AddMonths(12) >= DateTime.Now.Date)
                {
                    bid.TimeFactor = (decimal) .20;
                }
                else
                {
                    bid.TimeFactor = (decimal) .15;
                }
            }
            var totalTime = b.Sum(i => i.TimeFactor);
            if (totalTime == 0) return;
            foreach (var bid in b)
            {
                bid.TimeWeight = bid.TimeFactor / totalTime;
            }
        }

        public static decimal CalculateTimeWeightedAverage(IEnumerable<Bid> bids)
        {
            return bids.Sum(i => i.Price*i.TimeWeight);
        }

        public static decimal CalculateStandardDeviation(IEnumerable<Bid> bids)
        {
            var b = bids.ToList();
            var avg = Convert.ToDouble(b.Average(i => i.Price));
            return Convert.ToDecimal(Math.Sqrt(b.Select(i => i.Price).Average(v => Math.Pow(Convert.ToDouble(v) - avg, 2))));
        }
    }
}
