namespace Dqe.Domain.Model.Wt
{
    public class Milestone
    {
        public virtual long Id { get; set; }

        public virtual bool Main { get; set; }

        public virtual long? NumberOfUnits { get; set; }

        public virtual string Unit { get; set; }

        public virtual decimal? RoadCostPerTimeUnit { get; set; }

        public virtual string Description { get; set; }

        public virtual Proposal MyProposal { get; set; }
    }
}
