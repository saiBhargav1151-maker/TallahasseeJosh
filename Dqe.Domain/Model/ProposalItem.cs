using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Dqe.Domain.Model
{
    public class ProposalItem : Entity<Transformers.ProposalItem>
    {
        private readonly ICollection<ProjectItem> _projectItems;

        public ProposalItem()
        {
            _projectItems = new Collection<ProjectItem>();
        }

        [Required]
        public virtual SectionGroup MySectionGroup { get; protected internal set; }

        public virtual decimal Quantity { get; protected internal set; }

        public virtual decimal Price { get; protected internal set; }

        [Range(1, int.MaxValue)]
        public virtual long WtId { get; protected internal set; }

        [StringLength(12)]
        public virtual string PayItemNumber { get; protected internal set; }

        public virtual string AlternateSet { get; protected internal set; }

        public virtual string AlternateMember { get; protected internal set; }

        public virtual IEnumerable<ProjectItem> ProjectItems
        {
            get { return _projectItems.ToList().AsReadOnly(); }
        } 

        public override Transformers.ProposalItem GetTransformer()
        {
            throw new NotImplementedException();
        }

        public override void Transform(Transformers.ProposalItem transformer, DqeUser account)
        {
            throw new NotImplementedException();
        }
    }
}