using System.Collections.Generic;

namespace Dqe.Domain.Model
{
    public class ItemSet
    {
        public string Set { get; set; }
        public string Member { get; set; }
        public decimal Total { get; set; }
        public bool Included { get; internal set; }

        public IList<ProposalItem> ProposalItems { get; set; }
        public IList<ProjectItem> ProjectItems { get; set; }
    }
}