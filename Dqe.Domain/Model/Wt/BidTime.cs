namespace Dqe.Domain.Model.Wt
{
    public class BidTime
    {
        public virtual long Id { get; set; }
        public virtual bool ValidBid { get; set; }
        public virtual decimal CalculatedPrice { get; set; }
        public virtual int NumberOfUnits { get; set; }
        public virtual Milestone MyMilestone { get; set; }
        public virtual ProposalVendor MyProposalVendor { get; set; }
    }
}
