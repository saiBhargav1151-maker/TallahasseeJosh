using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Dqe.Domain.Model
{
    public class SectionGroup : Entity<Transformers.SectionGroup>
    {
        private readonly ICollection<ProposalItem> _proposalItems;

        public SectionGroup()
        {
            _proposalItems = new Collection<ProposalItem>();
        }

        [Required]
        public virtual Proposal MyProposal { get; protected internal set; }

        [Range(1, int.MaxValue)]
        public virtual long WtId { get; protected internal set; }

        public virtual string Name { get; protected internal set; }

        public virtual string Description { get; protected internal set; }

        public virtual string AlternateSet { get; protected internal set; }

        public virtual string AlternateMember { get; protected internal set; }

        protected internal virtual void AddProposalItem(ProposalItem proposalItem)
        {
            _proposalItems.Add(proposalItem);
        }

        public virtual IEnumerable<ProposalItem> ProposalItems
        {
            get { return _proposalItems.ToList().AsReadOnly(); }
        } 

        public override Transformers.SectionGroup GetTransformer()
        {
            throw new NotImplementedException();
        }

        public override void Transform(Transformers.SectionGroup transformer, DqeUser account)
        {
            throw new NotImplementedException();
        }
    }
}