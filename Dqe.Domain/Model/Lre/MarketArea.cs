using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Dqe.Domain.Model.Lre
{
    public class MarketArea
    {
        private readonly ICollection<PayItemMarketArea> _payItemMarketAreas;

        public MarketArea()
        {
            _payItemMarketAreas = new Collection<PayItemMarketArea>();
        }

        public virtual string Id { get; set; }

        public virtual string Description { get; set; }

        public virtual IEnumerable<PayItemMarketArea> PayItemMarketAreas
        {
            get { return _payItemMarketAreas.ToList().AsReadOnly(); }
        }
    }
}