using System;

namespace Dqe.Domain.Model.Wt
{
    public class ProjectItem
    {
        public virtual long Id { get; set; }

        public virtual RefItem MyRefItem { get; set; }

        public virtual Category MyCategory { get; set; }

        public virtual ProposalItem MyProposalItem { get; set; }

        public virtual Alternate MyAlternate { get; set; }

        public virtual FundPackage MyFundPackage { get; set; }

        public virtual string AlternateMember { get; set; }

        public virtual bool CombineLikeItems { get; set; }

        public virtual decimal Quantity { get; set; }

        public virtual string LineNumber { get; set; }

        public virtual string SupplementalDescription { get; set; }

        #region "Pricing"

        public virtual bool IsLowCost { get; set; }

        public virtual decimal Price { get; set; }

        public virtual decimal? ExtendedAmount { get; set; }

        public virtual string EstimateType { get; set; }

        public virtual string PricingComments { get; set; }

        public virtual DateTime? LastUpdatedDate { get; set; }

        public virtual string LastUpdatedBy { get; set; }
        public virtual Project MyProject { get; set; }


        #endregion

    }
}