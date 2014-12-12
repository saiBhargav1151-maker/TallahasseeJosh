using System.Collections.Generic;

namespace Dqe.ApplicationServices
{
    public class BidSet
    {
        public IEnumerable<Bid> Bids { get; set; }
        public decimal AveragePrice { get; set; }
    }
}