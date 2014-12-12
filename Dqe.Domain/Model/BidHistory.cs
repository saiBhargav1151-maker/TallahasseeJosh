using System;

namespace Dqe.Domain.Model
{
    public class BidHistory : Entity<Transformers.BidHistory>
    {
        public virtual ProposalHistory MyProposalHistory { get; protected internal set; }

        public virtual decimal Price { get; protected internal set; }

        public virtual bool IncludedInAverage { get; protected internal set; }

        public override Transformers.BidHistory GetTransformer()
        {
            throw new NotImplementedException();
        }

        public override void Transform(Transformers.BidHistory transformer, DqeUser account)
        {
            throw new NotImplementedException();
        }
    }
}