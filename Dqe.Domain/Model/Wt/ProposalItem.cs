using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Dqe.Domain.Model.Wt
{
    public class ProposalItem
    {
        private readonly ICollection<ProjectItem> _projectItems;
        private readonly ICollection<Bid> _bids;

        public ProposalItem()
        {
            _projectItems = new Collection<ProjectItem>();
            _bids = new Collection<Bid>();
        }

        public virtual long Id { get; set; }

        public virtual RefItem MyRefItem { get; set; }

        public virtual Section MySection { get; set; }

        public virtual decimal Quantity { get; set; }

        public virtual string AlternateSet { get; set; }

        public virtual string AlternateMember { get; set; }

        public virtual string SupplementalDescription { get; set; }

        public virtual string LineNumber { get; set; }

        #region "Pricing"

        public virtual bool IsLowCost { get; set; }

        public virtual decimal Price { get; set; }

        public virtual decimal? ExtendedAmount { get; set; }

        public virtual string EstimateType { get; set; }

        public virtual string PricingComments { get; set; }

        public virtual DateTime? LastUpdatedDate { get; set; }

        public virtual string LastUpdatedBy { get; set; }

        #endregion

        public virtual IEnumerable<ProjectItem> ProjectItems
        {
            get { return _projectItems.ToList().AsReadOnly(); }
        }

        public virtual IEnumerable<Bid> Bids
        {
            get { return _bids.ToList().AsReadOnly(); }
        } 
    }
}