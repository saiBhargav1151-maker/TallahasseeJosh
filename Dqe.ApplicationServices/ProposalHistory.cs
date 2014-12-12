using System;
using System.Collections.Generic;

namespace Dqe.ApplicationServices
{
    public class ProposalHistory
    {
        private readonly List<Bid> _bids = new List<Bid>(); 
        public string Proposal { get; set; }
        public string County { get; set; }
        public bool Included { get; set; }
        public DateTime Letting { get; set; }
        public IList<Bid> Bids { get { return _bids; } }
    }
}