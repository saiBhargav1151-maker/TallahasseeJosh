using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Dqe.Domain.Model
{
    public class EstimateGroup : Entity<Transformers.EstimateGroup>
    {
        private readonly ICollection<ProjectItem> _projectItems;
        
        public EstimateGroup()
        {
            _projectItems = new Collection<ProjectItem>();
        }

        [Required]
        public virtual ProjectEstimate MyProjectEstimate { get; protected internal set; }

        public virtual IEnumerable<ProjectItem> ProjectItems
        {
            get { return _projectItems.ToList().AsReadOnly(); }
        } 

        public virtual string Description { get; protected internal set; }

        protected internal virtual void AddProjectItem(ProjectItem projectItem)
        {
            _projectItems.Add(projectItem);
            projectItem.MyEstimateGroup = this;
        }

        public override Transformers.EstimateGroup GetTransformer()
        {
            throw new NotImplementedException();
        }

        public override void Transform(Transformers.EstimateGroup transformer, DqeUser account)
        {
            throw new NotImplementedException();
        }
    }
}