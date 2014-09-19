using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Dqe.Domain.Model
{
    public class ProjectSnapshot : Entity<Transformers.ProjectSnapshot>
    {
        private readonly ICollection<EstimateGroup> _estimateGroups;

        public ProjectSnapshot()
        {
            _estimateGroups = new Collection<EstimateGroup>();
        }

        [Range(1, int.MaxValue)]
        public virtual int Snapshot { get; protected internal set; }

        [StringLength(500)]
        public virtual string SnapshotComment { get; protected internal set; }

        public virtual DateTime Created { get; protected internal set; }

        public virtual DateTime LastUpdated { get; set; }

        public virtual bool IsWorkingEstimate { get; set; }

        public virtual SnapshotLabel Label { get; protected internal set; }

        [Required]
        public virtual ProjectVersion MyProjectVersion { get; protected internal set; }

        public virtual IEnumerable<EstimateGroup> EstimateGroups
        {
            get { return _estimateGroups.ToList().AsReadOnly(); }
        }

        protected internal virtual void AddEstimateGroup(EstimateGroup estimateGroup)
        {
            _estimateGroups.Add(estimateGroup);
            estimateGroup.MyProjectSnapshot = this;
        }

        public override Transformers.ProjectSnapshot GetTransformer()
        {
            throw new NotImplementedException();
        }

        public override void Transform(Transformers.ProjectSnapshot transformer, DqeUser account)
        {
            throw new NotImplementedException();
        }
    }
}