using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Dqe.Domain.Model
{
    public class Proposal : Entity<Transformers.Proposal>
    {
        private readonly ICollection<ProjectVersion> _projectVersions;

        public Proposal()
        {
            _projectVersions = new Collection<ProjectVersion>();
        }

        [Required]
        public virtual string ProposalNumber { get; protected internal set; }

        public virtual ProposalSourceType ProposalSource { get; protected internal set; }

        [StringLength(500)]
        public virtual string Comment { get; protected internal set; }

        public virtual DateTime Created { get; protected internal set; }

        public virtual DateTime LastUpdated { get; protected internal set; }

        public virtual IEnumerable<ProjectVersion> ProjectVersions
        {
            get { return _projectVersions.ToList().AsReadOnly(); }
        } 

        public override Transformers.Proposal GetTransformer()
        {
            return new Transformers.Proposal();
        }

        public override void Transform(Transformers.Proposal transformer, DqeUser account)
        {
            
        }
    }
}