using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Dqe.Domain.Model.Lre
{
    public class PayItem
    {
        private readonly ICollection<PayItemPayItemGroup> _payItemPayItemGroups;

        public PayItem()
        {
            _payItemPayItemGroups = new Collection<PayItemPayItemGroup>();
        }

        public virtual string Id { get; set; }

        public virtual string ShortDescription { get; set; }

        public virtual string OtherShortDescription { get; set; }

        public virtual string EnglishMetricCode { get; set; }

        public virtual string UnitOfMeasureCode { get; set; }

        public virtual decimal ReferencePrice { get; set; }

        public virtual string MetricPayItemId { get; set; }

        public virtual DateTime PayItemStatusDate { get; set; }

        public virtual string PayItemStatusCode { get; set; }

        public virtual string Description { get; set; }

        public virtual string Source { get; set; }

        public virtual IEnumerable<PayItemPayItemGroup> PayItemPayItemGroups
        {
            get { return _payItemPayItemGroups.ToList().AsReadOnly(); }
        }

        public virtual void AddPayItemPayItemGroup(PayItemPayItemGroup payItemPayItemGroup)
        {
            _payItemPayItemGroups.Add(payItemPayItemGroup);
            payItemPayItemGroup.MyPayItem = this;
            if (payItemPayItemGroup.Id == null)
            {
                payItemPayItemGroup.Id = new PayItemPayItemGroupId();
            }
            payItemPayItemGroup.Id.PayItemId = Id;
        }

    }
}