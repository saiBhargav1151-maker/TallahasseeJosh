using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Dqe.Domain.Model
{
    public class SpecYear : Entity<Transformers.SpecYear>
    {
        private readonly ICollection<PayItem> _payItems;

        public SpecYear()
        {
            _payItems = new Collection<PayItem>();
        }

        public virtual int Year { get; protected internal set; }

        public virtual IEnumerable<PayItem> PayItems
        {
            get { return _payItems.ToList().AsReadOnly(); }
        }

        public override Transformers.SpecYear GetTransformer()
        {
            return new Transformers.SpecYear
            {
                Id = Id,
                Year = Year
            };
        }

        public override void Transform(Transformers.SpecYear transformer, DqeUser account)
        {
            if (transformer == null) throw new ArgumentNullException("transformer");
            if (account == null) throw new ArgumentNullException("account");
            Year = transformer.Year;
        }
    }
}