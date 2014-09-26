using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Dqe.Domain.Model.Wt
{
    public class Proposal
    {
        private readonly ICollection<Project> _projects;

        public Proposal()
        {
            _projects = new Collection<Project>();
        }
        
        [Required]
        public virtual long Id { get; set; }

        [StringLength(20)]
        [Required]
        public virtual string ProposalNumber { get; set; }

        [StringLength(256)]
        [Required]
        public virtual string Description { get; set; }

        public virtual IEnumerable<Project> Projects
        {
            get { return _projects.ToList().AsReadOnly(); }
        } 

    }
}