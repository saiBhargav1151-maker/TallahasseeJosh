using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Dqe.Domain.Model.Lre
{
    public class PayItemCounty
    {
        private readonly ICollection<PayItemDistrict> _payItemDistricts;

        public PayItemCounty()
        {
            _payItemDistricts = new Collection<PayItemDistrict>();
        }

        public virtual PayItemCountyId Id { get; set; }

        public virtual PayItem MyPayItem { get; set; }
        
        public virtual decimal UnitPrice { get; set; }

        public virtual string LockCode { get; set; }

        public virtual IEnumerable<PayItemDistrict> PayItemDistricts
        {
            get { return _payItemDistricts.ToList().AsReadOnly(); }
        }
    }
}