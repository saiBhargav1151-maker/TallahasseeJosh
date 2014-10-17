using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Dqe.Domain.Model.Wt
{
    public class Proposal
    {
        private readonly ICollection<Project> _projects;
        private readonly ICollection<Section> _sections; 

        public Proposal()
        {
            _projects = new Collection<Project>();
            _sections = new Collection<Section>();
        }

        public virtual Letting MyLetting { get; set; }
        
        public virtual long Id { get; set; }

        public virtual string ProposalNumber { get; set; }

        public virtual string Description { get; set; }

        public virtual IEnumerable<Project> Projects
        {
            get { return _projects.ToList().AsReadOnly(); }
        }

        public virtual IEnumerable<Section> Sections
        {
            get { return _sections.ToList().AsReadOnly(); }
        } 
    }
}