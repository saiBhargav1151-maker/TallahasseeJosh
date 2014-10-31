using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Dqe.Domain.Model.Wt
{
    public class Section
    {
        private readonly ICollection<ProposalItem> _proposalItems;

        public Section()
        {
            _proposalItems = new Collection<ProposalItem>();
        }

        public virtual long Id { get; set; }

        public virtual Proposal MyProposal { get; set; }

        public virtual string Name { get; set; }

        public virtual string Description { get; set; }

        public virtual string AlternateSet { get; set; }

        public virtual string AlternateMember { get; set; }

        public virtual IEnumerable<ProposalItem> ProposalItems
        {
            get { return _proposalItems.ToList().AsReadOnly(); }
        } 
    }
}