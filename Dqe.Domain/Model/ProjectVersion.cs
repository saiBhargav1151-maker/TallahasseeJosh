using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Dqe.Domain.Model
{
    public class ProjectVersion : Entity<Transformers.ProjectVersion>
    {
        private readonly ICollection<ProjectEstimate> _projectEstimates;

        public ProjectVersion()
        {
            _projectEstimates = new Collection<ProjectEstimate>();
        }

        [Range(1, int.MaxValue)]
        public virtual int Version { get; protected internal set; }

        public virtual ProjectSourceType ProjectSource { get; protected internal set; }

        [Required]
        public virtual DqeUser VersionOwner { get; protected internal set; }

        [Required]
        public virtual Project MyProject { get; protected internal set; }

        public virtual Proposal MyProposal { get; protected internal set; }

        public virtual ProjectEstimate EstimateSource { get; protected internal set; }

        public virtual IEnumerable<ProjectEstimate> ProjectEstimates
        {
            get { return _projectEstimates.ToList().AsReadOnly(); }
        }

        protected internal virtual void AddEstimate(ProjectEstimate estimate)
        {
            _projectEstimates.Add(estimate);
            estimate.MyProjectVersion = this;
        }

        public override Transformers.ProjectVersion GetTransformer()
        {
            throw new NotImplementedException();
        }

        public override void Transform(Transformers.ProjectVersion transformer, DqeUser account)
        {
            throw new NotImplementedException();
        }
    }
}