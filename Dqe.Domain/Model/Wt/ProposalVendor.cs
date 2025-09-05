using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Dqe.Domain.Model.Wt
{
    public class ProposalVendor
    {
        private readonly ICollection<Bid> _bids;
        private readonly ICollection<BidTime> _bidTimes; 

        public ProposalVendor()
        {
            _bids = new Collection<Bid>();
            _bidTimes = new Collection<BidTime>();
        }

        public virtual long Id { get; set; }

        public virtual bool Awarded { get; set; }

        public virtual string BidType { get; set; }

        public virtual string BidStatus { get; set; }

        public virtual Proposal MyProposal { get; set; }

        public virtual RefVendor MyRefVendor { get; set; }

        public virtual decimal? BidTotal { get; set; }
        public virtual int? VendorRanking { get; set; }
        public virtual IEnumerable<Bid> Bids
        {
            get { return _bids.ToList().AsReadOnly(); }
        }

        public virtual IEnumerable<BidTime> BidTimes
        {
            get { return _bidTimes.ToList().AsReadOnly(); }
        }
    }
}