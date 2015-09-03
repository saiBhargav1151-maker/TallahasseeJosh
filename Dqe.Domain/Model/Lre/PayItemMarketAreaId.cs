using Fdot.Entity.Helpers;

namespace Dqe.Domain.Model.Lre
{
    public class PayItemMarketAreaId : ValueBasedCompositeId
    {
        public virtual string PayItemId { get; set; }

        public virtual string MarketArea { get; set; }
    }
}