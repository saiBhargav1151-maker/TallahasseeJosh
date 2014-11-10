using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Dqe.Domain.Model.Wt
{
    public class Category
    {
        private readonly ICollection<ProjectItem> _projectItems;

        public Category()
        {
            _projectItems = new Collection<ProjectItem>();
        }

        public virtual long Id { get; set; }

        public virtual string Name { get; set; }

        public virtual string Description { get; set; }

        public virtual bool CombineLikeItems { get; set; }

        public virtual string FederalConstructionClass { get; set; }

        public virtual Project MyProject { get; set; }

        public virtual CategoryAlternate MyCategoryAlternate { get; set; }

        public virtual string AlternateMember { get; set; }

        public virtual IEnumerable<ProjectItem> ProjectItems
        {
            get { return _projectItems.ToList().AsReadOnly(); }
        } 
    }
}