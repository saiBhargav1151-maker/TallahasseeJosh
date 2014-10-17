namespace Dqe.Domain.Model.Wt
{
    public class ProjectItem
    {
        public virtual long Id { get; set; }

        public virtual RefItem MyRefItem { get; set; }

        public virtual Category MyCategory { get; set; }

        public virtual ProposalItem MyProposalItem { get; set; }

        public virtual Alternate MyAlternate { get; set; }

        public virtual string AlternateMember { get; set; }

        public virtual bool CombineLikeItems { get; set; }

        public virtual decimal Quantity { get; set; }

        public virtual decimal Price { get; set; }

        public virtual string LineNumber { get; set; }

    }
}