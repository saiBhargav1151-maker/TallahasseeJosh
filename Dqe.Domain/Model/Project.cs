using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Dqe.Domain.Model
{
    public class Project : Entity<Transformers.Project>
    {
        private readonly ICollection<ProjectItem> _projectItems;

        public Project()
        {
            _projectItems = new Collection<ProjectItem>();
        }

        public virtual Proposal MyProposal { get; protected internal set; }

        public virtual IEnumerable<ProjectItem> ProjectItems
        {
            get { return _projectItems.ToList().AsReadOnly(); }
        } 

        public override Transformers.Project GetTransformer()
        {
            return new Transformers.Project();
        }

        public override void Transform(Transformers.Project transformer, DqeUser account)
        {
            
        }
    }
}