using System.Collections.Generic;
using System.Linq;

namespace Dqe.Domain.Model
{
    public class ItemGroup
    {
        private readonly IList<ProjectItem> _projectItems = new List<ProjectItem>(); 

        public IList<ProjectItem> ProjectItems
        {
            get { return _projectItems; }
        }

        public bool CombineCategories { get; set; }

        public bool CombineItems { get; set; }

        public string CategoryAlternateSet { get; set; }

        public string CategoryAlternateMember { get; set; }

        public string CategoryDescription { get; set; }

        public string FederalConstructionClass { get; set; }

        public string ItemAlternateSet { get; set; }

        public string ItemAlternateMember { get; set; }

        public string Fund { get; set; }

        public string ItemNumber { get; set; }

        public string ItemDescription { get; set; }

        public string SupplementalDescription { get; set; }

        public string Unit { get; set; }

        public decimal Quantity
        {
            get { return ProjectItems == null ? 0 : ProjectItems.Sum(i => i.Quantity); }
        }

        public decimal Price { get; set; }
    }
}
