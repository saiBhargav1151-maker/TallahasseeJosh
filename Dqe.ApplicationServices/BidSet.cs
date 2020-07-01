using System;
using System.Collections.Generic;

namespace Dqe.ApplicationServices
{
    public class BidSet
    {
        public IEnumerable<Bid> Bids { get; set; }
        public decimal UnweightedAveragePrice { get; set; }
        public decimal TimeWeightedAveragePrice { get; set; }
        public decimal QuantityWeightedAveragePrice { get; set; }
        public decimal LocationWeightedAveragePrice { get; set; }
        
        public decimal WeightedAveragePrice
        {
            get
            {
                return Math.Round((TimeWeightedAveragePrice + QuantityWeightedAveragePrice + LocationWeightedAveragePrice)/3, 2, MidpointRounding.AwayFromZero);
                //return (TimeWeightedAveragePrice + QuantityWeightedAveragePrice) / 2;
            }
        }
    }
}