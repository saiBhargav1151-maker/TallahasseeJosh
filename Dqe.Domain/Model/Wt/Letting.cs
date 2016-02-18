using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Dqe.Domain.Model.Wt
{
    public class Letting
    {
        private readonly ICollection<Proposal> _proposals;

        public Letting()
        {
            _proposals = new Collection<Proposal>();
        }

        public virtual long Id { get; set; }

        public virtual DateTime LettingDate { get; set; }

        public virtual string LettingName { get; set; }

        public virtual IEnumerable<Proposal> Proposals
        {
            get { return _proposals.ToList().AsReadOnly(); }
        }
    }
}