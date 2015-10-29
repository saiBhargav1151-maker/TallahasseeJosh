using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Dqe.Domain.Model.Wt
{
    public class Alternate
    {
        private readonly ICollection<ProjectItem> _projectItems;

        public Alternate()
        {
            _projectItems = new Collection<ProjectItem>();
        }

        public virtual long Id { get; set; }

        public virtual string Name { get; set; }

        public virtual string Description { get; set; }

        public virtual IEnumerable<ProjectItem> ProjectItems
        {
            get { return _projectItems.ToList().AsReadOnly(); }
        }
    }
}