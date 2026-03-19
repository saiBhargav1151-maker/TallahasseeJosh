using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Dqe.Domain.Model.Wt
{
    public class RefVendor
    {
        private readonly ICollection<ProposalVendor> _proposalVendors;

        public RefVendor()
        {
            _proposalVendors = new Collection<ProposalVendor>();
        }

        public virtual long Id { get; set; }

        public virtual string VendorName { get; set; }

        public virtual string VendorType { get; set; }

        public virtual string CertificationType { get; set; }

        public virtual IEnumerable<ProposalVendor> ProposalVendors
        {
            get { return _proposalVendors.ToList().AsReadOnly(); }
        }
    }
}
