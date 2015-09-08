namespace Dqe.Domain.Model.Lre
{
    public class PayItemMarketArea
    {
        public virtual PayItemMarketAreaId Id { get; set; }

        public virtual PayItem MyPayItem { get; set; }

        public virtual MarketArea MyMarketArea { get; set; }

        public virtual decimal UnitPrice { get; set; }
    }
}