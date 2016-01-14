using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Dqe.Domain.Model.Wt
{
    public class Milestone
    {
        private readonly ICollection<BidTime> _bidTimes;

        public Milestone()
        {
            _bidTimes = new Collection<BidTime>();
        }

        public virtual long Id { get; set; }

        public virtual bool Main { get; set; }

        public virtual long? NumberOfUnits { get; set; }

        public virtual string Unit { get; set; }

        public virtual decimal? RoadCostPerTimeUnit { get; set; }

        public virtual string Description { get; set; }

        public virtual Proposal MyProposal { get; set; }

        public virtual IEnumerable<BidTime> BidTimes
        {
            get { return _bidTimes.ToList().AsReadOnly(); }
        }
    }
}
