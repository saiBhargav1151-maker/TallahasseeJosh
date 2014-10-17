using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Dqe.Domain.Model.Wt
{
    public class ProposalItem
    {
        private readonly ICollection<ProjectItem> _projectItems;

        public ProposalItem()
        {
            _projectItems = new Collection<ProjectItem>();
        }

        public virtual long Id { get; set; }

        public virtual RefItem MyRefItem { get; set; }

        public virtual Section MySection { get; set; }

        public virtual decimal Quantity { get; set; }

        public virtual decimal Price { get; set; }

        public virtual string AlternateSet { get; set; }

        public virtual string AlternateMember { get; set; }

        public virtual IEnumerable<ProjectItem> ProjectItems
        {
            get { return _projectItems.ToList().AsReadOnly(); }
        } 
    }
}