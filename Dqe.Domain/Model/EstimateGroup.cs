using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security;

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
        public virtual Project MyProject { get; protected internal set; }

        public virtual IEnumerable<ProjectItem> ProjectItems
        {
            get { return _projectItems.ToList().AsReadOnly(); }
        } 

        public virtual string Description { get; protected internal set; }

        public virtual void AddProjectItem(ProjectItem projectItem, DqeUser account)
        {
            if (projectItem == null) throw new ArgumentNullException("projectItem");
            if (account == null) throw new ArgumentNullException("account");
            if (account.Role != DqeRole.System && account.Role != DqeRole.Administrator && account.Role != DqeRole.DistrictAdministrator && account.Role != DqeRole.Estimator)
            {
                throw new SecurityException(string.Format("Account role {0} is not authorized for this transaction.", account.Role));
            }
            _projectItems.Add(projectItem);
            projectItem.MyEstimateGroup = this;
        }

        public override Transformers.EstimateGroup GetTransformer()
        {
            return new Transformers.EstimateGroup
            {
                Description = Description,
                Id = Id
            };
        }

        public override void Transform(Transformers.EstimateGroup transformer, DqeUser account)
        {
            if (transformer == null) throw new ArgumentNullException("transformer");
            if (account == null) throw new ArgumentNullException("account");
            if (account.Role != DqeRole.System && account.Role != DqeRole.Administrator && account.Role != DqeRole.DistrictAdministrator && account.Role != DqeRole.Estimator)
            {
                throw new SecurityException(string.Format("Account role {0} is not authorized for this transaction.", account.Role));
            }
            Description = transformer.Description;
        }
    }
}