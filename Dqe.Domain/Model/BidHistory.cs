using System;

namespace Dqe.Domain.Model
{
    public class BidHistory : Entity<Transformers.BidHistory>
    {
        public virtual ProposalHistory MyProposalHistory { get; protected internal set; }

        public virtual decimal Price { get; protected internal set; }

        public virtual bool IsAwarded { get; protected internal set; }

        public virtual bool IsLowCost { get; protected internal set; }

        public virtual decimal BidTotal { get; protected internal set; }

        public override Transformers.BidHistory GetTransformer()
        {
            return new Transformers.BidHistory
            {
                Price = Price,
                IsAwarded = IsAwarded,
                IsLowCost = IsLowCost,
                BidTotal = BidTotal
            };
        }

        public override void Transform(Transformers.BidHistory transformer, DqeUser account)
        {
            Price = transformer.Price;
            IsAwarded = transformer.IsAwarded;
            IsLowCost = transformer.IsLowCost;
            BidTotal = transformer.BidTotal;
        }
    }
}