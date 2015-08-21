using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Dqe.Domain.Model.Lre
{
    public class ProjectSnapshot
    {
        private readonly ICollection<VersionSnapshot> _versions;

        public ProjectSnapshot()
        {
            _versions = new Collection<VersionSnapshot>();
        }

        public virtual long Id { get; set; }

        public virtual string ProjectName { get; set; }

        public virtual IEnumerable<VersionSnapshot> Versions
        {
            get { return _versions.ToList().AsReadOnly(); }
        } 
    }
}