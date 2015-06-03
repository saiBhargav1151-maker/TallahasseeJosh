using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Dqe.Domain.Model
{
    public class ProposalItem : Entity<Transformers.ProposalItem>
    {
        private ICollection<ProjectItem> _projectItems;

        public ProposalItem()
        {
            _projectItems = new Collection<ProjectItem>();
        }

        [Required]
        public virtual SectionGroup MySectionGroup { get; protected internal set; }

        public virtual decimal Quantity { get; protected internal set; }

        [Range(1, int.MaxValue)]
        public virtual long WtId { get; protected internal set; }

        [StringLength(12)]
        public virtual string PayItemNumber { get; protected internal set; }

        [StringLength(200)]
        public virtual string CalculatedUnit { get; protected internal set; }

        [StringLength(10)]
        public virtual string Unit { get; protected internal set; }

        [StringLength(256)]
        public virtual string PayItemDescription { get; protected internal set; }

        public virtual string AlternateSet { get; protected internal set; }

        public virtual string AlternateMember { get; protected internal set; }

        public virtual string SupplementalDescription { get; protected internal set; }

        protected internal virtual void AddProjectItem(ProjectItem projectItem)
        {
            _projectItems.Add(projectItem);
            projectItem.MyProposalItem = this;
        }

        public virtual void DisconnectedProjectItems()
        {
            _projectItems = new Collection<ProjectItem>();
        }

        public virtual void ClearProjectItems()
        {
            foreach (var projectItem in _projectItems)
            {
                projectItem.MyProposalItem = null;
            }
            _projectItems.Clear();
        }

        public virtual void AddDisconnectedProjectItem(ProjectItem projectItem)
        {
            _projectItems.Add(projectItem);
            projectItem.MyProposalItem = this;
        }

        public virtual IEnumerable<ProjectItem> GetEstimatorProjectItems(DqeUser estimator)
        {
            return
                _projectItems.Where(i => i.MyEstimateGroup.MyProjectEstimate.MyProjectVersion.VersionOwner == estimator)
                    .ToList();
        } 

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