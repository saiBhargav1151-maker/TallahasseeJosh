using System.Collections.Generic;

namespace Dqe.ApplicationServices
{
    public class BidHistory
    {
        private readonly List<ProposalHistory> _proposalHistories = new List<ProposalHistory>();
        public object MaxBiddersProposal { get; set; }
        public string ItemName { get; set; }
        public IList<ProposalHistory> Proposals { get { return _proposalHistories; } } 
    }
}