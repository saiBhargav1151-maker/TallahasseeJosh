using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Dqe.Domain.Model.Wt
{
    public class Project
    {
        private readonly ICollection<Category> _categories;
        private readonly ICollection<County> _counties;
        private readonly ICollection<District> _districts; 

        public Project()
        {
            _categories = new Collection<Category>();
            _counties = new Collection<County>();
            _districts = new Collection<District>();
        }

        public virtual long Id { get; set; }

        public virtual string ProjectNumber { get; set; }

        public virtual string SpecBook { get; set; }

        public virtual string Description { get; set; }

        public virtual string Designer { get; set; }

        public virtual Proposal MyProposal { get; set; }

        public virtual IEnumerable<Category> Categories
        {
            get { return _categories.ToList().AsReadOnly(); }
        }

        public virtual IEnumerable<County> Counties
        {
            get { return _counties.ToList().AsReadOnly(); }
        }

        public virtual IEnumerable<District> Districts
        {
            get { return _districts.ToList().AsReadOnly(); }
        }
    }
}