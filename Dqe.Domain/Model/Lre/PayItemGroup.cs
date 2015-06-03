using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Dqe.Domain.Model.Lre
{
    public class PayItemGroup
    {
        private readonly ICollection<PayItemPayItemGroup> _payItemPayItemGroups;

        public PayItemGroup()
        {
            _payItemPayItemGroups = new Collection<PayItemPayItemGroup>();
        }

        public virtual string Id { get; set; }

        public virtual string GroupDescription { get; set; }

        public virtual string GroupTypeCode { get; set; }

        public virtual IEnumerable<PayItemPayItemGroup> PayItemPayItemGroups
        {
            get { return _payItemPayItemGroups.ToList().AsReadOnly(); }
        }

        public virtual void AddPayItemPayItemGroup(PayItemPayItemGroup payItemPayItemGroup)
        {
            _payItemPayItemGroups.Add(payItemPayItemGroup);
            payItemPayItemGroup.MyPayItemGroup = this;
            if (payItemPayItemGroup.Id == null)
            {
                payItemPayItemGroup.Id = new PayItemPayItemGroupId();
            }
            payItemPayItemGroup.Id.PayItemGroupId = Id;
        }
    }
}