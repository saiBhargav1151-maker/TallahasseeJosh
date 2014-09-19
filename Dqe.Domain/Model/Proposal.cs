using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Dqe.Domain.Model
{
    public class Proposal : Entity<Transformers.Proposal>
    {
        private readonly ICollection<Project> _projects;

        public Proposal()
        {
            _projects = new Collection<Project>();
        }

        public virtual IEnumerable<Project> Projects
        {
            get { return _projects.ToList().AsReadOnly(); }
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