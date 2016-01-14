namespace Dqe.Domain.Model
{
    public class MarketAreaAveragePrice : AveragePrice
    {
        public virtual MarketArea MyMarketArea { get; set; }
    }
}