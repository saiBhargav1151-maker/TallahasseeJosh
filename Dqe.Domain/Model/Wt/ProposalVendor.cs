using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Dqe.Domain.Model.Wt
{
    public class ProposalVendor
    {
        private readonly ICollection<Bid> _bids;

        public ProposalVendor()
        {
            _bids = new Collection<Bid>();
        }

        public virtual long Id { get; set; }

        public virtual bool Awarded { get; set; }

        public virtual string BidType { get; set; }

        public virtual string BidStatus { get; set; }

        public virtual Proposal MyProposal { get; set; }

        public virtual RefVendor MyRefVendor { get; set; }

        public virtual decimal? BidTotal { get; set; }

        public virtual IEnumerable<Bid> Bids
        {
            get { return _bids.ToList().AsReadOnly(); }
        }
    }
}