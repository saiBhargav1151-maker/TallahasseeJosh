using Dqe.Domain.Model;

namespace Dqe.ApplicationServices
{
    public interface IPricingEngine
    {
        BidSet CalculateStateAveragePrice(BidHistory bidHistory, PayItemMaster payItemMaster, DqeUser dqeUser);
        BidSet CalculateMarketAreaAveragePrice(BidHistory bidHistory, MarketArea marketArea);
        BidSet CalculateCountyAveragePrice(BidHistory bidHistory, County county);
        BidSet CalculateAveragePrice(BidSet bidSet, bool resetIncludedBids, decimal refQuantity, County refCounty);
    }
}