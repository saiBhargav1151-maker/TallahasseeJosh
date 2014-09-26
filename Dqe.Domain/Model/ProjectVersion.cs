using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Dqe.Domain.Model
{
    public class ProjectVersion : Entity<Transformers.ProjectVersion>
    {
        private readonly ICollection<ProjectSnapshot> _projectSnapshots;

        public ProjectVersion()
        {
            _projectSnapshots = new Collection<ProjectSnapshot>();
        }

        public virtual Proposal MyProposal { get; protected internal set; }

        [Range(1, int.MaxValue)]
        public virtual int Version { get; protected internal set; }

        public virtual ProjectSourceType ProjectSource { get; protected internal set; }

        [Required]
        public virtual DqeUser VersionOwner { get; protected internal set; }

        [Required]
        public virtual Project MyProject { get; protected internal set; }

        public virtual ProjectSnapshot SnapshotSource { get; protected internal set; }

        public virtual IEnumerable<ProjectSnapshot> ProjectSnapshots
        {
            get { return _projectSnapshots.ToList().AsReadOnly(); }
        }

        protected internal virtual void AddSnapshot(ProjectSnapshot snapshot)
        {
            _projectSnapshots.Add(snapshot);
            snapshot.MyProjectVersion = this;
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