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
        public decimal Quantity { get; set; }
        public decimal EstimateAmount { get; set; }
        public decimal ExtendedAmount { get; set; }
        public DateTime Letting { get; set; }
        public string ProposalType { get; set; }
        public string ContractType { get; set; }
        public string ContractWorkType { get; set; }
        public long Duration { get; set; }
        public IList<Bid> Bids { get { return _bids; } }
    }
}