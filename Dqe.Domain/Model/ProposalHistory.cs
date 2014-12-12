using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Dqe.Domain.Model
{
    public class ProposalHistory : Entity<Transformers.ProposalHistory>
    {
        private readonly ICollection<BidHistory> _bidHistories;

        public ProposalHistory()
        {
            _bidHistories = new Collection<BidHistory>();
        }

        public virtual ProjectItem MyProjectItem { get; protected internal set; }

        [Required]
        public virtual string ProposalNumber { get; protected internal set; }

        public virtual DateTime LettingDate { get; protected internal set; }

        public virtual DateTime? AwardedDate { get; protected internal set; }

        public virtual DateTime? ExecutedDate { get; protected internal set; }

        public virtual IEnumerable<BidHistory> BidHistories
        {
            get { return _bidHistories.ToList().AsReadOnly(); }
        }

        public virtual void AddBidHistory(dynamic bidHistory)
        {
            var bh = new BidHistory
            {
                IncludedInAverage = bidHistory.include,
                Price = bidHistory.price,
                MyProposalHistory = this
            };
            _bidHistories.Add(bh);
        }

        public override Transformers.ProposalHistory GetTransformer()
        {
            throw new NotImplementedException();
        }

        public override void Transform(Transformers.ProposalHistory transformer, DqeUser account)
        {
            throw new NotImplementedException();
        }
    }
}