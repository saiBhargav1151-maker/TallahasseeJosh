using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Dqe.Domain.Model.Wt
{
    public class CategoryAlternate
    {
        private readonly ICollection<Category> _categories;

        public CategoryAlternate()
        {
            _categories = new Collection<Category>();
        }

        public virtual long Id { get; set; }

        public virtual string Name { get; set; }

        public virtual string Description { get; set; }

        public virtual IEnumerable<Category> Categories
        {
            get { return _categories.ToList().AsReadOnly(); }
        }
    }
}