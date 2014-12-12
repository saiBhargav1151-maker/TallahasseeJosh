using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Dqe.Domain.Model.Wt
{
    public class Proposal
    {
        private readonly ICollection<Project> _projects;
        private readonly ICollection<Section> _sections;
        private readonly ICollection<ProposalVendor> _proposalVendors;  

        public Proposal()
        {
            _projects = new Collection<Project>();
            _sections = new Collection<Section>();
            _proposalVendors = new Collection<ProposalVendor>();
        }

        public virtual Letting MyLetting { get; set; }
        
        public virtual long Id { get; set; }

        public virtual bool IsRejected { get; set; }

        public virtual string ProposalNumber { get; set; }

        public virtual string ProposalStatus { get; set; }

        public virtual string Description { get; set; }

        public virtual RefCounty County { get; set; }

        public virtual RefDistrict District { get; set; }

        public virtual IEnumerable<Project> Projects
        {
            get { return _projects.ToList().AsReadOnly(); }
        }

        public virtual IEnumerable<Section> Sections
        {
            get { return _sections.ToList().AsReadOnly(); }
        }

        public virtual IEnumerable<ProposalVendor> ProposalVendors
        {
            get { return _proposalVendors.ToList().AsReadOnly(); }
        } 
    }
}