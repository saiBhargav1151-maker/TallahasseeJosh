namespace Dqe.Domain.Model.Wt
{
    public class Bid
    {
        public virtual long Id { get; set; }

        public virtual decimal BidPrice { get; set; }

        public virtual bool ValidBid { get; set; }

        public virtual ProposalVendor MyProposalVendor { get; set; }

        public virtual ProposalItem MyProposalItem { get; set; }

        public virtual bool LowCost { get; set; }

    }
}
